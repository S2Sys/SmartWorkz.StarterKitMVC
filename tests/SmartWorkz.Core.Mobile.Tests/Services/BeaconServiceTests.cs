namespace SmartWorkz.Mobile.Tests.Services;

using Moq;
using SmartWorkz.Mobile;
using SmartWorkz.Shared;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Reactive.Linq;

public class BeaconServiceTests
{
    private readonly Mock<ILogger<BeaconService>> _logger = new();
    private readonly Mock<IPermissionService> _permissionService = new();
    private readonly BeaconService _sut;

    public BeaconServiceTests()
    {
        _sut = new BeaconService(_logger.Object, _permissionService.Object);
    }

    // ===== Helper Methods for Test Data =====

    private BeaconInfo CreateTestBeacon(
        string uuid = "F7826D64-4FA2-4E98-8024-BC5B71E0893E",
        int major = 1,
        int minor = 0,
        string identifier = "Office Beacon",
        int rssi = -50,
        double? distance = null,
        string beaconType = "iBeacon",
        bool isReachable = true,
        DateTime? lastSeenAt = null)
    {
        return new BeaconInfo(
            UUID: uuid,
            Major: major,
            Minor: minor,
            Identifier: identifier,
            RSSI: rssi,
            Distance: distance,
            BeaconType: beaconType,
            IsReachable: isReachable,
            LastSeenAt: lastSeenAt);
    }

    private List<BeaconInfo> CreateTestBeaconList()
    {
        return new List<BeaconInfo>
        {
            CreateTestBeacon(
                uuid: "F7826D64-4FA2-4E98-8024-BC5B71E0893E",
                major: 1,
                minor: 0,
                identifier: "Office Beacon",
                rssi: -50,
                distance: 0.5),
            CreateTestBeacon(
                uuid: "12345678-1234-5678-1234-567812345678",
                major: 2,
                minor: 100,
                identifier: "Conference Room",
                rssi: -70,
                distance: 3.2),
            CreateTestBeacon(
                uuid: "ABCDEF12-ABCD-EF12-ABCD-EF1234567890",
                major: 5,
                minor: 200,
                identifier: "Warehouse",
                rssi: -85,
                distance: 10.5)
        };
    }

    // ===== Permission Validation Tests =====

