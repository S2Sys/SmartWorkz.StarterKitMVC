# SmartWorkz.Core.Shared.Webhooks

Event-driven webhook publishing system with automatic retry logic, signature verification, and multi-tenancy support.

## Overview

The Webhooks module provides a robust implementation for publishing domain events to external webhook endpoints. It handles event serialization, HMAC-SHA256 signature generation, exponential backoff retry logic, and comprehensive failure tracking.

**Key Problems Solved:**
- Reliable event delivery to external systems without losing messages
- Security: Cryptographic signatures prevent tampering
- Fault tolerance: Exponential backoff prevents thundering herd
- Multi-tenancy: Tenant-scoped subscriptions and isolation
- Observability: Detailed logging of all delivery attempts

## Event Publishing Flow

```
Application Event
    ↓
Domain Event Handler
    ↓
IWebhookPublisher.PublishAsync(webhookEvent)
    ↓
Query IWebhookRegistry for subscriptions
    ↓
For each active endpoint:
    ├─ Serialize event to JSON
    ├─ Generate HMAC-SHA256 signature
    ├─ POST to endpoint with retry policy
    ├─ On success: Reset failure count
    └─ On failure: Increment counter, disable if MaxRetries exceeded
```

## Quick Start

### 1. Register Services in DI Container

```csharp
// Program.cs or Startup.cs
services.AddHttpClient();
services.AddScoped<IWebhookRegistry, SqlWebhookRegistry>();
services.AddScoped<IWebhookPublisher, WebhookPublisher>();
services.AddScoped<ILogger<WebhookPublisher>>(provider =>
    provider.GetRequiredService<ILoggerFactory>()
        .CreateLogger<WebhookPublisher>());
```

### 2. Publish an Event

```csharp
public class UserService
{
    private readonly IWebhookPublisher _webhookPublisher;

    public UserService(IWebhookPublisher webhookPublisher)
    {
        _webhookPublisher = webhookPublisher;
    }

    public async Task CreateUserAsync(CreateUserRequest request)
    {
        var user = new User 
        { 
            Id = Guid.NewGuid().ToString(), 
            Email = request.Email 
        };
        
        // Save to database...
        await _userRepository.AddAsync(user);

        // Publish webhook event
        var webhookEvent = new UserCreatedEvent(
            user.Id, 
            user.Email, 
            user.FirstName, 
            user.LastName
        )
        {
            TenantId = request.TenantId
        };

        await _webhookPublisher.PublishAsync(webhookEvent);
    }
}
```

## Available Events

### UserCreatedEvent
Fired when a new user is created.
```csharp
new UserCreatedEvent(
    userId: "user-123",
    email: "john@example.com",
    firstName: "John",
    lastName: "Doe"
) { TenantId = "tenant-456" }
```

### TransactionCompletedEvent
Fired when a transaction completes processing.
```csharp
new TransactionCompletedEvent(
    transactionId: "txn-789",
    amount: 100.00m,
    status: "completed"
) { TenantId = "tenant-456" }
```

To create custom events, inherit from `WebhookEvent`:
```csharp
public class OrderShippedEvent : WebhookEvent
{
    public OrderShippedEvent(string orderId, string trackingNumber)
        : base("order.shipped")
    {
        OrderId = orderId;
        TrackingNumber = trackingNumber;
    }

    public string OrderId { get; }
    public string TrackingNumber { get; }
}
```

## Webhook Registration & Discovery

Endpoints subscribe to events by registering in the `WebhookEndpoints` table:

```sql
INSERT INTO WebhookEndpoints 
(Id, TenantId, EventType, Url, SecretKey, IsActive, MaxRetries, TimeoutSeconds)
VALUES 
('endpoint-1', 'tenant-456', 'user.created', 
 'https://client.example.com/webhooks/users', 
 'sk_live_abcd1234...', 1, 5, 30)
```

The `IWebhookRegistry` discovers subscriptions at runtime:

