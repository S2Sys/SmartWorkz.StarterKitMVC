using SmartWorkz.StarterKitMVC.Domain.Entities.Master;
using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;
using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Application.Mappings;

public static class TenantMapper
{
    public static TenantDto ToDto(Tenant src) => new(
        src.TenantId,
        src.Name,
        src.DisplayName,
        src.Description,
        src.IsActive,
        src.CreatedAt,
        src.UpdatedAt
    );

    public static Tenant ToEntity(CreateTenantDto src) => new()
    {
        Name = src.Name,
        DisplayName = src.DisplayName,
        Description = src.Description
    };

    public static void ApplyUpdate(UpdateTenantDto src, Tenant dest)
    {
        dest.Name = src.Name;
        dest.DisplayName = src.DisplayName;
        dest.Description = src.Description;
        dest.IsActive = src.IsActive;
    }
}

public static class ProductMapper
{
    public static ProductDto ToDto(Product src) => new(
        src.ProductId,
        src.TenantId,
        src.CategoryId,
        src.Name,
        src.Description,
        src.SKU,
        src.Slug,
        src.Price,
        src.Stock,
        src.IsFeatured,
        src.CreatedAt,
        src.UpdatedAt
    );

    public static ProductSearchResultDto ToSearchResult(Product src) => new(
        src.ProductId,
        src.Name,
        src.Slug,
        src.Price,
        src.IsFeatured
    );

    public static Product ToEntity(CreateProductDto src, string tenantId) => new()
    {
        TenantId = tenantId,
        CategoryId = src.CategoryId,
        Name = src.Name,
        Description = src.Description,
        SKU = src.SKU,
        Slug = src.Slug,
        Price = src.Price,
        Stock = src.Stock,
        IsFeatured = src.IsFeatured
    };

    public static void ApplyUpdate(UpdateProductDto src, Product dest)
    {
        dest.Name = src.Name;
        dest.Description = src.Description;
        dest.Price = src.Price;
        dest.Stock = src.Stock;
        dest.Slug = src.Slug;
        dest.IsFeatured = src.IsFeatured;
    }
}

public static class CategoryMapper
{
    public static CategoryDto ToDto(Category src) => new(
        src.CategoryId,
        src.TenantId,
        src.Name,
        src.Description,
        src.Slug,
        src.ParentCategoryId,
        src.CreatedAt,
        src.UpdatedAt
    );

    public static CategoryTreeDto ToTreeDto(Category src) => new(
        src.CategoryId,
        src.Name,
        src.Slug
    )
    {
        Children = src.ChildCategories?
            .Select(ToTreeDto)
            .ToList() ?? new()
    };

    public static Category ToEntity(CreateCategoryDto src, string tenantId) => new()
    {
        TenantId = tenantId,
        Name = src.Name,
        Description = src.Description,
        Slug = src.Slug,
        ParentCategoryId = src.ParentCategoryId
    };

    public static void ApplyUpdate(UpdateCategoryDto src, Category dest)
    {
        dest.Name = src.Name;
        dest.Description = src.Description;
        dest.Slug = src.Slug;
        dest.ParentCategoryId = src.ParentCategoryId;
    }
}

public static class MenuMapper
{
    public static MenuDto ToDto(Menu src) => new(
        src.MenuId,
        src.TenantId,
        src.Name,
        src.MenuType,
        src.DisplayOrder,
        src.CreatedAt,
        src.UpdatedAt
    );

    public static MenuItemDto ToItemDto(MenuItem src) => new(
        src.MenuItemId,
        src.MenuId,
        src.Title,
        src.URL,
        src.Icon,
        src.DisplayOrder,
        src.CreatedAt,
        src.UpdatedAt
    );

    public static Menu ToEntity(CreateMenuDto src, string tenantId) => new()
    {
        TenantId = tenantId,
        Name = src.Name,
        MenuType = src.MenuType,
        DisplayOrder = src.DisplayOrder
    };

    public static MenuItem ToItemEntity(CreateMenuItemDto src, int menuId, string tenantId) => new()
    {
        MenuId = menuId,
        TenantId = tenantId,
        Title = src.Title,
        URL = src.URL,
        Icon = src.Icon,
        DisplayOrder = src.DisplayOrder
    };

    public static void ApplyUpdate(UpdateMenuDto src, Menu dest)
    {
        dest.Name = src.Name;
        dest.MenuType = src.MenuType;
        dest.DisplayOrder = src.DisplayOrder;
    }

    public static void ApplyItemUpdate(UpdateMenuItemDto src, MenuItem dest)
    {
        dest.Title = src.Title;
        dest.URL = src.URL;
        dest.Icon = src.Icon;
        dest.DisplayOrder = src.DisplayOrder;
    }
}

public static class SeoMetaMapper
{
    public static SeoMetaDto ToDto(SeoMeta src) => new(
        src.SeoMetaId,
        src.TenantId,
        src.EntityType,
        src.EntityId,
        src.Title,
        src.Description,
        src.Keywords,
        src.Slug,
        src.OgTitle,
        src.OgDescription,
        src.OgImageUrl,
        src.CreatedAt,
        src.UpdatedAt
    );

    public static SeoMeta ToEntity(CreateSeoMetaDto src, string tenantId) => new()
    {
        TenantId = tenantId,
        EntityType = src.EntityType,
        EntityId = src.EntityId,
        Title = src.Title,
        Description = src.Description,
        Keywords = src.Keywords,
        Slug = src.Slug,
        OgTitle = src.OgTitle,
        OgDescription = src.OgDescription,
        OgImageUrl = src.OgImageUrl
    };

    public static void ApplyUpdate(UpdateSeoMetaDto src, SeoMeta dest)
    {
        dest.Title = src.Title;
        dest.Description = src.Description;
        dest.Keywords = src.Keywords;
        dest.Slug = src.Slug;
        dest.OgTitle = src.OgTitle;
        dest.OgDescription = src.OgDescription;
        dest.OgImageUrl = src.OgImageUrl;
    }
}

public static class TagMapper
{
    public static TagDto ToDto(Tag src) => new(
        src.TagId,
        src.TenantId,
        src.TagName,
        src.EntityType,
        src.EntityId,
        src.CreatedAt,
        src.UpdatedAt
    );

    public static Tag ToEntity(CreateTagDto src, string tenantId) => new()
    {
        TenantId = tenantId,
        TagName = src.TagName
    };

    public static void ApplyUpdate(UpdateTagDto src, Tag dest)
    {
        dest.TagName = src.TagName;
    }
}
