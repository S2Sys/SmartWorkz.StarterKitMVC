namespace SmartWorkz.Core.Shared.Resilience;

/// <summary>
/// Defines the state of a circuit breaker in the state machine pattern.
/// </summary>
public enum CircuitBreakerState
{
    /// <summary>
    /// Normal operation state. The circuit breaker allows operations to pass through.
    /// Transitions to Open when consecutive failures reach the threshold.
    /// </summary>
    Closed = 0,

    /// <summary>
    /// Failing state. The circuit breaker rejects all operations immediately
    /// to prevent cascading failures. Transitions to HalfOpen after the timeout expires.
    /// </summary>
    Open = 1,

    /// <summary>
    /// Testing recovery state. The circuit breaker allows a limited number of operations
    /// to test if the dependency has recovered. Transitions to Closed if successful,
    /// or back to Open if failures occur.
    /// </summary>
    HalfOpen = 2
}
