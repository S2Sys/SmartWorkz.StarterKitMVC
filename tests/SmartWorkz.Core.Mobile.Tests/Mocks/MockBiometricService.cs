namespace SmartWorkz.Mobile.Tests.Mocks;

using SmartWorkz.Mobile;
using SmartWorkz.Shared;

/// <summary>
/// Mock biometric service for testing without requiring device biometric hardware.
/// Allows configuration of authentication success/failure, availability, and biometric type.
/// </summary>
public class MockBiometricService : IBiometricService
{
    private bool _isAvailable = true;
    private BiometricType _biometricType = BiometricType.Fingerprint;
    private bool _shouldSucceed = true;
    private bool _shouldThrow = false;
    private string _errorMessage = "Authentication failed";
    private int _authenticationAttempts = 0;

    public MockBiometricService()
    {
    }

    /// <summary>
    /// Configures whether biometric is available on the mock device.
    /// </summary>
    public MockBiometricService SetAvailable(bool available)
    {
        _isAvailable = available;
        return this;
    }

    /// <summary>
    /// Configures the type of biometric available on the mock device.
    /// </summary>
    public MockBiometricService SetBiometricType(BiometricType biometricType)
    {
        _biometricType = biometricType;
        return this;
    }

    /// <summary>
    /// Configures whether authentication should succeed or fail.
    /// </summary>
    public MockBiometricService SetAuthenticationResult(bool shouldSucceed)
    {
        _shouldSucceed = shouldSucceed;
        return this;
    }

    /// <summary>
    /// Configures whether authentication should throw an exception.
    /// </summary>
    public MockBiometricService SetThrowException(bool shouldThrow)
    {
        _shouldThrow = shouldThrow;
        return this;
    }

    /// <summary>
    /// Sets the error message returned on authentication failure.
    /// </summary>
    public MockBiometricService SetErrorMessage(string errorMessage)
    {
        _errorMessage = errorMessage;
        return this;
    }

    /// <summary>
    /// Gets the number of authentication attempts made on this mock.
    /// </summary>
    public int AuthenticationAttempts => _authenticationAttempts;

    /// <summary>
    /// Resets all configuration to defaults.
    /// </summary>
    public void Reset()
    {
        _isAvailable = true;
        _biometricType = BiometricType.Fingerprint;
        _shouldSucceed = true;
        _shouldThrow = false;
        _errorMessage = "Authentication failed";
        _authenticationAttempts = 0;
    }

    public Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_isAvailable);
    }

    public Task<BiometricType> GetBiometricTypeAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!_isAvailable)
            return Task.FromResult(BiometricType.None);

        return Task.FromResult(_biometricType);
    }

    public Task<Result<bool>> AuthenticateAsync(string reason, CancellationToken ct = default)
    {
        Guard.NotEmpty(reason, nameof(reason));
        ct.ThrowIfCancellationRequested();

        _authenticationAttempts++;

        if (_shouldThrow)
            throw new InvalidOperationException("Mock configured to throw exception on authentication");

        if (!_isAvailable)
            return Task.FromResult(Result.Fail<bool>(new Error("BIOMETRIC.UNAVAILABLE", "Biometric authentication is not available")));

        if (_shouldSucceed)
            return Task.FromResult(Result.Ok(true));

        return Task.FromResult(Result.Fail<bool>(new Error("BIOMETRIC.FAILED", _errorMessage)));
    }
}
