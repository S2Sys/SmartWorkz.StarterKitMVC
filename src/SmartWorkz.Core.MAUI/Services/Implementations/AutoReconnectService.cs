namespace SmartWorkz.Mobile.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Shared;

/// <summary>
/// Automatic reconnection service with exponential backoff for resilient real-time connections.
/// </summary>
public class AutoReconnectService : IAutoReconnectService
{
    private readonly IRealtimeService _realtimeService;
    private readonly IOfflineMessageQueue _messageQueue;
    private readonly ILogger<AutoReconnectService>? _logger;

    private IDisposable? _connectionStateSubscription;
    private CancellationTokenSource? _backoffCancellationTokenSource;
    private string? _currentUserId;
    private bool _isMonitoring;
    private bool _isReconnecting;

    // Statistics tracking
    private int _totalAttempts;
    private int _successfulReconnects;
    private DateTime _lastReconnectTime = DateTime.MinValue;
    private List<TimeSpan> _reconnectDurations = new();
    private int _currentRetryCount;

    // Backoff configuration: 0.5s, 1s, 2s, 4s, 8s, 16s, 30s max
    private static readonly TimeSpan[] BackoffDurations = new[]
    {
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(4),
        TimeSpan.FromSeconds(8),
        TimeSpan.FromSeconds(16),
        TimeSpan.FromSeconds(30),
    };

