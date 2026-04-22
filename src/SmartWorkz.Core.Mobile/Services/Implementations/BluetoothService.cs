namespace SmartWorkz.Mobile;

using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides Bluetooth device discovery, scanning, and connection management.
/// Uses partial classes and conditional compilation for platform-specific implementations.
/// </summary>
public sealed partial class BluetoothService : IBluetoothService
{
    private readonly IPermissionService _permissions;
    private readonly ILogger<BluetoothService> _logger;
    private readonly Subject<BluetoothDevice> _deviceDiscovered = new();
    private string? _connectedDeviceAddress;
    private readonly Dictionary<string, BluetoothConnectionState> _connectionStates = new();
    private readonly Subject<BluetoothConnectionState> _connectionStateChanged = new();

    public BluetoothService(IPermissionService permissions, ILogger<BluetoothService> logger)
    {
        _permissions = Guard.NotNull(permissions, nameof(permissions));
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    /// <summary>
    /// Scans for nearby Bluetooth devices with permission and availability checks.
    /// </summary>
    public async Task<Result<IReadOnlyList<BluetoothDevice>>> ScanDevicesAsync(TimeSpan timeout, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var available = await IsAvailableAsync(ct);
        if (!available)
        {
            _logger.LogWarning("Bluetooth is not available on this device");
            return Result.Fail<IReadOnlyList<BluetoothDevice>>(
                new Error("BT.UNAVAILABLE", "Bluetooth hardware is not available on this device"));
        }

        var enabled = await IsEnabledAsync(ct);
        if (!enabled)
        {
            _logger.LogWarning("Bluetooth is not enabled");
            return Result.Fail<IReadOnlyList<BluetoothDevice>>(
                new Error("BT.DISABLED", "Bluetooth is not enabled on this device"));
        }

        var checkStatus = await _permissions.CheckAsync(MobilePermission.Bluetooth, ct);
        if (checkStatus != PermissionStatus.Granted)
        {
            var requestStatus = await _permissions.RequestAsync(MobilePermission.Bluetooth, ct);
            if (requestStatus != PermissionStatus.Granted)
            {
                _logger.LogWarning("Bluetooth permission was denied");
                return Result.Fail<IReadOnlyList<BluetoothDevice>>(
                    new Error("BT.PERMISSION_DENIED", "Bluetooth permission was denied by user"));
            }
        }

        try
        {
            _logger.LogInformation("Starting Bluetooth device scan with timeout {Timeout}ms", timeout.TotalMilliseconds);
            return await ScanDevicesAsyncPlatform(timeout, ct);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Bluetooth scan was cancelled");
            return Result.Fail<IReadOnlyList<BluetoothDevice>>(
                new Error("BT.CANCELLED", "Bluetooth scan operation was cancelled"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bluetooth scan failed with exception");
            return Result.Fail<IReadOnlyList<BluetoothDevice>>(
                Error.FromException(ex, "BT.SCAN_FAILED"));
        }
    }

    /// <summary>
    /// Returns an observable stream of discovered Bluetooth devices during scanning.
    /// </summary>
    public IObservable<BluetoothDevice> OnDeviceDiscovered() =>
        _deviceDiscovered.AsObservable();

    /// <summary>
    /// Checks if Bluetooth hardware is available on the device.
    /// </summary>
    public Task<bool> IsAvailableAsync(CancellationToken ct = default) =>
        IsAvailableAsyncPlatform(ct);

    /// <summary>
    /// Checks if Bluetooth is enabled in device settings.
    /// </summary>
    public Task<bool> IsEnabledAsync(CancellationToken ct = default) =>
        IsEnabledAsyncPlatform(ct);

    /// <summary>
    /// Establishes a connection to a Bluetooth device.
    /// </summary>
    public async Task<Result<bool>> ConnectAsync(string deviceAddress, CancellationToken ct = default)
    {
        Guard.NotEmpty(deviceAddress, nameof(deviceAddress));
        ct.ThrowIfCancellationRequested();

        try
        {
            _logger.LogInformation("Attempting to connect to Bluetooth device {Device}", deviceAddress);
            await ConnectAsyncPlatform(deviceAddress, ct);
            _connectedDeviceAddress = deviceAddress;
            var connectionState = new BluetoothConnectionState(deviceAddress, IsConnected: true, ConnectedSince: DateTime.UtcNow);
            _connectionStates[deviceAddress] = connectionState;
            _connectionStateChanged.OnNext(connectionState);
            _logger.LogInformation("Successfully connected to Bluetooth device {Device}", deviceAddress);
            return Result.Ok(true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Bluetooth connection attempt was cancelled for device {Device}", deviceAddress);
            return Result.Fail<bool>(
                new Error("BT.CONNECT_CANCELLED", "Connection attempt was cancelled"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Bluetooth device {Device}", deviceAddress);
            return Result.Fail<bool>(
                Error.FromException(ex, "BT.CONNECT_FAILED"));
        }
    }

    /// <summary>
    /// Closes the connection to a Bluetooth device.
    /// </summary>
    public async Task<Result<bool>> DisconnectAsync(string deviceAddress, CancellationToken ct = default)
    {
        Guard.NotEmpty(deviceAddress, nameof(deviceAddress));
        ct.ThrowIfCancellationRequested();

        try
        {
            _logger.LogInformation("Attempting to disconnect from Bluetooth device {Device}", deviceAddress);
            await DisconnectAsyncPlatform(deviceAddress, ct);
            if (_connectedDeviceAddress == deviceAddress)
            {
                _connectedDeviceAddress = null;
            }
            var connectionState = new BluetoothConnectionState(deviceAddress, IsConnected: false, ConnectedSince: DateTime.UtcNow.AddSeconds(-1));
            _connectionStates[deviceAddress] = connectionState;
            _connectionStateChanged.OnNext(connectionState);
            _logger.LogInformation("Successfully disconnected from Bluetooth device {Device}", deviceAddress);
            return Result.Ok(true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Bluetooth disconnection attempt was cancelled for device {Device}", deviceAddress);
            return Result.Fail<bool>(
                new Error("BT.DISCONNECT_CANCELLED", "Disconnection attempt was cancelled"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect from Bluetooth device {Device}", deviceAddress);
            return Result.Fail<bool>(
                Error.FromException(ex, "BT.DISCONNECT_FAILED"));
        }
    }

    /// <summary>
    /// Gets the currently connected device address, if any.
    /// </summary>
    public string? ConnectedDeviceAddress => _connectedDeviceAddress;

    /// <summary>
    /// Returns the current connection state for a device.
    /// </summary>
    public async Task<BluetoothConnectionState?> GetConnectionStateAsync(string deviceAddress, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _connectionStates.TryGetValue(deviceAddress, out var state);
        return state;
    }

    /// <summary>
    /// Returns an observable stream of connection state changes for any device.
    /// </summary>
    public IObservable<BluetoothConnectionState> OnConnectionStateChanged() =>
        _connectionStateChanged.AsObservable();

    /// <summary>
    /// Internal method for platform implementations to publish discovered devices.
    /// </summary>
    internal void PublishDeviceDiscovered(BluetoothDevice device)
    {
        _logger.LogDebug("Device discovered: {Device}", device.DisplayName);
        _deviceDiscovered.OnNext(device);
    }

    // Platform-specific partial methods
    #if __ANDROID__ || __IOS__
    private partial Task<Result<IReadOnlyList<BluetoothDevice>>> ScanDevicesAsyncPlatform(TimeSpan timeout, CancellationToken ct);
    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct);
    private partial Task<bool> IsEnabledAsyncPlatform(CancellationToken ct);
    private partial Task ConnectAsyncPlatform(string deviceAddress, CancellationToken ct);
    private partial Task DisconnectAsyncPlatform(string deviceAddress, CancellationToken ct);
    #else
    // Fallback stubs for unsupported platforms
    private Task<Result<IReadOnlyList<BluetoothDevice>>> ScanDevicesAsyncPlatform(TimeSpan timeout, CancellationToken ct) =>
        Task.FromResult(Result.Ok((IReadOnlyList<BluetoothDevice>)new List<BluetoothDevice>()));

    private Task<bool> IsAvailableAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(false);

    private Task<bool> IsEnabledAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(false);

    private Task ConnectAsyncPlatform(string deviceAddress, CancellationToken ct) =>
        Task.CompletedTask;

    private Task DisconnectAsyncPlatform(string deviceAddress, CancellationToken ct) =>
        Task.CompletedTask;
    #endif
}
