namespace SmartWorkz.StarterKitMVC.Application.Events;

/// <summary>
/// Publishes events to the event bus for asynchronous processing.
/// </summary>
/// <example>
/// <code>
/// // Define a custom event
/// public record UserCreatedEvent(Guid UserId, string Email) : BaseEvent;
/// 
/// // Inject IEventPublisher via DI
/// public class UserService
/// {
///     private readonly IEventPublisher _events;
///     
///     public UserService(IEventPublisher events) => _events = events;
///     
///     public async Task CreateUserAsync(User user)
///     {
///         // ... create user logic ...
///         
///         // Publish event for other services to react
///         await _events.PublishAsync(new UserCreatedEvent(user.Id, user.Email));
///     }
/// }
/// </code>
/// </example>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event to all subscribers.
    /// </summary>
    /// <typeparam name="T">The event type.</typeparam>
    /// <param name="event">The event to publish.</param>
    /// <param name="ct">Cancellation token.</param>
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IEvent;
}
