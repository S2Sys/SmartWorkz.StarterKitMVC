using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Application.Services;

namespace SmartWorkz.Sample.ECommerce.Web.Controllers;

public class AccountController(ECommerceAuthService authService) : Controller
{
    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await authService.RegisterAsync(dto);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Error?.Message ?? "Registration failed");
            return View();
        }

        Response.Cookies.Append("ecommerce_auth", result.Data!, new Microsoft.AspNetCore.Http.CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var result = await authService.LoginAsync(email, password);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Invalid email or password");
            return View();
        }

        Response.Cookies.Append("ecommerce_auth", result.Data!, new Microsoft.AspNetCore.Http.CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("ecommerce_auth");
        return RedirectToAction("Index", "Home");
    }
}
