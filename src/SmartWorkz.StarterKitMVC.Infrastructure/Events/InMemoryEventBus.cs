using SmartWorkz.StarterKitMVC.Application.Events;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Events;

public sealed class InMemoryEventBus : IEventPublisher, IEventSubscriber
{
    private readonly List<Delegate> _handlers = new();

    public Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IEvent
    {
        foreach (var handler in _handlers.OfType<Func<T, CancellationToken, Task>>())
            _ = handler(@event, ct);
        return Task.CompletedTask;
    }

    public Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken ct = default) where T : IEvent
    {
        _handlers.Add(handler);
        return Task.CompletedTask;
    }
}
