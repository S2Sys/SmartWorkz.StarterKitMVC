using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartWorkz.StarterKitMVC.Shared.Models;

namespace SmartWorkz.StarterKitMVC.Shared.Pages;

/// <summary>
/// Base PageModel for all portal pages (Admin and Public).
/// Provides: TenantId, CurrentUser helpers, common utilities.
/// Note: Translation and service-specific helpers are implemented in derived classes
/// to avoid circular dependencies with Application layer.
/// </summary>
public abstract class BasePage : PageModel
{
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

    // ── ModelState helpers ────────────────────────────────────────────────────

    protected void AddErrors(Result result)
    {
        if (result.MessageKey != null)
            ModelState.AddModelError(string.Empty, result.MessageKey);
        foreach (var e in result.Errors)
            ModelState.AddModelError(string.Empty, e);
    }
}
