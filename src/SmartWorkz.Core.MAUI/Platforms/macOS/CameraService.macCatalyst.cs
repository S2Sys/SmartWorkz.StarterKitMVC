namespace SmartWorkz.Mobile;

#if MACCATALYST

public partial class CameraService
{
    private partial async Task<bool> IsCameraAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stub implementation - will be completed in Phase 3
        await Task.Delay(0, ct);
        return false;
    }

    private partial async Task<FileResult?> TakePhotoAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stub implementation - will be completed in Phase 3
        await Task.Delay(0, ct);
        return null;
    }

    private partial async Task<FileResult?> RecordVideoAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Stub implementation - will be completed in Phase 3
        await Task.Delay(0, ct);
        return null;
    }

    private partial string GetCameraFolderPlatform()
    {
        // Stub implementation - will be completed in Phase 3
        return string.Empty;
    }
}

#endif
