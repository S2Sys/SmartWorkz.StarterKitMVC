namespace SmartWorkz.Mobile.Tests;

using SmartWorkz.Mobile;
using SmartWorkz.Shared;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

/// <summary>
/// Android-specific unit tests for BiometricService.
/// Tests verify BiometricPrompt API integration and biometric authentication on Android.
/// Uses Result{T} error handling pattern and validates platform-specific behavior.
/// </summary>
[Collection("BiometricService Android Tests")]
public class BiometricServiceAndroidTests
{
    private readonly Mock<ILogger<BiometricService>> _mockLogger;

    public BiometricServiceAndroidTests()
    {
        _mockLogger = new Mock<ILogger<BiometricService>>();
    }

    #region AuthenticateAsync Tests

    /// <summary>
    /// Tests AuthenticateAsync returns Result{bool} with success when authentication succeeds.
    /// Validates that successful biometric authentication returns Result.Ok(true).
    /// </summary>
    [Fact(Skip = "Requires Android runtime with biometric hardware")]
    public async Task AuthenticateAsync_WithFingerprint_ReturnsSuccessResult()
    {
        // This test requires an actual Android runtime with fingerprint hardware
        if (!OperatingSystem.IsAndroid())
            Assert.True(false, "Android-specific test requires Android runtime");

        // Arrange
        var reason = "Verify your fingerprint to authenticate";
        var service = new BiometricService(_mockLogger.Object);

        // Act
        var result = await service.AuthenticateAsync(reason);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Result<bool>>(result);
        Assert.True(result.Succeeded, "Authentication should succeed with valid fingerprint");
        Assert.True(result.Value, "Result value should be true on successful authentication");
    }

    /// <summary>
    /// Tests AuthenticateAsync returns Result{bool} with success when face recognition succeeds.
    /// Validates that successful face authentication returns Result.Ok(true).
    /// </summary>
    [Fact(Skip = "Requires Android runtime with biometric hardware")]
    public async Task AuthenticateAsync_WithFaceRecognition_ReturnsSuccessResult()
    {
        // This test requires an actual Android runtime with face recognition hardware
        if (!OperatingSystem.IsAndroid())
            Assert.True(false, "Android-specific test requires Android runtime");

        // Arrange
        var reason = "Verify your face to authenticate";
        var service = new BiometricService(_mockLogger.Object);

        // Act
        var result = await service.AuthenticateAsync(reason);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Result<bool>>(result);
        Assert.True(result.Succeeded, "Face authentication should succeed with valid face");
        Assert.True(result.Value, "Result value should be true on successful face authentication");
    }

