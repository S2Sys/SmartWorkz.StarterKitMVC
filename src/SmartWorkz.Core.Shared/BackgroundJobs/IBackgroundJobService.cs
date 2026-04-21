namespace SmartWorkz.Shared;

public interface IBackgroundJobService
{
    /// <summary>Enqueue a fire-and-forget background job.</summary>
    Task<string> EnqueueAsync<TJob>(Func<TJob, Task> jobAction, CancellationToken cancellationToken = default)
        where TJob : class;

    /// <summary>Schedule a job to run at a specific time.</summary>
    Task<string> ScheduleAsync<TJob>(Func<TJob, Task> jobAction, DateTimeOffset enqueueAt, CancellationToken cancellationToken = default)
        where TJob : class;

    /// <summary>Schedule a recurring job (CRON expression).</summary>
    Task<string> AddOrUpdateRecurringAsync<TJob>(string recurringJobId, Func<TJob, Task> jobAction, string cronExpression, CancellationToken cancellationToken = default)
        where TJob : class;

    /// <summary>Delete a job by ID.</summary>
    Task DeleteAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>Requeue a failed job.</summary>
    Task<string> RequeueAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>Get job status.</summary>
    Task<BackgroundJobStatus?> GetStatusAsync(string jobId, CancellationToken cancellationToken = default);
}

public enum BackgroundJobStatus
{
    Enqueued,
    Processing,
    Succeeded,
    Failed,
    Deleted,
    Scheduled
}

public class BackgroundJobContext
{
    public string JobId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int? RetryCount { get; set; }
    public Exception? LastException { get; set; }
}
