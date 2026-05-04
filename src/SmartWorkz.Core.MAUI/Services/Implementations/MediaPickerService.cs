namespace SmartWorkz.Mobile;

using ILogger = Microsoft.Extensions.Logging.ILogger;

/// <summary>
/// Provides media picker services for selecting photos and videos from device storage.
/// </summary>
public partial class MediaPickerService : IMediaPickerService
{
    private readonly ILogger _logger;
    private readonly IPermissionService _permissions;

    public MediaPickerService(ILogger logger, IPermissionService permissions)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
        _permissions = Guard.NotNull(permissions, nameof(permissions));
    }

    /// <summary>
    /// Launches the media picker to select a photo.
    /// Permission must be granted before calling.
    /// </summary>
    public async Task<FileResult?> PickPhotoAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var permissionStatus = await _permissions.CheckAsync(MobilePermission.Gallery, ct);
            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await _permissions.RequestAsync(MobilePermission.Gallery, ct);
            }

            if (permissionStatus != PermissionStatus.Granted)
            {
                _logger.LogWarning("Photos permission denied");
                return null;
            }

            return await PickPhotoAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pick photo");
            return null;
        }
    }

    /// <summary>
    /// Launches the media picker to select a video.
    /// Permission must be granted before calling.
    /// </summary>
    public async Task<FileResult?> PickVideoAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var permissionStatus = await _permissions.CheckAsync(MobilePermission.Gallery, ct);
            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await _permissions.RequestAsync(MobilePermission.Gallery, ct);
            }

            if (permissionStatus != PermissionStatus.Granted)
            {
                _logger.LogWarning("Photos permission denied for video selection");
                return null;
            }

            return await PickVideoAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pick video");
            return null;
        }
    }

    /// <summary>
    /// Launches the media picker to select multiple media files.
    /// Permission must be granted before calling.
    /// </summary>
    public async Task<IEnumerable<FileResult>> PickMultipleAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var permissionStatus = await _permissions.CheckAsync(MobilePermission.Gallery, ct);
            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await _permissions.RequestAsync(MobilePermission.Gallery, ct);
            }

            if (permissionStatus != PermissionStatus.Granted)
            {
                _logger.LogWarning("Photos permission denied for multiple selection");
                return Enumerable.Empty<FileResult>();
            }

            return await PickMultipleAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pick multiple media files");
            return Enumerable.Empty<FileResult>();
        }
    }

    /// <summary>
    /// Checks if media picker is available on the device.
    /// </summary>
    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            return await IsAvailableAsyncPlatform(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check media picker availability");
            return false;
        }
    }

    // Platform-specific partial methods - implementation in platform-specific files
    private partial Task<FileResult?> PickPhotoAsyncPlatform(CancellationToken ct);
    private partial Task<FileResult?> PickVideoAsyncPlatform(CancellationToken ct);
    private partial Task<IEnumerable<FileResult>> PickMultipleAsyncPlatform(CancellationToken ct);
    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct);
}
