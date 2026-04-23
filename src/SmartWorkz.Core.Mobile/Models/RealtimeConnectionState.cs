namespace SmartWorkz.Mobile.Models;

/// <summary>
/// Represents the state of SignalR connection.
/// </summary>
public enum RealtimeConnectionState
{
    Disconnected = 0,
    Connecting = 1,
    Connected = 2,
    ReconnectingFromDisconnection = 3,
    Reconnecting = 4,
    Error = 5,
}
