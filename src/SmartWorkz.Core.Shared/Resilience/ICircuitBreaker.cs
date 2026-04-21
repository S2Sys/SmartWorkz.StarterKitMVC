namespace SmartWorkz.Shared;

/// <summary>
/// Defines the contract for a circuit breaker that implements the state machine pattern
/// to handle failing dependencies gracefully.
/// </summary>
public interface ICircuitBreaker
{
    /// <summary>
    /// Gets the current state of the circuit breaker.
    /// This property automatically transitions from Open to HalfOpen when the timeout expires.
    /// </summary>
    CircuitBreakerState State { get; }

    /// <summary>
    /// Gets the number of consecutive failures recorded in the current cycle.
    /// </summary>
    int ConsecutiveFailures { get; }

    /// <summary>
    /// Gets the number of consecutive successes recorded in HalfOpen state.
    /// </summary>
    int SuccessCount { get; }

    /// <summary>
    /// Executes an operation asynchronously with circuit breaker protection.
    /// </summary>
    /// <typeparam name="T">The type of data returned by the operation.</typeparam>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// A Result<T> containing the operation result on success, or a failure result if:
    /// - The circuit breaker is Open (rejected immediately)
    /// - The operation throws an exception
    /// </returns>
    Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a successful operation and updates state transitions accordingly.
    /// In Closed state: resets the failure counter.
    /// In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
    /// </summary>
    void RecordSuccess();

    /// <summary>
    /// Records a failed operation and updates state transitions accordingly.
    /// In Closed state: increments failure counter and transitions to Open if threshold is met.
    /// In HalfOpen state: immediately transitions back to Open.
    /// </summary>
    void RecordFailure();

    /// <summary>
    /// Resets the circuit breaker to the Closed state, clearing all failure and success counters.
    /// </summary>
    void Reset();
}
