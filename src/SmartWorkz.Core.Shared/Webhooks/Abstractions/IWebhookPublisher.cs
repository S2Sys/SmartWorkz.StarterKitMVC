using SmartWorkz.Core.Shared.Webhooks.Models;

namespace SmartWorkz.Core.Shared.Webhooks.Abstractions;

/// <summary>
/// Contract for publishing webhook events to registered endpoints.
///
/// <para><strong>Purpose</strong>: Defines the contract for publishing webhook events to all
/// subscribed endpoints or to specific endpoints. Implementations handle delivery logic,
/// retry policies, signature generation, and failure tracking.</para>
///
/// <para><strong>Implementations</strong>:
/// • WebhookPublisher - Default implementation with HTTP delivery and exponential backoff
/// </para>
/// </summary>
public interface IWebhookPublisher
{
    /// <summary>
    /// Publish a webhook event to all registered endpoints subscribed to the event type.
    ///
    /// <para>This method finds all active endpoints that subscribe to the given event type
    /// and publishes the event to each endpoint asynchronously. Delivery failures trigger
    /// retry logic with exponential backoff.</para>
    /// </summary>
    /// <param name="webhookEvent">The event to publish. Must include EventType and TenantId.</param>
    /// <param name="cancellationToken">Cancellation token for graceful shutdown.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if webhookEvent is null.</exception>
    Task PublishAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish a webhook event to a specific endpoint.
    ///
    /// <para>This method delivers the webhook event directly to the specified endpoint,
    /// handling HMAC-SHA256 payload signing, HTTP delivery, and failure tracking.
    /// On failure, updates the endpoint's failure count and reason for debugging.</para>
    /// </summary>
    /// <param name="endpoint">The webhook endpoint to deliver to. Must have valid URL and SecretKey.</param>
    /// <param name="webhookEvent">The event to publish.</param>
    /// <param name="cancellationToken">Cancellation token for graceful shutdown.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if endpoint or webhookEvent is null.</exception>
    /// <exception cref="ArgumentException">Thrown if endpoint URL is invalid or TenantId does not match.</exception>
    Task PublishToEndpointAsync(WebhookEndpointRegistration endpoint, WebhookEvent webhookEvent, CancellationToken cancellationToken = default);
}
