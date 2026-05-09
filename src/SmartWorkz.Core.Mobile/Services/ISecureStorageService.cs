namespace SmartWorkz.Mobile;

public interface ISecureStorageService
{
    Task<Result> SetAsync(string key, string value, CancellationToken ct = default);
    Task<Result<string>> GetAsync(string key, CancellationToken ct = default);
    Task<Result> DeleteAsync(string key, CancellationToken ct = default);
    Task<Result> ClearAsync(CancellationToken ct = default);
}
