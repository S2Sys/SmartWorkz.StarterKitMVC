# SmartWorkz.Core.Shared.Caching

High-performance caching system with tenant isolation, L1/L2 hybrid support, and automatic cache invalidation strategies.

## Overview

The Caching module provides a unified interface for caching across your application. It supports both in-memory caching (L1) and distributed cache backends (L2), with built-in tenant isolation, TTL management, and cache invalidation patterns.

**Key Problems Solved:**
- Reduce database roundtrips with strategic caching
- Tenant data isolation in multi-tenant systems
- Prevent stale data through automatic invalidation
- Flexible TTL strategies (absolute vs. sliding expiration)
- Unified API regardless of cache backend

## Cache Strategy Patterns

### 1. Absolute Expiration
Cache entry expires at a fixed time after creation, regardless of access.

```csharp
// Cache valid for exactly 30 minutes
var cacheOptions = new CacheOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
};

await _cacheService.SetAsync("user:123", user, ttlMinutes: 30, tenantId: "tenant-456");
```

**Use case:** Configuration data, read-only lookups, reference data

### 2. Sliding Expiration
Cache entry TTL extends with each access. Entry only expires if unused for the interval.

```csharp
// Sliding window: 30 min from last access
var cacheOptions = new CacheOptions
{
    SlidingExpiration = TimeSpan.FromMinutes(30)
};
```

**Use case:** User sessions, recently accessed products, active query results

## Quick Start

### 1. Register Services

```csharp
// Program.cs
services.AddMemoryCache();
services.AddScoped<ICacheService, MemoryCacheService>();
```

### 2. Inject and Use

```csharp
public class ProductService
{
    private readonly ICacheService _cacheService;
    private readonly IProductRepository _repository;

    public ProductService(
        ICacheService cacheService,
        IProductRepository repository)
    {
        _cacheService = cacheService;
        _repository = repository;
    }

    public async Task<Product> GetProductAsync(string productId, string tenantId)
    {
        // Try cache first
        var cached = await _cacheService.GetAsync<Product>(
            key: $"product:{productId}",
            tenantId: tenantId);

        if (cached.IsSuccess && cached.Data != null)
        {
            return cached.Data;
        }

        // Load from database
        var product = await _repository.GetByIdAsync(productId);

        // Cache for 1 hour
        await _cacheService.SetAsync(
            key: $"product:{productId}",
            value: product,
            ttlMinutes: 60,
            tenantId: tenantId);

        return product;
    }
}
```

## Query Cache Strategy

### N+1 Problem Prevention

**❌ Bad: N+1 queries (no cache)**
```csharp
var users = await _repository.GetAllUsersAsync();
foreach (var user in users)
{
    user.Orders = await _orderService.GetUserOrdersAsync(user.Id);
    // Executes separate query for each user!
}
```

**✅ Good: Batch load with cache**
```csharp
var users = await _repository.GetAllUsersAsync();
var userIds = users.Select(u => u.Id).ToList();

var cachedOrders = new Dictionary<string, List<Order>>();

foreach (var userId in userIds)
{
    var result = await _cacheService.GetAsync<List<Order>>(
        key: $"user:orders:{userId}",
        tenantId: tenantId);

    if (result.IsSuccess && result.Data != null)
    {
        cachedOrders[userId] = result.Data;
    }
    else
    {
        // Load in batch if not cached
        cachedOrders[userId] = await _orderService
            .GetUserOrdersAsync(userId);

        await _cacheService.SetAsync(
            key: $"user:orders:{userId}",
            value: cachedOrders[userId],
            ttlMinutes: 15,
            tenantId: tenantId);
    }
}

foreach (var user in users)
{
    user.Orders = cachedOrders.GetValueOrDefault(user.Id, new());
}
```

## CacheInvalidationStrategy Pattern

Invalidate cache strategically based on event type and data relationships.

### Pattern 1: Event-Based Invalidation

```csharp
public class UserUpdatedDomainEventHandler : IDomainEventHandler<UserUpdatedDomainEvent>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<UserUpdatedDomainEventHandler> _logger;

    public async Task HandleAsync(UserUpdatedDomainEvent domainEvent)
    {
        // Invalidate specific user cache
        await _cacheService.RemoveAsync(
            key: $"user:{domainEvent.UserId}",
            tenantId: domainEvent.TenantId);

        // Invalidate related caches
        await _cacheService.RemoveByPrefixAsync(
            prefix: $"user:{domainEvent.UserId}:orders",
            tenantId: domainEvent.TenantId);

        _logger.LogInformation(
            "Cache invalidated for user {UserId}",
            domainEvent.UserId);
    }
}
```

