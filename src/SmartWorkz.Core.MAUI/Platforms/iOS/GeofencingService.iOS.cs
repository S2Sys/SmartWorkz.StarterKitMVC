namespace SmartWorkz.Mobile;

#if __IOS__
using CoreLocation;
using Foundation;
using SmartWorkz.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

partial class GeofencingService
{
    // EventType constants - Issue 7
    private const int EventTypeEnter = 1;
    private const int EventTypeExit = 0;

    private CLLocationManager? _locationManager;
    // Issue 1: Per-region observer dictionary instead of single fields
    private readonly Dictionary<string, (NSObject enter, NSObject exit)> _observersByRegion = new();
    // Issue 2: Thread safety lock for observer access
    private readonly object _lockObject = new();

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

            // Issue 8: Check authorization status
            if (_locationManager.AuthorizationStatus != CLAuthorizationStatus.Authorized &&
                _locationManager.AuthorizationStatus != CLAuthorizationStatus.AuthorizedAlways &&
                _locationManager.AuthorizationStatus != CLAuthorizationStatus.AuthorizedWhenInUse)
            {
                return Result.Fail<bool>(Error.Validation("LOCATION.NOT_AUTHORIZED",
                    "Location permission not granted"));
            }

            var clRegion = new CLCircularRegion(
                new CLLocationCoordinate2D(region.Latitude, region.Longitude),
                region.RadiusMeters,
                region.Id);

            clRegion.NotifyOnEntry = true;
            clRegion.NotifyOnExit = true;

            // Issue 3: Thread-safe observer registration with exception handling in outer try-catch
            NSObject? enterObserver = null;
            NSObject? exitObserver = null;

            // Issue 2 & 4: Thread-safe registration, only register once
            lock (_lockObject)
            {
                if (!_observersByRegion.ContainsKey(region.Id))
                {
                    // Register observers only once, on first region
                    if (_observersByRegion.Count == 0)
                    {
                        enterObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                            new NSString("CLRegionDidEnterNotification"),
                            (notification) =>
                            {
                                if (notification?.Object is CLRegion clr)
                                {
                                    var latitude = _locationManager?.Location?.Coordinate.Latitude ?? 0;
                                    var longitude = _locationManager?.Location?.Coordinate.Longitude ?? 0;

                                    // Issue 5: Log if location is unavailable
                                    if (_locationManager?.Location == null)
                                    {
                                        _logger.LogWarning(
                                            "Location unavailable for enter geofence event on region {RegionId}",
                                            clr.Identifier);
                                    }

                                    RaiseGeofenceEvent(new GeofenceEvent(
                                        RegionId: clr.Identifier,
                                        EventType: EventTypeEnter, // Issue 7: Use constant
                                        DetectedAt: DateTime.UtcNow,
                                        CurrentLatitude: latitude,
                                        CurrentLongitude: longitude));
                                }
                            });

                        exitObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                            new NSString("CLRegionDidExitNotification"),
                            (notification) =>
                            {
                                if (notification?.Object is CLRegion clr)
                                {
                                    var latitude = _locationManager?.Location?.Coordinate.Latitude ?? 0;
                                    var longitude = _locationManager?.Location?.Coordinate.Longitude ?? 0;

                                    // Issue 5: Log if location is unavailable
                                    if (_locationManager?.Location == null)
                                    {
                                        _logger.LogWarning(
                                            "Location unavailable for exit geofence event on region {RegionId}",
                                            clr.Identifier);
                                    }

                                    RaiseGeofenceEvent(new GeofenceEvent(
                                        RegionId: clr.Identifier,
                                        EventType: EventTypeExit, // Issue 7: Use constant
                                        DetectedAt: DateTime.UtcNow,
                                        CurrentLatitude: latitude,
                                        CurrentLongitude: longitude));
                                }
                            });

                        if (enterObserver != null && exitObserver != null)
                        {
                            _observersByRegion[region.Id] = (enterObserver, exitObserver);
                        }
                    }
                    else
                    {
                        // Region already has an observer pair from the global registration
                        var existingPair = _observersByRegion.Values.First();
                        _observersByRegion[region.Id] = existingPair;
                    }
                }
            }

            _locationManager.StartMonitoring(clRegion);

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

            // Issue 2: Thread-safe removal of region observer
            lock (_lockObject)
            {
                if (_observersByRegion.ContainsKey(regionId))
                {
                    _observersByRegion.Remove(regionId);
                }

                // Issue 6: Only dispose observers if no more regions are being monitored
                if (_observersByRegion.Count == 0 && _locationManager.MonitoredRegions.Count == 0)
                {
                    CleanupResources();
                }
            }

            return await Task.FromResult(Result.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "iOS geofence stop failed");
            return Result.Fail<bool>(Error.FromException(ex));
        }
    }

    // Issue 6: Helper method to dispose resources when no regions remain
    private void CleanupResources()
    {
        foreach (var (enter, exit) in _observersByRegion.Values)
        {
            enter?.Dispose();
            exit?.Dispose();
        }

        _observersByRegion.Clear();
        _locationManager?.Dispose();
        _locationManager = null;
    }

    private partial async Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        return await Task.FromResult(CLLocationManager.LocationServicesEnabled);
    }
}
#endif
