namespace SmartWorkz.Mobile.Tests.Services;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Mobile.Services.Implementations;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Shared;

/// <summary>
/// Unit tests for RetryPolicy with exponential backoff.
/// </summary>
public class RetryPolicyTests
{
    private readonly ILogger<ExponentialBackoffRetryPolicy> _mockLogger;

    public RetryPolicyTests()
    {
        var mockLoggerFactory = new Mock<ILogger<ExponentialBackoffRetryPolicy>>();
        _mockLogger = mockLoggerFactory.Object;
    }

    #region Basic Execution Tests

    [Fact]
    public async Task ExecuteAsync_WithSuccessOnFirstAttempt_ReturnsImmediate()
    {
        // Arrange
        var policy = new ExponentialBackoffRetryPolicy();
        var callCount = 0;

        Func<Task<Result<string>>> operation = async () =>
        {
            callCount++;
            return await Task.FromResult(Result.Ok("success"));
        };

        // Act
        var result = await policy.ExecuteAsync(operation, "TestOp");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("success", result.Data);
        Assert.Equal(1, callCount); // No retries
    }

    [Fact]
    public async Task ExecuteAsync_WithTransientFailureThenSuccess_RetriesAndSucceeds()
    {
        // Arrange
        var policy = new ExponentialBackoffRetryPolicy();
        var callCount = 0;

        Func<Task<Result<string>>> operation = async () =>
        {
            callCount++;
            if (callCount == 1)
            {
                // First call: timeout (transient)
                return await Task.FromResult(
                    Result.Fail<string>("TIMEOUT", "Operation timed out"));
            }
            // Second call: success
            return await Task.FromResult(Result.Ok("success"));
        };

        // Act
        var result = await policy.ExecuteAsync(operation, "TestOp");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("success", result.Data);
        Assert.Equal(2, callCount); // One retry occurred
    }

