namespace SmartWorkz.Core.Tests.Services.Notifications;

using Moq;
using SmartWorkz.Shared;
using Microsoft.Extensions.Logging;
using Xunit;
using SmartWorkz.Core;
using PushNotificationPayload = SmartWorkz.Shared.PushNotificationPayload;
using PushNotificationAction = SmartWorkz.Shared.PushNotificationAction;

public class FirebaseCloudMessagingServiceTests
{
    private readonly Mock<ILogger<FirebaseCloudMessagingService>> _mockLogger;

    public FirebaseCloudMessagingServiceTests()
    {
        _mockLogger = new Mock<ILogger<FirebaseCloudMessagingService>>();
    }

    // ===== SendAsync(string userId, string title, string message) Tests =====

    [Fact]
    public void SendAsync_WithNullUserId_ThrowsArgumentException()
    {
        // Arrange
        var service = new FirebaseCloudMessagingService(_mockLogger.Object);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            SendAsyncStringOverload(service, null, "title", "message"));
        Assert.Equal("UserId cannot be null or empty (Parameter 'userId')", ex.Message);
    }

    [Fact]
    public void SendAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var service = new FirebaseCloudMessagingService(_mockLogger.Object);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            SendAsyncStringOverload(service, "", "title", "message"));
        Assert.Equal("UserId cannot be null or empty (Parameter 'userId')", ex.Message);
    }

    [Fact]
    public void SendAsync_WithNullTitle_ThrowsArgumentException()
    {
        // Arrange
        var service = new FirebaseCloudMessagingService(_mockLogger.Object);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            SendAsyncStringOverload(service, "user1", null, "message"));
        Assert.Contains("Title and message cannot be empty", ex.Message);
    }

    [Fact]
    public void SendAsync_WithEmptyMessage_ThrowsArgumentException()
    {
        // Arrange
        var service = new FirebaseCloudMessagingService(_mockLogger.Object);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            SendAsyncStringOverload(service, "user1", "title", ""));
        Assert.Contains("Title and message cannot be empty", ex.Message);
    }

    // Helper to call the correct SendAsync overload
    private void SendAsyncStringOverload(FirebaseCloudMessagingService service, string userId, string title, string message)
    {
        service.SendAsync(userId, title, message).GetAwaiter().GetResult();
    }

    // ===== SendAsync(IEnumerable<string> userIds, string title, string message) Tests =====

    [Fact]
    public void SendAsync_WithNullUserIds_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new FirebaseCloudMessagingService(_mockLogger.Object);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            SendAsyncBatchStringOverload(service, null, "title", "message"));
        Assert.Equal("userIds", ex.ParamName);
    }

    // Helper to call the batch SendAsync overload with strings
    private void SendAsyncBatchStringOverload(FirebaseCloudMessagingService service, IEnumerable<string> userIds, string title, string message)
    {
        service.SendAsync(userIds, title, message).GetAwaiter().GetResult();
    }

    [Fact]
    public void SendAsync_BatchWithNullTitle_ThrowsArgumentException()
    {
        // Arrange
        var service = new FirebaseCloudMessagingService(_mockLogger.Object);
        var userIds = new[] { "user1", "user2" };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            SendAsyncBatchStringOverload(service, userIds, null, "message"));
        Assert.Contains("Title and message cannot be empty", ex.Message);
    }

    // Helper to call the batch SendAsync overload with payload
    private void SendAsyncBatchPayloadOverload(FirebaseCloudMessagingService service, IEnumerable<string> userIds, PushNotificationPayload payload)
    {
        service.SendAsync(userIds, payload).GetAwaiter().GetResult();
    }

    // ===== SendAsync(string userId, PushNotificationPayload payload) Tests =====

    [Fact]
    public void SendAsync_WithPayload_NullUserId_ThrowsArgumentException()
    {
        // Arrange
        var service = new FirebaseCloudMessagingService(_mockLogger.Object);
        var payload = new PushNotificationPayload { Title = "title", Body = "body" };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            SendAsyncPayloadOverload(service, null, payload));
        Assert.Equal("UserId cannot be null or empty (Parameter 'userId')", ex.Message);
    }

    [Fact]
    public void SendAsync_WithPayload_NullPayload_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new FirebaseCloudMessagingService(_mockLogger.Object);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            SendAsyncPayloadOverload(service, "user1", null));
        Assert.Equal("payload", ex.ParamName);
    }

    // Helper to call the correct SendAsync overload with payload
    private void SendAsyncPayloadOverload(FirebaseCloudMessagingService service, string userId, PushNotificationPayload payload)
    {
        service.SendAsync(userId, payload).GetAwaiter().GetResult();
    }

    // ===== SendAsync(IEnumerable<string> userIds, PushNotificationPayload payload) Tests =====

    [Fact]
    public void SendAsync_BatchWithPayload_NullUserIds_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new FirebaseCloudMessagingService(_mockLogger.Object);
        var payload = new PushNotificationPayload { Title = "title", Body = "body" };

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            SendAsyncBatchPayloadOverload(service, null, payload));
        Assert.Equal("userIds", ex.ParamName);
    }

    [Fact]
    public void SendAsync_BatchWithPayload_NullPayload_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new FirebaseCloudMessagingService(_mockLogger.Object);
        var userIds = new[] { "user1", "user2" };

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            SendAsyncBatchPayloadOverload(service, userIds, null));
        Assert.Equal("payload", ex.ParamName);
    }

    // ===== SendToTopicAsync Tests =====

    [Fact]
    public void SendToTopicAsync_WithNullTopic_ThrowsArgumentException()
    {
        // Arrange
        var service = new FirebaseCloudMessagingService(_mockLogger.Object);
        var payload = new PushNotificationPayload { Title = "title", Body = "body" };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            SendToTopicAsyncHelper(service, null, payload));
        Assert.Equal("Topic cannot be null or empty (Parameter 'topic')", ex.Message);
    }

    [Fact]
    public void SendToTopicAsync_WithEmptyTopic_ThrowsArgumentException()
    {
        // Arrange
        var service = new FirebaseCloudMessagingService(_mockLogger.Object);
        var payload = new PushNotificationPayload { Title = "title", Body = "body" };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            SendToTopicAsyncHelper(service, "", payload));
        Assert.Equal("Topic cannot be null or empty (Parameter 'topic')", ex.Message);
    }

    [Fact]
    public void SendToTopicAsync_WithNullPayload_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new FirebaseCloudMessagingService(_mockLogger.Object);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            SendToTopicAsyncHelper(service, "topic", null));
        Assert.Equal("payload", ex.ParamName);
    }

    // Helper to call SendToTopicAsync
    private void SendToTopicAsyncHelper(FirebaseCloudMessagingService service, string topic, PushNotificationPayload payload)
    {
        service.SendToTopicAsync(topic, payload).GetAwaiter().GetResult();
    }

    // ===== SubscribeToTopicAsync Tests =====

    [Fact]
    public void SubscribeToTopicAsync_WithNullUserId_ThrowsArgumentException()
    {
        // Arrange
        var service = new FirebaseCloudMessagingService(_mockLogger.Object);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            SubscribeToTopicAsyncHelper(service, null, "topic"));
        Assert.Equal("UserId cannot be null or empty (Parameter 'userId')", ex.Message);
    }

    [Fact]
    public void SubscribeToTopicAsync_WithNullTopic_ThrowsArgumentException()
    {
        // Arrange
        var service = new FirebaseCloudMessagingService(_mockLogger.Object);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            SubscribeToTopicAsyncHelper(service, "user1", null));
        Assert.Equal("Topic cannot be null or empty (Parameter 'topic')", ex.Message);
    }

    // Helper to call SubscribeToTopicAsync
    private void SubscribeToTopicAsyncHelper(FirebaseCloudMessagingService service, string userId, string topic)
    {
        service.SubscribeToTopicAsync(userId, topic).GetAwaiter().GetResult();
    }

    // ===== UnsubscribeFromTopicAsync Tests =====

    [Fact]
    public void UnsubscribeFromTopicAsync_WithNullUserId_ThrowsArgumentException()
    {
        // Arrange
        var service = new FirebaseCloudMessagingService(_mockLogger.Object);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            UnsubscribeFromTopicAsyncHelper(service, null, "topic"));
        Assert.Equal("UserId cannot be null or empty (Parameter 'userId')", ex.Message);
    }

    [Fact]
    public void UnsubscribeFromTopicAsync_WithNullTopic_ThrowsArgumentException()
    {
        // Arrange
        var service = new FirebaseCloudMessagingService(_mockLogger.Object);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            UnsubscribeFromTopicAsyncHelper(service, "user1", null));
        Assert.Equal("Topic cannot be null or empty (Parameter 'topic')", ex.Message);
    }

    // Helper to call UnsubscribeFromTopicAsync
    private void UnsubscribeFromTopicAsyncHelper(FirebaseCloudMessagingService service, string userId, string topic)
    {
        service.UnsubscribeFromTopicAsync(userId, topic).GetAwaiter().GetResult();
    }

    // ===== PushNotificationPayload Tests =====

    [Fact]
    public void PushNotificationPayload_HasExpectedProperties()
    {
        // Arrange
        var payload = new PushNotificationPayload
        {
            Title = "Title",
            Body = "Body",
            ImageUrl = "http://example.com/image.jpg",
            Badge = 5,
            Data = new Dictionary<string, string> { { "key", "value" } }
        };

        // Assert
        Assert.Equal("Title", payload.Title);
        Assert.Equal("Body", payload.Body);
        Assert.Equal("http://example.com/image.jpg", payload.ImageUrl);
        Assert.Equal(5, payload.Badge);
        Assert.NotEmpty(payload.Data);
    }

    [Fact]
    public void PushNotificationPayload_WithAction_CreatesActionObject()
    {
        // Arrange
        var action = new PushNotificationAction
        {
            ActionId = "action-1",
            ActionUrl = "https://example.com",
            ActionTitle = "Open"
        };
        var payload = new PushNotificationPayload
        {
            Title = "Title",
            Body = "Body",
            Action = action
        };

        // Assert
        Assert.NotNull(payload.Action);
        Assert.Equal("action-1", payload.Action.ActionId);
        Assert.Equal("https://example.com", payload.Action.ActionUrl);
    }

    [Fact]
    public void PushNotificationPayload_WithoutOptionalFields_StillValid()
    {
        // Arrange & Act
        var payload = new PushNotificationPayload
        {
            Title = "Title",
            Body = "Body"
        };

        // Assert
        Assert.Null(payload.ImageUrl);
        Assert.Null(payload.Data);
        Assert.Null(payload.Action);
        Assert.Null(payload.Badge);
    }
}


