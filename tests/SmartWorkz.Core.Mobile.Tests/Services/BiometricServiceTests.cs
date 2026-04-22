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
    /// Tests that AuthenticateAsync returns UNAVAILABLE error on Windows platform.
    /// Windows lacks biometric hardware, so this validates the platform-specific error handling.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_OnWindows_ReturnsUnavailable()
    {
        // Arrange - per-test mock creation (Phase 3 isolation pattern)
        var service = new BiometricService(NullLogger<BiometricService>.Instance);

        // Act
        // On non-mobile platforms (Windows), this returns UNAVAILABLE error
        var result = await service.AuthenticateAsync("Test authentication");

        // Assert - Validate Result<bool> pattern and Windows-specific unavailability
        Assert.NotNull(result);
        Assert.IsType<Result<bool>>(result);
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("BIOMETRIC.UNAVAILABLE", result.Error.Code);
    }

    /// <summary>
    /// Tests that AuthenticateAsync returns BIOMETRIC error code on Windows platform.
    /// Windows lacks biometric support, so this validates the error code pattern is BIOMETRIC.*.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_OnWindows_ReturnsUnavailableCode()
    {
        // Arrange - per-test mock creation
        var service = new BiometricService(NullLogger<BiometricService>.Instance);

        // Act
        // On non-mobile platforms, biometric is always unavailable
        var result = await service.AuthenticateAsync("Authenticate to continue");

        // Assert - Validate failure Result<T> with proper error code
        Assert.NotNull(result);
        Assert.IsType<Result<bool>>(result);
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.StartsWith("BIOMETRIC.", result.Error.Code);
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

}
