namespace SmartWorkz.Shared;

/// <summary>
/// Represents the status of a saga execution.
/// </summary>
public enum SagaStatus
{
    /// <summary>The saga is currently executing.</summary>
    Running = 0,

    /// <summary>The saga completed successfully.</summary>
    Completed = 1,

    /// <summary>The saga failed during execution.</summary>
    Failed = 2,

    /// <summary>The saga is in the process of compensating (rolling back) changes.</summary>
    Compensating = 3
}

/// <summary>
/// Base class for saga state objects.
/// Provides common tracking properties for saga execution flow.
/// </summary>
public abstract class SagaState
{
    /// <summary>
    /// Gets or sets the unique identifier for this saga instance.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the current execution status of the saga.
    /// </summary>
    public SagaStatus Status { get; set; } = SagaStatus.Running;

    /// <summary>
    /// Gets or sets the timestamp when the saga started.
    /// </summary>
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the saga completed or failed.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the error message if the saga failed.
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Gets or sets the stack trace if the saga failed with an exception.
    /// </summary>
    public string? FailureStackTrace { get; set; }
}
