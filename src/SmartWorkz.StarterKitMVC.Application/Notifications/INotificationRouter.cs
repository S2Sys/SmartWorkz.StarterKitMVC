namespace SmartWorkz.StarterKitMVC.Application.Notifications;

public interface INotificationRouter
{
    Task SendAsync(NotificationMessage message, CancellationToken ct = default);
}
