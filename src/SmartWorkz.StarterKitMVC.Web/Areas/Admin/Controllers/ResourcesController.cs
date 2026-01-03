using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Localization;
using SmartWorkz.StarterKitMVC.Domain.Localization;
using SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

/// <summary>
/// Admin controller for managing localization resources
/// </summary>
[Area("Admin")]
public class ResourcesController : Controller
{
    private readonly IResourceService _resourceService;

    public ResourcesController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    #region Dashboard

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Localization Resources";
        
        var languages = await _resourceService.GetActiveLanguagesAsync();
        var resources = await _resourceService.GetResourceTreeAsync();
        
        var stats = new List<TranslationStats>();
        foreach (var lang in languages)
        {
            stats.Add(await _resourceService.GetTranslationStatsAsync(lang.Code));
        }
        
        var model = new ResourcesIndexViewModel
        {
            Languages = languages.Select(l => new LanguageViewModel
            {
                Code = l.Code,
                Name = l.Name,
                NativeName = l.NativeName,
                IsRtl = l.IsRtl,
                IsDefault = l.IsDefault,
                IsActive = l.IsActive
            }).ToList(),
            Resources = resources.Select(r => MapToResourceViewModel(r)).ToList(),
            Stats = stats,
            TotalResources = resources.Count
        };
        
