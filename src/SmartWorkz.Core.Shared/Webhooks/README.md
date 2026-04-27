# SmartWorkz Webhooks System

A robust, production-grade webhook system for event publishing with HMAC-SHA256 signature verification, exponential backoff retry logic, and multi-tenant isolation.

## Event Publishing Flow

The webhook system follows a clean event-driven architecture:

```
Application Domain Event
         ↓
  IWebhookPublisher
         ↓
  IWebhookRegistry (discovery)
         ↓
  Filter Active Endpoints
  (by event type & tenant)
         ↓
  For Each Endpoint:
  ├─ Serialize Event
  ├─ Generate HMAC-SHA256 Signature
  ├─ Create WebhookPayload
  └─ HTTP POST to Endpoint URL
         ↓
  On Success: Reset FailureCount
  On Failure: Increment FailureCount
              → Disable if MaxRetries exceeded
```

## Available Events

### UserCreatedEvent

Triggered when a new user is created in the system.

**Fields:**
- `id` (string): Unique event identifier
- `type` (string): Always `"user.created"`
- `timestamp` (ISO 8601): When the event occurred (UTC)
- `tenant_id` (string): Multi-tenant isolation
- `data` (object): Event-specific payload

**Example JSON:**
```json
{
  "delivery_id": "d7a89f12-c456-4d8e-9b1f-2e4c5d8a9b7c",
  "event": {
    "id": "evt_user_001",
    "type": "user.created",
    "timestamp": "2026-04-27T14:32:18.5832768Z",
    "tenant_id": "tenant-123",
    "data": {
      "userId": "user-789",
      "email": "alice@example.com",
      "firstName": "Alice",
      "lastName": "Johnson"
    }
  },
  "signature": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6",
  "created_at": "2026-04-27T14:32:18.5832768Z"
}
```

### TransactionCompletedEvent

Triggered when a transaction completes successfully.

**Fields:**
- `id` (string): Unique event identifier
- `type` (string): Always `"transaction.completed"`
- `timestamp` (ISO 8601): When the event occurred (UTC)
- `tenant_id` (string): Multi-tenant isolation
- `data` (object): Event-specific payload

**Example JSON:**
```json
{
  "delivery_id": "a9b8c7d6-e5f4-3c2b-1a0f-9e8d7c6b5a4f",
  "event": {
    "id": "evt_txn_002",
    "type": "transaction.completed",
    "timestamp": "2026-04-27T15:45:32.1234567Z",
    "tenant_id": "tenant-123",
    "data": {
      "transactionId": "txn-456",
      "amount": 249.99,
      "status": "completed"
    }
  },
  "signature": "b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6a1",
  "created_at": "2026-04-27T15:45:32.1234567Z"
}
```

### Creating Custom Events

To add a custom event, inherit from `WebhookEvent`:

```csharp
public record OrderShippedEvent(string OrderId, string TrackingNumber) : WebhookEvent
{
    public override string EventType => "order.shipped";
    public override object Data => new { OrderId, TrackingNumber };
}
```

Publish using:
```csharp
var @event = new OrderShippedEvent("order-999", "TRK123456789")
{
    TenantId = "tenant-123"
};
await _webhookPublisher.PublishAsync(@event);
```

## Webhook Registration & Discovery

### Creating Endpoint Registrations

Register a webhook endpoint to receive event notifications:

```csharp
var endpoint = new WebhookEndpointRegistration
{
    TenantId = "tenant-123",
    Url = "https://api.partner.com/webhooks/smartworkz",
    SecretKey = Guid.NewGuid().ToString("N"),  // Store securely
    EventTypes = new[] { "user.created", "transaction.completed" },
    IsActive = true,
    MaxRetries = 5,
    TimeoutSeconds = 30
};

var created = await webhookRegistry.CreateEndpointAsync(endpoint);
```

### Event Type Filtering

Endpoints subscribe to specific event types. Only matching events are delivered:

```csharp
var endpoint = new WebhookEndpointRegistration
{
    EventTypes = new[] { "user.created" }  // Only receives user.created events
};
```

### Multi-Tenant Isolation

The system enforces strict tenant isolation:
- Events are only delivered to endpoints with matching `TenantId`
- Registry queries filter by tenant automatically
- Prevents cross-tenant data leakage

```csharp
// Both must have matching TenantId
var endpoint = new WebhookEndpointRegistration { TenantId = "tenant-123" };
var @event = new UserCreatedEvent(...) { TenantId = "tenant-123" };

// Event will be delivered
await _webhookPublisher.PublishAsync(@event);
```

### Endpoint Lifecycle

**Create:** Register a new endpoint via `CreateEndpointAsync()`
- `IsActive` defaults to `true`
- `FailureCount` starts at `0`

**Active:** Endpoints receive webhook deliveries
- When delivery succeeds, `FailureCount` resets to `0`

**Disable on Failures:** After `MaxRetries` consecutive failures
- `IsActive` set to `false` automatically
- `FailureReason` populated with last error
- Requires manual re-activation via `UpdateEndpointAsync()`

