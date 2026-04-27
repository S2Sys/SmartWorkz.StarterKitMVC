# SmartWorkz.Core.Shared.Logging

Structured logging system with correlation IDs, audit trails, and ASP.NET Core integration for observability and compliance.

## Overview

The Logging module provides structured logging capabilities for domain events, commands, SAGA operations, file operations, and background jobs. It uses structured properties instead of string interpolation for better queryability and correlation tracking.

**Key Problems Solved:**
- Track requests across distributed systems with correlation IDs
- Audit critical business operations
- Query logs by structured properties (not just text search)
- Automatically enrich logs with context (user, tenant, request ID)
- Comply with audit and compliance requirements

## Quick Start

### 1. Register Logging in DI

```csharp
// Program.cs
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
    builder.ClearProviders();
    builder.AddApplicationInsights(); // Optional: Azure Monitor
});

services.AddScoped<ILogger>(provider =>
    provider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("SmartWorkz.Application"));

services.AddScoped<EnrichedLogger>();
```

### 2. Inject and Use

```csharp
public class UserService
{
    private readonly EnrichedLogger _logger;

    public UserService(EnrichedLogger logger)
    {
        _logger = logger;
    }

    public async Task CreateUserAsync(CreateUserRequest request)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var user = new User { Id = Guid.NewGuid().ToString(), Email = request.Email };
            await _repository.AddAsync(user);

            stopwatch.Stop();

            _logger.LogCommandExecuted(
                commandType: nameof(CreateUserRequest),
                duration: stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            _logger.LogCommandExecutionError(
                commandType: nameof(CreateUserRequest),
                exception: ex);
            throw;
        }
    }
}
```

## Structured Logging Setup

Structured logging captures properties as individual fields, not as formatted strings. This enables:

- **Searching** — Find all logs for user "john@example.com"
- **Aggregation** — Average command duration by type
- **Correlation** — Track a request ID across services
- **Alerts** — Alert when error rate exceeds threshold

### Example: Structured vs Unstructured

**❌ Unstructured (String Interpolation)**
```csharp
_logger.LogInformation(
    $"User {user.Id} created with email {user.Email}");

// Output: "User user-123 created with email john@example.com"
// Problem: Properties embedded in string, hard to query
```

**✅ Structured (Named Properties)**
```csharp
_logger.LogInformation(
    "User created: {UserId} {Email}",
    user.Id,
    user.Email);

// Output: LogLevel=Information, UserId=user-123, Email=john@example.com
// Benefit: Each property queryable separately
```

### Structured Logging Example

```csharp
public class OrderService
{
    private readonly ILogger<OrderService> _logger;

    public async Task<Order> CreateOrderAsync(CreateOrderCommand command)
    {
        var stopwatch = Stopwatch.StartNew();
        var orderId = Guid.NewGuid().ToString();

        try
        {
            _logger.LogInformation(
                "Creating order {OrderId} for customer {CustomerId} in tenant {TenantId}",
                orderId,
                command.CustomerId,
                command.TenantId);

            var order = new Order
            {
                Id = orderId,
                CustomerId = command.CustomerId,
                TenantId = command.TenantId,
                Items = command.Items,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(order);

            stopwatch.Stop();

            _logger.LogInformation(
                "Order created successfully: {OrderId} CustomerId={CustomerId} Duration={DurationMs}ms",
                orderId,
                command.CustomerId,
                stopwatch.ElapsedMilliseconds);

            return order;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Failed to create order {OrderId} for customer {CustomerId}: {ErrorMessage}",
                orderId,
                command.CustomerId,
                ex.Message);

            throw;
        }
    }
}
```

## Log Level Configuration

| Level | Usage | Example |
|-------|-------|---------|
| Critical | System failure, requires immediate action | Database connection lost, out of disk space |
| Error | Operation failed, needs investigation | Command handler exception, HTTP 500 |
| Warning | Unexpected but recoverable | Retry attempt #3, slow query detected |
| Information | General application flow | User created, command executed, order shipped |
| Debug | Detailed diagnostics | Query parameters, method entry/exit, cache hit |
| Trace | Very detailed, high volume | Every variable assignment, loop iteration |

