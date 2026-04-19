namespace SmartWorkz.Core.Services.Notifications;

public interface IPushNotificationService
{
    Task SendAsync(string userId, string title, string message, CancellationToken cancellationToken = default);
    Task SendAsync(IEnumerable<string> userIds, string title, string message, CancellationToken cancellationToken = default);
}
