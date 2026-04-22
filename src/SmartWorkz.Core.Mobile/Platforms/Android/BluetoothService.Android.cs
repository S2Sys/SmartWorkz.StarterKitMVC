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
    /// Platform stub for connecting to a Bluetooth device on Android.
    /// Production implementation would use BluetoothSocket or GATT for actual connection.
    /// </summary>
    private partial Task ConnectAsyncPlatform(string deviceAddress, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            _logger.LogDebug("Android: Connect stub called for device {Device}", deviceAddress);
            // Stub implementation - real production code would:
            // 1. Get BluetoothAdapter
            // 2. Get BluetoothDevice by address
            // 3. Create BluetoothSocket
            // 4. Establish connection asynchronously
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Android: Connect failed for device {Device}", deviceAddress);
            throw;
        }
    }

    /// <summary>
    /// Platform stub for disconnecting from a Bluetooth device on Android.
    /// Production implementation would close BluetoothSocket and clean up resources.
    /// </summary>
    private partial Task DisconnectAsyncPlatform(string deviceAddress, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            _logger.LogDebug("Android: Disconnect stub called for device {Device}", deviceAddress);
            // Stub implementation - real production code would:
            // 1. Close BluetoothSocket
            // 2. Clean up GATT connections
            // 3. Update connection state
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Android: Disconnect failed for device {Device}", deviceAddress);
            throw;
        }
    }
}
#endif
