namespace SmartWorkz.Mobile;

#if ANDROID

using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Provider;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

/// <summary>
/// Android-specific camera service implementation using Camera2 and Intent-based camera access.
/// Handles photo capture, video recording using native Android camera APIs.
/// </summary>
public partial class CameraService
{
    private static readonly object _lockObject = new();
    private static TaskCompletionSource<FileResult?>? _photoCompletionSource;
    private static TaskCompletionSource<FileResult?>? _videoCompletionSource;

    private const int RequestCodePhotoCapture = 1001;
    private const int RequestCodeVideoCapture = 1002;

    /// <summary>
    /// Gets the Android application context.
    /// </summary>
    private static Android.App.Application? AndroidContext =>
        Application.Context;

    /// <summary>
    /// Gets the Camera2 API CameraManager for checking camera availability.
    /// </summary>
    private static CameraManager? GetCameraManager()
    {
        var context = AndroidContext;
        if (context == null)
            return null;

        return context.GetSystemService(Context.CameraService) as CameraManager;
    }

    /// <summary>
    /// Android platform implementation for checking camera availability.
    /// Uses Camera2 API CameraManager to check for available devices.
    /// </summary>
    private partial async Task<bool> IsCameraAvailableAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var cameraManager = GetCameraManager();
            if (cameraManager == null)
                return false;

            var cameraIds = cameraManager.GetCameraIdList();
            var available = cameraIds != null && cameraIds.Length > 0;

