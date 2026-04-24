using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SmartWorkz.Windows.Services;

/// <summary>
/// Windows implementation of SignalR real-time communication client
/// </summary>
public class WindowsSignalRClientService : ISignalRClientService, IDisposable
{
    private readonly ILogger<WindowsSignalRClientService> _logger;
    private readonly Subject<SignalRConnectionState> _stateSubject = new();
    private readonly Subject<RealtimeMessage> _messageSubject = new();
    private readonly Dictionary<string, List<Func<string, Task>>> _subscriptions = new();
    private readonly object _lockObject = new();

    private SignalRConnectionState _currentState = SignalRConnectionState.Disconnected;
    private string? _currentHubUrl;
    private bool _disposed;

    public IObservable<SignalRConnectionState> StateChanged => _stateSubject;
    public IObservable<RealtimeMessage> MessageReceived => _messageSubject;
    public SignalRConnectionState CurrentState => _currentState;

    public WindowsSignalRClientService(ILogger<WindowsSignalRClientService>? logger = null)
    {
        _logger = logger ?? new NullLogger<WindowsSignalRClientService>();
    }

    public async Task ConnectAsync(string hubUrl, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(hubUrl))
        {
            UpdateState(SignalRConnectionState.Error);
            _logger.LogError("Hub URL cannot be null or empty");
            return;
        }

        try
        {
            UpdateState(SignalRConnectionState.Connecting);
            _currentHubUrl = hubUrl;

            // Simulate async operation
            await Task.Delay(100, cancellationToken);

            UpdateState(SignalRConnectionState.Connected);
            _logger.LogInformation("Connected to SignalR hub at {HubUrl}", hubUrl);
        }
        catch (OperationCanceledException)
        {
            UpdateState(SignalRConnectionState.Disconnected);
            _logger.LogInformation("Connection cancelled");
            throw;
        }
        catch (Exception ex)
        {
            UpdateState(SignalRConnectionState.Error);
            _logger.LogError(ex, "Failed to connect to SignalR hub");
            throw;
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            // Simulate async operation
            await Task.Delay(50, cancellationToken);

            UpdateState(SignalRConnectionState.Disconnected);
            _currentHubUrl = null;
            _logger.LogInformation("Disconnected from SignalR hub");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disconnect");
            throw;
        }
    }

    public async Task SubscribeAsync(string channelName, Func<string, Task> handler, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(channelName))
        {
            throw new ArgumentException("Channel name cannot be null or empty", nameof(channelName));
        }

        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        await Task.Run(() =>
        {
            lock (_lockObject)
            {
                if (!_subscriptions.ContainsKey(channelName))
                {
                    _subscriptions[channelName] = new List<Func<string, Task>>();
                }

                _subscriptions[channelName].Add(handler);
            }
        }, cancellationToken);

        _logger.LogInformation("Subscribed to channel: {ChannelName}", channelName);
    }

    public async Task SendAsync(string method, object? payload, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (CurrentState != SignalRConnectionState.Connected)
        {
            throw new InvalidOperationException("SignalR client is not connected");
        }

        if (string.IsNullOrWhiteSpace(method))
        {
            throw new ArgumentException("Method name cannot be null or empty", nameof(method));
        }

        try
        {
            // Simulate async operation
            await Task.Delay(50, cancellationToken);
            _logger.LogInformation("Sent message to method: {Method}", method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message");
            throw;
        }
    }

    /// <summary>
    /// Test helper to simulate receiving a message
    /// </summary>
    public async Task SimulateReceiveMessageAsync(RealtimeMessage message)
    {
        ThrowIfDisposed();
        await Task.Run(() => _messageSubject.OnNext(message));
    }

    /// <summary>
    /// Test helper to simulate state change
    /// </summary>
    public void SimulateStateChange(SignalRConnectionState state)
    {
        ThrowIfDisposed();
        UpdateState(state);
    }

    private void UpdateState(SignalRConnectionState newState)
    {
        lock (_lockObject)
        {
            if (_currentState != newState)
            {
                _currentState = newState;
                _stateSubject.OnNext(newState);
            }
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WindowsSignalRClientService));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _stateSubject.Dispose();
        _messageSubject.Dispose();
        _subscriptions.Clear();
        _disposed = true;
    }
}
