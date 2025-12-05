using Microsoft.AspNetCore.Mvc;

namespace SmartWorkz.StarterKitMVC.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
    
    public IActionResult About() => View();
    
    public IActionResult Features() => View();
    
    public IActionResult Contact() => View();
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Contact(string name, string email, string subject, string message)
    {
        // TODO: Implement contact form submission
        TempData["Success"] = "Thank you for your message! We'll get back to you soon.";
        return RedirectToAction(nameof(Contact));
    }
    
    public IActionResult Privacy() => View();
    
    public IActionResult Terms() => View();
    
    public IActionResult Search(string q)
    {
        ViewBag.Query = q;
        return View();
    }
    
    [Route("/error/{code?}")]
    public IActionResult Error(int? code)
    {
        ViewBag.ErrorCode = code ?? 500;
        ViewBag.ErrorMessage = code switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Page Not Found",
            500 => "Internal Server Error",
            503 => "Service Unavailable",
            _ => "An Error Occurred"
        };
        return View();
    }
}
