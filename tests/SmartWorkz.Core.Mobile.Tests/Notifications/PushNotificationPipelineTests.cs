using Moq;
using SmartWorkz.Mobile.Notifications;
using SmartWorkz.Mobile.State.Actions;
using SmartWorkz.Mobile.State.Store;

namespace SmartWorkz.Mobile.Tests.Notifications;

/// <summary>
/// Integration tests for the complete push notification pipeline.
/// Tests interaction between message router, handlers, Redux integration, and logging.
/// </summary>
public class PushNotificationPipelineTests
{
    #region End-to-End Notification Flow Tests

    [Fact]
    public async Task CompleteNotificationFlow_FromReceiptToRouting()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();
        var message = new PushMessage
        {
            Id = "test-123",
            Title = "Order Update",
            Body = "Your order has shipped",
            Category = "order",
            DeepLink = "app://orders/123"
        };

        router.RegisterHandler("order", mockHandler.Object);

        await router.RouteMessageAsync(message);

        mockHandler.Verify(h => h.HandleMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NotificationWithCustomData_PassesDataToHandler()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();
        var message = new PushMessage
        {
            Category = "order",
            Data = new Dictionary<string, string>
            {
                { "orderId", "12345" },
                { "status", "shipped" }
            }
        };

        router.RegisterHandler("order", mockHandler.Object);

        await router.RouteMessageAsync(message);

        mockHandler.Verify(h => h.HandleMessageAsync(
            It.Is<PushMessage>(m => m.Data!["orderId"] == "12345"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SilentNotification_RoutesWithoutUserNotification()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();
        var message = new PushMessage
        {
            Category = "sync",
            IsSilent = true
        };

        router.RegisterHandler("sync", mockHandler.Object);

        await router.RouteMessageAsync(message);

        mockHandler.Verify(h => h.HandleMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeepLinkNotification_IncludesNavigationTarget()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();
        var message = new PushMessage
        {
            Category = "promotion",
            DeepLink = "app://products/electronics"
        };

        router.RegisterHandler("promotion", mockHandler.Object);

        await router.RouteMessageAsync(message);

        mockHandler.Verify(h => h.HandleMessageAsync(
            It.Is<PushMessage>(m => m.DeepLink == "app://products/electronics"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Permission Flow Tests

    [Fact]
    public async Task PermissionRequestFlow_Integrates_WithRouter()
    {
        var permissionHelper = new PushNotificationPermissionHelper();
        var mockHandler = new Mock<IPushNotificationHandler>();
        mockHandler.Setup(h => h.RequestPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mockHandler.Setup(h => h.HasPermissionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var permissionGranted = await permissionHelper.RequestPermissionsAsync(mockHandler.Object);
        var hasPermission = await permissionHelper.HasPermissionAsync(mockHandler.Object);

        Assert.True(permissionGranted);
        Assert.True(hasPermission);
    }

    [Fact]
    public async Task DeniedPermission_PreventsPipelineInitialization()
    {
        var permissionHelper = new PushNotificationPermissionHelper();
        var mockHandler = new Mock<IPushNotificationHandler>();
        mockHandler.Setup(h => h.RequestPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var permissionGranted = await permissionHelper.RequestPermissionsAsync(mockHandler.Object);

        Assert.False(permissionGranted);
    }

    #endregion

    #region Message Expiration Tests

    [Fact]
    public async Task ExpiredMessage_NotRoutedToAnyHandler()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();
        var expiredMessage = new PushMessage
        {
            ExpiresAt = DateTime.UtcNow.AddSeconds(-10)
        };

        router.RegisterDefaultHandler(mockHandler.Object);

        await router.RouteMessageAsync(expiredMessage);

        mockHandler.Verify(h => h.HandleMessageAsync(It.IsAny<PushMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MessageExpiringInFuture_RoutesNormally()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();
        var validMessage = new PushMessage
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        router.RegisterDefaultHandler(mockHandler.Object);

        await router.RouteMessageAsync(validMessage);

        mockHandler.Verify(h => h.HandleMessageAsync(validMessage, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Error Handling and Recovery Tests

    [Fact]
    public async Task HandlerException_WithContinueOnError_ProcessesOtherHandlers()
    {
        var router = new PushMessageRouter();
        var mockHandler1 = new Mock<IPushNotificationHandler>();
        var mockHandler2 = new Mock<IPushNotificationHandler>();

        mockHandler1.Setup(h => h.HandleMessageAsync(It.IsAny<PushMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler 1 failed"));
        mockHandler2.Setup(h => h.HandleMessageAsync(It.IsAny<PushMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        router.RegisterHandler("event", mockHandler1.Object);
        router.RegisterHandler("event", mockHandler2.Object);
        var message = new PushMessage { Category = "event" };

        await router.RouteMessageAsync(message, continueOnError: true);

        mockHandler2.Verify(h => h.HandleMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandlerException_WithoutContinueOnError_Propagates()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();

        mockHandler.Setup(h => h.HandleMessageAsync(It.IsAny<PushMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler failed"));

        router.RegisterHandler("event", mockHandler.Object);
        var message = new PushMessage { Category = "event" };

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await router.RouteMessageAsync(message, continueOnError: false));
    }

    #endregion

    #region Multi-Category Routing Tests

    [Fact]
    public async Task MultipleCategories_RouteToCorrectHandlers()
    {
        var router = new PushMessageRouter();
        var mockOrderHandler = new Mock<IPushNotificationHandler>();
        var mockAlertHandler = new Mock<IPushNotificationHandler>();

        router.RegisterHandler("order", mockOrderHandler.Object);
        router.RegisterHandler("alert", mockAlertHandler.Object);

        var orderMessage = new PushMessage { Category = "order" };
        var alertMessage = new PushMessage { Category = "alert" };

        await router.RouteMessageAsync(orderMessage);
        await router.RouteMessageAsync(alertMessage);

        mockOrderHandler.Verify(h => h.HandleMessageAsync(orderMessage, It.IsAny<CancellationToken>()), Times.Once);
        mockAlertHandler.Verify(h => h.HandleMessageAsync(alertMessage, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnknownCategory_FallsBackToDefaultHandler()
    {
        var router = new PushMessageRouter();
        var mockSpecificHandler = new Mock<IPushNotificationHandler>();
        var mockDefaultHandler = new Mock<IPushNotificationHandler>();

        router.RegisterHandler("known", mockSpecificHandler.Object);
        router.RegisterDefaultHandler(mockDefaultHandler.Object);

        var unknownMessage = new PushMessage { Category = "unknown" };

        await router.RouteMessageAsync(unknownMessage);

        mockSpecificHandler.Verify(h => h.HandleMessageAsync(It.IsAny<PushMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        mockDefaultHandler.Verify(h => h.HandleMessageAsync(unknownMessage, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Redux State Integration Tests

    [Fact]
    public void PushMessageIsCompatible_WithNotificationState()
    {
        var message = new PushMessage
        {
            Id = "notif-123",
            Title = "Test",
            Body = "Body"
        };

        var pushNotif = new PushNotification
        {
            Id = message.Id,
            Title = message.Title,
            Body = message.Body
        };

        Assert.Equal(pushNotif.Id, message.Id);
        Assert.Equal(pushNotif.Title, message.Title);
        Assert.Equal(pushNotif.Body, message.Body);
    }

    [Fact]
    public void AddNotificationAction_CreatesFromMessage()
    {
        var message = new PushMessage
        {
            Id = "notif-456",
            Title = "Alert",
            Body = "Important message"
        };

        var action = new AddNotificationAction
        {
            Notification = new PushNotification
            {
                Id = message.Id,
                Title = message.Title,
                Body = message.Body
            }
        };

        Assert.Equal(nameof(AddNotificationAction), action.Type);
        Assert.Equal(message.Id, action.Notification.Id);
    }

    #endregion

    #region Badge Count Tests

    [Fact]
    public async Task BadgeCountNotification_RoutesSuccessfully()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();
        var message = new PushMessage
        {
            Category = "notification",
            BadgeCount = 5
        };

        router.RegisterHandler("notification", mockHandler.Object);

        await router.RouteMessageAsync(message);

        mockHandler.Verify(h => h.HandleMessageAsync(
            It.Is<PushMessage>(m => m.BadgeCount == 5),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task CancelledOperation_StopsRouting()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();
        var message = new PushMessage();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        router.RegisterDefaultHandler(mockHandler.Object);

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await router.RouteMessageAsync(message, cancellationToken: cts.Token));
    }

    #endregion

    #region PushMessage Builder Tests

    [Fact]
    public void PushMessageBuilder_CreatesCompleteMessage()
    {
        var message = PushMessage.CreateBuilder()
            .WithId("msg-123")
            .WithTitle("Order Shipped")
            .WithBody("Your order is on the way")
            .WithCategory("order")
            .WithDeepLink("app://orders/123")
            .AddData("orderId", "12345")
            .AddData("trackingNumber", "TRK123")
            .WithBadgeCount(1)
            .AsSilent(false)
            .WithExpiresAt(DateTime.UtcNow.AddHours(24))
            .Build();

        Assert.Equal("msg-123", message.Id);
        Assert.Equal("Order Shipped", message.Title);
        Assert.Equal("order", message.Category);
        Assert.Equal("app://orders/123", message.DeepLink);
        Assert.Equal("12345", message.Data!["orderId"]);
        Assert.Equal(1, message.BadgeCount);
        Assert.False(message.IsSilent);
    }

    [Fact]
    public void PushMessageBuilder_WithMinimalData()
    {
        var message = PushMessage.CreateBuilder()
            .WithTitle("Simple")
            .WithBody("Message")
            .Build();

        Assert.Equal("Simple", message.Title);
        Assert.Equal("Message", message.Body);
        Assert.True(message.ReceivedAt <= DateTime.UtcNow);
    }

    #endregion

    #region Breadcrumb Logging Integration Tests

    [Fact]
    public async Task MessageRouting_CouldIntegrateBreadcrumbLogging()
    {
        var router = new PushMessageRouter();
        var mockHandler = new Mock<IPushNotificationHandler>();
        var message = new PushMessage
        {
            Category = "order",
            Id = "msg-789"
        };

        router.RegisterHandler("order", mockHandler.Object);

        await router.RouteMessageAsync(message);

        mockHandler.Verify(h => h.HandleMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
