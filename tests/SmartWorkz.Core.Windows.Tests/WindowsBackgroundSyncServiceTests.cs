using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartWorkz.Windows.Services;
using Xunit;

namespace SmartWorkz.Windows.Tests;

public class WindowsBackgroundSyncServiceTests : IDisposable
{
    private readonly WindowsBackgroundSyncService _sut;
    private readonly Mock<ILogger<WindowsBackgroundSyncService>> _loggerMock;

    public WindowsBackgroundSyncServiceTests()
    {
        _loggerMock = new Mock<ILogger<WindowsBackgroundSyncService>>();
        _sut = new WindowsBackgroundSyncService(_loggerMock.Object);
    }

    /// <summary>
    /// Test that starting sync service succeeds
    /// </summary>
    [Fact]
    public async Task StartSyncAsync_ShouldStartService()
    {
        // Arrange
        var options = new SyncOptions { SyncIntervalSeconds = 1 };

        // Act
        await _sut.StartSyncAsync(options);

        // Assert
        var status = await _sut.GetStatusAsync();
        status.IsRunning.Should().BeTrue();

        // Cleanup
        await _sut.StopSyncAsync();
    }

    /// <summary>
    /// Test that queuing items works
    /// </summary>
    [Fact]
    public async Task QueueSyncAsync_ShouldQueueItem()
    {
        // Arrange
        var itemId = "item-123";
        var options = new SyncOptions { SyncIntervalSeconds = 1 };
        await _sut.StartSyncAsync(options);

        // Act
        await _sut.QueueSyncAsync(itemId);

        // Assert
        var status = await _sut.GetStatusAsync();
        status.ItemsQueued.Should().Be(1);

        // Cleanup
        await _sut.StopSyncAsync();
    }

    /// <summary>
    /// Test that sync status changes are emitted
    /// </summary>
    [Fact]
    public async Task SyncStatusChanged_WhenStarted_ShouldEmitEvent()
    {
        // Arrange
        var statusChangeReceived = false;
        var subscription = _sut.SyncStatusChanged.Subscribe(change =>
        {
            if (change.IsRunning)
            {
                statusChangeReceived = true;
            }
        });

        // Act
        var options = new SyncOptions { SyncIntervalSeconds = 1 };
        await _sut.StartSyncAsync(options);

        // Assert
        await Task.Delay(200);
        statusChangeReceived.Should().BeTrue();

        // Cleanup
        await _sut.StopSyncAsync();
        subscription.Dispose();
    }

    /// <summary>
    /// Test that stopping sync service works
    /// </summary>
    [Fact]
    public async Task StopSyncAsync_ShouldStopService()
    {
        // Arrange
        var options = new SyncOptions { SyncIntervalSeconds = 1 };
        await _sut.StartSyncAsync(options);

        // Act
        await _sut.StopSyncAsync();

        // Assert
        var status = await _sut.GetStatusAsync();
        status.IsRunning.Should().BeFalse();
    }

    public void Dispose()
    {
        _sut?.Dispose();
    }
}
