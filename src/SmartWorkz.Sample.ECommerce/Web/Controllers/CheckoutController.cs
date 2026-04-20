using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Application.Services;

namespace SmartWorkz.Sample.ECommerce.Web.Controllers;

public class CheckoutController(
    CartService cartService,
    OrderService orderService,
    IHttpContextAccessor httpContextAccessor) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        if (!User.Identity!.IsAuthenticated)
            return RedirectToAction("Login", "Account");

        var cart = cartService.GetCart();
        if (cart.Items.Count == 0)
            return RedirectToAction("Index", "Cart");

        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(CheckoutDto checkout)
    {
        var customerId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
        if (customerId == 0)
            return RedirectToAction("Login", "Account");

        var cart = cartService.GetCart();
        var result = await orderService.PlaceOrderAsync(customerId, cart, checkout);

        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Error?.Message ?? "Order placement failed");
            return View(nameof(Index), cart);
        }

        cartService.ClearCart();
        return RedirectToAction("Detail", "Order", new { id = result.Data });
    }
}
