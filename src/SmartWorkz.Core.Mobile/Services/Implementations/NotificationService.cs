namespace SmartWorkz.Mobile;

using ILogger = Microsoft.Extensions.Logging.ILogger;

/// <summary>
/// Provides local notifications on mobile devices.
/// Platform-specific implementations handle iOS, Android, and Windows.
/// </summary>
#if !WINDOWS
public partial class NotificationService : INotificationService
#else
public class NotificationService : INotificationService
#endif
{
    private readonly ILogger _logger;
    private static readonly Dictionary<string, System.Threading.CancellationTokenSource> ScheduledNotifications = new();

    public NotificationService(ILogger logger)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public async Task<string> SendAsync(string title, string message, CancellationToken ct = default)
    {
        Guard.NotEmpty(title, nameof(title));
        Guard.NotEmpty(message, nameof(message));
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        _logger.LogWarning("Notifications not available on Windows platform");
        return string.Empty;
        #else
        try
        {
            return await SendAsyncPlatform(title, message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification");
            return string.Empty;
        }
        #endif
    }

    public async Task<string> ScheduleAsync(string title, string message, int delaySeconds, CancellationToken ct = default)
    {
        Guard.NotEmpty(title, nameof(title));
        Guard.NotEmpty(message, nameof(message));
        if (delaySeconds < 0)
            throw new ArgumentException("Delay cannot be negative", nameof(delaySeconds));
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        _logger.LogWarning("Scheduled notifications not available on Windows platform");
        return string.Empty;
        #else
        try
        {
            return await ScheduleAsyncPlatform(title, message, delaySeconds, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule notification");
            return string.Empty;
        }
        #endif
    }

    public async Task CancelAsync(string notificationId, CancellationToken ct = default)
    {
        Guard.NotEmpty(notificationId, nameof(notificationId));
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        _logger.LogInformation("Cancel notification called on Windows (no-op)");
        #else
        try
        {
            await CancelAsyncPlatform(notificationId, ct);
            lock (ScheduledNotifications)
            {
                if (ScheduledNotifications.TryGetValue(notificationId, out var cts))
                {
                    cts.Cancel();
                    ScheduledNotifications.Remove(notificationId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel notification");
        }
        #endif
    }

    public async Task CancelAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        _logger.LogInformation("Cancel all notifications called on Windows (no-op)");
        #else
        try
        {
            await CancelAllAsyncPlatform(ct);
            lock (ScheduledNotifications)
            {
                foreach (var cts in ScheduledNotifications.Values)
                {
                    cts.Cancel();
                }
                ScheduledNotifications.Clear();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel all notifications");
        }
        #endif
    }

    public async Task<bool> AreNotificationsEnabledAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        return false;
        #else
        try
        {
            return await AreNotificationsEnabledAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check notification status");
            return false;
        }
        #endif
    }

    public async Task<bool> RequestNotificationPermissionAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        return false;
        #else
        try
        {
            return await RequestNotificationPermissionAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to request notification permission");
            return false;
        }
        #endif
    }

    #if !WINDOWS
    private partial Task<string> SendAsyncPlatform(string title, string message, CancellationToken ct);
    private partial Task<string> ScheduleAsyncPlatform(string title, string message, int delaySeconds, CancellationToken ct);
    private partial Task CancelAsyncPlatform(string notificationId, CancellationToken ct);
    private partial Task CancelAllAsyncPlatform(CancellationToken ct);
    private partial Task<bool> AreNotificationsEnabledAsyncPlatform(CancellationToken ct);
    private partial Task<bool> RequestNotificationPermissionAsyncPlatform(CancellationToken ct);
    #endif
}
