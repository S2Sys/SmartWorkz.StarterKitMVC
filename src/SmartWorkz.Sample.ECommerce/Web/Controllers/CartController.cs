using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Sample.ECommerce.Application.Services;

namespace SmartWorkz.Sample.ECommerce.Web.Controllers;

public class CartController(CartService cartService) : Controller
{
    public IActionResult Index()
    {
        var cart = cartService.GetCart();
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        await cartService.AddToCartAsync(productId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Remove(int productId)
    {
        cartService.RemoveFromCart(productId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Clear()
    {
        cartService.ClearCart();
        return RedirectToAction(nameof(Index));
    }
}
