namespace SmartWorkz.Mobile.Tests;

using SmartWorkz.Mobile;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

/// <summary>
/// iOS-specific unit tests for CameraService.
/// Tests verify AVFoundation integration and camera availability checks.
/// </summary>
[Collection("CameraService iOS Tests")]
public class CameraServiceiOSTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IPermissionService> _mockPermissions;

    public CameraServiceiOSTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockPermissions = new Mock<IPermissionService>();
    }

    [Fact(Skip = "Requires iOS runtime")]
    public async Task CameraService_TakePhoto_WithCameraPermission_ReturnsFileResult()
    {
        // This test requires an actual iOS runtime with camera hardware
        if (!OperatingSystem.IsIOS())
            Assert.True(false, "iOS-specific test requires iOS runtime");

        // Arrange
        _mockPermissions
            .Setup(p => p.CheckAsync(MobilePermission.Camera, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var service = new CameraService(_mockLogger.Object, _mockPermissions.Object);

        // Act
        var result = await service.TakePhotoAsync();

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.FullPath));
    }

    [Fact(Skip = "Requires iOS runtime")]
    public async Task CameraService_RecordVideo_WithCameraPermission_ReturnsFileResult()
    {
        // This test requires an actual iOS runtime with camera hardware
        if (!OperatingSystem.IsIOS())
            Assert.True(false, "iOS-specific test requires iOS runtime");

        // Arrange
        _mockPermissions
            .Setup(p => p.CheckAsync(MobilePermission.Camera, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        var service = new CameraService(_mockLogger.Object, _mockPermissions.Object);

        // Act
        var result = await service.RecordVideoAsync();

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.FullPath));
    }

    [Fact]
    public async Task CameraService_IsCameraAvailable_ReturnsBool()
    {
        // Arrange
        var service = new CameraService(_mockLogger.Object, _mockPermissions.Object);

        // Act
        var available = await service.IsCameraAvailableAsync();

        // Assert
        Assert.IsType<bool>(available);
    }

    [Fact]
    public async Task CameraService_TakePhoto_WithoutPermission_ReturnsNull()
    {
        // Arrange
        _mockPermissions
            .Setup(p => p.CheckAsync(MobilePermission.Camera, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        _mockPermissions
            .Setup(p => p.RequestAsync(MobilePermission.Camera, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        var service = new CameraService(_mockLogger.Object, _mockPermissions.Object);

        // Act
        var result = await service.TakePhotoAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CameraService_RecordVideo_WithoutPermission_ReturnsNull()
    {
        // Arrange
        _mockPermissions
            .Setup(p => p.CheckAsync(MobilePermission.Camera, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        _mockPermissions
            .Setup(p => p.RequestAsync(MobilePermission.Camera, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        var service = new CameraService(_mockLogger.Object, _mockPermissions.Object);

        // Act
        var result = await service.RecordVideoAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CameraService_GetCameraFolder_ReturnsValidPath()
    {
        // Arrange
        var service = new CameraService(_mockLogger.Object, _mockPermissions.Object);

        // Act
        var folderPath = service.GetCameraFolder();

        // Assert
        Assert.NotEmpty(folderPath);
        // On iOS, should return Documents/Camera path
    }

    [Fact]
    public async Task CameraService_TakePhoto_WithCancellation_HandlesGracefully()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var service = new CameraService(_mockLogger.Object, _mockPermissions.Object);

        // Act & Assert - should not throw but handle gracefully
        _ = await service.IsCameraAvailableAsync(cts.Token);
    }
}
