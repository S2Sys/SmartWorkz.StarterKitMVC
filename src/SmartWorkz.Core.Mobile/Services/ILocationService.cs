namespace SmartWorkz.Mobile;

/// <summary>
/// Cross-platform location service for accessing device geolocation and location tracking.
/// Handles permission checks and platform-specific location access.
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Gets the current device location.
    /// Permission must be granted before calling.
    /// </summary>
    /// <returns>Location with latitude and longitude, or null if unavailable</returns>
    Task<Location?> GetCurrentLocationAsync(CancellationToken ct = default);

    /// <summary>
    /// Starts continuous location tracking with periodic updates.
    /// Permission must be granted before calling.
    /// </summary>
    Task StartTrackingAsync(CancellationToken ct = default);

    /// <summary>
    /// Stops continuous location tracking.
    /// </summary>
    Task StopTrackingAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks if location services are available and enabled on the device.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Event raised when location is updated during tracking.
    /// </summary>
    event EventHandler<Location>? LocationChanged;
}
