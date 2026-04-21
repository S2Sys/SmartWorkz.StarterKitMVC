using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Application.Services;

namespace SmartWorkz.Sample.ECommerce.Web.Api;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersApiController : ControllerBase
{
    private readonly OrderService _orders;
    private readonly CartService _cart;

    public OrdersApiController(OrderService orders, CartService cart)
    {
        _orders = orders;
        _cart   = cart;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        var customerIdStr = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(customerIdStr, out var customerId)) return Unauthorized();

        var result = await _orders.GetByCustomerAsync(customerId);
        if (!result.Succeeded) return BadRequest(result.Error?.Message);
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] CheckoutDto checkout)
    {
        var customerIdStr = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(customerIdStr, out var customerId)) return Unauthorized();

        var cart = _cart.GetCart();
        var result = await _orders.PlaceOrderAsync(customerId, cart, checkout);
        if (!result.Succeeded) return BadRequest(result.Error?.Message);
        return Ok(new { OrderId = result.Data });
    }
}
