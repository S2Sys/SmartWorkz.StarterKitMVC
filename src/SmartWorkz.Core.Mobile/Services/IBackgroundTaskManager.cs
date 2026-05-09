namespace SmartWorkz.Mobile.Services;

using System;
using System.Threading.Tasks;
using SmartWorkz.Shared;

/// <summary>
/// Manages Android background tasks for real-time connectivity.
/// </summary>
public interface IBackgroundTaskManager
{
    /// <summary>
    /// Acquire wake lock to prevent CPU sleep.
    /// </summary>
    /// <param name="tag">The tag to identify the wake lock. Default: "realtime_wakelock".</param>
    /// <returns>A Task representing the asynchronous operation, returning a Result indicating success or failure.</returns>
    Task<Result> AcquireWakeLockAsync(string tag = "realtime_wakelock");

    /// <summary>
    /// Release wake lock.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation, returning a Result indicating success or failure.</returns>
    Task<Result> ReleaseWakeLockAsync();

    /// <summary>
    /// Register background task for periodic health checks.
    /// </summary>
    /// <param name="taskName">The name of the background task to register.</param>
    /// <param name="interval">The interval at which the background task should execute.</param>
    /// <returns>A Task representing the asynchronous operation, returning a Result indicating success or failure.</returns>
    Task<Result> RegisterBackgroundTaskAsync(
        string taskName,
        TimeSpan interval);

    /// <summary>
    /// Unregister background task.
    /// </summary>
    /// <param name="taskName">The name of the background task to unregister.</param>
    /// <returns>A Task representing the asynchronous operation, returning a Result indicating success or failure.</returns>
    Task<Result> UnregisterBackgroundTaskAsync(string taskName);

    /// <summary>
    /// Get current app lifecycle state.
    /// </summary>
    /// <returns>The current app lifecycle state.</returns>
    AppLifecycleState GetLifecycleState();

    /// <summary>
    /// Request high-priority notifications permission.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation, returning a Result containing whether permission was granted.</returns>
    Task<Result<bool>> RequestNotificationPermissionAsync();

    /// <summary>
    /// Check if app is in foreground.
    /// </summary>
    bool IsInForeground { get; }
}

/// <summary>
/// Represents the application lifecycle states.
/// </summary>
public enum AppLifecycleState
{
    /// <summary>
    /// App is running and visible to the user.
    /// </summary>
    Foreground = 0,

    /// <summary>
    /// App is running but not visible to the user.
    /// </summary>
    Background = 1,

    /// <summary>
    /// App is suspended but not destroyed.
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// App has been destroyed.
    /// </summary>
    Destroyed = 3
}
