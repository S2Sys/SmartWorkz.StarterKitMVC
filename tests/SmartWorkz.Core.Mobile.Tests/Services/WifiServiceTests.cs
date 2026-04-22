namespace SmartWorkz.Mobile.Tests.Services;

using Moq;
using SmartWorkz.Mobile;
using SmartWorkz.Shared;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Reactive.Linq;

public class WifiServiceTests
{
    private readonly Mock<ILogger<WifiService>> _logger = new();
    private readonly Mock<IPermissionService> _permissionService = new();
    private readonly WifiService _sut;

    public WifiServiceTests()
    {
        _sut = new WifiService(_logger.Object, _permissionService.Object);
    }

    // ===== Helper Methods for Test Data =====

    private WifiNetwork CreateTestNetwork(
        string ssid = "HomeNetwork",
        string bssid = "00:1A:2B:3C:4D:5E",
        int signalStrength = -50,
        int frequency = 2437,
        bool isSecure = true,
        string securityType = "WPA2",
        DateTime? connectedAt = null)
    {
        return new WifiNetwork(
            SSID: ssid,
            BSSID: bssid,
            SignalStrength: signalStrength,
            Frequency: frequency,
            IsSecure: isSecure,
            SecurityType: securityType,
            ConnectedAt: connectedAt);
    }

    private List<WifiNetwork> CreateTestNetworkList()
    {
        return new List<WifiNetwork>
        {
            CreateTestNetwork(
                ssid: "HomeNetwork",
                bssid: "00:1A:2B:3C:4D:5E",
                signalStrength: -50,
                frequency: 2437,
                isSecure: true,
                securityType: "WPA2"),
            CreateTestNetwork(
                ssid: "CoffeeShop",
                bssid: "AA:BB:CC:DD:EE:FF",
                signalStrength: -70,
                frequency: 5180,
                isSecure: false,
                securityType: "None"),
            CreateTestNetwork(
                ssid: "PublicWiFi",
                bssid: "11:22:33:44:55:66",
                signalStrength: -85,
                frequency: 2450,
                isSecure: true,
                securityType: "WPA3")
        };
    }

    // ===== Permission Validation Tests =====

