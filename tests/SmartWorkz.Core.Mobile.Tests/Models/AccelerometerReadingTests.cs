namespace SmartWorkz.Mobile.Tests.Models;

public class AccelerometerReadingTests
{
    [Fact]
    public void Magnitude_CalculatesVectorLength()
    {
        // Arrange (3-4-5 right triangle)
        var reading = new AccelerometerReading(3.0, 4.0, 0.0, DateTime.UtcNow);

        // Act
        var magnitude = reading.Magnitude;

        // Assert
        Assert.Equal(5.0, magnitude, precision: 5);
    }

    [Fact]
    public void GetComponent_ReturnsCorrectAxis()
    {
        // Arrange
        var reading = new AccelerometerReading(1.0, 2.0, 3.0, DateTime.UtcNow);

        // Act & Assert
        Assert.Equal(1.0, reading.GetComponent(Axis.X));
        Assert.Equal(2.0, reading.GetComponent(Axis.Y));
        Assert.Equal(3.0, reading.GetComponent(Axis.Z));
    }

    [Fact]
    public void Subtract_CalculatesDifference()
    {
        // Arrange
        var r1 = new AccelerometerReading(5.0, 6.0, 7.0, DateTime.UtcNow);
        var r2 = new AccelerometerReading(1.0, 2.0, 3.0, DateTime.UtcNow);

        // Act
        var diff = r1.Subtract(r2);

        // Assert
        Assert.Equal(4.0, diff.X);
        Assert.Equal(4.0, diff.Y);
        Assert.Equal(4.0, diff.Z);
    }

    [Fact]
    public void Constructor_AllValuesRequired()
    {
        // Act
        var reading = new AccelerometerReading(1.5, 2.5, 3.5, DateTime.UtcNow);

        // Assert
        Assert.Equal(1.5, reading.X);
        Assert.Equal(2.5, reading.Y);
        Assert.Equal(3.5, reading.Z);
    }

    [Fact]
    public void GetComponent_InvalidAxis_ReturnsZero()
    {
        // Arrange
        var reading = new AccelerometerReading(1.0, 2.0, 3.0, DateTime.UtcNow);

        // Act
        var result = reading.GetComponent((Axis)999); // Invalid axis

        // Assert
        Assert.Equal(0.0, result);
    }
}
