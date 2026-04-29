using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Sample.ECommerce.Application.DTOs;

namespace SmartWorkz.Sample.ECommerce.Web.Controllers;

public class DemoController : Controller
{
    // Sample product data for demos
    private static List<ProductDto> GetSampleProducts() => new()
    {
        new ProductDto(1, "Laptop", "laptop", "High-performance laptop with SSD", 999.99m, "USD", 15, true, 1, "Electronics"),
        new ProductDto(2, "Wireless Mouse", "wireless-mouse", "Ergonomic wireless mouse", 29.99m, "USD", 50, true, 1, "Electronics"),
        new ProductDto(3, "Mechanical Keyboard", "mechanical-keyboard", "RGB mechanical keyboard with Cherry switches", 79.99m, "USD", 5, true, 1, "Electronics"),
        new ProductDto(4, "4K Monitor", "4k-monitor", "27-inch 4K monitor (out of stock)", 299.99m, "USD", 0, false, 1, "Electronics"),
        new ProductDto(5, "Wireless Headphones", "wireless-headphones", "Noise-canceling wireless headphones", 149.99m, "USD", 8, true, 1, "Electronics"),
    };

    [HttpGet]
    public IActionResult GridDemo()
    {
        var products = GetSampleProducts();
        return View(products);
    }

    [HttpGet]
    public IActionResult ListViewDemo()
    {
        var products = GetSampleProducts();
        return View(products);
    }
}
