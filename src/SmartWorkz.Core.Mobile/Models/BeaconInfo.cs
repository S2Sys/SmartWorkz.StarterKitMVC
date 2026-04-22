namespace SmartWorkz.Mobile;

/// <summary>
/// Represents a detected Bluetooth Low Energy (BLE) beacon with identification and signal information.
/// </summary>
/// <remarks>
/// This record defines a BLE beacon with comprehensive metadata for beacon detection and tracking.
/// Supports iBeacon, Eddystone, AltBeacon, and custom beacon formats. Signal strength is measured in dBm,
/// and distance is calculated based on RSSI. All validation is performed during record construction.
/// </remarks>
public sealed record BeaconInfo(
    string UUID,
    int Major,
    int Minor,
    string Identifier,
    int RSSI,
    double? Distance = null,
    string BeaconType = "iBeacon",
    bool IsReachable = true,
    DateTime? LastSeenAt = null,
    Dictionary<string, string>? Metadata = null)
{
    /// <summary>
    /// Gets the unique identifier (UUID) of this beacon.
    /// Must be in UUID format: XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX
    /// </summary>
    public string UUID { get; } = ValidateUUID(UUID);

    /// <summary>
    /// Gets the major version/ID for iBeacon format.
    /// Valid range: 0 to 65535 (16-bit unsigned integer).
    /// </summary>
    public int Major { get; } = ValidateMajor(Major);

    /// <summary>
    /// Gets the minor version/ID for iBeacon format.
    /// Valid range: 0 to 65535 (16-bit unsigned integer).
    /// </summary>
    public int Minor { get; } = ValidateMinor(Minor);

    /// <summary>
    /// Gets the human-readable beacon identifier or name.
    /// Must be between 1 and 64 characters.
    /// </summary>
    public string Identifier { get; } = ValidateIdentifier(Identifier);

    /// <summary>
    /// Gets the signal strength in dBm (decibels relative to one milliwatt).
    /// Valid range: -100 to -30 dBm, with higher values indicating stronger signals.
    /// </summary>
    public int RSSI { get; } = ValidateRSSI(RSSI);

    /// <summary>
    /// Gets the calculated distance to the beacon in meters.
    /// Null if distance cannot be determined. Valid range: >= 0.
    /// </summary>
    public double? Distance { get; } = ValidateDistance(Distance);

    /// <summary>
    /// Gets the type of beacon (iBeacon, Eddystone, AltBeacon, or Other).
    /// Defaults to "iBeacon" if not provided.
    /// </summary>
    public string BeaconType { get; } = ValidateBeaconType(BeaconType);

    /// <summary>
    /// Gets a value indicating whether this beacon is currently in range.
    /// </summary>
    public bool IsReachable { get; } = IsReachable;

    /// <summary>
    /// Gets the timestamp when this beacon was last detected.
    /// If not provided or null, defaults to the current UTC time.
    /// </summary>
    public DateTime? LastSeenAt { get; } = LastSeenAt ?? DateTime.UtcNow;

    /// <summary>
    /// Gets optional custom metadata associated with this beacon.
    /// If null is provided, an empty dictionary is used.
    /// </summary>
    public Dictionary<string, string> Metadata { get; } = Metadata ?? new Dictionary<string, string>();

    /// <summary>
    /// Determines the proximity rating based on the calculated distance.
    /// </summary>
    /// <returns>A string representing proximity: "Immediate", "Near", "Far", or "Unknown"</returns>
    /// <remarks>
    /// Proximity ratings are determined as follows:
    /// - Immediate: Distance less than or equal to 1 meter
    /// - Near: Distance greater than 1 and less than or equal to 5 meters
    /// - Far: Distance greater than 5 meters
    /// - Unknown: Distance is null or unable to be determined
    /// </remarks>
    public string Proximity()
    {
        if (Distance is null)
            return "Unknown";

        return Distance switch
        {
            <= 1.0 => "Immediate",
            <= 5.0 => "Near",
            _ => "Far"
        };
    }

    /// <summary>
    /// Determines the signal quality rating based on the current RSSI value.
    /// </summary>
    /// <returns>A string representing quality: "Excellent", "Good", "Fair", "Poor", or "NoSignal"</returns>
    /// <remarks>
    /// Quality ratings are determined as follows:
    /// - Excellent: -30 to -50 dBm
    /// - Good: -50 to -70 dBm
    /// - Fair: -70 to -85 dBm
    /// - Poor: -85 to -100 dBm
    /// - NoSignal: below -100 dBm (should not occur with validation)
    /// </remarks>
    public string SignalQuality() => RSSI switch
    {
        >= -50 => "Excellent",
        >= -70 => "Good",
        >= -85 => "Fair",
        >= -100 => "Poor",
        _ => "NoSignal"
    };

    /// <summary>
    /// Determines whether this beacon is an iBeacon type.
    /// </summary>
    /// <returns>True if BeaconType is "iBeacon"; otherwise, false</returns>
    public bool IsiBeacon() => BeaconType == "iBeacon";

    /// <summary>
    /// Determines whether this beacon is an Eddystone type.
    /// </summary>
    /// <returns>True if BeaconType is "Eddystone"; otherwise, false</returns>
    public bool IsEddystone() => BeaconType == "Eddystone";

    /// <summary>
    /// Validates that the UUID is not null and matches the UUID format.
    /// </summary>
    /// <param name="uuid">The UUID to validate</param>
    /// <returns>The validated UUID</returns>
    /// <exception cref="ArgumentNullException">Thrown when UUID is null</exception>
    /// <exception cref="ArgumentException">Thrown when UUID is not in valid UUID format</exception>
    private static string ValidateUUID(string uuid)
    {
        if (uuid is null)
            throw new ArgumentNullException(nameof(uuid), "UUID cannot be null");

        // UUID format validation: XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX
        if (!Guid.TryParse(uuid, out _))
            throw new ArgumentException(
                "UUID must be in valid UUID format: XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX",
                nameof(uuid));

        return uuid;
    }

    /// <summary>
    /// Validates that the Major ID is within the valid range (0 to 65535).
    /// </summary>
    /// <param name="major">The Major ID to validate</param>
    /// <returns>The validated Major ID</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when Major is outside the valid range</exception>
    private static int ValidateMajor(int major)
    {
        if (major < 0 || major > 65535)
            throw new ArgumentOutOfRangeException(nameof(major), "Major must be between 0 and 65535");
        return major;
    }

    /// <summary>
    /// Validates that the Minor ID is within the valid range (0 to 65535).
    /// </summary>
    /// <param name="minor">The Minor ID to validate</param>
    /// <returns>The validated Minor ID</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when Minor is outside the valid range</exception>
    private static int ValidateMinor(int minor)
    {
        if (minor < 0 || minor > 65535)
            throw new ArgumentOutOfRangeException(nameof(minor), "Minor must be between 0 and 65535");
        return minor;
    }

    /// <summary>
    /// Validates that the Identifier is not null and has a length between 1 and 64 characters.
    /// </summary>
    /// <param name="identifier">The Identifier to validate</param>
    /// <returns>The validated Identifier</returns>
    /// <exception cref="ArgumentNullException">Thrown when Identifier is null</exception>
    /// <exception cref="ArgumentException">Thrown when Identifier length is not between 1 and 64 characters</exception>
    private static string ValidateIdentifier(string identifier)
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier), "Identifier cannot be null");
        if (identifier.Length < 1 || identifier.Length > 64)
            throw new ArgumentException("Identifier must be between 1 and 64 characters", nameof(identifier));
        return identifier;
    }

    /// <summary>
    /// Validates that the RSSI is within the valid dBm range (-100 to -30).
    /// </summary>
    /// <param name="rssi">The RSSI to validate</param>
    /// <returns>The validated RSSI</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when RSSI is outside the valid range</exception>
    private static int ValidateRSSI(int rssi)
    {
        if (rssi < -100 || rssi > -30)
            throw new ArgumentOutOfRangeException(nameof(rssi), "RSSI must be between -100 and -30 dBm");
        return rssi;
    }

    /// <summary>
    /// Validates that the Distance is not negative.
    /// </summary>
    /// <param name="distance">The Distance to validate</param>
    /// <returns>The validated Distance</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when Distance is negative</exception>
    private static double? ValidateDistance(double? distance)
    {
        if (distance.HasValue && distance.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(distance), "Distance must be greater than or equal to 0");
        return distance;
    }

    /// <summary>
    /// Validates that the BeaconType is one of the supported types.
    /// </summary>
    /// <param name="beaconType">The BeaconType to validate</param>
    /// <returns>The validated BeaconType</returns>
    /// <exception cref="ArgumentException">Thrown when BeaconType is not a supported type</exception>
    private static string ValidateBeaconType(string beaconType)
    {
        const string defaultType = "iBeacon";
        if (string.IsNullOrWhiteSpace(beaconType))
            return defaultType;

        var validTypes = new[] { "iBeacon", "Eddystone", "AltBeacon", "Other" };
        if (!validTypes.Contains(beaconType))
            throw new ArgumentException(
                "BeaconType must be one of: iBeacon, Eddystone, AltBeacon, or Other",
                nameof(beaconType));

        return beaconType;
    }
}
