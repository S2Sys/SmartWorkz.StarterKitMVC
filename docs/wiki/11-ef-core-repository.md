# EF Core Repository

Generic LINQ-based repository over EF Core `DbContext`. Use it when you want change tracking, navigations, and LINQ queries instead of stored procedures.

## Purpose

- One generic CRUD contract (`IRepository<T>`) for any EF-mapped entity.
- Works across all five DbContexts (`MasterDbContext`, `SharedDbContext`, `TransactionDbContext`, `ReportDbContext`, `AuthDbContext`).
- Writes are **deferred** — you must call `SaveChangesAsync()` explicitly. This is intentional: it lets you batch multiple operations into one transaction.

## When to Use EF Core vs Dapper

| Situation | Pick |
|-----------|------|
| Read-heavy, SP-defined query shape, tight control of SQL | [Dapper Repository](./10-dapper-repository.md) |
| Paged list with many filters / sort options | Dapper (via SP + `QueryPagedSpAsync`) |
| Aggregate with child collections loaded together | **EF Core** (navigations + `Include`) |
| Write workflows across multiple tables in one unit of work | **EF Core** (change tracker + `SaveChangesAsync`) |
| Schema migrations managed from entities | **EF Core** |
| Reports, dashboards, complex joins, CTEs | Dapper (via SP) |

Mixing both in one repository is fine (see `ProductRepository.cs`).

## Architecture

