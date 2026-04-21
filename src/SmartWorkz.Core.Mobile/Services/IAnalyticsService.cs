namespace SmartWorkz.Core.Mobile;

public interface IAnalyticsService
{
    Task TrackEventAsync(string eventName, Dictionary<string, object>? properties = null, CancellationToken ct = default);
    Task TrackPageViewAsync(string pageName, Dictionary<string, object>? properties = null, CancellationToken ct = default);
    Task TrackErrorAsync(Exception ex, Dictionary<string, object>? properties = null, CancellationToken ct = default);
    void SetUserId(string userId);
    void SetUserProperty(string key, string value);
}
