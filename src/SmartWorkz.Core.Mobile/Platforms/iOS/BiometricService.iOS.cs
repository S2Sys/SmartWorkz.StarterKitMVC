namespace SmartWorkz.Core.Mobile;

#if __IOS__
using LocalAuthentication;
using Foundation;

public partial class BiometricService
{
    private partial async Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var context = new LAContext();
            NSError? authError = null;

            var canEvaluate = context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out authError);
            return canEvaluate;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to check biometric availability on iOS", ex);
            return false;
        }
    }

    private partial async Task<BiometricType> GetBiometricTypeAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var context = new LAContext();
            NSError? authError = null;

            var canEvaluate = context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out authError);
            if (!canEvaluate)
            {
                return BiometricType.None;
            }

            // Check biometric type
            if (context.BiometryType == LABiometryType.FaceId)
            {
                return BiometricType.Face;
            }
            else if (context.BiometryType == LABiometryType.TouchId)
            {
                return BiometricType.Fingerprint;
            }

            return BiometricType.None;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get biometric type on iOS", ex);
            return BiometricType.None;
        }
    }

    private partial async Task<bool> AuthenticateAsyncPlatform(string reason, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var context = new LAContext();
            NSError? authError = null;

            var canEvaluate = context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out authError);
            if (!canEvaluate)
            {
                throw new InvalidOperationException("Biometric authentication not available");
            }

            var (success, _) = await context.EvaluatePolicyAsync(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, reason);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError("Biometric authentication failed on iOS", ex);
            throw;
        }
    }
}
#endif
