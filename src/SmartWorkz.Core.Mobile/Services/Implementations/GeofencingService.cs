namespace SmartWorkz.Mobile;

using Microsoft.Extensions.Logging;
using SmartWorkz.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Cross-platform geofencing service with location-based region monitoring.
/// Delegates platform-specific implementation to partial methods.
/// </summary>
public partial class GeofencingService : IGeofencingService
{
    private readonly ILogger<GeofencingService> _logger;
    private readonly IPermissionService _permissionService;
    private readonly Subject<GeofenceEvent> _geofenceEventSubject = new();
    private readonly Dictionary<string, GeofenceRegion> _monitoredRegions = new();

    public GeofencingService(
        ILogger<GeofencingService> logger,
        IPermissionService permissionService)
    {
        Guard.NotNull(logger, nameof(logger));
        Guard.NotNull(permissionService, nameof(permissionService));

        _logger = logger;
        _permissionService = permissionService;
    }

    public IObservable<GeofenceEvent> OnGeofenceEventDetected => _geofenceEventSubject.AsObservable();

    public async Task<Result<bool>> StartMonitoringAsync(GeofenceRegion region, CancellationToken ct = default)
    {
        Guard.NotNull(region, nameof(region));

        if (region.RadiusMeters < 10 || region.RadiusMeters > 10000)
        {
            _logger.LogWarning("Invalid geofence radius: {Radius}", region.RadiusMeters);
            return Result.Fail<bool>(Error.Validation("GEOFENCE.INVALID_REGION",
                "Radius must be between 10 and 10000 meters"));
        }

        var permissionStatus = await _permissionService.CheckAsync(MobilePermission.Location, ct);
        if (permissionStatus != PermissionStatus.Granted)
        {
            _logger.LogWarning("Location permission denied for geofencing");
            return Result.Fail<bool>(Error.Unauthorized("Location permission required for geofencing"));
        }

        var result = await StartMonitoringAsyncPlatform(region, ct);
        if (result.Succeeded)
        {
            _monitoredRegions[region.Id] = region;
            _logger.LogInformation("Started monitoring geofence: {RegionId}", region.Id);
        }

        return result;
    }

    public async Task<Result<bool>> StopMonitoringAsync(string regionId, CancellationToken ct = default)
    {
        Guard.NotEmpty(regionId, nameof(regionId));

        var result = await StopMonitoringAsyncPlatform(regionId, ct);
        if (result.Succeeded)
        {
            _monitoredRegions.Remove(regionId);
            _logger.LogInformation("Stopped monitoring geofence: {RegionId}", regionId);
        }

        return result;
    }

    public async Task<Result<IReadOnlyList<GeofenceRegion>>> GetMonitoredRegionsAsync(CancellationToken ct = default)
    {
        var regions = _monitoredRegions.Values.ToList();
        return await Task.FromResult(Result.Ok<IReadOnlyList<GeofenceRegion>>(regions));
    }

    public async Task<Result<bool>> IsAvailableAsync(CancellationToken ct = default)
    {
        var isAvailable = await IsAvailableAsyncPlatform(ct);
        return Result.Ok(isAvailable);
    }

#if __ANDROID__ || __IOS__ || __MACCATALYST__
    /// <summary>
    /// Platform-specific geofence monitoring start. Implemented per platform.
    /// </summary>
    private partial Task<Result<bool>> StartMonitoringAsyncPlatform(GeofenceRegion region, CancellationToken ct);

    /// <summary>
    /// Platform-specific geofence monitoring stop. Implemented per platform.
    /// </summary>
    private partial Task<Result<bool>> StopMonitoringAsyncPlatform(string regionId, CancellationToken ct);

    /// <summary>
    /// Platform-specific availability check. Implemented per platform.
    /// </summary>
    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct);
#else
    /// <summary>
    /// Platform-specific geofence monitoring start. Default implementation for unsupported platforms.
    /// </summary>
    private Task<Result<bool>> StartMonitoringAsyncPlatform(GeofenceRegion region, CancellationToken ct) =>
        Task.FromResult(Result.Fail<bool>(new Error("GEOFENCING.NOT_SUPPORTED",
            "Geofencing is not supported on this platform")));

    /// <summary>
    /// Platform-specific geofence monitoring stop. Default implementation for unsupported platforms.
    /// </summary>
    private Task<Result<bool>> StopMonitoringAsyncPlatform(string regionId, CancellationToken ct) =>
        Task.FromResult(Result.Fail<bool>(new Error("GEOFENCING.NOT_SUPPORTED",
            "Geofencing is not supported on this platform")));

    /// <summary>
    /// Platform-specific availability check. Default implementation for unsupported platforms.
    /// </summary>
    private Task<bool> IsAvailableAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(false);
#endif

    /// <summary>
    /// Raises geofence event (called from platform implementations).
    /// </summary>
    protected void RaiseGeofenceEvent(GeofenceEvent geofenceEvent)
    {
        _geofenceEventSubject.OnNext(geofenceEvent);
        _logger.LogInformation("Geofence event: {EventType} in region {RegionId}",
            geofenceEvent.EventType == 1 ? "ENTER" : "EXIT", geofenceEvent.RegionId);
    }
}
