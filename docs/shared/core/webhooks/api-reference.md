# Webhooks API Reference

## Classes & Interfaces

### WebhookStartupExtensions

- **Namespace:** `SmartWorkz.Shared.WebhookStartupExtensions`
- **Summary:** Dependency injection extensions for webhook services.
            Call AddWebhooks() during application startup to register required services.

#### Methods & Properties

- **AddWebhooks** - Register webhook services in the dependency injection container.
  - Parameters:
    - `services`: The service collection to configure.
  - Returns: The service collection for chaining.
- **CreateWebhookSchema** - Create the WebhookSubscriptions table schema if it doesn't exist.
            Call this during database initialization.
  - Parameters:
    - `connection`: The database connection.

### WebhookSubscription

- **Namespace:** `SmartWorkz.Shared.WebhookSubscription`
- **Summary:** Represents a registered webhook subscription for event notifications.
            Tracks subscription details, retry configuration, and delivery status.

### IWebhookPublisher

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.IWebhookPublisher`
- **Summary:** Contract for publishing webhook events to registered endpoints.

#### Methods & Properties

- **PublishAsync** - Publish a webhook event to all registered endpoints subscribed to the event type.
  - Parameters:
    - `webhookEvent`: The event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous operation.
- **PublishToEndpointAsync** - Publish a webhook event to a specific endpoint.
  - Parameters:
    - `endpoint`: The webhook endpoint to deliver to.
    - `webhookEvent`: The event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous operation.

### WebhookEndpointRegistration

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookEndpointRegistration`
- **Summary:** Represents a registered webhook endpoint.

### WebhookEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookEvent`
- **Summary:** Base class for webhook events published by the system.

### UserCreatedEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.UserCreatedEvent`
- **Summary:** Example webhook event for user creation.

#### Methods & Properties

- **#ctor** - Example webhook event for user creation.

### TransactionCompletedEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.TransactionCompletedEvent`
- **Summary:** Example webhook event for transaction completion.

#### Methods & Properties

- **#ctor** - Example webhook event for transaction completion.

### WebhookPayload

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookPayload`
- **Summary:** Webhook payload envelope sent to registered webhook endpoints.

### WebhookRetryPolicy

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookRetryPolicy`
- **Summary:** Retry policy configuration for webhook delivery failures.

#### Methods & Properties

- **GetRetryDelayMs** - Calculate the delay for the nth retry attempt.
            Uses exponential backoff: delay = initialDelay * (backoffMultiplier ^ attempt).

### WebhookSignature

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookSignature`
- **Summary:** Handles HMAC-SHA256 signing and verification of webhook payloads.

#### Methods & Properties

- **Sign** - Sign a webhook payload using the provided secret key.
  - Parameters:
    - `payload`: The webhook payload to sign.
    - `secretKey`: The webhook secret key.
  - Returns: HMAC-SHA256 signature in hex format.
- **Sign** - Sign a JSON string using the provided secret key.
- **Verify** - Verify a webhook payload signature.
  - Parameters:
    - `payload`: The webhook payload.
    - `signature`: The signature to verify.
    - `secretKey`: The webhook secret key.
  - Returns: True if signature is valid, false otherwise.

### WebhookStartupExtensions

- **Namespace:** `SmartWorkz.Shared.WebhookStartupExtensions`
- **Summary:** Dependency injection extensions for webhook services.
            Call AddWebhooks() during application startup to register required services.

#### Methods & Properties

- **AddWebhooks** - Register webhook services in the dependency injection container.
  - Parameters:
    - `services`: The service collection to configure.
  - Returns: The service collection for chaining.
- **CreateWebhookSchema** - Create the WebhookSubscriptions table schema if it doesn't exist.
            Call this during database initialization.
  - Parameters:
    - `connection`: The database connection.

### WebhookSubscription

- **Namespace:** `SmartWorkz.Shared.WebhookSubscription`
- **Summary:** Represents a registered webhook subscription for event notifications.
            Tracks subscription details, retry configuration, and delivery status.

### IWebhookPublisher

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.IWebhookPublisher`
- **Summary:** Contract for publishing webhook events to registered endpoints.

#### Methods & Properties

