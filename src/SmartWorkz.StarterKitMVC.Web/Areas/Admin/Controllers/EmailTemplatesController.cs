using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.EmailTemplates;
using SmartWorkz.StarterKitMVC.Domain.EmailTemplates;
using SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

/// <summary>
/// Admin controller for managing email templates.
/// </summary>
[Area("Admin")]
public class EmailTemplatesController : Controller
{
    private readonly IEmailTemplateService _templateService;

    public EmailTemplatesController(IEmailTemplateService templateService)
    {
        _templateService = templateService;
    }

    #region Templates

    /// <summary>
    /// List all email templates.
    /// </summary>
    public async Task<IActionResult> Index(string? search = null, string? category = null)
    {
        ViewData["Title"] = "Email Templates";

        var templates = await _templateService.GetAllTemplatesAsync();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            templates = templates.Where(t =>
                t.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.Subject.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (t.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
            ).ToList();
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            templates = templates.Where(t =>
                string.Equals(t.Category, category, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        var allTemplates = await _templateService.GetAllTemplatesAsync();
        var categories = allTemplates
            .Where(t => !string.IsNullOrEmpty(t.Category))
            .Select(t => t.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        var model = new EmailTemplateListViewModel
        {
            Templates = templates.Select(t => new EmailTemplateItemViewModel
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Subject = t.Subject,
                Category = t.Category,
                IsActive = t.IsActive,
                IsSystem = t.IsSystem,
                UpdatedAt = t.UpdatedAt,
                PlaceholderCount = t.Placeholders.Count,
                Tags = t.Tags
            }).ToList(),
            SearchTerm = search,
            CategoryFilter = category,
            Categories = categories
        };

        return View(model);
    }

    /// <summary>
    /// Create new template form.
    /// </summary>
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Create Email Template";

        var model = await BuildFormViewModelAsync(new EmailTemplateFormViewModel());
        return View(model);
    }

    /// <summary>
    /// Create new template.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmailTemplateFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model = await BuildFormViewModelAsync(model);
            return View(model);
        }

        try
        {
            var template = MapToEntity(model);
            await _templateService.CreateTemplateAsync(template);
            TempData["Success"] = $"Template '{model.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model = await BuildFormViewModelAsync(model);
            return View(model);
        }
    }

    /// <summary>
    /// Edit template form.
    /// </summary>
    public async Task<IActionResult> Edit(string id)
    {
        ViewData["Title"] = "Edit Email Template";

        var template = await _templateService.GetTemplateByIdAsync(id);
        if (template == null)
        {
            TempData["Error"] = "Template not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = await BuildFormViewModelAsync(MapToViewModel(template));
        model.IsEditMode = true;
        model.IsSystem = template.IsSystem;

        return View(model);
    }

    /// <summary>
    /// Update template.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EmailTemplateFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model = await BuildFormViewModelAsync(model);
            model.IsEditMode = true;
            return View(model);
        }

