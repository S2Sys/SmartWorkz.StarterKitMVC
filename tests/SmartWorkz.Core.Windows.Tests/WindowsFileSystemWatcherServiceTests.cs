using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartWorkz.Windows.Services;
using Xunit;

namespace SmartWorkz.Windows.Tests;

public class WindowsFileSystemWatcherServiceTests : IDisposable
{
    private readonly WindowsFileSystemWatcherService _sut;
    private readonly Mock<ILogger<WindowsFileSystemWatcherService>> _loggerMock;
    private readonly string _tempPath;

    public WindowsFileSystemWatcherServiceTests()
    {
        _loggerMock = new Mock<ILogger<WindowsFileSystemWatcherService>>();
        _sut = new WindowsFileSystemWatcherService(_loggerMock.Object);
        _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempPath);
    }

    /// <summary>
    /// Test that starting watcher with valid path succeeds
    /// </summary>
    [Fact]
    public async Task StartWatchingAsync_WithValidPath_ShouldSucceed()
    {
        // Act
        await _sut.StartWatchingAsync(_tempPath);

        // Assert
        _sut.CurrentWatchPath.Should().Be(_tempPath);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("File system watcher started")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Test that starting watcher with invalid path throws
    /// </summary>
    [Fact]
    public async Task StartWatchingAsync_WithInvalidPath_ShouldThrow()
    {
        // Arrange
        var invalidPath = Path.Combine(Path.GetTempPath(), "NonExistentPath" + Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => _sut.StartWatchingAsync(invalidPath));
    }

    /// <summary>
    /// Test that stopping watcher succeeds
    /// </summary>
    [Fact]
    public async Task StopWatchingAsync_AfterStarting_ShouldSucceed()
    {
        // Arrange
        await _sut.StartWatchingAsync(_tempPath);

        // Act
        await _sut.StopWatchingAsync();

        // Assert
        _sut.CurrentWatchPath.Should().BeNull();
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("File system watcher stopped")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Test that file system changes are emitted as events
    /// </summary>
    [Fact]
    public async Task FileSystemChanged_WhenFileCreated_ShouldEmitEvent()
    {
        // Arrange
        var changeReceived = false;
        var subscription = _sut.FileSystemChanged.Subscribe(change =>
        {
            if (change.ChangeType == FileSystemChangeType.Created)
            {
                changeReceived = true;
            }
        });

        await _sut.StartWatchingAsync(_tempPath);

        // Act - Create a test file
        var testFile = Path.Combine(_tempPath, "test.txt");
        File.WriteAllText(testFile, "test content");

        // Wait for event to be processed
        await Task.Delay(500);

        // Assert
        changeReceived.Should().BeTrue();

        // Cleanup
        File.Delete(testFile);
        subscription.Dispose();
    }

    public void Dispose()
    {
        _sut?.Dispose();
        try
        {
            Directory.Delete(_tempPath, true);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
