namespace SmartWorkz.Mobile;

public class AnalyticsService : IAnalyticsService
{
    private readonly ILogger _logger;

    public AnalyticsService(ILogger logger)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public Task TrackEventAsync(string eventName, Dictionary<string, object>? properties = null, CancellationToken ct = default)
    {
        _logger.LogDebug($"Analytics: Event '{eventName}' tracked (stub)");
        return Task.CompletedTask;
    }

    public Task TrackPageViewAsync(string pageName, Dictionary<string, object>? properties = null, CancellationToken ct = default)
    {
        _logger.LogDebug($"Analytics: Page '{pageName}' viewed (stub)");
        return Task.CompletedTask;
    }

    public Task TrackErrorAsync(Exception ex, Dictionary<string, object>? properties = null, CancellationToken ct = default)
    {
        _logger.LogDebug($"Analytics: Error tracked - {ex.Message} (stub)");
        return Task.CompletedTask;
    }

    public void SetUserId(string userId)
    {
        _logger.LogDebug($"Analytics: UserId set to '{userId}' (stub)");
    }

    public void SetUserProperty(string key, string value)
    {
        _logger.LogDebug($"Analytics: User property '{key}' set to '{value}' (stub)");
    }

    public Task ClearUserPropertiesAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Analytics stub: User properties cleared");
        return Task.CompletedTask;
    }

    public Task ResetAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Analytics stub: Analytics reset");
        return Task.CompletedTask;
    }
}
