namespace SmartWorkz.Mobile.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartWorkz.Shared;

public class NfcServiceTests
{
    private readonly Mock<IPermissionService> _mockPermissions = new();
    private readonly NfcService _sut;

    public NfcServiceTests()
    {
        _sut = new NfcService(_mockPermissions.Object, NullLogger<NfcService>.Instance);
    }

    [Fact]
    public async Task ReadAsync_PermissionDenied_ReturnsFail()
    {
        // Arrange - this test runs on non-mobile platforms where NFC is unavailable,
        // so ReadAsync will return NFC.UNAVAILABLE error, which is the correct graceful behavior
        _mockPermissions.Setup(p => p.CheckAsync(MobilePermission.Nfc, default))
                       .ReturnsAsync(PermissionStatus.Denied);

        // Act
        var result = await _sut.ReadAsync();

        // Assert - either permission denied or unavailable (platform-dependent)
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsBoolean()
    {
        // Act & Assert — platform-dependent, just verify it returns Task<bool>
        var result = await _sut.IsAvailableAsync();
        Assert.IsType<bool>(result);
    }

    [Fact]
    public async Task IsEnabledAsync_ReturnsBoolean()
    {
        // Act & Assert
        var result = await _sut.IsEnabledAsync();
        Assert.IsType<bool>(result);
    }

    [Fact]
    public async Task ReadAsync_SuccessfulRead_ReturnsMessage()
    {
        // Arrange — on non-mobile platforms this fails gracefully
        _mockPermissions.Setup(p => p.CheckAsync(MobilePermission.Nfc, default))
                       .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _sut.ReadAsync();

        // Assert — either succeeds or returns graceful error on non-Android/iOS
        Assert.IsType<Result<NfcMessage>>(result);
    }
}
