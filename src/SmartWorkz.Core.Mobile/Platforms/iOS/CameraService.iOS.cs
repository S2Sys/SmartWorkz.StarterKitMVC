namespace SmartWorkz.Mobile;

#if IOS

using AVFoundation;
using Foundation;
using UIKit;

/// <summary>
/// iOS-specific camera service implementation using AVFoundation framework.
/// Handles photo capture, video recording using native iOS camera APIs.
/// </summary>
public partial class CameraService
{
    private AVCaptureSession? _captureSession;
    private AVCapturePhotoOutput? _photoOutput;
    private TaskCompletionSource<FileResult?>? _photoCompletionSource;

    /// <summary>
    /// Ensures capture session is initialized for camera operations.
    /// </summary>
    private AVCaptureSession EnsureCaptureSession()
    {
        if (_captureSession != null)
            return _captureSession;

        _captureSession = new AVCaptureSession();
        _captureSession.SessionPreset = AVCaptureSession.PresetPhoto;

        var device = AVCaptureDevice.DefaultDeviceWithMediaType(AVMediaType.Video);
        if (device == null)
            throw new InvalidOperationException("No camera device available");

        NSError error;
        var input = AVCaptureDeviceInput.FromDevice(device, out error);
        if (error != null)
            throw new InvalidOperationException($"Failed to initialize camera: {error.LocalizedDescription}");

        _captureSession.AddInput(input);

        _photoOutput = new AVCapturePhotoOutput();
        _captureSession.AddOutput(_photoOutput);

        return _captureSession;
    }

    /// <summary>
    /// iOS platform implementation for checking camera availability.
    /// Uses AVCaptureDevice to check for available video devices.
    /// </summary>
    private partial async Task<bool> IsCameraAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var available = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video).Length > 0;
            return await Task.FromResult(available);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking camera availability on iOS");
            return false;
        }
    }

    /// <summary>
    /// iOS platform implementation for taking a photo.
    /// Uses AVCaptureSession and AVCapturePhotoOutput for camera access.
    /// Returns FileResult with the path to the captured photo.
    /// </summary>
    private partial async Task<FileResult?> TakePhotoAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var session = EnsureCaptureSession();

            if (!session.Running)
                session.StartRunning();

            _photoCompletionSource = new TaskCompletionSource<FileResult?>();

            var photoSettings = AVCapturePhotoSettings.Create();
            var delegate_ = new PhotoCaptureDelegate(_photoCompletionSource, GetCameraFolderPlatform());

            _photoOutput?.CapturePhoto(photoSettings, delegate_);

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            {
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);
                var result = await _photoCompletionSource.Task
                    .ConfigureAwait(false)
                    .WaitAsync(linkedCts.Token);
                return result;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Photo capture cancelled");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to take photo on iOS");
            return null;
        }
        finally
        {
            if (_captureSession?.Running ?? false)
                _captureSession.StopRunning();
        }
    }

    /// <summary>
    /// iOS platform implementation for recording video.
    /// Uses AVCaptureMovieFileOutput for video recording.
    /// Returns FileResult with the path to the recorded video.
    /// </summary>
    private partial async Task<FileResult?> RecordVideoAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var session = EnsureCaptureSession();
            var fileOutput = new AVCaptureMovieFileOutput();

            session.AddOutput(fileOutput);

            if (!session.Running)
                session.StartRunning();

            var cameraFolder = GetCameraFolderPlatform();
            var fileName = $"video_{Guid.NewGuid():N}.mov";
            var filePath = Path.Combine(cameraFolder, fileName);

            var tcs = new TaskCompletionSource<FileResult?>();
            var delegate_ = new VideoRecordingDelegate(tcs, filePath);

            fileOutput.StartRecordingToOutputFile(NSUrl.FromFilename(filePath), delegate_);

            using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(30)))
            {
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);
                var result = await tcs.Task
                    .ConfigureAwait(false)
                    .WaitAsync(linkedCts.Token);
                return result;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Video recording cancelled");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record video on iOS");
            return null;
        }
        finally
        {
            if (_captureSession?.Running ?? false)
                _captureSession.StopRunning();
        }
    }

    /// <summary>
    /// Gets the camera folder path for iOS.
    /// Uses the Documents directory to store camera files.
    /// </summary>
    private partial string GetCameraFolderPlatform()
    {
        var documentsPath = NSSearchPath.GetDirectories(
            NSSearchPathDirectory.DocumentDirectory,
            NSSearchPathDomain.User)[0];

        var cameraPath = Path.Combine(documentsPath, "Camera");

        if (!Directory.Exists(cameraPath))
            Directory.CreateDirectory(cameraPath);

        return cameraPath;
    }

    /// <summary>
    /// Delegate for handling photo capture callbacks from AVCapturePhotoOutput.
    /// </summary>
    private class PhotoCaptureDelegate : AVCapturePhotoCaptureDelegate
    {
        private readonly TaskCompletionSource<FileResult?> _tcs;
        private readonly string _folderPath;

        public PhotoCaptureDelegate(TaskCompletionSource<FileResult?> tcs, string folderPath)
        {
            _tcs = tcs;
            _folderPath = folderPath;
        }

        public override void DidFinishProcessingPhoto(AVCapturePhotoOutput output, AVCapturePhoto photo, NSError? error)
        {
            if (error != null)
            {
                _tcs.SetException(new InvalidOperationException($"Photo capture failed: {error.LocalizedDescription}"));
                return;
            }

            try
            {
                var fileData = photo.FileDataRepresentation;
                if (fileData == null)
                {
                    _tcs.SetResult(null);
                    return;
                }

                var fileName = $"photo_{Guid.NewGuid():N}.jpg";
                var filePath = Path.Combine(_folderPath, fileName);

                System.IO.File.WriteAllBytes(filePath, fileData.ToArray());

                var result = new FileResult(filePath);
                _tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                _tcs.SetException(ex);
            }
        }
    }

    /// <summary>
    /// Delegate for handling video recording callbacks from AVCaptureMovieFileOutput.
    /// </summary>
    private class VideoRecordingDelegate : AVCaptureFileOutputRecordingDelegate
    {
        private readonly TaskCompletionSource<FileResult?> _tcs;
        private readonly string _filePath;

        public VideoRecordingDelegate(TaskCompletionSource<FileResult?> tcs, string filePath)
        {
            _tcs = tcs;
            _filePath = filePath;
        }

        public override void DidFinishRecording(AVCaptureFileOutput captureOutput, NSUrl outputFileUrl, NSObject[] connections, NSError? error)
        {
            if (error != null)
            {
                _tcs.SetException(new InvalidOperationException($"Video recording failed: {error.LocalizedDescription}"));
                return;
            }

            try
            {
                var result = new FileResult(_filePath);
                _tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                _tcs.SetException(ex);
            }
        }
    }
}

#endif
