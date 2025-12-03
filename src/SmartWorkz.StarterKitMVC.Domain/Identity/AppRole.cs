namespace SmartWorkz.StarterKitMVC.Domain.Identity;

/// <summary>
/// Represents an application role for authorization.
/// </summary>
/// <example>
/// <code>
/// var role = new AppRole
/// {
///     Id = Guid.NewGuid(),
///     Name = "Admin",
///     Description = "Full system access"
/// };
/// </code>
/// </example>
public sealed class AppRole
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; }
    
    /// <summary>Role name (e.g., "Admin", "User").</summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>Role description.</summary>
    public string? Description { get; init; }
}
