using System.Text.Json.Serialization;

namespace SmartWorkz.Core.Shared.Webhooks;

/// <summary>
/// Base class for webhook events published by the system.
/// </summary>
public abstract record WebhookEvent
{
    /// <summary>
    /// Unique identifier for this event.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Type of event (e.g., user.created, transaction.completed).
    /// </summary>
    [JsonPropertyName("type")]
    public abstract string EventType { get; }

    /// <summary>
    /// When the event occurred (UTC).
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Tenant ID for multi-tenancy support.
    /// </summary>
    [JsonPropertyName("tenant_id")]
    public string TenantId { get; init; } = string.Empty;

    /// <summary>
    /// Event data payload.
    /// </summary>
    [JsonPropertyName("data")]
    public abstract object Data { get; }
}

/// <summary>
/// Example webhook event for user creation.
/// </summary>
public record UserCreatedEvent(string UserId, string Email, string FirstName, string LastName) : WebhookEvent
{
    public override string EventType => "user.created";
    public override object Data => new { UserId, Email, FirstName, LastName };
}

/// <summary>
/// Example webhook event for transaction completion.
/// </summary>
public record TransactionCompletedEvent(string TransactionId, decimal Amount, string Status) : WebhookEvent
{
    public override string EventType => "transaction.completed";
    public override object Data => new { TransactionId, Amount, Status };
}
