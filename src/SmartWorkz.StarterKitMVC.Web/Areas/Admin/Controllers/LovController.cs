using Microsoft.AspNetCore.Mvc;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class LovController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Categories() => View();
    public IActionResult Items() => View();
}
