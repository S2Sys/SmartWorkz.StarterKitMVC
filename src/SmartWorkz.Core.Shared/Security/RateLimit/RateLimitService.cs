namespace SmartWorkz.Shared.Security.RateLimit;

using System.Collections.Concurrent;

/// <summary>
/// Implements token bucket rate limiting algorithm for controlling request rates.
/// </summary>
public class RateLimitService : IRateLimitService
{
    /// <summary>
    /// Represents the token bucket state for a client.
    /// </summary>
    private class TokenBucket
    {
        /// <summary>
        /// The number of tokens currently available in the bucket.
        /// </summary>
        public double Tokens { get; set; }

        /// <summary>
        /// The timestamp when the bucket was last refilled.
        /// </summary>
        public DateTime LastRefillTime { get; set; }

        /// <summary>
        /// The maximum capacity of the bucket.
        /// </summary>
        public int MaxTokens { get; set; }

        /// <summary>
        /// The time window in seconds for the rate limit.
        /// </summary>
        public int WindowSeconds { get; set; }
    }

    /// <summary>
    /// Thread-safe store of token buckets indexed by client ID.
    /// </summary>
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();

    /// <summary>
    /// Lock object for thread-safe operations on buckets.
    /// </summary>
    private readonly ReaderWriterLockSlim _lock = new();

    /// <summary>
    /// Determines whether a request from the specified client is allowed based on rate limiting rules
    /// using the token bucket algorithm.
    /// </summary>
    public async Task<bool> IsRequestAllowedAsync(string clientId, int maxRequests, int windowSeconds)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Client ID cannot be null or empty.", nameof(clientId));

        if (maxRequests <= 0)
            throw new ArgumentException("Max requests must be greater than 0.", nameof(maxRequests));

        if (windowSeconds <= 0)
            throw new ArgumentException("Window seconds must be greater than 0.", nameof(windowSeconds));

        return await Task.Run(() =>
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                var bucket = _buckets.GetOrAdd(clientId, _ => new TokenBucket
                {
                    Tokens = maxRequests,
                    LastRefillTime = DateTime.UtcNow,
                    MaxTokens = maxRequests,
                    WindowSeconds = windowSeconds
                });

                // Refill tokens based on elapsed time
                var now = DateTime.UtcNow;
                var timeElapsed = (now - bucket.LastRefillTime).TotalSeconds;

                if (timeElapsed > 0)
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        // Recalculate in case another thread updated it
                        timeElapsed = (now - bucket.LastRefillTime).TotalSeconds;
                        var refillRate = bucket.MaxTokens / (double)bucket.WindowSeconds;
                        var tokensToAdd = timeElapsed * refillRate;

                        bucket.Tokens = Math.Min(bucket.MaxTokens, bucket.Tokens + tokensToAdd);
                        bucket.LastRefillTime = now;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }

                // Check if we have tokens available
                if (bucket.Tokens >= 1)
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        if (bucket.Tokens >= 1)
                        {
                            bucket.Tokens -= 1;
                            return true;
                        }

                        return false;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }

                return false;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        });
    }

    /// <summary>
    /// Gets the current rate limiting status for a specific client.
    /// </summary>
    public async Task<RateLimitStatus> GetStatusAsync(string clientId, int maxRequests)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Client ID cannot be null or empty.", nameof(clientId));

        if (maxRequests <= 0)
            throw new ArgumentException("Max requests must be greater than 0.", nameof(maxRequests));

        return await Task.Run(() =>
        {
            _lock.EnterReadLock();
            try
            {
                if (!_buckets.TryGetValue(clientId, out var bucket))
                {
                    return new RateLimitStatus
                    {
                        RemainingRequests = maxRequests,
                        MaxRequests = maxRequests,
                        ResetAfterSeconds = 0,
                        ResetTime = DateTime.UtcNow
                    };
                }

                var now = DateTime.UtcNow;
                var timeElapsed = (now - bucket.LastRefillTime).TotalSeconds;
                var refillRate = bucket.MaxTokens / (double)bucket.WindowSeconds;
                var potentialTokens = bucket.Tokens + (timeElapsed * refillRate);
                var availableTokens = Math.Min(bucket.MaxTokens, potentialTokens);
                var remainingRequests = (int)Math.Floor(availableTokens);
                var resetAfterSeconds = bucket.WindowSeconds - (int)Math.Ceiling(timeElapsed);

                return new RateLimitStatus
                {
                    RemainingRequests = Math.Max(0, remainingRequests),
                    MaxRequests = maxRequests,
                    ResetAfterSeconds = Math.Max(0, resetAfterSeconds),
                    ResetTime = bucket.LastRefillTime.AddSeconds(bucket.WindowSeconds)
                };
            }
            finally
            {
                _lock.ExitReadLock();
            }
        });
    }

    /// <summary>
    /// Resets the rate limit for a specific client, clearing all accumulated requests.
    /// </summary>
    public async Task<bool> ResetAsync(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Client ID cannot be null or empty.", nameof(clientId));

        return await Task.Run(() =>
        {
            _lock.EnterWriteLock();
            try
            {
                return _buckets.TryRemove(clientId, out _);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        });
    }
}
