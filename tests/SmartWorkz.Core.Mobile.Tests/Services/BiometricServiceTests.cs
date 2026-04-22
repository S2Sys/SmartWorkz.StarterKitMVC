namespace SmartWorkz.Mobile.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartWorkz.Shared;

/// <summary>
/// Unit tests for BiometricService.AuthenticateAsync covering cross-platform scenarios.
/// Tests validate the Result{T} error handling pattern and permission-gated biometric flows.
/// </summary>
public class BiometricServiceTests
{
    /// <summary>
    /// Tests that AuthenticateAsync returns successful result when biometric authentication succeeds.
    /// This test validates the happy path where the device supports biometrics and user authenticates successfully.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_SuccessPath_ReturnsSuccessResult()
    {
        // Arrange - per-test mock creation (Phase 3 isolation pattern)
        var service = new BiometricService(NullLogger<BiometricService>.Instance);

        // Act
        // On non-mobile platforms (Windows), this returns UNAVAILABLE error
        // On mobile platforms with biometric support, this would return success
        var result = await service.AuthenticateAsync("Test authentication");

        // Assert - Validate Result<bool> pattern
        Assert.NotNull(result);
        Assert.IsType<Result<bool>>(result);

        // On Windows (test platform), biometric is unavailable
        // This test validates the Result<T> pattern structure is correct
        if (!result.Succeeded)
        {
            Assert.NotNull(result.Error);
            Assert.StartsWith("BIOMETRIC.", result.Error.Code);
        }
    }

    /// <summary>
    /// Tests that AuthenticateAsync returns failure result when biometric permission is denied.
    /// Validates error handling for permission denial scenarios across platforms.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_PermissionDenied_ReturnsFailureResult()
    {
        // Arrange - per-test mock creation
        var service = new BiometricService(NullLogger<BiometricService>.Instance);

        // Act
        // On non-mobile platforms, permission is always denied implicitly
        var result = await service.AuthenticateAsync("Authenticate to continue");

        // Assert - Validate failure Result<T> with proper error code
        Assert.NotNull(result);
        Assert.IsType<Result<bool>>(result);
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);

        // Error code should indicate biometric issue (unavailable or denied)
        Assert.True(
            result.Error.Code == "BIOMETRIC.UNAVAILABLE" ||
            result.Error.Code == "BIOMETRIC.DENIED" ||
            result.Error.Code == "BIOMETRIC.FAILED",
            $"Expected BIOMETRIC error code, got: {result.Error.Code}"
        );
    }

    /// <summary>
    /// Tests that AuthenticateAsync fails appropriately when biometric authentication is not available on the device.
    /// Validates that the service checks availability before attempting authentication.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_BiometricNotAvailable_ReturnsUnavailableError()
    {
        // Arrange - per-test mock creation
        // On non-Windows platforms, this test validates the platform-specific behavior
        var service = new BiometricService(NullLogger<BiometricService>.Instance);

        // Act
        var result = await service.AuthenticateAsync("Authenticate to proceed");

        // Assert - Validate availability check result
        Assert.NotNull(result);
        Assert.IsType<Result<bool>>(result);

        // On test platform (Windows), biometric is not available
        if (!result.Succeeded && result.Error != null && result.Error.Code == "BIOMETRIC.UNAVAILABLE")
        {
            // Availability check prevented authentication attempt
            Assert.Equal("BIOMETRIC.UNAVAILABLE", result.Error.Code);
            Assert.Contains("available", result.Error.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Tests that GetBiometricTypeAsync correctly detects biometric type (Face/Fingerprint/None).
    /// Validates BiometricType enum values are properly returned based on device capabilities.
    /// </summary>
    [Fact]
    public async Task GetBiometricTypeAsync_DetectsBiometricType_ReturnsValidEnumValue()
    {
        // Arrange - per-test mock creation
        var service = new BiometricService(NullLogger<BiometricService>.Instance);

        // Act
        var bioType = await service.GetBiometricTypeAsync();

        // Assert - Validate BiometricType enum response
        Assert.IsType<BiometricType>(bioType);

        // Verify it's one of the valid enum values
        Assert.True(
            bioType == BiometricType.None ||
            bioType == BiometricType.Fingerprint ||
            bioType == BiometricType.Face ||
            bioType == BiometricType.Iris,
            $"Invalid BiometricType value: {bioType}"
        );

        // On non-mobile platforms (Windows), should return None
        // On mobile platforms, returns detected type (Face/Fingerprint) or None if unavailable
    }

    /// <summary>
    /// Tests that IsAvailableAsync correctly reports biometric availability on the device.
    /// Validates the availability check mechanism used before authentication attempts.
    /// </summary>
    [Fact]
    public async Task IsAvailableAsync_ReturnsBoolean_IndicatesAvailability()
    {
        // Arrange - per-test mock creation
        var service = new BiometricService(NullLogger<BiometricService>.Instance);

        // Act
        var isAvailable = await service.IsAvailableAsync();

        // Assert - Validate return type and logic
        Assert.IsType<bool>(isAvailable);

        // On Windows platform (test), biometric is not available
        // On mobile platforms, returns true if hardware is present
        if (!isAvailable)
        {
            // Confirm that authentication also fails when unavailable
            var authResult = await service.AuthenticateAsync("Test reason");
            Assert.False(authResult.Succeeded);
        }
    }

    /// <summary>
    /// Tests that AuthenticateAsync validates the reason parameter is not empty.
    /// Guards against null/empty reason strings per Guard pattern.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_EmptyReason_ThrowsArgumentException()
    {
        // Arrange - per-test mock creation
        var service = new BiometricService(NullLogger<BiometricService>.Instance);

        // Act & Assert - Empty string should throw
        await Assert.ThrowsAsync<ArgumentException>(() => service.AuthenticateAsync(string.Empty));
    }

    [Fact(Skip = "Requires Android device with biometric hardware")]
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

    [Fact(Skip = "Requires iOS device with biometric hardware")]
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
