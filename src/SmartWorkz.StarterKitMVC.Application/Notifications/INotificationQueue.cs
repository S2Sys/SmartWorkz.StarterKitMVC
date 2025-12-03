namespace SmartWorkz.StarterKitMVC.Application.Notifications;

public interface INotificationQueue
{
    Task EnqueueAsync(NotificationMessage message, CancellationToken ct = default);
    Task<NotificationMessage?> DequeueAsync(CancellationToken ct = default);
}
