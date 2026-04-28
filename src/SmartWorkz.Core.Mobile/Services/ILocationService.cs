namespace SmartWorkz.Core.Mobile.Services;

using SmartWorkz.Mobile;

/// <summary>
/// Provides unified access to platform-specific location services (GPS, geolocation).
/// Supports single location retrieval, continuous monitoring, and location history.
/// </summary>
/// <remarks>
/// Implementation varies by platform:
/// - iOS: Uses CoreLocation.CLLocationManager
/// - Android: Uses Google Play Services Location API
/// - Windows: Uses Geolocator from Windows Runtime
/// - macOS: Uses CoreLocation.CLLocationManager
///
/// Always request permissions before calling location methods.
/// Battery usage increases significantly with higher accuracy levels and continuous monitoring.
/// </remarks>
public interface ILocationService
{
    /// <summary>
    /// Gets the current device location asynchronously.
    /// </summary>
    /// <returns>Location with latitude, longitude, and metadata.</returns>
    /// <exception cref="OperationCanceledException">If location retrieval times out.</exception>
    /// <remarks>
    /// First call may take several seconds while GPS acquires signal lock.
    /// Subsequent calls may return cached location if within time threshold.
    /// Respects accuracy settings and may retry internally.
    /// </remarks>
    Task<Location> GetCurrentLocationAsync();

    /// <summary>
    /// Continuously monitors device location changes asynchronously.
    /// </summary>
    /// <param name="accuracy">Desired accuracy level (affects battery usage).</param>
    /// <returns>Async enumerable that yields locations as they are acquired.</returns>
    /// <remarks>
    /// This method does not complete on its own; continue enumerating until manually cancelled.
    /// Usage:
    /// <code>
    /// await foreach (var location in locationService.WatchLocationAsync())
    /// {
    ///     Console.WriteLine($"Location: {location.Latitude}, {location.Longitude}");
    /// }
    /// </code>
    /// </remarks>
    IAsyncEnumerable<Location> WatchLocationAsync(
        LocationAccuracy accuracy = LocationAccuracy.Best);

    /// <summary>
    /// Retrieves location history for a date range.
    /// </summary>
    /// <param name="startDate">Start date (inclusive).</param>
    /// <param name="endDate">End date (inclusive).</param>
    /// <returns>List of locations recorded within date range.</returns>
    /// <remarks>Availability depends on whether background tracking is enabled.</remarks>
    Task<List<Location>> GetLocationHistoryAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Starts background location tracking for offline synchronization.
    /// </summary>
    /// <remarks>
    /// Requires 'Always' permission level on iOS and backgroundLocation permission on Android.
    /// Enable based on use case; background tracking consumes significant battery.
    /// </remarks>
    Task StartBackgroundTrackingAsync();

    /// <summary>
    /// Stops background location tracking.
    /// </summary>
    Task StopBackgroundTrackingAsync();

    /// <summary>
    /// Gets whether device has location services enabled.
    /// </summary>
    Task<bool> IsLocationEnabledAsync();

    /// <summary>
    /// Gets the current location permission status.
    /// </summary>
    Task<PermissionStatus> GetPermissionStatusAsync();
}

/// <summary>Permission status for location access.</summary>
public enum PermissionStatus
{
    /// <summary>Permission not yet requested.</summary>
    NotRequested = 0,

    /// <summary>Permission denied by user or system policy.</summary>
    Denied = 1,

    /// <summary>Permission granted only when app is in use.</summary>
    WhenInUse = 2,

    /// <summary>Permission granted always, including background.</summary>
    Always = 3
}
