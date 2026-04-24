using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SmartWorkz.Windows.Services;

/// <summary>
/// Windows implementation of background synchronization service
/// </summary>
public class WindowsBackgroundSyncService : IBackgroundSyncService, IDisposable
{
    private readonly ILogger<WindowsBackgroundSyncService> _logger;
    private readonly Subject<SyncStatusChanged> _statusSubject = new();
    private readonly Queue<string> _syncQueue = new();
    private readonly object _lockObject = new();

    private bool _isRunning;
    private int _itemsSynced;
    private DateTime? _lastSyncTime;
    private string? _lastError;
    private SyncOptions _options;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _syncTask;
    private bool _disposed;

    public IObservable<SyncStatusChanged> SyncStatusChanged => _statusSubject;

    public WindowsBackgroundSyncService(ILogger<WindowsBackgroundSyncService>? logger = null)
    {
        _logger = logger ?? new NullLogger<WindowsBackgroundSyncService>();
        _options = new SyncOptions();
    }

    public async Task StartSyncAsync(SyncOptions? options = null, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_isRunning)
        {
            _logger.LogWarning("Sync is already running");
            return;
        }

        try
        {
            _options = options ?? new SyncOptions();
            _isRunning = true;
            _itemsSynced = 0;
            _lastError = null;

            EmitStatusChange("Background sync started");

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _syncTask = RunSyncLoopAsync(_cancellationTokenSource.Token);

            _logger.LogInformation("Background sync service started");

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _isRunning = false;
            _lastError = ex.Message;
            _logger.LogError(ex, "Failed to start background sync");
            throw;
        }
    }

    public async Task StopSyncAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (!_isRunning)
        {
            return;
        }

        try
        {
            _isRunning = false;
            _cancellationTokenSource?.Cancel();

            if (_syncTask != null)
            {
                await _syncTask;
            }

            EmitStatusChange("Background sync stopped");
            _logger.LogInformation("Background sync service stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping background sync");
            throw;
        }
    }

    public async Task<SyncStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await Task.CompletedTask;

        lock (_lockObject)
        {
            return new SyncStatus(
                IsRunning: _isRunning,
                ItemsQueued: _syncQueue.Count,
                ItemsSynced: _itemsSynced,
                LastSyncTime: _lastSyncTime,
                LastError: _lastError);
        }
    }

    public async Task QueueSyncAsync(string itemId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentException("Item ID cannot be null or empty", nameof(itemId));
        }

        await Task.Run(() =>
        {
            lock (_lockObject)
            {
                _syncQueue.Enqueue(itemId);
            }
        }, cancellationToken);

        _logger.LogInformation("Item queued for sync: {ItemId}", itemId);
    }

    private async Task RunSyncLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && _isRunning)
            {
                try
                {
                    // Process queued items
                    while (_syncQueue.Count > 0 && !cancellationToken.IsCancellationRequested)
                    {
                        string? itemId;
                        lock (_lockObject)
                        {
                            itemId = _syncQueue.Count > 0 ? _syncQueue.Dequeue() : null;
                        }

                        if (itemId != null)
                        {
                            _logger.LogInformation("Syncing item: {ItemId}", itemId);
                            // Simulate sync operation
                            await Task.Delay(100, cancellationToken);

                            lock (_lockObject)
                            {
                                _itemsSynced++;
                                _lastSyncTime = DateTime.UtcNow;
                            }
                        }
                    }

                    // Wait before next sync cycle
                    await Task.Delay(_options.SyncIntervalSeconds * 1000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _lastError = ex.Message;
                    _logger.LogError(ex, "Error during sync operation");
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Sync loop cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync loop error");
        }
    }

    private void EmitStatusChange(string message)
    {
        var statusChange = new SyncStatusChanged(DateTime.UtcNow, _isRunning, message);
        _statusSubject.OnNext(statusChange);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WindowsBackgroundSyncService));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _isRunning = false;
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _syncQueue.Clear();
        _statusSubject.Dispose();
        _disposed = true;
    }
}
