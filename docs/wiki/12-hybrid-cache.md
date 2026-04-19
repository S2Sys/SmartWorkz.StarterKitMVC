# Hybrid Cache

Two-tier cache with tenant-aware keys. L1 is in-process `IMemoryCache`, L2 is `IDistributedCache` (Redis when configured, falls back to in-memory). Read path checks L1 first, then L2, and **re-hydrates L1** on an L2 hit.

## Purpose

- **Fast path:** L1 returns in nanoseconds inside the process; no serialization.
- **Cross-node coherence:** L2 lets multiple web nodes share cache — important behind a load balancer.
- **Tenant isolation:** every key is automatically namespaced `{tenantId}:{key}`. You cannot accidentally read another tenant's data.
- **Fail-open:** L2 failures are logged at Warning and swallowed — L1 keeps serving.

## Architecture

| Component | Role | File |
|-----------|------|------|
| `ICacheService` | Contract | [`Application/Abstractions/ICacheService.cs`](../../src/SmartWorkz.StarterKitMVC.Application/Abstractions/ICacheService.cs) |
| `HybridCacheService` | Singleton implementation | [`Infrastructure/Services/HybridCacheService.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Services/HybridCacheService.cs) |
| L1 — `IMemoryCache` | Per-process, 2 min TTL | `AddMemoryCache()` in `AddApplicationServices` |
| L2 — `IDistributedCache` | Cross-node, default 30 min TTL | Wired in `AddCacheServices` |

### Flow

```
GetAsync(tenantId, key)
    ↓
  BuildKey →  "acme:user:42"
    ↓
  L1 TryGetValue
    ↓  miss
  L2 GetAsync(bytes) + JSON deserialize
    ↓
  Hydrate L1 (min of L1Ttl=2min, L2Ttl)
    ↓
  Return T or null
```

## DI Registration

All wiring is inside `AddApplicationStack`. Specifically:

- `AddApplicationServices` registers `IMemoryCache` (`services.AddMemoryCache()`).
- `AddCacheServices` picks the L2 backend based on config, then registers `ICacheService` as **Singleton**:

```csharp
var redis = configuration.GetConnectionString("Redis");

if (!string.IsNullOrEmpty(redis))
{
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redis;
        options.InstanceName  = "StarterKit_";
    });
}
else
{
    // SQL Server L2 is commented out pending package finalization;
    // fallback is in-memory (same process as L1 — loses cross-node coherence).
    services.AddDistributedMemoryCache();
}

services.AddSingleton<ICacheService, HybridCacheService>();
```

The Redis connection string lives at `ConnectionStrings:Redis`. Leave blank to use the in-memory fallback.

## Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=…",
    "Redis": "localhost:6379,abortConnect=false"
  },

  "Features": {
    "Caching": {
      "Enabled": true,
      "Provider": "Memory",
      "DefaultExpirationMinutes": 30,
      "Redis": {
        "Enabled": false,
        "InstanceName": "StarterKit_"
      }
    }
  }
}
```

> Note: the `Features:Caching` block is intended for future UI/flag wiring. The current implementation reads `ConnectionStrings:Redis` directly — if it's non-empty, Redis is used.

## Method Reference

```csharp
Task<T?> GetAsync<T>(string tenantId, string key, CancellationToken ct = default) where T : class;
Task     SetAsync<T>(string tenantId, string key, T value, TimeSpan? absoluteExpiry = null, CancellationToken ct = default) where T : class;
Task     RemoveAsync(string tenantId, string key, CancellationToken ct = default);
Task     RemoveByPrefixAsync(string tenantId, string prefix, CancellationToken ct = default);
```

### Basic read-through

```csharp
public class CatalogService
{
    private readonly ICacheService _cache;
    private readonly IProductRepository _repo;

    public CatalogService(ICacheService cache, IProductRepository repo)
    {
        _cache = cache;
        _repo  = repo;
    }

    public async Task<Product?> GetProductAsync(string tenantId, Guid productId, CancellationToken ct)
    {
        var key = $"product:{productId}";
        var cached = await _cache.GetAsync<Product>(tenantId, key, ct);
        if (cached is not null) return cached;

        var product = await _repo.GetByIdAsync(productId);
        if (product is not null)
            await _cache.SetAsync(tenantId, key, product, TimeSpan.FromMinutes(15), ct);

        return product;
    }
}
```

### Write invalidation

Invalidate **before** returning so the next read can't serve stale data:

```csharp
public async Task<Result> UpdateProductAsync(Product product, CancellationToken ct)
{
    await _repo.UpsertAsync(product);
    await _cache.RemoveAsync(product.TenantId, $"product:{product.ProductId}", ct);
    return Result.Success();
}
```

### Custom TTL

