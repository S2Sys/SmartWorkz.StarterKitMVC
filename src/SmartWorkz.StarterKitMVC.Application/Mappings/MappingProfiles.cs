using AutoMapper;
using SmartWorkz.StarterKitMVC.Domain.Entities.Master;
using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;
using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Application.Mappings;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // Tenant Mappings
        CreateMap<Tenant, TenantDto>().ReverseMap();
        CreateMap<CreateTenantDto, Tenant>();
        CreateMap<UpdateTenantDto, Tenant>();

        // Product Mappings
        CreateMap<Product, ProductDto>().ReverseMap();
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
            .ForMember(dest => dest.SKU, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        CreateMap<Product, ProductSearchResultDto>();

        // Category Mappings
        CreateMap<Category, CategoryDto>().ReverseMap();
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        CreateMap<Category, CategoryTreeDto>()
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.ChildCategories));

        // Menu Mappings
        CreateMap<Menu, MenuDto>().ReverseMap();
        CreateMap<CreateMenuDto, Menu>();
        CreateMap<UpdateMenuDto, Menu>()
            .ForMember(dest => dest.MenuId, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        // MenuItem Mappings
        CreateMap<MenuItem, MenuItemDto>().ReverseMap();
        CreateMap<CreateMenuItemDto, MenuItem>();
        CreateMap<UpdateMenuItemDto, MenuItem>()
            .ForMember(dest => dest.MenuItemId, opt => opt.Ignore())
            .ForMember(dest => dest.MenuId, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        // SeoMeta Mappings
        CreateMap<SeoMeta, SeoMetaDto>().ReverseMap();
        CreateMap<CreateSeoMetaDto, SeoMeta>();
        CreateMap<UpdateSeoMetaDto, SeoMeta>()
            .ForMember(dest => dest.SeoMetaId, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.EntityType, opt => opt.Ignore())
            .ForMember(dest => dest.EntityId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        // Tag Mappings
        CreateMap<Tag, TagDto>().ReverseMap();
        CreateMap<CreateTagDto, Tag>();
        CreateMap<UpdateTagDto, Tag>()
            .ForMember(dest => dest.TagId, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.EntityType, opt => opt.Ignore())
            .ForMember(dest => dest.EntityId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }
}