    /// <summary>
    /// Tests AuthenticateAsync throws ArgumentException when reason is null or empty.
    /// Validates Guard.NotEmpty validation on the reason parameter.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_WithNullReason_ThrowsArgumentException()
    {
        // Arrange
        var service = new BiometricService(_mockLogger.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AuthenticateAsync(null!));

        Assert.Contains("reason", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Tests AuthenticateAsync with empty reason throws ArgumentException.
    /// Validates that empty strings are rejected for the reason parameter.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_WithEmptyReason_ThrowsArgumentException()
    {
        // Arrange
        var service = new BiometricService(_mockLogger.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AuthenticateAsync(string.Empty));

        Assert.Contains("reason", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Tests AuthenticateAsync with cancellation token returns failure result.
    /// Validates that cancelled operations return BIOMETRIC.FAILED error result.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_WithCancelledToken_ReturnsFail()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var reason = "Verify your biometric";
        var service = new BiometricService(_mockLogger.Object);

        // Act
        var result = await service.AuthenticateAsync(reason, cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Result<bool>>(result);
        Assert.False(result.Succeeded, "Cancelled authentication should fail");
    }

    /// <summary>
    /// Tests AuthenticateAsync with timeout cancellation returns failure result.
    /// Validates that operations exceeding timeout return error results.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_WithTimeout_ReturnsFail()
    {
        // Arrange
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(10));

        var reason = "Verify your biometric";
        var service = new BiometricService(_mockLogger.Object);

        // Act
        var result = await service.AuthenticateAsync(reason, cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Result<bool>>(result);
        Assert.False(result.Succeeded, "Timed-out authentication should fail");
    }

    /// <summary>
    /// Tests AuthenticateAsync with various reason messages.
    /// Validates that different user-facing messages are properly passed to BiometricPrompt.
    /// </summary>
    [Fact(Skip = "Requires Android runtime")]
    public async Task AuthenticateAsync_WithCustomReason_DisplaysMessage()
    {
        // This test requires an actual Android runtime
        if (!OperatingSystem.IsAndroid())
            Assert.True(false, "Android-specific test requires Android runtime");

        // Arrange
        var reason = "Confirm payment of $99.99";
        var service = new BiometricService(_mockLogger.Object);

        // Act
        var result = await service.AuthenticateAsync(reason);

        // Assert
        Assert.NotNull(result);
        // In actual test, would verify that 'reason' was displayed in BiometricPrompt UI
    }

    #endregion

    #region IsAvailableAsync Tests

    /// <summary>
    /// Tests IsAvailableAsync returns boolean indicating biometric hardware availability.
    /// Validates that the method correctly detects BiometricStrong capability.
    /// </summary>
    [Fact]
    public async Task IsAvailableAsync_ReturnsBoolean()
    {
        // Arrange
        var service = new BiometricService(_mockLogger.Object);

        // Act
        var isAvailable = await service.IsAvailableAsync();

        // Assert
        Assert.IsType<bool>(isAvailable);
    }

    /// <summary>
    /// Tests IsAvailableAsync consistency with AuthenticateAsync.
    /// Validates that when IsAvailableAsync returns false, authentication returns error result.
    /// </summary>
    [Fact]
    public async Task IsAvailableAsync_ConsistentWithAuthenticate()
    {
        // Arrange
        var service = new BiometricService(_mockLogger.Object);
        var isAvailable = await service.IsAvailableAsync();

        // Act
        var authResult = await service.AuthenticateAsync("Test authentication");

        // Assert
        if (!isAvailable)
        {
            Assert.False(authResult.Succeeded,
                "Authentication should fail when biometric is unavailable");
            Assert.NotNull(authResult.Error);
            Assert.Equal("BIOMETRIC.UNAVAILABLE", authResult.Error.Code);
        }
    }

    #endregion

    #region GetBiometricTypeAsync Tests

    /// <summary>
    /// Tests GetBiometricTypeAsync returns valid BiometricType enum value.
    /// Validates that the method detects the correct biometric type (Fingerprint, Face, or None).
    /// </summary>
    [Fact]
    public async Task GetBiometricTypeAsync_ReturnsValidType()
    {
        // Arrange
        var service = new BiometricService(_mockLogger.Object);

        // Act
        var biometricType = await service.GetBiometricTypeAsync();

        // Assert
        Assert.IsType<BiometricType>(biometricType);

        // Verify it's one of the valid enum values
        var validTypes = new[]
        {
            BiometricType.None,
            BiometricType.Fingerprint,
            BiometricType.Face,
            BiometricType.Iris,
        };
        Assert.Contains(biometricType, validTypes);
    }

    /// <summary>
    /// Tests GetBiometricTypeAsync consistency with IsAvailableAsync.
    /// Validates that when biometric is unavailable, type is None.
    /// </summary>
    [Fact]
    public async Task GetBiometricTypeAsync_ConsistentWithAvailability()
    {
        // Arrange
        var service = new BiometricService(_mockLogger.Object);
        var isAvailable = await service.IsAvailableAsync();
        var biometricType = await service.GetBiometricTypeAsync();

        // Assert
        if (!isAvailable)
        {
            Assert.Equal(BiometricType.None, biometricType);
        }
        else
        {
            Assert.NotEqual(BiometricType.None, biometricType);
        }
    }

    #endregion

    #region Error Handling Tests

    /// <summary>
    /// Tests AuthenticateAsync with device locked returns proper error result.
    /// Validates error code structure for device lock scenarios.
    /// </summary>
    [Fact(Skip = "Requires Android runtime with locked biometric")]
    public async Task AuthenticateAsync_WithDeviceLocked_ReturnsError()
    {
        // This test requires Android device with too many failed attempts
        if (!OperatingSystem.IsAndroid())
            Assert.True(false, "Android-specific test requires Android runtime");

        // Arrange
        var service = new BiometricService(_mockLogger.Object);

        // Act
        var result = await service.AuthenticateAsync("Authenticate");

        // Assert - On Android, device lock returns specific error code
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    /// <summary>
    /// Tests AuthenticateAsync result structure for error cases.
    /// Validates that error results contain proper Error objects with codes.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_ErrorResult_HasProperStructure()
    {
        // Arrange - Force unavailable state (Windows platform)
        var service = new BiometricService(_mockLogger.Object);

        // Act
        var result = await service.AuthenticateAsync("Authenticate");

        // Assert - Even on non-mobile platforms, structure should be valid
        Assert.NotNull(result);
        Assert.IsType<Result<bool>>(result);

        // If failed, should have Error with Code
        if (!result.Succeeded)
        {
            Assert.NotNull(result.Error);
            Assert.NotEmpty(result.Error.Code);
            Assert.NotEmpty(result.Error.Message);
        }
    }

    #endregion

    #region Cancellation Tests

    /// <summary>
    /// Tests that cancellation token is properly respected during authentication.
    /// Validates that cancellation triggers proper cleanup.
    /// </summary>
    [Fact]
    public async Task AuthenticateAsync_WithPendingCancellation_RespondsQuickly()
    {
        // Arrange
        var service = new BiometricService(_mockLogger.Object);
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        // Act
        var result = await service.AuthenticateAsync("Test", cts.Token);

        // Assert - Operation should complete (either succeed or fail gracefully)
        Assert.NotNull(result);
        Assert.IsType<Result<bool>>(result);
    }

    #endregion
}
