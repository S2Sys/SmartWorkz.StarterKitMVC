namespace SmartWorkz.Mobile;

#if IOS

public partial class MediaPickerService
{
    private partial async Task<FileResult?> PickPhotoAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stub implementation - will be completed in Phase 3
        await Task.Delay(0, ct);
        return null;
    }

    private partial async Task<FileResult?> PickVideoAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stub implementation - will be completed in Phase 3
        await Task.Delay(0, ct);
        return null;
    }

    private partial async Task<IEnumerable<FileResult>> PickMultipleAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stub implementation - will be completed in Phase 3
        await Task.Delay(0, ct);
        return Enumerable.Empty<FileResult>();
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
