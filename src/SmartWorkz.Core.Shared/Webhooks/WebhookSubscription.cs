namespace SmartWorkz.Core.Shared.Webhooks;

/// <summary>
/// Represents a registered webhook subscription for event notifications.
/// Tracks subscription details, retry configuration, and delivery status.
/// </summary>
public class WebhookSubscription
{
    /// <summary>Unique identifier for this subscription.</summary>
    public Guid Id { get; set; }

    /// <summary>The webhook endpoint URL where events are posted.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Array of event names this subscription is interested in.</summary>
    public string[] Events { get; set; } = Array.Empty<string>();

    /// <summary>Optional HMAC-SHA256 secret for webhook signature verification.</summary>
    public string? Secret { get; set; }

    /// <summary>Indicates whether this subscription is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Maximum number of retry attempts for delivery (default: 3).</summary>
    public int? MaxRetries { get; set; } = 3;

    /// <summary>Timeout in seconds for HTTP requests (default: 30).</summary>
    public int? TimeoutSeconds { get; set; } = 30;

    /// <summary>Timestamp when this subscription was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Timestamp of the last delivery attempt.</summary>
    public DateTimeOffset? LastTriggeredAt { get; set; }

    /// <summary>Number of consecutive delivery failures.</summary>
    public int? FailureCount { get; set; }

    /// <summary>Description of the last failure (e.g., HTTP status code or exception message).</summary>
    public string? FailureReason { get; set; }
}
