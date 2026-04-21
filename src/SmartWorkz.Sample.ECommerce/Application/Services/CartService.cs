using Microsoft.AspNetCore.Http;
using SmartWorkz.Core;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Domain.Entities;
using System.Text.Json;

namespace SmartWorkz.Sample.ECommerce.Application.Services;

public class CartService(IHttpContextAccessor httpContextAccessor, IRepository<Product, int> productRepo)
{
    private const string CartKey = "ecommerce_cart";
    private ISession Session => httpContextAccessor.HttpContext!.Session;

    public CartDto GetCart()
    {
        var json = Session.GetString(CartKey);
        if (json == null) return new CartDto(new List<CartItemDto>());
        return JsonSerializer.Deserialize<CartDto>(json) ?? new CartDto(new List<CartItemDto>());
    }

    public async Task AddToCartAsync(int productId, int quantity = 1)
    {
        var product = await productRepo.GetByIdAsync(productId);
        if (product == null) return;
        var cart = GetCart();
        var existing = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        List<CartItemDto> items = cart.Items.ToList();
        if (existing != null)
            items = items.Select(i => i.ProductId == productId
                ? i with { Quantity = i.Quantity + quantity } : i).ToList();
        else
            items.Add(new CartItemDto(product.Id, product.Name, product.Slug, quantity, product.Price?.Amount ?? 0));
        SaveCart(new CartDto(items));
    }

    public void RemoveFromCart(int productId)
    {
        var cart = GetCart();
        SaveCart(new CartDto(cart.Items.Where(i => i.ProductId != productId).ToList()));
    }

    public void ClearCart() => Session.Remove(CartKey);

    private void SaveCart(CartDto cart) =>
        Session.SetString(CartKey, JsonSerializer.Serialize(cart));
}
