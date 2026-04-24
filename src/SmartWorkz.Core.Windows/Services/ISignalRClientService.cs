namespace SmartWorkz.Windows.Services;

/// <summary>
/// Interface for SignalR real-time communication client
/// </summary>
public interface ISignalRClientService
{
    /// <summary>
    /// Observable stream of connection state changes
    /// </summary>
    IObservable<SignalRConnectionState> StateChanged { get; }

    /// <summary>
    /// Observable stream of received real-time messages
    /// </summary>
    IObservable<RealtimeMessage> MessageReceived { get; }

    /// <summary>
    /// Gets the current connection state
    /// </summary>
    SignalRConnectionState CurrentState { get; }

    /// <summary>
    /// Connect to the SignalR hub asynchronously
    /// </summary>
    Task ConnectAsync(string hubUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect from the SignalR hub asynchronously
    /// </summary>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribe to a channel and register a message handler
    /// </summary>
    Task SubscribeAsync(string channelName, Func<string, Task> handler, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a message through the SignalR connection
    /// </summary>
    Task SendAsync(string method, object? payload, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the state of a SignalR connection
/// </summary>
public enum SignalRConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    Error
}

/// <summary>
/// Represents a real-time message received through SignalR
/// </summary>
public record RealtimeMessage(
    string MessageId,
    string Channel,
    string Method,
    string Payload,
    DateTime ReceivedAt,
    string? UserId = null);
