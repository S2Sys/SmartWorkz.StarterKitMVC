using SmartWorkz.Core.Shared.Webhooks.Models;

namespace SmartWorkz.Core.Shared.Webhooks.Abstractions;

/// <summary>
/// Contract for managing webhook endpoint registrations and queries.
///
/// <para><strong>Purpose</strong>: Defines the contract for persisting, querying, and managing
/// webhook endpoint registrations. Implementations typically use a database or cache backend.</para>
///
/// <para><strong>Key Responsibilities</strong>:
/// • Register and unregister webhook endpoints
/// • Query endpoints by event type and tenant
/// • Update endpoint status and failure information
/// • Retrieve failed endpoints for retry processing
/// </para>
/// </summary>
public interface IWebhookRegistry
{
    /// <summary>
    /// Get all active webhook endpoints subscribed to a specific event type.
    /// </summary>
    /// <param name="eventType">The event type to query (e.g., "user.created").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of active endpoints subscribed to this event type.</returns>
    Task<IEnumerable<WebhookEndpointRegistration>> GetSubscriptionsForEventAsync(
        string eventType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new webhook endpoint registration.
    /// </summary>
    /// <param name="endpoint">The endpoint registration to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created endpoint with assigned Id.</returns>
    Task<WebhookEndpointRegistration> CreateEndpointAsync(
        WebhookEndpointRegistration endpoint,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing webhook endpoint registration.
    /// </summary>
    /// <param name="endpointId">The ID of the endpoint to update.</param>
    /// <param name="updates">Object containing properties to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated endpoint.</returns>
    Task<WebhookEndpointRegistration> UpdateEndpointAsync(
        string endpointId,
        object updates,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get endpoints with pending retries (failed deliveries).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of endpoints that have failed deliveries.</returns>
    Task<IEnumerable<WebhookEndpointRegistration>> GetFailedEndpointsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a webhook endpoint registration.
    /// </summary>
    /// <param name="endpointId">The ID of the endpoint to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    Task DeleteEndpointAsync(
        string endpointId,
        CancellationToken cancellationToken = default);
}
