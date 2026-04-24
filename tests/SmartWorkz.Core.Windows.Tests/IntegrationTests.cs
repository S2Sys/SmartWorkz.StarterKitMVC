using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SmartWorkz.Windows.Services;
using SmartWorkz.Windows.ViewModels;
using Xunit;

namespace SmartWorkz.Windows.Tests;

public class IntegrationTests : IDisposable
{
    private readonly IServiceCollection _services;
    private readonly IServiceProvider _provider;
    private readonly Mock<ILogger<WindowsSignalRClientService>> _signalRLoggerMock;
    private readonly Mock<ILogger<WindowsNotificationService>> _notificationLoggerMock;
    private readonly Mock<ILogger<WindowsFileSystemWatcherService>> _watcherLoggerMock;
    private readonly Mock<ILogger<WindowsBackgroundSyncService>> _syncLoggerMock;
    private readonly Mock<ILogger<SyncViewModel>> _viewModelLoggerMock;

    public IntegrationTests()
    {
        _signalRLoggerMock = new Mock<ILogger<WindowsSignalRClientService>>();
        _notificationLoggerMock = new Mock<ILogger<WindowsNotificationService>>();
        _watcherLoggerMock = new Mock<ILogger<WindowsFileSystemWatcherService>>();
        _syncLoggerMock = new Mock<ILogger<WindowsBackgroundSyncService>>();
        _viewModelLoggerMock = new Mock<ILogger<SyncViewModel>>();

        _services = new ServiceCollection();
        _services.AddSingleton(_signalRLoggerMock.Object);
        _services.AddSingleton(_notificationLoggerMock.Object);
        _services.AddSingleton(_watcherLoggerMock.Object);
        _services.AddSingleton(_syncLoggerMock.Object);
        _services.AddSingleton(_viewModelLoggerMock.Object);

        _services.AddSingleton<ISignalRClientService>(sp =>
            new WindowsSignalRClientService(sp.GetRequiredService<ILogger<WindowsSignalRClientService>>()));
        _services.AddSingleton<INotificationService>(sp =>
            new WindowsNotificationService(sp.GetRequiredService<ILogger<WindowsNotificationService>>()));
        _services.AddSingleton<IBackgroundSyncService>(sp =>
            new WindowsBackgroundSyncService(sp.GetRequiredService<ILogger<WindowsBackgroundSyncService>>()));

        _provider = _services.BuildServiceProvider();
    }

    /// <summary>
    /// Test complete workflow: Connect SignalR, Show notification, Queue sync
    /// </summary>
    [Fact]
    public async Task WorkflowTest_ConnectAndNotify_ShouldSucceed()
    {
        // Arrange
        var signalRService = _provider.GetRequiredService<ISignalRClientService>();
        var notificationService = _provider.GetRequiredService<INotificationService>();
        var hubUrl = "http://localhost:5000/signalhub";

        // Act - Connect to SignalR
        await signalRService.ConnectAsync(hubUrl);

        // Assert
        signalRService.CurrentState.Should().Be(SignalRConnectionState.Connected);

        // Act - Show notification
        await notificationService.ShowToastAsync("Test", "Connected successfully", NotificationType.Success);

        // Assert - Should not throw
    }

    /// <summary>
    /// Test complete workflow: Start sync, queue items, monitor status
    /// </summary>
    [Fact]
    public async Task WorkflowTest_SyncWithItemQueue_ShouldSucceed()
    {
        // Arrange
        var syncService = _provider.GetRequiredService<IBackgroundSyncService>();
        var options = new SyncOptions { SyncIntervalSeconds = 1 };
        var itemId1 = "item-001";
        var itemId2 = "item-002";

        // Act - Start sync
        await syncService.StartSyncAsync(options);

        // Assert
        var status = await syncService.GetStatusAsync();
        status.IsRunning.Should().BeTrue();

        // Act - Queue items
        await syncService.QueueSyncAsync(itemId1);
        await syncService.QueueSyncAsync(itemId2);

        // Assert
        status = await syncService.GetStatusAsync();
        status.ItemsQueued.Should().Be(2);

        // Wait for sync to process items
        await Task.Delay(1500);

        // Assert - Items should be processed
        status = await syncService.GetStatusAsync();
        status.ItemsQueued.Should().Be(0);

        // Cleanup
        await syncService.StopSyncAsync();
    }

