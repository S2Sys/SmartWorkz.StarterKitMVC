namespace SmartWorkz.Mobile;

using System.Text.Json;

public class OfflineService : IOfflineService
{
    private readonly ILocalStorageService _localStorageService;
    private readonly IConnectionChecker _connectionChecker;
    private readonly ISyncService _syncService;
    private readonly ILogger _logger;
    private const string CacheKeyPrefix = "offline::";

    private sealed class CacheEntry<T>
    {
        public T? Data { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public OfflineService(
        ILocalStorageService localStorageService,
        IConnectionChecker connectionChecker,
        ISyncService syncService,
        ILogger logger)
    {
        _localStorageService = Guard.NotNull(localStorageService, nameof(localStorageService));
        _connectionChecker = Guard.NotNull(connectionChecker, nameof(connectionChecker));
        _syncService = Guard.NotNull(syncService, nameof(syncService));
        _logger = Guard.NotNull(logger, nameof(logger));

        // Subscribe to connection changes for automatic sync
        _connectionChecker.OnConnectivityChanged()
            .Subscribe(isOnline =>
            {
                if (isOnline)
                {
                    _ = SyncPendingAsync(CancellationToken.None);
                }
            });
    }

    /// <summary>
    /// Caches data with optional expiration.
    /// </summary>
    public async Task<Result> CacheAsync<T>(string key, T data, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        Guard.NotEmpty(key, nameof(key));
        ct.ThrowIfCancellationRequested();

        try
        {
            var cacheKey = $"{CacheKeyPrefix}{key}";
            var entry = new CacheEntry<T>
            {
                Data = data,
                ExpiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null
            };

            var serialized = JsonSerializer.Serialize(entry);
            var result = await _localStorageService.SaveAsync(cacheKey, serialized, ct);

            if (result.Succeeded)
            {
                _logger.LogDebug($"Cached data for key '{key}'");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to cache data: {ex.Message}");
            return Result.Fail(new Error("CACHE.SAVE_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Retrieves data from cache, checking expiration.
    /// </summary>
    public async Task<Result<T>> GetFromCacheAsync<T>(string key, CancellationToken ct = default)
    {
        Guard.NotEmpty(key, nameof(key));
        ct.ThrowIfCancellationRequested();

        try
        {
            var cacheKey = $"{CacheKeyPrefix}{key}";
            var result = await _localStorageService.GetAsync<string>(cacheKey, ct);

            if (!result.Succeeded)
            {
                return Result.Fail<T>(new Error("CACHE.MISS", "Not in cache"));
            }

            var entryJson = result.Data;
            if (string.IsNullOrWhiteSpace(entryJson))
            {
                return Result.Fail<T>(new Error("CACHE.MISS", "Not in cache"));
            }

            try
            {
                var entry = JsonSerializer.Deserialize<CacheEntry<T>>(entryJson);

                if (entry == null)
                {
                    return Result.Fail<T>(new Error("CACHE.MISS", "Not in cache"));
                }

                // Check expiration
                if (entry.ExpiresAt.HasValue && entry.ExpiresAt < DateTime.UtcNow)
                {
                    // Delete expired entry
                    await _localStorageService.DeleteAsync(cacheKey, ct);
                    _logger.LogDebug($"Cache expired for key '{key}'");
                    return Result.Fail<T>(new Error("CACHE.MISS", "Cache expired"));
                }

                _logger.LogDebug($"Retrieved cached data for key '{key}'");
                return Result.Ok(entry.Data!);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogWarning($"Failed to deserialize cache entry: {jsonEx.Message}");
                return Result.Fail<T>(new Error("CACHE.DESERIALIZE_FAILED", jsonEx.Message));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to retrieve cached data: {ex.Message}");
            return Result.Fail<T>(new Error("CACHE.LOAD_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Syncs all pending operations.
    /// </summary>
    public async Task<Result> SyncPendingAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            _logger.LogDebug("Starting pending sync");
            var result = await _syncService.SyncAsync(ct);

            if (result.Succeeded)
            {
                var syncResult = result.Data;
                _logger.LogInformation($"Sync completed: {syncResult.SyncedCount} synced, {syncResult.FailedCount} failed");
            }

            return result.ToNonGeneric();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to sync pending operations: {ex.Message}");
            return Result.Fail(new Error("SYNC.FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Checks if the device is online.
    /// </summary>
    public async Task<bool> IsOnlineAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return await _connectionChecker.IsOnlineAsync(ct);
    }

    /// <summary>
    /// Returns an observable for connection state changes.
    /// </summary>
    public IObservable<bool> OnConnectionChanged()
    {
        return _connectionChecker.OnConnectivityChanged();
    }
}
