namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

public record MenuDto(
    int MenuId,
    string TenantId,
    string Name,
    string MenuType,
    int DisplayOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record MenuItemDto(
    int MenuItemId,
    int MenuId,
    string Title,
    string URL,
    string Icon,
    int DisplayOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateMenuDto(
    string Name,
    string MenuType,
    int DisplayOrder
);

public record UpdateMenuDto(
    string Name,
    string MenuType,
    int DisplayOrder
);

public record CreateMenuItemDto(
    string Title,
    string URL,
    string Icon,
    int DisplayOrder
);

public record UpdateMenuItemDto(
    string Title,
    string URL,
    string Icon,
    int DisplayOrder
);
