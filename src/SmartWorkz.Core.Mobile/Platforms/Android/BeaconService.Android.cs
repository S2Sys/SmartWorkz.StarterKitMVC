#if __ANDROID__
namespace SmartWorkz.Mobile;

using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using SmartWorkz.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Android-specific BLE beacon service implementation using BluetoothAdapter and BLE scanning APIs.
/// </summary>
partial class BeaconService
{
    private BluetoothManager? _bluetoothManager;
    private BluetoothAdapter? _bluetoothAdapter;
    private BluetoothLeScanner? _bluetoothScanner;
    private ScanCallback? _scanCallback;
    private ScanCallback? _monitoringCallback;
    private readonly ConcurrentDictionary<string, BeaconInfo> _scanResults =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ScanCallback> _monitoringCallbacks =
        new(StringComparer.OrdinalIgnoreCase);
    private const int ScanDurationSeconds = 5;
    private const int RangeScanDurationSeconds = 3;

    /// <summary>
    /// Android-specific beacon scan implementation using BluetoothLeScanner.
    /// Scans for BLE devices for a duration and converts results to BeaconInfo records.
    /// </summary>
    private partial async Task<Result<IReadOnlyList<BeaconInfo>>> ScanForBeaconsAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            _logger.LogDebug("Starting Android BLE beacon scan");

            var context = GetAndroidContext();
            if (context == null)
            {
                _logger.LogError("Android context unavailable for beacon scan");
                return Result.Fail<IReadOnlyList<BeaconInfo>>(
                    Error.NotFound("BEACON.CONTEXT_NOT_FOUND", "Android context unavailable"));
            }

            _bluetoothManager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
            if (_bluetoothManager == null)
            {
                _logger.LogError("BluetoothManager service unavailable");
                return Result.Fail<IReadOnlyList<BeaconInfo>>(
                    Error.NotFound("BEACON.SERVICE_NOT_FOUND", "BluetoothManager service unavailable"));
            }

            _bluetoothAdapter = _bluetoothManager.Adapter;
            if (_bluetoothAdapter == null)
            {
                _logger.LogError("BluetoothAdapter unavailable");
                return Result.Fail<IReadOnlyList<BeaconInfo>>(
                    Error.NotFound("BEACON.ADAPTER_NOT_FOUND", "BluetoothAdapter unavailable"));
            }

            if (!_bluetoothAdapter.IsEnabled)
            {
                _logger.LogWarning("Bluetooth is disabled on device");
                return Result.Fail<IReadOnlyList<BeaconInfo>>(
                    Error.Validation("BEACON.BLUETOOTH_DISABLED", "Bluetooth is disabled"));
            }

            _bluetoothScanner = _bluetoothAdapter.BluetoothLeScanner;
            if (_bluetoothScanner == null)
            {
                _logger.LogError("BluetoothLeScanner unavailable");
                return Result.Fail<IReadOnlyList<BeaconInfo>>(
                    Error.NotFound("BEACON.SCANNER_NOT_FOUND", "BluetoothLeScanner unavailable"));
            }

            // Clear previous results
            _scanResults.Clear();

            // Create scan callback
            _scanCallback = new BeaconScanCallback(_scanResults, _logger);

            // Create scan settings for BLE scan
            var settings = new ScanSettings.Builder()
                .SetScanMode((int)Android.Bluetooth.LE.ScanMode.LowPower)
                .Build();

            // Start scan
            _bluetoothScanner.StartScan(null, settings, _scanCallback);

            // Wait for scan duration
            await Task.Delay(ScanDurationSeconds * 1000, ct);

            // Stop scan
            _bluetoothScanner.StopScan(_scanCallback);

