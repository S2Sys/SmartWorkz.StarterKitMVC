#if __IOS__
namespace SmartWorkz.Mobile;

using CoreLocation;
using Foundation;
using SmartWorkz.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// iOS-specific BLE beacon service implementation using Core Location and iBeacon regions.
/// Provides beacon scanning, monitoring, and ranging capabilities for iOS devices.
/// </summary>
partial class BeaconService
{
    // Private fields for beacon management
    private CLLocationManager? _locationManager;
    private readonly ConcurrentDictionary<string, CLBeaconRegion> _rangedBeacons = new();
    private readonly ConcurrentDictionary<string, CLBeaconRegion> _monitoredRegions = new();
    private NSObject? _proximityObserver;
    private readonly object _lockObject = new();

    /// <summary>
    /// Ensures the CLLocationManager is initialized.
    /// </summary>
    private void EnsureLocationManager()
    {
        if (_locationManager == null)
        {
            lock (_lockObject)
            {
                if (_locationManager == null)
                {
                    _locationManager = new CLLocationManager();
                    _logger.LogDebug("Initialized CLLocationManager for iOS beacon detection");
                }
            }
        }
    }

    /// <summary>
    /// iOS-specific beacon scan implementation using Core Location.
    /// Creates a wildcard beacon region and ranges for all available beacons.
    /// </summary>
    private partial async Task<Result<IReadOnlyList<BeaconInfo>>> ScanForBeaconsAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            _logger.LogDebug("Starting iOS Core Location beacon scan");

            // Check if location services are enabled
            if (!CLLocationManager.LocationServicesEnabled)
            {
                _logger.LogWarning("Location services disabled on iOS device");
                return Result.Fail<IReadOnlyList<BeaconInfo>>(
                    new Error("BEACON.LOCATION_DISABLED", "Location services are disabled on this device"));
            }

            EnsureLocationManager();

            // Check authorization status (requires WhenInUse or Always authorization)
            var authStatus = CLLocationManager.AuthorizationStatus;
            if (authStatus is not CLAuthorizationStatus.AuthorizedWhenInUse and not CLAuthorizationStatus.AuthorizedAlways)
            {
                _logger.LogWarning("Insufficient location authorization for beacon scan: {Status}", authStatus);
                return Result.Fail<IReadOnlyList<BeaconInfo>>(
                    new Error("BEACON.PERMISSION_DENIED", "Location authorization required for beacon scanning"));
            }

            // Create a wildcard beacon region to detect all iBeacons
            // Using NSUUID(string) to create a UUID, but for wildcard we need to range without a specific UUID
            // Use a generic UUID that will match all beacons (major/minor = any)
            var broadcastBeaconUUID = new NSUuid("00000000-0000-0000-0000-000000000000");
            var broadcastRegion = new CLBeaconRegion(broadcastBeaconUUID, "WildcardBeaconRegion")
            {
                NotifyOnEntry = true,
                NotifyOnExit = true
            };

