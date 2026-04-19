namespace SmartWorkz.Core.Shared.Communications;

/// <summary>
/// Abstraction for WebSocket client operations.
/// </summary>
public interface IWebSocketClient : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the WebSocket connection is open.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Sends a message asynchronously through the WebSocket connection.
    /// </summary>
    Task SendAsync(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Receives a message asynchronously from the WebSocket connection.
    /// Returns null if the connection is closed.
    /// </summary>
    Task<string?> ReceiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes the WebSocket connection gracefully.
    /// </summary>
    Task CloseAsync(CancellationToken cancellationToken = default);
}
