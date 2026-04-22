namespace SmartWorkz.Core;

/// <summary>
/// Service for sending push notifications to mobile and web applications.
/// </summary>
/// <remarks>
/// Purpose: Provides real-time notification delivery to users across web and mobile platforms
/// (iOS, Android, web browsers) using Firebase Cloud Messaging (FCM), Apple Push Notification (APN),
/// or similar providers.
///
/// Error Handling: Uses Task-based methods (no return values; failures should be handled
/// via exception handling). Expected failures (invalid user ID, device offline) may throw
/// exceptions. Callers should wrap SendAsync in try-catch and log failures.
///
/// Async Behavior: All methods are Task-based and await provider acknowledgment of the
/// notification. Important: Acknowledgment ≠ Delivery. The notification is queued by the
/// provider but may fail to reach the device (offline, uninstalled app, etc.).
///
/// Delivery Guarantee: Best-effort delivery. Push notifications are not guaranteed to be
/// delivered, especially if the device is offline or the app is uninstalled.
///
/// Topic-Based Messaging: Supports subscribing users to topics for broadcast messaging.
/// Examples: "sports_news", "product_updates", "system_alerts".
///
/// Typical Usage: Injected into services that need real-time user notifications (new orders,
/// messages, alerts). Often used in background jobs (Hangfire) for non-critical notifications.
///
/// Security: Use FCM/APN tokens securely, validate user ownership of tokens, implement
/// rate limiting to prevent abuse.
///
/// Performance: Push notifications are I/O-intensive. Batch multiple notifications when
/// possible. For non-critical notifications, use background jobs.
/// </remarks>
public interface IPushNotificationService : IService
{
    /// <summary>
    /// Sends a push notification with title and message to a single user asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to receive the notification.
    /// Typically an account/user ID, not a device token.</param>
    /// <param name="title">The notification title displayed on the device. Keep concise (recommended &lt;65 characters).</param>
    /// <param name="message">The notification body/content. Keep concise (recommended &lt;240 characters).</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async send operation. Throws on failure.</returns>
    /// <remarks>
    /// Simple Notification: This overload sends a basic notification without advanced features.
    /// For richer notifications with images, actions, or custom data, use the PushNotificationPayload overload.
    ///
    /// User ID vs Device Token: The method takes userId (account ID), not device tokens.
    /// The implementation should look up the user's registered devices/tokens internally.
    /// If the user has multiple devices, the notification is sent to all registered devices.
    ///
    /// Delivery: Sent to all devices associated with the user. If the user has no devices,
    /// the call may silently succeed or throw an exception (consult implementation docs).
    ///
    /// Error Codes (if exceptions are thrown):
    /// - USER_NOT_FOUND: User ID not recognized
    /// - NO_DEVICES: User has no registered devices
    /// - INVALID_TOKEN: Stored device token is invalid/expired
    /// - RATE_LIMIT_EXCEEDED: Too many notifications sent in short time
    /// - FCM_ERROR: Firebase Cloud Messaging error
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     await pushNotificationService.SendAsync(
    ///         userId: currentUser.Id,
    ///         title: "Order Placed",
    ///         message: "Your order #12345 has been confirmed"
    ///     );
    ///     logger.LogInformation($"Push notification sent to user {currentUser.Id}");
    /// }
    /// catch (Exception ex)
    /// {
    ///     logger.LogError($"Failed to send notification: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    Task SendAsync(string userId, string title, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a push notification with title and message to multiple users asynchronously.
    /// </summary>
    /// <param name="userIds">Collection of user IDs to receive the notification.
    /// Must not be null or empty.</param>
    /// <param name="title">The notification title displayed on the device. Keep concise (recommended &lt;65 characters).</param>
    /// <param name="message">The notification body/content. Keep concise (recommended &lt;240 characters).</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async send operation. Throws on failure.</returns>
    /// <remarks>
    /// Batch Sending: Sends the same notification to all specified users.
    /// For personalized notifications, call SendAsync multiple times or iterate over users.
    ///
    /// Implementation Options:
    /// 1. Send individual notifications to each user
    /// 2. Use FCM topic/batch API for efficiency
    /// 3. Queue notifications and send asynchronously
    ///
    /// Partial Failures: If one user's notification fails, the entire operation may fail.
    /// Consult implementation docs for partial success handling.
    ///
    /// Performance: Batch sending to many users may take time. For large broadcasts (&gt;1000 users),
    /// consider using topic-based messaging via SendToTopicAsync.
    /// </remarks>
    /// <example>
    /// <code>
    /// var userIds = new[] { "user1", "user2", "user3" };
    /// try
    /// {
    ///     await pushNotificationService.SendAsync(
    ///         userIds: userIds,
    ///         title: "System Maintenance",
    ///         message: "System will be down for maintenance tonight at 11 PM"
    ///     );
    ///     logger.LogInformation($"Notification sent to {userIds.Length} users");
    /// }
    /// catch (Exception ex)
    /// {
    ///     logger.LogError($"Batch notification send failed: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    Task SendAsync(IEnumerable<string> userIds, string title, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a rich push notification with advanced features (image, actions, custom data) to a single user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to receive the notification.</param>
    /// <param name="payload">The notification payload containing title, body, image, actions, and custom data.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async send operation. Throws on failure.</returns>
    /// <remarks>
    /// Rich Notifications: Use PushNotificationPayload for notifications with:
    /// - Large images/thumbnails
    /// - Action buttons (open app, take action, dismiss)
    /// - Custom data payload for handling taps
    /// - Badge counts (iOS)
    ///
    /// Delivery: Sent to all devices associated with the user.
    /// If the user has no devices, behavior depends on implementation.
    ///
    /// Device Capabilities: Not all devices support all payload features.
    /// Basic fallback (title + body) is always used if advanced features are unsupported.
    /// </remarks>
    /// <example>
    /// <code>
    /// var payload = new PushNotificationPayload
    /// {
    ///     Title = "New Message",
    ///     Body = "You have a new message from John",
    ///     ImageUrl = "https://example.com/avatar.jpg",
    ///     Badge = 1,
    ///     Data = new Dictionary&lt;string, string&gt;
    ///     {
    ///         { "messageId", "msg-12345" },
    ///         { "senderId", "user-john" }
    ///     },
    ///     Action = new PushNotificationAction
    ///     {
    ///         ActionId = "reply",
    ///         ActionTitle = "Reply",
    ///         ActionUrl = "app://messages/msg-12345"
    ///     }
    /// };
    ///
    /// try
    /// {
    ///     await pushNotificationService.SendAsync(userId, payload);
    ///     logger.LogInformation($"Rich notification sent to user {userId}");
    /// }
    /// catch (Exception ex)
    /// {
    ///     logger.LogError($"Failed to send notification: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    Task SendAsync(string userId, PushNotificationPayload payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a rich push notification with advanced features to multiple users asynchronously.
    /// </summary>
    /// <param name="userIds">Collection of user IDs to receive the notification. Must not be null or empty.</param>
    /// <param name="payload">The notification payload containing title, body, image, actions, and custom data.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async send operation. Throws on failure.</returns>
    /// <remarks>
    /// Batch Rich Notifications: Sends the same rich notification to all specified users.
    /// For personalized payloads, iterate and call SendAsync for each user.
    ///
    /// Performance: Sending rich notifications with images to many users may consume
    /// significant bandwidth. For large broadcasts, consider using topic-based messaging.
    /// </remarks>
    /// <example>
    /// <code>
    /// var userIds = new[] { "user1", "user2", "user3" };
    /// var payload = new PushNotificationPayload
    /// {
    ///     Title = "Flash Sale",
    ///     Body = "50% off everything for the next hour!",
    ///     ImageUrl = "https://example.com/sale-banner.jpg",
    ///     Action = new PushNotificationAction
    ///     {
    ///         ActionId = "view_sale",
    ///         ActionTitle = "Shop Now",
    ///         ActionUrl = "app://shop/sale"
    ///     }
    /// };
    ///
    /// try
    /// {
    ///     await pushNotificationService.SendAsync(userIds, payload);
    ///     logger.LogInformation($"Sale notification sent to {userIds.Length} users");
    /// }
    /// catch (Exception ex)
    /// {
    ///     logger.LogError($"Batch notification send failed: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    Task SendAsync(IEnumerable<string> userIds, PushNotificationPayload payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a push notification to all users subscribed to a specific topic asynchronously.
    /// </summary>
    /// <param name="topic">The topic name (e.g., "sports_news", "product_updates").
    /// Must not be null or empty. Use lowercase alphanumeric with underscores.</param>
    /// <param name="payload">The notification payload containing title, body, image, actions, and custom data.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async send operation. Throws on failure.</returns>
    /// <remarks>
    /// Topic-Based Broadcasting: Sends a notification to all users subscribed to the topic.
    /// Ideal for broadcast messages to large audiences without knowing specific user IDs.
    ///
    /// Performance: More efficient than sending to individual users for large audiences.
    /// A single topic message reaches all subscribers (thousands to millions).
    ///
    /// Topic Naming Conventions:
    /// - Use lowercase alphanumeric with underscores
    /// - Be descriptive: "sports_news", "product_updates", "system_alerts"
    /// - Avoid generic names that might conflict
    /// - Use hierarchical names if supported: "news_sports", "news_tech", etc.
    ///
    /// Topic Management: Users subscribe and unsubscribe using SubscribeToTopicAsync
    /// and UnsubscribeFromTopicAsync.
    /// </remarks>
    /// <example>
    /// <code>
    /// var payload = new PushNotificationPayload
    /// {
    ///     Title = "Breaking News",
    ///     Body = "Check out the latest sports updates",
    ///     ImageUrl = "https://example.com/sports-header.jpg"
    /// };
    ///
    /// try
    /// {
    ///     await pushNotificationService.SendToTopicAsync("sports_news", payload);
    ///     logger.LogInformation("Notification sent to all sports_news subscribers");
    /// }
    /// catch (Exception ex)
    /// {
    ///     logger.LogError($"Topic notification send failed: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    Task SendToTopicAsync(string topic, PushNotificationPayload payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes a user to a topic to receive topic-based push notifications asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user. Must not be null or empty.</param>
    /// <param name="topic">The topic name to subscribe to (e.g., "sports_news").
    /// Must not be null or empty. Use lowercase alphanumeric with underscores.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async subscription. Throws on failure.</returns>
    /// <remarks>
    /// Idempotent: Subscribing a user already subscribed to the topic should not cause an error.
    ///
    /// Use Cases:
    /// - User subscribes to news category (sports, tech, etc.)
    /// - User opt-in to promotional notifications
    /// - User subscribes to organization announcements
    ///
    /// Persistent: Subscriptions typically persist until explicitly removed via UnsubscribeFromTopicAsync.
    ///
    /// Error Handling: May throw if user ID is invalid or topic name is malformed.
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     // When user subscribes to sports news
    ///     await pushNotificationService.SubscribeToTopicAsync(userId, "sports_news");
    ///     logger.LogInformation($"User {userId} subscribed to sports_news");
    /// }
    /// catch (Exception ex)
    /// {
    ///     logger.LogError($"Failed to subscribe to topic: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    Task SubscribeToTopicAsync(string userId, string topic, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unsubscribes a user from a topic to stop receiving topic-based push notifications asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user. Must not be null or empty.</param>
    /// <param name="topic">The topic name to unsubscribe from (e.g., "sports_news").
    /// Must not be null or empty.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async unsubscription. Throws on failure.</returns>
    /// <remarks>
    /// Idempotent: Unsubscribing a user not subscribed to the topic should not cause an error.
    ///
    /// Use Cases:
    /// - User unsubscribes from a news category
    /// - User opts out of promotional notifications
    /// - User leaves an organization
    ///
    /// Effect: User will no longer receive messages sent to this topic via SendToTopicAsync.
    /// Individual user notifications (SendAsync) are unaffected.
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     // When user unsubscribes from sports news
    ///     await pushNotificationService.UnsubscribeFromTopicAsync(userId, "sports_news");
    ///     logger.LogInformation($"User {userId} unsubscribed from sports_news");
    /// }
    /// catch (Exception ex)
    /// {
    ///     logger.LogError($"Failed to unsubscribe from topic: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    Task UnsubscribeFromTopicAsync(string userId, string topic, CancellationToken cancellationToken = default);
}