### Pattern 2: Cascade Invalidation

When a parent entity changes, invalidate all dependent caches:

```csharp
public class TenantConfigurationUpdatedHandler
{
    private readonly ICacheService _cacheService;

    public async Task HandleAsync(TenantConfigurationUpdatedEvent evt)
    {
        // Clear all tenant caches (heavy invalidation)
        await _cacheService.ClearAsync(tenantId: evt.TenantId);

        _logger.LogInformation(
            "All caches cleared for tenant {TenantId}",
            evt.TenantId);
    }
}
```

### Pattern 3: TTL-Based Expiration

Let cache naturally expire rather than manual invalidation:

```csharp
public async Task<List<Product>> GetCategoryProductsAsync(
    string categoryId,
    string tenantId)
{
    var cacheKey = $"category:{categoryId}:products";

    var cached = await _cacheService.GetAsync<List<Product>>(
        key: cacheKey,
        tenantId: tenantId);

    if (cached.IsSuccess && cached.Data != null)
        return cached.Data;

    // Load from database
    var products = await _repository.GetByCategoryAsync(categoryId);

    // Cache for 30 minutes (product lists change less frequently)
    await _cacheService.SetAsync(
        key: cacheKey,
        value: products,
        ttlMinutes: 30,
        tenantId: tenantId);

    return products;
}
```

## TTL and Size Limits

### Default Configuration

| Setting | Default | Purpose |
|---------|---------|---------|
| Max memory | 100 MB | Prevent unbounded growth |
| Absolute TTL | 60 minutes | Safeguard for all entries |
| Sliding TTL | 30 minutes | Session-like behavior |
| Size limit | 10,000 items | LRU eviction when exceeded |

### Custom TTL Example

```csharp
public enum CacheDuration
{
    // Short-lived: High volatility
    VeryShort = 1,        // User sessions
    Short = 5,            // API responses
    
    // Medium-lived: Moderate volatility
    Medium = 15,          // Product lists
    Standard = 30,        // Configurations
    
    // Long-lived: Stable data
    Long = 60,            // Reference data
    VeryLong = 360,       // Static content
    
    // No expiration: Rarely changes
    Never = null          // Immutable lookups
}

// Usage
await _cacheService.SetAsync(
    key: "product:sku:lookup",
    value: productBySku,
    ttlMinutes: (int)CacheDuration.Long,
    tenantId: tenantId);
```

### Size Limit Handling

When cache reaches 10,000 items:
1. Least Recently Used (LRU) items are evicted
2. Items with closer expiration are prioritized for removal
3. Monitor cache statistics to detect eviction patterns

```csharp
public class CacheMonitor
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheMonitor> _logger;

    public async Task CheckCacheHealthAsync(string tenantId)
    {
        // If evictions increase, reduce TTLs or clear stale data
        var stats = await _cacheService.GetStatisticsAsync(tenantId);
        
        if (stats.EvictionCount > 100)
        {
            _logger.LogWarning(
                "High cache eviction rate: {EvictionCount}",
                stats.EvictionCount);

            // Clear low-priority caches
            await _cacheService.RemoveByPrefixAsync(
                prefix: "temp:*",
                tenantId: tenantId);
        }
    }
}
```

## Integration with CQRS

Cache query results directly in query handlers for fast subsequent access:

```csharp
public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, User>
{
    private readonly IUserRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public async Task<User> HandleAsync(GetUserByIdQuery query)
    {
        var cacheKey = $"query:user:{query.UserId}";

        // Try cache first
        var cached = await _cacheService.GetAsync<User>(
            key: cacheKey,
            tenantId: query.TenantId);

        if (cached.IsSuccess && cached.Data != null)
        {
            _logger.LogInformation("Query cache hit for user {UserId}", query.UserId);
            return cached.Data;
        }

        // Execute query
        var user = await _repository.GetByIdAsync(query.UserId);

        // Cache result for 10 minutes
        await _cacheService.SetAsync(
            key: cacheKey,
            value: user,
            ttlMinutes: 10,
            tenantId: query.TenantId);

        _logger.LogInformation("Query cache miss, result cached for user {UserId}", query.UserId);
        return user;
    }
}

// Later in command handler that updates user
public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand>
{
    private readonly IUserRepository _repository;
    private readonly ICacheService _cacheService;

    public async Task HandleAsync(UpdateUserCommand command)
    {
        // Update user
        await _repository.UpdateAsync(command.UserId, command.Changes);

        // Invalidate cache
        await _cacheService.RemoveAsync(
            key: $"query:user:{command.UserId}",
            tenantId: command.TenantId);

        // Invalidate related queries
        await _cacheService.RemoveByPrefixAsync(
            prefix: $"query:users:tenant:{command.TenantId}",
            tenantId: command.TenantId);
    }
}
```

