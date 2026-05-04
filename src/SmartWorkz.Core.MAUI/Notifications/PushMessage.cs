namespace SmartWorkz.Mobile.Notifications;

/// <summary>
/// Represents a push notification message received from a push notification service.
/// This is the core data model for the push notification pipeline.
/// </summary>
public class PushMessage
{
    /// <summary>
    /// Gets the unique identifier for this message.
    /// Typically provided by the push notification service (FCM, APNS, etc).
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the notification title displayed to the user.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the notification body/content displayed to the user.
    /// </summary>
    public string Body { get; init; } = string.Empty;

    /// <summary>
    /// Gets the category/type of message for routing and handling.
    /// Examples: "order", "alert", "promotion", "system"
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Gets custom data fields associated with the message.
    /// Platform-specific custom data from the push service.
    /// </summary>
    public Dictionary<string, string>? Data { get; init; }

    /// <summary>
    /// Gets the timestamp when this message was received by the client.
    /// </summary>
    public DateTime ReceivedAt { get; init; }

    /// <summary>
    /// Gets the deep link/navigation target for when the user taps the notification.
    /// Format: app-specific URI scheme or path (e.g., "app://orders/123")
    /// </summary>
    public string? DeepLink { get; init; }

    /// <summary>
    /// Gets the badge count to display on the app icon.
    /// </summary>
    public int BadgeCount { get; init; }

    /// <summary>
    /// Gets whether this is a silent notification (no user-visible notification).
    /// Used for background data sync notifications.
    /// </summary>
    public bool IsSilent { get; init; }

    /// <summary>
    /// Gets when this message expires and should be discarded.
    /// If null, message does not expire.
    /// </summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PushMessage"/> class with default values.
    /// </summary>
    public PushMessage()
    {
        ReceivedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new PushMessage instance from a builder pattern for fluent construction.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public static Builder CreateBuilder() => new();

    /// <summary>
    /// Builder class for fluent construction of PushMessage instances.
    /// </summary>
    public class Builder
    {
        private string _id = string.Empty;
        private string _title = string.Empty;
        private string _body = string.Empty;
        private string? _category;
        private Dictionary<string, string>? _data;
        private string? _deepLink;
        private int _badgeCount;
        private bool _isSilent;
        private DateTime? _expiresAt;

        /// <summary>Sets the message ID.</summary>
        public Builder WithId(string id)
        {
            _id = id ?? string.Empty;
            return this;
        }

        /// <summary>Sets the notification title.</summary>
        public Builder WithTitle(string title)
        {
            _title = title ?? string.Empty;
            return this;
        }

        /// <summary>Sets the notification body.</summary>
        public Builder WithBody(string body)
        {
            _body = body ?? string.Empty;
            return this;
        }

        /// <summary>Sets the message category.</summary>
        public Builder WithCategory(string? category)
        {
            _category = category;
            return this;
        }

        /// <summary>Sets the custom data.</summary>
        public Builder WithData(Dictionary<string, string>? data)
        {
            _data = data;
            return this;
        }

        /// <summary>Adds a single data entry.</summary>
        public Builder AddData(string key, string value)
        {
            _data ??= new();
            _data[key] = value;
            return this;
        }

        /// <summary>Sets the deep link.</summary>
        public Builder WithDeepLink(string? deepLink)
        {
            _deepLink = deepLink;
            return this;
        }

        /// <summary>Sets the badge count.</summary>
        public Builder WithBadgeCount(int badgeCount)
        {
            _badgeCount = badgeCount;
            return this;
        }

        /// <summary>Marks this as a silent notification.</summary>
        public Builder AsSilent(bool silent = true)
        {
            _isSilent = silent;
            return this;
        }

        /// <summary>Sets the expiration time.</summary>
        public Builder WithExpiresAt(DateTime? expiresAt)
        {
            _expiresAt = expiresAt;
            return this;
        }

        /// <summary>Builds and returns the PushMessage instance.</summary>
        public PushMessage Build()
        {
            return new PushMessage
            {
                Id = _id,
                Title = _title,
                Body = _body,
                Category = _category,
                Data = _data,
                DeepLink = _deepLink,
                BadgeCount = _badgeCount,
                IsSilent = _isSilent,
                ExpiresAt = _expiresAt,
                ReceivedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Determines if this message has expired.
    /// </summary>
    /// <returns>True if ExpiresAt is set and is before the current time; otherwise false.</returns>
    public bool IsExpired() => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;

    /// <summary>
    /// Gets the value of a custom data field, or null if not found.
    /// </summary>
    public string? GetDataValue(string key)
    {
        if (Data == null)
        {
            return null;
        }
        Data.TryGetValue(key, out var value);
        return value;
    }

    /// <summary>
    /// Determines if this message has any custom data.
    /// </summary>
    public bool HasData => Data != null && Data.Count > 0;
}
