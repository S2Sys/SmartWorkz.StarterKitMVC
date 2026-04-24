namespace SmartWorkz.Mobile.Tests.Persistence;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Persistence;
using SmartWorkz.Shared;

/// <summary>
/// Unit tests for FileSystemSyncStateStore persistence service.
/// Tests database initialization, pending changes, sync sessions, and cleanup.
/// </summary>
public class FileSystemSyncStateStoreTests : IAsyncLifetime
{
    private readonly string _testDatabasePath;
    private FileSystemSyncStateStore? _store;

    public FileSystemSyncStateStoreTests()
    {
        _testDatabasePath = Path.Combine(Path.GetTempPath(), $"sync_state_{Guid.NewGuid()}.db");
    }

    public async Task InitializeAsync()
    {
        _store = new FileSystemSyncStateStore(_testDatabasePath);
        var result = await _store.InitializeAsync();
        Assert.True(result.Succeeded, "Failed to initialize database");
    }

    public async Task DisposeAsync()
    {
        if (_store != null)
        {
            _store.Dispose();
        }

        try
        {
            // Clean up all test files
            var dir = Path.GetDirectoryName(_testDatabasePath);
            if (dir != null && Directory.Exists(dir))
            {
                var storeDir = Path.Combine(dir, "sync_state_store");
                if (Directory.Exists(storeDir))
                {
                    Directory.Delete(storeDir, true);
                }
            }

            if (File.Exists(_testDatabasePath))
            {
                File.Delete(_testDatabasePath);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task InitializeAsync_CreatesDatabase_Succeeds()
    {
        // Arrange - fresh store without initialize
        using var newStore = new FileSystemSyncStateStore(
            Path.Combine(Path.GetTempPath(), $"sync_init_{Guid.NewGuid()}.db"));

        // Act
        var result = await newStore.InitializeAsync();

        // Assert
        Assert.True(result.Succeeded);
        var checkResult = await newStore.IsInitializedAsync();
        Assert.True(checkResult.Succeeded);
        Assert.True(checkResult.Data);

        // Cleanup
        newStore.Dispose();
        if (File.Exists(newStore.DatabasePath))
        {
            File.Delete(newStore.DatabasePath);
        }
    }

    [Fact]
    public async Task SavePendingChangesAsync_WithValidChanges_Persists()
    {
        // Arrange
        var changes = new List<SyncChange>
        {
            new("123", "Order", "Status", "Pending", "Confirmed", DateTime.UtcNow, "user1"),
            new("124", "Order", "Total", 100m, 150m, DateTime.UtcNow, "user1"),
            new("125", "Order", "Items", "[]", "[{Id:1}]", DateTime.UtcNow, "user1")
        };

        // Act
        var saveResult = await _store!.SavePendingChangesAsync("Order", changes);

        // Assert
        Assert.True(saveResult.Succeeded);

        // Verify by loading
        var loadResult = await _store.LoadPendingChangesAsync("Order");
        Assert.True(loadResult.Succeeded);
        Assert.NotNull(loadResult.Data);
        Assert.Equal(3, loadResult.Data.Count);
        Assert.Equal("123", loadResult.Data[0].EntityId);
        // Check that value was persisted (type is dynamic, so check string representation)
        Assert.NotNull(loadResult.Data[0].NewValue);
        Assert.Equal("Confirmed", loadResult.Data[0].NewValue?.ToString());
    }

    [Fact]
    public async Task LoadPendingChangesAsync_WithNoChanges_ReturnsEmpty()
    {
        // Arrange - EntityType with no saved changes

        // Act
        var result = await _store!.LoadPendingChangesAsync("NonExistentEntity");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Empty(result.Data ?? []);
    }

    [Fact]
    public async Task ClearPendingChangesAsync_RemovesChanges()
    {
        // Arrange
        var changes = new List<SyncChange>
        {
            new("123", "Order", "Status", "Pending", "Confirmed", DateTime.UtcNow, "user1"),
            new("124", "Order", "Total", 100m, 150m, DateTime.UtcNow, "user1")
        };
        await _store!.SavePendingChangesAsync("Order", changes);

        // Act
        var clearResult = await _store.ClearPendingChangesAsync("Order");

        // Assert
        Assert.True(clearResult.Succeeded);

        // Verify cleared
        var loadResult = await _store.LoadPendingChangesAsync("Order");
        Assert.True(loadResult.Succeeded);
        Assert.Empty(loadResult.Data ?? []);
    }

    [Fact]
    public async Task RecordSyncSessionAsync_SavesSession()
    {
        // Arrange
        var session = new SyncSessionInfo(
            SessionId: Guid.NewGuid().ToString(),
            EntityType: "Order",
            StartTime: DateTime.UtcNow.AddSeconds(-30),
            EndTime: DateTime.UtcNow,
            IsSuccessful: true,
            ErrorMessage: null,
            ChangesProcessed: 5,
            ConflictsResolved: 0);

        // Act
        var result = await _store!.RecordSyncSessionAsync(session);

        // Assert
        Assert.True(result.Succeeded);

        // Verify by retrieving
        var sessionsResult = await _store.GetSyncSessionsAsync(10);
        Assert.True(sessionsResult.Succeeded);
        Assert.NotEmpty(sessionsResult.Data ?? []);
        Assert.Contains(sessionsResult.Data!, s => s.SessionId == session.SessionId);
    }

    [Fact]
    public async Task GetSyncStateAsync_ReturnsCorrectState()
    {
        // Arrange
        var changes = new List<SyncChange>
        {
            new("123", "Customer", "Name", "John", "Jane", DateTime.UtcNow, "user1"),
            new("124", "Customer", "Email", "old@ex.com", "new@ex.com", DateTime.UtcNow, "user1")
        };
        await _store!.SavePendingChangesAsync("Customer", changes);

        // Act
        var result = await _store.GetSyncStateAsync("Customer");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("Customer", result.Data!.EntityType);
        Assert.Equal(2, result.Data.PendingChangesCount);
        Assert.True(result.Data.NeedsSync);
    }

    [Fact]
    public async Task GetSyncSessionsAsync_WithLimit_RespectsLimit()
    {
        // Arrange - Record 5 sessions
        for (int i = 0; i < 5; i++)
        {
            var session = new SyncSessionInfo(
                SessionId: $"session_{i}",
                EntityType: "Order",
                StartTime: DateTime.UtcNow.AddSeconds(-60 + (i * 10)),
                EndTime: DateTime.UtcNow.AddSeconds(-30 + (i * 10)),
                IsSuccessful: true,
                ErrorMessage: null,
                ChangesProcessed: i + 1,
                ConflictsResolved: 0);
            await _store!.RecordSyncSessionAsync(session);
        }

        // Act
        var result = await _store.GetSyncSessionsAsync(limit: 3);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Count);
    }

    [Fact]
    public async Task DeleteOldSessionsAsync_RemovesOldSessions()
    {
        // Arrange - Record old and new sessions
        var oldTime = DateTime.UtcNow.AddDays(-10);
        var newTime = DateTime.UtcNow.AddHours(-1);

        var oldSession = new SyncSessionInfo(
            SessionId: "old_session",
            EntityType: "Order",
            StartTime: oldTime.AddSeconds(-60),
            EndTime: oldTime,
            IsSuccessful: true,
            ErrorMessage: null,
            ChangesProcessed: 5,
            ConflictsResolved: 0);

        var newSession = new SyncSessionInfo(
            SessionId: "new_session",
            EntityType: "Order",
            StartTime: newTime.AddSeconds(-60),
            EndTime: newTime,
            IsSuccessful: true,
            ErrorMessage: null,
            ChangesProcessed: 3,
            ConflictsResolved: 0);

        await _store!.RecordSyncSessionAsync(oldSession);
        await _store.RecordSyncSessionAsync(newSession);

        // Act
        var cutoffTime = DateTime.UtcNow.AddDays(-5);
        var deleteResult = await _store.DeleteOldSessionsAsync(cutoffTime);

        // Assert
        Assert.True(deleteResult.Succeeded);

        // Verify old session deleted, new session remains
        var sessionsResult = await _store.GetSyncSessionsAsync(10);
        Assert.True(sessionsResult.Succeeded);
        Assert.DoesNotContain(sessionsResult.Data!, s => s.SessionId == "old_session");
        Assert.Contains(sessionsResult.Data!, s => s.SessionId == "new_session");
    }

    [Fact]
    public async Task IsInitializedAsync_BeforeInit_ReturnsFalse()
    {
        // Arrange
        using var uninitializedStore = new FileSystemSyncStateStore(
            Path.Combine(Path.GetTempPath(), $"uninit_{Guid.NewGuid()}.db"));

        // Act
        var beforeInit = await uninitializedStore.IsInitializedAsync();

        // Initialize
        await uninitializedStore.InitializeAsync();
        var afterInit = await uninitializedStore.IsInitializedAsync();

        // Assert
        Assert.False(beforeInit.Data);
        Assert.True(afterInit.Data);

        // Cleanup
        uninitializedStore.Dispose();
        if (File.Exists(uninitializedStore.DatabasePath))
        {
            File.Delete(uninitializedStore.DatabasePath);
        }
    }

    [Fact]
    public async Task SavePendingChangesAsync_WithNullChanges_ReturnsFail()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _store!.SavePendingChangesAsync("Order", null!));
    }

