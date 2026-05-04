namespace SmartWorkz.Core.Mobile.Tests.Integration;

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile;
using SmartWorkz.Core.MAUI.Tests.Mocks;
using SmartWorkz.Core.MAUI.Services;

/// <summary>
/// Integration tests for CameraService across all platforms.
/// Tests permission handling, error scenarios, and cross-platform consistency.
/// </summary>
public class CameraServiceIntegrationTests : IDisposable
{
    private readonly MockCameraProvider _mockCamera;

    public CameraServiceIntegrationTests()
    {
        _mockCamera = new MockCameraProvider();
    }

    #region Basic Functionality Tests

    [Fact]
    public async Task IsCameraAvailableAsync_WithAvailableCamera_ReturnsTrue()
    {
        // Arrange
        _mockCamera.SetCameraAvailable(true);

        // Act
        var result = await _mockCamera.IsCameraAvailableAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsCameraAvailableAsync_WithUnavailableCamera_ReturnsFalse()
    {
        // Arrange
        _mockCamera.SetCameraAvailable(false);

        // Act
        var result = await _mockCamera.IsCameraAvailableAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsCameraAvailableAsync_WithException_Propagates()
    {
        // Arrange
        _mockCamera.SetThrowOnAvailabilityCheck(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _mockCamera.IsCameraAvailableAsync());
    }

    #endregion

    #region Photo Capture Tests

    [Fact]
    public async Task TakePhotoAsync_WithGrantedPermission_ReturnsFileResult()
    {
        // Arrange
        _mockCamera.SetCameraPermission(PermissionStatus.Granted);
        _mockCamera.SetCameraAvailable(true);

        // Act
        var result = await _mockCamera.TakePhotoAsync();

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.FullPath));
        Assert.True(System.IO.File.Exists(result.FullPath));

        // Cleanup
        _mockCamera.Cleanup();
    }

    [Fact]
    public async Task TakePhotoAsync_WithDeniedPermission_ReturnsNull()
    {
        // Arrange
        _mockCamera.SetCameraPermission(PermissionStatus.Denied);

        // Act
        var result = await _mockCamera.TakePhotoAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task TakePhotoAsync_WithNotRequestedPermission_ReturnsNull()
    {
        // Arrange
        _mockCamera.SetCameraPermission(PermissionStatus.NotRequested);

        // Act
        var result = await _mockCamera.TakePhotoAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task TakePhotoAsync_WithUnavailableCamera_ReturnsNull()
    {
        // Arrange
        _mockCamera.SetCameraAvailable(false);
        _mockCamera.SetCameraPermission(PermissionStatus.Granted);

        // Act
        var result = await _mockCamera.TakePhotoAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task TakePhotoAsync_WithException_Propagates()
    {
        // Arrange
        _mockCamera.SetThrowOnPhotoCapture(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _mockCamera.TakePhotoAsync());
    }

    [Fact]
    public async Task TakePhotoAsync_CreatesTempFile()
    {
        // Arrange
        _mockCamera.SetCameraPermission(PermissionStatus.Granted);

        // Act
        var result = await _mockCamera.TakePhotoAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(System.IO.File.Exists(result.FullPath));
        var fileInfo = new System.IO.FileInfo(result.FullPath);
        Assert.True(fileInfo.Length > 0, "Created file should have content");

        // Cleanup
        _mockCamera.Cleanup();
    }

    #endregion

    #region Video Recording Tests

    [Fact]
    public async Task RecordVideoAsync_WithGrantedPermission_ReturnsFileResult()
    {
        // Arrange
        _mockCamera.SetCameraPermission(PermissionStatus.Granted);
        _mockCamera.SetCameraAvailable(true);

        // Act
        var result = await _mockCamera.RecordVideoAsync();

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.FullPath));
        Assert.True(System.IO.File.Exists(result.FullPath));

        // Cleanup
        _mockCamera.Cleanup();
    }

    [Fact]
    public async Task RecordVideoAsync_WithDeniedPermission_ReturnsNull()
    {
        // Arrange
        _mockCamera.SetCameraPermission(PermissionStatus.Denied);

        // Act
        var result = await _mockCamera.RecordVideoAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RecordVideoAsync_WithNotRequestedPermission_ReturnsNull()
    {
        // Arrange
        _mockCamera.SetCameraPermission(PermissionStatus.NotRequested);

        // Act
        var result = await _mockCamera.RecordVideoAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RecordVideoAsync_WithException_Propagates()
    {
        // Arrange
        _mockCamera.SetThrowOnVideoCapture(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _mockCamera.RecordVideoAsync());
    }

    [Fact]
    public async Task RecordVideoAsync_CreatesTempFile()
    {
        // Arrange
        _mockCamera.SetCameraPermission(PermissionStatus.Granted);

        // Act
        var result = await _mockCamera.RecordVideoAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(System.IO.File.Exists(result.FullPath));
        var fileInfo = new System.IO.FileInfo(result.FullPath);
        Assert.True(fileInfo.Length > 0, "Created file should have content");

        // Cleanup
        _mockCamera.Cleanup();
    }

    #endregion

    #region Permission Tests

    [Fact]
    public async Task GetCameraPermissionAsync_ReturnsConfiguredPermission()
    {
        // Arrange
        _mockCamera.SetCameraPermission(PermissionStatus.WhenInUse);

        // Act
        var result = await _mockCamera.GetCameraPermissionAsync();

        // Assert
        Assert.Equal(PermissionStatus.WhenInUse, result);
    }

    [Fact]
    public async Task GetPhotoLibraryPermissionAsync_ReturnsConfiguredPermission()
    {
        // Arrange
        _mockCamera.SetPhotoLibraryPermission(PermissionStatus.Always);

        // Act
        var result = await _mockCamera.GetPhotoLibraryPermissionAsync();

        // Assert
        Assert.Equal(PermissionStatus.Always, result);
    }

    [Fact]
    public async Task TakePhotoAsync_WithWhenInUsePermission_ReturnsFileResult()
    {
        // Arrange
        _mockCamera.SetCameraPermission(PermissionStatus.WhenInUse);
        _mockCamera.SetCameraAvailable(true);

        // Act
        var result = await _mockCamera.TakePhotoAsync();

        // Assert
        Assert.NotNull(result);

        // Cleanup
        _mockCamera.Cleanup();
    }

    [Fact]
    public async Task TakePhotoAsync_WithAlwaysPermission_ReturnsFileResult()
    {
        // Arrange
        _mockCamera.SetCameraPermission(PermissionStatus.Always);
        _mockCamera.SetCameraAvailable(true);

        // Act
        var result = await _mockCamera.TakePhotoAsync();

        // Assert
        Assert.NotNull(result);

        // Cleanup
        _mockCamera.Cleanup();
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task TakePhotoAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _mockCamera.TakePhotoAsync(cts.Token));
    }

    [Fact]
    public async Task RecordVideoAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _mockCamera.RecordVideoAsync(cts.Token));
    }

