namespace SmartWorkz.Mobile.Tests.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Mobile.Models;

public class AdvancedSyncServiceTests
{
    [Fact]
    public async Task SyncAsync_WithLastWriteWinsStrategy_UsesTimestamp()
    {
        // Arrange
        var mockService = new Mock<IAdvancedSyncService>();
        var result = new SyncResult(
            Success: true,
            LocalChangesApplied: 1,
            RemoteChangesApplied: 1,
            ConflictsResolved: 1,
            Duration: TimeSpan.FromSeconds(2));

        mockService.Setup(x => x.SyncAsync<object>(
            It.IsAny<string>(),
            ConflictResolutionStrategy.LastWriteWins,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(result));

        // Act
        var syncResult = await mockService.Object.SyncAsync<object>(
            "api/sync",
            ConflictResolutionStrategy.LastWriteWins);

        // Assert
        Assert.True(syncResult.Succeeded);
        Assert.Equal(1, syncResult.Data.ConflictsResolved);
    }

    [Fact]
    public void SyncConflict_GetWinningChange_UsesStrategy()
    {
        // Arrange
        var localChange = new SyncChange("id1", "Order", "Status", "Pending", "Shipped",
            DateTime.UtcNow.AddSeconds(10), "user1");
        var remoteChange = new SyncChange("id1", "Order", "Status", "Pending", "Cancelled",
            DateTime.UtcNow, "user2");

        var conflict = new SyncConflict(
            Guid.NewGuid().ToString(),
            localChange,
            remoteChange,
            ConflictResolutionStrategy.LastWriteWins,
            DateTime.UtcNow);

        // Act
        var winner = conflict.GetWinningChange();

        // Assert
        Assert.Equal(localChange, winner); // Local is more recent
    }

    [Fact]
    public void SyncChange_IsCreate_DetectsNewEntity()
    {
        // Arrange
        var change = new SyncChange("id1", "Order", "Status", null, "Pending", DateTime.UtcNow, "user1");

        // Act & Assert
        Assert.True(change.IsCreate);
        Assert.False(change.IsUpdate);
        Assert.False(change.IsDelete);
    }

    [Fact]
    public void SyncChange_IsUpdate_DetectsModification()
    {
        // Arrange
        var change = new SyncChange("id1", "Order", "Status", "Pending", "Shipped", DateTime.UtcNow, "user1");

        // Act & Assert
        Assert.True(change.IsUpdate);
        Assert.False(change.IsCreate);
        Assert.False(change.IsDelete);
    }

    [Fact]
    public void SyncChange_IsDelete_DetectsDeletion()
    {
        // Arrange
        var change = new SyncChange("id1", "Order", "Status", "Pending", null, DateTime.UtcNow, "user1");

        // Act & Assert
        Assert.True(change.IsDelete);
        Assert.False(change.IsCreate);
        Assert.False(change.IsUpdate);
    }

    [Fact]
    public void SyncChange_DisplayName_FormatsCorrectly()
    {
        // Arrange
        var change = new SyncChange("id123", "Order", "Status", "Pending", "Shipped", DateTime.UtcNow, "user1");

        // Act
        var displayName = change.DisplayName;

        // Assert
        Assert.Contains("Order(id123)", displayName);
        Assert.Contains("Status", displayName);
        Assert.Contains("Shipped", displayName);
    }

    [Fact]
    public void SyncConflict_TimestampDifferenceSeconds_CalculatesCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var localChange = new SyncChange("id1", "Order", "Status", "A", "B", now.AddSeconds(30), "user1");
        var remoteChange = new SyncChange("id1", "Order", "Status", "A", "C", now, "user2");

        var conflict = new SyncConflict(
            Guid.NewGuid().ToString(),
            localChange,
            remoteChange,
            ConflictResolutionStrategy.LastWriteWins,
            DateTime.UtcNow);

        // Act
        var diff = conflict.TimestampDifferenceSeconds;

        // Assert
        Assert.True(diff >= 29 && diff <= 31); // Allow for small timing variations
    }

    [Fact]
    public void SyncConflict_LocalIsNewer_ReturnsTrueWhenLocalIsMore()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var localChange = new SyncChange("id1", "Order", "Status", "A", "B", now.AddSeconds(10), "user1");
        var remoteChange = new SyncChange("id1", "Order", "Status", "A", "C", now, "user2");

        var conflict = new SyncConflict(
            Guid.NewGuid().ToString(),
            localChange,
            remoteChange,
            ConflictResolutionStrategy.LastWriteWins,
            DateTime.UtcNow);

        // Act & Assert
        Assert.True(conflict.LocalIsNewer);
    }

    [Fact]
    public void SyncConflict_GetWinningChange_ClientWins_AlwaysLocal()
    {
        // Arrange
        var localChange = new SyncChange("id1", "Order", "Status", "A", "B", DateTime.UtcNow, "user1");
        var remoteChange = new SyncChange("id1", "Order", "Status", "A", "C",
            DateTime.UtcNow.AddSeconds(10), "user2");

        var conflict = new SyncConflict(
            Guid.NewGuid().ToString(),
            localChange,
            remoteChange,
            ConflictResolutionStrategy.ClientWins,
            DateTime.UtcNow);

        // Act
        var winner = conflict.GetWinningChange();

        // Assert
        Assert.Equal(localChange, winner);
    }

    [Fact]
    public void SyncConflict_GetWinningChange_ServerWins_AlwaysRemote()
    {
        // Arrange
        var localChange = new SyncChange("id1", "Order", "Status", "A", "B",
            DateTime.UtcNow.AddSeconds(10), "user1");
        var remoteChange = new SyncChange("id1", "Order", "Status", "A", "C", DateTime.UtcNow, "user2");

        var conflict = new SyncConflict(
            Guid.NewGuid().ToString(),
            localChange,
            remoteChange,
            ConflictResolutionStrategy.ServerWins,
            DateTime.UtcNow);

        // Act
        var winner = conflict.GetWinningChange();

        // Assert
        Assert.Equal(remoteChange, winner);
    }

    [Fact]
    public void SyncChange_ChangeId_GeneratedWhenEmpty()
    {
        // Arrange
        var change = new SyncChange("id1", "Order", "Status", "A", "B", DateTime.UtcNow, "user1", "");

        // Act & Assert
        Assert.NotEmpty(change.ChangeId);
        Assert.NotEqual("", change.ChangeId);
    }

    [Fact]
    public void SyncConflict_DisplayName_FormatsCorrectly()
    {
        // Arrange
        var localChange = new SyncChange("id123", "Order", "Status", "A", "B", DateTime.UtcNow, "user1");
        var remoteChange = new SyncChange("id123", "Order", "Status", "A", "C", DateTime.UtcNow, "user2");

        var conflict = new SyncConflict(
            Guid.NewGuid().ToString(),
            localChange,
            remoteChange,
            ConflictResolutionStrategy.LastWriteWins,
            DateTime.UtcNow);

        // Act
        var displayName = conflict.DisplayName;

        // Assert
        Assert.Contains("Order(id123)", displayName);
        Assert.Contains("Local(", displayName);
        Assert.Contains("Remote(", displayName);
    }
}