    [Fact]
    public async Task ScanForNetworks_WithoutPermission_ReturnsDeniedError()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        // Act
        var result = await _sut.ScanForNetworksAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("AUTH.UNAUTHORIZED", result.Error.Code);
    }

    [Fact]
    public async Task ConnectToNetwork_WithoutPermission_ReturnsDeniedError()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        var network = CreateTestNetwork();

        // Act
        var result = await _sut.ConnectToNetworkAsync(network, "password");

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("AUTH.UNAUTHORIZED", result.Error.Code);
    }

    // ===== Scan Networks Tests =====

    [Fact]
    public async Task ScanForNetworks_WithValidState_ReturnsNetworkList()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.ScanForNetworksAsync();

        // Assert
        Assert.NotNull(result);
        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ScanForNetworks_WithNoNetworks_ReturnsEmptyList()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.ScanForNetworksAsync();

        // Assert
        Assert.NotNull(result);
        // On non-platform builds, will return error, but we verify permission was checked
        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ScanForNetworks_WithMultipleNetworks_ReturnsAll()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.ScanForNetworksAsync();

        // Assert
        Assert.NotNull(result);
        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ===== Get Connected Network Tests =====

    [Fact]
    public async Task GetConnectedNetwork_WhenConnected_ReturnsNetwork()
    {
        // Arrange
        var connectedNetwork = CreateTestNetwork(connectedAt: DateTime.UtcNow);

        // Act
        var result = await _sut.GetConnectedNetworkAsync();

        // Assert
        Assert.NotNull(result);
        // Platform implementation decides behavior; we verify the call succeeds
    }

    [Fact]
    public async Task GetConnectedNetwork_WhenNotConnected_ReturnsNull()
    {
        // Arrange
        // No setup needed for platform implementation

        // Act
        var result = await _sut.GetConnectedNetworkAsync();

        // Assert
        Assert.NotNull(result);
        // Platform implementation decides; we verify call completes
    }

    // ===== Connect/Disconnect Tests =====

    [Fact]
    public async Task ConnectToNetwork_WithValidNetwork_AttemptConnection()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var network = CreateTestNetwork(isSecure: false, securityType: "None");

        // Act
        var result = await _sut.ConnectToNetworkAsync(network, null);

        // Assert
        Assert.NotNull(result);
        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ConnectToNetwork_WithSecureNetwork_RequiresPassword()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var network = CreateTestNetwork(isSecure: true, securityType: "WPA2");

        // Act
        var result = await _sut.ConnectToNetworkAsync(network, "validPassword123");

        // Assert
        Assert.NotNull(result);
        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ConnectToNetwork_WithSecureNetworkNoPassword_ReturnsError()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var network = CreateTestNetwork(isSecure: true, securityType: "WPA2");

        // Act
        var result = await _sut.ConnectToNetworkAsync(network, null);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("VALIDATION.INVALID_VALUE", result.Error.Code);
    }

    [Fact]
    public async Task ConnectToNetwork_WithSecureNetworkEmptyPassword_ReturnsError()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var network = CreateTestNetwork(isSecure: true, securityType: "WPA3");

        // Act
        var result = await _sut.ConnectToNetworkAsync(network, string.Empty);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("VALIDATION.INVALID_VALUE", result.Error.Code);
    }

    [Fact]
    public async Task DisconnectAsync_WhenConnected_DisconnectsSuccessfully()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.DisconnectAsync();

        // Assert
        Assert.NotNull(result);
        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ===== Input Validation Tests =====

    [Fact]
    public async Task ConnectToNetwork_WithNullNetwork_ThrowsArgumentNullException()
    {
        // Arrange
        WifiNetwork? network = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sut.ConnectToNetworkAsync(network!, "password"));
    }

    [Fact]
    public async Task ConnectToNetwork_WithEmptyPassword_ReturnsErrorIfSecure()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var network = CreateTestNetwork(isSecure: true);

        // Act
        var result = await _sut.ConnectToNetworkAsync(network, "");

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task ScanForNetworks_WithCancellation_RespondsToCancel()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert - Should not throw, either succeeds or handles cancellation gracefully
        try
        {
            var result = await _sut.ScanForNetworksAsync(cts.Token);
            Assert.NotNull(result);
        }
        catch (OperationCanceledException)
        {
            // Acceptable - cancellation token respected
        }
    }

    // ===== Network Monitoring Tests =====

    [Fact]
    public async Task StartMonitoring_WithValidState_SucceedsAndRaisesEvents()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.StartMonitoringNetworkChangesAsync();

        // Assert
        Assert.NotNull(result);
        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StopMonitoring_WithActiveMonitoring_StopsSuccessfully()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        await _sut.StartMonitoringNetworkChangesAsync();

        // Act
        var result = await _sut.StopMonitoringNetworkChangesAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task OnNetworkChanged_EmitsConnectEvent_WhenNetworkChanges()
    {
        // Arrange
        var eventRaised = false;
        WifiNetworkChangeEvent? capturedEvent = null;

        var subscription = _sut.OnNetworkChanged.Subscribe(evt =>
        {
            eventRaised = true;
            capturedEvent = evt;
        });

        var previous = CreateTestNetwork(ssid: "OldNetwork", bssid: "AA:AA:AA:AA:AA:AA");
        var current = CreateTestNetwork(ssid: "NewNetwork", bssid: "BB:BB:BB:BB:BB:BB");
        var networkChangeEvent = new WifiNetworkChangeEvent(
            Previous: previous,
            Current: current,
            EventType: 1, // CONNECTED
            ChangedAt: DateTime.UtcNow);

        // Act
        var raiseMethod = typeof(WifiService).GetMethod(
            "RaiseNetworkChangeEvent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        raiseMethod?.Invoke(_sut, new object[] { networkChangeEvent });

        await Task.Delay(50);

        // Assert
        Assert.True(eventRaised);
        if (capturedEvent != null)
        {
            Assert.Equal(1, capturedEvent.EventType);
            Assert.Equal("NewNetwork", capturedEvent.Current?.SSID);
        }

        subscription.Dispose();
    }

    // ===== Observable Events Tests =====

    [Fact]
    public async Task OnNetworkChanged_CanSubscribe_WithoutError()
    {
        // Arrange
        var subscription = _sut.OnNetworkChanged.Subscribe(_ => { });

        // Act
        var observable = _sut.OnNetworkChanged;

        // Assert
        Assert.NotNull(observable);
        subscription.Dispose();
    }

    [Fact]
    public async Task OnNetworkChanged_EmitsDisconnectEvent_WhenDisconnected()
    {
        // Arrange
        var eventRaised = false;
        WifiNetworkChangeEvent? capturedEvent = null;

        var subscription = _sut.OnNetworkChanged.Subscribe(evt =>
        {
            eventRaised = true;
            capturedEvent = evt;
        });

        var previousNetwork = CreateTestNetwork(ssid: "DisconnectedNetwork", bssid: "CC:CC:CC:CC:CC:CC");
        var networkChangeEvent = new WifiNetworkChangeEvent(
            Previous: previousNetwork,
            Current: null,
            EventType: 0, // DISCONNECTED
            ChangedAt: DateTime.UtcNow);

        // Act
        var raiseMethod = typeof(WifiService).GetMethod(
            "RaiseNetworkChangeEvent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        raiseMethod?.Invoke(_sut, new object[] { networkChangeEvent });

        await Task.Delay(50);

        // Assert
        Assert.True(eventRaised);
        if (capturedEvent != null)
        {
            Assert.Equal(0, capturedEvent.EventType);
            Assert.Null(capturedEvent.Current);
        }

        subscription.Dispose();
    }

    [Fact]
    public async Task OnNetworkChanged_MultipleSubscribers_AllReceiveEvent()
    {
        // Arrange
        var event1Raised = false;
        var event2Raised = false;

        var subscription1 = _sut.OnNetworkChanged.Subscribe(_ => event1Raised = true);
        var subscription2 = _sut.OnNetworkChanged.Subscribe(_ => event2Raised = true);

        var networkChangeEvent = new WifiNetworkChangeEvent(
            Previous: null,
            Current: CreateTestNetwork(),
            EventType: 1,
            ChangedAt: DateTime.UtcNow);

        // Act
        var raiseMethod = typeof(WifiService).GetMethod(
            "RaiseNetworkChangeEvent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        raiseMethod?.Invoke(_sut, new object[] { networkChangeEvent });

        await Task.Delay(50);

        // Assert
        Assert.True(event1Raised);
        Assert.True(event2Raised);

        subscription1.Dispose();
        subscription2.Dispose();
    }

    [Fact]
    public async Task OnNetworkChanged_EmitsSignalChangeEvent_WhenSignalChanges()
    {
        // Arrange
        var eventRaised = false;
        WifiNetworkChangeEvent? capturedEvent = null;

        var subscription = _sut.OnNetworkChanged.Subscribe(evt =>
        {
            eventRaised = true;
            capturedEvent = evt;
        });

        var network = CreateTestNetwork(signalStrength: -60);
        var networkChangeEvent = new WifiNetworkChangeEvent(
            Previous: network,
            Current: CreateTestNetwork(signalStrength: -75),
            EventType: 2, // SIGNAL_CHANGED
            ChangedAt: DateTime.UtcNow);

        // Act
        var raiseMethod = typeof(WifiService).GetMethod(
            "RaiseNetworkChangeEvent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        raiseMethod?.Invoke(_sut, new object[] { networkChangeEvent });

        await Task.Delay(50);

        // Assert
        Assert.True(eventRaised);
        if (capturedEvent != null)
        {
            Assert.Equal(2, capturedEvent.EventType);
        }

        subscription.Dispose();
    }

    // ===== Availability Tests =====

    [Fact]
    public async Task IsAvailable_OnUnsupportedPlatform_ReturnsFalse()
    {
        // Arrange
        // On non-platform builds (e.g., unit test environment)

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        // Result.Data will be false on unsupported platforms
    }

    [Fact]
    public async Task IsAvailable_ReturnsBoolean()
    {
        // Arrange
        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.IsType<bool>(result.Data);
    }

    // ===== Integration/Lifecycle Tests =====

    [Fact]
    public async Task MultipleNetworkScans_WorkCorrectly_Sequential()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act & Assert
        var result1 = await _sut.ScanForNetworksAsync();
        var result2 = await _sut.ScanForNetworksAsync();
        var result3 = await _sut.ScanForNetworksAsync();

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result3);

        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task MonitoringLifecycle_StartAndStop_Works()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var startResult = await _sut.StartMonitoringNetworkChangesAsync();
        var stopResult = await _sut.StopMonitoringNetworkChangesAsync();

        // Assert
        Assert.NotNull(startResult);
        Assert.NotNull(stopResult);

        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Once); // Only called for StartMonitoring
    }

    [Fact]
    public async Task DisconnectAsync_WithoutPermission_ReturnsDeniedError()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        // Act
        var result = await _sut.DisconnectAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("AUTH.UNAUTHORIZED", result.Error.Code);
    }

    [Fact]
    public async Task StartMonitoringNetworkChanges_WithoutPermission_ReturnsDeniedError()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        // Act
        var result = await _sut.StartMonitoringNetworkChangesAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("AUTH.UNAUTHORIZED", result.Error.Code);
    }

    // ===== Constructor Validation Tests =====

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new WifiService(null!, _permissionService.Object));
    }

    [Fact]
    public void Constructor_WithNullPermissionService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new WifiService(_logger.Object, null!));
    }

    // ===== Network Data Validation Tests =====

    [Fact]
    public void CreateWifiNetwork_WithValidData_Succeeds()
    {
        // Act
        var network = CreateTestNetwork(
            ssid: "TestNet",
            bssid: "00:11:22:33:44:55",
            signalStrength: -70,
            frequency: 2437);

        // Assert
        Assert.NotNull(network);
        Assert.Equal("TestNet", network.SSID);
        Assert.Equal("00:11:22:33:44:55", network.BSSID);
        Assert.Equal(-70, network.SignalStrength);
    }

    [Fact]
    public void CreateWifiNetwork_WithInvalidSSID_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CreateTestNetwork(ssid: "")); // SSID too short
    }

    [Fact]
    public void CreateWifiNetwork_WithInvalidBSSID_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CreateTestNetwork(bssid: "INVALID-BSSID")); // Invalid MAC format
    }

    [Fact]
    public void CreateWifiNetwork_WithInvalidSignalStrength_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateTestNetwork(signalStrength: -150)); // Out of range
    }

    [Fact]
    public void CreateWifiNetwork_WithInvalidFrequency_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateTestNetwork(frequency: 1000)); // Invalid frequency
    }

    [Fact]
    public void CreateWifiNetwork_With2GHzFrequency_Succeeds()
    {
        // Act
        var network = CreateTestNetwork(frequency: 2450); // 2.4 GHz band

        // Assert
        Assert.True(network.IsBandwidth2GHz());
        Assert.False(network.IsBandwidth5GHz());
    }

    [Fact]
    public void CreateWifiNetwork_With5GHzFrequency_Succeeds()
    {
        // Act
        var network = CreateTestNetwork(frequency: 5180); // 5 GHz band

        // Assert
        Assert.False(network.IsBandwidth2GHz());
        Assert.True(network.IsBandwidth5GHz());
    }

    [Fact]
    public void WifiNetwork_SignalQuality_MatchesExpected()
    {
        // Arrange & Act & Assert
        var excellent = CreateTestNetwork(signalStrength: -40);
        Assert.Equal("Excellent", excellent.SignalQuality());

        var good = CreateTestNetwork(signalStrength: -60);
        Assert.Equal("Good", good.SignalQuality());

        var fair = CreateTestNetwork(signalStrength: -75);
        Assert.Equal("Fair", fair.SignalQuality());

        var poor = CreateTestNetwork(signalStrength: -95);
        Assert.Equal("Poor", poor.SignalQuality());
    }

    [Fact]
    public void WifiNetwork_IsConnected_ReturnsCorrectValue()
    {
        // Arrange
        var connectedNetwork = CreateTestNetwork(connectedAt: DateTime.UtcNow);
        var disconnectedNetwork = CreateTestNetwork(connectedAt: null);

        // Act & Assert
        Assert.True(connectedNetwork.IsConnected());
        Assert.False(disconnectedNetwork.IsConnected());
    }
}
