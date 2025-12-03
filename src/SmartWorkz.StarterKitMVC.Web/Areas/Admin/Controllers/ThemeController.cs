using Microsoft.AspNetCore.Mvc;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class ThemeController : Controller
{
    public IActionResult Index() => View();
}
