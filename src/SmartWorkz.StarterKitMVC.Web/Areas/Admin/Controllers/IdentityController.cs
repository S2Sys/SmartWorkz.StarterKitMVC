using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Authorization;
using SmartWorkz.StarterKitMVC.Domain.Authorization;
using SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class IdentityController : Controller
{
    private readonly IClaimService _claimService;

    public IdentityController(IClaimService claimService)
    {
        _claimService = claimService;
    }

    #region Dashboard
    
    public IActionResult Index()
    {
        ViewData["Title"] = "Identity Management";
        return View();
    }
    
    #endregion

    #region Users
    
    public IActionResult Users()
    {
        ViewData["Title"] = "Users";
        // TODO: Get users from service
        var users = GetSampleUsers();
        return View(users);
    }

    public IActionResult UserCreate()
    {
        ViewData["Title"] = "Create User";
        var model = new UserFormViewModel
        {
            AvailableRoles = GetSampleRoles()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UserCreate(UserFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = GetSampleRoles();
            return View(model);
        }

        // TODO: Save user via service
        TempData["Success"] = "User created successfully.";
        return RedirectToAction(nameof(Users));
    }

    public IActionResult UserEdit(Guid id)
    {
        ViewData["Title"] = "Edit User";
        // TODO: Get user from service
        var model = new UserFormViewModel
        {
            Id = id,
            UserName = "john.doe",
            Email = "john@example.com",
            DisplayName = "John Doe",
            IsActive = true,
            AvailableRoles = GetSampleRoles()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UserEdit(Guid id, UserFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = GetSampleRoles();
            return View(model);
        }

        // TODO: Update user via service
        TempData["Success"] = "User updated successfully.";
        return RedirectToAction(nameof(Users));
    }

    public IActionResult UserDetails(Guid id)
    {
        ViewData["Title"] = "User Details";
        // TODO: Get user from service
        var model = new UserDetailsViewModel
        {
            Id = id,
            UserName = "john.doe",
            Email = "john@example.com",
            DisplayName = "John Doe",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            Roles = ["Administrator", "User"]
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UserDelete(Guid id)
    {
        // TODO: Delete user via service
        TempData["Success"] = "User deleted successfully.";
        return RedirectToAction(nameof(Users));
    }

    #endregion

    #region Roles
    
    public IActionResult Roles()
    {
        ViewData["Title"] = "Roles";
        // TODO: Get roles from service
        var roles = GetSampleRoleList();
        return View(roles);
    }

    public IActionResult RoleCreate()
    {
        ViewData["Title"] = "Create Role";
        var model = new RoleFormViewModel
        {
            PermissionGroups = GetSamplePermissionGroups()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RoleCreate(RoleFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.PermissionGroups = GetSamplePermissionGroups();
            return View(model);
        }

        // TODO: Save role via service
        TempData["Success"] = "Role created successfully.";
        return RedirectToAction(nameof(Roles));
    }

    public IActionResult RoleEdit(Guid id)
    {
        ViewData["Title"] = "Edit Role";
        // TODO: Get role from service
        var model = new RoleFormViewModel
        {
            Id = id,
            Name = "Manager",
            Description = "Management access",
            PermissionGroups = GetSamplePermissionGroups()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RoleEdit(Guid id, RoleFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.PermissionGroups = GetSamplePermissionGroups();
            return View(model);
        }

        // TODO: Update role via service
        TempData["Success"] = "Role updated successfully.";
        return RedirectToAction(nameof(Roles));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RoleDelete(Guid id)
    {
        // TODO: Delete role via service
        TempData["Success"] = "Role deleted successfully.";
        return RedirectToAction(nameof(Roles));
    }

    #endregion

    #region Claims
    
    public async Task<IActionResult> Claims()
    {
        ViewData["Title"] = "Claims Management";
        
        var claimTypes = await _claimService.GetAllClaimTypesAsync();
        var roleClaims = new List<RoleClaim>();
        foreach (var role in new[] { "Admin", "Manager", "User", "Guest" })
        {
            roleClaims.AddRange(await _claimService.GetRoleClaimsAsync(role));
        }
        
        var model = new ClaimsIndexViewModel
        {
            ClaimTypes = claimTypes.Select(ct => new ClaimTypeViewModel
            {
                Id = ct.Id,
                Key = ct.Key,
                Name = ct.Name,
                Description = ct.Description,
                Icon = ct.Icon,
                Category = ct.Category,
                AllowMultiple = ct.AllowMultiple,
                IsSystem = ct.IsSystem,
                IsActive = ct.IsActive,
                SortOrder = ct.SortOrder,
                ValueCount = ct.PredefinedValues.Count,
                PredefinedValues = ct.PredefinedValues.Select(v => new ClaimValueViewModel
                {
                    Id = v.Id,
                    Value = v.Value,
                    Label = v.Label,
                    Description = v.Description,
                    SortOrder = v.SortOrder,
                    IsActive = v.IsActive
                }).ToList()
            }).ToList(),
            TotalClaimTypes = claimTypes.Count,
            TotalRoleClaims = roleClaims.Count,
            Categories = claimTypes.Select(ct => ct.Category).Distinct().OrderBy(c => c).ToList()
        };
        
        return View(model);
    }

    public async Task<IActionResult> ClaimTypeCreate()
    {
        ViewData["Title"] = "Create Claim Type";
        var claimTypes = await _claimService.GetAllClaimTypesAsync();
        
        var model = new ClaimTypeFormViewModel
        {
            AvailableCategories = claimTypes.Select(ct => ct.Category).Distinct().OrderBy(c => c).ToList()
        };
        
        if (!model.AvailableCategories.Contains("General"))
            model.AvailableCategories.Insert(0, "General");
        
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClaimTypeCreate(ClaimTypeFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var claimTypes = await _claimService.GetAllClaimTypesAsync();
            model.AvailableCategories = claimTypes.Select(ct => ct.Category).Distinct().OrderBy(c => c).ToList();
            return View(model);
        }

        var claimType = new ClaimType
        {
            Key = model.Key,
            Name = model.Name,
            Description = model.Description,
            Icon = model.Icon,
            Category = model.Category,
            AllowMultiple = model.AllowMultiple,
            SortOrder = model.SortOrder,
            IsActive = true
        };

        await _claimService.CreateClaimTypeAsync(claimType);
        TempData["Success"] = $"Claim type '{model.Name}' created successfully.";
        return RedirectToAction(nameof(Claims));
    }

    public async Task<IActionResult> ClaimTypeEdit(Guid id)
    {
        var claimType = await _claimService.GetClaimTypeByIdAsync(id);
        if (claimType == null)
        {
            TempData["Error"] = "Claim type not found.";
            return RedirectToAction(nameof(Claims));
        }

        ViewData["Title"] = $"Edit Claim Type: {claimType.Name}";
        var claimTypes = await _claimService.GetAllClaimTypesAsync();
        
        var model = new ClaimTypeFormViewModel
        {
            Id = claimType.Id,
            Key = claimType.Key,
            Name = claimType.Name,
            Description = claimType.Description,
            Icon = claimType.Icon,
            Category = claimType.Category,
            AllowMultiple = claimType.AllowMultiple,
            SortOrder = claimType.SortOrder,
            IsSystem = claimType.IsSystem,
            AvailableCategories = claimTypes.Select(ct => ct.Category).Distinct().OrderBy(c => c).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClaimTypeEdit(Guid id, ClaimTypeFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var claimTypes = await _claimService.GetAllClaimTypesAsync();
            model.AvailableCategories = claimTypes.Select(ct => ct.Category).Distinct().OrderBy(c => c).ToList();
            return View(model);
        }

        var claimType = await _claimService.GetClaimTypeByIdAsync(id);
        if (claimType == null)
        {
            TempData["Error"] = "Claim type not found.";
            return RedirectToAction(nameof(Claims));
        }

        claimType.Name = model.Name;
        claimType.Description = model.Description;
        claimType.Icon = model.Icon;
        claimType.Category = model.Category;
        claimType.AllowMultiple = model.AllowMultiple;
        claimType.SortOrder = model.SortOrder;

        await _claimService.UpdateClaimTypeAsync(claimType);
        TempData["Success"] = $"Claim type '{model.Name}' updated successfully.";
        return RedirectToAction(nameof(Claims));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClaimTypeDelete(Guid id)
    {
        try
        {
            await _claimService.DeleteClaimTypeAsync(id);
            TempData["Success"] = "Claim type deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Claims));
    }

    public async Task<IActionResult> ClaimTypeDetails(Guid id)
    {
        var claimType = await _claimService.GetClaimTypeByIdAsync(id);
        if (claimType == null)
        {
            TempData["Error"] = "Claim type not found.";
            return RedirectToAction(nameof(Claims));
        }

        ViewData["Title"] = $"Claim Type: {claimType.Name}";
        
        var model = new ClaimTypeViewModel
        {
            Id = claimType.Id,
            Key = claimType.Key,
            Name = claimType.Name,
            Description = claimType.Description,
            Icon = claimType.Icon,
            Category = claimType.Category,
            AllowMultiple = claimType.AllowMultiple,
            IsSystem = claimType.IsSystem,
            IsActive = claimType.IsActive,
            SortOrder = claimType.SortOrder,
            ValueCount = claimType.PredefinedValues.Count,
            PredefinedValues = claimType.PredefinedValues.Select(v => new ClaimValueViewModel
            {
                Id = v.Id,
                Value = v.Value,
                Label = v.Label,
                Description = v.Description,
                SortOrder = v.SortOrder,
                IsActive = v.IsActive
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddClaimValue(ClaimValueFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid claim value data.";
            return RedirectToAction(nameof(ClaimTypeDetails), new { id = model.ClaimTypeId });
        }

        var value = new ClaimValue
        {
            Value = model.Value,
            Label = model.Label,
            Description = model.Description,
            SortOrder = model.SortOrder
        };

        await _claimService.AddClaimValueAsync(model.ClaimTypeId, value);
        TempData["Success"] = $"Claim value '{model.Label}' added successfully.";
        return RedirectToAction(nameof(ClaimTypeDetails), new { id = model.ClaimTypeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveClaimValue(Guid claimTypeId, Guid valueId)
    {
        await _claimService.RemoveClaimValueAsync(claimTypeId, valueId);
        TempData["Success"] = "Claim value removed successfully.";
        return RedirectToAction(nameof(ClaimTypeDetails), new { id = claimTypeId });
    }

    public async Task<IActionResult> RoleClaims(string roleId = "Admin")
    {
        ViewData["Title"] = $"Claims for Role: {roleId}";
        
        var claimTypes = await _claimService.GetActiveClaimTypesAsync();
        var roleClaims = await _claimService.GetRoleClaimsAsync(roleId);
        var grantedClaims = roleClaims.Where(rc => rc.IsGranted)
            .Select(rc => $"{rc.ClaimType}:{rc.ClaimValue}")
            .ToHashSet();
        
        var model = new RoleClaimsViewModel
        {
            RoleId = roleId,
            AvailableRoles = ["Admin", "Manager", "User", "Guest"], // TODO: Get from identity service
            ClaimTypes = claimTypes.Select(ct => new ClaimTypeWithValuesViewModel
            {
                Id = ct.Id,
                Key = ct.Key,
                Name = ct.Name,
                Icon = ct.Icon,
                Category = ct.Category,
                AllowMultiple = ct.AllowMultiple,
                Values = ct.PredefinedValues.Select(v => new ClaimValueCheckboxViewModel
                {
                    Id = v.Id,
                    Value = v.Value,
                    Label = v.Label,
                    Description = v.Description,
                    IsGranted = grantedClaims.Contains($"{ct.Key}:{v.Value}")
                }).ToList()
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveRoleClaims(string roleId, string claimType, List<string>? claimValues)
    {
        await _claimService.SetRoleClaimsAsync(roleId, claimType, claimValues ?? []);
        TempData["Success"] = $"Claims for role '{roleId}' saved successfully.";
        return RedirectToAction(nameof(RoleClaims), new { roleId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAllRoleClaims(string roleId, Dictionary<string, List<string>>? claims)
    {
        if (claims != null)
        {
            foreach (var (claimType, values) in claims)
            {
                await _claimService.SetRoleClaimsAsync(roleId, claimType, values ?? []);
            }
        }
        TempData["Success"] = $"All claims for role '{roleId}' saved successfully.";
        return RedirectToAction(nameof(RoleClaims), new { roleId });
    }

    public async Task<IActionResult> EntityClaims()
    {
        ViewData["Title"] = "Entity Claims";
        
        var entities = await _claimService.GetEntitiesWithClaimsAsync();
        var permissionType = await _claimService.GetClaimTypeByKeyAsync("permission");
        
        var model = entities.Select(e => new EntityClaimsSummaryViewModel
        {
            Entity = e,
            DisplayName = char.ToUpper(e[0]) + e[1..],
            Claims = permissionType?.PredefinedValues
                .Where(v => v.Value.StartsWith($"{e}."))
                .Select(v => v.Value)
                .ToList() ?? []
        }).ToList();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateEntityClaims(string entity, string displayName)
    {
        await _claimService.GenerateEntityClaimsAsync(entity, displayName);
        TempData["Success"] = $"Claims for entity '{displayName}' generated successfully.";
        return RedirectToAction(nameof(EntityClaims));
    }

    #endregion

    #region Sample Data Helpers
    
    private static List<UserListViewModel> GetSampleUsers() =>
    [
        new() { Id = Guid.NewGuid(), UserName = "admin", Email = "admin@example.com", DisplayName = "Administrator", IsActive = true, EmailConfirmed = true, CreatedAt = DateTime.UtcNow.AddDays(-90), Roles = ["Administrator"] },
        new() { Id = Guid.NewGuid(), UserName = "john.doe", Email = "john@example.com", DisplayName = "John Doe", IsActive = true, EmailConfirmed = true, CreatedAt = DateTime.UtcNow.AddDays(-30), Roles = ["User", "Manager"] },
        new() { Id = Guid.NewGuid(), UserName = "jane.smith", Email = "jane@example.com", DisplayName = "Jane Smith", IsActive = true, EmailConfirmed = false, CreatedAt = DateTime.UtcNow.AddDays(-15), Roles = ["User"] },
        new() { Id = Guid.NewGuid(), UserName = "bob.wilson", Email = "bob@example.com", DisplayName = "Bob Wilson", IsActive = false, EmailConfirmed = true, CreatedAt = DateTime.UtcNow.AddDays(-60), Roles = ["User"] },
    ];

    private static List<RoleSelectItem> GetSampleRoles() =>
    [
        new() { Id = Guid.NewGuid(), Name = "Administrator" },
        new() { Id = Guid.NewGuid(), Name = "Manager" },
        new() { Id = Guid.NewGuid(), Name = "User" },
    ];

    private static List<RoleListViewModel> GetSampleRoleList() =>
    [
        new() { Id = Guid.NewGuid(), Name = "Administrator", Description = "Full system access", IsSystemRole = true, UserCount = 1, CreatedAt = DateTime.UtcNow.AddDays(-90) },
        new() { Id = Guid.NewGuid(), Name = "Manager", Description = "Management access", IsSystemRole = true, UserCount = 5, CreatedAt = DateTime.UtcNow.AddDays(-90) },
        new() { Id = Guid.NewGuid(), Name = "User", Description = "Standard user access", IsSystemRole = true, UserCount = 25, CreatedAt = DateTime.UtcNow.AddDays(-90) },
    ];

    private static List<PermissionGroupViewModel> GetSamplePermissionGroups() =>
    [
        new() { Category = "Users", Permissions = [
            new() { Id = Guid.NewGuid(), Name = "users.view", DisplayName = "View Users" },
            new() { Id = Guid.NewGuid(), Name = "users.create", DisplayName = "Create Users" },
            new() { Id = Guid.NewGuid(), Name = "users.edit", DisplayName = "Edit Users" },
            new() { Id = Guid.NewGuid(), Name = "users.delete", DisplayName = "Delete Users" },
        ]},
        new() { Category = "Roles", Permissions = [
            new() { Id = Guid.NewGuid(), Name = "roles.view", DisplayName = "View Roles" },
            new() { Id = Guid.NewGuid(), Name = "roles.create", DisplayName = "Create Roles" },
            new() { Id = Guid.NewGuid(), Name = "roles.edit", DisplayName = "Edit Roles" },
            new() { Id = Guid.NewGuid(), Name = "roles.delete", DisplayName = "Delete Roles" },
        ]},
        new() { Category = "Settings", Permissions = [
            new() { Id = Guid.NewGuid(), Name = "settings.view", DisplayName = "View Settings" },
            new() { Id = Guid.NewGuid(), Name = "settings.edit", DisplayName = "Edit Settings" },
        ]},
    ];

    #endregion
}
