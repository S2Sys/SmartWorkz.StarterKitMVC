using SmartWorkz.Core.Shared.Webhooks.Models;

namespace SmartWorkz.Core.Shared.Webhooks;

/// <summary>
/// Contract for publishing webhook events to registered endpoints.
/// </summary>
public interface IWebhookPublisher
{
    /// <summary>
    /// Publish a webhook event to all registered endpoints subscribed to the event type.
    /// </summary>
    /// <param name="webhookEvent">The event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    Task PublishAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish a webhook event to a specific endpoint.
    /// </summary>
    /// <param name="endpoint">The webhook endpoint to deliver to.</param>
    /// <param name="webhookEvent">The event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    Task PublishToEndpointAsync(WebhookEndpointRegistration endpoint, WebhookEvent webhookEvent, CancellationToken cancellationToken = default);
}
