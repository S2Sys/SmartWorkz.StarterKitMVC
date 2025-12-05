using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class SettingsController : Controller
{
    public IActionResult Index(string? category = null)
    {
        ViewData["Title"] = "Settings";
        var model = new SettingsPageViewModel
        {
            Categories = GetSampleCategories(),
            ActiveCategoryKey = category ?? "general"
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveSettings(SaveSettingsViewModel model)
    {
        // TODO: Save settings via service
        TempData["Success"] = "Settings saved successfully.";
        return RedirectToAction(nameof(Index), new { category = model.CategoryKey });
    }

    public IActionResult Categories()
    {
        ViewData["Title"] = "Setting Categories";
        var categories = GetSampleCategoryList();
        return View(categories);
    }

    public IActionResult CategoryCreate()
    {
        ViewData["Title"] = "Create Category";
        return View(new SettingCategoryFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CategoryCreate(SettingCategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // TODO: Save category via service
        TempData["Success"] = "Category created successfully.";
        return RedirectToAction(nameof(Categories));
    }

    public IActionResult CategoryEdit(Guid id)
    {
        ViewData["Title"] = "Edit Category";
        // TODO: Get category from service
        var model = new SettingCategoryFormViewModel
        {
            Id = id,
            Key = "general",
            Name = "General",
            Description = "General application settings",
            Icon = "bi-gear",
            SortOrder = 1
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CategoryEdit(Guid id, SettingCategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // TODO: Update category via service
        TempData["Success"] = "Category updated successfully.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CategoryDelete(Guid id)
    {
        // TODO: Delete category via service
        TempData["Success"] = "Category deleted successfully.";
        return RedirectToAction(nameof(Categories));
    }

    public IActionResult Definitions()
    {
        ViewData["Title"] = "Setting Definitions";
        return View();
    }

    public IActionResult DefinitionCreate()
    {
        ViewData["Title"] = "Create Setting";
        var model = new SettingDefinitionFormViewModel
        {
            AvailableCategories = GetSampleCategoryList()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DefinitionCreate(SettingDefinitionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableCategories = GetSampleCategoryList();
            return View(model);
        }

        // TODO: Save definition via service
        TempData["Success"] = "Setting definition created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(string key)
    {
        ViewData["Title"] = "Edit Setting";
        // TODO: Get setting from service
        var model = new SettingDefinitionFormViewModel
        {
            Key = key,
            Name = "Application Name",
            ValueType = "String",
            DefaultValue = "SmartWorkz StarterKitMVC",
            AvailableCategories = GetSampleCategoryList()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(string key, SettingDefinitionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableCategories = GetSampleCategoryList();
            return View(model);
        }

        // TODO: Update setting via service
        TempData["Success"] = "Setting updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    #region Sample Data

    private static List<SettingCategoryWithSettingsViewModel> GetSampleCategories() =>
    [
        new() {
            Id = Guid.NewGuid(), Key = "general", Name = "General", Description = "General application settings", Icon = "bi-gear",
            Settings = [
                new() { Id = Guid.NewGuid(), Key = "app.name", Name = "Application Name", ValueType = "String", DefaultValue = "SmartWorkz StarterKitMVC", CurrentValue = "SmartWorkz StarterKitMVC", IsRequired = true },
                new() { Id = Guid.NewGuid(), Key = "app.description", Name = "Application Description", ValueType = "String", DefaultValue = "Enterprise ASP.NET Core MVC Boilerplate", CurrentValue = "Enterprise ASP.NET Core MVC Boilerplate" },
                new() { Id = Guid.NewGuid(), Key = "app.timezone", Name = "Default Timezone", ValueType = "String", DefaultValue = "UTC", CurrentValue = "UTC", IsRequired = true },
                new() { Id = Guid.NewGuid(), Key = "app.maintenance", Name = "Maintenance Mode", ValueType = "Bool", DefaultValue = "false", CurrentValue = "false" },
            ]
        },
        new() {
            Id = Guid.NewGuid(), Key = "security", Name = "Security", Description = "Security and authentication settings", Icon = "bi-shield-lock",
            Settings = [
                new() { Id = Guid.NewGuid(), Key = "security.passwordminlength", Name = "Minimum Password Length", ValueType = "Int", DefaultValue = "8", CurrentValue = "8", IsRequired = true, MinValue = "6", MaxValue = "50" },
                new() { Id = Guid.NewGuid(), Key = "security.requiredigit", Name = "Require Digit", ValueType = "Bool", DefaultValue = "true", CurrentValue = "true" },
                new() { Id = Guid.NewGuid(), Key = "security.lockoutmaxattempts", Name = "Max Login Attempts", ValueType = "Int", DefaultValue = "5", CurrentValue = "5", IsRequired = true },
                new() { Id = Guid.NewGuid(), Key = "security.sessiontimeout", Name = "Session Timeout (minutes)", ValueType = "Int", DefaultValue = "30", CurrentValue = "30", IsRequired = true },
            ]
        },
        new() {
            Id = Guid.NewGuid(), Key = "email", Name = "Email", Description = "Email and SMTP settings", Icon = "bi-envelope",
            Settings = [
                new() { Id = Guid.NewGuid(), Key = "email.smtphost", Name = "SMTP Host", ValueType = "String", DefaultValue = "smtp.example.com", CurrentValue = "" },
                new() { Id = Guid.NewGuid(), Key = "email.smtpport", Name = "SMTP Port", ValueType = "Int", DefaultValue = "587", CurrentValue = "587" },
                new() { Id = Guid.NewGuid(), Key = "email.smtppassword", Name = "SMTP Password", ValueType = "String", IsEncrypted = true },
                new() { Id = Guid.NewGuid(), Key = "email.fromaddress", Name = "From Address", ValueType = "String", DefaultValue = "noreply@example.com", CurrentValue = "noreply@example.com" },
            ]
        },
        new() {
            Id = Guid.NewGuid(), Key = "appearance", Name = "Appearance", Description = "UI and theme settings", Icon = "bi-palette",
            Settings = [
                new() { Id = Guid.NewGuid(), Key = "appearance.theme", Name = "Default Theme", ValueType = "String", DefaultValue = "light", CurrentValue = "light", Options = [new() { Value = "light", Label = "Light" }, new() { Value = "dark", Label = "Dark" }] },
                new() { Id = Guid.NewGuid(), Key = "appearance.primarycolor", Name = "Primary Color", ValueType = "String", DefaultValue = "#0d6efd", CurrentValue = "#0d6efd" },
            ]
        },
    ];

    private static List<SettingCategoryViewModel> GetSampleCategoryList() =>
    [
        new() { Id = Guid.NewGuid(), Key = "general", Name = "General", Description = "General application settings", Icon = "bi-gear", SortOrder = 1, IsSystem = true, SettingCount = 5 },
        new() { Id = Guid.NewGuid(), Key = "security", Name = "Security", Description = "Security and authentication settings", Icon = "bi-shield-lock", SortOrder = 2, IsSystem = true, SettingCount = 6 },
        new() { Id = Guid.NewGuid(), Key = "email", Name = "Email", Description = "Email and SMTP settings", Icon = "bi-envelope", SortOrder = 3, IsSystem = true, SettingCount = 7 },
        new() { Id = Guid.NewGuid(), Key = "appearance", Name = "Appearance", Description = "UI and theme settings", Icon = "bi-palette", SortOrder = 4, IsSystem = true, SettingCount = 4 },
    ];

    #endregion
}