### Log Level Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "SmartWorkz.Core": "Debug",
      "SmartWorkz.Core.Data": "Debug",
      "SmartWorkz.Core.Webhooks": "Information"
    }
  }
}
```

### Environment-Specific Configuration

```csharp
// Startup.cs
services.AddLogging(builder =>
{
    if (environment.IsDevelopment())
    {
        builder.SetMinimumLevel(LogLevel.Debug);
        builder.AddDebug();
    }
    else if (environment.IsProduction())
    {
        builder.SetMinimumLevel(LogLevel.Information);
        builder.AddApplicationInsights();
    }
});
```

## Correlation ID Usage

Correlation IDs link related log entries across services, enabling end-to-end tracing:

```
User Request arrives
    ↓ CorrelationId = "corr-123"
    ├─ UserService (logs with corr-123)
    ├─ OrderService (logs with corr-123)
    ├─ PaymentService (logs with corr-123)
    └─ WebhookPublisher (logs with corr-123)

# Later: Query all logs for "corr-123" to see full flow
```

### Adding Correlation ID Middleware

```csharp
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers
            .TryGetValue(CorrelationIdHeader, out var value)
                ? value.ToString()
                : Guid.NewGuid().ToString();

        // Make available to DI container
        context.Items["CorrelationId"] = correlationId;

        // Add to response headers
        context.Response.Headers.Add(CorrelationIdHeader, correlationId);

        // Add to logs scope
        using (_logger.BeginScope(
            new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["TenantId"] = context.User.GetTenantId()
            }))
        {
            await _next(context);
        }
    }
}

// Register middleware
app.UseMiddleware<CorrelationIdMiddleware>();
```

### Using Correlation ID in Services

```csharp
public class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IHttpContextAccessor _contextAccessor;

    public async Task CreateUserAsync(CreateUserRequest request)
    {
        var correlationId = _contextAccessor.HttpContext?.Items["CorrelationId"]?.ToString()
            ?? "unknown";

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Operation"] = "CreateUser",
            ["TenantId"] = request.TenantId
        }))
        {
            _logger.LogInformation("Starting user creation");

            var user = new User { Email = request.Email };
            await _repository.AddAsync(user);

            _logger.LogInformation("User created successfully: {UserId}", user.Id);
        }
    }
}

