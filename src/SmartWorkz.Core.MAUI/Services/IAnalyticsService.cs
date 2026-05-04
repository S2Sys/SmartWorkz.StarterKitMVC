namespace SmartWorkz.Mobile;

public interface IAnalyticsService
{
    Task TrackEventAsync(string eventName, Dictionary<string, object>? properties = null, CancellationToken ct = default);
    Task TrackPageViewAsync(string pageName, Dictionary<string, object>? properties = null, CancellationToken ct = default);
    Task TrackErrorAsync(Exception ex, Dictionary<string, object>? properties = null, CancellationToken ct = default);
    void SetUserId(string userId);
    void SetUserProperty(string key, string value);
    Task ClearUserPropertiesAsync(CancellationToken ct = default);
    Task ResetAsync(CancellationToken ct = default);
}
