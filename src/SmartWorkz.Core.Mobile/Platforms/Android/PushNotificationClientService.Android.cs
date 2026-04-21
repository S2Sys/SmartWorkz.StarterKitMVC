#if __ANDROID__
using Firebase.Messaging;

namespace SmartWorkz.Core.Mobile;

internal partial class PushNotificationClientService
{
    private partial async Task<string?> GetPushTokenAsyncPlatform(CancellationToken ct)
    {
        try
        {
            var token = await FirebaseMessaging.Instance.GetToken();
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get FCM token");
            return null;
        }
    }
}
#endif