- **PublishAsync** - Publish a webhook event to all registered endpoints subscribed to the event type.
  - Parameters:
    - `webhookEvent`: The event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous operation.
- **PublishToEndpointAsync** - Publish a webhook event to a specific endpoint.
  - Parameters:
    - `endpoint`: The webhook endpoint to deliver to.
    - `webhookEvent`: The event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous operation.

### WebhookEndpointRegistration

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookEndpointRegistration`
- **Summary:** Represents a registered webhook endpoint.

### WebhookEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookEvent`
- **Summary:** Base class for webhook events published by the system.

### UserCreatedEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.UserCreatedEvent`
- **Summary:** Example webhook event for user creation.

#### Methods & Properties

- **#ctor** - Example webhook event for user creation.

### TransactionCompletedEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.TransactionCompletedEvent`
- **Summary:** Example webhook event for transaction completion.

#### Methods & Properties

- **#ctor** - Example webhook event for transaction completion.

### WebhookPayload

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookPayload`
- **Summary:** Webhook payload envelope sent to registered webhook endpoints.

### WebhookRetryPolicy

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookRetryPolicy`
- **Summary:** Retry policy configuration for webhook delivery failures.

#### Methods & Properties

- **GetRetryDelayMs** - Calculate the delay for the nth retry attempt.
            Uses exponential backoff: delay = initialDelay * (backoffMultiplier ^ attempt).

### WebhookSignature

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookSignature`
- **Summary:** Handles HMAC-SHA256 signing and verification of webhook payloads.

#### Methods & Properties

- **Sign** - Sign a webhook payload using the provided secret key.
  - Parameters:
    - `payload`: The webhook payload to sign.
    - `secretKey`: The webhook secret key.
  - Returns: HMAC-SHA256 signature in hex format.
- **Sign** - Sign a JSON string using the provided secret key.
- **Verify** - Verify a webhook payload signature.
  - Parameters:
    - `payload`: The webhook payload.
    - `signature`: The signature to verify.
    - `secretKey`: The webhook secret key.
  - Returns: True if signature is valid, false otherwise.

### WebhookStartupExtensions

- **Namespace:** `SmartWorkz.Shared.WebhookStartupExtensions`
- **Summary:** Dependency injection extensions for webhook services.
            Call AddWebhooks() during application startup to register required services.

#### Methods & Properties

- **AddWebhooks** - Register webhook services in the dependency injection container.
  - Parameters:
    - `services`: The service collection to configure.
  - Returns: The service collection for chaining.
- **CreateWebhookSchema** - Create the WebhookSubscriptions table schema if it doesn't exist.
            Call this during database initialization.
  - Parameters:
    - `connection`: The database connection.

### WebhookSubscription

- **Namespace:** `SmartWorkz.Shared.WebhookSubscription`
- **Summary:** Represents a registered webhook subscription for event notifications.
            Tracks subscription details, retry configuration, and delivery status.

### IWebhookPublisher

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.IWebhookPublisher`
- **Summary:** Contract for publishing webhook events to registered endpoints.

#### Methods & Properties

- **PublishAsync** - Publish a webhook event to all registered endpoints subscribed to the event type.
  - Parameters:
    - `webhookEvent`: The event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous operation.
- **PublishToEndpointAsync** - Publish a webhook event to a specific endpoint.
  - Parameters:
    - `endpoint`: The webhook endpoint to deliver to.
    - `webhookEvent`: The event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous operation.

### WebhookEndpointRegistration

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookEndpointRegistration`
- **Summary:** Represents a registered webhook endpoint.

### WebhookEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookEvent`
- **Summary:** Base class for webhook events published by the system.

### UserCreatedEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.UserCreatedEvent`
- **Summary:** Example webhook event for user creation.

#### Methods & Properties

- **#ctor** - Example webhook event for user creation.

### TransactionCompletedEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.TransactionCompletedEvent`
- **Summary:** Example webhook event for transaction completion.

#### Methods & Properties

- **#ctor** - Example webhook event for transaction completion.

### WebhookPayload

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookPayload`
- **Summary:** Webhook payload envelope sent to registered webhook endpoints.

### WebhookRetryPolicy

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookRetryPolicy`
- **Summary:** Retry policy configuration for webhook delivery failures.

