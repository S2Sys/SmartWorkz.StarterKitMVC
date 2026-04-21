namespace SmartWorkz.Sample.ECommerce.Web.ApiControllers;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Core;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Application.Services;
using SmartWorkz.Sample.ECommerce.Domain.Entities;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class OrdersController(
    OrderService orderService,
    CartService cartService,
    IRepository<Order, int> orderRepo,
    SmartWorkz.Shared.IMapper mapper) : ControllerBase
{
    private int GetCurrentCustomerId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return int.TryParse(sub, out var id) ? id : 0;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OrderDto>>>> GetMyOrders()
    {
        var customerId = GetCurrentCustomerId();
        if (customerId == 0) return Unauthorized();

        var allOrders = await orderRepo.GetAllAsync();
        var myOrders = allOrders
            .Where(o => o.CustomerId == customerId)
            .Select(o => mapper.Map<Order, OrderDto>(o))
            .ToList()
            .AsReadOnly();

        return Ok(ApiResponse<IReadOnlyList<OrderDto>>.Ok(myOrders));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<int>>> PlaceOrder(
        [FromBody] CheckoutDto checkout)
    {
        var customerId = GetCurrentCustomerId();
        if (customerId == 0) return Unauthorized();

        var cart = cartService.GetCart();
        if (!cart.Items.Any())
            return BadRequest(ApiResponse<int>.Fail(
                new ApiError("Order.CartEmpty", "Cart is empty")));

        var result = await orderService.PlaceOrderAsync(customerId, cart, checkout);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<int>.Fail(
                ApiError.FromError(result.Error!)));

        cartService.ClearCart();
        return CreatedAtAction(nameof(GetOrderById), new { id = result.Data },
            ApiResponse<int>.Ok(result.Data));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrderById(int id)
    {
        var customerId = GetCurrentCustomerId();
        if (customerId == 0) return Unauthorized();

        var order = await orderRepo.GetByIdAsync(id);
        if (order == null)
            return NotFound(ApiResponse<OrderDto>.Fail(
                ApiError.FromError(Error.NotFound("Order", id))));

        if (order.CustomerId != customerId)
            return StatusCode(403, ApiResponse<OrderDto>.Fail(
                ApiError.FromError(Error.Unauthorized("You do not have access to this order"))));

        var dto = mapper.Map<Order, OrderDto>(order);
        return Ok(ApiResponse<OrderDto>.Ok(dto));
    }
}
