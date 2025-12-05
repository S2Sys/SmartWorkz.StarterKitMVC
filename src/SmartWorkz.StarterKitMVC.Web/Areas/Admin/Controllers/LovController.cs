using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class LovController : Controller
{
    public IActionResult Index(Guid? categoryId = null)
    {
        ViewData["Title"] = "List of Values";
        var model = new LovPageViewModel
        {
            Categories = GetSampleCategories(),
            SelectedCategory = categoryId.HasValue ? GetSampleCategories().FirstOrDefault(c => c.Id == categoryId) : GetSampleCategories().FirstOrDefault(),
            Items = GetSampleItems()
        };
        return View(model);
    }

    #region Categories

    public IActionResult Categories()
    {
        ViewData["Title"] = "LOV Categories";
        var categories = GetSampleCategories();
        return View(categories);
    }

    public IActionResult CategoryCreate()
    {
        ViewData["Title"] = "Create Category";
        return View(new LovCategoryFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CategoryCreate(LovCategoryFormViewModel model)
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
        var model = new LovCategoryFormViewModel
        {
            Id = id,
            Key = "countries",
            Name = "Countries",
            Description = "List of countries",
            Icon = "bi-globe",
            IsActive = true
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CategoryEdit(Guid id, LovCategoryFormViewModel model)
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

    #endregion

    #region Items

    public IActionResult Items(Guid? categoryId = null)
    {
        ViewData["Title"] = "LOV Items";
        var items = GetSampleItems();
        if (categoryId.HasValue)
        {
            // Filter by category
        }
        return View(items);
    }

    public IActionResult ItemCreate(Guid? categoryId = null)
    {
        ViewData["Title"] = "Create Item";
        var model = new LovItemFormViewModel
        {
            CategoryId = categoryId ?? Guid.Empty,
            AvailableCategories = GetSampleCategories()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ItemCreate(LovItemFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableCategories = GetSampleCategories();
            return View(model);
        }

        // TODO: Save item via service
        TempData["Success"] = "Item created successfully.";
        return RedirectToAction(nameof(Index), new { categoryId = model.CategoryId });
    }

    public IActionResult ItemEdit(Guid id)
    {
        ViewData["Title"] = "Edit Item";
        // TODO: Get item from service
        var model = new LovItemFormViewModel
        {
            Id = id,
            CategoryId = Guid.NewGuid(),
            Key = "US",
            DisplayName = "United States",
            SortOrder = 1,
            IsActive = true,
            AvailableCategories = GetSampleCategories()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ItemEdit(Guid id, LovItemFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableCategories = GetSampleCategories();
            return View(model);
        }

        // TODO: Update item via service
        TempData["Success"] = "Item updated successfully.";
        return RedirectToAction(nameof(Index), new { categoryId = model.CategoryId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ItemDelete(Guid id)
    {
        // TODO: Delete item via service
        TempData["Success"] = "Item deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region API Endpoints for AJAX

    [HttpGet]
    public IActionResult GetItems(Guid categoryId)
    {
        // TODO: Get items from service
        var items = GetSampleItems().Where(i => i.CategoryKey == "countries");
        return Json(items);
    }

    [HttpGet]
    public IActionResult GetSubCategories(Guid categoryId)
    {
        // TODO: Get subcategories from service
        var subCategories = new List<LovSubCategoryViewModel>();
        return Json(subCategories);
    }

    #endregion

    #region Sample Data

    private static List<LovCategoryListViewModel> GetSampleCategories() =>
    [
        new() { Id = Guid.NewGuid(), Key = "countries", Name = "Countries", Description = "List of countries", Icon = "bi-globe", IsSystem = true, IsActive = true, ItemCount = 8, CreatedAt = DateTime.UtcNow.AddDays(-90) },
        new() { Id = Guid.NewGuid(), Key = "statuses", Name = "Statuses", Description = "Common status values", Icon = "bi-check-circle", IsSystem = true, IsActive = true, ItemCount = 6, CreatedAt = DateTime.UtcNow.AddDays(-90) },
        new() { Id = Guid.NewGuid(), Key = "priorities", Name = "Priorities", Description = "Priority levels", Icon = "bi-flag", IsSystem = true, IsActive = true, ItemCount = 4, CreatedAt = DateTime.UtcNow.AddDays(-90) },
        new() { Id = Guid.NewGuid(), Key = "genders", Name = "Genders", Description = "Gender options", Icon = "bi-person", IsSystem = true, IsActive = true, ItemCount = 4, CreatedAt = DateTime.UtcNow.AddDays(-90) },
    ];

    private static List<LovItemListViewModel> GetSampleItems() =>
    [
        new() { Id = Guid.NewGuid(), Key = "US", DisplayName = "United States", CategoryKey = "countries", SortOrder = 1, IsActive = true },
        new() { Id = Guid.NewGuid(), Key = "GB", DisplayName = "United Kingdom", CategoryKey = "countries", SortOrder = 2, IsActive = true },
        new() { Id = Guid.NewGuid(), Key = "CA", DisplayName = "Canada", CategoryKey = "countries", SortOrder = 3, IsActive = true },
        new() { Id = Guid.NewGuid(), Key = "AU", DisplayName = "Australia", CategoryKey = "countries", SortOrder = 4, IsActive = true },
        new() { Id = Guid.NewGuid(), Key = "active", DisplayName = "Active", CategoryKey = "statuses", Color = "#198754", SortOrder = 1, IsActive = true },
        new() { Id = Guid.NewGuid(), Key = "inactive", DisplayName = "Inactive", CategoryKey = "statuses", Color = "#6c757d", SortOrder = 2, IsActive = true },
        new() { Id = Guid.NewGuid(), Key = "pending", DisplayName = "Pending", CategoryKey = "statuses", Color = "#ffc107", SortOrder = 3, IsActive = true },
        new() { Id = Guid.NewGuid(), Key = "low", DisplayName = "Low", CategoryKey = "priorities", Color = "#198754", Icon = "bi-arrow-down", SortOrder = 1, IsActive = true },
        new() { Id = Guid.NewGuid(), Key = "medium", DisplayName = "Medium", CategoryKey = "priorities", Color = "#ffc107", Icon = "bi-dash", SortOrder = 2, IsActive = true },
        new() { Id = Guid.NewGuid(), Key = "high", DisplayName = "High", CategoryKey = "priorities", Color = "#dc3545", Icon = "bi-arrow-up", SortOrder = 3, IsActive = true },
    ];

    #endregion
}