            return await Task.FromResult(available);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error checking camera availability on Android");
            return false;
        }
    }

    /// <summary>
    /// Android platform implementation for taking a photo.
    /// Uses Intent with ACTION_IMAGE_CAPTURE to launch the system camera app.
    /// Returns FileResult with the path to the captured photo.
    /// </summary>
    private partial async Task<FileResult?> TakePhotoAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var context = AndroidContext;
            if (context is not Activity activity)
            {
                _logger?.LogError("Android application context not available or not an Activity");
                return null;
            }

            var cameraFolder = GetCameraFolderPlatform();
            var fileName = $"photo_{Guid.NewGuid():N}.jpg";
            var filePath = System.IO.Path.Combine(cameraFolder, fileName);

            var photoUri = Android.Net.Uri.FromFile(new Java.IO.File(filePath));

            var intent = new Intent(MediaStore.ActionImageCapture);
            intent.PutExtra(MediaStore.ExtraOutput, photoUri);

            lock (_lockObject)
            {
                _photoCompletionSource = new TaskCompletionSource<FileResult?>();
            }

            activity.StartActivityForResult(intent, RequestCodePhotoCapture);

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            {
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token))
                {
                    TaskCompletionSource<FileResult?>? tcs;
                    lock (_lockObject)
                    {
                        tcs = _photoCompletionSource;
                    }

                    if (tcs == null)
                        return null;

                    var result = await tcs.Task.ConfigureAwait(false).WaitAsync(linkedCts.Token);
                    return result;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger?.LogInformation("Photo capture cancelled");
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to take photo on Android");
            return null;
        }
    }

    /// <summary>
    /// Android platform implementation for recording video.
    /// Uses Intent with ACTION_VIDEO_CAPTURE to launch the system camera app.
    /// Returns FileResult with the path to the recorded video.
    /// </summary>
    private partial async Task<FileResult?> RecordVideoAsyncPlatform(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var context = AndroidContext;
            if (context is not Activity activity)
            {
                _logger?.LogError("Android application context not available or not an Activity");
                return null;
            }

            var cameraFolder = GetCameraFolderPlatform();
            var fileName = $"video_{Guid.NewGuid():N}.mp4";
            var filePath = System.IO.Path.Combine(cameraFolder, fileName);

            var videoUri = Android.Net.Uri.FromFile(new Java.IO.File(filePath));

            var intent = new Intent(MediaStore.ActionVideoCapture);
            intent.PutExtra(MediaStore.ExtraOutput, videoUri);
            intent.PutExtra(MediaStore.ExtraVideoQuality, 1);

            lock (_lockObject)
            {
                _videoCompletionSource = new TaskCompletionSource<FileResult?>();
            }

            activity.StartActivityForResult(intent, RequestCodeVideoCapture);

            using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(30)))
            {
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token))
                {
                    TaskCompletionSource<FileResult?>? tcs;
                    lock (_lockObject)
                    {
                        tcs = _videoCompletionSource;
                    }

                    if (tcs == null)
                        return null;

                    var result = await tcs.Task.ConfigureAwait(false).WaitAsync(linkedCts.Token);
                    return result;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger?.LogInformation("Video recording cancelled");
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to record video on Android");
            return null;
        }
    }

    /// <summary>
    /// Gets the camera folder path for Android.
    /// Uses app-specific external files directory with fallback to cache and app files.
    /// </summary>
    private partial string GetCameraFolderPlatform()
    {
        try
        {
            var context = AndroidContext;
            if (context == null)
                return string.Empty;

            string? cameraPath = null;

            // Try external files directory first
            var externalFilesDir = context.GetExternalFilesDir(null);
            if (externalFilesDir?.Exists() ?? false)
            {
                cameraPath = System.IO.Path.Combine(externalFilesDir.AbsolutePath, "Camera");
                var cameraDirExternal = new Java.IO.File(cameraPath);
                if (!cameraDirExternal.Exists())
                    cameraDirExternal.Mkdirs();

                if (cameraDirExternal.Exists())
                    return cameraPath;
            }

            // Fallback to cache directory
            var cacheDir = context.CacheDir;
            if (cacheDir?.Exists() ?? false)
            {
                cameraPath = System.IO.Path.Combine(cacheDir.AbsolutePath, "Camera");
                var cameraDirCache = new Java.IO.File(cameraPath);
                if (!cameraDirCache.Exists())
                    cameraDirCache.Mkdirs();

                if (cameraDirCache.Exists())
                    return cameraPath;
            }

            // Final fallback to app files directory
            var filesDir = context.FilesDir;
            if (filesDir?.Exists() ?? false)
            {
                cameraPath = System.IO.Path.Combine(filesDir.AbsolutePath, "Camera");
                var cameraDirApp = new Java.IO.File(cameraPath);
                if (!cameraDirApp.Exists())
                    cameraDirApp.Mkdirs();

                if (cameraDirApp.Exists())
                    return cameraPath;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get camera folder path on Android");
            return string.Empty;
        }
    }

    /// <summary>
    /// Handles Activity result callbacks from camera intents.
    /// Must be called from Activity.OnActivityResult() method.
    /// </summary>
    /// <param name="requestCode">Request code from Activity</param>
    /// <param name="resultCode">Result code from camera</param>
    /// <param name="data">Intent data from camera</param>
    /// <param name="outputPath">Expected output file path</param>
    public static void HandleCameraActivityResult(int requestCode, Android.App.Result resultCode, Intent? data, string? outputPath)
    {
        try
        {
            lock (_lockObject)
            {
                if (requestCode == RequestCodePhotoCapture && _photoCompletionSource != null)
                {
                    if (resultCode == Android.App.Result.Ok)
                    {
                        if (!string.IsNullOrEmpty(outputPath) && System.IO.File.Exists(outputPath))
                        {
                            _photoCompletionSource.SetResult(new FileResult(outputPath));
                        }
                        else
                        {
                            _photoCompletionSource.SetResult(null);
                        }
                    }
                    else
                    {
                        _photoCompletionSource.SetResult(null);
                    }
                    _photoCompletionSource = null;
                }
                else if (requestCode == RequestCodeVideoCapture && _videoCompletionSource != null)
                {
                    if (resultCode == Android.App.Result.Ok)
                    {
                        if (!string.IsNullOrEmpty(outputPath) && System.IO.File.Exists(outputPath))
                        {
                            _videoCompletionSource.SetResult(new FileResult(outputPath));
                        }
                        else
                        {
                            _videoCompletionSource.SetResult(null);
                        }
                    }
                    else
                    {
                        _videoCompletionSource.SetResult(null);
                    }
                    _videoCompletionSource = null;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error handling camera activity result: {ex.Message}");
        }
    }
}

#endif
