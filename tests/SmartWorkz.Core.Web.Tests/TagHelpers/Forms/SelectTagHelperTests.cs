using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using SmartWorkz.Core.Web.Services.Components;
using SmartWorkz.Core.Web.TagHelpers.Forms;
using Xunit;

namespace SmartWorkz.Core.Web.Tests.TagHelpers.Forms;

public class SelectTagHelperTests
{
    private readonly TagHelperContext _context;
    private readonly Mock<IFormComponentProvider> _mockFormComponentProvider;

    public SelectTagHelperTests()
    {
        var attributes = new TagHelperAttributeList();
        _context = new TagHelperContext(
            attributes,
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N")
        );
        _mockFormComponentProvider = new Mock<IFormComponentProvider>();

        // Setup default configuration
        var defaultConfig = new FormComponentConfig();
        _mockFormComponentProvider
            .Setup(x => x.GetConfiguration())
            .Returns(defaultConfig);
    }

    private TagHelperOutput CreateSelectOutput()
    {
        var attributes = new TagHelperAttributeList();
        return new TagHelperOutput(
            "select-tag",
            attributes,
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(
                new DefaultTagHelperContent()
            )
        );
    }

    [Fact]
    public void Process_WithItemsList_RendersOptionsFromSelectListItems()
    {
        // Arrange
        var items = new List<SelectListItem>
        {
            new SelectListItem { Text = "Option 1", Value = "value1" },
            new SelectListItem { Text = "Option 2", Value = "value2" },
            new SelectListItem { Text = "Option 3", Value = "value3" }
        };

        var tagHelper = new SelectTagHelper(_mockFormComponentProvider.Object)
        {
            For = "Status",
            Items = items,
            AddBlank = false
        };
        var output = CreateSelectOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.Contains("<select", html);
        Assert.Contains("</select>", html);
        Assert.Contains("<option value=\"value1\">Option 1</option>", html);
        Assert.Contains("<option value=\"value2\">Option 2</option>", html);
        Assert.Contains("<option value=\"value3\">Option 3</option>", html);
    }

    [Fact]
    public void Process_WithEnumType_RendersOptionsFromEnumValues()
    {
        // Arrange
        var tagHelper = new SelectTagHelper(_mockFormComponentProvider.Object)
        {
            For = "Status",
            EnumType = typeof(TestStatus),
            AddBlank = false
        };
        var output = CreateSelectOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.Contains("<select", html);
        Assert.Contains("<option value=\"Active\">Active</option>", html);
        Assert.Contains("<option value=\"Inactive\">Inactive</option>", html);
        Assert.Contains("<option value=\"Pending\">Pending</option>", html);
    }

    [Fact]
    public void Process_AddBlankTrue_IncludesBlankOptionAtBeginning()
    {
        // Arrange
        var items = new List<SelectListItem>
        {
            new SelectListItem { Text = "Option 1", Value = "value1" }
        };

        var tagHelper = new SelectTagHelper(_mockFormComponentProvider.Object)
        {
            For = "Status",
            Items = items,
            AddBlank = true,
            BlankText = "-- Select --"
        };
        var output = CreateSelectOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        // Blank option should come first
        var blankIndex = html.IndexOf("<option value=\"\">-- Select --</option>");
        var optionIndex = html.IndexOf("<option value=\"value1\">");
        Assert.True(blankIndex >= 0, "Blank option should exist");
        Assert.True(optionIndex >= 0, "First option should exist");
        Assert.True(blankIndex < optionIndex, "Blank option should come before other options");
    }

    [Fact]
    public void Process_AddBlankFalse_ExcludesBlankOption()
    {
        // Arrange
        var items = new List<SelectListItem>
        {
            new SelectListItem { Text = "Option 1", Value = "value1" }
        };

        var tagHelper = new SelectTagHelper(_mockFormComponentProvider.Object)
        {
            For = "Status",
            Items = items,
            AddBlank = false
        };
        var output = CreateSelectOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.DoesNotContain("<option value=\"\">", html);
    }

    [Fact]
    public void Process_CustomBlankText_UsesCustomTextForBlankOption()
    {
        // Arrange
        var items = new List<SelectListItem>
        {
            new SelectListItem { Text = "Option 1", Value = "value1" }
        };

        var tagHelper = new SelectTagHelper(_mockFormComponentProvider.Object)
        {
            For = "Status",
            Items = items,
            AddBlank = true,
            BlankText = "Please choose an option"
        };
        var output = CreateSelectOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.Contains("<option value=\"\">Please choose an option</option>", html);
    }

