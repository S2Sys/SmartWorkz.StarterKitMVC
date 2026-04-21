namespace SmartWorkz.Core.Services.Notifications;

public interface IPushNotificationService
{
    Task SendAsync(string userId, string title, string message, CancellationToken cancellationToken = default);
    Task SendAsync(IEnumerable<string> userIds, string title, string message, CancellationToken cancellationToken = default);
    Task SendAsync(string userId, PushNotificationPayload payload, CancellationToken cancellationToken = default);
    Task SendAsync(IEnumerable<string> userIds, PushNotificationPayload payload, CancellationToken cancellationToken = default);
    Task SendToTopicAsync(string topic, PushNotificationPayload payload, CancellationToken cancellationToken = default);
    Task SubscribeToTopicAsync(string userId, string topic, CancellationToken cancellationToken = default);
    Task UnsubscribeFromTopicAsync(string userId, string topic, CancellationToken cancellationToken = default);
}
