# Dependency Injection Configuration Guide

## Overview: Modular DI Pattern

The SmartWorkz.Core framework employs a **modular dependency injection** pattern that decomposes application infrastructure into cohesive, independently-registerable modules. Each module encapsulates a distinct cross-cutting concern (caching, logging, webhooks, CQRS) and provides extension methods that register its services in a single call.

### Philosophy

Traditional monolithic DI registration couples infrastructure concerns, making it difficult to:
- Test individual components in isolation
- Swap implementations (e.g., in-memory cache vs. Redis)
- Understand which services depend on which
- Reuse modules across different applications

**Modular registration** solves these problems by:
1. **Composition**: Each module registers itself independently; combine modules as needed
2. **Testability**: Mock or stub individual modules during unit/integration tests
3. **Separation of Concerns**: Logging logic stays in Logging module, caching in Caching module
4. **Discoverability**: Clear naming convention (`AddXxx()`) makes registration intuitive

### Benefits

| Benefit | Explanation |
|---------|-------------|
| **Testability** | Spin up only the modules you need for a specific test scenario |
| **Composition** | Mix and match modules to build different application configurations |
| **Maintainability** | Changes to one module don't ripple through the entire DI setup |
| **Reusability** | Register the same modules in web apps, APIs, background services, etc. |
| **Documentation** | Each `AddXxx()` method documents what it registers and why |

---

## Module-by-Module Registration

### 1. Caching Module: `AddQueryCache()`

**Purpose**: Registers in-memory query result caching with configurable expiration and dependency tracking.

**What It Registers**:
- `IMemoryCache` (Singleton) - .NET Core built-in memory cache
- `IQueryCacheService` (Scoped) - SmartWorkz query cache wrapper
- `CacheInvalidationStrategy` (Static) - Predefined expiration durations and cache keys

**Options**:
```csharp
public class QueryCacheOptions
{
    /// <summary>Default expiration for cached queries (default: 1 hour).</summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromHours(1);

    /// <summary>Maximum number of cache entries before eviction (default: 10,000).</summary>
    public int SizeLimit { get; set; } = 10_000;

    /// <summary>Percentage of cache to evict when limit reached (default: 25%).</summary>
    public double CompactionPercentage { get; set; } = 0.25;
}
```

**Typical Usage**:
```csharp
services.AddMemoryCache(opts =>
{
    opts.SizeLimit = 10_000;
});

services.AddScoped<IQueryCacheService, QueryCacheService>();
```

**Common Pattern**:
```csharp
// In repository or service
var users = await _cache.GetOrSetAsync(
    "users:all",
    async () => await _db.QueryAllAsync<User>(),
    TimeSpan.FromMinutes(5)  // 5-minute expiration
);
```

---

### 2. CQRS Module: `AddQueryHandlers()`

**Purpose**: Discovers and registers query handlers that follow the CQRS (Command Query Responsibility Segregation) pattern. Each handler implements `IQueryHandler<TQuery, TResult>`.

**What It Registers**:
- All types implementing `IQueryHandler<TQuery, TResult>` (Transient)
- Query dispatcher service for routing queries to handlers
- Optional decorator for wrapping handlers (e.g., logging, caching, validation)

**Key Interfaces**:
```csharp
public interface IQuery<out TResult> { }
public interface IQueryHandler<in TQuery, out TResult> 
    where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct = default);
}
```

**Typical Usage**:
```csharp
// Register all query handlers in current assembly
services.AddQueryHandlers(typeof(Program).Assembly);

// Later: inject and dispatch
var getUersQuery = new GetAllUsersQuery();
var result = await queryDispatcher.DispatchAsync(getUsersQuery);
```

**Handler Discovery**: Auto-discovers types where:
- The type implements `IQueryHandler<T, U>` for some `T` and `U`
- The type is concrete (not abstract or interface)

**Lifecycle**: Transient (new instance per request) to ensure stateless query processing.

