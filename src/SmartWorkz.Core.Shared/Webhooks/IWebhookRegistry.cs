namespace SmartWorkz.Shared;

/// <summary>
/// Abstraction for managing webhook subscriptions and registrations.
/// Supports CRUD operations and subscription queries.
/// </summary>
public interface IWebhookRegistry
{
    /// <summary>
    /// Register a new webhook subscription.
    /// </summary>
    /// <param name="url">The webhook endpoint URL.</param>
    /// <param name="events">Array of event names to subscribe to.</param>
    /// <param name="secret">Optional HMAC-SHA256 secret for signature verification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the newly registered subscription.</returns>
    Task<Guid> RegisterAsync(string url, string[] events, string? secret = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregister and remove a webhook subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to unregister.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UnregisterAsync(Guid subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active subscriptions for a specific event.
    /// </summary>
    /// <param name="eventName">The event name to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of subscriptions interested in this event.</returns>
    Task<IReadOnlyCollection<WebhookSubscription>> GetSubscriptionsForEventAsync(string eventName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all currently active subscriptions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of all active subscriptions.</returns>
    Task<IReadOnlyCollection<WebhookSubscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update the status and failure tracking of a subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to update.</param>
    /// <param name="isActive">Whether the subscription should remain active.</param>
    /// <param name="failureCount">Number of consecutive failures (null to leave unchanged).</param>
    /// <param name="failureReason">Reason for failure (null to clear).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateSubscriptionStatusAsync(Guid subscriptionId, bool isActive, int? failureCount = null, string? failureReason = null, CancellationToken cancellationToken = default);
}