#### Methods & Properties

- **GetRetryDelayMs** - Calculate the delay for the nth retry attempt.
            Uses exponential backoff: delay = initialDelay * (backoffMultiplier ^ attempt).

### WebhookSignature

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookSignature`
- **Summary:** Handles HMAC-SHA256 signing and verification of webhook payloads.

#### Methods & Properties

- **Sign** - Sign a webhook payload using the provided secret key.
  - Parameters:
    - `payload`: The webhook payload to sign.
    - `secretKey`: The webhook secret key.
  - Returns: HMAC-SHA256 signature in hex format.
- **Sign** - Sign a JSON string using the provided secret key.
- **Verify** - Verify a webhook payload signature.
  - Parameters:
    - `payload`: The webhook payload.
    - `signature`: The signature to verify.
    - `secretKey`: The webhook secret key.
  - Returns: True if signature is valid, false otherwise.

### WebhookStartupExtensions

- **Namespace:** `SmartWorkz.Shared.WebhookStartupExtensions`
- **Summary:** Dependency injection extensions for webhook services.
            Call AddWebhooks() during application startup to register required services.

#### Methods & Properties

- **AddWebhooks** - Register webhook services in the dependency injection container.
  - Parameters:
    - `services`: The service collection to configure.
  - Returns: The service collection for chaining.
- **CreateWebhookSchema** - Create the WebhookSubscriptions table schema if it doesn't exist.
            Call this during database initialization.
  - Parameters:
    - `connection`: The database connection.

### WebhookSubscription

- **Namespace:** `SmartWorkz.Shared.WebhookSubscription`
- **Summary:** Represents a registered webhook subscription for event notifications.
            Tracks subscription details, retry configuration, and delivery status.

### IWebhookPublisher

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.IWebhookPublisher`
- **Summary:** Contract for publishing webhook events to registered endpoints.

#### Methods & Properties

- **PublishAsync** - Publish a webhook event to all registered endpoints subscribed to the event type.
  - Parameters:
    - `webhookEvent`: The event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous operation.
- **PublishToEndpointAsync** - Publish a webhook event to a specific endpoint.
  - Parameters:
    - `endpoint`: The webhook endpoint to deliver to.
    - `webhookEvent`: The event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous operation.

### WebhookEndpointRegistration

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookEndpointRegistration`
- **Summary:** Represents a registered webhook endpoint.

### WebhookEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookEvent`
- **Summary:** Base class for webhook events published by the system.

### UserCreatedEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.UserCreatedEvent`
- **Summary:** Example webhook event for user creation.

#### Methods & Properties

- **#ctor** - Example webhook event for user creation.

### TransactionCompletedEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.TransactionCompletedEvent`
- **Summary:** Example webhook event for transaction completion.

#### Methods & Properties

- **#ctor** - Example webhook event for transaction completion.

### WebhookPayload

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookPayload`
- **Summary:** Webhook payload envelope sent to registered webhook endpoints.

### WebhookRetryPolicy

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookRetryPolicy`
- **Summary:** Retry policy configuration for webhook delivery failures.

#### Methods & Properties

- **GetRetryDelayMs** - Calculate the delay for the nth retry attempt.
            Uses exponential backoff: delay = initialDelay * (backoffMultiplier ^ attempt).

