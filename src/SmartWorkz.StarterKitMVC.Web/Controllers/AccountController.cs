using Microsoft.AspNetCore.Mvc;

namespace SmartWorkz.StarterKitMVC.Web.Controllers;

public class AccountController : Controller
{
    public IActionResult Login() => View();
    public IActionResult Register() => View();
    public IActionResult Profile() => View();
}
