namespace SmartWorkz.Core.Mobile.Tests;

using SmartWorkz.Mobile;
using SmartWorkz.Core.MAUI.Services;
using Xunit;

[Collection("LocationService Tests")]
public class LocationServiceTests
{
    [Fact]
    public void Location_GetDistanceTo_NYCtoLA_ReturnsApproximately3944km()
    {
        var location1 = new Location { Latitude = 40.7128, Longitude = -74.0060 }; // NYC
        var location2 = new Location { Latitude = 34.0522, Longitude = -118.2437 }; // LA

        var distance = location1.GetDistanceTo(location2);

        // NYC to LA is approximately 3944 km
        Assert.InRange(distance, 3900000, 4000000);
    }

    [Fact]
    public void Location_Equals_SameCoordinates_ReturnsTrue()
    {
        var loc1 = new Location { Latitude = 40.7128, Longitude = -74.0060 };
        var loc2 = new Location { Latitude = 40.7128, Longitude = -74.0060 };
        var loc3 = new Location { Latitude = 34.0522, Longitude = -118.2437 };

        Assert.Equal(loc1, loc2);
        Assert.NotEqual(loc1, loc3);
    }

    [Fact]
    public void Location_Constructor_NullAccuracy_DoesNotThrow()
    {
        var location = new Location
        {
            Latitude = 40.7128,
            Longitude = -74.0060,
            Accuracy = null
        };

        Assert.Null(location.Accuracy);
        Assert.NotNull(location);
    }
}
