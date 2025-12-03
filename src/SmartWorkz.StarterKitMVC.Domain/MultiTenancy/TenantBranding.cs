namespace SmartWorkz.StarterKitMVC.Domain.MultiTenancy;

/// <summary>
/// Represents branding customization for a tenant.
/// </summary>
/// <example>
/// <code>
/// var branding = new TenantBranding
/// {
///     TenantId = "acme-corp",
///     LogoUrl = "https://acme.com/logo.png",
///     PrimaryColor = "#0078d4",
///     SecondaryColor = "#50e6ff"
/// };
/// </code>
/// </example>
public sealed class TenantBranding
{
    /// <summary>Associated tenant ID.</summary>
    public string TenantId { get; init; } = string.Empty;
    
    /// <summary>URL to tenant's logo image.</summary>
    public string? LogoUrl { get; init; }
    
    /// <summary>Primary brand color (hex format).</summary>
    public string? PrimaryColor { get; init; }
    
    /// <summary>Secondary brand color (hex format).</summary>
    public string? SecondaryColor { get; init; }
}
