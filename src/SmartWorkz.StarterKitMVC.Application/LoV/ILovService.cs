using SmartWorkz.StarterKitMVC.Domain.LoV;

namespace SmartWorkz.StarterKitMVC.Application.LoV;

/// <summary>
/// Service for managing List of Values (LoV) data.
/// Provides hierarchical category/subcategory/item access with localization and tenant support.
/// </summary>
/// <example>
/// <code>
/// // Inject ILovService via DI
/// public class DropdownController : Controller
/// {
///     private readonly ILovService _lov;
///     
///     public DropdownController(ILovService lov) => _lov = lov;
///     
///     public async Task&lt;IActionResult&gt; GetCountries()
///     {
///         var items = await _lov.GetItemsAsync("countries", locale: "en-US");
///         return Ok(items.Select(i => new { i.Key, i.DisplayName }));
///     }
/// }
/// </code>
/// </example>
public interface ILovService
{
    /// <summary>
    /// Gets the hierarchical category tree.
    /// </summary>
    /// <param name="tenantId">Optional tenant ID for tenant-specific overrides.</param>
    /// <param name="locale">Optional locale for localized display names.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Collection of categories.</returns>
    Task<IReadOnlyCollection<Category>> GetCategoryTreeAsync(string? tenantId = null, string? locale = null, CancellationToken ct = default);
    
    /// <summary>
    /// Gets subcategories for a given category.
    /// </summary>
    /// <param name="categoryKey">The parent category key.</param>
    /// <param name="tenantId">Optional tenant ID.</param>
    /// <param name="locale">Optional locale.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Collection of subcategories.</returns>
    Task<IReadOnlyCollection<SubCategory>> GetSubCategoriesAsync(string categoryKey, string? tenantId = null, string? locale = null, CancellationToken ct = default);
    
    /// <summary>
    /// Gets LoV items with optional filtering by subcategory and tags.
    /// </summary>
    /// <param name="categoryKey">The category key.</param>
    /// <param name="subCategoryKey">Optional subcategory key.</param>
    /// <param name="tags">Optional tags to filter by.</param>
    /// <param name="tenantId">Optional tenant ID.</param>
    /// <param name="locale">Optional locale.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Collection of LoV items.</returns>
    Task<IReadOnlyCollection<LovItem>> GetItemsAsync(string categoryKey, string? subCategoryKey = null, IEnumerable<string>? tags = null, string? tenantId = null, string? locale = null, CancellationToken ct = default);
}
