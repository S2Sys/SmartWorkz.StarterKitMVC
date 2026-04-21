namespace SmartWorkz.Core.Shared.Notifications;

using FirebaseAdmin.Messaging;
using SmartWorkz.Core.Services.Notifications;
using Microsoft.Extensions.Logging;

/// <summary>
/// Firebase Cloud Messaging service implementation for sending push notifications.
/// Supports single/batch user notifications, topic-based broadcasting, and multi-platform delivery (Android, iOS, Web).
/// </summary>
public class FirebaseCloudMessagingService : IPushNotificationService
{
    private const int DefaultTtlSeconds = 3600;
    private const string DefaultAndroidPriority = "high";

    private readonly FirebaseMessaging _firebaseMessaging;
    private readonly ILogger<FirebaseCloudMessagingService> _logger;

    /// <summary>Initializes a new instance of the FirebaseCloudMessagingService.</summary>
    /// <param name="logger">Logger for diagnostic and error information.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public FirebaseCloudMessagingService(ILogger<FirebaseCloudMessagingService> logger)
    {
        _firebaseMessaging = FirebaseMessaging.DefaultInstance;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Sends a simple push notification to a single user.</summary>
    /// <param name="userId">User identifier (Firebase device token).</param>
    /// <param name="title">Notification title.</param>
    /// <param name="message">Notification body text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentException">Thrown when userId, title, or message is null/empty.</exception>
    public Task SendAsync(string userId, string title, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be null or empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Title and message cannot be empty");

        var payload = new PushNotificationPayload
        {
            Title = title,
            Body = message
        };
        return SendAsync(userId, payload, cancellationToken);
    }

    /// <summary>Sends simple push notifications to multiple users.</summary>
    /// <param name="userIds">Collection of user identifiers (Firebase device tokens).</param>
    /// <param name="title">Notification title.</param>
    /// <param name="message">Notification body text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentNullException">Thrown when userIds is null.</exception>
    /// <exception cref="ArgumentException">Thrown when title or message is null/empty.</exception>
    public Task SendAsync(IEnumerable<string> userIds, string title, string message, CancellationToken cancellationToken = default)
    {
        if (userIds == null)
            throw new ArgumentNullException(nameof(userIds));
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Title and message cannot be empty");

        var payload = new PushNotificationPayload
        {
            Title = title,
            Body = message
        };
        return SendAsync(userIds, payload, cancellationToken);
    }

    /// <summary>Sends a rich push notification with metadata to a single user.</summary>
    /// <param name="userId">User identifier (Firebase device token).</param>
    /// <param name="payload">Notification payload with title, body, images, data, and actions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentException">Thrown when userId is null/empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when payload is null.</exception>
    public async Task SendAsync(string userId, PushNotificationPayload payload, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be null or empty", nameof(userId));
        if (payload == null)
            throw new ArgumentNullException(nameof(payload));

        try
        {
            var message = BuildMessage(userId, payload);
            var messageId = await _firebaseMessaging.SendAsync(message, cancellationToken);
            _logger.LogInformation("Push notification sent to {UserId}: {MessageId}", userId, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to {UserId}", userId);
            throw;
        }
    }

    /// <summary>Sends a rich push notification to multiple users.</summary>
    /// <param name="userIds">Collection of user identifiers (Firebase device tokens).</param>
    /// <param name="payload">Notification payload with title, body, images, data, and actions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentNullException">Thrown when userIds or payload is null.</exception>
    public async Task SendAsync(IEnumerable<string> userIds, PushNotificationPayload payload, CancellationToken cancellationToken = default)
    {
        if (userIds == null)
            throw new ArgumentNullException(nameof(userIds));
        if (payload == null)
            throw new ArgumentNullException(nameof(payload));

        try
        {
            var tasks = userIds.Select(userId => SendAsync(userId, payload, cancellationToken));
            await Task.WhenAll(tasks);
            _logger.LogInformation("Batch notifications sent to {Count} users", userIds.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send batch notifications");
            throw;
        }
    }

    /// <summary>Sends a rich push notification to all users subscribed to a topic (broadcast).</summary>
    /// <param name="topic">Topic name (e.g., "news", "promotions").</param>
    /// <param name="payload">Notification payload with title, body, images, data, and actions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentException">Thrown when topic is null/empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when payload is null.</exception>
    public async Task SendToTopicAsync(string topic, PushNotificationPayload payload, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic cannot be null or empty", nameof(topic));
        if (payload == null)
            throw new ArgumentNullException(nameof(payload));

        try
        {
            var message = new Message
            {
                Topic = topic,
                Notification = new Notification
                {
                    Title = payload.Title,
                    Body = payload.Body,
                    ImageUrl = payload.ImageUrl
                },
                Data = payload.Data ?? new Dictionary<string, string>(),
                Android = new AndroidConfig { Priority = Priority.High },
                Webpush = new WebpushConfig { Headers = new Dictionary<string, string> { { "TTL", DefaultTtlSeconds.ToString() } } }
            };

            var messageId = await _firebaseMessaging.SendAsync(message, cancellationToken);
            _logger.LogInformation("Topic notification sent to {Topic}: {MessageId}", topic, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send topic notification to {Topic}", topic);
            throw;
        }
    }

    /// <summary>Subscribes a user to a topic for broadcast notifications.</summary>
    /// <param name="userId">User identifier (Firebase device token).</param>
    /// <param name="topic">Topic name to subscribe to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentException">Thrown when userId or topic is null/empty.</exception>
    public async Task SubscribeToTopicAsync(string userId, string topic, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be null or empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic cannot be null or empty", nameof(topic));

        try
        {
            var tokens = new[] { userId };
            await _firebaseMessaging.SubscribeToTopicAsync(tokens, topic);
            _logger.LogInformation("User {UserId} subscribed to topic {Topic}", userId, topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe {UserId} to topic {Topic}", userId, topic);
            throw;
        }
    }

    /// <summary>Unsubscribes a user from a topic.</summary>
    /// <param name="userId">User identifier (Firebase device token).</param>
    /// <param name="topic">Topic name to unsubscribe from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentException">Thrown when userId or topic is null/empty.</exception>
    public async Task UnsubscribeFromTopicAsync(string userId, string topic, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be null or empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic cannot be null or empty", nameof(topic));

        try
        {
            var tokens = new[] { userId };
            await _firebaseMessaging.UnsubscribeFromTopicAsync(tokens, topic);
            _logger.LogInformation("User {UserId} unsubscribed from topic {Topic}", userId, topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unsubscribe {UserId} from topic {Topic}", userId, topic);
            throw;
        }
    }

    private Message BuildMessage(string userId, PushNotificationPayload payload)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be null or empty", nameof(userId));
        if (payload == null)
            throw new ArgumentNullException(nameof(payload));

        return new Message
        {
            Token = userId,
            Notification = new Notification
            {
                Title = payload.Title,
                Body = payload.Body,
                ImageUrl = payload.ImageUrl
            },
            Data = payload.Data ?? new Dictionary<string, string>(),
            Android = new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification()
            },
            Webpush = new WebpushConfig
            {
                Headers = new Dictionary<string, string> { { "TTL", DefaultTtlSeconds.ToString() } }
            },
            Apns = new ApnsConfig
            {
                Aps = new Aps { Badge = payload.Badge }
            }
        };
    }
}
