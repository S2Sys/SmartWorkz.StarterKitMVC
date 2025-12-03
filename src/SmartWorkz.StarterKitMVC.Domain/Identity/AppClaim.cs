namespace SmartWorkz.StarterKitMVC.Domain.Identity;

/// <summary>
/// Represents a claim (key-value pair) associated with a user.
/// </summary>
/// <example>
/// <code>
/// var claim = new AppClaim
/// {
///     Type = "department",
///     Value = "Engineering"
/// };
/// </code>
/// </example>
public sealed class AppClaim
{
    /// <summary>Claim type (e.g., "role", "department").</summary>
    public string Type { get; init; } = string.Empty;
    
    /// <summary>Claim value.</summary>
    public string Value { get; init; } = string.Empty;
}
