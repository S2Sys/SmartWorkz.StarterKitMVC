namespace SmartWorkz.Core;

/// <summary>
/// Data structure for rich push notifications with advanced features (images, actions, custom data).
/// </summary>
/// <remarks>
/// Purpose: Encapsulates complete push notification payload for IPushNotificationService.SendAsync.
/// Service Consumer: IPushNotificationService sends via Firebase Cloud Messaging (FCM), Apple Push Notification (APN), or similar providers.
///
/// Usage Pattern:
/// 1. Create new PushNotificationPayload instance
/// 2. Set required properties: Title, Body
/// 3. Optionally set: ImageUrl, Data, Action, Badge
/// 4. Pass to pushNotificationService.SendAsync(userId, payload) or SendAsync(userIds, payload) or SendToTopicAsync(topic, payload)
///
/// Validation Constraints:
/// - Title: Required, must not be null or empty; recommended &lt;65 characters for device display
/// - Body: Required, must not be null or empty; recommended &lt;240 characters for device display
/// - ImageUrl: Optional, must be HTTPS URL if provided; typically 1-2 MB or less
/// - Data: Optional key-value dictionary for custom app handling; typically &lt;4 KB total
/// - Action: Optional button/action to display with notification
/// - Badge: Optional app badge count (iOS); typically 0-999
///
/// Device Delivery:
/// - Delivered to all devices where the user has the app installed and is subscribed
/// - Offline devices will receive notification when they come online (if within retention period)
/// - Best-effort delivery; not guaranteed (device offline too long, app uninstalled, carrier issues)
/// - Delivery acknowledgment ≠ User notification (may be silently delivered if app in foreground)
///
/// Platform Differences:
/// - iOS (APN): Supports images, custom data, badge, actions via interactive notifications
/// - Android (FCM): Supports images, custom data, actions; no native badge (app-managed)
/// - Web (FCM): Supports images, title, body, actions; limited custom data support
/// - Some platforms may ignore unsupported fields (graceful degradation)
///
/// Length Limits:
/// - Title: &lt;65 characters (Android shows &lt;30 chars, iOS &lt;40 chars in lock screen preview)
/// - Body: &lt;240 characters (most devices display 2-3 lines of text)
/// - Total payload: Typically &lt;4 KB (FCM limit is 4 KB including metadata)
/// - ImageUrl: Absolute HTTPS URL only; image downloaded at delivery time
///
/// Security Considerations:
/// - ImageUrl must be HTTPS (not HTTP)
/// - Do not include sensitive data in Title, Body, or Data (notifications may be visible on lock screen)
/// - Data dictionary is visible to recipient app but not on lock screen
/// - Consider encryption for sensitive data in Data field if needed
/// - Validate imageUrl is trusted source to prevent loading malicious content
/// </remarks>
public class PushNotificationPayload
{
    /// <summary>
    /// Gets or sets the notification title displayed on the device (required).
    /// </summary>
    /// <remarks>
    /// Format: Plain text title line.
    /// Length: Recommended maximum &lt;65 characters. Device display varies:
    /// - Android lock screen: Shows first 30-40 characters
    /// - iOS lock screen: Shows first 40 characters
    /// - Web notifications: Shows entire title
    ///
    /// Best Practices:
    /// - Use action-oriented, concise titles
    /// - Avoid generic titles ("Notification" or "Alert")
    /// - Include relevant context (e.g., "Order Placed" not just "Update")
    /// - Avoid excessive capitalization or emoji (may not render correctly)
    ///
    /// Examples of Valid Values:
    /// - "New Message"
    /// - "Order #12345 Shipped"
    /// - "Flash Sale Starts in 30 Minutes"
    /// - "Payment Received"
    /// - "System Update Available"
    /// </remarks>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the notification body/detail text (required).
    /// </summary>
    /// <remarks>
    /// Format: Plain text message body.
    /// Length: Recommended maximum &lt;240 characters. Device display varies:
    /// - Most devices show 2-3 lines of text (approximately 100-200 characters visible)
    /// - Longer text is truncated with "..." or requires expanding notification
    /// - Very long messages may not be fully visible unless user expands
    ///
    /// Best Practices:
    /// - Use complete, actionable messages
    /// - First ~50 characters are most visible; put important context first
    /// - Avoid line breaks (use periods or - for separation)
    /// - Be concise and specific
    /// - Avoid including sensitive data (passwords, payment info, PII)
    ///
    /// Examples of Valid Values:
    /// - "You have a new message from John"
    /// - "Your order #12345 has been shipped. Expected delivery: tomorrow."
    /// - "Limited time offer: 50% off all items!"
    /// - "Your appointment is confirmed for Monday at 2 PM"
    /// - "System maintenance scheduled for tonight 11 PM - 1 AM"
    /// </remarks>
    public required string Body { get; set; }

