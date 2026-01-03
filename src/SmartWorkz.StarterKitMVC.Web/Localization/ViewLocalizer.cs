using SmartWorkz.StarterKitMVC.Application.Localization;

namespace SmartWorkz.StarterKitMVC.Web.Localization;

/// <summary>
/// View localizer for accessing localized strings in views
/// </summary>
public class ViewLocalizer : IViewLocalizer
{
    private readonly IResourceService _resourceService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Dictionary<string, string>? _translations;
    private string _currentLanguage = "en";

    public ViewLocalizer(IResourceService resourceService, IHttpContextAccessor httpContextAccessor)
    {
        _resourceService = resourceService;
        _httpContextAccessor = httpContextAccessor;
    }

    public string this[string key] => GetString(key);

    public string this[string key, params object[] args] => GetString(key, args);

    public string GetString(string key)
    {
        EnsureTranslationsLoaded();
        return _translations!.TryGetValue(key, out var value) ? value : key;
    }

    public string GetString(string key, params object[] args)
    {
        var value = GetString(key);
        return args.Length > 0 ? string.Format(value, args) : value;
    }

    private void EnsureTranslationsLoaded()
    {
        if (_translations != null) return;

        // Get language from cookie, query string, or accept-language header
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            _currentLanguage = httpContext.Request.Cookies["lang"] 
                ?? httpContext.Request.Query["lang"].FirstOrDefault()
                ?? httpContext.Request.Headers.AcceptLanguage.FirstOrDefault()?.Split(',').FirstOrDefault()?.Split('-').FirstOrDefault()
                ?? "en";
        }

        // Load translations synchronously for view usage
        _translations = _resourceService.GetAllTranslationsAsync(_currentLanguage).GetAwaiter().GetResult();
    }
}

/// <summary>
/// Interface for view localizer
/// </summary>
public interface IViewLocalizer
{
    string this[string key] { get; }
    string this[string key, params object[] args] { get; }
    string GetString(string key);
    string GetString(string key, params object[] args);
}
