namespace SmartWorkz.Mobile;

using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;

public sealed partial class BluetoothPairingService : IBluetoothPairingService
{
    private readonly IPermissionService _permissions;
    private readonly ILogger<BluetoothPairingService> _logger;
    private readonly Subject<(string DeviceAddress, bool IsPaired)> _pairingStateChanged = new();

    public BluetoothPairingService(IPermissionService permissions, ILogger<BluetoothPairingService> logger)
    {
        _permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> PairAsync(BluetoothDevice device, CancellationToken ct = default)
    {
        var status = await _permissions.RequestAsync(MobilePermission.Bluetooth, ct);
        if (status != PermissionStatus.Granted)
            return Result.Fail<bool>(new Error("BT.PERMISSION_DENIED", "Bluetooth permission denied"));

        try
        {
            var success = await PairAsyncPlatform(device, ct);
            if (success)
            {
                _pairingStateChanged.OnNext((device.Address, true));
                _logger.LogInformation("Paired with device {Device}", device.Address);
            }
            return Result.Ok(success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pairing failed for device {Device}", device.Address);
            return Result.Fail<bool>(Error.FromException(ex, "BT.PAIR_FAILED"));
        }
    }

    public async Task<Result<bool>> UnpairAsync(string deviceAddress, CancellationToken ct = default)
    {
        try
        {
            var success = await UnpairAsyncPlatform(deviceAddress, ct);
            if (success)
            {
                _pairingStateChanged.OnNext((deviceAddress, false));
                _logger.LogInformation("Unpaired device {Device}", deviceAddress);
            }
            return Result.Ok(success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unpairing failed for device {Device}", deviceAddress);
            return Result.Fail<bool>(Error.FromException(ex, "BT.UNPAIR_FAILED"));
        }
    }

    public Task<Result<IReadOnlyList<BluetoothDevice>>> GetPairedDevicesAsync(CancellationToken ct = default) =>
        GetPairedDevicesAsyncPlatform(ct);

    public IObservable<(string DeviceAddress, bool IsPaired)> OnPairingStateChanged() =>
        _pairingStateChanged.AsObservable();

#if __ANDROID__ || __IOS__
    private partial Task<bool> PairAsyncPlatform(BluetoothDevice device, CancellationToken ct);
    private partial Task<bool> UnpairAsyncPlatform(string deviceAddress, CancellationToken ct);
    private partial Task<Result<IReadOnlyList<BluetoothDevice>>> GetPairedDevicesAsyncPlatform(CancellationToken ct);
#else
    private Task<bool> PairAsyncPlatform(BluetoothDevice device, CancellationToken ct) =>
        Task.FromResult(false);

    private Task<bool> UnpairAsyncPlatform(string deviceAddress, CancellationToken ct) =>
        Task.FromResult(false);

    private Task<Result<IReadOnlyList<BluetoothDevice>>> GetPairedDevicesAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(Result.Ok((IReadOnlyList<BluetoothDevice>)new List<BluetoothDevice>()));
#endif
}
