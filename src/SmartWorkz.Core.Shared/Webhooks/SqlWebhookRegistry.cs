namespace SmartWorkz.Core.Shared.Webhooks;

using System.Data;
using System.Text.Json;
using Dapper;
using SmartWorkz.Core.Shared.Guards;
using Microsoft.Extensions.Logging;

/// <summary>
/// SQL Server implementation of IWebhookRegistry.
/// Persists webhook subscriptions to the database with support for querying and status updates.
/// </summary>
public class SqlWebhookRegistry : IWebhookRegistry
{
    private readonly IDbConnection _connection;
    private readonly ILogger<SqlWebhookRegistry> _logger;

    public SqlWebhookRegistry(IDbConnection connection, ILogger<SqlWebhookRegistry> logger)
    {
        _connection = Guard.NotNull(connection, nameof(connection));
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public async Task<Guid> RegisterAsync(string url, string[] events, string? secret = null, CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(url, nameof(url));
        Guard.NotNull(events, nameof(events));

        var id = Guid.NewGuid();
        var eventsJson = JsonSerializer.Serialize(events);

        const string sql = @"
            INSERT INTO [WebhookSubscriptions] (Id, Url, Events, Secret, IsActive, MaxRetries, TimeoutSeconds, CreatedAt)
            VALUES (@Id, @Url, @Events, @Secret, @IsActive, @MaxRetries, @TimeoutSeconds, @CreatedAt)
        ";

        await _connection.ExecuteAsync(sql, new
        {
            Id = id,
            Url = url,
            Events = eventsJson,
            Secret = secret,
            IsActive = true,
            MaxRetries = 3,
            TimeoutSeconds = 30,
            CreatedAt = DateTimeOffset.UtcNow
        });

        _logger.LogInformation("Webhook registered: {Url} for events {Events}", url, string.Join(", ", events));
        return id;
    }

    public async Task UnregisterAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM [WebhookSubscriptions] WHERE Id = @Id";
        await _connection.ExecuteAsync(sql, new { Id = subscriptionId });
        _logger.LogInformation("Webhook unregistered: {SubscriptionId}", subscriptionId);
    }

    public async Task<IReadOnlyCollection<WebhookSubscription>> GetSubscriptionsForEventAsync(string eventName, CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(eventName, nameof(eventName));

        const string sql = @"
            SELECT Id, Url, Events, Secret, IsActive, MaxRetries, TimeoutSeconds, CreatedAt, LastTriggeredAt, FailureCount, FailureReason
            FROM [WebhookSubscriptions]
            WHERE IsActive = 1 AND Events LIKE @EventPattern
            ORDER BY CreatedAt
        ";

        var subscriptions = await _connection.QueryAsync<WebhookSubscription>(
            sql,
            new { EventPattern = $"%{eventName}%" }
        );

        return subscriptions.ToList();
    }

    public async Task<IReadOnlyCollection<WebhookSubscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT Id, Url, Events, Secret, IsActive, MaxRetries, TimeoutSeconds, CreatedAt, LastTriggeredAt, FailureCount, FailureReason FROM [WebhookSubscriptions] WHERE IsActive = 1 ORDER BY CreatedAt";
        var subscriptions = await _connection.QueryAsync<WebhookSubscription>(sql);
        return subscriptions.ToList();
    }

    public async Task UpdateSubscriptionStatusAsync(Guid subscriptionId, bool isActive, int? failureCount = null, string? failureReason = null, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE [WebhookSubscriptions]
            SET IsActive = @IsActive, FailureCount = @FailureCount, FailureReason = @FailureReason, LastTriggeredAt = @LastTriggeredAt
            WHERE Id = @Id
        ";

        await _connection.ExecuteAsync(sql, new
        {
            Id = subscriptionId,
            IsActive = isActive,
            FailureCount = failureCount,
            FailureReason = failureReason,
            LastTriggeredAt = DateTimeOffset.UtcNow
        });

        _logger.LogInformation("Webhook status updated: {SubscriptionId} active={IsActive}", subscriptionId, isActive);
    }
}
