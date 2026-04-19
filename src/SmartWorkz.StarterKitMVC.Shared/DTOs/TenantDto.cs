namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

/// <summary>DTO for Tenant entity</summary>
public class TenantDto
{
    public string TenantId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string LogoUrl { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Website { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string CountryCode { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? SubscriptionExpiresAt { get; set; }
    public string SubscriptionTier { get; set; } // Free, Standard, Premium
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

public record CreateTenantDto(
    string Name,
    string DisplayName,
    string Description
);

public record UpdateTenantDto(
    string Name,
    string DisplayName,
    string Description,
    bool IsActive
);
