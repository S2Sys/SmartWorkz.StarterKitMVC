namespace SmartWorkz.Mobile;

using System;

/// <summary>
/// Represents a real-time message from SignalR hub.
/// </summary>
public interface IRealtimeMessage
{
    string Channel { get; }
    string Method { get; }
    object? Payload { get; }
    DateTime ReceivedAt { get; }
    string CorrelationId { get; }
}
