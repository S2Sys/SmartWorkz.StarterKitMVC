namespace SmartWorkz.Mobile.Tests.Services;

using SmartWorkz.Mobile;
using SmartWorkz.Mobile.Services;
using Xunit;

[Collection("LocationService iOS Tests")]
public class LocationServiceiOSTests
{
    [Fact]
    public async Task LocationService_GetCurrentLocation_ReturnsValidLocation()
    {
        if (!OperatingSystem.IsIOS())
            throw new SkipTestException("iOS-specific test");

        var service = new LocationService();
        var location = await service.GetCurrentLocationAsync();

        Assert.NotNull(location);
        Assert.True(location.Latitude >= -90 && location.Latitude <= 90);
        Assert.True(location.Longitude >= -180 && location.Longitude <= 180);
    }
}
