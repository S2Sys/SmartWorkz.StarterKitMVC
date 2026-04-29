namespace SmartWorkz.Core.Mobile.Tests.Mocks;

using SmartWorkz.Mobile;

/// <summary>
/// Mock implementation of ICameraService for testing without real hardware.
/// Allows configuration of success/failure scenarios and permission states.
/// </summary>
public class MockCameraProvider : ICameraService
{
    private bool _cameraAvailable = true;
    private PermissionStatus _cameraPermission = PermissionStatus.Granted;
    private PermissionStatus _photoLibraryPermission = PermissionStatus.Granted;
    private bool _throwOnPhotoCapture = false;
    private bool _throwOnVideoCapture = false;
    private bool _throwOnAvailabilityCheck = false;
    private string _testFilesDirectory = string.Empty;

    public MockCameraProvider()
    {
        _testFilesDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MockCameraTests");
        if (!System.IO.Directory.Exists(_testFilesDirectory))
            System.IO.Directory.CreateDirectory(_testFilesDirectory);
    }

    /// <summary>
    /// Configures whether camera is available.
    /// </summary>
    public MockCameraProvider SetCameraAvailable(bool available)
    {
        _cameraAvailable = available;
        return this;
    }

    /// <summary>
    /// Configures camera permission status.
    /// </summary>
    public MockCameraProvider SetCameraPermission(PermissionStatus status)
    {
        _cameraPermission = status;
        return this;
    }

    /// <summary>
    /// Configures photo library permission status.
    /// </summary>
    public MockCameraProvider SetPhotoLibraryPermission(PermissionStatus status)
    {
        _photoLibraryPermission = status;
        return this;
    }

    /// <summary>
    /// Configures whether photo capture throws exception.
    /// </summary>
    public MockCameraProvider SetThrowOnPhotoCapture(bool shouldThrow)
    {
        _throwOnPhotoCapture = shouldThrow;
        return this;
    }

    /// <summary>
    /// Configures whether video capture throws exception.
    /// </summary>
    public MockCameraProvider SetThrowOnVideoCapture(bool shouldThrow)
    {
        _throwOnVideoCapture = shouldThrow;
        return this;
    }

    /// <summary>
    /// Configures whether availability check throws exception.
    /// </summary>
    public MockCameraProvider SetThrowOnAvailabilityCheck(bool shouldThrow)
    {
        _throwOnAvailabilityCheck = shouldThrow;
        return this;
    }

    public Task<bool> IsCameraAvailableAsync(CancellationToken ct = default)
    {
        if (_throwOnAvailabilityCheck)
            throw new InvalidOperationException("Mock configured to throw on availability check");

        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_cameraAvailable);
    }

    public Task<FileResult?> TakePhotoAsync(CancellationToken ct = default)
    {
        if (_throwOnPhotoCapture)
            throw new InvalidOperationException("Mock configured to throw on photo capture");

        ct.ThrowIfCancellationRequested();

        if (_cameraPermission != PermissionStatus.Granted)
            return Task.FromResult<FileResult?>(null);

        if (!_cameraAvailable)
            return Task.FromResult<FileResult?>(null);

        // Create a minimal JPEG file for testing
        var fileName = $"photo_{Guid.NewGuid():N}.jpg";
        var filePath = System.IO.Path.Combine(_testFilesDirectory, fileName);

        // Minimal JPEG magic bytes + basic structure
        var jpegBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
        System.IO.File.WriteAllBytes(filePath, jpegBytes);

        return Task.FromResult<FileResult?>(new FileResult(filePath));
    }

    public Task<FileResult?> RecordVideoAsync(CancellationToken ct = default)
    {
        if (_throwOnVideoCapture)
            throw new InvalidOperationException("Mock configured to throw on video capture");

        ct.ThrowIfCancellationRequested();

        if (_cameraPermission != PermissionStatus.Granted)
            return Task.FromResult<FileResult?>(null);

        if (!_cameraAvailable)
            return Task.FromResult<FileResult?>(null);

        // Create a minimal MP4 file for testing
        var fileName = $"video_{Guid.NewGuid():N}.mp4";
        var filePath = System.IO.Path.Combine(_testFilesDirectory, fileName);

        // Minimal MP4 magic bytes (ftyp box)
        var mp4Bytes = new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70 };
        System.IO.File.WriteAllBytes(filePath, mp4Bytes);

        return Task.FromResult<FileResult?>(new FileResult(filePath));
    }

    public Task<PermissionStatus> GetCameraPermissionAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_cameraPermission);
    }

    public Task<PermissionStatus> GetPhotoLibraryPermissionAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_photoLibraryPermission);
    }

    /// <summary>
    /// Cleans up test files created by this mock provider.
    /// </summary>
    public void Cleanup()
    {
        try
        {
            if (System.IO.Directory.Exists(_testFilesDirectory))
            {
                foreach (var file in System.IO.Directory.GetFiles(_testFilesDirectory))
                {
                    System.IO.File.Delete(file);
                }
                System.IO.Directory.Delete(_testFilesDirectory, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    /// <summary>
    /// Resets all configuration to defaults.
    /// </summary>
    public void Reset()
    {
        _cameraAvailable = true;
        _cameraPermission = PermissionStatus.Granted;
        _photoLibraryPermission = PermissionStatus.Granted;
        _throwOnPhotoCapture = false;
        _throwOnVideoCapture = false;
        _throwOnAvailabilityCheck = false;
    }
}
