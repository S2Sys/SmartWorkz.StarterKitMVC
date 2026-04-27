using SmartWorkz.Shared;

namespace SmartWorkz.Core.Tests.Integration;

/// <summary>
/// Integration tests for Cache + CQRS module interactions.
/// Tests scenarios where cache and CQRS work together in queries and command handling.
/// </summary>
public class CacheIntegrationTests
{
    /// <summary>
    /// Test 1: MemoryCacheService stores and retrieves data correctly.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_StoresAndRetrievesData_Succeeds()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var cacheKey = "query:test:all";
        var testData = "Test Data";

        // Act
        var setResult = await cache.SetAsync(cacheKey, testData);
        var getResult = await cache.GetAsync<string>(cacheKey);

        // Assert
        Assert.True(setResult.Succeeded);
        Assert.True(getResult.Succeeded);
        Assert.Equal(testData, getResult.Data);
    }

    /// <summary>
    /// Test 2: Cache miss returns failure result.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_CacheMiss_ReturnsFail()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var nonExistentKey = "non:existent:key";

        // Act
        var result = await cache.GetAsync<string>(nonExistentKey);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Data);
    }

    /// <summary>
    /// Test 3: Cache invalidation works correctly.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_CacheInvalidation_Succeeds()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var cacheKey = "query:items:all";
        var testData = "Items Data";

        // Act - Set cache
        await cache.SetAsync(cacheKey, testData);
        var getBeforeRemove = await cache.GetAsync<string>(cacheKey);

        // Act - Remove from cache
        var removeResult = await cache.RemoveAsync(cacheKey);
        var getAfterRemove = await cache.GetAsync<string>(cacheKey);

        // Assert
        Assert.True(getBeforeRemove.Succeeded);
        Assert.True(removeResult.Succeeded);
        Assert.False(getAfterRemove.Succeeded);
    }

    /// <summary>
    /// Test 4: Different cache keys are independent.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_DifferentKeys_AreIndependent()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var key1 = "query:users:all";
        var key2 = "query:products:all";
        var data1 = "Users";
        var data2 = "Products";

        // Act
        await cache.SetAsync(key1, data1);
        await cache.SetAsync(key2, data2);

        var result1 = await cache.GetAsync<string>(key1);
        var result2 = await cache.GetAsync<string>(key2);

        // Assert
        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded);
        Assert.Equal(data1, result1.Data);
        Assert.Equal(data2, result2.Data);
    }

    /// <summary>
    /// Test 5: Cache with TTL still returns data within TTL window.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_WithTtl_ReturnsCachedData()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var cacheKey = "query:temp:data";
        var ttlMinutes = 60;
        var testData = new { Id = 1, Name = "Temp" };

        // Act
        var setResult = await cache.SetAsync(cacheKey, testData, ttlMinutes);
        var getResult = await cache.GetAsync<dynamic>(cacheKey);

        // Assert
        Assert.True(setResult.Succeeded);
        Assert.True(getResult.Succeeded);
        Assert.NotNull(getResult.Data);
    }

    /// <summary>
    /// Test 6: Cache clear removes all entries for tenant.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_Clear_RemovesAllEntries()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var key1 = "data:1";
        var key2 = "data:2";

        // Act
        await cache.SetAsync(key1, "Value1");
        await cache.SetAsync(key2, "Value2");

        var result1Before = await cache.GetAsync<string>(key1);
        var result2Before = await cache.GetAsync<string>(key2);

        var clearResult = await cache.ClearAsync();

        var result1After = await cache.GetAsync<string>(key1);
        var result2After = await cache.GetAsync<string>(key2);

        // Assert
        Assert.True(result1Before.Succeeded);
        Assert.True(result2Before.Succeeded);
        Assert.True(clearResult.Succeeded);
        Assert.False(result1After.Succeeded);
        Assert.False(result2After.Succeeded);
    }
}
