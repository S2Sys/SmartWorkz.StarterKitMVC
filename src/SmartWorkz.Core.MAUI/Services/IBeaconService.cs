namespace SmartWorkz.Mobile;

/// <summary>
/// Service for discovering, monitoring, and ranging BLE (Bluetooth Low Energy) beacons.
/// Provides functionality for scanning available beacons, monitoring specific beacons for proximity changes,
/// and obtaining ranging data (distance and signal strength) for beacons in range.
/// </summary>
public interface IBeaconService
{
    /// <summary>
    /// Scans for all available BLE beacons in range of the device.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing a read-only list of discovered BeaconInfo objects.
    /// On success, returns all beacons currently in range.
    /// Returns an empty list if no beacons are discovered.
    /// </returns>
    /// <remarks>
    /// Scanning may take several seconds depending on the device and number of available beacons.
    /// This operation requires appropriate Bluetooth and location permissions on the platform.
    /// Possible error codes: BEACON.SCAN_FAILED, BEACON.PERMISSION_DENIED, BEACON.NOT_SUPPORTED
    /// </remarks>
    Task<Result<IReadOnlyList<BeaconInfo>>> ScanForBeaconsAsync(CancellationToken ct = default);

    /// <summary>
    /// Starts monitoring a specific beacon for proximity events (enter/exit range).
    /// </summary>
    /// <param name="beacon">The BeaconInfo object representing the beacon to monitor.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing true if monitoring started successfully; false otherwise.
    /// </returns>
    /// <remarks>
    /// The beacon parameter must have a valid, non-empty UUID.
    /// Once monitoring is started, the OnBeaconProximityChanged observable will emit BeaconProximityEvent instances
    /// when the monitored beacon enters or exits range, or when proximity changes.
    /// Multiple calls to monitor the same beacon are idempotent.
    /// Possible error codes: BEACON.INVALID_BEACON, BEACON.PERMISSION_DENIED, BEACON.NOT_SUPPORTED
    /// </remarks>
    Task<Result<bool>> StartMonitoringAsync(BeaconInfo beacon, CancellationToken ct = default);

    /// <summary>
    /// Stops monitoring a specific beacon for proximity events.
    /// </summary>
    /// <param name="beaconUUID">The UUID string identifying the beacon to stop monitoring.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing true if monitoring was stopped successfully; false if the beacon was not being monitored.
    /// </returns>
    /// <remarks>
    /// After this is called, proximity events for this specific beacon will no longer be emitted from the OnBeaconProximityChanged observable.
    /// Possible error codes: BEACON.NOT_FOUND, BEACON.NOT_SUPPORTED
    /// </remarks>
    Task<Result<bool>> StopMonitoringAsync(string beaconUUID, CancellationToken ct = default);

    /// <summary>
    /// Gets a list of all currently monitored beacons.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing a read-only list of BeaconInfo objects representing currently monitored beacons.
    /// Returns an empty list if no beacons are currently being monitored.
    /// </returns>
    /// <remarks>
    /// This method returns the beacons that are actively being monitored for proximity events via StartMonitoringAsync.
    /// Possible error codes: BEACON.NOT_SUPPORTED
    /// </remarks>
    Task<Result<IReadOnlyList<BeaconInfo>>> GetMonitoredBeaconsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets ranging data (distance and signal strength) for beacons matching a UUID.
    /// </summary>
    /// <param name="uuid">The UUID to range for. Can be a specific UUID string or null to range all beacons in range.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing a read-only list of BeaconInfo objects with ranging data (distance, RSSI) populated.
    /// Returns an empty list if no beacons match the UUID or if no beacons are in range.
    /// </returns>
    /// <remarks>
    /// Ranging provides more detailed information about beacons including estimated distance and signal strength (RSSI).
    /// If uuid is null, ranging is performed for all beacons currently in range.
    /// Ranging updates may be continuous and high-frequency; filtering may be required depending on use case.
    /// Possible error codes: BEACON.RANGE_FAILED, BEACON.NOT_SUPPORTED
    /// </remarks>
    Task<Result<IReadOnlyList<BeaconInfo>>> RangeBeaconsAsync(string? uuid, CancellationToken ct = default);

