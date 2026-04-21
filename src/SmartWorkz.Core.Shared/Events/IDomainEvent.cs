namespace SmartWorkz.Shared;

/// <summary>
/// Base interface for domain events in event-driven architecture.
/// Provides core event metadata for tracking and publishing.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// When the event occurred.
    /// </summary>
    DateTimeOffset OccurredAt { get; }

    /// <summary>
    /// The aggregate (entity) that generated this event.
    /// </summary>
    string AggregateId { get; }
}