```csharp
public interface IWebhookRegistry
{
    Task<IEnumerable<WebhookEndpointRegistration>> GetSubscriptionsForEventAsync(
        string eventType, 
        CancellationToken cancellationToken = default);

    Task UpdateEndpointAsync(
        string endpointId, 
        object updates, 
        CancellationToken cancellationToken = default);
}
```

## Retry Policy

Failed deliveries are retried with exponential backoff and jitter:

**Example Retry Schedule** (MaxRetries = 5):
| Attempt | Delay | Backoff Formula |
|---------|-------|-----------------|
| 1 | 0s (immediate) | N/A |
| 2 | 2s + jitter | 2^1 |
| 3 | 4s + jitter | 2^2 |
| 4 | 8s + jitter | 2^3 |
| 5 | 16s + jitter | 2^4 |
| 6 (disabled) | N/A | Endpoint deactivated |

Jitter (0-15% variance) prevents thundering herd when multiple endpoints fail simultaneously.

```csharp
// Configure retry policy on endpoint
var endpoint = new WebhookEndpointRegistration
{
    Url = "https://client.example.com/webhooks",
    MaxRetries = 5,
    TimeoutSeconds = 30
};
```

## Signature Verification

Every webhook is signed using HMAC-SHA256 with the endpoint's secret key.

### Client-Side Verification (Receiver)

```csharp
public static class WebhookSignature
{
    public static bool Verify(
        string payload, 
        string signature, 
        string secretKey)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expectedSignature = Convert.ToBase64String(hash);
            
            // Constant-time comparison prevents timing attacks
            return signature == expectedSignature;
        }
    }
}
```

### Webhook Payload Structure

```json
{
  "event": {
    "eventType": "user.created",
    "eventId": "evt-uuid",
    "publishedAt": "2024-04-27T10:30:00Z",
    "tenantId": "tenant-456",
    "userId": "user-123",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe"
  },
  "signature": "base64-encoded-hmac-sha256"
}
```

Client validates the signature from the HTTP header:
```
X-Webhook-Signature: base64-encoded-hmac-sha256
```

## Integration Example

**Complete webhook receiver in ASP.NET Core:**

```csharp
[ApiController]
[Route("api/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly ILogger<WebhookController> _logger;
    private readonly string _secretKey = "sk_live_...";

    public WebhookController(ILogger<WebhookController> logger)
    {
        _logger = logger;
    }

    [HttpPost("smartworkz")]
    public async Task<IActionResult> HandleWebhook([FromBody] WebhookPayload payload)
    {
        // 1. Verify signature
        var json = JsonSerializer.Serialize(payload.Event);
        if (!WebhookSignature.Verify(json, payload.Signature, _secretKey))
        {
            _logger.LogWarning("Webhook signature verification failed");
            return Unauthorized("Invalid signature");
        }

        // 2. Process event based on type
        switch (payload.Event.EventType)
        {
            case "user.created":
                var userEvent = JsonSerializer.Deserialize<UserCreatedEvent>(json);
                await _userService.HandleUserCreatedAsync(userEvent);
                break;

            case "transaction.completed":
                var txnEvent = JsonSerializer.Deserialize<TransactionCompletedEvent>(json);
                await _transactionService.HandleCompletionAsync(txnEvent);
                break;

            default:
                _logger.LogWarning("Unknown event type: {EventType}", payload.Event.EventType);
                return BadRequest("Unknown event type");
        }

        // 3. Return 2xx to acknowledge receipt
        return Ok(new { status = "received" });
    }
}
```

## Security Best Practices

### 1. **Always Verify Signatures**
Never trust webhook data without cryptographic verification. HMAC-SHA256 with constant-time comparison prevents tampering.

### 2. **Use Strong Secret Keys**
- Generate using cryptographically secure RNG
- Store in secure vaults (Azure Key Vault, AWS Secrets Manager)
- Rotate periodically (every 90 days recommended)
- Never commit secrets to version control

```csharp
// ✅ Correct: Load from configuration
var secretKey = configuration["Webhooks:SecretKey"];

// ❌ Wrong: Hardcoded secrets
var secretKey = "sk_test_1234567890abcdef";
```

