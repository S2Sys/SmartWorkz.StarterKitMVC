#if MACCATALYST || MACOS
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CoreLocation;
using Foundation;
using SmartWorkz.Mobile;
using SmartWorkz.Mobile.Services;

namespace SmartWorkz.Core.MAUI.Services;

/// <summary>
/// macOS and Mac Catalyst-specific location service using CoreLocation framework.
/// </summary>
public partial class LocationService : ILocationService
{
    private CLLocationManager? _locationManager;
    private LocationDelegate? _delegate;
    private TaskCompletionSource<Location>? _locationCompletionSource;

    /// <summary>
    /// Initializes the CLLocationManager.
    /// </summary>
    private CLLocationManager EnsureLocationManager()
    {
        if (_locationManager != null)
            return _locationManager;

        _locationManager = new CLLocationManager();
        _delegate = new LocationDelegate(this);
        _locationManager.Delegate = _delegate;

        return _locationManager;
    }

    /// <summary>
    /// Gets the current device location asynchronously.
    /// </summary>
    public async Task<Location> GetCurrentLocationAsync()
    {
        var manager = EnsureLocationManager();

        var status = CLLocationManager.Status;
        if (status == CLAuthorizationStatus.Denied ||
            status == CLAuthorizationStatus.Restricted)
            throw new UnauthorizedAccessException("Location access denied");

        if (status == CLAuthorizationStatus.NotDetermined)
        {
#if MACCATALYST
            manager.RequestWhenInUseAuthorization();
#else
            // macOS pre-10.14 doesn't require authorization request
            // For 10.14+, request when in use
            if (OperatingSystem.IsMacOS() && OperatingSystem.IsMacOSVersionAtLeast(10, 14))
                manager.RequestWhenInUseAuthorization();
#endif
        }

        _locationCompletionSource = new TaskCompletionSource<Location>();
        manager.StartUpdatingLocation();

        try
        {
            var task = _locationCompletionSource.Task;
            if (!task.Wait(TimeSpan.FromSeconds(30)))
            {
                throw new OperationCanceledException("Location acquisition timed out after 30 seconds");
            }
            return task.Result;
        }
        finally
        {
            manager.StopUpdatingLocation();
        }
    }

    /// <summary>
    /// Continuously monitors device location changes asynchronously.
    /// </summary>
    public async IAsyncEnumerable<Location> WatchLocationAsync(
        LocationAccuracy accuracy = LocationAccuracy.Best,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var manager = EnsureLocationManager();
        SetAccuracy(manager, accuracy);
        manager.StartUpdatingLocation();

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var tcs = new TaskCompletionSource<Location>();
                _locationCompletionSource = tcs;

                var location = await tcs.Task.ConfigureAwait(false);
                yield return location;
            }
        }
        finally
        {
            manager.StopUpdatingLocation();
        }
    }

    /// <summary>
    /// Not supported on macOS - location history requires custom implementation.
    /// </summary>
    public Task<List<Location>> GetLocationHistoryAsync(DateTime startDate, DateTime endDate)
    {
        throw new NotSupportedException(
            "Location history not supported on macOS. Enable background tracking and implement custom storage.");
    }

    /// <summary>
    /// Starts background location tracking.
    /// </summary>
    public Task StartBackgroundTrackingAsync()
    {
        EnsureLocationManager().StartUpdatingLocation();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops background location tracking.
    /// </summary>
    public Task StopBackgroundTrackingAsync()
    {
        EnsureLocationManager().StopUpdatingLocation();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets whether device has location services enabled.
    /// </summary>
    public Task<bool> IsLocationEnabledAsync()
    {
        var enabled = CLLocationManager.LocationServicesEnabled;
        return Task.FromResult(enabled);
    }

    /// <summary>
    /// Gets the current location permission status.
    /// </summary>
    public Task<PermissionStatus> GetPermissionStatusAsync()
    {
        var status = CLLocationManager.Status;
        return Task.FromResult(status switch
        {
#if MACCATALYST
            CLAuthorizationStatus.AuthorizedAlways => PermissionStatus.Always,
            CLAuthorizationStatus.AuthorizedWhenInUse => PermissionStatus.WhenInUse,
#else
            // macOS uses different authorization levels depending on version
            CLAuthorizationStatus.AuthorizedAlways => PermissionStatus.Always,
            CLAuthorizationStatus.AuthorizedWhenInUse => PermissionStatus.WhenInUse,
#endif
            CLAuthorizationStatus.Denied => PermissionStatus.Denied,
            _ => PermissionStatus.NotRequested
        });
    }

    /// <summary>Maps LocationAccuracy to iOS/macOS CLLocationManager accuracy constants.</summary>
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

    /// <summary>Converts NSDate to DateTime in UTC.</summary>
    private static DateTime NSDateToDateTime(NSDate nsDate)
    {
        return new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(nsDate.SecondsSinceReferenceDate);
    }

    /// <summary>Handles CLLocationManager delegate callbacks for location updates and errors.</summary>
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
                Timestamp = NSDateToDateTime(newLocation.Timestamp)
            };

            _service._locationCompletionSource?.SetResult(location);
        }

        public override void Failed(CLLocationManager manager, NSError error)
        {
            _service._locationCompletionSource?.SetException(
                new Exception($"Location failed: {error?.LocalizedDescription}"));
        }
    }
}
#endif
