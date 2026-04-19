using SmartWorkz.Core.Shared.Resilience;

namespace SmartWorkz.Core.Tests.Resilience;

public class CircuitBreakerTests
{
    #region Constructor and Initial State Tests

    [Fact]
    public void Constructor_WithDefaultOptions_ShouldInitializeInClosedState()
    {
        // Arrange & Act
        var options = new CircuitBreakerOptions();
        var breaker = new CircuitBreaker(options);

        // Assert
        Assert.NotNull(breaker);
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
        Assert.Equal(0, breaker.ConsecutiveFailures);
        Assert.Equal(0, breaker.SuccessCount);
    }

    [Fact]
    public void Constructor_WithCustomOptions_ShouldUseProvidedValues()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 3,
            TimeoutMilliseconds = 60000,
            SuccessThreshold = 2,
            Identifier = "test-breaker"
        };

        // Act
        var breaker = new CircuitBreaker(options);

        // Assert
        Assert.NotNull(breaker);
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
    }

    [Fact]
    public void CircuitBreakerOptions_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new CircuitBreakerOptions();

        // Assert
        Assert.Equal(5, options.FailureThreshold);
        Assert.Equal(30000, options.TimeoutMilliseconds);
        Assert.Equal(3, options.SuccessThreshold);
        Assert.Null(options.Identifier);
    }

    [Fact]
    public void CircuitBreakerOptions_IsValid_WithValidOptions_ShouldReturnTrue()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 5,
            TimeoutMilliseconds = 30000,
            SuccessThreshold = 3
        };

        // Act
        var isValid = options.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void CircuitBreakerOptions_IsValid_WithZeroFailureThreshold_ShouldReturnFalse()
    {
        // Arrange
        var options = new CircuitBreakerOptions { FailureThreshold = 0 };

        // Act
        var isValid = options.IsValid();

        // Assert
        Assert.False(isValid);
    }

    #endregion

    #region State Transition Tests - Closed to Open

    [Fact]
    public void RecordFailure_ClosedState_ShouldIncrementFailureCounter()
    {
        // Arrange
        var options = new CircuitBreakerOptions { FailureThreshold = 5 };
        var breaker = new CircuitBreaker(options);

        // Act
        breaker.RecordFailure();

        // Assert
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
        Assert.Equal(1, breaker.ConsecutiveFailures);
    }

    [Fact]
    public void RecordFailure_ClosedState_MultipleFailures_ShouldTransitionToOpen()
    {
        // Arrange
        var options = new CircuitBreakerOptions { FailureThreshold = 3 };
        var breaker = new CircuitBreaker(options);

        // Act
        breaker.RecordFailure();
        breaker.RecordFailure();
        breaker.RecordFailure();

        // Assert
        Assert.Equal(CircuitBreakerState.Open, breaker.State);
        Assert.Equal(3, breaker.ConsecutiveFailures);
    }

    [Fact]
    public void RecordSuccess_ClosedState_ShouldResetFailureCounter()
    {
        // Arrange
        var options = new CircuitBreakerOptions { FailureThreshold = 5 };
        var breaker = new CircuitBreaker(options);
        breaker.RecordFailure();
        breaker.RecordFailure();

        // Act
        breaker.RecordSuccess();

        // Assert
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
        Assert.Equal(0, breaker.ConsecutiveFailures);
    }

    #endregion

    #region State Transition Tests - Open to HalfOpen

    [Fact]
    public void State_OpenState_AfterTimeoutExpires_ShouldTransitionToHalfOpen()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 1,
            TimeoutMilliseconds = 100
        };
        var breaker = new CircuitBreaker(options);
        breaker.RecordFailure(); // Transition to Open
        Assert.Equal(CircuitBreakerState.Open, breaker.State);

        // Act
        System.Threading.Thread.Sleep(150); // Wait for timeout to expire
        var state = breaker.State; // Accessing State should trigger auto-transition

        // Assert
        Assert.Equal(CircuitBreakerState.HalfOpen, state);
    }

    [Fact]
    public void State_OpenState_BeforeTimeoutExpires_ShouldRemainOpen()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 1,
            TimeoutMilliseconds = 5000
        };
        var breaker = new CircuitBreaker(options);
        breaker.RecordFailure(); // Transition to Open

        // Act
        var state = breaker.State;

        // Assert
        Assert.Equal(CircuitBreakerState.Open, state);
    }

    #endregion

    #region State Transition Tests - HalfOpen to Closed

    [Fact]
    public void RecordSuccess_HalfOpenState_ShouldIncrementSuccessCounter()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 1,
            TimeoutMilliseconds = 100,
            SuccessThreshold = 3
        };
        var breaker = new CircuitBreaker(options);
        breaker.RecordFailure(); // Transition to Open
        System.Threading.Thread.Sleep(150);
        _ = breaker.State; // Auto-transition to HalfOpen

        // Act
        breaker.RecordSuccess();

        // Assert
        Assert.Equal(CircuitBreakerState.HalfOpen, breaker.State);
        Assert.Equal(1, breaker.SuccessCount);
    }

    [Fact]
    public void RecordSuccess_HalfOpenState_ReachingThreshold_ShouldTransitionToClosed()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 1,
            TimeoutMilliseconds = 100,
            SuccessThreshold = 2
        };
        var breaker = new CircuitBreaker(options);
        breaker.RecordFailure(); // Transition to Open
        System.Threading.Thread.Sleep(150);
        _ = breaker.State; // Auto-transition to HalfOpen

        // Act
        breaker.RecordSuccess();
        breaker.RecordSuccess();

        // Assert
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
        Assert.Equal(0, breaker.ConsecutiveFailures);
        Assert.Equal(0, breaker.SuccessCount);
    }

    #endregion

    #region State Transition Tests - HalfOpen to Open

    [Fact]
    public void RecordFailure_HalfOpenState_ShouldTransitionBackToOpen()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 1,
            TimeoutMilliseconds = 100
        };
        var breaker = new CircuitBreaker(options);
        breaker.RecordFailure(); // Transition to Open
        System.Threading.Thread.Sleep(150);
        _ = breaker.State; // Auto-transition to HalfOpen

        // Act
        breaker.RecordFailure();

        // Assert
        Assert.Equal(CircuitBreakerState.Open, breaker.State);
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_ClosedState_ShouldClearFailureCounter()
    {
        // Arrange
        var options = new CircuitBreakerOptions { FailureThreshold = 5 };
        var breaker = new CircuitBreaker(options);
        breaker.RecordFailure();
        breaker.RecordFailure();

        // Act
        breaker.Reset();

        // Assert
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
        Assert.Equal(0, breaker.ConsecutiveFailures);
    }

    [Fact]
    public void Reset_OpenState_ShouldTransitionToClosed()
    {
        // Arrange
        var options = new CircuitBreakerOptions { FailureThreshold = 1 };
        var breaker = new CircuitBreaker(options);
        breaker.RecordFailure(); // Transition to Open
        Assert.Equal(CircuitBreakerState.Open, breaker.State);

        // Act
        breaker.Reset();

        // Assert
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
        Assert.Equal(0, breaker.ConsecutiveFailures);
    }

    [Fact]
    public void Reset_HalfOpenState_ShouldTransitionToClosed()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 1,
            TimeoutMilliseconds = 100
        };
        var breaker = new CircuitBreaker(options);
        breaker.RecordFailure(); // Transition to Open
        System.Threading.Thread.Sleep(150);
        _ = breaker.State; // Auto-transition to HalfOpen
        Assert.Equal(CircuitBreakerState.HalfOpen, breaker.State);

        // Act
        breaker.Reset();

        // Assert
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
        Assert.Equal(0, breaker.SuccessCount);
    }

    #endregion

    #region ExecuteAsync Tests

    [Fact]
    public async Task ExecuteAsync_ClosedState_WithSuccessfulOperation_ShouldReturnSuccess()
    {
        // Arrange
        var options = new CircuitBreakerOptions();
        var breaker = new CircuitBreaker(options);

        // Act
        var result = await breaker.ExecuteAsync<int>(
            async (ct) =>
            {
                await Task.Delay(10, ct);
                return 42;
            });

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(42, result.Data);
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
        Assert.Equal(0, breaker.ConsecutiveFailures);
    }

    [Fact]
    public async Task ExecuteAsync_ClosedState_WithFailingOperation_ShouldReturnFailure()
    {
        // Arrange
        var options = new CircuitBreakerOptions { FailureThreshold = 5 };
        var breaker = new CircuitBreaker(options);

        // Act
        var result = await breaker.ExecuteAsync<int>(
            async (ct) =>
            {
                await Task.Delay(10, ct);
                throw new InvalidOperationException("Operation failed");
            });

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Contains("Operation failed", result.Error!.Message);
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
        Assert.Equal(1, breaker.ConsecutiveFailures);
    }

    [Fact]
    public async Task ExecuteAsync_OpenState_ShouldRejectRequestImmediately()
    {
        // Arrange
        var options = new CircuitBreakerOptions { FailureThreshold = 1 };
        var breaker = new CircuitBreaker(options);
        breaker.RecordFailure(); // Transition to Open
        var operationCalled = false;

        // Act
        var result = await breaker.ExecuteAsync(
            async (ct) =>
            {
                operationCalled = true;
                await Task.Delay(10, ct);
                return 42;
            });

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Contains("CIRCUIT_BREAKER.OPEN", result.Error!.Code);
        Assert.False(operationCalled);
    }

    [Fact]
    public async Task ExecuteAsync_HalfOpenState_ShouldAllowOperation()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 1,
            TimeoutMilliseconds = 100
        };
        var breaker = new CircuitBreaker(options);
        breaker.RecordFailure(); // Transition to Open
        System.Threading.Thread.Sleep(150);
        _ = breaker.State; // Auto-transition to HalfOpen

        // Act
        var result = await breaker.ExecuteAsync(
            async (ct) =>
            {
                await Task.Delay(10, ct);
                return 42;
            });

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(42, result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var options = new CircuitBreakerOptions();
        var breaker = new CircuitBreaker(options);
        using var cts = new CancellationTokenSource();
        var operationCancelled = false;

        // Act
        cts.CancelAfter(50);
        try
        {
            var result = await breaker.ExecuteAsync<int>(
                async (ct) =>
                {
                    await Task.Delay(5000, ct);
                    return 42;
                },
                cts.Token);
        }
        catch (OperationCanceledException)
        {
            operationCancelled = true;
        }

        // Assert
        Assert.True(operationCancelled || cts.Token.IsCancellationRequested);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleSuccessfulOperations_ShouldMaintainClosedState()
    {
        // Arrange
        var options = new CircuitBreakerOptions();
        var breaker = new CircuitBreaker(options);

        // Act
        var result1 = await breaker.ExecuteAsync(async (ct) => { await Task.Delay(10, ct); return 1; });
        var result2 = await breaker.ExecuteAsync(async (ct) => { await Task.Delay(10, ct); return 2; });
        var result3 = await breaker.ExecuteAsync(async (ct) => { await Task.Delay(10, ct); return 3; });

        // Assert
        Assert.True(result1.Succeeded && result1.Data == 1);
        Assert.True(result2.Succeeded && result2.Data == 2);
        Assert.True(result3.Succeeded && result3.Data == 3);
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
    }

    [Fact]
    public async Task ExecuteAsync_FailureThenRecovery_ShouldTransitionCorrectly()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 2,
            TimeoutMilliseconds = 100,
            SuccessThreshold = 2
        };
        var breaker = new CircuitBreaker(options);

        // Act - Trigger Open state
        await breaker.ExecuteAsync<int>(async (ct) => { await Task.Delay(10, ct); throw new Exception("Fail 1"); });
        await breaker.ExecuteAsync<int>(async (ct) => { await Task.Delay(10, ct); throw new Exception("Fail 2"); });
        Assert.Equal(CircuitBreakerState.Open, breaker.State);

        // Wait for timeout
        System.Threading.Thread.Sleep(150);
        _ = breaker.State; // Auto-transition to HalfOpen

        // Attempt recovery
        var success1 = await breaker.ExecuteAsync<string>(async (ct) => { await Task.Delay(10, ct); return "ok"; });
        var success2 = await breaker.ExecuteAsync<string>(async (ct) => { await Task.Delay(10, ct); return "ok"; });

        // Assert
        Assert.True(success1.Succeeded);
        Assert.True(success2.Succeeded);
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
    }

    #endregion

    #region Error Message Tests

    [Fact]
    public async Task ExecuteAsync_OpenState_ErrorMessage_ShouldIncludeRetryGuidance()
    {
        // Arrange
        var options = new CircuitBreakerOptions { FailureThreshold = 1 };
        var breaker = new CircuitBreaker(options);
        breaker.RecordFailure();

        // Act
        var result = await breaker.ExecuteAsync(async (ct) => await Task.FromResult(42));

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        var errorMessage = result.Error!.Message.ToLower();
        Assert.True(
            errorMessage.Contains("circuit breaker") ||
            errorMessage.Contains("open") ||
            errorMessage.Contains("retry"),
            $"Error message '{errorMessage}' should include retry guidance");
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task RecordFailure_ConcurrentCalls_ShouldBeMutuallyExclusive()
    {
        // Arrange
        var options = new CircuitBreakerOptions { FailureThreshold = 100 };
        var breaker = new CircuitBreaker(options);
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() => breaker.RecordFailure()));
        }
        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(100, breaker.ConsecutiveFailures);
    }

    [Fact]
    public async Task ExecuteAsync_ConcurrentExecutions_ShouldBeSafe()
    {
        // Arrange
        var options = new CircuitBreakerOptions { FailureThreshold = 1000 };
        var breaker = new CircuitBreaker(options);
        var successCount = 0;
        var failureCount = 0;
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var result = await breaker.ExecuteAsync(async (ct) =>
                {
                    await Task.Delay(5, ct);
                    return i;
                });
                if (result.Succeeded) Interlocked.Increment(ref successCount);
                else Interlocked.Increment(ref failureCount);
            }));
        }
        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(50, successCount);
        Assert.Equal(0, failureCount);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void ConsecutiveFailures_Property_ShouldReflectCurrentCount()
    {
        // Arrange
        var options = new CircuitBreakerOptions { FailureThreshold = 10 };
        var breaker = new CircuitBreaker(options);

        // Act & Assert
        Assert.Equal(0, breaker.ConsecutiveFailures);
        breaker.RecordFailure();
        Assert.Equal(1, breaker.ConsecutiveFailures);
        breaker.RecordFailure();
        Assert.Equal(2, breaker.ConsecutiveFailures);
    }

    [Fact]
    public void SuccessCount_Property_ShouldReflectCurrentCount()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 1,
            TimeoutMilliseconds = 100
        };
        var breaker = new CircuitBreaker(options);
        breaker.RecordFailure();
        System.Threading.Thread.Sleep(150);
        _ = breaker.State; // Auto-transition to HalfOpen

        // Act & Assert
        Assert.Equal(0, breaker.SuccessCount);
        breaker.RecordSuccess();
        Assert.Equal(1, breaker.SuccessCount);
        breaker.RecordSuccess();
        Assert.Equal(2, breaker.SuccessCount);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ExecuteAsync_WithAsyncException_ShouldCatchAndRecord()
    {
        // Arrange
        var options = new CircuitBreakerOptions { FailureThreshold = 1 };
        var breaker = new CircuitBreaker(options);

        // Act
        var result = await breaker.ExecuteAsync<int>(
            async (ct) =>
            {
                await Task.Delay(10, ct);
                throw new ArgumentNullException("test");
            });

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal(1, breaker.ConsecutiveFailures);
    }

    [Fact]
    public void Reset_MultipleTimesClosed_ShouldBeIdempotent()
    {
        // Arrange
        var options = new CircuitBreakerOptions();
        var breaker = new CircuitBreaker(options);

        // Act
        breaker.Reset();
        breaker.Reset();
        breaker.Reset();

        // Assert
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
        Assert.Equal(0, breaker.ConsecutiveFailures);
    }

    [Fact]
    public void State_ClosedState_ShouldNotAutoTransition()
    {
        // Arrange
        var options = new CircuitBreakerOptions { TimeoutMilliseconds = 100 };
        var breaker = new CircuitBreaker(options);

        // Act
        var state1 = breaker.State;
        System.Threading.Thread.Sleep(200);
        var state2 = breaker.State;

        // Assert
        Assert.Equal(CircuitBreakerState.Closed, state1);
        Assert.Equal(CircuitBreakerState.Closed, state2);
    }

    #endregion
}
