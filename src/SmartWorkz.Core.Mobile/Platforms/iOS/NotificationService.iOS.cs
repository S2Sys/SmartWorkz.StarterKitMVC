namespace SmartWorkz.Mobile;

#if __IOS__

using UserNotifications;
using Foundation;

public partial class NotificationService
{
    private partial async Task<string> SendAsyncPlatform(string title, string message, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var notificationId = Guid.NewGuid().ToString();

        try
        {
            var content = new UNMutableNotificationContent
            {
                Title = title,
                Body = message,
                Sound = UNNotificationSound.Default,
                Badge = NSNumber.FromInt32(1)
            };

            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);
            var request = UNNotificationRequest.FromIdentifier(notificationId, content, trigger);

            await UNUserNotificationCenter.Current.AddNotificationRequestAsync(request);
            return notificationId;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"iOS SendAsync failed: {ex.Message}");
            return string.Empty;
        }
    }

    private partial async Task<string> ScheduleAsyncPlatform(string title, string message, int delaySeconds, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var notificationId = Guid.NewGuid().ToString();

        try
        {
            var content = new UNMutableNotificationContent
            {
                Title = title,
                Body = message,
                Sound = UNNotificationSound.Default,
                Badge = NSNumber.FromInt32(1)
            };

            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(delaySeconds, false);
            var request = UNNotificationRequest.FromIdentifier(notificationId, content, trigger);

            await UNUserNotificationCenter.Current.AddNotificationRequestAsync(request);
            return notificationId;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"iOS ScheduleAsync failed: {ex.Message}");
            return string.Empty;
        }
    }

    private partial async Task CancelAsyncPlatform(string notificationId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            UNUserNotificationCenter.Current.RemovePendingNotificationRequests(new[] { notificationId });
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"iOS CancelAsync failed: {ex.Message}");
        }
    }

    private partial async Task CancelAllAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            UNUserNotificationCenter.Current.RemoveAllPendingNotificationRequests();
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"iOS CancelAllAsync failed: {ex.Message}");
        }
    }

    private partial async Task<bool> AreNotificationsEnabledAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var settings = await UNUserNotificationCenter.Current.GetNotificationSettingsAsync();
            return settings?.AuthorizationStatus == UNAuthorizationStatus.Authorized;
        }
        catch
        {
            return false;
        }
    }

    private partial async Task<bool> RequestNotificationPermissionAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var (granted, error) = await UNUserNotificationCenter.Current.RequestAuthorizationAsync(
                UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound | UNAuthorizationOptions.Badge);

            if (granted)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UIApplication.SharedApplication.InvokeOnMainThread(UIApplication.SharedApplication.RegisterForRemoteNotifications);
                });
            }

            return granted;
        }
        catch
        {
            return false;
        }
    }
}

#endif
