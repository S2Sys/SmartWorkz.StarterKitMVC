#if __ANDROID__
using Firebase.Messaging;
using Microsoft.Extensions.Logging;

namespace SmartWorkz.Mobile;

internal partial class PushNotificationClientService
{
    private partial async Task<string?> GetPushTokenAsyncPlatform(CancellationToken ct)
    {
        try
        {
            var task = FirebaseMessaging.Instance.GetToken();
            var token = (await (Task<string>)(object)task);
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
