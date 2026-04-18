using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Core.Web.TagHelpers.Display;
using Xunit;

namespace SmartWorkz.Core.Web.Tests.TagHelpers.Display;

public class BadgeTagHelperTests
{
    private readonly TagHelperContext _context;

    public BadgeTagHelperTests()
    {
        var attributes = new TagHelperAttributeList();
        _context = new TagHelperContext(
            attributes,
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N")
        );
    }

    private TagHelperOutput CreateBadgeOutput()
    {
        var attributes = new TagHelperAttributeList();
        return new TagHelperOutput(
            "badge",
            attributes,
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(
                new DefaultTagHelperContent()
            )
        );
    }

    [Fact]
    public void Process_WithTypePrimary_RendersBgPrimaryClass()
    {
        // Arrange
        var tagHelper = new BadgeTagHelper
        {
            Type = "primary",
            Text = "Primary Badge"
        };
        var output = CreateBadgeOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("bg-primary", content);
        Assert.Contains("Primary Badge", content);
        Assert.Contains("<span class=\"badge", content);
    }

    [Fact]
    public void Process_WithTypeSuccess_RendersBgSuccessClass()
    {
        // Arrange
        var tagHelper = new BadgeTagHelper
        {
            Type = "success",
            Text = "Success Badge"
        };
        var output = CreateBadgeOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("bg-success", content);
        Assert.Contains("Success Badge", content);
    }

    [Fact]
    public void Process_WithTypeDanger_RendersBgDangerClass()
    {
        // Arrange
        var tagHelper = new BadgeTagHelper
        {
            Type = "danger",
            Text = "Danger Badge"
        };
        var output = CreateBadgeOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("bg-danger", content);
        Assert.Contains("Danger Badge", content);
    }

    [Fact]
    public void Process_WithTypeWarning_RendersBgWarningClass()
    {
        // Arrange
        var tagHelper = new BadgeTagHelper
        {
            Type = "warning",
            Text = "Warning Badge"
        };
        var output = CreateBadgeOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("bg-warning", content);
        Assert.Contains("Warning Badge", content);
    }

    [Fact]
    public void Process_WithTypeInfo_RendersBgInfoClass()
    {
        // Arrange
        var tagHelper = new BadgeTagHelper
        {
            Type = "info",
            Text = "Info Badge"
        };
        var output = CreateBadgeOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("bg-info", content);
        Assert.Contains("Info Badge", content);
    }

    [Fact]
    public void Process_WithTypeLight_RendersBgLightAndTextDarkClasses()
    {
        // Arrange
        var tagHelper = new BadgeTagHelper
        {
            Type = "light",
            Text = "Light Badge"
        };
        var output = CreateBadgeOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("bg-light", content);
        Assert.Contains("text-dark", content);
        Assert.Contains("Light Badge", content);
    }

    [Fact]
    public void Process_WithTypeDark_RendersBgDarkClass()
    {
        // Arrange
        var tagHelper = new BadgeTagHelper
        {
            Type = "dark",
            Text = "Dark Badge"
        };
        var output = CreateBadgeOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("bg-dark", content);
        Assert.Contains("Dark Badge", content);
    }

    [Fact]
    public void Process_WithDefaultType_RendersBgSecondaryClass()
    {
        // Arrange
        var tagHelper = new BadgeTagHelper
        {
            Text = "Default Badge"
        };
        var output = CreateBadgeOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("bg-secondary", content);
        Assert.Contains("Default Badge", content);
    }

    [Fact]
    public void Process_WithUnknownType_DefaultsToBgSecondaryClass()
    {
        // Arrange
        var tagHelper = new BadgeTagHelper
        {
            Type = "unknown",
            Text = "Unknown Badge"
        };
        var output = CreateBadgeOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("bg-secondary", content);
        Assert.Contains("Unknown Badge", content);
    }

    [Fact]
    public void Process_WithText_RendersTextInSpan()
    {
        // Arrange
        var tagHelper = new BadgeTagHelper
        {
            Type = "primary",
            Text = "Badge with text"
        };
        var output = CreateBadgeOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("Badge with text", content);
        Assert.Contains("<span class=\"badge", content);
    }

    [Fact]
    public void Process_WithNullText_RendersEmptySpan()
    {
        // Arrange
        var tagHelper = new BadgeTagHelper
        {
            Type = "primary",
            Text = null
        };
        var output = CreateBadgeOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("<span class=\"badge bg-primary\"></span>", content);
    }
}
