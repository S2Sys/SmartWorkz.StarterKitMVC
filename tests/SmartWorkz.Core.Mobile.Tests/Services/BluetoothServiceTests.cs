namespace SmartWorkz.Mobile.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartWorkz.Shared;

public class BluetoothServiceTests
{
    [Fact]
    public async Task ScanDevicesAsync_UnavailableHardware_ReturnsBtUnavailableError()
    {
        // Arrange - per-test mock creation (Phase 3 isolation pattern)
        var mockPermissions = new Mock<IPermissionService>();
        var service = new BluetoothService(mockPermissions.Object, NullLogger<BluetoothService>.Instance);

        // Act
        var result = await service.ScanDevicesAsync(TimeSpan.FromSeconds(5));

        // Assert - on non-mobile platforms, Bluetooth hardware is unavailable
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("BT.UNAVAILABLE", result.Error.Code);
    }

    [Fact]
    public async Task ScanDevicesAsync_ReturnsResultType()
    {
        // Arrange - per-test mock creation
        var mockPermissions = new Mock<IPermissionService>();
        var service = new BluetoothService(mockPermissions.Object, NullLogger<BluetoothService>.Instance);

        // Act
        var result = await service.ScanDevicesAsync(TimeSpan.FromSeconds(5));

        // Assert - Result type is always returned
        Assert.IsType<Result<IReadOnlyList<BluetoothDevice>>>(result);
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsBoolean()
    {
        // Arrange - per-test mock creation
        var mockPermissions = new Mock<IPermissionService>();
        var service = new BluetoothService(mockPermissions.Object, NullLogger<BluetoothService>.Instance);

        // Act
        var available = await service.IsAvailableAsync();

        // Assert
        Assert.IsType<bool>(available);
    }

    [Fact]
    public async Task IsEnabledAsync_ReturnsBoolean()
    {
        // Arrange - per-test mock creation
        var mockPermissions = new Mock<IPermissionService>();
        var service = new BluetoothService(mockPermissions.Object, NullLogger<BluetoothService>.Instance);

        // Act
        var enabled = await service.IsEnabledAsync();

        // Assert
        Assert.IsType<bool>(enabled);
    }

    [Fact]
    public void OnDeviceDiscovered_ReturnsObservable()
    {
        // Arrange - per-test mock creation
        var mockPermissions = new Mock<IPermissionService>();
        var service = new BluetoothService(mockPermissions.Object, NullLogger<BluetoothService>.Instance);

        // Act
        var observable = service.OnDeviceDiscovered();

        // Assert
        Assert.NotNull(observable);
    }

    [Fact]
    public void OnConnectionStateChanged_ReturnsObservable()
    {
        // Arrange - per-test mock creation
        var mockPermissions = new Mock<IPermissionService>();
        var service = new BluetoothService(mockPermissions.Object, NullLogger<BluetoothService>.Instance);

        // Act
        var observable = service.OnConnectionStateChanged();

        // Assert
        Assert.NotNull(observable);
    }

    [Fact]
    public async Task ConnectAsync_ValidAddress_ReturnsResultType()
    {
        // Arrange - per-test mock creation
        var mockPermissions = new Mock<IPermissionService>();
        var service = new BluetoothService(mockPermissions.Object, NullLogger<BluetoothService>.Instance);
        var deviceAddress = "00:11:22:33:44:55";

        // Act
        var result = await service.ConnectAsync(deviceAddress);

        // Assert
        Assert.IsType<Result<bool>>(result);
    }

    [Fact]
    public async Task DisconnectAsync_ValidAddress_ReturnsResultType()
    {
        // Arrange - per-test mock creation
        var mockPermissions = new Mock<IPermissionService>();
        var service = new BluetoothService(mockPermissions.Object, NullLogger<BluetoothService>.Instance);
        var deviceAddress = "00:11:22:33:44:55";

        // Act
        var result = await service.DisconnectAsync(deviceAddress);

        // Assert
        Assert.IsType<Result<bool>>(result);
    }

    [Fact]
    public async Task ConnectAsync_EmptyAddress_ThrowsArgumentException()
    {
        // Arrange - per-test mock creation
        var mockPermissions = new Mock<IPermissionService>();
        var service = new BluetoothService(mockPermissions.Object, NullLogger<BluetoothService>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.ConnectAsync(string.Empty));
    }

    [Fact]
    public async Task DisconnectAsync_EmptyAddress_ThrowsArgumentException()
    {
        // Arrange - per-test mock creation
        var mockPermissions = new Mock<IPermissionService>();
        var service = new BluetoothService(mockPermissions.Object, NullLogger<BluetoothService>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.DisconnectAsync(string.Empty));
    }
}
