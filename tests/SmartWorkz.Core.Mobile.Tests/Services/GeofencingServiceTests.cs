namespace SmartWorkz.Mobile.Tests.Services;

using Moq;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Mobile.Services.Implementations;
using Xunit;
using Microsoft.Extensions.Logging;

public class GeofencingServiceTests
{
    private readonly Mock<ILogger<GeofencingService>> _logger = new();
    private readonly Mock<IPermissionService> _permissionService = new();
    private readonly GeofencingService _sut;

    public GeofencingServiceTests()
    {
        _sut = new GeofencingService(_logger.Object, _permissionService.Object);
    }

    [Fact]
    public async Task StartMonitoringAsync_WithValidRegion_ReturnsSuccess()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckPermissionAsync("Location"))
            .ReturnsAsync(true);

        var region = new GeofenceRegion
        {
            Id = "downtown",
            Name = "Downtown Area",
            Latitude = 40.7128,
            Longitude = -74.0060,
            RadiusMeters = 500
        };

        // Act
        var result = await _sut.StartMonitoringAsync(region);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task StartMonitoringAsync_WithoutPermission_ReturnsFailed()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckPermissionAsync("Location"))
            .ReturnsAsync(false);

        var region = new GeofenceRegion
        {
            Id = "downtown",
            Name = "Downtown Area",
            Latitude = 40.7128,
            Longitude = -74.0060,
            RadiusMeters = 500
        };

        // Act
        var result = await _sut.StartMonitoringAsync(region);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("LOCATION.PERMISSION_DENIED", result.Error?.Code);
    }

    [Fact]
    public async Task StartMonitoringAsync_WithInvalidRadius_ReturnsFailed()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckPermissionAsync("Location"))
            .ReturnsAsync(true);

        var region = new GeofenceRegion
        {
            Id = "downtown",
            Name = "Downtown Area",
            Latitude = 40.7128,
            Longitude = -74.0060,
            RadiusMeters = 5 // Too small, minimum is 10
        };

        // Act
        var result = await _sut.StartMonitoringAsync(region);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("GEOFENCE.INVALID_REGION", result.Error?.Code);
    }

    [Fact]
    public async Task GetMonitoredRegionsAsync_AfterStartMonitoring_ReturnsRegion()
    {
        // Arrange
        _permissionService.Setup(x => x.CheckPermissionAsync("Location"))
            .ReturnsAsync(true);

        var region = new GeofenceRegion
        {
            Id = "downtown",
            Name = "Downtown Area",
            Latitude = 40.7128,
            Longitude = -74.0060,
            RadiusMeters = 500
        };

        await _sut.StartMonitoringAsync(region);

        // Act
        var result = await _sut.GetMonitoredRegionsAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.Single(result.Data);
        Assert.Equal("downtown", result.Data![0].Id);
    }
}
