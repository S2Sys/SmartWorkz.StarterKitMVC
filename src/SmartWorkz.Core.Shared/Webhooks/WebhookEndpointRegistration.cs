namespace SmartWorkz.Core.Shared.Webhooks;

/// <summary>
/// Represents a registered webhook endpoint.
/// </summary>
public class WebhookEndpointRegistration
{
    /// <summary>
    /// Unique identifier for this webhook endpoint.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Tenant ID that owns this webhook.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// The URL where webhooks will be sent.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Secret key for signing webhook payloads (HMAC-SHA256).
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Event types this endpoint is subscribed to (e.g., "user.created", "transaction.completed").
    /// </summary>
    public List<string> EventTypes { get; set; } = new();

    /// <summary>
    /// Whether this webhook endpoint is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the endpoint was registered.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the endpoint was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Number of consecutive delivery failures (resets on success).
    /// </summary>
    public int FailureCount { get; set; } = 0;

    /// <summary>
    /// When the last delivery attempt was made.
    /// </summary>
    public DateTime? LastAttemptAt { get; set; }
}
