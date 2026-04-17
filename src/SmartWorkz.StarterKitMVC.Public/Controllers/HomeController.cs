using Microsoft.AspNetCore.Mvc;

namespace SmartWorkz.StarterKitMVC.Public.Controllers;

/// <summary>
/// Home page controller for public website
/// </summary>
[Route("[controller]")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Homepage with hero section and features
    /// </summary>
    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index()
    {
        _logger.LogInformation("Homepage accessed");

        var model = new HomeViewModel
        {
            HeroTitle = "Welcome to SmartWorkz",
            HeroSubtitle = "Build amazing applications with modern technologies",
            HeroImage = "/images/hero-bg.jpg",
            Features = new List<FeatureViewModel>
            {
                new() { Icon = "fa-bolt", Title = "Fast & Reliable", Description = "Built on .NET for high performance and reliability" },
                new() { Icon = "fa-shield-alt", Title = "Secure", Description = "Enterprise-grade security with role-based access control" },
                new() { Icon = "fa-code", Title = "Open Source", Description = "Fully customizable and extensible architecture" },
                new() { Icon = "fa-mobile-alt", Title = "Responsive", Description = "Works seamlessly on all devices and screen sizes" }
            },
            CallToActionText = "Get Started Today",
            CallToActionUrl = "/authentication/register"
        };

        return View(model);
    }

    /// <summary>
    /// About page
    /// </summary>
    [HttpGet("About")]
    public IActionResult About()
    {
        _logger.LogInformation("About page accessed");

        var model = new AboutViewModel
        {
            CompanyName = "SmartWorkz",
            Description = "SmartWorkz is a modern web application starter kit built with ASP.NET Core MVC.",
            Mission = "To provide developers with a production-ready foundation for building scalable web applications.",
            Vision = "To be the go-to starter kit for enterprise-level web applications.",
            TeamMembers = new List<TeamMemberViewModel>
            {
                new() { Name = "John Doe", Title = "Founder & Lead Developer", Image = "/images/team/john.jpg" },
                new() { Name = "Jane Smith", Title = "Product Manager", Image = "/images/team/jane.jpg" },
                new() { Name = "Mike Johnson", Title = "DevOps Engineer", Image = "/images/team/mike.jpg" }
            }
        };

        return View(model);
    }

    /// <summary>
    /// Privacy policy page
    /// </summary>
    [HttpGet("Privacy")]
    public IActionResult Privacy()
    {
        _logger.LogInformation("Privacy policy accessed");

        var model = new PolicyPageViewModel
        {
            Title = "Privacy Policy",
            LastUpdated = DateTime.UtcNow.AddMonths(-3),
            Content = "Our privacy policy outlines how we collect, use, and protect your personal information..."
        };

        return View(model);
    }

    /// <summary>
    /// Terms of service page
    /// </summary>
    [HttpGet("Terms")]
    public IActionResult Terms()
    {
        _logger.LogInformation("Terms of service accessed");

        var model = new PolicyPageViewModel
        {
            Title = "Terms of Service",
            LastUpdated = DateTime.UtcNow.AddMonths(-6),
            Content = "These terms of service govern your use of our application and services..."
        };

        return View(model);
    }

    /// <summary>
    /// Error page
    /// </summary>
    [HttpGet("Error")]
    public IActionResult Error(int? statusCode)
    {
        var model = new ErrorPageViewModel
        {
            StatusCode = statusCode ?? 500,
            Message = statusCode switch
            {
                404 => "Page not found. The page you're looking for doesn't exist.",
                403 => "Access denied. You don't have permission to access this resource.",
                500 => "Server error. Something went wrong on our end.",
                _ => "An unexpected error occurred. Please try again later."
            }
        };

        return View(model);
    }
}

public class HomeViewModel
{
    public string HeroTitle { get; set; }
    public string HeroSubtitle { get; set; }
    public string HeroImage { get; set; }
    public List<FeatureViewModel> Features { get; set; } = new();
    public string CallToActionText { get; set; }
    public string CallToActionUrl { get; set; }
}

public class FeatureViewModel
{
    public string Icon { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}

public class AboutViewModel
{
    public string CompanyName { get; set; }
    public string Description { get; set; }
    public string Mission { get; set; }
    public string Vision { get; set; }
    public List<TeamMemberViewModel> TeamMembers { get; set; } = new();
}

public class TeamMemberViewModel
{
    public string Name { get; set; }
    public string Title { get; set; }
    public string Image { get; set; }
}

public class PolicyPageViewModel
{
    public string Title { get; set; }
    public DateTime LastUpdated { get; set; }
    public string Content { get; set; }
}

public class ErrorPageViewModel
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
}
