namespace SmartWorkz.Core.Mobile;

#if __ANDROID__

public partial class BiometricService
{
    private partial async Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            // Check if device has any biometric capability by attempting to access SecureStorage
            // with biometric authentication
            return true; // Simplified: Android devices with biometric are assumed available when requested
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to check biometric availability on Android", ex);
            return false;
        }
    }

    private partial async Task<BiometricType> GetBiometricTypeAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            // Android doesn't provide a simple way to differentiate between fingerprint and face ID
            // at the MAUI level. Default to Fingerprint which is most common.
            return BiometricType.Fingerprint;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get biometric type on Android", ex);
            return BiometricType.None;
        }
    }

    private partial async Task<bool> AuthenticateAsyncPlatform(string reason, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var isAvailable = await IsAvailableAsyncPlatform(ct);
            if (!isAvailable)
            {
                throw new InvalidOperationException("Biometric authentication not available");
            }

            // For Android, biometric authentication is typically done through fingerprint
            // MAUI's SecureStorage doesn't directly expose biometric auth on Android in the public API
            // This is a placeholder that demonstrates the intent
            // In production, you'd use platform-specific code or third-party libraries

            _logger.LogWarning("Biometric authentication on Android requires platform-specific implementation");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError("Biometric authentication failed on Android", ex);
            throw;
        }
    }
}
#endif
