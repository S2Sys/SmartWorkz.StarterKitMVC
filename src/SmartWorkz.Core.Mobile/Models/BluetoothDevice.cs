namespace SmartWorkz.Mobile;

public sealed record BluetoothDevice(
    string Address,
    string Name,
    int SignalStrength,
    bool IsPaired,
    string[]? ServiceUuids = null)
{
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? Address : Name;
    public bool HasServices => ServiceUuids?.Length > 0;
}
