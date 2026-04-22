namespace SmartWorkz.Mobile;

using ILogger = Microsoft.Extensions.Logging.ILogger;

/// <summary>
/// Provides camera services for capturing photos and videos.
/// </summary>
public partial class CameraService : ICameraService
{
    private readonly ILogger _logger;
    private readonly IPermissionService _permissions;

    public CameraService(ILogger logger, IPermissionService permissions)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
        _permissions = Guard.NotNull(permissions, nameof(permissions));
    }

    /// <summary>
    /// Checks if camera hardware is available on the device.
    /// </summary>
    public async Task<bool> IsCameraAvailableAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            return await IsCameraAvailableAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check camera availability");
            return false;
        }
    }

    /// <summary>
    /// Launches the camera to capture a photo.
    /// Permission must be granted before calling.
    /// </summary>
    public async Task<FileResult?> TakePhotoAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var permissionStatus = await _permissions.CheckAsync(MobilePermission.Camera, ct);
            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await _permissions.RequestAsync(MobilePermission.Camera, ct);
            }

            if (permissionStatus != PermissionStatus.Granted)
            {
                _logger.LogWarning("Camera permission denied");
                return null;
            }

            return await TakePhotoAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to take photo");
            return null;
        }
    }

    /// <summary>
    /// Launches the camera to record a video.
    /// Permission must be granted before calling.
    /// </summary>
    public async Task<FileResult?> RecordVideoAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var permissionStatus = await _permissions.CheckAsync(MobilePermission.Camera, ct);
            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await _permissions.RequestAsync(MobilePermission.Camera, ct);
            }

            if (permissionStatus != PermissionStatus.Granted)
            {
                _logger.LogWarning("Camera permission denied for video recording");
                return null;
            }

            return await RecordVideoAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record video");
            return null;
        }
    }

    /// <summary>
    /// Gets the path where camera files are stored on this platform.
    /// </summary>
    public string GetCameraFolder()
    {
        try
        {
            return GetCameraFolderPlatform();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get camera folder path");
            return string.Empty;
        }
    }

    // Platform-specific partial methods - implementation in platform-specific files
    private partial Task<bool> IsCameraAvailableAsyncPlatform(CancellationToken ct);
    private partial Task<FileResult?> TakePhotoAsyncPlatform(CancellationToken ct);
    private partial Task<FileResult?> RecordVideoAsyncPlatform(CancellationToken ct);
    private partial string GetCameraFolderPlatform();
}
