namespace SmartWorkz.Mobile.Persistence;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Shared;
using ILogger = Microsoft.Extensions.Logging.ILogger;

/// <summary>
/// JSON file-based persistent storage for sync state (cross-platform compatible).
/// Enables sync progress to survive app restarts without platform-specific SQLite dependencies.
/// Uses JSON serialization for simple, portable persistence.
/// </summary>
public class SqliteSyncStateStore : ISyncStateStore, IDisposable
{
    private readonly string _databasePath;
    private readonly string _sessionsFile;
    private readonly string _changesDir;
    private readonly string _stateFile;
    private readonly ILogger? _logger;
    private bool _disposed;
    private bool _initialized;
    private readonly object _initLock = new();
    private readonly object _fileLock = new();

    private class SyncStateDocument
    {
        public string EntityType { get; set; } = "";
        public string LastSyncTime { get; set; } = "";
        public int PendingChangesCount { get; set; }
        public int ResolvedConflictsCount { get; set; }
        public string? LastErrorMessage { get; set; }
    }

    public string DatabasePath => _databasePath;

    public SqliteSyncStateStore(string databasePath, ILogger<SqliteSyncStateStore>? logger = null)
    {
        Guard.NotNull(databasePath, nameof(databasePath));

        _databasePath = databasePath;
        _logger = logger;

        // Create storage directories
        var dir = Path.GetDirectoryName(databasePath) ?? Path.GetTempPath();
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var storeDir = Path.Combine(dir, "sync_state_store");
        if (!Directory.Exists(storeDir))
        {
            Directory.CreateDirectory(storeDir);
        }

        _sessionsFile = Path.Combine(storeDir, "sessions.json");
        _changesDir = Path.Combine(storeDir, "changes");
        _stateFile = Path.Combine(storeDir, "state.json");
    }

