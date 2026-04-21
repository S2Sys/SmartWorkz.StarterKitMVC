namespace SmartWorkz.ECommerce.Mobile.Repositories;

using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.DTOs;

public class ProductRepository
{
    private readonly IApiClient      _api;
    private readonly IMobileCacheService _cache;
    private readonly ILogger<ProductRepository> _logger;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public ProductRepository(IApiClient api, IMobileCacheService cache, ILogger<ProductRepository> logger)
    {
        _api    = Guard.NotNull(api,    nameof(api));
        _cache  = Guard.NotNull(cache,  nameof(cache));
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public virtual async Task<Result<IReadOnlyList<ProductDto>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var cached = await _cache.GetAsync<IReadOnlyList<ProductDto>>("products:all", ct);
            if (cached is not null)
            {
                _logger.LogDebug("Products retrieved from cache");
                return Result.Ok(cached);
            }

            var result = await _api.GetAsync<List<ProductDto>>("/api/products", ct);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to fetch products: {Error}", result.Error?.Message);
                return Result.Fail<IReadOnlyList<ProductDto>>(result.Error!);
            }

            var list = (IReadOnlyList<ProductDto>)(result.Data ?? new List<ProductDto>());
            await _cache.SetAsync("products:all", list, CacheTtl, ct);
            _logger.LogDebug("Products cached (count: {Count})", list.Count);
            return Result.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading products");
            return Result.Fail<IReadOnlyList<ProductDto>>(Error.FromException(ex, "PRODUCTS.LOAD_FAILED"));
        }
    }

    public virtual async Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var result = await _api.GetAsync<ProductDto>($"/api/products/{id}", ct);
            return result.Succeeded
                ? Result.Ok(result.Data!)
                : Result.Fail<ProductDto>(result.Error!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load product {ProductId}", id);
            return Result.Fail<ProductDto>(Error.FromException(ex, "PRODUCTS.LOAD_FAILED"));
        }
    }
}
