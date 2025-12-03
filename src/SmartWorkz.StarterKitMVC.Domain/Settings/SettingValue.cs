namespace SmartWorkz.StarterKitMVC.Domain.Settings;

/// <summary>
/// Defines the scope level for a setting value.
/// Settings cascade: System → Tenant → User.
/// </summary>
public enum SettingScope
{
    /// <summary>System-wide default.</summary>
    System,
    /// <summary>Tenant-specific override.</summary>
    Tenant,
    /// <summary>User-specific override.</summary>
    User
}

/// <summary>
/// Represents a setting value at a specific scope.
/// </summary>
/// <example>
/// <code>
/// // System-level setting
/// var systemSetting = new SettingValue
/// {
///     Key = "app.theme",
///     Scope = SettingScope.System,
///     Value = "light"
/// };
/// 
/// // User-level override
/// var userSetting = new SettingValue
/// {
///     Key = "app.theme",
///     Scope = SettingScope.User,
///     UserId = "user-123",
///     Value = "dark"
/// };
/// </code>
/// </example>
public sealed class SettingValue
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; }
    
    /// <summary>Setting key.</summary>
    public string Key { get; init; } = string.Empty;
    
    /// <summary>Scope level (System, Tenant, User).</summary>
    public SettingScope Scope { get; init; }
    
    /// <summary>Tenant ID (required for Tenant/User scope).</summary>
    public string? TenantId { get; init; }
    
    /// <summary>User ID (required for User scope).</summary>
    public string? UserId { get; init; }
    
    /// <summary>The setting value.</summary>
    public string? Value { get; init; }
}
