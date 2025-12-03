using Microsoft.AspNetCore.Mvc;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class TenantsController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Settings(string tenantId) => View(model: tenantId);
    public IActionResult Branding(string tenantId) => View(model: tenantId);
}
