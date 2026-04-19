using System.Collections.Concurrent;

namespace SmartWorkz.Core.Shared.Events;

/// <summary>
/// In-memory event subscriber that maintains a registry of event handlers.
/// Supports multiple handlers per event type using thread-safe concurrent collections.
/// </summary>
public sealed class InMemoryEventSubscriber : IEventSubscriber
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();

    /// <summary>
    /// Subscribes a handler to an event type.
    /// Multiple handlers can be registered for the same event type and will execute sequentially.
    /// </summary>
    /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
    /// <param name="handler">Async handler function that receives event and cancellation token.</param>
    public void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler)
        where TEvent : class, new()
    {
        var eventType = typeof(TEvent);

        // Use GetOrAdd to ensure thread-safe handler list creation
        _handlers.AddOrUpdate(eventType,
            _ => new List<Delegate> { handler },
            (_, existingHandlers) =>
            {
                lock (existingHandlers)
                {
                    existingHandlers.Add(handler);
                }
                return existingHandlers;
            });
    }

    /// <summary>
    /// Gets all registered handlers for a given event type.
    /// Returns an empty list if no handlers are registered for the type.
    /// </summary>
    /// <param name="eventType">The event type to retrieve handlers for.</param>
    /// <returns>List of registered handlers (delegates).</returns>
    public List<Delegate> GetHandlers(Type eventType)
    {
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            lock (handlers)
            {
                return new List<Delegate>(handlers);
            }
        }

        return new List<Delegate>();
    }
}
