using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartWorkz.StarterKitMVC.Application.Localization;
using SmartWorkz.StarterKitMVC.Shared.Models;

namespace SmartWorkz.StarterKitMVC.Public.Pages;

/// <summary>
/// Base PageModel for all Public portal pages.
/// Provides: TenantId, CurrentUser helpers, Toast (TempData), T() translation.
/// </summary>
public abstract class BasePage : PageModel
{
    private ITranslationService? _translationService;

    protected ITranslationService TranslationService =>
        _translationService ??= (ITranslationService)HttpContext.RequestServices
            .GetService(typeof(ITranslationService))!;

    // ── Tenant ────────────────────────────────────────────────────────────────

    protected string TenantId =>
        HttpContext.Items.TryGetValue("TenantId", out var t) && t is string s ? s : "DEFAULT";

    // ── Current user ──────────────────────────────────────────────────────────

    protected string? CurrentUserId =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    protected string? CurrentUserEmail =>
        User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

    protected string? CurrentUserDisplayName =>
        User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

    protected IEnumerable<string> CurrentUserRoles =>
        User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);

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

    protected void AddErrors(Result result)
    {
        if (result.MessageKey != null)
            ModelState.AddModelError(string.Empty, T(result.MessageKey));
        foreach (var e in result.Errors)
            ModelState.AddModelError(string.Empty, e);
    }
}
