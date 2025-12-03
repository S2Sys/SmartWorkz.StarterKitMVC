namespace SmartWorkz.StarterKitMVC.Application.Abstractions;

public interface IBackgroundJobScheduler
{
    string Enqueue(Func<CancellationToken, Task> job, string? description = null);
}