    private const int MaxRetries = 7;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoReconnectService"/> class.
    /// </summary>
    /// <param name="realtimeService">The real-time service to manage connections.</param>
    /// <param name="messageQueue">The offline message queue to flush on reconnection.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public AutoReconnectService(
        IRealtimeService realtimeService,
        IOfflineMessageQueue messageQueue,
        ILogger<AutoReconnectService>? logger = null)
    {
        Guard.NotNull(realtimeService, nameof(realtimeService));
        Guard.NotNull(messageQueue, nameof(messageQueue));

        _realtimeService = realtimeService;
        _messageQueue = messageQueue;
        _logger = logger;
    }

    /// <summary>
    /// Gets a value indicating whether the service is currently reconnecting.
    /// </summary>
    public bool IsReconnecting => _isReconnecting;

    /// <summary>
    /// Start auto-reconnect monitoring.
    /// </summary>
    /// <param name="userId">The user ID to reconnect as.</param>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result> StartAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Fail("AutoReconnect.InvalidUserId", "User ID cannot be null or empty");
        }

        if (_isMonitoring)
        {
            _logger?.LogWarning("Auto-reconnect monitoring is already active for user {UserId}", _currentUserId);
            return Result.Ok(); // Already monitoring
        }

        _currentUserId = userId;
        _isMonitoring = true;
        _totalAttempts = 0;
        _successfulReconnects = 0;
        _currentRetryCount = 0;
        _reconnectDurations.Clear();

        _logger?.LogInformation("Starting auto-reconnect monitoring for user {UserId}", userId);

        // Subscribe to connection state changes
        _connectionStateSubscription = _realtimeService
            .OnConnectionStateChanged()
            .Subscribe(OnConnectionStateChanged);

        return await Task.FromResult(Result.Ok());
    }

    /// <summary>
    /// Stop auto-reconnect monitoring.
    /// </summary>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result> StopAsync()
    {
        if (!_isMonitoring)
        {
            return await Task.FromResult(Result.Ok());
        }

        _isMonitoring = false;
        _isReconnecting = false;

        _connectionStateSubscription?.Dispose();
        _connectionStateSubscription = null;

        _backoffCancellationTokenSource?.Cancel();
        _backoffCancellationTokenSource?.Dispose();
        _backoffCancellationTokenSource = null;

        _logger?.LogInformation("Stopped auto-reconnect monitoring");

        return await Task.FromResult(Result.Ok());
    }

    /// <summary>
    /// Get reconnection statistics.
    /// </summary>
    /// <returns>Result containing reconnection statistics.</returns>
    public async Task<Result<ReconnectStats>> GetStatsAsync()
    {
        var avgDuration = _reconnectDurations.Any()
            ? TimeSpan.FromMilliseconds(_reconnectDurations.Average(d => d.TotalMilliseconds))
            : TimeSpan.Zero;

        var stats = new ReconnectStats(
            TotalAttempts: _totalAttempts,
            SuccessfulReconnects: _successfulReconnects,
            LastReconnectTime: _lastReconnectTime,
            AvgReconnectDuration: avgDuration,
            CurrentRetryCount: _currentRetryCount);

        return await Task.FromResult(Result.Ok(stats));
    }

    /// <summary>
    /// Force immediate reconnection attempt.
    /// </summary>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result> ReconnectImmediatelyAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentUserId))
        {
            return Result.Fail("AutoReconnect.NoActiveSession", "No active reconnection session");
        }

        // Cancel current backoff timer
        _backoffCancellationTokenSource?.Cancel();
        _backoffCancellationTokenSource?.Dispose();
        _backoffCancellationTokenSource = new CancellationTokenSource();

        return await PerformReconnectionAttemptAsync(_currentUserId);
    }

    /// <summary>
    /// Handle connection state changes and initiate reconnection if needed.
    /// </summary>
    private void OnConnectionStateChanged(RealtimeConnectionState state)
    {
        if (!_isMonitoring)
        {
            return;
        }

        _logger?.LogDebug("Connection state changed to {State}", state);

        switch (state)
        {
            case RealtimeConnectionState.Connected:
                OnConnected();
                break;

            case RealtimeConnectionState.Disconnected:
            case RealtimeConnectionState.Error:
            case RealtimeConnectionState.ReconnectingFromDisconnection:
                OnConnectionLost();
                break;

            case RealtimeConnectionState.Connecting:
            case RealtimeConnectionState.Reconnecting:
                // Already attempting to connect, no action needed
                break;
        }
    }

    /// <summary>
    /// Called when the connection is successfully established.
    /// </summary>
    private void OnConnected()
    {
        _isReconnecting = false;
        _currentRetryCount = 0;
        _backoffCancellationTokenSource?.Cancel();

        _logger?.LogInformation("Connection active, monitoring for state changes");

        // Fire and forget: flush offline message queue
        _ = FlushOfflineQueueAsync();
    }

    /// <summary>
    /// Called when the connection is lost or an error occurs.
    /// </summary>
    private void OnConnectionLost()
    {
        if (_isReconnecting)
        {
            return; // Already reconnecting
        }

        _isReconnecting = true;
        _currentRetryCount = 0;

        _logger?.LogWarning("Connection lost, initiating auto-reconnect with exponential backoff");

        // Start reconnection backoff loop
        _ = PerformReconnectionBackoffAsync();
    }

    /// <summary>
    /// Perform reconnection attempts with exponential backoff.
    /// </summary>
    private async Task PerformReconnectionBackoffAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentUserId))
        {
            return;
        }

        _backoffCancellationTokenSource = new CancellationTokenSource();

        while (_isReconnecting && _currentRetryCount < MaxRetries)
        {
            try
            {
                // Determine backoff duration
                var backoffDuration = GetBackoffDuration(_currentRetryCount);

                _logger?.LogInformation(
                    "Reconnection attempt {Attempt} of {MaxRetries}, waiting {BackoffMs}ms",
                    _currentRetryCount + 1,
                    MaxRetries,
                    backoffDuration.TotalMilliseconds);

                // Wait for backoff duration or cancellation
                await Task.Delay(backoffDuration, _backoffCancellationTokenSource.Token);

                if (!_isReconnecting)
                {
                    break; // Stopped before trying to reconnect
                }

                // Attempt reconnection
                var result = await PerformReconnectionAttemptAsync(_currentUserId);

                if (result.Succeeded)
                {
                    _isReconnecting = false;
                    _currentRetryCount = 0;
                    _successfulReconnects++;
                    _lastReconnectTime = DateTime.UtcNow;
                    _logger?.LogInformation("Successfully reconnected after {Attempts} attempts", _totalAttempts);
                    break;
                }

                _currentRetryCount++;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogDebug("Reconnection backoff was cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error during reconnection backoff");
                _currentRetryCount++;
            }
        }

        if (_isReconnecting && _currentRetryCount >= MaxRetries)
        {
            _isReconnecting = false;
            _logger?.LogError(
                "Reconnection failed after {Attempts} attempts, stopping auto-reconnect",
                _totalAttempts);
        }
    }

    /// <summary>
    /// Perform a single reconnection attempt.
    /// </summary>
    private async Task<Result> PerformReconnectionAttemptAsync(string userId)
    {
        var stopwatch = Stopwatch.StartNew();
        _totalAttempts++;

        _logger?.LogDebug("Attempting reconnection for user {UserId} (attempt {Attempt})", userId, _totalAttempts);

        try
        {
            var result = await _realtimeService.ConnectAsync(userId, CancellationToken.None);

            stopwatch.Stop();
            _reconnectDurations.Add(stopwatch.Elapsed);

            if (result.Succeeded)
            {
                _logger?.LogInformation("Reconnection attempt {Attempt} succeeded in {ElapsedMs}ms", _totalAttempts, stopwatch.ElapsedMilliseconds);
                return result;
            }

            _logger?.LogWarning(
                "Reconnection attempt {Attempt} failed in {ElapsedMs}ms: {Error}",
                _totalAttempts,
                stopwatch.ElapsedMilliseconds,
                result.MessageKey ?? "Unknown error");

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger?.LogError(ex, "Exception during reconnection attempt {Attempt} after {ElapsedMs}ms", _totalAttempts, stopwatch.ElapsedMilliseconds);
            return Result.Fail("AutoReconnect.ConnectionException", ex.Message);
        }
    }

    /// <summary>
    /// Flush all queued messages that were waiting for a connection.
    /// </summary>
    private async Task FlushOfflineQueueAsync()
    {
        try
        {
            var messagesResult = await _messageQueue.GetQueuedMessagesAsync();

            if (!messagesResult.Succeeded || messagesResult.Data == null || !messagesResult.Data.Any())
            {
                return;
            }

            _logger?.LogInformation(
                "Flushing {MessageCount} queued messages after reconnection",
                messagesResult.Data.Count);

            foreach (var message in messagesResult.Data)
            {
                try
                {
                    var sendResult = await _realtimeService.SendAsync(
                        message.Method,
                        message.Args ?? [],
                        CancellationToken.None);

                    if (sendResult.Succeeded)
                    {
                        await _messageQueue.DequeueAsync(message.MessageId);
                        _logger?.LogDebug("Dequeued message {MessageId} after successful send", message.MessageId);
                    }
                    else
                    {
                        _logger?.LogWarning(
                            "Failed to send queued message {MessageId}: {Error}",
                            message.MessageId,
                            sendResult.MessageKey);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Exception while flushing queued message {MessageId}", message.MessageId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Exception during queue flush operation");
        }
    }

    /// <summary>
    /// Get the backoff duration for the current retry count.
    /// </summary>
    /// <param name="retryCount">The current retry count (0-based).</param>
    /// <returns>The backoff duration to wait before the next attempt.</returns>
    private static TimeSpan GetBackoffDuration(int retryCount)
    {
        if (retryCount < 0 || retryCount >= BackoffDurations.Length)
        {
            return BackoffDurations[^1]; // Return max duration
        }

        return BackoffDurations[retryCount];
    }
}
