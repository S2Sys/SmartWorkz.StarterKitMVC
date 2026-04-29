using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Core;
using SmartWorkz.Sample.ECommerce.Application.Services;
using SmartWorkz.Sample.ECommerce.Domain.Entities;

namespace SmartWorkz.Sample.ECommerce.Web.Controllers;

public class CartController(CartService cartService, IRepository<Product, int> productRepository) : Controller
{
    public IActionResult Index()
    {
        var cart = cartService.GetCart();
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        var product = await productRepository.GetByIdAsync(productId);
        if (product != null)
        {
            await cartService.AddToCartAsync(productId, quantity);
            TempData["ToastMessage"] = $"'{product.Name}' added to cart!";
        }
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
