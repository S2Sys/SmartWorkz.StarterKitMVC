using System.Collections.Concurrent;

namespace SmartWorkz.Core.Web.GraphQL.Middleware;

/// <summary>
/// In-memory rate limit store for tracking API requests per key.
/// Enforces 1000 requests per hour per API key.
/// </summary>
public class GraphQLRateLimitStore
{
    private readonly ConcurrentDictionary<string, RateLimitEntry> _limits = new();
    private readonly int _maxRequests = 1000;
    private readonly TimeSpan _window = TimeSpan.FromHours(1);

    /// <summary>
    /// Checks if a request is allowed for the given API key.
    /// Increments the request counter and returns true if within limit.
    /// </summary>
    public bool IsAllowed(string apiKey, out int remaining, out DateTime resetTime)
    {
        var now = DateTime.UtcNow;

        if (_limits.TryGetValue(apiKey, out var entry))
        {
            // Check if window has expired
            if (now > entry.ResetTime)
            {
                // Window expired, reset
                var newEntry = new RateLimitEntry { Count = 1, ResetTime = now.Add(_window) };
                _limits[apiKey] = newEntry;
                remaining = _maxRequests - 1;
                resetTime = newEntry.ResetTime;
                return true;
            }

            // Still within window
            if (entry.Count >= _maxRequests)
            {
                remaining = 0;
                resetTime = entry.ResetTime;
                return false; // Rate limit exceeded
            }

            // Increment and allow
            entry.Count++;
            remaining = _maxRequests - entry.Count;
            resetTime = entry.ResetTime;
            return true;
        }

        // First request for this key
        var firstEntry = new RateLimitEntry { Count = 1, ResetTime = now.Add(_window) };
        _limits[apiKey] = firstEntry;
        remaining = _maxRequests - 1;
        resetTime = firstEntry.ResetTime;
        return true;
    }

    /// <summary>
    /// Gets current limit status for an API key without incrementing.
    /// </summary>
    public void GetStatus(string apiKey, out int remaining, out DateTime resetTime)
    {
        var now = DateTime.UtcNow;

        if (_limits.TryGetValue(apiKey, out var entry))
        {
            if (now > entry.ResetTime)
            {
                // Window has expired
                remaining = _maxRequests;
                resetTime = now.Add(_window);
            }
            else
            {
                remaining = Math.Max(0, _maxRequests - entry.Count);
                resetTime = entry.ResetTime;
            }
        }
        else
        {
            // No entry yet
            remaining = _maxRequests;
            resetTime = now.Add(_window);
        }
    }

    /// <summary>
    /// Internal entry for tracking requests.
    /// </summary>
    private class RateLimitEntry
    {
        public int Count { get; set; }
        public DateTime ResetTime { get; set; }
    }
}