            var results = _scanResults.Values.ToList().AsReadOnly();
            _logger.LogInformation("BLE beacon scan completed: Found {Count} beacons", results.Count);
            return Result.Ok((IReadOnlyList<BeaconInfo>)results);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("BLE beacon scan cancelled");
            StopScanSafely();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Android BLE beacon scan failed");
            StopScanSafely();
            return Result.Fail<IReadOnlyList<BeaconInfo>>(Error.FromException(ex));
        }
    }

    /// <summary>
    /// Android-specific start monitoring implementation.
    /// Starts a continuous BLE scan for a specific beacon UUID and emits proximity events.
    /// </summary>
    private partial async Task<Result<bool>> StartMonitoringAsyncPlatform(BeaconInfo beacon, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            _logger.LogDebug("Starting Android beacon monitoring for UUID: {UUID}", beacon.UUID);

            // Validate UUID format
            if (!Guid.TryParse(beacon.UUID, out _))
            {
                _logger.LogWarning("Invalid UUID format for monitoring: {UUID}", beacon.UUID);
                return Result.Fail<bool>(
                    Error.Validation("BEACON.INVALID_UUID", "Invalid beacon UUID format"));
            }

            var context = GetAndroidContext();
            if (context == null)
            {
                _logger.LogError("Android context unavailable for beacon monitoring");
                return Result.Fail<bool>(
                    Error.NotFound("BEACON.CONTEXT_NOT_FOUND", "Android context unavailable"));
            }

            _bluetoothManager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
            _bluetoothAdapter = _bluetoothManager?.Adapter;

            if (_bluetoothAdapter == null || !_bluetoothAdapter.IsEnabled)
            {
                _logger.LogWarning("BluetoothAdapter unavailable or disabled for monitoring");
                return Result.Fail<bool>(
                    Error.Validation("BEACON.BLUETOOTH_DISABLED", "Bluetooth is disabled"));
            }

            _bluetoothScanner = _bluetoothAdapter.BluetoothLeScanner;
            if (_bluetoothScanner == null)
            {
                _logger.LogError("BluetoothLeScanner unavailable for monitoring");
                return Result.Fail<bool>(
                    Error.NotFound("BEACON.SCANNER_NOT_FOUND", "BluetoothLeScanner unavailable"));
            }

            // Create monitoring callback for this specific beacon
            var monitoringCallback = new BeaconMonitoringCallback(beacon, this, _logger);
            _monitoringCallbacks.TryAdd(beacon.UUID, monitoringCallback);

            // Start continuous scan
            var settings = new ScanSettings.Builder()
                .SetScanMode((int)Android.Bluetooth.LE.ScanMode.LowLatency)
                .Build();

            _bluetoothScanner.StartScan(null, settings, monitoringCallback);

            _logger.LogInformation("Started monitoring beacon: {UUID}", beacon.UUID);
            return await Task.FromResult(Result.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Android beacon monitoring for {UUID}", beacon.UUID);
            return Result.Fail<bool>(Error.FromException(ex));
        }
    }

    /// <summary>
    /// Android-specific stop monitoring implementation.
    /// Stops the continuous BLE scan for a specific beacon UUID.
    /// </summary>
    private partial async Task<Result<bool>> StopMonitoringAsyncPlatform(string beaconUUID, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            _logger.LogDebug("Stopping Android beacon monitoring for UUID: {UUID}", beaconUUID);

            if (_bluetoothScanner == null)
            {
                _logger.LogDebug("No active beacon monitoring to stop");
                return await Task.FromResult(Result.Ok(true));
            }

            if (_monitoringCallbacks.TryRemove(beaconUUID, out var callback))
            {
                _bluetoothScanner.StopScan(callback);
                _logger.LogInformation("Stopped monitoring beacon: {UUID}", beaconUUID);
            }

            return await Task.FromResult(Result.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop Android beacon monitoring for {UUID}", beaconUUID);
            return Result.Fail<bool>(Error.FromException(ex));
        }
    }

    /// <summary>
    /// Android-specific beacon ranging implementation.
    /// Performs a brief BLE scan and calculates distances using path-loss model.
    /// </summary>
    private partial async Task<Result<IReadOnlyList<BeaconInfo>>> RangeBeaconsAsyncPlatform(string? uuid, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            _logger.LogDebug("Starting Android beacon ranging for UUID: {UUID}", uuid ?? "all");

            var context = GetAndroidContext();
            if (context == null)
            {
                _logger.LogError("Android context unavailable for beacon ranging");
                return Result.Fail<IReadOnlyList<BeaconInfo>>(
                    Error.NotFound("BEACON.CONTEXT_NOT_FOUND", "Android context unavailable"));
            }

            _bluetoothManager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
            _bluetoothAdapter = _bluetoothManager?.Adapter;

            if (_bluetoothAdapter == null || !_bluetoothAdapter.IsEnabled)
            {
                _logger.LogWarning("BluetoothAdapter unavailable or disabled for ranging");
                return Result.Fail<IReadOnlyList<BeaconInfo>>(
                    Error.Validation("BEACON.BLUETOOTH_DISABLED", "Bluetooth is disabled"));
            }

            _bluetoothScanner = _bluetoothAdapter.BluetoothLeScanner;
            if (_bluetoothScanner == null)
            {
                _logger.LogError("BluetoothLeScanner unavailable for ranging");
                return Result.Fail<IReadOnlyList<BeaconInfo>>(
                    Error.NotFound("BEACON.SCANNER_NOT_FOUND", "BluetoothLeScanner unavailable"));
            }

            // Clear range results
            _scanResults.Clear();

            // Create scan callback
            _scanCallback = new BeaconScanCallback(_scanResults, _logger);

            // Create scan settings for brief scan
            var settings = new ScanSettings.Builder()
                .SetScanMode((int)Android.Bluetooth.LE.ScanMode.LowLatency)
                .Build();

            // Start scan
            _bluetoothScanner.StartScan(null, settings, _scanCallback);

            // Wait for range scan duration
            await Task.Delay(RangeScanDurationSeconds * 1000, ct);

            // Stop scan
            _bluetoothScanner.StopScan(_scanCallback);

            // Filter by UUID if specified
            var results = _scanResults.Values
                .Where(b => uuid == null || b.UUID.Equals(uuid, StringComparison.OrdinalIgnoreCase))
                .Select(b => CalculateDistance(b))
                .ToList()
                .AsReadOnly();

            _logger.LogInformation("Beacon ranging completed: Found {Count} beacons", results.Count);
            return Result.Ok((IReadOnlyList<BeaconInfo>)results);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Beacon ranging cancelled");
            StopScanSafely();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Android beacon ranging failed");
            StopScanSafely();
            return Result.Fail<IReadOnlyList<BeaconInfo>>(Error.FromException(ex));
        }
    }

    /// <summary>
    /// Android-specific availability check implementation.
    /// Checks for Bluetooth LE hardware support via PackageManager.
    /// </summary>
    private partial async Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var context = GetAndroidContext();
            if (context == null)
            {
                _logger.LogDebug("Android context unavailable for beacon availability check");
                return false;
            }

            var packageManager = context.PackageManager;
            if (packageManager == null)
            {
                return false;
            }

            // Check for Bluetooth LE support
            var hasBluetoothLE = packageManager.HasSystemFeature(
                PackageManager.FeatureBluetoothLe);

            _bluetoothManager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
            var hasAdapter = _bluetoothManager?.Adapter != null;

            var isAvailable = hasBluetoothLE && hasAdapter;
            _logger.LogDebug("Beacon availability (BLE: {BLE}, Adapter: {Adapter}): {Available}",
                hasBluetoothLE, hasAdapter, isAvailable);

            return await Task.FromResult(isAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error checking beacon availability");
            return false;
        }
    }

    /// <summary>
    /// Calculates distance from RSSI using path-loss model.
    /// Formula: distance = 10^((TxPower - RSSI) / (10 * N))
    /// Where N is the path-loss exponent (typically 2 for BLE).
    /// </summary>
    private BeaconInfo CalculateDistance(BeaconInfo beacon)
    {
        const int txPower = -59; // Default TX power at 1 meter for typical BLE beacons
        const double pathLossExponent = 2.0;

        if (beacon.RSSI >= 0)
        {
            // Invalid RSSI, return original
            return beacon;
        }

        try
        {
            var distance = Math.Pow(10, ((double)(txPower - beacon.RSSI) / (10 * pathLossExponent)));

            // Clamp distance to reasonable bounds (0.5 to 100 meters)
            distance = Math.Max(0.5, Math.Min(100, distance));

            // Create new BeaconInfo with calculated distance
            return beacon with { Distance = distance };
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error calculating distance for beacon {UUID}", beacon.UUID);
            return beacon;
        }
    }

    /// <summary>
    /// Safely stops all ongoing BLE scans.
    /// </summary>
    private void StopScanSafely()
    {
        try
        {
            if (_bluetoothScanner == null || _scanCallback == null)
            {
                return;
            }

            _bluetoothScanner.StopScan(_scanCallback);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error stopping BLE scan");
        }
    }

    /// <summary>
    /// Helper method to get Android context from MAUI application.
    /// </summary>
    private static Context? GetAndroidContext()
    {
        try
        {
            var context = Microsoft.Maui.Controls.Application.Current?.MainPage?.Handler?.MauiContext?.Context;
            if (context != null)
            {
                return context;
            }

            return Android.App.Application.Context;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting Android context: {ex.Message}");
            return null;
        }
    }
}

