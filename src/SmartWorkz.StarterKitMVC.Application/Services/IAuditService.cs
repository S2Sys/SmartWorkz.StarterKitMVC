using SmartWorkz.StarterKitMVC.Shared.DTOs;
using SmartWorkz.StarterKitMVC.Application.Repositories;
namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Service for compliance auditing and change tracking.
/// Logs all significant data changes for audit trails and compliance reporting.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs a data change for audit purposes.
    /// </summary>
    Task<bool> LogChangeAsync(AuditLogDto log);

    /// <summary>
    /// Gets audit logs for a specific entity.
    /// </summary>
    Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityType, string entityId, string tenantId);

    /// <summary>
    /// Gets audit logs for a specific user.
    /// </summary>
    Task<IEnumerable<AuditLogDto>> GetByUserAsync(string userId, string tenantId);

    /// <summary>
    /// Gets audit logs within a specific date range.
    /// </summary>
    Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, string tenantId);

    /// <summary>
    /// Gets audit logs for a specific action type.
    /// </summary>
    Task<IEnumerable<AuditLogDto>> GetByActionAsync(string action, string tenantId);
}
