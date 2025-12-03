using Microsoft.Extensions.Hosting;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Storage;

public sealed class JsonFileLocalStorage : ILocalStorage
{
    private readonly string _rootPath;

    public JsonFileLocalStorage(IHostEnvironment env)
    {
        _rootPath = Path.Combine(env.ContentRootPath, "_localdata");
        Directory.CreateDirectory(_rootPath);
    }

    public async Task SaveAsync(string key, string content, CancellationToken cancellationToken = default)
    {
        var path = GetPath(key);
        await File.WriteAllTextAsync(path, content, cancellationToken);
    }

    public async Task<string?> LoadAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = GetPath(key);
        return File.Exists(path) ? await File.ReadAllTextAsync(path, cancellationToken) : null;
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = GetPath(key);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private string GetPath(string key) => Path.Combine(_rootPath, key + ".json");
}
