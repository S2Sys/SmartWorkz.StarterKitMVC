namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

public record SeoMetaDto(
    int SeoMetaId,
    string TenantId,
    string EntityType,
    int EntityId,
    string Title,
    string Description,
    string Keywords,
    string Slug,
    string OgTitle,
    string OgDescription,
    string OgImageUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateSeoMetaDto(
    string EntityType,
    int EntityId,
    string Title,
    string Description,
    string Keywords,
    string Slug,
    string OgTitle,
    string OgDescription,
    string OgImageUrl
);

public record UpdateSeoMetaDto(
    string Title,
    string Description,
    string Keywords,
    string Slug,
    string OgTitle,
    string OgDescription,
    string OgImageUrl
);
