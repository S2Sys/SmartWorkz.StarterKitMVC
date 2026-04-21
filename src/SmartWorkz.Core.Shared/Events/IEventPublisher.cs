namespace SmartWorkz.Shared;

/// <summary>
/// Publishes domain events for event-driven architecture.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a single domain event.
    /// </summary>
    /// <typeparam name="TEvent">Event type implementing IDomainEvent.</typeparam>
    /// <param name="event">Event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IDomainEvent;

    /// <summary>
    /// Publishes multiple domain events.
    /// </summary>
    /// <typeparam name="TEvent">Event type implementing IDomainEvent.</typeparam>
    /// <param name="events">Collection of events to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : class, IDomainEvent;
}
