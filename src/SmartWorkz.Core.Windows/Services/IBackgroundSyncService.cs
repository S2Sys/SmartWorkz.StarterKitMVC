namespace SmartWorkz.Windows.Services;

/// <summary>
/// Interface for background synchronization service
/// </summary>
public interface IBackgroundSyncService
{
    /// <summary>
    /// Observable stream of sync status changes
    /// </summary>
    IObservable<SyncStatusChanged> SyncStatusChanged { get; }

    /// <summary>
    /// Start background synchronization
    /// </summary>
    Task StartSyncAsync(SyncOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop background synchronization
    /// </summary>
    Task StopSyncAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current sync status
    /// </summary>
    Task<SyncStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Queue a sync operation
    /// </summary>
    Task QueueSyncAsync(string itemId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Options for background sync configuration
/// </summary>
public class SyncOptions
{
    /// <summary>
    /// Interval between sync operations in seconds
    /// </summary>
    public int SyncIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Whether to sync only on WiFi
    /// </summary>
    public bool RequireWifi { get; set; } = false;
}

/// <summary>
/// Current sync status
/// </summary>
public record SyncStatus(
    bool IsRunning,
    int ItemsQueued,
    int ItemsSynced,
    DateTime? LastSyncTime,
    string? LastError);

/// <summary>
/// Sync status change event
/// </summary>
public record SyncStatusChanged(
    DateTime Timestamp,
    bool IsRunning,
    string? Message);
