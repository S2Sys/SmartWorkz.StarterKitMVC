# EF Core Multi-Context Migrations

## Overview

The `MigrationManager` service orchestrates database migrations across five independent Entity Framework Core DbContexts (Auth, Master, Shared, Transaction, Report) in a coordinated sequence. Each context manages its own schema and can evolve independently, while the manager ensures consistency and proper ordering during application startup. Use `MigrationManager` when your application requires:

- Multiple databases or logical database schemas in a single application
- Coordinated migrations across contexts with proper error handling and logging
- A centralized point to verify migration status and perform rollbacks
- Startup verification that all schemas are synchronized before the app begins serving requests

---

## Architecture

The migration system coordinates five DbContexts through a centralized manager that runs immediately after application build.

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        Application Startup                              │
│                                                                         │
│  var app = builder.Build();                                            │
│  ↓                                                                      │
│  MigrationManager.MigrateAsync() ← BLOCKS HERE until all done         │
│                                                                         │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │  MigrationManager Coordination Layer                            │  │
│  │                                                                  │  │
│  │  1. AuthDbContext.Database.MigrateAsync()                      │  │
│  │     ✓ Migrated [logs to ILogger]                              │  │
│  │  2. MasterDbContext.Database.MigrateAsync()                   │  │
│  │     ✓ Migrated [logs to ILogger]                              │  │
│  │  3. SharedDbContext.Database.MigrateAsync()                   │  │
│  │     ✓ Migrated [logs to ILogger]                              │  │
│  │  4. TransactionDbContext.Database.MigrateAsync()              │  │
│  │     ✓ Migrated [logs to ILogger]                              │  │
│  │  5. ReportDbContext.Database.MigrateAsync()                   │  │
│  │     ✓ Migrated [logs to ILogger]                              │  │
│  │                                                                  │  │
│  │  ┌─ On Success ─┐      ┌─ On Failure ──────────────────┐     │  │
│  │  │ App runs     │      │ throw + app.Logger.LogError() │     │  │
│  │  │ normally     │      │ Application will NOT start    │     │  │
│  │  └──────────────┘      └───────────────────────────────┘     │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  All contexts use: ConnectionStrings:DefaultConnection                │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Quick Start

Get the migration system running immediately:

```csharp
// In Program.cs, right after app.Build()
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var migrationManager = scope.ServiceProvider.GetRequiredService<IMigrationManager>();
    await migrationManager.MigrateAsync();
}

// App continues...
app.UseStaticFiles();
app.MapRazorPages();
app.Run();
```

That's it. All five contexts migrate in sequence, or the application fails to start if any migration fails.

---

## Configuration

### Connection String

All five DbContexts share a single connection string from configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SmartWorkz;Integrated Security=true;"
  }
}
```

**No dedicated migration configuration section exists.** The system uses the connection string directly from `ConnectionStrings:DefaultConnection`.

### Per-Context Customization (Advanced)

If you need a custom connection string for a specific context (not recommended), override the context's `OnConfiguring` method:

```csharp
public class ReportDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Use custom connection if needed
            var customConnection = "Server=custom-server;Database=ReportDb;...";
            optionsBuilder.UseSqlServer(customConnection);
        }
    }
}
```

This approach works but breaks the principle of centralized configuration. Use only in special circumstances (e.g., reporting database on a separate server).

---

## Usage Examples

### Example 1: Program.cs Startup Integration

This is the standard startup pattern shown in the actual application:

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;
using SmartWorkz.StarterKitMVC.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Services setup
builder.Services.AddControllers();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToFolder("/Account");
});

// Register all DbContexts and migration manager
builder.Services.AddApplicationStack(builder.Configuration);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// ─────────────────────────────────────────────────────────────
// CRITICAL: Run migrations immediately after app.Build()
// ─────────────────────────────────────────────────────────────
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var migrationManager = scope.ServiceProvider.GetRequiredService<IMigrationManager>();
        await migrationManager.MigrateAsync();
        app.Logger.LogInformation("✓ All database migrations completed successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "✗ Database migration failed - application will not start");
        throw;  // Application will NOT start
    }
}

// Continue with middleware and routing
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.Run();
```

**Key points:**
- Migrations run in a scoped service provider (creates fresh DbContext instances)
- Failure throws an exception, preventing the application from starting
- Logging shows which contexts migrated and any errors

---

### Example 2: Health Check Endpoint with Pending Migrations

Use `GetPendingMigrationsAsync()` in a health check to detect unmigrated changes:

