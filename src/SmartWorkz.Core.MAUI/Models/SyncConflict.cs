namespace SmartWorkz.Mobile.Models;

using System;

/// <summary>
/// Represents a conflict between local and remote changes.
/// </summary>
public sealed record SyncConflict(
    string ConflictId,
    SyncChange LocalChange,
    SyncChange RemoteChange,
    ConflictResolutionStrategy ResolutionStrategy,
    DateTime DetectedAt)
{
    /// <summary>
    /// Unique identifier for this conflict.
    /// </summary>
    public string ConflictId { get; } = ConflictId;

    /// <summary>
    /// Get time difference between changes (in seconds).
    /// </summary>
    public double TimestampDifferenceSeconds =>
        Math.Abs((RemoteChange.Timestamp - LocalChange.Timestamp).TotalSeconds);

    /// <summary>
    /// Check if local change is more recent.
    /// </summary>
    public bool LocalIsNewer => LocalChange.Timestamp > RemoteChange.Timestamp;

    /// <summary>
    /// Get the winning change based on strategy.
    /// </summary>
    public SyncChange GetWinningChange() => ResolutionStrategy switch
    {
        ConflictResolutionStrategy.LastWriteWins => LocalIsNewer ? LocalChange : RemoteChange,
        ConflictResolutionStrategy.ClientWins => LocalChange,
        ConflictResolutionStrategy.ServerWins => RemoteChange,
        _ => LocalChange, // Default to local for custom resolver
    };

    /// <summary>
    /// Get display name for logging.
    /// </summary>
    public string DisplayName =>
        $"{LocalChange.EntityType}({LocalChange.EntityId}): " +
        $"Local({LocalChange.Timestamp:O}) vs Remote({RemoteChange.Timestamp:O})";
}
