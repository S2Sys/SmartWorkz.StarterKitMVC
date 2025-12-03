using Microsoft.AspNetCore.Mvc;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class SettingsController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Categories() => View();
    public IActionResult Edit(string key) => View(model: key);
}
