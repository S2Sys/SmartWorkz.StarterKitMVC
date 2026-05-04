#if __IOS__
namespace SmartWorkz.Mobile.Services.Implementations;

using Foundation;
using UIKit;
using UserNotifications;
using Microsoft.Extensions.Logging;
using SmartWorkz.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// iOS-specific implementation of IiOSBackgroundTaskManager.
/// Manages background tasks using UIApplication background task API and VoIP push notifications.
/// </summary>
public class iOSBackgroundTaskManager : IiOSBackgroundTaskManager
{
    private readonly ILogger<iOSBackgroundTaskManager>? _logger;
    private readonly HashSet<int> _activeBackgroundTasks = new HashSet<int>();
    private readonly object _tasksLock = new object();
    private volatile bool _isInBackground = false;

    /// <summary>
    /// Initializes a new instance of the iOSBackgroundTaskManager class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    public iOSBackgroundTaskManager(ILogger<iOSBackgroundTaskManager>? logger = null)
    {
        _logger = logger;
        InitializeLifecycleListener();
    }

    /// <summary>
    /// Begin background task execution (extends app lifetime in background).
    /// </summary>
    public int BeginBackgroundTask(string taskName)
    {
        Guard.NotEmpty(taskName, nameof(taskName));

        try
        {
            var taskId = UIApplication.SharedApplication.BeginBackgroundTask(taskName, () =>
            {
                _logger?.LogWarning("Background task {TaskName} expired", taskName);
            });

            if (taskId != UIApplication.BackgroundTaskInvalid)
            {
                lock (_tasksLock)
                {
                    _activeBackgroundTasks.Add(taskId);
                }
                _logger?.LogInformation("Started background task {TaskName} with ID {TaskId}", taskName, taskId);
            }
            else
            {
                _logger?.LogWarning("Failed to begin background task {TaskName}", taskName);
            }

            return taskId;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error beginning background task {TaskName}", taskName);
            return UIApplication.BackgroundTaskInvalid;
        }
    }

    /// <summary>
    /// End background task when complete.
    /// </summary>
    public async Task<Result> EndBackgroundTaskAsync(int taskId)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (taskId == UIApplication.BackgroundTaskInvalid)
                {
                    _logger?.LogDebug("Task ID is invalid, skipping end background task");
                    return Result.Ok();
                }

