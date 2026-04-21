namespace SmartWorkz.Shared;

/// <summary>
/// Configuration options for the circuit breaker.
/// </summary>
public sealed class CircuitBreakerOptions
{
    /// <summary>
    /// Gets or sets the number of consecutive failures required to trigger the Open state.
    /// Default: 5 failures.
    /// </summary>
    public int FailureThreshold { get; set; } = 5;

    /// <summary>
    /// Gets or sets the timeout in milliseconds before the circuit breaker automatically
    /// transitions from Open to HalfOpen state to test recovery.
    /// Default: 30000 milliseconds (30 seconds).
    /// </summary>
    public int TimeoutMilliseconds { get; set; } = 30000;

    /// <summary>
    /// Gets or sets the number of consecutive successful operations required
    /// in HalfOpen state to transition back to Closed state.
    /// Default: 3 successful operations.
    /// </summary>
    public int SuccessThreshold { get; set; } = 3;

    /// <summary>
    /// Gets or sets an optional identifier for this circuit breaker instance.
    /// Useful for logging and diagnostics.
    /// </summary>
    public string? Identifier { get; set; }

    /// <summary>
    /// Validates the options for correctness.
    /// </summary>
    /// <returns>True if options are valid; otherwise false.</returns>
    public bool IsValid()
    {
        return FailureThreshold > 0 &&
               TimeoutMilliseconds > 0 &&
               SuccessThreshold > 0;
    }
}