---

### 3. Logging Module: `AddStructuredLogging()`

**Purpose**: Configures Serilog structured logging with environment-aware sinks, JSON formatting, and log enrichment.

**What It Registers**:
- Serilog logger configuration (global `Log.Logger`)
- ILoggingProvider implementation (Serilog)
- `EnrichedLogger` wrapper (Scoped) for context-aware logging

**Configuration by Environment**:

| Environment | Console Sink | File Sink | Retention | File Size |
|-------------|-------------|----------|-----------|-----------|
| **Development** | Formatted text | JSON | 7 days | Unlimited |
| **Production** | Formatted text | JSON (compact) | 30 days | 100 MB per file |

**Enrichment**: All logs automatically include:
- `Environment` property (Development/Staging/Production)
- `Application` property (SmartWorkz)
- `Timestamp` (ISO 8601)
- `LogLevel` (Debug, Info, Warning, Error, Fatal)

**Output Template** (Console):
```
[HH:mm:ss Level] Message{NewLine}{Exception}
```

**Example Log Entry** (File, JSON):
```json
{
  "Timestamp": "2026-04-27T14:35:22.1234567Z",
  "Level": "Warning",
  "MessageTemplate": "Query execution slow: {QueryName} took {ElapsedMs}ms",
  "Properties": {
    "QueryName": "GetAllUsers",
    "ElapsedMs": 523,
    "Environment": "Production",
    "Application": "SmartWorkz"
  }
}
```

**Usage**:
```csharp
var logger = serviceProvider.GetRequiredService<ILogger<MyService>>();
logger.LogInformation("Processing user {UserId}", userId);
```

---

### 4. Services Module: `AddUserService()`

**Purpose**: Registers business-logic services that depend on infrastructure (caching, logging, repositories).

**What It Registers**:
- `IUserService` (Scoped) - User business logic
- Any dependent repositories or data access services
- Decorators (validation, authorization, audit logging)

**Lifecycle**: Scoped, since service may maintain request-specific state (current user, audit context).

**Typical Structure**:
```csharp
public interface IUserService
{
    Task<UserDto> GetUserAsync(string id);
    Task<List<UserDto>> GetAllUsersAsync();
}

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IQueryCacheService _cache;
    private readonly IDbConnection _db;

    public UserService(
        ILogger<UserService> logger,
        IQueryCacheService cache,
        IDbConnection db)
    {
        _logger = logger;
        _cache = cache;
        _db = db;
    }

    public async Task<UserDto> GetUserAsync(string id)
    {
        _logger.LogInformation("Fetching user {UserId}", id);
        return await _cache.GetOrSetAsync(
            $"user:{id}",
            async () => await _db.QuerySingleOrDefaultAsync<UserDto>(
                "SELECT * FROM Users WHERE Id = @Id",
                new { Id = id }),
            TimeSpan.FromHours(1)
        );
    }
}
```

**Registration**:
```csharp
services.AddScoped<IUserService, UserService>();
```

---

### 5. Webhooks Module: `AddWebhookSystem()`

**Purpose**: Registers event publishing and webhook delivery infrastructure for asynchronous event handling.

**What It Registers**:
- `IWebhookPublisher` (Scoped) - Event publishing and delivery
- `IWebhookRegistry` (Scoped) - Webhook endpoint registration and lookup
- `HttpClientFactory` (Singleton) - Efficient HTTP client pooling

**Webhook Flow**:
```
Event → IWebhookPublisher.PublishAsync()
   ↓
Lookup endpoints in IWebhookRegistry
   ↓
For each endpoint: POST webhook payload (signed with HMAC-SHA256)
   ↓
Handle retries (exponential backoff)
   ↓
Log success/failure
```

