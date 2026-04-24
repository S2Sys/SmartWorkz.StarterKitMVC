using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartWorkz.Windows.Services;
using SmartWorkz.Windows.ViewModels;
using Xunit;

namespace SmartWorkz.Windows.Tests;

public class SyncViewModelTests : IDisposable
{
    private readonly Mock<IBackgroundSyncService> _syncServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<SyncViewModel>> _loggerMock;
    private readonly SyncViewModel _sut;

    public SyncViewModelTests()
    {
        _syncServiceMock = new Mock<IBackgroundSyncService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<SyncViewModel>>();

        // Setup default mock behavior
        _syncServiceMock.Setup(s => s.GetStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncStatus(false, 0, 0, null, null));

        _syncServiceMock.Setup(s => s.SyncStatusChanged)
            .Returns(new System.Reactive.Subjects.Subject<SyncStatusChanged>());

        _notificationServiceMock.Setup(n => n.ShowToastAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new SyncViewModel(_syncServiceMock.Object, _notificationServiceMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Test that starting sync updates IsSyncing property
    /// </summary>
    [Fact]
    public async Task StartSyncAsync_ShouldUpdateIsSyncingProperty()
    {
        // Arrange
        var propertyChanged = false;
        _sut.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SyncViewModel.IsSyncing))
            {
                propertyChanged = true;
            }
        };

        _syncServiceMock.Setup(s => s.GetStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncStatus(true, 0, 0, null, null));

        // Act
        await _sut.StartSyncAsync();

        // Assert
        propertyChanged.Should().BeTrue();
        _syncServiceMock.Verify(s => s.StartSyncAsync(It.IsAny<SyncOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Test that stopping sync updates IsSyncing property
    /// </summary>
    [Fact]
    public async Task StopSyncAsync_ShouldUpdateIsSyncingProperty()
    {
        // Arrange
        _sut.IsSyncing = true;

        _syncServiceMock.Setup(s => s.GetStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncStatus(false, 0, 0, null, null));

        // Act
        await _sut.StopSyncAsync();

        // Assert
        _sut.IsSyncing.Should().BeFalse();
        _syncServiceMock.Verify(s => s.StopSyncAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Test that queuing an item works
    /// </summary>
    [Fact]
    public async Task QueueItemAsync_ShouldQueueItem()
    {
        // Arrange
        var itemId = "item-123";

        // Act
        await _sut.QueueItemAsync(itemId);

        // Assert
        _syncServiceMock.Verify(s => s.QueueSyncAsync(itemId, It.IsAny<CancellationToken>()), Times.Once);
        _sut.SyncLog.Count.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Test that refresh status updates properties
    /// </summary>
    [Fact]
    public async Task RefreshStatusAsync_ShouldUpdateProperties()
    {
        // Arrange
        var lastSyncTime = DateTime.UtcNow;
        _syncServiceMock.Setup(s => s.GetStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncStatus(true, 2, 5, lastSyncTime, null));

        // Act
        await _sut.RefreshStatusAsync();

        // Assert
        _sut.IsSyncing.Should().BeTrue();
        _sut.ItemsQueued.Should().Be(2);
        _sut.ItemsSynced.Should().Be(5);
        _sut.LastSyncTime.Should().NotBeNullOrEmpty();
    }

    public void Dispose()
    {
        _sut?.Dispose();
    }
}
