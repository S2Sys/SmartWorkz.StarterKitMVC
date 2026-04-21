namespace SmartWorkz.Shared;

using System.Data;
using SmartWorkz.Core;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection and schema setup for audit trail functionality.
/// </summary>
public static class AuditStartupExtensions
{
    /// <summary>
    /// Register IAuditTrail with SQL Server implementation.
    /// </summary>
    public static IServiceCollection AddAuditTrail(this IServiceCollection services)
    {
        Guard.NotNull(services, nameof(services));
        services.AddScoped<IAuditTrail, SqlAuditTrail>();
        return services;
    }

    /// <summary>
    /// Create the AuditTrail table and indexes if they don't exist.
    /// Call this during application startup or migration.
    /// </summary>
    public static void CreateAuditTrailSchema(this IDbConnection connection)
    {
        Guard.NotNull(connection, nameof(connection));

        const string schema = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditTrail')
            BEGIN
                CREATE TABLE [AuditTrail] (
                    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                    [EntityType] NVARCHAR(MAX) NOT NULL,
                    [EntityId] NVARCHAR(MAX) NOT NULL,
                    [Action] NVARCHAR(MAX) NOT NULL,
                    [UserId] NVARCHAR(MAX),
                    [IpAddress] NVARCHAR(50),
                    [Changes] NVARCHAR(MAX),
                    [CorrelationId] NVARCHAR(MAX),
                    [TraceId] NVARCHAR(MAX),
                    [Timestamp] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
                    [ReasonCode] NVARCHAR(MAX)
                );

                CREATE INDEX [IX_AuditTrail_EntityType_EntityId] ON [AuditTrail] (EntityType, EntityId);
                CREATE INDEX [IX_AuditTrail_Action] ON [AuditTrail] (Action);
                CREATE INDEX [IX_AuditTrail_UserId] ON [AuditTrail] (UserId);
                CREATE INDEX [IX_AuditTrail_Timestamp] ON [AuditTrail] (Timestamp DESC);
            END
        ";

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = schema;
            cmd.ExecuteNonQuery();
        }
    }
}
