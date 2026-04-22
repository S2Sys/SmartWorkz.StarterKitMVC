namespace SmartWorkz.Mobile;

/// <summary>
/// Represents a discovered Bluetooth device with connection metadata.
/// </summary>
/// <param name="Address">Hardware address (MAC on Android, UUID on iOS)</param>
/// <param name="Name">User-friendly device name</param>
/// <param name="SignalStrength">Signal strength in dBm (-127 to 0)</param>
/// <param name="IsPaired">Whether device is already paired/bonded with this device</param>
/// <param name="ServiceUuids">Advertised GATT service UUIDs (null if not available)</param>
public sealed record BluetoothDevice(
    string Address,
    string Name,
    int SignalStrength,
    bool IsPaired,
    string[]? ServiceUuids = null)
{
    /// <summary>Returns Name if non-empty, otherwise returns Address</summary>
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? Address : Name;

    /// <summary>Returns true if ServiceUuids contains at least one service</summary>
    public bool HasServices => ServiceUuids?.Length > 0;
}