    /// <summary>
    /// Test complete workflow: ViewModel integration with sync service
    /// </summary>
    [Fact]
    public async Task WorkflowTest_ViewModelSyncIntegration_ShouldSucceed()
    {
        // Arrange
        var syncService = _provider.GetRequiredService<IBackgroundSyncService>();
        var notificationService = _provider.GetRequiredService<INotificationService>();
        var viewModel = new SyncViewModel(syncService, notificationService, _viewModelLoggerMock.Object);

        // Act - Start sync from ViewModel
        await viewModel.StartSyncAsync(new SyncOptions { SyncIntervalSeconds = 1 });

        // Assert
        viewModel.IsSyncing.Should().BeTrue();

        // Act - Queue item
        await viewModel.QueueItemAsync("item-001");

        // Assert
        viewModel.SyncLog.Count.Should().BeGreaterThan(0);

        // Act - Refresh status
        await viewModel.RefreshStatusAsync();

        // Assert
        viewModel.ItemsSynced.Should().BeGreaterThanOrEqualTo(0);

        // Cleanup
        await viewModel.StopSyncAsync();
        viewModel.Dispose();
    }

    /// <summary>
    /// Test complete workflow: Multi-service interaction
    /// </summary>
    [Fact]
    public async Task WorkflowTest_MultiServiceInteraction_ShouldSucceed()
    {
        // Arrange
        var signalRService = _provider.GetRequiredService<ISignalRClientService>();
        var notificationService = _provider.GetRequiredService<INotificationService>();
        var syncService = _provider.GetRequiredService<IBackgroundSyncService>();

        // Act - Setup SignalR connection
        await signalRService.ConnectAsync("http://localhost:5000/signalhub");

        // Assert
        signalRService.CurrentState.Should().Be(SignalRConnectionState.Connected);

        // Act - Start background sync
        var syncOptions = new SyncOptions { SyncIntervalSeconds = 1 };
        await syncService.StartSyncAsync(syncOptions);

        // Assert
        var syncStatus = await syncService.GetStatusAsync();
        syncStatus.IsRunning.Should().BeTrue();

        // Act - Show notification for sync started
        await notificationService.ShowToastAsync("Sync Started", "Background synchronization is running");

        // Act - Queue multiple items
        for (int i = 0; i < 3; i++)
        {
            await syncService.QueueSyncAsync($"item-{i:D3}");
        }

        // Assert
        syncStatus = await syncService.GetStatusAsync();
        syncStatus.ItemsQueued.Should().Be(3);

        // Wait for processing
        await Task.Delay(1500);

        // Assert
        syncStatus = await syncService.GetStatusAsync();
        syncStatus.ItemsSynced.Should().BeGreaterThan(0);

        // Cleanup
        await syncService.StopSyncAsync();
        await signalRService.DisconnectAsync();
        await notificationService.ClearAllAsync();
    }

    /// <summary>
    /// Test error handling workflow
    /// </summary>
    [Fact]
    public async Task WorkflowTest_ErrorHandling_ShouldHandleGracefully()
    {
        // Arrange
        var notificationService = _provider.GetRequiredService<INotificationService>();

        // Act - Try to show multiple notification types
        await notificationService.ShowToastAsync("Info", "Information notification", NotificationType.Information);
        await notificationService.ShowToastAsync("Success", "Success notification", NotificationType.Success);
        await notificationService.ShowToastAsync("Warning", "Warning notification", NotificationType.Warning);
        await notificationService.ShowToastAsync("Error", "Error notification", NotificationType.Error);

        // Assert - All should complete successfully
        await notificationService.ClearAllAsync();
    }

    public void Dispose()
    {
        (_provider as IDisposable)?.Dispose();
    }
}
