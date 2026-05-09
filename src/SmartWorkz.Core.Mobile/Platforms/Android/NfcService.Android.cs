#if __ANDROID__
namespace SmartWorkz.Mobile;

using Android.App;
using Android.Content;
using Android.Nfc;
using Android.Nfc.Tech;

public sealed partial class NfcService
{
    private partial async Task<NfcMessage?> ReadAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var context = Android.App.Application.Context;
        if (context is null)
            return null;

        var nfcManager = context.GetSystemService(Context.NfcService) as NfcManager;
        if (nfcManager?.DefaultAdapter is null)
            return null;

        var adapter = nfcManager.DefaultAdapter;
        if (!adapter.IsEnabled)
            return null;

        try
        {
            var intent = GetNfcIntent();
            if (intent == null)
                return null;

            var tag = intent.GetParcelableExtra(Android.Nfc.NfcAdapter.ExtraTag) as Android.Nfc.Tag;
            if (tag == null)
                return null;

            var ndef = Ndef.Get(tag);
            if (ndef == null)
                return null;

            ndef.Connect();
            var message = ndef.CachedNdefMessage;
            ndef.Close();

            if (message == null)
                return null;

            var records = message.GetRecords();
            if (records == null || records.Length == 0)
                return null;

            var record = records[0];
            var payload = System.Text.Encoding.UTF8.GetString(record.GetPayload());

            // Attempt to parse as URI
            string? uri = null;
            if (System.Uri.TryCreate(payload, System.UriKind.Absolute, out var parsedUri))
                uri = payload;

            return new NfcMessage(
                record.TnfAsString(),
                payload,
                DateTime.UtcNow,
                uri,
                null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Android NFC read failed");
            return null;
        }
    }

    private Intent? GetNfcIntent()
    {
        // Note: In production, this should be called from Activity.OnNewIntent()
        // For service context, return null - this would need MainActivity integration
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
