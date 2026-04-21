using SmartWorkz.Core.Shared.EventSourcing;
using SmartWorkz.Core.Shared.Events;

namespace SmartWorkz.Core.Tests.EventSourcing;

/// <summary>
/// In-memory implementation of IEventStore for testing purposes.
/// Provides a simple event store without SQL Server dependency.
/// </summary>
internal class InMemoryEventStore : IEventStore
{
    private readonly Dictionary<string, List<StoredEvent>> _events = new();
    private readonly Dictionary<string, EventStoreSnapshot> _snapshots = new();

    /// <summary>
    /// Appends events to the event stream for an aggregate.
    /// </summary>
    public Task AppendEventsAsync(
        string aggregateId,
        IEnumerable<SmartWorkz.Core.Shared.Events.IDomainEvent> events,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
        {
            throw new ArgumentNullException(nameof(aggregateId));
        }

        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        var eventsList = events.ToList();
        if (!eventsList.Any())
        {
            return Task.CompletedTask;
        }

        if (!_events.ContainsKey(aggregateId))
        {
            _events[aggregateId] = new List<StoredEvent>();
        }

        var currentVersion = _events[aggregateId].Count;

        foreach (var @event in eventsList)
        {
            currentVersion++;
            _events[aggregateId].Add(new StoredEvent
            {
                EventId = @event.EventId,
                AggregateId = aggregateId,
                EventData = @event,
                Version = currentVersion,
                OccurredAt = @event.OccurredAt,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves all events for an aggregate in chronological order.
    /// </summary>
    public Task<IEnumerable<SmartWorkz.Core.Shared.Events.IDomainEvent>> GetEventsAsync(
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
        {
            throw new ArgumentNullException(nameof(aggregateId));
        }

        if (!_events.ContainsKey(aggregateId))
        {
            return Task.FromResult(Enumerable.Empty<SmartWorkz.Core.Shared.Events.IDomainEvent>());
        }

        var events = _events[aggregateId]
            .OrderBy(e => e.Version)
            .Select(e => e.EventData)
            .ToList();

        return Task.FromResult(events.AsEnumerable());
    }

    /// <summary>
    /// Retrieves events for an aggregate after a specific version.
    /// </summary>
    public Task<IEnumerable<SmartWorkz.Core.Shared.Events.IDomainEvent>> GetEventsSinceAsync(
        string aggregateId,
        int version,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
        {
            throw new ArgumentNullException(nameof(aggregateId));
        }

        if (version < 0)
        {
            throw new ArgumentException("Version cannot be negative.", nameof(version));
        }

        if (!_events.ContainsKey(aggregateId))
        {
            return Task.FromResult(Enumerable.Empty<SmartWorkz.Core.Shared.Events.IDomainEvent>());
        }

        var events = _events[aggregateId]
            .Where(e => e.Version > version)
            .OrderBy(e => e.Version)
            .Select(e => e.EventData)
            .ToList();

        return Task.FromResult(events.AsEnumerable());
    }

    /// <summary>
    /// Retrieves the latest snapshot for an aggregate if one exists.
    /// </summary>
    public Task<EventStoreSnapshot?> GetSnapshotAsync(
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
        {
            throw new ArgumentNullException(nameof(aggregateId));
        }

        var snapshot = _snapshots.ContainsKey(aggregateId) ? _snapshots[aggregateId] : null;
        return Task.FromResult(snapshot);
    }

    /// <summary>
    /// Saves a snapshot of aggregate state at a specific version.
    /// </summary>
    public Task SaveSnapshotAsync(
        EventStoreSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        if (snapshot == null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        if (string.IsNullOrWhiteSpace(snapshot.AggregateId))
        {
            throw new ArgumentException("Snapshot AggregateId cannot be null or empty.", nameof(snapshot));
        }

        _snapshots[snapshot.AggregateId] = snapshot;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Reconstructs an aggregate from its event history.
    /// </summary>
    public async Task<T?> GetAggregateAsync<T>(
        string aggregateId,
        CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
        {
            throw new ArgumentNullException(nameof(aggregateId));
        }

        // Try to get snapshot
        var snapshot = await GetSnapshotAsync(aggregateId, cancellationToken);
        T? aggregate = null;
        var startVersion = 0;

        if (snapshot != null && snapshot.SnapshotData is T snapshotAggregate)
        {
            aggregate = snapshotAggregate;
            startVersion = snapshot.Version;
        }

        // Get events since snapshot
        var events = await GetEventsSinceAsync(aggregateId, startVersion, cancellationToken);

        // If no snapshot and no events, return null
        if (aggregate == null && !events.Any())
        {
            return null;
        }

        // Create new aggregate if needed
        aggregate ??= Activator.CreateInstance<T>();

        // Update version and ID
        if (aggregate != null)
        {
            var versionProperty = typeof(T).GetProperty("Version");
            if (versionProperty?.CanWrite == true)
            {
                versionProperty.SetValue(aggregate, events.Count() + startVersion);
            }

            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty?.CanWrite == true)
            {
                idProperty.SetValue(aggregate, aggregateId);
            }
        }

        return aggregate;
    }

    /// <summary>
    /// Internal model for storing events in memory.
    /// </summary>
    private class StoredEvent
    {
        public Guid EventId { get; set; }
        public string AggregateId { get; set; } = string.Empty;
        public SmartWorkz.Core.Shared.Events.IDomainEvent EventData { get; set; } = default!;
        public int Version { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
