namespace SmartWorkz.Mobile.Services.Implementations;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile.Models;
using SmartWorkz.Shared;

/// <summary>
/// Manages real-time channel subscriptions with thread-safe in-memory storage.
/// </summary>
public class RealtimeSubscriptionManager : IRealtimeSubscriptionManager
{
    private readonly ILogger<RealtimeSubscriptionManager>? _logger;
    private readonly ConcurrentDictionary<string, RealtimeSubscription> _subscriptions;

    public RealtimeSubscriptionManager(ILogger<RealtimeSubscriptionManager>? logger = null)
    {
        _logger = logger;
        _subscriptions = new ConcurrentDictionary<string, RealtimeSubscription>();
    }

    public async Task<Result<RealtimeSubscription>> SubscribeAsync(string channel)
    {
        Guard.NotEmpty(channel, nameof(channel));

        // Check if already subscribed
        if (_subscriptions.TryGetValue(channel, out var existingSubscription))
        {
            _logger?.LogWarning("Channel {Channel} already subscribed with SubscriptionId {SubscriptionId}",
                channel, existingSubscription.SubscriptionId);
            return Result.Ok(existingSubscription);
        }

        // Create new subscription
        var subscription = new RealtimeSubscription(
            SubscriptionId: Guid.NewGuid().ToString(),
            Channel: channel,
            SubscribedAt: DateTime.UtcNow,
            IsActive: true,
            MessageCount: 0,
            LastMessageAt: null);

        if (_subscriptions.TryAdd(channel, subscription))
        {
            _logger?.LogInformation("Subscribed to channel {Channel}", channel);
            return Result.Ok(subscription);
        }

        // Race condition: another thread added the subscription
        if (_subscriptions.TryGetValue(channel, out var raceSubscription))
        {
            _logger?.LogWarning("Race condition on channel {Channel}, returning existing subscription", channel);
            return Result.Ok(raceSubscription);
        }

        return Result.Fail<RealtimeSubscription>("SUBSCRIPTION_FAILED", "Failed to create subscription");
    }

    public async Task<Result> UnsubscribeAsync(string channel)
    {
        Guard.NotEmpty(channel, nameof(channel));

        if (_subscriptions.TryRemove(channel, out var removed))
        {
            _logger?.LogInformation("Unsubscribed from channel {Channel}", channel);
            return Result.Ok();
        }

        _logger?.LogWarning("Channel {Channel} is not subscribed", channel);
        return Result.Fail("CHANNEL_NOT_SUBSCRIBED", $"Channel {channel} is not subscribed");
    }

    public async Task<Result<RealtimeSubscription>> GetSubscriptionAsync(string channel)
    {
        Guard.NotEmpty(channel, nameof(channel));

        if (_subscriptions.TryGetValue(channel, out var subscription))
        {
            return Result.Ok(subscription);
        }

        _logger?.LogDebug("Subscription not found for channel {Channel}", channel);
        return Result.Fail<RealtimeSubscription>("CHANNEL_NOT_FOUND", $"Channel {channel} not found");
    }

    public async Task<Result<IReadOnlyList<RealtimeSubscription>>> GetActiveSubscriptionsAsync()
    {
        var subscriptions = _subscriptions.Values.ToList().AsReadOnly();
        return Result.Ok<IReadOnlyList<RealtimeSubscription>>(subscriptions);
    }

    public async Task<Result> TrackMessageAsync(string channel)
    {
        Guard.NotEmpty(channel, nameof(channel));

        if (!_subscriptions.TryGetValue(channel, out var subscription))
        {
            _logger?.LogWarning("Attempted to track message on non-subscribed channel {Channel}", channel);
            return Result.Fail("CHANNEL_NOT_SUBSCRIBED", $"Channel {channel} is not subscribed");
        }

        // Create updated subscription with incremented count and updated timestamp
        var updated = new RealtimeSubscription(
            SubscriptionId: subscription.SubscriptionId,
            Channel: subscription.Channel,
            SubscribedAt: subscription.SubscribedAt,
            IsActive: subscription.IsActive,
            MessageCount: subscription.MessageCount + 1,
            LastMessageAt: DateTime.UtcNow);

        if (_subscriptions.TryUpdate(channel, updated, subscription))
        {
            _logger?.LogDebug("Tracked message on channel {Channel}, count: {MessageCount}",
                channel, updated.MessageCount);
            return Result.Ok();
        }

        // Retry once in case of CAS failure
        if (_subscriptions.TryGetValue(channel, out var currentSubscription) &&
            currentSubscription.SubscriptionId == subscription.SubscriptionId)
        {
            var retry = new RealtimeSubscription(
                SubscriptionId: currentSubscription.SubscriptionId,
                Channel: currentSubscription.Channel,
                SubscribedAt: currentSubscription.SubscribedAt,
                IsActive: currentSubscription.IsActive,
                MessageCount: currentSubscription.MessageCount + 1,
                LastMessageAt: DateTime.UtcNow);

            if (_subscriptions.TryUpdate(channel, retry, currentSubscription))
            {
                _logger?.LogDebug("Tracked message on channel {Channel} (retry), count: {MessageCount}",
                    channel, retry.MessageCount);
                return Result.Ok();
            }
        }

        _logger?.LogError("Failed to track message on channel {Channel} after retry", channel);
        return Result.Fail("TRACK_MESSAGE_FAILED", "Failed to track message");
    }

    public async Task<Result<int>> GetSubscriptionCountAsync()
    {
        return Result.Ok(_subscriptions.Count);
    }
}
