#if __IOS__
namespace SmartWorkz.Mobile;

using CoreNFC;
using Foundation;

public sealed partial class NfcService
{
    private NFCNDEFReaderSession? _nfcSession;
    private TaskCompletionSource<NfcMessage>? _readTcs;

    private partial async Task<NfcMessage?> ReadAsyncPlatform(CancellationToken ct)
    {
        if (!NFCNDEFReaderSession.ReadingAvailable)
            throw new InvalidOperationException("NFC reading not available on this device");

        try
        {
            _readTcs = new TaskCompletionSource<NfcMessage>();
            _nfcSession = new NFCNDEFReaderSession(this, null, true);
            _nfcSession.BeginSession();

            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(30));

            var message = await _readTcs.Task.ConfigureAwait(false);
            return message;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "iOS NFC read failed");
            throw;
        }
        finally
        {
            _nfcSession?.InvalidateSession();
            _nfcSession = null;
        }
    }

    public void DidDetect(NFCNDEFReaderSession session, NFCNdefMessage[] messages)
    {
        if (messages.Length == 0) return;
        var firstRecord = messages[0].Records[0];
        var payload = firstRecord.Payload.ToString();
        var message = new NfcMessage(firstRecord.TypeNameFormat.ToString(), payload, DateTime.UtcNow,
            Uri.TryCreate(payload, UriKind.Absolute, out var uri) ? payload : null, null);
        _readTcs?.SetResult(message);
    }

    public void DidInvalidate(NFCNDEFReaderSession session, NSError error)
    {
        if (error != null) _readTcs?.SetException(new InvalidOperationException(error.LocalizedDescription));
    }

    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(NFCNDEFReaderSession.ReadingAvailable);

    private partial Task<bool> IsEnabledAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(NFCNDEFReaderSession.ReadingAvailable);
}
#endif
