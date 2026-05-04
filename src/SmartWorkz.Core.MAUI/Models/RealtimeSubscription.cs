namespace SmartWorkz.Mobile.Models;

using System;

/// <summary>
/// Represents a subscription to a real-time channel.
/// Tracks subscription metadata and message activity.
/// </summary>
public sealed record RealtimeSubscription(
    string SubscriptionId,
    string Channel,
    DateTime SubscribedAt,
    bool IsActive,
    int MessageCount,
    DateTime? LastMessageAt)
{
    /// <summary>
    /// Gets the time elapsed since this subscription was created.
    /// </summary>
    public TimeSpan Age => DateTime.UtcNow - SubscribedAt;

    /// <summary>
    /// Gets a user-friendly display name for this subscription.
    /// </summary>
    public string DisplayName => $"{Channel} (active: {MessageCount} messages)";
}
