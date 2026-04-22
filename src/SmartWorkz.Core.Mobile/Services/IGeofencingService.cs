namespace SmartWorkz.Mobile;

/// <summary>
/// Service for monitoring geofence regions and detecting location-based events.
/// Provides real-time geofence entry/exit event detection with support for multiple regions.
/// </summary>
public interface IGeofencingService
{
    /// <summary>
    /// Starts monitoring a geofence region for entry and exit events.
    /// </summary>
    /// <param name="region">The geofence region to monitor. Must have valid coordinates and radius.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result indicating success or failure. On success, the region monitoring has been initiated.
    /// Failure can occur if the region is invalid, geofencing is unavailable, or a permission issue occurs.
    /// </returns>
    /// <remarks>
    /// The same region can be monitored multiple times; each call adds another monitoring instance.
    /// Monitoring continues in the background even when the app is backgrounded or suspended,
    /// subject to platform limitations (iOS and Android).
    /// </remarks>
    Task<Result<bool>> StartMonitoringAsync(GeofenceRegion region, CancellationToken ct = default);

    /// <summary>
    /// Stops monitoring a geofence region by its ID.
    /// </summary>
    /// <param name="regionId">The unique identifier of the geofence region to stop monitoring.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result indicating success or failure. On success, the region monitoring has been stopped.
    /// Returns failure if the region ID was not found in the currently monitored regions.
    /// </returns>
    Task<Result<bool>> StopMonitoringAsync(string regionId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the list of all currently monitored geofence regions.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing the list of currently monitored GeofenceRegion objects.
    /// Returns an empty list if no regions are currently being monitored.
    /// </returns>
    Task<Result<IReadOnlyList<GeofenceRegion>>> GetMonitoredRegionsAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks if geofencing is available on the current device and platform.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result indicating if geofencing is available. Returns true if geofencing hardware and APIs
    /// are available; false if unavailable (e.g., on older Android versions or lacking location hardware).
    /// </returns>
    Task<Result<bool>> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets an observable stream of geofence events (entry and exit).
    /// </summary>
    /// <value>
    /// An observable that emits GeofenceEvent instances when the device enters or exits a monitored geofence region.
    /// The observable emits events for all monitored regions and continues until the service is disposed or monitoring is stopped.
    /// </value>
    IObservable<GeofenceEvent> OnGeofenceEventDetected { get; }
}

/// <summary>
/// Represents a geofence event triggered when the device enters or exits a monitored region.
/// </summary>
/// <param name="RegionId">The unique identifier of the geofence region that triggered this event.</param>
/// <param name="EventType">The type of geofence event: 1 for ENTER, 0 for EXIT.</param>
/// <param name="DetectedAt">The UTC timestamp when the geofence event was detected by the system.</param>
/// <param name="CurrentLatitude">The device's latitude coordinate at the time the event was detected.</param>
/// <param name="CurrentLongitude">The device's longitude coordinate at the time the event was detected.</param>
public sealed record GeofenceEvent(
    /// <summary>
    /// Gets the unique identifier of the geofence region that triggered this event.
    /// </summary>
    string RegionId,

    /// <summary>
    /// Gets the type of geofence event: 1 indicates ENTER (device entered the region),
    /// 0 indicates EXIT (device exited the region).
    /// </summary>
    int EventType,

    /// <summary>
    /// Gets the UTC timestamp when the geofence event was detected by the system.
    /// </summary>
    DateTime DetectedAt,

    /// <summary>
    /// Gets the device's latitude coordinate at the time the event was detected.
    /// Valid range: -90 to 90 degrees.
    /// </summary>
    double CurrentLatitude,

    /// <summary>
    /// Gets the device's longitude coordinate at the time the event was detected.
    /// Valid range: -180 to 180 degrees.
    /// </summary>
    double CurrentLongitude)
{
    /// <summary>
    /// Gets the unique identifier of the geofence region that triggered this event.
    /// </summary>
    public string RegionId { get; } = RegionId ?? throw new ArgumentNullException(nameof(RegionId));

    /// <summary>
    /// Gets the type of geofence event: 1 indicates ENTER (device entered the region),
    /// 0 indicates EXIT (device exited the region).
    /// </summary>
    public int EventType { get; } = EventType is 0 or 1
        ? EventType
        : throw new ArgumentException("EventType must be 0 (EXIT) or 1 (ENTER)", nameof(EventType));

    /// <summary>
    /// Gets the UTC timestamp when the geofence event was detected by the system.
    /// </summary>
    public DateTime DetectedAt { get; } = DetectedAt;

    /// <summary>
    /// Gets the device's latitude coordinate at the time the event was detected.
    /// Valid range: -90 to 90 degrees.
    /// </summary>
    public double CurrentLatitude { get; } = CurrentLatitude < -90 || CurrentLatitude > 90
        ? throw new ArgumentOutOfRangeException(nameof(CurrentLatitude), "Latitude must be between -90 and 90 degrees")
        : CurrentLatitude;

    /// <summary>
    /// Gets the device's longitude coordinate at the time the event was detected.
    /// Valid range: -180 to 180 degrees.
    /// </summary>
    public double CurrentLongitude { get; } = CurrentLongitude < -180 || CurrentLongitude > 180
        ? throw new ArgumentOutOfRangeException(nameof(CurrentLongitude), "Longitude must be between -180 and 180 degrees")
        : CurrentLongitude;
}