            var detectedBeacons = new List<BeaconInfo>();
            var scanCompleted = new TaskCompletionSource<bool>();
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(4), ct);

            // Set up observer for ranging updates
            EventHandler<CLBeaconsRangedEventArgs>? rangingHandler = null;
            rangingHandler = (sender, e) =>
            {
                try
                {
                    if (e.Beacons.Length > 0)
                    {
                        _logger.LogDebug("Beacon scan detected {Count} beacons", e.Beacons.Length);

                        foreach (var clBeacon in e.Beacons)
                        {
                            try
                            {
                                var beaconInfo = ConvertCLBeaconToBeaconInfo(clBeacon);
                                detectedBeacons.Add(beaconInfo);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to convert CLBeacon to BeaconInfo");
                            }
                        }

                        // Signal scan completion after first detection
                        if (!scanCompleted.Task.IsCompleted)
                        {
                            scanCompleted.TrySetResult(true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in beacon ranging handler");
                }
            };

            _locationManager!.BeaconsRanged += rangingHandler;

            try
            {
                // Start ranging for beacons in the wildcard region
                _locationManager.StartRangingBeacons(broadcastRegion);
                _logger.LogDebug("Started ranging for wildcard beacon region");

                // Wait for results with timeout
                var completedTask = await Task.WhenAny(scanCompleted.Task, timeoutTask);
                if (completedTask == timeoutTask && !scanCompleted.Task.IsCompleted)
                {
                    scanCompleted.TrySetResult(true);
                }

                // Stop ranging
                _locationManager.StopRangingBeacons(broadcastRegion);
                _logger.LogDebug("Stopped ranging for wildcard beacon region");

                var resultList = detectedBeacons.Distinct(new BeaconInfoComparer()).ToList().AsReadOnly();
                _logger.LogInformation("iOS beacon scan completed: Found {Count} unique beacons", resultList.Count);

                return Result.Ok((IReadOnlyList<BeaconInfo>)resultList);
            }
            finally
            {
                // Clean up event handler
                if (rangingHandler != null)
                {
                    _locationManager.BeaconsRanged -= rangingHandler;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "iOS beacon scan failed");
            return Result.Fail<IReadOnlyList<BeaconInfo>>(Error.FromException(ex));
        }
    }

    /// <summary>
    /// iOS-specific start monitoring implementation using Core Location region monitoring.
    /// Monitors for beacon region entry/exit and emits proximity events.
    /// </summary>
    private partial async Task<Result<bool>> StartMonitoringAsyncPlatform(BeaconInfo beacon, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            _logger.LogDebug("Starting iOS Core Location beacon monitoring for UUID: {UUID}", beacon.UUID);

            // Validate UUID format
            if (!Guid.TryParse(beacon.UUID, out var parsedUuid))
            {
                _logger.LogWarning("Invalid beacon UUID format: {UUID}", beacon.UUID);
                return Result.Fail<bool>(
                    new Error("BEACON.INVALID_UUID", "Beacon UUID must be in valid UUID format"));
            }

            // Check if location services are enabled
            if (!CLLocationManager.LocationServicesEnabled)
            {
                _logger.LogWarning("Location services disabled - cannot start monitoring");
                return Result.Fail<bool>(
                    new Error("BEACON.LOCATION_DISABLED", "Location services are disabled on this device"));
            }

            EnsureLocationManager();

            // Check authorization status
            var authStatus = CLLocationManager.AuthorizationStatus;
            if (authStatus is not CLAuthorizationStatus.AuthorizedWhenInUse and not CLAuthorizationStatus.AuthorizedAlways)
            {
                _logger.LogWarning("Insufficient location authorization for beacon monitoring: {Status}", authStatus);
                return Result.Fail<bool>(
                    new Error("BEACON.PERMISSION_DENIED", "Location authorization required for beacon monitoring"));
            }

            // Create beacon region with UUID, Major, and Minor
            var nsUuid = new NSUuid(parsedUuid.ToString("D"));
            var beaconRegion = new CLBeaconRegion(
                nsUuid,
                (ushort)beacon.Major,
                (ushort)beacon.Minor,
                $"Beacon_{beacon.UUID}_{beacon.Major}_{beacon.Minor}")
            {
                NotifyOnEntry = true,
                NotifyOnExit = true
            };

            // Set up region monitoring event handlers
            EventHandler<CLRegionEventArgs>? didEnterHandler = null;
            EventHandler<CLRegionEventArgs>? didExitHandler = null;

            didEnterHandler = (sender, e) =>
            {
                if (e.Region is CLBeaconRegion)
                {
                    try
                    {
                        _logger.LogInformation("Beacon entered region: {UUID}", beacon.UUID);
                        RaiseProximityChangeEvent(new BeaconProximityEvent(
                            Beacon: beacon,
                            EventType: 1, // ENTER
                            PreviousProximity: null,
                            CurrentProximity: beacon.Proximity(),
                            DetectedAt: DateTime.UtcNow));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling beacon region entry");
                    }
                }
            };

            didExitHandler = (sender, e) =>
            {
                if (e.Region is CLBeaconRegion)
                {
                    try
                    {
                        _logger.LogInformation("Beacon exited region: {UUID}", beacon.UUID);
                        RaiseProximityChangeEvent(new BeaconProximityEvent(
                            Beacon: beacon,
                            EventType: 0, // EXIT
                            PreviousProximity: beacon.Proximity(),
                            CurrentProximity: "Unknown",
                            DetectedAt: DateTime.UtcNow));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling beacon region exit");
                    }
                }
            };

            // Register event handlers
            _locationManager!.DidEnterRegion += didEnterHandler;
            _locationManager.DidExitRegion += didExitHandler;

            // Track the region for cleanup
            lock (_lockObject)
            {
                _monitoredRegions.TryAdd(beacon.UUID, beaconRegion);
            }

            // Start monitoring the region
            _locationManager.StartMonitoring(beaconRegion);
            _logger.LogInformation("Started monitoring beacon region: {UUID}", beacon.UUID);

            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start iOS beacon monitoring for UUID: {UUID}", beacon.UUID);
            return Result.Fail<bool>(Error.FromException(ex));
        }
    }

    /// <summary>
    /// iOS-specific stop monitoring implementation.
    /// Stops monitoring a beacon region and unregisters event handlers.
    /// </summary>
    private partial async Task<Result<bool>> StopMonitoringAsyncPlatform(string beaconUUID, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            _logger.LogDebug("Stopping iOS beacon monitoring for UUID: {UUID}", beaconUUID);

            lock (_lockObject)
            {
                if (!_monitoredRegions.TryRemove(beaconUUID, out var region))
                {
                    _logger.LogWarning("Beacon region not found for UUID: {UUID}", beaconUUID);
                    return Result.Fail<bool>(
                        new Error("BEACON.NOT_FOUND", $"Beacon region not found for UUID: {beaconUUID}"));
                }

                if (_locationManager != null)
                {
                    // Stop monitoring the region
                    _locationManager.StopMonitoring(region);
                    _logger.LogInformation("Stopped monitoring beacon region: {UUID}", beaconUUID);
                }
            }

            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop iOS beacon monitoring for UUID: {UUID}", beaconUUID);
            return Result.Fail<bool>(Error.FromException(ex));
        }
    }

    /// <summary>
    /// iOS-specific beacon ranging implementation using Core Location.
    /// Returns beacons matching the specified UUID with distance/proximity information.
    /// </summary>
    private partial async Task<Result<IReadOnlyList<BeaconInfo>>> RangeBeaconsAsyncPlatform(string? uuid, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            _logger.LogDebug("Starting iOS beacon ranging for UUID: {UUID}", uuid ?? "all");

            // Check if location services are enabled
            if (!CLLocationManager.LocationServicesEnabled)
            {
                _logger.LogWarning("Location services disabled - cannot range beacons");
                return Result.Fail<IReadOnlyList<BeaconInfo>>(
                    new Error("BEACON.LOCATION_DISABLED", "Location services are disabled on this device"));
            }

            EnsureLocationManager();

            // Check authorization status
            var authStatus = CLLocationManager.AuthorizationStatus;
            if (authStatus is not CLAuthorizationStatus.AuthorizedWhenInUse and not CLAuthorizationStatus.AuthorizedAlways)
            {
                _logger.LogWarning("Insufficient location authorization for beacon ranging: {Status}", authStatus);
                return Result.Fail<IReadOnlyList<BeaconInfo>>(
                    new Error("BEACON.PERMISSION_DENIED", "Location authorization required for beacon ranging"));
            }

            // Create beacon region
            CLBeaconRegion rangeRegion;
            if (!string.IsNullOrWhiteSpace(uuid))
            {
                // Range for specific UUID
                if (!Guid.TryParse(uuid, out var parsedUuid))
                {
                    _logger.LogWarning("Invalid beacon UUID format for ranging: {UUID}", uuid);
                    return Result.Fail<IReadOnlyList<BeaconInfo>>(
                        new Error("BEACON.INVALID_UUID", "Beacon UUID must be in valid UUID format"));
                }
                var nsUuid = new NSUuid(parsedUuid.ToString("D"));
                rangeRegion = new CLBeaconRegion(nsUuid, $"RangeRegion_{uuid}");
            }
            else
            {
                // Range all beacons using wildcard UUID
                var wildCardUuid = new NSUuid("00000000-0000-0000-0000-000000000000");
                rangeRegion = new CLBeaconRegion(wildCardUuid, "WildcardRangeRegion");
            }

            var detectedBeacons = new List<BeaconInfo>();
            var rangingCompleted = new TaskCompletionSource<bool>();
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(4), ct);

            // Set up event handler for ranging results
            EventHandler<CLBeaconsRangedEventArgs>? rangingHandler = null;
            rangingHandler = (sender, e) =>
            {
                try
                {
                    if (e.Beacons.Length > 0)
                    {
                        _logger.LogDebug("Beacon ranging detected {Count} beacons", e.Beacons.Length);

                        foreach (var clBeacon in e.Beacons)
                        {
                            try
                            {
                                var beaconInfo = ConvertCLBeaconToBeaconInfo(clBeacon);
                                detectedBeacons.Add(beaconInfo);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to convert CLBeacon to BeaconInfo during ranging");
                            }
                        }
                    }

                    // Signal completion after first update
                    if (!rangingCompleted.Task.IsCompleted)
                    {
                        rangingCompleted.TrySetResult(true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in beacon ranging handler");
                }
            };

            _locationManager!.BeaconsRanged += rangingHandler;

            try
            {
                // Start ranging
                _locationManager.StartRangingBeacons(rangeRegion);
                _logger.LogDebug("Started ranging for beacon region: {UUID}", uuid ?? "wildcard");

                // Wait for results or timeout
                var completedTask = await Task.WhenAny(rangingCompleted.Task, timeoutTask);
                if (completedTask == timeoutTask && !rangingCompleted.Task.IsCompleted)
                {
                    rangingCompleted.TrySetResult(true);
                }

                // Stop ranging
                _locationManager.StopRangingBeacons(rangeRegion);
                _logger.LogDebug("Stopped ranging for beacon region: {UUID}", uuid ?? "wildcard");

                var resultList = detectedBeacons.Distinct(new BeaconInfoComparer()).ToList().AsReadOnly();
                _logger.LogInformation("iOS beacon ranging completed: Found {Count} unique beacons", resultList.Count);

                return Result.Ok((IReadOnlyList<BeaconInfo>)resultList);
            }
            finally
            {
                // Clean up event handler
                if (rangingHandler != null)
                {
                    _locationManager.BeaconsRanged -= rangingHandler;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "iOS beacon ranging failed");
            return Result.Fail<IReadOnlyList<BeaconInfo>>(Error.FromException(ex));
        }
    }

    /// <summary>
    /// iOS-specific availability check using CLLocationManager.
    /// </summary>
    private partial async Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            _logger.LogDebug("Checking iOS beacon availability");

            // Check if location services are enabled
            if (!CLLocationManager.LocationServicesEnabled)
            {
                _logger.LogDebug("Location services not enabled on iOS device");
                return false;
            }

            // Check if CLLocationManager is available
            if (CLLocationManager.ClassHandle == IntPtr.Zero)
            {
                _logger.LogDebug("CLLocationManager not available on iOS device");
                return false;
            }

            // Check if beacon ranging is supported
            // Beacon ranging requires iOS 7+, which is always true for modern iOS
            var isAvailable = CLLocationManager.IsRangingAvailable;
            return await Task.FromResult(isAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error checking beacon availability");
            return false;
        }
    }

    /// <summary>
    /// Converts a CLBeacon object to a BeaconInfo record.
    /// </summary>
    private BeaconInfo ConvertCLBeaconToBeaconInfo(CLBeacon clBeacon)
    {
        try
        {
            // Extract beacon properties
            var uuid = clBeacon.Uuid.AsString();
            var major = (int)(clBeacon.Major?.UInt16Value ?? 0);
            var minor = (int)(clBeacon.Minor?.UInt16Value ?? 0);
            var rssi = (int)clBeacon.Rssi;

            // Calculate proximity/distance from accuracy
            var distance = clBeacon.Accuracy > 0 ? clBeacon.Accuracy : null;

            // Generate identifier
            var identifier = $"{uuid}_{major}_{minor}";

            // Determine reachability based on RSSI
            var isReachable = rssi > -100;

            var beaconInfo = new BeaconInfo(
                UUID: uuid,
                Major: major,
                Minor: minor,
                Identifier: identifier,
                RSSI: rssi,
                Distance: distance,
                BeaconType: "iBeacon",
                IsReachable: isReachable,
                LastSeenAt: DateTime.UtcNow,
                Metadata: new Dictionary<string, string>
                {
                    { "Proximity", clBeacon.Proximity.ToString() },
                    { "Accuracy", distance?.ToString("F2") ?? "Unknown" }
                });

            return beaconInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting CLBeacon to BeaconInfo");
            throw;
        }
    }

    /// <summary>
    /// Comparer for BeaconInfo to detect duplicates.
    /// </summary>
    private class BeaconInfoComparer : IEqualityComparer<BeaconInfo>
    {
        public bool Equals(BeaconInfo? x, BeaconInfo? y)
        {
            if (x == null || y == null)
                return x == y;
            return x.UUID == y.UUID && x.Major == y.Major && x.Minor == y.Minor;
        }

        public int GetHashCode(BeaconInfo obj)
        {
            return HashCode.Combine(obj.UUID, obj.Major, obj.Minor);
        }
    }
}

#endif
