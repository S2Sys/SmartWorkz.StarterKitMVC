namespace SmartWorkz.Mobile;

using Microsoft.Extensions.Logging;
using SmartWorkz.Shared;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Cross-platform WiFi service for discovering, monitoring, and managing network connections.
/// Delegates platform-specific implementation to partial methods.
/// </summary>
public partial class WifiService : IWifiService
{
    private readonly ILogger<WifiService> _logger;
    private readonly IPermissionService _permissionService;
    private readonly Subject<WifiNetworkChangeEvent> _networkChangeSubject = new();
    private WifiNetwork? _currentNetwork;

    /// <summary>
    /// Initializes a new instance of the WifiService class.
    /// </summary>
    /// <param name="logger">The logger instance for this service.</param>
    /// <param name="permissionService">The permission service for checking WiFi-related permissions.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or permissionService is null.</exception>
    public WifiService(
        ILogger<WifiService> logger,
        IPermissionService permissionService)
    {
        Guard.NotNull(logger, nameof(logger));
        Guard.NotNull(permissionService, nameof(permissionService));

        _logger = logger;
        _permissionService = permissionService;
    }

    /// <summary>
    /// Gets an observable stream of WiFi network change events.
    /// </summary>
    public IObservable<WifiNetworkChangeEvent> OnNetworkChanged => _networkChangeSubject.AsObservable();

    /// <summary>
    /// Scans for all available WiFi networks in range of the device.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing a read-only list of discovered WifiNetwork objects.
    /// Returns an error if WiFi permissions are not granted.
    /// </returns>
    public async Task<Result<IReadOnlyList<WifiNetwork>>> ScanForNetworksAsync(CancellationToken ct = default)
    {
        var permissionStatus = await _permissionService.CheckAsync(MobilePermission.Location, ct);
        if (permissionStatus != PermissionStatus.Granted)
        {
            _logger.LogWarning("WiFi permission denied for network scan");
            return Result.Fail<IReadOnlyList<WifiNetwork>>(Error.Unauthorized("WiFi permission required"));
        }

        _logger.LogInformation("WiFi scan started");
        var result = await ScanForNetworksAsyncPlatform(ct);
        if (result.Succeeded)
        {
            _logger.LogInformation("WiFi scan completed: {Count} networks found", result.Value?.Count ?? 0);
        }
        else
        {
            _logger.LogWarning("WiFi scan failed: {Error}", result.Error?.Message);
        }

        return result;
    }

    /// <summary>
    /// Retrieves information about the currently connected WiFi network.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing the WifiNetwork object if connected, or null if not currently connected.
    /// </returns>
    public async Task<Result<WifiNetwork?>> GetConnectedNetworkAsync(CancellationToken ct = default)
    {
        var result = await GetConnectedNetworkAsyncPlatform(ct);
        if (result.Succeeded)
        {
            _currentNetwork = result.Value;
            if (result.Value != null)
            {
                _logger.LogInformation("Retrieved connected network: {SSID}", result.Value.SSID);
            }
            else
            {
                _logger.LogInformation("No connected network");
            }
        }
        else
        {
            _logger.LogWarning("Failed to retrieve connected network: {Error}", result.Error?.Message);
        }

        return result;
    }

