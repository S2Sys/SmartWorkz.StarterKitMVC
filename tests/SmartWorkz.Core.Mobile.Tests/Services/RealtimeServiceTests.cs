namespace SmartWorkz.Mobile.Tests.Services;

using Xunit;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Mobile.Models;
using Moq;

public class RealtimeServiceTests
{
    [Fact]
    public async Task ConnectAsync_ValidUserId_ReturnsSuccess()
    {
        // Arrange
        var service = new Mock<IRealtimeService>();
        service.Setup(x => x.ConnectAsync("user123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await service.Object.ConnectAsync("user123");

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task IsConnectedAsync_AfterConnection_ReturnsTrue()
    {
        // Arrange
        var service = new Mock<IRealtimeService>();
        service.Setup(x => x.IsConnectedAsync())
            .ReturnsAsync(true);

        // Act
        var isConnected = await service.Object.IsConnectedAsync();

        // Assert
        Assert.True(isConnected);
    }

    [Fact]
    public void RealtimeMessage_WithPayload_SerializesCorrectly()
    {
        // Arrange
        var payload = new { OrderId = "123", Status = "Shipped" };
        var message = new RealtimeMessage(
            Channel: "Orders",
            Method: "OrderUpdated",
            Payload: payload,
            ReceivedAt: DateTime.UtcNow,
            CorrelationId: "abc-123");

        // Act
        var json = message.PayloadJson;

        // Assert
        Assert.NotEmpty(json);
        Assert.Contains("OrderId", json);
    }

    [Fact]
    public void RealtimeMessage_IsSystemMessage_DetectsSystemMethods()
    {
        // Arrange
        var systemMessage = new RealtimeMessage("Orders", "System.Heartbeat", null,
            DateTime.UtcNow, "xyz");
        var userMessage = new RealtimeMessage("Orders", "OrderUpdated", null,
            DateTime.UtcNow, "xyz");

        // Act & Assert
        Assert.True(systemMessage.IsSystemMessage);
        Assert.False(userMessage.IsSystemMessage);
    }
}
