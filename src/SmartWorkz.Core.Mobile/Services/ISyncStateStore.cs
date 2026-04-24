namespace SmartWorkz.Mobile.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Shared;

/// <summary>
/// Persistent storage for sync state using SQLite.
/// Enables sync progress to survive app restarts and track sync history.
/// </summary>
public interface ISyncStateStore
{
    /// <summary>
    /// Initialize the database schema and create tables if they don't exist.
    /// </summary>
    /// <returns>Result indicating success or database initialization failure.</returns>
    Task<Result> InitializeAsync();

    /// <summary>
    /// Save pending changes to persistent storage for an entity type.
    /// Overwrites previous pending changes for the same entity type.
    /// </summary>
    /// <param name="entityType">The entity type identifier (e.g. "Order", "Customer").</param>
    /// <param name="changes">The list of changes to persist. Must not be null.</param>
    /// <returns>Result indicating success or persistence failure.</returns>
    Task<Result> SavePendingChangesAsync(string entityType, IReadOnlyList<SyncChange> changes);

    /// <summary>
    /// Load pending changes from persistent storage for an entity type.
    /// </summary>
    /// <param name="entityType">The entity type identifier.</param>
    /// <returns>Result with read-only list of deserialized changes, empty list if none exist.</returns>
    Task<Result<IReadOnlyList<SyncChange>>> LoadPendingChangesAsync(string entityType);

    /// <summary>
    /// Clear all pending changes for an entity type after successful sync.
    /// </summary>
    /// <param name="entityType">The entity type identifier.</param>
    /// <returns>Result indicating success or deletion failure.</returns>
    Task<Result> ClearPendingChangesAsync(string entityType);

    /// <summary>
    /// Record the completion of a sync session in the audit trail.
    /// </summary>
    /// <param name="session">The completed sync session metadata.</param>
    /// <returns>Result indicating success or insertion failure.</returns>
    Task<Result> RecordSyncSessionAsync(SyncSessionInfo session);

    /// <summary>
    /// Get the current persistent sync state for an entity type.
    /// </summary>
    /// <param name="entityType">The entity type identifier.</param>
    /// <returns>Result with PersistentSyncState, or failure if entity type not found.</returns>
    Task<Result<PersistentSyncState>> GetSyncStateAsync(string entityType);

    /// <summary>
    /// Get recent sync sessions (for auditing and debugging).
    /// </summary>
    /// <param name="limit">Maximum number of sessions to return. Default 10.</param>
    /// <returns>Result with read-only list of sessions ordered by StartTime descending.</returns>
    Task<Result<IReadOnlyList<SyncSessionInfo>>> GetSyncSessionsAsync(int limit = 10);

    /// <summary>
    /// Delete old sync sessions to maintain database size.
    /// </summary>
    /// <param name="before">Delete sessions that ended before this timestamp.</param>
    /// <returns>Result indicating success or deletion failure.</returns>
    Task<Result> DeleteOldSessionsAsync(DateTime before);

    /// <summary>
    /// Check if the database has been initialized with required tables.
    /// </summary>
    /// <returns>Result with boolean indicating initialization status.</returns>
    Task<Result<bool>> IsInitializedAsync();
}
