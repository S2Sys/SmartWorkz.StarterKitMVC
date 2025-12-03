using SmartWorkz.StarterKitMVC.Application.Abstractions;

namespace SmartWorkz.StarterKitMVC.Infrastructure.BackgroundJobs;

public sealed class InMemoryBackgroundJobScheduler : IBackgroundJobScheduler
{
    public string Enqueue(Func<CancellationToken, Task> job, string? description = null)
    {
        _ = Task.Run(() => job(CancellationToken.None));
        return Guid.NewGuid().ToString();
    }
}
