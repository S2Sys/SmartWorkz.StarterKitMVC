# Error Handling & Resilience Guide

## Overview

This guide defines error handling philosophy, exception hierarchy, retry policies, and resilience patterns across SmartWorkz.Core.Shared modules. All error handling must prioritize observability, graceful degradation, and clear recovery paths.

---

## 1. Error Handling Philosophy

### Fail-Safe Principles

1. **Fail Gracefully**: Prefer partial success over total failure
   - Cache misses should not break application flow
   - Webhook delivery failures should not block primary operations
   - Log configuration errors should not prevent startup

2. **Observability First**: Every error must be observable
   - All errors must be logged with context
   - Include CorrelationId for distributed tracing
   - Provide structured fields for monitoring

3. **Graceful Degradation**: Maintain functionality at reduced capacity
   - Cache unavailable → query source directly
   - Webhook delivery fails → retry asynchronously
   - Non-critical service down → continue with fallbacks

### When to Throw vs. Return Errors

| Scenario | Action | Rationale |
|----------|--------|-----------|
| Invalid input (null, validation) | **Throw** ArgumentException/ValidationException | Programming error, fail fast |
| Configuration missing | **Throw** ConfigurationException | Prevents silent failures |
| Resource not found | **Return null/empty** or throw NotFoundException | Depends on context |
| Transient network failure | **Return error result** + retry | Expected, recoverable |
| Authentication/authorization failure | **Throw** UnauthorizedException | Security-critical |
| Service call timeout | **Return error** + apply fallback | Expected in distributed systems |
| Cache miss | **Return null** (not an error) | Normal operation |
| Webhook signature mismatch | **Throw** SignatureVerificationException | Security-critical |

---

## 2. Exception Hierarchy

### Base Exception Architecture

```
System.Exception
├── SmartWorkz.Core.Shared.ApplicationException (base for all custom)
│   ├── BusinessException (domain logic violations)
│   ├── ValidationException (input validation failures)
│   ├── NotFoundException (resource not found)
│   ├── UnauthorizedException (authentication/authorization)
│   └── Module-Specific Exceptions
│       ├── CacheException (caching failures)
│       ├── QueryDispatchException (CQRS dispatch failures)
│       ├── HandlerNotFoundException (handler resolution)
│       ├── LoggingException (logging framework errors)
│       ├── ConfigurationException (config validation)
│       ├── WebhookException (webhook infrastructure)
│       ├── SignatureVerificationException (security)
│       └── DeliveryException (webhook delivery)
```

### Per-Module Exception Definitions

#### Caching Module
```csharp
namespace SmartWorkz.Core.Shared.Caching.Exceptions;

/// <summary>Thrown when cache operations fail.</summary>
public class CacheException : ApplicationException
{
    public CacheException(string message) : base(message) { }
    public CacheException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Thrown when cache key is invalid.</summary>
public class InvalidCacheKeyException : ArgumentException
{
    public InvalidCacheKeyException(string key) 
        : base($"Cache key '{key}' is invalid") { }
}
```

#### CQRS Module
```csharp
namespace SmartWorkz.Core.Shared.CQRS.Exceptions;

/// <summary>Thrown when query dispatch fails.</summary>
public class QueryDispatchException : ApplicationException
{
    public string? QueryTypeName { get; }
    public QueryDispatchException(string message, string? queryType = null) 
        : base(message) => QueryTypeName = queryType;
}

/// <summary>Thrown when no handler found for query type.</summary>
public class HandlerNotFoundException : QueryDispatchException
{
    public Type? QueryType { get; }
    public HandlerNotFoundException(Type queryType) 
        : base($"No handler registered for query: {queryType.Name}", queryType.Name)
        => QueryType = queryType;
}
```

#### Logging Module
```csharp
namespace SmartWorkz.Core.Shared.Logging.Exceptions;

/// <summary>Thrown when logging configuration is invalid.</summary>
public class LoggingException : ApplicationException
{
    public LoggingException(string message) : base(message) { }
    public LoggingException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Thrown when logger configuration fails validation.</summary>
public class ConfigurationException : LoggingException
{
    public ConfigurationException(string parameter) 
        : base($"Logging configuration error: {parameter}") { }
}
```

