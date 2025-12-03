using Microsoft.AspNetCore.Mvc;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class NotificationsController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Templates() => View();
}
