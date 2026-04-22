namespace SmartWorkz.Mobile;

#if MACCATALYST

public partial class LocationService
{
    private partial async Task<GpsLocation?> GetCurrentLocationAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stub implementation - will be completed in Phase 3
        await Task.Delay(0, ct);
        return null;
    }

    private partial void StartTrackingPlatform(LocationTrackingOptions? options)
    {
        // Stub implementation - will be completed in Phase 3
    }

    private partial void StopTrackingPlatform()
    {
        // Stub implementation - will be completed in Phase 3
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
