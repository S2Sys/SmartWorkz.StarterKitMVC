using System.ComponentModel.DataAnnotations;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

#region Index

public class PermissionsIndexViewModel
{
    public List<FeatureViewModel> Features { get; set; } = [];
    public int TotalPermissions { get; set; }
    public int TotalFeatures { get; set; }
}

public class FeatureViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsSystem { get; set; }
    public int PermissionCount { get; set; }
    public List<FeatureViewModel> Children { get; set; } = [];
}

#endregion

#region Feature Form

public class FeatureFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Key is required")]
    [StringLength(128, MinimumLength = 2)]
    [RegularExpression(@"^[a-z0-9_]+$", ErrorMessage = "Key can only contain lowercase letters, numbers, and underscores")]
    public string Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? Icon { get; set; }

    public Guid? ParentId { get; set; }

    public int SortOrder { get; set; }

    public bool IsSystem { get; set; }

    public bool GenerateStandardPermissions { get; set; } = true;

    public List<SelectOption> AvailableParents { get; set; } = [];
}

#endregion

#region Permission List

public class PermissionListViewModel
{
    public List<PermissionItemViewModel> Permissions { get; set; } = [];
    public string? EntityFilter { get; set; }
    public List<string> Entities { get; set; } = [];
}

public class PermissionItemViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Entity { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Group { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
}

#endregion

#region Permission Form

public class PermissionFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Entity is required")]
    public string Entity { get; set; } = string.Empty;

    [Required(ErrorMessage = "Action is required")]
    public string Action { get; set; } = string.Empty;

    [StringLength(128)]
    public string? Group { get; set; }

    public int SortOrder { get; set; }

    public List<SelectOption> AvailableEntities { get; set; } = [];
    public List<SelectOption> AvailableActions { get; set; } = [];
}

#endregion

#region Role Permissions

public class RolePermissionsViewModel
{
    public string RoleId { get; set; } = string.Empty;
    public List<FeatureWithPermissionsViewModel> Features { get; set; } = [];
    public List<string> AvailableRoles { get; set; } = [];
}

public class FeatureWithPermissionsViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public List<PermissionCheckboxViewModel> Permissions { get; set; } = [];
    public List<FeatureWithPermissionsViewModel> Children { get; set; } = [];
}

public class PermissionCheckboxViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
}

#endregion

#region Common

public class SelectOption
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

#endregion
