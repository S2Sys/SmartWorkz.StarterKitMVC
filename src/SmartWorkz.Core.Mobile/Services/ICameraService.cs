namespace SmartWorkz.Mobile;

/// <summary>
/// Cross-platform camera service for capturing photos and videos.
/// Handles permission checks and platform-specific camera access.
/// </summary>
public interface ICameraService
{
    /// <summary>
    /// Checks if camera hardware is available on the device.
    /// </summary>
    Task<bool> IsCameraAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Launches the camera to capture a photo.
    /// Permission must be granted before calling.
    /// </summary>
    /// <returns>FileResult with path to photo, or null if cancelled</returns>
    Task<FileResult?> TakePhotoAsync(CancellationToken ct = default);

    /// <summary>
    /// Launches the camera to record a video.
    /// Permission must be granted before calling.
    /// </summary>
    /// <returns>FileResult with path to video, or null if cancelled</returns>
    Task<FileResult?> RecordVideoAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the path where camera files are stored on this platform.
    /// </summary>
    string GetCameraFolder();
}
