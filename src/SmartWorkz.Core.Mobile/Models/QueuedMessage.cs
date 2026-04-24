namespace SmartWorkz.Mobile.Models;

using System;

/// <summary>
/// Represents a message queued for sending when offline.
/// </summary>
public sealed record QueuedMessage(
    string MessageId,
    string Channel,
    string Method,
    object?[] Args,
    DateTime QueuedAt,
    int RetryCount = 0)
{
    /// <summary>
    /// Gets a value indicating whether this message has expired (older than 24 hours).
    /// </summary>
    public bool IsExpired => DateTime.UtcNow - QueuedAt > TimeSpan.FromHours(24);

    /// <summary>
    /// Gets a human-readable display name for the queued message.
    /// </summary>
    public string DisplayName => $"{Method} on {Channel} (queued {(int)(DateTime.UtcNow - QueuedAt).TotalSeconds}s ago)";
}
