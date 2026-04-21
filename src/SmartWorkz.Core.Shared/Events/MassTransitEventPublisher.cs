using MassTransit;
using Microsoft.Extensions.Logging;

namespace SmartWorkz.Core.Shared.Events;

/// <summary>
/// Distributed event publisher using MassTransit message bus.
/// Supports both single and batch event publishing with async/await patterns.
/// Suitable for production environments with message broker backend (RabbitMQ, Azure Service Bus, etc).
/// </summary>
public sealed class MassTransitEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MassTransitEventPublisher> _logger;

    /// <summary>
    /// Initializes a new instance of MassTransitEventPublisher.
    /// </summary>
    /// <param name="publishEndpoint">MassTransit publish endpoint for message distribution.</param>
    /// <param name="logger">Logger for event publication tracking.</param>
    /// <exception cref="ArgumentNullException">Thrown when publishEndpoint or logger is null.</exception>
    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint, ILogger<MassTransitEventPublisher> logger)
    {
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Publishes a single domain event to the message bus asynchronously.
    /// </summary>
    /// <typeparam name="TEvent">Event type implementing IDomainEvent.</typeparam>
    /// <param name="event">Event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous publish operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when event is null.</exception>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IDomainEvent
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        try
        {
            await _publishEndpoint.Publish(@event, cancellationToken);
            _logger.LogInformation(
                "Event published: Type={EventType}, EventId={EventId}, AggregateId={AggregateId}",
                typeof(TEvent).Name,
                @event.EventId,
                @event.AggregateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish event: Type={EventType}, EventId={EventId}, AggregateId={AggregateId}",
                typeof(TEvent).Name,
                @event.EventId,
                @event.AggregateId);
            throw;
        }
    }

    /// <summary>
    /// Publishes multiple domain events to the message bus asynchronously.
    /// Events are published sequentially in the order provided.
    /// </summary>
    /// <typeparam name="TEvent">Event type implementing IDomainEvent.</typeparam>
    /// <param name="events">Collection of events to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous batch publish operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when events collection is null.</exception>
    /// <exception cref="AggregateException">Thrown when any event publication fails, contains all failures.</exception>
    public async Task PublishAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : class, IDomainEvent
    {
        if (events == null)
            throw new ArgumentNullException(nameof(events));

        var eventList = events.ToList();

        if (eventList.Count == 0)
        {
            return; // No events to publish
        }

        var errors = new List<Exception>();

        foreach (var @event in eventList)
        {
            try
            {
                await _publishEndpoint.Publish(@event, cancellationToken);
                _logger.LogInformation(
                    "Event published: Type={EventType}, EventId={EventId}, AggregateId={AggregateId}",
                    typeof(TEvent).Name,
                    @event.EventId,
                    @event.AggregateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to publish event: Type={EventType}, EventId={EventId}, AggregateId={AggregateId}",
                    typeof(TEvent).Name,
                    @event.EventId,
                    @event.AggregateId);
                errors.Add(ex);
            }
        }

        if (errors.Count > 0)
        {
            throw new AggregateException(
                $"Failed to publish {errors.Count} out of {eventList.Count} events",
                errors);
        }
    }
}
