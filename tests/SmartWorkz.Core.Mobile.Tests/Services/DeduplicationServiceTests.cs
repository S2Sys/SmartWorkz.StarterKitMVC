namespace SmartWorkz.Mobile.Tests.Services;

using System;
using System.Threading.Tasks;
using Xunit;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Mobile.Services.Implementations;

public class DeduplicationServiceTests
{
    [Fact]
    public async Task IsDuplicateAsync_WithNewMessageId_ReturnsFalse()
    {
        // Arrange
        var service = new DeduplicationService();

        // Act
        var result = await service.IsDuplicateAsync("msg123");

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.Data);  // Not a duplicate (new message)
    }

    [Fact]
    public async Task IsDuplicateAsync_WithDuplicateMessageId_ReturnsTrue()
    {
        // Arrange
        var service = new DeduplicationService();
        await service.IsDuplicateAsync("msg123");  // First time

        // Act
        var result = await service.IsDuplicateAsync("msg123");  // Second time

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);  // Is a duplicate
    }

    [Fact]
    public async Task IsDuplicateAsync_AfterTimeWindow_NotDuplicate()
    {
        // Arrange
        var service = new DeduplicationService(TimeSpan.FromMilliseconds(100));
        await service.IsDuplicateAsync("msg123");

        // Act: Wait for window to expire
        await Task.Delay(150);
        var result = await service.IsDuplicateAsync("msg123");

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.Data);  // No longer a duplicate
    }

    [Fact]
    public async Task RecordMessageAsync_TracksMessage()
    {
        // Arrange
        var service = new DeduplicationService();

        // Act
        await service.RecordMessageAsync("msg456");
        var dupResult = await service.IsDuplicateAsync("msg456");

        // Assert
        Assert.True(dupResult.Data);  // Should be duplicate now
    }

    [Fact]
    public async Task CleanupAsync_RemovesOldMessages()
    {
        // Arrange
        var service = new DeduplicationService();
        await service.RecordMessageAsync("old_msg");

        // Act: Cleanup old messages
        var result = await service.CleanupAsync(TimeSpan.FromSeconds(-1));  // Everything is "old"

        // Assert
        Assert.True(result.Succeeded);
        var countResult = await service.GetTrackedCountAsync();
        Assert.Equal(0, countResult.Data);
    }

    [Fact]
    public async Task GetTrackedCountAsync_ReturnsAccurate()
    {
        // Arrange
        var service = new DeduplicationService();
        await service.RecordMessageAsync("msg1");
        await service.RecordMessageAsync("msg2");
        await service.RecordMessageAsync("msg3");

        // Act
        var result = await service.GetTrackedCountAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(3, result.Data);
    }

    [Fact]
    public async Task IsDuplicateAsync_WithNullMessageId_Throws()
    {
        // Arrange
        var service = new DeduplicationService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.IsDuplicateAsync(null));
    }

    [Fact]
    public async Task IsDuplicateAsync_WithEmptyMessageId_Throws()
    {
        // Arrange
        var service = new DeduplicationService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.IsDuplicateAsync(""));
    }
}
