#if __IOS__
namespace SmartWorkz.Mobile.Services.Implementations;

using System;
using System.Threading.Tasks;
using SmartWorkz.Mobile.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// iOS platform-specific implementation of RealtimeService.
/// Provides background task management and VoIP push registration for real-time connectivity.
/// </summary>
public partial class RealtimeService
{
    private IiOSBackgroundTaskManager? _iosBackgroundTaskManager;
    private int _backgroundTaskId = -1;

    /// <summary>
    /// Set the iOS background task manager for this real-time service.
    /// This should be called during initialization to enable iOS-specific background handling.
    /// </summary>
    /// <param name="bgManager">The iOS background task manager instance.</param>
    public void SetiOSBackgroundTaskManager(IiOSBackgroundTaskManager bgManager)
    {
        _iosBackgroundTaskManager = Guard.NotNull(bgManager, nameof(bgManager));

        // Register for VoIP push immediately
        try
        {
            bgManager.RegisterVoIPPushAsync().GetAwaiter().GetResult();
            _logger.LogInformation("iOS background task manager initialized with VoIP push registration");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to register for VoIP push during iOS background task manager setup");
        }
    }

    /// <summary>
    /// Handle app moving to background by starting a background task extension.
    /// This keeps the real-time connection alive longer when the app moves to background.
    /// </summary>
    public async Task OnAppMovedToBackgroundAsync()
    {
        try
        {
            if (_iosBackgroundTaskManager != null)
            {
                _backgroundTaskId = _iosBackgroundTaskManager.BeginBackgroundTask("realtime_connection");

                if (_backgroundTaskId >= 0)
                {
                    _logger.LogDebug("App moved to background, started background task with ID {TaskId}", _backgroundTaskId);

                    // Schedule a background app refresh to maintain connectivity
                    var refreshResult = await _iosBackgroundTaskManager.ScheduleBackgroundAppRefreshAsync(TimeSpan.FromMinutes(15));
                    if (refreshResult.Succeeded)
                    {
                        _logger.LogDebug("Scheduled background app refresh for real-time connection maintenance");
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to begin background task for real-time connection");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling app moved to background");
        }
    }

    /// <summary>
    /// Handle app moving to foreground by ending the background task extension.
    /// </summary>
    public async Task OnAppMovedToForegroundAsync()
    {
        try
        {
            if (_iosBackgroundTaskManager != null && _backgroundTaskId >= 0)
            {
                var result = await _iosBackgroundTaskManager.EndBackgroundTaskAsync(_backgroundTaskId);

                if (result.Succeeded)
                {
                    _logger.LogDebug("App moved to foreground, ended background task");
                }
                else
                {
                    _logger.LogWarning("Failed to end background task: {Error}", result.Error);
                }

                _backgroundTaskId = -1;

                // Cancel background app refresh when in foreground
                var cancelResult = await _iosBackgroundTaskManager.CancelBackgroundAppRefreshAsync();
                if (cancelResult.Succeeded)
                {
                    _logger.LogDebug("Cancelled background app refresh in foreground");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling app moved to foreground");
        }
    }

    /// <summary>
    /// Get the remaining background execution time.
    /// Returns the approximate time remaining before the app is suspended.
    /// </summary>
    public TimeSpan GetRemainingBackgroundTime()
    {
        if (_iosBackgroundTaskManager != null)
        {
            return _iosBackgroundTaskManager.GetRemainingBackgroundTime();
        }

        return TimeSpan.Zero;
    }

    /// <summary>
    /// Check if the app is currently in the background.
    /// </summary>
    public bool IsAppInBackground()
    {
        if (_iosBackgroundTaskManager != null)
        {
            return _iosBackgroundTaskManager.IsInBackground;
        }

        return false;
    }
}
#endif
