namespace SmartWorkz.Core.Mobile.Tests;

using SmartWorkz.Core.Mobile.Models;
using SmartWorkz.Core.Mobile.Services;
using Xunit;

[Collection("LocationService Tests")]
public class LocationServiceTests
{
    [Fact]
    public void Location_GetDistanceTo_CalculatesCorrectly()
    {
        var location1 = new Location { Latitude = 40.7128, Longitude = -74.0060 }; // NYC
        var location2 = new Location { Latitude = 34.0522, Longitude = -118.2437 }; // LA

        var distance = location1.GetDistanceTo(location2);

        // NYC to LA is approximately 3944 km
        Assert.True(distance > 3900000 && distance < 4000000,
            $"Expected distance around 3944km, got {distance / 1000}km");
    }

    [Fact]
    public void Location_Equals_WorksCorrectly()
    {
        var loc1 = new Location { Latitude = 40.7128, Longitude = -74.0060 };
        var loc2 = new Location { Latitude = 40.7128, Longitude = -74.0060 };
        var loc3 = new Location { Latitude = 34.0522, Longitude = -118.2437 };

        Assert.Equal(loc1, loc2);
        Assert.NotEqual(loc1, loc3);
    }

    [Fact]
    public void Location_WithNullAccuracy_IsValid()
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
