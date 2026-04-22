namespace SmartWorkz.Mobile;

using ILogger = Microsoft.Extensions.Logging.ILogger;

/// <summary>
/// Provides location services for accessing device geolocation and location tracking.
/// </summary>
public partial class LocationService : ILocationService
{
    private readonly ILogger _logger;
    private readonly IPermissionService _permissions;

    public event EventHandler<Location>? LocationChanged;

    public LocationService(ILogger logger, IPermissionService permissions)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
        _permissions = Guard.NotNull(permissions, nameof(permissions));
    }

    /// <summary>
    /// Gets the current device location.
    /// Permission must be granted before calling.
    /// </summary>
    public async Task<Location?> GetCurrentLocationAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var permissionStatus = await _permissions.CheckAsync(MobilePermission.Location, ct);
            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await _permissions.RequestAsync(MobilePermission.Location, ct);
            }

            if (permissionStatus != PermissionStatus.Granted)
            {
                _logger.LogWarning("Location permission denied");
                return null;
            }

            return await GetCurrentLocationAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current location");
            return null;
        }
    }

    /// <summary>
    /// Starts continuous location tracking with periodic updates.
    /// Permission must be granted before calling.
    /// </summary>
    public async Task StartTrackingAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var permissionStatus = await _permissions.CheckAsync(MobilePermission.Location, ct);
            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await _permissions.RequestAsync(MobilePermission.Location, ct);
            }

            if (permissionStatus != PermissionStatus.Granted)
            {
                _logger.LogWarning("Location permission denied for tracking");
                return;
            }

            await StartTrackingAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start location tracking");
        }
    }

    /// <summary>
    /// Stops continuous location tracking.
    /// </summary>
    public async Task StopTrackingAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            await StopTrackingAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop location tracking");
        }
    }

    /// <summary>
    /// Checks if location services are available and enabled on the device.
    /// </summary>
    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            return await IsAvailableAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check location service availability");
            return false;
        }
    }

    /// <summary>
    /// Raises the LocationChanged event when a location update occurs.
    /// </summary>
    protected virtual void OnLocationChanged(Location location)
    {
        LocationChanged?.Invoke(this, location);
    }

    // Platform-specific partial methods - implementation in platform-specific files
    private partial Task<Location?> GetCurrentLocationAsyncPlatform(CancellationToken ct);
    private partial Task StartTrackingAsyncPlatform(CancellationToken ct);
    private partial Task StopTrackingAsyncPlatform(CancellationToken ct);
    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct);
}
