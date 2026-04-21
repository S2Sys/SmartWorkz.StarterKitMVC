namespace SmartWorkz.Core.Mobile;

using Microsoft.Extensions.Logging;

internal class CorrelationInterceptor(ILogger logger) : IRequestInterceptor
{
    public async Task InterceptAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        var correlationId = Guid.NewGuid().ToString("D");
        request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
        logger.LogDebug("Added correlation ID: {CorrelationId}", correlationId);
        await Task.CompletedTask;
    }
}
