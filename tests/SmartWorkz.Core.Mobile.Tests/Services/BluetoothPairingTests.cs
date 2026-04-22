namespace SmartWorkz.Mobile.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartWorkz.Shared;

public class BluetoothPairingTests
{
    [Fact]
    public async Task PairAsync_PermissionDenied_ReturnsFail()
    {
        var mockPermissions = new Mock<IPermissionService>();
        mockPermissions.Setup(p => p.RequestAsync(MobilePermission.Bluetooth, default))
                       .ReturnsAsync(PermissionStatus.Denied);
        var service = new BluetoothPairingService(mockPermissions.Object, NullLogger<BluetoothPairingService>.Instance);
        var device = new BluetoothDevice("AA:BB:CC:DD:EE:FF", "Test Device", -60, false, null);

        var result = await service.PairAsync(device);

        Assert.False(result.Succeeded);
        Assert.Equal("BT.PERMISSION_DENIED", result.Error?.Code);
    }

    [Fact]
    public async Task GetPairedDevicesAsync_ReturnsList()
    {
        var mockPermissions = new Mock<IPermissionService>();
        var service = new BluetoothPairingService(mockPermissions.Object, NullLogger<BluetoothPairingService>.Instance);

        var result = await service.GetPairedDevicesAsync();

        Assert.IsType<Result<IReadOnlyList<BluetoothDevice>>>(result);
    }

    [Fact]
    public void OnPairingStateChanged_ReturnsObservable()
    {
        var mockPermissions = new Mock<IPermissionService>();
        var service = new BluetoothPairingService(mockPermissions.Object, NullLogger<BluetoothPairingService>.Instance);

        var observable = service.OnPairingStateChanged();

        Assert.NotNull(observable);
    }
}
