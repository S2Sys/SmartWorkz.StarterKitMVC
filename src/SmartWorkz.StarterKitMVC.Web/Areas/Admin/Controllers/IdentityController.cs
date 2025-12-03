using Microsoft.AspNetCore.Mvc;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class IdentityController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Users() => View();
    public IActionResult Roles() => View();
    public IActionResult Claims() => View();
}
