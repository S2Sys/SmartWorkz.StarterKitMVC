namespace SmartWorkz.Mobile;

using ILogger = Microsoft.Extensions.Logging.ILogger;

/// <summary>
/// Provides biometric authentication services (fingerprint, face recognition, etc.).
/// </summary>
#if !WINDOWS
public partial class BiometricService : IBiometricService
#else
public class BiometricService : IBiometricService
#endif
{
    private readonly ILogger _logger;

    public BiometricService(ILogger logger)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    /// <summary>
    /// Checks if biometric authentication is available on the device.
    /// </summary>
    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        _logger.LogWarning("Biometric authentication is not available on Windows platform");
        return false;
        #else
        try
        {
            return await IsAvailableAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check biometric availability");
            return false;
        }
        #endif
    }

    /// <summary>
    /// Gets the type of biometric authentication available on the device.
    /// </summary>
    public async Task<BiometricType> GetBiometricTypeAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        return BiometricType.None;
        #else
        try
        {
            return await GetBiometricTypeAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get biometric type");
            return BiometricType.None;
        }
        #endif
    }

    /// <summary>
    /// Authenticates the user using biometric authentication.
    /// </summary>
    public async Task<Result<bool>> AuthenticateAsync(string reason, CancellationToken ct = default)
    {
        Guard.NotEmpty(reason, nameof(reason));
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        return Result.Fail<bool>(new Error("BIOMETRIC.UNAVAILABLE", "Biometric authentication is not available on Windows platform"));
        #else
        var isAvailable = await IsAvailableAsync(ct);
        if (!isAvailable)
        {
            _logger.LogWarning("Biometric authentication is not available on this device");
            return Result.Fail<bool>(new Error("BIOMETRIC.UNAVAILABLE", "Biometric authentication is not available on this device"));
        }

        try
        {
            var result = await AuthenticateAsyncPlatform(reason, ct);
            return result
                ? Result.Ok(true)
                : Result.Fail<bool>(new Error("BIOMETRIC.FAILED", "Biometric authentication failed"));
        }
        catch (NotImplementedException ex)
        {
            _logger.LogWarning($"Biometric authentication not implemented: {ex.Message}");
            return Result.Fail<bool>(new Error("BIOMETRIC.UNAVAILABLE", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Biometric authentication error");

            if (ex.Message.Contains("denied", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("permission", StringComparison.OrdinalIgnoreCase))
            {
                return Result.Fail<bool>(new Error("BIOMETRIC.DENIED", "User denied biometric permission"));
            }

            return Result.Fail<bool>(new Error("BIOMETRIC.FAILED", ex.Message));
        }
        #endif
    }

    // Platform-specific partial methods (declared only for non-Windows)
    #if !WINDOWS
    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct);
    private partial Task<BiometricType> GetBiometricTypeAsyncPlatform(CancellationToken ct);
    private partial Task<bool> AuthenticateAsyncPlatform(string reason, CancellationToken ct);
    #endif
}