/// <summary>
/// BLE scan callback for processing BLE scan results and converting to BeaconInfo.
/// </summary>
public class BeaconScanCallback : ScanCallback
{
    private readonly ConcurrentDictionary<string, BeaconInfo> _results;
    private readonly ILogger<BeaconService> _logger;

    public BeaconScanCallback(ConcurrentDictionary<string, BeaconInfo> results, ILogger<BeaconService> logger)
    {
        _results = results;
        _logger = logger;
    }

    public override void OnScanResult(ScanCallbackType callbackType, ScanResult? result)
    {
        base.OnScanResult(callbackType, result);

        if (result == null)
        {
            return;
        }

        try
        {
            var device = result.Device;
            if (device == null)
            {
                return;
            }

            // Try to parse iBeacon format from advertisement data
            var beaconInfo = ParseBeaconFromAdvertisement(result);
            if (beaconInfo != null)
            {
                _results.AddOrUpdate(beaconInfo.UUID, beaconInfo, (_, _) => beaconInfo);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error processing scan result");
        }
    }

    public override void OnScanFailed(ScanFailure errorCode)
    {
        base.OnScanFailed(errorCode);
        _logger.LogWarning("BLE scan failed with error code: {ErrorCode}", errorCode);
    }

    /// <summary>
    /// Parses iBeacon format from BLE advertisement data.
    /// iBeacon format: Apple manufacturer ID (0x004C) followed by:
    /// - Type: 0x02
    /// - Length: 0x15 (21 bytes)
    /// - UUID: 16 bytes
    /// - Major: 2 bytes (big-endian)
    /// - Minor: 2 bytes (big-endian)
    /// </summary>
    private BeaconInfo? ParseBeaconFromAdvertisement(ScanResult result)
    {
        try
        {
            var scanRecord = result.ScanRecord;
            if (scanRecord == null)
            {
                return null;
            }

            var manufacturerData = scanRecord.GetManufacturerSpecificData(0x004C); // Apple manufacturer ID
            if (manufacturerData == null || manufacturerData.Length < 21)
            {
                return null;
            }

            // Check iBeacon type and length
            if (manufacturerData[0] != 0x02 || manufacturerData[1] != 0x15)
            {
                return null;
            }

            // Extract UUID (16 bytes, starting at index 2)
            var uuidBytes = new byte[16];
            Array.Copy(manufacturerData, 2, uuidBytes, 0, 16);
            var uuid = new Guid(uuidBytes).ToString().ToUpper();

            // Extract Major (2 bytes at index 18, big-endian)
            var major = (manufacturerData[18] << 8) | manufacturerData[19];

            // Extract Minor (2 bytes at index 20, big-endian)
            var minor = (manufacturerData[20] << 8) | manufacturerData[21];

            // Get RSSI
            var rssi = result.Rssi;

            // Create BeaconInfo
            var beaconInfo = new BeaconInfo(
                UUID: uuid,
                Major: major,
                Minor: minor,
                Identifier: $"{uuid}-{major}-{minor}",
                RSSI: rssi,
                Distance: null,
                BeaconType: "iBeacon",
                IsReachable: true,
                LastSeenAt: DateTime.UtcNow);

            return beaconInfo;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}

/// <summary>
/// BLE scan callback for continuous beacon monitoring with proximity event detection.
/// </summary>
public class BeaconMonitoringCallback : ScanCallback
{
    private readonly BeaconInfo _targetBeacon;
    private readonly BeaconService _service;
    private readonly ILogger<BeaconService> _logger;
    private bool _beaconDetected = false;
    private string? _lastProximity;

    public BeaconMonitoringCallback(BeaconInfo targetBeacon, BeaconService service, ILogger<BeaconService> logger)
    {
        _targetBeacon = targetBeacon;
        _service = service;
        _logger = logger;
    }

    public override void OnScanResult(ScanCallbackType callbackType, ScanResult? result)
    {
        base.OnScanResult(callbackType, result);

        if (result == null)
        {
            return;
        }

        try
        {
            // Parse beacon from advertisement
            var beaconInfo = ParseBeaconFromAdvertisement(result);
            if (beaconInfo == null || !beaconInfo.UUID.Equals(_targetBeacon.UUID, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var currentProximity = beaconInfo.Proximity();

            // Detect enter event (beacon not detected before but detected now)
            if (!_beaconDetected)
            {
                _beaconDetected = true;
                _lastProximity = currentProximity;
                var @event = new BeaconProximityEvent(
                    Beacon: beaconInfo,
                    EventType: 1, // ENTER
                    PreviousProximity: null,
                    CurrentProximity: currentProximity,
                    DetectedAt: DateTime.UtcNow);
                _service.RaiseProximityChangeEvent(@event);
            }
            // Detect proximity change (beacon detected, but proximity changed)
            else if (_lastProximity != currentProximity)
            {
                _lastProximity = currentProximity;
                var @event = new BeaconProximityEvent(
                    Beacon: beaconInfo,
                    EventType: 2, // PROXIMITY_CHANGED
                    PreviousProximity: _lastProximity,
                    CurrentProximity: currentProximity,
                    DetectedAt: DateTime.UtcNow);
                _service.RaiseProximityChangeEvent(@event);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error processing monitoring scan result");
        }
    }

    public override void OnScanFailed(ScanFailure errorCode)
    {
        base.OnScanFailed(errorCode);
        _logger.LogWarning("Beacon monitoring scan failed with error code: {ErrorCode}", errorCode);
    }

    /// <summary>
    /// Simulates exit event when beacon is lost.
    /// </summary>
    public void SimulateExitEvent()
    {
        if (_beaconDetected)
        {
            _beaconDetected = false;
            var @event = new BeaconProximityEvent(
                Beacon: _targetBeacon,
                EventType: 0, // EXIT
                PreviousProximity: _lastProximity,
                CurrentProximity: "Unknown",
                DetectedAt: DateTime.UtcNow);
            _service.RaiseProximityChangeEvent(@event);
        }
    }

    /// <summary>
    /// Parses iBeacon format from BLE advertisement data.
    /// </summary>
    private BeaconInfo? ParseBeaconFromAdvertisement(ScanResult result)
    {
        try
        {
            var scanRecord = result.ScanRecord;
            if (scanRecord == null)
            {
                return null;
            }

            var manufacturerData = scanRecord.GetManufacturerSpecificData(0x004C); // Apple manufacturer ID
            if (manufacturerData == null || manufacturerData.Length < 21)
            {
                return null;
            }

            // Check iBeacon type and length
            if (manufacturerData[0] != 0x02 || manufacturerData[1] != 0x15)
            {
                return null;
            }

            // Extract UUID (16 bytes, starting at index 2)
            var uuidBytes = new byte[16];
            Array.Copy(manufacturerData, 2, uuidBytes, 0, 16);
            var uuid = new Guid(uuidBytes).ToString().ToUpper();

            // Extract Major (2 bytes at index 18, big-endian)
            var major = (manufacturerData[18] << 8) | manufacturerData[19];

            // Extract Minor (2 bytes at index 20, big-endian)
            var minor = (manufacturerData[20] << 8) | manufacturerData[21];

            // Get RSSI and calculate distance
            var rssi = result.Rssi;
            var distance = CalculateDistance(rssi);

            // Create BeaconInfo with distance calculated
            var beaconInfo = new BeaconInfo(
                UUID: uuid,
                Major: major,
                Minor: minor,
                Identifier: $"{uuid}-{major}-{minor}",
                RSSI: rssi,
                Distance: distance,
                BeaconType: "iBeacon",
                IsReachable: true,
                LastSeenAt: DateTime.UtcNow);

            return beaconInfo;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    /// <summary>
    /// Calculates distance from RSSI using path-loss model.
    /// </summary>
    private double CalculateDistance(int rssi)
    {
        const int txPower = -59;
        const double pathLossExponent = 2.0;

        if (rssi >= 0)
        {
            return double.NaN;
        }

        try
        {
            var distance = Math.Pow(10, ((double)(txPower - rssi) / (10 * pathLossExponent)));
            return Math.Max(0.5, Math.Min(100, distance));
        }
        catch
        {
            return double.NaN;
        }
    }
}

#endif
