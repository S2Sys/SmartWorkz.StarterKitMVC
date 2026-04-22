#if __ANDROID__
namespace SmartWorkz.Mobile;

using Android.App;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using SmartWorkz.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Android-specific WiFi service implementation using native WifiManager and ConnectivityManager APIs.
/// </summary>
partial class WifiService
{
    private WifiManager? _wifiManager;
    private ConnectivityManager? _connectivityManager;
    private Context? _context;
    private ConnectivityManager.NetworkCallback? _networkCallback;
    private const int ConnectionTimeoutSeconds = 10;

    /// <summary>
    /// Scans for available WiFi networks on Android using WifiManager.
    /// </summary>
    private partial async Task<Result<IReadOnlyList<WifiNetwork>>> ScanForNetworksAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var context = GetAndroidContext();
            if (context == null)
            {
                _logger.LogError("Android context unavailable for WiFi scan");
                return Result.Fail<IReadOnlyList<WifiNetwork>>(
                    Error.NotFound("WIFI.CONTEXT_NOT_FOUND", "Android context unavailable"));
            }

            _context = context;
            _wifiManager = context.GetSystemService(Context.WifiService) as WifiManager;

            if (_wifiManager == null)
            {
                _logger.LogError("WifiManager service unavailable");
                return Result.Fail<IReadOnlyList<WifiNetwork>>(
                    Error.NotFound("WIFI.SERVICE_NOT_FOUND", "WifiManager service unavailable"));
            }

            if (!_wifiManager.IsWifiEnabled)
            {
                _logger.LogWarning("WiFi is disabled on device");
                return Result.Fail<IReadOnlyList<WifiNetwork>>(
                    Error.Validation("WIFI.DISABLED", "WiFi is disabled on device"));
            }

            // Request WiFi scan
            var scanSuccess = _wifiManager.StartScan();
            if (!scanSuccess)
            {
                _logger.LogWarning("Failed to initiate WiFi scan");
                return Result.Fail<IReadOnlyList<WifiNetwork>>(
                    Error.Validation("WIFI.SCAN_FAILED", "Failed to initiate WiFi scan"));
            }

            // Wait a bit for scan results to be populated
            await Task.Delay(1000, ct);

            var scanResults = _wifiManager.ScanResults;
            if (scanResults == null || scanResults.Count == 0)
            {
                _logger.LogInformation("No WiFi networks found during scan");
                return Result.Ok((IReadOnlyList<WifiNetwork>)new List<WifiNetwork>());
            }

            var networks = new List<WifiNetwork>();
            foreach (var scanResult in scanResults)
            {
                try
                {
                    var network = ConvertScanResultToWifiNetwork(scanResult);
                    if (network != null)
                    {
                        networks.Add(network);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error processing scan result for SSID {SSID}",
                        scanResult?.Ssid ?? "Unknown");
                }
            }

