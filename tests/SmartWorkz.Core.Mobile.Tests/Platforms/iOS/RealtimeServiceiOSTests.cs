#if __IOS__
namespace SmartWorkz.Mobile.Tests.Platforms.iOS;

using Moq;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Shared;
using System;
using System.Threading.Tasks;
using Xunit;

public class iOSBackgroundTaskManagerTests
{
    private ILogger<iOSBackgroundTaskManager>? CreateMockLogger()
    {
        return Mock.Of<ILogger<iOSBackgroundTaskManager>>();
    }

    [Fact]
    public void BeginBackgroundTask_ReturnsValidTaskId()
    {
        // Arrange
        var manager = new iOSBackgroundTaskManager();

        // Act
        var taskId = manager.BeginBackgroundTask("connectivity");

        // Assert
        Assert.True(taskId >= 0);
    }

    [Fact]
    public async Task EndBackgroundTaskAsync_WithValidId_Succeeds()
    {
        // Arrange
        var manager = new iOSBackgroundTaskManager();
        var taskId = manager.BeginBackgroundTask("connectivity");

        // Act
        var result = await manager.EndBackgroundTaskAsync(taskId);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task RegisterVoIPPushAsync_RequestsPermissions()
    {
        // Arrange
        var manager = new iOSBackgroundTaskManager();

        // Act
        var result = await manager.RegisterVoIPPushAsync();

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task UnregisterVoIPPushAsync_DisablesPush()
    {
        // Arrange
        var manager = new iOSBackgroundTaskManager();
        await manager.RegisterVoIPPushAsync();

        // Act
        var result = await manager.UnregisterVoIPPushAsync();

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task RequestBackgroundAppRefreshPermissionAsync_ReturnsBool()
    {
        // Arrange
        var manager = new iOSBackgroundTaskManager();

        // Act
        var result = await manager.RequestBackgroundAppRefreshPermissionAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.IsType<bool>(result.Data);
    }

    [Fact]
    public async Task ScheduleBackgroundAppRefreshAsync_WithValidInterval_Succeeds()
    {
        // Arrange
        var manager = new iOSBackgroundTaskManager();

        // Act
        var result = await manager.ScheduleBackgroundAppRefreshAsync(TimeSpan.FromMinutes(15));

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task CancelBackgroundAppRefreshAsync_Succeeds()
    {
        // Arrange
        var manager = new iOSBackgroundTaskManager();
        await manager.ScheduleBackgroundAppRefreshAsync(TimeSpan.FromMinutes(15));

        // Act
        var result = await manager.CancelBackgroundAppRefreshAsync();

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void GetRemainingBackgroundTime_ReturnsTimeSpan()
    {
        // Arrange
        var manager = new iOSBackgroundTaskManager();

        // Act
        var remaining = manager.GetRemainingBackgroundTime();

        // Assert
        Assert.IsType<TimeSpan>(remaining);
        Assert.True(remaining >= TimeSpan.Zero);
    }
}
#endif
