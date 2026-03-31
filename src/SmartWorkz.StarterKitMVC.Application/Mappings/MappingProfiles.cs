using SmartWorkz.StarterKitMVC.Domain.Entities.Master;
using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;
using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Application.Mappings;

public static class TenantMapper
{
    public static TenantDto ToDto(Tenant src) => new()
    {
        TenantId = src.TenantId,
        Name = src.Name,
        Slug = src.Slug,
        Description = src.Description,
        IsActive = src.IsActive,
        CreatedAt = src.CreatedAt,
        UpdatedAt = src.UpdatedAt
    };

    public static Tenant ToEntity(CreateTenantDto src) => new()
    {
        Name = src.Name,
        Slug = src.Slug,
        Description = src.Description
    };

    public static void ApplyUpdate(UpdateTenantDto src, Tenant dest)
    {
        dest.Name = src.Name;
        dest.Slug = src.Slug;
        dest.Description = src.Description;
        dest.IsActive = src.IsActive;
    }
}

public static class ProductMapper
{
    public static ProductDto ToDto(Product src) => new()
    {
        ProductId = src.ProductId,
        TenantId = src.TenantId,
        CategoryId = src.CategoryId,
        Name = src.Name,
        Description = src.Description,
        SKU = src.SKU,
        Slug = src.Slug,
        Price = src.Price,
        StockQuantity = src.StockQuantity,
        IsFeatured = src.IsFeatured,
        CreatedAt = src.CreatedAt,
        UpdatedAt = src.UpdatedAt
    };

    public static ProductSearchResultDto ToSearchResult(Product src) => new()
    {
        ProductId = src.ProductId,
        Name = src.Name,
        Slug = src.Slug,
        Price = src.Price,
        IsFeatured = src.IsFeatured
    };

    public static Product ToEntity(CreateProductDto src, string tenantId) => new()
    {
        TenantId = tenantId,
        CategoryId = src.CategoryId,
        Name = src.Name,
        Description = src.Description,
        SKU = src.SKU,
        Slug = src.Slug,
        Price = src.Price,
        StockQuantity = src.StockQuantity,
        IsFeatured = src.IsFeatured
    };

    public static void ApplyUpdate(UpdateProductDto src, Product dest)
    {
        dest.Name = src.Name;
        dest.Description = src.Description;
        dest.Price = src.Price;
        dest.StockQuantity = src.StockQuantity;
        dest.Slug = src.Slug;
        dest.IsFeatured = src.IsFeatured;
    }
}

public static class CategoryMapper
{
    public static CategoryDto ToDto(Category src) => new()
    {
        CategoryId = src.CategoryId,
        TenantId = src.TenantId,
        Name = src.Name,
        Description = src.Description,
        Slug = src.Slug,
        ParentCategoryId = src.ParentCategoryId,
        CreatedAt = src.CreatedAt,
        UpdatedAt = src.UpdatedAt
    };

    public static CategoryTreeDto ToTreeDto(Category src) => new()
    {
        CategoryId = src.CategoryId,
        Name = src.Name,
        Slug = src.Slug,
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
    public static MenuDto ToDto(Menu src) => new()
    {
        MenuId = src.MenuId,
        TenantId = src.TenantId,
        Name = src.Name,
        Slug = src.Slug,
        CreatedAt = src.CreatedAt,
        UpdatedAt = src.UpdatedAt
    };

    public static MenuItemDto ToItemDto(MenuItem src) => new()
    {
        MenuItemId = src.MenuItemId,
        MenuId = src.MenuId,
        Label = src.Label,
        Url = src.Url,
        DisplayOrder = src.DisplayOrder,
        CreatedAt = src.CreatedAt,
        UpdatedAt = src.UpdatedAt
    };

    public static Menu ToEntity(CreateMenuDto src, string tenantId) => new()
    {
        TenantId = tenantId,
        Name = src.Name,
        Slug = src.Slug
    };

    public static MenuItem ToItemEntity(CreateMenuItemDto src, int menuId, string tenantId) => new()
    {
        MenuId = menuId,
        TenantId = tenantId,
        Label = src.Label,
        Url = src.Url,
        DisplayOrder = src.DisplayOrder
    };

    public static void ApplyUpdate(UpdateMenuDto src, Menu dest)
    {
        dest.Name = src.Name;
        dest.Slug = src.Slug;
    }

    public static void ApplyItemUpdate(UpdateMenuItemDto src, MenuItem dest)
    {
        dest.Label = src.Label;
        dest.Url = src.Url;
        dest.DisplayOrder = src.DisplayOrder;
    }
}

public static class SeoMetaMapper
{
    public static SeoMetaDto ToDto(SeoMeta src) => new()
    {
        SeoMetaId = src.SeoMetaId,
        TenantId = src.TenantId,
        EntityType = src.EntityType,
        EntityId = src.EntityId,
        Title = src.Title,
        Description = src.Description,
        Keywords = src.Keywords,
        Slug = src.Slug,
        OgImage = src.OgImage,
        CreatedAt = src.CreatedAt,
        UpdatedAt = src.UpdatedAt
    };

    public static SeoMeta ToEntity(CreateSeoMetaDto src, string tenantId) => new()
    {
        TenantId = tenantId,
        EntityType = src.EntityType,
        EntityId = src.EntityId,
        Title = src.Title,
        Description = src.Description,
        Keywords = src.Keywords,
        Slug = src.Slug,
        OgImage = src.OgImage
    };

    public static void ApplyUpdate(UpdateSeoMetaDto src, SeoMeta dest)
    {
        dest.Title = src.Title;
        dest.Description = src.Description;
        dest.Keywords = src.Keywords;
        dest.Slug = src.Slug;
        dest.OgImage = src.OgImage;
    }
}

public static class TagMapper
{
    public static TagDto ToDto(Tag src) => new()
    {
        TagId = src.TagId,
        TenantId = src.TenantId,
        Name = src.Name,
        Description = src.Description,
        EntityType = src.EntityType,
        EntityId = src.EntityId,
        CreatedAt = src.CreatedAt,
        UpdatedAt = src.UpdatedAt
    };

    public static Tag ToEntity(CreateTagDto src, string tenantId) => new()
    {
        TenantId = tenantId,
        Name = src.Name,
        Description = src.Description
    };

    public static void ApplyUpdate(UpdateTagDto src, Tag dest)
    {
        dest.Name = src.Name;
        dest.Description = src.Description;
    }
}
