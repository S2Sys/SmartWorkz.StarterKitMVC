namespace SmartWorkz.StarterKitMVC.Domain.Settings;

/// <summary>
/// Represents a category for grouping settings.
/// </summary>
/// <example>
/// <code>
/// var category = new SettingCategory
/// {
///     Id = Guid.NewGuid(),
///     Key = "appearance",
///     DisplayName = "Appearance",
///     SortOrder = 1
/// };
/// </code>
/// </example>
public sealed class SettingCategory
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; }
    
    /// <summary>Unique key for lookups.</summary>
    public string Key { get; init; } = string.Empty;
    
    /// <summary>Display name shown in UI.</summary>
    public string DisplayName { get; init; } = string.Empty;
    
    /// <summary>Parent category key for nesting.</summary>
    public string? ParentKey { get; init; }
    
    /// <summary>Sort order for display.</summary>
    public int SortOrder { get; init; }
}