#### Services Module
```csharp
namespace SmartWorkz.Core.Shared.Services.Exceptions;

/// <summary>Thrown for service operation failures.</summary>
public class ServiceException : ApplicationException
{
    public ServiceException(string message) : base(message) { }
    public ServiceException(string message, Exception inner) : base(message, inner) { }
}
```

#### Webhooks Module
```csharp
namespace SmartWorkz.Core.Shared.Webhooks.Exceptions;

/// <summary>Thrown for webhook infrastructure errors.</summary>
public class WebhookException : ApplicationException
{
    public WebhookException(string message) : base(message) { }
    public WebhookException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Thrown when webhook signature verification fails.</summary>
public class SignatureVerificationException : WebhookException
{
    public SignatureVerificationException(string expectedSignature, string receivedSignature)
        : base($"Signature mismatch: expected {expectedSignature}, got {receivedSignature}") { }
}

/// <summary>Thrown when webhook delivery fails after retries.</summary>
public class DeliveryException : WebhookException
{
    public int Attempt { get; }
    public DeliveryException(string message, int attempt, Exception? inner = null)
        : base($"Webhook delivery failed at attempt {attempt}: {message}", inner)
        => Attempt = attempt;
}
```

---

## 3. Retry Policy

### Exponential Backoff Formula

```
delay(n) = initialDelay * (multiplier ^ (n - 1))
         = 1s * (2 ^ (attemptNumber - 1))

Example progression (default settings):
- Attempt 1: 1s delay
- Attempt 2: 2s delay
- Attempt 3: 4s delay
- Attempt 4: 8s delay
- Attempt 5: 16s delay
- Max: 300s (5 minutes)
```

### Jitter Application

Add randomization to prevent thundering herd:

```csharp
// Apply ±10% jitter
var jitterAmount = delay * 0.1; // 10% of delay
var jitter = Random.Shared.NextDouble() * jitterAmount * 2 - jitterAmount; // [-10%, +10%]
var finalDelay = delay + jitter;
```

### Retry Policy Configuration

```csharp
public class RetryPolicyConfiguration
{
    /// <summary>Maximum retry attempts (default: 5)</summary>
    public int MaxRetries { get; set; } = 5;
    
    /// <summary>Initial delay in milliseconds (default: 1000ms = 1s)</summary>
    public int InitialDelayMs { get; set; } = 1000;
    
    /// <summary>Exponential multiplier (default: 2.0)</summary>
    public double Multiplier { get; set; } = 2.0;
    
    /// <summary>Maximum delay between retries (default: 300000ms = 5min)</summary>
    public int MaxDelayMs { get; set; } = 300_000;
    
    /// <summary>Enable jitter randomization (default: true)</summary>
    public bool ApplyJitter { get; set; } = true;
}
```

### When to Retry vs. Fail Fast

