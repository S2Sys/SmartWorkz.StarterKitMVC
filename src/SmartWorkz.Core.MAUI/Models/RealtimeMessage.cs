namespace SmartWorkz.Mobile.Models;

using System;
using System.Text.Json;

/// <summary>
/// Data transfer object for real-time messages from SignalR.
/// </summary>
public sealed record RealtimeMessage(
    string Channel,
    string Method,
    object? Payload,
    DateTime ReceivedAt,
    string CorrelationId) : IRealtimeMessage
{
    /// <summary>
    /// Gets serialized payload as JSON (for logging/debugging).
    /// </summary>
    public string? PayloadJson => Payload != null
        ? JsonSerializer.Serialize(Payload)
        : null;

    /// <summary>
    /// Checks if this message is a system message (from server).
    /// </summary>
    public bool IsSystemMessage => Method.StartsWith("System.");

    /// <summary>
    /// Gets time elapsed since message was received.
    /// </summary>
    public TimeSpan Age => DateTime.UtcNow - ReceivedAt;
}
