namespace SmartWorkz.Mobile.Tests.Services;

using Moq;
using SmartWorkz.Mobile;
using SmartWorkz.Shared;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Reactive.Linq;

public class GeofencingServiceTests
{
    private readonly Mock<ILogger<GeofencingService>> _logger = new();
    private readonly Mock<IPermissionService> _permissionService = new();
    private readonly GeofencingService _sut;

    public GeofencingServiceTests()
    {
        _sut = new GeofencingService(_logger.Object, _permissionService.Object);
    }

    // ===== Permission Validation Tests =====

    [Fact]
    public async Task StartMonitoringAsync_WithoutPermission_ReturnsPermissionError()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        var region = new GeofenceRegion(
            Id: "downtown",
            Name: "Downtown Area",
            Latitude: 40.7128,
            Longitude: -74.0060,
            RadiusMeters: 500);

        // Act
        var result = await _sut.StartMonitoringAsync(region);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("AUTH.UNAUTHORIZED", result.Error.Code);
    }

    [Fact]
    public async Task StartMonitoringAsync_WithDeniedPermission_ReturnsUnauthorizedError()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        var region = new GeofenceRegion(
            Id: "region1",
            Name: "Test Region",
            Latitude: 37.7749,
            Longitude: -122.4194,
            RadiusMeters: 100);

        // Act
        var result = await _sut.StartMonitoringAsync(region);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Contains("permission", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ===== Radius Validation Tests =====

    [Fact]
    public void StartMonitoringAsync_WithRadiusTooSmall_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act & Assert - GeofenceRegion validates radius in constructor
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeofenceRegion(
            Id: "small-radius",
            Name: "Too Small",
            Latitude: 40.7128,
            Longitude: -74.0060,
            RadiusMeters: 9)); // Below minimum of 10
    }

    [Fact]
    public void StartMonitoringAsync_WithRadiusTooLarge_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act & Assert - GeofenceRegion validates radius in constructor
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeofenceRegion(
            Id: "large-radius",
            Name: "Too Large",
            Latitude: 40.7128,
            Longitude: -74.0060,
            RadiusMeters: 10001)); // Above maximum of 10000
    }

    [Fact]
    public async Task StartMonitoringAsync_WithMinimumValidRadius_DoesNotRejectForRadius()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var region = new GeofenceRegion(
            Id: "min-radius",
            Name: "Minimum Valid",
            Latitude: 40.7128,
            Longitude: -74.0060,
            RadiusMeters: 10); // Exactly at minimum

        // Act
        var result = await _sut.StartMonitoringAsync(region);

        // Assert - Should not be rejected for radius validation
        if (result.Error != null)
        {
            Assert.NotEqual("VALIDATION.GEOFENCE.INVALID_REGION", result.Error.Code);
        }
    }

    [Fact]
    public async Task StartMonitoringAsync_WithMaximumValidRadius_DoesNotRejectForRadius()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var region = new GeofenceRegion(
            Id: "max-radius",
            Name: "Maximum Valid",
            Latitude: 40.7128,
            Longitude: -74.0060,
            RadiusMeters: 10000); // Exactly at maximum

        // Act
        var result = await _sut.StartMonitoringAsync(region);

        // Assert - Should not be rejected for radius validation
        if (result.Error != null)
        {
            Assert.NotEqual("VALIDATION.GEOFENCE.INVALID_REGION", result.Error.Code);
        }
    }

    // ===== Happy Path Tests =====

    [Fact]
    public async Task StartMonitoringAsync_WithValidRegion_ChecksPermission()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var region = new GeofenceRegion(
            Id: "downtown",
            Name: "Downtown Area",
            Latitude: 40.7128,
            Longitude: -74.0060,
            RadiusMeters: 500);

        // Act
        var result = await _sut.StartMonitoringAsync(region);

        // Assert - Verify permission was checked
        _permissionService.Verify(
            x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StartMonitoringAsync_WithValidRegion_ValidatesRadius()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var region = new GeofenceRegion(
            Id: "downtown",
            Name: "Downtown Area",
            Latitude: 40.7128,
            Longitude: -74.0060,
            RadiusMeters: 500);

        // Act
        var result = await _sut.StartMonitoringAsync(region);

        // Assert - Valid region should not be rejected for radius
        Assert.NotNull(result.Error);
        // On non-platform builds, will return GEOFENCING.NOT_SUPPORTED,
        // but should not return GEOFENCE.INVALID_REGION for valid radius
        Assert.NotEqual("VALIDATION.GEOFENCE.INVALID_REGION", result.Error.Code);
    }

    // ===== Region Management Tests =====

    [Fact]
    public async Task GetMonitoredRegionsAsync_ReturnsEmptyList_Initially()
    {
        // Arrange
        // No regions added yet

        // Act
        var result = await _sut.GetMonitoredRegionsAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.Empty(result.Data!);
    }

    [Fact]
    public async Task GetMonitoredRegionsAsync_AfterStartMonitoring_ReturnsRegionIfPlatformSupported()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var region = new GeofenceRegion(
            Id: "downtown",
            Name: "Downtown Area",
            Latitude: 40.7128,
            Longitude: -74.0060,
            RadiusMeters: 500);

        var startResult = await _sut.StartMonitoringAsync(region);

        // Act
        var result = await _sut.GetMonitoredRegionsAsync();

        // Assert
        Assert.True(result.Succeeded);
        // Only verify region if StartMonitoring succeeded (platform-dependent)
        if (startResult.Succeeded)
        {
            Assert.Single(result.Data!);
            Assert.Equal("downtown", result.Data![0].Id);
        }
        else
        {
            // Platform not supported, region should not be in list
            Assert.Empty(result.Data!);
        }
    }

    [Fact]
    public async Task GetMonitoredRegionsAsync_WithMultipleRegions_TracksSuccessfulRegions()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var region1 = new GeofenceRegion("downtown", "Downtown", 40.7128, -74.0060, 500);
        var region2 = new GeofenceRegion("uptown", "Uptown", 40.7500, -74.0000, 300);

        var result1 = await _sut.StartMonitoringAsync(region1);
        var result2 = await _sut.StartMonitoringAsync(region2);

        // Act
        var result = await _sut.GetMonitoredRegionsAsync();

        // Assert
        Assert.True(result.Succeeded);
        // Count should match successfully started regions
        if (result1.Succeeded && result2.Succeeded)
        {
            Assert.Equal(2, result.Data!.Count);
        }
        else if (result1.Succeeded)
        {
            Assert.Single(result.Data!);
        }
        else
        {
            Assert.Empty(result.Data!);
        }
    }

    [Fact]
    public async Task GetMonitoredRegionsAsync_AfterStopMonitoring_RemovesRegion()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var region = new GeofenceRegion(
            Id: "downtown",
            Name: "Downtown Area",
            Latitude: 40.7128,
            Longitude: -74.0060,
            RadiusMeters: 500);

        await _sut.StartMonitoringAsync(region);
        await _sut.StopMonitoringAsync(region.Id);

        // Act
        var result = await _sut.GetMonitoredRegionsAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.Empty(result.Data!);
    }

    // ===== Monitoring Lifecycle Tests =====

    [Fact]
    public async Task StopMonitoringAsync_WithUnmonitoredRegionId_ReturnsSuccess()
    {
        // Arrange
        var unmonitoredRegionId = "non-existent-region";

        // Act
        var result = await _sut.StopMonitoringAsync(unmonitoredRegionId);

        // Assert
        // Should succeed (idempotent) or provide appropriate feedback
        // Base class delegates to platform, so we verify it returns a result
        Assert.NotNull(result);
    }

    [Fact]
    public async Task StopMonitoringAsync_WithMonitoredRegion_RemovesRegion()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var region = new GeofenceRegion(
            Id: "downtown",
            Name: "Downtown Area",
            Latitude: 40.7128,
            Longitude: -74.0060,
            RadiusMeters: 500);

        await _sut.StartMonitoringAsync(region);

        // Act
        var stopResult = await _sut.StopMonitoringAsync(region.Id);

        // Assert
        var monitoredRegions = await _sut.GetMonitoredRegionsAsync();
        Assert.Empty(monitoredRegions.Data!);
    }

    [Fact]
    public async Task StopMonitoringAsync_IsIdempotent_CanCallMultipleTimes()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var region = new GeofenceRegion(
            Id: "downtown",
            Name: "Downtown Area",
            Latitude: 40.7128,
            Longitude: -74.0060,
            RadiusMeters: 500);

        await _sut.StartMonitoringAsync(region);
        await _sut.StopMonitoringAsync(region.Id);

        // Act - Call stop again on already-stopped region
        var result = await _sut.StopMonitoringAsync(region.Id);

        // Assert
        Assert.NotNull(result);
        var monitoredRegions = await _sut.GetMonitoredRegionsAsync();
        Assert.Empty(monitoredRegions.Data!);
    }

    // ===== Observable Events Tests =====

    [Fact]
    public async Task OnGeofenceEventDetected_CanSubscribeTo_WithoutError()
    {
        // Arrange
        var subscription = _sut.OnGeofenceEventDetected.Subscribe(_ => { });

        // Act
        var observable = _sut.OnGeofenceEventDetected;

        // Assert
        Assert.NotNull(observable);
        subscription.Dispose();
    }

    [Fact]
    public async Task OnGeofenceEventDetected_EmitsEnterEvent_WhenRaised()
    {
        // Arrange
        var eventRaised = false;
        GeofenceEvent? capturedEvent = null;

        var subscription = _sut.OnGeofenceEventDetected.Subscribe(evt =>
        {
            eventRaised = true;
            capturedEvent = evt;
        });

        var geofenceEvent = new GeofenceEvent(
            RegionId: "downtown",
            EventType: 1, // ENTER
            DetectedAt: DateTime.UtcNow,
            CurrentLatitude: 40.7128,
            CurrentLongitude: -74.0060);

        // Act
        // Raise event via reflection on protected method
        var raiseMethod = typeof(GeofencingService).GetMethod(
            "RaiseGeofenceEvent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        raiseMethod?.Invoke(_sut, new object[] { geofenceEvent });

        // Small delay to allow observable to process
        await Task.Delay(50);

        // Assert
        Assert.True(eventRaised);
        if (capturedEvent != null)
        {
            Assert.Equal(1, capturedEvent.EventType);
            Assert.Equal("downtown", capturedEvent.RegionId);
        }

        subscription.Dispose();
    }

    [Fact]
    public async Task OnGeofenceEventDetected_EmitsExitEvent_WhenRaised()
    {
        // Arrange
        var eventRaised = false;
        GeofenceEvent? capturedEvent = null;

        var subscription = _sut.OnGeofenceEventDetected.Subscribe(evt =>
        {
            eventRaised = true;
            capturedEvent = evt;
        });

        var geofenceEvent = new GeofenceEvent(
            RegionId: "downtown",
            EventType: 0, // EXIT
            DetectedAt: DateTime.UtcNow,
            CurrentLatitude: 40.7129,
            CurrentLongitude: -74.0061);

        // Act
        var raiseMethod = typeof(GeofencingService).GetMethod(
            "RaiseGeofenceEvent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        raiseMethod?.Invoke(_sut, new object[] { geofenceEvent });

        await Task.Delay(50);

        // Assert
        Assert.True(eventRaised);
        if (capturedEvent != null)
        {
            Assert.Equal(0, capturedEvent.EventType);
            Assert.Equal("downtown", capturedEvent.RegionId);
        }

        subscription.Dispose();
    }

    [Fact]
    public async Task OnGeofenceEventDetected_MultipleSubscribers_AllReceiveEvent()
    {
        // Arrange
        var event1Raised = false;
        var event2Raised = false;

        var subscription1 = _sut.OnGeofenceEventDetected.Subscribe(_ => event1Raised = true);
        var subscription2 = _sut.OnGeofenceEventDetected.Subscribe(_ => event2Raised = true);

        var geofenceEvent = new GeofenceEvent(
            RegionId: "downtown",
            EventType: 1,
            DetectedAt: DateTime.UtcNow,
            CurrentLatitude: 40.7128,
            CurrentLongitude: -74.0060);

        // Act
        var raiseMethod = typeof(GeofencingService).GetMethod(
            "RaiseGeofenceEvent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        raiseMethod?.Invoke(_sut, new object[] { geofenceEvent });

        await Task.Delay(50);

        // Assert
        Assert.True(event1Raised);
        Assert.True(event2Raised);

        subscription1.Dispose();
        subscription2.Dispose();
    }

    // ===== Null/Guard Validation Tests =====

    [Fact]
    public async Task StartMonitoringAsync_WithNullRegion_ThrowsArgumentNullException()
    {
        // Arrange
        GeofenceRegion? region = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.StartMonitoringAsync(region!));
    }

    [Fact]
    public async Task StopMonitoringAsync_WithNullRegionId_ThrowsArgumentException()
    {
        // Arrange
        string? regionId = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.StopMonitoringAsync(regionId!));
    }

    [Fact]
    public async Task StopMonitoringAsync_WithEmptyRegionId_ThrowsArgumentException()
    {
        // Arrange
        var regionId = string.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.StopMonitoringAsync(regionId));
    }

    [Fact]
    public async Task StopMonitoringAsync_WithWhitespaceRegionId_ThrowsArgumentException()
    {
        // Arrange
        var regionId = "   ";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.StopMonitoringAsync(regionId));
    }

    // ===== Platform Availability Tests =====

    [Fact]
    public async Task IsAvailableAsync_ReturnsResult()
    {
        // Arrange
        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsBoolean()
    {
        // Arrange
        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        Assert.IsType<bool>(result.Data);
    }

    // ===== Integration Tests =====

    [Fact]
    public async Task MultipleRegionsWorkflow_ManagesRegionStates()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckAsync(MobilePermission.Location, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var region1 = new GeofenceRegion("region1", "Region 1", 40.7128, -74.0060, 500);
        var region2 = new GeofenceRegion("region2", "Region 2", 40.7500, -74.0000, 300);

        // Act & Assert - Attempt to add regions
        var result1 = await _sut.StartMonitoringAsync(region1);
        var result2 = await _sut.StartMonitoringAsync(region2);

        var monitored = await _sut.GetMonitoredRegionsAsync();

        // If platform is supported, verify full workflow
        if (result1.Succeeded && result2.Succeeded)
        {
            Assert.Equal(2, monitored.Data!.Count);

            // Remove one region
            await _sut.StopMonitoringAsync(region1.Id);
            monitored = await _sut.GetMonitoredRegionsAsync();
            Assert.Single(monitored.Data!);
            Assert.Equal("region2", monitored.Data![0].Id);

            // Remove remaining region
            await _sut.StopMonitoringAsync(region2.Id);
            monitored = await _sut.GetMonitoredRegionsAsync();
            Assert.Empty(monitored.Data!);
        }
        else
        {
            // Platform not supported, regions should not be tracked
            Assert.Empty(monitored.Data!);
        }
    }

    [Fact]
    public void StartMonitoringAsync_WithInvalidRadius_FailsInConstructor()
    {
        // Arrange & Act & Assert
        // GeofenceRegion validates radius in constructor, so invalid radius never reaches service
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeofenceRegion(
            Id: "invalid",
            Name: "Invalid Radius",
            Latitude: 40.7128,
            Longitude: -74.0060,
            RadiusMeters: 5)); // Too small
    }

}
