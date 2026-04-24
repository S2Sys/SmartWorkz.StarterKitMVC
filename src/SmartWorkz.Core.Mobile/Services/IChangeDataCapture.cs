namespace SmartWorkz.Mobile.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Shared;

/// <summary>
/// Change Data Capture (CDC) service for tracking entity changes during sync.
/// Provides in-memory storage of change records with filtering and lifecycle management.
/// </summary>
public interface IChangeDataCapture
{
    /// <summary>
    /// Record a change to be tracked for sync.
    /// </summary>
    /// <param name="change">The change to record. Must not be null.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> RecordChangeAsync(SyncChange change);

    /// <summary>
    /// Get tracked changes with optional filtering.
    /// </summary>
    /// <param name="entityType">Optional entity type filter (exact match, case-sensitive). Null returns all.</param>
    /// <param name="since">Optional timestamp filter (returns changes >= this time). Null returns all.</param>
    /// <returns>Result with read-only list of changes ordered by Timestamp DESC.</returns>
    Task<Result<IReadOnlyList<SyncChange>>> GetChangesAsync(
        string? entityType = null,
        DateTime? since = null);

    /// <summary>
    /// Get a specific change by ID.
    /// </summary>
    /// <param name="changeId">The unique change identifier.</param>
    /// <returns>Result with the change, or failure if not found.</returns>
    Task<Result<SyncChange>> GetChangeAsync(string changeId);

    /// <summary>
    /// Clear tracked changes (optionally before a timestamp).
    /// </summary>
    /// <param name="before">Optional timestamp. If null, clears all. If provided, clears only changes before this time.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> ClearChangesAsync(DateTime? before = null);

    /// <summary>
    /// Get total count of tracked changes.
    /// </summary>
    /// <returns>Result with the current change count.</returns>
    Task<Result<int>> GetChangeCountAsync();
}
