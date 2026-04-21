namespace SmartWorkz.Shared;

/// <summary>
/// A thread-safe implementation of the circuit breaker pattern for handling failing dependencies gracefully.
///
/// The circuit breaker operates in three states:
/// - Closed: Normal operation. Requests pass through. Failures are tracked.
/// - Open: Failing. All requests are rejected immediately to prevent cascading failures.
/// - HalfOpen: Testing recovery. Limited requests are allowed to test if the dependency has recovered.
///
/// State transitions:
/// - Closed → Open: When ConsecutiveFailures >= FailureThreshold
/// - Open → HalfOpen: Automatically when (DateTime.UtcNow - LastFailureTime) >= TimeoutMilliseconds
/// - HalfOpen → Closed: When SuccessCount >= SuccessThreshold
/// - HalfOpen → Open: When RecordFailure() is called in HalfOpen state
/// - Closed → Closed: When RecordSuccess() is called (resets failure counter)
/// </summary>
public sealed class CircuitBreaker : ICircuitBreaker
{
    private readonly CircuitBreakerOptions _options;
    private readonly object _lockObject = new();
    private CircuitBreakerState _state = CircuitBreakerState.Closed;
    private int _consecutiveFailures;
    private int _successCount;
    private DateTime _lastFailureTime = DateTime.MinValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreaker"/> class.
    /// </summary>
    /// <param name="options">The circuit breaker configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    /// <exception cref="ArgumentException">Thrown when options are invalid.</exception>
    public CircuitBreaker(CircuitBreakerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!options.IsValid())
        {
            throw new ArgumentException("CircuitBreakerOptions are invalid. All thresholds and timeouts must be positive.", nameof(options));
        }

        _options = options;
    }

    /// <summary>
    /// Gets the current state of the circuit breaker.
    /// This property automatically transitions from Open to HalfOpen when the timeout expires.
    /// </summary>
    public CircuitBreakerState State
    {
        get
        {
            lock (_lockObject)
            {
                // Auto-transition from Open to HalfOpen if timeout has expired
                if (_state == CircuitBreakerState.Open)
                {
                    var timeSinceLastFailure = (DateTime.UtcNow - _lastFailureTime).TotalMilliseconds;
                    if (timeSinceLastFailure >= _options.TimeoutMilliseconds)
                    {
                        _state = CircuitBreakerState.HalfOpen;
                        _successCount = 0; // Reset success count when entering HalfOpen
                    }
                }

                return _state;
            }
        }
    }

    /// <summary>
    /// Gets the number of consecutive failures recorded in the current cycle.
    /// </summary>
    public int ConsecutiveFailures
    {
        get
        {
            lock (_lockObject)
            {
                return _consecutiveFailures;
            }
        }
    }

    /// <summary>
    /// Gets the number of consecutive successes recorded in HalfOpen state.
    /// </summary>
    public int SuccessCount
    {
        get
        {
            lock (_lockObject)
            {
                return _successCount;
            }
        }
    }

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
    public async Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        // Check if circuit is open (performs auto-transition if needed)
        var currentState = State;
        if (currentState == CircuitBreakerState.Open)
        {
            return Result.Fail<T>(
                new Error("CIRCUIT_BREAKER.OPEN",
                    $"Circuit breaker is open. Please retry after {_options.TimeoutMilliseconds}ms. The dependency may be temporarily unavailable."));
        }

        try
        {
            var result = await operation(cancellationToken);
            RecordSuccess();
            return Result.Ok(result);
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation exceptions
        }
        catch (Exception ex)
        {
            RecordFailure();
            return Result.Fail<T>(
                new Error("CIRCUIT_BREAKER.OPERATION_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Records a successful operation and updates state transitions accordingly.
    /// In Closed state: resets the failure counter.
    /// In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
    /// </summary>
    public void RecordSuccess()
    {
        lock (_lockObject)
        {
            if (_state == CircuitBreakerState.Closed)
            {
                _consecutiveFailures = 0;
            }
            else if (_state == CircuitBreakerState.HalfOpen)
            {
                _successCount++;
                if (_successCount >= _options.SuccessThreshold)
                {
                    _state = CircuitBreakerState.Closed;
                    _consecutiveFailures = 0;
                    _successCount = 0;
                }
            }
        }
    }

    /// <summary>
    /// Records a failed operation and updates state transitions accordingly.
    /// In Closed state: increments failure counter and transitions to Open if threshold is met.
    /// In HalfOpen state: immediately transitions back to Open.
    /// </summary>
    public void RecordFailure()
    {
        lock (_lockObject)
        {
            _lastFailureTime = DateTime.UtcNow;

            if (_state == CircuitBreakerState.Closed)
            {
                _consecutiveFailures++;
                if (_consecutiveFailures >= _options.FailureThreshold)
                {
                    _state = CircuitBreakerState.Open;
                }
            }
            else if (_state == CircuitBreakerState.HalfOpen)
            {
                _state = CircuitBreakerState.Open;
            }
        }
    }

    /// <summary>
    /// Resets the circuit breaker to the Closed state, clearing all failure and success counters.
    /// </summary>
    public void Reset()
    {
        lock (_lockObject)
        {
            _state = CircuitBreakerState.Closed;
            _consecutiveFailures = 0;
            _successCount = 0;
            _lastFailureTime = DateTime.MinValue;
        }
    }
}
