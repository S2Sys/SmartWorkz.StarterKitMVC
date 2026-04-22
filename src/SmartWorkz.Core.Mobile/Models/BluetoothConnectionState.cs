namespace SmartWorkz.Mobile;

/// <summary>Represents the current state of a Bluetooth device connection.</summary>
public sealed record BluetoothConnectionState(
    string DeviceAddress,
    bool IsConnected,
    DateTime ConnectedSince,
    int? Rssi = null,
    string? ServiceUuids = null)
{
    /// <summary>Time elapsed since connection was established.</summary>
    public TimeSpan ConnectionDuration => DateTime.UtcNow - ConnectedSince;

    /// <summary>Signal strength indicator (dBm range).</summary>
    public SignalStrength SignalLevel => Rssi switch
    {
        >= -50 => SignalStrength.Excellent,
        >= -60 => SignalStrength.Good,
        >= -70 => SignalStrength.Fair,
        >= -80 => SignalStrength.Weak,
        _ => SignalStrength.Poor
    };
}

/// <summary>Bluetooth signal strength levels.</summary>
public enum SignalStrength { Excellent, Good, Fair, Weak, Poor }