    [Fact]
    public async Task SavePendingChangesAsync_WithEmptyChanges_ClearsChanges()
    {
        // Arrange - save initial changes
        var initialChanges = new List<SyncChange>
        {
            new("123", "Order", "Status", "Pending", "Confirmed", DateTime.UtcNow, "user1")
        };
        await _store!.SavePendingChangesAsync("Order", initialChanges);

        // Act - save empty list
        var result = await _store.SavePendingChangesAsync("Order", []);

        // Assert
        Assert.True(result.Succeeded);
        var loadResult = await _store.LoadPendingChangesAsync("Order");
        Assert.Empty(loadResult.Data ?? []);
    }

    [Fact]
    public async Task MultipleEntityTypes_MaintainSeparateState()
    {
        // Arrange
        var orderChanges = new List<SyncChange>
        {
            new("O1", "Order", "Status", "Pending", "Confirmed", DateTime.UtcNow, "user1")
        };
        var customerChanges = new List<SyncChange>
        {
            new("C1", "Customer", "Name", "John", "Jane", DateTime.UtcNow, "user1"),
            new("C2", "Customer", "Email", "a@b.com", "c@d.com", DateTime.UtcNow, "user1")
        };

        // Act
        await _store!.SavePendingChangesAsync("Order", orderChanges);
        await _store.SavePendingChangesAsync("Customer", customerChanges);

        var orderState = await _store.GetSyncStateAsync("Order");
        var customerState = await _store.GetSyncStateAsync("Customer");

        // Assert
        Assert.Equal(1, orderState.Data?.PendingChangesCount);
        Assert.Equal(2, customerState.Data?.PendingChangesCount);

        var orderLoaded = await _store.LoadPendingChangesAsync("Order");
        var customerLoaded = await _store.LoadPendingChangesAsync("Customer");
        Assert.Single(orderLoaded.Data!);
        Assert.Equal(2, customerLoaded.Data!.Count);
    }

