using System.Collections.Concurrent;
using SmartWorkz.StarterKitMVC.Application.LoV;
using SmartWorkz.StarterKitMVC.Domain.LoV;

namespace SmartWorkz.StarterKitMVC.Infrastructure.LoV;

public sealed class InMemoryLovCache : ILovCache
{
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public Task<IReadOnlyCollection<Category>?> GetCategoriesAsync(string cacheKey, CancellationToken ct = default)
        => Task.FromResult(_cache.TryGetValue(cacheKey, out var val) ? val as IReadOnlyCollection<Category> : null);

    public Task SetCategoriesAsync(string cacheKey, IReadOnlyCollection<Category> categories, CancellationToken ct = default)
    {
        _cache[cacheKey] = categories;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<LovItem>?> GetItemsAsync(string cacheKey, CancellationToken ct = default)
        => Task.FromResult(_cache.TryGetValue(cacheKey, out var val) ? val as IReadOnlyCollection<LovItem> : null);

    public Task SetItemsAsync(string cacheKey, IReadOnlyCollection<LovItem> items, CancellationToken ct = default)
    {
        _cache[cacheKey] = items;
        return Task.CompletedTask;
    }

    public Task InvalidateAsync(string cacheKeyPrefix, CancellationToken ct = default)
    {
        foreach (var key in _cache.Keys.Where(k => k.StartsWith(cacheKeyPrefix)))
            _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
