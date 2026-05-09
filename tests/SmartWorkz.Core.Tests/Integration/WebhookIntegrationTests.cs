using SmartWorkz.Core.Shared.Webhooks.Models;

namespace SmartWorkz.Core.Tests.Integration;

/// <summary>
/// Integration tests for Webhook events and logging interactions.
/// Tests webhook event creation and properties.
/// </summary>
public class WebhookIntegrationTests
{
    /// <summary>
    /// Test 1: UserCreatedEvent contains correct data.
    /// </summary>
    [Fact]
    public void UserCreatedEvent_WithData_SucceedsWithValidProperties()
    {
        // Arrange
        var userId = "user-123";
        var email = "user@example.com";
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var @event = new UserCreatedEvent(userId, email, firstName, lastName);

        // Assert
        Assert.NotNull(@event);
        Assert.Equal("user.created", @event.EventType);
        Assert.NotEmpty(@event.Id);
        Assert.NotEqual(default(DateTime), @event.Timestamp);
        Assert.NotNull(@event.Data);
    }

    /// <summary>
    /// Test 2: TransactionCompletedEvent contains correct data.
    /// </summary>
    [Fact]
    public void TransactionCompletedEvent_WithData_SucceedsWithValidProperties()
    {
        // Arrange
        var transactionId = "trans-456";
        var amount = 99.99m;
        var status = "completed";

        // Act
        var @event = new TransactionCompletedEvent(transactionId, amount, status);

        // Assert
        Assert.NotNull(@event);
        Assert.Equal("transaction.completed", @event.EventType);
        Assert.NotEmpty(@event.Id);
        Assert.NotEqual(default(DateTime), @event.Timestamp);
        Assert.NotNull(@event.Data);
    }

    /// <summary>
    /// Test 3: WebhookEvent with TenantId.
    /// </summary>
    [Fact]
    public void WebhookEvent_WithTenantId_SucceedsWithTenantIsolation()
    {
        // Arrange
        var tenantId = "tenant-001";
        var @event = new UserCreatedEvent("user-789", "test@example.com", "Jane", "Smith")
        {
            TenantId = tenantId
        };

        // Act & Assert
        Assert.Equal(tenantId, @event.TenantId);
        Assert.Equal("user.created", @event.EventType);
    }

    /// <summary>
    /// Test 4: Multiple webhook events have unique IDs.
    /// </summary>
    [Fact]
    public void WebhookEvents_MultipleInstances_HaveUniqueIds()
    {
        // Act
        var event1 = new UserCreatedEvent("user-1", "user1@example.com", "First", "User");
        var event2 = new UserCreatedEvent("user-2", "user2@example.com", "Second", "User");
        var event3 = new TransactionCompletedEvent("trans-1", 50.00m, "pending");

        // Assert
        Assert.NotEqual(event1.Id, event2.Id);
        Assert.NotEqual(event2.Id, event3.Id);
        Assert.NotEqual(event1.Id, event3.Id);
    }

    /// <summary>
    /// Test 5: WebhookEvent timestamps are set.
    /// </summary>
    [Fact]
    public void WebhookEvent_Timestamp_IsUtcNow()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;

        // Act
        var @event = new UserCreatedEvent("user-ts", "ts@example.com", "Time", "Stamp");

        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.True(@event.Timestamp >= beforeCreate && @event.Timestamp <= afterCreate);
    }

    /// <summary>
    /// Test 6: WebhookEvent record allows with expressions.
    /// </summary>
    [Fact]
    public void WebhookEvent_WithExpression_CreatesNewInstanceWithUpdates()
    {
        // Arrange
        var originalEvent = new UserCreatedEvent("user-orig", "orig@example.com", "Original", "User");
        var newTenantId = "tenant-new";

        // Act
        var updatedEvent = originalEvent with { TenantId = newTenantId };

        // Assert
        Assert.Equal(newTenantId, updatedEvent.TenantId);
        Assert.Equal(originalEvent.Id, updatedEvent.Id);
        Assert.Equal(originalEvent.EventType, updatedEvent.EventType);
    }
}
