namespace SmartWorkz.Shared;

/// <summary>
/// Represents a snapshot of an aggregate's state at a specific version.
/// Snapshots optimize event sourcing by reducing the number of events needed for reconstruction.
/// </summary>
public class EventStoreSnapshot
{
    /// <summary>
    /// Gets the unique identifier of the aggregate root.
    /// </summary>
    public required string AggregateId { get; init; }

    /// <summary>
    /// Gets the version number of the aggregate when the snapshot was taken.
    /// This represents the event version used for optimistic concurrency.
    /// </summary>
    public required int Version { get; init; }

    /// <summary>
    /// Gets the snapshot data containing the serialized aggregate state.
    /// The actual type depends on the aggregate being stored.
    /// </summary>
    public required object SnapshotData { get; init; }

    /// <summary>
    /// Gets the date and time when the snapshot was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}
