namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

/// <summary>Consolidated DTO for AuditLog entity - unified across Repositories</summary>
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

/// <summary>Consolidated DTO for BlogPost entity</summary>
public class BlogPostDto
{
    public Guid BlogPostId { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Content { get; set; }
    public string Summary { get; set; }
    public string AuthorId { get; set; }
    public string Tags { get; set; } // Comma-separated or JSON array
    public string TenantId { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>Consolidated DTO for Configuration entity</summary>
public class ConfigurationDto
{
    public Guid ConfigurationId { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public string Description { get; set; }
    public string ConfigType { get; set; } // String, Integer, Boolean, DateTime, Json
    public string TenantId { get; set; }
    public bool IsEncrypted { get; set; }
    public bool IsEditable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>Consolidated DTO for Country entity</summary>
public class CountryDto
{
    public int CountryId { get; set; }
    public string Code { get; set; } // ISO 3166-1 alpha-2
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string FlagEmoji { get; set; }
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>Consolidated DTO for CustomPage entity</summary>
public class CustomPageDto
{
    public Guid CustomPageId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string MetaDescription { get; set; }
    public string MetaKeywords { get; set; }
    public string TenantId { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}


/// <summary>Consolidated DTO for Permission entity</summary>
public class PermissionDto
{
    public Guid PermissionId { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>Consolidated DTO for Role entity</summary>
public class RoleDto
{
    public Guid RoleId { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public List<string> Permissions { get; set; } = new();
}
