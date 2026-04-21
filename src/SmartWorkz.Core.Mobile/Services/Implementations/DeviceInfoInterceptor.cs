namespace SmartWorkz.Core.Mobile;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal class DeviceInfoInterceptor(
    IMobileContext mobileContext,
    IOptions<MobileApiConfig> options,
    ILogger logger) : IRequestInterceptor
{
    public async Task InterceptAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(mobileContext.DeviceId))
            request.Headers.TryAddWithoutValidation("X-Device-Id", mobileContext.DeviceId);

        if (!string.IsNullOrEmpty(mobileContext.Platform))
            request.Headers.TryAddWithoutValidation("X-Platform", mobileContext.Platform);

        if (!string.IsNullOrEmpty(options.Value.UserAgent))
            request.Headers.TryAddWithoutValidation("X-App-Version", options.Value.UserAgent);

        if (!string.IsNullOrEmpty(mobileContext.DeviceId) || !string.IsNullOrEmpty(mobileContext.Platform))
        {
            logger.LogDebug("Added device headers: DeviceId={DeviceId}, Platform={Platform}",
                mobileContext.DeviceId ?? "unset", mobileContext.Platform ?? "unset");
        }

        await Task.CompletedTask;
    }
}
