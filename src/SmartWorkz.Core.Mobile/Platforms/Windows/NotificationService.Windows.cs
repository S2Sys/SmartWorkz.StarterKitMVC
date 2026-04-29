namespace SmartWorkz.Mobile;

#if WINDOWS

public partial class NotificationService
{
    private partial async Task<string> SendAsyncPlatform(string title, string message, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _logger?.LogWarning("Notifications not available on Windows platform");
        return string.Empty;
    }

    private partial async Task<string> ScheduleAsyncPlatform(string title, string message, int delaySeconds, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _logger?.LogWarning("Scheduled notifications not available on Windows platform");
        return string.Empty;
    }

    private partial async Task CancelAsyncPlatform(string notificationId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
    }

    private partial async Task CancelAllAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
    }

    private partial async Task<bool> AreNotificationsEnabledAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return false;
    }

    private partial async Task<bool> RequestNotificationPermissionAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return false;
    }
}

#endif
