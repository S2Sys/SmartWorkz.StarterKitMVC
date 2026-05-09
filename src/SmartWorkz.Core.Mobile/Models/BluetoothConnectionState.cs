namespace SmartWorkz.Mobile;

/// <summary>
/// Represents the current connection state of a Bluetooth device.
/// </summary>
/// <param name="DeviceAddress">Bluetooth device MAC address</param>
/// <param name="Status">Current connection status</param>
/// <param name="Timestamp">When connection was established</param>
/// <param name="RSSI">Signal strength in dBm (-30 to -100 range, higher is stronger)</param>
/// <param name="ServiceUuids">Available services on device</param>
public sealed record BluetoothConnectionState(
    string DeviceAddress,
    ConnectionStatus Status,
    DateTime Timestamp,
    int RSSI,
    IReadOnlyList<string>? ServiceUuids = null)
{
    /// <summary>
    /// Calculates signal strength from RSSI value.
    /// RSSI ranges from ~-30 (excellent, very close) to -100 (poor, far away).
    /// </summary>
    public SignalStrength SignalStrength => RSSI switch
    {
        >= -50 => SignalStrength.Excellent,
        >= -60 => SignalStrength.Good,
        >= -70 => SignalStrength.Fair,
        >= -85 => SignalStrength.Weak,
        _ => SignalStrength.Poor,
    };

    /// <summary>
    /// Time elapsed since the connection was established.
    /// </summary>
    public TimeSpan ConnectionDuration => DateTime.UtcNow - Timestamp;
}

/// <summary>
/// Enumerates possible Bluetooth connection states.
/// </summary>
public enum ConnectionStatus
{
    /// <summary>Device is connected and ready for communication.</summary>
    Connected,

    /// <summary>Device is disconnected.</summary>
    Disconnected,

    /// <summary>Connection attempt is in progress.</summary>
    Connecting,

    /// <summary>An error occurred during connection.</summary>
    Error
}

/// <summary>
/// Signal strength levels computed from RSSI (Received Signal Strength Indicator).
/// RSSI is measured in dBm (decibels relative to one milliwatt).
/// </summary>
public enum SignalStrength
{
    /// <summary>Excellent signal strength (-30 to -50 dBm)</summary>
    Excellent,

    /// <summary>Good signal strength (-51 to -60 dBm)</summary>
    Good,

    /// <summary>Fair signal strength (-61 to -70 dBm)</summary>
    Fair,

    /// <summary>Weak signal strength (-71 to -85 dBm)</summary>
    Weak,

    /// <summary>Poor signal strength (below -85 dBm)</summary>
    Poor
}
