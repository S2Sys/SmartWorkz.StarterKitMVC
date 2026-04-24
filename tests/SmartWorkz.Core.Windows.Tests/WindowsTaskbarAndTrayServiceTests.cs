using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartWorkz.Windows.Services;
using Xunit;

namespace SmartWorkz.Windows.Tests;

public class WindowsTaskbarServiceTests : IDisposable
{
    private readonly WindowsTaskbarService _sut;
    private readonly Mock<ILogger<WindowsTaskbarService>> _loggerMock;

    public WindowsTaskbarServiceTests()
    {
        _loggerMock = new Mock<ILogger<WindowsTaskbarService>>();
        _sut = new WindowsTaskbarService(_loggerMock.Object);
    }

    /// <summary>
    /// Test that setting valid progress succeeds
    /// </summary>
    [Fact]
    public async Task SetProgressAsync_WithValidValues_ShouldSucceed()
    {
        // Arrange
        var current = 50;
        var total = 100;

        // Act
        await _sut.SetProgressAsync(current, total);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("progress set")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Test that setting progress with invalid values throws
    /// </summary>
    [Theory]
    [InlineData(-1, 100)]
    [InlineData(101, 100)]
    [InlineData(50, 0)]
    public async Task SetProgressAsync_WithInvalidValues_ShouldThrow(int current, int total)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.SetProgressAsync(current, total));
    }

    /// <summary>
    /// Test that setting badge works
    /// </summary>
    [Fact]
    public async Task SetBadgeAsync_WithValidBadge_ShouldSucceed()
    {
        // Arrange
        var badge = "5";

        // Act
        await _sut.SetBadgeAsync(badge);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("badge set")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public void Dispose()
    {
        _sut?.Dispose();
    }
}

public class WindowsSystemTrayServiceTests : IDisposable
{
    private readonly WindowsSystemTrayService _sut;
    private readonly Mock<ILogger<WindowsSystemTrayService>> _loggerMock;

    public WindowsSystemTrayServiceTests()
    {
        _loggerMock = new Mock<ILogger<WindowsSystemTrayService>>();
        _sut = new WindowsSystemTrayService(_loggerMock.Object);
    }

    /// <summary>
    /// Test that showing tray icon with valid tooltip succeeds
    /// </summary>
    [Fact]
    public async Task ShowIconAsync_WithValidTooltip_ShouldSucceed()
    {
        // Arrange
        var tooltip = "Application Running";

        // Act
        await _sut.ShowIconAsync(tooltip);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tray icon shown")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Test that hiding tray icon succeeds
    /// </summary>
    [Fact]
    public async Task HideIconAsync_ShouldHideIcon()
    {
        // Arrange
        await _sut.ShowIconAsync("Test");

        // Act
        await _sut.HideIconAsync();

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tray icon hidden")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Test that icon click event is published
    /// </summary>
    [Fact]
    public async Task IconClicked_WhenSimulated_ShouldEmitEvent()
    {
        // Arrange
        var clickReceived = false;
        var subscription = _sut.IconClicked.Subscribe(_ => clickReceived = true);

        // Act
        _sut.SimulateIconClick();
        await Task.Delay(100);

        // Assert
        clickReceived.Should().BeTrue();
        subscription.Dispose();
    }

    public void Dispose()
    {
        _sut?.Dispose();
    }
}
