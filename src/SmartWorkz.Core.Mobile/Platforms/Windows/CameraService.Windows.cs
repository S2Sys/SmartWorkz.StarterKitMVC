namespace SmartWorkz.Mobile;

#if WINDOWS

using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Storage;
using Microsoft.Extensions.Logging;

/// <summary>
/// Windows-specific camera service implementation using MediaCapture API.
/// Handles photo capture and video recording using native Windows camera APIs.
/// </summary>
public partial class CameraService
{
    private ILogger<CameraService>? _logger;

    /// <summary>
    /// Windows platform implementation for checking camera availability.
    /// Uses Windows.Devices.Enumeration to check for video devices.
    /// </summary>
    private partial async Task<bool> IsCameraAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var devices = await DeviceInformation.FindAllAsync(
                DeviceClass.VideoCapture).AsTask(ct);

            var available = devices != null && devices.Count > 0;
            return available;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error checking camera availability on Windows");
            return false;
        }
    }

    /// <summary>
    /// Windows platform implementation for taking a photo.
    /// Uses MediaCapture to capture a photo to a file.
    /// Returns FileResult with the path to the captured photo.
    /// </summary>
    private partial async Task<FileResult?> TakePhotoAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        MediaCapture? mediaCapture = null;

        try
        {
            var cameraFolder = GetCameraFolderPlatform();
            if (string.IsNullOrEmpty(cameraFolder))
            {
                _logger?.LogError("Failed to get camera folder on Windows");
                return null;
            }

            mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync().AsTask(ct);

            var fileName = $"photo_{Guid.NewGuid():N}.jpg";
            var cameraFolderObj = await StorageFolder.GetFolderFromPathAsync(cameraFolder);
            var photoFile = await cameraFolderObj.CreateFileAsync(fileName,
                CreationCollisionOption.ReplaceExisting).AsTask(ct);

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            {
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token))
                {
                    var photoProperties = mediaCapture.VideoDeviceController.PhotoDeviceController;
                    if (photoProperties != null)
                    {
                        var imageProperties = photoProperties.GetAvailableMediaStreamProperties(
                            Windows.Media.Capture.MediaStreamType.Photo);
                        if (imageProperties.Count > 0)
                        {
                            await photoProperties.SetMediaStreamPropertiesAsync(
                                Windows.Media.Capture.MediaStreamType.Photo,
                                imageProperties[0]).AsTask(linkedCts.Token);
                        }
                    }

                    var encodingProperties = new Windows.Media.MediaProperties.ImageEncodingProperties();
                    encodingProperties.Subtype = Windows.Media.MediaProperties.MediaEncodingSubtypes.Jpeg;

                    await mediaCapture.CapturePhotoToStorageFileAsync(
                        encodingProperties, photoFile).AsTask(linkedCts.Token);

                    return new FileResult(photoFile.Path);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger?.LogInformation("Photo capture cancelled on Windows");
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to take photo on Windows");
            return null;
        }
        finally
        {
            mediaCapture?.Dispose();
        }
    }

    /// <summary>
    /// Windows platform implementation for recording video.
    /// Uses MediaCapture to record video to a file.
    /// Returns FileResult with the path to the recorded video.
    /// </summary>
    private partial async Task<FileResult?> RecordVideoAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        MediaCapture? mediaCapture = null;

        try
        {
            var cameraFolder = GetCameraFolderPlatform();
            if (string.IsNullOrEmpty(cameraFolder))
            {
                _logger?.LogError("Failed to get camera folder on Windows");
                return null;
            }

            mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync().AsTask(ct);

            var fileName = $"video_{Guid.NewGuid():N}.mp4";
            var cameraFolderObj = await StorageFolder.GetFolderFromPathAsync(cameraFolder);
            var videoFile = await cameraFolderObj.CreateFileAsync(fileName,
                CreationCollisionOption.ReplaceExisting).AsTask(ct);

            using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(30)))
            {
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token))
                {
                    var videoProperties = new Windows.Media.MediaProperties.VideoEncodingProperties();
                    videoProperties.Codec = Windows.Media.MediaProperties.VideoCodec.H264;
                    videoProperties.Height = 1080;
                    videoProperties.Width = 1920;
                    videoProperties.Bitrate = 10000000;
                    videoProperties.FrameRate.Numerator = 30;
                    videoProperties.FrameRate.Denominator = 1;

                    await mediaCapture.StartRecordToStorageFileAsync(
                        videoProperties,
                        new Windows.Media.MediaProperties.AudioEncodingProperties(),
                        videoFile).AsTask(linkedCts.Token);

                    // Wait until cancellation (either from timeout or user)
                    await Task.Delay(Timeout.Infinite, linkedCts.Token).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger?.LogInformation("Video recording cancelled on Windows");
            try
            {
                if (mediaCapture != null)
                {
                    await mediaCapture.StopRecordAsync();
                }
            }
            catch
            {
                // Ignore stop errors on cancellation
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to record video on Windows");
            try
            {
                if (mediaCapture != null)
                {
                    await mediaCapture.StopRecordAsync();
                }
            }
            catch
            {
                // Ignore stop errors on exception
            }
            return null;
        }
        finally
        {
            mediaCapture?.Dispose();
        }
    }

    /// <summary>
    /// Gets the camera folder path for Windows.
    /// Uses LocalAppDataFolder with fallback to temp directory.
    /// </summary>
    private partial string GetCameraFolderPlatform()
    {
        try
        {
            var localFolder = ApplicationData.Current?.LocalFolder;
            if (localFolder != null)
            {
                var cameraDirPath = System.IO.Path.Combine(localFolder.Path, "Camera");
                if (!System.IO.Directory.Exists(cameraDirPath))
                {
                    System.IO.Directory.CreateDirectory(cameraDirPath);
                }
                return cameraDirPath;
            }

            // Fallback to temp directory
            var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CameraCapture");
            if (!System.IO.Directory.Exists(tempPath))
            {
                System.IO.Directory.CreateDirectory(tempPath);
            }
            return tempPath;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get camera folder path on Windows");
            return string.Empty;
        }
    }
}

#endif
