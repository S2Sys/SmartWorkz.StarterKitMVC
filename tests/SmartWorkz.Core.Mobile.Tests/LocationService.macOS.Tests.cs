namespace SmartWorkz.Mobile.Tests.Services;

using SmartWorkz.Mobile;
using SmartWorkz.Mobile.Services;
using Xunit;

[Collection("LocationService macOS Tests")]
public class LocationServiceMacOsTests
{
    [Fact(Skip = "Requires macOS runtime")]
    public async Task LocationService_GetCurrentLocation_ReturnsValidLocation()
    {
        if (!OperatingSystem.IsMacOS())
            Assert.True(false, "macOS-specific test");

        var service = new LocationService();
        var location = await service.GetCurrentLocationAsync();

        Assert.NotNull(location);
        Assert.True(location.Latitude >= -90 && location.Latitude <= 90);
        Assert.True(location.Longitude >= -180 && location.Longitude <= 180);
        Assert.NotEqual(default(DateTime), location.Timestamp);
        Assert.Equal(DateTimeKind.Utc, location.Timestamp.Kind);
    }

    [Fact]
    public async Task LocationService_GetPermissionStatus_ReturnsValidStatus()
    {
        // Can run without macOS runtime
        var service = new LocationService();
        var status = await service.GetPermissionStatusAsync();

        Assert.True(status == PermissionStatus.NotRequested ||
                    status == PermissionStatus.Denied ||
                    status == PermissionStatus.WhenInUse ||
                    status == PermissionStatus.Always);
    }
}
