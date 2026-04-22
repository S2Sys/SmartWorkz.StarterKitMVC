namespace SmartWorkz.Mobile;

#if __IOS__
using CoreLocation;
using Foundation;
using SmartWorkz.Shared;
using System.Threading;
using System.Threading.Tasks;

partial class GeofencingService
{
    private CLLocationManager? _locationManager;
    private NSObject? _didEnterRegionObserver;
    private NSObject? _didExitRegionObserver;

    private partial async Task<Result<bool>> StartMonitoringAsyncPlatform(GeofenceRegion region, CancellationToken ct)
    {
        try
        {
            _locationManager ??= new CLLocationManager();

            if (CLLocationManager.LocationServicesEnabled == false)
            {
                return Result.Fail<bool>(Error.Validation("LOCATION.SERVICES_DISABLED",
                    "Location services are disabled"));
            }

            var clRegion = new CLCircularRegion(
                new CLLocationCoordinate2D(region.Latitude, region.Longitude),
                region.RadiusMeters,
                region.Id);

            clRegion.NotifyOnEntry = true;
            clRegion.NotifyOnExit = true;

            _locationManager.StartMonitoring(clRegion);

            _didEnterRegionObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                new NSString("CLRegionDidEnterNotification"),
                (notification) =>
                {
                    if (notification?.Object is CLRegion clr)
                    {
                        RaiseGeofenceEvent(new GeofenceEvent(
                            RegionId: clr.Identifier,
                            EventType: 1, // ENTER
                            DetectedAt: DateTime.UtcNow,
                            CurrentLatitude: _locationManager.Location?.Coordinate.Latitude ?? 0,
                            CurrentLongitude: _locationManager.Location?.Coordinate.Longitude ?? 0));
                    }
                });

            _didExitRegionObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                new NSString("CLRegionDidExitNotification"),
                (notification) =>
                {
                    if (notification?.Object is CLRegion clr)
                    {
                        RaiseGeofenceEvent(new GeofenceEvent(
                            RegionId: clr.Identifier,
                            EventType: 0, // EXIT
                            DetectedAt: DateTime.UtcNow,
                            CurrentLatitude: _locationManager.Location?.Coordinate.Latitude ?? 0,
                            CurrentLongitude: _locationManager.Location?.Coordinate.Longitude ?? 0));
                    }
                });

            return await Task.FromResult(Result.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "iOS geofence start failed");
            return Result.Fail<bool>(Error.FromException(ex));
        }
    }

    private partial async Task<Result<bool>> StopMonitoringAsyncPlatform(string regionId, CancellationToken ct)
    {
        try
        {
            if (_locationManager == null)
                return await Task.FromResult(Result.Ok(true));

            var monitoredRegions = _locationManager.MonitoredRegions;
            foreach (var region in monitoredRegions)
            {
                if (region.Identifier == regionId)
                {
                    _locationManager.StopMonitoring(region);
                    break;
                }
            }

            // Only dispose observers if no more regions are being monitored
            if (_locationManager.MonitoredRegions.Count == 0)
            {
                _didEnterRegionObserver?.Dispose();
                _didExitRegionObserver?.Dispose();
                _didEnterRegionObserver = null;
                _didExitRegionObserver = null;
            }

            return await Task.FromResult(Result.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "iOS geofence stop failed");
            return Result.Fail<bool>(Error.FromException(ex));
        }
    }

    private partial async Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        return await Task.FromResult(CLLocationManager.LocationServicesEnabled);
    }
}
#endif
