namespace SmartWorkz.Mobile.Models;

using System;

/// <summary>
/// Represents the persistent state of sync progress for a specific entity type.
/// Persisted to SQLite and used to determine if sync is needed and track progress.
/// </summary>
public sealed record PersistentSyncState(
    string EntityType,
    DateTime LastSyncTime,
    int PendingChangesCount,
    int ResolvedConflictsCount,
    string? LastErrorMessage)
{
    /// <summary>
    /// Check if this entity type has pending changes that need to be synced.
    /// </summary>
    public bool NeedsSync => PendingChangesCount > 0;

    /// <summary>
    /// Get the time elapsed since the last successful sync.
    /// </summary>
    public TimeSpan TimeSinceLastSync => DateTime.UtcNow - LastSyncTime;
}
