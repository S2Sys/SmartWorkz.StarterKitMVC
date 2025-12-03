using Microsoft.AspNetCore.Mvc;

namespace SmartWorkz.StarterKitMVC.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
