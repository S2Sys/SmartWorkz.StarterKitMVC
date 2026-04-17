namespace SmartWorkz.StarterKitMVC.Application.Repositories;

/// <summary>
/// Repository interface for audit logs (Auth.AuditTrail table)
/// </summary>
public interface IAuditLogRepository : IDapperRepository<AuditLogDto>
{
    /// <summary>Get audit logs for a specific entity</summary>
    Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityType, object entityId, string tenantId);

    /// <summary>Get audit logs for a specific user</summary>
    Task<IEnumerable<AuditLogDto>> GetByUserAsync(string userId, string tenantId);

    /// <summary>Get audit logs within a date range</summary>
    Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime from, DateTime to, string tenantId);

    /// <summary>Get paged audit logs</summary>
    Task<(IEnumerable<AuditLogDto> Items, int Total)> GetPagedAsync(
        string tenantId, string? entityType = null, int pageNumber = 1, int pageSize = 20);

    /// <summary>Log an entity change</summary>
    Task LogChangeAsync(string entityType, object entityId, string action, string changes, string userId, string tenantId);

    /// <summary>Delete old audit logs (cleanup)</summary>
    Task DeleteOlderThanAsync(DateTime beforeDate, string tenantId);
}

/// <summary>DTO for AuditLog entity</summary>
public class AuditLogDto
{
    public Guid AuditLogId { get; set; }
    public string UserId { get; set; }
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string Action { get; set; } // Created, Updated, Deleted
    public string OldValues { get; set; } // JSON
    public string NewValues { get; set; } // JSON
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string IPAddress { get; set; }
    public string UserAgent { get; set; }
}
