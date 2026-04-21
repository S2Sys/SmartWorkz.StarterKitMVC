namespace SmartWorkz.Core.Services.Notifications;

/// <summary>
/// Represents the payload data for a push notification.
/// </summary>
public class PushNotificationPayload
{
    /// <summary>
    /// Gets or sets the notification title. Required.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the notification body text. Required.
    /// </summary>
    public required string Body { get; set; }

    /// <summary>
    /// Gets or sets the optional URL to a notification image.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets custom key-value data pairs to be included in the notification.
    /// </summary>
    public Dictionary<string, string>? Data { get; set; }

    /// <summary>
    /// Gets or sets the optional action associated with the notification.
    /// </summary>
    public PushNotificationAction? Action { get; set; }

    /// <summary>
    /// Gets or sets the notification badge count for iOS notifications.
    /// </summary>
    public int? Badge { get; set; }
}

/// <summary>
/// Represents an action that can be performed from a push notification.
/// </summary>
public class PushNotificationAction
{
    /// <summary>
    /// Gets or sets the unique identifier for this action. Required.
    /// </summary>
    public required string ActionId { get; set; }

    /// <summary>
    /// Gets or sets the URL to navigate to when the action is triggered. Required.
    /// </summary>
    public required string ActionUrl { get; set; }

    /// <summary>
    /// Gets or sets the display title for this action. Required.
    /// </summary>
    public required string ActionTitle { get; set; }
}
