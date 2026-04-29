using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Core;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Domain.Entities;

namespace SmartWorkz.Sample.ECommerce.Web.Controllers;

public class OrderController(IRepository<Order, int> orderRepo, IMapper mapper) : Controller
{
    [HttpGet]
    public async Task<IActionResult> History()
    {
        var customerId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
        if (customerId == 0)
            return RedirectToAction("Login", "Account");

        var orders = await orderRepo.GetAllAsync();
        var customerOrders = orders.Where(o => o.CustomerId == customerId).ToList();
        var dtos = customerOrders.Select(o => mapper.Map<Order, OrderDto>(o)).ToList();
        return View(dtos);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var order = await orderRepo.GetByIdAsync(id);
        if (order == null || (User.Identity!.IsAuthenticated && order.CustomerId != int.Parse(User.FindFirst("sub")?.Value ?? "0")))
            return NotFound();

        var dto = mapper.Map<Order, OrderDto>(order);
        return View(dto);
    }
}
