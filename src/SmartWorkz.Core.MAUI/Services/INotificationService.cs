namespace SmartWorkz.Mobile;

/// <summary>
/// Service for local and push notifications on mobile devices.
/// Handles notification scheduling, delivery, and user interaction handling.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends an immediate local notification.
    /// </summary>
    /// <param name="title">Notification title</param>
    /// <param name="message">Notification message body</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Notification ID for tracking</returns>
    Task<string> SendAsync(string title, string message, CancellationToken ct = default);

    /// <summary>
    /// Schedules a local notification for a future time.
    /// </summary>
    /// <param name="title">Notification title</param>
    /// <param name="message">Notification message body</param>
    /// <param name="delaySeconds">Delay in seconds before notification is shown</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Notification ID for tracking and cancellation</returns>
    Task<string> ScheduleAsync(string title, string message, int delaySeconds, CancellationToken ct = default);

    /// <summary>
    /// Cancels a scheduled or pending notification.
    /// </summary>
    /// <param name="notificationId">ID of notification to cancel</param>
    /// <param name="ct">Cancellation token</param>
    Task CancelAsync(string notificationId, CancellationToken ct = default);

    /// <summary>
    /// Cancels all pending notifications.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    Task CancelAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks if notifications are enabled/permitted on the device.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if notifications are permitted and enabled</returns>
    Task<bool> AreNotificationsEnabledAsync(CancellationToken ct = default);

    /// <summary>
    /// Requests notification permission from the user.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if user granted permission, false otherwise</returns>
    Task<bool> RequestNotificationPermissionAsync(CancellationToken ct = default);
}