```csharp
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IMigrationManager _migrationManager;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IMigrationManager migrationManager, ILogger<HealthController> logger)
    {
        _migrationManager = migrationManager;
        _logger = logger;
    }

    [HttpGet("migrations")]
    public async Task<IActionResult> GetMigrationStatus()
    {
        try
        {
            // IMPORTANT GOTCHA: GetPendingMigrationsAsync returns Task (NOT Task<IEnumerable>)
            // Output goes to ILogger only, NOT to the caller
            // If you need to return pending migrations to the client, use the extension below
            
            await _migrationManager.GetPendingMigrationsAsync();
            
            // This endpoint just triggers logging; pending counts appear in logs only
            return Ok(new { status = "checked", message = "See logs for pending migration count" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check migrations");
            return StatusCode(500, "Migration check failed");
        }
    }
}
```

**Gotcha:** `GetPendingMigrationsAsync()` returns `Task`, not `Task<IEnumerable<string>>`. The method logs pending migration counts but does not return them to the caller. This is a limitation of the current implementation.

**Extension Pattern** (to work around the limitation):

```csharp
public static class MigrationManagerExtensions
{
    public async Task<MigrationStatus> GetPendingMigrationsDetailedAsync(
        this IMigrationManager manager,
        AuthDbContext authDb,
        MasterDbContext masterDb,
        SharedDbContext sharedDb,
        TransactionDbContext transactionDb,
        ReportDbContext reportDb)
    {
        return new MigrationStatus
        {
            AuthPending = (await authDb.Database.GetPendingMigrationsAsync()).ToList(),
            MasterPending = (await masterDb.Database.GetPendingMigrationsAsync()).ToList(),
            SharedPending = (await sharedDb.Database.GetPendingMigrationsAsync()).ToList(),
            TransactionPending = (await transactionDb.Database.GetPendingMigrationsAsync()).ToList(),
            ReportPending = (await reportDb.Database.GetPendingMigrationsAsync()).ToList(),
        };
    }
}

public class MigrationStatus
{
    public List<string> AuthPending { get; set; }
    public List<string> MasterPending { get; set; }
    public List<string> SharedPending { get; set; }
    public List<string> TransactionPending { get; set; }
    public List<string> ReportPending { get; set; }
    
    public bool HasPending => AuthPending.Any() || MasterPending.Any() || 
                               SharedPending.Any() || TransactionPending.Any() || 
                               ReportPending.Any();
}
```

Then call from the health endpoint:

```csharp
[HttpGet("migrations/detailed")]
public async Task<IActionResult> GetMigrationStatusDetailed(
    [FromServices] AuthDbContext authDb,
    [FromServices] MasterDbContext masterDb,
    [FromServices] SharedDbContext sharedDb,
    [FromServices] TransactionDbContext transactionDb,
    [FromServices] ReportDbContext reportDb)
{
    var status = await _migrationManager.GetPendingMigrationsDetailedAsync(
        authDb, masterDb, sharedDb, transactionDb, reportDb);
    
    return Ok(new
    {
        hasPending = status.HasPending,
        pending = status
    });
}
```

---

### Example 3: Rollback with Safety Warning

Roll back all contexts to a specific migration (use with extreme caution):

```csharp
[HttpPost("migrations/rollback")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> RollbackMigration([FromBody] RollbackRequest request)
{
    // SAFETY WARNING: This deletes schema changes. Only use in development.
    // Production rollbacks should be:
    // 1. Done during maintenance windows with full backups
    // 2. Tested in staging first
    // 3. Accompanied by data migration scripts for any breaking changes
    
    if (!User.IsInRole("Admin"))
        return Forbid("Only administrators can rollback migrations");

    try
    {
        _logger.LogWarning(
            $"Admin {User.Identity.Name} initiated rollback to migration: {request.MigrationName}");
        
        await _migrationManager.RollbackAsync(request.MigrationName);
        
        _logger.LogInformation("Rollback completed successfully");
        return Ok(new { message = "Rollback completed", migration = request.MigrationName });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Rollback failed");
        return StatusCode(500, new { error = "Rollback failed", message = ex.Message });
    }
}

public class RollbackRequest
{
    public string MigrationName { get; set; }  // e.g., "20240415120000_AddUserTable"
}
```

**Safety considerations:**
- `RollbackAsync()` only removes the migration history record, not data
- Deletes rows from `[dbo].[__EFMigrationsHistory]` table where `MigrationId > {migrationName}`
- Does NOT reverse data changes (schema is still changed)
- Combine with manual data scripts if you need to preserve data
- Restrict endpoint to Admin role only

