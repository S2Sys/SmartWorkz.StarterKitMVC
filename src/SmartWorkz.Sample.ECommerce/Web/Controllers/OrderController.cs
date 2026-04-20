using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Core.Abstractions;
using SmartWorkz.Sample.ECommerce.Domain.Entities;

namespace SmartWorkz.Sample.ECommerce.Web.Controllers;

public class OrderController(IRepository<Order, int> orderRepo) : Controller
{
    [HttpGet]
    public async Task<IActionResult> History()
    {
        var customerId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
        if (customerId == 0)
            return RedirectToAction("Login", "Account");

        var orders = await orderRepo.GetAllAsync();
        var customerOrders = orders.Where(o => o.CustomerId == customerId).ToList();
        return View(customerOrders);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var order = await orderRepo.GetByIdAsync(id);
        if (order == null || (User.Identity!.IsAuthenticated && order.CustomerId != int.Parse(User.FindFirst("sub")?.Value ?? "0")))
            return NotFound();

        return View(order);
    }
}
