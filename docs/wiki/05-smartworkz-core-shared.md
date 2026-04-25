# SmartWorkz.Core.Shared — Cross-Cutting Concerns

## Assembly Overview

`SmartWorkz.Core.Shared` is not a single .csproj but four related source folders, each a self-contained capability:
- **Caching** — Query result caching with dependency tracking
- **CQRS** — Query handler pattern for read-side logic
- **Logging** — Structured logging with Serilog
- **Webhooks** — Event publishing with retry and signing

---

## 1. Caching

**Namespace:** `SmartWorkz.Core.Shared.Caching`

### IQueryCacheService — Async Cache Abstraction

```csharp
public interface IQueryCacheService
{
    // Get or set with async getter
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> getter, TimeSpan? expiration = null);
    
    // Synchronous operations
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? expiration = null);
    
    // Invalidation
    void Remove(string key);
    void RemoveByPattern(string pattern);  // Remove all keys matching pattern
    void Clear();                           // Remove all
    
    // Existence check
    bool Exists(string key);
}
```

### QueryCacheService — In-Memory Implementation

Backed by `IMemoryCache`. Default expiration: **1 hour**.

```csharp
var service = _serviceProvider.GetRequiredService<IQueryCacheService>();

// Get or set pattern (cache-aside)
var users = await service.GetOrSetAsync(
    "users:all",
    async () => await _repository.GetAllUsersAsync(),
    TimeSpan.FromMinutes(30)
);
```

### CacheInvalidationStrategy — Pre-Defined Patterns

Predefined time spans for common cache durations:

| Span | Duration | Use Case |
|------|----------|----------|
| `Short` | 5 min | Frequently-changing data (stock prices) |
| `Medium` | 30 min | User preferences, product details |
| `Long` | 1 hour | Configuration, roles, permissions |
| `VeryLong` | 4 hours | Static reference data |

**Keys Helper — Standard Cache Key Templates:**
```csharp
public class Keys
{
    public static string AllUsers => "users:all";
    public static string UserById(int id) => $"user:{id}";
    public static string UsersByDept(int deptId) => $"users:dept:{deptId}";
    
    public static string AllProducts => "products:all";
    public static string ProductById(int id) => $"product:{id}";
    
    public static string AllRoles => "roles:all";
    public static string RoleById(int id) => $"role:{id}";
    
    // Custom format method
    public static string Format(string template, params object[] args)
        => string.Format(template, args);
}
```

**Usage:**
```csharp
await _cacheService.RemoveByPattern(CacheInvalidationStrategy.Keys.Format("user:{0}", userId));
// Removes user:123, user:456, etc.
```

---

## 2. CQRS

**Namespace:** `SmartWorkz.Core.Shared.CQRS`

### IQuery<TResult> — Query Marker Interface

Marks a class as a query object (read-only, no state changes).

```csharp
public interface IQuery<out TResult>
{
    // No members; pure marker for compile-time safety
}
```

**Covariance:** `out TResult` means if you have `IQuery<Animal>`, you can use it where `IQuery<Dog>` is expected (Dog is-a Animal).

### IQueryHandler<TQuery, TResult> — Query Executor

```csharp
public interface IQueryHandler<in TQuery, out TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
```

**Contravariance on TQuery:** `in TQuery` means if handler accepts `Animal` queries, it can handle `Dog` queries too.

### Implementation Example

```csharp
// Query definition
public class GetProductByIdQuery : IQuery<ProductDto>
{
    public int Id { get; set; }
}

// Query handler
public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductRepository _repository;
    
    public GetProductByIdQueryHandler(IProductRepository repository)
        => _repository = repository;
    
    public async Task<ProductDto> HandleAsync(GetProductByIdQuery query, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(query.Id);
        return _mapper.Map<ProductDto>(product);
    }
}
```

### DI Registration & Dispatch

```csharp
// Register (in Program.cs)
builder.Services.AddScoped<IQueryHandler<GetProductByIdQuery, ProductDto>, GetProductByIdQueryHandler>();

// Dispatch (in page model or service)
var handler = _serviceProvider.GetRequiredService<IQueryHandler<GetProductByIdQuery, ProductDto>>();
var result = await handler.HandleAsync(
    new GetProductByIdQuery { Id = 123 },
    CancellationToken.None
);
```

---

## 3. Logging (Serilog)

**Namespace:** `SmartWorkz.Core.Shared.Logging`

### AddStructuredLogging Extension

Configures Serilog with console and rolling file sinks, environment-aware settings.

```csharp
public static IServiceCollection AddStructuredLogging(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Reads "Serilog" section from appsettings.json
    // Configures:
    // - MinimumLevel: Debug
    // - Console sink with template
    // - Rolling file sink (daily roll-over)
    // - Environment-specific settings (dev vs prod)
}
```

### appsettings.json Configuration

```json
{
  "Serilog": {
    "MinimumLevel": "Debug",
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.txt",
          "rollingInterval": "Day",
          "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter",
          "retainedFileCountLimit": 30,
          "fileSizeLimitBytes": 104857600
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithProperty"],
    "Properties": {
      "Environment": "Development",
      "Application": "SmartWorkz"
    }
  }
}
```

### Development vs Production

