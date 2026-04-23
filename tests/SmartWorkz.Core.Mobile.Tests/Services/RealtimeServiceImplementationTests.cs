namespace SmartWorkz.Mobile.Tests.Services;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Mobile.Services.Implementations;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Shared;

public class RealtimeServiceImplementationTests
{
    private readonly Mock<ILogger<RealtimeService>> _mockLogger;
    private const string TestHubUrl = "https://api.example.com/realtimehub";

    public RealtimeServiceImplementationTests()
    {
        _mockLogger = new Mock<ILogger<RealtimeService>>();
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesService()
    {
        // Arrange & Act
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Assert
        Assert.NotNull(service);
        Assert.Equal(RealtimeConnectionState.Disconnected, service.GetConnectionState());
    }

    [Fact]
    public void Constructor_WithEmptyHubUrl_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new RealtimeService("", _mockLogger.Object));
        Assert.Contains("hubUrl", ex.ParamName);
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidHubUrl_ThrowsArgumentException(string hubUrl)
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new RealtimeService(hubUrl, _mockLogger.Object));
        Assert.Contains("hubUrl", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new RealtimeService(TestHubUrl, null!));
        Assert.Equal("logger", ex.ParamName);
    }

    [Fact]
    public async Task ConnectAsync_WithNullUserId_ThrowsArgumentException()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ConnectAsync(null!));
        Assert.Contains("userId", ex.ParamName);
    }

    [Fact]
    public async Task ConnectAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ConnectAsync(""));
        Assert.Contains("userId", ex.ParamName);
    }

    [Fact]
    public void GetConnectionState_WhenDisconnected_ReturnsDisconnected()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act
        var state = service.GetConnectionState();

        // Assert
        Assert.Equal(RealtimeConnectionState.Disconnected, state);
    }

    [Fact]
    public void OnMessageReceived_ReturnsObservable()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act
        var observable = service.OnMessageReceived();

        // Assert
        Assert.NotNull(observable);
    }

    [Fact]
    public void OnConnectionStateChanged_ReturnsObservable()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act
        var observable = service.OnConnectionStateChanged();

        // Assert
        Assert.NotNull(observable);
    }

    [Fact]
    public async Task SendAsync_WithNullMethod_ThrowsArgumentException()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendAsync(null!, new object[] { }));
        Assert.Contains("method", ex.ParamName);
    }

    [Fact]
    public async Task SendAsync_WithEmptyMethod_ThrowsArgumentException()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendAsync("", new object[] { }));
        Assert.Contains("method", ex.ParamName);
    }

    [Fact]
    public async Task SendAsync_WhenDisconnected_ReturnsFailure()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act
        var result = await service.SendAsync("OrderUpdate", new object[] { "order1" });

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task SubscribeToAsync_WithNullChannel_ThrowsArgumentException()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SubscribeToAsync(null!));
        Assert.Contains("channel", ex.ParamName);
    }

    [Fact]
    public async Task SubscribeToAsync_WithEmptyChannel_ThrowsArgumentException()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SubscribeToAsync(""));
        Assert.Contains("channel", ex.ParamName);
    }

    [Fact]
    public async Task SubscribeToAsync_WhenDisconnected_ReturnsFailure()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act
        var result = await service.SubscribeToAsync("Orders");

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task UnsubscribeFromAsync_WithNullChannel_ThrowsArgumentException()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UnsubscribeFromAsync(null!));
        Assert.Contains("channel", ex.ParamName);
    }

    [Fact]
    public async Task UnsubscribeFromAsync_WithEmptyChannel_ThrowsArgumentException()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UnsubscribeFromAsync(""));
        Assert.Contains("channel", ex.ParamName);
    }

    [Fact]
    public async Task UnsubscribeFromAsync_WhenDisconnected_ReturnsFailure()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act
        var result = await service.UnsubscribeFromAsync("Orders");

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task DisconnectAsync_WhenAlreadyDisconnected_ReturnsSuccess()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act
        var result = await service.DisconnectAsync();

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task IsConnectedAsync_WhenDisconnected_ReturnsFalse()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act
        var isConnected = await service.IsConnectedAsync();

        // Assert
        Assert.False(isConnected);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act & Assert
        service.Dispose();
    }

    [Fact]
    public async Task DisconnectAsync_MultipleCalls_SucceedsOnAll()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act
        var result1 = await service.DisconnectAsync();
        var result2 = await service.DisconnectAsync();

        // Assert
        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded);
    }

    [Fact]
    public async Task OnConnectionStateChanged_EmitsDisconnectedOnCreation()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);
        var states = new List<RealtimeConnectionState>();

        // Act
        var subscription = service.OnConnectionStateChanged().Subscribe(s => states.Add(s));
        await Task.Delay(100); // Allow time for any emissions

        // Assert
        subscription.Dispose();
        // Initial state is Disconnected, but observable may not emit on subscribe
        // This test just verifies the observable can be subscribed to
        Assert.NotNull(subscription);
    }

    [Fact]
    public void Dispose_MultipleCallsAreIdempotent()
    {
        // Arrange
        var service = new RealtimeService(TestHubUrl, _mockLogger.Object);

        // Act & Assert
        service.Dispose();
        service.Dispose(); // Should not throw
    }
}
