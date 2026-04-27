using SmartWorkz.Shared;
using Xunit;

namespace SmartWorkz.Core.Tests.Caching;

/// <summary>
/// Tests for TTL (Time To Live) expiration enforcement in cache services.
/// </summary>
public class CacheTTLExpirationTests : IDisposable
{
    private readonly MemoryCacheService _cache;

    public CacheTTLExpirationTests()
    {
        _cache = new MemoryCacheService();
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    [Fact]
    public async Task SetAsync_WithShortTTL_ExpiresAfterTime()
    {
        // Arrange
        string key = "shortTTL";
        string value = "expiring-value";
        int ttlSeconds = 1;

        // Act
        await _cache.SetAsync(key, value, ttlSeconds: ttlSeconds);

        // Verify it exists immediately
        var immediateResult = await _cache.GetAsync<string>(key);
        Assert.True(immediateResult.Succeeded);

        // Wait for expiration
        await Task.Delay(TimeSpan.FromSeconds(ttlSeconds + 0.5));

        // Try to retrieve after expiration
        var expiredResult = await _cache.GetAsync<string>(key);

        // Assert
        Assert.False(expiredResult.Succeeded);
        Assert.Null(expiredResult.Data);
    }

    [Fact]
    public async Task SetAsync_WithLongTTL_DoesNotExpireQuickly()
    {
        // Arrange
        string key = "longTTL";
        string value = "persistent-value";
        int ttlMinutes = 60;

        // Act
        await _cache.SetAsync(key, value, ttlMinutes: ttlMinutes);
        await Task.Delay(TimeSpan.FromMilliseconds(100));

        var result = await _cache.GetAsync<string>(key);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(value, result.Data);
    }

    [Fact]
    public async Task SetAsync_WithZeroTTL_ExpireImmediately()
    {
        // Arrange
        string key = "zeroTTL";
        string value = "immediate-expiry";

        // Act
        await _cache.SetAsync(key, value, ttlMinutes: 0);
        await Task.Delay(TimeSpan.FromMilliseconds(50));

        var result = await _cache.GetAsync<string>(key);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task SetAsync_WithMillisecondsPrecision_EnforcesTTLAccurately()
    {
        // Arrange
        string key = "precisionTTL";
        string value = "precise-value";
        int ttlSeconds = 2;

        // Act
        await _cache.SetAsync(key, value, ttlSeconds: ttlSeconds);

        // Check at 1 second - should exist
        await Task.Delay(TimeSpan.FromSeconds(1));
        var result1 = await _cache.GetAsync<string>(key);

        // Check at 3 seconds - should be expired
        await Task.Delay(TimeSpan.FromSeconds(2));
        var result2 = await _cache.GetAsync<string>(key);

        // Assert
        Assert.True(result1.Succeeded);
        Assert.False(result2.Succeeded);
    }

    [Fact]
    public async Task SetAsync_MultipleKeys_EachExpireIndependently()
    {
        // Arrange
        string key1 = "expireFirst";
        string key2 = "expireLater";
        string value = "value";

        // Act
        await _cache.SetAsync(key1, value, ttlSeconds: 1);
        await _cache.SetAsync(key2, value, ttlSeconds: 3);

        // Wait 1.5 seconds
        await Task.Delay(TimeSpan.FromSeconds(1.5));

        var result1 = await _cache.GetAsync<string>(key1);
        var result2 = await _cache.GetAsync<string>(key2);

        // Assert - key1 should be expired, key2 should exist
        Assert.False(result1.Succeeded);
        Assert.True(result2.Succeeded);
    }

    [Fact]
    public async Task SetAsync_OverwritingKey_ResetsExpiration()
    {
        // Arrange
        string key = "resetExpiry";
        string value1 = "first";
        string value2 = "second";

        // Act
        await _cache.SetAsync(key, value1, ttlSeconds: 2);
        await Task.Delay(TimeSpan.FromSeconds(1));

        // Overwrite with new TTL
        await _cache.SetAsync(key, value2, ttlSeconds: 3);
        await Task.Delay(TimeSpan.FromSeconds(1.5));

        var result = await _cache.GetAsync<string>(key);

        // Assert - key should still exist because it was reset
        Assert.True(result.Succeeded);
        Assert.Equal(value2, result.Data);
    }

    [Fact]
    public async Task SetAsync_NegativeTTL_TreatAsExpired()
    {
        // Arrange
        string key = "negativeTTL";
        string value = "should-expire";

        // Act
        try
        {
            await _cache.SetAsync(key, value, ttlSeconds: -1);
            await Task.Delay(TimeSpan.FromMilliseconds(50));
            var result = await _cache.GetAsync<string>(key);

            // Assert - should either be expired or throw
            Assert.False(result.Succeeded);
        }
        catch (ArgumentException)
        {
            // Also acceptable - negative TTL should be rejected
        }
    }

    [Fact]
    public async Task SetAsync_WithMinutesPrecision_ExpiresProperly()
    {
        // Arrange
        string key = "minutesTTL";
        string value = "minutes-value";
        int ttlMinutes = 1;

        // Act
        await _cache.SetAsync(key, value, ttlMinutes: ttlMinutes);

        // Immediately check - should exist
        var result1 = await _cache.GetAsync<string>(key);

        // Act - wait 61 seconds
        await Task.Delay(TimeSpan.FromSeconds(61));
        var result2 = await _cache.GetAsync<string>(key);

        // Assert
        Assert.True(result1.Succeeded);
        Assert.False(result2.Succeeded);
    }

    [Fact]
    public async Task SetAsync_LargeDataWithTTL_ExpiresCorrectly()
    {
        // Arrange
        string key = "largeData";
        var largeObject = new
        {
            Id = "large-123",
            Data = string.Join(",", Enumerable.Range(0, 1000).Select(i => $"item-{i}"))
        };

        // Act
        await _cache.SetAsync(key, largeObject, ttlSeconds: 1);

        var result1 = await _cache.GetAsync<dynamic>(key);
        await Task.Delay(TimeSpan.FromSeconds(1.5));
        var result2 = await _cache.GetAsync<dynamic>(key);

        // Assert
        Assert.True(result1.Succeeded);
        Assert.False(result2.Succeeded);
    }

    [Fact]
    public async Task SetAsync_DefaultTTL_WithoutSpecification()
    {
        // Arrange
        string key = "defaultTTL";
        string value = "default-value";

        // Act
        await _cache.SetAsync(key, value); // No TTL specified
        await Task.Delay(TimeSpan.FromSeconds(1));

        var result = await _cache.GetAsync<string>(key);

        // Assert - should still exist (default TTL is infinite or very long)
        Assert.True(result.Succeeded);
        Assert.Equal(value, result.Data);
    }

    [Fact]
    public async Task SetAsync_EdgeCase_TTLBoundary()
    {
        // Arrange
        string key = "boundaryTTL";
        string value = "boundary-value";
        int ttlSeconds = 1;

        // Act
        await _cache.SetAsync(key, value, ttlSeconds: ttlSeconds);

        // Retrieve at exact boundary (just before expiration)
        await Task.Delay(TimeSpan.FromSeconds(0.9));
        var resultBefore = await _cache.GetAsync<string>(key);

        // Retrieve just after boundary
        await Task.Delay(TimeSpan.FromSeconds(0.2));
        var resultAfter = await _cache.GetAsync<string>(key);

        // Assert
        Assert.True(resultBefore.Succeeded);
        Assert.False(resultAfter.Succeeded);
    }
}