| Setting | Development | Production |
|---------|-------------|----------|
| **MinLevel** | Debug | Warning |
| **Console** | Enabled | Disabled |
| **File Retention** | 7 days | 30 days |
| **File Size** | 10 MB | 100 MB |
| **Format** | Readable text | Compact JSON |

### Usage in Code

```csharp
private readonly ILogger<ProductService> _logger;

public async Task<Result<ProductDto>> GetByIdAsync(int id)
{
    _logger.LogInformation("Fetching product {ProductId}", id);
    
    try
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Product {ProductId} not found", id);
            return Result<ProductDto>.Failure("NOT_FOUND", "Product not found");
        }
        
        _logger.LogInformation("Product {ProductId} retrieved successfully", id);
        return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error fetching product {ProductId}", id);
        throw;
    }
}
```

---

## 4. Webhooks

**Namespace:** `SmartWorkz.Core.Shared.Webhooks`

### IWebhookPublisher — Event Publisher

```csharp
public interface IWebhookPublisher
{
    // Publish to all registered endpoints
    Task PublishAsync(WebhookEvent @event, CancellationToken ct);
    
    // Publish to specific endpoint
    Task PublishToEndpointAsync(
        WebhookEndpointRegistration endpoint,
        WebhookEvent @event,
        CancellationToken ct);
}
```

### WebhookEvent — Abstract Event Base

```csharp
public abstract record WebhookEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public abstract string EventType { get; set; }  // "user.created", "order.shipped", etc.
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string TenantId { get; set; }
    public abstract object Data { get; set; }       // Event-specific payload
}
```

**Concrete Event Examples:**

```csharp
public record UserCreatedEvent : WebhookEvent
{
    public override string EventType => "user.created";
    public override object Data { get; set; }  // UserDto
}

public record TransactionCompletedEvent : WebhookEvent
{
    public override string EventType => "transaction.completed";
    public override object Data { get; set; }  // TransactionDto
}
```

### WebhookEndpointRegistration — Subscription Target

```csharp
public class WebhookEndpointRegistration
{
    public string Id { get; set; }                      // Unique endpoint ID
    public string TenantId { get; set; }                // Tenant that owns this subscription
    public string Url { get; set; }                     // POST target
    public string SecretKey { get; set; }               // For HMAC signing
    public List<string> EventTypes { get; set; }        // Which events to deliver
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Failure tracking
    public int FailureCount { get; set; }
    public DateTime? LastAttemptAt { get; set; }
}
```

### WebhookSignature — HMAC-SHA256 Signing

```csharp
public static class WebhookSignature
{
    public const string Algorithm = "sha256";
    
    // Sign a payload
    public static string Sign(WebhookPayload payload, string secretKey);
    public static string Sign(string json, string secretKey);
    
    // Verify signature
    public static bool Verify(WebhookPayload payload, string signature, string secretKey);
}
```

**Usage:**
```csharp
var payload = new WebhookPayload { Event = @event, Timestamp = DateTime.UtcNow };
var signature = WebhookSignature.Sign(payload, endpoint.SecretKey);

// Receiver verifies:
var valid = WebhookSignature.Verify(receivedPayload, receivedSignature, secretKey);
```

### WebhookRetryPolicy — Exponential Backoff

```csharp
public class WebhookRetryPolicy
{
    public const int MaxRetries = 5;
    public const int InitialDelayMs = 1000;           // 1 second
    public const double BackoffMultiplier = 2.0;      // Double each time
    public const int MaxDelayMs = 300_000;            // Cap at 5 minutes
    
    public static int GetRetryDelayMs(int attemptNumber)
    {
        // Formula: min(1000 * 2^(attempt-1), 300000)
        // Attempt 1: 1s
        // Attempt 2: 2s
        // Attempt 3: 4s
        // Attempt 4: 8s
        // Attempt 5: 16s
    }
}
```

**Delivery Flow:**
1. Webhook publisher attempts POST to endpoint URL with signed payload
2. If fails, wait 1s, retry
3. If fails again, wait 2s, retry
4. If still failing, backoff exponentially up to 5 minutes
5. After 5 failed attempts, mark endpoint as stale and alert operator

---

## Integration Pattern: Caching + CQRS + Logging

Typical usage combining all three:

```csharp
public class GetProductsQuery : IQuery<IEnumerable<ProductDto>> { }

public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly IQueryCacheService _cache;
    private readonly ILogger<GetProductsQueryHandler> _logger;
    private readonly IMapper _mapper;
    
    public async Task<IEnumerable<ProductDto>> HandleAsync(
        GetProductsQuery query,
        CancellationToken ct)
    {
        _logger.LogInformation("Getting all products (cache-aside pattern)");
        
        var result = await _cache.GetOrSetAsync(
            CacheInvalidationStrategy.Keys.AllProducts,
            async () =>
            {
                _logger.LogInformation("Cache miss; fetching from database");
                var products = await _repository.GetAllAsync();
                return _mapper.Map<IEnumerable<ProductDto>>(products);
            },
            CacheInvalidationStrategy.Medium
        );
        
        _logger.LogInformation("Returning {Count} products", result?.Count() ?? 0);
        return result ?? Enumerable.Empty<ProductDto>();
    }
}
```

---

**Next:** [SmartWorkz.Core.External — PDF & Excel Export](06-smartworkz-core-external.md)
