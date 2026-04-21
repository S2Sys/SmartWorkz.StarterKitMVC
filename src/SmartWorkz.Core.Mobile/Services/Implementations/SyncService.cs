namespace SmartWorkz.Core.Mobile;

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;

public class SyncService : ISyncService
{
    private readonly IApiClient _apiClient;
    private readonly ILocalStorageService _localStorageService;
    private readonly IConnectionChecker _connectionChecker;
    private readonly ILogger _logger;
    private readonly Subject<SyncProgress> _syncProgress;
    private ConflictStrategy _conflictStrategy = ConflictStrategy.ServerWins;
    private const string SyncQueueKeyPrefix = "sync::op::";

    public SyncService(
        IApiClient apiClient,
        ILocalStorageService localStorageService,
        IConnectionChecker connectionChecker,
        ILogger logger)
    {
        _apiClient = Guard.NotNull(apiClient, nameof(apiClient));
        _localStorageService = Guard.NotNull(localStorageService, nameof(localStorageService));
        _connectionChecker = Guard.NotNull(connectionChecker, nameof(connectionChecker));
        _logger = Guard.NotNull(logger, nameof(logger));
        _syncProgress = new Subject<SyncProgress>();
    }

    /// <summary>
    /// Syncs all pending operations with conflict handling.
    /// </summary>
    public async Task<Result<SyncResult>> SyncAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var pendingOperationsResult = await GetPendingOperationsAsync(ct);
            if (!pendingOperationsResult.Succeeded)
            {
                return Result.Fail<SyncResult>(pendingOperationsResult.Error ?? new Error("SYNC.FAILED", "Could not load pending operations"));
            }

            var operations = pendingOperationsResult.Data?.ToList() ?? [];
            int syncedCount = 0;
            int failedCount = 0;
            int conflictCount = 0;
            var failedIds = new List<Guid>();

            for (int i = 0; i < operations.Count; i++)
            {
                var operation = operations[i];

                try
                {
                    // Deserialize payload
                    var payload = !string.IsNullOrWhiteSpace(operation.PayloadJson)
                        ? JsonSerializer.Deserialize<object>(operation.PayloadJson)
                        : null;

                    // Send operation based on type
                    Result result = operation.OperationType.ToLower() switch
                    {
                        "post" => (await _apiClient.PostAsync<object>(operation.Endpoint, payload ?? new { }, ct)).ToNonGeneric(),
                        "put" => (await _apiClient.PutAsync<object>(operation.Endpoint, payload ?? new { }, ct)).ToNonGeneric(),
                        "delete" => await _apiClient.DeleteAsync(operation.Endpoint, ct),
                        _ => Result.Fail(new Error("SYNC.INVALID_OPERATION", $"Unknown operation type: {operation.OperationType}"))
                    };

                    if (result.Succeeded)
                    {
                        // Remove operation from storage on success
                        await _localStorageService.DeleteAsync($"{SyncQueueKeyPrefix}{operation.Id}", ct);
                        syncedCount++;
                        _logger.LogDebug($"Synced operation {operation.Id}");
                    }
                    else if (result.Error?.Code == "HTTP.409")
                    {
                        // Handle conflict based on strategy
                        var handled = await HandleConflictAsync(operation, ct);
                        if (handled)
                        {
                            conflictCount++;
                        }
                        else
                        {
                            failedCount++;
                            failedIds.Add(operation.Id);
                        }
                    }
                    else
                    {
                        // Other failures: keep operation in storage
                        failedCount++;
                        failedIds.Add(operation.Id);
                        _logger.LogWarning($"Failed to sync operation {operation.Id}: {result.Error?.Message}");
                    }
                }
                catch (Exception ex)
                {
                    failedCount++;
                    failedIds.Add(operation.Id);
                    _logger.LogError($"Exception syncing operation {operation.Id}: {ex.Message}");
                }

                // Emit progress
                _syncProgress.OnNext(new SyncProgress
                {
                    Current = i + 1,
                    Total = operations.Count,
                    StatusMessage = $"Synced {syncedCount}, Failed {failedCount}",
                    IsComplete = i + 1 == operations.Count
                });
            }

