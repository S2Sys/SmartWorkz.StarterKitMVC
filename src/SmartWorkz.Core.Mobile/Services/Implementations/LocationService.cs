namespace SmartWorkz.Mobile;

using ILogger = Microsoft.Extensions.Logging.ILogger;

/// <summary>
/// Cross-platform location service providing device geolocation and location tracking.
/// Platform-specific implementations are provided in partial class files (iOS, Android, Windows, macOS).
/// </summary>
/// <remarks>
/// This class provides the ILocationService interface implementation.
/// Each platform overrides the required methods via partial class definitions in platform-specific files.
///
/// Type System: Uses Location (class with rich metadata) for the interface contract.
/// This allows full location data (heading, speed, altitude accuracy) across all platforms.
/// </remarks>
public partial class LocationService : ILocationService
{
    protected readonly ILogger Logger;
    private readonly IPermissionService? _permissionService;

    /// <summary>
    /// Initializes a new instance of the LocationService.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output</param>
    /// <param name="permissionService">Optional permission service for legacy compatibility</param>
    public LocationService(ILogger logger, IPermissionService? permissionService = null)
    {
        Logger = Guard.NotNull(logger, nameof(logger));
        _permissionService = permissionService;
    }

    /// <summary>
    /// Gets the current device location asynchronously.
    /// Platform implementations override this method.
    /// </summary>
    public virtual Task<Location> GetCurrentLocationAsync()
    {
        throw new NotImplementedException($"Location services not available on this platform ({GetPlatformName()})");
    }

    /// <summary>
    /// Continuously monitors device location changes asynchronously.
    /// Platform implementations override this method.
    /// </summary>
    public virtual async IAsyncEnumerable<Location> WatchLocationAsync(
        LocationAccuracy accuracy = LocationAccuracy.Best,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        throw new NotImplementedException($"Location watching not available on this platform ({GetPlatformName()})");
        yield break; // Never executed but required for async generator syntax
    }

    /// <summary>
    /// Retrieves location history for a date range.
    /// Platform implementations may override this method if history tracking is supported.
    /// </summary>
    public virtual Task<List<Location>> GetLocationHistoryAsync(DateTime startDate, DateTime endDate)
    {
        throw new NotSupportedException($"Location history not supported on this platform ({GetPlatformName()})");
    }

    /// <summary>
    /// Starts background location tracking for offline synchronization.
    /// Platform implementations may override this method.
    /// </summary>
    public virtual Task StartBackgroundTrackingAsync()
    {
        throw new NotSupportedException($"Background location tracking not supported on this platform ({GetPlatformName()})");
    }

    /// <summary>
    /// Stops background location tracking.
    /// Platform implementations may override this method.
    /// </summary>
    public virtual Task StopBackgroundTrackingAsync()
    {
        throw new NotSupportedException($"Background location tracking not supported on this platform ({GetPlatformName()})");
    }

    /// <summary>
    /// Checks if location services are available and enabled on the device.
    /// Platform implementations override this method.
    /// </summary>
    public virtual Task<bool> IsLocationEnabledAsync()
    {
        return Task.FromResult(false);
    }

    /// <summary>
    /// Gets the current location permission status.
    /// Platform implementations override this method.
    /// </summary>
    public virtual Task<PermissionStatus> GetPermissionStatusAsync()
    {
        return Task.FromResult(PermissionStatus.NotRequested);
    }

    /// <summary>
    /// Gets a human-readable name for the current platform.
    /// </summary>
    private static string GetPlatformName() =>
        OperatingSystem.IsIOS() ? "iOS" :
        OperatingSystem.IsAndroid() ? "Android" :
        OperatingSystem.IsWindows() ? "Windows" :
        OperatingSystem.IsMacOS() ? "macOS" :
        "Unknown";
}
