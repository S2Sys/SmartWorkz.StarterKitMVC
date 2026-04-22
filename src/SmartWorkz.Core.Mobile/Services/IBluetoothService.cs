namespace SmartWorkz.Mobile;

public interface IBluetoothService
{
    Task<Result<IReadOnlyList<BluetoothDevice>>> ScanDevicesAsync(TimeSpan timeout, CancellationToken ct = default);
    IObservable<BluetoothDevice> OnDeviceDiscovered();
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
    Task<bool> IsEnabledAsync(CancellationToken ct = default);
    Task ConnectAsync(string deviceAddress, CancellationToken ct = default);
    Task DisconnectAsync(string deviceAddress, CancellationToken ct = default);
    IObservable<bool> OnConnectionStateChanged();
}
