namespace SmartWorkz.Mobile.Notifications;

/// <summary>
/// Handles push notifications for a specific platform (iOS, Android, Windows).
/// Implementations manage platform-specific permission handling, device registration, and message processing.
/// </summary>
public interface IPushNotificationHandler
{
    /// <summary>
    /// Event raised when a push notification is received.
    /// </summary>
    event EventHandler<PushMessage>? NotificationReceived;

    /// <summary>
    /// Event raised when the permission status changes (granted, denied, unknown).
    /// </summary>
    event EventHandler<string>? PermissionStatusChanged;

    /// <summary>
    /// Gets the name of the platform this handler supports (e.g., "iOS", "Android", "Windows").
    /// </summary>
    string PlatformName { get; }

    /// <summary>
    /// Requests push notification permissions from the user.
    /// This is platform-specific: iOS shows a user dialog, Android checks runtime permissions, Windows doesn't require permissions.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the permission request.</param>
    /// <returns>True if permissions were granted; false otherwise.</returns>
    Task<bool> RequestPermissionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if push notification permissions are currently granted without prompting the user.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the check.</param>
    /// <returns>True if permissions are granted; false otherwise.</returns>
    Task<bool> HasPermissionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers the device with the platform's push notification service.
    /// This typically exchanges a device token with a backend service.
    /// </summary>
    /// <param name="deviceToken">The platform-specific device token (FCM token for Android, APNs token for iOS, etc).</param>
    /// <param name="cancellationToken">Token to cancel the registration.</param>
    /// <returns>A task representing the asynchronous registration operation.</returns>
    Task RegisterAsync(string deviceToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregisters the device from push notifications.
    /// Called when the user opts out or logs out.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the unregistration.</param>
    /// <returns>A task representing the asynchronous unregistration operation.</returns>
    Task UnregisterAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes an incoming push notification message.
    /// This is where the handler performs actions based on the message content and category.
    /// May involve updating UI, storing data, triggering navigation, etc.
    /// </summary>
    /// <param name="message">The push message to handle.</param>
    /// <param name="cancellationToken">Token to cancel the message handling.</param>
    /// <returns>A task representing the asynchronous message processing operation.</returns>
    Task HandleMessageAsync(PushMessage message, CancellationToken cancellationToken = default);
}
