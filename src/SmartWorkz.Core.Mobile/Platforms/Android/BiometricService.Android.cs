namespace SmartWorkz.Core.Mobile;

#if __ANDROID__
using Android.Hardware.Fingerprints;
using Android.Content;

public partial class BiometricService
{
    private FingerprintManager? _fingerprintManager;

    private FingerprintManager GetFingerprintManager()
    {
        if (_fingerprintManager == null)
        {
            var context = Android.App.Application.Context;
            _fingerprintManager = context?.GetSystemService(Context.FingerprintService) as FingerprintManager;
        }
        return _fingerprintManager!;
    }

    private partial async Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var fingerprintManager = GetFingerprintManager();
            if (fingerprintManager == null)
            {
                return false;
            }

            // Check if device has fingerprint hardware and enrolled fingerprints
            var hasHardware = fingerprintManager.IsHardwareDetected;
            var hasEnrolled = fingerprintManager.HasEnrolledFingerprints;

            return hasHardware && hasEnrolled;
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Biometric check failed: {ex.Message}");
            return false;
        }
    }

    private partial async Task<BiometricType> GetBiometricTypeAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var fingerprintManager = GetFingerprintManager();
            if (fingerprintManager == null)
            {
                return BiometricType.None;
            }

            // Check for fingerprint hardware
            if (fingerprintManager.IsHardwareDetected && fingerprintManager.HasEnrolledFingerprints)
            {
                return BiometricType.Fingerprint;
            }

            return BiometricType.None;
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Failed to get biometric type: {ex.Message}");
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

            // For modern Android versions, BiometricPrompt from AndroidX should be used
            // This is a simplified implementation that returns success when available
            // In production, use AndroidX.Biometric.BiometricPrompt with proper UI
            _logger.LogDebug("Biometric authentication initiated for Android");

            // Simulate successful authentication for now
            // Production code should integrate with AndroidX.Biometric.BiometricPrompt
            await Task.Delay(500, ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Biometric authentication error: {ex.Message}");
            throw;
        }
    }
}
#endif
