namespace SmartWorkz.Mobile;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Shared;

/// <summary>
/// Manages real-time channel subscriptions.
/// Tracks which channels the client is subscribed to and monitors message activity.
/// </summary>
public interface IRealtimeSubscriptionManager
{
    /// <summary>
    /// Subscribe to a channel.
    /// If already subscribed, returns the existing subscription.
    /// </summary>
    /// <param name="channel">The channel name (e.g., "orders", "notifications").</param>
    /// <returns>Result containing the created or existing subscription.</returns>
    Task<Result<RealtimeSubscription>> SubscribeAsync(string channel);

    /// <summary>
    /// Unsubscribe from a channel.
    /// </summary>
    /// <param name="channel">The channel name to unsubscribe from.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> UnsubscribeAsync(string channel);

    /// <summary>
    /// Get active subscription for a specific channel.
    /// </summary>
    /// <param name="channel">The channel name.</param>
    /// <returns>Result containing the subscription if found.</returns>
    Task<Result<RealtimeSubscription>> GetSubscriptionAsync(string channel);

    /// <summary>
    /// Get all active subscriptions.
    /// </summary>
    /// <returns>Result containing a read-only list of all active subscriptions.</returns>
    Task<Result<IReadOnlyList<RealtimeSubscription>>> GetActiveSubscriptionsAsync();

    /// <summary>
    /// Track a message received on a subscription.
    /// Increments the message count and updates the last message timestamp.
    /// </summary>
    /// <param name="channel">The channel where the message was received.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> TrackMessageAsync(string channel);

    /// <summary>
    /// Get the total count of active subscriptions.
    /// </summary>
    /// <returns>Result containing the subscription count.</returns>
    Task<Result<int>> GetSubscriptionCountAsync();
}
