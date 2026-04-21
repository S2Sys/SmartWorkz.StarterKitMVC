namespace SmartWorkz.Core.Mobile;

#if __ANDROID__
using AndroidX.Biometric;
using Android.Content;
using AndroidX.Fragment.App;
using Java.Lang;
using Java.Util.Concurrent;

public partial class BiometricService
{
    private partial async Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var biometricManager = BiometricManager.From(Android.App.Application.Context);
            var result = biometricManager.CanAuthenticate(BiometricManager.Authenticators.BiometricStrong);
            return result == BiometricManager.BiometricSuccess;
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
            // Android API doesn't expose specific biometric type at manager level
            // Return Fingerprint when available
            var isAvailable = await IsAvailableAsyncPlatform(ct);
            return isAvailable ? BiometricType.Fingerprint : BiometricType.None;
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

        var isAvailable = await IsAvailableAsyncPlatform(ct);
        if (!isAvailable)
            return false;

        var promptInfo = new BiometricPrompt.PromptInfo.Builder()
            .SetTitle("Biometric Authentication")
            .SetSubtitle("Authenticate to continue")
            .SetNegativeButtonText("Cancel")
            .Build();

        var tcs = new TaskCompletionSource<bool>();

        var callback = new BiometricAuthCallback(tcs);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var fragmentActivity = Platform.CurrentActivity as FragmentActivity;
            var executor = new MainThreadExecutor();
            var biometricPrompt = new BiometricPrompt(fragmentActivity, executor, callback);
            biometricPrompt.Authenticate(promptInfo);
        });

        ct.Register(() => tcs.TrySetCanceled());
        return await tcs.Task;
    }

    private class BiometricAuthCallback : BiometricPrompt.AuthenticationCallback
    {
        private readonly TaskCompletionSource<bool> _tcs;

        public BiometricAuthCallback(TaskCompletionSource<bool> tcs)
        {
            _tcs = tcs;
        }

        public override void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult result)
        {
            base.OnAuthenticationSucceeded(result);
            _tcs.TrySetResult(true);
        }

        public override void OnAuthenticationError(int errorCode, ICharSequence errString)
        {
            base.OnAuthenticationError(errorCode, errString);
            _tcs.TrySetResult(false);
        }

        public override void OnAuthenticationFailed()
        {
            base.OnAuthenticationFailed();
            // No-op per spec: attempt counter incremented by framework
        }
    }

    private class MainThreadExecutor : Java.Lang.Object, Java.Util.Concurrent.IExecutor
    {
        public void Execute(IRunnable runnable)
        {
            MainThread.BeginInvokeOnMainThread(() => runnable.Run());
        }
    }
}
#endif
