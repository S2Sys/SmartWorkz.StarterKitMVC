using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class IdentityController : Controller
{
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
    
    public IActionResult Claims()
    {
        ViewData["Title"] = "Claims";
        return View();
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
