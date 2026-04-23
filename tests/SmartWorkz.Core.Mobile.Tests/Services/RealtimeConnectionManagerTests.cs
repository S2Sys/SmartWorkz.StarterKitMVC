namespace SmartWorkz.Mobile.Tests.Services;

using Moq;
using Xunit;
using SmartWorkz.Mobile;
using SmartWorkz.Shared;

public class RealtimeConnectionManagerTests
{
    [Fact]
    public async Task EnsureConnectedAsync_WhenDisconnected_Reconnects()
    {
        // Arrange
        var mockRealtime = new Mock<IRealtimeService>();
        mockRealtime.Setup(x => x.IsConnectedAsync()).ReturnsAsync(false);
        mockRealtime.Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var manager = new RealtimeConnectionManager(mockRealtime.Object);

        // Act
        var result = await manager.EnsureConnectedAsync("user123");

        // Assert
        Assert.True(result.Succeeded);
        mockRealtime.Verify(x => x.ConnectAsync("user123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnsureConnectedAsync_WhenConnected_SkipsReconnect()
    {
        // Arrange
        var mockRealtime = new Mock<IRealtimeService>();
        mockRealtime.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);

        var manager = new RealtimeConnectionManager(mockRealtime.Object);

        // Act
        var result = await manager.EnsureConnectedAsync("user123");

        // Assert
        Assert.True(result.Succeeded);
        mockRealtime.Verify(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HealthCheckAsync_WhenDisconnected_Reconnects()
    {
        // Arrange
        var mockRealtime = new Mock<IRealtimeService>();
        mockRealtime.Setup(x => x.IsConnectedAsync()).ReturnsAsync(false);
        mockRealtime.Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var manager = new RealtimeConnectionManager(mockRealtime.Object);
        await manager.EnsureConnectedAsync("user123");

        // Act
        mockRealtime.Setup(x => x.IsConnectedAsync()).ReturnsAsync(false);
        var healthy = await manager.HealthCheckAsync();

        // Assert
        Assert.True(healthy);
        mockRealtime.Verify(x => x.ConnectAsync("user123", It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task HealthCheckAsync_WhenConnected_ReturnsTrue()
    {
        // Arrange
        var mockRealtime = new Mock<IRealtimeService>();
        mockRealtime.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        mockRealtime.Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var manager = new RealtimeConnectionManager(mockRealtime.Object);
        await manager.EnsureConnectedAsync("user123");

        // Act
        var healthy = await manager.HealthCheckAsync();

        // Assert
        Assert.True(healthy);
    }

    [Fact]
    public async Task DisconnectAsync_CleansUpResources()
    {
        // Arrange
        var mockRealtime = new Mock<IRealtimeService>();
        mockRealtime.Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        mockRealtime.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        mockRealtime.Setup(x => x.DisconnectAsync()).ReturnsAsync(Result.Ok());

        var manager = new RealtimeConnectionManager(mockRealtime.Object);
        await manager.EnsureConnectedAsync("user123");

        // Act
        var result = await manager.DisconnectAsync();

        // Assert
        Assert.True(result.Succeeded);
        mockRealtime.Verify(x => x.DisconnectAsync(), Times.Once);
    }

    [Fact]
    public async Task EnsureConnectedAsync_WithNullUserId_ThrowsException()
    {
        // Arrange
        var mockRealtime = new Mock<IRealtimeService>();
        var manager = new RealtimeConnectionManager(mockRealtime.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => manager.EnsureConnectedAsync(null!));
    }

    [Fact]
    public async Task EnsureConnectedAsync_WithEmptyUserId_ThrowsException()
    {
        // Arrange
        var mockRealtime = new Mock<IRealtimeService>();
        var manager = new RealtimeConnectionManager(mockRealtime.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => manager.EnsureConnectedAsync(""));
    }

    [Fact]
    public async Task HealthCheckAsync_BeforeAnyConnection_ReturnsFalse()
    {
        // Arrange
        var mockRealtime = new Mock<IRealtimeService>();
        var manager = new RealtimeConnectionManager(mockRealtime.Object);

        // Act
        var healthy = await manager.HealthCheckAsync();

        // Assert
        Assert.False(healthy);
    }

    [Fact]
    public async Task EnsureConnectedAsync_OnConnectFailure_ReturnsFailure()
    {
        // Arrange
        var mockRealtime = new Mock<IRealtimeService>();
        mockRealtime.Setup(x => x.IsConnectedAsync()).ReturnsAsync(false);
        mockRealtime.Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Connection.Failed", "Unable to connect"));

        var manager = new RealtimeConnectionManager(mockRealtime.Object);

        // Act
        var result = await manager.EnsureConnectedAsync("user123");

        // Assert
        Assert.False(result.Succeeded);
    }
}
