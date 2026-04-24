namespace SmartWorkz.Mobile.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Shared;

/// <summary>
/// Service for detecting conflicts between local and remote changes.
/// Identifies when the same entity property has been modified differently in both locations.
/// </summary>
public class ConflictDetector : IConflictDetector
{
    private readonly ILogger<ConflictDetector>? _logger;
    private readonly object _lockObject = new();
    private int _totalDetectionRuns;
    private int _totalConflicts;
    private int _entitiesInvolved;
    private DateTime _lastDetectionTime;

    /// <summary>
    /// Initializes a new instance of the ConflictDetector class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic information</param>
    public ConflictDetector(ILogger<ConflictDetector>? logger = null)
    {
        _logger = logger;
        _totalDetectionRuns = 0;
        _totalConflicts = 0;
        _entitiesInvolved = 0;
        _lastDetectionTime = DateTime.MinValue;
    }

    /// <summary>
    /// Detect conflicts between local and remote changes.
    /// A conflict exists when the same EntityId and Property have been modified
    /// with different values in both local and remote changes.
    /// </summary>
    /// <param name="localChanges">Changes made locally</param>
    /// <param name="remoteChanges">Changes received from server</param>
    /// <returns>Result with list of detected conflicts, empty if none</returns>
    public async Task<Result<IReadOnlyList<SyncConflict>>> DetectConflictsAsync(
        IReadOnlyList<SyncChange> localChanges,
        IReadOnlyList<SyncChange> remoteChanges)
    {
        Guard.NotNull(localChanges, nameof(localChanges));
        Guard.NotNull(remoteChanges, nameof(remoteChanges));

        try
        {
            var conflicts = new List<SyncConflict>();
            var detectedAt = DateTime.UtcNow;

            // Build a lookup dictionary for remote changes by (EntityId, EntityType, Property) tuple
            var remoteMap = new Dictionary<(string, string, string), SyncChange>();
            foreach (var remote in remoteChanges)
            {
                var key = (remote.EntityId, remote.EntityType, remote.Property);
                remoteMap[key] = remote;
            }

            // Track unique entities involved in conflicts
            var entitiesInConflict = new HashSet<string>();

            // Iterate through local changes and check for conflicts
            foreach (var localChange in localChanges)
            {
                var key = (localChange.EntityId, localChange.EntityType, localChange.Property);

                if (remoteMap.TryGetValue(key, out var remoteChange))
                {
                    // Check if this is actually a conflict (values differ)
                    if (ValuesConflict(localChange, remoteChange))
                    {
                        var conflict = new SyncConflict(
                            Guid.NewGuid().ToString(),
                            localChange,
                            remoteChange,
                            ConflictResolutionStrategy.LastWriteWins, // Default strategy
                            detectedAt);

                        conflicts.Add(conflict);
                        entitiesInConflict.Add(localChange.EntityId);

                        _logger?.LogDebug(
                            "Conflict detected: {Entity}({EntityId}): {Property} - Local: {LocalValue} vs Remote: {RemoteValue}",
                            localChange.EntityType,
                            localChange.EntityId,
                            localChange.Property,
                            localChange.NewValue ?? localChange.OldValue,
                            remoteChange.NewValue ?? remoteChange.OldValue);
                    }
                }
            }

            // Update statistics (thread-safe)
            lock (_lockObject)
            {
                _totalDetectionRuns++;
                _totalConflicts += conflicts.Count;
                _entitiesInvolved += entitiesInConflict.Count;
                _lastDetectionTime = detectedAt;
            }

            _logger?.LogDebug(
                "Conflict detection completed: {ConflictCount} conflicts found in {LocalCount} local vs {RemoteCount} remote changes",
                conflicts.Count,
                localChanges.Count,
                remoteChanges.Count);

            return Result.Ok<IReadOnlyList<SyncConflict>>(conflicts.AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during conflict detection");
            return Result.Fail<IReadOnlyList<SyncConflict>>(
                "ConflictDetection.Error",
                $"Failed to detect conflicts: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if two changes conflict.
    /// Conflicts occur when the same EntityId and Property have different values
    /// in the local and remote changes.
    /// </summary>
    /// <param name="local">Local change to check</param>
    /// <param name="remote">Remote change to check</param>
    /// <returns>Result with boolean indicating if conflict exists</returns>
    public async Task<Result<bool>> ConflictExistsAsync(SyncChange local, SyncChange remote)
    {
        Guard.NotNull(local, nameof(local));
        Guard.NotNull(remote, nameof(remote));

        try
        {
            // Check if same EntityId, EntityType, and Property
            if (local.EntityId != remote.EntityId || local.EntityType != remote.EntityType || local.Property != remote.Property)
            {
                return Result.Ok(false);
            }

            // Check if values conflict
            bool hasConflict = ValuesConflict(local, remote);
            return Result.Ok(hasConflict);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error checking if conflict exists");
            return Result.Fail<bool>(
                "ConflictExists.Error",
                $"Failed to check conflict: {ex.Message}");
        }
    }

    /// <summary>
    /// Get conflict detection statistics.
    /// </summary>
    /// <returns>Result with current conflict detection statistics</returns>
    public async Task<Result<ConflictDetectionStats>> GetStatisticsAsync()
    {
        try
        {
            lock (_lockObject)
            {
                int averageConflicts = _totalDetectionRuns > 0
                    ? _totalConflicts / _totalDetectionRuns
                    : 0;

                var stats = new ConflictDetectionStats(
                    TotalDetectionRuns: _totalDetectionRuns,
                    ConflictsFound: _totalConflicts,
                    EntitiesInvolved: _entitiesInvolved,
                    LastDetectionTime: _lastDetectionTime,
                    AverageConflictsPerRun: averageConflicts);

                _logger?.LogDebug("Conflict detection statistics: {Stats}", stats.DisplaySummary);
                return Result.Ok(stats);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error retrieving conflict detection statistics");
            return Result.Fail<ConflictDetectionStats>(
                "Statistics.Error",
                $"Failed to retrieve statistics: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if two change values conflict.
    /// A conflict exists only when the final values (NewValue) differ.
    /// Different OldValues are not a conflict if both changes produce the same result.
    /// </summary>
    /// <param name="local">Local change</param>
    /// <param name="remote">Remote change</param>
    /// <returns>True if NewValues differ, false otherwise</returns>
    private static bool ValuesConflict(SyncChange local, SyncChange remote)
    {
        // Values conflict only if the final new values differ
        // Use object.Equals for comparison to handle nulls correctly
        return !Equals(local.NewValue, remote.NewValue);
    }
}