    public async Task<Result> InitializeAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_initLock)
        {
            if (_initialized)
            {
                return Result.Ok();
            }
        }

        return await Task.Run(() =>
        {
            try
            {
                // Ensure directories exist
                var dir = Path.GetDirectoryName(_sessionsFile);
                if (dir != null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (!Directory.Exists(_changesDir))
                {
                    Directory.CreateDirectory(_changesDir);
                }

                // Create empty files if they don't exist
                if (!File.Exists(_sessionsFile))
                {
                    File.WriteAllText(_sessionsFile, "[]");
                }

                if (!File.Exists(_stateFile))
                {
                    File.WriteAllText(_stateFile, "{}");
                }

                lock (_initLock)
                {
                    _initialized = true;
                }

                _logger?.LogInformation("Sync state store initialized at {DbPath}", _databasePath);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize sync state store");
                return Result.Fail("SyncStateStore.InitFailed", "Store initialization failed");
            }
        });
    }

    public async Task<Result> SavePendingChangesAsync(string entityType, IReadOnlyList<SyncChange> changes)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.NotNull(entityType, nameof(entityType));
        Guard.NotNull(changes, nameof(changes));
        EnsureInitialized();

        return await Task.Run(() =>
        {
            try
            {
                lock (_fileLock)
                {
                    var changesFile = Path.Combine(_changesDir, $"{entityType}.json");
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var json = JsonSerializer.Serialize(changes.ToList(), options);
                    File.WriteAllText(changesFile, json);

                    // Update state file
                    UpdateSyncState(entityType, pending: changes.Count);
                }

                _logger?.LogDebug(
                    "Saved {ChangeCount} pending changes for {EntityType}",
                    changes.Count,
                    entityType);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save pending changes for {EntityType}", entityType);
                return Result.Fail("SyncStateStore.SavePendingFailed", "Failed to persist pending changes");
            }
        });
    }

    public async Task<Result<IReadOnlyList<SyncChange>>> LoadPendingChangesAsync(string entityType)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.NotNull(entityType, nameof(entityType));
        EnsureInitialized();

        return await Task.Run(() =>
        {
            try
            {
                lock (_fileLock)
                {
                    var changesFile = Path.Combine(_changesDir, $"{entityType}.json");
                    if (!File.Exists(changesFile))
                    {
                        return Result.Ok<IReadOnlyList<SyncChange>>(new List<SyncChange>().AsReadOnly());
                    }

                    var json = File.ReadAllText(changesFile);
                    var changes = JsonSerializer.Deserialize<List<SyncChange>>(json) ?? [];

                    _logger?.LogDebug("Loaded {ChangeCount} pending changes for {EntityType}", changes.Count, entityType);
                    return Result.Ok<IReadOnlyList<SyncChange>>(changes.AsReadOnly());
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load pending changes for {EntityType}", entityType);
                return Result.Fail<IReadOnlyList<SyncChange>>(
                    "SyncStateStore.LoadPendingFailed",
                    "Failed to load pending changes");
            }
        });
    }

    public async Task<Result> ClearPendingChangesAsync(string entityType)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.NotNull(entityType, nameof(entityType));
        EnsureInitialized();

        return await Task.Run(() =>
        {
            try
            {
                lock (_fileLock)
                {
                    var changesFile = Path.Combine(_changesDir, $"{entityType}.json");
                    if (File.Exists(changesFile))
                    {
                        File.Delete(changesFile);
                    }

                    // Update state file
                    UpdateSyncState(entityType, pending: 0);
                }

                _logger?.LogDebug("Cleared pending changes for {EntityType}", entityType);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to clear pending changes for {EntityType}", entityType);
                return Result.Fail("SyncStateStore.ClearPendingFailed", "Failed to clear pending changes");
            }
        });
    }

    public async Task<Result> RecordSyncSessionAsync(SyncSessionInfo session)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.NotNull(session, nameof(session));
        EnsureInitialized();

        return await Task.Run(() =>
        {
            try
            {
                lock (_fileLock)
                {
                    // Load existing sessions
                    var sessions = new List<SyncSessionInfo>();
                    if (File.Exists(_sessionsFile))
                    {
                        var json = File.ReadAllText(_sessionsFile);
                        sessions = JsonSerializer.Deserialize<List<SyncSessionInfo>>(json) ?? [];
                    }

                    // Add new session
                    sessions.Add(session);

                    // Save sessions
                    var sessionsJson = JsonSerializer.Serialize(sessions, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_sessionsFile, sessionsJson);

                    // Update state file
                    UpdateSyncState(
                        session.EntityType,
                        lastSyncTime: session.EndTime ?? DateTime.UtcNow,
                        errorMessage: session.IsSuccessful ? null : session.ErrorMessage,
                        conflictsResolved: session.ConflictsResolved);
                }

                _logger?.LogInformation(
                    "Recorded sync session {SessionId} for {EntityType}: Success={Success}",
                    session.SessionId,
                    session.EntityType,
                    session.IsSuccessful);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to record sync session {SessionId}", session.SessionId);
                return Result.Fail("SyncStateStore.RecordSessionFailed", "Failed to record sync session");
            }
        });
    }

    public async Task<Result<PersistentSyncState>> GetSyncStateAsync(string entityType)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Guard.NotNull(entityType, nameof(entityType));
        EnsureInitialized();

        return await Task.Run(() =>
        {
            try
            {
                lock (_fileLock)
                {
                    var states = LoadAllStates();

                    if (states.TryGetValue(entityType, out var state))
                    {
                        return Result.Ok(state);
                    }

                    // Return default state if not found
                    var defaultState = new PersistentSyncState(
                        EntityType: entityType,
                        LastSyncTime: DateTime.UtcNow,
                        PendingChangesCount: 0,
                        ResolvedConflictsCount: 0,
                        LastErrorMessage: null);

                    return Result.Ok(defaultState);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get sync state for {EntityType}", entityType);
                return Result.Fail<PersistentSyncState>(
                    "SyncStateStore.GetStateFailed",
                    "Failed to retrieve sync state");
            }
        });
    }

    public async Task<Result<IReadOnlyList<SyncSessionInfo>>> GetSyncSessionsAsync(int limit = 10)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureInitialized();

        return await Task.Run(() =>
        {
            try
            {
                lock (_fileLock)
                {
                    var sessions = new List<SyncSessionInfo>();
                    if (File.Exists(_sessionsFile))
                    {
                        var json = File.ReadAllText(_sessionsFile);
                        sessions = JsonSerializer.Deserialize<List<SyncSessionInfo>>(json) ?? [];
                    }

                    var result = sessions
                        .OrderByDescending(s => s.StartTime)
                        .Take(Math.Max(1, limit))
                        .ToList();

                    return Result.Ok<IReadOnlyList<SyncSessionInfo>>(result.AsReadOnly());
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get sync sessions");
                return Result.Fail<IReadOnlyList<SyncSessionInfo>>(
                    "SyncStateStore.GetSessionsFailed",
                    "Failed to retrieve sync sessions");
            }
        });
    }

    public async Task<Result> DeleteOldSessionsAsync(DateTime before)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureInitialized();

        return await Task.Run(() =>
        {
            try
            {
                lock (_fileLock)
                {
                    if (!File.Exists(_sessionsFile))
                    {
                        return Result.Ok();
                    }

                    var json = File.ReadAllText(_sessionsFile);
                    var sessions = JsonSerializer.Deserialize<List<SyncSessionInfo>>(json) ?? [];

                    // Keep sessions that have no EndTime OR EndTime is after the cutoff
                    var filtered = sessions
                        .Where(s => s.EndTime == null || s.EndTime > before)
                        .ToList();

                    var deletedCount = sessions.Count - filtered.Count;

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var updatedJson = JsonSerializer.Serialize(filtered, options);
                    File.WriteAllText(_sessionsFile, updatedJson);

                    _logger?.LogInformation("Deleted {DeletedCount} old sync sessions before {Before}", deletedCount, before);
                    return Result.Ok();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to delete old sessions");
                return Result.Fail("SyncStateStore.DeleteOldFailed", "Failed to delete old sessions");
            }
        });
    }

    public async Task<Result<bool>> IsInitializedAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return await Task.Run(() =>
        {
            try
            {
                lock (_initLock)
                {
                    return Result.Ok(_initialized);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to check initialization status");
                return Result.Fail<bool>("SyncStateStore.CheckInitFailed", "Failed to check initialization");
            }
        });
    }

    private void UpdateSyncState(
        string entityType,
        int? pending = null,
        DateTime? lastSyncTime = null,
        string? errorMessage = null,
        int? conflictsResolved = null)
    {
        try
        {
            var states = LoadAllStates();

            if (states.TryGetValue(entityType, out var existing))
            {
                // Update existing
                var updated = new PersistentSyncState(
                    EntityType: entityType,
                    LastSyncTime: lastSyncTime ?? existing.LastSyncTime,
                    PendingChangesCount: pending ?? existing.PendingChangesCount,
                    ResolvedConflictsCount: (conflictsResolved ?? 0) + existing.ResolvedConflictsCount,
                    LastErrorMessage: errorMessage ?? existing.LastErrorMessage);

                states[entityType] = updated;
            }
            else
            {
                // Create new
                states[entityType] = new PersistentSyncState(
                    EntityType: entityType,
                    LastSyncTime: lastSyncTime ?? DateTime.UtcNow,
                    PendingChangesCount: pending ?? 0,
                    ResolvedConflictsCount: conflictsResolved ?? 0,
                    LastErrorMessage: errorMessage);
            }

            SaveAllStates(states);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error updating sync state for {EntityType}", entityType);
            throw;
        }
    }

    private Dictionary<string, PersistentSyncState> LoadAllStates()
    {
        try
        {
            if (!File.Exists(_stateFile))
            {
                return [];
            }

            var json = File.ReadAllText(_stateFile);
            if (string.IsNullOrWhiteSpace(json) || json == "{}")
            {
                return [];
            }

            var items = JsonSerializer.Deserialize<List<PersistentSyncState>>(json) ?? [];
            return items.ToDictionary(s => s.EntityType);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load sync states");
            return [];
        }
    }

    private void SaveAllStates(Dictionary<string, PersistentSyncState> states)
    {
        try
        {
            var items = states.Values.ToList();
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(items, options);
            File.WriteAllText(_stateFile, json);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save sync states");
            throw;
        }
    }

    private void EnsureInitialized()
    {
        lock (_initLock)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("SqliteSyncStateStore must be initialized before use. Call InitializeAsync() first.");
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            // No resources to clean up - using file-based storage
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error disposing store");
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
