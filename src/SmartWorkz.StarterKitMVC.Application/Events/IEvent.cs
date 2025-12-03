namespace SmartWorkz.StarterKitMVC.Application.Events;

/// <summary>
/// Base interface for all domain events.
/// </summary>
public interface IEvent
{
    /// <summary>Unique event identifier.</summary>
    Guid EventId { get; }
    
    /// <summary>When the event occurred (UTC).</summary>
    DateTime OccurredAt { get; }
}

/// <summary>
/// Base record for creating domain events.
/// </summary>
/// <example>
/// <code>
/// // Define a custom event
/// public record OrderCreatedEvent(Guid OrderId, decimal Total) : BaseEvent;
/// 
/// // Create and use
/// var evt = new OrderCreatedEvent(Guid.NewGuid(), 99.99m);
/// Console.WriteLine($"Event {evt.EventId} at {evt.OccurredAt}");
/// </code>
/// </example>
public abstract record BaseEvent : IEvent
{
    /// <inheritdoc />
    public Guid EventId { get; init; } = Guid.NewGuid();
    
    /// <inheritdoc />
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
