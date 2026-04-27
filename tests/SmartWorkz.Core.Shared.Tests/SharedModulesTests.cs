using SmartWorkz.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.Extensions.Logging;

namespace SmartWorkz.Core.Shared.Tests;

/// <summary>
/// Comprehensive test suite for SmartWorkz.Core.Shared modules covering
/// Caching, CQRS, Logging, and core functionality
/// </summary>
public class SharedModulesTests
{
    #region Caching Tests

    [Fact]
    public async Task MemoryCacheService_SetAndGet_StoresAndRetrievesValue()
    {
        // Arrange
        var cache = new MemoryCacheService();
        const string key = "test_key";
        const string value = "test_value";

        // Act
        await cache.SetAsync(key, value);
        var result = await cache.GetAsync<string>(key);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(value, result.Data);
    }

    [Fact]
    public async Task MemoryCacheService_Remove_DeletesValue()
    {
        // Arrange
        var cache = new MemoryCacheService();
        const string key = "removable";
        await cache.SetAsync(key, "value");

        // Act
        await cache.RemoveAsync(key);
        var result = await cache.GetAsync<string>(key);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task MemoryCacheService_RemoveByPrefix_DeletesMatchingKeys()
    {
        // Arrange
        var cache = new MemoryCacheService();
        await cache.SetAsync("user:1", "Alice");
        await cache.SetAsync("user:2", "Bob");
        await cache.SetAsync("product:1", "Widget");

        // Act
        await cache.RemoveByPrefixAsync("user:*");
        var user1 = await cache.GetAsync<string>("user:1");
        var product1 = await cache.GetAsync<string>("product:1");

        // Assert
        Assert.False(user1.Succeeded);
        Assert.True(product1.Succeeded);
    }

    [Fact]
    public async Task MemoryCacheService_TenantIsolation_IsolatesDataBetweenTenants()
    {
        // Arrange
        var cache = new MemoryCacheService();
        const string key = "shared_key";

        // Act
        await cache.SetAsync(key, "tenant1_value", tenantId: "tenant1");
        await cache.SetAsync(key, "tenant2_value", tenantId: "tenant2");

        var tenant1Result = await cache.GetAsync<string>(key, tenantId: "tenant1");
        var tenant2Result = await cache.GetAsync<string>(key, tenantId: "tenant2");

        // Assert
        Assert.Equal("tenant1_value", tenant1Result.Data);
        Assert.Equal("tenant2_value", tenant2Result.Data);
    }

    [Fact]
    public async Task MemoryCacheService_ExistsAsync_ChecksKeyPresence()
    {
        // Arrange
        var cache = new MemoryCacheService();
        await cache.SetAsync("exists_key", "value");

        // Act
        var exists = await cache.ExistsAsync("exists_key");
        var notExists = await cache.ExistsAsync("missing_key");

        // Assert
        Assert.True(exists);
        Assert.False(notExists);
    }

    [Fact]
    public async Task MemoryCacheService_ClearAsync_RemovesTenantData()
    {
        // Arrange
        var cache = new MemoryCacheService();
        await cache.SetAsync("key1", "value1", tenantId: "tenant1");
        await cache.SetAsync("key2", "value2", tenantId: "tenant1");
        await cache.SetAsync("key3", "value3", tenantId: "tenant2");

        // Act
        await cache.ClearAsync(tenantId: "tenant1");
        var tenant1Key1 = await cache.GetAsync<string>("key1", tenantId: "tenant1");
        var tenant2Key3 = await cache.GetAsync<string>("key3", tenantId: "tenant2");

        // Assert
        Assert.False(tenant1Key1.Succeeded);
        Assert.True(tenant2Key3.Succeeded);
    }

    [Fact]
    public async Task MemoryCacheService_ConcurrentOperations_AreThreadSafe()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var tasks = new List<Task>();

        // Act - Simulate concurrent writes
        for (int i = 0; i < 100; i++)
        {
            int index = i;
            tasks.Add(cache.SetAsync($"key_{index}", $"value_{index}"));
        }
        await Task.WhenAll(tasks);

        // Assert
        for (int i = 0; i < 100; i++)
        {
            var result = await cache.GetAsync<string>($"key_{i}");
            Assert.True(result.Succeeded);
            Assert.Equal($"value_{i}", result.Data);
        }
    }

