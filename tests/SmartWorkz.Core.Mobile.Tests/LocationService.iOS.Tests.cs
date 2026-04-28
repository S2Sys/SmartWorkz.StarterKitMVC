namespace SmartWorkz.Mobile.Tests.Services;

using SmartWorkz.Mobile;
using SmartWorkz.Mobile.Services;
using Xunit;

[Collection("LocationService iOS Tests")]
public class LocationServiceIosTests
{
    [Fact(Skip = "Requires iOS runtime")]
    public async Task LocationService_GetCurrentLocation_ReturnsValidLocation()
    {
        var service = new LocationService();
        var location = await service.GetCurrentLocationAsync();

        Assert.NotNull(location);
        Assert.True(location.Latitude >= -90 && location.Latitude <= 90);
        Assert.True(location.Longitude >= -180 && location.Longitude <= 180);
        Assert.NotEqual(default(DateTime), location.Timestamp);
        Assert.Equal(DateTimeKind.Utc, location.Timestamp.Kind);
    }
}
