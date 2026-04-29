using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Abstractions;
using StackExchange.Redis;

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
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<HybridCacheService> _logger;

    private static readonly TimeSpan L1Ttl = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan DefaultL2Ttl = TimeSpan.FromMinutes(30);

    public HybridCacheService(
        IMemoryCache l1,
        IDistributedCache l2,
        ILogger<HybridCacheService> logger,
        IConnectionMultiplexer? redis = null)
    {
        _l1 = l1;
        _l2 = l2;
        _logger = logger;
        _redis = redis;
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

    public async Task RemoveByPrefixAsync(string tenantId, string prefix, CancellationToken ct = default)
    {
        var fullPrefix = BuildKey(tenantId, prefix);

        // Always remove from L1 (local iteration)
        // Note: IMemoryCache has no enumeration API, so we track nothing here.
        // In production, consider a secondary collection to track prefix sets.
        _logger.LogDebug("RemoveByPrefixAsync: L1 enumeration not available via IMemoryCache");

        // Remove from L2 via Redis if available
        if (_redis is not null)
        {
            try
            {
                var db = _redis.GetDatabase();
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var pattern = $"{fullPrefix}*";

                // SCAN all keys matching the prefix pattern
                var keys = new List<RedisKey>();
                var cursor = 0L;
                do
                {
                    var result = server.Execute("SCAN", cursor, "MATCH", pattern);
                    if (result is null)
                        break;

                    // Parse SCAN result: [cursor, [keys...]]
                    // RedisResult[] contains [cursor (as string), keys (as array)]
                    var scanResult = (RedisResult[])result!;
                    cursor = long.Parse((string?)scanResult[0] ?? "0");

                    if (scanResult[1].IsNull == false)
                    {
                        var scannedKeys = (RedisKey[])scanResult[1];
                        foreach (var key in scannedKeys)
                        {
                            keys.Add(key);
                        }
                    }
                } while (cursor != 0);

                // Delete all matched keys in a single batch
                if (keys.Count > 0)
                {
                    await db.KeyDeleteAsync(keys.ToArray());
                    _logger.LogDebug("RemoveByPrefixAsync: Deleted {KeyCount} keys for prefix {TenantId}:{Prefix}", keys.Count, tenantId, prefix);
                }
                else
                {
                    _logger.LogDebug("RemoveByPrefixAsync: No keys found for prefix {TenantId}:{Prefix}", tenantId, prefix);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RemoveByPrefixAsync: L2 prefix scan/delete failed for {TenantId}:{Prefix}", tenantId, prefix);
            }
        }
        else
        {
            _logger.LogDebug("RemoveByPrefixAsync: Redis not configured, prefix eviction skipped for {TenantId}:{Prefix}", tenantId, prefix);
        }
    }
}
