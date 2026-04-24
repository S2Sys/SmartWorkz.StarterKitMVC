namespace SmartWorkz.Mobile.Services;

using SmartWorkz.Mobile.Models;
using SmartWorkz.Shared;

/// <summary>
/// Retry policy with exponential backoff for transient failures.
/// </summary>
/// <remarks>
/// Provides automatic retry mechanism for operations that may fail due to transient issues:
/// - Network timeouts
/// - Temporary service unavailability (5xx errors)
/// - Rate limiting (429 errors)
/// - Transient database locks
/// - Resource contention
///
/// Uses exponential backoff with jitter to avoid thundering herd problem.
/// Automatically detects transient failures and retries with increasing delays.
/// </remarks>
public interface IRetryPolicy
{
    /// <summary>
    /// Execute operation with automatic retry on transient failure.
    /// </summary>
    /// <typeparam name="T">The type of data returned by the operation.</typeparam>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="operationName">Optional name for logging purposes.</param>
    /// <returns>Result containing the operation outcome and data (if successful).</returns>
    /// <remarks>
    /// Executes the operation and automatically retries on transient failures.
    /// Retries are performed with exponential backoff delays.
    /// Non-transient failures are returned immediately without retry.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if operation is null.</exception>
    Task<Result<T>> ExecuteAsync<T>(
        Func<Task<Result<T>>> operation,
        string operationName = "");

    /// <summary>
    /// Execute void operation with automatic retry on transient failure.
    /// </summary>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="operationName">Optional name for logging purposes.</param>
    /// <returns>Result indicating success or failure.</returns>
    /// <remarks>
    /// Executes the operation and automatically retries on transient failures.
    /// Similar to ExecuteAsync&lt;T&gt; but for void operations.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if operation is null.</exception>
    Task<Result> ExecuteAsync(
        Func<Task<Result>> operation,
        string operationName = "");

    /// <summary>
    /// Check if an exception is transient (retry-worthy).
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception is transient and should be retried; false otherwise.</returns>
    /// <remarks>
    /// Transient failures include:
    /// - HttpRequestException (network issues)
    /// - TimeoutException (operation timeout)
    /// - OperationCanceledException (usually due to timeout)
    /// - InvalidOperationException with "connection" in message
    ///
    /// Non-transient failures include argument validation errors, permission errors, etc.
    /// </remarks>
    bool IsTransientFailure(Exception exception);

    /// <summary>
    /// Get the retry policy configuration.
    /// </summary>
    /// <returns>The current RetryConfig.</returns>
    RetryConfig GetConfig();
}
