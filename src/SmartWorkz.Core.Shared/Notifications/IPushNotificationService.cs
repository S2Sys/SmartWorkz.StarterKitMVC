namespace SmartWorkz.Core.Services.Notifications;

/// <summary>
/// Service for sending push notifications using Firebase Cloud Messaging.
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Sends a simple push notification to a single user.
    /// </summary>
    /// <param name="userId">User identifier (Firebase token).</param>
    /// <param name="title">Notification title.</param>
    /// <param name="message">Notification body text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    /// <exception cref="ArgumentException">Thrown when userId, title, or message is null or empty.</exception>
    Task SendAsync(string userId, string title, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a simple push notification to multiple users.
    /// </summary>
    /// <param name="userIds">Collection of user identifiers (Firebase tokens).</param>
    /// <param name="title">Notification title.</param>
    /// <param name="message">Notification body text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    /// <exception cref="ArgumentException">Thrown when title or message is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when userIds is null.</exception>
    Task SendAsync(IEnumerable<string> userIds, string title, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a rich push notification with metadata to a single user.
    /// </summary>
    /// <param name="userId">User identifier (Firebase token).</param>
    /// <param name="payload">Notification payload with title, body, images, and custom data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    /// <exception cref="ArgumentException">Thrown when userId is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when payload is null.</exception>
    Task SendAsync(string userId, PushNotificationPayload payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a rich push notification with metadata to multiple users.
    /// </summary>
    /// <param name="userIds">Collection of user identifiers (Firebase tokens).</param>
    /// <param name="payload">Notification payload with title, body, images, and custom data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when userIds or payload is null.</exception>
    Task SendAsync(IEnumerable<string> userIds, PushNotificationPayload payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a push notification to all users subscribed to a topic.
    /// </summary>
    /// <param name="topic">The topic name.</param>
    /// <param name="payload">Notification payload with title, body, images, and custom data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    /// <exception cref="ArgumentException">Thrown when topic is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when payload is null.</exception>
    Task SendToTopicAsync(string topic, PushNotificationPayload payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes a user to receive notifications from a topic.
    /// </summary>
    /// <param name="userId">User identifier (Firebase token).</param>
    /// <param name="topic">The topic name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous subscription operation.</returns>
    /// <exception cref="ArgumentException">Thrown when userId or topic is null or empty.</exception>
    Task SubscribeToTopicAsync(string userId, string topic, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unsubscribes a user from a topic.
    /// </summary>
    /// <param name="userId">User identifier (Firebase token).</param>
    /// <param name="topic">The topic name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous unsubscription operation.</returns>
    /// <exception cref="ArgumentException">Thrown when userId or topic is null or empty.</exception>
    Task UnsubscribeFromTopicAsync(string userId, string topic, CancellationToken cancellationToken = default);
}
