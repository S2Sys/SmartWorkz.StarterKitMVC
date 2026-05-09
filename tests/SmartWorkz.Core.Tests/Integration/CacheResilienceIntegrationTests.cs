using SmartWorkz.Shared;

namespace SmartWorkz.Core.Tests.Integration;

/// <summary>
/// Integration tests for Cache resilience and error handling patterns.
/// Tests cache behavior under various scenarios including errors and edge cases.
/// </summary>
public class CacheResilienceIntegrationTests
{
    /// <summary>
    /// Test 1: Cache handles null values gracefully.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_NullValue_HandledGracefully()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var cacheKey = "nullable:value";
        string? nullValue = null;

        // Act
        var setResult = await cache.SetAsync(cacheKey, nullValue);
        var getResult = await cache.GetAsync<string?>(cacheKey);

        // Assert
        Assert.True(setResult.Succeeded);
        Assert.True(getResult.Succeeded);
        Assert.Null(getResult.Data);
    }

    /// <summary>
    /// Test 2: Cache handles complex objects.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_ComplexObject_HandledCorrectly()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var cacheKey = "complex:object";
        var complexData = new { Id = 1, Name = "Test", Values = new[] { 1, 2, 3 } };

        // Act
        var setResult = await cache.SetAsync(cacheKey, complexData);
        var getResult = await cache.GetAsync<dynamic>(cacheKey);

        // Assert
        Assert.True(setResult.Succeeded);
        Assert.True(getResult.Succeeded);
        Assert.NotNull(getResult.Data);
    }

    /// <summary>
    /// Test 3: Removing non-existent key returns success.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_RemoveNonExistent_ReturnsSuccess()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var nonExistentKey = "does:not:exist";

        // Act
        var result = await cache.RemoveAsync(nonExistentKey);

        // Assert
        Assert.True(result.Succeeded);
    }

    /// <summary>
    /// Test 4: RemoveByPrefix handles multiple matching keys.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_RemoveByPrefix_RemovesAllMatches()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var prefix = "user:";

        // Act
        await cache.SetAsync("user:1:profile", "data1");
        await cache.SetAsync("user:2:profile", "data2");
        await cache.SetAsync("user:3:settings", "data3");
        await cache.SetAsync("product:1", "notremoved");

        var removeResult = await cache.RemoveByPrefixAsync(prefix + "*");

        var check1 = await cache.GetAsync<string>("user:1:profile");
        var check2 = await cache.GetAsync<string>("product:1");

        // Assert
        Assert.True(removeResult.Succeeded);
        Assert.False(check1.Succeeded);
        Assert.True(check2.Succeeded);
    }

    /// <summary>
    /// Test 5: Cache operations are idempotent.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_MultipleRemoves_IsIdempotent()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var cacheKey = "idempotent:key";

        // Act
        await cache.SetAsync(cacheKey, "value");
        var remove1 = await cache.RemoveAsync(cacheKey);
        var remove2 = await cache.RemoveAsync(cacheKey);
        var remove3 = await cache.RemoveAsync(cacheKey);

        // Assert
        Assert.True(remove1.Succeeded);
        Assert.True(remove2.Succeeded);
        Assert.True(remove3.Succeeded);
    }

    /// <summary>
    /// Test 6: Cache set with different TTL values.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_DifferentTtlValues_Succeeds()
    {
        // Arrange
        var cache = new MemoryCacheService();

        // Act
        var result1 = await cache.SetAsync("short:lived", "data", 1);
        var result2 = await cache.SetAsync("long:lived", "data", 1440);
        var result3 = await cache.SetAsync("no:ttl", "data", null);

        // Assert
        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded);
        Assert.True(result3.Succeeded);
    }

    /// <summary>
    /// Test 7: Exists returns correct status before and after operations.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_Exists_TracksCorrectly()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var cacheKey = "track:existence";

        // Act
        var existsBefore = await cache.ExistsAsync(cacheKey);
        await cache.SetAsync(cacheKey, "value");
        var existsAfterSet = await cache.ExistsAsync(cacheKey);
        await cache.RemoveAsync(cacheKey);
        var existsAfterRemove = await cache.ExistsAsync(cacheKey);

        // Assert
        Assert.False(existsBefore);
        Assert.True(existsAfterSet);
        Assert.False(existsAfterRemove);
    }
}
