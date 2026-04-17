using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;

namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Implementation of audit logging service.
/// Tracks all significant changes to entities for compliance and audit trails.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _repository;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IAuditLogRepository repository,
        ILogger<AuditService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<bool> LogChangeAsync(AuditLogDto log)
    {
        if (log == null)
            throw new ArgumentNullException(nameof(log));
        if (string.IsNullOrWhiteSpace(log.EntityType))
            throw new ArgumentException("Entity type is required", nameof(log.EntityType));
        if (string.IsNullOrWhiteSpace(log.EntityId))
            throw new ArgumentException("Entity ID is required", nameof(log.EntityId));
        if (string.IsNullOrWhiteSpace(log.Action))
            throw new ArgumentException("Action is required", nameof(log.Action));

        try
        {
            var repositoryLog = new Repositories.AuditLogDto
            {
                AuditLogId = Guid.NewGuid(),
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                Action = log.Action,
                OldValues = log.OldValue,
                NewValues = log.NewValue,
                UserId = log.UserId,
                TenantId = log.TenantId,
                CreatedAt = DateTime.UtcNow,
                IPAddress = log.IPAddress,
                UserAgent = log.UserAgent
            };

            await _repository.UpsertAsync(repositoryLog);

            _logger.LogDebug(
                "Audit log created: {EntityType} {EntityId} - {Action} by {UserId}",
                log.EntityType, log.EntityId, log.Action, log.UserId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error logging audit entry: {EntityType} {EntityId}",
                log.EntityType, log.EntityId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityType, string entityId, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type cannot be empty", nameof(entityType));
        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("Entity ID cannot be empty", nameof(entityId));
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        try
        {
            var logs = await _repository.GetByEntityAsync(entityType, entityId, tenantId);

            _logger.LogDebug(
                "Retrieved {Count} audit logs for entity {EntityType} {EntityId}",
                logs.Count(), entityType, entityId);

            return logs.OrderByDescending(l => l.CreatedAt).Select(l => new Services.AuditLogDto
            {
                AuditLogId = l.AuditLogId,
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                Action = l.Action,
                OldValue = l.OldValues,
                NewValue = l.NewValues,
                UserId = l.UserId,
                TenantId = l.TenantId,
                CreatedAt = l.CreatedAt,
                IPAddress = l.IPAddress,
                UserAgent = l.UserAgent,
                Details = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving audit logs for entity {EntityType} {EntityId}",
                entityType, entityId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditLogDto>> GetByUserAsync(string userId, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        try
        {
            var logs = await _repository.GetByUserAsync(userId, tenantId);

            _logger.LogDebug(
                "Retrieved {Count} audit logs for user {UserId}",
                logs.Count(), userId);

            return logs.OrderByDescending(l => l.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving audit logs for user {UserId}",
                userId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
        if (endDate < startDate)
            throw new ArgumentException("End date must be after start date", nameof(endDate));

        try
        {
            var logs = await _repository.GetByDateRangeAsync(startDate, endDate, tenantId);

            _logger.LogDebug(
                "Retrieved {Count} audit logs between {StartDate} and {EndDate}",
                logs.Count(), startDate, endDate);

            return logs.OrderByDescending(l => l.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving audit logs for date range {StartDate} - {EndDate}",
                startDate, endDate);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditLogDto>> GetByActionAsync(string action, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be empty", nameof(action));
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        try
        {
            var logs = await _repository.GetByActionAsync(action, tenantId);

            _logger.LogDebug(
                "Retrieved {Count} audit logs for action {Action}",
                logs.Count(), action);

            return logs.OrderByDescending(l => l.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving audit logs for action {Action}",
                action);
            throw;
        }
    }
}