| Error Type | Transient? | Action |
|-----------|-----------|--------|
| Network timeout | Yes | **Retry** with backoff |
| Connection refused | Yes | **Retry** (service may be starting) |
| DNS lookup failure | Yes | **Retry** |
| HTTP 429 (Too Many Requests) | Yes | **Retry** with longer backoff |
| HTTP 503 (Service Unavailable) | Yes | **Retry** with exponential backoff |
| HTTP 500 (Internal Server Error) | Maybe | **Retry** 1-2 times, then fail |
| HTTP 400 (Bad Request) | No | **Fail fast** (client error) |
| HTTP 401 (Unauthorized) | No | **Fail fast** (auth error) |
| HTTP 403 (Forbidden) | No | **Fail fast** (authorization error) |
| HTTP 404 (Not Found) | No | **Fail fast** (resource doesn't exist) |
| Invalid signature | No | **Fail fast** (security violation) |
| Invalid configuration | No | **Fail fast** (programming error) |

---

## 4. Error Response Format

### HTTP Status Codes

```
400 Bad Request      → Validation error, malformed request
401 Unauthorized     → Missing or invalid authentication
403 Forbidden        → Authentication valid, but insufficient permissions
404 Not Found        → Resource doesn't exist
409 Conflict         → State conflict (e.g., duplicate entity)
429 Too Many Requests → Rate limit exceeded
500 Internal Server Error → Unhandled server error
503 Service Unavailable  → Service temporarily down (retry-safe)
```

### Error Response Schema (REST/GraphQL)

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Request validation failed",
    "status": 400,
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "timestamp": "2026-04-27T10:30:00Z",
    "details": [
      {
        "field": "email",
        "message": "Invalid email format"
      }
    ],
    "traceId": "0HN1GKJS3Q2EC:00000001"  // Production: omitted or hashed
  }
}
```

### GraphQL Error Format

```json
{
  "errors": [
    {
      "message": "Authentication required",
      "extensions": {
        "code": "UNAUTHENTICATED",
        "statusCode": 401,
        "correlationId": "550e8400-e29b-41d4-a716-446655440000",
        "timestamp": "2026-04-27T10:30:00Z",
        "traceId": "0HN1GKJS3Q2EC:00000001"
      }
    }
  ]
}
```

### Stack Traces & Sensitive Data

```csharp
// Development: Include full stack trace
if (isDevelopment)
{
    response.Error.StackTrace = exception.StackTrace;
    response.Error.InnerException = exception.InnerException?.Message;
}

// Production: Never expose internal details
// Only include generic message and correlationId
response.Error.Message = "An error occurred processing your request";
response.Error.CorrelationId = correlationId;
// TraceId can be hashed or omitted
response.Error.TraceId = SHA256Hash(traceId); // Optional, hashed only
```

---

## 5. Logging & Correlation IDs

### CorrelationId Propagation

```csharp
// 1. Extract from request header (HTTP middleware)
var correlationId = httpContext.Request.Headers["X-Correlation-ID"].ToString()
    ?? Guid.NewGuid().ToString();
httpContext.Items["CorrelationId"] = correlationId;

// 2. Add to response header
httpContext.Response.Headers.Add("X-Correlation-ID", correlationId);

