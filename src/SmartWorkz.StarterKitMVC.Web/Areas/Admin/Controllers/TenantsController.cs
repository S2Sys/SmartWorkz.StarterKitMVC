using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class TenantsController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Tenants";
        var tenants = GetSampleTenants();
        return View(tenants);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Create Tenant";
        var model = new TenantFormViewModel();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TenantFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // TODO: Save tenant via service
        TempData["Success"] = "Tenant created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(string id)
    {
        ViewData["Title"] = "Edit Tenant";
        // TODO: Get tenant from service
        var model = new TenantFormViewModel
        {
            Id = id,
            TenantId = id,
            Name = "Acme Corporation",
            Subdomain = "acme",
            IsActive = true,
            MaxUsers = 50,
            Branding = new TenantBrandingViewModel
            {
                PrimaryColor = "#0d6efd",
                SecondaryColor = "#6c757d",
                AccentColor = "#198754"
            }
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(string id, TenantFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // TODO: Update tenant via service
        TempData["Success"] = "Tenant updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(string id)
    {
        ViewData["Title"] = "Tenant Details";
        // TODO: Get tenant from service
        var model = new TenantDetailsViewModel
        {
            Id = id,
            Name = "Acme Corporation",
            Subdomain = "acme",
            DatabaseProvider = "SqlServer",
            IsActive = true,
            MaxUsers = 50,
            CurrentUserCount = 12,
            CreatedAt = DateTime.UtcNow.AddDays(-90),
            Features = ["MultiTenancy", "Notifications", "AuditLog"]
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(string id)
    {
        // TODO: Delete tenant via service
        TempData["Success"] = "Tenant deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Settings(string id)
    {
        ViewData["Title"] = "Tenant Settings";
        ViewData["TenantId"] = id;
        return View();
    }

    public IActionResult Branding(string id)
    {
        ViewData["Title"] = "Tenant Branding";
        var model = new TenantBrandingViewModel
        {
            PrimaryColor = "#0d6efd",
            SecondaryColor = "#6c757d",
            AccentColor = "#198754",
            FooterText = "© 2024 Acme Corporation"
        };
        ViewData["TenantId"] = id;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Branding(string id, TenantBrandingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["TenantId"] = id;
            return View(model);
        }

        // TODO: Save branding via service
        TempData["Success"] = "Branding updated successfully.";
        return RedirectToAction(nameof(Branding), new { id });
    }

    #region Sample Data

    private static List<TenantListViewModel> GetSampleTenants() =>
    [
        new() { Id = "default", Name = "Default Tenant", Subdomain = "default", IsActive = true, UserCount = 5, CreatedAt = DateTime.UtcNow.AddDays(-90) },
        new() { Id = "acme", Name = "Acme Corporation", Subdomain = "acme", Domain = "acme.example.com", IsActive = true, UserCount = 25, ExpirationDate = DateTime.UtcNow.AddYears(1), CreatedAt = DateTime.UtcNow.AddDays(-60) },
        new() { Id = "contoso", Name = "Contoso Ltd", Subdomain = "contoso", IsActive = true, UserCount = 12, CreatedAt = DateTime.UtcNow.AddDays(-30) },
        new() { Id = "fabrikam", Name = "Fabrikam Inc", Subdomain = "fabrikam", IsActive = false, UserCount = 0, ExpirationDate = DateTime.UtcNow.AddDays(-10), CreatedAt = DateTime.UtcNow.AddDays(-120) },
    ];

    #endregion
}
