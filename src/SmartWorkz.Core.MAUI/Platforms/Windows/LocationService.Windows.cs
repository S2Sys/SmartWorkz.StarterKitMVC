namespace SmartWorkz.Mobile;

#if WINDOWS

public partial class LocationService
{
    private partial async Task<GpsLocation?> GetCurrentLocationAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _logger.LogWarning("Location service is not fully supported on Windows");
        await Task.Delay(0, ct);
        return null;
    }

    private partial void StartTrackingPlatform(LocationTrackingOptions? options)
    {
        _logger.LogWarning("Location tracking is not supported on Windows");
    }

    private partial void StopTrackingPlatform()
    {
        // No-op on Windows
    }

    private partial async Task<bool> IsAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Task.Delay(0, ct);
        return false;
    }
}

#endif
