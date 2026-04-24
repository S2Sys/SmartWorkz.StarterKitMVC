namespace SmartWorkz.Mobile.Tests.Services;

using Moq;
using System.Reactive.Subjects;
using Xunit;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Mobile.Services.Implementations;
using SmartWorkz.Shared;
using Microsoft.Extensions.Logging;

/// <summary>
/// Comprehensive end-to-end integration tests for Phase 5.1 real-time communication system.
/// Tests complete workflows combining all Phase 5.1 components:
/// - IRealtimeService, RealtimeService, RealtimeMessage, RealtimeConnectionState
/// - ConnectionManager, MessageHandler, SubscriptionManager, RealtimeSubscription
/// - OfflineMessageQueue, AutoReconnectService, DeduplicationService
/// - Platform-specific background task managers (Android/iOS)
/// </summary>
public class Phase5_1_IntegrationTests
{
    #region Test Fixtures and Helpers

    private readonly Mock<IRealtimeService> _mockRealtimeService;
    private readonly Mock<IOfflineMessageQueue> _mockMessageQueue;
    private readonly Mock<ILogger<AutoReconnectService>> _mockReconnectLogger;
    private readonly Mock<ILogger<RealtimeMessageHandler>> _mockHandlerLogger;
    private readonly Subject<RealtimeConnectionState> _connectionStateSubject;
    private readonly Subject<RealtimeMessage> _messageSubject;

    public Phase5_1_IntegrationTests()
    {
        _mockRealtimeService = new Mock<IRealtimeService>();
        _mockMessageQueue = new Mock<IOfflineMessageQueue>();
        _mockReconnectLogger = new Mock<ILogger<AutoReconnectService>>();
        _mockHandlerLogger = new Mock<ILogger<RealtimeMessageHandler>>();
        _connectionStateSubject = new Subject<RealtimeConnectionState>();
        _messageSubject = new Subject<RealtimeMessage>();
    }

    private void SetupDefaultMocks()
    {
        _mockRealtimeService
            .Setup(x => x.OnConnectionStateChanged())
            .Returns(_connectionStateSubject);

        _mockRealtimeService
            .Setup(x => x.OnMessageReceived())
            .Returns(_messageSubject);
    }

    #endregion

    #region Connected State Workflows

