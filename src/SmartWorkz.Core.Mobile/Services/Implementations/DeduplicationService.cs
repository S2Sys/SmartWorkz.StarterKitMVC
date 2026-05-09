namespace SmartWorkz.Mobile.Services.Implementations;

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartWorkz.Shared;

/// <summary>
/// Service to prevent processing duplicate messages within a configurable time window.
/// Uses ConcurrentDictionary to track message IDs and their timestamps.
/// </summary>
public class DeduplicationService : IDeduplicationService
{
    private readonly TimeSpan _dedupWindow;
    private readonly ILogger<DeduplicationService>? _logger;
    private readonly ConcurrentDictionary<string, DateTime> _trackedMessages;

    /// <summary>
    /// Initializes a new instance of the DeduplicationService.
    /// </summary>
    /// <param name="dedupWindow">Time window for tracking message IDs. Default: 5 minutes.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    public DeduplicationService(TimeSpan? dedupWindow = null, ILogger<DeduplicationService>? logger = null)
    {
        _dedupWindow = dedupWindow ?? TimeSpan.FromMinutes(5);
        _logger = logger;
        _trackedMessages = new ConcurrentDictionary<string, DateTime>();
    }

    /// <summary>
    /// Check if message is duplicate and record if new.
    /// </summary>
    /// <param name="messageId">Unique identifier for the message.</param>
    /// <returns>Result with true if duplicate, false if new message.</returns>
    /// <exception cref="ArgumentException">Thrown when messageId is null or empty.</exception>
    public Task<Result<bool>> IsDuplicateAsync(string messageId)
    {
        if (string.IsNullOrWhiteSpace(messageId))
        {
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));
        }

        var now = DateTime.UtcNow;
        var isDuplicate = false;

        // Check if message exists and is within the dedup window
        if (_trackedMessages.TryGetValue(messageId, out var recordedTime))
        {
            var elapsed = now - recordedTime;
            if (elapsed <= _dedupWindow)
            {
                // Message is a duplicate (within window)
                isDuplicate = true;
            }
            else
            {
                // Message is outside the window, treat as new
                _trackedMessages.AddOrUpdate(messageId, now, (_, _) => now);
                _logger?.LogDebug("Re-recording message {MessageId} after dedup window expired", messageId);
                isDuplicate = false;
            }
        }
        else
        {
            // New message - add to tracking
            _trackedMessages.AddOrUpdate(messageId, now, (_, _) => now);
            _logger?.LogDebug("Recording new message {MessageId}", messageId);
            isDuplicate = false;
        }

        return Task.FromResult(Result.Ok(isDuplicate));
    }

    /// <summary>
    /// Record a message as seen (for manual tracking).
    /// </summary>
    /// <param name="messageId">Unique identifier for the message.</param>
    /// <returns>Result indicating success.</returns>
    /// <exception cref="ArgumentException">Thrown when messageId is null or empty.</exception>
    public Task<Result> RecordMessageAsync(string messageId)
    {
        if (string.IsNullOrWhiteSpace(messageId))
        {
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));
        }

        var now = DateTime.UtcNow;
        _trackedMessages.AddOrUpdate(messageId, now, (_, _) => now);
        _logger?.LogDebug("Manually recorded message {MessageId}", messageId);

        return Task.FromResult(Result.Ok());
    }

    /// <summary>
    /// Clear old deduplication records older than the specified time span.
    /// </summary>
    /// <param name="olderThan">Time span for determining old records. Records older than DateTime.UtcNow - olderThan are removed.</param>
    /// <returns>Result indicating success.</returns>
    public Task<Result> CleanupAsync(TimeSpan olderThan)
    {
        var cutoffTime = DateTime.UtcNow - olderThan;
        var keysToRemove = new List<string>();

        foreach (var kvp in _trackedMessages)
        {
            if (kvp.Value < cutoffTime)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        int removedCount = 0;
        foreach (var key in keysToRemove)
        {
            if (_trackedMessages.TryRemove(key, out _))
            {
                removedCount++;
            }
        }

        _logger?.LogDebug("Cleaned up {Count} old messages from deduplication tracking", removedCount);

        return Task.FromResult(Result.Ok());
    }

    /// <summary>
    /// Get count of currently tracked messages.
    /// </summary>
    /// <returns>Result with count of tracked message IDs.</returns>
    public Task<Result<int>> GetTrackedCountAsync()
    {
        var count = _trackedMessages.Count;

        if (count > 1000)
        {
            _logger?.LogInformation("Deduplication service has {Count} tracked messages. Consider running cleanup.", count);
        }

        return Task.FromResult(Result.Ok(count));
    }
}
