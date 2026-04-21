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
        request.Headers.TryAddWithoutValidation("X-Device-Id", mobileContext.DeviceId);
        request.Headers.TryAddWithoutValidation("X-Platform", mobileContext.Platform);
        request.Headers.TryAddWithoutValidation("X-App-Version", options.Value.UserAgent);

        logger.LogDebug("Added device headers: DeviceId={DeviceId}, Platform={Platform}",
            mobileContext.DeviceId, mobileContext.Platform);
        await Task.CompletedTask;
    }
}
