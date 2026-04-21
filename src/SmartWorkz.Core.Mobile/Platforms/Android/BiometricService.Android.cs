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

        // Android biometric prompt not yet implemented
        // TODO: Integrate with AndroidX.Biometric.BiometricPrompt for production
        throw new NotImplementedException("Android biometric authentication not yet implemented. Use biometric context API instead.");
    }
}
#endif
