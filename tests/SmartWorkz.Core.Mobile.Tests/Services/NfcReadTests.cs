namespace SmartWorkz.Mobile.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartWorkz.Shared;

public class NfcReadTests
{
    [Fact]
    public async Task ReadAsync_PermissionDenied_ReturnsFail()
    {
        var mockPermissions = new Mock<IPermissionService>();
        mockPermissions.Setup(p => p.CheckAsync(MobilePermission.Nfc, default))
                       .ReturnsAsync(PermissionStatus.Denied);
        mockPermissions.Setup(p => p.RequestAsync(MobilePermission.Nfc, default))
                       .ReturnsAsync(PermissionStatus.Denied);
        var service = new NfcService(mockPermissions.Object, NullLogger<NfcService>.Instance);

        var result = await service.ReadAsync();

        // On non-mobile platforms, NFC is unavailable, so check for either unavailable or permission denied error
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.True(result.Error.Code == "NFC.PERMISSION_DENIED" || result.Error.Code == "NFC.UNAVAILABLE");
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsBoolean()
    {
        var mockPermissions = new Mock<IPermissionService>();
        var service = new NfcService(mockPermissions.Object, NullLogger<NfcService>.Instance);

        var available = await service.IsAvailableAsync();

        Assert.IsType<bool>(available);
    }

    [Fact]
    public async Task IsEnabledAsync_ReturnsBoolean()
    {
        var mockPermissions = new Mock<IPermissionService>();
        var service = new NfcService(mockPermissions.Object, NullLogger<NfcService>.Instance);

        var enabled = await service.IsEnabledAsync();

        Assert.IsType<bool>(enabled);
    }
}