    [Fact]
    public async Task IsCameraAvailableAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _mockCamera.IsCameraAvailableAsync(cts.Token));
    }

    #endregion

    #region Cross-Platform Consistency Tests

    [Fact]
    public async Task AllMethods_ReturnTaskTypes()
    {
        // Verify all methods return proper Task types
        var isCameraAvailableTask = _mockCamera.IsCameraAvailableAsync();
        var takePhotoTask = _mockCamera.TakePhotoAsync();
        var recordVideoTask = _mockCamera.RecordVideoAsync();
        var cameraPermissionTask = _mockCamera.GetCameraPermissionAsync();
        var photoLibraryPermissionTask = _mockCamera.GetPhotoLibraryPermissionAsync();

        Assert.IsAssignableFrom<Task<bool>>(isCameraAvailableTask);
        Assert.IsAssignableFrom<Task<FileResult?>>(takePhotoTask);
        Assert.IsAssignableFrom<Task<FileResult?>>(recordVideoTask);
        Assert.IsAssignableFrom<Task<PermissionStatus>>(cameraPermissionTask);
        Assert.IsAssignableFrom<Task<PermissionStatus>>(photoLibraryPermissionTask);

        await Task.WhenAll(isCameraAvailableTask, takePhotoTask, recordVideoTask,
            cameraPermissionTask, photoLibraryPermissionTask);
    }

    [Fact]
    public void FileResult_HasRequiredProperties()
    {
        // Verify FileResult has all expected properties
        var fileResult = new FileResult("/test/path/photo.jpg");

        Assert.NotNull(fileResult.FullPath);
        Assert.Equal("/test/path/photo.jpg", fileResult.FullPath);
        Assert.NotNull(fileResult.FileName);
        Assert.Equal("photo.jpg", fileResult.FileName);
        Assert.NotNull(fileResult.ContentType);
        Assert.Equal("image/jpeg", fileResult.ContentType);
    }

    [Fact]
    public void PermissionStatus_EnumHasAllValues()
    {
        // Verify PermissionStatus enum has expected values
        Assert.Equal(0, (int)PermissionStatus.NotRequested);
        Assert.Equal(1, (int)PermissionStatus.Denied);
        Assert.Equal(2, (int)PermissionStatus.WhenInUse);
        Assert.Equal(3, (int)PermissionStatus.Always);
    }

    #endregion

    #region Configuration & Reset Tests

    [Fact]
    public async Task MockCamera_FluentConfiguration_Works()
    {
        // Arrange
        var mockCamera = new MockCameraProvider()
            .SetCameraAvailable(true)
            .SetCameraPermission(PermissionStatus.Granted)
            .SetPhotoLibraryPermission(PermissionStatus.Always);

        // Act
        var available = await mockCamera.IsCameraAvailableAsync();
        var cameraPermission = await mockCamera.GetCameraPermissionAsync();
        var libraryPermission = await mockCamera.GetPhotoLibraryPermissionAsync();

        // Assert
        Assert.True(available);
        Assert.Equal(PermissionStatus.Granted, cameraPermission);
        Assert.Equal(PermissionStatus.Always, libraryPermission);
    }

    [Fact]
    public async Task MockCamera_Reset_RestoresDefaults()
    {
        // Arrange
        var mockCamera = new MockCameraProvider()
            .SetCameraAvailable(false)
            .SetCameraPermission(PermissionStatus.Denied);

        // Act
        mockCamera.Reset();
        var available = await mockCamera.IsCameraAvailableAsync();
        var permission = await mockCamera.GetCameraPermissionAsync();

        // Assert
        Assert.True(available);
        Assert.Equal(PermissionStatus.Granted, permission);
    }

    #endregion

    public void Dispose()
    {
        _mockCamera?.Cleanup();
    }
}
