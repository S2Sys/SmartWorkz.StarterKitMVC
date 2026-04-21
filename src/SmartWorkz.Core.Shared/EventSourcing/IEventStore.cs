
namespace SmartWorkz.Shared;

/// <summary>
/// Abstraction for an immutable event store that persists domain events.
/// Enables event sourcing patterns for temporal queries, audit trails, and event replay.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Appends events to the event stream for an aggregate.
    /// Events are immutable and persist as an append-only log.
    /// </summary>
    /// <param name="aggregateId">The unique identifier of the aggregate root</param>
    /// <param name="events">The domain events to append</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown if aggregateId or events is null</exception>
    /// <exception cref="InvalidOperationException">Thrown on version conflict or optimistic concurrency violation</exception>
    Task AppendEventsAsync(
        string aggregateId,
        IEnumerable<IDomainEvent> events,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all events for an aggregate in chronological order.
    /// </summary>
    /// <param name="aggregateId">The unique identifier of the aggregate root</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of domain events for the aggregate, empty if none exist</returns>
    /// <exception cref="ArgumentNullException">Thrown if aggregateId is null</exception>
    Task<IEnumerable<IDomainEvent>> GetEventsAsync(
        string aggregateId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves events for an aggregate after a specific version.
    /// Useful for incremental event replay and event streaming.
    /// </summary>
    /// <param name="aggregateId">The unique identifier of the aggregate root</param>
    /// <param name="version">The version after which to retrieve events</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of events after the specified version, empty if none exist</returns>
    /// <exception cref="ArgumentNullException">Thrown if aggregateId is null</exception>
    /// <exception cref="ArgumentException">Thrown if version is negative</exception>
    Task<IEnumerable<IDomainEvent>> GetEventsSinceAsync(
        string aggregateId,
        int version,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest snapshot for an aggregate if one exists.
    /// Snapshots optimize aggregate reconstruction by storing intermediate state.
    /// </summary>
    /// <param name="aggregateId">The unique identifier of the aggregate root</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Snapshot data if exists; null if no snapshot is available</returns>
    /// <exception cref="ArgumentNullException">Thrown if aggregateId is null</exception>
    Task<EventStoreSnapshot?> GetSnapshotAsync(
        string aggregateId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a snapshot of aggregate state at a specific version.
    /// Snapshots reduce the number of events needed to replay an aggregate.
    /// </summary>
    /// <param name="snapshot">The snapshot to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown if snapshot is null</exception>
    Task SaveSnapshotAsync(
        EventStoreSnapshot snapshot,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reconstructs an aggregate from its event history.
    /// Optionally uses snapshots for performance optimization.
    /// </summary>
    /// <typeparam name="T">The aggregate type to reconstruct</typeparam>
    /// <param name="aggregateId">The unique identifier of the aggregate root</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The reconstructed aggregate instance, or null if no events exist</returns>
    /// <exception cref="ArgumentNullException">Thrown if aggregateId is null</exception>
    Task<T?> GetAggregateAsync<T>(
        string aggregateId,
        CancellationToken cancellationToken = default) where T : class;
}
