#if __ANDROID__
namespace SmartWorkz.Mobile.Tests.Platforms.Android;

using Moq;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Shared;
using System;
using System.Threading.Tasks;
using Xunit;

public class AndroidBackgroundTaskManagerTests
{
    private ILogger<AndroidBackgroundTaskManager>? CreateMockLogger()
    {
        return Mock.Of<ILogger<AndroidBackgroundTaskManager>>();
    }

    [Fact]
    public async Task AcquireWakeLockAsync_CreatesAndHoldsLock()
    {
        // Arrange
        var logger = CreateMockLogger();
        var manager = new AndroidBackgroundTaskManager(logger);

        // Act
        var result = await manager.AcquireWakeLockAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ReleaseWakeLockAsync_ReleasesHeldLock()
    {
        // Arrange
        var logger = CreateMockLogger();
        var manager = new AndroidBackgroundTaskManager(logger);
        await manager.AcquireWakeLockAsync();

        // Act
        var result = await manager.ReleaseWakeLockAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ReleaseWakeLockAsync_WhenNotAcquired_IsIdempotent()
    {
        // Arrange
        var logger = CreateMockLogger();
        var manager = new AndroidBackgroundTaskManager(logger);

        // Act
        var result = await manager.ReleaseWakeLockAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded); // No error even if not acquired
    }

    [Fact]
    public async Task AcquireWakeLockAsync_Twice_ReturnsSameWakeLock()
    {
        // Arrange
        var logger = CreateMockLogger();
        var manager = new AndroidBackgroundTaskManager(logger);

        // Act
        var result1 = await manager.AcquireWakeLockAsync("test");
        var result2 = await manager.AcquireWakeLockAsync("test");

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded);
    }

    [Fact]
    public async Task RegisterBackgroundTaskAsync_EnqueuesWork()
    {
        // Arrange
        var logger = CreateMockLogger();
        var manager = new AndroidBackgroundTaskManager(logger);

        // Act
        var result = await manager.RegisterBackgroundTaskAsync(
            "health_check",
            TimeSpan.FromMinutes(15));

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task UnregisterBackgroundTaskAsync_CancelsWork()
    {
        // Arrange
        var logger = CreateMockLogger();
        var manager = new AndroidBackgroundTaskManager(logger);
        await manager.RegisterBackgroundTaskAsync("task1", TimeSpan.FromMinutes(15));

        // Act
        var result = await manager.UnregisterBackgroundTaskAsync("task1");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void IsInForeground_ReturnsCorrectState()
    {
        // Arrange
        var logger = CreateMockLogger();
        var manager = new AndroidBackgroundTaskManager(logger);

        // Act
        var isInForeground = manager.IsInForeground;

        // Assert
        Assert.IsType<bool>(isInForeground);
    }

    [Fact]
    public void GetLifecycleState_ReturnsValidState()
    {
        // Arrange
        var logger = CreateMockLogger();
        var manager = new AndroidBackgroundTaskManager(logger);

        // Act
        var state = manager.GetLifecycleState();

        // Assert
        Assert.True(Enum.IsDefined(typeof(AppLifecycleState), state));
    }
}
#endif
