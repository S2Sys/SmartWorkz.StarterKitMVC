namespace SmartWorkz.Shared;

/// <summary>
/// Represents the result of executing a single saga step.
/// Provides success/failure status and optional compensation logic for rollback.
/// </summary>
public class StepResult
{
    /// <summary>
    /// Gets a value indicating whether the step executed successfully.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Gets the timestamp when this step completed.
    /// </summary>
    public DateTimeOffset CompletedAt { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the reason for step failure, if any.
    /// </summary>
    public string? FailureReason { get; private set; }

    /// <summary>
    /// Gets the optional compensation handler that will be called if a later step fails
    /// to allow rollback of this step's changes.
    /// </summary>
    public Func<SagaState, Exception, Task>? CompensationHandler { get; private set; }

    /// <summary>
    /// Creates a successful step result.
    /// </summary>
    /// <returns>A StepResult indicating success.</returns>
    public static StepResult Success()
    {
        return new StepResult { IsSuccess = true };
    }

    /// <summary>
    /// Creates a failed step result with an optional compensation handler.
    /// </summary>
    /// <param name="failureReason">The reason for the step failure.</param>
    /// <param name="compensationHandler">
    /// Optional handler to compensate/rollback this step if a later step fails.
    /// </param>
    /// <returns>A StepResult indicating failure.</returns>
    public static StepResult Failure(string failureReason, Func<SagaState, Exception, Task>? compensationHandler = null)
    {
        if (string.IsNullOrWhiteSpace(failureReason))
        {
            throw new ArgumentException("Failure reason cannot be null or empty.", nameof(failureReason));
        }

        return new StepResult
        {
            IsSuccess = false,
            FailureReason = failureReason,
            CompensationHandler = compensationHandler
        };
    }

    /// <summary>
    /// Creates a failed step result for an exception with optional compensation.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <param name="compensationHandler">Optional compensation handler.</param>
    /// <returns>A StepResult indicating failure.</returns>
    public static StepResult FromException(Exception exception, Func<SagaState, Exception, Task>? compensationHandler = null)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        return new StepResult
        {
            IsSuccess = false,
            FailureReason = exception.Message,
            CompensationHandler = compensationHandler
        };
    }
}