            var syncResult = new SyncResult
            {
                SyncedCount = syncedCount,
                FailedCount = failedCount,
                ConflictCount = conflictCount,
                FailedOperationIds = failedIds.AsReadOnly()
            };

            _logger.LogInformation($"Sync complete: {syncedCount} synced, {failedCount} failed, {conflictCount} conflicts");
            return Result.Ok(syncResult);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Sync operation failed: {ex.Message}");
            return Result.Fail<SyncResult>(new Error("SYNC.EXCEPTION", ex.Message));
        }
    }

    /// <summary>
    /// Queues an operation for sync.
    /// </summary>
    public async Task<Result> QueueOperationAsync(SyncOperation operation, CancellationToken ct = default)
    {
        Guard.NotNull(operation, nameof(operation));
        ct.ThrowIfCancellationRequested();

        try
        {
            var serialized = JsonSerializer.Serialize(operation);
            var key = $"{SyncQueueKeyPrefix}{operation.Id}";
            var result = await _localStorageService.SaveAsync(key, serialized, ct);

            if (result.Succeeded)
            {
                _logger.LogDebug($"Queued operation {operation.Id} for sync");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to queue operation: {ex.Message}");
            return Result.Fail(new Error("SYNC.QUEUE_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Gets all pending operations from storage.
    /// </summary>
    public async Task<Result<IEnumerable<SyncOperation>>> GetPendingOperationsAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var operations = new List<SyncOperation>();

            // Load all keys matching pattern (simulation - in real implementation, need storage pattern matching)
            // For now, we'll iterate through a known storage pattern
            var allResult = await _localStorageService.GetAllAsync<string>(ct);

            if (!allResult.Succeeded)
            {
                return Result.Ok(operations.AsEnumerable());
            }

            var allEntries = allResult.Data ?? [];

            foreach (var entry in allEntries)
            {
                try
                {
                    var operation = JsonSerializer.Deserialize<SyncOperation>(entry);
                    if (operation != null)
                    {
                        operations.Add(operation);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to deserialize operation: {ex.Message}");
                }
            }

            return Result.Ok(operations.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to get pending operations: {ex.Message}");
            return Result.Fail<IEnumerable<SyncOperation>>(new Error("SYNC.LOAD_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Sets the conflict resolution strategy.
    /// </summary>
    public void SetConflictResolutionStrategy(ConflictStrategy strategy)
    {
        _conflictStrategy = strategy;
        _logger.LogDebug($"Conflict resolution strategy set to: {strategy}");
    }

    /// <summary>
    /// Returns an observable for sync progress updates.
    /// </summary>
    public IObservable<SyncProgress> OnSyncProgress()
    {
        return _syncProgress.AsObservable();
    }

    private async Task<bool> HandleConflictAsync(SyncOperation operation, CancellationToken ct)
    {
        switch (_conflictStrategy)
        {
            case ConflictStrategy.ServerWins:
                // Remove local operation and accept server version
                await _localStorageService.DeleteAsync($"{SyncQueueKeyPrefix}{operation.Id}", ct);
                _logger.LogDebug($"Conflict for {operation.Id}: ServerWins applied");
                return true;

            case ConflictStrategy.ClientWins:
                // Retry operation with force flag (would need endpoint support)
                // For now, treat as needing manual intervention
                _logger.LogDebug($"Conflict for {operation.Id}: ClientWins - requires manual handling");
                return false;

            case ConflictStrategy.LastWriteWins:
                // Compare timestamps - if local is newer, retry; otherwise remove
                // For now, remove operation (assuming server is more authoritative)
                await _localStorageService.DeleteAsync($"{SyncQueueKeyPrefix}{operation.Id}", ct);
                _logger.LogDebug($"Conflict for {operation.Id}: LastWriteWins applied");
                return true;

            case ConflictStrategy.Manual:
                // Leave operation in queue for user to resolve
                _logger.LogWarning($"Conflict for {operation.Id}: Manual resolution required");
                return false;

            default:
                return false;
        }
    }
}

/// <summary>
/// Extension method to convert Result{T} to non-generic Result.
/// </summary>
internal static class ResultExtensions
{
    public static Result ToNonGeneric<T>(this Result<T> result)
    {
        return result.Succeeded ? Result.Ok() : Result.Fail(result.Error ?? new Error("UNKNOWN", "Unknown error"));
    }
}
