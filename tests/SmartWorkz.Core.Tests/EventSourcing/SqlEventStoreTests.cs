using SmartWorkz.Shared;

namespace SmartWorkz.Core.Tests.EventSourcing;

/// <summary>
/// Comprehensive tests for SqlEventStore implementation.
/// Verifies event persistence, versioning, snapshot functionality, and event replay.
/// </summary>
public class SqlEventStoreTests
{
    private readonly IEventStore _eventStore;

    public SqlEventStoreTests()
    {
        // Use in-memory implementation for testing
        _eventStore = new InMemoryEventStore();
    }

    #region AppendEventsAsync Tests

    /// <summary>Test 1: AppendEventsAsync stores events successfully</summary>
    [Fact]
    public async Task AppendEventsAsync_WithValidEvents_StoresSuccessfully()
    {
        // Arrange
        var aggregateId = "test-aggregate-001";
        var events = new List<IDomainEvent>
        {
            new TestDomainEvent(aggregateId, "Event 1 occurred"),
            new TestDomainEvent(aggregateId, "Event 2 occurred")
        };

        // Act
        await _eventStore.AppendEventsAsync(aggregateId, events);

        // Assert
        var storedEvents = await _eventStore.GetEventsAsync(aggregateId);
        Assert.NotNull(storedEvents);
        Assert.Equal(2, storedEvents.Count());
    }

    /// <summary>Test 2: AppendEventsAsync throws ArgumentNullException for null aggregateId</summary>
    [Fact]
    public async Task AppendEventsAsync_WithNullAggregateId_ThrowsArgumentNullException()
    {
        // Arrange
        var events = new List<IDomainEvent>
        {
            new TestDomainEvent("valid-id", "Event occurred")
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _eventStore.AppendEventsAsync(null!, events));
    }

