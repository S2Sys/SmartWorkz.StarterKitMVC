using SmartWorkz.Core.Shared.Caching;
using SmartWorkz.Core.Shared.Results;

namespace SmartWorkz.Core.Tests.Caching;

public class MemoryCacheStoreTests
{
    private readonly MemoryCacheStore _cacheStore;

    public MemoryCacheStoreTests()
    {
        _cacheStore = new MemoryCacheStore();
    }

    #region SetAsync Tests

    [Fact]
    public async Task SetAsync_WithValidKeyAndValue_ShouldSucceed()
    {
        // Arrange
        var key = "test_key";
        var value = "test_value";

        // Act
        var result = await _cacheStore.SetAsync(key, value);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task SetAsync_WithNullKey_ShouldThrow()
    {
        // Arrange
        var value = "test_value";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _cacheStore.SetAsync(null!, value));
    }

    [Fact]
    public async Task SetAsync_WithEmptyKey_ShouldThrow()
    {
        // Arrange
        var value = "test_value";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _cacheStore.SetAsync("", value));
    }

    [Fact]
    public async Task SetAsync_WithTtl_ShouldStoreWithExpiry()
    {
        // Arrange
        var key = "ttl_key";
        var value = "ttl_value";
        var ttlMinutes = 5;

        // Act
        var result = await _cacheStore.SetAsync(key, value, ttlMinutes);

        // Assert
        Assert.True(result.Succeeded);
        var exists = await _cacheStore.ExistsAsync(key);
        Assert.True(exists.Data);
    }

    [Fact]
    public async Task SetAsync_WithCacheOptions_ShouldStoreWithOptions()
    {
        // Arrange
        var key = "options_key";
        var value = "options_value";
        var options = new CacheOptions(10, CacheStrategy.Absolute, false);

        // Act
        var result = await _cacheStore.SetAsync(key, value, options);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task SetAsync_OverwritingExistingKey_ShouldUpdateValue()
    {
        // Arrange
        var key = "overwrite_key";
        var value1 = "value1";
        var value2 = "value2";

        // Act
        await _cacheStore.SetAsync(key, value1);
        var resultBefore = await _cacheStore.GetAsync<string>(key);
        await _cacheStore.SetAsync(key, value2);
        var resultAfter = await _cacheStore.GetAsync<string>(key);

        // Assert
        Assert.Equal(value1, resultBefore.Data);
        Assert.Equal(value2, resultAfter.Data);
    }

    #endregion

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_WithExistingKey_ShouldReturnValue()
    {
        // Arrange
        var key = "get_key";
        var value = "get_value";
        await _cacheStore.SetAsync(key, value);

        // Act
        var result = await _cacheStore.GetAsync<string>(key);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(value, result.Data);
    }

    [Fact]
    public async Task GetAsync_WithNonExistentKey_ShouldReturnNull()
    {
        // Arrange
        var key = "nonexistent_key";

        // Act
        var result = await _cacheStore.GetAsync<string>(key);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetAsync_WithNullKey_ShouldThrow()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _cacheStore.GetAsync<string>(null!));
    }

    [Fact]
    public async Task GetAsync_WithDifferentTypes_ShouldReturnTypedValue()
    {
        // Arrange
        var stringKey = "string_key";
        var stringValue = "string_value";
        var intKey = "int_key";
        var intValue = 42;
        var objectKey = "object_key";
        var objectValue = new { Name = "Test", Value = 123 };

        // Act
        await _cacheStore.SetAsync(stringKey, stringValue);
        await _cacheStore.SetAsync(intKey, intValue);
        await _cacheStore.SetAsync(objectKey, objectValue);

        var stringResult = await _cacheStore.GetAsync<string>(stringKey);
        var intResult = await _cacheStore.GetAsync<int>(intKey);
        var objectResult = await _cacheStore.GetAsync<dynamic>(objectKey);

        // Assert
        Assert.Equal(stringValue, stringResult.Data);
        Assert.Equal(intValue, intResult.Data);
        Assert.NotNull(objectResult.Data);
    }

    #endregion

    #region RemoveAsync Tests

    [Fact]
    public async Task RemoveAsync_WithExistingKey_ShouldRemoveValue()
    {
        // Arrange
        var key = "remove_key";
        var value = "remove_value";
        await _cacheStore.SetAsync(key, value);

        // Act
        var removeResult = await _cacheStore.RemoveAsync(key);
        var getResult = await _cacheStore.GetAsync<string>(key);

        // Assert
        Assert.True(removeResult.Data);
        Assert.Null(getResult.Data);
    }

    [Fact]
    public async Task RemoveAsync_WithNonExistentKey_ShouldReturnFalse()
    {
        // Arrange
        var key = "nonexistent_remove_key";

        // Act
        var result = await _cacheStore.RemoveAsync(key);

        // Assert
        Assert.False(result.Data);
    }

    [Fact]
    public async Task RemoveAsync_WithNullKey_ShouldThrow()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _cacheStore.RemoveAsync(null!));
    }

    #endregion

    #region RemoveByPrefixAsync Tests

    [Fact]
    public async Task RemoveByPrefixAsync_WithMatchingKeys_ShouldRemoveAll()
    {
        // Arrange
        var prefix = "cache:user:";
        var keys = new[] { "cache:user:1", "cache:user:2", "cache:user:3", "cache:other:1" };
        foreach (var key in keys)
        {
            await _cacheStore.SetAsync(key, $"value_for_{key}");
        }

        // Act
        var result = await _cacheStore.RemoveByPrefixAsync(prefix);

        // Assert
        Assert.Equal(3, result.Data);
        var exists1 = await _cacheStore.ExistsAsync("cache:user:1");
        var exists4 = await _cacheStore.ExistsAsync("cache:other:1");
        Assert.False(exists1.Data);
        Assert.True(exists4.Data);
    }

    [Fact]
    public async Task RemoveByPrefixAsync_WithNoMatchingKeys_ShouldReturnZero()
    {
        // Arrange
        var prefix = "nonexistent:";

        // Act
        var result = await _cacheStore.RemoveByPrefixAsync(prefix);

        // Assert
        Assert.Equal(0, result.Data);
    }

    [Fact]
    public async Task RemoveByPrefixAsync_WithNullPrefix_ShouldThrow()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _cacheStore.RemoveByPrefixAsync(null!));
    }

    [Fact]
    public async Task RemoveByPrefixAsync_CaseSensitivity_ShouldRemoveWithCaseInsensitivity()
    {
        // Arrange
        var keys = new[] { "Cache:Test:1", "CACHE:TEST:2", "cache:test:3" };
        foreach (var key in keys)
        {
            await _cacheStore.SetAsync(key, $"value_for_{key}");
        }

        // Act
        var result = await _cacheStore.RemoveByPrefixAsync("cache:test:");

        // Assert
        Assert.Equal(3, result.Data);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WithExistingKey_ShouldReturnTrue()
    {
        // Arrange
        var key = "exists_key";
        var value = "exists_value";
        await _cacheStore.SetAsync(key, value);

        // Act
        var result = await _cacheStore.ExistsAsync(key);

        // Assert
        Assert.True(result.Data);
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentKey_ShouldReturnFalse()
    {
        // Arrange
        var key = "nonexistent_exists_key";

        // Act
        var result = await _cacheStore.ExistsAsync(key);

        // Assert
        Assert.False(result.Data);
    }

    [Fact]
    public async Task ExistsAsync_WithNullKey_ShouldThrow()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _cacheStore.ExistsAsync(null!));
    }

    #endregion

    #region ClearAsync Tests

    [Fact]
    public async Task ClearAsync_WithMultipleEntries_ShouldRemoveAll()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3" };
        foreach (var key in keys)
        {
            await _cacheStore.SetAsync(key, $"value_for_{key}");
        }

        // Act
        var clearResult = await _cacheStore.ClearAsync();
        var exists1 = await _cacheStore.ExistsAsync("key1");

        // Assert
        Assert.True(clearResult.Data);
        Assert.False(exists1.Data);
    }

    [Fact]
    public async Task ClearAsync_WithEmptyCache_ShouldSucceed()
    {
        // Act
        var result = await _cacheStore.ClearAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
    }

    #endregion

    #region TTL and Expiration Tests

    [Fact]
    public async Task GetAsync_WithExpiredEntry_ShouldReturnNull()
    {
        // Arrange
        var key = "expired_key";
        var value = "expired_value";
        var options = new CacheOptions(-1); // Already expired (1 minute in the past)
        await _cacheStore.SetAsync(key, value, options);

        // Act
        var result = await _cacheStore.GetAsync<string>(key);

        // Assert
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ExistsAsync_WithExpiredEntry_ShouldReturnFalse()
    {
        // Arrange
        var key = "expired_exists_key";
        var value = "expired_value";
        var options = new CacheOptions(-1); // Already expired
        await _cacheStore.SetAsync(key, value, options);

        // Act
        var result = await _cacheStore.ExistsAsync(key);

        // Assert
        Assert.False(result.Data);
    }

    [Fact]
    public async Task SetAsync_WithoutTtl_ShouldStoreWithoutExpiry()
    {
        // Arrange
        var key = "no_ttl_key";
        var value = "no_ttl_value";
        var options = new CacheOptions(null); // No TTL

        // Act
        var result = await _cacheStore.SetAsync(key, value, options);
        var exists = await _cacheStore.ExistsAsync(key);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(exists.Data);
    }

    #endregion

    #region Sliding Expiration Tests

    [Fact]
    public async Task GetAsync_WithSlidingExpiration_ShouldRenewTtl()
    {
        // Arrange
        var key = "sliding_key";
        var value = "sliding_value";
        var options = new CacheOptions(10, CacheStrategy.Sliding, slidingExpiration: true);
        await _cacheStore.SetAsync(key, value, options);

        // Act
        var result1 = await _cacheStore.GetAsync<string>(key);
        await Task.Delay(100); // Small delay
        var result2 = await _cacheStore.GetAsync<string>(key);

        // Assert
        Assert.Equal(value, result1.Data);
        Assert.Equal(value, result2.Data);
    }

    [Fact]
    public async Task SetAsync_WithAbsoluteExpiration_ShouldNotRenewTtl()
    {
        // Arrange
        var key = "absolute_key";
        var value = "absolute_value";
        var options = new CacheOptions(10, CacheStrategy.Absolute, slidingExpiration: false);
        await _cacheStore.SetAsync(key, value, options);

        // Act
        var result = await _cacheStore.GetAsync<string>(key);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(value, result.Data);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task ConcurrentOperations_ShouldBeThreadSafe()
    {
        // Arrange
        var tasks = new List<Task>();
        var keyCount = 100;

        // Act - Concurrent writes
        for (int i = 0; i < keyCount; i++)
        {
            var key = $"concurrent_key_{i}";
            var value = $"concurrent_value_{i}";
            tasks.Add(_cacheStore.SetAsync(key, value));
        }
        await Task.WhenAll(tasks);

        // Assert - All writes succeeded
        Assert.Equal(keyCount, _cacheStore.Count);

        // Act - Concurrent reads
        tasks.Clear();
        for (int i = 0; i < keyCount; i++)
        {
            var key = $"concurrent_key_{i}";
            tasks.Add(_cacheStore.GetAsync<string>(key));
        }
        await Task.WhenAll(tasks);

        // Assert - Cache still has all entries
        Assert.Equal(keyCount, _cacheStore.Count);
    }

    [Fact]
    public async Task ConcurrentReadAndRemove_ShouldBeThreadSafe()
    {
        // Arrange
        var key = "concurrent_read_remove";
        await _cacheStore.SetAsync(key, "value");
        var tasks = new List<Task>();

        // Act - Concurrent read and remove
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_cacheStore.GetAsync<string>(key));
            tasks.Add(_cacheStore.RemoveAsync(key));
        }
        await Task.WhenAll(tasks);

        // Assert - No exceptions thrown (thread-safe)
    }

    #endregion

    #region CleanupExpiredEntries Tests

    [Fact]
    public async Task CleanupExpiredEntries_ShouldRemoveExpiredEntries()
    {
        // Arrange
        var expiredKey = "cleanup_expired_key";
        var validKey = "cleanup_valid_key";
        var expiredOptions = new CacheOptions(-1); // Already expired
        var validOptions = new CacheOptions(10); // Valid for 10 minutes

        await _cacheStore.SetAsync(expiredKey, "expired_value", expiredOptions);
        await _cacheStore.SetAsync(validKey, "valid_value", validOptions);

        // Act
        var cleanupCount = _cacheStore.CleanupExpiredEntries();

        // Assert
        Assert.Equal(1, cleanupCount);
        var exists1 = await _cacheStore.ExistsAsync(expiredKey);
        var exists2 = await _cacheStore.ExistsAsync(validKey);
        Assert.False(exists1.Data);
        Assert.True(exists2.Data);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task SetAsync_WithNullValue_ShouldStore()
    {
        // Arrange
        var key = "null_value_key";
        string? value = null;

        // Act
        var result = await _cacheStore.SetAsync(key, value);
        var getResult = await _cacheStore.GetAsync<string>(key);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Null(getResult.Data);
    }

    [Fact]
    public async Task GetAsync_WithEmptyString_ShouldReturnEmptyString()
    {
        // Arrange
        var key = "empty_string_key";
        var value = "";

        // Act
        await _cacheStore.SetAsync(key, value);
        var result = await _cacheStore.GetAsync<string>(key);

        // Assert
        Assert.Equal(value, result.Data);
    }

    [Fact]
    public async Task SetAsync_WithLargeObject_ShouldStore()
    {
        // Arrange
        var key = "large_object_key";
        var largeList = Enumerable.Range(0, 1000).Select(i => new { Id = i, Value = $"Item {i}" }).ToList();

        // Act
        var result = await _cacheStore.SetAsync(key, largeList);
        var getResult = await _cacheStore.GetAsync<object>(key);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(getResult.Data);
        var list = (System.Collections.ICollection)getResult.Data;
        Assert.Equal(1000, list.Count);
    }

    #endregion
}
