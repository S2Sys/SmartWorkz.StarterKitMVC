namespace SmartWorkz.Mobile;

using System.Reactive.Subjects;
using ILogger = Microsoft.Extensions.Logging.ILogger;

/// <summary>
/// Provides location services for accessing device geolocation and location tracking.
/// </summary>
public partial class LocationService : ILocationService
{
    private readonly ILogger _logger;
    private readonly IPermissionService _permissions;
    private readonly Subject<GpsLocation> _locationSubject = new();
    private bool _isTracking;

    public bool IsTracking => _isTracking;

    public LocationService(ILogger logger, IPermissionService permissions)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
        _permissions = Guard.NotNull(permissions, nameof(permissions));
    }

    /// <summary>
    /// Gets the current device location.
    /// Permission must be granted before calling.
    /// </summary>
    public async Task<GpsLocation?> GetCurrentLocationAsync(CancellationToken ct = default)
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
    /// Starts continuous location tracking.
    /// Returns an observable stream of location updates.
    /// </summary>
    public IObservable<GpsLocation> StartTracking(LocationTrackingOptions? options = null)
    {
        try
        {
            if (_isTracking)
            {
                return _locationSubject;
            }

            _isTracking = true;
            StartTrackingPlatform(options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start location tracking");
        }

        return _locationSubject;
    }

    /// <summary>
    /// Stops continuous location tracking.
    /// </summary>
    public void StopTracking()
    {
        try
        {
            if (!_isTracking)
            {
                return;
            }

            _isTracking = false;
            StopTrackingPlatform();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop location tracking");
        }
    }

    /// <summary>
    /// Checks if location services are available and enabled on the device.
    /// </summary>
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            return await IsAvailableAsyncPlatform(CancellationToken.None);
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
    protected virtual void OnLocationChanged(GpsLocation location)
    {
        if (_isTracking)
        {
            _locationSubject.OnNext(location);
        }
    }

    // Platform-specific partial methods - implementation in platform-specific files
    private partial Task<GpsLocation?> GetCurrentLocationAsyncPlatform(CancellationToken ct);
    private partial void StartTrackingPlatform(LocationTrackingOptions? options);
    private partial void StopTrackingPlatform();
    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct);
}
