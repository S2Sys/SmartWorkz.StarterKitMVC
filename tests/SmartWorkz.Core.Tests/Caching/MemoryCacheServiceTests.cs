using SmartWorkz.Core.Shared.Caching;

namespace SmartWorkz.Core.Tests.Caching;

/// <summary>
/// Tests for MemoryCacheService - in-memory L1 cache.
/// </summary>
public class MemoryCacheServiceTests : IDisposable
{
    private readonly MemoryCacheService _cache;

    public MemoryCacheServiceTests()
    {
        _cache = new MemoryCacheService();
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    #region SetAsync and GetAsync Tests

    [Fact]
    public async Task SetAsync_And_GetAsync_Succeeds()
    {
        // Arrange
        string key = "testKey";
        var value = new { Name = "Test" };

        // Act
        await _cache.SetAsync(key, value);
        var result = await _cache.GetAsync<dynamic>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result!.Name);
    }

    [Fact]
    public async Task GetAsync_NonExistentKey_ReturnsNull()
    {
        // Act
        var result = await _cache.GetAsync<string>("nonExistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_ExpiredValue_ReturnsNull()
    {
        // Arrange
        string key = "expiredKey";
        await _cache.SetAsync(key, "value", ttl: TimeSpan.FromMilliseconds(50));
        await Task.Delay(100);

        // Act
        var result = await _cache.GetAsync<string>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_ValidValue_ReturnsValue()
    {
        // Arrange
        string key = "validKey";
        string value = "testValue";
        await _cache.SetAsync(key, value);

        // Act
        var result = await _cache.GetAsync<string>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task SetAsync_WithNoTtl_StoresInfinitely()
    {
        // Arrange
        string key = "permanentKey";
        string value = "permanentValue";

        // Act
        await _cache.SetAsync(key, value);
        await Task.Delay(100);
        var result = await _cache.GetAsync<string>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(value, result);
    }

    #endregion

    #region RemoveAsync Tests

    [Fact]
    public async Task RemoveAsync_ExistingKey_RemovesSuccessfully()
    {
        // Arrange
        string key = "removeKey";
        await _cache.SetAsync(key, "value");

        // Act
        await _cache.RemoveAsync(key);
        var result = await _cache.GetAsync<string>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveAsync_NonExistentKey_DoesNotThrow()
    {
        // Act & Assert
        await _cache.RemoveAsync("nonExistentKey");
    }

    #endregion

    #region RemoveByPrefixAsync Tests

    [Fact]
    public async Task RemoveByPrefixAsync_RemovesAllMatchingKeys()
    {
        // Arrange
        await _cache.SetAsync("user:1", "value1");
        await _cache.SetAsync("user:2", "value2");
        await _cache.SetAsync("post:1", "value3");

        // Act
        await _cache.RemoveByPrefixAsync("user:");
        var user1 = await _cache.GetAsync<string>("user:1");
        var post1 = await _cache.GetAsync<string>("post:1");

        // Assert
        Assert.Null(user1);
        Assert.NotNull(post1);
        Assert.Equal("value3", post1);
    }

    [Fact]
    public async Task RemoveByPrefixAsync_WithWildcard_RemovesMatchingKeys()
    {
        // Arrange
        await _cache.SetAsync("user:1", "value1");
        await _cache.SetAsync("user:2", "value2");
        await _cache.SetAsync("post:1", "value3");

        // Act
        await _cache.RemoveByPrefixAsync("user:*");
        var user1 = await _cache.GetAsync<string>("user:1");
        var post1 = await _cache.GetAsync<string>("post:1");

        // Assert
        Assert.Null(user1);
        Assert.NotNull(post1);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_ExistingKey_ReturnsTrue()
    {
        // Arrange
        await _cache.SetAsync("existingKey", "value");

        // Act
        var exists = await _cache.ExistsAsync("existingKey");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_NonExistentKey_ReturnsFalse()
    {
        // Act
        var exists = await _cache.ExistsAsync("nonExistentKey");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsAsync_ExpiredKey_ReturnsFalse()
    {
        // Arrange
        string key = "expiredKey";
        await _cache.SetAsync(key, "value", ttl: TimeSpan.FromMilliseconds(50));
        await Task.Delay(100);

        // Act
        var exists = await _cache.ExistsAsync(key);

        // Assert
        Assert.False(exists);
    }

    #endregion

    #region ClearAsync Tests

    [Fact]
    public async Task ClearAsync_RemovesAllEntries()
    {
        // Arrange
        await _cache.SetAsync("key1", "value1");
        await _cache.SetAsync("key2", "value2");
        await _cache.SetAsync("key3", "value3");

        // Act
        await _cache.ClearAsync();
        var result1 = await _cache.GetAsync<string>("key1");
        var result2 = await _cache.GetAsync<string>("key2");
        var result3 = await _cache.GetAsync<string>("key3");

        // Assert
        Assert.Null(result1);
        Assert.Null(result2);
        Assert.Null(result3);
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task ConcurrentSetsAndGets_DoNotCorruptState()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            int index = i;
            tasks.Add(_cache.SetAsync($"key{index}", $"value{index}"));
        }
        await Task.WhenAll(tasks);

        // Assert - all values should be retrievable
        for (int i = 0; i < 100; i++)
        {
            var result = await _cache.GetAsync<string>($"key{i}");
            Assert.NotNull(result);
            Assert.Equal($"value{i}", result);
        }
    }

    [Fact]
    public async Task ConcurrentOperations_AreThreadSafe()
    {
        // Arrange
        var setTasks = new List<Task>();
        var getTasks = new List<Task>();

        // Act - Concurrent sets
        for (int i = 0; i < 50; i++)
        {
            int index = i;
            setTasks.Add(_cache.SetAsync($"key{index}", $"value{index}"));
        }

        await Task.WhenAll(setTasks);

        // Act - Concurrent gets
        for (int i = 0; i < 50; i++)
        {
            int index = i;
            getTasks.Add(Task.Run(async () =>
            {
                var result = await _cache.GetAsync<string>($"key{index}");
                Assert.NotNull(result);
                Assert.Equal($"value{index}", result);
            }));
        }

        // Assert
        await Task.WhenAll(getTasks);
    }

    #endregion

    #region Type Conversion Tests

    [Fact]
    public async Task GetAsync_WithDifferentTypes_ReturnsCorrectType()
    {
        // Arrange
        int intValue = 42;
        string stringValue = "test";
        var objectValue = new { Id = 1, Name = "Test" };

        await _cache.SetAsync("intKey", intValue);
        await _cache.SetAsync("stringKey", stringValue);
        await _cache.SetAsync("objectKey", objectValue);

        // Act
        var intResult = await _cache.GetAsync<int>("intKey");
        var stringResult = await _cache.GetAsync<string>("stringKey");
        var objectResult = await _cache.GetAsync<dynamic>("objectKey");

        // Assert
        Assert.Equal(intValue, intResult);
        Assert.Equal(stringValue, stringResult);
        Assert.NotNull(objectResult);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task SetAsync_WithNullKey_Throws()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _cache.SetAsync(null!, "value"));
    }

    [Fact]
    public async Task SetAsync_WithEmptyKey_Throws()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _cache.SetAsync("", "value"));
    }

    [Fact]
    public async Task GetAsync_WithNullKey_Throws()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _cache.GetAsync<string>(null!));
    }

    [Fact]
    public async Task RemoveAsync_WithNullKey_Throws()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _cache.RemoveAsync(null!));
    }

    #endregion
}
