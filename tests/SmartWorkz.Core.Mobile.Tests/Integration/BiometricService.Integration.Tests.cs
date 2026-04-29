namespace SmartWorkz.Mobile.Tests.Integration;

using SmartWorkz.Mobile;
using SmartWorkz.Mobile.Tests.Mocks;
using SmartWorkz.Shared;
using Xunit;

/// <summary>
/// Integration tests for BiometricService across all platforms.
/// Tests cross-platform consistency, error handling, and authentication scenarios
/// using mock provider for platform-independent testing.
/// </summary>
public class BiometricServiceIntegrationTests
{
    private readonly MockBiometricService _mockBiometric;

    public BiometricServiceIntegrationTests()
    {
        _mockBiometric = new MockBiometricService();
    }

    #region Availability Tests

    [Fact]
    public async Task IsAvailableAsync_WithAvailableBiometric_ReturnsTrue()
    {
        // Arrange
        _mockBiometric.SetAvailable(true);

        // Act
        var result = await _mockBiometric.IsAvailableAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsAvailableAsync_WithUnavailableBiometric_ReturnsFalse()
    {
        // Arrange
        _mockBiometric.SetAvailable(false);

        // Act
        var result = await _mockBiometric.IsAvailableAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsAvailableAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _mockBiometric.IsAvailableAsync(cts.Token));
    }

    #endregion

    #region Biometric Type Detection Tests

    [Fact]
    public async Task GetBiometricTypeAsync_WithAvailableFingerprint_ReturnsFingerprint()
    {
        // Arrange
        _mockBiometric.SetAvailable(true).SetBiometricType(BiometricType.Fingerprint);

        // Act
        var result = await _mockBiometric.GetBiometricTypeAsync();

        // Assert
        Assert.Equal(BiometricType.Fingerprint, result);
    }

    [Fact]
    public async Task GetBiometricTypeAsync_WithAvailableFace_ReturnsFace()
    {
        // Arrange
        _mockBiometric.SetAvailable(true).SetBiometricType(BiometricType.Face);

        // Act
        var result = await _mockBiometric.GetBiometricTypeAsync();

        // Assert
        Assert.Equal(BiometricType.Face, result);
    }

    [Fact]
    public async Task GetBiometricTypeAsync_WithAvailableIris_ReturnsIris()
    {
        // Arrange
        _mockBiometric.SetAvailable(true).SetBiometricType(BiometricType.Iris);

        // Act
        var result = await _mockBiometric.GetBiometricTypeAsync();

        // Assert
        Assert.Equal(BiometricType.Iris, result);
    }

    [Fact]
    public async Task GetBiometricTypeAsync_WithUnavailableBiometric_ReturnsNone()
    {
        // Arrange
        _mockBiometric.SetAvailable(false);

        // Act
        var result = await _mockBiometric.GetBiometricTypeAsync();

        // Assert
        Assert.Equal(BiometricType.None, result);
    }

