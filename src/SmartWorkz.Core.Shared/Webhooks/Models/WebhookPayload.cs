using System.Text.Json.Serialization;

namespace SmartWorkz.Core.Shared.Webhooks.Models;

/// <summary>
/// Webhook payload envelope sent to registered webhook endpoints.
/// </summary>
public record WebhookPayload
{
    /// <summary>
    /// Unique identifier for this webhook delivery.
    /// </summary>
    [JsonPropertyName("delivery_id")]
    public string DeliveryId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The webhook event being delivered.
    /// </summary>
    [JsonPropertyName("event")]
    public required WebhookEvent Event { get; init; }

    /// <summary>
    /// HMAC-SHA256 signature of the payload for verification.
    /// </summary>
    [JsonPropertyName("signature")]
    public required string Signature { get; init; }

    /// <summary>
    /// Timestamp when the payload was created (UTC).
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
