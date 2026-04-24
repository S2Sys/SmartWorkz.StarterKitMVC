using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SmartWorkz.Windows.Services;

namespace SmartWorkz.Windows.ViewModels;

/// <summary>
/// ViewModel for managing background synchronization UI
/// </summary>
public class SyncViewModel : ViewModelBase, IDisposable
{
    private readonly IBackgroundSyncService _syncService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SyncViewModel> _logger;
    private bool _isSyncing;
    private int _itemsQueued;
    private int _itemsSynced;
    private string? _lastSyncTime;
    private string? _lastError;
    private IDisposable? _statusSubscription;
    private bool _disposed;

    public bool IsSyncing
    {
        get => _isSyncing;
        set => SetProperty(ref _isSyncing, value);
    }

    public int ItemsQueued
    {
        get => _itemsQueued;
        set => SetProperty(ref _itemsQueued, value);
    }

    public int ItemsSynced
    {
        get => _itemsSynced;
        set => SetProperty(ref _itemsSynced, value);
    }

    public string? LastSyncTime
    {
        get => _lastSyncTime;
        set => SetProperty(ref _lastSyncTime, value);
    }

    public string? LastError
    {
        get => _lastError;
        set => SetProperty(ref _lastError, value);
    }

    public ObservableCollection<string> SyncLog { get; } = new();

    public SyncViewModel(
        IBackgroundSyncService syncService,
        INotificationService notificationService,
        ILogger<SyncViewModel>? logger = null)
    {
        _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? new NullLogger<SyncViewModel>();

        SubscribeToSyncStatus();
    }

    /// <summary>
    /// Start synchronization
    /// </summary>
    public async Task StartSyncAsync(SyncOptions? options = null)
    {
        ThrowIfDisposed();

        try
        {
            await _syncService.StartSyncAsync(options);
            await RefreshStatusAsync();

            _logger.LogInformation("Sync started from ViewModel");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start sync");
            LastError = ex.Message;
            await _notificationService.ShowToastAsync("Sync Error", ex.Message, NotificationType.Error);
        }
    }

    /// <summary>
    /// Stop synchronization
    /// </summary>
    public async Task StopSyncAsync()
    {
        ThrowIfDisposed();

        try
        {
            await _syncService.StopSyncAsync();
            await RefreshStatusAsync();

            _logger.LogInformation("Sync stopped from ViewModel");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop sync");
            LastError = ex.Message;
        }
    }

    /// <summary>
    /// Queue an item for synchronization
    /// </summary>
    public async Task QueueItemAsync(string itemId)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentException("Item ID cannot be null or empty", nameof(itemId));
        }

        try
        {
            await _syncService.QueueSyncAsync(itemId);
            AddLogEntry($"Item queued: {itemId}");
            await RefreshStatusAsync();

            _logger.LogInformation("Item queued: {ItemId}", itemId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue item");
            LastError = ex.Message;
        }
    }

    /// <summary>
    /// Refresh the current sync status
    /// </summary>
    public async Task RefreshStatusAsync()
    {
        ThrowIfDisposed();

        try
        {
            var status = await _syncService.GetStatusAsync();
            IsSyncing = status.IsRunning;
            ItemsQueued = status.ItemsQueued;
            ItemsSynced = status.ItemsSynced;

            if (status.LastSyncTime.HasValue)
            {
                LastSyncTime = status.LastSyncTime.Value.ToString("g");
            }

            LastError = status.LastError;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh status");
        }
    }

    private void SubscribeToSyncStatus()
    {
        _statusSubscription = _syncService.SyncStatusChanged
            .Subscribe(change =>
            {
                IsSyncing = change.IsRunning;
                AddLogEntry($"{(change.IsRunning ? "Started" : "Stopped")}: {change.Message}");

                if (change.IsRunning)
                {
                    _notificationService.ShowToastAsync("Sync", "Synchronization started", NotificationType.Information).ConfigureAwait(false);
                }
            });
    }

    private void AddLogEntry(string entry)
    {
        var logEntry = $"[{DateTime.Now:HH:mm:ss}] {entry}";
        SyncLog.Insert(0, logEntry);

        // Keep log size reasonable
        while (SyncLog.Count > 100)
        {
            SyncLog.RemoveAt(SyncLog.Count - 1);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SyncViewModel));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _statusSubscription?.Dispose();
        _disposed = true;
    }
}