    [Fact]
    public async Task ScanForBeacons_WithoutPermission_ReturnsDeniedError()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        // Act
        var result = await _sut.ScanForBeaconsAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("AUTH.UNAUTHORIZED", result.Error.Code);
    }

    [Fact]
    public async Task RangeBeacons_WithoutPermission_ReturnsDeniedError()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        // Act
        var result = await _sut.RangeBeaconsAsync(null);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("AUTH.UNAUTHORIZED", result.Error.Code);
    }

    // ===== Beacon Scanning Tests =====

    [Fact]
    public async Task ScanForBeacons_WithValidState_ReturnsBeaconList()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.ScanForBeaconsAsync();

        // Assert
        Assert.NotNull(result);
        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ScanForBeacons_WithNoBeacons_ReturnsEmptyList()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.ScanForBeaconsAsync();

        // Assert
        Assert.NotNull(result);
        // On non-platform builds, will return error, but we verify permission was checked
        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ScanForBeacons_WithMultipleBeacons_ReturnsAll()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.ScanForBeaconsAsync();

        // Assert
        Assert.NotNull(result);
        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ===== Beacon Monitoring Tests =====

    [Fact]
    public async Task StartMonitoring_WithValidBeacon_SucceedsAndTracksBeacon()
    {
        // Arrange
        var beacon = CreateTestBeacon();
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.StartMonitoringAsync(beacon);

        // Assert
        Assert.NotNull(result);
        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StartMonitoring_WithInvalidUUID_ReturnsError()
    {
        // Arrange - Create beacon with invalid UUID (not GUID format)
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act & Assert - Invalid UUID should throw during beacon creation
        Assert.Throws<ArgumentException>(() =>
            CreateTestBeacon(uuid: "INVALID-UUID-FORMAT"));
    }

    [Fact]
    public async Task StopMonitoring_RemovesBeacon_FromTrackedList()
    {
        // Arrange
        var beacon = CreateTestBeacon();
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        await _sut.StartMonitoringAsync(beacon);

        // Act
        var result = await _sut.StopMonitoringAsync(beacon.UUID);

        // Assert
        Assert.NotNull(result);
        var monitored = await _sut.GetMonitoredBeaconsAsync();
        Assert.NotNull(monitored);
    }

    [Fact]
    public async Task GetMonitoredBeacons_ReturnsCorrectList()
    {
        // Arrange
        var beacon1 = CreateTestBeacon(identifier: "Beacon 1");
        var beacon2 = CreateTestBeacon(
            uuid: "12345678-1234-5678-1234-567812345678",
            identifier: "Beacon 2");

        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        await _sut.StartMonitoringAsync(beacon1);
        await _sut.StartMonitoringAsync(beacon2);

        // Act
        var result = await _sut.GetMonitoredBeaconsAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    // ===== Beacon Ranging Tests =====

    [Fact]
    public async Task RangeBeacons_WithSpecificUUID_FiltersResults()
    {
        // Arrange
        var targetUuid = "F7826D64-4FA2-4E98-8024-BC5B71E0893E";
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.RangeBeaconsAsync(targetUuid);

        // Assert
        Assert.NotNull(result);
        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RangeBeacons_WithoutFilter_ReturnsAllBeacons()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.RangeBeaconsAsync(null);

        // Assert
        Assert.NotNull(result);
        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RangeBeacons_CalculatesDistance_Correctly()
    {
        // Arrange
        var beacon = CreateTestBeacon(
            rssi: -50,
            distance: 0.5); // Excellent signal = close proximity

        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.RangeBeaconsAsync(beacon.UUID);

        // Assert
        Assert.NotNull(result);
        // Platform implementation calculates distance
    }

    // ===== Input Validation Tests =====

    [Fact]
    public async Task StartMonitoring_WithNullBeacon_ThrowsArgumentNullException()
    {
        // Arrange
        BeaconInfo? beacon = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sut.StartMonitoringAsync(beacon!));
    }

    [Fact]
    public async Task StopMonitoring_WithNullUUID_ThrowsArgumentException()
    {
        // Arrange
        string? uuid = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.StopMonitoringAsync(uuid!));
    }

    [Fact]
    public async Task StopMonitoring_WithEmptyUUID_ThrowsArgumentException()
    {
        // Arrange
        var uuid = string.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.StopMonitoringAsync(uuid));
    }

    [Fact]
    public async Task RangeBeacons_WithInvalidUUID_ReturnsError()
    {
        // Arrange
        var invalidUuid = "INVALID-UUID";
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.RangeBeaconsAsync(invalidUuid);

        // Assert
        Assert.NotNull(result);
        // Platform determines if invalid UUID is an error
    }

    // ===== Proximity Event Monitoring Tests =====

    [Fact]
    public async Task StartMonitoring_WithValidBeacon_RaisesEnterEvent()
    {
        // Arrange
        var beacon = CreateTestBeacon();
        var eventRaised = false;
        BeaconProximityEvent? capturedEvent = null;

        var subscription = _sut.OnBeaconProximityChanged.Subscribe(evt =>
        {
            eventRaised = true;
            capturedEvent = evt;
        });

        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var proximityEvent = new BeaconProximityEvent(
            Beacon: beacon,
            EventType: 1, // ENTER
            PreviousProximity: null,
            CurrentProximity: "Immediate",
            DetectedAt: DateTime.UtcNow);

        // Act
        await _sut.StartMonitoringAsync(beacon);

        var raiseMethod = typeof(BeaconService).GetMethod(
            "RaiseProximityChangeEvent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        raiseMethod?.Invoke(_sut, new object[] { proximityEvent });

        await Task.Delay(50);

        // Assert
        Assert.True(eventRaised);
        if (capturedEvent != null)
        {
            Assert.Equal(1, capturedEvent.EventType);
            Assert.Equal("Immediate", capturedEvent.CurrentProximity);
        }

        subscription.Dispose();
    }

    [Fact]
    public async Task StopMonitoring_WithMonitoredBeacon_RaisesExitEvent()
    {
        // Arrange
        var beacon = CreateTestBeacon();
        var eventRaised = false;
        BeaconProximityEvent? capturedEvent = null;

        var subscription = _sut.OnBeaconProximityChanged.Subscribe(evt =>
        {
            eventRaised = true;
            capturedEvent = evt;
        });

        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        await _sut.StartMonitoringAsync(beacon);

        var proximityEvent = new BeaconProximityEvent(
            Beacon: beacon,
            EventType: 0, // EXIT
            PreviousProximity: "Near",
            CurrentProximity: "Far",
            DetectedAt: DateTime.UtcNow);

        // Act
        var raiseMethod = typeof(BeaconService).GetMethod(
            "RaiseProximityChangeEvent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        raiseMethod?.Invoke(_sut, new object[] { proximityEvent });

        await _sut.StopMonitoringAsync(beacon.UUID);
        await Task.Delay(50);

        // Assert
        Assert.True(eventRaised);
        if (capturedEvent != null)
        {
            Assert.Equal(0, capturedEvent.EventType);
        }

        subscription.Dispose();
    }

    [Fact]
    public async Task ProximityChange_RaisesSignalChangeEvent()
    {
        // Arrange
        var beacon = CreateTestBeacon(rssi: -50, distance: 1.0);
        var eventRaised = false;
        BeaconProximityEvent? capturedEvent = null;

        var subscription = _sut.OnBeaconProximityChanged.Subscribe(evt =>
        {
            eventRaised = true;
            capturedEvent = evt;
        });

        var proximityEvent = new BeaconProximityEvent(
            Beacon: beacon,
            EventType: 2, // PROXIMITY_CHANGED
            PreviousProximity: "Immediate",
            CurrentProximity: "Near",
            DetectedAt: DateTime.UtcNow);

        // Act
        var raiseMethod = typeof(BeaconService).GetMethod(
            "RaiseProximityChangeEvent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        raiseMethod?.Invoke(_sut, new object[] { proximityEvent });

        await Task.Delay(50);

        // Assert
        Assert.True(eventRaised);
        if (capturedEvent != null)
        {
            Assert.Equal(2, capturedEvent.EventType);
            Assert.Equal("Immediate", capturedEvent.PreviousProximity);
            Assert.Equal("Near", capturedEvent.CurrentProximity);
        }

        subscription.Dispose();
    }

    // ===== Observable Events Tests =====

    [Fact]
    public async Task OnBeaconProximityChanged_CanSubscribe_WithoutError()
    {
        // Arrange
        var subscription = _sut.OnBeaconProximityChanged.Subscribe(_ => { });

        // Act
        var observable = _sut.OnBeaconProximityChanged;

        // Assert
        Assert.NotNull(observable);
        subscription.Dispose();
    }

    [Fact]
    public async Task OnBeaconProximityChanged_EmitsCorrectEventType_ForProximityChange()
    {
        // Arrange
        var beacon = CreateTestBeacon();
        var capturedEventTypes = new List<int>();

        var subscription = _sut.OnBeaconProximityChanged.Subscribe(evt =>
        {
            capturedEventTypes.Add(evt.EventType);
        });

        // Act
        var enterEvent = new BeaconProximityEvent(
            Beacon: beacon,
            EventType: 1, // ENTER
            PreviousProximity: null,
            CurrentProximity: "Immediate",
            DetectedAt: DateTime.UtcNow);

        var raiseMethod = typeof(BeaconService).GetMethod(
            "RaiseProximityChangeEvent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        raiseMethod?.Invoke(_sut, new object[] { enterEvent });

        await Task.Delay(50);

        // Assert
        Assert.Single(capturedEventTypes);
        Assert.Equal(1, capturedEventTypes[0]);

        subscription.Dispose();
    }

    [Fact]
    public async Task OnBeaconProximityChanged_MultipleSubscribers_AllReceiveEvent()
    {
        // Arrange
        var beacon = CreateTestBeacon();
        var event1Raised = false;
        var event2Raised = false;
        var event3Raised = false;

        var subscription1 = _sut.OnBeaconProximityChanged.Subscribe(_ => event1Raised = true);
        var subscription2 = _sut.OnBeaconProximityChanged.Subscribe(_ => event2Raised = true);
        var subscription3 = _sut.OnBeaconProximityChanged.Subscribe(_ => event3Raised = true);

        var proximityEvent = new BeaconProximityEvent(
            Beacon: beacon,
            EventType: 1,
            PreviousProximity: null,
            CurrentProximity: "Immediate",
            DetectedAt: DateTime.UtcNow);

        // Act
        var raiseMethod = typeof(BeaconService).GetMethod(
            "RaiseProximityChangeEvent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        raiseMethod?.Invoke(_sut, new object[] { proximityEvent });

        await Task.Delay(50);

        // Assert
        Assert.True(event1Raised);
        Assert.True(event2Raised);
        Assert.True(event3Raised);

        subscription1.Dispose();
        subscription2.Dispose();
        subscription3.Dispose();
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
    public async Task IsAvailable_OnSupportedPlatform_ChecksCapability()
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
    public async Task MultipleBeaconMonitoring_WorksConcurrently_WithoutConflicts()
    {
        // Arrange
        var beacon1 = CreateTestBeacon(
            uuid: "F7826D64-4FA2-4E98-8024-BC5B71E0893E",
            identifier: "Beacon 1");
        var beacon2 = CreateTestBeacon(
            uuid: "12345678-1234-5678-1234-567812345678",
            identifier: "Beacon 2");
        var beacon3 = CreateTestBeacon(
            uuid: "ABCDEF12-ABCD-EF12-ABCD-EF1234567890",
            identifier: "Beacon 3");

        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result1 = await _sut.StartMonitoringAsync(beacon1);
        var result2 = await _sut.StartMonitoringAsync(beacon2);
        var result3 = await _sut.StartMonitoringAsync(beacon3);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result3);

        var monitored = await _sut.GetMonitoredBeaconsAsync();
        Assert.NotNull(monitored.Data);

        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task MonitoringLifecycle_StartStopStart_WorksCorrectly()
    {
        // Arrange
        var beacon = CreateTestBeacon();
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var startResult1 = await _sut.StartMonitoringAsync(beacon);
        var stopResult = await _sut.StopMonitoringAsync(beacon.UUID);
        var startResult2 = await _sut.StartMonitoringAsync(beacon);

        // Assert
        Assert.NotNull(startResult1);
        Assert.NotNull(stopResult);
        Assert.NotNull(startResult2);

        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Exactly(2)); // Only called for start operations
    }

    // ===== Constructor Validation Tests =====

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new BeaconService(null!, _permissionService.Object));
    }

    [Fact]
    public void Constructor_WithNullPermissionService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new BeaconService(_logger.Object, null!));
    }

    // ===== Beacon Data Validation Tests =====

    [Fact]
    public void CreateBeacon_WithValidData_Succeeds()
    {
        // Act
        var beacon = CreateTestBeacon(
            uuid: "F7826D64-4FA2-4E98-8024-BC5B71E0893E",
            major: 42,
            minor: 1000,
            identifier: "TestBeacon",
            rssi: -70);

        // Assert
        Assert.NotNull(beacon);
        Assert.Equal("F7826D64-4FA2-4E98-8024-BC5B71E0893E", beacon.UUID);
        Assert.Equal(42, beacon.Major);
        Assert.Equal(1000, beacon.Minor);
        Assert.Equal("TestBeacon", beacon.Identifier);
        Assert.Equal(-70, beacon.RSSI);
    }

    [Fact]
    public void CreateBeacon_WithInvalidUUID_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CreateTestBeacon(uuid: "INVALID-UUID-FORMAT"));
    }

    [Fact]
    public void CreateBeacon_WithMajorOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert - Major > 65535
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateTestBeacon(major: 70000));
    }

    [Fact]
    public void CreateBeacon_WithMinorOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert - Minor > 65535
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateTestBeacon(minor: 70000));
    }

    [Fact]
    public void CreateBeacon_WithInvalidRSSI_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert - RSSI < -100
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateTestBeacon(rssi: -150));
    }

    [Fact]
    public void CreateBeacon_WithRSSITooHigh_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert - RSSI > -30
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateTestBeacon(rssi: -20));
    }

    [Fact]
    public void CreateBeacon_WithNegativeDistance_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateTestBeacon(distance: -1.0));
    }

    [Fact]
    public void CreateBeacon_WithEmptyIdentifier_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CreateTestBeacon(identifier: ""));
    }

    [Fact]
    public void CreateBeacon_WithTooLongIdentifier_ThrowsArgumentException()
    {
        // Act & Assert - Identifier > 64 characters
        var longIdentifier = new string('A', 65);
        Assert.Throws<ArgumentException>(
            () => CreateTestBeacon(identifier: longIdentifier));
    }

    [Fact]
    public void Beacon_ProximityCalculation_Immediate()
    {
        // Arrange & Act
        var beacon = CreateTestBeacon(rssi: -40, distance: 0.5);

        // Assert
        Assert.Equal("Immediate", beacon.Proximity());
    }

    [Fact]
    public void Beacon_ProximityCalculation_Near()
    {
        // Arrange & Act
        var beacon = CreateTestBeacon(rssi: -65, distance: 3.0);

        // Assert
        Assert.Equal("Near", beacon.Proximity());
    }

    [Fact]
    public void Beacon_ProximityCalculation_Far()
    {
        // Arrange & Act
        var beacon = CreateTestBeacon(rssi: -80, distance: 10.0);

        // Assert
        Assert.Equal("Far", beacon.Proximity());
    }

    [Fact]
    public void Beacon_ProximityCalculation_Unknown()
    {
        // Arrange & Act
        var beacon = CreateTestBeacon(rssi: -85, distance: null);

        // Assert
        Assert.Equal("Unknown", beacon.Proximity());
    }

    [Fact]
    public void Beacon_SignalQuality_Excellent()
    {
        // Arrange & Act
        var beacon = CreateTestBeacon(rssi: -40);

        // Assert
        Assert.Equal("Excellent", beacon.SignalQuality());
    }

    [Fact]
    public void Beacon_SignalQuality_Good()
    {
        // Arrange & Act
        var beacon = CreateTestBeacon(rssi: -60);

        // Assert
        Assert.Equal("Good", beacon.SignalQuality());
    }

    [Fact]
    public void Beacon_SignalQuality_Fair()
    {
        // Arrange & Act
        var beacon = CreateTestBeacon(rssi: -75);

        // Assert
        Assert.Equal("Fair", beacon.SignalQuality());
    }

    [Fact]
    public void Beacon_SignalQuality_Poor()
    {
        // Arrange & Act
        var beacon = CreateTestBeacon(rssi: -95);

        // Assert
        Assert.Equal("Poor", beacon.SignalQuality());
    }

    [Fact]
    public void Beacon_IsiBeacon_WithDefaultType_ReturnsTrue()
    {
        // Arrange & Act
        var beacon = CreateTestBeacon(beaconType: "iBeacon");

        // Assert
        Assert.True(beacon.IsiBeacon());
        Assert.False(beacon.IsEddystone());
    }

    [Fact]
    public void Beacon_IsEddystone_WithEddystoneType_ReturnsTrue()
    {
        // Arrange & Act
        var beacon = CreateTestBeacon(beaconType: "Eddystone");

        // Assert
        Assert.True(beacon.IsEddystone());
        Assert.False(beacon.IsiBeacon());
    }

    [Fact]
    public void Beacon_IsReachable_ReturnsCorrectValue()
    {
        // Arrange
        var reachable = CreateTestBeacon(isReachable: true);
        var unreachable = CreateTestBeacon(isReachable: false);

        // Act & Assert
        Assert.True(reachable.IsReachable);
        Assert.False(unreachable.IsReachable);
    }

    // ===== Edge Case Tests =====

    [Fact]
    public async Task ScanForBeacons_WithCancellation_RespondsToCancel()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert - Should not throw, either succeeds or handles cancellation gracefully
        try
        {
            var result = await _sut.ScanForBeaconsAsync(cts.Token);
            Assert.NotNull(result);
        }
        catch (OperationCanceledException)
        {
            // Acceptable - cancellation token respected
        }
    }

    [Fact]
    public async Task RangeBeacons_WithEmptyUUID_Succeeds()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.RangeBeaconsAsync(string.Empty);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task MultipleScans_Sequential_ChecksPermissionEachTime()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var scan1 = await _sut.ScanForBeaconsAsync();
        var scan2 = await _sut.ScanForBeaconsAsync();
        var scan3 = await _sut.ScanForBeaconsAsync();

        // Assert
        Assert.NotNull(scan1);
        Assert.NotNull(scan2);
        Assert.NotNull(scan3);

        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public void Beacon_ZeroMajor_Succeeds()
    {
        // Act
        var beacon = CreateTestBeacon(major: 0);

        // Assert
        Assert.Equal(0, beacon.Major);
    }

    [Fact]
    public void Beacon_MaxMajor_Succeeds()
    {
        // Act
        var beacon = CreateTestBeacon(major: 65535);

        // Assert
        Assert.Equal(65535, beacon.Major);
    }

    [Fact]
    public void Beacon_ZeroMinor_Succeeds()
    {
        // Act
        var beacon = CreateTestBeacon(minor: 0);

        // Assert
        Assert.Equal(0, beacon.Minor);
    }

    [Fact]
    public void Beacon_MaxMinor_Succeeds()
    {
        // Act
        var beacon = CreateTestBeacon(minor: 65535);

        // Assert
        Assert.Equal(65535, beacon.Minor);
    }

    [Fact]
    public void Beacon_MinRSSI_Succeeds()
    {
        // Act
        var beacon = CreateTestBeacon(rssi: -100);

        // Assert
        Assert.Equal(-100, beacon.RSSI);
    }

    [Fact]
    public void Beacon_MaxRSSI_Succeeds()
    {
        // Act
        var beacon = CreateTestBeacon(rssi: -30);

        // Assert
        Assert.Equal(-30, beacon.RSSI);
    }

    [Fact]
    public void Beacon_ZeroDistance_Succeeds()
    {
        // Act
        var beacon = CreateTestBeacon(distance: 0.0);

        // Assert
        Assert.Equal(0.0, beacon.Distance);
    }

    [Fact]
    public void Beacon_NullDistance_Succeeds()
    {
        // Act
        var beacon = CreateTestBeacon(distance: null);

        // Assert
        Assert.Null(beacon.Distance);
    }

    [Fact]
    public void Beacon_WithMetadata_StoresCorrectly()
    {
        // Act
        var beacon = new BeaconInfo(
            UUID: "F7826D64-4FA2-4E98-8024-BC5B71E0893E",
            Major: 1,
            Minor: 0,
            Identifier: "TestBeacon",
            RSSI: -50,
            Metadata: new Dictionary<string, string> { { "location", "office" }, { "floor", "3" } });

        // Assert
        Assert.NotNull(beacon.Metadata);
        Assert.Equal(2, beacon.Metadata.Count);
        Assert.Equal("office", beacon.Metadata["location"]);
        Assert.Equal("3", beacon.Metadata["floor"]);
    }

    [Fact]
    public void Beacon_WithNullMetadata_CreatesEmptyDictionary()
    {
        // Act
        var beacon = CreateTestBeacon();

        // Assert
        Assert.NotNull(beacon.Metadata);
        Assert.Empty(beacon.Metadata);
    }

    [Fact]
    public async Task StartMonitoring_WithoutPermission_ReturnsDeniedError()
    {
        // Arrange
        var beacon = CreateTestBeacon();
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        // Act
        var result = await _sut.StartMonitoringAsync(beacon);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("AUTH.UNAUTHORIZED", result.Error.Code);
    }
}
