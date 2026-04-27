using Xunit;
using System.Text.Json;
using SmartWorkz.Core.Shared.Webhooks.Models;

namespace SmartWorkz.Core.Tests.Webhooks;

/// <summary>
/// Unit tests for WebhookEvent model serialization and deserialization.
/// </summary>
public class WebhookEventModelTests
{
    [Fact]
    public void UserCreatedEvent_SerializesToJson()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-1",
            Timestamp = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)
        };

        // Act
        var json = JsonSerializer.Serialize(@event);

        // Assert
        Assert.NotEmpty(json);
        Assert.Contains("user-123", json);
        Assert.Contains("user@example.com", json);
        Assert.Contains("John", json);
    }

    [Fact]
    public void UserCreatedEvent_DeserializesFromJson()
    {
        // Arrange
        var json = """
        {
            "userId":"user-123",
            "email":"user@example.com",
            "firstName":"John",
            "lastName":"Doe",
            "tenantId":"tenant-1",
            "timestamp":"2024-01-15T10:30:00Z",
            "eventType":"user.created",
            "id":"event-id-123"
        }
        """;

        // Act
        var @event = JsonSerializer.Deserialize<UserCreatedEvent>(json);

        // Assert
        Assert.NotNull(@event);
        Assert.Equal("user-123", @event.UserId);
        Assert.Equal("user@example.com", @event.Email);
        Assert.Equal("John", @event.FirstName);
        Assert.Equal("Doe", @event.LastName);
    }

    [Fact]
    public void WebhookEvent_PreservesMetadata()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-123", "user@example.com", "Jane", "Smith")
        {
            TenantId = "tenant-2"
        };
        var originalId = @event.Id;
        var originalTimestamp = @event.Timestamp;

        // Act
        var json = JsonSerializer.Serialize(@event);
        var deserialized = JsonSerializer.Deserialize<UserCreatedEvent>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(originalId, deserialized.Id);
        Assert.Equal(originalTimestamp, deserialized.Timestamp);
        Assert.Equal("tenant-2", deserialized.TenantId);
    }

    [Fact]
    public void WebhookEvent_WithNullableFields_SerializesCorrectly()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-456", "user456@example.com", "Test", "User")
        {
            TenantId = null  // Nullable field
        };

        // Act
        var json = JsonSerializer.Serialize(@event);
        var deserialized = JsonSerializer.Deserialize<UserCreatedEvent>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Null(deserialized.TenantId);
    }

    [Fact]
    public void WebhookEvent_JsonFormat_IncludesEventType()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-789", "user789@example.com", "Bob", "Builder")
        {
            TenantId = "tenant-3"
        };

        // Act
        var json = JsonSerializer.Serialize(@event);

        // Assert
        Assert.Contains("user.created", json);
    }

    [Fact]
    public void MultipleWebhookEvents_SerializeToValidJson()
    {
        // Arrange
        var events = new WebhookEvent[]
        {
            new UserCreatedEvent("user-1", "user1@example.com", "First", "User"),
            new UserCreatedEvent("user-2", "user2@example.com", "Second", "User")
        };

        // Act
        var json = JsonSerializer.Serialize(events);

        // Assert
        Assert.NotEmpty(json);
        Assert.Contains("user-1", json);
        Assert.Contains("user-2", json);
    }

    [Fact]
    public void WebhookEvent_WithSpecialCharacters_SerializesProperly()
    {
        // Arrange
        var @event = new UserCreatedEvent(
            "user-special",
            "user+special@example.com",
            "José",
            "García-López"
        );

        // Act
        var json = JsonSerializer.Serialize(@event);
        var deserialized = JsonSerializer.Deserialize<UserCreatedEvent>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("José", deserialized.FirstName);
        Assert.Equal("García-López", deserialized.LastName);
        Assert.Equal("user+special@example.com", deserialized.Email);
    }

    [Fact]
    public void WebhookEvent_RoundTrip_PreservesAllData()
    {
        // Arrange
        var originalEvent = new UserCreatedEvent(
            "user-roundtrip",
            "roundtrip@example.com",
            "Round",
            "Trip"
        )
        {
            TenantId = "tenant-roundtrip"
        };

        // Act
        var json = JsonSerializer.Serialize(originalEvent);
        var deserializedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(json);

        // Assert
        Assert.NotNull(deserializedEvent);
        Assert.Equal(originalEvent.UserId, deserializedEvent.UserId);
        Assert.Equal(originalEvent.Email, deserializedEvent.Email);
        Assert.Equal(originalEvent.FirstName, deserializedEvent.FirstName);
        Assert.Equal(originalEvent.LastName, deserializedEvent.LastName);
        Assert.Equal(originalEvent.TenantId, deserializedEvent.TenantId);
        Assert.Equal(originalEvent.Id, deserializedEvent.Id);
    }

    [Fact]
    public void WebhookEvent_JsonSize_IsReasonable()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-123", "user@example.com", "John", "Doe")
        {
            TenantId = "tenant-1"
        };

        // Act
        var json = JsonSerializer.Serialize(@event);

        // Assert - JSON should be relatively compact
        Assert.True(json.Length < 500, "Webhook event JSON should be reasonably sized");
    }

    [Fact]
    public void WebhookEvent_CanBeSerializedMultipleTimes()
    {
        // Arrange
        var @event = new UserCreatedEvent("user-dup", "user-dup@example.com", "Dup", "Event");

        // Act
        var json1 = JsonSerializer.Serialize(@event);
        var json2 = JsonSerializer.Serialize(@event);

        // Assert
        Assert.Equal(json1, json2);
    }

    [Fact]
    public void WebhookEvent_Timestamp_IsPreservedInJson()
    {
        // Arrange
        var timestamp = new DateTime(2024, 3, 15, 14, 25, 30, DateTimeKind.Utc);
        var @event = new UserCreatedEvent("user-ts", "user-ts@example.com", "Time", "Stamp")
        {
            Timestamp = timestamp
        };

        // Act
        var json = JsonSerializer.Serialize(@event);
        var deserialized = JsonSerializer.Deserialize<UserCreatedEvent>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(timestamp, deserialized.Timestamp);
    }

    [Fact]
    public void WebhookEvent_EmptyStrings_SerializeCorrectly()
    {
        // Arrange
        var @event = new UserCreatedEvent(
            "",  // Empty userId
            "email@example.com",
            "",  // Empty firstName
            ""   // Empty lastName
        );

        // Act
        var json = JsonSerializer.Serialize(@event);
        var deserialized = JsonSerializer.Deserialize<UserCreatedEvent>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("", deserialized.UserId);
        Assert.Equal("", deserialized.FirstName);
        Assert.Equal("", deserialized.LastName);
    }
}
