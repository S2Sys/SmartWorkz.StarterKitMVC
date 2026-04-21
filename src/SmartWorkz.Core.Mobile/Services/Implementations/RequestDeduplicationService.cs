namespace SmartWorkz.Mobile;

using Microsoft.Extensions.Logging;
using SmartWorkz.Shared;

/// <summary>
/// Prevents duplicate API requests in-flight by caching and reusing Task results.
/// Thread-safe dictionary maintains in-flight requests keyed by (method:endpoint).
/// When same request is detected, returns cached Task instead of executing.
/// After execution completes, entry is cleaned up.
/// </summary>
public sealed class RequestDeduplicationService : IRequestDeduplicationService
{
    private readonly ILogger<RequestDeduplicationService> _logger;
    private readonly Dictionary<string, object> _inFlightRequests = new();
    private readonly object _lock = new();

    public RequestDeduplicationService(ILogger<RequestDeduplicationService> logger)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public Task<Result<T>> GetOrExecuteAsync<T>(
        string key,
        Func<Task<Result<T>>> executeAsync,
        CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (_inFlightRequests.TryGetValue(key, out var cachedTask) && cachedTask is Task<Result<T>> cachedTypedTask)
            {
                _logger.LogDebug("Request deduplication: reusing in-flight request for key {Key}", key);
                return cachedTypedTask;
            }

            // Create task INSIDE the lock
            var newTask = ExecuteAndCleanupAsync<T>(key, executeAsync, ct);

            // Store INSIDE the lock — atomic with the check above
            _inFlightRequests[key] = newTask;

            _logger.LogDebug("Executing request for deduplication key {Key}", key);
            return newTask;
        }
    }

    private async Task<Result<T>> ExecuteAndCleanupAsync<T>(
        string key,
        Func<Task<Result<T>>> executeAsync,
        CancellationToken ct)
    {
        try
        {
            return await executeAsync();
        }
        finally
        {
            lock (_lock)
            {
                _inFlightRequests.Remove(key);
                _logger.LogDebug("Deduplication cache entry removed for key {Key}", key);
            }
        }
    }
}
