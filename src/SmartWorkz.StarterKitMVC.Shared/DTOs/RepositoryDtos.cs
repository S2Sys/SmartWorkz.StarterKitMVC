namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

/// <summary>Consolidated DTO for AuditLog entity - unified across Services and Repositories</summary>
public class AuditLogDto
{
    public Guid AuditLogId { get; set; }
    public string UserId { get; set; }
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string Action { get; set; }
    public string OldValues { get; set; }
    public string NewValues { get; set; }
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string IPAddress { get; set; }
    public string UserAgent { get; set; }
}

/// <summary>Consolidated DTO for BlogPost entity</summary>
public class BlogPostDto
{
    public Guid PostId { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Content { get; set; }
    public string Summary { get; set; }
    public string Author { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsPublished { get; set; }
    public string TenantId { get; set; }
}

/// <summary>Consolidated DTO for Notification entity</summary>
public class NotificationDto
{
    public Guid NotificationId { get; set; }
    public string UserId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string TenantId { get; set; }
    public string ActionUrl { get; set; }
    public int Priority { get; set; }
    public string Data { get; set; }
}

/// <summary>Consolidated DTO for Role entity</summary>
public class RoleDto
{
    public string RoleId { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<string> Permissions { get; set; } = new();
}

/// <summary>Consolidated DTO for Permission entity</summary>
public class PermissionDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Consolidated DTO for Lookup entity</summary>
public class LookupDto
{
    public Guid LookupId { get; set; }
    public string CategoryKey { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public string DisplayName { get; set; }
    public int SortOrder { get; set; } = 0;
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>Consolidated DTO for Configuration entity</summary>
public class ConfigurationDto
{
    public string Key { get; set; }
    public string Value { get; set; }
    public string Description { get; set; }
    public string TenantId { get; set; }
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>Consolidated DTO for Tenant entity</summary>
public class TenantDto
{
    public string TenantId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
