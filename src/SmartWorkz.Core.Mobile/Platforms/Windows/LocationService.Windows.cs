namespace SmartWorkz.Mobile;

#if WINDOWS

public partial class LocationService
{
    private partial async Task<Location?> GetCurrentLocationAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stub implementation - will be completed in Phase 3
        await Task.Delay(0, ct);
        return null;
    }

    private partial async Task StartTrackingAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stub implementation - will be completed in Phase 3
        await Task.Delay(0, ct);
    }

    private partial async Task StopTrackingAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stub implementation - will be completed in Phase 3
        await Task.Delay(0, ct);
    }

    private partial async Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stub implementation - will be completed in Phase 3
        await Task.Delay(0, ct);
        return false;
    }
}

#endif
