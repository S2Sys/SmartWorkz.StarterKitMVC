namespace SmartWorkz.Mobile;

public interface ILocalStorageService
{
    Task<Result> SaveAsync<T>(string key, T value, CancellationToken ct = default);
    Task<Result<T>> GetAsync<T>(string key, CancellationToken ct = default);
    Task<Result> DeleteAsync(string key, CancellationToken ct = default);
    Task<Result<IEnumerable<T>>> GetAllAsync<T>(CancellationToken ct = default);
    Task<Result<IEnumerable<T>>> GetAllByPrefixAsync<T>(string keyPrefix, CancellationToken ct = default);
    Task<Result> ClearAsync(CancellationToken ct = default);
}
