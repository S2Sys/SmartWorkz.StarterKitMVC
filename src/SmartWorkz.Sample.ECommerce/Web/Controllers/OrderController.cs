using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Core;
using SmartWorkz.Shared;
using SmartWorkz.Core.External.Export;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Domain.Entities;
using SmartWorkz.Sample.ECommerce.Web.Models;

namespace SmartWorkz.Sample.ECommerce.Web.Controllers;

public class OrderController(
    IRepository<Order, int> orderRepo,
    IMapper mapper,
    IExcelExportService excelExport,
    IPdfExportService pdfExport,
    IExportService csvExport) : Controller
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

    [HttpGet]
    public async Task<IActionResult> ExportExcel()
    {
        var customerId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
        if (customerId == 0)
            return RedirectToAction("Login", "Account");

        var orders = await orderRepo.GetAllAsync();
        var customerOrders = orders.Where(o => o.CustomerId == customerId).ToList();
        var exportData = customerOrders.Select(o => new OrderExportDto(
            o.Id, o.CustomerId, o.Status.ToString(), o.Total?.Amount ?? 0m, o.Total?.Currency ?? "USD",
            o.PlacedAt, o.Items?.Count ?? 0)).ToList();
        var result = await excelExport.ExportAsync(exportData, "Orders");
        if (!result.Succeeded) return BadRequest(result.Error);
        return File(result.Data!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "orders.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf()
    {
        var customerId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
        if (customerId == 0)
            return RedirectToAction("Login", "Account");

        var orders = await orderRepo.GetAllAsync();
        var customerOrders = orders.Where(o => o.CustomerId == customerId).ToList();
        var exportData = customerOrders.Select(o => new OrderExportDto(
            o.Id, o.CustomerId, o.Status.ToString(), o.Total?.Amount ?? 0m, o.Total?.Currency ?? "USD",
            o.PlacedAt, o.Items?.Count ?? 0)).ToList();
        var result = await pdfExport.ExportAsync(exportData, "My Orders");
        if (!result.Succeeded) return BadRequest(result.Error);
        return File(result.Data!, "application/pdf", "orders.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv()
    {
        var customerId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
        if (customerId == 0)
            return RedirectToAction("Login", "Account");

        var orders = await orderRepo.GetAllAsync();
        var customerOrders = orders.Where(o => o.CustomerId == customerId).ToList();
        var exportData = customerOrders.Select(o => new OrderExportDto(
            o.Id, o.CustomerId, o.Status.ToString(), o.Total?.Amount ?? 0m, o.Total?.Currency ?? "USD",
            o.PlacedAt, o.Items?.Count ?? 0)).ToList();
        var result = await csvExport.ExportAsync(exportData);
        if (!result.Succeeded) return BadRequest(result.Error);
        return File(result.Data!, "text/csv", "orders.csv");
    }
}
