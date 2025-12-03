namespace SmartWorkz.StarterKitMVC.Infrastructure.Storage;

public interface ILocalStorage
{
    Task SaveAsync(string key, string content, CancellationToken cancellationToken = default);
    Task<string?> LoadAsync(string key, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
