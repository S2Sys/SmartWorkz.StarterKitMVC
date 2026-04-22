#if __MACOS__
namespace SmartWorkz.Mobile;

public sealed partial class BluetoothPairingService
{
    private partial async Task<bool> PairAsyncPlatform(BluetoothDevice device, CancellationToken ct)
    {
        await Task.CompletedTask;
        return false;
    }

    private partial async Task<bool> UnpairAsyncPlatform(string deviceAddress, CancellationToken ct)
    {
        await Task.CompletedTask;
        return false;
    }

    private partial Task<Result<IReadOnlyList<BluetoothDevice>>> GetPairedDevicesAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(Result.Ok((IReadOnlyList<BluetoothDevice>)new List<BluetoothDevice>()));
}
#endif
