namespace SmartWorkz.Mobile.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Shared;

/// <summary>
/// Advanced sync service with conflict resolution and change tracking.
/// </summary>
public interface IAdvancedSyncService
{
    /// <summary>
    /// Sync data with conflict resolution.
    /// </summary>
    Task<Result<SyncResult>> SyncAsync<T>(
        string endpoint,
        ConflictResolutionStrategy strategy = ConflictResolutionStrategy.LastWriteWins,
        CancellationToken ct = default)
        where T : class;

    /// <summary>
    /// Get pending local changes.
    /// </summary>
    Task<IEnumerable<SyncChange>> GetChangesAsync<T>() where T : class;

    /// <summary>
    /// Get current sync status.
    /// </summary>
    Task<SyncStatus> GetSyncStatusAsync();

    /// <summary>
    /// Observable of sync progress updates.
    /// </summary>
    IObservable<SyncProgress> OnSyncProgressChanged();

    /// <summary>
    /// Register a custom conflict resolver.
    /// </summary>
    void RegisterResolver(IConflictResolver resolver);

    /// <summary>
    /// Get resolved conflicts log.
    /// </summary>
    Task<IReadOnlyList<SyncConflict>> GetResolvedConflictsAsync(int limit = 100);
}

/// <summary>
/// Result of a sync operation.
/// </summary>
public record SyncResult(
    bool Success,
    int LocalChangesApplied,
    int RemoteChangesApplied,
    int ConflictsResolved,
    TimeSpan Duration,
    string? Error = null);

/// <summary>
/// Current status of sync operations.
/// </summary>
public record SyncStatus(
    bool IsSyncing,
    int PendingChanges,
    DateTime? LastSyncTime,
    string? LastError);

/// <summary>
/// Progress update during sync operation.
/// </summary>
public record SyncProgress(
    int ProcessedChanges,
    int TotalChanges,
    double PercentComplete,
    string CurrentPhase);
