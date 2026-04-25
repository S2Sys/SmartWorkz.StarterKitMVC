namespace SmartWorkz.Core.Shared.Webhooks.Models;

/// <summary>
/// Represents a registered webhook endpoint for event delivery.
///
/// <para><strong>Purpose</strong>: Stores webhook subscription configuration including URL, secret key,
/// event types, and delivery status. Supports multi-tenancy, exponential backoff retries,
/// and failure tracking.</para>
///
/// <para><strong>Key Features</strong>:
/// • HMAC-SHA256 secret key for payload signing
/// • Event type filtering (subscribe to specific events)
/// • Exponential backoff retry support (configurable delays)
/// • Failure tracking with consecutive failure count
/// • Multi-tenancy support via TenantId
/// </para>
///
/// <para><strong>Dependency Injection Usage</strong>:</para>
///
/// <example>
/// Example 1: Register webhook endpoint via service
/// <code>
/// var endpoint = new WebhookEndpointRegistration
/// {
///     TenantId = "tenant-123",
///     Url = "https://api.example.com/webhooks",
///     SecretKey = Guid.NewGuid().ToString("N"),
///     EventTypes = new[] { "user.created", "transaction.completed" },
///     IsActive = true
/// };
///
/// await webhookRegistry.CreateEndpointAsync(endpoint);
/// </code>
/// </example>
///
/// <example>
/// Example 2: Query active endpoints for an event
/// <code>
/// public class UserCreationHandler
/// {
///     private readonly IWebhookRegistry _registry;
///
///     public UserCreationHandler(IWebhookRegistry registry)
///     {
///         _registry = registry;
///     }
///
///     public async Task OnUserCreatedAsync(User user)
///     {
///         var endpoints = await _registry.GetSubscriptionsForEventAsync("user.created");
///         var evt = new UserCreatedEvent(user.Id, user.Email, user.FirstName, user.LastName);
///
///         foreach (var endpoint in endpoints)
///         {
///             await _webhookPublisher.PublishToEndpointAsync(endpoint, evt);
///         }
///     }
/// }
/// </code>
/// </example>
///
/// <example>
/// Example 3: Use in domain event handler with MassTransit
/// <code>
/// public class UserCreatedEventHandler : IConsumer&lt;UserCreatedDomainEvent&gt;
/// {
///     private readonly IWebhookRegistry _registry;
///     private readonly IWebhookPublisher _publisher;
///
///     public UserCreatedEventHandler(IWebhookRegistry registry, IWebhookPublisher publisher)
///     {
///         _registry = registry;
///         _publisher = publisher;
///     }
///
///     public async Task Consume(ConsumeContext&lt;UserCreatedDomainEvent&gt; context)
///     {
///         var endpoints = await _registry.GetSubscriptionsForEventAsync("user.created");
///
///         foreach (var endpoint in endpoints)
///         {
///             var webhookEvent = new UserCreatedEvent(
///                 context.Message.UserId,
///                 context.Message.Email,
///                 context.Message.FirstName,
///                 context.Message.LastName
///             ) { TenantId = context.Message.TenantId };
///
///             await _publisher.PublishToEndpointAsync(endpoint, webhookEvent);
///         }
///     }
/// }
/// </code>
/// </example>
/// </summary>
public class WebhookEndpointRegistration
{
    private string _url = string.Empty;
    private string _tenantId = string.Empty;

    /// <summary>
    /// Unique identifier for this webhook endpoint registration.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Tenant ID for multi-tenancy isolation. Ensures webhooks only deliver to endpoints
    /// belonging to the same tenant. Cannot be null or empty.
    /// </summary>
    public string TenantId
    {
        get => _tenantId;
        set => _tenantId = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("TenantId cannot be empty", nameof(TenantId))
            : value;
    }

    /// <summary>
    /// The HTTPS URL where webhook payloads will be delivered. Must be a valid,
    /// publicly accessible endpoint that accepts POST requests with JSON body.
    /// Cannot be null or empty.
    /// </summary>
    public string Url
    {
        get => _url;
        set => _url = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("URL cannot be empty", nameof(Url))
            : value;
    }

    /// <summary>
    /// Secret key used to sign webhook payloads via HMAC-SHA256. Recipients should
    /// verify this signature to ensure payload authenticity. Never expose this value
    /// in logs or error messages.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Array of event types this endpoint subscribes to (e.g., "user.created",
    /// "transaction.completed"). Webhook payloads are only sent if their event type
    /// matches one of these values.
    /// </summary>
    public string[] EventTypes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether this endpoint is currently active. Inactive endpoints are not
    /// delivered to even if they match the event type.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Maximum number of retry attempts for failed deliveries. Uses exponential
    /// backoff: delay = 1s * (2 ^ attemptNumber). Retries stop after this limit.
    /// Default: 5 retries.
    /// </summary>
    public int? MaxRetries { get; set; } = 5;

    /// <summary>
    /// HTTP request timeout in seconds for webhook delivery attempts. If the
    /// endpoint takes longer than this, the delivery fails and triggers retry logic.
    /// Default: 30 seconds.
    /// </summary>
    public int? TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// When this endpoint registration was created (UTC).
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// When this endpoint registration was last modified (UTC).
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Count of consecutive delivery failures. Resets to 0 on successful delivery.
    /// Used to identify flaky endpoints.
    /// </summary>
    public int FailureCount { get; set; } = 0;

    /// <summary>
    /// When the last delivery attempt was made (UTC). Used to calculate
    /// when the next retry should occur.
    /// </summary>
    public DateTimeOffset? LastAttemptAt { get; set; }

    /// <summary>
    /// Description of the last failure (e.g., "HTTP 500", "Timeout", "Connection refused").
    /// Helps with debugging delivery issues.
    /// </summary>
    public string? FailureReason { get; set; }
}