---

### Example 4: Adding a 6th DbContext to the Manager

When you need to add a new context (e.g., `AnalyticsDbContext`):

**Step 1: Create the DbContext**

```csharp
public class AnalyticsDbContext : DbContext
{
    public DbSet<Event> Events { get; set; }
    public DbSet<Metric> Metrics { get; set; }

    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure entities...
    }
}
```

**Step 2: Register in Program.cs**

```csharp
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
```

**Step 3: Update MigrationManager**

```csharp
public class MigrationManager : IMigrationManager
{
    private readonly AuthDbContext _authDb;
    private readonly MasterDbContext _masterDb;
    private readonly SharedDbContext _sharedDb;
    private readonly TransactionDbContext _transactionDb;
    private readonly ReportDbContext _reportDb;
    private readonly AnalyticsDbContext _analyticsDb;  // ← Add new context
    private readonly ILogger<MigrationManager> _logger;

    public MigrationManager(
        AuthDbContext authDb,
        MasterDbContext masterDb,
        SharedDbContext sharedDb,
        TransactionDbContext transactionDb,
        ReportDbContext reportDb,
        AnalyticsDbContext analyticsDb,  // ← Add parameter
        ILogger<MigrationManager> logger)
    {
        _authDb = authDb;
        _masterDb = masterDb;
        _sharedDb = sharedDb;
        _transactionDb = transactionDb;
        _reportDb = reportDb;
        _analyticsDb = analyticsDb;  // ← Store reference
        _logger = logger;
    }

    public async Task MigrateAsync()
    {
        try
        {
            _logger.LogInformation("Starting database migrations...");

            await _authDb.Database.MigrateAsync();
            _logger.LogInformation("✓ AuthDbContext migrated");

            await _masterDb.Database.MigrateAsync();
            _logger.LogInformation("✓ MasterDbContext migrated");

            await _sharedDb.Database.MigrateAsync();
            _logger.LogInformation("✓ SharedDbContext migrated");

            await _transactionDb.Database.MigrateAsync();
            _logger.LogInformation("✓ TransactionDbContext migrated");

            await _reportDb.Database.MigrateAsync();
            _logger.LogInformation("✓ ReportDbContext migrated");

            await _analyticsDb.Database.MigrateAsync();  // ← Add migration call
            _logger.LogInformation("✓ AnalyticsDbContext migrated");

            _logger.LogInformation("Database migrations completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database migration failed");
            throw;
        }
    }

    // Similar changes for RollbackAsync() and GetPendingMigrationsAsync()...
}
```

**Step 4: Update DI Registration**

If using an extension method to register all contexts:

```csharp
public static IServiceCollection AddApplicationStack(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // ... existing registrations ...
    
    services.AddDbContext<AnalyticsDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
    );

    services.AddScoped<IMigrationManager, MigrationManager>();

    return services;
}
```

---

## API Reference

### IMigrationManager Interface

```csharp
public interface IMigrationManager
{
    /// <summary>
    /// Runs migrations on all five DbContexts in sequence.
    /// Blocks until all migrations complete or throws on failure.
    /// Must be called immediately after app.Build() in Program.cs.
    /// </summary>
    Task MigrateAsync();

    /// <summary>
    /// Rolls back all contexts to a specific migration by clearing migration history.
    /// WARNING: Does not reverse data changes; use with caution.
    /// Requires admin authorization.
    /// </summary>
    /// <param name="migrationName">Migration ID to rollback to (e.g., "20240415120000_AddUserTable")</param>
    Task RollbackAsync(string migrationName);

    /// <summary>
    /// Logs pending migrations for all contexts.
    /// GOTCHA: Returns Task (not Task&lt;IEnumerable&gt;).
    /// Output goes to ILogger only; use extension pattern to retrieve values.
    /// </summary>
    Task GetPendingMigrationsAsync();
}
```

### MigrationManager Class

| Method | Signature | Purpose |
|--------|-----------|---------|
| `MigrateAsync()` | `public async Task MigrateAsync()` | Execute pending migrations on all 5 contexts in sequence |
| `RollbackAsync()` | `public async Task RollbackAsync(string migrationName)` | Delete migration history records after a specific migration |
| `GetPendingMigrationsAsync()` | `public async Task GetPendingMigrationsAsync()` | Log pending migration counts (does not return to caller) |

### Constructor Parameters

