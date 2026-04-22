namespace SmartWorkz.Mobile;

/// <summary>
/// Represents a geofence region with geographic boundaries and metadata.
/// </summary>
/// <remarks>
/// This record defines a circular geofence area for location-based services.
/// The radius is constrained between 10 and 10,000 meters for practical use cases.
/// </remarks>
public sealed record GeofenceRegion(
    string Id,
    string Name,
    double Latitude,
    double Longitude,
    double RadiusMeters,
    Dictionary<string, object>? Metadata = null,
    DateTime CreatedAt = default)
{
    /// <summary>
    /// Gets the unique identifier for this geofence region.
    /// </summary>
    public string Id { get; } = Id ?? throw new ArgumentNullException(nameof(Id));

    /// <summary>
    /// Gets the display name of this geofence region.
    /// </summary>
    public string Name { get; } = Name ?? throw new ArgumentNullException(nameof(Name));

    /// <summary>
    /// Gets the latitude coordinate of the geofence center.
    /// Valid range: -90 to 90 degrees.
    /// </summary>
    public double Latitude { get; } = ValidateLatitude(Latitude);

    /// <summary>
    /// Gets the longitude coordinate of the geofence center.
    /// Valid range: -180 to 180 degrees.
    /// </summary>
    public double Longitude { get; } = ValidateLongitude(Longitude);

    /// <summary>
    /// Gets the radius of the circular geofence in meters.
    /// Valid range: 10 to 10,000 meters.
    /// </summary>
    public double RadiusMeters { get; } = ValidateRadius(RadiusMeters);

    /// <summary>
    /// Gets optional metadata associated with this geofence region.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; } = Metadata;

    /// <summary>
    /// Gets the creation timestamp for this geofence region.
    /// If not provided or set to default, returns the current UTC time.
    /// </summary>
    public DateTime CreatedAt { get; } = CreatedAt == default ? DateTime.UtcNow : CreatedAt;

    /// <summary>
    /// Determines whether a given location is within this geofence region.
    /// </summary>
    /// <param name="latitude">The latitude coordinate to check</param>
    /// <param name="longitude">The longitude coordinate to check</param>
    /// <returns>True if the location is within the geofence radius; otherwise, false</returns>
    public bool IsWithinRegion(double latitude, double longitude)
    {
        var location = new GpsLocation(latitude, longitude);
        var center = new GpsLocation(Latitude, Longitude);
        var distanceMeters = center.DistanceToMeters(location);
        return distanceMeters <= RadiusMeters;
    }

    /// <summary>
    /// Determines whether a given GPS location is within this geofence region.
    /// </summary>
    /// <param name="location">The GPS location to check</param>
    /// <returns>True if the location is within the geofence radius; otherwise, false</returns>
    public bool IsWithinRegion(GpsLocation location) =>
        IsWithinRegion(location.Latitude, location.Longitude);

    /// <summary>
    /// Calculates the distance in meters from the geofence center to the given coordinates.
    /// </summary>
    /// <param name="latitude">The latitude coordinate</param>
    /// <param name="longitude">The longitude coordinate</param>
    /// <returns>Distance in meters from the geofence center</returns>
    public double DistanceToCenter(double latitude, double longitude)
    {
        var location = new GpsLocation(latitude, longitude);
        var center = new GpsLocation(Latitude, Longitude);
        return center.DistanceToMeters(location);
    }

    /// <summary>
    /// Calculates the distance in meters from the geofence center to the given GPS location.
    /// </summary>
    /// <param name="location">The GPS location</param>
    /// <returns>Distance in meters from the geofence center</returns>
    public double DistanceToCenter(GpsLocation location) =>
        DistanceToCenter(location.Latitude, location.Longitude);

    private static double ValidateLatitude(double latitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90 degrees");
        return latitude;
    }

    private static double ValidateLongitude(double longitude)
    {
        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180 degrees");
        return longitude;
    }

    private static double ValidateRadius(double radiusMeters)
    {
        if (radiusMeters < 10 || radiusMeters > 10000)
            throw new ArgumentOutOfRangeException(nameof(radiusMeters), "Radius must be between 10 and 10,000 meters");
        return radiusMeters;
    }
}
