using System.ComponentModel.DataAnnotations;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

/// <summary>
/// View model for displaying tenant in list
/// </summary>
public class TenantListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Subdomain { get; set; }
    public string? Domain { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int UserCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// View model for creating/editing tenant
/// </summary>
public class TenantFormViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Tenant ID is required")]
    [StringLength(128, MinimumLength = 2, ErrorMessage = "Tenant ID must be between 2 and 128 characters")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Tenant ID can only contain lowercase letters, numbers, and hyphens")]
    public string TenantId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [StringLength(256, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 256 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(128)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Subdomain can only contain lowercase letters, numbers, and hyphens")]
    public string? Subdomain { get; set; }

    [StringLength(256)]
    public string? Domain { get; set; }

    [StringLength(500)]
    public string? ConnectionString { get; set; }

    [StringLength(50)]
    public string DatabaseProvider { get; set; } = "SqlServer";

    public bool IsActive { get; set; } = true;

    public DateTime? ExpirationDate { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Max users must be 0 or greater")]
    public int MaxUsers { get; set; }

    public List<string> Features { get; set; } = [];

    // Branding
    public TenantBrandingViewModel Branding { get; set; } = new();
}

public class TenantBrandingViewModel
{
    [StringLength(500)]
    [Url(ErrorMessage = "Invalid URL format")]
    public string? LogoUrl { get; set; }

    [StringLength(500)]
    [Url(ErrorMessage = "Invalid URL format")]
    public string? FaviconUrl { get; set; }

    [StringLength(20)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Invalid color format. Use hex format like #0d6efd")]
    public string PrimaryColor { get; set; } = "#0d6efd";

    [StringLength(20)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Invalid color format")]
    public string SecondaryColor { get; set; } = "#6c757d";

    [StringLength(20)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Invalid color format")]
    public string AccentColor { get; set; } = "#198754";

    public string? CustomCss { get; set; }

    [StringLength(500)]
    public string? FooterText { get; set; }

    [StringLength(256)]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? SupportEmail { get; set; }

    [StringLength(50)]
    public string? SupportPhone { get; set; }
}

/// <summary>
/// View model for tenant details
/// </summary>
public class TenantDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Subdomain { get; set; }
    public string? Domain { get; set; }
    public string DatabaseProvider { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int MaxUsers { get; set; }
    public int CurrentUserCount { get; set; }
    public List<string> Features { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public TenantBrandingViewModel Branding { get; set; } = new();
}
