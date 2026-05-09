namespace SmartWorkz.Mobile;

using Microsoft.Extensions.Logging;
using SmartWorkz.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Cross-platform BLE beacon service for discovering, monitoring, and ranging beacons.
/// Delegates platform-specific implementation to partial methods.
/// </summary>
public partial class BeaconService : IBeaconService
{
    private readonly ILogger<BeaconService> _logger;
    private readonly IPermissionService _permissionService;
    private readonly Subject<BeaconProximityEvent> _proximityChangeSubject = new();
    private readonly ConcurrentDictionary<string, BeaconInfo> _monitoredBeacons = new();

    /// <summary>
    /// Initializes a new instance of the BeaconService class.
    /// </summary>
    /// <param name="logger">The logger instance for this service.</param>
    /// <param name="permissionService">The permission service for checking beacon-related permissions.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or permissionService is null.</exception>
    public BeaconService(
        ILogger<BeaconService> logger,
        IPermissionService permissionService)
    {
        Guard.NotNull(logger, nameof(logger));
        Guard.NotNull(permissionService, nameof(permissionService));

        _logger = logger;
        _permissionService = permissionService;
    }

    /// <summary>
    /// Gets an observable stream of beacon proximity change events.
    /// </summary>
    public IObservable<BeaconProximityEvent> OnBeaconProximityChanged => _proximityChangeSubject.AsObservable();

    /// <summary>
    /// Scans for all available BLE beacons in range of the device.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing a read-only list of discovered BeaconInfo objects.
    /// Returns an error if location permissions are not granted.
    /// </returns>
    public async Task<Result<IReadOnlyList<BeaconInfo>>> ScanForBeaconsAsync(CancellationToken ct = default)
    {
        var permissionStatus = await _permissionService.CheckAsync(MobilePermission.Location, ct);
        if (permissionStatus != PermissionStatus.Granted)
        {
            _logger.LogWarning("Beacon permission denied for beacon scan");
            return Result.Fail<IReadOnlyList<BeaconInfo>>(Error.Unauthorized("Location permission required for beacon scanning"));
        }

        _logger.LogInformation("BLE beacon scan started");
        var result = await ScanForBeaconsAsyncPlatform(ct);
        if (result.Succeeded)
        {
            _logger.LogInformation("Beacon scan completed: Found {Count} beacons", result.Data?.Count ?? 0);
        }
        else
        {
            _logger.LogWarning("Beacon scan failed: {Error}", result.Error?.Message);
        }

        return result;
    }

    /// <summary>
    /// Starts monitoring a specific beacon for proximity events (enter/exit range).
    /// </summary>
    /// <param name="beacon">The BeaconInfo object representing the beacon to monitor.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing true if monitoring started successfully; false otherwise.
    /// </returns>
    public async Task<Result<bool>> StartMonitoringAsync(BeaconInfo beacon, CancellationToken ct = default)
    {
        Guard.NotNull(beacon, nameof(beacon));

        // Validate UUID is not empty
        if (string.IsNullOrWhiteSpace(beacon.UUID))
        {
            _logger.LogWarning("Attempt to monitor beacon with invalid UUID");
            return Result.Fail<bool>(Error.Validation("BEACON.INVALID_BEACON",
                "Beacon UUID must be a valid non-empty UUID"));
        }

        var permissionStatus = await _permissionService.CheckAsync(MobilePermission.Location, ct);
        if (permissionStatus != PermissionStatus.Granted)
        {
            _logger.LogWarning("Beacon permission denied for monitoring");
            return Result.Fail<bool>(Error.Unauthorized("Location permission required for beacon monitoring"));
        }

        var result = await StartMonitoringAsyncPlatform(beacon, ct);
        if (result.Succeeded)
        {
            _monitoredBeacons.TryAdd(beacon.UUID, beacon);
            _logger.LogInformation("Started monitoring beacon: {UUID}", beacon.UUID);
        }
        else
        {
            _logger.LogWarning("Failed to start monitoring beacon {UUID}: {Error}", beacon.UUID, result.Error?.Message);
        }

        return result;
    }

    /// <summary>
    /// Stops monitoring a specific beacon for proximity events.
    /// </summary>
    /// <param name="beaconUUID">The UUID string identifying the beacon to stop monitoring.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing true if monitoring was stopped successfully; false if the beacon was not being monitored.
    /// </returns>
    public async Task<Result<bool>> StopMonitoringAsync(string beaconUUID, CancellationToken ct = default)
    {
        Guard.NotEmpty(beaconUUID, nameof(beaconUUID));

        var result = await StopMonitoringAsyncPlatform(beaconUUID, ct);
        if (result.Succeeded)
        {
            _monitoredBeacons.TryRemove(beaconUUID, out _);
            _logger.LogInformation("Stopped monitoring beacon: {UUID}", beaconUUID);
        }
        else
        {
            _logger.LogWarning("Failed to stop monitoring beacon {UUID}: {Error}", beaconUUID, result.Error?.Message);
        }

        return result;
    }

    /// <summary>
    /// Gets a list of all currently monitored beacons.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing a read-only list of BeaconInfo objects representing currently monitored beacons.
    /// Returns an empty list if no beacons are currently being monitored.
    /// </returns>
    public Task<Result<IReadOnlyList<BeaconInfo>>> GetMonitoredBeaconsAsync(CancellationToken ct = default)
    {
        var beaconList = _monitoredBeacons.Values.ToList().AsReadOnly();
        return Task.FromResult(Result.Ok<IReadOnlyList<BeaconInfo>>(beaconList));
    }

