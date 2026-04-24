namespace SmartWorkz.Mobile.Tests.Services;

using Xunit;
using SmartWorkz.Mobile;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Services.Implementations;
using SmartWorkz.Shared;

public class RealtimeSubscriptionManagerTests
{
    [Fact]
    public async Task SubscribeAsync_WithValidChannel_CreatesSubscription()
    {
        // Arrange
        var manager = new RealtimeSubscriptionManager();

        // Act
        var result = await manager.SubscribeAsync("orders");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data!.SubscriptionId);
        Assert.Equal("orders", result.Data.Channel);
        Assert.True(result.Data.IsActive);
        Assert.Equal(0, result.Data.MessageCount);
        Assert.Null(result.Data.LastMessageAt);
    }

    [Fact]
    public async Task SubscribeAsync_WithDuplicateChannel_ReturnExisting()
    {
        // Arrange
        var manager = new RealtimeSubscriptionManager();
        var firstSubscribe = await manager.SubscribeAsync("orders");

        // Act
        var secondSubscribe = await manager.SubscribeAsync("orders");

        // Assert
        Assert.True(secondSubscribe.Succeeded);
        Assert.NotNull(secondSubscribe.Data);
        Assert.Equal(firstSubscribe.Data!.SubscriptionId, secondSubscribe.Data.SubscriptionId);
    }

    [Fact]
    public async Task UnsubscribeAsync_RemovesSubscription()
    {
        // Arrange
        var manager = new RealtimeSubscriptionManager();
        await manager.SubscribeAsync("orders");

        // Act
        var result = await manager.UnsubscribeAsync("orders");

        // Assert
        Assert.True(result.Succeeded);
        var getResult = await manager.GetSubscriptionAsync("orders");
        Assert.False(getResult.Succeeded);
    }

    [Fact]
    public async Task GetSubscriptionAsync_WithActiveSubscription_Returns()
    {
        // Arrange
        var manager = new RealtimeSubscriptionManager();
        var subscribeResult = await manager.SubscribeAsync("orders");

        // Act
        var result = await manager.GetSubscriptionAsync("orders");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(subscribeResult.Data!.SubscriptionId, result.Data.SubscriptionId);
        Assert.Equal("orders", result.Data.Channel);
    }

    [Fact]
    public async Task GetSubscriptionAsync_WithoutSubscription_Fails()
    {
        // Arrange
        var manager = new RealtimeSubscriptionManager();

        // Act
        var result = await manager.GetSubscriptionAsync("nonexistent");

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task GetActiveSubscriptionsAsync_WithMultipleSubscriptions_Returns()
    {
        // Arrange
        var manager = new RealtimeSubscriptionManager();
        await manager.SubscribeAsync("orders");
        await manager.SubscribeAsync("contacts");
        await manager.SubscribeAsync("notifications");

        // Act
        var result = await manager.GetActiveSubscriptionsAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Count);
    }

    [Fact]
    public async Task TrackMessageAsync_IncrementsCount()
    {
        // Arrange
        var manager = new RealtimeSubscriptionManager();
        await manager.SubscribeAsync("orders");

        // Act
        await manager.TrackMessageAsync("orders");
        var firstTrack = await manager.GetSubscriptionAsync("orders");

        await manager.TrackMessageAsync("orders");
        var secondTrack = await manager.GetSubscriptionAsync("orders");

        // Assert
        Assert.Equal(1, firstTrack.Data!.MessageCount);
        Assert.Equal(2, secondTrack.Data!.MessageCount);
        Assert.NotNull(secondTrack.Data.LastMessageAt);
    }

    [Fact]
    public async Task TrackMessageAsync_WithoutSubscription_Fails()
    {
        // Arrange
        var manager = new RealtimeSubscriptionManager();

        // Act
        var result = await manager.TrackMessageAsync("nonexistent");

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task GetSubscriptionCountAsync_ReturnsAccurateCount()
    {
        // Arrange
        var manager = new RealtimeSubscriptionManager();
        await manager.SubscribeAsync("orders");
        await manager.SubscribeAsync("contacts");
        await manager.SubscribeAsync("notifications");
        await manager.SubscribeAsync("alerts");
        await manager.SubscribeAsync("messages");

        // Act
        await manager.UnsubscribeAsync("alerts");
        await manager.UnsubscribeAsync("messages");
        var result = await manager.GetSubscriptionCountAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(3, result.Data);
    }

    [Fact]
    public async Task SubscribeAsync_WithNullChannel_Throws()
    {
        // Arrange
        var manager = new RealtimeSubscriptionManager();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => manager.SubscribeAsync(null!));
    }

    [Fact]
    public async Task SubscribeAsync_WithEmptyChannel_Throws()
    {
        // Arrange
        var manager = new RealtimeSubscriptionManager();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => manager.SubscribeAsync(""));
    }
}
