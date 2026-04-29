using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Application.Services;
using SmartWorkz.Sample.ECommerce.Web.Models;

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

        var vm = new CheckoutViewModel { Cart = cart };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Index(CheckoutViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Cart = cartService.GetCart();
            return View(vm);
        }

        var customerId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
        if (customerId == 0)
            return RedirectToAction("Login", "Account");

        var result = await orderService.PlaceOrderAsync(customerId, vm.Cart, vm.Checkout);

        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Error?.Message ?? "Order placement failed");
            vm.Cart = cartService.GetCart();
            return View(vm);
        }

        cartService.ClearCart();
        TempData["ToastMessage"] = "Order placed successfully!";
        return RedirectToAction("Detail", "Order", new { id = result.Data });
    }
}