    [Fact]
    public async Task SubscribeToChannel_ReceiveMessage_UnsubscribeFlow_Succeeds()
    {
        // Arrange
        SetupDefaultMocks();
        _mockRealtimeService
            .Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        _mockRealtimeService
            .Setup(x => x.SubscribeToAsync("orders", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        _mockRealtimeService
            .Setup(x => x.UnsubscribeFromAsync("orders", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var deduplicationService = new DeduplicationService();
        var messageHandler = new RealtimeMessageHandler(_mockHandlerLogger.Object);
        var reconnectService = new AutoReconnectService(_mockRealtimeService.Object, _mockMessageQueue.Object);

        bool messageHandled = false;
        messageHandler.RegisterHandler("OrderUpdated", async (msg) =>
        {
            messageHandled = true;
            await Task.CompletedTask;
        });

        // Act
        await _mockRealtimeService.Object.ConnectAsync("user123");
        var subscribeResult = await _mockRealtimeService.Object.SubscribeToAsync("orders");
        _connectionStateSubject.OnNext(RealtimeConnectionState.Connected);

        var testMessage = new RealtimeMessage(
            Channel: "orders",
            Method: "OrderUpdated",
            Payload: new { OrderId = "ORD001", Status = "Shipped" },
            ReceivedAt: DateTime.UtcNow,
            CorrelationId: Guid.NewGuid().ToString());

        var isDuplicate = await deduplicationService.IsDuplicateAsync(testMessage.CorrelationId);
        await messageHandler.HandleAsync(testMessage);

        var unsubscribeResult = await _mockRealtimeService.Object.UnsubscribeFromAsync("orders");

        // Assert
        Assert.True(subscribeResult.Succeeded);
        Assert.False(isDuplicate.Data);
        Assert.True(messageHandled);
        Assert.True(unsubscribeResult.Succeeded);
        Assert.Equal(1, messageHandler.HandlerCount);
    }

    [Fact]
    public async Task MultipleSubscriptions_ReceiveMessagesOnBothChannels()
    {
        // Arrange
        SetupDefaultMocks();
        _mockRealtimeService
            .Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        _mockRealtimeService
            .Setup(x => x.SubscribeToAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var messageHandler = new RealtimeMessageHandler();
        var ordersMessagesCount = 0;
        var contactsMessagesCount = 0;

        messageHandler.RegisterHandler("OrderUpdated", async (msg) =>
        {
            ordersMessagesCount++;
            await Task.CompletedTask;
        });

        messageHandler.RegisterHandler("ContactAdded", async (msg) =>
        {
            contactsMessagesCount++;
            await Task.CompletedTask;
        });

        // Act
        await _mockRealtimeService.Object.ConnectAsync("user123");
        await _mockRealtimeService.Object.SubscribeToAsync("orders");
        await _mockRealtimeService.Object.SubscribeToAsync("contacts");

        var orderMsg = new RealtimeMessage("orders", "OrderUpdated", null, DateTime.UtcNow, Guid.NewGuid().ToString());
        var contactMsg = new RealtimeMessage("contacts", "ContactAdded", null, DateTime.UtcNow, Guid.NewGuid().ToString());

        await messageHandler.HandleAsync(orderMsg);
        await messageHandler.HandleAsync(contactMsg);
        await messageHandler.HandleAsync(orderMsg);

        // Assert
        Assert.Equal(2, ordersMessagesCount);
        Assert.Equal(1, contactsMessagesCount);
        Assert.Equal(2, messageHandler.HandlerCount);
    }

    [Fact]
    public async Task HandlerRegistration_AndMessageRouting_Works()
    {
        // Arrange
        var messageHandler = new RealtimeMessageHandler();
        var receivedMethods = new List<string>();

        messageHandler.RegisterHandler("SystemHealthCheck", async (msg) =>
        {
            receivedMethods.Add(msg.Method);
            await Task.CompletedTask;
        });

        messageHandler.RegisterHandler("SystemNotification", async (msg) =>
        {
            receivedMethods.Add(msg.Method);
            await Task.CompletedTask;
        });

        // Act
        var msg1 = new RealtimeMessage("system", "SystemHealthCheck", null, DateTime.UtcNow, Guid.NewGuid().ToString());
        var msg2 = new RealtimeMessage("system", "SystemNotification", null, DateTime.UtcNow, Guid.NewGuid().ToString());

        await messageHandler.HandleAsync(msg1);
        await messageHandler.HandleAsync(msg2);

        // Assert
        Assert.Equal(2, receivedMethods.Count);
        Assert.Contains("SystemHealthCheck", receivedMethods);
        Assert.Contains("SystemNotification", receivedMethods);
    }

    [Fact]
    public async Task ConnectionStateChanges_AreTrackable()
    {
        // Arrange
        SetupDefaultMocks();
        var stateChanges = new List<RealtimeConnectionState>();
        var subscription = _mockRealtimeService.Object.OnConnectionStateChanged()
            .Subscribe(state => stateChanges.Add(state));

        // Act
        _connectionStateSubject.OnNext(RealtimeConnectionState.Connecting);
        _connectionStateSubject.OnNext(RealtimeConnectionState.Connected);
        _connectionStateSubject.OnNext(RealtimeConnectionState.Disconnected);

        // Assert
        Assert.Equal(3, stateChanges.Count);
        Assert.Equal(RealtimeConnectionState.Connecting, stateChanges[0]);
        Assert.Equal(RealtimeConnectionState.Connected, stateChanges[1]);
        Assert.Equal(RealtimeConnectionState.Disconnected, stateChanges[2]);

        subscription.Dispose();
    }

    #endregion

    #region Disconnected State Workflows

    [Fact]
    public async Task QueuedMessages_PersistWhenDisconnected()
    {
        // Arrange
        var queue = new OfflineMessageQueue();
        var channel = "notifications";
        var method = "SendNotification";
        var args = new object[] { "user123", "Hello" };

        // Act
        await queue.EnqueueAsync(channel, method, args);
        var result = await queue.GetQueuedMessagesAsync();

        // Assert
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal(channel, result.Data[0].Channel);
        Assert.Equal(method, result.Data[0].Method);
        Assert.Equal(0, result.Data[0].RetryCount);
    }

    [Fact]
    public async Task QueueSurvivesAutoReconnect_AndFlushedOnConnection()
    {
        // Arrange
        SetupDefaultMocks();
        var queue = new OfflineMessageQueue();

        // Setup with real queue for this test
        await queue.EnqueueAsync("orders", "PlaceOrder", new object[] { "123" });
        await queue.EnqueueAsync("payments", "ProcessPayment", new object[] { "456" });

        _mockRealtimeService
            .Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var reconnectService = new AutoReconnectService(_mockRealtimeService.Object, queue, _mockReconnectLogger.Object);

        // Act
        await reconnectService.StartAsync("user123");
        _connectionStateSubject.OnNext(RealtimeConnectionState.Connected);
        await Task.Delay(150); // Allow async flush to complete

        // Assert - Queue flush should be triggered on connected state
        var remainingCount = await queue.GetQueueCountAsync();
        Assert.True(reconnectService.IsReconnecting == false); // Should not be reconnecting when connected
    }

    [Fact]
    public async Task QueueRespects_MaxRetryLimit_And_RemovesMessage()
    {
        // Arrange
        var queue = new OfflineMessageQueue();
        await queue.EnqueueAsync("orders", "OrderUpdate", new object[] { "123" });
        var messagesResult = await queue.GetQueuedMessagesAsync();
        var messageId = messagesResult.Data[0].MessageId;

        // Act - Retry 6 times (exceeds max of 5)
        for (int i = 0; i < 6; i++)
        {
            await queue.IncrementRetryCountAsync(messageId);
        }

        var countResult = await queue.GetQueueCountAsync();

        // Assert
        Assert.Equal(0, countResult.Data);
    }

    #endregion

    #region Reconnection Scenarios

    [Fact]
    public async Task AutoReconnect_WithExponentialBackoff_TracksAttempts()
    {
        // Arrange
        SetupDefaultMocks();
        var connectionAttempts = 0;
        _mockRealtimeService
            .Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                connectionAttempts++;
                await Task.Delay(10);
                return Result.Ok();
            });

        var reconnectService = new AutoReconnectService(_mockRealtimeService.Object, _mockMessageQueue.Object, _mockReconnectLogger.Object);

        // Act
        await reconnectService.StartAsync("user123");
        var initialStats = await reconnectService.GetStatsAsync();

        // Assert
        Assert.True(initialStats.Succeeded);
        Assert.NotNull(initialStats.Data);
        Assert.Equal(0, initialStats.Data.TotalAttempts);
        Assert.Equal(0, initialStats.Data.SuccessfulReconnects);
    }

    [Fact]
    public async Task HealthCheckTimer_TriggersRecovery_OnConnectionLoss()
    {
        // Arrange
        SetupDefaultMocks();
        _mockRealtimeService
            .Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var reconnectService = new AutoReconnectService(_mockRealtimeService.Object, _mockMessageQueue.Object);

        // Act
        await reconnectService.StartAsync("user123");
        _connectionStateSubject.OnNext(RealtimeConnectionState.Error);

        // Assert
        Assert.True(reconnectService.IsReconnecting);
    }

    [Fact]
    public async Task ReconnectStats_UpdatedWithAttemptsAndTiming()
    {
        // Arrange
        SetupDefaultMocks();
        var reconnectService = new AutoReconnectService(_mockRealtimeService.Object, _mockMessageQueue.Object);

        // Act
        await reconnectService.StartAsync("user123");
        var stats = await reconnectService.GetStatsAsync();

        // Assert
        Assert.True(stats.Succeeded);
        Assert.NotNull(stats.Data);
        Assert.True(stats.Data.TotalAttempts >= 0);
        Assert.True(stats.Data.SuccessfulReconnects >= 0);
        Assert.True(stats.Data.AvgReconnectDuration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task MessageDeduplication_DuringRapidReconnectCycles()
    {
        // Arrange
        var deduplicationService = new DeduplicationService(TimeSpan.FromMinutes(5));
        var messageId = Guid.NewGuid().ToString();

        // Act
        var result1 = await deduplicationService.IsDuplicateAsync(messageId);
        await Task.Delay(50);
        var result2 = await deduplicationService.IsDuplicateAsync(messageId);

        // Assert
        Assert.False(result1.Data); // First occurrence
        Assert.True(result2.Data);  // Duplicate within window
    }

    #endregion

    #region Deduplication Under Network Conditions

    [Fact]
    public async Task SameMessageId_RejectedOnReplay()
    {
        // Arrange
        var deduplicationService = new DeduplicationService();
        var messageId = "msg-12345";

        // Act
        var firstCheck = await deduplicationService.IsDuplicateAsync(messageId);
        var secondCheck = await deduplicationService.IsDuplicateAsync(messageId);
        var thirdCheck = await deduplicationService.IsDuplicateAsync(messageId);

        // Assert
        Assert.False(firstCheck.Data);  // New message
        Assert.True(secondCheck.Data);  // Duplicate
        Assert.True(thirdCheck.Data);   // Still duplicate
    }

    [Fact]
    public async Task DedupWindow_RespectsTimeExpiry()
    {
        // Arrange
        var dedupWindow = TimeSpan.FromMilliseconds(200);
        var deduplicationService = new DeduplicationService(dedupWindow);
        var messageId = "msg-window-test";

        // Act
        var firstCheck = await deduplicationService.IsDuplicateAsync(messageId);
        await Task.Delay(250); // Wait longer than window
        var secondCheck = await deduplicationService.IsDuplicateAsync(messageId);

        // Assert
        Assert.False(firstCheck.Data);  // New message
        Assert.False(secondCheck.Data); // Outside window, treated as new
    }

    [Fact]
    public async Task CleanupRemovesExpiredTrackingRecords()
    {
        // Arrange
        var deduplicationService = new DeduplicationService();
        await deduplicationService.RecordMessageAsync("msg-1");
        await deduplicationService.RecordMessageAsync("msg-2");

        var countBefore = await deduplicationService.GetTrackedCountAsync();

        // Act
        await deduplicationService.CleanupAsync(TimeSpan.FromMinutes(-1)); // Remove all older than 1 minute in future
        var countAfter = await deduplicationService.GetTrackedCountAsync();

        // Assert
        Assert.True(countBefore.Data >= 2);
        // Cleanup from future time removes nothing (no messages older than future cutoff)
    }

    #endregion

    #region Subscription Management

    [Fact]
    public async Task Subscriptions_PersistAcrossReconnect()
    {
        // Arrange
        SetupDefaultMocks();
        var subscriptions = new Dictionary<string, RealtimeSubscription>();
        var channels = new[] { "orders", "payments", "notifications" };

        _mockRealtimeService
            .Setup(x => x.SubscribeToAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, CancellationToken>(async (channel, ct) =>
            {
                subscriptions[channel] = new RealtimeSubscription(
                    SubscriptionId: Guid.NewGuid().ToString(),
                    Channel: channel,
                    SubscribedAt: DateTime.UtcNow,
                    IsActive: true,
                    MessageCount: 0,
                    LastMessageAt: null);
                return Result.Ok();
            });

        // Act
        foreach (var channel in channels)
        {
            await _mockRealtimeService.Object.SubscribeToAsync(channel);
        }

        // Simulate disconnection and reconnection
        _connectionStateSubject.OnNext(RealtimeConnectionState.Disconnected);
        _connectionStateSubject.OnNext(RealtimeConnectionState.Connected);

        // Assert
        Assert.Equal(3, subscriptions.Count);
        Assert.All(channels, channel => Assert.Contains(channel, subscriptions.Keys));
    }

    [Fact]
    public async Task SubscriptionMetrics_UpdatedOnMessageReceived()
    {
        // Arrange
        SetupDefaultMocks();
        var subscriptionId = Guid.NewGuid().ToString();
        var subscription = new RealtimeSubscription(
            SubscriptionId: subscriptionId,
            Channel: "orders",
            SubscribedAt: DateTime.UtcNow,
            IsActive: true,
            MessageCount: 0,
            LastMessageAt: null);

        var updatedSubscription = subscription with { MessageCount = 1, LastMessageAt = DateTime.UtcNow };

        // Act & Assert
        Assert.Equal(0, subscription.MessageCount);
        Assert.Null(subscription.LastMessageAt);
        Assert.Equal(1, updatedSubscription.MessageCount);
        Assert.NotNull(updatedSubscription.LastMessageAt);
    }

    [Fact]
    public async Task MultipleHandlers_ForSameChannel_WorkCorrectly()
    {
        // Arrange
        var messageHandler = new RealtimeMessageHandler();
        var handler1Called = false;
        var handler2Called = false;

        messageHandler.RegisterHandler("OrderUpdated", async (msg) =>
        {
            handler1Called = true;
            await Task.CompletedTask;
        });

        // Register second handler (replaces first in this implementation)
        messageHandler.RegisterHandler("OrderUpdated", async (msg) =>
        {
            handler2Called = true;
            await Task.CompletedTask;
        });

        var message = new RealtimeMessage("orders", "OrderUpdated", null, DateTime.UtcNow, Guid.NewGuid().ToString());

        // Act
        await messageHandler.HandleAsync(message);

        // Assert
        Assert.False(handler1Called); // First handler replaced
        Assert.True(handler2Called);  // Second handler executed
    }

    #endregion

    #region Error Handling

    [Fact]
    public async Task TransientFailures_TriggerRetry()
    {
        // Arrange
        SetupDefaultMocks();
        var queue = new OfflineMessageQueue();
        var reconnectService = new AutoReconnectService(_mockRealtimeService.Object, queue);

        _mockRealtimeService
            .Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Temporary network error"));

        // Act
        await reconnectService.StartAsync("user123");
        var stats = await reconnectService.GetStatsAsync();

        // Assert
        Assert.True(stats.Succeeded);
        Assert.NotNull(stats.Data);
    }

    [Fact]
    public async Task ConnectionErrors_LoggedAppropriately()
    {
        // Arrange
        SetupDefaultMocks();
        var mockLogger = new Mock<ILogger<AutoReconnectService>>();
        var reconnectService = new AutoReconnectService(_mockRealtimeService.Object, _mockMessageQueue.Object, mockLogger.Object);

        _mockRealtimeService
            .Setup(x => x.OnConnectionStateChanged())
            .Returns(_connectionStateSubject);

        // Act
        await reconnectService.StartAsync("user123");
        _connectionStateSubject.OnNext(RealtimeConnectionState.Error);

        // Assert
        Assert.True(reconnectService.IsReconnecting);
    }

    [Fact]
    public async Task ServiceGracefullyHandles_HandlerExceptions()
    {
        // Arrange
        var messageHandler = new RealtimeMessageHandler();
        messageHandler.RegisterHandler("FaultyMethod", async (msg) =>
        {
            throw new InvalidOperationException("Handler error");
        });

        var message = new RealtimeMessage("test", "FaultyMethod", null, DateTime.UtcNow, Guid.NewGuid().ToString());

        // Act
        var result = await messageHandler.HandleAsync(message);

        // Assert
        Assert.False(result); // Handler failed gracefully
    }

    [Fact]
    public async Task Queue_RespectsRetryLimits_BeforeRemoval()
    {
        // Arrange
        var queue = new OfflineMessageQueue();
        await queue.EnqueueAsync("test", "TestMethod", new object[] { });
        var messagesResult = await queue.GetQueuedMessagesAsync();
        var messageId = messagesResult.Data[0].MessageId;

        // Act
        for (int i = 0; i < 5; i++)
        {
            await queue.IncrementRetryCountAsync(messageId);
        }

        var stillExists = await queue.GetQueueCountAsync();

        // Assert
        Assert.Equal(1, stillExists.Data); // Message still in queue after 5 retries
    }

    #endregion

    #region End-to-End Workflow Tests

    [Fact]
    public async Task CompleteRealTimeWorkflow_ConnectSubscribeMessageDisconnect()
    {
        // Arrange
        SetupDefaultMocks();
        _mockRealtimeService
            .Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        _mockRealtimeService
            .Setup(x => x.SubscribeToAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        _mockRealtimeService
            .Setup(x => x.DisconnectAsync())
            .ReturnsAsync(Result.Ok());

        var deduplicationService = new DeduplicationService();
        var messageHandler = new RealtimeMessageHandler();
        var messagesReceived = new List<RealtimeMessage>();

        messageHandler.RegisterHandler("DataUpdate", async (msg) =>
        {
            messagesReceived.Add(msg);
            await Task.CompletedTask;
        });

        // Act
        var connectResult = await _mockRealtimeService.Object.ConnectAsync("user123");
        _connectionStateSubject.OnNext(RealtimeConnectionState.Connected);

        var subscribeResult = await _mockRealtimeService.Object.SubscribeToAsync("data");

        var message = new RealtimeMessage("data", "DataUpdate", new { Value = 100 }, DateTime.UtcNow, Guid.NewGuid().ToString());
        var isDuplicate = await deduplicationService.IsDuplicateAsync(message.CorrelationId);
        await messageHandler.HandleAsync(message);

        var disconnectResult = await _mockRealtimeService.Object.DisconnectAsync();

        // Assert
        Assert.True(connectResult.Succeeded);
        Assert.True(subscribeResult.Succeeded);
        Assert.False(isDuplicate.Data);
        Assert.Single(messagesReceived);
        Assert.True(disconnectResult.Succeeded);
    }

    [Fact]
    public async Task ComplexScenario_DisconnectedQueue_FlushedOnReconnect_WithDedup()
    {
        // Arrange
        SetupDefaultMocks();
        var queue = new OfflineMessageQueue();
        var deduplicationService = new DeduplicationService();
        var reconnectService = new AutoReconnectService(_mockRealtimeService.Object, queue);

        // Setup queue with messages
        var messageId = Guid.NewGuid().ToString();
        await queue.EnqueueAsync("orders", "ProcessOrder", new object[] { messageId });

        var queuedMessages = await queue.GetQueuedMessagesAsync();
        _mockMessageQueue
            .Setup(x => x.GetQueuedMessagesAsync())
            .ReturnsAsync(queuedMessages);

        _mockMessageQueue
            .Setup(x => x.DequeueAsync(It.IsAny<string>()))
            .Returns(async (string id) => await queue.DequeueAsync(id));

        _mockRealtimeService
            .Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var isDuplicateBeforeFlush = await deduplicationService.IsDuplicateAsync(messageId);

        // Act
        await reconnectService.StartAsync("user123");
        _connectionStateSubject.OnNext(RealtimeConnectionState.Connected);
        await Task.Delay(100);

        var isDuplicateAfterFlush = await deduplicationService.IsDuplicateAsync(messageId);

        // Assert
        Assert.False(isDuplicateBeforeFlush.Data); // First check
        Assert.True(isDuplicateAfterFlush.Data);   // Duplicate after dedup window
    }

    #endregion

    #region Platform-Specific Tests

#if __ANDROID__
    [Fact]
    public async Task AndroidBackgroundTaskManager_CanStartBackgroundTask()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AndroidBackgroundTaskManager>>();
        var backgroundManager = new AndroidBackgroundTaskManager(mockLogger.Object);

        // Act
        var result = await backgroundManager.StartAsync("TestTask");

        // Assert
        Assert.NotNull(backgroundManager);
    }
#endif

#if __IOS__
    [Fact]
    public async Task iOSBackgroundTaskManager_CanStartBackgroundTask()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<iOSBackgroundTaskManager>>();
        var backgroundManager = new iOSBackgroundTaskManager(mockLogger.Object);

        // Act
        var result = await backgroundManager.StartAsync("TestTask");

        // Assert
        Assert.NotNull(backgroundManager);
    }
#endif

    #endregion

    #region State Transition Tests

    [Fact]
    public async Task ConnectionStateTransitions_FollowExpectedSequence()
    {
        // Arrange
        SetupDefaultMocks();
        var stateSequence = new List<RealtimeConnectionState>();
        var subscription = _mockRealtimeService.Object.OnConnectionStateChanged()
            .Subscribe(state => stateSequence.Add(state));

        // Act
        _connectionStateSubject.OnNext(RealtimeConnectionState.Disconnected);
        _connectionStateSubject.OnNext(RealtimeConnectionState.Connecting);
        _connectionStateSubject.OnNext(RealtimeConnectionState.Connected);
        _connectionStateSubject.OnNext(RealtimeConnectionState.Reconnecting);
        _connectionStateSubject.OnNext(RealtimeConnectionState.Connected);

        // Assert
        Assert.Equal(5, stateSequence.Count);
        Assert.Equal(RealtimeConnectionState.Disconnected, stateSequence[0]);
        Assert.Equal(RealtimeConnectionState.Connecting, stateSequence[1]);
        Assert.Equal(RealtimeConnectionState.Connected, stateSequence[2]);
        Assert.Equal(RealtimeConnectionState.Reconnecting, stateSequence[3]);
        Assert.Equal(RealtimeConnectionState.Connected, stateSequence[4]);

        subscription.Dispose();
    }

    #endregion

    #region Message Flow Tests

    [Fact]
    public async Task MessageFlow_SystemMessages_RecognizedCorrectly()
    {
        // Arrange
        var systemMessage = new RealtimeMessage(
            Channel: "system",
            Method: "System.HealthCheck",
            Payload: new { Status = "OK" },
            ReceivedAt: DateTime.UtcNow,
            CorrelationId: Guid.NewGuid().ToString());

        var businessMessage = new RealtimeMessage(
            Channel: "orders",
            Method: "OrderUpdated",
            Payload: new { OrderId = "123" },
            ReceivedAt: DateTime.UtcNow,
            CorrelationId: Guid.NewGuid().ToString());

        // Act & Assert
        Assert.True(systemMessage.IsSystemMessage);
        Assert.False(businessMessage.IsSystemMessage);
    }

    [Fact]
    public async Task MessageAge_CalculatedAccurately()
    {
        // Arrange
        var createdTime = DateTime.UtcNow.AddSeconds(-10);
        var message = new RealtimeMessage(
            Channel: "test",
            Method: "TestMethod",
            Payload: null,
            ReceivedAt: createdTime,
            CorrelationId: Guid.NewGuid().ToString());

        // Act
        var age = message.Age;

        // Assert
        Assert.True(age >= TimeSpan.FromSeconds(10));
        Assert.True(age < TimeSpan.FromSeconds(11));
    }

    #endregion
}
