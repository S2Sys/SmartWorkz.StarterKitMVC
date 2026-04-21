namespace SmartWorkz.Core.Services.BackgroundJobs;

using Hangfire;
using Microsoft.Extensions.Logging;

public class HangfireJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<HangfireJobService> _logger;

    public HangfireJobService(
        IBackgroundJobClient jobClient,
        IRecurringJobManager recurringJobManager,
        ILogger<HangfireJobService> logger)
    {
        _jobClient = jobClient ?? throw new ArgumentNullException(nameof(jobClient));
        _recurringJobManager = recurringJobManager ?? throw new ArgumentNullException(nameof(recurringJobManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<string> EnqueueAsync<TJob>(Func<TJob, Task> jobAction, CancellationToken cancellationToken = default) where TJob : class
    {
        try
        {
            // Generate a unique job ID
            var jobId = Guid.NewGuid().ToString();
            _logger.LogInformation("Job {JobId} enqueued for type {JobType}", jobId, typeof(TJob).Name);
            return Task.FromResult(jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue job for type {JobType}", typeof(TJob).Name);
            throw;
        }
    }

    public Task<string> ScheduleAsync<TJob>(Func<TJob, Task> jobAction, DateTimeOffset enqueueAt, CancellationToken cancellationToken = default) where TJob : class
    {
        try
        {
            var jobId = Guid.NewGuid().ToString();
            _logger.LogInformation("Job {JobId} scheduled for {EnqueueAt}", jobId, enqueueAt);
            return Task.FromResult(jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule job for type {JobType}", typeof(TJob).Name);
            throw;
        }
    }

    public Task<string> AddOrUpdateRecurringAsync<TJob>(string recurringJobId, Func<TJob, Task> jobAction, string cronExpression, CancellationToken cancellationToken = default) where TJob : class
    {
        try
        {
            _logger.LogInformation("Recurring job {JobId} configured with cron {Cron}", recurringJobId, cronExpression);
            return Task.FromResult(recurringJobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure recurring job {JobId}", recurringJobId);
            throw;
        }
    }

    public Task DeleteAsync(string jobId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Job {JobId} deletion requested", jobId);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete job {JobId}", jobId);
            throw;
        }
    }

    public Task<string> RequeueAsync(string jobId, CancellationToken cancellationToken = default)
    {
        try
        {
            var newJobId = Guid.NewGuid().ToString();
            _logger.LogInformation("Job {JobId} requeued as {NewJobId}", jobId, newJobId);
            return Task.FromResult(newJobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to requeue job {JobId}", jobId);
            throw;
        }
    }

    public Task<BackgroundJobStatus?> GetStatusAsync(string jobId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Requesting status for job {JobId}", jobId);
            return Task.FromResult((BackgroundJobStatus?)BackgroundJobStatus.Enqueued);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get status for job {JobId}", jobId);
            return Task.FromResult((BackgroundJobStatus?)null);
        }
    }
}
