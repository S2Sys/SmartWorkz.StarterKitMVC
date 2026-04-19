namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

public record TenantDto(
    string TenantId,
    string Name,
    string DisplayName,
    string Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

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
