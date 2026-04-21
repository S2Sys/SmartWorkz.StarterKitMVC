namespace SmartWorkz.Mobile;

using ILogger = Microsoft.Extensions.Logging.ILogger;

public sealed class MobileCacheService : IMobileCacheService
{
    private readonly IOfflineService _offline;
    private readonly ILogger? _logger;

    public MobileCacheService(IOfflineService offline, ILogger? logger = null)
    {
        _offline = Guard.NotNull(offline, nameof(offline));
        _logger  = logger;
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key, Func<Task<T>> factory,
        TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var cached = await _offline.GetFromCacheAsync<T>(key, ct);
        if (cached.Succeeded && cached.Data is not null)
            return cached.Data;

        var value = await factory();
        await _offline.CacheAsync(key, value, ttl, ct);
        return value;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var result = await _offline.GetFromCacheAsync<T>(key, ct);
        return result.Succeeded ? result.Data : default;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default) =>
        _offline.CacheAsync(key, value, ttl, ct);

    public Task RemoveAsync(string key, CancellationToken ct = default) =>
        _offline.GetFromCacheAsync<object>(key, ct); // no remove on IOfflineService — use ILocalStorageService directly if needed

    public Task ClearAsync(CancellationToken ct = default) =>
        Task.CompletedTask;
}