### 3. **Implement Idempotency**
Use `EventId` to detect duplicate deliveries and ensure exactly-once processing:

```csharp
public async Task<IActionResult> HandleWebhook(WebhookPayload payload)
{
    // Check if we've already processed this event
    if (await _eventStore.EventExistsAsync(payload.Event.EventId))
    {
        _logger.LogInformation("Duplicate webhook received: {EventId}", 
            payload.Event.EventId);
        return Ok(); // Still return success
    }

    // Process and store
    await ProcessEventAsync(payload.Event);
    await _eventStore.RecordAsync(payload.Event.EventId);
    
    return Ok();
}
```

### 4. **Validate Event Age**
Reject events older than a threshold (e.g., 5 minutes) to prevent replay attacks:

```csharp
var maxAge = TimeSpan.FromMinutes(5);
var age = DateTime.UtcNow - payload.Event.PublishedAt;

if (age > maxAge)
{
    _logger.LogWarning("Stale webhook rejected: {EventId}", payload.Event.EventId);
    return BadRequest("Event too old");
}
```

### 5. **Use HTTPS Only**
Always receive webhooks over HTTPS with valid certificates. Enforce certificate pinning in production:

```csharp
// In webhook registration
var endpoint = new WebhookEndpointRegistration
{
    Url = "https://client.example.com/webhooks", // https required
    MaxRetries = 5
};
```

### 6. **Rate Limit and Monitor**
Implement rate limiting on your webhook endpoints to prevent abuse:

```csharp
[HttpPost("smartworkz")]
[RateLimit(10, TimeUnit.Minute)] // 10 requests per minute
public async Task<IActionResult> HandleWebhook(WebhookPayload payload)
{
    // ...
}
```

## Configuration

### Endpoint Configuration

```csharp
// In appsettings.json
{
  "Webhooks": {
    "MaxRetries": 5,
    "TimeoutSeconds": 30,
    "RetryDelayMs": 2000,
    "MaxJitterMs": 3000
  }
}
```

### DI Registration

```csharp
services.Configure<WebhookOptions>(configuration.GetSection("Webhooks"));
services.AddHttpClient<WebhookPublisher>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));
```

## Key Classes

| Class | Purpose |
|-------|---------|
| `IWebhookPublisher` | Publishes events to registered endpoints |
| `IWebhookRegistry` | Discovers subscriptions and manages endpoint state |
| `WebhookEvent` | Base class for all webhook events |
| `WebhookEndpointRegistration` | Represents a registered webhook endpoint |
| `WebhookSignature` | HMAC-SHA256 signing/verification |
| `WebhookPayload` | Container for event + signature |
| `WebhookRetryPolicy` | Defines retry strategy (exponential backoff) |

## Related Modules

- **Logging/** — Use `EnrichedLogger` for structured webhook delivery logs
- **CQRS/** — Publish webhooks from command handlers
- **Caching/** — Cache endpoint registrations with TTL

## Common Issues

### Webhook Not Delivered
1. Check endpoint is `IsActive = 1` in database
2. Verify `TenantId` matches event's `TenantId`
3. Check application logs for error details
4. Ensure external endpoint is accessible from application server

### Signature Verification Failing
1. Confirm secret key is identical on both sides
2. Verify signature was computed over event JSON (not wrapper)
3. Check for encoding mismatches (UTF-8 required)
4. Ensure constant-time comparison is used

### Endpoints Stuck in Retry Loop
1. Increase `MaxRetries` if transient failures expected
2. Check external endpoint for errors (500s, timeouts)
3. Verify network connectivity and firewall rules
4. Consider higher `TimeoutSeconds` for slow endpoints

## Performance Notes

- Webhook publishing is **fire-and-forget** by default (async without blocking)
- Each endpoint gets its own HTTP request (parallel delivery)
- Failed deliveries are retried asynchronously without blocking the main request
- Consider background job processor for high-volume scenarios (>1000 events/sec)