            _logger.LogInformation("WiFi scan completed: {Count} networks found", networks.Count);
            return Result.Ok((IReadOnlyList<WifiNetwork>)networks);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("WiFi scan cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Android WiFi scan encountered an error");
            return Result.Fail<IReadOnlyList<WifiNetwork>>(Error.FromException(ex));
        }
    }

    /// <summary>
    /// Retrieves information about the currently connected WiFi network.
    /// </summary>
    private partial async Task<Result<WifiNetwork?>> GetConnectedNetworkAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var context = GetAndroidContext();
            if (context == null)
            {
                _logger.LogError("Android context unavailable for connected network check");
                return Result.Fail<WifiNetwork?>(
                    Error.NotFound("WIFI.CONTEXT_NOT_FOUND", "Android context unavailable"));
            }

            _context = context;
            _wifiManager = context.GetSystemService(Context.WifiService) as WifiManager;
            _connectivityManager = context.GetSystemService(Context.ConnectivityService) as ConnectivityManager;

            if (_wifiManager == null || _connectivityManager == null)
            {
                _logger.LogWarning("WifiManager or ConnectivityManager unavailable");
                return Result.Ok<WifiNetwork?>(null);
            }

            var connectionInfo = _wifiManager.ConnectionInfo;
            if (connectionInfo == null || connectionInfo.NetworkId == -1)
            {
                _logger.LogInformation("Device is not connected to any WiFi network");
                return Result.Ok<WifiNetwork?>(null);
            }

            // Get SSID (remove quotes if present)
            var ssid = connectionInfo.SSID?.Trim('"') ?? "Unknown";
            var bssid = connectionInfo.MacAddress ?? "00:00:00:00:00:00";

            // Get signal strength in dBm
            var signalStrength = connectionInfo.Rssi;
            if (signalStrength < -100) signalStrength = -100;
            if (signalStrength > -30) signalStrength = -30;

            // Get frequency from WiFi info
            int frequency = 2400; // Default to 2.4 GHz
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
            {
                frequency = connectionInfo.Frequency;
            }

            // Determine security type
            var configuredNetworks = _wifiManager.ConfiguredNetworks;
            var currentConfig = configuredNetworks?.FirstOrDefault(n => n.NetworkId == connectionInfo.NetworkId);
            var securityType = DetermineSecurityType(currentConfig?.AllowedKeyManagement?.Count() ?? 0);
            var isSecure = securityType != "None";

            var wifiNetwork = new WifiNetwork(
                SSID: ssid,
                BSSID: bssid,
                SignalStrength: signalStrength,
                Frequency: frequency,
                IsSecure: isSecure,
                SecurityType: securityType,
                ConnectedAt: DateTime.UtcNow,
                LastSeenAt: DateTime.UtcNow);

            _logger.LogInformation("Retrieved connected network: {SSID}", ssid);
            return Result.Ok<WifiNetwork?>(wifiNetwork);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve connected network");
            return Result.Fail<WifiNetwork?>(
                Error.Access("WIFI.ACCESS_DENIED", "Cannot access WiFi info"));
        }
    }

    /// <summary>
    /// Attempts to connect to a specified WiFi network.
    /// </summary>
    private partial async Task<Result<bool>> ConnectToNetworkAsyncPlatform(
        WifiNetwork network,
        string? password,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var context = GetAndroidContext();
            if (context == null)
            {
                _logger.LogError("Android context unavailable for WiFi connection");
                return Result.Fail<bool>(
                    Error.NotFound("WIFI.CONTEXT_NOT_FOUND", "Android context unavailable"));
            }

            _context = context;
            _wifiManager = context.GetSystemService(Context.WifiService) as WifiManager;
            _connectivityManager = context.GetSystemService(Context.ConnectivityService) as ConnectivityManager;

            if (_wifiManager == null || _connectivityManager == null)
            {
                _logger.LogError("WifiManager or ConnectivityManager unavailable");
                return Result.Fail<bool>(
                    Error.NotFound("WIFI.SERVICE_NOT_FOUND", "Service unavailable"));
            }

            // For Android 10+ (API 29+), use WifiNetworkSpecifier
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Q)
            {
                return await ConnectUsingNetworkSpecifier(network, password, ct);
            }
            else
            {
                // For older Android versions, use WifiConfiguration
                return await ConnectUsingWifiConfiguration(network, password, ct);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("WiFi connection cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to network {SSID}", network.SSID);
            return Result.Fail<bool>(
                Error.Validation("WIFI.CONNECTION_FAILED", "Failed to connect to network"));
        }
    }

    /// <summary>
    /// Disconnects from the currently connected WiFi network.
    /// </summary>
    private partial async Task<Result<bool>> DisconnectAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var context = GetAndroidContext();
            if (context == null)
            {
                _logger.LogWarning("Android context unavailable for WiFi disconnect");
                return Result.Ok(true);
            }

            _context = context;
            _wifiManager = context.GetSystemService(Context.WifiService) as WifiManager;

            if (_wifiManager == null)
            {
                _logger.LogWarning("WifiManager unavailable for disconnect");
                return Result.Ok(true);
            }

            // Try to disconnect from current network
            _wifiManager.Disconnect();
            _logger.LogInformation("Disconnected from WiFi");

            return await Task.FromResult(Result.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect from WiFi");
            return Result.Fail<bool>(
                Error.Unknown("WIFI.DISCONNECT_FAILED", "Failed to disconnect from WiFi"));
        }
    }

    /// <summary>
    /// Checks if WiFi capability is available on the device.
    /// </summary>
    private partial async Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var context = GetAndroidContext();
            if (context == null)
            {
                _logger.LogDebug("Android context unavailable for WiFi availability check");
                return false;
            }

            var packageManager = context.PackageManager;
            if (packageManager == null)
            {
                return false;
            }

            // Check if device has WiFi capability
            var hasWiFi = packageManager.HasSystemFeature(
                Android.Content.PM.PackageManager.FeatureWifi);

            _logger.LogDebug("WiFi availability: {Available}", hasWiFi);
            return await Task.FromResult(hasWiFi);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error checking WiFi availability");
            return false;
        }
    }

    /// <summary>
    /// Starts monitoring for WiFi network changes.
    /// </summary>
    private partial async Task<Result<bool>> StartMonitoringAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var context = GetAndroidContext();
            if (context == null)
            {
                _logger.LogError("Android context unavailable for network monitoring");
                return Result.Fail<bool>(
                    Error.NotFound("WIFI.CONTEXT_NOT_FOUND", "Android context unavailable"));
            }

            _context = context;
            _connectivityManager = context.GetSystemService(Context.ConnectivityService) as ConnectivityManager;

            if (_connectivityManager == null)
            {
                _logger.LogError("ConnectivityManager unavailable");
                return Result.Fail<bool>(
                    Error.NotFound("WIFI.SERVICE_NOT_FOUND", "ConnectivityManager unavailable"));
            }

            // Create and register network callback
            _networkCallback = new WifiNetworkCallback(this);

            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.N)
            {
                _connectivityManager.RegisterNetworkCallback(
                    new NetworkRequest.Builder()
                        .AddTransportType(NetworkCapabilities.TransportWifi)
                        .Build(),
                    _networkCallback);
            }
            else
            {
                // Fallback for older Android versions - register for any network changes
                var intentFilter = new IntentFilter(ConnectivityManager.ConnectivityAction);
                context.RegisterReceiver(_networkCallback as BroadcastReceiver, intentFilter,
                    Android.Content.PM.Protection.Normal);
            }

            _logger.LogInformation("Started monitoring WiFi network changes");
            return await Task.FromResult(Result.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start network monitoring");
            return Result.Fail<bool>(Error.FromException(ex));
        }
    }

    /// <summary>
    /// Stops monitoring for WiFi network changes.
    /// </summary>
    private partial async Task<Result<bool>> StopMonitoringAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var context = GetAndroidContext();
            if (context == null || _connectivityManager == null || _networkCallback == null)
            {
                _logger.LogDebug("No active network monitoring to stop");
                return await Task.FromResult(Result.Ok(true));
            }

            // Unregister network callback
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.N)
            {
                _connectivityManager.UnregisterNetworkCallback(_networkCallback);
            }
            else
            {
                // For older Android versions
                if (_networkCallback is BroadcastReceiver br)
                {
                    context.UnregisterReceiver(br);
                }
            }

            _networkCallback = null;
            _logger.LogInformation("Stopped monitoring WiFi network changes");
            return await Task.FromResult(Result.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop network monitoring");
            return Result.Fail<bool>(Error.FromException(ex));
        }
    }

    /// <summary>
    /// Helper method to get Android context from MAUI application.
    /// </summary>
    private static Context? GetAndroidContext()
    {
        try
        {
            // Try to get context from MAUI application
            var context = Microsoft.Maui.Controls.Application.Current?.MainPage?.Handler?.MauiContext?.Context;
            if (context != null)
            {
                return context;
            }

            // Fallback to Android Application context
            return Android.App.Application.Context;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting Android context: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Converts an Android ScanResult to a WifiNetwork record.
    /// </summary>
    private WifiNetwork? ConvertScanResultToWifiNetwork(Android.Net.Wifi.ScanResult scanResult)
    {
        try
        {
            if (scanResult == null || string.IsNullOrEmpty(scanResult.Ssid))
            {
                return null;
            }

            var ssid = scanResult.Ssid;
            var bssid = scanResult.Bssid ?? "00:00:00:00:00:00";
            var signalStrength = scanResult.Level;
            var frequency = scanResult.Frequency;

            // Determine security type from capabilities
            var securityType = DetermineSecurityTypeFromCapabilities(scanResult.Capabilities);
            var isSecure = securityType != "None";

            var network = new WifiNetwork(
                SSID: ssid,
                BSSID: bssid,
                SignalStrength: signalStrength,
                Frequency: frequency,
                IsSecure: isSecure,
                SecurityType: securityType,
                LastSeenAt: DateTime.UtcNow);

            return network;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error converting scan result to WifiNetwork");
            return null;
        }
    }

    /// <summary>
    /// Determines security type from WiFi capabilities string.
    /// </summary>
    private string DetermineSecurityTypeFromCapabilities(string? capabilities)
    {
        if (string.IsNullOrEmpty(capabilities))
        {
            return "None";
        }

        var caps = capabilities.ToUpperInvariant();

        // Check for WPA3
        if (caps.Contains("WPA3"))
        {
            return "WPA3";
        }

        // Check for WPA2
        if (caps.Contains("WPA2") || caps.Contains("RSN"))
        {
            return "WPA2";
        }

        // Check for WPA
        if (caps.Contains("WPA"))
        {
            return "WPA";
        }

        // Check for WEP
        if (caps.Contains("WEP"))
        {
            return "WEP";
        }

        return "None";
    }

    /// <summary>
    /// Determines security type from network configuration key management settings.
    /// </summary>
    private string DetermineSecurityType(int keyMgmtCount)
    {
        return keyMgmtCount > 0 ? "WPA2" : "None";
    }

    /// <summary>
    /// Connects to network using WifiNetworkSpecifier (Android 10+).
    /// </summary>
    private async Task<Result<bool>> ConnectUsingNetworkSpecifier(
        WifiNetwork network,
        string? password,
        CancellationToken ct)
    {
        try
        {
            if (_connectivityManager == null)
            {
                return Result.Fail<bool>(
                    Error.NotFound("WIFI.SERVICE_NOT_FOUND", "ConnectivityManager unavailable"));
            }

            var specifierBuilder = new WifiNetworkSpecifier.Builder()
                .SetSsid(network.SSID);

            // Set password if network is secure
            if (network.IsSecure && !string.IsNullOrEmpty(password))
            {
                specifierBuilder.SetWpa2Passphrase(password);
            }

            var specifier = specifierBuilder.Build();
            var request = new NetworkRequest.Builder()
                .AddTransport(NetworkCapabilities.TransportWifi)
                .SetNetworkSpecifier(specifier)
                .Build();

            // Register callback for connection
            var callback = new WifiConnectionCallback();
            _connectivityManager.RequestNetwork(request, callback,
                new Handler(Android.OS.Looper.MainLooper ?? Android.OS.Looper.GetMainLooper()),
                ConnectionTimeoutSeconds * 1000);

            // Wait for connection with timeout
            var connected = await callback.WaitForConnectionAsync(
                TimeSpan.FromSeconds(ConnectionTimeoutSeconds), ct);

            if (!connected)
            {
                _logger.LogWarning("WiFi connection timeout for network {SSID}", network.SSID);
                return Result.Fail<bool>(
                    Error.Timeout("WIFI.CONNECTION_FAILED", "Connection timeout"));
            }

            _logger.LogInformation("Successfully connected to network {SSID}", network.SSID);
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect using WifiNetworkSpecifier");
            return Result.Fail<bool>(Error.FromException(ex));
        }
    }

    /// <summary>
    /// Connects to network using WifiConfiguration (Android API 29 and below).
    /// </summary>
    private async Task<Result<bool>> ConnectUsingWifiConfiguration(
        WifiNetwork network,
        string? password,
        CancellationToken ct)
    {
        try
        {
            if (_wifiManager == null)
            {
                return Result.Fail<bool>(
                    Error.NotFound("WIFI.SERVICE_NOT_FOUND", "WifiManager unavailable"));
            }

            var config = new WifiConfiguration();
            config.Ssid = $"\"{network.SSID}\"";
            config.Bssid = network.BSSID;

            if (network.IsSecure && !string.IsNullOrEmpty(password))
            {
                config.PreSharedKey = $"\"{password}\"";
                config.AllowedKeyManagement?.Clear();
                config.AllowedKeyManagement?.Set((int)KeyMgmtMask.Wpa2Psk);
            }
            else
            {
                config.AllowedKeyManagement?.Clear();
                config.AllowedKeyManagement?.Set((int)KeyMgmtMask.None);
            }

            // Remove existing configuration if present
            var existingNetworkId = FindNetworkId(network.SSID);
            if (existingNetworkId >= 0)
            {
                _wifiManager.RemoveNetwork(existingNetworkId);
            }

            // Add and enable the network
            var networkId = _wifiManager.AddNetwork(config);
            if (networkId < 0)
            {
                _logger.LogError("Failed to add WiFi network {SSID}", network.SSID);
                return Result.Fail<bool>(
                    Error.Validation("WIFI.CONNECTION_FAILED", "Failed to add network configuration"));
            }

            _wifiManager.EnableNetwork(networkId, true);
            _wifiManager.Reconnect();

            // Wait for connection
            var connectionTask = WaitForConnectionAsync(network.SSID,
                TimeSpan.FromSeconds(ConnectionTimeoutSeconds), ct);
            var connected = await connectionTask;

            if (!connected)
            {
                _logger.LogWarning("WiFi connection timeout for network {SSID}", network.SSID);
                _wifiManager.DisableNetwork(networkId);
                return Result.Fail<bool>(
                    Error.Timeout("WIFI.CONNECTION_FAILED", "Connection timeout"));
            }

            _logger.LogInformation("Successfully connected to network {SSID}", network.SSID);
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect using WifiConfiguration");
            return Result.Fail<bool>(Error.FromException(ex));
        }
    }

    /// <summary>
    /// Finds the network ID for a given SSID.
    /// </summary>
    private int FindNetworkId(string ssid)
    {
        try
        {
            if (_wifiManager == null)
            {
                return -1;
            }

            var configuredNetworks = _wifiManager.ConfiguredNetworks;
            if (configuredNetworks == null)
            {
                return -1;
            }

            foreach (var config in configuredNetworks)
            {
                if (config?.Ssid?.Trim('"') == ssid)
                {
                    return config.NetworkId;
                }
            }

            return -1;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error finding network ID for SSID {SSID}", ssid);
            return -1;
        }
    }

    /// <summary>
    /// Waits for WiFi connection to a specific network.
    /// </summary>
    private async Task<bool> WaitForConnectionAsync(string ssid, TimeSpan timeout, CancellationToken ct)
    {
        var startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < timeout)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var connectionInfo = _wifiManager?.ConnectionInfo;
                if (connectionInfo != null)
                {
                    var currentSsid = connectionInfo.SSID?.Trim('"');
                    if (currentSsid == ssid && connectionInfo.NetworkId >= 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error checking connection status");
            }

            await Task.Delay(500, ct);
        }

        return false;
    }
}

