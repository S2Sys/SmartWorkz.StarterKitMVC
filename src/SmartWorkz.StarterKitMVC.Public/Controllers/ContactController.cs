using Microsoft.AspNetCore.Mvc;

namespace SmartWorkz.StarterKitMVC.Public.Controllers;

/// <summary>
/// Contact form controller
/// </summary>
[Route("[controller]")]
public class ContactController : Controller
{
    private readonly ILogger<ContactController> _logger;

    public ContactController(ILogger<ContactController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Contact form page
    /// </summary>
    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index()
    {
        _logger.LogInformation("Contact page accessed");

        var model = new ContactViewModel();
        return View(model);
    }

    /// <summary>
    /// Process contact form submission
    /// </summary>
    [HttpPost("Index")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _logger.LogInformation("Contact form submitted by: {Email}, subject: {Subject}", model.Email, model.Subject);

        // TODO: Implement email sending logic to admin
        // TODO: Implement spam protection (reCAPTCHA, rate limiting)

        TempData["SuccessMessage"] = "Thank you for your message. We'll get back to you soon!";
        return RedirectToAction(nameof(Index));
    }
}

public class ContactViewModel
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
}
