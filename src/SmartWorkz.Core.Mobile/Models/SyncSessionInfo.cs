namespace SmartWorkz.Mobile.Models;

using System;

/// <summary>
/// Represents a completed sync session with metadata about its execution.
/// Used for auditing and tracking sync progress across sessions.
/// </summary>
public sealed record SyncSessionInfo(
    string SessionId,
    string EntityType,
    DateTime StartTime,
    DateTime? EndTime,
    bool IsSuccessful,
    string? ErrorMessage,
    int ChangesProcessed,
    int ConflictsResolved)
{
    /// <summary>
    /// Get the total duration of the sync session.
    /// If still in progress (EndTime is null), calculates from StartTime to UtcNow.
    /// </summary>
    public TimeSpan Duration => (EndTime ?? DateTime.UtcNow) - StartTime;

    /// <summary>
    /// Check if the session has completed (EndTime is set).
    /// </summary>
    public bool IsCompleted => EndTime.HasValue;
}