    /// <summary>
    /// Attempts to connect to a specified WiFi network.
    /// </summary>
    /// <param name="network">The WifiNetwork object representing the network to connect to.</param>
    /// <param name="password">The password for the network. Required if network.IsSecure is true.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing true if the connection was successful; false otherwise.
    /// </returns>
    public async Task<Result<bool>> ConnectToNetworkAsync(WifiNetwork network, string? password, CancellationToken ct = default)
    {
        Guard.NotNull(network, nameof(network));

        var permissionStatus = await _permissionService.CheckAsync(MobilePermission.Location, ct);
        if (permissionStatus != PermissionStatus.Granted)
        {
            _logger.LogWarning("WiFi permission denied for network connection");
            return Result.Fail<bool>(Error.Unauthorized("WiFi permission required"));
        }

        if (network.IsSecure && string.IsNullOrEmpty(password))
        {
            _logger.LogWarning("Attempt to connect to secure network without password: {SSID}", network.SSID);
            return Result.Fail<bool>(Error.Validation("WIFI.INVALID_PASSWORD",
                "Password is required for secure networks"));
        }

        _logger.LogInformation("Connecting to network: {SSID}", network.SSID);
        var result = await ConnectToNetworkAsyncPlatform(network, password, ct);
        if (result.Succeeded)
        {
            _currentNetwork = network;
            _logger.LogInformation("Successfully connected to network: {SSID}", network.SSID);
        }
        else
        {
            _logger.LogWarning("Failed to connect to network {SSID}: {Error}", network.SSID, result.Error?.Message);
        }

        return result;
    }

    /// <summary>
    /// Disconnects from the currently connected WiFi network.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing true if disconnection was successful; false otherwise.
    /// </returns>
    public async Task<Result<bool>> DisconnectAsync(CancellationToken ct = default)
    {
        var permissionStatus = await _permissionService.CheckAsync(MobilePermission.Location, ct);
        if (permissionStatus != PermissionStatus.Granted)
        {
            _logger.LogWarning("WiFi permission denied for disconnect operation");
            return Result.Fail<bool>(Error.Unauthorized("WiFi permission required"));
        }

        var result = await DisconnectAsyncPlatform(ct);
        if (result.Succeeded)
        {
            _currentNetwork = null;
            _logger.LogInformation("Disconnected from WiFi");
        }
        else
        {
            _logger.LogWarning("Failed to disconnect from WiFi: {Error}", result.Error?.Message);
        }

        return result;
    }

    /// <summary>
    /// Checks whether WiFi connectivity capability is available on the current device.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing true if the device supports WiFi connectivity; false otherwise.
    /// </returns>
    public async Task<Result<bool>> IsAvailableAsync(CancellationToken ct = default)
    {
        var isAvailable = await IsAvailableAsyncPlatform(ct);
        return Result.Ok(isAvailable);
    }

    /// <summary>
    /// Starts monitoring the device for WiFi network changes.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing true if monitoring started successfully; false otherwise.
    /// </returns>
    public async Task<Result<bool>> StartMonitoringNetworkChangesAsync(CancellationToken ct = default)
    {
        var permissionStatus = await _permissionService.CheckAsync(MobilePermission.Location, ct);
        if (permissionStatus != PermissionStatus.Granted)
        {
            _logger.LogWarning("WiFi permission denied for network monitoring");
            return Result.Fail<bool>(Error.Unauthorized("WiFi permission required"));
        }

        var result = await StartMonitoringAsyncPlatform(ct);
        if (result.Succeeded)
        {
            _logger.LogInformation("Started monitoring network changes");
        }
        else
        {
            _logger.LogWarning("Failed to start network monitoring: {Error}", result.Error?.Message);
        }

        return result;
    }

    /// <summary>
    /// Stops monitoring for WiFi network changes.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result containing true if monitoring was stopped successfully; false otherwise.
    /// </returns>
    public async Task<Result<bool>> StopMonitoringNetworkChangesAsync(CancellationToken ct = default)
    {
        var result = await StopMonitoringAsyncPlatform(ct);
        if (result.Succeeded)
        {
            _logger.LogInformation("Stopped monitoring network changes");
        }
        else
        {
            _logger.LogWarning("Failed to stop network monitoring: {Error}", result.Error?.Message);
        }

        return result;
    }