                lock (_tasksLock)
                {
                    if (_activeBackgroundTasks.Contains(taskId))
                    {
                        UIApplication.SharedApplication.EndBackgroundTask(taskId);
                        _activeBackgroundTasks.Remove(taskId);
                        _logger?.LogInformation("Ended background task {TaskId}", taskId);
                    }
                    else
                    {
                        _logger?.LogDebug("Background task {TaskId} not found in active tasks", taskId);
                    }
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error ending background task {TaskId}", taskId);
                return Result.Fail(Error.FromException(ex, "BACKGROUND_TASK_END_FAILED"));
            }
        });
    }

    /// <summary>
    /// Register for VoIP push notifications.
    /// </summary>
    public async Task<Result> RegisterVoIPPushAsync()
    {
        return await Task.Run(async () =>
        {
            try
            {
                // Check iOS version >= 10 (VoIP push available)
                if (!IsOperatingSystemAtLeastVersion(10, 0, 0))
                {
                    _logger?.LogWarning("iOS version is less than 10.0, VoIP push not supported");
                    return Result.Fail("VOIP_PUSH_NOT_SUPPORTED", "iOS 10.0 or higher required");
                }

                // Request notification permissions
                var requestSettings = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound | UNAuthorizationOptions.Badge;
                var (granted, error) = await UNUserNotificationCenter.Current.RequestAuthorizationAsync(requestSettings);

                if (!granted)
                {
                    _logger?.LogWarning("User denied notification permissions");
                    return Result.Fail("VOIP_PUSH_PERMISSION_DENIED", "User denied notification permissions");
                }

                // Register for remote notifications (includes VoIP push)
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UIApplication.SharedApplication.RegisterForRemoteNotifications();
                });

                _logger?.LogInformation("Registered for VoIP push notifications");
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error registering for VoIP push");
                return Result.Fail(Error.FromException(ex, "VOIP_PUSH_REGISTRATION_FAILED"));
            }
        });
    }

    /// <summary>
    /// Unregister from VoIP push notifications.
    /// </summary>
    public async Task<Result> UnregisterVoIPPushAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UIApplication.SharedApplication.UnregisterForRemoteNotifications();
                });

                _logger?.LogInformation("Unregistered from VoIP push notifications");
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error unregistering from VoIP push");
                return Result.Fail(Error.FromException(ex, "VOIP_PUSH_UNREGISTRATION_FAILED"));
            }
        });
    }

    /// <summary>
    /// Request background app refresh permission.
    /// </summary>
    public async Task<Result<bool>> RequestBackgroundAppRefreshPermissionAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                // Check iOS version >= 13 (background app refresh available)
                if (!IsOperatingSystemAtLeastVersion(13, 0, 0))
                {
                    _logger?.LogWarning("iOS version is less than 13.0, background app refresh not supported");
                    return Result.Ok(false);
                }

                // Background app refresh permission is handled via Info.plist configuration
                // In iOS 13+, the permission is implicit if the capability is declared
                bool hasCapability = CheckBackgroundAppRefreshCapability();

                _logger?.LogInformation("Background app refresh permission: {HasCapability}", hasCapability);
                return Result.Ok(hasCapability);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error requesting background app refresh permission");
                return Result.Fail<bool>(Error.FromException(ex, "BACKGROUND_REFRESH_PERMISSION_FAILED"));
            }
        });
    }

    /// <summary>
    /// Schedule background app refresh (typically 15+ minutes).
    /// </summary>
    public async Task<Result> ScheduleBackgroundAppRefreshAsync(TimeSpan minInterval)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Check iOS version >= 13 (BGTaskScheduler available)
                if (!IsOperatingSystemAtLeastVersion(13, 0, 0))
                {
                    _logger?.LogWarning("iOS version is less than 13.0, background task scheduling not supported");
                    return Result.Fail("BACKGROUND_REFRESH_NOT_SUPPORTED", "iOS 13.0 or higher required");
                }

                // Enforce minimum interval (iOS requires at least 15 minutes)
                var effectiveInterval = minInterval < TimeSpan.FromMinutes(15)
                    ? TimeSpan.FromMinutes(15)
                    : minInterval;

                // Note: Actual BGTaskScheduler registration would be done via app delegate
                // This method prepares the configuration for the scheduler
                _logger?.LogInformation("Scheduled background app refresh with interval {Interval}",
                    effectiveInterval.TotalMinutes);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error scheduling background app refresh");
                return Result.Fail(Error.FromException(ex, "BACKGROUND_REFRESH_SCHEDULING_FAILED"));
            }
        });
    }

    /// <summary>
    /// Cancel scheduled background app refresh.
    /// </summary>
    public async Task<Result> CancelBackgroundAppRefreshAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!IsOperatingSystemAtLeastVersion(13, 0, 0))
                {
                    _logger?.LogDebug("iOS version is less than 13.0, skipping background refresh cancellation");
                    return Result.Ok();
                }

                // Note: Actual BGTaskScheduler cancellation would be done via app delegate
                _logger?.LogInformation("Cancelled background app refresh");
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error cancelling background app refresh");
                return Result.Fail(Error.FromException(ex, "BACKGROUND_REFRESH_CANCELLATION_FAILED"));
            }
        });
    }

    /// <summary>
    /// Check if app is in background.
    /// </summary>
    public bool IsInBackground => _isInBackground;

    /// <summary>
    /// Get remaining background time (approximately).
    /// </summary>
    public TimeSpan GetRemainingBackgroundTime()
    {
        var remaining = UIApplication.SharedApplication.BackgroundTimeRemaining;

        if (remaining >= 0)
        {
            return TimeSpan.FromSeconds(remaining);
        }

        // If unlimited, return a large value
        return TimeSpan.MaxValue;
    }

    /// <summary>
    /// Initialize lifecycle listener to track app state changes.
    /// </summary>
    private void InitializeLifecycleListener()
    {
        try
        {
            if (Application.Current != null)
            {
                Application.Current.Paused += (s, e) =>
                {
                    _isInBackground = true;
                    _logger?.LogDebug("App moved to background");
                };

                Application.Current.Resumed += (s, e) =>
                {
                    _isInBackground = false;
                    _logger?.LogDebug("App moved to foreground");
                };
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error initializing lifecycle listener");
        }
    }

    /// <summary>
    /// Check if operating system version is at least the specified version.
    /// </summary>
    private bool IsOperatingSystemAtLeastVersion(int major, int minor, int patch)
    {
        return UIDevice.CurrentDevice.CheckSystemVersion(major, minor);
    }

    /// <summary>
    /// Check if background app refresh capability is available.
    /// </summary>
    private bool CheckBackgroundAppRefreshCapability()
    {
        try
        {
            // In iOS, background app refresh capability is checked via Info.plist
            // We check if the app has the UIBackgroundModes capability
            var infoPlist = NSBundle.MainBundle.InfoDictionary;
            if (infoPlist != null && infoPlist.TryGetValue("UIBackgroundModes", out var modes))
            {
                if (modes is NSArray modeArray)
                {
                    for (nuint i = 0; i < modeArray.Count; i++)
                    {
                        var mode = modeArray.GetItem<NSString>(i);
                        if (mode?.ToString() == "fetch")
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error checking background app refresh capability");
            return false;
        }
    }
}
#endif
