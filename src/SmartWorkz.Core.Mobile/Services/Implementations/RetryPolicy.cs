namespace SmartWorkz.Mobile.Services.Implementations;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Shared;

/// <summary>
/// Retry policy implementation with exponential backoff for resilient operations.
/// </summary>
/// <remarks>
/// Automatically retries transient failures with exponential backoff:
/// - Formula: delay = min(initialDelay * (multiplier ^ attempt), maxDelay)
/// - Jitter: ±20% random variation to avoid thundering herd
/// - Transient detection: HttpRequestException, TimeoutException, OperationCanceledException
/// - Logging: WARN for retries, ERROR for final failure
/// </remarks>
public class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    private readonly RetryConfig _config;
    private readonly ILogger<ExponentialBackoffRetryPolicy>? _logger;

    /// <summary>
    /// Creates a new ExponentialBackoffRetryPolicy with optional configuration and logging.
    /// </summary>
    /// <param name="config">Configuration for retry behavior. If null, uses default configuration.</param>
    /// <param name="logger">Optional logger for retry attempts and failures.</param>
    public ExponentialBackoffRetryPolicy(RetryConfig? config = null, ILogger<ExponentialBackoffRetryPolicy>? logger = null)
    {
        _config = config ?? new RetryConfig();
        _logger = logger;
    }

    /// <summary>
    /// Execute operation with automatic retry on transient failure.
    /// </summary>
    public async Task<Result<T>> ExecuteAsync<T>(
        Func<Task<Result<T>>> operation,
        string operationName = "")
    {
        operation = Guard.NotNull(operation, nameof(operation));

        if (!_config.IsValid)
        {
            throw new InvalidOperationException(
                $"Invalid retry configuration: MaxRetries={_config.MaxRetries}, " +
                $"BackoffMultiplier={_config.BackoffMultiplier}");
        }

        var attempt = 0;

        while (true)
        {
            try
            {
                // Execute the operation
                var result = await operation();

                // If successful, return immediately
                if (result.Succeeded)
                {
                    return result;
                }

                // Check if failure is transient
                if (!IsTransientFailure(result.Error, result.MessageKey))
                {
                    // Non-transient failure, return immediately
                    _logger?.LogError(
                        "Non-transient failure in {OperationName}: {ErrorCode} - {ErrorMessage}",
                        operationName,
                        result.Error?.Code ?? "UNKNOWN",
                        result.Error?.Message ?? result.MessageKey);

                    return result;
                }

                // Check if we have retries remaining
                if (attempt >= _config.MaxRetries)
                {
                    // No more retries
                    _logger?.LogError(
                        "Operation {OperationName} exhausted {MaxRetries} retries. " +
                        "Final error: {ErrorCode} - {ErrorMessage}",
                        operationName,
                        _config.MaxRetries,
                        result.Error?.Code ?? "UNKNOWN",
                        result.Error?.Message ?? result.MessageKey);

                    return result;
                }

                // Calculate delay for next retry
                var delay = CalculateDelay(attempt);

                _logger?.LogWarning(
                    "Retry attempt {Attempt}/{MaxRetries} for {OperationName} after {DelayMs}ms. " +
                    "Error: {ErrorCode}",
                    attempt + 1,
                    _config.MaxRetries,
                    operationName,
                    delay.TotalMilliseconds,
                    result.Error?.Code ?? "UNKNOWN");

                // Wait before retry
                await Task.Delay(delay);

                attempt++;
            }
            catch (Exception ex)
            {
                // Handle exceptions thrown by the operation
                if (!IsTransientFailure(ex))
                {
                    // Non-transient exception
                    _logger?.LogError(
                        ex,
                        "Non-transient exception in {OperationName}",
                        operationName);

                    return Result.Fail<T>(new Error(
                        "EXCEPTION.NON_TRANSIENT",
                        ex.Message));
                }

                // Check if we have retries remaining
                if (attempt >= _config.MaxRetries)
                {
                    // No more retries
                    _logger?.LogError(
                        ex,
                        "Operation {OperationName} exhausted {MaxRetries} retries",
                        operationName,
                        _config.MaxRetries);

                    return Result.Fail<T>(new Error(
                        "EXCEPTION.TRANSIENT",
                        ex.Message));
                }

                // Calculate delay for next retry
                var delay = CalculateDelay(attempt);

                _logger?.LogWarning(
                    "Retry attempt {Attempt}/{MaxRetries} for {OperationName} after {DelayMs}ms. " +
                    "Exception: {ExceptionType}",
                    attempt + 1,
                    _config.MaxRetries,
                    operationName,
                    delay.TotalMilliseconds,
                    ex.GetType().Name);

                // Wait before retry
                await Task.Delay(delay);

                attempt++;
            }
        }
    }

    /// <summary>
    /// Execute void operation with automatic retry on transient failure.
    /// </summary>
    public async Task<Result> ExecuteAsync(
        Func<Task<Result>> operation,
        string operationName = "")
    {
        operation = Guard.NotNull(operation, nameof(operation));

        if (!_config.IsValid)
        {
            throw new InvalidOperationException(
                $"Invalid retry configuration: MaxRetries={_config.MaxRetries}, " +
                $"BackoffMultiplier={_config.BackoffMultiplier}");
        }

        var attempt = 0;

        while (true)
        {
            try
            {
                // Execute the operation
                var result = await operation();

                // If successful, return immediately
                if (result.Succeeded)
                {
                    return result;
                }

                // Check if failure is transient
                if (!IsTransientFailure(result.Error, result.MessageKey))
                {
                    // Non-transient failure, return immediately
                    _logger?.LogError(
                        "Non-transient failure in {OperationName}: {ErrorCode} - {ErrorMessage}",
                        operationName,
                        result.Error?.Code ?? "UNKNOWN",
                        result.Error?.Message ?? result.MessageKey);

                    return result;
                }

                // Check if we have retries remaining
                if (attempt >= _config.MaxRetries)
                {
                    // No more retries
                    _logger?.LogError(
                        "Operation {OperationName} exhausted {MaxRetries} retries. " +
                        "Final error: {ErrorCode} - {ErrorMessage}",
                        operationName,
                        _config.MaxRetries,
                        result.Error?.Code ?? "UNKNOWN",
                        result.Error?.Message ?? result.MessageKey);

                    return result;
                }

                // Calculate delay for next retry
                var delay = CalculateDelay(attempt);

                _logger?.LogWarning(
                    "Retry attempt {Attempt}/{MaxRetries} for {OperationName} after {DelayMs}ms. " +
                    "Error: {ErrorCode}",
                    attempt + 1,
                    _config.MaxRetries,
                    operationName,
                    delay.TotalMilliseconds,
                    result.Error?.Code ?? "UNKNOWN");

                // Wait before retry
                await Task.Delay(delay);

                attempt++;
            }
            catch (Exception ex)
            {
                // Handle exceptions thrown by the operation
                if (!IsTransientFailure(ex))
                {
                    // Non-transient exception
                    _logger?.LogError(
                        ex,
                        "Non-transient exception in {OperationName}",
                        operationName);

                    return Result.Fail(new Error(
                        "EXCEPTION.NON_TRANSIENT",
                        ex.Message));
                }

                // Check if we have retries remaining
                if (attempt >= _config.MaxRetries)
                {
                    // No more retries
                    _logger?.LogError(
                        ex,
                        "Operation {OperationName} exhausted {MaxRetries} retries",
                        operationName,
                        _config.MaxRetries);

                    return Result.Fail(new Error(
                        "EXCEPTION.TRANSIENT",
                        ex.Message));
                }

                // Calculate delay for next retry
                var delay = CalculateDelay(attempt);

                _logger?.LogWarning(
                    "Retry attempt {Attempt}/{MaxRetries} for {OperationName} after {DelayMs}ms. " +
                    "Exception: {ExceptionType}",
                    attempt + 1,
                    _config.MaxRetries,
                    operationName,
                    delay.TotalMilliseconds,
                    ex.GetType().Name);

                // Wait before retry
                await Task.Delay(delay);

                attempt++;
            }
        }
    }

    /// <summary>
    /// Check if an exception is transient (retry-worthy).
    /// </summary>
    public bool IsTransientFailure(Exception exception)
    {
        if (exception == null)
        {
            return false;
        }

        // Network/communication errors
        if (exception is HttpRequestException)
        {
            return true;
        }

        // Timeout errors
        if (exception is TimeoutException)
        {
            return true;
        }

        // Cancellation (usually due to timeout)
        if (exception is OperationCanceledException)
        {
            return true;
        }

        // Connection-related errors
        if (exception is InvalidOperationException invalidOpEx &&
            invalidOpEx.Message.Contains("connection", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if an error is transient (retry-worthy).
    /// </summary>
    private bool IsTransientFailure(Error? error, string? messageKey = null)
    {
        // Check error code if present
        if (error != null)
        {
            var code = error.Code.ToUpperInvariant();

            // Network/timeout errors
            if (code.Contains("TIMEOUT") ||
                code.Contains("NETWORK") ||
                code.Contains("CONNECTION") ||
                code.Contains("UNAVAILABLE") ||
                code.Contains("TRANSIENT"))
            {
                return true;
            }

            // Rate limiting
            if (code.Contains("RATE_LIMIT") ||
                code.Contains("429"))
            {
                return true;
            }

            // Service errors (5xx)
            if (code.Contains("SERVICE_ERROR") ||
                code.Contains("SERVER_ERROR") ||
                code.Contains("500") ||
                code.Contains("502") ||
                code.Contains("503") ||
                code.Contains("504"))
            {
                return true;
            }
        }

        // Check message key if present (for Result.Fail with messageKey pattern)
        if (!string.IsNullOrEmpty(messageKey))
        {
            var key = messageKey.ToUpperInvariant();

            // Network/timeout errors
            if (key.Contains("TIMEOUT") ||
                key.Contains("NETWORK") ||
                key.Contains("CONNECTION") ||
                key.Contains("UNAVAILABLE") ||
                key.Contains("TRANSIENT"))
            {
                return true;
            }

            // Rate limiting
            if (key.Contains("RATE_LIMIT") ||
                key.Contains("429"))
            {
                return true;
            }

            // Service errors (5xx)
            if (key.Contains("SERVICE_ERROR") ||
                key.Contains("SERVER_ERROR") ||
                key.Contains("500") ||
                key.Contains("502") ||
                key.Contains("503") ||
                key.Contains("504"))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Get the retry policy configuration.
    /// </summary>
    public RetryConfig GetConfig() => _config;

    /// <summary>
    /// Calculate the delay for the given attempt number using exponential backoff with optional jitter.
    /// </summary>
    /// <param name="attempt">The attempt number (0-indexed).</param>
    /// <returns>The delay timespan.</returns>
    /// <remarks>
    /// Formula: delay = min(initialDelay * (multiplier ^ attempt), maxDelay)
    /// With jitter: delay += random(-20% to +20% of delay)
    /// </remarks>
    private TimeSpan CalculateDelay(int attempt)
    {
        var initialDelayMs = _config.GetInitialDelay().TotalMilliseconds;
        var maxDelayMs = _config.GetMaxDelay().TotalMilliseconds;

        // Calculate exponential backoff: initialDelay * (multiplier ^ attempt)
        var exponentialDelayMs = initialDelayMs * Math.Pow(_config.BackoffMultiplier, attempt);

        // Cap at max delay
        var cappedDelayMs = Math.Min(exponentialDelayMs, maxDelayMs);

        // Apply jitter if enabled
        if (_config.UseJitter)
        {
            // ±20% jitter
            var jitterPercent = (Random.Shared.NextDouble() - 0.5) * 0.4; // -0.2 to +0.2
            var jitterMs = cappedDelayMs * jitterPercent;
            cappedDelayMs = Math.Max(1, cappedDelayMs + jitterMs); // Ensure at least 1ms
        }

        return TimeSpan.FromMilliseconds(cappedDelayMs);
    }
}