    /// <summary>
    /// Gets or sets the image URL to display with the notification (optional).
    /// </summary>
    /// <remarks>
    /// Format: Absolute HTTPS URL (HTTP not supported by most providers).
    /// Examples:
    /// - "https://example.com/image.jpg"
    /// - "https://cdn.example.com/notifications/sale-banner.png"
    /// - "https://api.example.com/attachments/avatar-user123.jpg"
    ///
    /// Image Requirements:
    /// - Must be HTTPS (not HTTP)
    /// - Supported formats: JPEG, PNG, GIF, WebP (varies by provider)
    /// - Typical size limits: 1-2 MB (consult provider docs)
    /// - Recommended dimensions:
    ///   - Thumbnail: 100x100 to 400x400 px
    ///   - Large image: 400x300 to 1200x800 px (aspect ratio 4:3 or 16:9)
    ///   - Varies by platform; test on target platforms
    /// - Will be downloaded at delivery time; ensure URL is accessible and fast
    /// - Image loading may fail (network issues, URL 404); notification still displays without image
    ///
    /// Platform Support:
    /// - Android: Large image shown in expanded notification
    /// - iOS: Shows as notification thumbnail or in action
    /// - Web: Shows as notification banner image
    ///
    /// Nullable: Yes; null means no image (notification shows title and body only).
    ///
    /// Examples of Valid Values:
    /// - "https://example.com/products/shoe-sale-banner.jpg"
    /// - "https://cdn.example.com/avatars/user-john.png"
    /// - "https://api.example.com/notifications/icon.jpg"
    /// - null (no image)
    /// </remarks>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets custom data dictionary for app-specific handling (optional).
    /// </summary>
    /// <remarks>
    /// Format: Dictionary&lt;string, string&gt; where key-value pairs contain custom application data.
    /// Purpose: Allows passing additional context to the receiving app beyond title and body.
    /// Examples: message ID, action type, deep link, user ID, etc.
    ///
    /// Data Constraints:
    /// - Total payload (title, body, data, image, etc.) must be &lt;4 KB (FCM limit)
    /// - Each value should be relatively small (keep to minimal required data)
    /// - Only string values supported (convert complex objects to JSON or serialized format)
    /// - Keys should be lowercase alphanumeric with underscores (e.g., "message_id", "user_id")
    ///
    /// Usage Pattern:
    /// App receives notification and uses Data dictionary to:
    /// 1. Determine notification type/action
    /// 2. Route to specific page or handler
    /// 3. Update app state or UI
    /// 4. Perform background operations
    ///
    /// Visibility:
    /// - Data is visible to the app that handles the notification
    /// - NOT displayed to user on lock screen or notification shade
    /// - Useful for sensitive/internal metadata that users don't need to see
    /// - Still should not contain passwords, tokens, or highly sensitive PII
    ///
    /// Nullable: Yes; null means no custom data.
    ///
    /// Examples of Valid Values:
    /// - new Dictionary&lt;string, string&gt; { { "messageId", "msg-12345" }, { "senderId", "user-john" } }
    /// - new Dictionary&lt;string, string&gt; { { "orderId", "ORD-54321" }, { "action", "view_details" } }
    /// - new Dictionary&lt;string, string&gt; { { "deepLink", "app://products/item-123" }, { "campaignId", "summer-sale-2024" } }
    /// - null (no custom data)
    /// - Empty dictionary (no custom data)
    /// </remarks>
    public Dictionary<string, string>? Data { get; set; }

