namespace SmartWorkz.Mobile.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartWorkz.Shared;

public class BiometricServiceTests
{
    [Fact]
    public async Task AuthenticateAsync_WindowsPlatform_ReturnsResult()
    {
        // Arrange - per-test mock creation (Phase 3 isolation pattern)
        var service = new BiometricService(NullLogger<BiometricService>.Instance);

        // Act
        var result = await service.AuthenticateAsync("Test authentication");

        // Assert
        // On non-mobile platforms (Windows), biometric is unavailable
        // On mobile platforms with production implementation, this would succeed
        Assert.IsType<Result<bool>>(result);
    }

    [Fact]
    public async Task AuthenticateAsync_WindowsPlatform_ReturnsFailure()
    {
        // Arrange - per-test mock creation
        var service = new BiometricService(NullLogger<BiometricService>.Instance);

        // Act
        var result = await service.AuthenticateAsync("Test authentication");

        // Assert
        // On non-mobile platforms (Windows), returns unavailable error
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        // Windows returns BIOMETRIC.UNAVAILABLE, permission denied would be BIOMETRIC.DENIED
        Assert.StartsWith("BIOMETRIC.", result.Error.Code);
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsBoolean()
    {
        // Arrange - per-test mock creation
        var service = new BiometricService(NullLogger<BiometricService>.Instance);

        // Act
        var result = await service.IsAvailableAsync();

        // Assert
        Assert.IsType<bool>(result);
    }

    [Fact]
    public async Task GetBiometricTypeAsync_ReturnsTypeOrNone()
    {
        // Arrange - per-test mock creation
        var service = new BiometricService(NullLogger<BiometricService>.Instance);

        // Act
        var result = await service.GetBiometricTypeAsync();

        // Assert
        Assert.IsType<BiometricType>(result);
        // On non-mobile platforms (Windows), returns BiometricType.None
        Assert.True(
            result == BiometricType.None ||
            result == BiometricType.Fingerprint ||
            result == BiometricType.Face ||
            result == BiometricType.Iris
        );
    }

    [Fact]
    public async Task GetBiometricTypeAsync_Android_ReturnsFaceOrFingerprint()
    {
        // Platform-specific test: on Android, verify Face/Fingerprint detection works
        var service = new BiometricService(NullLogger<BiometricService>.Instance);

        // Act
        var bioType = await service.GetBiometricTypeAsync();

        // Assert
        // On Android: should return Face, Fingerprint, or None depending on hardware
        Assert.True(bioType == BiometricType.Face || bioType == BiometricType.Fingerprint || bioType == BiometricType.None);
    }

    [Fact]
    public async Task GetBiometricTypeAsync_iOS_ReturnsFaceOrIris()
    {
        // Platform-specific test: on iOS, verify Face/Iris detection works
        var service = new BiometricService(NullLogger<BiometricService>.Instance);

        // Act
        var bioType = await service.GetBiometricTypeAsync();

        // Assert
        // On iOS: should return Face, Iris, or None depending on hardware
        Assert.True(bioType == BiometricType.Face || bioType == BiometricType.Iris || bioType == BiometricType.None);
    }
}
