namespace SmartWorkz.Mobile;

/// <summary>
/// Service for accessing device location via GPS.
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Gets the current device location.
    /// Requires LOCATION or FINE_LOCATION permission (Android) or Location permission (iOS).
    /// </summary>
    Task<GpsLocation?> GetCurrentLocationAsync(CancellationToken ct = default);

    /// <summary>
    /// Starts continuous location tracking.
    /// Requires LOCATION or FINE_LOCATION permission (Android) or Location permission (iOS).
    /// Returns an observable stream of location updates.
    /// </summary>
    IObservable<GpsLocation> StartTracking(LocationTrackingOptions? options = null);

    /// <summary>
    /// Stops continuous location tracking.
    /// </summary>
    void StopTracking();

    /// <summary>
    /// Checks if location service is available on this platform.
    /// </summary>
    Task<bool> IsAvailableAsync();

    /// <summary>
    /// Checks if location tracking is currently active.
    /// </summary>
    bool IsTracking { get; }
}

/// <summary>
/// Options for configuring location tracking behavior.
/// </summary>
public sealed record LocationTrackingOptions(
    /// <summary>Minimum distance in meters between location updates (0 = all updates)</summary>
    double? MinimumDistanceMeters = null,

    /// <summary>Minimum time in milliseconds between location updates (0 = all updates)</summary>
    int? MinimumTimeMilliseconds = null,

    /// <summary>Desired accuracy level</summary>
    LocationAccuracy? Accuracy = LocationAccuracy.Default);

/// <summary>
/// Location accuracy levels for platform-specific configuration.
/// </summary>
public enum LocationAccuracy
{
    /// <summary>Default accuracy for the platform</summary>
    Default = 0,

    /// <summary>Low accuracy, better battery life</summary>
    Low = 1,

    /// <summary>Medium accuracy, balanced battery/accuracy</summary>
    Medium = 2,

    /// <summary>High accuracy, best location precision, worse battery life</summary>
    High = 3
}
