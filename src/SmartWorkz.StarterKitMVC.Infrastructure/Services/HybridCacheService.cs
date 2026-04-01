using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Abstractions;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Services;

/// <summary>
/// Hybrid two-level cache implementation.
/// L1 = IMemoryCache (in-process, 2 min TTL).
/// L2 = IDistributedCache (Redis primary, SQL Server fallback).
/// Registered as Singleton (IMemoryCache is Singleton-scoped).
/// </summary>
public sealed class HybridCacheService : ICacheService
{
    private readonly IMemoryCache _l1;
    private readonly IDistributedCache _l2;
    private readonly ILogger<HybridCacheService> _logger;

    private static readonly TimeSpan L1Ttl = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan DefaultL2Ttl = TimeSpan.FromMinutes(30);

    public HybridCacheService(
        IMemoryCache l1,
        IDistributedCache l2,
        ILogger<HybridCacheService> logger)
    {
        _l1 = l1;
        _l2 = l2;
        _logger = logger;
    }

    private static string BuildKey(string tenantId, string key)
        => $"{tenantId}:{key}";

    public async Task<T?> GetAsync<T>(string tenantId, string key, CancellationToken ct = default)
        where T : class
    {
        var fullKey = BuildKey(tenantId, key);

        // L1 hit — return immediately
        if (_l1.TryGetValue(fullKey, out T? cached))
            return cached;

        // L2 hit — hydrate L1 and return
        try
        {
            var bytes = await _l2.GetAsync(fullKey, ct);
            if (bytes is null)
                return null;

            var value = JsonSerializer.Deserialize<T>(bytes);
            if (value is null)
                return null;

            // Re-populate L1
            _l1.Set(fullKey, value, L1Ttl);
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "L2 cache read failed for key {Key}", fullKey);
            return null;
        }
    }

    public async Task SetAsync<T>(string tenantId, string key, T value,
        TimeSpan? absoluteExpiry = null, CancellationToken ct = default)
        where T : class
    {
        var fullKey = BuildKey(tenantId, key);
        var l2Expiry = absoluteExpiry ?? DefaultL2Ttl;

        // Always write L1
        _l1.Set(fullKey, value, TimeSpan.FromTicks(Math.Min(L1Ttl.Ticks, l2Expiry.Ticks)));

        // Write L2 — fire and forget, swallow exceptions (L1 still serves)
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            await _l2.SetAsync(fullKey, bytes, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = l2Expiry
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "L2 cache write failed for key {Key}", fullKey);
        }
    }

    public async Task RemoveAsync(string tenantId, string key, CancellationToken ct = default)
    {
        var fullKey = BuildKey(tenantId, key);
        _l1.Remove(fullKey);

        try
        {
            await _l2.RemoveAsync(fullKey, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "L2 cache remove failed for key {Key}", fullKey);
        }
    }

    public Task RemoveByPrefixAsync(string tenantId, string prefix, CancellationToken ct = default)
    {
        // IDistributedCache has no prefix scan API.
        // Redis SCAN + DEL requires IConnectionMultiplexer injection (deferred).
        // SQL Server DELETE WHERE Id LIKE prefix also requires branching on provider type (deferred).
        _logger.LogDebug("RemoveByPrefixAsync called for prefix {TenantId}:{Prefix} — prefix eviction not yet implemented", tenantId, prefix);
        return Task.CompletedTask;
    }
}
