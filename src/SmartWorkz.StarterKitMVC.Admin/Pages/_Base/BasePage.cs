using SmartWorkz.StarterKitMVC.Application.Localization;
using SmartWorkz.StarterKitMVC.Shared.Models;
using SharedPages = SmartWorkz.StarterKitMVC.Shared.Pages;

namespace SmartWorkz.StarterKitMVC.Admin.Pages;

/// <summary>
/// Admin portal base PageModel extending Shared.BasePage.
/// Adds translation and service access specific to Admin.
/// </summary>
public abstract class BasePage : SharedPages.BasePage
{
    private ITranslationService? _translationService;

    protected ITranslationService TranslationService =>
        _translationService ??= (ITranslationService)HttpContext.RequestServices
            .GetService(typeof(ITranslationService))!;

    // ── Translation ───────────────────────────────────────────────────────────

    protected string T(string key)
    {
        var locale = User.FindFirst("locale")?.Value ?? "en-US";
        return TranslationService.Get(key, TenantId, locale);
    }

    protected string T(string key, params object[] args)
    {
        var locale = User.FindFirst("locale")?.Value ?? "en-US";
        return TranslationService.Get(key, TenantId, locale, args);
    }

    // ── Toast (TempData) ──────────────────────────────────────────────────────

    protected void ToastSuccess(string messageKey) => TempData["ToastSuccess"] = T(messageKey);
    protected void ToastError(string messageKey)   => TempData["ToastError"]   = T(messageKey);
    protected void ToastInfo(string messageKey)    => TempData["ToastInfo"]    = T(messageKey);

    // ── ModelState helpers ────────────────────────────────────────────────────

    protected new void AddErrors(Result result)
    {
        if (result.MessageKey != null)
            ModelState.AddModelError(string.Empty, T(result.MessageKey));
        foreach (var e in result.Errors)
            ModelState.AddModelError(string.Empty, e);
    }
}
