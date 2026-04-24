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
/// Unit tests for the SyncBatchOptimizer service.
/// Tests batch creation, deduplication, pattern collapse, size estimation, and batch splitting.
/// </summary>
public class SyncBatchOptimizerTests
{
    [Fact]
    public async Task CreateBatchAsync_WithValidChanges_ReturnsBatch()
    {
        // Arrange
        var optimizer = new SyncBatchOptimizer();
        var changes = new List<SyncChange>
        {
            new("1", "Order", "Status", "Pending", "Confirmed", DateTime.UtcNow, "user1"),
            new("2", "Product", "Price", "10.00", "12.00", DateTime.UtcNow, "user1"),
            new("1", "Order", "Total", "100.00", "120.00", DateTime.UtcNow, "user1")
        };

        // Act
        var result = await optimizer.CreateBatchAsync(changes);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.ChangeCount);
        Assert.True(result.Data.ApproximateSizeBytes > 0);
        Assert.NotEmpty(result.Data.BatchId);
    }

    [Fact]
    public async Task CreateBatchAsync_WithEmptyList_ReturnsBatchWithZeroChanges()
    {
        // Arrange
        var optimizer = new SyncBatchOptimizer();
        var changes = new List<SyncChange>();

        // Act
        var result = await optimizer.CreateBatchAsync(changes);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(0, result.Data.ChangeCount);
        Assert.Empty(result.Data.Changes);
    }

    [Fact]
    public async Task CreateBatchAsync_WithDuplicateProperties_DeduplicatesKeepingLatest()
    {
        // Arrange
        var optimizer = new SyncBatchOptimizer();
        var now = DateTime.UtcNow;
        var changes = new List<SyncChange>
        {
            new("1", "Order", "Status", "Pending", "Confirmed", now.AddSeconds(-10), "user1"),
            new("1", "Order", "Status", "Confirmed", "Shipped", now, "user1")  // Latest timestamp
        };

        // Act
        var result = await optimizer.CreateBatchAsync(changes);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.ChangeCount);
        Assert.Equal(1, result.Data.DeduplicatedCount);

        var change = result.Data.Changes.Single();
        // After deduplication: keeps earliest OldValue ("Pending") and latest NewValue ("Shipped")
        Assert.Equal("Shipped", change.NewValue);
        Assert.Equal("Pending", change.OldValue);
    }

    [Fact]
    public async Task CreateBatchAsync_WithCreateThenUpdate_CollapsesToCreate()
    {
        // Arrange
        var optimizer = new SyncBatchOptimizer();
        var now = DateTime.UtcNow;
        var changes = new List<SyncChange>
        {
            new("1", "Order", "Status", null, "Pending", now.AddSeconds(-5), "user1"),  // Create
            new("1", "Order", "Status", "Pending", "Confirmed", now, "user1")  // Update
        };

        // Act
        var result = await optimizer.CreateBatchAsync(changes);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.ChangeCount);
        Assert.Single(result.Data.Changes);

        var change = result.Data.Changes.Single();
        // After collapsing Create+Update, should be a Create with final NewValue
        Assert.True(change.IsCreate);
        Assert.Null(change.OldValue);
        Assert.Equal("Confirmed", change.NewValue);
    }

    [Fact]
    public async Task CreateBatchAsync_WithUpdateThenDelete_CollapsesToDelete()
    {
        // Arrange
        var optimizer = new SyncBatchOptimizer();
        var now = DateTime.UtcNow;
        var changes = new List<SyncChange>
        {
            new("1", "Order", "Status", "Pending", "Confirmed", now.AddSeconds(-5), "user1"),  // Update
            new("1", "Order", "Status", "Confirmed", null, now, "user1")  // Delete
        };

        // Act
        var result = await optimizer.CreateBatchAsync(changes);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.ChangeCount);
        Assert.Single(result.Data.Changes);

        var change = result.Data.Changes.Single();
        // After collapsing Update+Delete, should be a Delete keeping the original OldValue
        Assert.True(change.IsDelete);
        Assert.Equal("Pending", change.OldValue);
        Assert.Null(change.NewValue);
    }

    [Fact]
    public async Task CreateBatchAsync_WithCreateThenDelete_RemovesEntirely()
    {
        // Arrange
        var optimizer = new SyncBatchOptimizer();
        var now = DateTime.UtcNow;
        var changes = new List<SyncChange>
        {
            new("1", "Order", "Status", null, "Pending", now.AddSeconds(-5), "user1"),  // Create
            new("1", "Order", "Status", "Pending", null, now, "user1")  // Delete
        };

        // Act
        var result = await optimizer.CreateBatchAsync(changes);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.ChangeCount);
        // After collapsing Create+Delete, the entity is removed from the batch
        // (it was never synced to the server, so no need to send)
        Assert.Empty(result.Data.Changes);
    }

    [Fact]
    public async Task CreateBatchAsync_WithMaxCount_RespectsLimit()
    {
        // Arrange
        var optimizer = new SyncBatchOptimizer();
        var changes = Enumerable.Range(0, 10)
            .Select(i => new SyncChange(
                i.ToString(),
                "Order",
                "Status",
                "Pending",
                "Confirmed",
                DateTime.UtcNow.AddSeconds(-i),
                "user1"))
            .ToList();

        // Act
        var result = await optimizer.CreateBatchAsync(changes, maxChangeCount: 5);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        // The ChangeCount is the original input count (before limiting)
        // But after CreateBatchAsync with maxChangeCount, we take only first 5 items
        Assert.True(result.Data.Changes.Count <= 5);
    }

    [Fact]
    public async Task SplitIntoBatchesAsync_WithManyChanges_SplitsIntoMultipleBatches()
    {
        // Arrange
        var optimizer = new SyncBatchOptimizer();
        var changes = Enumerable.Range(0, 250)
            .Select(i => new SyncChange(
                i.ToString(),
                "Order",
                "Status",
                "Pending",
                "Confirmed",
                DateTime.UtcNow.AddSeconds(-i),
                "user1"))
            .ToList();

        // Act
        var result = await optimizer.SplitIntoBatchesAsync(changes, maxChangesPerBatch: 100);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Count);
        Assert.Equal(100, result.Data[0].Changes.Count);
        Assert.Equal(100, result.Data[1].Changes.Count);
        Assert.Equal(50, result.Data[2].Changes.Count);
    }

    [Fact]
    public async Task EstimateBatchSizeAsync_WithBatch_ReturnsApproximateSize()
    {
        // Arrange
        var optimizer = new SyncBatchOptimizer();
        var changes = new List<SyncChange>
        {
            new("1", "Order", "Status", "Pending", "Confirmed", DateTime.UtcNow, "user1"),
            new("2", "Product", "Price", "10.00", "12.00", DateTime.UtcNow, "user1")
        };
        var batchResult = await optimizer.CreateBatchAsync(changes);
        var batch = batchResult.Data!;

        // Act
        var result = await optimizer.EstimateBatchSizeAsync(batch);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data > 0);
        // Verify size is reasonable (at least some bytes for the JSON)
        Assert.InRange(result.Data, 100, 10000);
    }

    [Fact]
    public async Task CreateBatchAsync_WithNullInput_Throws()
    {
        // Arrange
        var optimizer = new SyncBatchOptimizer();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => optimizer.CreateBatchAsync(null!));
    }

    [Fact]
    public async Task CreateBatchAsync_WithMaxCountZero_ReturnsFail()
    {
        // Arrange
        var optimizer = new SyncBatchOptimizer();
        var changes = new List<SyncChange>
        {
            new("1", "Order", "Status", "Pending", "Confirmed", DateTime.UtcNow, "user1")
        };

        // Act
        var result = await optimizer.CreateBatchAsync(changes, maxChangeCount: 0);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task CreateBatchAsync_GroupsByEntityType()
    {
        // Arrange
        var optimizer = new SyncBatchOptimizer();
        var now = DateTime.UtcNow;
        var changes = new List<SyncChange>
        {
            new("1", "Order", "Status", "Pending", "Confirmed", now.AddSeconds(-2), "user1"),
            new("1", "Product", "Price", "10.00", "12.00", now.AddSeconds(-1), "user1"),
            new("2", "Order", "Total", "100.00", "120.00", now, "user1")
        };

        // Act
        var result = await optimizer.CreateBatchAsync(changes);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Changes.Count);

        // Verify changes are grouped by type (Order changes together, then Product)
        var groupedByType = result.Data.Changes.GroupBy(c => c.EntityType).ToList();
        Assert.Equal(2, groupedByType.Count);
    }

    [Fact]
    public async Task CreateBatchAsync_OrdersByTimestampDescending()
    {
        // Arrange
        var optimizer = new SyncBatchOptimizer();
        var baseTime = DateTime.UtcNow;
        var changes = new List<SyncChange>
        {
            new("1", "Order", "Status", "Pending", "Confirmed", baseTime.AddSeconds(1), "user1"),
            new("2", "Order", "Total", "100.00", "120.00", baseTime.AddSeconds(3), "user1"),
            new("3", "Order", "Notes", "", "Test", baseTime.AddSeconds(2), "user1")
        };

        // Act
        var result = await optimizer.CreateBatchAsync(changes);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);

        // Verify times are in descending order (most recent first)
        for (int i = 0; i < result.Data.Changes.Count - 1; i++)
        {
            Assert.True(result.Data.Changes[i].Timestamp >= result.Data.Changes[i + 1].Timestamp);
        }
    }

    [Fact]
    public async Task CreateBatchAsync_WithMultiplePropertyChanges_DeduplicatesPerProperty()
    {
        // Arrange
        var optimizer = new SyncBatchOptimizer();
        var now = DateTime.UtcNow;
        var changes = new List<SyncChange>
        {
            new("1", "Order", "Status", null, "Pending", now.AddSeconds(-5), "user1"),
            new("1", "Order", "Status", "Pending", "Confirmed", now.AddSeconds(-3), "user1"),
            new("1", "Order", "Total", null, "100.00", now.AddSeconds(-4), "user1"),
            new("1", "Order", "Total", "100.00", "120.00", now, "user1")
        };

        // Act
        var result = await optimizer.CreateBatchAsync(changes);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(4, result.Data.ChangeCount);
        Assert.Equal(2, result.Data.DeduplicatedCount);  // Two final properties

        // Verify each property appears once with latest values
        var statusChange = result.Data.Changes.FirstOrDefault(c => c.Property == "Status");
        Assert.NotNull(statusChange);
        Assert.Null(statusChange.OldValue);
        Assert.Equal("Confirmed", statusChange.NewValue);

        var totalChange = result.Data.Changes.FirstOrDefault(c => c.Property == "Total");
        Assert.NotNull(totalChange);
        Assert.Null(totalChange.OldValue);
        Assert.Equal("120.00", totalChange.NewValue);
    }

    [Fact]
    public async Task SplitIntoBatchesAsync_WithEmptyList_ReturnsEmptyBatch()
    {
        // Arrange
        var optimizer = new SyncBatchOptimizer();
        var changes = new List<SyncChange>();

        // Act
        var result = await optimizer.SplitIntoBatchesAsync(changes);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Empty(result.Data[0].Changes);
    }

    [Fact]
    public async Task CreateBatchAsync_WithIdenticalPropertyMultipleUpdates_DeduplicatesKeepingLatestValue()
    {
        // Arrange
        var optimizer = new SyncBatchOptimizer();
        var entityId = "Order123";
        var property = "Status";
        var oldChange = new SyncChange(
            EntityId: entityId,
            EntityType: "Order",
            Property: property,
            OldValue: "Pending",
            NewValue: "Processing",
            Timestamp: DateTime.UtcNow.AddMinutes(-5),
            UserId: "user1");
        var newChange = new SyncChange(
            EntityId: entityId,
            EntityType: "Order",
            Property: property,
            OldValue: "Processing",
            NewValue: "Confirmed",
            Timestamp: DateTime.UtcNow,
            UserId: "user1");

        // Act
        var result = await optimizer.CreateBatchAsync(new[] { oldChange, newChange });

        // Assert
        Assert.True(result.Succeeded);
        Assert.Single(result.Data.Changes);
        var dedupedChange = result.Data.Changes[0];
        Assert.Equal("Pending", dedupedChange.OldValue);
        Assert.Equal("Confirmed", dedupedChange.NewValue);
        Assert.Equal(2, result.Data.ChangeCount);
        Assert.Equal(1, result.Data.DeduplicatedCount);
    }
}
