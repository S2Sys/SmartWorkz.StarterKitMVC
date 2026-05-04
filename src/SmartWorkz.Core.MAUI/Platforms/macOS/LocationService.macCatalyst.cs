namespace SmartWorkz.Mobile;

#if MACCATALYST

public partial class LocationService
{
    private partial async Task<GpsLocation?> GetCurrentLocationAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _logger.LogWarning("Location service is not fully supported on macOS");
        await Task.Delay(0, ct);
        return null;
    }

    private partial void StartTrackingPlatform(LocationTrackingOptions? options)
    {
        _logger.LogWarning("Location tracking is not supported on macOS");
    }

    private partial void StopTrackingPlatform()
    {
        // No-op on macOS
    }

    private partial async Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Task.Delay(0, ct);
        return false;
    }
}

#endif
