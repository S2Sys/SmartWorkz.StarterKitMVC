namespace SmartWorkz.Core.Mobile.Models;

/// <summary>
/// Represents a geographic location with coordinates, accuracy, and timestamp.
/// </summary>
public class Location : IEquatable<Location>
{
    /// <summary>Gets or sets latitude in degrees (-90 to +90).</summary>
    public double Latitude { get; set; }

    /// <summary>Gets or sets longitude in degrees (-180 to +180).</summary>
    public double Longitude { get; set; }

    /// <summary>Gets or sets horizontal accuracy in meters (null if unknown).</summary>
    public double? Accuracy { get; set; }

    /// <summary>Gets or sets altitude in meters above sea level (null if unknown).</summary>
    public double? Altitude { get; set; }

    /// <summary>Gets or sets vertical accuracy in meters (null if unknown).</summary>
    public double? AltitudeAccuracy { get; set; }

    /// <summary>Gets or sets heading in degrees (0-360, null if unknown).</summary>
    public double? Heading { get; set; }

    /// <summary>Gets or sets speed in meters per second (null if unknown).</summary>
    public double? Speed { get; set; }

    /// <summary>Gets or sets UTC timestamp when location was acquired.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Gets the distance in meters between this location and another.</summary>
    public double GetDistanceTo(Location other)
    {
        const double earthRadiusMeters = 6371000;

        var dLat = ToRadians(other.Latitude - Latitude);
        var dLng = ToRadians(other.Longitude - Longitude);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusMeters * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    public bool Equals(Location? other) =>
        other != null &&
        Latitude == other.Latitude &&
        Longitude == other.Longitude;

    public override bool Equals(object? obj) => Equals(obj as Location);
    public override int GetHashCode() => (Latitude, Longitude).GetHashCode();
}
