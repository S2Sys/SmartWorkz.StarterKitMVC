namespace SmartWorkz.Mobile;

public interface IApiClient
{
    Task<Result<T>> GetAsync<T>(string endpoint, CancellationToken ct = default);
    Task<Result<T>> PostAsync<T>(string endpoint, object data, CancellationToken ct = default);
    Task<Result<T>> PutAsync<T>(string endpoint, object data, CancellationToken ct = default);
    Task<Result> DeleteAsync(string endpoint, CancellationToken ct = default);
    Task<Result<Stream>> GetStreamAsync(string endpoint, CancellationToken ct = default);
    Task<Result<T>> GetAsync<T>(string endpoint, int retryCount, TimeSpan timeout, CancellationToken ct = default);
}
