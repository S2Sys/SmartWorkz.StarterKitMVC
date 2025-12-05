using System.ComponentModel.DataAnnotations;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

/// <summary>
/// View model for displaying user in list
/// </summary>
public class UserListViewModel
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<string> Roles { get; set; } = [];
}

/// <summary>
/// View model for creating/editing user
/// </summary>
public class UserFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(256, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 256 characters")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Username can only contain letters, numbers, dots, underscores, and hyphens")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [StringLength(256)]
    public string? DisplayName { get; set; }

    [StringLength(500)]
    [Url(ErrorMessage = "Invalid URL format")]
    public string? AvatarUrl { get; set; }

    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    [StringLength(10)]
    public string? Locale { get; set; } = "en-US";

    public bool IsActive { get; set; } = true;

    public bool EmailConfirmed { get; set; }

    [StringLength(128)]
    public string? TenantId { get; set; }

    // For new users
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    public string? Password { get; set; }

    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string? ConfirmPassword { get; set; }

    // Selected roles
    public List<Guid> SelectedRoleIds { get; set; } = [];

    // Available roles for dropdown
    public List<RoleSelectItem> AvailableRoles { get; set; } = [];
}

public class RoleSelectItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

/// <summary>
/// View model for user details
/// </summary>
public class UserDetailsViewModel
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Locale { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public IEnumerable<string> Roles { get; set; } = [];
    public IEnumerable<ClaimViewModel> Claims { get; set; } = [];
}

public class ClaimViewModel
{
    public string Type { get; set; } = string.Empty;
    public string? Value { get; set; }
}
