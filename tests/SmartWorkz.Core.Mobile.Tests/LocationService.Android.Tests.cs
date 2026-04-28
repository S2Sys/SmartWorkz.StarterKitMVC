namespace SmartWorkz.Mobile.Tests.Services;

using SmartWorkz.Mobile;
using SmartWorkz.Mobile.Services;
using Xunit;

[Collection("LocationService Android Tests")]
public class LocationServiceAndroidTests
{
    [Fact(Skip = "Requires Android runtime")]
    public async Task LocationService_GetCurrentLocation_ReturnsValidLocation()
    {
        if (!OperatingSystem.IsAndroid())
            Assert.True(false, "Android-specific test");

        var service = new LocationService();
        var location = await service.GetCurrentLocationAsync();

        Assert.NotNull(location);
        Assert.True(location.Latitude >= -90 && location.Latitude <= 90);
        Assert.True(location.Longitude >= -180 && location.Longitude <= 180);
        Assert.NotEqual(default(DateTime), location.Timestamp);
        Assert.Equal(DateTimeKind.Utc, location.Timestamp.Kind);
    }

    [Fact]
    public async Task LocationService_GetPermissionStatus_ReturnsCurrentStatus()
    {
        // This test can run without Android runtime
        var service = new LocationService();
        var status = await service.GetPermissionStatusAsync();

        // Should return one of the valid PermissionStatus values
        Assert.True(status == PermissionStatus.NotRequested ||
                    status == PermissionStatus.Denied ||
                    status == PermissionStatus.WhenInUse ||
                    status == PermissionStatus.Always);
    }
}