    [Fact]
    public async Task GetBiometricTypeAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _mockBiometric.GetBiometricTypeAsync(cts.Token));
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task AuthenticateAsync_WithValidBiometric_ReturnsSuccess()
    {
        // Arrange
        var reason = "Authenticate to access your account";
        _mockBiometric.SetAvailable(true).SetAuthenticationResult(true);

        // Act
        var result = await _mockBiometric.AuthenticateAsync(reason);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Result<bool>>(result);
        Assert.True(result.Succeeded);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidBiometric_ReturnsFailure()
    {
        // Arrange
        var reason = "Authenticate to access your account";
        _mockBiometric.SetAvailable(true).SetAuthenticationResult(false);

        // Act
        var result = await _mockBiometric.AuthenticateAsync(reason);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Result<bool>>(result);
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("BIOMETRIC.FAILED", result.Error.Code);
    }

    [Fact]
    public async Task AuthenticateAsync_WithUnavailableBiometric_ReturnsBiometricUnavailableError()
    {
        // Arrange
        var reason = "Authenticate to access your account";
        _mockBiometric.SetAvailable(false);

        // Act
        var result = await _mockBiometric.AuthenticateAsync(reason);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal("BIOMETRIC.UNAVAILABLE", result.Error.Code);
    }

    [Fact]
    public async Task AuthenticateAsync_WithNullReason_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _mockBiometric.AuthenticateAsync(null!));
    }

    [Fact]
    public async Task AuthenticateAsync_WithEmptyReason_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _mockBiometric.AuthenticateAsync(string.Empty));
    }

    [Fact]
    public async Task AuthenticateAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var reason = "Authenticate to access your account";

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _mockBiometric.AuthenticateAsync(reason, cts.Token));
    }

    [Fact]
    public async Task AuthenticateAsync_WithException_ThrowsAndIsHandled()
    {
        // Arrange
        var reason = "Authenticate to access your account";
        _mockBiometric.SetThrowException(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _mockBiometric.AuthenticateAsync(reason));
    }

    [Fact]
    public async Task AuthenticateAsync_TrackingAttempts()
    {
        // Arrange
        var reason = "Test authentication";
        _mockBiometric.SetAvailable(true).SetAuthenticationResult(true);

        // Act
        await _mockBiometric.AuthenticateAsync(reason);
        await _mockBiometric.AuthenticateAsync(reason);
        await _mockBiometric.AuthenticateAsync(reason);

        // Assert
        Assert.Equal(3, _mockBiometric.AuthenticationAttempts);
    }

    #endregion

    #region Cross-Platform Consistency Tests

    [Fact]
    public async Task AllMethods_ReturnCorrectTaskTypes()
    {
        // Arrange
        var reason = "Test authentication";
        _mockBiometric.SetAvailable(true);

        // Act
        var availableTask = _mockBiometric.IsAvailableAsync();
        var typeTask = _mockBiometric.GetBiometricTypeAsync();
        var authTask = _mockBiometric.AuthenticateAsync(reason);

        // Assert
        Assert.IsAssignableFrom<Task<bool>>(availableTask);
        Assert.IsAssignableFrom<Task<BiometricType>>(typeTask);
        Assert.IsAssignableFrom<Task<Result<bool>>>(authTask);

        await Task.WhenAll(availableTask, typeTask, authTask);
    }

    [Fact]
    public async Task BiometricType_EnumHasAllValues()
    {
        // Verify all enum values exist and can be used
        var types = new[]
        {
            BiometricType.None,
            BiometricType.Fingerprint,
            BiometricType.Face,
            BiometricType.Iris,
        };

        // Test each type can be set and retrieved
        foreach (var biometricType in types)
        {
            _mockBiometric.SetBiometricType(biometricType).SetAvailable(true);
            var result = await _mockBiometric.GetBiometricTypeAsync();
            Assert.Equal(biometricType, result);
        }
    }

    [Fact]
    public async Task ErrorCodes_AreConsistent()
    {
        // Arrange
        var reason = "Test";

        // Act - Test BIOMETRIC.UNAVAILABLE
        _mockBiometric.SetAvailable(false);
        var unavailableResult = await _mockBiometric.AuthenticateAsync(reason);

        // Act - Test BIOMETRIC.FAILED
        _mockBiometric.SetAvailable(true).SetAuthenticationResult(false);
        var failedResult = await _mockBiometric.AuthenticateAsync(reason);

        // Assert
        Assert.Equal("BIOMETRIC.UNAVAILABLE", unavailableResult.Error.Code);
        Assert.Equal("BIOMETRIC.FAILED", failedResult.Error.Code);
    }

    #endregion

    #region Configuration & Reset Tests

    [Fact]
    public async Task MockBiometric_FluentConfiguration_Works()
    {
        // Arrange & Act
        var mock = new MockBiometricService()
            .SetAvailable(true)
            .SetBiometricType(BiometricType.Face)
            .SetAuthenticationResult(true);

        var available = await mock.IsAvailableAsync();
        var biometricType = await mock.GetBiometricTypeAsync();
        var authResult = await mock.AuthenticateAsync("Test");

        // Assert
        Assert.True(available);
        Assert.Equal(BiometricType.Face, biometricType);
        Assert.True(authResult.Succeeded);
    }

    [Fact]
    public async Task MockBiometric_Reset_RestoresDefaults()
    {
        // Arrange
        _mockBiometric.SetAvailable(false)
            .SetBiometricType(BiometricType.Iris)
            .SetAuthenticationResult(false);

        // Act
        _mockBiometric.Reset();
        var available = await _mockBiometric.IsAvailableAsync();
        var biometricType = await _mockBiometric.GetBiometricTypeAsync();
        var authResult = await _mockBiometric.AuthenticateAsync("Test");

        // Assert
        Assert.True(available);
        Assert.Equal(BiometricType.Fingerprint, biometricType);
        Assert.True(authResult.Succeeded);
        Assert.Equal(0, _mockBiometric.AuthenticationAttempts);
    }

    #endregion

    #region Scenario Tests

    [Fact]
    public async Task Scenario_FingerPrintAuthentication()
    {
        // Arrange - Configure mock for fingerprint scenario
        _mockBiometric
            .SetAvailable(true)
            .SetBiometricType(BiometricType.Fingerprint)
            .SetAuthenticationResult(true);

        // Act
        var isAvailable = await _mockBiometric.IsAvailableAsync();
        var biometricType = await _mockBiometric.GetBiometricTypeAsync();
        var authResult = await _mockBiometric.AuthenticateAsync("Verify your fingerprint");

        // Assert
        Assert.True(isAvailable);
        Assert.Equal(BiometricType.Fingerprint, biometricType);
        Assert.True(authResult.Succeeded);
        Assert.Equal(1, _mockBiometric.AuthenticationAttempts);
    }

    [Fact]
    public async Task Scenario_FaceRecognitionAuthentication()
    {
        // Arrange - Configure mock for face recognition scenario
        _mockBiometric
            .SetAvailable(true)
            .SetBiometricType(BiometricType.Face)
            .SetAuthenticationResult(true);

        // Act
        var isAvailable = await _mockBiometric.IsAvailableAsync();
        var biometricType = await _mockBiometric.GetBiometricTypeAsync();
        var authResult = await _mockBiometric.AuthenticateAsync("Verify your face");

        // Assert
        Assert.True(isAvailable);
        Assert.Equal(BiometricType.Face, biometricType);
        Assert.True(authResult.Succeeded);
    }

    [Fact]
    public async Task Scenario_NoDeviceBiometric()
    {
        // Arrange - Configure mock for device without biometric
        _mockBiometric.SetAvailable(false);

        // Act
        var isAvailable = await _mockBiometric.IsAvailableAsync();
        var biometricType = await _mockBiometric.GetBiometricTypeAsync();
        var authResult = await _mockBiometric.AuthenticateAsync("Authenticate");

        // Assert
        Assert.False(isAvailable);
        Assert.Equal(BiometricType.None, biometricType);
        Assert.False(authResult.Succeeded);
        Assert.Equal("BIOMETRIC.UNAVAILABLE", authResult.Error.Code);
    }

    [Fact]
    public async Task Scenario_InvalidBiometricRetry()
    {
        // Arrange - Setup scenario where user fails biometric
        _mockBiometric
            .SetAvailable(true)
            .SetBiometricType(BiometricType.Fingerprint)
            .SetAuthenticationResult(false)
            .SetErrorMessage("Fingerprint not recognized");

        // Act - First attempt fails
        var firstAttempt = await _mockBiometric.AuthenticateAsync("Try again");
        Assert.False(firstAttempt.Succeeded);
        Assert.Equal(1, _mockBiometric.AuthenticationAttempts);

        // Act - User fixes and succeeds on retry
        _mockBiometric.SetAuthenticationResult(true);
        var secondAttempt = await _mockBiometric.AuthenticateAsync("Try again");

        // Assert
        Assert.True(secondAttempt.Succeeded);
        Assert.Equal(2, _mockBiometric.AuthenticationAttempts);
    }

    #endregion
}
