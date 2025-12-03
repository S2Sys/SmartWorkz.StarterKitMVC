namespace SmartWorkz.StarterKitMVC.Application.LoV;

/// <summary>
/// Service for generating dropdown items from LoV data.
/// </summary>
/// <example>
/// <code>
/// // Inject IDropdownService via DI
/// public class FormController : Controller
/// {
///     private readonly IDropdownService _dropdown;
///     
///     public async Task&lt;IActionResult&gt; GetCountries()
///     {
///         var items = await _dropdown.GetDropdownAsync("countries", locale: "en-US");
///         return Json(items);
///     }
/// }
/// </code>
/// </example>
public interface IDropdownService
{
    /// <summary>
    /// Gets dropdown items for a category with optional filtering.
    /// </summary>
    /// <param name="categoryKey">The category key.</param>
    /// <param name="subCategoryKey">Optional subcategory filter.</param>
    /// <param name="tags">Optional tags to filter by.</param>
    /// <param name="tenantId">Optional tenant ID.</param>
    /// <param name="locale">Optional locale for localized text.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Collection of dropdown items.</returns>
    Task<IReadOnlyCollection<DropdownItem>> GetDropdownAsync(string categoryKey, string? subCategoryKey = null, IEnumerable<string>? tags = null, string? tenantId = null, string? locale = null, CancellationToken ct = default);
}
