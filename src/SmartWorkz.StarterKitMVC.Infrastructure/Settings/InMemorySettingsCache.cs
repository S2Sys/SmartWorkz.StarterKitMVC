using System.Collections.Concurrent;
using SmartWorkz.StarterKitMVC.Application.Settings;
using SmartWorkz.StarterKitMVC.Domain.Settings;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Settings;

public sealed class InMemorySettingsCache : ISettingsCache
{
    private readonly ConcurrentDictionary<string, SettingValue> _cache = new();

    public Task<SettingValue?> GetAsync(string cacheKey, CancellationToken ct = default)
        => Task.FromResult(_cache.TryGetValue(cacheKey, out var val) ? val : null);

    public Task SetAsync(string cacheKey, SettingValue value, CancellationToken ct = default)
    {
        _cache[cacheKey] = value;
        return Task.CompletedTask;
    }

    public Task InvalidateAsync(string cacheKeyPrefix, CancellationToken ct = default)
    {
        foreach (var key in _cache.Keys.Where(k => k.StartsWith(cacheKeyPrefix)))
            _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
