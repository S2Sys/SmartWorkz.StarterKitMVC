namespace SmartWorkz.Mobile;

/// <summary>
/// Provides Bluetooth device discovery, scanning, and connection services.
/// Supports Android and iOS platforms; graceful degradation on unsupported platforms.
/// </summary>
public interface IBluetoothService
{
    /// <summary>
    /// Scans for nearby Bluetooth devices asynchronously.
    /// Requires Bluetooth permission (Android) or location access (iOS for device scanning).
    /// </summary>
    /// <param name="timeout">Maximum time to scan for devices before returning results.</param>
    /// <param name="ct">Cancellation token to cancel the scanning operation.</param>
    /// <returns>A Result containing the list of discovered devices, or an error if scanning failed.</returns>
    Task<Result<IReadOnlyList<BluetoothDevice>>> ScanDevicesAsync(TimeSpan timeout, CancellationToken ct = default);

    /// <summary>
    /// Returns an observable stream of newly discovered Bluetooth devices during scanning.
    /// </summary>
    /// <returns>An observable that emits BluetoothDevice instances as they are discovered.</returns>
    IObservable<BluetoothDevice> OnDeviceDiscovered();

    /// <summary>
    /// Checks if Bluetooth hardware is available on the device.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if Bluetooth hardware is available; otherwise false.</returns>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks if Bluetooth is enabled by the user in device settings.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if Bluetooth is enabled; otherwise false.</returns>
    Task<bool> IsEnabledAsync(CancellationToken ct = default);

    /// <summary>
    /// Establishes a connection to a Bluetooth device.
    /// </summary>
    /// <param name="deviceAddress">The address (MAC on Android, UUID on iOS) of the device to connect to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result indicating success or failure of the connection attempt.</returns>
    Task<Result<bool>> ConnectAsync(string deviceAddress, CancellationToken ct = default);

    /// <summary>
    /// Closes the connection to a Bluetooth device.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to disconnect from.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result indicating success or failure of the disconnection attempt.</returns>
    Task<Result<bool>> DisconnectAsync(string deviceAddress, CancellationToken ct = default);

    /// <summary>
    /// Returns an observable stream of connection state changes.
    /// Emits true when connection is established, false when disconnected.
    /// </summary>
    /// <returns>An observable that emits boolean values indicating connection state.</returns>
    IObservable<bool> OnConnectionStateChanged();
}
