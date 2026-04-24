namespace SmartWorkz.Mobile.Tests.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Mobile.Services.Implementations;
using SmartWorkz.Mobile.Models;

/// <summary>
/// Unit tests for the ChangeDataCapture service.
/// </summary>
public class ChangeDataCaptureTests
{
    [Fact]
    public async Task RecordChangeAsync_WithValidChange_Succeeds()
    {
        // Arrange
        var capture = new ChangeDataCapture();
        var change = new SyncChange(
            EntityId: "123",
            EntityType: "Order",
            Property: "Status",
            OldValue: "Pending",
            NewValue: "Confirmed",
            Timestamp: DateTime.UtcNow,
            UserId: "user1",
            ChangeId: Guid.NewGuid().ToString());

        // Act
        var result = await capture.RecordChangeAsync(change);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task RecordChangeAsync_WithNullChange_ReturnsFail()
    {
        // Arrange
        var capture = new ChangeDataCapture();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => capture.RecordChangeAsync(null!));
    }

    [Fact]
    public async Task GetChangesAsync_WithoutFilter_ReturnsAll()
    {
        // Arrange
        var capture = new ChangeDataCapture();
        var change1 = CreateChange("Order1", "Status", changeId: "123");
        var change2 = CreateChange("Order2", "Amount", changeId: "456");
        await capture.RecordChangeAsync(change1);
        await capture.RecordChangeAsync(change2);

        // Act
        var result = await capture.GetChangesAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task GetChangesAsync_WithEntityTypeFilter_ReturnsFiltered()
    {
        // Arrange
        var capture = new ChangeDataCapture();
        var orderChange = CreateChange("Order1", "Status", "123", "Order");
        var contactChange = CreateChange("Contact1", "Email", "456", "Contact");
        await capture.RecordChangeAsync(orderChange);
        await capture.RecordChangeAsync(contactChange);

        // Act
        var result = await capture.GetChangesAsync(entityType: "Order");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Single(result.Data);
        Assert.Equal("Order", result.Data[0].EntityType);
    }

    [Fact]
    public async Task GetChangesAsync_WithSinceFilter_ReturnsRecent()
    {
        // Arrange
        var capture = new ChangeDataCapture();
        var oldTime = DateTime.UtcNow.AddHours(-2);
        var newTime = DateTime.UtcNow;
        var oldChange = CreateChange("Order1", "Status", "123", timestamp: oldTime);
        var newChange = CreateChange("Order2", "Amount", "456", timestamp: newTime);
        await capture.RecordChangeAsync(oldChange);
        await capture.RecordChangeAsync(newChange);

        // Act
        var result = await capture.GetChangesAsync(since: DateTime.UtcNow.AddHours(-1));

        // Assert
        Assert.True(result.Succeeded);
        Assert.Single(result.Data);
        Assert.Equal("Order2", result.Data[0].EntityId);
    }

    [Fact]
    public async Task GetChangeAsync_WithValidId_ReturnsChange()
    {
        // Arrange
        var capture = new ChangeDataCapture();
        var changeId = "change-123";
        var change = CreateChange("Order1", "Status", changeId: changeId);
        await capture.RecordChangeAsync(change);

        // Act
        var result = await capture.GetChangeAsync(changeId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(changeId, result.Data.ChangeId);
    }

    [Fact]
    public async Task GetChangeAsync_WithInvalidId_ReturnsFail()
    {
        // Arrange
        var capture = new ChangeDataCapture();

        // Act
        var result = await capture.GetChangeAsync("nonexistent");

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ClearChangesAsync_WithoutFilter_ClearsAll()
    {
        // Arrange
        var capture = new ChangeDataCapture();
        await capture.RecordChangeAsync(CreateChange("Order1", "Status", changeId: "123"));
        await capture.RecordChangeAsync(CreateChange("Order2", "Amount", changeId: "456"));

        // Act
        var result = await capture.ClearChangesAsync();

        // Assert
        Assert.True(result.Succeeded);
        var countResult = await capture.GetChangeCountAsync();
        Assert.Equal(0, countResult.Data);
    }

    [Fact]
    public async Task ClearChangesAsync_WithTimestamp_ClearsOldOnly()
    {
        // Arrange
        var capture = new ChangeDataCapture();
        var cutoffTime = DateTime.UtcNow;
        var oldChange = CreateChange("Order1", "Status", "123", timestamp: cutoffTime.AddMinutes(-1));
        var newChange = CreateChange("Order2", "Amount", "456", timestamp: cutoffTime.AddMinutes(1));
        await capture.RecordChangeAsync(oldChange);
        await capture.RecordChangeAsync(newChange);

        // Act
        var result = await capture.ClearChangesAsync(before: cutoffTime);

        // Assert
        Assert.True(result.Succeeded);
        var countResult = await capture.GetChangeCountAsync();
        Assert.Equal(1, countResult.Data);
    }

    [Fact]
    public async Task GetChangeCountAsync_ReturnsAccurateCount()
    {
        // Arrange
        var capture = new ChangeDataCapture();
        await capture.RecordChangeAsync(CreateChange("Order1", "Status", changeId: "123"));
        await capture.RecordChangeAsync(CreateChange("Order2", "Amount", changeId: "456"));
        await capture.RecordChangeAsync(CreateChange("Order3", "Price", changeId: "789"));

        // Act
        var result = await capture.GetChangeCountAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(3, result.Data);
    }

    private static SyncChange CreateChange(
        string entityId,
        string property,
        string changeId,
        string entityType = "Order",
        DateTime? timestamp = null)
    {
        return new SyncChange(
            EntityId: entityId,
            EntityType: entityType,
            Property: property,
            OldValue: "old",
            NewValue: "new",
            Timestamp: timestamp ?? DateTime.UtcNow,
            UserId: "user1",
            ChangeId: changeId);
    }
}