| Component | Role | File |
|-----------|------|------|
| `IRepository<T>` | Generic CRUD contract | [`Application/Repositories/IRepository.cs`](../../src/SmartWorkz.StarterKitMVC.Application/Repositories/IRepository.cs) |
| `Repository<T>` | Default EF Core implementation | [`Infrastructure/Repositories/Repository.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Repositories/Repository.cs) |
| 5 × DbContext | One per schema (Master, Shared, Transaction, Report, Auth) | [`Infrastructure/Data/*.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Data/) |

## DI Registration

DbContexts are registered by `AddInfrastructureServices` (called from `AddApplicationStack`):

```csharp
services.AddDbContext<MasterDbContext>(o => o.UseSqlServer(cs));
services.AddDbContext<SharedDbContext>(o => o.UseSqlServer(cs));
services.AddDbContext<TransactionDbContext>(o => o.UseSqlServer(cs));
services.AddDbContext<ReportDbContext>(o => o.UseSqlServer(cs));
services.AddDbContext<AuthDbContext>(o => o.UseSqlServer(cs));
```

Typed repositories are registered by `AddRepositories` and choose the **correct DbContext** in their constructor:

```csharp
public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    private readonly MasterDbContext _master;
    public TenantRepository(MasterDbContext context) : base(context)
        => _master = context;
}
```

If you need a generic `IRepository<T>` resolvable by any entity type, add one extra registration per entity (EF can't infer the right DbContext automatically):

```csharp
services.AddScoped<IRepository<Tenant>>(sp =>
    new Repository<Tenant>(sp.GetRequiredService<MasterDbContext>()));
```

## Writing a Repository

Inherit `Repository<T>`, pass the right DbContext, add your domain-specific queries:

```csharp
using Microsoft.EntityFrameworkCore;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Master;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    private readonly MasterDbContext _ctx;

    public TenantRepository(MasterDbContext context) : base(context) => _ctx = context;

    public Task<Tenant?> GetByNameAsync(string name) =>
        _ctx.Tenants.FirstOrDefaultAsync(t => t.Name == name && !t.IsDeleted);

    public Task<List<Tenant>> GetActiveTenantsAsync() =>
        _ctx.Tenants.Where(t => t.IsActive && !t.IsDeleted).ToListAsync();
}
```

## Method Reference

All methods are `virtual` — override in derived classes when you need `.Include()`, `AsNoTracking()`, projections, or tenant scoping.

### Read

```csharp
Task<T>              GetByIdAsync(int id);
Task<T>              GetByIdAsync(string id);
Task<IEnumerable<T>> GetAllAsync();
Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
Task<T>              FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
Task<int>            CountAsync(Expression<Func<T, bool>> predicate = null);
Task<bool>           AnyAsync(Expression<Func<T, bool>> predicate);
```

```csharp
// Single by PK
var tenant = await repo.GetByIdAsync("ACME");

// Predicate
var active = await repo.FindAsync(t => t.IsActive && !t.IsDeleted);

// First match
var admin = await repo.FirstOrDefaultAsync(t => t.Name == "Admin");

// Existence / count
var exists = await repo.AnyAsync(t => t.Email == email);
var total  = await repo.CountAsync(t => !t.IsDeleted);
```

For tenant-scoped lookups always add the filter yourself — the generic repo **does not** auto-apply `TenantId`:

```csharp
Task<List<Product>> FindByTenantAsync(string tenantId) =>
    _ctx.Products
        .Where(p => p.TenantId == tenantId && !p.IsDeleted)
        .AsNoTracking()
        .ToListAsync();
```

### Write (deferred)

```csharp
Task AddAsync(T entity);
Task AddRangeAsync(IEnumerable<T> entities);
void Update(T entity);
void UpdateRange(IEnumerable<T> entities);
void Remove(T entity);
void RemoveRange(IEnumerable<T> entities);
Task SaveChangesAsync();
```

None of these hit the DB on their own. The change tracker records them; `SaveChangesAsync` flushes:

```csharp
public async Task<Result<Tenant>> CreateTenantAsync(Tenant tenant)
{
    if (await _repo.AnyAsync(t => t.Name == tenant.Name))
        return Result<Tenant>.Failure(MessageKeys.Tenant.DuplicateName);

    tenant.CreatedAt = DateTime.UtcNow;
    await _repo.AddAsync(tenant);
    await _repo.SaveChangesAsync();   // ← single DB round trip for all pending changes
    return Result<Tenant>.Success(tenant);
}
```

For multi-table transactions, wrap `SaveChangesAsync` in an explicit transaction scope on the underlying DbContext (EF's default behaviour already wraps a single `SaveChangesAsync` in a transaction).

### Soft delete

`Remove` issues a hard `DELETE`. For the soft-delete pattern used elsewhere (`IsDeleted = 1, UpdatedAt = now`), either:

```csharp
public async Task SoftDeleteAsync(T entity)
{
    if (entity is ISoftDelete s)
    {
        s.IsDeleted = true;
        s.UpdatedAt = DateTime.UtcNow;
        Update(entity);
        await SaveChangesAsync();
    }
}
```

…or call `sp_Soft…` via Dapper. The codebase uses both patterns depending on repo.

## Provider Swap — SQL Server / PostgreSQL / Oracle

EF Core provider is set by the `Use…` call on each DbContext registration. Swap all five consistently.

### PostgreSQL (Npgsql.EntityFrameworkCore.PostgreSQL)

```csharp
services.AddDbContext<MasterDbContext>(o =>
    o.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
// repeat for Shared, Transaction, Report, Auth
```

PostgreSQL quirks to watch for:
- Identifiers are lower-cased by default — configure via `[Table("Tenants", Schema = "master")]` or model builder.
- `DateTime` must be `DateTimeKind.Utc` for `timestamp with time zone` columns — set in entity or use `EnableLegacyTimestampBehavior`.

### Oracle (Oracle.EntityFrameworkCore)

```csharp
services.AddDbContext<MasterDbContext>(o =>
    o.UseOracle(configuration.GetConnectionString("DefaultConnection")));
```

Oracle quirks: identifier length ≤ 30 chars on older versions; no native `bool` (maps to `NUMBER(1)`); sequences instead of `IDENTITY` for some entities.

### MySQL (Pomelo.EntityFrameworkCore.MySql)

```csharp
services.AddDbContext<MasterDbContext>(o =>
    o.UseMySql(cs, ServerVersion.AutoDetect(cs)));
```

### SQLite (for tests)

```csharp
services.AddDbContext<MasterDbContext>(o => o.UseSqlite("Data Source=:memory:"));
```

> Whichever provider you pick, keep all five DbContexts on the **same** provider. Mixing providers (e.g. Master on Postgres, Auth on SQL Server) is possible but loses the "one `DefaultConnection`" simplicity that every other component assumes.

## Common Mistakes

- **Forgetting `SaveChangesAsync`** — `AddAsync`/`Update`/`Remove` alone don't persist. The change tracker silently holds them.
- **Re-injecting `DbContext` across scopes** — `Repository<T>` takes the scoped DbContext; don't store it on a singleton.
- **Tracking reads that should be read-only** — use `AsNoTracking()` on list/report queries to cut memory and CPU.
- **Cross-context updates in one SaveChanges** — each DbContext has its own change tracker. Saving on `MasterDbContext` won't flush pending changes in `AuthDbContext`.
- **Ignoring `TenantId` in predicates** — the generic repo is tenant-blind by design. Always filter at the call site.
- **Calling `GetByIdAsync(id)` expecting a tenant check** — there isn't one. Use `FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId)` for anything tenant-owned.
- **Using `Update(entity)` for a disconnected entity that wasn't loaded** — works, but overwrites every column. For partial updates, load then modify tracked properties.

## See Also

- [10 — Dapper Repository](./10-dapper-repository.md) — the SP-backed alternative for read-heavy paths
- [00 — Getting Started](./00-getting-started.md) — DbContext registration happens inside `AddApplicationStack`
- [04 — Result Pattern](./04-result-pattern.md) — returning Result from repo-backed services
