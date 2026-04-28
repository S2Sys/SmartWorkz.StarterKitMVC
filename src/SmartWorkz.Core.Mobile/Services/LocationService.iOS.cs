#if IOS
using CoreLocation;
using Foundation;
using SmartWorkz.Mobile;
using SmartWorkz.Mobile.Services;

namespace SmartWorkz.Core.Mobile.Services;

/// <summary>
/// iOS-specific location service using CoreLocation framework.
/// </summary>
public partial class LocationService : ILocationService
{
    private CLLocationManager? _locationManager;
    private TaskCompletionSource<Location>? _locationCompletionSource;

    private CLLocationManager EnsureLocationManager()
    {
        if (_locationManager != null)
            return _locationManager;

        _locationManager = new CLLocationManager();
        _locationManager.WeakDelegate = new LocationDelegate(this);

        return _locationManager;
    }

    public async Task<Location> GetCurrentLocationAsync()
    {
        var manager = EnsureLocationManager();

        var status = CLLocationManager.Status;
        if (status == CLAuthorizationStatus.Denied ||
            status == CLAuthorizationStatus.Restricted)
            throw new UnauthorizedAccessException("Location access denied");

        if (status == CLAuthorizationStatus.NotDetermined)
            manager.RequestWhenInUseAuthorization();

        _locationCompletionSource = new TaskCompletionSource<Location>();
        manager.StartUpdatingLocation();

        var location = await _locationCompletionSource.Task.ConfigureAwait(false);
        manager.StopUpdatingLocation();

        return location;
    }

    public async IAsyncEnumerable<Location> WatchLocationAsync(
        LocationAccuracy accuracy = LocationAccuracy.Best)
    {
        var manager = EnsureLocationManager();
        SetAccuracy(manager, accuracy);

        manager.StartUpdatingLocation();

        while (true)
        {
            var tcs = new TaskCompletionSource<Location>();
            _locationCompletionSource = tcs;

            var location = await tcs.Task.ConfigureAwait(false);
            yield return location;
        }
    }

    public Task<List<Location>> GetLocationHistoryAsync(DateTime startDate, DateTime endDate)
    {
        // iOS: Not natively supported; would require custom implementation
        throw new NotSupportedException("Location history not supported on iOS");
    }

    public Task StartBackgroundTrackingAsync()
    {
        EnsureLocationManager().StartUpdatingLocation();
        return Task.CompletedTask;
    }

    public Task StopBackgroundTrackingAsync()
    {
        EnsureLocationManager().StopUpdatingLocation();
        return Task.CompletedTask;
    }

    public Task<bool> IsLocationEnabledAsync()
    {
        var enabled = CLLocationManager.LocationServicesEnabled;
        return Task.FromResult(enabled);
    }

    public async Task<PermissionStatus> GetPermissionStatusAsync()
    {
        var status = CLLocationManager.Status;
        return status switch
        {
            CLAuthorizationStatus.AuthorizedAlways => PermissionStatus.Always,
            CLAuthorizationStatus.AuthorizedWhenInUse => PermissionStatus.WhenInUse,
            CLAuthorizationStatus.Denied => PermissionStatus.Denied,
            _ => PermissionStatus.NotRequested
        };
    }

    private static void SetAccuracy(CLLocationManager manager, LocationAccuracy accuracy)
    {
        manager.DesiredAccuracy = accuracy switch
        {
            LocationAccuracy.Lowest => CLLocation.AccuracyThreeKilometers,
            LocationAccuracy.Low => CLLocation.AccuracyKilometer,
            LocationAccuracy.Medium => CLLocation.AccuracyHundredMeters,
            LocationAccuracy.High => CLLocation.AccuracyTenMeters,
            LocationAccuracy.Best => CLLocation.AccuracyBest,
            _ => CLLocation.AccuracyBest
        };
    }

    private class LocationDelegate : CLLocationManagerDelegate
    {
        private readonly LocationService _service;

        public LocationDelegate(LocationService service) => _service = service;

        public override void UpdatedLocation(CLLocationManager manager, CLLocation newLocation, CLLocation? oldLocation)
        {
            var location = new Location
            {
                Latitude = newLocation.Coordinate.Latitude,
                Longitude = newLocation.Coordinate.Longitude,
                Accuracy = newLocation.HorizontalAccuracy >= 0 ? newLocation.HorizontalAccuracy : null,
                Altitude = newLocation.Altitude >= 0 ? newLocation.Altitude : null,
                AltitudeAccuracy = newLocation.VerticalAccuracy >= 0 ? newLocation.VerticalAccuracy : null,
                Timestamp = DateTime.UtcNow
            };

            _service._locationCompletionSource?.SetResult(location);
        }

        public override void Failed(CLLocationManager manager, NSError error)
        {
            _service._locationCompletionSource?.SetException(
                new Exception($"Location failed: {error.LocalizedDescription}"));
        }
    }
}
#endif
