using SmartWorkz.Core;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Domain.Entities;
using SmartWorkz.Sample.ECommerce.Domain.Specifications;

namespace SmartWorkz.Sample.ECommerce.Application.Services;

public class ProductService(IRepository<Product, int> repo, SmartWorkz.Shared.IMapper mapper)
    : ServiceBase<Product, ProductDto>(repo)
{
    private readonly SmartWorkz.Shared.IMapper _mapper = mapper;

    protected override ProductDto Map(Product entity) =>
        _mapper.Map<Product, ProductDto>(entity);

    protected override Product MapToEntity(ProductDto dto) =>
        throw new NotImplementedException("Use domain constructors");

    public async Task<IReadOnlyList<ProductDto>> GetByCategoryAsync(int categoryId)
    {
        var spec = new ProductByCategorySpecification(categoryId);
        var products = await repo.FindAllAsync(spec);
        return products.Select(Map).ToList();
    }
}

