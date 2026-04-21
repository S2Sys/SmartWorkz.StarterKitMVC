namespace SmartWorkz.Core.Shared.Webhooks;

using System.Data;
using SmartWorkz.Core.Shared.Guards;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extensions for webhook services.
/// Call AddWebhooks() during application startup to register required services.
/// </summary>
public static class WebhookStartupExtensions
{
    /// <summary>
    /// Register webhook services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWebhooks(this IServiceCollection services)
    {
        Guard.NotNull(services, nameof(services));
        services.AddScoped<IWebhookRegistry, SqlWebhookRegistry>();
        services.AddHttpClient<WebhookDeliveryService>()
            .ConfigureHttpClient(client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "SmartWorkz-Webhook-Delivery/1.0");
            });

        return services;
    }

    /// <summary>
    /// Create the WebhookSubscriptions table schema if it doesn't exist.
    /// Call this during database initialization.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    public static void CreateWebhookSchema(this IDbConnection connection)
    {
        Guard.NotNull(connection, nameof(connection));

        const string schema = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WebhookSubscriptions')
            BEGIN
                CREATE TABLE [WebhookSubscriptions] (
                    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                    [Url] NVARCHAR(MAX) NOT NULL,
                    [Events] NVARCHAR(MAX) NOT NULL,
                    [Secret] NVARCHAR(MAX),
                    [IsActive] BIT NOT NULL DEFAULT 1,
                    [MaxRetries] INT DEFAULT 3,
                    [TimeoutSeconds] INT DEFAULT 30,
                    [CreatedAt] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
                    [LastTriggeredAt] DATETIMEOFFSET,
                    [FailureCount] INT,
                    [FailureReason] NVARCHAR(MAX)
                );

                CREATE INDEX [IX_WebhookSubscriptions_IsActive] ON [WebhookSubscriptions] (IsActive);
                CREATE INDEX [IX_WebhookSubscriptions_CreatedAt] ON [WebhookSubscriptions] (CreatedAt DESC);
            END
        ";

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = schema;
            cmd.ExecuteNonQuery();
        }
    }
}