## Tenant-Scoped Cache Keys

All cache keys are automatically prefixed with tenant ID to ensure isolation:

```csharp
// Request cache for "product:123" in tenant "acme"
await _cacheService.GetAsync<Product>(
    key: "product:123",
    tenantId: "acme");

// Internally stored as: "acme:product:123"
// Never accessible to other tenants
```

If `tenantId` is omitted, defaults to "default":

```csharp
// No tenant specified - uses "default" tenant
await _cacheService.GetAsync<Settings>(
    key: "global:settings");

// Stored as: "default:global:settings"
```

## Best Practices

### 1. **Use Consistent Key Patterns**
```csharp
// ✅ Good: Hierarchical, predictable
$"user:{userId}:profile"
$"order:{orderId}:items"
$"category:{categoryId}:products"

// ❌ Bad: Inconsistent
$"u{userId}"
$"getOrders{orderId}"
$"prod-{categoryId}"
```

### 2. **Cache Only Serializable Data**
```csharp
// ✅ Good: Serializable POCO
await _cacheService.SetAsync(
    key: "user:123",
    value: new UserDto { Id = "123", Name = "John" },
    ttlMinutes: 30);

// ❌ Bad: Non-serializable objects
await _cacheService.SetAsync(
    key: "db:connection",
    value: dbConnection, // Can't serialize!
    ttlMinutes: 30);
```

### 3. **Monitor Cache Hit Ratios**
```csharp
public class CacheMetrics
{
    public int Hits { get; set; }
    public int Misses { get; set; }

    public double HitRatio => Hits / (Hits + Misses);
    // Target: >80% for well-tuned caching
}
```

### 4. **Handle Cache Misses Gracefully**
```csharp
// ✅ Always have a fallback
var result = await _cacheService.GetAsync<Product>(key, tenantId);

if (!result.IsSuccess || result.Data == null)
{
    // Load from source of truth
    product = await _repository.GetAsync(productId);
}
else
{
    product = result.Data;
}

// ❌ Don't assume cache always works
var product = await _cacheService.GetAsync<Product>(key, tenantId);
// What if cache is null or failed?
```

### 5. **Avoid Cache Stampede**
When an entry expires, prevent multiple requests from all reloading simultaneously:

```csharp
// Use pessimistic locking
private static readonly SemaphoreSlim _cacheLock = new(1, 1);

public async Task<Product> GetProductAsync(string id, string tenantId)
{
    var cached = await _cacheService.GetAsync<Product>($"product:{id}", tenantId);
    if (cached.IsSuccess && cached.Data != null)
        return cached.Data;

    // Only one request loads data
    await _cacheLock.WaitAsync();
    try
    {
        // Double-check after acquiring lock
        cached = await _cacheService.GetAsync<Product>($"product:{id}", tenantId);
        if (cached.IsSuccess && cached.Data != null)
            return cached.Data;

        // Load and cache
        var product = await _repository.GetAsync(id);
        await _cacheService.SetAsync($"product:{id}", product, ttlMinutes: 30, tenantId);
        return product;
    }
    finally
    {
        _cacheLock.Release();
    }
}
```

## Performance Considerations

| Operation | Complexity | Notes |
|-----------|-----------|-------|
| Get | O(1) | Hash table lookup |
| Set | O(1) | Hash table insert |
| Remove | O(1) | Hash table delete |
| RemoveByPrefix | O(n) | Iterates all keys, use sparingly |
| Clear | O(n) | Clears entire tenant scope |

For high-frequency prefix invalidations, consider restructuring your cache keys.

## Related Modules

- **CQRS/** — Cache query handler results
- **Logging/** — Log cache hits/misses for observability
- **Webhooks/** — Invalidate cache on external events

## Common Issues

### Cache Not Working
1. Verify service registered in DI: `services.AddMemoryCache()`
2. Check `tenantId` matches across get/set operations
3. Ensure `ttlMinutes` is specified (null = no expiration)
4. Monitor logs for cache service errors

### Stale Data in Cache
1. Reduce TTL for frequently changing data
2. Add event handler to invalidate on updates
3. Implement cache versioning (append version to key)

### Memory Growing Unbounded
1. Check TTL is reasonable (not null for large objects)
2. Monitor for cache key leaks (keys not following pattern)
3. Reduce size limits or implement aggressive TTL
4. Enable cache statistics to identify heavy hitters
