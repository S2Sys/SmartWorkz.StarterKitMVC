namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Service for tracking analytics events
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Track a custom event with optional properties
    /// </summary>
    Task TrackEventAsync(string eventName, object? properties = null, CancellationToken ct = default);
}