    /// <summary>
    /// Gets or sets the action button/handler for the notification (optional).
    /// </summary>
    /// <remarks>
    /// Format: PushNotificationAction object defining interactive button action.
    /// Purpose: Allows user to take action directly from the notification without opening the app.
    ///
    /// Platform Support:
    /// - iOS: Shown as action buttons below notification or in interactive notification UI
    /// - Android: Shown as action button or swipe action depending on Android version
    /// - Web: Shown as notification action button
    /// - Some platforms may not support or may limit number of actions (typically 1-3)
    ///
    /// User Experience:
    /// - User can tap action without opening app
    /// - App receives action ID and can perform corresponding operation
    /// - Example: "Reply" button on message notification, "View Order" on order notification
    /// - Improves engagement and reduces friction
    ///
    /// Nullable: Yes; null means no action button (notification is informational only).
    ///
    /// Design Consideration:
    /// - Keep action titles concise and action-oriented ("Reply", "Confirm", "Shop Now", "Learn More")
    /// - ActionUrl should be deep link into the app or web URL for handling
    /// - Consider what happens if user taps action vs. tapping notification itself
    /// </remarks>
    public PushNotificationAction? Action { get; set; }

    /// <summary>
    /// Gets or sets the app badge count (optional, iOS primarily).
    /// </summary>
    /// <remarks>
    /// Format: Non-negative integer representing badge number on app icon.
    /// Purpose: Shows unread count or pending action count on the app icon (red badge circle).
    /// Example: 1, 5, 23, 999
    ///
    /// Platform Support:
    /// - iOS: Directly supported; shows as red circle with number on app icon
    /// - Android: No native support; app must implement custom badge display logic
    /// - Web: Not typically supported (no app icon in traditional sense)
    ///
    /// Typical Usage:
    /// - Set to number of unread messages
    /// - Set to 0 to clear badge
    /// - Increment by 1 for each new notification
    /// - Set to total pending items count
    ///
    /// Nullable: Yes; null means no badge change or no badge displayed.
    ///
    /// Design Consideration:
    /// - Badge value should reflect app state (synchronized between push notifications and app logic)
    /// - Users should be able to clear badge by opening app or completing action
    /// - Avoid rapidly changing badges (spam effect on user)
    /// - Consider providing both badge increment and absolute value options
    ///
    /// Examples of Valid Values:
    /// - 1 (one pending item)
    /// - 5 (five unread messages)
    /// - 23 (twenty-three pending notifications)
    /// - 0 (clear badge/no pending items)
    /// - null (no badge or no badge change)
    /// </remarks>
    public int? Badge { get; set; }
}

/// <summary>
/// Data structure for push notification action button with URL deep linking.
/// </summary>
/// <remarks>
/// Purpose: Defines an interactive action button for PushNotificationPayload.
/// Service Consumer: Used by IPushNotificationService to display action button on notification.
///
/// Usage Pattern:
/// 1. Create PushNotificationAction with ActionId, ActionTitle, ActionUrl
/// 2. Assign to PushNotificationPayload.Action
/// 3. Pass payload to IPushNotificationService.SendAsync
/// 4. App receives action tap and navigates to ActionUrl or handles ActionId
///
/// User Experience:
/// - Action button appears on notification (text: ActionTitle)
/// - User taps button to trigger action without opening app main UI
/// - App receives notification with action ID and can navigate or perform operation
///
/// Platform Support:
/// - iOS: Shows as interactive notification action or button
/// - Android: Shows as action button or swipe action
/// - Web: Shows as notification action button
/// - Typically supports 1-3 actions per notification (varies by platform)
///
/// Deep Linking:
/// - ActionUrl should be deep link format (e.g., "app://messages/msg-123", "app://orders/ORD-456")
/// - Or web URL (e.g., "https://example.com/orders/ORD-456")
/// - App must have deep link routing implemented to handle URLs
/// - Ensures user lands in correct section of app when tapping action
/// </remarks>
public class PushNotificationAction
{
    /// <summary>
    /// Gets or sets the unique identifier for the action (required).
    /// </summary>
    /// <remarks>
    /// Format: Unique string identifier for the action type.
    /// Purpose: App uses ActionId to determine which action was tapped and respond accordingly.
    ///
    /// Naming Convention:
    /// - Use lowercase alphanumeric with underscores (e.g., "reply", "view_details", "confirm_order")
    /// - Keep concise but descriptive
    /// - Should map to action handler in app code
    ///
    /// Examples of Valid Values:
    /// - "reply" (message reply action)
    /// - "view_order" (view order details)
    /// - "confirm" (confirm action)
    /// - "dismiss" (dismiss notification)
    /// - "shop_now" (open shop)
    /// - "learn_more" (open information page)
    /// </remarks>
    public required string ActionId { get; set; }

