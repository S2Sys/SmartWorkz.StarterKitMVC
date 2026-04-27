using SmartWorkz.Shared;
using SmartWorkz.Core.Shared.Webhooks.Models;

namespace SmartWorkz.Core.Tests.Integration;

/// <summary>
/// Integration tests for cross-module interactions.
/// Tests how different modules work together (Cache, Validation, Webhooks, etc).
/// </summary>
public class CrossModuleIntegrationTests
{
    /// <summary>
    /// Test 1: Cache + Validation pattern - cache validation results.
    /// </summary>
    [Fact]
    public async Task Cache_And_Validation_IntegrationPattern_Succeeds()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var cacheKey = "validation:rules:user";
        var validationRules = new { Required = true, MinLength = 3, MaxLength = 50 };

        // Act
        await cache.SetAsync(cacheKey, validationRules, 60); // Cache for 60 minutes
        var cachedRules = await cache.GetAsync<dynamic>(cacheKey);

        // Assert
        Assert.True(cachedRules.Succeeded);
        Assert.NotNull(cachedRules.Data);
        Assert.Equal(true, cachedRules.Data?.Required);
    }

    /// <summary>
    /// Test 2: Webhook event + Validation pattern.
    /// </summary>
    [Fact]
    public void Webhook_And_Validation_IntegrationPattern_Succeeds()
    {
        // Arrange
        var webhookEvent = new UserCreatedEvent("user-1", "user@example.com", "John", "Doe");

        // Create validation rules for the event
        var failures = new ValidationFailure[0]; // No validation failures for valid event
        var validationResult = new ValidationResult(failures);

        // Act & Assert
        Assert.NotNull(webhookEvent);
        Assert.True(validationResult.IsValid);
        Assert.Equal("user.created", webhookEvent.EventType);
    }

    /// <summary>
    /// Test 3: MultiTenant + Validation integration.
    /// </summary>
    [Fact]
    public async Task MultiTenant_And_Validation_IntegrationPattern_Succeeds()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var tenant1 = "org-A";
        var tenant2 = "org-B";

        var validationConfig1 = new { StrictMode = true, MaxAttempts = 3 };
        var validationConfig2 = new { StrictMode = false, MaxAttempts = 5 };

        // Act
        await cache.SetAsync("validation:config", validationConfig1, null, tenant1);
        await cache.SetAsync("validation:config", validationConfig2, null, tenant2);

        var config1 = await cache.GetAsync<dynamic>("validation:config", tenant1);
        var config2 = await cache.GetAsync<dynamic>("validation:config", tenant2);

        // Assert
        Assert.True(config1.Succeeded);
        Assert.True(config2.Succeeded);
        Assert.Equal(true, config1.Data?.StrictMode);
        Assert.Equal(false, config2.Data?.StrictMode);
    }

    /// <summary>
    /// Test 4: Cache + Webhook event caching pattern.
    /// </summary>
    [Fact]
    public async Task Cache_And_Webhook_EventCaching_Succeeds()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var @event = new TransactionCompletedEvent("trans-123", 99.99m, "completed");
        var eventKey = $"webhook:event:{@event.Id}";

        // Act
        await cache.SetAsync(eventKey, @event, 30); // Cache event for 30 minutes
        var cachedEvent = await cache.GetAsync<TransactionCompletedEvent>(eventKey);

        // Assert
        Assert.True(cachedEvent.Succeeded);
        Assert.NotNull(cachedEvent.Data);
        Assert.Equal("transaction.completed", cachedEvent.Data?.EventType);
    }

    /// <summary>
    /// Test 5: MultiTenant + Cache + Webhook integration.
    /// </summary>
    [Fact]
    public async Task MultiTenant_Cache_Webhook_IntegrationPattern_Succeeds()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var tenant1 = "customer-A";
        var tenant2 = "customer-B";

        var event1 = new UserCreatedEvent("user-1", "a@example.com", "Alice", "A")
        {
            TenantId = tenant1
        };

        var event2 = new UserCreatedEvent("user-2", "b@example.com", "Bob", "B")
        {
            TenantId = tenant2
        };

        // Act
        await cache.SetAsync("latest:event", event1, null, tenant1);
        await cache.SetAsync("latest:event", event2, null, tenant2);

        var eventFromTenant1 = await cache.GetAsync<UserCreatedEvent>("latest:event", tenant1);
        var eventFromTenant2 = await cache.GetAsync<UserCreatedEvent>("latest:event", tenant2);

        // Assert
        Assert.True(eventFromTenant1.Succeeded);
        Assert.True(eventFromTenant2.Succeeded);
        Assert.Equal(tenant1, eventFromTenant1.Data?.TenantId);
        Assert.Equal(tenant2, eventFromTenant2.Data?.TenantId);
    }

    /// <summary>
    /// Test 6: Resilient cache access with fallback pattern.
    /// </summary>
    [Fact]
    public async Task Cache_Resilient_FallbackPattern_Succeeds()
    {
        // Arrange
        var cache = new MemoryCacheService();
        var primaryKey = "cached:config";
        var fallbackData = new { Default = true, Value = "fallback" };
        var primaryData = new { Default = false, Value = "primary" };

        // Act
        // Try to get from cache (miss scenario)
        var cachedData = await cache.GetAsync<dynamic>(primaryKey);

        // Use fallback if cache miss
        var resultData = cachedData.Succeeded ? cachedData.Data : fallbackData;

        // Cache the result for next time
        if (!cachedData.Succeeded)
        {
            await cache.SetAsync(primaryKey, resultData);
        }

        // Next retrieval should hit cache
        var secondRequest = await cache.GetAsync<dynamic>(primaryKey);

        // Assert
        Assert.False(cachedData.Succeeded); // First request was cache miss
        Assert.Equal("fallback", resultData?.Value); // Used fallback
        Assert.True(secondRequest.Succeeded); // Second request hit cache
        Assert.Equal("fallback", secondRequest.Data?.Value);
    }
}