// 3. Propagate to downstream calls
using (LogContext.PushProperty("CorrelationId", correlationId))
{
    // All logs in this scope include CorrelationId
}
```

### Structured Logging Fields

Every application log should include:

```json
{
  "@timestamp": "2026-04-27T10:30:00Z",
  "level": "Error",
  "message": "Webhook delivery failed",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "requestId": "req-12345",
  "userId": "user-999",
  "tenantId": "tenant-888",
  "module": "Webhooks",
  "operation": "PublishEvent",
  "duration_ms": 1250,
  "attempt": 3,
  "error": {
    "type": "DeliveryException",
    "message": "HTTP 503 Service Unavailable",
    "innerException": "Connection timeout"
  },
  "context": {
    "webhookEndpointId": "endpoint-111",
    "eventType": "UserCreated",
    "endpointUrl": "[REDACTED]"
  }
}
```

### Log Levels

```
Critical (5) → System-wide failure, immediate action required
Error (4)    → Operation failed, recovery possible
Warning (3)  → Unexpected behavior, potential issue
Information (2) → Normal operations, audit trail
Debug (1)    → Diagnostic information
```

---

## 6. Per-Module Error Handling

### Webhooks Module

**Publish with Retry + Signature Verification**

```csharp
public class WebhookPublisher : IWebhookPublisher
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<WebhookPublisher> _logger;
    private readonly WebhookRetryPolicy _retryPolicy;

    public async Task PublishAsync(WebhookPayload payload, WebhookEndpointRegistration endpoint, CancellationToken cancellationToken)
    {
        using var activity = new Activity("PublishWebhook").Start();
        var correlationId = activity.Id ?? Guid.NewGuid().ToString();
        
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            int attempt = 0;
            while (attempt < _retryPolicy.MaxRetries)
            {
                attempt++;
                try
                {
                    // Sign the payload
                    var signature = WebhookSignature.Sign(payload, endpoint.SecretKey);
                    
                    // POST with signature header
                    using var client = _httpFactory.CreateClient();
                    using var request = new HttpRequestMessage(HttpMethod.Post, endpoint.Url);
                    request.Headers.Add("X-Webhook-Signature", signature);
                    request.Headers.Add("X-Webhook-Id", payload.Id);
                    request.Headers.Add("X-Correlation-ID", correlationId);
                    request.Content = new StringContent(
                        JsonSerializer.Serialize(payload),
                        Encoding.UTF8,
                        "application/json");
                    
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(TimeSpan.FromSeconds(30)); // 30s timeout
                    
                    var response = await client.SendAsync(request, cts.Token);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation(
                            "Webhook published successfully to {Endpoint} (attempt {Attempt})",
                            endpoint.Url, attempt);
                        return;
                    }
                    
                    if ((int)response.StatusCode >= 500 && response.StatusCode != System.Net.HttpStatusCode.NotImplemented)
                    {
                        // Server error, retry
                        var delay = CalculateRetryDelay(attempt);
                        _logger.LogWarning(
                            "Webhook delivery failed with {StatusCode} to {Endpoint}, retrying in {DelayMs}ms (attempt {Attempt})",
                            response.StatusCode, endpoint.Url, delay, attempt);
                        await Task.Delay(delay, cancellationToken);
                        continue;
                    }
                    
                    // Client error, fail fast
                    throw new DeliveryException(
                        $"HTTP {response.StatusCode}: {await response.Content.ReadAsStringAsync()}",
                        attempt);
                }
                catch (HttpRequestException ex) when (attempt < _retryPolicy.MaxRetries && IsTransientError(ex))
                {
                    var delay = CalculateRetryDelay(attempt);
                    _logger.LogWarning(ex,
                        "Webhook delivery failed with transient error to {Endpoint}, retrying in {DelayMs}ms (attempt {Attempt})",
                        endpoint.Url, delay, attempt);
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }
                catch (OperationCanceledException)
                {
                    throw new DeliveryException("Request timeout", attempt);
                }
            }
            
            throw new DeliveryException(
                $"Max retries ({_retryPolicy.MaxRetries}) exceeded",
                attempt);
        }
    }

    private int CalculateRetryDelay(int attempt)
    {
        var baseDelay = (int)(_retryPolicy.InitialDelayMs * Math.Pow(_retryPolicy.BackoffMultiplier, attempt - 1));
        var capped = Math.Min(baseDelay, _retryPolicy.MaxDelayMs);
        
        if (_retryPolicy.ApplyJitter)
        {
            var jitter = Random.Shared.NextDouble() * capped * 0.2 - capped * 0.1;
            return (int)(capped + jitter);
        }
        return capped;
    }

    private bool IsTransientError(HttpRequestException ex)
        => ex.InnerException is TimeoutException
        || ex.InnerException is IOException
        || ex.InnerException is System.Net.Sockets.SocketException;
}
```

### Cache Module

**Miss Strategy with Fallback**

```csharp
public class CacheWithFallback
{
    private readonly IMemoryCache _cache;
    private readonly IDataSource _source;
    private readonly ILogger<CacheWithFallback> _logger;

