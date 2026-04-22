namespace SmartWorkz.Mobile.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartWorkz.Shared;

public class BluetoothConnectionTests
{
    [Fact]
    public async Task ConnectAsync_ValidAddress_ReturnsResultType()
    {
        var mockPermissions = new Mock<IPermissionService>();
        var service = new BluetoothService(mockPermissions.Object, NullLogger<BluetoothService>.Instance);

        var result = await service.ConnectAsync("AA:BB:CC:DD:EE:FF");

        Assert.IsType<Result<bool>>(result);
    }

    [Fact]
    public async Task GetConnectionStateAsync_ReturnsNullWhenNotConnected()
    {
        var mockPermissions = new Mock<IPermissionService>();
        var service = new BluetoothService(mockPermissions.Object, NullLogger<BluetoothService>.Instance);

        var state = await service.GetConnectionStateAsync("AA:BB:CC:DD:EE:FF");

        Assert.Null(state);
    }

    [Fact]
    public void OnConnectionStateChanged_ReturnsObservable()
    {
        var mockPermissions = new Mock<IPermissionService>();
        var service = new BluetoothService(mockPermissions.Object, NullLogger<BluetoothService>.Instance);

        var observable = service.OnConnectionStateChanged();

        Assert.NotNull(observable);
    }
}
