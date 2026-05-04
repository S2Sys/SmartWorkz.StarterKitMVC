namespace SmartWorkz.Mobile;

/// <summary>
/// Represents a GPS location with coordinates and optional metadata.
/// </summary>
public sealed record GpsLocation(
    double Latitude,
    double Longitude,
    double? Altitude = null,
    double? Accuracy = null,
    DateTime? Timestamp = null)
{
    /// <summary>
    /// Calculates the distance in kilometers between this location and another using the Haversine formula.
    /// </summary>
    /// <param name="other">The other location to calculate distance to</param>
    /// <returns>Distance in kilometers</returns>
    public double DistanceTo(GpsLocation other)
    {
        const double earthRadiusKm = 6371.0;

        var dLat = DegreesToRadians(other.Latitude - Latitude);
        var dLon = DegreesToRadians(other.Longitude - Longitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(Latitude)) * Math.Cos(DegreesToRadians(other.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Asin(Math.Sqrt(a));
        return earthRadiusKm * c;
    }

    /// <summary>
    /// Calculates the distance in meters between this location and another using the Haversine formula.
    /// </summary>
    /// <param name="other">The other location to calculate distance to</param>
    /// <returns>Distance in meters</returns>
    public double DistanceToMeters(GpsLocation other) => DistanceTo(other) * 1000;

    /// <summary>
    /// Checks if this location is approximately equal to another (within specified accuracy in kilometers).
    /// </summary>
    /// <param name="other">The other location to compare with</param>
    /// <param name="toleranceKm">Tolerance in kilometers (default: 0.01 km = 10 meters)</param>
    /// <returns>True if locations are within tolerance distance</returns>
    public bool IsApproximatelyEqual(GpsLocation other, double toleranceKm = 0.01) =>
        DistanceTo(other) <= toleranceKm;

    private static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180.0;
}
