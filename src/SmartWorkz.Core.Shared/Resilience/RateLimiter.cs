using System.Collections.Concurrent;

namespace SmartWorkz.Core.Shared.Resilience;

/// <summary>
/// Thread-safe token bucket rate limiter implementation.
///
/// This class maintains a per-identifier token bucket that refills at a constant rate.
/// Tokens are consumed when requests are made; if insufficient tokens exist, the request is denied.
///
/// Thread-safe operations use ConcurrentDictionary and locks on individual buckets to ensure
/// consistent state without global locking bottlenecks.
/// </summary>
public sealed class RateLimiter : IRateLimiter
{
    private readonly RateLimiterOptions _options;
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets;

    /// <summary>
    /// Initializes a new instance of the RateLimiter class.
    /// </summary>
    /// <param name="options">Configuration options for the rate limiter.</param>
    public RateLimiter(RateLimiterOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        if (!options.IsValid())
            throw new ArgumentException("Rate limiter options are invalid. MaxRequests and WindowMilliseconds must be greater than 0.", nameof(options));

        _options = options;
        _buckets = new ConcurrentDictionary<string, TokenBucket>();
    }

    /// <summary>
    /// Attempts to acquire tokens for a request identified by the given identifier.
    /// </summary>
    /// <param name="identifier">The identifier for rate limiting (e.g., IP address, user ID).</param>
    /// <param name="cost">The number of tokens to acquire (default: 1).</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// A result containing true if tokens were acquired successfully; otherwise false.
    /// On failure, the Error will include retry-after information.
    /// </returns>
    public async Task<Result<bool>> TryAcquireAsync(
        string identifier,
        int cost = 1,
        CancellationToken cancellationToken = default)
    {
        // Validate cost
        if (cost <= 0)
        {
            return Result.Ok<bool>(false);
        }

        // Cost cannot exceed MaxRequests
        if (cost > _options.MaxRequests)
        {
            var errorMessage = $"Cost {cost} exceeds maximum requests {_options.MaxRequests}. Please retry with a smaller cost. Retry-After: 1000ms";
            return Result.Fail<bool>(new Error("InvalidCost", errorMessage));
        }

        // Get or create the bucket for this identifier
        var bucket = _buckets.GetOrAdd(identifier, _ => new TokenBucket(_options.MaxRequests, _options.WindowMilliseconds));

        // Try to acquire tokens
        var acquired = bucket.TryAcquire(cost);

        if (acquired)
        {
            return Result.Ok(true);
        }

        // Rate limit hit - return success with Data=false
        // The operation succeeded (no exceptions), but the request was rate limited
        return Result.Ok(false);
    }

    /// <summary>
    /// Gets the number of available tokens for a given identifier.
    /// </summary>
    /// <param name="identifier">The identifier to check.</param>
    /// <returns>The number of available tokens.</returns>
    public async Task<int> GetAvailableTokensAsync(string identifier)
    {
        if (_buckets.TryGetValue(identifier, out var bucket))
        {
            return bucket.GetAvailableTokens();
        }

        // New identifier has all tokens available
        return _options.MaxRequests;
    }

    /// <summary>
    /// Resets the rate limit state for a given identifier.
    /// </summary>
    /// <param name="identifier">The identifier to reset.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ResetAsync(string identifier, CancellationToken cancellationToken = default)
    {
        if (_buckets.TryRemove(identifier, out _))
        {
            // Bucket removed; it will be recreated on next access
        }
    }

    /// <summary>
    /// Clears all rate limit state.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _buckets.Clear();
    }

    /// <summary>
    /// Internal token bucket implementation with thread-safe token refill calculation.
    /// </summary>
    private sealed class TokenBucket
    {
        private readonly int _maxTokens;
        private readonly int _windowMilliseconds;
        private readonly double _refillRate;
        private readonly object _lock = new();

        private double _tokens;
        private long _lastRefillTimestamp;

        public TokenBucket(int maxTokens, int windowMilliseconds)
        {
            _maxTokens = maxTokens;
            _windowMilliseconds = windowMilliseconds;
            _refillRate = maxTokens / (double)windowMilliseconds;
            _tokens = maxTokens;
            _lastRefillTimestamp = Environment.TickCount64;
        }

        /// <summary>
        /// Tries to acquire the specified number of tokens.
        /// </summary>
        public bool TryAcquire(int cost)
        {
            lock (_lock)
            {
                RefillTokens();

                if (_tokens >= cost)
                {
                    _tokens -= cost;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the current number of available tokens.
        /// </summary>
        public int GetAvailableTokens()
        {
            lock (_lock)
            {
                RefillTokens();
                return (int)Math.Floor(_tokens);
            }
        }

        /// <summary>
        /// Gets the number of milliseconds to wait before retrying.
        /// </summary>
        public int GetRetryAfterMilliseconds()
        {
            lock (_lock)
            {
                // If we can acquire 1 token, return a small delay
                if (_tokens >= 1)
                {
                    return 0;
                }

                // Calculate time to get 1 token
                var tokensNeeded = 1 - _tokens;
                var timeToWaitMs = (int)Math.Ceiling(tokensNeeded / _refillRate);

                return Math.Max(1, timeToWaitMs);
            }
        }

        /// <summary>
        /// Refills the token bucket based on elapsed time.
        /// </summary>
        private void RefillTokens()
        {
            var now = Environment.TickCount64;
            var elapsedMs = now - _lastRefillTimestamp;

            if (elapsedMs > 0)
            {
                var tokensToAdd = elapsedMs * _refillRate;
                _tokens = Math.Min(_maxTokens, _tokens + tokensToAdd);
                _lastRefillTimestamp = now;
            }
        }
    }
}