    public async Task<T?> GetWithFallbackAsync<T>(
        string key,
        Func<Task<T>> loader,
        TimeSpan? expiration = null)
    {
        try
        {
            // Try cache first
            if (_cache.TryGetValue(key, out T? cachedValue))
            {
                _logger.LogDebug("Cache hit for key {Key}", key);
                return cachedValue;
            }
            
            _logger.LogDebug("Cache miss for key {Key}, loading from source", key);
            
            // Cache miss, load from source
            var value = await loader();
            
            if (value != null)
            {
                try
                {
                    _cache.Set(key, value, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
                    });
                }
                catch (Exception ex)
                {
                    // Cache write failed, log but don't fail the operation
                    _logger.LogWarning(ex,
                        "Failed to cache value for key {Key}, returning uncached value", key);
                }
            }
            
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error loading value for key {Key} from source", key);
            
            // Attempt to return stale cache entry as fallback
            if (_cache.TryGetValue($"{key}:stale", out T? staleValue))
            {
                _logger.LogWarning("Returning stale cache entry as fallback for key {Key}", key);
                return staleValue;
            }
            
            // No fallback available, propagate error
            throw new CacheException($"Failed to retrieve value for key {key}", ex);
        }
    }
}
```

### CQRS Module

**Handler Resolution with Diagnostics**

```csharp
public class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _services;
    private readonly ILogger<QueryDispatcher> _logger;

    public async Task<TResult> DispatchAsync<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default)
    {
        var queryType = query.GetType();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));
        
        try
        {
            var handler = _services.GetService(handlerType)
                ?? throw new HandlerNotFoundException(queryType);
            
            var method = handlerType.GetMethod("HandleAsync");
            if (method == null)
            {
                throw new QueryDispatchException(
                    $"Handler {handlerType.Name} does not have HandleAsync method",
                    queryType.Name);
            }
            
            var task = (Task<TResult>)method.Invoke(handler, new object[] { query, cancellationToken })!;
            return await task;
        }
        catch (HandlerNotFoundException)
        {
            _logger.LogError(
                "No handler found for query {QueryType}. Registered handlers: {RegisteredHandlers}",
                queryType.Name,
                GetRegisteredHandlers(handlerType));
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Query dispatch failed for {QueryType}",
                queryType.Name);
            throw new QueryDispatchException(
                $"Failed to dispatch query {queryType.Name}: {ex.Message}",
                queryType.Name);
        }
    }

    private string GetRegisteredHandlers(Type handlerType)
    {
        try
        {
            // Attempt to enumerate registered handlers for debugging
            return "[diagnostics not available]";
        }
        catch { return "[error retrieving diagnostics]"; }
    }
}
```

### Logging Module

**Configuration Validation with Error Context**

```csharp
public static class LoggingStartupExtensions
{
    public static IServiceCollection AddStructuredLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        try
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            
            var logSection = configuration.GetSection("Logging");
            if (!logSection.Exists())
            {
                throw new ConfigurationException("Logging section not found in configuration");
            }
            
            var environment = GetEnvironment(configuration)
                ?? throw new ConfigurationException("ASPNETCORE_ENVIRONMENT not configured");
            
            var logPath = Path.Combine(AppContext.BaseDirectory, "logs");
            if (!Directory.Exists(logPath))
            {
                try
                {
                    Directory.CreateDirectory(logPath);
                }
                catch (IOException ex)
                {
                    throw new LoggingException(
                        $"Failed to create logs directory at {logPath}: {ex.Message}", ex);
                }
            }
            
            // Configure Serilog...
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Environment", environment)
                .WriteTo.Console();
            
            Log.Logger = loggerConfig.CreateLogger();
            services.AddLogging(builder => builder.AddSerilog(Log.Logger));
            
            return services;
        }
        catch (ConfigurationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new LoggingException(
                "Failed to configure structured logging: " + ex.Message, ex);
        }
    }
}
```

---

## 7. Error Handling Checklist

When handling errors in SmartWorkz.Core.Shared, ensure:

- [ ] Correct exception type selected (throw vs. return)
- [ ] CorrelationId included in all logs
- [ ] Structured logging with relevant context
- [ ] Sensitive data redacted (passwords, keys, PII)
- [ ] Transient errors identified and retried
- [ ] Fallback strategies for graceful degradation
- [ ] HTTP status codes match error category
- [ ] Stack traces hidden in production
- [ ] Timeout configured for async operations
- [ ] Cancellation tokens propagated
- [ ] Inner exceptions preserved for diagnostics

---

## References

- **Retry Patterns**: Exponential backoff with jitter (See: AWS best practices)
- **Structured Logging**: Serilog with context enrichment
- **CQRS Pattern**: Handler resolution and dispatch
- **Webhook Security**: HMAC-SHA256 signatures with constant-time comparison
- **Graceful Degradation**: Fallback strategies and circuit breakers
