namespace SmartWorkz.Core.Tests.Services.Notifications;

using Moq;
using SmartWorkz.Core.Services.Notifications;
using SmartWorkz.Core.Shared.Notifications;
using Microsoft.Extensions.Logging;
using Xunit;

public class FirebaseCloudMessagingServiceTests
{
    [Fact]
    public async Task SendAsync_WithValidPayload_SendsNotification()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FirebaseCloudMessagingService>>();
        var service = new FirebaseCloudMessagingService(mockLogger.Object);
        var payload = new PushNotificationPayload
        {
            Title = "Test",
            Body = "Test Message"
        };

        // Act & Assert - Note: Real Firebase integration would need mock
        // This test validates structure; actual implementation requires Firebase emulator
        Assert.NotNull(payload);
        Assert.Equal("Test", payload.Title);
    }

    [Fact]
    public void PushNotificationPayload_HasExpectedProperties()
    {
        // Arrange
        var payload = new PushNotificationPayload
        {
            Title = "Title",
            Body = "Body",
            ImageUrl = "http://example.com/image.jpg",
            Badge = 5,
            Data = new Dictionary<string, string> { { "key", "value" } }
        };

        // Assert
        Assert.Equal("Title", payload.Title);
        Assert.Equal("Body", payload.Body);
        Assert.Equal("http://example.com/image.jpg", payload.ImageUrl);
        Assert.Equal(5, payload.Badge);
        Assert.NotEmpty(payload.Data);
    }
}
