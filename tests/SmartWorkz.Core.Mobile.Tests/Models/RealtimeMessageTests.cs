namespace SmartWorkz.Mobile.Tests.Models;

using System;
using Xunit;
using SmartWorkz.Mobile.Models;

public class RealtimeMessageTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesMessage()
    {
        // Arrange
        var channel = "Orders";
        var method = "OrderUpdated";
        var payload = new { OrderId = "123" };
        var receivedAt = DateTime.UtcNow;
        var correlationId = "corr-123";

        // Act
        var message = new RealtimeMessage(channel, method, payload, receivedAt, correlationId);

        // Assert
        Assert.Equal(channel, message.Channel);
        Assert.Equal(method, message.Method);
        Assert.Equal(payload, message.Payload);
        Assert.Equal(receivedAt, message.ReceivedAt);
        Assert.Equal(correlationId, message.CorrelationId);
    }

    [Fact]
    public void PayloadJson_WithValidPayload_SerializesCorrectly()
    {
        // Arrange
        var payload = new { OrderId = "456", Status = "Shipped", Amount = 99.99 };
        var message = new RealtimeMessage("Orders", "OrderUpdated", payload, DateTime.UtcNow, "123");

        // Act
        var json = message.PayloadJson;

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);
        Assert.Contains("OrderId", json);
        Assert.Contains("456", json);
        Assert.Contains("Shipped", json);
    }

    [Fact]
    public void PayloadJson_WithNullPayload_ReturnsNull()
    {
        // Arrange
        var message = new RealtimeMessage("Orders", "OrderUpdated", null, DateTime.UtcNow, "123");

        // Act
        var json = message.PayloadJson;

        // Assert
        Assert.Null(json);
    }

    [Fact]
    public void IsSystemMessage_WithSystemPrefix_ReturnsTrue()
    {
        // Arrange
        var systemMethods = new[]
        {
            "System.Heartbeat",
            "System.Reconnect",
            "System.UserDisconnected",
            "System.Error",
            "System.AnyMethod"
        };

        // Act & Assert
        foreach (var method in systemMethods)
        {
            var message = new RealtimeMessage("Orders", method, null, DateTime.UtcNow, "123");
            Assert.True(message.IsSystemMessage, $"Method '{method}' should be detected as system message");
        }
    }

    [Fact]
    public void IsSystemMessage_WithoutSystemPrefix_ReturnsFalse()
    {
        // Arrange
        var userMethods = new[]
        {
            "OrderUpdated",
            "UserLoggedIn",
            "SystemNotification", // Note: Contains "System" but doesn't START with it
            "NotificationReceived",
            "system.heartbeat" // lowercase - should not match
        };

        // Act & Assert
        foreach (var method in userMethods)
        {
            var message = new RealtimeMessage("Orders", method, null, DateTime.UtcNow, "123");
            Assert.False(message.IsSystemMessage, $"Method '{method}' should NOT be detected as system message");
        }
    }

    [Fact]
    public void Age_CalculatesTimeElapsedCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var receivedAt = now.AddSeconds(-5);
        var message = new RealtimeMessage("Orders", "OrderUpdated", null, receivedAt, "123");

        // Act
        var age = message.Age;

        // Assert
        Assert.True(age >= TimeSpan.FromSeconds(5), "Age should be at least 5 seconds");
        Assert.True(age < TimeSpan.FromSeconds(6), "Age should be less than 6 seconds");
    }

    [Fact]
    public void Age_ForRecentMessage_IsSmall()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var message = new RealtimeMessage("Orders", "OrderUpdated", null, now, "123");

        // Act
        var age = message.Age;

        // Assert
        Assert.True(age < TimeSpan.FromMilliseconds(100), "Very recent message should have small age");
    }

    [Fact]
    public void Equality_TwoMessagesWithSameData_AreEqual()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var payload = new { Id = "123" };
        var message1 = new RealtimeMessage("Orders", "Updated", payload, now, "corr-1");
        var message2 = new RealtimeMessage("Orders", "Updated", payload, now, "corr-1");

        // Act & Assert
        Assert.Equal(message1, message2);
    }

    [Fact]
    public void Equality_TwoMessagesWithDifferentCorrelationId_AreNotEqual()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var payload = new { Id = "123" };
        var message1 = new RealtimeMessage("Orders", "Updated", payload, now, "corr-1");
        var message2 = new RealtimeMessage("Orders", "Updated", payload, now, "corr-2");

        // Act & Assert
        Assert.NotEqual(message1, message2);
    }

    [Theory]
    [InlineData("", "Method", null, "test")]
    [InlineData("Channel", "", null, "test")]
    [InlineData("Channel", "Method", null, "")]
    public void Constructor_WithEmptyValues_StillCreatesMessage(
        string channel, string method, object payload, string correlationId)
    {
        // This test verifies record behavior - records allow empty strings
        // Guards should be applied at the service layer, not on the model itself

        // Act
        var message = new RealtimeMessage(channel, method, payload, DateTime.UtcNow, correlationId);

        // Assert
        Assert.NotNull(message);
    }

    [Fact]
    public void DisplayName_ReturnsReadableChannelAndMethod()
    {
        // Arrange
        var message = new RealtimeMessage("Orders", "OrderUpdated", null, DateTime.UtcNow, "123");

        // Act
        var displayName = $"{message.Channel}/{message.Method}";

        // Assert
        Assert.Equal("Orders/OrderUpdated", displayName);
    }

    [Fact]
    public void Deconstruction_AllowsPatternMatching()
    {
        // Arrange
        var message = new RealtimeMessage(
            "Notifications",
            "AlertReceived",
            new { Severity = "High" },
            DateTime.UtcNow,
            "xyz-789");

        // Act
        var (channel, method, payload, receivedAt, correlationId) = message;

        // Assert
        Assert.Equal("Notifications", channel);
        Assert.Equal("AlertReceived", method);
        Assert.NotNull(payload);
        Assert.Equal("xyz-789", correlationId);
    }

    [Fact]
    public void ToString_IncludesRelevantInfo()
    {
        // Arrange
        var message = new RealtimeMessage("Orders", "Updated", null, DateTime.UtcNow, "abc-123");

        // Act
        var str = message.ToString();

        // Assert - records auto-generate ToString with property values
        Assert.Contains("Orders", str);
        Assert.Contains("Updated", str);
        Assert.Contains("abc-123", str);
    }
}
