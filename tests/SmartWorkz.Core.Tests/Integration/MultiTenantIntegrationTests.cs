using SmartWorkz.Shared;

namespace SmartWorkz.Core.Tests.Integration;

/// <summary>
/// Integration tests for Multi-tenant scenarios.
/// Tests tenant isolation in cache and data access.
/// </summary>
public class MultiTenantIntegrationTests
{
    /// <summary>
    /// Test 1: MemoryCacheService isolates data by tenant.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_IsolatesByTenant_Succeeds()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var tenant1Id = "tenant-001";
        var tenant2Id = "tenant-002";
        var cacheKey = "user:profile";
        var data1 = "Tenant1Data";
        var data2 = "Tenant2Data";

        // Act
        await cache.SetAsync(cacheKey, data1, null, tenant1Id);
        await cache.SetAsync(cacheKey, data2, null, tenant2Id);

        var result1 = await cache.GetAsync<string>(cacheKey, tenant1Id);
        var result2 = await cache.GetAsync<string>(cacheKey, tenant2Id);

        // Assert
        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded);
        Assert.Equal(data1, result1.Data);
        Assert.Equal(data2, result2.Data);
    }

    /// <summary>
    /// Test 2: Cache operations for different tenants don't interfere.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_MultiTenant_NoInterference()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var tenant1 = "org-A";
        var tenant2 = "org-B";
        var key = "config:settings";

        var config1 = new { Theme = "dark", Language = "en" };
        var config2 = new { Theme = "light", Language = "fr" };

        // Act
        await cache.SetAsync(key, config1, null, tenant1);
        await cache.SetAsync(key, config2, null, tenant2);

        var getFromTenant1 = await cache.GetAsync<dynamic>(key, tenant1);
        var getFromTenant2 = await cache.GetAsync<dynamic>(key, tenant2);

        // Assert
        Assert.True(getFromTenant1.Succeeded);
        Assert.True(getFromTenant2.Succeeded);
        Assert.Equal("dark", getFromTenant1.Data?.Theme);
        Assert.Equal("light", getFromTenant2.Data?.Theme);
    }

    /// <summary>
    /// Test 3: Cache removal for one tenant doesn't affect another.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_RemoveForTenant_DoesntAffectOthers()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var tenant1 = "customer-A";
        var tenant2 = "customer-B";
        var key = "user:preferences";
        var data = new { Notifications = true };

        // Act
        await cache.SetAsync(key, data, null, tenant1);
        await cache.SetAsync(key, data, null, tenant2);

        var beforeRemove1 = await cache.GetAsync<dynamic>(key, tenant1);
        var beforeRemove2 = await cache.GetAsync<dynamic>(key, tenant2);

        await cache.RemoveAsync(key, tenant1);

        var afterRemove1 = await cache.GetAsync<dynamic>(key, tenant1);
        var afterRemove2 = await cache.GetAsync<dynamic>(key, tenant2);

        // Assert
        Assert.True(beforeRemove1.Succeeded);
        Assert.True(beforeRemove2.Succeeded);
        Assert.False(afterRemove1.Succeeded);
        Assert.True(afterRemove2.Succeeded);
    }

    /// <summary>
    /// Test 4: Clear only removes data for specified tenant.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_ClearForTenant_OnlyAffectsTenant()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var tenant1 = "shop-A";
        var tenant2 = "shop-B";

        // Act
        await cache.SetAsync("item:1", "data1", null, tenant1);
        await cache.SetAsync("item:2", "data2", null, tenant1);
        await cache.SetAsync("item:3", "data3", null, tenant2);

        var beforeClear1 = await cache.GetAsync<string>("item:1", tenant1);
        var beforeClear2 = await cache.GetAsync<string>("item:3", tenant2);

        await cache.ClearAsync(tenant1);

        var afterClear1 = await cache.GetAsync<string>("item:1", tenant1);
        var afterClear2 = await cache.GetAsync<string>("item:2", tenant1);
        var afterClear3 = await cache.GetAsync<string>("item:3", tenant2);

        // Assert
        Assert.True(beforeClear1.Succeeded);
        Assert.True(beforeClear2.Succeeded);
        Assert.False(afterClear1.Succeeded);
        Assert.False(afterClear2.Succeeded);
        Assert.True(afterClear3.Succeeded);
    }

    /// <summary>
    /// Test 5: Default tenant handling.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_DefaultTenant_Succeeds()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var defaultData = "DefaultTenantData";

        // Act - Set without explicit tenant ID (defaults to "default")
        var setResult = await cache.SetAsync("default:key", defaultData);
        var getResult = await cache.GetAsync<string>("default:key");

        // Assert
        Assert.True(setResult.Succeeded);
        Assert.True(getResult.Succeeded);
        Assert.Equal(defaultData, getResult.Data);
    }

    /// <summary>
    /// Test 6: Exists method respects tenant isolation.
    /// </summary>
    [Fact]
    public async Task MemoryCacheService_Exists_RespectsTenantIsolation()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var tenant1 = "platform-A";
        var tenant2 = "platform-B";
        var key = "resource:123";

        // Act
        await cache.SetAsync(key, "value", null, tenant1);

        var existsInTenant1 = await cache.ExistsAsync(key, tenant1);
        var existsInTenant2 = await cache.ExistsAsync(key, tenant2);

        // Assert
        Assert.True(existsInTenant1);
        Assert.False(existsInTenant2);
    }
}
