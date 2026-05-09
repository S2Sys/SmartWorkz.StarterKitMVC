namespace SmartWorkz.Mobile.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Shared;

/// <summary>
/// In-memory queue for messages when connection is offline.
/// Stores pending messages locally and sends them when reconnected.
/// </summary>
public class OfflineMessageQueue : IOfflineMessageQueue
{
    private readonly List<QueuedMessage> _queue = new();
    private readonly ILogger<OfflineMessageQueue>? _logger;

    /// <summary>
    /// Initializes a new instance of the OfflineMessageQueue class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    public OfflineMessageQueue(ILogger<OfflineMessageQueue>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Add message to queue for later sending.
    /// </summary>
    public async Task<Result> EnqueueAsync(string channel, string method, object?[] args)
    {
        // Validate inputs synchronously to allow exceptions to propagate
        Guard.NotEmpty(channel, nameof(channel));
        Guard.NotEmpty(method, nameof(method));
        Guard.NotNull(args, nameof(args));

        return await Task.Run(() =>
        {
            try
            {
                var messageId = Guid.NewGuid().ToString();
                var queuedMessage = new QueuedMessage(
                    MessageId: messageId,
                    Channel: channel,
                    Method: method,
                    Args: args,
                    QueuedAt: DateTime.UtcNow,
                    RetryCount: 0);

                _queue.Add(queuedMessage);

                _logger?.LogInformation(
                    "Queued message {Method} to {Channel} with ID {MessageId}",
                    method, channel, messageId);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error enqueueing message");
                return Result.Fail(ex.Message);
            }
        });
    }

    /// <summary>
    /// Get all queued messages.
    /// </summary>
    public async Task<Result<IReadOnlyList<QueuedMessage>>> GetQueuedMessagesAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                var nonExpiredMessages = _queue
                    .Where(m => !m.IsExpired)
                    .ToList()
                    .AsReadOnly();

                var expiredCount = _queue.Count - nonExpiredMessages.Count;

                _logger?.LogDebug(
                    "Retrieved {ActiveCount} queued messages ({ExpiredCount} expired)",
                    nonExpiredMessages.Count, expiredCount);

                return Result.Ok<IReadOnlyList<QueuedMessage>>(nonExpiredMessages);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving queued messages");
                return Result.Fail<IReadOnlyList<QueuedMessage>>(ex.Message);
            }
        });
    }

    /// <summary>
    /// Remove message from queue (after successful send).
    /// </summary>
    public async Task<Result> DequeueAsync(string messageId)
    {
        return await Task.Run(() =>
        {
            try
            {
                Guard.NotEmpty(messageId, nameof(messageId));

                var message = _queue.FirstOrDefault(m => m.MessageId == messageId);

                if (message == null)
                {
                    _logger?.LogWarning("Message {MessageId} not found for dequeue", messageId);
                    return Result.Fail("MESSAGE_NOT_FOUND");
                }

                _queue.Remove(message);

                _logger?.LogInformation(
                    "Dequeued message {MessageId}: {DisplayName}",
                    messageId, message.DisplayName);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error dequeueing message {MessageId}", messageId);
                return Result.Fail(ex.Message);
            }
        });
    }

    /// <summary>
    /// Clear all queued messages.
    /// </summary>
    public async Task<Result> ClearQueueAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                var count = _queue.Count;
                _queue.Clear();

                _logger?.LogInformation("Cleared message queue ({Count} messages removed)", count);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error clearing message queue");
                return Result.Fail(ex.Message);
            }
        });
    }

    /// <summary>
    /// Get queue count.
    /// </summary>
    public async Task<Result<int>> GetQueueCountAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                var nonExpiredMessages = _queue.Where(m => !m.IsExpired).ToList();
                var expiredCount = _queue.Count - nonExpiredMessages.Count;

                if (expiredCount > 0)
                {
                    _logger?.LogDebug(
                        "Queue count: {ActiveCount} active, {ExpiredCount} expired",
                        nonExpiredMessages.Count, expiredCount);
                }

                return Result.Ok(nonExpiredMessages.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting queue count");
                return Result.Fail<int>(ex.Message);
            }
        });
    }

    /// <summary>
    /// Increment retry count for message.
    /// </summary>
    public async Task<Result> IncrementRetryCountAsync(string messageId)
    {
        return await Task.Run(() =>
        {
            try
            {
                Guard.NotEmpty(messageId, nameof(messageId));

                var messageIndex = _queue.FindIndex(m => m.MessageId == messageId);

                if (messageIndex == -1)
                {
                    _logger?.LogWarning("Message {MessageId} not found for retry count increment", messageId);
                    return Result.Fail("MESSAGE_NOT_FOUND");
                }

                var originalMessage = _queue[messageIndex];
                var newRetryCount = originalMessage.RetryCount + 1;

                if (newRetryCount > 5)
                {
                    _queue.RemoveAt(messageIndex);
                    _logger?.LogWarning(
                        "Removed message {MessageId} after exceeding max retries ({MaxRetries})",
                        messageId, 5);
                    return Result.Ok();
                }

                // Create a new record with updated retry count
                var updatedMessage = originalMessage with { RetryCount = newRetryCount };
                _queue[messageIndex] = updatedMessage;

                _logger?.LogDebug(
                    "Incremented retry count for {MessageId} to {RetryCount}",
                    messageId, newRetryCount);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error incrementing retry count for message {MessageId}", messageId);
                return Result.Fail(ex.Message);
            }
        });
    }
}
