namespace SmartWorkz.StarterKitMVC.Domain.Identity;

/// <summary>
/// Represents a permission that can be assigned to roles or users.
/// </summary>
/// <example>
/// <code>
/// var permission = new AppPermission
/// {
///     Key = "users.create",
///     Description = "Can create new users"
/// };
/// </code>
/// </example>
public sealed class AppPermission
{
    /// <summary>Unique permission key (e.g., "users.create").</summary>
    public string Key { get; init; } = string.Empty;
    
    /// <summary>Human-readable description.</summary>
    public string? Description { get; init; }
}