```csharp
public MigrationManager(
    AuthDbContext authDb,
    MasterDbContext masterDb,
    SharedDbContext sharedDb,
    TransactionDbContext transactionDb,
    ReportDbContext reportDb,
    ILogger<MigrationManager> logger)
```

All five contexts are injected via dependency injection. The logger is used for all output (logging, warnings, errors).

---

## Integration Notes

### Related Wiki Pages

- **[11-multi-tenant-architecture.md](11-multi-tenant-architecture.md)** — Overview of how the five contexts fit into the multi-tenant design
- **[10-why-tenantid-in-multiple-tables.md](10-why-tenantid-in-multiple-tables.md)** — Schema design decisions across contexts
- **[04-result-pattern.md](04-result-pattern.md)** — Error handling patterns used throughout the codebase

### How This Integrates with Other Features

1. **Startup Order:**
   - DI container registers contexts and migration manager
   - Migrations run immediately after `app.Build()`
   - Middleware and routing configured after migrations complete

2. **Logging:**
   - All output uses Serilog (configured in `Program.cs`)
   - Each context migration logged with ✓ success indicators
   - Errors logged with full exception details

3. **Error Handling:**
   - Migration failure throws exception
   - Application does NOT start if any context fails
   - Exception message contains full error details

4. **Health Checks:**
   - Use `GetPendingMigrationsAsync()` in health endpoints
   - Or create custom extension methods (see Example 2) to retrieve values programmatically

---

## Troubleshooting

### Issue 1: "GetPendingMigrationsAsync() returns Task, not Task<IEnumerable>"

**Symptom:** You call `GetPendingMigrationsAsync()` expecting to get a list of pending migrations back, but the method returns `Task` with no data.

**Root cause:** The method only logs to `ILogger`; it doesn't return data to the caller.

**Solution:** 
- Use the extension pattern from Example 2 to wrap the underlying `DbContext.Database.GetPendingMigrationsAsync()` calls
- Or read pending migrations directly from each context before calling the manager method

```csharp
var authPending = await authDb.Database.GetPendingMigrationsAsync();
var masterPending = await masterDb.Database.GetPendingMigrationsAsync();
// ... etc
```

---

### Issue 2: "Migration fails with 'Cannot open database' error"

**Symptom:** `MigrateAsync()` throws an exception about the database not existing or connection failing.

**Root cause:** Database server is not running, or connection string is incorrect.

**Solution:**
1. Verify connection string in `appsettings.json`: `ConnectionStrings:DefaultConnection`
2. Confirm SQL Server is running: `net start MSSQLSERVER` (on Windows) or check service status
3. Test connection manually: Open SQL Server Management Studio and verify you can connect
4. Check user has permission to create databases (if auto-creating)

---

### Issue 3: "Application starts but one context didn't migrate"

**Symptom:** App is running, but logs show only 4 contexts migrated, not 5.

**Root cause:** 
- An exception was silently caught somewhere
- Or migration took too long and operation timed out
- Or database permissions issue for one context

**Solution:**
1. Check logs for the specific context that didn't migrate
2. Look for timeout or permission errors
3. Verify each context has:
   - Valid `DbSet<Entity>` declarations
   - Proper fluent API configuration in `OnModelCreating()`
   - Correct migrations folder with migration classes
4. Run Entity Framework migrations manually to diagnose:

```bash
dotnet ef migrations list --context AuthDbContext
dotnet ef migrations list --context MasterDbContext
# ... etc
```

---

### Issue 4: "Rollback fails with 'Cannot delete from __EFMigrationsHistory'"

**Symptom:** `RollbackAsync()` throws an exception when trying to delete migration records.

**Root cause:**
- User/connection does not have ALTER permission on the table
- Or the `[dbo].[__EFMigrationsHistory]` table is locked
- Or migration name doesn't exist in history

**Solution:**
1. Verify the user account running the app has ALTER permission on the database
2. Stop all connections to the database and retry
3. Verify migration name is correct:

```sql
SELECT MigrationId FROM [dbo].[__EFMigrationsHistory] ORDER BY MigrationId DESC;
```

4. Manually perform rollback with SQL (advanced):

```sql
DELETE FROM [dbo].[__EFMigrationsHistory] 
WHERE MigrationId > '20240415120000_AddUserTable'
ORDER BY MigrationId DESC;
```

---

## Source Files

- **[MigrationManager.cs](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Data/Services/MigrationManager.cs)** — Full implementation (127 lines, no XML docs)
- **[Program.cs](../../src/SmartWorkz.StarterKitMVC.Public/Program.cs)** — Startup integration (lines 69-83)
