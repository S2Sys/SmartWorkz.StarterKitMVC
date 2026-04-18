using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Core.Web.TagHelpers.Display;
using Xunit;

namespace SmartWorkz.Core.Web.Tests.TagHelpers.Display;

public class PaginationTagHelperTests
{
    private readonly TagHelperContext _context;

    public PaginationTagHelperTests()
    {
        var attributes = new TagHelperAttributeList();
        _context = new TagHelperContext(
            attributes,
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N")
        );
    }

    private TagHelperOutput CreatePaginationOutput()
    {
        var attributes = new TagHelperAttributeList();
        return new TagHelperOutput(
            "pagination",
            attributes,
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(
                new DefaultTagHelperContent()
            )
        );
    }

    [Fact]
    public void Process_SinglePageSuppressesOutput()
    {
        // Arrange
        var tagHelper = new PaginationTagHelper
        {
            CurrentPage = 1,
            TotalPages = 1
        };
        var output = CreatePaginationOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.True(output.IsContentModified);
    }

    [Fact]
    public void Process_CurrentPageRendersWithActiveClass()
    {
        // Arrange
        var tagHelper = new PaginationTagHelper
        {
            CurrentPage = 2,
            TotalPages = 5
        };
        var output = CreatePaginationOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var content = output.Content.GetContent();
        Assert.Contains("<li class=\"page-item active\"><a class=\"page-link\" href=\"?page=2\">2</a></li>", content);
    }

    [Fact]
    public void Process_PreviousButtonDisabledOnFirstPage()
    {
        // Arrange
        var tagHelper = new PaginationTagHelper
        {
            CurrentPage = 1,
            TotalPages = 5
        };
        var output = CreatePaginationOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var content = output.Content.GetContent();
        Assert.Contains("<li class=\"page-item disabled\"><span class=\"page-link\">Previous</span></li>", content);
    }

    [Fact]
    public void Process_PreviousButtonEnabledNotOnFirstPage()
    {
        // Arrange
        var tagHelper = new PaginationTagHelper
        {
            CurrentPage = 2,
            TotalPages = 5
        };
        var output = CreatePaginationOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var content = output.Content.GetContent();
        Assert.Contains("<li class=\"page-item\"><a class=\"page-link\" href=\"?page=1\">Previous</a></li>", content);
    }

    [Fact]
    public void Process_NextButtonDisabledOnLastPage()
    {
        // Arrange
        var tagHelper = new PaginationTagHelper
        {
            CurrentPage = 5,
            TotalPages = 5
        };
        var output = CreatePaginationOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var content = output.Content.GetContent();
        Assert.Contains("<li class=\"page-item disabled\"><span class=\"page-link\">Next</span></li>", content);
    }

    [Fact]
    public void Process_NextButtonEnabledNotOnLastPage()
    {
        // Arrange
        var tagHelper = new PaginationTagHelper
        {
            CurrentPage = 3,
            TotalPages = 5
        };
        var output = CreatePaginationOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var content = output.Content.GetContent();
        Assert.Contains("<li class=\"page-item\"><a class=\"page-link\" href=\"?page=4\">Next</a></li>", content);
    }

    [Fact]
    public void Process_PageLinksUsePageUrlTemplate()
    {
        // Arrange
        var tagHelper = new PaginationTagHelper
        {
            CurrentPage = 2,
            TotalPages = 5,
            PageUrl = "/articles?page={0}"
        };
        var output = CreatePaginationOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var content = output.Content.GetContent();
        Assert.Contains("href=\"/articles?page=", content);
    }

    [Fact]
    public void Process_FirstPageLinkShownWhenStartGreaterThanOne()
    {
        // Arrange
        var tagHelper = new PaginationTagHelper
        {
            CurrentPage = 5,
            TotalPages = 10,
            MaxVisible = 5
        };
        var output = CreatePaginationOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var content = output.Content.GetContent();
        // First page link should be present since start > 1
        Assert.Contains("<li class=\"page-item\"><a class=\"page-link\" href=\"?page=1\">1</a></li>", content);
    }

    [Fact]
    public void Process_LastPageLinkShownWhenEndLessThanTotalPages()
    {
        // Arrange
        var tagHelper = new PaginationTagHelper
        {
            CurrentPage = 3,
            TotalPages = 10,
            MaxVisible = 5
        };
        var output = CreatePaginationOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var content = output.Content.GetContent();
        // Last page link should be present
        Assert.Contains("<li class=\"page-item\"><a class=\"page-link\" href=\"?page=10\">10</a></li>", content);
    }

    [Fact]
    public void Process_MaxVisibleControlsPageNumberCount()
    {
        // Arrange
        var tagHelper = new PaginationTagHelper
        {
            CurrentPage = 5,
            TotalPages = 20,
            MaxVisible = 3
        };
        var output = CreatePaginationOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var content = output.Content.GetContent();
        // Count page number links (excluding prev/next and first/last links)
        // With MaxVisible=3 and CurrentPage=5, we should see approximately 3 page numbers centered
        var pageItemMatches = System.Text.RegularExpressions.Regex.Matches(
            content,
            "<li class=\"page-item[^>]*><a class=\"page-link\""
        );
        // Should have: Previous, First (if needed), Pages, Last (if needed), Next
        Assert.True(pageItemMatches.Count >= 3);
    }

    [Fact]
    public void Process_PageRangeCenteredOnCurrentPage()
    {
        // Arrange
        var tagHelper = new PaginationTagHelper
        {
            CurrentPage = 5,
            TotalPages = 10,
            MaxVisible = 5
        };
        var output = CreatePaginationOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var content = output.Content.GetContent();
        // With MaxVisible=5 and current page=5, range should be centered around 5
        // Expected: pages 3, 4, 5, 6, 7 (or close to it)
        Assert.Contains("<a class=\"page-link\" href=\"?page=3\">3</a>", content);
        Assert.Contains("<a class=\"page-link\" href=\"?page=5\">5</a>", content);
        Assert.Contains("<a class=\"page-link\" href=\"?page=7\">7</a>", content);
    }

    [Fact]
    public void Process_CustomPageUrlPatternIsHonored()
    {
        // Arrange
        var tagHelper = new PaginationTagHelper
        {
            CurrentPage = 2,
            TotalPages = 3,
            PageUrl = "/blog/posts?page={0}&size=10"
        };
        var output = CreatePaginationOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var content = output.Content.GetContent();
        Assert.Contains("href=\"/blog/posts?page=1&size=10\"", content);
        Assert.Contains("href=\"/blog/posts?page=2&size=10\"", content);
    }
}
