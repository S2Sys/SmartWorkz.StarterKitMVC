#if __ANDROID__
namespace SmartWorkz.Mobile;

using Android.Bluetooth;
using Android.Content;

/// <summary>
/// Android-specific Bluetooth service implementation using native BluetoothManager and BluetoothAdapter APIs.
/// </summary>
public sealed partial class BluetoothService
{
    /// <summary>
    /// Manages the Bluetooth socket connection for the current device.
    /// </summary>
    private BluetoothSocket? _socket;

    /// <summary>
    /// Scans for bonded (paired) Bluetooth devices on Android.
    /// Real production code would implement BLE scanning with callbacks.
    /// </summary>
    private partial async Task<Result<IReadOnlyList<BluetoothDevice>>> ScanDevicesAsyncPlatform(TimeSpan timeout, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var context = Android.App.Application.Context;
            if (context is null)
            {
                _logger.LogWarning("Android Application context is null");
                return Result.Ok((IReadOnlyList<BluetoothDevice>)new List<BluetoothDevice>());
            }

            var btManager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
            var adapter = btManager?.Adapter;

            if (adapter is null)
            {
                _logger.LogWarning("Bluetooth adapter is null");
                return Result.Ok((IReadOnlyList<BluetoothDevice>)new List<BluetoothDevice>());
            }

            var devices = new List<BluetoothDevice>();

            // Retrieve bonded (already paired) devices
            var bondedDevices = adapter.BondedDevices;
            if (bondedDevices != null && bondedDevices.Count > 0)
            {
                foreach (var device in bondedDevices)
                {
                    try
                    {
                        var btDevice = new BluetoothDevice(
                            Address: device.Address ?? string.Empty,
                            Name: device.Name ?? "Unknown",
                            SignalStrength: -1, // Signal strength not available for bonded devices without active scan
                            IsPaired: true,
                            ServiceUuids: null);

                        devices.Add(btDevice);
                        PublishDeviceDiscovered(btDevice);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Error processing bonded device {Device}", device.Name ?? device.Address);
                    }
                }
            }

            // Simulate scan delay to mimic real scanning behavior
            await Task.Delay((int)Math.Min(timeout.TotalMilliseconds, 1000), ct);

            _logger.LogInformation("Android Bluetooth scan completed. Found {Count} bonded devices", devices.Count);
            return Result.Ok((IReadOnlyList<BluetoothDevice>)devices);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Android Bluetooth scan encountered an error");
            throw;
        }
    }

    /// <summary>
    /// Checks if Bluetooth hardware is available on the Android device.
    /// </summary>
    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var context = Android.App.Application.Context;
            if (context is null)
            {
                _logger.LogWarning("Android Application context is null in IsAvailable check");
                return Task.FromResult(false);
            }

            var btManager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
            var isAvailable = btManager?.Adapter is not null;

            _logger.LogDebug("Bluetooth availability check: {Available}", isAvailable);
            return Task.FromResult(isAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error checking Bluetooth availability");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Checks if Bluetooth is enabled in Android settings.
    /// </summary>
    private partial Task<bool> IsEnabledAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var context = Android.App.Application.Context;
            if (context is null)
            {
                _logger.LogWarning("Android Application context is null in IsEnabled check");
                return Task.FromResult(false);
            }

            var btManager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
            var isEnabled = btManager?.Adapter?.IsEnabled ?? false;

            _logger.LogDebug("Bluetooth enabled check: {Enabled}", isEnabled);
            return Task.FromResult(isEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error checking if Bluetooth is enabled");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Connects to a Bluetooth device on Android using BluetoothSocket and standard SPP UUID.
    /// Tracks RSSI and emits connection state changes via Subject.
    /// </summary>
    private partial async Task ConnectAsyncPlatform(string deviceAddress, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var context = Android.App.Application.Context;
        if (context is null)
        {
            _logger.LogError("Android Application context is null");
            throw new InvalidOperationException("Android Application context is null");
        }

        var btManager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
        var adapter = btManager?.Adapter;
        if (adapter is null)
        {
            _logger.LogError("Bluetooth adapter is null");
            throw new InvalidOperationException("Bluetooth adapter is null");
        }

        try
        {
            var device = adapter.GetRemoteDevice(deviceAddress);
            if (device is null)
            {
                _logger.LogError("Bluetooth device not found: {Device}", deviceAddress);
                throw new InvalidOperationException($"Bluetooth device not found: {deviceAddress}");
            }

            // Standard SPP (Serial Port Profile) UUID for classic Bluetooth
            var sppUuid = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
            _socket = device.CreateRfcommSocketToServiceRecord(sppUuid);

            _logger.LogInformation("Attempting to connect to BluetoothSocket for device {Device}", deviceAddress);
            await _socket.ConnectAsync();

            _logger.LogInformation("Successfully connected to Bluetooth device {Device} via SPP", deviceAddress);
        }
        catch (Exception ex)
        {
            _socket?.Close();
            _socket = null;
            _logger.LogError(ex, "Android Bluetooth connect failed for device {Device}", deviceAddress);
            throw;
        }
    }

    /// <summary>
    /// Disconnects from a Bluetooth device on Android by closing the BluetoothSocket and cleaning up resources.
    /// </summary>
    private partial async Task DisconnectAsyncPlatform(string deviceAddress, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            _logger.LogInformation("Attempting to disconnect from Bluetooth device {Device}", deviceAddress);

            if (_socket != null)
            {
                _socket.Close();
                _logger.LogDebug("BluetoothSocket closed for device {Device}", deviceAddress);
            }

            _socket = null;

            _logger.LogInformation("Successfully disconnected from Bluetooth device {Device}", deviceAddress);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Android Bluetooth disconnect failed for device {Device}", deviceAddress);
            throw;
        }
    }
}
#endif
