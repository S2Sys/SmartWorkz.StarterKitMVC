namespace SmartWorkz.Mobile.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;

public class LocationServiceTests
{
    private readonly Mock<IPermissionService> _permissions = new();
    private readonly LocationService _sut;

    public LocationServiceTests()
    {
        _sut = new LocationService(NullLogger<LocationService>.Instance, _permissions.Object);
    }

    [Fact]
    public void IsTracking_InitiallyFalse()
    {
        Assert.False(_sut.IsTracking);
    }

    [Fact]
    public void StartTracking_SetsIsTracking()
    {
        // Act
        _sut.StartTracking();

        // Assert
        Assert.True(_sut.IsTracking);
    }

    [Fact]
    public void StartTracking_ReturnsObservable()
    {
        // Act
        var observable = _sut.StartTracking();

        // Assert
        Assert.NotNull(observable);
    }

    [Fact]
    public void StopTracking_ClearsIsTracking()
    {
        // Arrange
        _sut.StartTracking();

        // Act
        _sut.StopTracking();

        // Assert
        Assert.False(_sut.IsTracking);
    }

    [Fact]
    public void StartTracking_MultipleCall_ReturnsObservableBothTimes()
    {
        // Act
        var obs1 = _sut.StartTracking();
        var obs2 = _sut.StartTracking();

        // Assert
        Assert.NotNull(obs1);
        Assert.NotNull(obs2);
        Assert.True(_sut.IsTracking);
    }

    [Fact]
    public async Task GetCurrentLocationAsync_PermissionDenied_ReturnsNull()
    {
        // Arrange
        _permissions.Setup(p => p.CheckAsync(MobilePermission.Location, default))
                    .ReturnsAsync(PermissionStatus.Denied);

        // Act
        var location = await _sut.GetCurrentLocationAsync();

        // Assert
        Assert.Null(location);
    }

    [Fact]
    public void StopTracking_WhenNotTracking_DoesNotThrow()
    {
        // Act & Assert - should not throw
        _sut.StopTracking();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsBoolean()
    {
        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        Assert.IsType<bool>(result);
    }
}
