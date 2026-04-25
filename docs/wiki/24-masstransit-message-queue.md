# MassTransit Message Queue

## Overview

MassTransit is a lightweight message bus framework for event-driven systems. It routes events from publishers to multiple subscribers, decoupling components. Use MassTransit when you need asynchronous processing of business events like user registration, order processing, or payments without blocking main requests.

---

## Architecture

Events flow from publishers through a configurable message broker to multiple consumers:

```
┌──────────────────┐
│ Event Publisher  │
└────────┬─────────┘
         │
    ┌────▼────────────────────┐
    │  MassTransit Bus         │
    │  (InMemory/RabbitMQ/ASB) │
    └────┬──────────┬──────────┘
         │          │
    ┌────▼────┐  ┌──▼──────────┐
    │Consumer1 │  │Consumer2     │
    └──────────┘  └──────────────┘
```

### Event Subscribers

| Event | Properties | Subscribers |
|-------|-----------|-------------|
| `UserRegisteredEvent` | UserId, Email, FirstName, LastName, RegisteredAt | SendWelcomeEmailConsumer, PublishAnalyticsEventConsumer |
| `OrderProcessedEvent` | OrderId, UserId, Amount, ProcessedAt | SendOrderConfirmationConsumer |
| `PaymentCompletedEvent` | PaymentId, OrderId, TransactionId, CompletedAt | (Ready for custom subscribers) |

---

## Quick Start

Publish an event:

```csharp
public class AuthService
{
    private readonly IPublishEndpoint _publishEndpoint;

    public async Task RegisterAsync(User user)
    {
        // Create user...
        await _publishEndpoint.Publish(new UserRegisteredEvent(
            user.Id, user.Email, user.FirstName, user.LastName
        ));
    }
}
```

MassTransit automatically routes to all registered consumers.

---

## Configuration

### InMemory Transport (Testing)

Events stay in-process. No external broker needed.

```json
{
  "MessageBroker": {
    "Type": "InMemory"
  }
}
```

| Setting | Type | Default |
|---------|------|---------|
| `MessageBroker:Type` | string | `InMemory` |

### RabbitMQ Transport (Production)

Enterprise message broker with persistence and clustering.

```json
{
  "MessageBroker": {
    "Type": "RabbitMQ",
    "RabbitMQ": {
      "Host": "rabbitmq.example.com",
      "Username": "smartworkz-app",
      "Password": "SecurePassword123"
    }
  }
}
```

| Setting | Type | Default | Example |
|---------|------|---------|---------|
| `MessageBroker:Type` | string | — | `RabbitMQ` |
| `MessageBroker:RabbitMQ:Host` | string | `localhost` | `rabbitmq.example.com` |
| `MessageBroker:RabbitMQ:Username` | string | `guest` | `smartworkz-app` |
| `MessageBroker:RabbitMQ:Password` | string | `guest` | `SecurePassword123` |

Start RabbitMQ:
```bash
docker run -d -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### Azure Service Bus Transport (Enterprise)

Cloud-managed message broker for global scale.

```json
{
  "MessageBroker": {
    "Type": "AzureServiceBus",
    "AzureServiceBus": {
      "ConnectionString": "Endpoint=sb://smartworkz.servicebus.windows.net/;SharedAccessKeyName=..."
    }
  }
}
```

| Setting | Type | Default |
|---------|------|---------|
| `MessageBroker:Type` | string | — |
| `MessageBroker:AzureServiceBus:ConnectionString` | string | — |

**Note:** Azure Service Bus is scaffolding (`NotImplementedException`). Complete `ConfigureAzureServiceBus()` method after installing `MassTransit.Azure.ServiceBus.Core`.

---

## Usage Examples

### Example 1: Publish UserRegisteredEvent

```csharp
public class UserAuthService
{
    private readonly IPublishEndpoint _publishEndpoint;

