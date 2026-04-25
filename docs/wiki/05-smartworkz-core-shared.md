# SmartWorkz.Core.Shared — Caching, CQRS, Logging, Webhooks

## Assembly Overview

Not a single .csproj but four source folders, each self-contained capability:

**Caching** — Query result caching with dependency tracking  
**CQRS** — Query handler pattern for read-side logic  
**Logging** — Structured logging with Serilog  
**Webhooks** — Event publishing with retry and signing

---

## 1. Caching (XML Documented)

### IQueryCacheService

GetOrSetAsync<T>(key, getter, expiration?) — cache-aside pattern  
Get/Set — synchronous operations  
Remove/RemoveByPattern/Clear — invalidation  
Exists — check key presence

### QueryCacheService

In-memory implementation backed by IMemoryCache.  
Default expiration: **1 hour**

### CacheInvalidationStrategy Time Spans

Short (5 min), Medium (30 min), Long (1 hour), VeryLong (4 hours)

### Keys Helper — Cache Key Templates

AllUsers, UserById(id), AllProducts, ProductById(id), etc.  
Format(template, args) — custom patterns

---

## 2. CQRS (XML Documented)

### IQuery<TResult> — Query Marker

Marks class as read-only query. Covariance on TResult.

### IQueryHandler<TQuery, TResult> — Query Executor

HandleAsync(query, cancellationToken) → TResult

Contravariance on TQuery.

### Usage Example

```csharp
// Define query
public class GetProductByIdQuery : IQuery<ProductDto>
{
    public int Id { get; set; }
}

// Implement handler
public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> HandleAsync(GetProductByIdQuery query, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(query.Id);
        return _mapper.Map<ProductDto>(product);
    }
}

// Register in DI
services.AddScoped<IQueryHandler<GetProductByIdQuery, ProductDto>, GetProductByIdQueryHandler>();

// Dispatch
var handler = serviceProvider.GetRequiredService<IQueryHandler<GetProductByIdQuery, ProductDto>>();
var result = await handler.HandleAsync(new GetProductByIdQuery { Id = 123 }, CancellationToken.None);
```

---

## 3. Logging (Serilog) (XML Documented)

### AddStructuredLogging Extension

Configures Serilog:  
Console sink (formatted output)  
Rolling file sink (daily roll-over)  
Environment-aware settings (dev vs prod)

### Serilog Configuration (appsettings.json)

```json
{
  "Serilog": {
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "Console", "Args": { "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}" } },
      { "Name": "File", "Args": { "path": "logs/app-.txt", "rollingInterval": "Day", "retainedFileCountLimit": 30 } }
    ]
  }
}
```

### Development vs Production

Dev: MinLevel Debug, Console enabled, 7-day retention, readable text  
Prod: MinLevel Warning, Console disabled, 30-day retention, Compact JSON

---

## 4. Webhooks (XML Documented)

### IWebhookPublisher

PublishAsync(event, ct)  
PublishToEndpointAsync(endpoint, event, ct)

### WebhookEvent — Abstract Record

Id, EventType, Timestamp, TenantId, Data (abstract)

Concrete examples: UserCreatedEvent, TransactionCompletedEvent

### WebhookEndpointRegistration

Id, TenantId, Url, SecretKey, EventTypes[], IsActive, CreatedAt, UpdatedAt  
FailureCount, LastAttemptAt — failure tracking

### WebhookSignature — HMAC-SHA256

Sign(payload, secretKey) → string  
Verify(payload, signature, secretKey) → bool

Algorithm: "sha256"

### WebhookRetryPolicy — Exponential Backoff

MaxRetries: 5  
InitialDelayMs: 1000 (1 sec)  
BackoffMultiplier: 2.0  
MaxDelayMs: 300000 (5 min)

Formula: min(1000 * 2^(attempt-1), 300000)

Delivery flow: Attempt → fail → wait 1s → retry → fail → wait 2s → ... max 5 attempts

---

## Integration Example

Combining all three:

```csharp
public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly IQueryCacheService _cache;
    private readonly ILogger<GetProductsQueryHandler> _logger;

    public async Task<IEnumerable<ProductDto>> HandleAsync(GetProductsQuery query, CancellationToken ct)
    {
        _logger.LogInformation("Getting all products (cache-aside pattern)");
        
        var result = await _cache.GetOrSetAsync(
            "products:all",
            async () =>
            {
                _logger.LogInformation("Cache miss; fetching from database");
                var products = await _repository.GetAllAsync();
                return products;
            },
            TimeSpan.FromMinutes(30)
        );

        _logger.LogInformation("Returning {Count} products", result?.Count() ?? 0);
        return result ?? Enumerable.Empty<ProductDto>();
    }
}
```

---

**Next:** [06-smartworkz-core-external.md](./06-smartworkz-core-external.md)
