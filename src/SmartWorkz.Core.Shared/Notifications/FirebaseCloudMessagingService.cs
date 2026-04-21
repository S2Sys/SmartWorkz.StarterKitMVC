namespace SmartWorkz.Core.Shared.Notifications;

using FirebaseAdmin.Messaging;
using SmartWorkz.Core.Services.Notifications;
using Microsoft.Extensions.Logging;

public class FirebaseCloudMessagingService : IPushNotificationService
{
    private const int DefaultTtlSeconds = 3600;
    private const string DefaultAndroidPriority = "high";

    private readonly FirebaseMessaging _firebaseMessaging;
    private readonly ILogger<FirebaseCloudMessagingService> _logger;

    public FirebaseCloudMessagingService(ILogger<FirebaseCloudMessagingService> logger)
    {
        _firebaseMessaging = FirebaseMessaging.DefaultInstance;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

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
