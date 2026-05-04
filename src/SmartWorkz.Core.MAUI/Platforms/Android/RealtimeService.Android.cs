#if __ANDROID__
namespace SmartWorkz.Mobile.Services.Implementations;

using SmartWorkz.Mobile.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

/// <summary>
/// Android-specific platform overrides for RealtimeService.
/// Manages wake locks and background task lifecycle.
/// </summary>
public partial class RealtimeService
{
    private IBackgroundTaskManager? _backgroundTaskManager;

    /// <summary>
    /// Set the Android background task manager for managing wake locks and background tasks.
    /// </summary>
    /// <param name="bgManager">The background task manager instance.</param>
    public void SetAndroidBackgroundTaskManager(IBackgroundTaskManager bgManager)
    {
        Guard.NotNull(bgManager, nameof(bgManager));

        _backgroundTaskManager = bgManager;

        // Acquire wake lock when real-time service is initialized on Android
        // This prevents the CPU from sleeping while maintaining a real-time connection
        var result = bgManager.AcquireWakeLockAsync("realtime_signal")
            .GetAwaiter()
            .GetResult();

        if (result.Succeeded)
        {
            _logger.LogInformation("Acquired wake lock for real-time connectivity");
        }
        else
        {
            _logger.LogWarning("Failed to acquire wake lock for real-time connectivity: {Error}", result.Error?.Message);
        }
    }

    /// <summary>
    /// Called when the connection is successfully established.
    /// Updates lifecycle-aware behavior for background/foreground states.
    /// </summary>
    private async Task OnConnectedAsyncPlatform()
    {
        if (_backgroundTaskManager == null)
            return;

        try
        {
            var lifecycleState = _backgroundTaskManager.GetLifecycleState();

            if (_backgroundTaskManager.IsInForeground)
            {
                _logger.LogDebug("Real-time connection established while app is in foreground");
            }
            else
            {
                _logger.LogDebug("Real-time connection established while app is in background");
                // Register periodic health check to maintain connection in background
                await RegisterBackgroundHealthCheckAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error updating platform-specific connection state");
        }
    }

    /// <summary>
    /// Called when the connection is disconnected.
    /// Cleanup platform-specific resources.
    /// </summary>
    private async Task OnDisconnectedAsyncPlatform()
    {
        if (_backgroundTaskManager == null)
            return;

        try
        {
            // Release wake lock when disconnected to conserve battery
            var result = await _backgroundTaskManager.ReleaseWakeLockAsync();
            if (result.Succeeded)
            {
                _logger.LogInformation("Released wake lock after real-time disconnection");
            }

            // Unregister background health check
            await UnregisterBackgroundHealthCheckAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cleaning up platform-specific resources");
        }
    }

    /// <summary>
    /// Register a periodic background health check task.
    /// </summary>
    private async Task RegisterBackgroundHealthCheckAsync()
    {
        if (_backgroundTaskManager == null)
            return;

        try
        {
            var result = await _backgroundTaskManager.RegisterBackgroundTaskAsync(
                "realtime_health_check",
                TimeSpan.FromMinutes(15));

            if (result.Succeeded)
            {
                _logger.LogInformation("Registered background health check task");
            }
            else
            {
                _logger.LogWarning("Failed to register background health check: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error registering background health check");
        }
    }

    /// <summary>
    /// Unregister the periodic background health check task.
    /// </summary>
    private async Task UnregisterBackgroundHealthCheckAsync()
    {
        if (_backgroundTaskManager == null)
            return;

        try
        {
            var result = await _backgroundTaskManager.UnregisterBackgroundTaskAsync("realtime_health_check");
            if (result.Succeeded)
            {
                _logger.LogInformation("Unregistered background health check task");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error unregistering background health check");
        }
    }

    /// <summary>
    /// Request high-priority notification permission for real-time alerts.
    /// </summary>
    public async Task<Result<bool>> RequestNotificationPermissionAsync()
    {
        if (_backgroundTaskManager == null)
        {
            return Result.Fail<bool>("NOTIF.MANAGER_NOT_SET", "Background task manager not initialized");
        }

        try
        {
            var result = await _backgroundTaskManager.RequestNotificationPermissionAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting notification permission");
            return Result.Fail<bool>(Error.FromException(ex, "NOTIF.REQUEST_FAILED"));
        }
    }
}
#endif
