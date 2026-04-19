namespace SmartWorkz.Core.Shared.Events;

/// <summary>
/// Registers event handlers for domain events.
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    /// Subscribes a handler to an event type.
    /// Multiple handlers can subscribe to the same event.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <param name="handler">Async handler function. Receives event and cancellation token.</param>
    void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler)
        where TEvent : class, new();
}
