namespace SmartWorkz.Mobile.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Shared;

/// <summary>
/// Service for detecting conflicts between local and remote changes.
/// </summary>
public interface IConflictDetector
{
    /// <summary>
    /// Detect conflicts between local and remote changes.
    /// </summary>
    /// <param name="localChanges">Changes made locally</param>
    /// <param name="remoteChanges">Changes received from server</param>
    /// <returns>Result with list of detected conflicts, empty if none</returns>
    Task<Result<IReadOnlyList<SyncConflict>>> DetectConflictsAsync(
        IReadOnlyList<SyncChange> localChanges,
        IReadOnlyList<SyncChange> remoteChanges);

    /// <summary>
    /// Check if two changes conflict.
    /// </summary>
    /// <param name="local">Local change to check</param>
    /// <param name="remote">Remote change to check</param>
    /// <returns>Result with boolean indicating if conflict exists</returns>
    Task<Result<bool>> ConflictExistsAsync(SyncChange local, SyncChange remote);

    /// <summary>
    /// Get conflict detection statistics.
    /// </summary>
    /// <returns>Result with current conflict detection statistics</returns>
    Task<Result<ConflictDetectionStats>> GetStatisticsAsync();
}
