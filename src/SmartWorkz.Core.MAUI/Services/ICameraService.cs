namespace SmartWorkz.Core.MAUI.Services;

using SmartWorkz.Mobile;

/// <summary>
/// Provides unified access to device camera for photo capture, video recording, and media picking.
/// Supports taking photos/videos and selecting from device photo library.
/// </summary>
/// <remarks>
/// Implementation varies by platform:
/// - iOS: Uses AVFoundation framework
/// - Android: Uses Camera2/CameraX API
/// - Windows: Uses MediaCapture from Windows Runtime
///
/// Always request permissions before calling camera methods.
/// </remarks>
public interface ICameraService
{
    /// <summary>
    /// Captures a single photo from device camera.
    /// </summary>
    /// <returns>Photo with image data and metadata.</returns>
    /// <exception cref="OperationCanceledException">If user cancels camera.</exception>
    Task<Photo> TakePhotoAsync();

    /// <summary>
    /// Records video from device camera with optional duration limit.
    /// </summary>
    /// <param name="maxDuration">Maximum recording duration (null for unlimited).</param>
    /// <returns>Video with file path and metadata.</returns>
    Task<Video> RecordVideoAsync(TimeSpan? maxDuration = null);

    /// <summary>
    /// Opens photo library to select multiple photos.
    /// </summary>
    /// <returns>List of selected photos.</returns>
    Task<List<Photo>> PickMultiplePhotosAsync();

    /// <summary>
    /// Opens photo library to select single photo.
    /// </summary>
    /// <returns>Selected photo or null if cancelled.</returns>
    Task<Photo?> PickSinglePhotoAsync();

    /// <summary>
    /// Gets whether device camera is available.
    /// </summary>
    Task<bool> IsCameraAvailableAsync();

    /// <summary>
    /// Gets camera permission status.
    /// </summary>
    Task<PermissionStatus> GetCameraPermissionAsync();

    /// <summary>
    /// Gets photo library permission status.
    /// </summary>
    Task<PermissionStatus> GetPhotoLibraryPermissionAsync();
}