**Example: Re-activate disabled endpoint**
```csharp
await webhookRegistry.UpdateEndpointAsync(endpointId, new
{
    IsActive = true,
    FailureCount = 0
});
```

## Retry Policy: Exponential Backoff with Jitter

Failed deliveries trigger automatic retries with exponential backoff.

### Formula

```
delay = InitialDelayMs × (BackoffMultiplier ^ attemptNumber) ± jitter
```

**Default Configuration:**
- `InitialDelayMs`: 1000ms (1 second)
- `BackoffMultiplier`: 2.0x
- `MaxDelayMs`: 300,000ms (5 minutes)
- `MaxRetries`: 5 attempts
- `Jitter`: ±10% randomization

### Retry Timeline (5 Attempts)

| Attempt | Formula | Delay | Actual (w/jitter) |
|---------|---------|-------|-------------------|
| 1 | 1s × 2^0 | 1s | 0.9s – 1.1s |
| 2 | 1s × 2^1 | 2s | 1.8s – 2.2s |
| 3 | 1s × 2^2 | 4s | 3.6s – 4.4s |
| 4 | 1s × 2^3 | 8s | 7.2s – 8.8s |
| 5 | 1s × 2^4 | 16s | 14.4s – 17.6s |

**Total time for 5 retries:** ~32 seconds (average with jitter)

### Configuration

Override defaults via `WebhookRetryPolicy`:

```csharp
var retryPolicy = new WebhookRetryPolicy
{
    InitialDelayMs = 500,
    BackoffMultiplier = 1.5,
    MaxDelayMs = 60_000,
    MaxRetries = 3
};

int delay = retryPolicy.GetRetryDelayMs(attemptNumber: 2);  // 750ms
```

## Signature Verification: HMAC-SHA256

Endpoints verify webhook authenticity using HMAC-SHA256 signatures. The signature is computed over the serialized event JSON using a shared secret key.

### Signature Format

Each webhook payload includes:
- `event`: The JSON-serialized event
- `signature`: Hex-encoded HMAC-SHA256 hash
- `delivery_id`: Unique identifier for this delivery
- `created_at`: Timestamp of delivery

### Verification Process

1. Extract the `signature` from the received payload
2. Serialize the `event` to JSON
3. Compute HMAC-SHA256 using your secret key
4. **Constant-time comparison** with the received signature
5. If match, payload is authentic

### Example Verification Code (10 lines)

```csharp
using System.Security.Cryptography;
using System.Text;

public static bool VerifyWebhookSignature(
    string eventJson, string receivedSignature, string secretKey)
{
    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
    var expectedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(eventJson));
    var expectedSig = BitConverter.ToString(expectedHash).Replace("-", "").ToLower();
    
    // Constant-time comparison prevents timing attacks
    return CryptographicOperations.FixedTimeEquals(
        Encoding.UTF8.GetBytes(expectedSig), 
        Encoding.UTF8.GetBytes(receivedSignature));
}
```

## Integration Example

Full webhook receiver implementation (30 lines):

```csharp
[ApiController]
[Route("api/webhooks")]
public class WebhookReceiverController : ControllerBase
{
    private readonly ILogger<WebhookReceiverController> _logger;
    private readonly string _webhookSecret = "your-secret-key";

    [HttpPost("smartworkz")]
    public async Task<IActionResult> HandleWebhook([FromBody] WebhookPayload payload)
    {
        try
        {
            // 1. Verify signature
            var eventJson = JsonSerializer.Serialize(payload.Event);
            if (!VerifySignature(eventJson, payload.Signature))
            {
                _logger.LogWarning("Invalid webhook signature");
                return Unauthorized();
            }

            // 2. Check timestamp to prevent replay attacks
            var age = DateTimeOffset.UtcNow - payload.CreatedAt;
            if (age > TimeSpan.FromMinutes(5))
            {
                _logger.LogWarning("Webhook too old (replay attack)");
                return BadRequest("Payload too old");
            }

            // 3. Deserialize and process
            return payload.Event switch
            {
                UserCreatedEvent userEvent => 
                    await ProcessUserCreated(userEvent),
                TransactionCompletedEvent txnEvent => 
                    await ProcessTransactionCompleted(txnEvent),
                _ => BadRequest("Unknown event type")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook processing failed");
            return StatusCode(500);  // Trigger server-side retry
        }
    }

    private bool VerifySignature(string json, string signature)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(json));
        var expected = BitConverter.ToString(hash).Replace("-", "").ToLower();
        
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature));
    }

    private async Task<IActionResult> ProcessUserCreated(UserCreatedEvent evt)
    {
        _logger.LogInformation("User created: {UserId}", evt.Data.UserId);
        // TODO: Your processing logic
        return Ok();
    }

    private async Task<IActionResult> ProcessTransactionCompleted(TransactionCompletedEvent evt)
    {
        _logger.LogInformation("Transaction completed: {TxnId}", evt.Data.TransactionId);
        // TODO: Your processing logic
        return Ok();
    }
}
```

## Security Best Practices

