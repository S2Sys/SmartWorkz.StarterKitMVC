using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Authorization;
using SmartWorkz.StarterKitMVC.Domain.Authorization;
using SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

/// <summary>
/// Admin controller for managing permissions and role assignments
/// </summary>
[Area("Admin")]
public class PermissionsController : Controller
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    #region Features

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Permissions & Features";
        
        var features = await _permissionService.GetFeatureTreeAsync();
        var permissions = await _permissionService.GetAllPermissionsAsync();
        
        var model = new PermissionsIndexViewModel
        {
            Features = features.Select(f => MapToFeatureViewModel(f)).ToList(),
            TotalPermissions = permissions.Count,
            TotalFeatures = features.Count
        };
        
        return View(model);
    }

    public async Task<IActionResult> FeatureCreate()
    {
        ViewData["Title"] = "Create Feature";
        var features = await _permissionService.GetAllFeaturesAsync();
        
        var model = new FeatureFormViewModel
        {
            AvailableParents = features.Select(f => new SelectOption { Value = f.Id.ToString(), Text = f.Name }).ToList()
        };
        
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FeatureCreate(FeatureFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var features = await _permissionService.GetAllFeaturesAsync();
            model.AvailableParents = features.Select(f => new SelectOption { Value = f.Id.ToString(), Text = f.Name }).ToList();
            return View(model);
        }

        var feature = new Feature
        {
            Key = model.Key,
            Name = model.Name,
            Description = model.Description,
            Icon = model.Icon,
            ParentId = model.ParentId,
            SortOrder = model.SortOrder,
            IsActive = true
        };

        await _permissionService.CreateFeatureAsync(feature);
        
        // Generate standard permissions if requested
        if (model.GenerateStandardPermissions)
        {
            await _permissionService.GenerateEntityPermissionsAsync(feature.Key, feature.Name);
        }

        TempData["Success"] = $"Feature '{model.Name}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> FeatureEdit(Guid id)
    {
        var feature = await _permissionService.GetFeatureByIdAsync(id);
        if (feature == null)
        {
            TempData["Error"] = "Feature not found.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = $"Edit Feature: {feature.Name}";
        var features = await _permissionService.GetAllFeaturesAsync();
        
        var model = new FeatureFormViewModel
        {
            Id = feature.Id,
            Key = feature.Key,
            Name = feature.Name,
            Description = feature.Description,
            Icon = feature.Icon,
            ParentId = feature.ParentId,
            SortOrder = feature.SortOrder,
            IsSystem = feature.IsSystem,
            AvailableParents = features.Where(f => f.Id != id).Select(f => new SelectOption { Value = f.Id.ToString(), Text = f.Name }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FeatureEdit(Guid id, FeatureFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var features = await _permissionService.GetAllFeaturesAsync();
            model.AvailableParents = features.Where(f => f.Id != id).Select(f => new SelectOption { Value = f.Id.ToString(), Text = f.Name }).ToList();
            return View(model);
        }

        var feature = await _permissionService.GetFeatureByIdAsync(id);
        if (feature == null)
        {
            TempData["Error"] = "Feature not found.";
            return RedirectToAction(nameof(Index));
        }

        feature.Name = model.Name;
        feature.Description = model.Description;
        feature.Icon = model.Icon;
        feature.ParentId = model.ParentId;
        feature.SortOrder = model.SortOrder;

        await _permissionService.UpdateFeatureAsync(feature);
        TempData["Success"] = $"Feature '{model.Name}' updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FeatureDelete(Guid id)
    {
        try
        {
            await _permissionService.DeleteFeatureAsync(id);
            TempData["Success"] = "Feature deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Permissions

    public async Task<IActionResult> Permissions(string? entity = null)
    {
        ViewData["Title"] = "Manage Permissions";
        
        var permissions = string.IsNullOrEmpty(entity)
            ? await _permissionService.GetAllPermissionsAsync()
            : await _permissionService.GetPermissionsByEntityAsync(entity);
        
        var features = await _permissionService.GetAllFeaturesAsync();
        
        var model = new PermissionListViewModel
        {
            Permissions = permissions.Select(p => new PermissionItemViewModel
            {
                Id = p.Id,
                Key = p.Key,
                Name = p.Name,
                Description = p.Description,
                Entity = p.Entity,
                Action = p.Action.ToString(),
                Group = p.Group,
                IsSystem = p.IsSystem,
                IsActive = p.IsActive
            }).ToList(),
            EntityFilter = entity,
            Entities = features.Select(f => f.Key).Distinct().ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> PermissionCreate()
    {
        ViewData["Title"] = "Create Permission";
        var features = await _permissionService.GetAllFeaturesAsync();
        
        var model = new PermissionFormViewModel
        {
            AvailableEntities = features.Select(f => new SelectOption { Value = f.Key, Text = f.Name }).ToList(),
            AvailableActions = Enum.GetValues<PermissionAction>().Select(a => new SelectOption { Value = a.ToString(), Text = a.ToString() }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PermissionCreate(PermissionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var features = await _permissionService.GetAllFeaturesAsync();
            model.AvailableEntities = features.Select(f => new SelectOption { Value = f.Key, Text = f.Name }).ToList();
            model.AvailableActions = Enum.GetValues<PermissionAction>().Select(a => new SelectOption { Value = a.ToString(), Text = a.ToString() }).ToList();
            return View(model);
        }

        var permission = new Permission
        {
            Key = $"{model.Entity.ToLowerInvariant()}.{model.Action.ToLowerInvariant()}",
            Name = model.Name,
            Description = model.Description,
            Entity = model.Entity,
            Action = Enum.Parse<PermissionAction>(model.Action),
            Group = model.Group,
            SortOrder = model.SortOrder,
            IsActive = true
        };

        await _permissionService.CreatePermissionAsync(permission);
        TempData["Success"] = $"Permission '{model.Name}' created successfully.";
        return RedirectToAction(nameof(Permissions));
    }

    #endregion

    #region Role Permissions

    public async Task<IActionResult> RolePermissions(string roleId = "Admin")
    {
        ViewData["Title"] = $"Permissions for Role: {roleId}";
        
        var features = await _permissionService.GetFeatureTreeAsync();
        var rolePermissions = await _permissionService.GetRolePermissionsAsync(roleId);
        var grantedPermissionIds = rolePermissions.Where(rp => rp.IsGranted).Select(rp => rp.PermissionId).ToHashSet();
        
        var model = new RolePermissionsViewModel
        {
            RoleId = roleId,
            Features = features.Select(f => MapToFeatureWithPermissionsViewModel(f, grantedPermissionIds)).ToList(),
            AvailableRoles = new List<string> { "Admin", "Manager", "User", "Guest" } // TODO: Get from identity service
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveRolePermissions(string roleId, List<Guid> permissionIds)
    {
        await _permissionService.SetRolePermissionsAsync(roleId, permissionIds ?? []);
        TempData["Success"] = $"Permissions for role '{roleId}' saved successfully.";
        return RedirectToAction(nameof(RolePermissions), new { roleId });
    }

    #endregion

    #region Helpers

    private FeatureViewModel MapToFeatureViewModel(Feature feature)
    {
        return new FeatureViewModel
        {
            Id = feature.Id,
            Key = feature.Key,
            Name = feature.Name,
            Description = feature.Description,
            Icon = feature.Icon,
            IsSystem = feature.IsSystem,
            PermissionCount = feature.Permissions.Count,
            Children = feature.Children.Select(c => MapToFeatureViewModel(c)).ToList()
        };
    }

    private FeatureWithPermissionsViewModel MapToFeatureWithPermissionsViewModel(Feature feature, HashSet<Guid> grantedPermissionIds)
    {
        return new FeatureWithPermissionsViewModel
        {
            Id = feature.Id,
            Key = feature.Key,
            Name = feature.Name,
            Icon = feature.Icon,
            Permissions = feature.Permissions.Select(p => new PermissionCheckboxViewModel
            {
                Id = p.Id,
                Key = p.Key,
                Name = p.Name,
                Action = p.Action.ToString(),
                IsGranted = grantedPermissionIds.Contains(p.Id)
            }).ToList(),
            Children = feature.Children.Select(c => MapToFeatureWithPermissionsViewModel(c, grantedPermissionIds)).ToList()
        };
    }

    #endregion
}
