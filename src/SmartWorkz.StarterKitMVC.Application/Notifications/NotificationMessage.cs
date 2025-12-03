namespace SmartWorkz.StarterKitMVC.Application.Notifications;

/// <summary>
/// Defines the delivery channel for a notification.
/// </summary>
public enum NotificationChannel
{
    /// <summary>Email notification.</summary>
    Email,
    /// <summary>SMS text message.</summary>
    Sms,
    /// <summary>Push notification (mobile/web).</summary>
    Push
}

/// <summary>
/// Represents a notification message to be sent.
/// </summary>
/// <param name="Channel">Delivery channel (Email, SMS, Push).</param>
/// <param name="Recipient">Recipient address (email, phone, device token).</param>
/// <param name="Subject">Message subject (for email).</param>
/// <param name="Body">Message body content.</param>
/// <param name="Metadata">Optional key-value metadata.</param>
/// <example>
/// <code>
/// var email = new NotificationMessage(
///     Channel: NotificationChannel.Email,
///     Recipient: "user@example.com",
///     Subject: "Welcome!",
///     Body: "Thank you for signing up."
/// );
/// 
/// var sms = new NotificationMessage(
///     Channel: NotificationChannel.Sms,
///     Recipient: "+1234567890",
///     Subject: "",
///     Body: "Your code is 123456"
/// );
/// </code>
/// </example>
public sealed record NotificationMessage(
    NotificationChannel Channel,
    string Recipient,
    string Subject,
    string Body,
    IDictionary<string, string>? Metadata = null
);
