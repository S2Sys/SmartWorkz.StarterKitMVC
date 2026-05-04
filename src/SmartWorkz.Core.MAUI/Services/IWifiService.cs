namespace SmartWorkz.Mobile;

/// <summary>
/// Service for discovering, monitoring, and managing WiFi network connections.
/// Provides functionality for scanning available networks, connecting/disconnecting,
/// and monitoring network changes in real-time.
/// </summary>
public interface IWifiService
{
    /// <summary>
    /// Scans for all available WiFi networks in range of the device.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing a read-only list of discovered WifiNetwork objects.
    /// On success, returns all available networks currently in range.
    /// Returns an empty list if no networks are discovered.
    /// </returns>
    /// <remarks>
    /// Scanning may take several seconds depending on the device and number of available networks.
    /// This operation requires appropriate WiFi permissions on the platform.
    /// Possible error codes: WIFI.SCAN_FAILED, WIFI.PERMISSION_DENIED, WIFI.NOT_SUPPORTED
    /// </remarks>
    Task<Result<IReadOnlyList<WifiNetwork>>> ScanForNetworksAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves information about the currently connected WiFi network.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing the WifiNetwork object if connected, or null if not currently connected to any network.
    /// </returns>
    /// <remarks>
    /// This method returns the active WiFi connection or null. It does not perform a scan;
    /// it only returns the current connection status.
    /// Possible error codes: WIFI.ACCESS_DENIED, WIFI.NOT_SUPPORTED
    /// </remarks>
    Task<Result<WifiNetwork?>> GetConnectedNetworkAsync(CancellationToken ct = default);

    /// <summary>
    /// Attempts to connect to a specified WiFi network.
    /// </summary>
    /// <param name="network">The WifiNetwork object representing the network to connect to.</param>
    /// <param name="password">The password for the network. Required if network.IsSecure is true; may be null for open networks.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing true if the connection was successful; false otherwise.
    /// </returns>
    /// <remarks>
    /// For secure networks (IsSecure == true), the password parameter must not be null.
    /// Connection attempts may fail due to incorrect password, network unavailability, or device settings.
    /// The connection process may take several seconds depending on the network and device.
    /// Possible error codes: WIFI.CONNECTION_FAILED, WIFI.INVALID_PASSWORD, WIFI.TIMEOUT, WIFI.NOT_SUPPORTED
    /// </remarks>
    Task<Result<bool>> ConnectToNetworkAsync(WifiNetwork network, string? password, CancellationToken ct = default);

    /// <summary>
    /// Disconnects from the currently connected WiFi network.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing true if disconnection was successful; false if not connected or disconnection failed.
    /// </returns>
    /// <remarks>
    /// This operation disconnects from whatever network the device is currently connected to.
    /// If the device is not currently connected to any network, this operation may return false.
    /// Possible error codes: WIFI.DISCONNECT_FAILED, WIFI.NOT_SUPPORTED
    /// </remarks>
    Task<Result<bool>> DisconnectAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks whether WiFi connectivity capability is available on the current device.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing true if the device supports WiFi connectivity; false otherwise.
    /// </returns>
    /// <remarks>
    /// This method checks for hardware and OS-level support for WiFi operations.
    /// A return value of false indicates the device may lack WiFi hardware or running OS is unsupported.
    /// Possible error codes: WIFI.NOT_SUPPORTED
    /// </remarks>
    Task<Result<bool>> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Starts monitoring the device for WiFi network changes such as connections, disconnections, and signal strength changes.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing true if monitoring started successfully; false otherwise.
    /// </returns>
    /// <remarks>
    /// Once monitoring is started, the OnNetworkChanged observable will emit WifiNetworkChangeEvent instances
    /// whenever the network status changes. Multiple calls to this method while monitoring is already active
    /// may result in a Result indicating that monitoring is already in progress.
    /// Monitoring continues in the background and respects platform limitations.
    /// Possible error codes: WIFI.PERMISSION_DENIED, WIFI.NOT_SUPPORTED
    /// </remarks>
    Task<Result<bool>> StartMonitoringNetworkChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Stops monitoring for WiFi network changes.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing true if monitoring was stopped successfully; false if monitoring was not active.
    /// </returns>
    /// <remarks>
    /// This method disables the network change monitoring started by StartMonitoringNetworkChangesAsync.
    /// After this is called, the OnNetworkChanged observable will no longer emit new events.
    /// Possible error codes: WIFI.NOT_SUPPORTED
    /// </remarks>
    Task<Result<bool>> StopMonitoringNetworkChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets an observable stream of WiFi network change events.
    /// </summary>
    /// <value>
    /// An IObservable that emits WifiNetworkChangeEvent instances when the device's WiFi connection changes.
    /// The observable emits events for network connections, disconnections, and signal strength changes.
    /// Subscription to this observable does not automatically start monitoring; call StartMonitoringNetworkChangesAsync
    /// to enable event notifications. The observable will emit events until monitoring is stopped or the service is disposed.
    /// </value>
    IObservable<WifiNetworkChangeEvent> OnNetworkChanged { get; }
}

/// <summary>
/// Represents a WiFi network change event triggered when the device's WiFi connection status or network changes.
/// </summary>
/// <param name="Previous">The previously connected WifiNetwork, or null if transitioning from disconnected state.</param>
/// <param name="Current">The currently connected WifiNetwork, or null if transitioning to disconnected state.</param>
/// <param name="EventType">The type of network change: 1 for CONNECTED, 0 for DISCONNECTED, 2 for SIGNAL_CHANGED.</param>
/// <param name="ChangedAt">The UTC timestamp when the network change was detected by the system.</param>
public sealed record WifiNetworkChangeEvent(
    /// <summary>
    /// Gets the previously connected WifiNetwork before this change event.
    /// Null if the device was not connected to any network prior to this event.
    /// </summary>
    WifiNetwork? Previous,

    /// <summary>
    /// Gets the currently connected WifiNetwork after this change event.
    /// Null if the device is now disconnected from all networks.
    /// </summary>
    WifiNetwork? Current,

    /// <summary>
    /// Gets the type of network change event: 1 indicates CONNECTED (device connected to a new network),
    /// 0 indicates DISCONNECTED (device disconnected from a network),
    /// 2 indicates SIGNAL_CHANGED (signal strength of current network changed).
    /// </summary>
    int EventType,

    /// <summary>
    /// Gets the UTC timestamp when the network change was detected by the system.
    /// </summary>
    DateTime ChangedAt)
{
    /// <summary>
    /// Gets the previously connected WifiNetwork before this change event.
    /// Null if the device was not connected to any network prior to this event.
    /// </summary>
    public WifiNetwork? Previous { get; } = Previous;

    /// <summary>
    /// Gets the currently connected WifiNetwork after this change event.
    /// Null if the device is now disconnected from all networks.
    /// </summary>
    public WifiNetwork? Current { get; } = Current;

    /// <summary>
    /// Gets the type of network change event: 1 indicates CONNECTED (device connected to a new network),
    /// 0 indicates DISCONNECTED (device disconnected from a network),
    /// 2 indicates SIGNAL_CHANGED (signal strength of current network changed).
    /// </summary>
    public int EventType { get; } = EventType is 0 or 1 or 2
        ? EventType
        : throw new ArgumentException("EventType must be 0 (DISCONNECTED), 1 (CONNECTED), or 2 (SIGNAL_CHANGED)", nameof(EventType));

    /// <summary>
    /// Gets the UTC timestamp when the network change was detected by the system.
    /// </summary>
    public DateTime ChangedAt { get; } = ChangedAt;
}