        return View(model);
    }

    #endregion

    #region Languages

    public async Task<IActionResult> Languages()
    {
        ViewData["Title"] = "Manage Languages";
        
        var languages = await _resourceService.GetAllLanguagesAsync();
        var model = languages.Select(l => new LanguageViewModel
        {
            Code = l.Code,
            Name = l.Name,
            NativeName = l.NativeName,
            Icon = l.Icon,
            IsRtl = l.IsRtl,
            IsDefault = l.IsDefault,
            IsActive = l.IsActive,
            SortOrder = l.SortOrder
        }).ToList();
        
        return View(model);
    }

    public IActionResult LanguageCreate()
    {
        ViewData["Title"] = "Add Language";
        return View(new LanguageFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LanguageCreate(LanguageFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var language = new Language
        {
            Code = model.Code,
            Name = model.Name,
            NativeName = model.NativeName,
            Icon = model.Icon,
            IsRtl = model.IsRtl,
            IsDefault = model.IsDefault,
            IsActive = true,
            SortOrder = model.SortOrder
        };

        await _resourceService.CreateLanguageAsync(language);
        TempData["Success"] = $"Language '{model.Name}' added successfully.";
        return RedirectToAction(nameof(Languages));
    }

    public async Task<IActionResult> LanguageEdit(string code)
    {
        var language = await _resourceService.GetLanguageByCodeAsync(code);
        if (language == null)
        {
            TempData["Error"] = "Language not found.";
            return RedirectToAction(nameof(Languages));
        }

        ViewData["Title"] = $"Edit Language: {language.Name}";
        
        var model = new LanguageFormViewModel
        {
            Code = language.Code,
            Name = language.Name,
            NativeName = language.NativeName,
            Icon = language.Icon,
            IsRtl = language.IsRtl,
            IsDefault = language.IsDefault,
            SortOrder = language.SortOrder,
            IsEditMode = true
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LanguageEdit(string code, LanguageFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.IsEditMode = true;
            return View(model);
        }

        var language = await _resourceService.GetLanguageByCodeAsync(code);
        if (language == null)
        {
            TempData["Error"] = "Language not found.";
            return RedirectToAction(nameof(Languages));
        }

        language.Name = model.Name;
        language.NativeName = model.NativeName;
        language.Icon = model.Icon;
        language.IsRtl = model.IsRtl;
        language.IsDefault = model.IsDefault;
        language.SortOrder = model.SortOrder;

        await _resourceService.UpdateLanguageAsync(language);
        TempData["Success"] = $"Language '{model.Name}' updated successfully.";
        return RedirectToAction(nameof(Languages));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LanguageDelete(string code)
    {
        try
        {
            await _resourceService.DeleteLanguageAsync(code);
            TempData["Success"] = "Language deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Languages));
    }

    #endregion

    #region Resources

    public async Task<IActionResult> Resources(string? module = null, string? category = null)
    {
        ViewData["Title"] = "Manage Resources";
        
        var resources = await _resourceService.GetResourceTreeAsync();
        var languages = await _resourceService.GetActiveLanguagesAsync();
        
        var model = new ResourceListViewModel
        {
            Resources = resources.Select(r => MapToResourceViewModel(r)).ToList(),
            Languages = languages.Select(l => l.Code).ToList(),
            ModuleFilter = module,
            CategoryFilter = category,
            Modules = resources.Select(r => r.Module).Where(m => !string.IsNullOrEmpty(m)).Distinct().ToList()!,
            Categories = resources.Select(r => r.Category).Distinct().ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> ResourceCreate()
    {
        ViewData["Title"] = "Create Resource";
        var resources = await _resourceService.GetAllResourcesAsync();
        
        var model = new ResourceFormViewModel
        {
            AvailableParents = resources.Select(r => new SelectOption { Value = r.Id.ToString(), Text = r.Key }).ToList(),
            Categories = new List<string> { "General", "Labels", "Buttons", "Messages", "Errors", "Validation" }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResourceCreate(ResourceFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var resources = await _resourceService.GetAllResourcesAsync();
            model.AvailableParents = resources.Select(r => new SelectOption { Value = r.Id.ToString(), Text = r.Key }).ToList();
            model.Categories = new List<string> { "General", "Labels", "Buttons", "Messages", "Errors", "Validation" };
            return View(model);
        }

        var resource = new Resource
        {
            Key = model.Key,
            ParentId = model.ParentId,
            Category = model.Category,
            Module = model.Module,
            Description = model.Description,
            MaxLength = model.MaxLength,
            SupportsPluralForms = model.SupportsPluralForms,
            Placeholders = model.PlaceholdersInput?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? [],
            SortOrder = model.SortOrder,
            IsActive = true
        };

        var created = await _resourceService.CreateResourceAsync(resource);
        
        // Add default language translation if provided
        if (!string.IsNullOrWhiteSpace(model.DefaultValue))
        {
            var defaultLang = await _resourceService.GetDefaultLanguageCodeAsync();
            await _resourceService.SetTranslationAsync(created.Id, defaultLang, model.DefaultValue, model.PluralValue);
        }

        TempData["Success"] = $"Resource '{model.Key}' created successfully.";
        return RedirectToAction(nameof(Resources));
    }

    public async Task<IActionResult> ResourceEdit(Guid id)
    {
        var resource = await _resourceService.GetResourceByIdAsync(id);
        if (resource == null)
        {
            TempData["Error"] = "Resource not found.";
            return RedirectToAction(nameof(Resources));
        }

        ViewData["Title"] = $"Edit Resource: {resource.Key}";
        var resources = await _resourceService.GetAllResourcesAsync();
        
        var model = new ResourceFormViewModel
        {
            Id = resource.Id,
            Key = resource.Key,
            ParentId = resource.ParentId,
            Category = resource.Category,
            Module = resource.Module,
            Description = resource.Description,
            MaxLength = resource.MaxLength,
            SupportsPluralForms = resource.SupportsPluralForms,
            PlaceholdersInput = string.Join(", ", resource.Placeholders),
            SortOrder = resource.SortOrder,
            IsSystem = resource.IsSystem,
            AvailableParents = resources.Where(r => r.Id != id).Select(r => new SelectOption { Value = r.Id.ToString(), Text = r.Key }).ToList(),
            Categories = new List<string> { "General", "Labels", "Buttons", "Messages", "Errors", "Validation" }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResourceEdit(Guid id, ResourceFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var resources = await _resourceService.GetAllResourcesAsync();
            model.AvailableParents = resources.Where(r => r.Id != id).Select(r => new SelectOption { Value = r.Id.ToString(), Text = r.Key }).ToList();
            model.Categories = new List<string> { "General", "Labels", "Buttons", "Messages", "Errors", "Validation" };
            return View(model);
        }

        var resource = await _resourceService.GetResourceByIdAsync(id);
        if (resource == null)
        {
            TempData["Error"] = "Resource not found.";
            return RedirectToAction(nameof(Resources));
        }

        resource.Key = model.Key;
        resource.ParentId = model.ParentId;
        resource.Category = model.Category;
        resource.Module = model.Module;
        resource.Description = model.Description;
        resource.MaxLength = model.MaxLength;
        resource.SupportsPluralForms = model.SupportsPluralForms;
        resource.Placeholders = model.PlaceholdersInput?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? [];
        resource.SortOrder = model.SortOrder;

        await _resourceService.UpdateResourceAsync(resource);
        TempData["Success"] = $"Resource '{model.Key}' updated successfully.";
        return RedirectToAction(nameof(Resources));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResourceDelete(Guid id)
    {
        try
        {
            await _resourceService.DeleteResourceAsync(id);
            TempData["Success"] = "Resource deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Resources));
    }

    #endregion

    #region Translations

    public async Task<IActionResult> Translations(string languageCode = "en")
    {
        ViewData["Title"] = $"Translations: {languageCode.ToUpperInvariant()}";
        
        var language = await _resourceService.GetLanguageByCodeAsync(languageCode);
        if (language == null)
        {
            TempData["Error"] = "Language not found.";
            return RedirectToAction(nameof(Index));
        }

        var resources = await _resourceService.GetAllResourcesAsync();
        var translations = await _resourceService.GetTranslationsForLanguageAsync(languageCode);
        var translationDict = translations.ToDictionary(t => t.ResourceId, t => t);
        var languages = await _resourceService.GetActiveLanguagesAsync();
        var stats = await _resourceService.GetTranslationStatsAsync(languageCode);
        
        var model = new TranslationsViewModel
        {
            LanguageCode = languageCode,
            LanguageName = language.Name,
            Languages = languages.Select(l => new LanguageViewModel
            {
                Code = l.Code,
                Name = l.Name,
                NativeName = l.NativeName,
                IsDefault = l.IsDefault
            }).ToList(),
            Resources = resources.Select(r => new ResourceTranslationItemViewModel
            {
                ResourceId = r.Id,
                ResourceKey = r.Key,
                Category = r.Category,
                Description = r.Description,
                Placeholders = r.Placeholders,
                Value = translationDict.TryGetValue(r.Id, out var t) ? t.Value : null,
                PluralValue = translationDict.TryGetValue(r.Id, out var tp) ? tp.PluralValue : null,
                Status = translationDict.TryGetValue(r.Id, out var ts) ? ts.Status : null,
                SupportsPluralForms = r.SupportsPluralForms
            }).ToList(),
            Stats = stats
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTranslation(Guid resourceId, string languageCode, string value, string? pluralValue)
    {
        await _resourceService.SetTranslationAsync(resourceId, languageCode, value, pluralValue);
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAllTranslations(string languageCode, List<TranslationUpdateModel> translations)
    {
        foreach (var t in translations.Where(t => !string.IsNullOrWhiteSpace(t.Value)))
        {
            await _resourceService.SetTranslationAsync(t.ResourceId, languageCode, t.Value!, t.PluralValue);
        }
        
        TempData["Success"] = $"Translations saved successfully.";
        return RedirectToAction(nameof(Translations), new { languageCode });
    }

    #endregion

    #region Import/Export

    public async Task<IActionResult> Export(string? languageCode = null)
    {
        var json = await _resourceService.ExportAsync(languageCode);
        var fileName = languageCode == null 
            ? $"resources-all-{DateTime.Now:yyyyMMdd}.json"
            : $"resources-{languageCode}-{DateTime.Now:yyyyMMdd}.json";
        
        return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", fileName);
    }

    public IActionResult Import()
    {
        ViewData["Title"] = "Import Resources";
        return View(new ResourceImportViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(ResourceImportViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var count = await _resourceService.ImportAsync(model.JsonContent, model.Overwrite);
            TempData["Success"] = $"Successfully imported {count} items.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Import failed: {ex.Message}");
            return View(model);
        }
    }

    #endregion

    #region Helpers

    private ResourceViewModel MapToResourceViewModel(Resource resource)
    {
        return new ResourceViewModel
        {
            Id = resource.Id,
            Key = resource.Key,
            Category = resource.Category,
            Module = resource.Module,
            Description = resource.Description,
            IsSystem = resource.IsSystem,
            TranslationCount = resource.Translations.Count,
            Children = resource.Children.Select(c => MapToResourceViewModel(c)).ToList()
        };
    }

    #endregion
}

public class TranslationUpdateModel
{
    public Guid ResourceId { get; set; }
    public string? Value { get; set; }
    public string? PluralValue { get; set; }
}
