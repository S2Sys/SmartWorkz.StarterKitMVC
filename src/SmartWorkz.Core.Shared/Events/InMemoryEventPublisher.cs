using SmartWorkz.Core.Shared.Guards;

namespace SmartWorkz.Core.Shared.Events;

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
    /// Publishes an event to all registered handlers.
    /// Handlers are invoked sequentially in registration order.
    /// If any handler throws an exception, it is caught and a failure Result is returned.
    /// Other handlers will attempt to execute even if a previous handler fails.
    /// </summary>
    /// <typeparam name="TEvent">The event type being published.</typeparam>
    /// <param name="event">The event instance to publish.</param>
    /// <param name="cancellationToken">Cancellation token to cancel handler execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, new()
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
}
