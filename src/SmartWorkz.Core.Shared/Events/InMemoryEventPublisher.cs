using SmartWorkz.Core.Shared.Guards;

namespace SmartWorkz.Shared;

/// <summary>
/// In-memory event publisher that executes all registered handlers sequentially.
/// Provides synchronous event delivery with exception handling and result reporting.
/// </summary>
public sealed class InMemoryEventPublisher : IEventPublisher
{
    private readonly InMemoryEventSubscriber _subscriber;

    /// <summary>
    /// Initializes a new instance of the InMemoryEventPublisher with a subscriber.
    /// </summary>
    /// <param name="subscriber">The event subscriber containing registered handlers.</param>
    /// <exception cref="ArgumentNullException">Thrown when subscriber is null.</exception>
    public InMemoryEventPublisher(InMemoryEventSubscriber subscriber)
    {
        _subscriber = Guard.NotNull(subscriber, nameof(subscriber));
    }

    /// <summary>
    /// Publishes a single domain event to all registered handlers.
    /// Handlers are invoked sequentially in registration order.
    /// If any handler throws an exception, it is caught and a failure Result is returned.
    /// Other handlers will attempt to execute even if a previous handler fails.
    /// </summary>
    /// <typeparam name="TEvent">The event type implementing IDomainEvent.</typeparam>
    /// <param name="event">The event instance to publish.</param>
    /// <param name="cancellationToken">Cancellation token to cancel handler execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IDomainEvent
    {
        var eventType = typeof(TEvent);
        var handlers = _subscriber.GetHandlers(eventType);

        // If no handlers registered, consider it a success
        if (handlers.Count == 0)
        {
            return;
        }

        var errors = new List<string>();

        // Execute each handler sequentially
        foreach (var handler in handlers)
        {
            try
            {
                // Cast the delegate to the correct handler type and invoke
                if (handler is Func<TEvent, CancellationToken, Task> typedHandler)
                {
                    await typedHandler(@event, cancellationToken);
                }
                else
                {
                    // Handler type mismatch (should not happen in normal usage)
                    errors.Add($"Handler type mismatch for event type {eventType.Name}");
                }
            }
            catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
            {
                // Propagate cancellation
                errors.Add($"Handler execution cancelled: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Capture exception details but continue with other handlers
                errors.Add($"Handler error: {ex.GetType().Name} - {ex.Message}");
            }
        }

        // If there were errors, throw an exception (since we can't return Result via Task)
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"One or more event handlers failed: {string.Join("; ", errors)}");
        }
    }

    /// <summary>
    /// Publishes multiple domain events to all registered handlers.
    /// Events are published sequentially in the order provided.
    /// </summary>
    /// <typeparam name="TEvent">The event type implementing IDomainEvent.</typeparam>
    /// <param name="events">Collection of events to publish.</param>
    /// <param name="cancellationToken">Cancellation token to cancel handler execution.</param>
    /// <returns>A task representing the asynchronous batch operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when any event handler fails.</exception>
    public async Task PublishAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : class, IDomainEvent
    {
        var eventList = events?.ToList() ?? new List<TEvent>();

        if (eventList.Count == 0)
        {
            return; // No events to publish
        }

        var errors = new List<string>();

        foreach (var @event in eventList)
        {
            try
            {
                await PublishAsync(@event, cancellationToken);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to publish event: {ex.Message}");
            }
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Failed to publish {errors.Count} out of {eventList.Count} events: {string.Join("; ", errors)}");
        }
    }
}
