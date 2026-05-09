namespace SmartWorkz.Mobile;

/// <summary>Service for managing Bluetooth device pairing.</summary>
public interface IBluetoothPairingService
{
    /// <summary>Initiates pairing with a discovered device.</summary>
    Task<Result<bool>> PairAsync(BluetoothDevice device, CancellationToken ct = default);

    /// <summary>Removes pairing with a device.</summary>
    Task<Result<bool>> UnpairAsync(string deviceAddress, CancellationToken ct = default);

    /// <summary>Returns list of paired devices.</summary>
    Task<Result<IReadOnlyList<BluetoothDevice>>> GetPairedDevicesAsync(CancellationToken ct = default);

    /// <summary>Observable stream of pairing state changes.</summary>
    IObservable<(string DeviceAddress, bool IsPaired)> OnPairingStateChanged();
}