### WebhookSignature

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookSignature`
- **Summary:** Handles HMAC-SHA256 signing and verification of webhook payloads.

#### Methods & Properties

- **Sign** - Sign a webhook payload using the provided secret key.
  - Parameters:
    - `payload`: The webhook payload to sign.
    - `secretKey`: The webhook secret key.
  - Returns: HMAC-SHA256 signature in hex format.
- **Sign** - Sign a JSON string using the provided secret key.
- **Verify** - Verify a webhook payload signature.
  - Parameters:
    - `payload`: The webhook payload.
    - `signature`: The signature to verify.
    - `secretKey`: The webhook secret key.
  - Returns: True if signature is valid, false otherwise.

### WebhookStartupExtensions

- **Namespace:** `SmartWorkz.Shared.WebhookStartupExtensions`
- **Summary:** Dependency injection extensions for webhook services.
            Call AddWebhooks() during application startup to register required services.

#### Methods & Properties

- **AddWebhooks** - Register webhook services in the dependency injection container.
  - Parameters:
    - `services`: The service collection to configure.
  - Returns: The service collection for chaining.
- **CreateWebhookSchema** - Create the WebhookSubscriptions table schema if it doesn't exist.
            Call this during database initialization.
  - Parameters:
    - `connection`: The database connection.

### WebhookSubscription

- **Namespace:** `SmartWorkz.Shared.WebhookSubscription`
- **Summary:** Represents a registered webhook subscription for event notifications.
            Tracks subscription details, retry configuration, and delivery status.

### IWebhookPublisher

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.IWebhookPublisher`
- **Summary:** Contract for publishing webhook events to registered endpoints.

#### Methods & Properties

- **PublishAsync** - Publish a webhook event to all registered endpoints subscribed to the event type.
  - Parameters:
    - `webhookEvent`: The event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous operation.
- **PublishToEndpointAsync** - Publish a webhook event to a specific endpoint.
  - Parameters:
    - `endpoint`: The webhook endpoint to deliver to.
    - `webhookEvent`: The event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous operation.

### WebhookEndpointRegistration

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookEndpointRegistration`
- **Summary:** Represents a registered webhook endpoint.

### WebhookEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookEvent`
- **Summary:** Base class for webhook events published by the system.

### UserCreatedEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.UserCreatedEvent`
- **Summary:** Example webhook event for user creation.

#### Methods & Properties

- **#ctor** - Example webhook event for user creation.

### TransactionCompletedEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.TransactionCompletedEvent`
- **Summary:** Example webhook event for transaction completion.

#### Methods & Properties

- **#ctor** - Example webhook event for transaction completion.

### WebhookPayload

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookPayload`
- **Summary:** Webhook payload envelope sent to registered webhook endpoints.

### WebhookRetryPolicy

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookRetryPolicy`
- **Summary:** Retry policy configuration for webhook delivery failures.

#### Methods & Properties

- **GetRetryDelayMs** - Calculate the delay for the nth retry attempt.
            Uses exponential backoff: delay = initialDelay * (backoffMultiplier ^ attempt).

### WebhookSignature

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookSignature`
- **Summary:** Handles HMAC-SHA256 signing and verification of webhook payloads.

#### Methods & Properties

- **Sign** - Sign a webhook payload using the provided secret key.
  - Parameters:
    - `payload`: The webhook payload to sign.
    - `secretKey`: The webhook secret key.
  - Returns: HMAC-SHA256 signature in hex format.
- **Sign** - Sign a JSON string using the provided secret key.
- **Verify** - Verify a webhook payload signature.
  - Parameters:
    - `payload`: The webhook payload.
    - `signature`: The signature to verify.
    - `secretKey`: The webhook secret key.
  - Returns: True if signature is valid, false otherwise.

### WebhookStartupExtensions

- **Namespace:** `SmartWorkz.Shared.WebhookStartupExtensions`
- **Summary:** Dependency injection extensions for webhook services.
            Call AddWebhooks() during application startup to register required services.

#### Methods & Properties

- **AddWebhooks** - Register webhook services in the dependency injection container.
  - Parameters:
    - `services`: The service collection to configure.
  - Returns: The service collection for chaining.
- **CreateWebhookSchema** - Create the WebhookSubscriptions table schema if it doesn't exist.
            Call this during database initialization.
  - Parameters:
    - `connection`: The database connection.

### WebhookSubscription

- **Namespace:** `SmartWorkz.Shared.WebhookSubscription`
- **Summary:** Represents a registered webhook subscription for event notifications.
            Tracks subscription details, retry configuration, and delivery status.

### IWebhookPublisher

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.IWebhookPublisher`
- **Summary:** Contract for publishing webhook events to registered endpoints.

#### Methods & Properties

- **PublishAsync** - Publish a webhook event to all registered endpoints subscribed to the event type.
  - Parameters:
    - `webhookEvent`: The event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous operation.
