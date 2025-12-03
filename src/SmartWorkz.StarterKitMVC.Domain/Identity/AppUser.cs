namespace SmartWorkz.StarterKitMVC.Domain.Identity;

/// <summary>
/// Represents an application user.
/// </summary>
/// <example>
/// <code>
/// var user = new AppUser
/// {
///     Id = Guid.NewGuid(),
///     UserName = "john.doe",
///     Email = "john@example.com",
///     DisplayName = "John Doe",
///     Locale = "en-US",
///     IsActive = true
/// };
/// </code>
/// </example>
public sealed class AppUser
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; }
    
    /// <summary>Unique username for login.</summary>
    public string UserName { get; init; } = string.Empty;
    
    /// <summary>Email address.</summary>
    public string Email { get; init; } = string.Empty;
    
    /// <summary>Display name shown in UI.</summary>
    public string? DisplayName { get; init; }
    
    /// <summary>URL to user's avatar image.</summary>
    public string? AvatarUrl { get; init; }
    
    /// <summary>User's preferred locale (e.g., "en-US").</summary>
    public string? Locale { get; init; }
    
    /// <summary>Whether the user account is active.</summary>
    public bool IsActive { get; init; } = true;
}
