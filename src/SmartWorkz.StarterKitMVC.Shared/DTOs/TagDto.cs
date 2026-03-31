namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

public record TagDto(
    int TagId,
    string TenantId,
    string TagName,
    string EntityType,
    int? EntityId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateTagDto(string TagName);

public record UpdateTagDto(string TagName);

public record TagAssignmentDto(string EntityType, int EntityId);
