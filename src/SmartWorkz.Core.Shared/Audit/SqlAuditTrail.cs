namespace SmartWorkz.Shared;

using System.Data;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Logging;

/// <summary>
/// SQL Server implementation of IAuditTrail for immutable audit log persistence.
/// Appends audit entries to a single table with indexes for efficient querying.
/// </summary>
public class SqlAuditTrail : IAuditTrail
{
    private readonly IDbConnection _connection;
    private readonly ILogger<SqlAuditTrail> _logger;

    public SqlAuditTrail(IDbConnection connection, ILogger<SqlAuditTrail> logger)
    {
        _connection = Guard.NotNull(connection, nameof(connection));
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    /// <inheritdoc />
    public async Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(entry, nameof(entry));

        try
        {
            const string sql = @"
                INSERT INTO [AuditTrail]
                (Id, EntityType, EntityId, Action, UserId, IpAddress, Changes, CorrelationId, TraceId, Timestamp, ReasonCode)
                VALUES (@Id, @EntityType, @EntityId, @Action, @UserId, @IpAddress, @Changes, @CorrelationId, @TraceId, @Timestamp, @ReasonCode)
            ";

            var changes = entry.Changes != null ? JsonSerializer.Serialize(entry.Changes) : null;

            await _connection.ExecuteAsync(sql, new
            {
                Id = entry.Id == Guid.Empty ? Guid.NewGuid() : entry.Id,
                EntityType = entry.EntityType,
                EntityId = entry.EntityId,
                Action = entry.Action,
                UserId = entry.UserId,
                IpAddress = entry.IpAddress,
                Changes = changes,
                CorrelationId = entry.CorrelationId,
                TraceId = entry.TraceId,
                Timestamp = entry.Timestamp == default ? DateTimeOffset.UtcNow : entry.Timestamp,
                ReasonCode = entry.ReasonCode
            });

            _logger.LogInformation(
                "Audit recorded: {Action} on {EntityType} {EntityId} by {UserId}",
                entry.Action, entry.EntityType, entry.EntityId, entry.UserId ?? "system");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record audit entry for {Action}", entry.Action);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<AuditEntry>> GetEntriesAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(entityType, nameof(entityType));
        Guard.NotEmpty(entityId, nameof(entityId));

        const string sql = @"
            SELECT Id, EntityType, EntityId, Action, UserId, IpAddress, Changes, CorrelationId, TraceId, Timestamp, ReasonCode
            FROM [AuditTrail]
            WHERE EntityType = @EntityType AND EntityId = @EntityId
            ORDER BY Timestamp DESC
        ";

        var entries = await _connection.QueryAsync<AuditEntry>(sql, new { EntityType = entityType, EntityId = entityId });
        return entries.ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<AuditEntry>> GetEntriesByActionAsync(string action, DateTimeOffset? since = null, CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(action, nameof(action));

        const string sql = @"
            SELECT Id, EntityType, EntityId, Action, UserId, IpAddress, Changes, CorrelationId, TraceId, Timestamp, ReasonCode
            FROM [AuditTrail]
            WHERE Action = @Action AND (@Since IS NULL OR Timestamp >= @Since)
            ORDER BY Timestamp DESC
        ";

        var entries = await _connection.QueryAsync<AuditEntry>(sql, new { Action = action, Since = since });
        return entries.ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<AuditEntry>> GetEntriesByUserAsync(string userId, DateTimeOffset? since = null, CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(userId, nameof(userId));

        const string sql = @"
            SELECT Id, EntityType, EntityId, Action, UserId, IpAddress, Changes, CorrelationId, TraceId, Timestamp, ReasonCode
            FROM [AuditTrail]
            WHERE UserId = @UserId AND (@Since IS NULL OR Timestamp >= @Since)
            ORDER BY Timestamp DESC
        ";

        var entries = await _connection.QueryAsync<AuditEntry>(sql, new { UserId = userId, Since = since });
        return entries.ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<AuditEntry>> SearchAsync(
        string? entityType = null,
        string? action = null,
        string? userId = null,
        DateTimeOffset? since = null,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, EntityType, EntityId, Action, UserId, IpAddress, Changes, CorrelationId, TraceId, Timestamp, ReasonCode
            FROM [AuditTrail]
            WHERE (@EntityType IS NULL OR EntityType = @EntityType)
              AND (@Action IS NULL OR Action = @Action)
              AND (@UserId IS NULL OR UserId = @UserId)
              AND (@Since IS NULL OR Timestamp >= @Since)
            ORDER BY Timestamp DESC
        ";

        var entries = await _connection.QueryAsync<AuditEntry>(sql, new { EntityType = entityType, Action = action, UserId = userId, Since = since });
        return entries.ToList();
    }
}
