#if __ANDROID__
namespace SmartWorkz.Mobile;

using Android.App;
using Android.Content;
using Android.Nfc;

public sealed partial class NfcService
{
    private partial async Task<NfcMessage?> ReadAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var context = Android.App.Application.Context;
        if (context is null)
            return null;
        var nfcManager = context?.GetSystemService(Context.NfcService) as NfcManager;
        if (nfcManager?.DefaultAdapter is null)
            return null;

        var adapter = nfcManager.DefaultAdapter;
        if (!adapter.IsEnabled)
            return null;

        // Stub: In real implementation, would use NFC intent interception via Activity
        // For now, return null to indicate pending/not-ready since we can't access Intent from service context
        await Task.Delay(100, ct); // Allow cancellation
        return null;
    }

    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var context = Android.App.Application.Context;
        var nfcManager = context?.GetSystemService(Context.NfcService) as NfcManager;
        return Task.FromResult(nfcManager?.DefaultAdapter is not null);
    }

    private partial Task<bool> IsEnabledAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var context = Android.App.Application.Context;
        var nfcManager = context?.GetSystemService(Context.NfcService) as NfcManager;
        return Task.FromResult(nfcManager?.DefaultAdapter?.IsEnabled ?? false);
    }
}
#endif
