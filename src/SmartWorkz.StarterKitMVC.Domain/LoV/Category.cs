namespace SmartWorkz.StarterKitMVC.Domain.LoV;

/// <summary>
/// Represents a category in the List of Values (LoV) hierarchy.
/// Categories can be nested via ParentKey for hierarchical organization.
/// </summary>
/// <example>
/// <code>
/// var category = new Category
/// {
///     Id = Guid.NewGuid(),
///     Key = "countries",
///     DisplayName = "Countries",
///     SortOrder = 1,
///     LocalizedNames = new Dictionary&lt;string, string&gt;
///     {
///         ["en-US"] = "Countries",
///         ["es-ES"] = "Países"
///     }
/// };
/// </code>
/// </example>
public sealed class Category
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; }
    
    /// <summary>Unique key for lookups (e.g., "countries").</summary>
    public string Key { get; init; } = string.Empty;
    
    /// <summary>Default display name.</summary>
    public string DisplayName { get; init; } = string.Empty;
    
    /// <summary>Parent category key for hierarchical nesting.</summary>
    public string? ParentKey { get; init; }
    
    /// <summary>Sort order for display.</summary>
    public int SortOrder { get; init; }
    
    /// <summary>Tags for filtering and grouping.</summary>
    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();
    
    /// <summary>Localized display names by culture code.</summary>
    public IReadOnlyDictionary<string, string> LocalizedNames { get; init; } = new Dictionary<string, string>();
}
