using Microsoft.AspNetCore.Mvc;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class WikiController : Controller
{
    public IActionResult Index() => View();
    
    public IActionResult GettingStarted() => View();
    
    public IActionResult Configuration() => View();
    
    public IActionResult Architecture() => View();
    
    public IActionResult Database() => View();
    
    public IActionResult Security() => View();
    
    public IActionResult Api() => View();
}
