namespace SmartWorkz.Mobile;

using SmartWorkz.Shared;

/// <summary>
/// Service that prevents duplicate API requests in-flight by caching and reusing Task results.
/// If the same request is made twice concurrently (same key), returns the same Task instead of executing twice.
/// Cache entries are released after completion (both success and failure).
/// </summary>
public interface IRequestDeduplicationService
{
    /// <summary>
    /// Executes an async operation or returns a cached Task if the same key is already in-flight.
    /// Prevents duplicate concurrent requests. Cache is cleared after operation completes (success or failure).
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="key">The unique key identifying the request (e.g., "GET:/api/users/123").</param>
    /// <param name="executeAsync">The async factory function to execute if not already in-flight.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A Task that resolves to a Result{T}. If the same key is in-flight, the same Task is returned.</returns>
    Task<Result<T>> GetOrExecuteAsync<T>(
        string key,
        Func<Task<Result<T>>> executeAsync,
        CancellationToken ct = default);
}
