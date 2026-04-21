namespace SmartWorkz.Core.Services.Notifications;

public class PushNotificationPayload
{
    public required string Title { get; set; }
    public required string Body { get; set; }
    public string? ImageUrl { get; set; }
    public Dictionary<string, string>? Data { get; set; }
    public PushNotificationAction? Action { get; set; }
    public int? Badge { get; set; }
}

public class PushNotificationAction
{
    public required string ActionId { get; set; }
    public required string ActionUrl { get; set; }
    public required string ActionTitle { get; set; }
}