        try
        {
            var template = MapToEntity(model);
            await _templateService.UpdateTemplateAsync(template);
            TempData["Success"] = $"Template '{model.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model = await BuildFormViewModelAsync(model);
            model.IsEditMode = true;
            return View(model);
        }
    }

    /// <summary>
    /// Delete template.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _templateService.DeleteTemplateAsync(id);
            TempData["Success"] = "Template deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Clone template.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clone(string id, string newId, string newName)
    {
        try
        {
            await _templateService.CloneTemplateAsync(id, newId, newName);
            TempData["Success"] = $"Template cloned as '{newName}'.";
            return RedirectToAction(nameof(Edit), new { id = newId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Preview template.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Preview([FromBody] EmailTemplateFormViewModel model)
    {
        var template = MapToEntity(model);
        var result = await _templateService.RenderPreviewAsync(template);

        return Json(new EmailTemplatePreviewViewModel
        {
            TemplateId = model.Id,
            Subject = result.Subject,
            HtmlBody = result.HtmlBody,
            Success = result.Success,
            Errors = result.Errors.ToList()
        });
    }

    #endregion

    #region Sections

    /// <summary>
    /// List all sections (headers and footers).
    /// </summary>
    public async Task<IActionResult> Sections()
    {
        ViewData["Title"] = "Email Template Sections";

        var sections = await _templateService.GetAllSectionsAsync();

        var model = new EmailTemplateSectionListViewModel
        {
            Headers = sections.Where(s => s.Type == SectionType.Header).Select(MapToSectionItemViewModel).ToList(),
            Footers = sections.Where(s => s.Type == SectionType.Footer).Select(MapToSectionItemViewModel).ToList()
        };

        return View(model);
    }

    /// <summary>
    /// Create section form.
    /// </summary>
    public IActionResult CreateSection(SectionType type = SectionType.Header)
    {
        ViewData["Title"] = $"Create {type}";

        var model = new EmailTemplateSectionFormViewModel
        {
            Type = type,
            SystemPlaceholders = _templateService.GetSystemPlaceholders().ToList()
        };

        return View(model);
    }

    /// <summary>
    /// Create section.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSection(EmailTemplateSectionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.SystemPlaceholders = _templateService.GetSystemPlaceholders().ToList();
            return View(model);
        }

        try
        {
            var section = MapToSectionEntity(model);
            await _templateService.CreateSectionAsync(section);
            TempData["Success"] = $"{model.Type} '{model.Name}' created successfully.";
            return RedirectToAction(nameof(Sections));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.SystemPlaceholders = _templateService.GetSystemPlaceholders().ToList();
            return View(model);
        }
    }

    /// <summary>
    /// Edit section form.
    /// </summary>
    public async Task<IActionResult> EditSection(string id)
    {
        var section = await _templateService.GetSectionByIdAsync(id);
        if (section == null)
        {
            TempData["Error"] = "Section not found.";
            return RedirectToAction(nameof(Sections));
        }

        ViewData["Title"] = $"Edit {section.Type}";

        var model = new EmailTemplateSectionFormViewModel
        {
            Id = section.Id,
            Name = section.Name,
            Type = section.Type,
            HtmlContent = section.HtmlContent,
            IsDefault = section.IsDefault,
            IsActive = section.IsActive,
            IsEditMode = true,
            SystemPlaceholders = _templateService.GetSystemPlaceholders().ToList()
        };

        return View(model);
    }

    /// <summary>
    /// Update section.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSection(string id, EmailTemplateSectionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.IsEditMode = true;
            model.SystemPlaceholders = _templateService.GetSystemPlaceholders().ToList();
            return View(model);
        }

        try
        {
            var section = MapToSectionEntity(model);
            await _templateService.UpdateSectionAsync(section);
            TempData["Success"] = $"{model.Type} '{model.Name}' updated successfully.";
            return RedirectToAction(nameof(Sections));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.IsEditMode = true;
            model.SystemPlaceholders = _templateService.GetSystemPlaceholders().ToList();
            return View(model);
        }
    }

    /// <summary>
    /// Delete section.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSection(string id)
    {
        try
        {
            await _templateService.DeleteSectionAsync(id);
            TempData["Success"] = "Section deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Sections));
    }

    #endregion

    #region Import/Export

    /// <summary>
    /// Export all templates and sections.
    /// </summary>
    public async Task<IActionResult> Export()
    {
        var json = await _templateService.ExportAllAsync();
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"email-templates-{DateTime.Now:yyyyMMdd}.json");
    }

    /// <summary>
    /// Import form.
    /// </summary>
    public IActionResult Import()
    {
        ViewData["Title"] = "Import Email Templates";
        return View(new EmailTemplateImportViewModel());
    }

    /// <summary>
    /// Import templates and sections.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(EmailTemplateImportViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var count = await _templateService.ImportAsync(model.JsonContent, model.Overwrite);
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

    private async Task<EmailTemplateFormViewModel> BuildFormViewModelAsync(EmailTemplateFormViewModel model)
    {
        var sections = await _templateService.GetAllSectionsAsync();

        model.AvailableHeaders = sections
            .Where(s => s.Type == SectionType.Header && s.IsActive)
            .Select(s => new EmailTemplateSectionOptionViewModel
            {
                Id = s.Id,
                Name = s.Name,
                IsDefault = s.IsDefault
            }).ToList();

        model.AvailableFooters = sections
            .Where(s => s.Type == SectionType.Footer && s.IsActive)
            .Select(s => new EmailTemplateSectionOptionViewModel
            {
                Id = s.Id,
                Name = s.Name,
                IsDefault = s.IsDefault
            }).ToList();

        model.SystemPlaceholders = _templateService.GetSystemPlaceholders().ToList();

        return model;
    }

    private static EmailTemplate MapToEntity(EmailTemplateFormViewModel model)
    {
        return new EmailTemplate
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Subject = model.Subject,
            HeaderId = model.HeaderId,
            FooterId = model.FooterId,
            BodyContent = model.BodyContent,
            PlainTextContent = model.PlainTextContent,
            Category = model.Category,
            Tags = string.IsNullOrWhiteSpace(model.TagsInput)
                ? new List<string>()
                : model.TagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            IsActive = model.IsActive,
            Placeholders = model.Placeholders.Select(p => new TemplatePlaceholder
            {
                Key = p.Key.StartsWith("{{") ? p.Key : $"{{{{{p.Key}}}}}",
                DisplayName = p.DisplayName,
                Description = p.Description,
                DefaultValue = p.DefaultValue,
                SampleValue = p.SampleValue,
                Type = p.Type,
                IsRequired = p.IsRequired,
                Order = p.Order
            }).ToList()
        };
    }

    private static EmailTemplateFormViewModel MapToViewModel(EmailTemplate template)
    {
        return new EmailTemplateFormViewModel
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Subject = template.Subject,
            HeaderId = template.HeaderId,
            FooterId = template.FooterId,
            BodyContent = template.BodyContent,
            PlainTextContent = template.PlainTextContent,
            Category = template.Category,
            TagsInput = string.Join(", ", template.Tags),
            IsActive = template.IsActive,
            Placeholders = template.Placeholders.Select(p => new PlaceholderFormViewModel
            {
                Key = p.Key,
                DisplayName = p.DisplayName,
                Description = p.Description,
                DefaultValue = p.DefaultValue,
                SampleValue = p.SampleValue,
                Type = p.Type,
                IsRequired = p.IsRequired,
                Order = p.Order
            }).ToList()
        };
    }

    private static EmailTemplateSection MapToSectionEntity(EmailTemplateSectionFormViewModel model)
    {
        return new EmailTemplateSection
        {
            Id = model.Id,
            Name = model.Name,
            Type = model.Type,
            HtmlContent = model.HtmlContent,
            IsDefault = model.IsDefault,
            IsActive = model.IsActive
        };
    }

    private static EmailTemplateSectionItemViewModel MapToSectionItemViewModel(EmailTemplateSection section)
    {
        return new EmailTemplateSectionItemViewModel
        {
            Id = section.Id,
            Name = section.Name,
            Type = section.Type,
            IsDefault = section.IsDefault,
            IsActive = section.IsActive,
            UpdatedAt = section.UpdatedAt
        };
    }

    #endregion
}