```csharp
// Short-lived (feature-flag check)
await _cache.SetAsync(tenantId, "flags:beta-checkout", flags, TimeSpan.FromMinutes(1), ct);

// Long-lived (country lookup)
await _cache.SetAsync(tenantId, "lov:countries", countries, TimeSpan.FromHours(6), ct);
```

`SetAsync` trims the **L1** TTL to the smaller of `L1Ttl` (2 min) and your requested value, so short TTLs work as expected. L2 honours the value as given.

### Prefix eviction — currently a no-op

`RemoveByPrefixAsync` is declared on the interface but not yet implemented. It logs at Debug and returns `Task.CompletedTask`. Rationale:

- Redis needs `SCAN + DEL` via `IConnectionMultiplexer` (not exposed by `IDistributedCache`).
- SQL Server backend needs `DELETE … WHERE Id LIKE`, which forks by provider.

Until implemented, evict explicit keys or rely on natural TTL expiry. If you're adding it, raise the discussion on PR — implementation should be behind a provider check (`IConnectionMultiplexer` optional dependency).

## Provider Swap — L2 Backend

### Redis (recommended for multi-node)

```json
"Redis": "localhost:6379"
```

Uses `Microsoft.Extensions.Caching.StackExchangeRedis`. Supports connection strings with `password`, `ssl=true`, `abortConnect=false`, cluster endpoints, etc. — see StackExchange.Redis docs.

### SQL Server distributed cache

Preferred for single-DB deployments that don't want a separate Redis node. Add package `Microsoft.Extensions.Caching.SqlServer` and replace the `else` branch:

```csharp
services.AddSqlServerCache(options =>
{
    options.ConnectionString = configuration.GetConnectionString("DefaultConnection");
    options.SchemaName       = "Master";
    options.TableName        = "CacheEntries";
});
```

The table is already created by the migrations (`Master.CacheEntries`). The block is commented in [`ServiceCollectionExtensions.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Extensions/ServiceCollectionExtensions.cs) — uncomment and drop the `AddDistributedMemoryCache` fallback.

### In-memory (single-node dev / tests)

```csharp
services.AddDistributedMemoryCache();
```

Already the fallback when `Redis` is empty. Cache is per-process — every restart starts cold, and multiple nodes won't share values.

### NCache / Memcached / other

Any `IDistributedCache` implementation works. Register your provider, leave the rest of the wiring unchanged.

## Tenant-Aware Keys

Every call passes `tenantId` explicitly. `HybridCacheService` prepends it: `{tenantId}:{key}`. You never see the fully-qualified key unless you're debugging.

In an MVC page model:

```csharp
public class SomePageModel : BasePage
{
    private readonly ICacheService _cache;

    public async Task OnGetAsync(CancellationToken ct)
    {
        // TenantId comes from BasePage → resolved by TenantMiddleware
        var settings = await _cache.GetAsync<AppSettings>(TenantId, "settings:ui", ct);
        // …
    }
}
```

**Never bake a tenant into the key yourself** (e.g. `"acme:settings:ui"`). Let the service do it — otherwise you'll double-prefix.

## Samples from the Codebase

- `TranslationService` warms translations via `CachedDapperRepository` + `IMemoryCache` (different cache — it's a per-repo local cache, not the hybrid one). See [01 — Translation System](./01-translation-system.md).
- `HybridCacheService` is registered Singleton; inject it from any scoped or singleton service.

## Common Mistakes

- **Injecting `IMemoryCache` directly** to gain "faster" reads — bypasses the tenant-key discipline and the L2 coherence. Always use `ICacheService`.
- **Storing non-serializable types** — values must survive `JsonSerializer.Serialize`. Anonymous types, closures, `DbContext`-tracked entities (with proxies) often fail.
- **Large objects** — L2 stores bytes across the wire. Keep cached values small (<100KB). For bulk data, store an ID and re-query.
- **Caching secrets / PII keyed only by user ID** — remember, L2 is shared across nodes, and Redis isn't encrypted by default. Don't cache things you wouldn't log.
- **Relying on `RemoveByPrefixAsync`** — it's a no-op today. If you need group eviction, keep an index list in another cache key.
- **Different tenants leaking via shared dependency** — if two scoped services share a cached entity, the key's tenant prefix is your only protection. Double-check `tenantId` at the call site.
- **Very long absolute expiries combined with writes** — stale data is the #1 cache bug. Invalidate aggressively on write paths.

## See Also

- [00 — Getting Started](./00-getting-started.md) — where `AddCacheServices` is wired
- [10 — Dapper Repository](./10-dapper-repository.md) — has its own per-call `IMemoryCache` for SP results (`QuerySpCachedAsync`)
- [01 — Translation System](./01-translation-system.md) — uses a dedicated cache for translation key lookup