// Log output includes CorrelationId, Operation, TenantId automatically
// [2024-04-27 10:30:00] INFO CorrelationId=corr-123 Operation=CreateUser 
// TenantId=tenant-456 Starting user creation
```

## Integration with ASP.NET Core

### Automatic Request Logging

```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        // Log request
        _logger.LogInformation(
            "HTTP request started: {RequestMethod} {RequestPath}",
            requestMethod,
            requestPath);

        try
        {
            await _next(context);

            stopwatch.Stop();

            _logger.LogInformation(
                "HTTP request completed: {RequestMethod} {RequestPath} {StatusCode} {DurationMs}ms",
                requestMethod,
                requestPath,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "HTTP request failed: {RequestMethod} {RequestPath} {DurationMs}ms",
                requestMethod,
                requestPath,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}

// Register in pipeline
app.UseMiddleware<RequestLoggingMiddleware>();
```

### Logging in Controllers

```csharp
[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;

    public UserController(
        ILogger<UserController> logger,
        IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(CreateUserRequest request)
    {
        var userId = Guid.NewGuid().ToString();

        _logger.LogInformation(
            "CreateUser request received: {UserId} {Email} from {RemoteIP}",
            userId,
            request.Email,
            HttpContext.Connection.RemoteIpAddress);

        try
        {
            var user = await _userService.CreateUserAsync(request);

            _logger.LogInformation(
                "User created successfully: {UserId}",
                user.Id);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create user: {Email}",
                request.Email);

            return StatusCode(500, "Failed to create user");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(string id)
    {
        _logger.LogDebug("GetUser request for {UserId}", id);

        var user = await _userService.GetUserAsync(id);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            return NotFound();
        }

        return Ok(user);
    }
}
```

## Audit Logging

Track critical business operations for compliance:

```csharp
public class AuditRecord
{
    public string Id { get; set; }
    public string TenantId { get; set; }
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string Operation { get; set; } // Create, Update, Delete
    public string UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string OldValues { get; set; }
    public string NewValues { get; set; }
    public string ChangedProperties { get; set; }
}

public class AuditLogger : IAuditLogger
{
    private readonly ILogger<AuditLogger> _logger;
    private readonly IAuditRepository _repository;

    public async Task LogAsync(AuditRecord record)
    {
        // Store in database for compliance
        await _repository.AddAsync(record);

        // Also log to application logs
        _logger.LogInformation(
            "Audit: {Operation} {EntityType} {EntityId} by {UserId}",
            record.Operation,
            record.EntityType,
            record.EntityId,
            record.UserId);
    }
}
```

## Best Practices

### 1. **Use Named Placeholders, Not String Interpolation**
```csharp
// ✅ Good: Properties extracted for querying
_logger.LogInformation(
    "Order created: {OrderId} Amount={Amount} Tenant={TenantId}",
    order.Id,
    order.Amount,
    order.TenantId);

// ❌ Bad: Properties embedded in string
_logger.LogInformation(
    $"Order created: {order.Id} Amount={order.Amount} Tenant={order.TenantId}");
```

### 2. **Log at Appropriate Levels**
```csharp
// Critical: Needs immediate investigation
_logger.LogCritical("Database connection lost - service unavailable");

// Error: Operation failed, but service continues
_logger.LogError(ex, "User creation failed: {Email}", request.Email);

// Warning: Unexpected but recoverable
_logger.LogWarning("Retry attempt {Attempt}/{MaxRetries}", attempt, maxRetries);

// Information: Normal application flow
_logger.LogInformation("Order completed: {OrderId}", orderId);

// Debug: Detailed diagnostics
_logger.LogDebug("Query parameters: {@Params}", queryParams);
```

### 3. **Include Relevant Context**
```csharp
// ✅ Good: Includes all relevant context for investigation
_logger.LogError(
    ex,
    "Payment failed for order {OrderId} customer {CustomerId} " +
    "tenant {TenantId} amount {Amount} error {ErrorCode}",
    order.Id,
    order.CustomerId,
    order.TenantId,
    order.Amount,
    ex.HResult);

// ❌ Bad: Missing context
_logger.LogError(ex, "Payment failed");
```

### 4. **Use Scopes for Context**
```csharp
// ✅ Good: All logs within scope include context
using (_logger.BeginScope(new Dictionary<string, object>
{
    ["UserId"] = userId,
    ["TenantId"] = tenantId,
    ["CorrelationId"] = correlationId
}))
{
    _logger.LogInformation("Starting import");
    // ... more operations ...
    _logger.LogInformation("Import completed"); // Includes all scope properties
}
```

### 5. **Measure Duration for Performance Analysis**
```csharp
// ✅ Good: Captures execution time
var stopwatch = Stopwatch.StartNew();
try
{
    var result = await _repository.GetAsync();
    stopwatch.Stop();

    _logger.LogInformation(
        "Query completed: {Duration}ms",
        stopwatch.ElapsedMilliseconds);
}
catch (Exception ex)
{
    stopwatch.Stop();
    _logger.LogError(
        ex,
        "Query failed after {Duration}ms",
        stopwatch.ElapsedMilliseconds);
}
```

## Related Modules

- **CQRS/** — Log command/query execution
- **Webhooks/** — Log webhook delivery attempts
- **Caching/** — Log cache hits/misses

## Common Issues

### Logs Not Appearing
1. Check minimum log level in configuration
2. Verify logger is created in DI container
3. Ensure logging providers (Console, Debug) are registered
4. Check environment (Development vs Production)

### Performance Impact from Excessive Logging
1. Reduce log level in production (Info instead of Debug)
2. Avoid logging in tight loops
3. Use lazy evaluation for expensive logs: `_logger.LogInformation("Data: {@Data}", () => GetExpensiveData())`
4. Monitor log volume and storage costs

### Correlation ID Not Propagating
1. Ensure CorrelationIdMiddleware is registered early
2. Check HttpContext is available in service
3. Verify correlation ID passed to async operations
4. Add logging at each service boundary
