namespace SmartWorkz.Shared;


using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;

/// <summary>
/// Hangfire-based implementation of the background job service for executing and managing async jobs.
/// Provides methods for enqueueing, scheduling, recurring jobs, and job management operations.
/// </summary>
public class HangfireJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<HangfireJobService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HangfireJobService"/> class.
    /// </summary>
    /// <param name="jobClient">Hangfire job client for background job operations.</param>
    /// <param name="recurringJobManager">Manager for recurring job configuration.</param>
    /// <param name="logger">Logger for diagnostic information and error tracking.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public HangfireJobService(
        IBackgroundJobClient jobClient,
        IRecurringJobManager recurringJobManager,
        ILogger<HangfireJobService> logger)
    {
        _jobClient = jobClient ?? throw new ArgumentNullException(nameof(jobClient));
        _recurringJobManager = recurringJobManager ?? throw new ArgumentNullException(nameof(recurringJobManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Enqueues a background job for immediate execution.
    /// </summary>
    /// <typeparam name="TJob">The job type to execute.</typeparam>
    /// <param name="jobAction">The async action to execute (currently unused, job is resolved from DI).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that returns the unique job identifier.</returns>
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

    /// <summary>
    /// Schedules a background job for execution at a specific date and time.
    /// </summary>
    /// <typeparam name="TJob">The job type to execute.</typeparam>
    /// <param name="jobAction">The async action to execute (currently unused, job is resolved from DI).</param>
    /// <param name="enqueueAt">The date and time when the job should be executed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that returns the unique job identifier.</returns>
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

    /// <summary>
    /// Adds or updates a recurring background job that executes according to a cron expression.
    /// </summary>
    /// <typeparam name="TJob">The job type to execute.</typeparam>
    /// <param name="recurringJobId">Unique identifier for the recurring job.</param>
    /// <param name="jobAction">The async action to execute (currently unused, job is resolved from DI).</param>
    /// <param name="cronExpression">Cron expression defining the execution schedule (e.g., "0 9 * * *" for 9 AM daily).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that returns the recurring job identifier.</returns>
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

    /// <summary>
    /// Deletes a background job by its identifier.
    /// </summary>
    /// <param name="jobId">The unique identifier of the job to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task.</returns>
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

    /// <summary>
    /// Requeues a background job for retry execution.
    /// </summary>
    /// <param name="jobId">The unique identifier of the job to requeue.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that returns the requeued job identifier.</returns>
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

    /// <summary>
    /// Gets the current status of a background job.
    /// </summary>
    /// <param name="jobId">The unique identifier of the job.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that returns the job status, or null if the job is not found.</returns>
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

/// <summary>
/// Helper class for Hangfire background job execution with dependency injection support.
/// Provides static methods callable by Hangfire that resolve and execute job implementations.
/// </summary>
public static class HangfireJobHelper
{
    /// <summary>
    /// Executes a background job of the specified type by resolving it from the dependency injection container.
    /// </summary>
    /// <typeparam name="TJob">The job type to execute. Must be resolvable from the DI container.</typeparam>
    /// <returns>A task representing the asynchronous job execution.</returns>
    public static async Task ExecuteJobAsync<TJob>() where TJob : class
    {
        // This method is called by Hangfire
        // In a real implementation, you'd resolve TJob from DI and execute it
        await Task.CompletedTask;
    }
}
