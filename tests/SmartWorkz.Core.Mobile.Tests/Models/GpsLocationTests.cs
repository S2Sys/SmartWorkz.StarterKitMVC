namespace SmartWorkz.Mobile.Tests.Models;

public class GpsLocationTests
{
    [Fact]
    public void DistanceTo_SameLocation_ReturnsZero()
    {
        // Arrange
        var location = new GpsLocation(40.7128, -74.0060);

        // Act
        var distance = location.DistanceTo(location);

        // Assert
        Assert.Equal(0, distance, precision: 5);
    }

    [Fact]
    public void DistanceTo_NewYorkToLondon_ReturnsApproximateDistance()
    {
        // Arrange
        var newYork = new GpsLocation(40.7128, -74.0060);
        var london = new GpsLocation(51.5074, -0.1278);
        const double expectedDistanceKm = 5570; // Approximate distance NYC to London

        // Act
        var distance = newYork.DistanceTo(london);

        // Assert - Allow 100km tolerance for rounding
        Assert.InRange(distance, expectedDistanceKm - 100, expectedDistanceKm + 100);
    }

    [Fact]
    public void DistanceToMeters_ReturnsMetersConversion()
    {
        // Arrange
        var location1 = new GpsLocation(0, 0);
        var location2 = new GpsLocation(0.00898, 0); // Approximately 1 km

        // Act
        var distanceMeters = location1.DistanceToMeters(location2);

        // Assert - Should be close to 1000 meters
        Assert.InRange(distanceMeters, 900, 1100);
    }

    [Fact]
    public void IsApproximatelyEqual_SameLocation_ReturnsTrue()
    {
        // Arrange
        var location = new GpsLocation(40.7128, -74.0060);

        // Act & Assert
        Assert.True(location.IsApproximatelyEqual(location));
    }

    [Fact]
    public void IsApproximatelyEqual_WithinTolerance_ReturnsTrue()
    {
        // Arrange
        var location1 = new GpsLocation(40.7128, -74.0060);
        var location2 = new GpsLocation(40.7129, -74.0061); // Very close

        // Act & Assert
        Assert.True(location1.IsApproximatelyEqual(location2, toleranceKm: 0.5));
    }

    [Fact]
    public void IsApproximatelyEqual_OutsideTolerance_ReturnsFalse()
    {
        // Arrange
        var newYork = new GpsLocation(40.7128, -74.0060);
        var losAngeles = new GpsLocation(34.0522, -118.2437);

        // Act & Assert
        Assert.False(newYork.IsApproximatelyEqual(losAngeles, toleranceKm: 100));
    }

    [Fact]
    public void Constructor_WithAllParameters_StoresValues()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;

        // Act
        var location = new GpsLocation(40.7128, -74.0060, Altitude: 10.5, Accuracy: 5.0, Timestamp: timestamp);

        // Assert
        Assert.Equal(40.7128, location.Latitude);
        Assert.Equal(-74.0060, location.Longitude);
        Assert.Equal(10.5, location.Altitude);
        Assert.Equal(5.0, location.Accuracy);
        Assert.Equal(timestamp, location.Timestamp);
    }

    [Fact]
    public void Constructor_WithMinimalParameters_HasNullMetadata()
    {
        // Arrange & Act
        var location = new GpsLocation(40.7128, -74.0060);

        // Assert
        Assert.Equal(40.7128, location.Latitude);
        Assert.Equal(-74.0060, location.Longitude);
        Assert.Null(location.Altitude);
        Assert.Null(location.Accuracy);
        Assert.Null(location.Timestamp);
    }
}