- **PublishToEndpointAsync** - Publish a webhook event to a specific endpoint.
  - Parameters:
    - `endpoint`: The webhook endpoint to deliver to.
    - `webhookEvent`: The event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous operation.

### WebhookEndpointRegistration

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookEndpointRegistration`
- **Summary:** Represents a registered webhook endpoint.

### WebhookEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookEvent`
- **Summary:** Base class for webhook events published by the system.

### UserCreatedEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.UserCreatedEvent`
- **Summary:** Example webhook event for user creation.

#### Methods & Properties

- **#ctor** - Example webhook event for user creation.

### TransactionCompletedEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.TransactionCompletedEvent`
- **Summary:** Example webhook event for transaction completion.

#### Methods & Properties

- **#ctor** - Example webhook event for transaction completion.

### WebhookPayload

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookPayload`
- **Summary:** Webhook payload envelope sent to registered webhook endpoints.

### WebhookRetryPolicy

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookRetryPolicy`
- **Summary:** Retry policy configuration for webhook delivery failures.

#### Methods & Properties

- **GetRetryDelayMs** - Calculate the delay for the nth retry attempt.
            Uses exponential backoff: delay = initialDelay * (backoffMultiplier ^ attempt).

### WebhookSignature

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookSignature`
- **Summary:** Handles HMAC-SHA256 signing and verification of webhook payloads.

#### Methods & Properties

- **Sign** - Sign a webhook payload using the provided secret key.
  - Parameters:
    - `payload`: The webhook payload to sign.
    - `secretKey`: The webhook secret key.
  - Returns: HMAC-SHA256 signature in hex format.
- **Sign** - Sign a JSON string using the provided secret key.
- **Verify** - Verify a webhook payload signature.
  - Parameters:
    - `payload`: The webhook payload.
    - `signature`: The signature to verify.
    - `secretKey`: The webhook secret key.
  - Returns: True if signature is valid, false otherwise.

### WebhookStartupExtensions

- **Namespace:** `SmartWorkz.Shared.WebhookStartupExtensions`
- **Summary:** Dependency injection extensions for webhook services.
            Call AddWebhooks() during application startup to register required services.

#### Methods & Properties

- **AddWebhooks** - Register webhook services in the dependency injection container.
  - Parameters:
    - `services`: The service collection to configure.
  - Returns: The service collection for chaining.
- **CreateWebhookSchema** - Create the WebhookSubscriptions table schema if it doesn't exist.
            Call this during database initialization.
  - Parameters:
    - `connection`: The database connection.

### WebhookSubscription

- **Namespace:** `SmartWorkz.Shared.WebhookSubscription`
- **Summary:** Represents a registered webhook subscription for event notifications.
            Tracks subscription details, retry configuration, and delivery status.

### IWebhookPublisher

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.IWebhookPublisher`
- **Summary:** Contract for publishing webhook events to registered endpoints.

#### Methods & Properties

- **PublishAsync** - Publish a webhook event to all registered endpoints subscribed to the event type.
  - Parameters:
    - `webhookEvent`: The event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous operation.
- **PublishToEndpointAsync** - Publish a webhook event to a specific endpoint.
  - Parameters:
    - `endpoint`: The webhook endpoint to deliver to.
    - `webhookEvent`: The event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous operation.

### WebhookEndpointRegistration

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookEndpointRegistration`
- **Summary:** Represents a registered webhook endpoint.

### WebhookEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookEvent`
- **Summary:** Base class for webhook events published by the system.

### UserCreatedEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.UserCreatedEvent`
- **Summary:** Example webhook event for user creation.

#### Methods & Properties

- **#ctor** - Example webhook event for user creation.

### TransactionCompletedEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.TransactionCompletedEvent`
- **Summary:** Example webhook event for transaction completion.

#### Methods & Properties

- **#ctor** - Example webhook event for transaction completion.

### WebhookPayload

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookPayload`
- **Summary:** Webhook payload envelope sent to registered webhook endpoints.

### WebhookRetryPolicy

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookRetryPolicy`
- **Summary:** Retry policy configuration for webhook delivery failures.

#### Methods & Properties

- **GetRetryDelayMs** - Calculate the delay for the nth retry attempt.
            Uses exponential backoff: delay = initialDelay * (backoffMultiplier ^ attempt).

