#if WINDOWS
using Microsoft.Extensions.Logging;

namespace SmartWorkz.Mobile;

internal partial class PushNotificationClientService
{
    private partial async Task<string?> GetPushTokenAsyncPlatform(CancellationToken ct)
    {
        try
        {
            // Windows does not support push notifications via FCM or APNs
            _logger.LogWarning("Push notifications are not available on Windows platform");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get push token on Windows");
            return null;
        }
    }
}
#endif
