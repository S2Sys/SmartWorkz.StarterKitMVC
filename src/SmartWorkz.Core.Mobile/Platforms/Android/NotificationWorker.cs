namespace SmartWorkz.Mobile;

#if __ANDROID__

using Android.Content;
using AndroidX.Core.App;
using AndroidX.Work;

/// <summary>
/// Background worker for delivering scheduled notifications on Android.
/// Integrates with WorkManager for reliable scheduling.
/// </summary>
public class NotificationWorker : Worker
{
    private const string NotificationChannelId = "smartworkz_notifications";
    private static int _notificationId = 2000;

    public NotificationWorker(Context context, WorkerParameters workerParams)
        : base(context, workerParams)
    {
    }

    public override Result DoWork()
    {
        try
        {
            var data = InputData;
            var notificationId = data.GetString("notification_id") ?? Guid.NewGuid().ToString();
            var title = data.GetString("title") ?? "Notification";
            var message = data.GetString("message") ?? string.Empty;

            ShowNotification(title, message);
            return Result.InvokeSuccess();
        }
        catch (Exception ex)
        {
            return Result.InvokeRetry();
        }
    }

    private void ShowNotification(string title, string message)
    {
        CreateNotificationChannel();

        var notificationManager = NotificationManagerCompat.From(ApplicationContext);
        var notifId = Interlocked.Increment(ref _notificationId);

        var builder = new NotificationCompat.Builder(ApplicationContext, NotificationChannelId)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)
            .SetAutoCancel(true)
            .SetPriority(NotificationCompat.PriorityDefault);

        notificationManager.Notify(notifId, builder.Build());
    }

    private void CreateNotificationChannel()
    {
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        {
            var channelName = "SmartWorkz Notifications";
            var importance = NotificationImportance.Default;
            var channel = new Android.App.NotificationChannel(NotificationChannelId, channelName, importance);
            var notificationManager = (Android.App.NotificationManager)ApplicationContext.GetSystemService(
                Context.NotificationService);
            notificationManager?.CreateNotificationChannel(channel);
        }
    }
}

#endif