    /// <summary>
    /// Checks whether BLE beacon detection capability is available on the current device.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing true if the device supports BLE beacon detection; false otherwise.
    /// </returns>
    /// <remarks>
    /// This method checks for hardware and OS-level support for BLE beacon operations.
    /// A return value of false indicates the device may lack Bluetooth hardware or the running OS does not support beacon detection.
    /// Possible error codes: BEACON.NOT_SUPPORTED
    /// </remarks>
    Task<Result<bool>> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets an observable stream of beacon proximity change events.
    /// </summary>
    /// <value>
    /// An IObservable that emits BeaconProximityEvent instances when monitored beacons enter/exit range or proximity changes.
    /// The observable emits events for beacons that have been registered via StartMonitoringAsync.
    /// Subscription to this observable does not automatically start monitoring; call StartMonitoringAsync for specific beacons
    /// to enable event notifications. The observable will emit events until monitoring is stopped or the service is disposed.
    /// </value>
    IObservable<BeaconProximityEvent> OnBeaconProximityChanged { get; }
}

/// <summary>
/// Represents a beacon proximity change event triggered when a monitored beacon enters/exits range or proximity changes.
/// </summary>
/// <param name="Beacon">The BeaconInfo object representing the beacon that triggered this event.</param>
/// <param name="EventType">The type of proximity change: 1 for ENTER, 0 for EXIT, 2 for PROXIMITY_CHANGED.</param>
/// <param name="PreviousProximity">The previous proximity level before this event: "Immediate", "Near", "Far", or "Unknown". Null for ENTER events.</param>
/// <param name="CurrentProximity">The current proximity level after this event: "Immediate", "Near", "Far", or "Unknown".</param>
/// <param name="DetectedAt">The UTC timestamp when the proximity change was detected by the system.</param>
public sealed record BeaconProximityEvent(
    /// <summary>
    /// Gets the BeaconInfo object representing the beacon that triggered this proximity event.
    /// </summary>
    BeaconInfo Beacon,

    /// <summary>
    /// Gets the type of proximity change event: 1 indicates ENTER (beacon entered detection range),
    /// 0 indicates EXIT (beacon exited detection range),
    /// 2 indicates PROXIMITY_CHANGED (proximity level changed while beacon remains in range).
    /// </summary>
    int EventType,

    /// <summary>
    /// Gets the previous proximity level before this change event.
    /// Values are "Immediate", "Near", "Far", or "Unknown". Null for ENTER events (no previous proximity).
    /// </summary>
    string? PreviousProximity,

    /// <summary>
    /// Gets the current proximity level after this change event.
    /// Values are "Immediate", "Near", "Far", or "Unknown".
    /// </summary>
    string CurrentProximity,

    /// <summary>
    /// Gets the UTC timestamp when the proximity change was detected by the system.
    /// </summary>
    DateTime DetectedAt)
{
    /// <summary>
    /// Gets the BeaconInfo object representing the beacon that triggered this proximity event.
    /// </summary>
    public BeaconInfo Beacon { get; } = Beacon ?? throw new ArgumentNullException(nameof(Beacon));

    /// <summary>
    /// Gets the type of proximity change event: 1 indicates ENTER (beacon entered detection range),
    /// 0 indicates EXIT (beacon exited detection range),
    /// 2 indicates PROXIMITY_CHANGED (proximity level changed while beacon remains in range).
    /// </summary>
    public int EventType { get; } = EventType is 0 or 1 or 2
        ? EventType
        : throw new ArgumentException("EventType must be 0 (EXIT), 1 (ENTER), or 2 (PROXIMITY_CHANGED)", nameof(EventType));

    /// <summary>
    /// Gets the previous proximity level before this change event.
    /// Values are "Immediate", "Near", "Far", or "Unknown". Null for ENTER events (no previous proximity).
    /// </summary>
    public string? PreviousProximity { get; } = PreviousProximity;

    /// <summary>
    /// Gets the current proximity level after this change event.
    /// Values are "Immediate", "Near", "Far", or "Unknown".
    /// </summary>
    public string CurrentProximity { get; } = CurrentProximity ?? throw new ArgumentNullException(nameof(CurrentProximity));

    /// <summary>
    /// Gets the UTC timestamp when the proximity change was detected by the system.
    /// </summary>
    public DateTime DetectedAt { get; } = DetectedAt;
}
