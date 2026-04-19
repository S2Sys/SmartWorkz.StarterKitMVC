using SmartWorkz.Core.Shared.Caching;

namespace SmartWorkz.Core.Tests.Caching;

/// <summary>
/// Tests for MemoryCacheService - in-memory L1 cache with tenant isolation.
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
        var setResult = await _cache.SetAsync(key, value);
        var getResult = await _cache.GetAsync<dynamic>(key);

        // Assert
        Assert.True(setResult.Succeeded);
        Assert.True(getResult.Succeeded);
        Assert.NotNull(getResult.Data);
        Assert.Equal("Test", getResult.Data!.Name);
    }

    [Fact]
    public async Task GetAsync_NonExistentKey_ReturnsFail()
    {
        // Act
        var result = await _cache.GetAsync<string>("nonExistent");

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetAsync_ExpiredValue_ReturnsFail()
    {
        // Arrange
        string key = "expiredKey";
        await _cache.SetAsync(key, "value", ttlMinutes: 0); // 0 minutes = expired immediately
        await Task.Delay(100);

        // Act
        var result = await _cache.GetAsync<string>(key);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Data);
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
        Assert.True(result.Succeeded);
        Assert.Equal(value, result.Data);
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
        Assert.True(result.Succeeded);
        Assert.Equal(value, result.Data);
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
        var removeResult = await _cache.RemoveAsync(key);
        var getResult = await _cache.GetAsync<string>(key);

        // Assert
        Assert.True(removeResult.Succeeded);
        Assert.False(getResult.Succeeded);
    }

    [Fact]
    public async Task RemoveAsync_NonExistentKey_Succeeds()
    {
        // Act
        var result = await _cache.RemoveAsync("nonExistentKey");

        // Assert
        Assert.True(result.Succeeded);
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
        var removeResult = await _cache.RemoveByPrefixAsync("user:");
        var user1 = await _cache.GetAsync<string>("user:1");
        var post1 = await _cache.GetAsync<string>("post:1");

        // Assert
        Assert.True(removeResult.Succeeded);
        Assert.False(user1.Succeeded);
        Assert.True(post1.Succeeded);
        Assert.Equal("value3", post1.Data);
    }

    [Fact]
    public async Task RemoveByPrefixAsync_WithWildcard_RemovesMatchingKeys()
    {
        // Arrange
        await _cache.SetAsync("user:1", "value1");
        await _cache.SetAsync("user:2", "value2");
        await _cache.SetAsync("post:1", "value3");

        // Act
        var removeResult = await _cache.RemoveByPrefixAsync("user:*");
        var user1 = await _cache.GetAsync<string>("user:1");
        var post1 = await _cache.GetAsync<string>("post:1");

        // Assert
        Assert.True(removeResult.Succeeded);
        Assert.False(user1.Succeeded);
        Assert.True(post1.Succeeded);
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
        await _cache.SetAsync(key, "value", ttlMinutes: 0); // 0 minutes = expired immediately
        await Task.Delay(100);

        // Act
        var exists = await _cache.ExistsAsync(key);

        // Assert
        Assert.False(exists);
    }

    #endregion

    #region ClearAsync Tests

    [Fact]
    public async Task ClearAsync_RemovesAllEntriesForDefaultTenant()
    {
        // Arrange
        await _cache.SetAsync("key1", "value1");
        await _cache.SetAsync("key2", "value2");
        await _cache.SetAsync("key3", "value3");

        // Act
        var clearResult = await _cache.ClearAsync();
        var result1 = await _cache.GetAsync<string>("key1");
        var result2 = await _cache.GetAsync<string>("key2");
        var result3 = await _cache.GetAsync<string>("key3");

        // Assert
        Assert.True(clearResult.Succeeded);
        Assert.False(result1.Succeeded);
        Assert.False(result2.Succeeded);
        Assert.False(result3.Succeeded);
    }

    [Fact]
    public async Task ClearAsync_TenantScoped_OnlyClearsSpecificTenant()
    {
        // Arrange
        await _cache.SetAsync("key1", "default-value1", tenantId: null); // "default" tenant
        await _cache.SetAsync("key2", "default-value2", tenantId: null);
        await _cache.SetAsync("key1", "tenant1-value1", tenantId: "tenant1");
        await _cache.SetAsync("key2", "tenant1-value2", tenantId: "tenant1");

        // Act
        var clearResult = await _cache.ClearAsync(tenantId: "tenant1");

        // Default tenant values should still exist
        var defaultKey1 = await _cache.GetAsync<string>("key1", tenantId: null);
        var defaultKey2 = await _cache.GetAsync<string>("key2", tenantId: null);

        // Tenant1 values should be removed
        var tenant1Key1 = await _cache.GetAsync<string>("key1", tenantId: "tenant1");
        var tenant1Key2 = await _cache.GetAsync<string>("key2", tenantId: "tenant1");

        // Assert
        Assert.True(clearResult.Succeeded);
        Assert.True(defaultKey1.Succeeded);
        Assert.Equal("default-value1", defaultKey1.Data);
        Assert.True(defaultKey2.Succeeded);
        Assert.Equal("default-value2", defaultKey2.Data);
        Assert.False(tenant1Key1.Succeeded);
        Assert.False(tenant1Key2.Succeeded);
    }

    #endregion

    #region Multi-Tenant Isolation Tests

    [Fact]
    public async Task MultipleTenants_HaveIsolatedCache()
    {
        // Arrange
        const string key = "sharedKey";
        const string tenant1Value = "tenant1-data";
        const string tenant2Value = "tenant2-data";

        // Act - Set same key in different tenants
        await _cache.SetAsync(key, tenant1Value, tenantId: "tenant1");
        await _cache.SetAsync(key, tenant2Value, tenantId: "tenant2");
        await _cache.SetAsync(key, "default-data", tenantId: null); // default tenant

        // Get from each tenant
        var tenant1Result = await _cache.GetAsync<string>(key, tenantId: "tenant1");
        var tenant2Result = await _cache.GetAsync<string>(key, tenantId: "tenant2");
        var defaultResult = await _cache.GetAsync<string>(key, tenantId: null);

        // Assert - each tenant should have its own value
        Assert.True(tenant1Result.Succeeded);
        Assert.Equal(tenant1Value, tenant1Result.Data);

        Assert.True(tenant2Result.Succeeded);
        Assert.Equal(tenant2Value, tenant2Result.Data);

        Assert.True(defaultResult.Succeeded);
        Assert.Equal("default-data", defaultResult.Data);
    }

    [Fact]
    public async Task RemoveAsync_TenantScoped_OnlyRemovesFromSpecificTenant()
    {
        // Arrange
        const string key = "testKey";
        await _cache.SetAsync(key, "tenant1-value", tenantId: "tenant1");
        await _cache.SetAsync(key, "tenant2-value", tenantId: "tenant2");

        // Act
        var removeResult = await _cache.RemoveAsync(key, tenantId: "tenant1");

        // Assert
        var tenant1Result = await _cache.GetAsync<string>(key, tenantId: "tenant1");
        var tenant2Result = await _cache.GetAsync<string>(key, tenantId: "tenant2");

        Assert.True(removeResult.Succeeded);
        Assert.False(tenant1Result.Succeeded); // Should be removed
        Assert.True(tenant2Result.Succeeded); // Should still exist
        Assert.Equal("tenant2-value", tenant2Result.Data);
    }

    [Fact]
    public async Task RemoveByPrefixAsync_TenantScoped_OnlyRemovesFromSpecificTenant()
    {
        // Arrange
        await _cache.SetAsync("user:1", "tenant1-user1", tenantId: "tenant1");
        await _cache.SetAsync("user:2", "tenant1-user2", tenantId: "tenant1");
        await _cache.SetAsync("user:1", "tenant2-user1", tenantId: "tenant2");
        await _cache.SetAsync("user:2", "tenant2-user2", tenantId: "tenant2");

        // Act
        var removeResult = await _cache.RemoveByPrefixAsync("user:", tenantId: "tenant1");

        // Assert
        var tenant1User1 = await _cache.GetAsync<string>("user:1", tenantId: "tenant1");
        var tenant1User2 = await _cache.GetAsync<string>("user:2", tenantId: "tenant1");
        var tenant2User1 = await _cache.GetAsync<string>("user:1", tenantId: "tenant2");
        var tenant2User2 = await _cache.GetAsync<string>("user:2", tenantId: "tenant2");

        Assert.True(removeResult.Succeeded);
        Assert.False(tenant1User1.Succeeded);
        Assert.False(tenant1User2.Succeeded);
        Assert.True(tenant2User1.Succeeded);
        Assert.True(tenant2User2.Succeeded);
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
            Assert.True(result.Succeeded);
            Assert.Equal($"value{i}", result.Data);
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
                Assert.True(result.Succeeded);
                Assert.Equal($"value{index}", result.Data);
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
        Assert.True(intResult.Succeeded);
        Assert.Equal(intValue, intResult.Data);

        Assert.True(stringResult.Succeeded);
        Assert.Equal(stringValue, stringResult.Data);

        Assert.True(objectResult.Succeeded);
        Assert.NotNull(objectResult.Data);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task SetAsync_WithNullKey_Throws()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _cache.SetAsync<string>(null!, "value"));
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
