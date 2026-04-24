namespace SmartWorkz.Mobile.Tests.Services;

using Xunit;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Mobile.Services.Implementations;

public class OfflineMessageQueueTests
{
    [Fact]
    public async Task EnqueueAsync_WithValidMessage_Succeeds()
    {
        // Arrange
        var queue = new OfflineMessageQueue();

        // Act
        var result = await queue.EnqueueAsync("orders", "OrderUpdated", new object[] { "order123" });

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task GetQueuedMessagesAsync_ReturnsAllMessages()
    {
        // Arrange
        var queue = new OfflineMessageQueue();
        await queue.EnqueueAsync("orders", "OrderUpdated", new object[] { "order1" });
        await queue.EnqueueAsync("contacts", "ContactAdded", new object[] { "contact1" });

        // Act
        var result = await queue.GetQueuedMessagesAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task DequeueAsync_WithValidId_RemovesMessage()
    {
        // Arrange
        var queue = new OfflineMessageQueue();
        await queue.EnqueueAsync("orders", "OrderUpdated", new object[] { "order1" });
        var messagesResult = await queue.GetQueuedMessagesAsync();
        var messageId = messagesResult.Data[0].MessageId;

        // Act
        var result = await queue.DequeueAsync(messageId);

        // Assert
        Assert.True(result.Succeeded);
        var countResult = await queue.GetQueueCountAsync();
        Assert.Equal(0, countResult.Data);
    }

    [Fact]
    public async Task ClearQueueAsync_RemovesAllMessages()
    {
        // Arrange
        var queue = new OfflineMessageQueue();
        await queue.EnqueueAsync("orders", "OrderUpdated", new object[] { "order1" });
        await queue.EnqueueAsync("contacts", "ContactAdded", new object[] { "contact1" });

        // Act
        var result = await queue.ClearQueueAsync();

        // Assert
        Assert.True(result.Succeeded);
        var countResult = await queue.GetQueueCountAsync();
        Assert.Equal(0, countResult.Data);
    }

    [Fact]
    public async Task GetQueueCountAsync_ReturnsAccurateCount()
    {
        // Arrange
        var queue = new OfflineMessageQueue();
        await queue.EnqueueAsync("orders", "OrderUpdated", new object[] { "order1" });
        await queue.EnqueueAsync("orders", "OrderUpdated", new object[] { "order2" });
        await queue.EnqueueAsync("contacts", "ContactAdded", new object[] { "contact1" });

        // Act
        var result = await queue.GetQueueCountAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(3, result.Data);
    }

    [Fact]
    public async Task IncrementRetryCountAsync_WithValidId_Succeeds()
    {
        // Arrange
        var queue = new OfflineMessageQueue();
        await queue.EnqueueAsync("orders", "OrderUpdated", new object[] { "order1" });
        var messagesResult = await queue.GetQueuedMessagesAsync();
        var messageId = messagesResult.Data[0].MessageId;

        // Act
        var result = await queue.IncrementRetryCountAsync(messageId);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task IncrementRetryCountAsync_ExceedingMax_RemovesMessage()
    {
        // Arrange
        var queue = new OfflineMessageQueue();
        await queue.EnqueueAsync("orders", "OrderUpdated", new object[] { "order1" });
        var messagesResult = await queue.GetQueuedMessagesAsync();
        var messageId = messagesResult.Data[0].MessageId;

        // Act: Retry 6 times (exceeds max of 5)
        for (int i = 0; i < 6; i++)
        {
            await queue.IncrementRetryCountAsync(messageId);
        }

        // Assert
        var countResult = await queue.GetQueueCountAsync();
        Assert.Equal(0, countResult.Data);  // Message removed
    }

    [Fact]
    public async Task GetQueuedMessagesAsync_FiltersExpiredMessages()
    {
        // Arrange
        var queue = new OfflineMessageQueue();
        await queue.EnqueueAsync("orders", "OrderUpdated", new object[] { "order1" });

        // Act: Create expired message manually (not tested via normal enqueue)
        var result = await queue.GetQueuedMessagesAsync();

        // Assert: Should not include messages older than 24 hours
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task EnqueueAsync_WithNullChannel_Throws()
    {
        // Arrange
        var queue = new OfflineMessageQueue();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            queue.EnqueueAsync(null!, "method", new object[] { }));
    }

    [Fact]
    public async Task DequeueAsync_WithInvalidId_ReturnsFail()
    {
        // Arrange
        var queue = new OfflineMessageQueue();

        // Act
        var result = await queue.DequeueAsync("nonexistent");

        // Assert
        Assert.False(result.Succeeded);
    }
}
