#if __MACCATALYST__
using Microsoft.Extensions.Logging;

namespace SmartWorkz.Core.Mobile;

internal partial class PushNotificationClientService
{
    private partial async Task<string?> GetPushTokenAsyncPlatform(CancellationToken ct)
    {
        try
        {
            // macOS app delegate stores the token in SecureStorage when APNs registration completes
            var tokenResult = await _secureStorageService.GetAsync("apns::device_token", ct);
            return tokenResult.Succeeded ? tokenResult.Data : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get APNs token");
            return null;
        }
    }
}
#endif
