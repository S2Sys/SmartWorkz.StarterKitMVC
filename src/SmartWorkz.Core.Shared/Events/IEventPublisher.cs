namespace SmartWorkz.Core.Shared.Events;

/// <summary>
/// Publishes domain events for event-driven architecture.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event to all registered subscribers.
    /// Event delivery is synchronous by default (handlers execute in order).
    /// </summary>
    /// <typeparam name="TEvent">Event type (must have parameterless constructor).</typeparam>
    /// <param name="event">Event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, new();
}
