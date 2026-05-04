namespace SmartWorkz.Mobile;

#if __ANDROID__

using Android.App;
using Android.Content;
using AndroidX.Core.App;
using AndroidX.Work;
using Google.Android.Material.Snackbar;
using Java.Util.Concurrent;

public partial class NotificationService
{
    private const string NotificationChannelId = "smartworkz_notifications";
    private const int BaseNotificationId = 1000;
    private static int _notificationCounter = BaseNotificationId;

    private partial async Task<string> SendAsyncPlatform(string title, string message, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var notificationId = Interlocked.Increment(ref _notificationCounter);
        var notifIdString = notificationId.ToString();

        try
        {
            CreateNotificationChannel();
            var context = Android.App.Application.Context;
            var notificationManager = NotificationManagerCompat.From(context);

            var builder = new NotificationCompat.Builder(context, NotificationChannelId)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityDefault);

            notificationManager.Notify(notificationId, builder.Build());
            return notifIdString;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Android SendAsync failed: {ex.Message}");
            return string.Empty;
        }
    }

    private partial async Task<string> ScheduleAsyncPlatform(string title, string message, int delaySeconds, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var notificationId = Guid.NewGuid().ToString();

        try
        {
            CreateNotificationChannel();

            var workRequest = new OneTimeWorkRequest.Builder(typeof(NotificationWorker))
                .SetInitialDelay(delaySeconds, TimeUnit.Seconds)
                .SetInputData(new Data.Builder()
                    .PutString("notification_id", notificationId)
                    .PutString("title", title)
                    .PutString("message", message)
                    .Build())
                .Build();

            WorkManager.GetInstance(Android.App.Application.Context).EnqueueUniqueWork(
                notificationId,
                ExistingWorkPolicy.Keep,
                workRequest);

            return notificationId;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Android ScheduleAsync failed: {ex.Message}");
            return string.Empty;
        }
    }

    private partial async Task CancelAsyncPlatform(string notificationId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            WorkManager.GetInstance(Android.App.Application.Context).CancelUniqueWork(notificationId);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Android CancelAsync failed: {ex.Message}");
        }
    }

    private partial async Task CancelAllAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            WorkManager.GetInstance(Android.App.Application.Context).CancelAllWork();
            var notificationManager = NotificationManagerCompat.From(Android.App.Application.Context);
            notificationManager.CancelAll();
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Android CancelAllAsync failed: {ex.Message}");
        }
    }

    private partial async Task<bool> AreNotificationsEnabledAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var notificationManager = NotificationManagerCompat.From(Android.App.Application.Context);
            return notificationManager.AreNotificationsEnabled();
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
            // Android 13+ requires POST_NOTIFICATIONS permission
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
            {
                var permission = Android.Manifest.Permission.PostNotifications;
                var context = Android.App.Application.Context;
                var status = AndroidX.Core.Content.ContextCompat.CheckSelfPermission(context, permission);
                return status == Android.Content.PM.Permission.Granted;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private void CreateNotificationChannel()
    {
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        {
            var channelName = "SmartWorkz Notifications";
            var importance = NotificationImportance.Default;
            var channel = new NotificationChannel(NotificationChannelId, channelName, importance);
            var notificationManager = (NotificationManager)Android.App.Application.Context.GetSystemService(
                Context.NotificationService);
            notificationManager?.CreateNotificationChannel(channel);
        }
    }
}

#endif
