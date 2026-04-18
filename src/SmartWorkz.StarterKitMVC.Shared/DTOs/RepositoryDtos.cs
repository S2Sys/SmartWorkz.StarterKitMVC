namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

/// <summary>Consolidated DTO for AuditLog entity</summary>
public record AuditLogDto
{
    public Guid AuditLogId { get; set; }
    public string? UserId { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Action { get; set; }
    public string? OldValues { get; set; }
    public string? OldValue { get; set; }
    public string? NewValues { get; set; }
    public string? NewValue { get; set; }
    public string? TenantId { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Details { get; set; }
}

/// <summary>Consolidated DTO for BlogPost entity</summary>
public record BlogPostDto
{
    public Guid BlogPostId { get; set; }
    public string? PostId { get; set; }
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Content { get; set; }
    public string? Summary { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? AuthorId { get; set; }
    public string? Tags { get; set; }
    public string? TenantId { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
}

/// <summary>Consolidated DTO for Configuration entity</summary>
public record ConfigurationDto
{
    public Guid ConfigurationId { get; set; }
    public string? Key { get; set; }
    public string? Value { get; set; }
    public string? Description { get; set; }
    public string? ConfigType { get; set; }
    public string? TenantId { get; set; }
    public bool IsEncrypted { get; set; }
    public bool IsEditable { get; set; } = true;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>Consolidated DTO for Country entity</summary>
public record CountryDto
{
    public int CountryId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? FlagEmoji { get; set; }
    public string? TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>Consolidated DTO for CustomPage entity</summary>
public record CustomPageDto
{
    public Guid CustomPageId { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? TenantId { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>Consolidated DTO for Permission entity</summary>
public record PermissionDto
{
    public Guid PermissionId { get; set; }
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>Consolidated DTO for Role entity</summary>
public record RoleDto
{
    public Guid RoleId { get; set; }
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? TenantId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public List<string> Permissions { get; set; } = new();
}

/// <summary>DTO for Password Reset Token</summary>
public record PasswordResetToken
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public string? TenantId { get; set; }
}

/// <summary>DTO for Refresh Token</summary>
public record RefreshToken
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? TenantId { get; set; }
    public DateTime? CreatedAt { get; set; }
}

/// <summary>DTO for User</summary>
public record UserDto
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LockoutEndAt { get; set; }
    public int AccessFailedCount { get; set; }
    public bool LockoutEnabled { get; set; } = true;
    public string? TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>DTO for Email Queue</summary>
public record EmailQueueDto
{
    public Guid Id { get; set; }
    public string? ToEmail { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? BodyHtml { get; set; }
    public string? Status { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public string? TenantId { get; set; }
    public DateTime? CreatedAt { get; set; }
}