    #endregion

    #region CQRS Tests

    [Fact]
    public void IQuery_MarkerInterface_DefinedCorrectly()
    {
        // Arrange
        var queryType = typeof(IQuery<>);

        // Act & Assert
        Assert.True(queryType.IsInterface);
        var args = queryType.GetGenericArguments();
        Assert.Single(args);
    }

    [Fact]
    public void IQueryHandler_HasHandleAsyncMethod()
    {
        // Arrange
        var handlerType = typeof(IQueryHandler<,>);

        // Act
        var method = handlerType.GetMethod("HandleAsync");

        // Assert
        Assert.NotNull(method);
        Assert.True(method!.IsGenericMethodDefinition || method!.GetParameters().Length >= 1);
    }

    #endregion

    #region Caching Options Tests

    [Fact]
    public void CacheOptions_CanBeInstantiatedWithDefaults()
    {
        // Arrange & Act
        var options = new CacheOptions();

        // Assert
        Assert.NotNull(options);
        Assert.Equal(5, options.TtlMinutes); // Default is 5 minutes
    }

    [Fact]
    public void CacheOptions_SupportsCustomTTL()
    {
        // Arrange & Act
        var options = new CacheOptions(30);

        // Assert
        Assert.Equal(30, options.TtlMinutes);
    }

    [Fact]
    public void CacheOptions_SupportsNullTTL()
    {
        // Arrange & Act
        var options = new CacheOptions(null);

        // Assert
        Assert.Null(options.TtlMinutes);
    }

    [Fact]
    public void CacheOptions_DefaultsToAbsoluteStrategy()
    {
        // Arrange & Act
        var options = new CacheOptions();

        // Assert
        Assert.Equal(CacheStrategy.Absolute, options.CacheStrategy);
    }

    #endregion

    #region Logging Tests

    [Fact]
    public void LoggingStartupExtensions_AddStructuredLogging_ConfiguresLogging()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ASPNETCORE_ENVIRONMENT", "Development" }
            })
            .Build();

        // Act
        var result = services.AddStructuredLogging(config);

        // Assert - Verify it returns the service collection for chaining
        Assert.NotNull(result);
        Assert.Same(services, result);
    }

    [Fact]
    public void LoggingStartupExtensions_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;
        var config = new ConfigurationBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddStructuredLogging(config));
    }

    [Fact]
    public void LoggingStartupExtensions_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration? config = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddStructuredLogging(config!));
    }

    [Fact]
    public void LoggingStartupExtensions_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        // Act
        var result = services.AddStructuredLogging(config);

        // Assert
        Assert.Same(services, result); // Should return same collection for chaining
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task CacheService_WithComplexObjects_StoresSuccessfully()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var user = new { Id = 1, Name = "Alice", Email = "alice@example.com" };

        // Act
        await cache.SetAsync("user", user);
        var result = await cache.GetAsync<dynamic>("user");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task CacheService_WithMultipleTenants_MaintainsIsolation()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var tenants = new[] { "tenant1", "tenant2", "tenant3" };
        var key = "shared_key";

        // Act
        foreach (var tenant in tenants)
        {
            await cache.SetAsync(key, $"value_{tenant}", tenantId: tenant);
        }

        // Assert
        foreach (var tenant in tenants)
        {
            var result = await cache.GetAsync<string>(key, tenantId: tenant);
            Assert.True(result.Succeeded);
            Assert.Equal($"value_{tenant}", result.Data);
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task MemoryCacheService_RemoveNonexistentKey_ReturnsSuccess()
    {
        // Arrange
        var cache = new MemoryCacheService();

        // Act
        var result = await cache.RemoveAsync("nonexistent");

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task MemoryCacheService_EmptyKey_ThrowsArgumentException()
    {
        // Arrange
        var cache = new MemoryCacheService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => cache.SetAsync("", "value"));
    }

    [Fact]
    public async Task MemoryCacheService_WithNullValue_StoresSuccessfully()
    {
        // Arrange
        var cache = new MemoryCacheService();

        // Act
        await cache.SetAsync("null_key", (string?)null);
        var result = await cache.GetAsync<string?>("null_key");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Null(result.Data);
    }

    #endregion
}
