namespace SmartWorkz.Mobile;

/// <summary>
/// Cross-platform media picker service for selecting photos and videos from device storage.
/// Handles permission checks and platform-specific media picker access.
/// </summary>
public interface IMediaPickerService
{
    /// <summary>
    /// Launches the media picker to select a photo.
    /// Permission must be granted before calling.
    /// </summary>
    /// <returns>FileResult with path to selected photo, or null if cancelled</returns>
    Task<FileResult?> PickPhotoAsync(CancellationToken ct = default);

    /// <summary>
    /// Launches the media picker to select a video.
    /// Permission must be granted before calling.
    /// </summary>
    /// <returns>FileResult with path to selected video, or null if cancelled</returns>
    Task<FileResult?> PickVideoAsync(CancellationToken ct = default);

    /// <summary>
    /// Launches the media picker to select multiple media files.
    /// Permission must be granted before calling.
    /// </summary>
    /// <returns>Collection of FileResult with paths to selected media, or empty if cancelled</returns>
    Task<IEnumerable<FileResult>> PickMultipleAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks if media picker is available on the device.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
}