    /// <summary>
    /// Gets or sets the action button display text (required).
    /// </summary>
    /// <remarks>
    /// Format: User-visible text displayed on the action button.
    /// Length: Recommended &lt;20 characters; exact limit varies by platform and device size.
    /// - Most platforms show full text if it fits
    /// - Very long text may be truncated or wrapped (varies by platform)
    ///
    /// Best Practices:
    /// - Use action-oriented, imperative verbs
    /// - Be concise and specific
    /// - Use title case or all caps for consistency
    /// - Avoid generic text
    ///
    /// Examples of Valid Values:
    /// - "Reply" (message action)
    /// - "View Order" (order details)
    /// - "Confirm" (confirmation action)
    /// - "Shop Now" (promotional action)
    /// - "Learn More" (informational action)
    /// - "Accept" (agreement/invitation action)
    /// - "Download" (file action)
    /// </remarks>
    public required string ActionTitle { get; set; }

    /// <summary>
    /// Gets or sets the deep link URL or app action URL (required).
    /// </summary>
    /// <remarks>
    /// Format: Deep link URI or web URL that specifies target of the action.
    /// Protocol: Typically "app://" for deep links or "https://" for web URLs.
    /// Purpose: Directs app to specific screen/handler when action is tapped.
    ///
    /// Deep Link Format (app-specific):
    /// - "app://messages/msg-12345" (view specific message)
    /// - "app://orders/ORD-54321" (view order details)
    /// - "app://shop/category/electronics" (browse category)
    /// - "app://settings/notifications" (notification settings)
    /// - Your app must have deep link routing configured to handle these
    ///
    /// Web URL Format (fallback or web apps):
    /// - "https://example.com/orders/ORD-54321"
    /// - "https://example.com/messages/msg-12345"
    /// - "https://example.com/products/item-electronics"
    /// - Typically used for web push notifications or app web fallback
    ///
    /// Best Practices:
    /// - Use deep links for specific, contextual actions (e.g., "view this order")
    /// - Avoid generic URLs (e.g., just "https://example.com")
    /// - Ensure URL is properly URL-encoded if parameters contain special characters
    /// - Test deep link routing before deploying notifications
    /// - Consider fallback behavior if deep link is not recognized
    ///
    /// Examples of Valid Values:
    /// - "app://messages/msg-12345"
    /// - "app://orders/ORD-54321/view"
    /// - "app://settings/notifications"
    /// - "https://example.com/orders/ORD-54321"
    /// - "https://example.com/products/electronics"
    /// </remarks>
    public required string ActionUrl { get; set; }

    /// <example>
    /// Message reply action:
    /// <code>
    /// new PushNotificationAction
    /// {
    ///     ActionId = "reply",
    ///     ActionTitle = "Reply",
    ///     ActionUrl = "app://messages/msg-12345/reply"
    /// }
    /// </code>
    ///
    /// Order action with web fallback:
    /// <code>
    /// new PushNotificationAction
    /// {
    ///     ActionId = "view_order",
    ///     ActionTitle = "View Order",
    ///     ActionUrl = "app://orders/ORD-54321" // or "https://example.com/orders/ORD-54321"
    /// }
    /// </code>
    ///
    /// Shop action from promotional notification:
    /// <code>
    /// new PushNotificationAction
    /// {
    ///     ActionId = "shop_sale",
    ///     ActionTitle = "Shop Now",
    ///     ActionUrl = "app://shop/sale" // or "https://example.com/shop/summer-sale-2024"
    /// }
    /// </code>
    /// </example>
}
