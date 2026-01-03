using System.Collections.Concurrent;
using SmartWorkz.StarterKitMVC.Application.Notifications;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Notifications;

/// <summary>
/// In-memory implementation of notification queue for development/testing.
/// For production, consider using a persistent queue like Azure Service Bus, RabbitMQ, etc.
/// </summary>
public class InMemoryNotificationQueue : INotificationQueue
{
    private readonly ConcurrentQueue<NotificationMessage> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);

    public Task EnqueueAsync(NotificationMessage message, CancellationToken ct = default)
    {
        _queue.Enqueue(message);
        _signal.Release();
        return Task.CompletedTask;
    }

    public async Task<NotificationMessage?> DequeueAsync(CancellationToken ct = default)
    {
        await _signal.WaitAsync(ct);
        _queue.TryDequeue(out var message);
        return message;
    }
}
