namespace SmartWorkz.StarterKitMVC.Application.Events;

public interface IEventRouter
{
    Task RouteAsync<T>(T @event, CancellationToken ct = default) where T : IEvent;
}
