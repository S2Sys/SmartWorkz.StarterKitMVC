namespace SmartWorkz.Mobile;

/// <summary>
/// Represents a discovered or connected WiFi network with connectivity and signal information.
/// </summary>
/// <remarks>
/// This record defines a WiFi network with comprehensive metadata for network scanning
/// and connectivity tracking. Signal strength is measured in dBm, frequency in MHz,
/// and all validation is performed during record construction.
/// </remarks>
public sealed record WifiNetwork(
    string SSID,
    string BSSID,
    int SignalStrength,
    int Frequency,
    bool IsSecure,
    string SecurityType = "None",
    DateTime? ConnectedAt = null,
    DateTime LastSeenAt = default,
    Dictionary<string, string>? Metadata = null)
{
    /// <summary>
    /// Gets the network name (SSID) of this WiFi network.
    /// Must be between 1 and 32 characters as per WiFi standard.
    /// </summary>
    public string SSID { get; } = ValidateSSID(SSID);

    /// <summary>
    /// Gets the MAC address (BSSID) of the access point.
    /// Must be in the format "XX:XX:XX:XX:XX:XX" where X is a hexadecimal digit.
    /// </summary>
    public string BSSID { get; } = ValidateBSSID(BSSID);

    /// <summary>
    /// Gets the signal strength in dBm (decibels relative to one milliwatt).
    /// Valid range: -100 to -30 dBm, with higher values indicating stronger signals.
    /// </summary>
    public int SignalStrength { get; } = ValidateSignalStrength(SignalStrength);

    /// <summary>
    /// Gets the channel frequency in MHz.
    /// Valid ranges: 2400-2500 MHz (2.4 GHz band) or 5000-5900 MHz (5 GHz band).
    /// </summary>
    public int Frequency { get; } = ValidateFrequency(Frequency);

    /// <summary>
    /// Gets a value indicating whether this network requires authentication.
    /// </summary>
    public bool IsSecure { get; } = IsSecure;

    /// <summary>
    /// Gets the security type of this network (None, WEP, WPA, WPA2, WPA3).
    /// Defaults to "None" if not provided.
    /// </summary>
    public string SecurityType { get; } = SecurityType ?? "None";

    /// <summary>
    /// Gets the timestamp when this network was last connected to.
    /// Null if the device has never connected to this network.
    /// </summary>
    public DateTime? ConnectedAt { get; } = ConnectedAt;

    /// <summary>
    /// Gets the timestamp when this network was last detected.
    /// If not provided or set to default, returns the current UTC time.
    /// </summary>
    public DateTime LastSeenAt { get; } = LastSeenAt == default ? DateTime.UtcNow : LastSeenAt;

    /// <summary>
    /// Gets optional custom metadata associated with this WiFi network.
    /// If null is provided, an empty dictionary is used.
    /// </summary>
    public Dictionary<string, string> Metadata { get; } = Metadata ?? new Dictionary<string, string>();

    /// <summary>
    /// Determines whether the device is currently connected to this network.
    /// </summary>
    /// <returns>True if ConnectedAt has a value; otherwise, false</returns>
    public bool IsConnected() => ConnectedAt.HasValue;

    /// <summary>
    /// Gets the signal quality rating based on the current signal strength.
    /// </summary>
    /// <returns>A string representing the quality: "Excellent", "Good", "Fair", "Poor", or "NoSignal"</returns>
    /// <remarks>
    /// Quality ratings are determined as follows:
    /// - Excellent: -30 to -50 dBm
    /// - Good: -50 to -70 dBm
    /// - Fair: -70 to -85 dBm
    /// - Poor: -85 to -100 dBm
    /// - NoSignal: below -100 dBm (should not occur with validation)
    /// </remarks>
    public string SignalQuality() => SignalStrength switch
    {
        >= -50 => "Excellent",
        >= -70 => "Good",
        >= -85 => "Fair",
        >= -100 => "Poor",
        _ => "NoSignal"
    };

    /// <summary>
    /// Determines whether this network operates on the 2.4 GHz frequency band.
    /// </summary>
    /// <returns>True if the frequency is in the 2400-2500 MHz range; otherwise, false</returns>
    public bool IsBandwidth2GHz() => Frequency is >= 2400 and <= 2500;

    /// <summary>
    /// Determines whether this network operates on the 5 GHz frequency band.
    /// </summary>
    /// <returns>True if the frequency is in the 5000-5900 MHz range; otherwise, false</returns>
    public bool IsBandwidth5GHz() => Frequency is >= 5000 and <= 5900;

    /// <summary>
    /// Validates that the SSID is not null and has a length between 1 and 32 characters.
    /// </summary>
    /// <param name="ssid">The SSID to validate</param>
    /// <returns>The validated SSID</returns>
    /// <exception cref="ArgumentNullException">Thrown when SSID is null</exception>
    /// <exception cref="ArgumentException">Thrown when SSID length is not between 1 and 32 characters</exception>
    private static string ValidateSSID(string ssid)
    {
        if (ssid is null)
            throw new ArgumentNullException(nameof(ssid), "SSID cannot be null");
        if (ssid.Length < 1 || ssid.Length > 32)
            throw new ArgumentException("SSID must be between 1 and 32 characters", nameof(ssid));
        return ssid;
    }

    /// <summary>
    /// Validates that the BSSID is not null and matches the MAC address format "XX:XX:XX:XX:XX:XX".
    /// </summary>
    /// <param name="bssid">The BSSID to validate</param>
    /// <returns>The validated BSSID</returns>
    /// <exception cref="ArgumentNullException">Thrown when BSSID is null</exception>
    /// <exception cref="ArgumentException">Thrown when BSSID does not match the MAC address format</exception>
    private static string ValidateBSSID(string bssid)
    {
        if (bssid is null)
            throw new ArgumentNullException(nameof(bssid), "BSSID cannot be null");

        // MAC address format: XX:XX:XX:XX:XX:XX (case-insensitive)
        const string macPattern = @"^([0-9A-Fa-f]{2}:){5}([0-9A-Fa-f]{2})$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(bssid, macPattern))
            throw new ArgumentException("BSSID must be a valid MAC address in format XX:XX:XX:XX:XX:XX", nameof(bssid));

        return bssid;
    }

    /// <summary>
    /// Validates that the signal strength is within the valid dBm range (-100 to -30).
    /// </summary>
    /// <param name="signalStrength">The signal strength to validate</param>
    /// <returns>The validated signal strength</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when signal strength is outside the valid range</exception>
    private static int ValidateSignalStrength(int signalStrength)
    {
        if (signalStrength < -100 || signalStrength > -30)
            throw new ArgumentOutOfRangeException(nameof(signalStrength), "Signal strength must be between -100 and -30 dBm");
        return signalStrength;
    }

    /// <summary>
    /// Validates that the frequency is within valid WiFi ranges (2400-2500 or 5000-5900 MHz).
    /// </summary>
    /// <param name="frequency">The frequency to validate</param>
    /// <returns>The validated frequency</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when frequency is outside valid WiFi ranges</exception>
    private static int ValidateFrequency(int frequency)
    {
        bool is2GHz = frequency is >= 2400 and <= 2500;
        bool is5GHz = frequency is >= 5000 and <= 5900;

        if (!is2GHz && !is5GHz)
            throw new ArgumentOutOfRangeException(nameof(frequency), "Frequency must be 2400-2500 MHz (2.4 GHz) or 5000-5900 MHz (5 GHz)");

        return frequency;
    }
}
