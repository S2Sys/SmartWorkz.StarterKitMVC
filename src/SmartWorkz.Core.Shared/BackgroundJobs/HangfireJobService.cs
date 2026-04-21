namespace SmartWorkz.Core.Shared.BackgroundJobs;

using SmartWorkz.Core.Services.BackgroundJobs;

using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
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
            // Create a Job instance from a static method and enqueue it
            var job = Job.FromExpression(() => HangfireJobHelper.ExecuteJobAsync<TJob>());
            var jobId = _jobClient.Create(job, new EnqueuedState());
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
            // Create a Job instance and schedule it with Hangfire
            var job = Job.FromExpression(() => HangfireJobHelper.ExecuteJobAsync<TJob>());
            var scheduledState = new ScheduledState(enqueueAt.UtcDateTime);
            var jobId = _jobClient.Create(job, scheduledState);
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
            // Create a Job instance and add or update the recurring job with Hangfire
            var job = Job.FromExpression(() => HangfireJobHelper.ExecuteJobAsync<TJob>());
            _recurringJobManager.AddOrUpdate(recurringJobId, job, cronExpression);
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
            // Delete the job from Hangfire
            BackgroundJob.Delete(jobId);
            _logger.LogInformation("Job {JobId} deleted successfully", jobId);
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
            // Requeue the job with Hangfire
            var requeued = BackgroundJob.Requeue(jobId);
            _logger.LogInformation("Job {JobId} requeue result: {Requeued}", jobId, requeued);
            return Task.FromResult(jobId); // Return the original job ID since requeue doesn't return a new ID
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
            // Query the actual job state from Hangfire storage
            var connection = JobStorage.Current.GetConnection();
            var jobData = connection.GetJobData(jobId);

            if (jobData == null)
            {
                _logger.LogWarning("Job {JobId} not found in storage", jobId);
                return Task.FromResult((BackgroundJobStatus?)null);
            }

            var status = jobData.State switch
            {
                "Enqueued" => BackgroundJobStatus.Enqueued,
                "Processing" => BackgroundJobStatus.Processing,
                "Succeeded" => BackgroundJobStatus.Succeeded,
                "Failed" => BackgroundJobStatus.Failed,
                "Deleted" => BackgroundJobStatus.Deleted,
                "Scheduled" => BackgroundJobStatus.Scheduled,
                _ => (BackgroundJobStatus?)null
            };

            _logger.LogInformation("Job {JobId} status: {Status}", jobId, status);
            return Task.FromResult(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get status for job {JobId}", jobId);
            return Task.FromResult((BackgroundJobStatus?)null);
        }
    }
}

// Helper class for Hangfire job execution
public static class HangfireJobHelper
{
    public static async Task ExecuteJobAsync<TJob>() where TJob : class
    {
        // This method is called by Hangfire
        // In a real implementation, you'd resolve TJob from DI and execute it
        await Task.CompletedTask;
    }
}
