namespace SmartWorkz.StarterKitMVC.Domain.LoV;

/// <summary>
/// Represents a subcategory within a LoV category.
/// </summary>
/// <example>
/// <code>
/// var subCategory = new SubCategory
/// {
///     Id = Guid.NewGuid(),
///     CategoryKey = "locations",
///     Key = "north-america",
///     DisplayName = "North America",
///     SortOrder = 1
/// };
/// </code>
/// </example>
public sealed class SubCategory
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; }
    
    /// <summary>Parent category key.</summary>
    public string CategoryKey { get; init; } = string.Empty;
    
    /// <summary>Unique key within the category.</summary>
    public string Key { get; init; } = string.Empty;
    
    /// <summary>Default display name.</summary>
    public string DisplayName { get; init; } = string.Empty;
    
    /// <summary>Sort order for display.</summary>
    public int SortOrder { get; init; }
    
    /// <summary>Tags for filtering.</summary>
    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();
    
    /// <summary>Localized display names by culture code.</summary>
    public IReadOnlyDictionary<string, string> LocalizedNames { get; init; } = new Dictionary<string, string>();
}
