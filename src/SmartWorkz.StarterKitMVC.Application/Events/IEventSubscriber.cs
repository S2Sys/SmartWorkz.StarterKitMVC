namespace SmartWorkz.StarterKitMVC.Application.Events;

public interface IEventSubscriber
{
    Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken ct = default) where T : IEvent;
}