**Example Event Publishing**:
```csharp
public class UserCreatedEvent
{
    public string UserId { get; init; }
    public string Email { get; init; }
    public DateTime CreatedAt { get; init; }
}

// In service
var evt = new UserCreatedEvent
{
    UserId = newUser.Id,
    Email = newUser.Email,
    CreatedAt = DateTime.UtcNow
};

await _publisher.PublishAsync(evt, cancellationToken);
```

**Webhook Endpoint Registration**:
```csharp
// Register webhook endpoint (usually in admin panel)
public record WebhookEndpointRegistration
{
    public string Url { get; init; }
    public string? Secret { get; init; }  // For HMAC signing
    public List<string> EventTypes { get; init; }
    public bool IsActive { get; init; }
}
```

---

## Complete Program.cs Example

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SmartWorkz.Core.Shared.Caching;
using SmartWorkz.Core.Shared.Logging;
using SmartWorkz.Core.Shared.Services;
using SmartWorkz.Core.Shared.Webhooks.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// STEP 1: Add Core Infrastructure Services (Order Matters!)
// ============================================================================

// 1a. Logging (must be first, needed by other modules)
builder.Services.AddStructuredLogging(builder.Configuration);
var logger = Log.ForContext<Program>();

// 1b. Memory Cache (foundation for query caching)
builder.Services.AddMemoryCache(opts =>
{
    opts.SizeLimit = 10_000;
    opts.CompactionPercentage = 0.25;
});

// 1c. Query Cache Service (builds on memory cache)
builder.Services.AddScoped<IQueryCacheService, QueryCacheService>();

// ============================================================================
// STEP 2: Add Feature Modules
// ============================================================================

// 2a. CQRS Query Handlers (business logic layer)
builder.Services.AddQueryHandlers(typeof(Program).Assembly);

// 2b. Business Services (depends on cache, logging, repositories)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 2c. Webhooks (event publishing infrastructure)
builder.Services.AddWebhooks<SqlWebhookRegistry>();

// ============================================================================
// STEP 3: Add ASP.NET Core Framework Services
// ============================================================================

builder.Services.AddControllers();
builder.Services.AddHttpClient();  // HTTP client factory for external calls
builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// ============================================================================
// STEP 4: Build and Configure Middleware Pipeline
// ============================================================================

var app = builder.Build();

// Configure middleware (order matters!)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    logger.Information("Running in Development mode");
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
    logger.Information("Running in Production mode");
}

// Middleware pipeline
app.UseSerilogRequestLogging();  // Log all HTTP requests
app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

// ============================================================================
// STEP 5: Run Application
// ============================================================================

