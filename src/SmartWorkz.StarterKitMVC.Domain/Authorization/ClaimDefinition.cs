namespace SmartWorkz.StarterKitMVC.Domain.Authorization;

/// <summary>
/// Represents a claim type definition (e.g., "department", "permission", "feature")
/// </summary>
public class ClaimType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>Unique key for the claim type (e.g., "department", "permission")</summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>Display name for the claim type</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Description of what this claim type represents</summary>
    public string? Description { get; set; }
    
    /// <summary>Icon for UI display (Bootstrap Icons class)</summary>
    public string? Icon { get; set; }
    
    /// <summary>Category for grouping claim types</summary>
    public string Category { get; set; } = "General";
    
    /// <summary>Whether this claim type allows multiple values per user/role</summary>
    public bool AllowMultiple { get; set; } = true;
    
    /// <summary>Whether this is a system claim type that cannot be deleted</summary>
    public bool IsSystem { get; set; }
    
    /// <summary>Whether this claim type is active</summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>Sort order for display</summary>
    public int SortOrder { get; set; }
    
    /// <summary>Predefined values for this claim type (if any)</summary>
    public List<ClaimValue> PredefinedValues { get; set; } = [];
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Represents a predefined value for a claim type
/// </summary>
public class ClaimValue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>The actual value</summary>
    public string Value { get; set; } = string.Empty;
    
    /// <summary>Display label for the value</summary>
    public string Label { get; set; } = string.Empty;
    
    /// <summary>Description of this value</summary>
    public string? Description { get; set; }
    
    /// <summary>Sort order for display</summary>
    public int SortOrder { get; set; }
    
    /// <summary>Whether this value is active</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Represents a claim assigned to a role
/// </summary>
public class RoleClaim
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>The role this claim is assigned to</summary>
    public string RoleId { get; set; } = string.Empty;
    
    /// <summary>The claim type key</summary>
    public string ClaimType { get; set; } = string.Empty;
    
    /// <summary>The claim value</summary>
    public string ClaimValue { get; set; } = string.Empty;
    
    /// <summary>Optional condition for when this claim applies</summary>
    public string? Condition { get; set; }
    
    /// <summary>Whether this claim is granted (true) or denied (false)</summary>
    public bool IsGranted { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a claim assigned to a user (overrides role claims)
/// </summary>
public class UserClaim
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>The user this claim is assigned to</summary>
    public Guid UserId { get; set; }
    
    /// <summary>The claim type key</summary>
    public string ClaimType { get; set; } = string.Empty;
    
    /// <summary>The claim value</summary>
    public string ClaimValue { get; set; } = string.Empty;
    
    /// <summary>Whether this claim is granted (true) or denied (false)</summary>
    public bool IsGranted { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
