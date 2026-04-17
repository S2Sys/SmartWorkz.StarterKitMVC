using Microsoft.AspNetCore.Mvc;

namespace SmartWorkz.StarterKitMVC.Public.Controllers;

/// <summary>
/// Blog controller for public blog viewing
/// </summary>
[Route("[controller]")]
public class BlogController : Controller
{
    private readonly ILogger<BlogController> _logger;
    private const int PageSize = 10;

    public BlogController(ILogger<BlogController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Blog listing page with pagination
    /// </summary>
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(int page = 1, string tag = null)
    {
        _logger.LogInformation("Blog list accessed, page {Page}, tag {Tag}", page, tag);

        var model = new BlogListViewModel
        {
            CurrentPage = page,
            PageSize = PageSize,
            TotalItems = 42,
            SelectedTag = tag,
            Posts = new List<BlogPostViewModel>
            {
                new() { Id = Guid.NewGuid(), Title = "Getting Started with ASP.NET Core", Slug = "getting-started-aspnetcore", Excerpt = "Learn the basics of ASP.NET Core development...", Author = "John Doe", PublishedAt = DateTime.UtcNow.AddDays(-5), Views = 1250, Tags = "aspnetcore,dotnet" },
                new() { Id = Guid.NewGuid(), Title = "Best Practices for Database Design", Slug = "db-design-best-practices", Excerpt = "Essential tips for designing scalable databases...", Author = "Jane Smith", PublishedAt = DateTime.UtcNow.AddDays(-10), Views = 890, Tags = "database,sql" },
                new() { Id = Guid.NewGuid(), Title = "Advanced LINQ Patterns", Slug = "advanced-linq-patterns", Excerpt = "Master complex LINQ queries and patterns...", Author = "John Doe", PublishedAt = DateTime.UtcNow.AddDays(-15), Views = 620, Tags = "csharp,linq" }
            },
            PopularTags = new[] { "aspnetcore", "csharp", "dotnet", "database", "sql", "linq" }
        };

        return View(model);
    }

    /// <summary>
    /// Single blog post view
    /// </summary>
    [HttpGet("Post/{slug}")]
    public async Task<IActionResult> Post(string slug)
    {
        _logger.LogInformation("Blog post accessed: {Slug}", slug);

        var model = new BlogPostDetailViewModel
        {
            Id = Guid.NewGuid(),
            Title = "Getting Started with ASP.NET Core",
            Slug = slug,
            Content = @"
                <h2>Introduction</h2>
                <p>ASP.NET Core is a modern, cross-platform framework for building web applications...</p>
                <h2>Key Features</h2>
                <ul>
                    <li>High performance</li>
                    <li>Cross-platform support</li>
                    <li>Built-in dependency injection</li>
                    <li>Unified programming model</li>
                </ul>
                <h2>Getting Started</h2>
                <p>To get started with ASP.NET Core, you'll need to install the .NET SDK...</p>
            ",
            Excerpt = "Learn the basics of ASP.NET Core development and get started with building modern web applications.",
            Author = "John Doe",
            AuthorImage = "/images/authors/john.jpg",
            PublishedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-3),
            Views = 1250,
            Tags = new[] { "aspnetcore", "dotnet", "tutorial" },
            FeaturedImage = "/images/blog/aspnetcore.jpg",
            RelatedPosts = new List<RelatedBlogPostViewModel>
            {
                new() { Title = "ASP.NET Core Advanced Patterns", Slug = "aspnetcore-advanced-patterns", PublishedAt = DateTime.UtcNow.AddDays(-20) },
                new() { Title = "Entity Framework Core Tips", Slug = "ef-core-tips", PublishedAt = DateTime.UtcNow.AddDays(-25) }
            }
        };

        return View(model);
    }

    /// <summary>
    /// Blog search results
    /// </summary>
    [HttpGet("Search")]
    public async Task<IActionResult> Search(string query, int page = 1)
    {
        _logger.LogInformation("Blog search: {Query}, page {Page}", query, page);

        if (string.IsNullOrWhiteSpace(query))
            return RedirectToAction(nameof(Index));

        var model = new BlogSearchResultsViewModel
        {
            Query = query,
            CurrentPage = page,
            PageSize = PageSize,
            TotalResults = 5,
            Results = new List<BlogPostViewModel>
            {
                new() { Id = Guid.NewGuid(), Title = "Advanced LINQ Patterns", Slug = "advanced-linq-patterns", Excerpt = "Master complex LINQ queries...", Author = "John Doe", PublishedAt = DateTime.UtcNow.AddDays(-15), Views = 620, Tags = "csharp,linq" }
            }
        };

        return View(model);
    }
}

public class BlogListViewModel
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public string SelectedTag { get; set; }
    public List<BlogPostViewModel> Posts { get; set; } = new();
    public string[] PopularTags { get; set; }

    public int TotalPages => (TotalItems + PageSize - 1) / PageSize;
}

public class BlogPostViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Excerpt { get; set; }
    public string Author { get; set; }
    public DateTime PublishedAt { get; set; }
    public int Views { get; set; }
    public string Tags { get; set; }
}

public class BlogPostDetailViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Content { get; set; }
    public string Excerpt { get; set; }
    public string Author { get; set; }
    public string AuthorImage { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int Views { get; set; }
    public string[] Tags { get; set; }
    public string FeaturedImage { get; set; }
    public List<RelatedBlogPostViewModel> RelatedPosts { get; set; } = new();
}

public class RelatedBlogPostViewModel
{
    public string Title { get; set; }
    public string Slug { get; set; }
    public DateTime PublishedAt { get; set; }
}

public class BlogSearchResultsViewModel
{
    public string Query { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalResults { get; set; }
    public List<BlogPostViewModel> Results { get; set; } = new();

    public int TotalPages => (TotalResults + PageSize - 1) / PageSize;
}