    [Fact]
    public void Process_WithSelectedItem_RendersSelectedAttribute()
    {
        // Arrange
        var items = new List<SelectListItem>
        {
            new SelectListItem { Text = "Option 1", Value = "value1" },
            new SelectListItem { Text = "Option 2", Value = "value2", Selected = true },
            new SelectListItem { Text = "Option 3", Value = "value3" }
        };

        var tagHelper = new SelectTagHelper(_mockFormComponentProvider.Object)
        {
            For = "Status",
            Items = items,
            AddBlank = false
        };
        var output = CreateSelectOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.Contains("<option value=\"value2\" selected>Option 2</option>", html);
    }

    [Fact]
    public void Process_OptionValuesAndText_RenderedCorrectly()
    {
        // Arrange
        var items = new List<SelectListItem>
        {
            new SelectListItem { Text = "First Option", Value = "first" },
            new SelectListItem { Text = "Second Option", Value = "second" }
        };

        var tagHelper = new SelectTagHelper(_mockFormComponentProvider.Object)
        {
            For = "Status",
            Items = items,
            AddBlank = false
        };
        var output = CreateSelectOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.Contains("<option value=\"first\">First Option</option>", html);
        Assert.Contains("<option value=\"second\">Second Option</option>", html);
    }

    [Fact]
    public void Process_AppliesFormComponentConfigInputClass()
    {
        // Arrange
        var config = new FormComponentConfig { InputClass = "form-control custom-select" };
        _mockFormComponentProvider
            .Setup(x => x.GetConfiguration())
            .Returns(config);

        var items = new List<SelectListItem>
        {
            new SelectListItem { Text = "Option 1", Value = "value1" }
        };

        var tagHelper = new SelectTagHelper(_mockFormComponentProvider.Object)
        {
            For = "Status",
            Items = items,
            AddBlank = false
        };
        var output = CreateSelectOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.Contains("class=\"form-control custom-select\"", html);
    }

    [Fact]
    public void Process_GeneratesFieldIdFromForName()
    {
        // Arrange
        var items = new List<SelectListItem>
        {
            new SelectListItem { Text = "Option 1", Value = "value1" }
        };

        var tagHelper = new SelectTagHelper(_mockFormComponentProvider.Object)
        {
            For = "UserStatus",
            Items = items,
            AddBlank = false
        };
        var output = CreateSelectOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        // Field ID should be lowercase
        Assert.Contains("id=\"field_userstatus\"", html);
    }

    [Fact]
    public void Process_EmptyItemsAndNoEnumType_NoOptionsExceptBlank()
    {
        // Arrange
        var tagHelper = new SelectTagHelper(_mockFormComponentProvider.Object)
        {
            For = "Status",
            Items = new List<SelectListItem>(),
            AddBlank = true,
            BlankText = "-- Select --"
        };
        var output = CreateSelectOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        // Should only have the blank option
        Assert.Contains("<option value=\"\">-- Select --</option>", html);
        // Count that there's only one option
        var optionCount = html.Split("<option").Length - 1;
        Assert.Equal(1, optionCount);
    }

    [Fact]
    public void Process_DefaultAddBlankIsTrue()
    {
        // Arrange
        var items = new List<SelectListItem>
        {
            new SelectListItem { Text = "Option 1", Value = "value1" }
        };

        var tagHelper = new SelectTagHelper(_mockFormComponentProvider.Object)
        {
            For = "Status",
            Items = items
            // AddBlank not specified, should default to true
        };
        var output = CreateSelectOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        // Default blank option should be present
        Assert.Contains("<option value=\"\">-- Select --</option>", html);
    }

    [Fact]
    public void Process_BlankOptionHasEmptyStringValue()
    {
        // Arrange
        var items = new List<SelectListItem>
        {
            new SelectListItem { Text = "Option 1", Value = "value1" }
        };

        var tagHelper = new SelectTagHelper(_mockFormComponentProvider.Object)
        {
            For = "Status",
            Items = items,
            AddBlank = true,
            BlankText = "Choose"
        };
        var output = CreateSelectOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        // Blank option must have empty value attribute
        Assert.Contains("<option value=\"\">Choose</option>", html);
    }

}

// Test enum for EnumType testing
public enum TestStatus
{
    Active,
    Inactive,
    Pending
}