try
{
    logger.Information("Starting SmartWorkz.Core web application");
    app.Run();
}
catch (Exception ex)
{
    logger.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

**Key Points**:
1. **Order matters**: Log first (used by everything), cache before cache service
2. **Chain registrations**: Each module calls `services.AddXxx()` and returns `IServiceCollection`
3. **Use `services` alias**: Use `builder.Services` for brevity
4. **Log application state**: Log startup/shutdown events for observability
5. **Middleware order**: Logging middleware first, then routing, then features

---

## Common DI Patterns

### 1. Decorator Pattern: Cached Query Handler

Wraps a query handler to add cross-cutting concerns (caching, logging, validation).

```csharp
public class CachedQueryHandlerDecorator<TQuery, TResult> 
    : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly IQueryHandler<TQuery, TResult> _inner;
    private readonly IQueryCacheService _cache;
    private readonly ILogger<CachedQueryHandlerDecorator<TQuery, TResult>> _logger;

    public CachedQueryHandlerDecorator(
        IQueryHandler<TQuery, TResult> inner,
        IQueryCacheService cache,
        ILogger<CachedQueryHandlerDecorator<TQuery, TResult>> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResult> HandleAsync(TQuery query, CancellationToken ct = default)
    {
        var cacheKey = $"{typeof(TQuery).Name}:{query.GetHashCode()}";
        
        _logger.LogDebug("Attempting to retrieve {QueryType} from cache with key {CacheKey}",
            typeof(TQuery).Name, cacheKey);
        
        return await _cache.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                _logger.LogDebug("Cache miss for {QueryType}, executing handler", typeof(TQuery).Name);
                return await _inner.HandleAsync(query, ct);
            },
            TimeSpan.FromMinutes(5));
    }
}
```

**Registration** (wraps base handler):
```csharp
services.AddTransient(typeof(IQueryHandler<,>), typeof(BaseQueryHandler<,>));
services.Decorate(typeof(IQueryHandler<,>), typeof(CachedQueryHandlerDecorator<,>));
```

---

### 2. Factory Pattern: Platform-Specific Services

Returns different implementations based on runtime conditions.

```csharp
public interface INotificationService
{
    Task SendAsync(string message, string recipient);
}

public class NotificationServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public NotificationServiceFactory(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public INotificationService CreateNotificationService()
    {
        var provider = _configuration["Notification:Provider"] ?? "Email";

        return provider switch
        {
            "Email" => _serviceProvider.GetRequiredService<EmailNotificationService>(),
            "Sms" => _serviceProvider.GetRequiredService<SmsNotificationService>(),
            "Slack" => _serviceProvider.GetRequiredService<SlackNotificationService>(),
            _ => throw new InvalidOperationException($"Unknown notification provider: {provider}")
        };
    }
}
```

**Registration**:
```csharp
services.AddScoped<EmailNotificationService>();
services.AddScoped<SmsNotificationService>();
services.AddScoped<SlackNotificationService>();
services.AddSingleton<NotificationServiceFactory>();
```

---

### 3. Singleton vs. Scoped: Lifecycle Decisions

| Service Type | Lifetime | When to Use | Example |
|--------------|----------|------------|---------|
| **Singleton** | One instance per application | Stateless, thread-safe, expensive initialization | Cache, Configuration, Logger configuration, HTTP client factory |
| **Scoped** | One instance per HTTP request | Request-specific state, user context, audit trail | DbContext, UnitOfWork, UserService, RequestCorrelationId |
| **Transient** | New instance every time | Stateful, unique per operation | Query handlers, Command handlers, Validation rules |

**Example Decisions**:
```csharp
// ✓ Singleton: Expensive, stateless, thread-safe
services.AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));

// ✓ Scoped: Request-specific user context
services.AddScoped<ICurrentUserService, CurrentUserService>();

// ✓ Transient: New instance per query dispatch
services.AddTransient(typeof(IQueryHandler<,>), typeof(GetAllUsersQueryHandler));
```

---

### 4. Anti-Pattern: Service Locator

**❌ AVOID**: Using `IServiceProvider` to resolve dependencies at runtime.

```csharp
// BAD: Service Locator anti-pattern
public class UserService
{
    private readonly IServiceProvider _serviceProvider;

    public UserService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;  // ❌ Hides dependencies
    }

    public async Task<User> GetUserAsync(string id)
    {
        var logger = _serviceProvider.GetService<ILogger>();  // ❌ Runtime resolution
        var cache = _serviceProvider.GetService<IQueryCacheService>();  // ❌ Hidden dependency
        // ...
    }
}
```

**✓ GOOD**: Explicit constructor injection.

```csharp
public class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IQueryCacheService _cache;

    public UserService(
        ILogger<UserService> logger,
        IQueryCacheService cache)
    {
        _logger = logger;  // ✓ Clear, upfront dependency
        _cache = cache;
    }

    public async Task<User> GetUserAsync(string id)
    {
        _logger.LogInformation("Fetching user {UserId}", id);
        return await _cache.GetOrSetAsync($"user:{id}", () => FetchFromDb(id));
    }
}
```

**Why Service Locator is Bad**:
1. **Hidden dependencies**: Can't tell what a service needs from its constructor
2. **Hard to test**: Can't easily mock services used at runtime
3. **Weak contracts**: Constructor doesn't promise what will be available
4. **Fragile**: Runtime resolution failures instead of compile-time safety

---

## Troubleshooting DI Errors

### Error: "Unable to Resolve Service Type X"

**Cause**: Service type `X` is not registered in the DI container.

**Solution**:
```csharp
// Check: Is the service registered?
services.AddScoped<IUserService, UserService>();  // ✓ Add this line

// Check: Is the concrete type registered, not interface?
services.AddScoped<IUserService, UserService>();  // ✓ Correct
// NOT: services.AddScoped<UserService>();  // ❌ Only registers UserService, not IUserService
```

**Example Fix**:
```csharp
// ❌ BROKEN: GetUserAsync fails because IQueryCacheService is not registered
services.AddScoped<IUserService, UserService>();
var app = builder.Build();
var userService = app.Services.GetRequiredService<IUserService>();
// Throws: Unable to resolve service for type 'IQueryCacheService'

// ✓ FIXED: Register cache before service
services.AddMemoryCache();
services.AddScoped<IQueryCacheService, QueryCacheService>();
services.AddScoped<IUserService, UserService>();
var app = builder.Build();
var userService = app.Services.GetRequiredService<IUserService>();  // ✓ Works
```

---

### Error: "Circular Dependency Detected"

**Cause**: Service A depends on B, which depends on C, which depends on A.

**Example**:
```csharp
public class ServiceA
{
    public ServiceA(ServiceB b) { }  // A → B
}

public class ServiceB
{
    public ServiceB(ServiceC c) { }  // B → C
}

public class ServiceC
{
    public ServiceC(ServiceA a) { }  // C → A (cycle!)
}
```

**Solution**: Break the cycle with a factory or abstract interface.

```csharp
// ✓ Option 1: Lazy initialization
public class ServiceA
{
    private readonly Lazy<ServiceB> _b;
    public ServiceA(IServiceProvider sp) => _b = new(() => sp.GetRequiredService<ServiceB>());
}

// ✓ Option 2: Separate concerns
public class ServiceB
{
    // Depend on interface, not concrete type
    public ServiceB(IConfigProvider config) { }  // Break cycle
}
```

---

### Error: "Lifetime Mismatch: Scoped Service X in Singleton Service Y"

**Cause**: Singleton service depends on scoped service (scoped instance will be reused).

```csharp
// ❌ BROKEN: Singleton depends on Scoped
services.AddScoped<ICurrentUserService, CurrentUserService>();
services.AddSingleton<NotificationService>(sp =>
    new NotificationService(sp.GetRequiredService<ICurrentUserService>())  // ❌ Scoped!
);
```

**Solution**: Make dependent service scoped, or use factory.

```csharp
// ✓ FIXED: Both scoped
services.AddScoped<ICurrentUserService, CurrentUserService>();
services.AddScoped<NotificationService>();  // Scoped instead of Singleton

// ✓ OR: Use factory to defer resolution
services.AddScoped<ICurrentUserService, CurrentUserService>();
services.AddSingleton<NotificationServiceFactory>();  // Factory can create scoped instances
```

---

## Verification Checklist

- [ ] All `AddXxx()` registrations are called in `Program.cs`
- [ ] Registration order is correct (logging first, then dependencies)
- [ ] No circular dependencies between services
- [ ] Lifetime mismatches are resolved (scoped ↔ scoped, singleton ↔ singleton)
- [ ] Interfaces are registered, not just concrete types
- [ ] All constructor dependencies are declared explicitly
- [ ] Logging is configured before creating the host
- [ ] Exception handling is in place for startup failures
- [ ] Configuration is loaded before services that depend on it

---

## References

- [Microsoft DI Container Documentation](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [CQRS Pattern Overview](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [Serilog Structured Logging](https://github.com/serilog/serilog/wiki)
- [Service Lifetimes](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)
