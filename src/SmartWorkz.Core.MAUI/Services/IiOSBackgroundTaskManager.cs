namespace SmartWorkz.Mobile.Services;

using System;
using System.Threading.Tasks;
using SmartWorkz.Shared;

/// <summary>
/// Manages iOS background tasks for real-time connectivity.
/// </summary>
public interface IiOSBackgroundTaskManager
{
    /// <summary>
    /// Begin background task execution (extends app lifetime in background).
    /// </summary>
    int BeginBackgroundTask(string taskName);

    /// <summary>
    /// End background task when complete.
    /// </summary>
    Task<Result> EndBackgroundTaskAsync(int taskId);

    /// <summary>
    /// Register for VoIP push notifications.
    /// </summary>
    Task<Result> RegisterVoIPPushAsync();

    /// <summary>
    /// Unregister from VoIP push notifications.
    /// </summary>
    Task<Result> UnregisterVoIPPushAsync();

    /// <summary>
    /// Request background app refresh permission.
    /// </summary>
    Task<Result<bool>> RequestBackgroundAppRefreshPermissionAsync();

    /// <summary>
    /// Schedule background app refresh (typically 15+ minutes).
    /// </summary>
    Task<Result> ScheduleBackgroundAppRefreshAsync(TimeSpan minInterval);

    /// <summary>
    /// Cancel scheduled background app refresh.
    /// </summary>
    Task<Result> CancelBackgroundAppRefreshAsync();

    /// <summary>
    /// Check if app is in background.
    /// </summary>
    bool IsInBackground { get; }

    /// <summary>
    /// Get remaining background time (approximately).
    /// </summary>
    TimeSpan GetRemainingBackgroundTime();
}