### 1. **Store Secrets Securely**
- Never log or expose `SecretKey`
- Use environment variables or secure vaults (Azure Key Vault, AWS Secrets Manager)
- Rotate secrets periodically
- Never commit secrets to version control

```csharp
// BAD: Exposed in code
var secret = "hardcoded-secret-123";

// GOOD: From environment
var secret = Environment.GetEnvironmentVariable("WEBHOOK_SECRET_KEY");
```

### 2. **Idempotency: Deduplication on Receiver**
- Webhooks may be delivered multiple times (at-least-once guarantee)
- Use `delivery_id` to deduplicate
- Store processed delivery IDs in database
- Skip if `delivery_id` already processed

```csharp
public async Task<bool> IsDeliveryProcessed(string deliveryId)
{
    return await _db.ProcessedDeliveries.AnyAsync(d => d.Id == deliveryId);
}

public async Task ProcessDelivery(WebhookPayload payload)
{
    if (await IsDeliveryProcessed(payload.DeliveryId))
    {
        _logger.LogInformation("Webhook already processed: {DeliveryId}", payload.DeliveryId);
        return;  // Idempotent
    }

    // Process webhook...
    await _db.ProcessedDeliveries.AddAsync(new ProcessedDelivery { Id = payload.DeliveryId });
    await _db.SaveChangesAsync();
}
```

### 3. **Replay Attack Prevention**
- Validate `created_at` timestamp
- Reject payloads older than 5 minutes (configurable)
- Prevents attacker from replaying old payloads

```csharp
var age = DateTimeOffset.UtcNow - payload.CreatedAt;
if (age > TimeSpan.FromMinutes(5))
{
    return BadRequest("Payload too old");
}
```

### 4. **HTTPS Only**
- Always register endpoints with `https://` URLs
- Prevents man-in-the-middle interception
- Encryption in transit protects payloads and signatures

```csharp
var endpoint = new WebhookEndpointRegistration
{
    Url = "https://api.partner.com/webhooks",  // HTTPS required
};
```

### 5. **Rate Limiting**
- Implement rate limiting on webhook receiver
- Prevents DDoS attacks via webhook flooding
- Use sliding window or token bucket algorithms

```csharp
[ServiceFilter(typeof(RateLimitFilter))]
[HttpPost("smartworkz")]
public async Task<IActionResult> HandleWebhook([FromBody] WebhookPayload payload)
{
    // Handler code...
}
```

### 6. **Audit Logging**
- Log all webhook deliveries (delivery_id, event_type, tenant, timestamp)
- Log all failures with reason (HTTP status, timeout, network error)
- Log signature verification failures (potential tampering)
- Retain logs for 90+ days for compliance

```csharp
_logger.LogInformation(
    "Webhook delivered | DeliveryId={DeliveryId}, EventType={EventType}, " +
    "Tenant={Tenant}, Status={Status}, Timestamp={Timestamp}",
    payload.DeliveryId, payload.Event.EventType, payload.Event.TenantId,
    "success", payload.CreatedAt);

_logger.LogWarning(
    "Webhook signature verification failed | DeliveryId={DeliveryId}, " +
    "Tenant={Tenant}", payload.DeliveryId, payload.Event.TenantId);
```

## Configuration & Dependency Injection

Register the webhook system in your startup:

```csharp
services.AddScoped<IWebhookPublisher, WebhookPublisher>();
services.AddScoped<IWebhookRegistry, SqlWebhookRegistry>();  // Your DB impl
services.AddHttpClient();
```

## Error Handling & Debugging

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Endpoint disabled after failures | MaxRetries exceeded | Check endpoint logs, verify receiver URL is operational |
| Signature verification fails | Secret key mismatch | Confirm SecretKey matches on both sides |
| Timeout errors | Receiver too slow | Increase `TimeoutSeconds`, optimize receiver |
| No webhooks delivered | Event type mismatch | Verify endpoint `EventTypes` matches published event |
| Duplicate processing | Missing idempotency | Implement `delivery_id` deduplication on receiver |

### Debugging Checklist

- [ ] Endpoint URL is HTTPS and publicly accessible
- [ ] Receiver accepts POST with JSON body
- [ ] Receiver responds with 2xx status code
- [ ] SecretKey matches on publisher and receiver
- [ ] Event type is in endpoint's EventTypes array
- [ ] TenantId matches exactly
- [ ] Receiver logs all webhook deliveries
- [ ] Check firewall/network rules if endpoint is unreachable

## Performance Considerations

- **Async delivery:** Webhooks are published asynchronously, not blocking business logic
- **Batching:** Consider batching multiple events for high-volume scenarios
- **Timeouts:** Default 30 seconds; adjust based on receiver performance
- **Retry delays:** Exponential backoff prevents hammering failed endpoints
- **Database:** Webhook registry queries should be indexed by (TenantId, EventType)

## References

- **Specification:** HMAC-SHA256 (RFC 2104)
- **JSON Serialization:** System.Text.Json
- **Retry Algorithm:** Exponential backoff with jitter (AWS Lambda standard)
- **Multi-tenancy:** Tenant isolation at query and delivery layer