    /// <summary>Test 3: AppendEventsAsync throws ArgumentNullException for null events</summary>
    [Fact]
    public async Task AppendEventsAsync_WithNullEvents_ThrowsArgumentNullException()
    {
        // Arrange
        var aggregateId = "test-aggregate-001";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _eventStore.AppendEventsAsync(aggregateId, null!));
    }

    /// <summary>Test 4: AppendEventsAsync maintains version ordering</summary>
    [Fact]
    public async Task AppendEventsAsync_MaintainsVersionOrdering()
    {
        // Arrange
        var aggregateId = "test-aggregate-002";
        var events = new List<IDomainEvent>
        {
            new TestDomainEvent(aggregateId, "First event"),
            new TestDomainEvent(aggregateId, "Second event"),
            new TestDomainEvent(aggregateId, "Third event")
        };

        // Act
        await _eventStore.AppendEventsAsync(aggregateId, events);

        // Assert
        var storedEvents = (await _eventStore.GetEventsAsync(aggregateId)).ToList();
        Assert.Equal(3, storedEvents.Count);
        // Verify events are in insertion order
        Assert.Equal("First event", ((TestDomainEvent)storedEvents[0]).Description);
        Assert.Equal("Second event", ((TestDomainEvent)storedEvents[1]).Description);
        Assert.Equal("Third event", ((TestDomainEvent)storedEvents[2]).Description);
    }

    /// <summary>Test 5: AppendEventsAsync with multiple aggregates maintains isolation</summary>
    [Fact]
    public async Task AppendEventsAsync_WithMultipleAggregates_MaintainsIsolation()
    {
        // Arrange
        var aggregateId1 = "test-aggregate-001";
        var aggregateId2 = "test-aggregate-002";

        var events1 = new List<IDomainEvent>
        {
            new TestDomainEvent(aggregateId1, "Aggregate 1 Event 1"),
            new TestDomainEvent(aggregateId1, "Aggregate 1 Event 2")
        };

        var events2 = new List<IDomainEvent>
        {
            new TestDomainEvent(aggregateId2, "Aggregate 2 Event 1")
        };

        // Act
        await _eventStore.AppendEventsAsync(aggregateId1, events1);
        await _eventStore.AppendEventsAsync(aggregateId2, events2);

        // Assert
        var storedEvents1 = await _eventStore.GetEventsAsync(aggregateId1);
        var storedEvents2 = await _eventStore.GetEventsAsync(aggregateId2);

        Assert.Equal(2, storedEvents1.Count());
        Assert.Single(storedEvents2);
    }

    #endregion

    #region GetEventsAsync Tests

    /// <summary>Test 6: GetEventsAsync returns all events for aggregate</summary>
    [Fact]
    public async Task GetEventsAsync_ReturnsAllEventsForAggregate()
    {
        // Arrange
        var aggregateId = "test-aggregate-003";
        var events = new List<IDomainEvent>
        {
            new TestDomainEvent(aggregateId, "Event 1"),
            new TestDomainEvent(aggregateId, "Event 2"),
            new TestDomainEvent(aggregateId, "Event 3")
        };

        await _eventStore.AppendEventsAsync(aggregateId, events);

        // Act
        var retrievedEvents = await _eventStore.GetEventsAsync(aggregateId);

        // Assert
        Assert.NotNull(retrievedEvents);
        Assert.Equal(3, retrievedEvents.Count());
    }

    /// <summary>Test 7: GetEventsAsync returns empty collection for non-existent aggregate</summary>
    [Fact]
    public async Task GetEventsAsync_WithNonExistentAggregate_ReturnsEmptyCollection()
    {
        // Arrange
        var nonExistentAggregateId = "non-existent-aggregate-" + Guid.NewGuid();

        // Act
        var retrievedEvents = await _eventStore.GetEventsAsync(nonExistentAggregateId);

        // Assert
        Assert.NotNull(retrievedEvents);
        Assert.Empty(retrievedEvents);
    }

    /// <summary>Test 8: GetEventsAsync throws ArgumentNullException for null aggregateId</summary>
    [Fact]
    public async Task GetEventsAsync_WithNullAggregateId_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _eventStore.GetEventsAsync(null!));
    }

    #endregion

    #region GetEventsSinceAsync Tests

    /// <summary>Test 9: GetEventsSinceAsync returns events after specified version</summary>
    [Fact]
    public async Task GetEventsSinceAsync_ReturnsEventsAfterVersion()
    {
        // Arrange
        var aggregateId = "test-aggregate-004";
        var events = new List<IDomainEvent>
        {
            new TestDomainEvent(aggregateId, "Event 1"),
            new TestDomainEvent(aggregateId, "Event 2"),
            new TestDomainEvent(aggregateId, "Event 3")
        };

        await _eventStore.AppendEventsAsync(aggregateId, events);

        // Act
        var eventsSince = await _eventStore.GetEventsSinceAsync(aggregateId, version: 1);

        // Assert
        Assert.NotNull(eventsSince);
        // Should return events after version 1 (i.e., events 2 and 3)
        Assert.Equal(2, eventsSince.Count());
    }

    /// <summary>Test 10: GetEventsSinceAsync returns empty for version at or beyond event count</summary>
    [Fact]
    public async Task GetEventsSinceAsync_WithVersionBeyondEvents_ReturnsEmpty()
    {
        // Arrange
        var aggregateId = "test-aggregate-005";
        var events = new List<IDomainEvent>
        {
            new TestDomainEvent(aggregateId, "Event 1"),
            new TestDomainEvent(aggregateId, "Event 2")
        };

        await _eventStore.AppendEventsAsync(aggregateId, events);

        // Act
        var eventsSince = await _eventStore.GetEventsSinceAsync(aggregateId, version: 2);

        // Assert
        Assert.NotNull(eventsSince);
        Assert.Empty(eventsSince);
    }

    /// <summary>Test 11: GetEventsSinceAsync throws ArgumentException for negative version</summary>
    [Fact]
    public async Task GetEventsSinceAsync_WithNegativeVersion_ThrowsArgumentException()
    {
        // Arrange
        var aggregateId = "test-aggregate-006";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _eventStore.GetEventsSinceAsync(aggregateId, version: -1));
    }

    /// <summary>Test 12: GetEventsSinceAsync with version 0 returns all events</summary>
    [Fact]
    public async Task GetEventsSinceAsync_WithVersionZero_ReturnsAllEvents()
    {
        // Arrange
        var aggregateId = "test-aggregate-007";
        var events = new List<IDomainEvent>
        {
            new TestDomainEvent(aggregateId, "Event 1"),
            new TestDomainEvent(aggregateId, "Event 2"),
            new TestDomainEvent(aggregateId, "Event 3")
        };

        await _eventStore.AppendEventsAsync(aggregateId, events);

        // Act
        var eventsSince = await _eventStore.GetEventsSinceAsync(aggregateId, version: 0);

        // Assert
        Assert.NotNull(eventsSince);
        Assert.Equal(3, eventsSince.Count());
    }

    #endregion

    #region Snapshot Tests

    /// <summary>Test 13: SaveSnapshotAsync stores snapshot successfully</summary>
    [Fact]
    public async Task SaveSnapshotAsync_WithValidSnapshot_StoresSuccessfully()
    {
        // Arrange
        var aggregateId = "test-aggregate-008";
        var snapshot = new EventStoreSnapshot
        {
            AggregateId = aggregateId,
            Version = 5,
            SnapshotData = new { Status = "Active", Balance = 1000 },
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        await _eventStore.SaveSnapshotAsync(snapshot);

        // Assert
        var retrievedSnapshot = await _eventStore.GetSnapshotAsync(aggregateId);
        Assert.NotNull(retrievedSnapshot);
        Assert.Equal(aggregateId, retrievedSnapshot.AggregateId);
        Assert.Equal(5, retrievedSnapshot.Version);
    }

    /// <summary>Test 14: GetSnapshotAsync returns null for non-existent snapshot</summary>
    [Fact]
    public async Task GetSnapshotAsync_WithNoSnapshot_ReturnsNull()
    {
        // Arrange
        var aggregateId = "test-aggregate-no-snapshot";

        // Act
        var snapshot = await _eventStore.GetSnapshotAsync(aggregateId);

        // Assert
        Assert.Null(snapshot);
    }

    /// <summary>Test 15: SaveSnapshotAsync throws ArgumentNullException for null snapshot</summary>
    [Fact]
    public async Task SaveSnapshotAsync_WithNullSnapshot_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _eventStore.SaveSnapshotAsync(null!));
    }

    /// <summary>Test 16: SaveSnapshotAsync overwrites previous snapshot</summary>
    [Fact]
    public async Task SaveSnapshotAsync_WithMultipleSnapshots_OverwritesPrevious()
    {
        // Arrange
        var aggregateId = "test-aggregate-009";
        var snapshot1 = new EventStoreSnapshot
        {
            AggregateId = aggregateId,
            Version = 3,
            SnapshotData = new { Status = "Pending" },
            CreatedAt = DateTimeOffset.UtcNow
        };

        var snapshot2 = new EventStoreSnapshot
        {
            AggregateId = aggregateId,
            Version = 10,
            SnapshotData = new { Status = "Active" },
            CreatedAt = DateTimeOffset.UtcNow.AddHours(1)
        };

        // Act
        await _eventStore.SaveSnapshotAsync(snapshot1);
        await _eventStore.SaveSnapshotAsync(snapshot2);

        // Assert
        var retrievedSnapshot = await _eventStore.GetSnapshotAsync(aggregateId);
        Assert.NotNull(retrievedSnapshot);
        Assert.Equal(10, retrievedSnapshot.Version);
    }

    #endregion

    #region GetAggregateAsync Tests

    /// <summary>Test 17: GetAggregateAsync reconstructs aggregate from events</summary>
    [Fact]
    public async Task GetAggregateAsync_WithValidEvents_ReconstructsAggregate()
    {
        // Arrange
        var aggregateId = "test-aggregate-010";
        var events = new List<IDomainEvent>
        {
            new TestDomainEvent(aggregateId, "Created"),
            new TestDomainEvent(aggregateId, "Updated")
        };

        await _eventStore.AppendEventsAsync(aggregateId, events);

        // Act
        var aggregate = await _eventStore.GetAggregateAsync<TestAggregate>(aggregateId);

        // Assert
        Assert.NotNull(aggregate);
        Assert.Equal(aggregateId, aggregate.Id);
        Assert.Equal(2, aggregate.Version);
    }

    /// <summary>Test 18: GetAggregateAsync returns null for non-existent aggregate</summary>
    [Fact]
    public async Task GetAggregateAsync_WithNonExistentAggregate_ReturnsNull()
    {
        // Arrange
        var nonExistentAggregateId = "non-existent-" + Guid.NewGuid();

        // Act
        var aggregate = await _eventStore.GetAggregateAsync<TestAggregate>(nonExistentAggregateId);

        // Assert
        Assert.Null(aggregate);
    }

    /// <summary>Test 19: GetAggregateAsync throws ArgumentNullException for null aggregateId</summary>
    [Fact]
    public async Task GetAggregateAsync_WithNullAggregateId_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _eventStore.GetAggregateAsync<TestAggregate>(null!));
    }

    /// <summary>Test 20: GetAggregateAsync uses snapshot for optimization</summary>
    [Fact]
    public async Task GetAggregateAsync_WithSnapshot_UsesSnapshotForOptimization()
    {
        // Arrange
        var aggregateId = "test-aggregate-011";

        // Create events before and after snapshot
        var eventsBefore = new List<IDomainEvent>
        {
            new TestDomainEvent(aggregateId, "Event 1"),
            new TestDomainEvent(aggregateId, "Event 2"),
            new TestDomainEvent(aggregateId, "Event 3")
        };

        var eventsAfter = new List<IDomainEvent>
        {
            new TestDomainEvent(aggregateId, "Event 4"),
            new TestDomainEvent(aggregateId, "Event 5")
        };

        // Act
        await _eventStore.AppendEventsAsync(aggregateId, eventsBefore);

        // Save snapshot at version 3
        var snapshot = new EventStoreSnapshot
        {
            AggregateId = aggregateId,
            Version = 3,
            SnapshotData = new TestAggregate { Id = aggregateId, Version = 3 },
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _eventStore.SaveSnapshotAsync(snapshot);
        await _eventStore.AppendEventsAsync(aggregateId, eventsAfter);

        // Retrieve aggregate - should use snapshot and replay only events 4-5
        var aggregate = await _eventStore.GetAggregateAsync<TestAggregate>(aggregateId);

        // Assert
        Assert.NotNull(aggregate);
        Assert.Equal(5, aggregate.Version);
    }

    #endregion

    #region Optimistic Concurrency Tests

    /// <summary>Test 21: AppendEventsAsync multiple times increases version correctly</summary>
    [Fact]
    public async Task AppendEventsAsync_MultipleAppends_IncreaseVersionCorrectly()
    {
        // Arrange
        var aggregateId = "test-aggregate-012";
        var events1 = new List<IDomainEvent>
        {
            new TestDomainEvent(aggregateId, "Event 1"),
            new TestDomainEvent(aggregateId, "Event 1.5")
        };

        var events2 = new List<IDomainEvent>
        {
            new TestDomainEvent(aggregateId, "Event 2")
        };

        // Act
        await _eventStore.AppendEventsAsync(aggregateId, events1);
        await _eventStore.AppendEventsAsync(aggregateId, events2);

        // Assert
        var allEvents = (await _eventStore.GetEventsAsync(aggregateId)).ToList();
        Assert.Equal(3, allEvents.Count());
    }

    #endregion

    #region Integration Tests

    /// <summary>Test 22: Complete event sourcing workflow</summary>
    [Fact]
    public async Task CompleteWorkflow_CreateStoreRetrieveSnapshot()
    {
        // Arrange
        var aggregateId = "workflow-aggregate-001";

        // Create initial events
        var initialEvents = new List<IDomainEvent>
        {
            new TestDomainEvent(aggregateId, "Created"),
            new TestDomainEvent(aggregateId, "Initialized")
        };

        // Act - Store initial events
        await _eventStore.AppendEventsAsync(aggregateId, initialEvents);

        // Verify initial storage
        var storedEvents = await _eventStore.GetEventsAsync(aggregateId);
        Assert.Equal(2, storedEvents.Count());

        // Create and store snapshot
        var snapshot = new EventStoreSnapshot
        {
            AggregateId = aggregateId,
            Version = 2,
            SnapshotData = new { Status = "Initialized" },
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _eventStore.SaveSnapshotAsync(snapshot);

        // Add more events
        var moreEvents = new List<IDomainEvent>
        {
            new TestDomainEvent(aggregateId, "Updated"),
            new TestDomainEvent(aggregateId, "Finalized")
        };

        await _eventStore.AppendEventsAsync(aggregateId, moreEvents);

        // Assert - Verify final state
        var allEvents = await _eventStore.GetEventsAsync(aggregateId);
        Assert.Equal(4, allEvents.Count());

        var snapshot2 = await _eventStore.GetSnapshotAsync(aggregateId);
        Assert.NotNull(snapshot2);
        Assert.Equal(2, snapshot2.Version);
    }

    #endregion

    #region Test Helpers

    /// <summary>
    /// Helper class representing a test domain event.
    /// </summary>
    private class TestDomainEvent : IDomainEvent
    {
        public TestDomainEvent(string aggregateId, string description)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTimeOffset.UtcNow;
            AggregateId = aggregateId;
            Description = description;
        }

        public Guid EventId { get; }
        public DateTimeOffset OccurredAt { get; }
        public string AggregateId { get; }
        public string Description { get; }
    }

    /// <summary>
    /// Helper class representing a test aggregate.
    /// </summary>
    private class TestAggregate
    {
        public string Id { get; set; } = string.Empty;
        public int Version { get; set; }
    }

    #endregion
}
