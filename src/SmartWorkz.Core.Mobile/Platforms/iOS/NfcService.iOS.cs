#if __IOS__
namespace SmartWorkz.Mobile;

public sealed partial class NfcService
{
    private partial async Task<NfcMessage?> ReadAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        // Stub: Full implementation requires NFCNDEFReaderSession with delegate pattern
        // Real impl would create session and wait for tag detection
        await Task.Delay(100, ct);
        return null;  // Return null to indicate pending/not-ready
    }

    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stub: Check NFC hardware availability - iOS NFC not yet fully implemented
        return Task.FromResult(false);
    }

    private partial Task<bool> IsEnabledAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stub: Check NFC enabled state - iOS NFC not yet fully implemented
        return Task.FromResult(false);
    }
}
#endif
