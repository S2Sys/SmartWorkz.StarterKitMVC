using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Web.TagHelpers.Navigation;
using Xunit;

namespace SmartWorkz.Core.Web.Tests.TagHelpers.Navigation;

public class BreadcrumbTagHelperTests
{
    private readonly TagHelperContext _context;

    public BreadcrumbTagHelperTests()
    {
        var attributes = new TagHelperAttributeList();
        _context = new TagHelperContext(
            attributes,
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N")
        );
    }

    private TagHelperOutput CreateBreadcrumbOutput()
    {
        var attributes = new TagHelperAttributeList();
        return new TagHelperOutput(
            "breadcrumb",
            attributes,
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(
                new DefaultTagHelperContent()
            )
        );
    }

    [Fact]
    public void Process_WithEmptyItemsList_SuppressesOutput()
    {
        // Arrange
        var tagHelper = new BreadcrumbTagHelper { Items = new() };
        var output = CreateBreadcrumbOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.True(output.IsContentModified);
        Assert.Empty(output.Content.GetContent());
    }

    [Fact]
    public void Process_WithSingleItem_RendersWithActiveClass()
    {
        // Arrange
        var tagHelper = new BreadcrumbTagHelper
        {
            Items = new() { new BreadcrumbItem { Label = "Home" } }
        };
        var output = CreateBreadcrumbOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains("breadcrumb-item active", html);
        Assert.Contains("Home", html);
        Assert.DoesNotContain("<a href", html);
    }

    [Fact]
    public void Process_WithMultipleItems_RendersCorrectActiveStates()
    {
        // Arrange
        var tagHelper = new BreadcrumbTagHelper
        {
            Items = new()
            {
                new BreadcrumbItem { Label = "Home", Url = "/" },
                new BreadcrumbItem { Label = "Products", Url = "/products" },
                new BreadcrumbItem { Label = "Widget" }
            }
        };
        var output = CreateBreadcrumbOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var html = output.Content.GetContent();

        // Check non-active items have links
        Assert.Contains(@"<a href=""/"">Home</a>", html);
        Assert.Contains(@"<a href=""/products"">Products</a>", html);

        // Check active item doesn't have link
        Assert.Contains(@"<li class=""breadcrumb-item active"" aria-current=""page"">Widget</li>", html);
    }

    [Fact]
    public void Process_WithMultipleItems_LastItemHasAriaCurrent()
    {
        // Arrange
        var tagHelper = new BreadcrumbTagHelper
        {
            Items = new()
            {
                new BreadcrumbItem { Label = "Home", Url = "/" },
                new BreadcrumbItem { Label = "Current Page" }
            }
        };
        var output = CreateBreadcrumbOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains(@"aria-current=""page""", html);
        var lastItemMatch = html.Substring(html.LastIndexOf("<li"));
        Assert.Contains("aria-current=\"page\"", lastItemMatch);
    }

    [Fact]
    public void Process_WithNonLastItems_RendersAsLinks()
    {
        // Arrange
        var tagHelper = new BreadcrumbTagHelper
        {
            Items = new()
            {
                new BreadcrumbItem { Label = "Home", Url = "/" },
                new BreadcrumbItem { Label = "About", Url = "/about" },
                new BreadcrumbItem { Label = "Team" }
            }
        };
        var output = CreateBreadcrumbOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var html = output.Content.GetContent();

        // Non-last items should have links
        var homeLink = html.Contains(@"<li class=""breadcrumb-item""><a href=""/"">Home</a></li>");
        var aboutLink = html.Contains(@"<li class=""breadcrumb-item""><a href=""/about"">About</a></li>");

        Assert.True(homeLink, "Home should have a link");
        Assert.True(aboutLink, "About should have a link");
    }

    [Fact]
    public void Process_WithItems_UsesBreadcrumbItemUrlProperty()
    {
        // Arrange
        var tagHelper = new BreadcrumbTagHelper
        {
            Items = new()
            {
                new BreadcrumbItem { Label = "Dashboard", Url = "/dashboard" },
                new BreadcrumbItem { Label = "Settings" }
            }
        };
        var output = CreateBreadcrumbOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains(@"href=""/dashboard""", html);
    }

    [Fact]
    public void Process_WithItems_RendersLabelsCorrectly()
    {
        // Arrange
        var tagHelper = new BreadcrumbTagHelper
        {
            Items = new()
            {
                new BreadcrumbItem { Label = "First", Url = "/first" },
                new BreadcrumbItem { Label = "Second", Url = "/second" },
                new BreadcrumbItem { Label = "Third" }
            }
        };
        var output = CreateBreadcrumbOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains("First", html);
        Assert.Contains("Second", html);
        Assert.Contains("Third", html);
    }

    [Fact]
    public void Process_WithItems_IncludesNavigationWithAriaLabel()
    {
        // Arrange
        var tagHelper = new BreadcrumbTagHelper
        {
            Items = new() { new BreadcrumbItem { Label = "Home" } }
        };
        var output = CreateBreadcrumbOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains(@"<nav aria-label=""breadcrumb"">", html);
        Assert.Contains("</nav>", html);
    }

    [Fact]
    public void Process_WithItems_UsesOrderedListForBreadcrumbs()
    {
        // Arrange
        var tagHelper = new BreadcrumbTagHelper
        {
            Items = new()
            {
                new BreadcrumbItem { Label = "Home", Url = "/" },
                new BreadcrumbItem { Label = "Current" }
            }
        };
        var output = CreateBreadcrumbOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains("<ol class=\"breadcrumb\">", html);
        Assert.Contains("</ol>", html);
    }
}