    /// <summary>
    /// Gets ranging data (distance and signal strength) for beacons matching a UUID.
    /// </summary>
    /// <param name="uuid">The UUID to range for. Can be a specific UUID string or null to range all beacons in range.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing a read-only list of BeaconInfo objects with ranging data (distance, RSSI) populated.
    /// Returns an empty list if no beacons match the UUID or if no beacons are in range.
    /// </returns>
    public async Task<Result<IReadOnlyList<BeaconInfo>>> RangeBeaconsAsync(string? uuid, CancellationToken ct = default)
    {
        var permissionStatus = await _permissionService.CheckAsync(MobilePermission.Location, ct);
        if (permissionStatus != PermissionStatus.Granted)
        {
            _logger.LogWarning("Beacon permission denied for ranging");
            return Result.Fail<IReadOnlyList<BeaconInfo>>(Error.Unauthorized("Location permission required for beacon ranging"));
        }

        _logger.LogInformation("Ranging beacons for UUID: {UUID}", uuid ?? "all");
        var result = await RangeBeaconsAsyncPlatform(uuid, ct);
        if (result.Succeeded)
        {
            _logger.LogInformation("Ranging completed: Found {Count} beacons", result.Data?.Count ?? 0);
        }
        else
        {
            _logger.LogWarning("Beacon ranging failed: {Error}", result.Error?.Message);
        }

        return result;
    }

    /// <summary>
    /// Checks whether BLE beacon detection capability is available on the current device.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing true if the device supports BLE beacon detection; false otherwise.
    /// </returns>
    public async Task<Result<bool>> IsAvailableAsync(CancellationToken ct = default)
    {
        var isAvailable = await IsAvailableAsyncPlatform(ct);
        return Result.Ok(isAvailable);
    }

    /// <summary>
    /// Raises a beacon proximity change event (called from platform implementations).
    /// </summary>
    /// <param name="event">The proximity change event to raise.</param>
    protected void RaiseProximityChangeEvent(BeaconProximityEvent @event)
    {
        _proximityChangeSubject.OnNext(@event);
        var eventTypeStr = @event.EventType switch
        {
            0 => "EXIT",
            1 => "ENTER",
            2 => "PROXIMITY_CHANGED",
            _ => "UNKNOWN"
        };
        _logger.LogInformation("Beacon proximity event: {EventType} for UUID: {UUID}", eventTypeStr, @event.Beacon.UUID);
    }

#if __ANDROID__ || __IOS__ || __MACCATALYST__
    /// <summary>
    /// Platform-specific beacon scan. Implemented per platform.
    /// </summary>
    private partial Task<Result<IReadOnlyList<BeaconInfo>>> ScanForBeaconsAsyncPlatform(CancellationToken ct);

    /// <summary>
    /// Platform-specific start monitoring. Implemented per platform.
    /// </summary>
    private partial Task<Result<bool>> StartMonitoringAsyncPlatform(BeaconInfo beacon, CancellationToken ct);

    /// <summary>
    /// Platform-specific stop monitoring. Implemented per platform.
    /// </summary>
    private partial Task<Result<bool>> StopMonitoringAsyncPlatform(string beaconUUID, CancellationToken ct);

    /// <summary>
    /// Platform-specific beacon ranging. Implemented per platform.
    /// </summary>
    private partial Task<Result<IReadOnlyList<BeaconInfo>>> RangeBeaconsAsyncPlatform(string? uuid, CancellationToken ct);

    /// <summary>
    /// Platform-specific availability check. Implemented per platform.
    /// </summary>
    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct);
#else
    /// <summary>
    /// Platform-specific beacon scan. Default implementation for unsupported platforms.
    /// </summary>
    private Task<Result<IReadOnlyList<BeaconInfo>>> ScanForBeaconsAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(Result.Fail<IReadOnlyList<BeaconInfo>>(new Error("BEACON.NOT_SUPPORTED",
            "Beacon service is not supported on this platform")));

    /// <summary>
    /// Platform-specific start monitoring. Default implementation for unsupported platforms.
    /// </summary>
    private Task<Result<bool>> StartMonitoringAsyncPlatform(BeaconInfo beacon, CancellationToken ct) =>
        Task.FromResult(Result.Fail<bool>(new Error("BEACON.NOT_SUPPORTED",
            "Beacon service is not supported on this platform")));

    /// <summary>
    /// Platform-specific stop monitoring. Default implementation for unsupported platforms.
    /// </summary>
    private Task<Result<bool>> StopMonitoringAsyncPlatform(string beaconUUID, CancellationToken ct) =>
        Task.FromResult(Result.Fail<bool>(new Error("BEACON.NOT_SUPPORTED",
            "Beacon service is not supported on this platform")));

    /// <summary>
    /// Platform-specific beacon ranging. Default implementation for unsupported platforms.
    /// </summary>
    private Task<Result<IReadOnlyList<BeaconInfo>>> RangeBeaconsAsyncPlatform(string? uuid, CancellationToken ct) =>
        Task.FromResult(Result.Fail<IReadOnlyList<BeaconInfo>>(new Error("BEACON.NOT_SUPPORTED",
            "Beacon service is not supported on this platform")));

    /// <summary>
    /// Platform-specific availability check. Default implementation for unsupported platforms.
    /// </summary>
    private Task<bool> IsAvailableAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(false);
#endif
}
