using SmartWorkz.Core;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Application.Requests;
using SmartWorkz.Sample.ECommerce.Application.Validators;
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
        var products = await Repository.FindAllAsync(spec);
        return products.Select(Map).ToList();
    }

    public async Task<SmartWorkz.Shared.Result<ProductDto>> CreateProductAsync(
        CreateProductRequest request,
        CreateProductValidator validator,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Failures.First();
            return SmartWorkz.Shared.Result.Fail<ProductDto>(new SmartWorkz.Shared.Error("Validation.Failed", firstError.Message));
        }

        var priceResult = Money.Create(request.Price, request.Currency);
        if (!priceResult.Succeeded)
            return SmartWorkz.Shared.Result.Fail<ProductDto>(priceResult.Error!);

        var product = new Product
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            Price = priceResult.Data!,
            Stock = request.Stock,
            IsActive = request.IsActive,
            CategoryId = request.CategoryId
        };

        await Repository.AddAsync(product, cancellationToken);
        var created = await Repository.GetByIdAsync(product.Id, cancellationToken);
        return SmartWorkz.Shared.Result.Ok(Map(created!));
    }

    public async Task<SmartWorkz.Shared.Result<ProductDto>> UpdateProductAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await Repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
            return SmartWorkz.Shared.Result.Fail<ProductDto>(SmartWorkz.Shared.Error.NotFound("Product", id));

        if (request.Name is not null)
            product.Name = request.Name;

        if (request.Slug is not null)
            product.Slug = request.Slug;

        if (request.Description is not null)
            product.Description = request.Description;

        if (request.Stock.HasValue)
            product.Stock = request.Stock.Value;

        if (request.IsActive.HasValue)
            product.IsActive = request.IsActive.Value;

        if (request.CategoryId.HasValue)
            product.CategoryId = request.CategoryId.Value;

        if (request.Price.HasValue)
        {
            var currency = request.Currency ?? product.Price?.Currency ?? "USD";
            var priceResult = Money.Create(request.Price.Value, currency);
            if (!priceResult.Succeeded)
                return SmartWorkz.Shared.Result.Fail<ProductDto>(priceResult.Error!);
            product.Price = priceResult.Data!;
        }

        await Repository.UpdateAsync(product, cancellationToken);
        var updated = await Repository.GetByIdAsync(id, cancellationToken);
        return SmartWorkz.Shared.Result.Ok(Map(updated!));
    }
}