    public async Task<Result<User>> RegisterAsync(RegisterRequest request)
    {
        var user = new User(request.Email, request.FirstName, request.LastName);
        await _userRepository.CreateAsync(user);

        // Event published to all consumers
        await _publishEndpoint.Publish(new UserRegisteredEvent(
            user.Id, user.Email, user.FirstName, user.LastName
        ));

        return Result.Ok(user);
    }
}
```

**Note:** `SendWelcomeEmailConsumer` throws if email sending fails, which sends the message to the dead-letter queue. See Example 3 for the non-throwing analytics pattern alternative.

### Example 2: Consumer for OrderProcessedEvent

Real implementation from the codebase:

```csharp
public class SendOrderConfirmationConsumer : IConsumer<OrderProcessedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SendOrderConfirmationConsumer> _logger;

    public SendOrderConfirmationConsumer(
        IEmailSender emailSender,
        IUserRepository userRepository,
        ILogger<SendOrderConfirmationConsumer> logger)
    {
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<OrderProcessedEvent> context)
    {
        var @event = context.Message;

        try
        {
            _logger.LogInformation(
                "Processing order processed event - OrderId: {OrderId}, UserId: {UserId}",
                @event.OrderId,
                @event.UserId);

            // Fetch user details for email
            var user = await _userRepository.GetByIdAsync(@event.UserId);
            if (user == null)
                throw new InvalidOperationException($"User {(@event.UserId)} not found");

            // Send order confirmation email
            var subject = $"Order Confirmation - Order #{@event.OrderId}";
            var body = $@"<h2>Order Confirmation</h2>
                <p>Dear {user.DisplayName ?? user.Username},</p>
                <p>Thank you for your order!</p>
                <p><strong>Order ID:</strong> {@event.OrderId}</p>
                <p><strong>Amount:</strong> ${@event.Amount:F2}</p>";

            var result = await _emailSender.SendAsync(user.Email, subject, body, isHtml: true);

            if (!result.Succeeded)
                throw new InvalidOperationException($"Email sending failed: {result.MessageKey}");

            _logger.LogInformation("Order confirmation sent for order {OrderId}", @event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order created event - OrderId: {OrderId}", @event.OrderId);
            throw;
        }
    }
}
```

Register consumer:
```csharp
x.AddConsumer<SendOrderConfirmationConsumer>();
```

### Example 3: Non-Throwing Analytics Pattern

**Note:** Consumer exception handling strategy depends on criticality:
- Critical path consumers (`SendWelcomeEmailConsumer`, `SendOrderConfirmationConsumer`): Throw exceptions on failure
- Side-effect consumers (`PublishAnalyticsEventConsumer`): Swallow exceptions to prevent blocking

Analytics failures must not block main workflows:

```csharp
public class PublishAnalyticsEventConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<PublishAnalyticsEventConsumer> _logger;

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        try
        {
            await _analyticsService.TrackEventAsync("UserRegistered", new
            {
                UserId = context.Message.UserId,
                RegisteredAt = context.Message.RegisteredAt
            });
        }
        catch (Exception ex)
        {
            // IMPORTANT: Don't throw - analytics should not block user registration
            _logger.LogError(ex, "Analytics failed for user {UserId}", context.Message.UserId);
        }
    }
}
```

### Example 4: Activate Azure Service Bus

**Step 1:** Install package
```bash
dotnet add package MassTransit.Azure.ServiceBus.Core
```

**Step 2:** Implement in `MassTransitExtension.cs`
```csharp
private static void ConfigureAzureServiceBus(
    IBusRegistrationConfigurator x,
    IConfiguration messageBrokerConfig)
{
    var connectionString = messageBrokerConfig
        .GetSection("AzureServiceBus")
        .GetValue<string>("ConnectionString");

    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("Connection string required");

    x.UsingAzureServiceBus((context, cfg) =>
    {
        cfg.ConnectionString(connectionString);
        cfg.ConfigureEndpoints(context);
    });
}
```

---

## API Reference

### Registration

```csharp
// In Program.cs
builder.Services.AddMassTransitMessaging(builder.Configuration);
```

Registers MassTransit, all consumers, and selects transport from config.

### Publishing

```csharp
// Inject IPublishEndpoint into any service
public interface IPublishEndpoint
{
    Task Publish<T>(T message, CancellationToken cancellationToken = default) where T : class;
}
```

### Consumer Interface

```csharp
public interface IConsumer<in TMessage> where TMessage : class
{
    Task Consume(ConsumeContext<TMessage> context);
}
```

### Event Class

```csharp
public class UserRegisteredEvent
{
    public string UserId { get; }
    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public DateTime RegisteredAt { get; }
}
```

Events are plain C# classes. No base classes or interfaces required.

---

## Integration Notes

**Related:** Email service, repository patterns, structured logging

**Flow:**
1. Auth Service publishes `UserRegisteredEvent`
2. SendWelcomeEmailConsumer → sends email via `IEmailSender`
3. PublishAnalyticsEventConsumer → tracks signup (swallows exceptions)
4. Order Service publishes `OrderProcessedEvent`
5. SendOrderConfirmationConsumer → sends order email
6. Payment Service publishes `PaymentCompletedEvent`
7. Custom consumers update order status

Consumers auto-register queue endpoints from class names (SendWelcomeEmailConsumer → SendWelcomeEmail queue).

---

## Troubleshooting

### "NotImplementedException: Azure Service Bus not configured"

**Cause:** Azure Service Bus is intentional scaffolding (not production-ready yet).

**Fix:** Install `MassTransit.Azure.ServiceBus.Core` and implement `ConfigureAzureServiceBus()`, or switch to RabbitMQ/InMemory.

### "Message not routed to consumer"

**Cause:** Consumer not registered, or event class mismatch.

**Fix:** Verify `x.AddConsumer<YourConsumer>()` in extension; check event fully qualified names match between publisher and consumer.

### "Exception from consumer blocks queue"

**Cause:** Consumer throws unhandled exception. Message goes to dead-letter queue.

**Fix:** Don't throw from analytics/non-critical consumers (see Example 3); log errors instead.

### "RabbitMQ connection refused"

**Cause:** RabbitMQ not running or wrong host/credentials.

**Fix:** Start with `docker run -d -p 5672:5672 rabbitmq:3`; verify host/username/password in config.

---

## Reference

- **MassTransit Docs:** https://masstransit-project.com/
- **RabbitMQ:** https://www.rabbitmq.com/
- **Azure Service Bus:** https://learn.microsoft.com/en-us/azure/service-bus-messaging/
- **Source:** `src/SmartWorkz.StarterKitMVC.Infrastructure/Extensions/MassTransitExtension.cs`