### WebhookSignature

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookSignature`
- **Summary:** Handles HMAC-SHA256 signing and verification of webhook payloads.

#### Methods & Properties

- **Sign** - Sign a webhook payload using the provided secret key.
  - Parameters:
    - `payload`: The webhook payload to sign.
    - `secretKey`: The webhook secret key.
  - Returns: HMAC-SHA256 signature in hex format.
- **Sign** - Sign a JSON string using the provided secret key.
- **Verify** - Verify a webhook payload signature.
  - Parameters:
    - `payload`: The webhook payload.
    - `signature`: The signature to verify.
    - `secretKey`: The webhook secret key.
  - Returns: True if signature is valid, false otherwise.

### WebhookStartupExtensions

- **Namespace:** `SmartWorkz.Shared.WebhookStartupExtensions`
- **Summary:** Dependency injection extensions for webhook services.
            Call AddWebhooks() during application startup to register required services.

#### Methods & Properties

- **AddWebhooks** - Register webhook services in the dependency injection container.
  - Parameters:
    - `services`: The service collection to configure.
  - Returns: The service collection for chaining.
- **CreateWebhookSchema** - Create the WebhookSubscriptions table schema if it doesn't exist.
            Call this during database initialization.
  - Parameters:
    - `connection`: The database connection.

### WebhookSubscription

- **Namespace:** `SmartWorkz.Shared.WebhookSubscription`
- **Summary:** Represents a registered webhook subscription for event notifications.
            Tracks subscription details, retry configuration, and delivery status.

### IWebhookPublisher

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.IWebhookPublisher`
- **Summary:** Contract for publishing webhook events to registered endpoints.

#### Methods & Properties

- **PublishAsync** - Publish a webhook event to all registered endpoints subscribed to the event type.
  - Parameters:
    - `webhookEvent`: The event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous operation.
- **PublishToEndpointAsync** - Publish a webhook event to a specific endpoint.
  - Parameters:
    - `endpoint`: The webhook endpoint to deliver to.
    - `webhookEvent`: The event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous operation.

### WebhookEndpointRegistration

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookEndpointRegistration`
- **Summary:** Represents a registered webhook endpoint.

### WebhookEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookEvent`
- **Summary:** Base class for webhook events published by the system.

### UserCreatedEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.UserCreatedEvent`
- **Summary:** Example webhook event for user creation.

#### Methods & Properties

- **#ctor** - Example webhook event for user creation.

### TransactionCompletedEvent

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.TransactionCompletedEvent`
- **Summary:** Example webhook event for transaction completion.

#### Methods & Properties

- **#ctor** - Example webhook event for transaction completion.

### WebhookPayload

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookPayload`
- **Summary:** Webhook payload envelope sent to registered webhook endpoints.

### WebhookRetryPolicy

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookRetryPolicy`
- **Summary:** Retry policy configuration for webhook delivery failures.

#### Methods & Properties

- **GetRetryDelayMs** - Calculate the delay for the nth retry attempt.
            Uses exponential backoff: delay = initialDelay * (backoffMultiplier ^ attempt).

### WebhookSignature

- **Namespace:** `SmartWorkz.Core.Shared.Webhooks.WebhookSignature`
- **Summary:** Handles HMAC-SHA256 signing and verification of webhook payloads.

#### Methods & Properties

- **Sign** - Sign a webhook payload using the provided secret key.
  - Parameters:
    - `payload`: The webhook payload to sign.
    - `secretKey`: The webhook secret key.
  - Returns: HMAC-SHA256 signature in hex format.
- **Sign** - Sign a JSON string using the provided secret key.
- **Verify** - Verify a webhook payload signature.
  - Parameters:
    - `payload`: The webhook payload.
    - `signature`: The signature to verify.
    - `secretKey`: The webhook secret key.
  - Returns: True if signature is valid, false otherwise.

### WebhooksClient

- **Namespace:** `SmartWorkz.Clients.WebhooksClient`
- **Summary:** Default implementation of .

### WebhooksClient

- **Namespace:** `SmartWorkz.Clients.WebhooksClient`
- **Summary:** Default implementation of .

