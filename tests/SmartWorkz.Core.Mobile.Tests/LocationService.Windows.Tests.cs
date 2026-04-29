namespace SmartWorkz.Mobile.Tests.Services;

using SmartWorkz.Mobile;
using SmartWorkz.Mobile.Services;
using Xunit;

[Collection("LocationService Windows Tests")]
public class LocationServiceWindowsTests
{
    [Fact(Skip = "Requires Windows Runtime")]
    public async Task LocationService_GetCurrentLocation_ReturnsValidLocation()
    {
        if (!OperatingSystem.IsWindows())
            Assert.True(false, "Windows-specific test");

        var service = new LocationService();
        var location = await service.GetCurrentLocationAsync();

        Assert.NotNull(location);
        Assert.True(location.Latitude >= -90 && location.Latitude <= 90);
        Assert.True(location.Longitude >= -180 && location.Longitude <= 180);
        Assert.NotEqual(default(DateTime), location.Timestamp);
        Assert.Equal(DateTimeKind.Utc, location.Timestamp.Kind);
    }

    [Fact]
    public async Task LocationService_IsLocationEnabled_ReturnsBoolean()
    {
        // Can run without Windows runtime
        var service = new LocationService();
        var enabled = await service.IsLocationEnabledAsync();

        Assert.IsType<bool>(enabled);
    }
}