    [Fact]
    public async Task RecordSyncSessionAsync_WithFailedSession_StoresErrorMessage()
    {
        // Arrange
        var failedSession = new SyncSessionInfo(
            SessionId: Guid.NewGuid().ToString(),
            EntityType: "Order",
            StartTime: DateTime.UtcNow.AddSeconds(-30),
            EndTime: DateTime.UtcNow,
            IsSuccessful: false,
            ErrorMessage: "Network timeout during sync",
            ChangesProcessed: 2,
            ConflictsResolved: 0);

        // Act
        await _store!.RecordSyncSessionAsync(failedSession);

        // Verify
        var state = await _store.GetSyncStateAsync("Order");
        Assert.NotNull(state.Data?.LastErrorMessage);
        Assert.Equal("Network timeout during sync", state.Data.LastErrorMessage);
    }

    [Fact]
    public async Task RecordSyncSessionAsync_WithSuccessfulSessionAfterFailure_ClearsErrorMessage()
    {
        // Arrange
        var store = _store!;

        // Record a failed session first
        var failedSession = new SyncSessionInfo(
            SessionId: "session1",
            EntityType: "Order",
            StartTime: DateTime.UtcNow.AddMinutes(-5),
            EndTime: DateTime.UtcNow.AddMinutes(-3),
            IsSuccessful: false,
            ErrorMessage: "Network timeout",
            ChangesProcessed: 0,
            ConflictsResolved: 0);
        await store.RecordSyncSessionAsync(failedSession);

        // Verify error is stored
        var stateAfterFailure = await store.GetSyncStateAsync("Order");
        Assert.Equal("Network timeout", stateAfterFailure.Data!.LastErrorMessage);

        // Record a successful session
        var successSession = new SyncSessionInfo(
            SessionId: "session2",
            EntityType: "Order",
            StartTime: DateTime.UtcNow.AddMinutes(-2),
            EndTime: DateTime.UtcNow,
            IsSuccessful: true,
            ErrorMessage: null,
            ChangesProcessed: 5,
            ConflictsResolved: 0);
        await store.RecordSyncSessionAsync(successSession);

        // Assert: error message should be cleared
        var stateAfterSuccess = await store.GetSyncStateAsync("Order");
        Assert.Null(stateAfterSuccess.Data!.LastErrorMessage);
    }
}
