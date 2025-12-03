using SmartWorkz.StarterKitMVC.Domain.Settings;

namespace SmartWorkz.StarterKitMVC.Application.Settings;

/// <summary>
/// Service for managing application settings with System → Tenant → User override hierarchy.
/// </summary>
/// <example>
/// <code>
/// // Inject ISettingsService via DI
/// public class ConfigController : Controller
/// {
///     private readonly ISettingsService _settings;
///     
///     public ConfigController(ISettingsService settings) => _settings = settings;
///     
///     public async Task&lt;IActionResult&gt; GetTheme(string userId)
///     {
///         // Gets user-level setting, falls back to tenant, then system
///         var theme = await _settings.GetAsync("app.theme", SettingScope.User, userId: userId);
///         return Ok(theme?.Value ?? "light");
///     }
///     
///     public async Task&lt;IActionResult&gt; SetTheme(string userId, string theme)
///     {
///         await _settings.SetAsync("app.theme", theme, SettingScope.User, userId: userId);
///         return Ok();
///     }
/// }
/// </code>
/// </example>
public interface ISettingsService
{
    /// <summary>
    /// Gets a setting value with scope-based override resolution.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <param name="scope">The scope level (System, Tenant, User).</param>
    /// <param name="tenantId">Optional tenant ID for tenant-scoped settings.</param>
    /// <param name="userId">Optional user ID for user-scoped settings.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The setting value or null if not found.</returns>
    Task<SettingValue?> GetAsync(string key, SettingScope scope = SettingScope.System, string? tenantId = null, string? userId = null, CancellationToken ct = default);
    
    /// <summary>
    /// Sets a setting value at the specified scope.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="scope">The scope level.</param>
    /// <param name="tenantId">Optional tenant ID.</param>
    /// <param name="userId">Optional user ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SetAsync(string key, string? value, SettingScope scope = SettingScope.System, string? tenantId = null, string? userId = null, CancellationToken ct = default);
    
    /// <summary>
    /// Gets all setting categories.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Collection of setting categories.</returns>
    Task<IReadOnlyCollection<SettingCategory>> GetCategoriesAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Gets setting definitions, optionally filtered by category.
    /// </summary>
    /// <param name="categoryKey">Optional category key to filter by.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Collection of setting definitions.</returns>
    Task<IReadOnlyCollection<SettingDefinition>> GetDefinitionsAsync(string? categoryKey = null, CancellationToken ct = default);
}
