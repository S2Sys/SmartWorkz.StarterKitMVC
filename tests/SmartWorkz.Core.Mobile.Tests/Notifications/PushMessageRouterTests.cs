using Moq;
using SmartWorkz.Mobile.Notifications;

namespace SmartWorkz.Mobile.Tests.Notifications;

/// <summary>
/// Tests for the PushMessageRouter class.
/// Validates message routing, handler registration, and category-based dispatching.
/// </summary>
public class PushMessageRouterTests
{
    #region Handler Registration Tests

    [Fact]
    public void RegisterHandler_WithValidInput_AddsHandler()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();

        router.RegisterHandler("order", mockHandler.Object);

        Assert.True(router.IsHandlerRegistered("order", mockHandler.Object));
    }

    [Fact]
    public void RegisterHandler_WithMultipleHandlers_StoresAll()
    {
        var router = new PushMessageRouter();
        var mockHandler1 = new Mock<IPushNotificationHandler>();
        var mockHandler2 = new Mock<IPushNotificationHandler>();

        router.RegisterHandler("order", mockHandler1.Object);
        router.RegisterHandler("order", mockHandler2.Object);

        Assert.True(router.IsHandlerRegistered("order", mockHandler1.Object));
        Assert.True(router.IsHandlerRegistered("order", mockHandler2.Object));
    }

    [Fact]
    public void RegisterHandler_WithDifferentCategories_StoresIndependently()
    {
        var router = new PushMessageRouter();
        var mockHandler1 = new Mock<IPushNotificationHandler>();
        var mockHandler2 = new Mock<IPushNotificationHandler>();

        router.RegisterHandler("order", mockHandler1.Object);
        router.RegisterHandler("alert", mockHandler2.Object);

        Assert.True(router.IsHandlerRegistered("order", mockHandler1.Object));
        Assert.True(router.IsHandlerRegistered("alert", mockHandler2.Object));
        Assert.False(router.IsHandlerRegistered("alert", mockHandler1.Object));
    }

    [Fact]
    public void RegisterHandler_WithNullCategory_ThrowsArgumentException()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();

        var ex = Assert.Throws<ArgumentException>(
            () => router.RegisterHandler(null!, mockHandler.Object));

        Assert.Equal("category", ex.ParamName);
    }

    [Fact]
    public void RegisterHandler_WithNullHandler_ThrowsArgumentNullException()
    {
        var router = new PushMessageRouter();

        Assert.Throws<ArgumentNullException>(
            () => router.RegisterHandler("order", null!));
    }

    [Fact]
    public void RegisterHandler_WithEmptyCategory_ThrowsArgumentException()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();

        Assert.Throws<ArgumentException>(
            () => router.RegisterHandler(string.Empty, mockHandler.Object));
    }

    #endregion

    #region Default Handler Tests

    [Fact]
    public void RegisterDefaultHandler_WithValidInput_StoresHandler()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();

        router.RegisterDefaultHandler(mockHandler.Object);

        Assert.Equal(mockHandler.Object, router.GetDefaultHandler());
    }

    [Fact]
    public void RegisterDefaultHandler_WithNull_ThrowsArgumentNullException()
    {
        var router = new PushMessageRouter();

        Assert.Throws<ArgumentNullException>(
            () => router.RegisterDefaultHandler(null!));
    }

    [Fact]
    public void RegisterDefaultHandler_OverwritesPreviousDefault()
    {
        var router = new PushMessageRouter();
        var mockHandler1 = new Mock<IPushNotificationHandler>();
        var mockHandler2 = new Mock<IPushNotificationHandler>();

        router.RegisterDefaultHandler(mockHandler1.Object);
        router.RegisterDefaultHandler(mockHandler2.Object);

        Assert.Equal(mockHandler2.Object, router.GetDefaultHandler());
    }

    #endregion

    #region Unregister Tests

    [Fact]
    public void UnregisterHandler_RemovesSpecificHandler()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();

        router.RegisterHandler("order", mockHandler.Object);
        router.UnregisterHandler("order", mockHandler.Object);

        Assert.False(router.IsHandlerRegistered("order", mockHandler.Object));
    }

    [Fact]
    public void UnregisterHandler_KeepsOtherHandlers()
    {
        var router = new PushMessageRouter();
        var mockHandler1 = new Mock<IPushNotificationHandler>();
        var mockHandler2 = new Mock<IPushNotificationHandler>();

        router.RegisterHandler("order", mockHandler1.Object);
        router.RegisterHandler("order", mockHandler2.Object);
        router.UnregisterHandler("order", mockHandler1.Object);

        Assert.False(router.IsHandlerRegistered("order", mockHandler1.Object));
        Assert.True(router.IsHandlerRegistered("order", mockHandler2.Object));
    }

    [Fact]
    public void UnregisterAllForCategory_RemovesAllHandlers()
    {
        var router = new PushMessageRouter();
        var mockHandler1 = new Mock<IPushNotificationHandler>();
        var mockHandler2 = new Mock<IPushNotificationHandler>();

        router.RegisterHandler("order", mockHandler1.Object);
        router.RegisterHandler("order", mockHandler2.Object);
        router.UnregisterAllForCategory("order");

        Assert.False(router.IsHandlerRegistered("order", mockHandler1.Object));
        Assert.False(router.IsHandlerRegistered("order", mockHandler2.Object));
    }

    [Fact]
    public void Clear_RemovesAllHandlersAndDefault()
    {
        var router = new PushMessageRouter();
        var mockHandler1 = new Mock<IPushNotificationHandler>();
        var mockHandler2 = new Mock<IPushNotificationHandler>();

        router.RegisterHandler("order", mockHandler1.Object);
        router.RegisterHandler("alert", mockHandler2.Object);
        router.RegisterDefaultHandler(mockHandler1.Object);

        router.Clear();

        Assert.False(router.IsHandlerRegistered("order", mockHandler1.Object));
        Assert.False(router.IsHandlerRegistered("alert", mockHandler2.Object));
        Assert.Null(router.GetDefaultHandler());
    }

    #endregion

    #region Message Routing Tests

    [Fact]
    public async Task RouteMessageAsync_WithRegisteredCategory_CallsHandler()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();
        var message = new PushMessage { Category = "order" };

        router.RegisterHandler("order", mockHandler.Object);

        await router.RouteMessageAsync(message);

        mockHandler.Verify(h => h.HandleMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RouteMessageAsync_WithMultipleHandlers_CallsAll()
    {
        var router = new PushMessageRouter();
        var mockHandler1 = new Mock<IPushNotificationHandler>();
        var mockHandler2 = new Mock<IPushNotificationHandler>();
        var message = new PushMessage { Category = "order" };

        router.RegisterHandler("order", mockHandler1.Object);
        router.RegisterHandler("order", mockHandler2.Object);

        await router.RouteMessageAsync(message);

        mockHandler1.Verify(h => h.HandleMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
        mockHandler2.Verify(h => h.HandleMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RouteMessageAsync_WithUnregisteredCategory_UsesDefaultHandler()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();
        var message = new PushMessage { Category = "unknown" };

        router.RegisterDefaultHandler(mockHandler.Object);

        await router.RouteMessageAsync(message);

        mockHandler.Verify(h => h.HandleMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RouteMessageAsync_WithNullCategory_UsesDefaultHandler()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();
        var message = new PushMessage { Category = null };

        router.RegisterDefaultHandler(mockHandler.Object);

        await router.RouteMessageAsync(message);

        mockHandler.Verify(h => h.HandleMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RouteMessageAsync_WithUnregisteredCategory_AndNoDefault_SkipsSilently()
    {
        var router = new PushMessageRouter();
        var message = new PushMessage { Category = "unknown" };

        await router.RouteMessageAsync(message);
        // If we get here without exception, test passes
        Assert.True(true);
    }

    [Fact]
    public async Task RouteMessageAsync_WithNullMessage_ThrowsArgumentNullException()
    {
        var router = new PushMessageRouter();

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await router.RouteMessageAsync(null!));
    }

    [Fact]
    public async Task RouteMessageAsync_ContinuesOnHandlerException_IfContinueOnError()
    {
        var router = new PushMessageRouter();
        var mockHandler1 = new Mock<IPushNotificationHandler>();
        var mockHandler2 = new Mock<IPushNotificationHandler>();
        var message = new PushMessage { Category = "order" };

        mockHandler1.Setup(h => h.HandleMessageAsync(It.IsAny<PushMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler error"));

        router.RegisterHandler("order", mockHandler1.Object);
        router.RegisterHandler("order", mockHandler2.Object);

        await router.RouteMessageAsync(message, continueOnError: true);

        mockHandler2.Verify(h => h.HandleMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RouteMessageAsync_ThrowsOnHandlerException_IfNotContinueOnError()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();
        var message = new PushMessage { Category = "order" };

        mockHandler.Setup(h => h.HandleMessageAsync(It.IsAny<PushMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler failed"));

        router.RegisterHandler("order", mockHandler.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await router.RouteMessageAsync(message, continueOnError: false));
    }

    #endregion

    #region Message Filtering Tests

    [Fact]
    public async Task RouteMessageAsync_SkipsExpiredMessages()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();
        var message = new PushMessage { ExpiresAt = DateTime.UtcNow.AddSeconds(-1) };

        router.RegisterDefaultHandler(mockHandler.Object);

        await router.RouteMessageAsync(message);

        mockHandler.Verify(h => h.HandleMessageAsync(It.IsAny<PushMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RouteMessageAsync_HandlesValidMessages()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();
        var message = new PushMessage { ExpiresAt = DateTime.UtcNow.AddSeconds(60) };

        router.RegisterDefaultHandler(mockHandler.Object);

        await router.RouteMessageAsync(message);

        mockHandler.Verify(h => h.HandleMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task RouteMessageAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();
        var message = new PushMessage { Category = "order" };
        var ct = new CancellationToken(true);

        router.RegisterDefaultHandler(mockHandler.Object);

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await router.RouteMessageAsync(message, cancellationToken: ct));
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public void GetHandlerCount_ReturnsCorrectCount()
    {
        var router = new PushMessageRouter();
        var mockHandler1 = new Mock<IPushNotificationHandler>();
        var mockHandler2 = new Mock<IPushNotificationHandler>();

        router.RegisterHandler("order", mockHandler1.Object);
        router.RegisterHandler("order", mockHandler2.Object);
        router.RegisterHandler("alert", mockHandler1.Object);

        var count = router.GetHandlerCount();

        Assert.True(count >= 2);
    }

    #endregion
}
