namespace SmartWorkz.StarterKitMVC.Domain.LoV;

/// <summary>
/// Represents an item in the List of Values (LoV) system.
/// Used for dropdown options, lookup values, etc.
/// </summary>
/// <example>
/// <code>
/// var item = new LovItem
/// {
///     Id = Guid.NewGuid(),
///     CategoryKey = "countries",
///     Key = "US",
///     DisplayName = "United States",
///     Tags = new[] { "north-america", "english" },
///     LocalizedNames = new Dictionary&lt;string, string&gt;
///     {
///         ["es-ES"] = "Estados Unidos"
///     }
/// };
/// </code>
/// </example>
public sealed class LovItem
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; }
    
    /// <summary>Parent category key.</summary>
    public string CategoryKey { get; init; } = string.Empty;
    
    /// <summary>Optional subcategory key.</summary>
    public string? SubCategoryKey { get; init; }
    
    /// <summary>Unique key within the category (e.g., "US", "CA").</summary>
    public string Key { get; init; } = string.Empty;
    
    /// <summary>Default display name.</summary>
    public string DisplayName { get; init; } = string.Empty;
    
    /// <summary>Sort order for display.</summary>
    public int SortOrder { get; init; }
    
    /// <summary>Tags for filtering and grouping.</summary>
    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();
    
    /// <summary>Localized display names by culture code.</summary>
    public IReadOnlyDictionary<string, string> LocalizedNames { get; init; } = new Dictionary<string, string>();
}
