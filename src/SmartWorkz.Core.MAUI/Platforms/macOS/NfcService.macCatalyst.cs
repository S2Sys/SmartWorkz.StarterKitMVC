namespace SmartWorkz.Mobile;

#if MACCATALYST

public sealed partial class NfcService
{
    private partial async Task<NfcMessage?> ReadAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stub implementation - macOS Catalyst does not support NFC
        await Task.Delay(0, ct);
        return null;
    }

    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stub implementation - macOS Catalyst does not support NFC
        return Task.FromResult(false);
    }

    private partial Task<bool> IsEnabledAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stub implementation - macOS Catalyst does not support NFC
        return Task.FromResult(false);
    }
}

#endif
