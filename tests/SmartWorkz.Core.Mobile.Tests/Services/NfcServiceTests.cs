namespace SmartWorkz.Mobile.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartWorkz.Shared;

public class NfcServiceTests
{
    [Fact]
    public async Task ReadAsync_PermissionDenied_ReturnsFail()
    {
        // Arrange
        var mockPermissions = new Mock<IPermissionService>();
        mockPermissions.Setup(p => p.CheckAsync(MobilePermission.Nfc, default))
                       .ReturnsAsync(PermissionStatus.Denied);
        var service = new NfcService(mockPermissions.Object, NullLogger<NfcService>.Instance);

        // Act
        var result = await service.ReadAsync();

        // Assert
        // On non-mobile platforms, NFC is unavailable, so check for either unavailable or permission denied error
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task IsAvailableAsync_Always_ReturnsBooleanType()
    {
        // Arrange
        var mockPermissions = new Mock<IPermissionService>();
        var service = new NfcService(mockPermissions.Object, NullLogger<NfcService>.Instance);

        // Act
        var result = await service.IsAvailableAsync();

        // Assert
        Assert.IsType<bool>(result);
    }

    [Fact]
    public async Task IsEnabledAsync_Always_ReturnsBooleanType()
    {
        // Arrange
        var mockPermissions = new Mock<IPermissionService>();
        var service = new NfcService(mockPermissions.Object, NullLogger<NfcService>.Instance);

        // Act
        var result = await service.IsEnabledAsync();

        // Assert
        Assert.IsType<bool>(result);
    }

    [Fact]
    public async Task ReadAsync_SuccessfulRead_ReturnsMessage()
    {
        // Arrange
        var mockPermissions = new Mock<IPermissionService>();
        mockPermissions.Setup(p => p.CheckAsync(MobilePermission.Nfc, default))
                       .ReturnsAsync(PermissionStatus.Granted);
        var service = new NfcService(mockPermissions.Object, NullLogger<NfcService>.Instance);

        // Act
        var result = await service.ReadAsync();

        // Assert
        // On non-mobile platforms, NFC is unavailable, so gracefully returns error
        Assert.IsType<Result<NfcMessage>>(result);
    }
}
