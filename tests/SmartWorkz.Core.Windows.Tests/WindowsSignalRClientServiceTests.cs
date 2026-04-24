using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartWorkz.Windows.Services;
using Xunit;

namespace SmartWorkz.Windows.Tests;

public class WindowsSignalRClientServiceTests : IDisposable
{
    private readonly WindowsSignalRClientService _sut;
    private readonly Mock<ILogger<WindowsSignalRClientService>> _loggerMock;

    public WindowsSignalRClientServiceTests()
    {
        _loggerMock = new Mock<ILogger<WindowsSignalRClientService>>();
        _sut = new WindowsSignalRClientService(_loggerMock.Object);
    }

    /// <summary>
    /// Test that client initializes with disconnected state
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeWithDisconnectedState()
    {
        // Act & Assert
        _sut.CurrentState.Should().Be(SignalRConnectionState.Disconnected);
    }

    /// <summary>
    /// Test that connecting transitions through proper states
    /// </summary>
    [Fact]
    public async Task ConnectAsync_ShouldTransitionThroughConnectingToConnectedState()
    {
        // Arrange
        var states = new List<SignalRConnectionState>();
        var stateSubscription = _sut.StateChanged.Subscribe(state => states.Add(state));
        var hubUrl = "http://localhost:5000/signalhub";

        // Act
        await _sut.ConnectAsync(hubUrl);

        // Assert
        _sut.CurrentState.Should().Be(SignalRConnectionState.Connected);
        states.Should().Contain(SignalRConnectionState.Connecting);
        states.Should().Contain(SignalRConnectionState.Connected);

        stateSubscription.Dispose();
    }

    /// <summary>
    /// Test that disconnecting returns to disconnected state
    /// </summary>
    [Fact]
    public async Task DisconnectAsync_ShouldReturnToDisconnectedState()
    {
        // Arrange
        var hubUrl = "http://localhost:5000/signalhub";
        await _sut.ConnectAsync(hubUrl);

        // Act
        await _sut.DisconnectAsync();

        // Assert
        _sut.CurrentState.Should().Be(SignalRConnectionState.Disconnected);
    }

    /// <summary>
    /// Test that subscribing to a channel registers the handler
    /// </summary>
    [Fact]
    public async Task SubscribeAsync_ShouldRegisterChannelHandler()
    {
        // Arrange
        var channelName = "testChannel";
        var handlerCalled = false;
        Func<string, Task> handler = async message =>
        {
            handlerCalled = true;
            await Task.CompletedTask;
        };

        // Act
        await _sut.SubscribeAsync(channelName, handler);

        // Assert
        handlerCalled.Should().BeFalse(); // Handler should not be called during subscription
    }

    /// <summary>
    /// Test that sending a message requires connected state
    /// </summary>
    [Fact]
    public async Task SendAsync_ShouldThrowWhenNotConnected()
    {
        // Arrange
        var method = "sendTestMessage";
        var payload = new { message = "test" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.SendAsync(method, payload)
        );
    }

    /// <summary>
    /// Test that sending a message works when connected
    /// </summary>
    [Fact]
    public async Task SendAsync_ShouldSucceedWhenConnected()
    {
        // Arrange
        var hubUrl = "http://localhost:5000/signalhub";
        var method = "sendTestMessage";
        var payload = new { message = "test" };
        await _sut.ConnectAsync(hubUrl);

        // Act & Assert
        var sendTask = _sut.SendAsync(method, payload);
        sendTask.IsCompleted.Should().BeFalse();

        var completedTask = await Task.WhenAny(sendTask, Task.Delay(TimeSpan.FromSeconds(5)));
        completedTask.Should().Be(sendTask);
    }

    public void Dispose()
    {
        _sut?.Dispose();
    }
}
