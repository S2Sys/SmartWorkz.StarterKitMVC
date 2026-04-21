namespace SmartWorkz.Mobile;

public interface IOfflineService
{
    Task<Result> CacheAsync<T>(string key, T data, TimeSpan? expiration = null, CancellationToken ct = default);
    Task<Result<T>> GetFromCacheAsync<T>(string key, CancellationToken ct = default);
    Task<Result> SyncPendingAsync(CancellationToken ct = default);
    Task<bool> IsOnlineAsync(CancellationToken ct = default);
    IObservable<bool> OnConnectionChanged();
}
