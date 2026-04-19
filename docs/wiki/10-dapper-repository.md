# Dapper Repository

Provider-agnostic Dapper access layer for stored-procedure backed repositories.

## Purpose

- One base class (`CachedDapperRepository`) for every SP-driven repository.
- Uniform error handling: every `SqlException` is wrapped as `RepositoryException` with the SP name.
- Opt-in `IMemoryCache` integration on a per-call basis.
- First-class support for **paged SPs** (items + total-count in two result sets).
- `IDbConnection` is injected, so the repository itself doesn't care which DB provider is in use.

## Architecture

| Component | Role | File |
|-----------|------|------|
| `IDapperRepository<T>` | Generic single-table CRUD contract | [`Application/Repositories/IDapperRepository.cs`](../../src/SmartWorkz.StarterKitMVC.Application/Repositories/IDapperRepository.cs) |
| `TableAttribute`, `KeyAttribute`, `IdentityAttribute`, `NotMappedAttribute` | Metadata for generic SQL building | same file |
| `CachedDapperRepository` | Abstract base — SP helpers + optional caching | [`Infrastructure/Repositories/CachedDapperRepository.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Repositories/CachedDapperRepository.cs) |
| `RepositoryException` | Wraps SP failures with SP name + error number | same file |
| Concrete repos | One class per aggregate (e.g. `UserRepository`, `ProductRepository`) | `Infrastructure/Repositories/*.cs` |

> **Current state:** `CachedDapperRepository` implements the SP-execution helpers used by every concrete repo. The generic `IDapperRepository<T>` interface (GetByIdAsync, UpsertAsync, GetPagedAsync…) is defined and consumed by `BaseListPage<T>` but does **not** yet have a concrete implementation registered in DI. Until a `GenericDapperRepository<T>` is added, keep writing typed repos that inherit `CachedDapperRepository` and expose SP-specific methods.

## DI Registration

The `IDbConnection` factory and each concrete repo are registered by `AddApplicationStack` — specifically `AddInfrastructureServices` and `AddRepositories` in [`ServiceCollectionExtensions.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Extensions/ServiceCollectionExtensions.cs):

```csharp
// Connection factory (one per request scope)
services.AddScoped<IDbConnection>(sp =>
{
    var cs = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not configured.");
    return new SqlConnection(cs);
});

// Concrete repos
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IProductRepository, ProductRepository>();
services.AddScoped<ITenantRepository, TenantRepository>();
// … etc.
```

`IMemoryCache` is supplied by `AddApplicationServices` (`services.AddMemoryCache()`) and injected into any Dapper repo that wants caching.

## Writing a New Repository

```csharp
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

public class OrderRepository : CachedDapperRepository, IOrderRepository
{
    public OrderRepository(
        IDbConnection connection,
        IMemoryCache cache,
        ILogger<OrderRepository> logger)
        : base(connection, cache, logger) { }

    public Task<Order?> GetByIdAsync(Guid orderId) =>
        QuerySingleSpAsync<Order>("Transaction.sp_GetOrderById", new { OrderId = orderId });

    public async Task<List<Order>> GetByCustomerAsync(Guid customerId, string tenantId)
    {
        var rows = await QuerySpAsync<Order>(
            "Transaction.sp_GetOrdersByCustomer",
            new { CustomerId = customerId, TenantId = tenantId });
        return rows.ToList();
    }
}
```

Register it alongside the other repos in `AddRepositories`:

```csharp
services.AddScoped<IOrderRepository, OrderRepository>();
```

## Method Reference — `CachedDapperRepository`

All helpers are `protected` — call them from derived repos, never from callers directly.

### Execute SP (no caching)

| Method | Returns | Use for |
|--------|---------|---------|
| `QuerySpAsync<T>(spName, param, timeoutSeconds)` | `IEnumerable<T>` | SELECT returning 0–N rows |
| `QuerySingleSpAsync<T>(spName, param, timeoutSeconds)` | `T?` | SELECT returning 0–1 rows |
| `ExecuteSpAsync(spName, param, timeoutSeconds)` | `void` | INSERT / UPDATE / DELETE with no SELECT |

```csharp
// 0..N rows
var products = await QuerySpAsync<Product>(
    "Catalog.sp_GetProductsByCategory",
    new { CategoryId = id, TenantId = tenantId });

// 0..1 row
var user = await QuerySingleSpAsync<User>(
    "Auth.sp_GetUserByEmail",
    new { Email = email, TenantId = tenantId });

// no result
await ExecuteSpAsync(
    "Catalog.sp_UpsertProduct",
    new { product.ProductId, product.Sku, product.Name, TenantId = product.TenantId });
```

### Execute SP (with cache)

Use when the SP result is **expensive** and **safe to cache** for the provided TTL. The cache key must be unique per tenant / input.

| Method | Returns |
|--------|---------|
| `QuerySpCachedAsync<T>(cacheKey, ttl, spName, param, timeoutSeconds)` | `IEnumerable<T>` |
| `QuerySingleSpCachedAsync<T>(cacheKey, ttl, spName, param, timeoutSeconds)` | `T?` |
| `InvalidateCacheKey(cacheKey)` | `void` — call after writes that affect the cached rows |

```csharp
public Task<List<Country>> GetCountriesAsync(string locale) =>
    QuerySpCachedAsync<Country>(
        cacheKey: $"countries:{locale}",
        ttl: TimeSpan.FromHours(6),
        spName: "Master.sp_GetCountries",
        param: new { Locale = locale })
    .ContinueWith(t => t.Result.ToList());

public async Task UpsertCountryAsync(Country c)
{
    await ExecuteSpAsync("Master.sp_UpsertCountry", c);
    InvalidateCacheKey($"countries:{c.Locale}"); // keep cache honest
}
```

If no `IMemoryCache` was injected (`null`), the cached helpers transparently fall through to the non-cached version.

### Multi result-set SPs

For SPs that return **more than one** result set.

```csharp
// Two result sets of different types
var (orders, lines) = await QueryMultipleSpAsync<Order, OrderLine>(
    "Transaction.sp_GetOrderWithLines",
    new { OrderId = id });

// Paged: items + single-row total count
var (items, total) = await QueryPagedSpAsync<Product>(
    "Catalog.sp_SearchProducts",
    new { TenantId = tenantId, Search = term, Page = 1, PageSize = 20 });

// Raw reader (advanced — caller manages reading order)
using var reader = await QueryMultipleSpAsync(
    "Reports.sp_ComplexReport",
    new { ReportId = id });
var header   = await reader.ReadSingleAsync<ReportHeader>();
var rows     = await reader.ReadAsync<ReportRow>();
var summary  = await reader.ReadSingleAsync<ReportSummary>();
```

The paged SP convention is **results first, then single-row count**:

```sql
CREATE PROCEDURE Catalog.sp_SearchProducts
    @TenantId NVARCHAR(50), @Search NVARCHAR(200),
    @Page INT = 1, @PageSize INT = 20
AS
BEGIN
    -- 1st result set: page of rows
    SELECT * FROM Catalog.Products
    WHERE TenantId = @TenantId AND Name LIKE '%' + @Search + '%'
    ORDER BY CreatedAt DESC
    OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;

    -- 2nd result set: single-row total
    SELECT COUNT(*) FROM Catalog.Products
    WHERE TenantId = @TenantId AND Name LIKE '%' + @Search + '%';
END
```

### Error handling — `RepositoryException`

Every SP helper catches **`System.Data.Common.DbException`** (the ADO.NET base class — covers `SqlException`, `NpgsqlException` / `PostgresException`, `OracleException`, `MySqlException`, etc.), logs it with structured fields (`{SpName}`, `{ErrorNumber}`), and rethrows as `RepositoryException`:

```csharp
try
{
    await _orderRepo.UpsertOrderAsync(order);
}
catch (RepositoryException ex) when (ex.SqlErrorNumber == 2627) // SQL Server unique violation
{
    return Result.Fail(MessageKeys.Crud.DuplicateKey);
}
catch (RepositoryException ex)
{
    _logger.LogError(ex, "Failed SP {Sp}", ex.StoredProcedure);
    return Result.Fail(MessageKeys.General.InternalError);
}
```

`RepositoryException.SqlErrorNumber` is populated for **SQL Server** (from `SqlException.Number`). For other providers it's `null` — the provider's own exception is still available via `ex.InnerException`:

```csharp
catch (RepositoryException ex)
    when (ex.InnerException is Npgsql.PostgresException pg && pg.SqlState == "23505")
{
    return Result.Fail(MessageKeys.Crud.DuplicateKey);  // Postgres unique violation
}
```

Transient errors are logged at **Warning**, not **Error**. Currently the transient list is SQL Server–specific:
- 1205 — deadlock victim
- -2 — command timeout
- 40197 — service busy (Azure SQL)
- 64 — connection dropped

When you add Postgres / Oracle / MySQL, extend `ExtractErrorNumber` in [`CachedDapperRepository.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Repositories/CachedDapperRepository.cs) to pull that provider's numeric code and extend the transient list accordingly. Non–SQL Server providers fall through to the non-transient branch today — they still get wrapped and logged, just at Error level.

## Provider Swap — SQL Server / Oracle / PostgreSQL

The Dapper layer itself is provider-agnostic — **Dapper works against any `IDbConnection`**. What's currently hardwired is (a) the `SqlConnection` factory and (b) the `catch (SqlException …)` in `CachedDapperRepository`. The SP names and param shapes must match the target DB's own conventions.

### 1. SQL Server (default)

```csharp
// appsettings.json
"DefaultConnection": "Server=localhost;Database=StarterKitMVC;Trusted_Connection=True;TrustServerCertificate=True"

// ServiceCollectionExtensions.cs (already wired)
services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(configuration.GetConnectionString("DefaultConnection")));
```

### 2. PostgreSQL (Npgsql)

Add `Npgsql` to `SmartWorkz.StarterKitMVC.Infrastructure.csproj`, then:

```csharp
// appsettings.json
"DefaultConnection": "Host=localhost;Database=starterkit;Username=app;Password=…"

// ServiceCollectionExtensions.cs
services.AddScoped<IDbConnection>(sp =>
    new Npgsql.NpgsqlConnection(configuration.GetConnectionString("DefaultConnection")));
```

SP naming on Postgres: `schema.function_name` and call via `CALL` (stored procedure) or `SELECT … FROM function(...)` (function). Dapper calls work fine — just match your SP/function signatures.

### 3. Oracle (Oracle.ManagedDataAccess.Core)

```csharp
// appsettings.json
"DefaultConnection": "User Id=app;Password=…;Data Source=//host:1521/XEPDB1"

// ServiceCollectionExtensions.cs
services.AddScoped<IDbConnection>(sp =>
    new Oracle.ManagedDataAccess.Client.OracleConnection(
        configuration.GetConnectionString("DefaultConnection")));
```

Oracle stored procedures usually return result sets via `OUT SYS_REFCURSOR` — declare the param with `OracleDbType.RefCursor, ParameterDirection.Output` using Dapper's `DynamicParameters`.

### 4. MySQL (MySqlConnector)

```csharp
services.AddScoped<IDbConnection>(sp =>
    new MySqlConnector.MySqlConnection(configuration.GetConnectionString("DefaultConnection")));
```

### Cross-provider exception handling

`CachedDapperRepository` catches the ADO.NET base type `System.Data.Common.DbException`, so **every provider's exceptions get wrapped as `RepositoryException` automatically** — no subclass required.

What you lose on non–SQL Server providers is **fidelity on the numeric error code**. `ExtractErrorNumber` pattern-matches `SqlException` today and returns `null` for anything else. To regain fidelity per provider, extend the switch:

```csharp
private static int? ExtractErrorNumber(DbException ex) => ex switch
{
    SqlException              se => se.Number,
    Npgsql.PostgresException  pg => int.TryParse(pg.SqlState, out var n) ? n : null,
    Oracle.ManagedDataAccess.Client.OracleException oe => oe.Number,
    MySqlConnector.MySqlException my => my.Number,
    _                             => null
};
```

Add the matching transient-error numbers to `LogDbError` when you do. Postgres's `SqlState` is a 5-char string — keep it as-is or map well-known codes (`23505` unique violation, `40P01` deadlock, `57014` query cancelled) to a synthetic int if you want the numeric contract.

## Samples from the Codebase

- **User queries + writes + SP sync:** [`UserRepository.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Repositories/UserRepository.cs) — good template for a production repo with roles/permissions sync.
- **Simple SP-backed lookup:** `TranslationRepository.cs`.
- **Mixed EF + Dapper:** `ProductRepository.cs` (uses EF Core for writes, Dapper SP for search).
- **List-page wiring:** `BaseListPage<T>` in [`Admin/Pages/_Base/BaseListPage.cs`](../../src/SmartWorkz.StarterKitMVC.Admin/Pages/_Base/BaseListPage.cs) — consumes `IDapperRepository<T>.GetPagedAsync`.

## Common Mistakes

- **Using `new SqlConnection(…)` directly inside a repo** → breaks DI, breaks tests, breaks provider swap. Always inject `IDbConnection`.
- **Forgetting to `InvalidateCacheKey` after a write** → stale reads until TTL expires. Cache helpers never auto-invalidate.
- **Missing `TenantId` in `param`** → rows from other tenants leak in. Every `QuerySpAsync` param must include `TenantId` unless the SP is explicitly global.
- **Returning `IEnumerable<T>` directly from a cached call** without materializing → Dapper's deferred enumeration plus the cache storing the same reference can lead to surprising "already consumed" errors. `CachedDapperRepository` already calls `ToList()` before caching; preserve that when extending.
- **Catching `SqlException` / `DbException` in the caller** → always catch `RepositoryException` instead. The provider exception has already been wrapped; reach `ex.InnerException` if you need the provider-specific type.
- **Relying on provider-specific types in the repo** (e.g. `SqlDbType`) → defeats the `IDbConnection` abstraction. Keep repos free of provider types; if you need them, use Dapper's `DynamicParameters`.

## See Also

- [00 — Getting Started (Core Setup)](./00-getting-started.md) — where `AddApplicationStack` wires the connection factory
- [11 — EF Core Repository](./11-ef-core-repository.md) — when to pick EF over Dapper
- [12 — Hybrid Cache](./12-hybrid-cache.md) — the tenant-aware L1/L2 cache (separate from the `IMemoryCache` inside `CachedDapperRepository`)
- [04 — Result Pattern](./04-result-pattern.md) — translating `RepositoryException` into `Result.Failure(messageKey)`