    /// <summary>
    /// Raises a WiFi network change event (called from platform implementations).
    /// </summary>
    /// <param name="networkChangeEvent">The network change event to raise.</param>
    protected void RaiseNetworkChangeEvent(WifiNetworkChangeEvent networkChangeEvent)
    {
        _networkChangeSubject.OnNext(networkChangeEvent);
        _logger.LogInformation("Network change event: {EventType}",
            networkChangeEvent.EventType switch
            {
                0 => "DISCONNECTED",
                1 => "CONNECTED",
                2 => "SIGNAL_CHANGED",
                _ => "UNKNOWN"
            });
    }

#if __ANDROID__ || __IOS__ || __MACCATALYST__
    /// <summary>
    /// Platform-specific network scan. Implemented per platform.
    /// </summary>
    private partial Task<Result<IReadOnlyList<WifiNetwork>>> ScanForNetworksAsyncPlatform(CancellationToken ct);

    /// <summary>
    /// Platform-specific get connected network. Implemented per platform.
    /// </summary>
    private partial Task<Result<WifiNetwork?>> GetConnectedNetworkAsyncPlatform(CancellationToken ct);

    /// <summary>
    /// Platform-specific network connection. Implemented per platform.
    /// </summary>
    private partial Task<Result<bool>> ConnectToNetworkAsyncPlatform(WifiNetwork network, string? password, CancellationToken ct);

    /// <summary>
    /// Platform-specific network disconnection. Implemented per platform.
    /// </summary>
    private partial Task<Result<bool>> DisconnectAsyncPlatform(CancellationToken ct);

    /// <summary>
    /// Platform-specific availability check. Implemented per platform.
    /// </summary>
    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct);

    /// <summary>
    /// Platform-specific start monitoring. Implemented per platform.
    /// </summary>
    private partial Task<Result<bool>> StartMonitoringAsyncPlatform(CancellationToken ct);

    /// <summary>
    /// Platform-specific stop monitoring. Implemented per platform.
    /// </summary>
    private partial Task<Result<bool>> StopMonitoringAsyncPlatform(CancellationToken ct);
#else
    /// <summary>
    /// Platform-specific network scan. Default implementation for unsupported platforms.
    /// </summary>
    private Task<Result<IReadOnlyList<WifiNetwork>>> ScanForNetworksAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(Result.Fail<IReadOnlyList<WifiNetwork>>(new Error("WIFI.NOT_SUPPORTED",
            "WiFi service is not supported on this platform")));

    /// <summary>
    /// Platform-specific get connected network. Default implementation for unsupported platforms.
    /// </summary>
    private Task<Result<WifiNetwork?>> GetConnectedNetworkAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(Result.Fail<WifiNetwork?>(new Error("WIFI.NOT_SUPPORTED",
            "WiFi service is not supported on this platform")));

    /// <summary>
    /// Platform-specific network connection. Default implementation for unsupported platforms.
    /// </summary>
    private Task<Result<bool>> ConnectToNetworkAsyncPlatform(WifiNetwork network, string? password, CancellationToken ct) =>
        Task.FromResult(Result.Fail<bool>(new Error("WIFI.NOT_SUPPORTED",
            "WiFi service is not supported on this platform")));

    /// <summary>
    /// Platform-specific network disconnection. Default implementation for unsupported platforms.
    /// </summary>
    private Task<Result<bool>> DisconnectAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(Result.Fail<bool>(new Error("WIFI.NOT_SUPPORTED",
            "WiFi service is not supported on this platform")));

    /// <summary>
    /// Platform-specific availability check. Default implementation for unsupported platforms.
    /// </summary>
    private Task<bool> IsAvailableAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(false);

    /// <summary>
    /// Platform-specific start monitoring. Default implementation for unsupported platforms.
    /// </summary>
    private Task<Result<bool>> StartMonitoringAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(Result.Fail<bool>(new Error("WIFI.NOT_SUPPORTED",
            "WiFi service is not supported on this platform")));

    /// <summary>
    /// Platform-specific stop monitoring. Default implementation for unsupported platforms.
    /// </summary>
    private Task<Result<bool>> StopMonitoringAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(Result.Fail<bool>(new Error("WIFI.NOT_SUPPORTED",
            "WiFi service is not supported on this platform")));
#endif
}