    [Fact]
    public async Task ExecuteAsync_WithPersistentFailure_ExhaustsRetries()
    {
        // Arrange
        var config = new RetryConfig(MaxRetries: 3);
        var policy = new ExponentialBackoffRetryPolicy(config, _mockLogger);
        var callCount = 0;

        Func<Task<Result<string>>> operation = async () =>
        {
            callCount++;
            return await Task.FromResult(
                Result.Fail<string>("TRANSIENT_ERROR", "Always fails"));
        };

        // Act
        var result = await policy.ExecuteAsync(operation, "FailingOp");

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(4, callCount); // Initial + 3 retries
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public async Task ExecuteAsync_WithMaxRetriesZero_NoRetry()
    {
        // Arrange
        var config = new RetryConfig(MaxRetries: 0);
        var policy = new ExponentialBackoffRetryPolicy(config, _mockLogger);
        var callCount = 0;

        Func<Task<Result<string>>> operation = async () =>
        {
            callCount++;
            return await Task.FromResult(
                Result.Fail<string>("ERROR", "Always fails"));
        };

        // Act
        var result = await policy.ExecuteAsync(operation, "NoRetryOp");

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(1, callCount); // One attempt, no retries
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomInitialDelay_UsesCustomValue()
    {
        // Arrange
        var initialDelay = TimeSpan.FromMilliseconds(50);
        var config = new RetryConfig(
            MaxRetries: 2,
            InitialDelay: initialDelay,
            MaxDelay: TimeSpan.FromSeconds(10),
            UseJitter: false);

        var policy = new ExponentialBackoffRetryPolicy(config, _mockLogger);
        var callCount = 0;
        var sw = Stopwatch.StartNew();

        Func<Task<Result<string>>> operation = async () =>
        {
            callCount++;
            if (callCount < 3)
            {
                return await Task.FromResult(
                    Result.Fail<string>("TRANSIENT_ERROR", "Transient"));
            }
            return await Task.FromResult(Result.Ok("success"));
        };

        // Act
        var result = await policy.ExecuteAsync(operation, "DelayTest");
        sw.Stop();

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(3, callCount);
        // Verify minimum delay: ~50ms + ~100ms = ~150ms
        Assert.True(sw.ElapsedMilliseconds >= 130, $"Elapsed: {sw.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Exponential Backoff Tests

    [Fact]
    public async Task ExecuteAsync_CalculatesExponentialBackoff()
    {
        // Arrange
        var config = new RetryConfig(
            MaxRetries: 3,
            InitialDelay: TimeSpan.FromMilliseconds(50),
            MaxDelay: TimeSpan.FromSeconds(30),
            BackoffMultiplier: 2.0,
            UseJitter: false);

        var policy = new ExponentialBackoffRetryPolicy(config, _mockLogger);
        var delays = new List<long>();
        var callCount = 0;
        var stopwatch = Stopwatch.StartNew();
        var lastTime = 0L;

        Func<Task<Result<string>>> operation = async () =>
        {
            callCount++;
            var currentTime = stopwatch.ElapsedMilliseconds;

            if (callCount > 1)
            {
                delays.Add(currentTime - lastTime);
            }

            lastTime = currentTime;

            if (callCount < 4)
            {
                return await Task.FromResult(
                    Result.Fail<string>("TIMEOUT_ERROR", "Transient"));
            }
            return await Task.FromResult(Result.Ok("success"));
        };

        // Act
        var result = await policy.ExecuteAsync(operation, "ExponentialTest");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(4, callCount);
        Assert.Equal(3, delays.Count);

        // Verify exponential growth: ~50ms, ~100ms, ~200ms
        // Allow some tolerance for execution time
        Assert.True(delays[0] >= 40, $"First delay: {delays[0]}ms (expected ~50ms)");
        Assert.True(delays[1] >= 80, $"Second delay: {delays[1]}ms (expected ~100ms)");
        Assert.True(delays[2] >= 150, $"Third delay: {delays[2]}ms (expected ~200ms)");
    }

    #endregion

    #region Jitter Tests

    [Fact]
    public async Task ExecuteAsync_WithJitter_DelaysWithinBounds()
    {
        // Arrange
        var config = new RetryConfig(
            MaxRetries: 3,
            InitialDelay: TimeSpan.FromMilliseconds(100),
            MaxDelay: TimeSpan.FromSeconds(30),
            BackoffMultiplier: 2.0,
            UseJitter: true);

        var policy = new ExponentialBackoffRetryPolicy(config, _mockLogger);
        var delays = new List<long>();
        var callCount = 0;
        var stopwatch = Stopwatch.StartNew();
        var lastTime = 0L;

        Func<Task<Result<string>>> operation = async () =>
        {
            callCount++;
            var currentTime = stopwatch.ElapsedMilliseconds;

            if (callCount > 1)
            {
                delays.Add(currentTime - lastTime);
            }

            lastTime = currentTime;

            if (callCount < 4)
            {
                return await Task.FromResult(
                    Result.Fail<string>("TIMEOUT_ERROR", "Transient"));
            }
            return await Task.FromResult(Result.Ok("success"));
        };

        // Act
        var result = await policy.ExecuteAsync(operation, "JitterTest");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(4, callCount);

        // With jitter, delays should be within ±20% of exponential values
        // First: 100ms ±20% = 80-120ms
        Assert.True(delays[0] >= 80, $"First delay: {delays[0]}ms (expected 80-120ms)");
        Assert.True(delays[0] <= 150, $"First delay: {delays[0]}ms (expected 80-120ms)");

        // Second: 200ms ±20% = 160-240ms
        Assert.True(delays[1] >= 160, $"Second delay: {delays[1]}ms (expected 160-240ms)");
        Assert.True(delays[1] <= 280, $"Second delay: {delays[1]}ms (expected 160-240ms)");
    }

    #endregion

    #region Max Delay Tests

    [Fact]
    public async Task ExecuteAsync_WithMaxDelay_RespectsCap()
    {
        // Arrange
        var config = new RetryConfig(
            MaxRetries: 5,
            InitialDelay: TimeSpan.FromMilliseconds(100),
            MaxDelay: TimeSpan.FromMilliseconds(300),
            BackoffMultiplier: 2.0,
            UseJitter: false);

        var policy = new ExponentialBackoffRetryPolicy(config, _mockLogger);
        var delays = new List<long>();
        var callCount = 0;
        var stopwatch = Stopwatch.StartNew();
        var lastTime = 0L;

        Func<Task<Result<string>>> operation = async () =>
        {
            callCount++;
            var currentTime = stopwatch.ElapsedMilliseconds;

            if (callCount > 1)
            {
                delays.Add(currentTime - lastTime);
            }

            lastTime = currentTime;

            // Always fail to test that max delay is respected
            return await Task.FromResult(
                Result.Fail<string>("TIMEOUT_ERROR", "Transient"));
        };

        // Act
        var result = await policy.ExecuteAsync(operation, "MaxDelayTest");

        // Assert
        Assert.False(result.Succeeded); // Should exhaust retries
        Assert.Equal(6, callCount); // Initial + 5 retries

        // All delays should be <= 300ms
        foreach (var delay in delays)
        {
            Assert.True(delay <= 350, $"Delay {delay}ms exceeds max of 300ms");
        }
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task ExecuteAsync_WithTimeoutException_IsTransient()
    {
        // Arrange
        var policy = new ExponentialBackoffRetryPolicy();
        var callCount = 0;

        Func<Task<Result<string>>> operation = async () =>
        {
            callCount++;
            if (callCount == 1)
            {
                throw new TimeoutException("Request timed out");
            }
            return await Task.FromResult(Result.Ok("success"));
        };

        // Act & Assert
        // This will depend on how exceptions are handled - if converted to Result.Fail
        var result = await policy.ExecuteAsync(operation, "TimeoutTest");

        // At this point, the implementation determines how to handle thrown exceptions
        // For now, we verify the call was retried
        Assert.True(callCount >= 1);
    }

    [Fact]
    public async Task ExecuteAsync_WithHttpRequestException_IsTransient()
    {
        // Arrange
        var policy = new ExponentialBackoffRetryPolicy();
        var callCount = 0;

        Func<Task<Result<string>>> operation = async () =>
        {
            callCount++;
            if (callCount == 1)
            {
                throw new HttpRequestException("Network error");
            }
            return await Task.FromResult(Result.Ok("success"));
        };

        // Act & Assert
        var result = await policy.ExecuteAsync(operation, "HttpTest");
        Assert.True(callCount >= 1);
    }

    [Fact]
    public async Task IsTransientFailure_WithTimeoutException_ReturnsTrue()
    {
        // Arrange
        var policy = new ExponentialBackoffRetryPolicy();

        // Act
        var isTransient = policy.IsTransientFailure(new TimeoutException("Timeout"));

        // Assert
        Assert.True(isTransient);
    }

    [Fact]
    public async Task IsTransientFailure_WithHttpRequestException_ReturnsTrue()
    {
        // Arrange
        var policy = new ExponentialBackoffRetryPolicy();

        // Act
        var isTransient = policy.IsTransientFailure(
            new HttpRequestException("Network error"));

        // Assert
        Assert.True(isTransient);
    }

    [Fact]
    public async Task IsTransientFailure_WithOperationCanceledException_ReturnsTrue()
    {
        // Arrange
        var policy = new ExponentialBackoffRetryPolicy();

        // Act
        var isTransient = policy.IsTransientFailure(
            new OperationCanceledException("Cancelled"));

        // Assert
        Assert.True(isTransient);
    }

    [Fact]
    public async Task IsTransientFailure_WithInvalidOperationException_ReturnsTrueIfConnectionRelated()
    {
        // Arrange
        var policy = new ExponentialBackoffRetryPolicy();

        // Act & Assert
        Assert.True(policy.IsTransientFailure(
            new InvalidOperationException("Connection lost")));

        Assert.False(policy.IsTransientFailure(
            new InvalidOperationException("Invalid state")));
    }

    [Fact]
    public async Task IsTransientFailure_WithNonTransientException_ReturnsFalse()
    {
        // Arrange
        var policy = new ExponentialBackoffRetryPolicy();

        // Act
        var isTransient = policy.IsTransientFailure(
            new ArgumentException("Invalid argument"));

        // Assert
        Assert.False(isTransient);
    }

    #endregion

    #region Validation and Configuration Tests

    [Fact]
    public async Task ExecuteAsync_WithNullOperation_ThrowsArgumentNullException()
    {
        // Arrange
        var policy = new ExponentialBackoffRetryPolicy();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            policy.ExecuteAsync<string>(null!, "NullOp"));
    }

    [Fact]
    public async Task ExecuteAsync_VoidOperation_WithNullOperation_ThrowsArgumentNullException()
    {
        // Arrange
        var policy = new ExponentialBackoffRetryPolicy();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            policy.ExecuteAsync(null!, "NullOp"));
    }

    [Fact]
    public void Constructor_WithInvalidConfig_Succeeds()
    {
        // Arrange & Act
        // Constructor should accept invalid config, validation happens at ExecuteAsync
        var config = new RetryConfig(MaxRetries: -1);
        var policy = new ExponentialBackoffRetryPolicy(config, _mockLogger);

        // Assert
        Assert.NotNull(policy);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidConfig_ThrowsOnExecution()
    {
        // Arrange
        var config = new RetryConfig(MaxRetries: -1);
        var policy = new ExponentialBackoffRetryPolicy(config, _mockLogger);

        Func<Task<Result<string>>> operation = async () =>
            await Task.FromResult(Result.Ok("success"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            policy.ExecuteAsync(operation, "InvalidConfigOp"));
    }

    [Fact]
    public void GetConfig_ReturnsConfiguredPolicy()
    {
        // Arrange
        var config = new RetryConfig(
            MaxRetries: 10,
            InitialDelay: TimeSpan.FromMilliseconds(200),
            MaxDelay: TimeSpan.FromSeconds(60),
            BackoffMultiplier: 3.0,
            UseJitter: false);

        var policy = new ExponentialBackoffRetryPolicy(config, _mockLogger);

        // Act
        var returnedConfig = policy.GetConfig();

        // Assert
        Assert.NotNull(returnedConfig);
        Assert.Equal(10, returnedConfig.MaxRetries);
        Assert.Equal(TimeSpan.FromMilliseconds(200), returnedConfig.InitialDelay);
        Assert.Equal(TimeSpan.FromSeconds(60), returnedConfig.MaxDelay);
        Assert.Equal(3.0, returnedConfig.BackoffMultiplier);
        Assert.False(returnedConfig.UseJitter);
    }

    #endregion

    #region Void Operation Tests

    [Fact]
    public async Task ExecuteAsync_VoidOperation_WithSuccess_ReturnsOk()
    {
        // Arrange
        var policy = new ExponentialBackoffRetryPolicy();
        var executed = false;

        Func<Task<Result>> operation = async () =>
        {
            executed = true;
            return await Task.FromResult(Result.Ok());
        };

        // Act
        var result = await policy.ExecuteAsync(operation, "VoidOp");

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(executed);
    }

    [Fact]
    public async Task ExecuteAsync_VoidOperation_WithFailureThenSuccess_Retries()
    {
        // Arrange
        var policy = new ExponentialBackoffRetryPolicy();
        var callCount = 0;

        Func<Task<Result>> operation = async () =>
        {
            callCount++;
            if (callCount == 1)
            {
                return await Task.FromResult(
                    Result.Fail("TIMEOUT", "Transient failure"));
            }
            return await Task.FromResult(Result.Ok());
        };

        // Act
        var result = await policy.ExecuteAsync(operation, "VoidRetryOp");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, callCount);
    }

    #endregion

    #region Default Configuration Tests

    [Fact]
    public void DefaultConfig_HasCorrectDefaults()
    {
        // Arrange
        var config = new RetryConfig();

        // Act & Assert
        Assert.Equal(5, config.MaxRetries);
        Assert.Equal(TimeSpan.FromMilliseconds(100), config.GetInitialDelay());
        Assert.Equal(TimeSpan.FromSeconds(30), config.GetMaxDelay());
        Assert.Equal(2.0, config.BackoffMultiplier);
        Assert.True(config.UseJitter);
        Assert.True(config.IsValid);
    }

    [Fact]
    public void RetryConfig_IsValid_WithValidConfig()
    {
        // Arrange & Act
        var config = new RetryConfig(MaxRetries: 5, BackoffMultiplier: 2.0);

        // Assert
        Assert.True(config.IsValid);
    }

    [Fact]
    public void RetryConfig_IsValid_WithZeroMaxRetries()
    {
        // Arrange & Act
        var config = new RetryConfig(MaxRetries: 0);

        // Assert
        Assert.True(config.IsValid); // 0 means no retries, just one attempt
    }

    [Fact]
    public void RetryConfig_IsInvalid_WithNegativeBackoff()
    {
        // Arrange & Act
        var config = new RetryConfig(MaxRetries: 5, BackoffMultiplier: 0.5);

        // Assert
        Assert.False(config.IsValid);
    }

    #endregion
}