/// <summary>
/// Network callback for monitoring WiFi connection changes on Android 7+.
/// </summary>
public class WifiNetworkCallback : ConnectivityManager.NetworkCallback
{
    private readonly WifiService _service;

    public WifiNetworkCallback(WifiService service)
    {
        _service = service;
    }

    public override void OnAvailable(Network network)
    {
        base.OnAvailable(network);
        _service.RaiseNetworkChangeEvent(
            new WifiNetworkChangeEvent(EventType: 1, DetectedAt: DateTime.UtcNow));
    }

    public override void OnLost(Network network)
    {
        base.OnLost(network);
        _service.RaiseNetworkChangeEvent(
            new WifiNetworkChangeEvent(EventType: 0, DetectedAt: DateTime.UtcNow));
    }

    public override void OnCapabilitiesChanged(Network network, NetworkCapabilities capabilities)
    {
        base.OnCapabilitiesChanged(network, capabilities);
        _service.RaiseNetworkChangeEvent(
            new WifiNetworkChangeEvent(EventType: 2, DetectedAt: DateTime.UtcNow));
    }
}

/// <summary>
/// Callback for WiFi network connection on Android 10+.
/// </summary>
public class WifiConnectionCallback : ConnectivityManager.NetworkCallback
{
    private TaskCompletionSource<bool> _completionSource = new();

    public override void OnAvailable(Network network)
    {
        base.OnAvailable(network);
        if (!_completionSource.Task.IsCompleted)
        {
            _completionSource.SetResult(true);
        }
    }

    public override void OnUnavailable()
    {
        base.OnUnavailable();
        if (!_completionSource.Task.IsCompleted)
        {
            _completionSource.SetResult(false);
        }
    }

    public async Task<bool> WaitForConnectionAsync(TimeSpan timeout, CancellationToken ct)
    {
        try
        {
            var task = await Task.WhenAny(
                _completionSource.Task,
                Task.Delay(timeout, ct));

            return _completionSource.Task.IsCompleted && _completionSource.Task.Result;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }
}
#endif
