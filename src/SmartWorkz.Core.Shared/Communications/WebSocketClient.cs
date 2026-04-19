namespace SmartWorkz.Core.Shared.Communications;

using System.Net.WebSockets;
using System.Text;

/// <summary>
/// Sealed implementation of IWebSocketClient using System.Net.WebSockets.
/// </summary>
public sealed class WebSocketClient : IWebSocketClient
{
    private ClientWebSocket? _webSocket;
    private bool _disposed;
    private const int BufferSize = 1024 * 4;

    public bool IsConnected => _webSocket?.State == WebSocketState.Open;

    /// <summary>
    /// Connects to a WebSocket server at the specified URI.
    /// </summary>
    public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        if (_webSocket != null)
            throw new InvalidOperationException("WebSocket is already connected or disposed");

        _webSocket = new ClientWebSocket();
        try
        {
            await _webSocket.ConnectAsync(uri, cancellationToken);
        }
        catch
        {
            _webSocket?.Dispose();
            _webSocket = null;
            throw;
        }
    }

    /// <summary>
    /// Sends a message asynchronously through the WebSocket connection.
    /// </summary>
    public async Task SendAsync(string message, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposedOrNotConnected();

        var bytes = Encoding.UTF8.GetBytes(message);
        await _webSocket!.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken);
    }

    /// <summary>
    /// Receives a message asynchronously from the WebSocket connection.
    /// Returns null if the connection is closed.
    /// </summary>
    public async Task<string?> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposedOrNotConnected();

        var buffer = new byte[BufferSize];
        var result = await _webSocket!.ReceiveAsync(
            new ArraySegment<byte>(buffer),
            cancellationToken);

        if (result.MessageType == WebSocketMessageType.Close)
            return null;

        return Encoding.UTF8.GetString(buffer, 0, result.Count);
    }

    /// <summary>
    /// Closes the WebSocket connection gracefully.
    /// </summary>
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (_webSocket?.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Closing",
                cancellationToken);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _webSocket?.Dispose();
        _webSocket = null;
        _disposed = true;
    }

    private void ThrowIfDisposedOrNotConnected()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
        if (!IsConnected)
            throw new InvalidOperationException("WebSocket is not connected");
    }
}
