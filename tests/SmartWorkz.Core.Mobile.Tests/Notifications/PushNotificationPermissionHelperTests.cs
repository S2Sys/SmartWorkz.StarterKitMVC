using Moq;
using SmartWorkz.Mobile.Notifications;

namespace SmartWorkz.Mobile.Tests.Notifications;

/// <summary>
/// Tests for the PushNotificationPermissionHelper class.
/// Validates permission checking, requesting, and caching behavior across platforms.
/// </summary>
public class PushNotificationPermissionHelperTests
{
    #region Permission Check Tests

    [Fact]
    public async Task HasPermissionAsync_WhenHandlerReturnsTrue_ReturnsTrue()
    {
        var permissionHelper = new PushNotificationPermissionHelper();
        var mockHandler = new Mock<IPushNotificationHandler>();
        mockHandler.Setup(h => h.HasPermissionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await permissionHelper.HasPermissionAsync(mockHandler.Object);

        Assert.True(result);
    }

    [Fact]
    public async Task HasPermissionAsync_WhenHandlerReturnsFalse_ReturnsFalse()
    {
        var permissionHelper = new PushNotificationPermissionHelper();
        var mockHandler = new Mock<IPushNotificationHandler>();
        mockHandler.Setup(h => h.HasPermissionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await permissionHelper.HasPermissionAsync(mockHandler.Object);

        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_CachesResult_DoesNotCallHandlerTwice()
    {
        var permissionHelper = new PushNotificationPermissionHelper();
        var mockHandler = new Mock<IPushNotificationHandler>();
        mockHandler.Setup(h => h.HasPermissionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await permissionHelper.HasPermissionAsync(mockHandler.Object);
        await permissionHelper.HasPermissionAsync(mockHandler.Object);

        mockHandler.Verify(h => h.HasPermissionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HasPermissionAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        var permissionHelper = new PushNotificationPermissionHelper();
        var mockHandler = new Mock<IPushNotificationHandler>();
        mockHandler.Setup(h => h.HasPermissionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await permissionHelper.HasPermissionAsync(mockHandler.Object, new CancellationToken(true)));
    }

    #endregion

    #region Permission Request Tests

    [Fact]
    public async Task RequestPermissionsAsync_WhenGranted_ReturnsTrue()
    {
        var permissionHelper = new PushNotificationPermissionHelper();
        var mockHandler = new Mock<IPushNotificationHandler>();
        mockHandler.Setup(h => h.RequestPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await permissionHelper.RequestPermissionsAsync(mockHandler.Object);

        Assert.True(result);
    }

    [Fact]
    public async Task RequestPermissionsAsync_WhenDenied_ReturnsFalse()
    {
        var permissionHelper = new PushNotificationPermissionHelper();
        var mockHandler = new Mock<IPushNotificationHandler>();
        mockHandler.Setup(h => h.RequestPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await permissionHelper.RequestPermissionsAsync(mockHandler.Object);

        Assert.False(result);
    }

    [Fact]
    public async Task RequestPermissionsAsync_UpdatesCache_WhenGranted()
    {
        var permissionHelper = new PushNotificationPermissionHelper();
        var mockHandler = new Mock<IPushNotificationHandler>();
        mockHandler.Setup(h => h.RequestPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mockHandler.Setup(h => h.HasPermissionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await permissionHelper.RequestPermissionsAsync(mockHandler.Object);
        var cached = await permissionHelper.HasPermissionAsync(mockHandler.Object);

        Assert.True(cached);
        mockHandler.Verify(h => h.HasPermissionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RequestPermissionsAsync_ClearsCacheOnDenial()
    {
        var permissionHelper = new PushNotificationPermissionHelper();
        var mockHandler = new Mock<IPushNotificationHandler>();

        // First grant permissions
        mockHandler.Setup(h => h.RequestPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        await permissionHelper.RequestPermissionsAsync(mockHandler.Object);

        // Then deny permissions
        mockHandler.Setup(h => h.RequestPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var result = await permissionHelper.RequestPermissionsAsync(mockHandler.Object);

        Assert.False(result);
    }

    [Fact]
    public async Task RequestPermissionsAsync_RaisesPermissionStatusChangedEvent()
    {
        var permissionHelper = new PushNotificationPermissionHelper();
        var mockHandler = new Mock<IPushNotificationHandler>();
        mockHandler.Setup(h => h.RequestPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var eventRaised = false;
        var eventStatus = string.Empty;

        permissionHelper.PermissionStatusChanged += (s, e) =>
        {
            eventRaised = true;
            eventStatus = e;
        };

        await permissionHelper.RequestPermissionsAsync(mockHandler.Object);

        Assert.True(eventRaised);
        Assert.Contains("granted", eventStatus, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Cache Management Tests

    [Fact]
    public async Task ClearCache_RemovesStoredPermissions()
    {
        var permissionHelper = new PushNotificationPermissionHelper();
        var mockHandler = new Mock<IPushNotificationHandler>();
        mockHandler.Setup(h => h.HasPermissionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await permissionHelper.HasPermissionAsync(mockHandler.Object);
        permissionHelper.ClearCache();

        mockHandler.Setup(h => h.HasPermissionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await permissionHelper.HasPermissionAsync(mockHandler.Object);

        Assert.False(result);
        mockHandler.Verify(h => h.HasPermissionAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public void GetCachedStatus_ReturnsNullWhenNotCached()
    {
        var permissionHelper = new PushNotificationPermissionHelper();
        var mockHandler = new Mock<IPushNotificationHandler>();

        var status = permissionHelper.GetCachedStatus(mockHandler.Object);

        Assert.Null(status);
    }

    [Fact]
    public async Task GetCachedStatus_ReturnsCachedValueWhenAvailable()
    {
        var permissionHelper = new PushNotificationPermissionHelper();
        var mockHandler = new Mock<IPushNotificationHandler>();
        mockHandler.Setup(h => h.HasPermissionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await permissionHelper.HasPermissionAsync(mockHandler.Object);
        var status = permissionHelper.GetCachedStatus(mockHandler.Object);

        Assert.True(status);
    }

    #endregion

    #region Multi-Handler Tests

    [Fact]
    public async Task CanManageMultipleHandlers_WithIndependentCaches()
    {
        var permissionHelper = new PushNotificationPermissionHelper();
        var mockHandler1 = new Mock<IPushNotificationHandler>();
        var mockHandler2 = new Mock<IPushNotificationHandler>();

        mockHandler1.Setup(h => h.PlatformName).Returns("iOS");
        mockHandler2.Setup(h => h.PlatformName).Returns("Android");

        mockHandler1.Setup(h => h.HasPermissionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mockHandler2.Setup(h => h.HasPermissionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result1 = await permissionHelper.HasPermissionAsync(mockHandler1.Object);
        var result2 = await permissionHelper.HasPermissionAsync(mockHandler2.Object);

        Assert.True(result1);
        Assert.False(result2);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task HasPermissionAsync_WithNullHandler_ThrowsArgumentNullException()
    {
        var permissionHelper = new PushNotificationPermissionHelper();

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await permissionHelper.HasPermissionAsync(null!));
    }

    [Fact]
    public async Task RequestPermissionsAsync_WithNullHandler_ThrowsArgumentNullException()
    {
        var permissionHelper = new PushNotificationPermissionHelper();

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await permissionHelper.RequestPermissionsAsync(null!));
    }

    [Fact]
    public async Task HasPermissionAsync_PropagatesHandlerExceptions()
    {
        var permissionHelper = new PushNotificationPermissionHelper();
        var mockHandler = new Mock<IPushNotificationHandler>();
        mockHandler.Setup(h => h.HasPermissionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler error"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await permissionHelper.HasPermissionAsync(mockHandler.Object));

        Assert.Contains("Handler error", ex.Message);
    }

    #endregion
}
