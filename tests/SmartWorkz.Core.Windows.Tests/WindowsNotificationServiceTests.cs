using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartWorkz.Windows.Services;
using Xunit;

namespace SmartWorkz.Windows.Tests;

public class WindowsNotificationServiceTests : IDisposable
{
    private readonly WindowsNotificationService _sut;
    private readonly Mock<ILogger<WindowsNotificationService>> _loggerMock;

    public WindowsNotificationServiceTests()
    {
        _loggerMock = new Mock<ILogger<WindowsNotificationService>>();
        _sut = new WindowsNotificationService(_loggerMock.Object);
    }

    /// <summary>
    /// Test that showing a toast notification with valid parameters succeeds
    /// </summary>
    [Fact]
    public async Task ShowToastAsync_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var title = "Test Notification";
        var message = "This is a test message";

        // Act
        var toastTask = _sut.ShowToastAsync(title, message);
        var completedTask = await Task.WhenAny(toastTask, Task.Delay(TimeSpan.FromSeconds(5)));

        // Assert
        completedTask.Should().Be(toastTask);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Toast notification shown")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Test that showing a toast with different notification types works
    /// </summary>
    [Theory]
    [InlineData(NotificationType.Information)]
    [InlineData(NotificationType.Success)]
    [InlineData(NotificationType.Warning)]
    [InlineData(NotificationType.Error)]
    public async Task ShowToastAsync_WithDifferentTypes_ShouldSucceed(NotificationType type)
    {
        // Arrange
        var title = "Test";
        var message = "Message";

        // Act
        await _sut.ShowToastAsync(title, message, type);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Toast notification shown")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Test that showing a dialog notification succeeds
    /// </summary>
    [Fact]
    public async Task ShowDialogAsync_ShouldShowDialog()
    {
        // Arrange
        var title = "Dialog Title";
        var message = "Dialog Message";

        // Act
        await _sut.ShowDialogAsync(title, message);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Dialog notification shown")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Test that clearing all notifications succeeds
    /// </summary>
    [Fact]
    public async Task ClearAllAsync_ShouldClearActiveNotifications()
    {
        // Arrange
        await _sut.ShowToastAsync("Title1", "Message1");
        await _sut.ShowToastAsync("Title2", "Message2");

        // Act
        await _sut.ClearAllAsync();

        // Assert - Should log clearing action
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cleared")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    public void Dispose()
    {
        _sut?.Dispose();
    }
}
