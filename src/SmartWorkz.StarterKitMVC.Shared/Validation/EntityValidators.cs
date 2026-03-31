using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Shared.Validation;

/// <summary>
/// Validation rules for entity DTOs.
/// </summary>
public static class EntityValidators
{
    public static bool IsValidTenantDto(TenantDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.Name)
            && dto.Name.Length <= 256;
    }

    public static bool IsValidProductDto(ProductDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.Name)
            && !string.IsNullOrWhiteSpace(dto.SKU)
            && !string.IsNullOrWhiteSpace(dto.TenantId)
            && dto.Name.Length <= 256
            && dto.SKU.Length <= 50
            && dto.Price >= 0;
    }

    public static bool IsValidCategoryDto(CategoryDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.Name)
            && !string.IsNullOrWhiteSpace(dto.TenantId)
            && dto.Name.Length <= 256;
    }

    public static bool IsValidMenuDto(MenuDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.Name)
            && !string.IsNullOrWhiteSpace(dto.TenantId)
            && dto.Name.Length <= 256;
    }

    public static bool IsValidSeoMetaDto(SeoMetaDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.TenantId)
            && !string.IsNullOrWhiteSpace(dto.EntityType)
            && (!string.IsNullOrWhiteSpace(dto.Title) || !string.IsNullOrWhiteSpace(dto.Description));
    }

    public static bool IsValidTagDto(TagDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.TenantId)
            && !string.IsNullOrWhiteSpace(dto.TagName)
            && !string.IsNullOrWhiteSpace(dto.EntityType)
            && dto.TagName.Length <= 100;
    }

    public static bool IsValidUserProfileDto(UserProfileDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.UserId)
            && !string.IsNullOrWhiteSpace(dto.Email)
            && !string.IsNullOrWhiteSpace(dto.TenantId);
    }
}
