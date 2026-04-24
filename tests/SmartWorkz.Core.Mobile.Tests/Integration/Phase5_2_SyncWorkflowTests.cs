namespace SmartWorkz.Mobile.Tests.Integration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Services.Implementations;
using SmartWorkz.Mobile.Persistence;

/// <summary>
/// Integration tests for Phase 5.2 Sync Workflow components.
/// Verifies complete end-to-end sync scenarios combining all Phase 5.2 components:
/// - ChangeDataCapture: Track local changes
/// - SyncBatchOptimizer: Optimize changes into batches
/// - ConflictDetector: Detect conflicts between local/remote
/// - ConflictResolutionEngine: Resolve conflicts with strategies
/// - RetryPolicy: Retry with exponential backoff
/// - FileSystemSyncStateStore: Persist sync state
/// </summary>
public class Phase5_2_SyncWorkflowTests : IDisposable
{
    private readonly string _tempStoragePath;

    public Phase5_2_SyncWorkflowTests()
    {
        // Create temporary directory for test storage
        _tempStoragePath = Path.Combine(Path.GetTempPath(), $"sync_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempStoragePath);
    }

    public void Dispose()
    {
        // Cleanup temp directory
        try
        {
            if (Directory.Exists(_tempStoragePath))
            {
                Directory.Delete(_tempStoragePath, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    /// <summary>
    /// Test 1: Happy Path - No Conflicts
    /// Local: Order.Status Pending→Processing
    /// Remote: Contact.Email old→new (different entity)
    /// Expected: No conflicts, both sync successfully
    /// </summary>
    [Fact]
    public async Task Scenario1_HappyPath_NoConflicts()
    {
        // Arrange
        var changeCapture = new ChangeDataCapture();
        var batchOptimizer = new SyncBatchOptimizer();
        var conflictDetector = new ConflictDetector();
        var now = DateTime.UtcNow;

        // Record local change
        var localOrder = CreateChange(
            entityId: "Order1",
            property: "Status",
            oldValue: "Pending",
            newValue: "Processing",
            timestamp: now,
            entityType: "Order");

        await changeCapture.RecordChangeAsync(localOrder);

        // Simulate remote change (different entity - no conflict possible)
        var remoteContact = CreateChange(
            entityId: "Contact1",
            property: "Email",
            oldValue: "old@example.com",
            newValue: "new@example.com",
            timestamp: now.AddSeconds(-1),
            entityType: "Contact");

        // Act
        var changes = await changeCapture.GetChangesAsync();
        var conflicts = await conflictDetector.DetectConflictsAsync(
            changes.Data,
            new[] { remoteContact });

        var batch = await batchOptimizer.CreateBatchAsync(changes.Data);

        // Assert
        Assert.True(changes.Succeeded);
        Assert.Single(changes.Data);
        Assert.True(conflicts.Succeeded);
        Assert.Empty(conflicts.Data);  // No conflicts expected
        Assert.True(batch.Succeeded);
        Assert.Single(batch.Data.Changes);
        Assert.Equal(1, batch.Data.ChangeCount);
    }

    /// <summary>
    /// Test 2: Simple Conflict - Last-Write-Wins Resolution
    /// Local: Order.Status Pending→Processing (timestamp: now)
    /// Remote: Order.Status Pending→Confirmed (timestamp: now-1s)
    /// Conflict strategy: LastWriteWins
    /// Expected: Local change wins (more recent), remote discarded
    /// </summary>
    [Fact]
    public async Task Scenario2_SimpleConflict_LastWriteWinsResolution()
    {
        // Arrange
        var changeCapture = new ChangeDataCapture();
        var conflictDetector = new ConflictDetector();
        var resolutionEngine = new ConflictResolutionEngine();
        var now = DateTime.UtcNow;

        // Local change (more recent)
        var localChange = CreateChange(
            entityId: "Order1",
            property: "Status",
            oldValue: "Pending",
            newValue: "Processing",
            timestamp: now,
            entityType: "Order");

        await changeCapture.RecordChangeAsync(localChange);

        // Remote change (older)
        var remoteChange = CreateChange(
            entityId: "Order1",
            property: "Status",
            oldValue: "Pending",
            newValue: "Confirmed",
            timestamp: now.AddSeconds(-1),
            entityType: "Order");

        // Act
        var changes = await changeCapture.GetChangesAsync();
        var conflicts = await conflictDetector.DetectConflictsAsync(
            changes.Data,
            new[] { remoteChange });

        // Resolve the conflict
        SyncChange resolvedChange = null;
        if (conflicts.Data.Count > 0)
        {
            var conflict = conflicts.Data[0];
            Assert.Equal(ConflictResolutionStrategy.LastWriteWins, conflict.ResolutionStrategy);
            resolvedChange = await resolutionEngine.ResolveAsync(conflict);
        }

        // Assert
        Assert.True(conflicts.Succeeded);
        Assert.Single(conflicts.Data);
        Assert.NotNull(resolvedChange);
        // LocalChange is newer, so it should win
        Assert.Equal("Processing", resolvedChange.NewValue);
        Assert.Equal(localChange.Timestamp, resolvedChange.Timestamp);
    }

    /// <summary>
    /// Test 3: Multiple Changes - Same Entity Optimization
    /// Local changes: Order created, Status updated, Amount changed
    /// Expected: Batch optimizer deduplicates and collapses patterns
    /// ChangeCount in batch reflects original (3), DeduplicatedCount shows optimized
    /// </summary>
    [Fact]
    public async Task Scenario3_MultipleChanges_SameEntityOptimization()
    {
        // Arrange
        var changeCapture = new ChangeDataCapture();
        var batchOptimizer = new SyncBatchOptimizer();
        var now = DateTime.UtcNow;

        // Record multiple changes to same order
        var createChange = CreateChange(
            entityId: "Order1",
            property: "Status",
            oldValue: null,
            newValue: "Pending",
            timestamp: now.AddSeconds(-10),
            entityType: "Order");

        var updateChange = CreateChange(
            entityId: "Order1",
            property: "Status",
            oldValue: "Pending",
            newValue: "Processing",
            timestamp: now.AddSeconds(-5),
            entityType: "Order");

        var amountChange = CreateChange(
            entityId: "Order1",
            property: "Amount",
            oldValue: null,
            newValue: "150.00",
            timestamp: now,
            entityType: "Order");

        await changeCapture.RecordChangeAsync(createChange);
        await changeCapture.RecordChangeAsync(updateChange);
        await changeCapture.RecordChangeAsync(amountChange);

        // Act
        var changes = await changeCapture.GetChangesAsync();
        Assert.Equal(3, changes.Data.Count);

        var batch = await batchOptimizer.CreateBatchAsync(changes.Data);

        // Assert
        Assert.True(batch.Succeeded);
        Assert.Equal(3, batch.Data.ChangeCount);  // Original count
        Assert.Equal(2, batch.Data.DeduplicatedCount);  // Create+Update on Status collapsed
        // Verify batch contains the optimized changes
        Assert.NotEmpty(batch.Data.Changes);
        Assert.True(batch.Data.ApproximateSizeBytes > 0);
    }

    /// <summary>
    /// Test 4: Transient Failure with Retry
    /// Sync operation fails with TimeoutException on first call
    /// Succeeds on second call
    /// Expected: RetryPolicy retries once, ultimately succeeds
    /// </summary>
    [Fact]
    public async Task Scenario4_TransientFailure_WithRetry()
    {
        // Arrange
        var retryConfig = new RetryConfig(
            MaxRetries: 2,
            InitialDelay: TimeSpan.FromMilliseconds(10),  // Fast for testing
            MaxDelay: TimeSpan.FromMilliseconds(50));

        var retryPolicy = new ExponentialBackoffRetryPolicy(retryConfig);
        var attemptCount = 0;

        // Simulate an operation that fails once then succeeds
        async Task<Result> OperationWithTransientFailure()
        {
            attemptCount++;
            if (attemptCount == 1)
            {
                // First attempt fails with timeout
                return Result.Fail("TIMEOUT", "Operation timed out");
            }
            // Second attempt succeeds
            return Result.Ok();
        }

        // Act
        var result = await retryPolicy.ExecuteAsync(OperationWithTransientFailure, "TestOperation");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, attemptCount);  // Verify retry happened
    }

    /// <summary>
    /// Test 5: Create-Update Pattern Collapse
    /// Local: Order created (timestamp: t1), then updated (timestamp: t2)
    /// Expected: SyncBatchOptimizer collapses to single Create change with final values
    /// </summary>
    [Fact]
    public async Task Scenario5_CreateUpdatePatternCollapse()
    {
        // Arrange
        var changeCapture = new ChangeDataCapture();
        var batchOptimizer = new SyncBatchOptimizer();
        var now = DateTime.UtcNow;

        // Create order
        var createChange = CreateChange(
            entityId: "Order1",
            property: "Amount",
            oldValue: null,
            newValue: "100.00",
            timestamp: now,
            entityType: "Order");

        // Update amount
        var updateChange = CreateChange(
            entityId: "Order1",
            property: "Amount",
            oldValue: "100.00",
            newValue: "150.00",
            timestamp: now.AddSeconds(5),
            entityType: "Order");

        await changeCapture.RecordChangeAsync(createChange);
        await changeCapture.RecordChangeAsync(updateChange);

        // Act
        var changes = await changeCapture.GetChangesAsync();
        var batch = await batchOptimizer.CreateBatchAsync(changes.Data);

        // Assert
        Assert.Equal(2, batch.Data.ChangeCount);  // Original: 2 changes
        Assert.Equal(1, batch.Data.DeduplicatedCount);  // After collapse: 1 change

        var collapsedChange = batch.Data.Changes.Single();
        Assert.Equal("Order1", collapsedChange.EntityId);
        Assert.Equal("Amount", collapsedChange.Property);
        Assert.Null(collapsedChange.OldValue);  // Create's OldValue
        Assert.Equal("150.00", collapsedChange.NewValue);  // Update's final NewValue
    }

    /// <summary>
    /// Test 6: Update-Delete Pattern Collapse
    /// Local: Order updated (timestamp: t1), then deleted (timestamp: t2)
    /// Expected: SyncBatchOptimizer collapses to single Delete change
    /// </summary>
    [Fact]
    public async Task Scenario6_UpdateDeletePatternCollapse()
    {
        // Arrange
        var changeCapture = new ChangeDataCapture();
        var batchOptimizer = new SyncBatchOptimizer();
        var now = DateTime.UtcNow;

        // Update order
        var updateChange = CreateChange(
            entityId: "Order1",
            property: "Status",
            oldValue: "Pending",
            newValue: "Processing",
            timestamp: now,
            entityType: "Order");

        // Delete order
        var deleteChange = CreateChange(
            entityId: "Order1",
            property: "Status",
            oldValue: "Processing",
            newValue: null,
            timestamp: now.AddSeconds(5),
            entityType: "Order");

        await changeCapture.RecordChangeAsync(updateChange);
        await changeCapture.RecordChangeAsync(deleteChange);

        // Act
        var changes = await changeCapture.GetChangesAsync();
        var batch = await batchOptimizer.CreateBatchAsync(changes.Data);

        // Assert
        Assert.Equal(2, batch.Data.ChangeCount);  // Original: 2 changes
        Assert.Equal(1, batch.Data.DeduplicatedCount);  // After collapse: 1 change

        var collapsedChange = batch.Data.Changes.Single();
        Assert.Equal("Order1", collapsedChange.EntityId);
        Assert.Equal("Status", collapsedChange.Property);
        Assert.Equal("Pending", collapsedChange.OldValue);  // Update's original OldValue
        Assert.Null(collapsedChange.NewValue);  // Delete's NewValue
        Assert.True(collapsedChange.IsDelete);
    }

    /// <summary>
    /// Test 7: Persistent State Across Operations
    /// Record pending changes to FileSystemSyncStateStore
    /// Simulate app restart: load state from store
    /// Continue sync from persisted state
    /// Expected: All changes recovered, no loss
    /// </summary>
    [Fact]
    public async Task Scenario7_PersistentState_AcrossOperations()
    {
        // Arrange
        var store = new FileSystemSyncStateStore(_tempStoragePath);
        await store.InitializeAsync();

        var now = DateTime.UtcNow;
        var changes = new List<SyncChange>
        {
            CreateChange("Order1", "Status", "Pending", "Processing", now, "Order"),
            CreateChange("Order1", "Amount", "100.00", "150.00", now.AddSeconds(1), "Order"),
            CreateChange("Order2", "Status", null, "Confirmed", now.AddSeconds(2), "Order")
        };

        // Act - Save changes
        var saveResult = await store.SavePendingChangesAsync("Order", changes);
        Assert.True(saveResult.Succeeded);

        // Simulate app restart - Load from storage
        var store2 = new FileSystemSyncStateStore(_tempStoragePath);
        await store2.InitializeAsync();

        var loadResult = await store2.LoadPendingChangesAsync("Order");

        // Assert
        Assert.True(loadResult.Succeeded);
        Assert.Equal(3, loadResult.Data.Count);

        // Verify all changes were recovered
        Assert.Contains(loadResult.Data, c => c.EntityId == "Order1" && c.Property == "Status");
        Assert.Contains(loadResult.Data, c => c.EntityId == "Order1" && c.Property == "Amount");
        Assert.Contains(loadResult.Data, c => c.EntityId == "Order2" && c.Property == "Status");

        // Verify change values
        var order1Status = loadResult.Data.First(c => c.EntityId == "Order1" && c.Property == "Status");
        Assert.Equal("Processing", order1Status.NewValue?.ToString());
    }

    /// <summary>
    /// Test 8: Three-Way Conflict Resolution
    /// Local changes: Property A, Property B
    /// Remote changes: Property A, Property C (overlaps A, new C)
    /// Conflict on Property A, no conflict on B and C
    /// Resolution: Custom strategy
    /// Expected: Resolved changes stored, sync can proceed
    /// </summary>
    [Fact]
    public async Task Scenario8_ThreeWayConflictResolution()
    {
        // Arrange
        var changeCapture = new ChangeDataCapture();
        var conflictDetector = new ConflictDetector();
        var resolutionEngine = new ConflictResolutionEngine();
        var batchOptimizer = new SyncBatchOptimizer();
        var now = DateTime.UtcNow;

        // Local changes: Property A and B
        var localChangeA = CreateChange(
            entityId: "Order1",
            property: "Status",
            oldValue: "Pending",
            newValue: "Processing",
            timestamp: now,
            entityType: "Order");

        var localChangeB = CreateChange(
            entityId: "Order1",
            property: "Amount",
            oldValue: "100.00",
            newValue: "125.00",
            timestamp: now.AddSeconds(1),
            entityType: "Order");

        await changeCapture.RecordChangeAsync(localChangeA);
        await changeCapture.RecordChangeAsync(localChangeB);

        // Remote changes: Property A (conflict) and C (no conflict)
        var remoteChangeA = CreateChange(
            entityId: "Order1",
            property: "Status",
            oldValue: "Pending",
            newValue: "Confirmed",  // Different from local
            timestamp: now.AddSeconds(-1),
            entityType: "Order");

        var remoteChangeC = CreateChange(
            entityId: "Order1",
            property: "Priority",
            oldValue: null,
            newValue: "High",
            timestamp: now.AddSeconds(2),
            entityType: "Order");

        // Act
        var localChanges = await changeCapture.GetChangesAsync();

        // Detect conflicts (only A should conflict)
        var conflicts = await conflictDetector.DetectConflictsAsync(
            localChanges.Data,
            new[] { remoteChangeA, remoteChangeC });

        Assert.Single(conflicts.Data);  // Only one conflict (Property A)

        var conflict = conflicts.Data[0];
        // Register custom resolver (using ClientWins strategy)
        resolutionEngine.RegisterResolver(
            ConflictResolutionStrategy.ClientWins,
            new ClientWinsResolver(null));

        conflict = new SyncConflict(
            conflict.ConflictId,
            conflict.LocalChange,
            conflict.RemoteChange,
            ConflictResolutionStrategy.ClientWins,  // Use ClientWins for this conflict
            conflict.DetectedAt);

        var resolved = await resolutionEngine.ResolveAsync(conflict);

        // Create list of all changes: resolved conflict + non-conflicting remote + non-conflicting local
        var allChanges = new List<SyncChange>
        {
            resolved,  // Resolved conflict change
            localChangeB,  // Local change B (no conflict)
            remoteChangeC  // Remote change C (no conflict)
        };

        var finalBatch = await batchOptimizer.CreateBatchAsync(allChanges);

        // Assert
        Assert.True(conflicts.Succeeded);
        Assert.Single(conflicts.Data);
        Assert.NotNull(resolved);
        Assert.Equal("Processing", resolved.NewValue);  // ClientWins chose local
        Assert.True(finalBatch.Succeeded);
        Assert.Equal(3, finalBatch.Data.Changes.Count);
    }

    /// <summary>
    /// Test 9: Batch Splitting with Large Change Sets
    /// Create multiple changes exceeding batch size limit
    /// Expected: Changes split into multiple batches, each respecting limit
    /// </summary>
    [Fact]
    public async Task Scenario9_BatchSplitting_LargeChangeSets()
    {
        // Arrange
        var batchOptimizer = new SyncBatchOptimizer();
        var changeCapture = new ChangeDataCapture();
        var now = DateTime.UtcNow;

        // Create 15 changes
        for (int i = 0; i < 15; i++)
        {
            var change = CreateChange(
                entityId: $"Order{i}",
                property: "Status",
                oldValue: "Pending",
                newValue: "Processing",
                timestamp: now.AddSeconds(i),
                entityType: "Order");

            await changeCapture.RecordChangeAsync(change);
        }

        // Act
        var changes = await changeCapture.GetChangesAsync();
        var batches = await batchOptimizer.SplitIntoBatchesAsync(changes.Data, maxChangesPerBatch: 5);

        // Assert
        Assert.True(batches.Succeeded);
        Assert.Equal(3, batches.Data.Count);  // 15 changes / 5 per batch = 3 batches
        Assert.Equal(5, batches.Data[0].Changes.Count);
        Assert.Equal(5, batches.Data[1].Changes.Count);
        Assert.Equal(5, batches.Data[2].Changes.Count);
    }

    /// <summary>
    /// Test 10: Sync Session Persistence and Recovery
    /// Record sync session with conflict information
    /// Load session and verify all metadata preserved
    /// Expected: Session state persists across operations
    /// </summary>
    [Fact]
    public async Task Scenario10_SyncSessionPersistence_AndRecovery()
    {
        // Arrange
        var store = new FileSystemSyncStateStore(_tempStoragePath);
        await store.InitializeAsync();

        var sessionId = Guid.NewGuid().ToString();
        var now = DateTime.UtcNow;
        var session = new SyncSessionInfo(
            SessionId: sessionId,
            EntityType: "Order",
            StartTime: now,
            EndTime: now.AddSeconds(5),
            IsSuccessful: true,
            ErrorMessage: null,
            ChangesProcessed: 5,
            ConflictsResolved: 2);

        // Act - Record session
        var recordResult = await store.RecordSyncSessionAsync(session);
        Assert.True(recordResult.Succeeded);

        // Load sessions
        var sessionsResult = await store.GetSyncSessionsAsync(limit: 10);

        // Assert
        Assert.True(sessionsResult.Succeeded);
        Assert.NotEmpty(sessionsResult.Data);

        var loadedSession = sessionsResult.Data.FirstOrDefault(s => s.SessionId == sessionId);
        Assert.NotNull(loadedSession);
        Assert.Equal("Order", loadedSession.EntityType);
        Assert.True(loadedSession.IsSuccessful);
        Assert.Equal(2, loadedSession.ConflictsResolved);
        Assert.Equal(5, loadedSession.ChangesProcessed);
    }

    /// <summary>
    /// Helper method to create SyncChange with realistic values
    /// </summary>
    private SyncChange CreateChange(
        string entityId,
        string property,
        object? oldValue,
        object? newValue,
        DateTime timestamp,
        string entityType = "Order",
        string userId = "user1")
    {
        return new SyncChange(
            EntityId: entityId,
            EntityType: entityType,
            Property: property,
            OldValue: oldValue,
            NewValue: newValue,
            Timestamp: timestamp,
            UserId: userId,
            ChangeId: Guid.NewGuid().ToString());
    }
}
