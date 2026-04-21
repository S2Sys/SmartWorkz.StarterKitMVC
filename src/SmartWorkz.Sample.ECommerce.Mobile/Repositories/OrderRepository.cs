namespace SmartWorkz.ECommerce.Mobile.Repositories;

using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.DTOs;

public class OrderRepository
{
    private readonly IApiClient _api;
    private readonly IMobileCacheService _cache;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(IApiClient api, IMobileCacheService cache, ILogger<OrderRepository> logger)
    {
        _api    = Guard.NotNull(api,    nameof(api));
        _cache  = Guard.NotNull(cache,  nameof(cache));
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public virtual async Task<Result<IReadOnlyList<OrderDto>>> GetMyOrdersAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await _api.GetAsync<List<OrderDto>>("/api/orders", ct);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to fetch orders: {Error}", result.Error?.Message);
                return Result.Fail<IReadOnlyList<OrderDto>>(result.Error!);
            }

            var orders = (IReadOnlyList<OrderDto>)(result.Data ?? new List<OrderDto>());
            _logger.LogDebug("Orders retrieved (count: {Count})", orders.Count);
            return Result.Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading orders");
            return Result.Fail<IReadOnlyList<OrderDto>>(Error.FromException(ex, "ORDERS.LOAD_FAILED"));
        }
    }

    public virtual async Task<Result<int>> PlaceOrderAsync(CheckoutDto checkout, CancellationToken ct = default)
    {
        try
        {
            Guard.NotNull(checkout, nameof(checkout));

            var result = await _api.PostAsync<PlaceOrderResponse>("/api/orders", checkout, ct);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to place order: {Error}", result.Error?.Message);
                return Result.Fail<int>(result.Error!);
            }

            // API response should always include OrderId > 0 on success.
            // If missing, log warning but return 0 rather than failing,
            // allowing client to retry or handle gracefully.
            var orderId = result.Data?.OrderId ?? 0;
            if (orderId > 0)
            {
                _logger.LogInformation("Order placed successfully (OrderId: {OrderId})", orderId);
            }
            else
            {
                _logger.LogWarning("Order placement response missing OrderId");
            }

            return Result.Ok(orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error placing order");
            return Result.Fail<int>(Error.FromException(ex, "ORDER.PLACEMENT_FAILED"));
        }
    }
}

internal sealed record PlaceOrderResponse(int OrderId);
