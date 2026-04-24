namespace SmartWorkz.Mobile.Tests.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Mobile.Services.Implementations;
using SmartWorkz.Mobile.Models;

/// <summary>
/// Unit tests for ConflictDetector service.
/// Tests conflict detection logic between local and remote changes.
/// </summary>
public class ConflictDetectorTests
{
    /// <summary>
    /// Test: No conflict when local and remote changes affect different properties.
    /// </summary>
    [Fact]
    public async Task DetectConflictsAsync_WithNoConflicts_ReturnsEmpty()
    {
        // Arrange
        var detector = new ConflictDetector();
        var localChanges = new List<SyncChange>
        {
            new("order1", "Order", "Status", "Pending", "Processing", DateTime.UtcNow, "user1")
        };
        var remoteChanges = new List<SyncChange>
        {
            new("order1", "Order", "Amount", 100, 110, DateTime.UtcNow.AddSeconds(5), "user2")
        };

        // Act
        var result = await detector.DetectConflictsAsync(localChanges, remoteChanges);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    /// <summary>
    /// Test: Detect conflict when same property is modified with different values.
    /// </summary>
    [Fact]
    public async Task DetectConflictsAsync_WithSamePropertyDifferentValues_DetectsConflict()
    {
        // Arrange
        var detector = new ConflictDetector();
        var now = DateTime.UtcNow;
        var localChanges = new List<SyncChange>
        {
            new("order1", "Order", "Status", "Pending", "Processing", now, "user1")
        };
        var remoteChanges = new List<SyncChange>
        {
            new("order1", "Order", "Status", "Pending", "Confirmed", now.AddSeconds(2), "user2")
        };

        // Act
        var result = await detector.DetectConflictsAsync(localChanges, remoteChanges);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("order1", result.Data[0].LocalChange.EntityId);
        Assert.Equal("Status", result.Data[0].LocalChange.Property);
    }

    /// <summary>
    /// Test: No conflict when same change is applied identically in both local and remote.
    /// </summary>
    [Fact]
    public async Task DetectConflictsAsync_WithIdenticalChanges_NoConflict()
    {
        // Arrange
        var detector = new ConflictDetector();
        var now = DateTime.UtcNow;
        var change = new SyncChange("order1", "Order", "Status", "Pending", "Processing", now, "user1");
        var localChanges = new List<SyncChange> { change };
        var remoteChanges = new List<SyncChange>
        {
            new("order1", "Order", "Status", "Pending", "Processing", now, "user2")
        };

        // Act
        var result = await detector.DetectConflictsAsync(localChanges, remoteChanges);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    /// <summary>
    /// Test: Detect multiple conflicts when multiple properties overlap with different values.
    /// </summary>
    [Fact]
    public async Task DetectConflictsAsync_WithMultipleConflicts_DetectsAll()
    {
        // Arrange
        var detector = new ConflictDetector();
        var now = DateTime.UtcNow;
        var localChanges = new List<SyncChange>
        {
            new("order1", "Order", "Status", "Pending", "Processing", now, "user1"),
            new("order1", "Order", "Amount", 100, 120, now.AddSeconds(1), "user1"),
            new("order1", "Order", "Notes", "Old", "New", now.AddSeconds(2), "user1"),
            new("order2", "Order", "Status", "Pending", "Shipped", now.AddSeconds(3), "user1"),
            new("customer1", "Customer", "Email", "old@test.com", "new@test.com", now.AddSeconds(4), "user1")
        };
        var remoteChanges = new List<SyncChange>
        {
            new("order1", "Order", "Status", "Pending", "Confirmed", now.AddSeconds(1), "user2"),
            new("order1", "Order", "Amount", 100, 130, now.AddSeconds(2), "user2"),
            new("order1", "Order", "Notes", "Old", "Different", now.AddSeconds(3), "user2"), // Conflict: different value
            new("order2", "Order", "Amount", 200, 250, now.AddSeconds(4), "user2") // No local overlap
        };

        // Act
        var result = await detector.DetectConflictsAsync(localChanges, remoteChanges);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Count); // Status, Amount, and Notes conflicts
    }

    /// <summary>
    /// Test: No conflicts when local changes are empty.
    /// </summary>
    [Fact]
    public async Task DetectConflictsAsync_WithEmptyLocalChanges_ReturnsEmpty()
    {
        // Arrange
        var detector = new ConflictDetector();
        var localChanges = new List<SyncChange>();
        var remoteChanges = new List<SyncChange>
        {
            new("order1", "Order", "Status", "Pending", "Confirmed", DateTime.UtcNow, "user2")
        };

        // Act
        var result = await detector.DetectConflictsAsync(localChanges, remoteChanges);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    /// <summary>
    /// Test: No conflicts when remote changes are empty.
    /// </summary>
    [Fact]
    public async Task DetectConflictsAsync_WithEmptyRemoteChanges_ReturnsEmpty()
    {
        // Arrange
        var detector = new ConflictDetector();
        var localChanges = new List<SyncChange>
        {
            new("order1", "Order", "Status", "Pending", "Processing", DateTime.UtcNow, "user1")
        };
        var remoteChanges = new List<SyncChange>();

        // Act
        var result = await detector.DetectConflictsAsync(localChanges, remoteChanges);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    /// <summary>
    /// Test: ConflictExistsAsync returns true when same property has different values.
    /// </summary>
    [Fact]
    public async Task ConflictExistsAsync_WithConflict_ReturnsTrue()
    {
        // Arrange
        var detector = new ConflictDetector();
        var now = DateTime.UtcNow;
        var localChange = new SyncChange("order1", "Order", "Status", "Pending", "Processing", now, "user1");
        var remoteChange = new SyncChange("order1", "Order", "Status", "Pending", "Confirmed", now.AddSeconds(2), "user2");

        // Act
        var result = await detector.ConflictExistsAsync(localChange, remoteChange);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
    }

    /// <summary>
    /// Test: ConflictExistsAsync returns false when properties don't conflict.
    /// </summary>
    [Fact]
    public async Task ConflictExistsAsync_WithoutConflict_ReturnsFalse()
    {
        // Arrange
        var detector = new ConflictDetector();
        var now = DateTime.UtcNow;
        var localChange = new SyncChange("order1", "Order", "Status", "Pending", "Processing", now, "user1");
        var remoteChange = new SyncChange("order1", "Order", "Amount", 100, 110, now.AddSeconds(2), "user2");

        // Act
        var result = await detector.ConflictExistsAsync(localChange, remoteChange);

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.Data);
    }

    /// <summary>
    /// Test: GetStatisticsAsync returns correct statistics after detection runs.
    /// </summary>
    [Fact]
    public async Task GetStatisticsAsync_AfterDetection_ReturnsCorrectStats()
    {
        // Arrange
        var detector = new ConflictDetector();
        var now = DateTime.UtcNow;

        // First detection: 3 conflicts
        var localChanges1 = new List<SyncChange>
        {
            new("order1", "Order", "Status", "Pending", "Processing", now, "user1"),
            new("order1", "Order", "Amount", 100, 120, now.AddSeconds(1), "user1"),
            new("customer1", "Customer", "Email", "old@test.com", "new@test.com", now.AddSeconds(2), "user1")
        };
        var remoteChanges1 = new List<SyncChange>
        {
            new("order1", "Order", "Status", "Pending", "Confirmed", now.AddSeconds(1), "user2"),
            new("order1", "Order", "Amount", 100, 130, now.AddSeconds(2), "user2"),
            new("customer1", "Customer", "Email", "old@test.com", "updated@test.com", now.AddSeconds(3), "user2")
        };

        // Second detection: 2 conflicts
        var localChanges2 = new List<SyncChange>
        {
            new("order2", "Order", "Status", "Pending", "Shipped", now.AddSeconds(10), "user1"),
            new("order2", "Order", "Notes", "Old", "New", now.AddSeconds(11), "user1")
        };
        var remoteChanges2 = new List<SyncChange>
        {
            new("order2", "Order", "Status", "Pending", "Confirmed", now.AddSeconds(11), "user2"),
            new("order2", "Order", "Notes", "Old", "Changed", now.AddSeconds(12), "user2")
        };

        // Act - Run first detection
        var result1 = await detector.DetectConflictsAsync(localChanges1, remoteChanges1);

        // Run second detection
        var result2 = await detector.DetectConflictsAsync(localChanges2, remoteChanges2);

        // Get statistics
        var statsResult = await detector.GetStatisticsAsync();

        // Assert
        Assert.True(result1.Succeeded);
        Assert.Equal(3, result1.Data.Count);
        Assert.True(result2.Succeeded);
        Assert.Equal(2, result2.Data.Count);

        Assert.True(statsResult.Succeeded);
        Assert.NotNull(statsResult.Data);
        Assert.Equal(2, statsResult.Data.TotalDetectionRuns);
        Assert.Equal(5, statsResult.Data.ConflictsFound);
        Assert.Equal(2, statsResult.Data.AverageConflictsPerRun); // 5 / 2 = 2
        Assert.NotEqual(default, statsResult.Data.LastDetectionTime);
    }

    /// <summary>
    /// Test: DetectConflictsAsync throws ArgumentNullException when localChanges is null.
    /// </summary>
    [Fact]
    public async Task DetectConflictsAsync_WithNullLocalChanges_Throws()
    {
        // Arrange
        var detector = new ConflictDetector();
        var remoteChanges = new List<SyncChange>();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(
            () => detector.DetectConflictsAsync(null!, remoteChanges));
        Assert.Contains("localChanges", ex.Message);
    }

    /// <summary>
    /// Test: DetectConflictsAsync throws ArgumentNullException when remoteChanges is null.
    /// </summary>
    [Fact]
    public async Task DetectConflictsAsync_WithNullRemoteChanges_Throws()
    {
        // Arrange
        var detector = new ConflictDetector();
        var localChanges = new List<SyncChange>();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(
            () => detector.DetectConflictsAsync(localChanges, null!));
        Assert.Contains("remoteChanges", ex.Message);
    }

    /// <summary>
    /// Test: ConflictExistsAsync throws ArgumentNullException when local change is null.
    /// </summary>
    [Fact]
    public async Task ConflictExistsAsync_WithNullLocalChange_Throws()
    {
        // Arrange
        var detector = new ConflictDetector();
        var remoteChange = new SyncChange("order1", "Order", "Status", "Pending", "Confirmed", DateTime.UtcNow, "user2");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(
            () => detector.ConflictExistsAsync(null!, remoteChange));
        Assert.Contains("local", ex.Message);
    }

    /// <summary>
    /// Test: ConflictExistsAsync throws ArgumentNullException when remote change is null.
    /// </summary>
    [Fact]
    public async Task ConflictExistsAsync_WithNullRemoteChange_Throws()
    {
        // Arrange
        var detector = new ConflictDetector();
        var localChange = new SyncChange("order1", "Order", "Status", "Pending", "Processing", DateTime.UtcNow, "user1");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(
            () => detector.ConflictExistsAsync(localChange, null!));
        Assert.Contains("remote", ex.Message);
    }

    /// <summary>
    /// Test: Statistics correctly tracks different entities in conflicts.
    /// </summary>
    [Fact]
    public async Task GetStatisticsAsync_TracksEntitiesInvolved()
    {
        // Arrange
        var detector = new ConflictDetector();
        var now = DateTime.UtcNow;
        var localChanges = new List<SyncChange>
        {
            new("order1", "Order", "Status", "Pending", "Processing", now, "user1"),
            new("customer1", "Customer", "Email", "old@test.com", "new@test.com", now.AddSeconds(1), "user1"),
            new("product1", "Product", "Price", 100, 120, now.AddSeconds(2), "user1")
        };
        var remoteChanges = new List<SyncChange>
        {
            new("order1", "Order", "Status", "Pending", "Confirmed", now.AddSeconds(1), "user2"),
            new("customer1", "Customer", "Email", "old@test.com", "updated@test.com", now.AddSeconds(2), "user2"),
            new("product1", "Product", "Price", 100, 130, now.AddSeconds(3), "user2")
        };

        // Act
        var detectResult = await detector.DetectConflictsAsync(localChanges, remoteChanges);
        var statsResult = await detector.GetStatisticsAsync();

        // Assert
        Assert.True(detectResult.Succeeded);
        Assert.Equal(3, detectResult.Data.Count);
        Assert.True(statsResult.Succeeded);
        Assert.Equal(3, statsResult.Data.EntitiesInvolved);
    }

    /// <summary>
    /// Test: Conflict detection with different entity IDs doesn't create conflict.
    /// </summary>
    [Fact]
    public async Task DetectConflictsAsync_WithDifferentEntityIds_NoConflict()
    {
        // Arrange
        var detector = new ConflictDetector();
        var now = DateTime.UtcNow;
        var localChanges = new List<SyncChange>
        {
            new("order1", "Order", "Status", "Pending", "Processing", now, "user1")
        };
        var remoteChanges = new List<SyncChange>
        {
            new("order2", "Order", "Status", "Pending", "Confirmed", now.AddSeconds(1), "user2")
        };

        // Act
        var result = await detector.DetectConflictsAsync(localChanges, remoteChanges);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    /// <summary>
    /// Test: Conflict detection with different entity types doesn't create conflict.
    /// </summary>
    [Fact]
    public async Task DetectConflictsAsync_WithDifferentEntityTypes_NoConflict()
    {
        // Arrange
        var detector = new ConflictDetector();
        var now = DateTime.UtcNow;
        var localChanges = new List<SyncChange>
        {
            new("id1", "Order", "Status", "Pending", "Processing", now, "user1")
        };
        var remoteChanges = new List<SyncChange>
        {
            new("id1", "Customer", "Status", "Active", "Inactive", now.AddSeconds(1), "user2")
        };

        // Act
        var result = await detector.DetectConflictsAsync(localChanges, remoteChanges);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    /// <summary>
    /// Test: No conflict when OldValue differs but NewValue is the same.
    /// Both changes produce the same final state, so there is no conflict.
    /// </summary>
    [Fact]
    public async Task DetectConflictsAsync_WithDifferentOldValueSameNewValue_NoConflict()
    {
        // Arrange
        var detector = new ConflictDetector();
        var now = DateTime.UtcNow;
        var local = new SyncChange(
            EntityId: "Order1",
            EntityType: "Order",
            Property: "Status",
            OldValue: "Pending",      // Different old value
            NewValue: "Processing",   // Same final value
            Timestamp: now,
            UserId: "user1");
        var remote = new SyncChange(
            EntityId: "Order1",
            EntityType: "Order",
            Property: "Status",
            OldValue: "Draft",        // Different old value
            NewValue: "Processing",   // Same final value
            Timestamp: now.AddMinutes(-1),
            UserId: "user2");

        // Act
        var result = await detector.DetectConflictsAsync(new[] { local }, new[] { remote });

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);  // No conflict because final values match
    }
}
