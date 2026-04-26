using Microsoft.Extensions.Logging;

namespace SmartWorkz.Mobile.Notifications.Handlers;

/// <summary>
/// Android push notification handler using Firebase Cloud Messaging (FCM).
/// Manages FCM device tokens, permissions, and message reception on Android.
/// </summary>
#if !WINDOWS
public class AndroidPushHandler : IPushNotificationHandler
{
    private readonly ILogger<AndroidPushHandler> _logger;
    private string? _currentDeviceToken;
    private bool _permissionGranted;
    private readonly object _lockObject = new();

    public event EventHandler<PushMessage>? NotificationReceived;
    public event EventHandler<string>? PermissionStatusChanged;

    public string PlatformName => "Android";

    /// <summary>
    /// Initializes a new instance of the <see cref="AndroidPushHandler"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and debugging.</param>
    public AndroidPushHandler(ILogger<AndroidPushHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Requests push notification permissions from the user on Android.
    /// On Android 13+, requires runtime permission request for POST_NOTIFICATIONS.
    /// On Android 12 and below, notifications are enabled by default.
    /// </summary>
    public async Task<bool> RequestPermissionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Requesting Android push notification permissions");

            // On Android, notifications are enabled by default for API 32 and below
            // For Android 13+ (API 33+), we need to request POST_NOTIFICATIONS permission
            var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.PostNotifications>();
            }

            var granted = status == PermissionStatus.Granted;

            lock (_lockObject)
            {
                _permissionGranted = granted;
            }

            PermissionStatusChanged?.Invoke(this, granted ? "Granted" : "Denied");
            _logger.LogInformation("Android push notification permission {Status}", granted ? "granted" : "denied");

            return granted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting Android push notification permissions");
            throw;
        }
    }

    /// <summary>
    /// Checks if push notification permissions are currently granted.
    /// </summary>
    public async Task<bool> HasPermissionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
            var granted = status == PermissionStatus.Granted;

            lock (_lockObject)
            {
                _permissionGranted = granted;
            }

            return granted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Android push notification permissions");
            return false;
        }
    }

    /// <summary>
    /// Registers the device with FCM using the provided device token.
    /// This token should be obtained from Firebase Cloud Messaging.
    /// </summary>
    public async Task RegisterAsync(string deviceToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceToken, nameof(deviceToken));

        try
        {
            _logger.LogInformation("Registering Android device with FCM token: {Token}", deviceToken[..8] + "...");

            // Check permissions first
            var hasPermission = await HasPermissionAsync(cancellationToken);
            if (!hasPermission)
            {
                _logger.LogWarning("Attempting to register without notification permissions");
            }

            lock (_lockObject)
            {
                _currentDeviceToken = deviceToken;
            }

            // In a real implementation, this would:
            // 1. Send the FCM token to the backend
            // 2. Store the token securely
            // 3. Subscribe to FCM message handlers

            _logger.LogInformation("Android device registered successfully");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering Android device");
            throw;
        }
    }

    /// <summary>
    /// Unregisters the device from FCM.
    /// </summary>
    public async Task UnregisterAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Unregistering Android device from FCM");

            lock (_lockObject)
            {
                _currentDeviceToken = null;
            }

            // In a real implementation, this would:
            // 1. Send unregistration request to the backend
            // 2. Clear stored tokens
            // 3. Unsubscribe from FCM handlers

            _logger.LogInformation("Android device unregistered successfully");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering Android device");
            throw;
        }
    }

    /// <summary>
    /// Processes an incoming FCM message.
    /// In a real implementation, this would be called from the FCM message receiver.
    /// </summary>
    public async Task HandleMessageAsync(PushMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        try
        {
            _logger.LogInformation(
                "Android handler processing message {MessageId}: {Title}",
                message.Id,
                message.Title);

            // Raise the notification received event
            NotificationReceived?.Invoke(this, message);

            // In a real implementation, this would:
            // 1. Display a notification to the user (unless silent)
            // 2. Handle deep linking
            // 3. Update badge count
            // 4. Trigger any custom handlers based on message category

            if (!message.IsSilent)
            {
                _logger.LogDebug("Displaying Android notification for message {MessageId}", message.Id);
            }
            else
            {
                _logger.LogDebug("Silent notification received for message {MessageId}", message.Id);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Android push message {MessageId}", message.Id);
            throw;
        }
    }

    /// <summary>
    /// Gets the current FCM device token (for testing and diagnostics).
    /// </summary>
    public string? GetCurrentDeviceToken()
    {
        lock (_lockObject)
        {
            return _currentDeviceToken;
        }
    }
}
#endif
