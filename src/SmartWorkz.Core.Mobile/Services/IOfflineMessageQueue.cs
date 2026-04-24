namespace SmartWorkz.Mobile.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Shared;

/// <summary>
/// Queue for messages when connection is offline.
/// </summary>
public interface IOfflineMessageQueue
{
    /// <summary>
    /// Add message to queue for later sending.
    /// </summary>
    /// <param name="channel">The channel name to send to.</param>
    /// <param name="method">The method name to invoke.</param>
    /// <param name="args">The arguments to pass to the method.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> EnqueueAsync(string channel, string method, object?[] args);

    /// <summary>
    /// Get all queued messages.
    /// </summary>
    /// <returns>Result containing a read-only list of queued messages.</returns>
    Task<Result<IReadOnlyList<QueuedMessage>>> GetQueuedMessagesAsync();

    /// <summary>
    /// Remove message from queue (after successful send).
    /// </summary>
    /// <param name="messageId">The ID of the message to remove.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> DequeueAsync(string messageId);

    /// <summary>
    /// Clear all queued messages.
    /// </summary>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> ClearQueueAsync();

    /// <summary>
    /// Get queue count.
    /// </summary>
    /// <returns>Result containing the count of non-expired messages.</returns>
    Task<Result<int>> GetQueueCountAsync();

    /// <summary>
    /// Increment retry count for message.
    /// </summary>
    /// <param name="messageId">The ID of the message to retry.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> IncrementRetryCountAsync(string messageId);
}
