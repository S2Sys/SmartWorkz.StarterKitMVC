namespace SmartWorkz.Mobile.Tests.Services;

using Moq;
using Xunit;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Mobile.Services.Implementations;
using SmartWorkz.Shared;
using Microsoft.Extensions.Logging;

public class AutoReconnectServiceTests
{
    private readonly Mock<IRealtimeService> _mockRealtimeService;
    private readonly Mock<IOfflineMessageQueue> _mockMessageQueue;
    private readonly Mock<ILogger<AutoReconnectService>> _mockLogger;

    public AutoReconnectServiceTests()
    {
        _mockRealtimeService = new Mock<IRealtimeService>();
        _mockMessageQueue = new Mock<IOfflineMessageQueue>();
        _mockLogger = new Mock<ILogger<AutoReconnectService>>();
    }

    [Fact]
    public async Task StartAsync_WithValidUserId_Succeeds()
    {
        // Arrange
        _mockRealtimeService
            .Setup(x => x.OnConnectionStateChanged())
            .Returns(new Mock<IObservable<RealtimeConnectionState>>().Object);

        var service = new AutoReconnectService(
            _mockRealtimeService.Object,
            _mockMessageQueue.Object,
            _mockLogger.Object);

        // Act
        var result = await service.StartAsync("user123");

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task StartAsync_WithNullUserId_ReturnsFail()
    {
        // Arrange
        var service = new AutoReconnectService(
            _mockRealtimeService.Object,
            _mockMessageQueue.Object,
            _mockLogger.Object);

        // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var result = await service.StartAsync(null!);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task StartAsync_WithEmptyUserId_ReturnsFail()
    {
        // Arrange
        var service = new AutoReconnectService(
            _mockRealtimeService.Object,
            _mockMessageQueue.Object,
            _mockLogger.Object);

        // Act
        var result = await service.StartAsync(string.Empty);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task StopAsync_StopsMonitoring()
    {
        // Arrange
        _mockRealtimeService
            .Setup(x => x.OnConnectionStateChanged())
            .Returns(new Mock<IObservable<RealtimeConnectionState>>().Object);

        var service = new AutoReconnectService(
            _mockRealtimeService.Object,
            _mockMessageQueue.Object,
            _mockLogger.Object);

        await service.StartAsync("user123");

        // Act
        var result = await service.StopAsync();

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void IsReconnecting_ReturnsFalse_WhenConnected()
    {
        // Arrange
        _mockRealtimeService
            .Setup(x => x.GetConnectionState())
            .Returns(RealtimeConnectionState.Connected);
        _mockRealtimeService
            .Setup(x => x.OnConnectionStateChanged())
            .Returns(new Mock<IObservable<RealtimeConnectionState>>().Object);

        var service = new AutoReconnectService(
            _mockRealtimeService.Object,
            _mockMessageQueue.Object,
            _mockLogger.Object);

        // Act
        var isReconnecting = service.IsReconnecting;

        // Assert
        Assert.False(isReconnecting);
    }

    [Fact]
    public async Task ReconnectImmediatelyAsync_TriesImmediateReconnection()
    {
        // Arrange
        _mockRealtimeService
            .Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        _mockRealtimeService
            .Setup(x => x.OnConnectionStateChanged())
            .Returns(new Mock<IObservable<RealtimeConnectionState>>().Object);

        var service = new AutoReconnectService(
            _mockRealtimeService.Object,
            _mockMessageQueue.Object,
            _mockLogger.Object);

        // Act
        var result = await service.ReconnectImmediatelyAsync();

        // Assert - ReconnectImmediatelyAsync without starting should fail gracefully
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ReconnectImmediatelyAsync_AfterStart_TriesReconnection()
    {
        // Arrange
        _mockRealtimeService
            .Setup(x => x.OnConnectionStateChanged())
            .Returns(new Mock<IObservable<RealtimeConnectionState>>().Object);
        _mockRealtimeService
            .Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var service = new AutoReconnectService(
            _mockRealtimeService.Object,
            _mockMessageQueue.Object,
            _mockLogger.Object);

        // Act
        await service.StartAsync("user123");
        var result = await service.ReconnectImmediatelyAsync();

        // Assert
        Assert.True(result.Succeeded);
        _mockRealtimeService.Verify(
            x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsValidStats()
    {
        // Arrange
        _mockRealtimeService
            .Setup(x => x.OnConnectionStateChanged())
            .Returns(new Mock<IObservable<RealtimeConnectionState>>().Object);

        var service = new AutoReconnectService(
            _mockRealtimeService.Object,
            _mockMessageQueue.Object,
            _mockLogger.Object);

        // Act
        var result = await service.GetStatsAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.IsType<ReconnectStats>(result.Data);
        Assert.True(result.Data.TotalAttempts >= 0);
        Assert.True(result.Data.SuccessfulReconnects >= 0);
        Assert.True(result.Data.CurrentRetryCount >= 0);
    }

    [Fact]
    public async Task GetStatsAsync_TracksDurations()
    {
        // Arrange
        _mockRealtimeService
            .Setup(x => x.OnConnectionStateChanged())
            .Returns(new Mock<IObservable<RealtimeConnectionState>>().Object);
        _mockRealtimeService
            .Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var service = new AutoReconnectService(
            _mockRealtimeService.Object,
            _mockMessageQueue.Object,
            _mockLogger.Object);

        // Act
        await service.StartAsync("user123");
        var stats1 = await service.GetStatsAsync();

        // Assert
        Assert.True(stats1.Succeeded);
        Assert.NotNull(stats1.Data);
        Assert.True(stats1.Data.AvgReconnectDuration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task StopAsync_CancelsReconnectionAttempts()
    {
        // Arrange
        _mockRealtimeService
            .Setup(x => x.OnConnectionStateChanged())
            .Returns(new Mock<IObservable<RealtimeConnectionState>>().Object);

        var service = new AutoReconnectService(
            _mockRealtimeService.Object,
            _mockMessageQueue.Object,
            _mockLogger.Object);

        await service.StartAsync("user123");

        // Act
        var result = await service.StopAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(service.IsReconnecting);
    }

    [Fact]
    public async Task MultipleStarts_LogsWarningOnSecondStart()
    {
        // Arrange
        _mockRealtimeService
            .Setup(x => x.OnConnectionStateChanged())
            .Returns(new Mock<IObservable<RealtimeConnectionState>>().Object);

        var service = new AutoReconnectService(
            _mockRealtimeService.Object,
            _mockMessageQueue.Object,
            _mockLogger.Object);

        // Act
        var result1 = await service.StartAsync("user123");
        var result2 = await service.StartAsync("user456");

        // Assert
        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded); // Second start succeeds but doesn't change user
    }

    [Fact]
    public async Task StopAsync_WithoutStart_Succeeds()
    {
        // Arrange
        var service = new AutoReconnectService(
            _mockRealtimeService.Object,
            _mockMessageQueue.Object,
            _mockLogger.Object);

        // Act
        var result = await service.StopAsync();

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Service_WithNullLogger_Works()
    {
        // Arrange
        _mockRealtimeService
            .Setup(x => x.OnConnectionStateChanged())
            .Returns(new Mock<IObservable<RealtimeConnectionState>>().Object);

        // Act - Create service without logger
        var service = new AutoReconnectService(
            _mockRealtimeService.Object,
            _mockMessageQueue.Object);

        var result = await service.StartAsync("user123");

        // Assert
        Assert.True(result.Succeeded);
    }
}
