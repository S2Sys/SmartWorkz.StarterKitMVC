using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using SmartWorkz.Web.Services.Components;
using SmartWorkz.Web.TagHelpers.Forms;
using Xunit;

namespace SmartWorkz.Core.Web.Tests.TagHelpers.Forms;

public class InputTagHelperTests
{
    private readonly TagHelperContext _context;
    private readonly Mock<IFormComponentProvider> _mockFormComponentProvider;
    private readonly Mock<IIconProvider> _mockIconProvider;

    public InputTagHelperTests()
    {
        var attributes = new TagHelperAttributeList();
        _context = new TagHelperContext(
            attributes,
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N")
        );
        _mockFormComponentProvider = new Mock<IFormComponentProvider>();
        _mockIconProvider = new Mock<IIconProvider>();

        // Setup default configuration
        var defaultConfig = new FormComponentConfig();
        _mockFormComponentProvider
            .Setup(x => x.GetConfiguration())
            .Returns(defaultConfig);
    }

    private TagHelperOutput CreateInputOutput()
    {
        var attributes = new TagHelperAttributeList();
        return new TagHelperOutput(
            "input-tag",
            attributes,
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(
                new DefaultTagHelperContent()
            )
        );
    }


    [Fact]
    public void Process_WithTypeText_RendersTextInput()
    {
        // Arrange
        var tagHelper = new InputTagHelper(
            _mockFormComponentProvider.Object,
            _mockIconProvider.Object
        )
        {
            For = nameof(TestModel.Name),
            Type = "text"
        };
        var output = CreateInputOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.Contains("type=\"text\"", html);
        Assert.Contains("id=\"field_name\"", html);
    }

    [Fact]
    public void Process_WithTypeEmail_RendersEmailInput()
    {
        // Arrange
        var tagHelper = new InputTagHelper(
            _mockFormComponentProvider.Object,
            _mockIconProvider.Object
        )
        {
            For = nameof(TestModel.Email),
            Type = "email"
        };
        var output = CreateInputOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.Contains("type=\"email\"", html);
        Assert.Contains("id=\"field_email\"", html);
    }

    [Fact]
    public void Process_WithTypeNumber_RendersNumberInput()
    {
        // Arrange
        var tagHelper = new InputTagHelper(
            _mockFormComponentProvider.Object,
            _mockIconProvider.Object
        )
        {
            For = nameof(TestModel.Age),
            Type = "number"
        };
        var output = CreateInputOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.Contains("type=\"number\"", html);
        Assert.Contains("id=\"field_age\"", html);
    }

    [Fact]
    public void Process_WithPlaceholder_IncludesPlaceholderAttribute()
    {
        // Arrange
        var tagHelper = new InputTagHelper(
            _mockFormComponentProvider.Object,
            _mockIconProvider.Object
        )
        {
            For = nameof(TestModel.Name),
            Placeholder = "Enter your name"
        };
        var output = CreateInputOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.Contains("placeholder=\"Enter your name\"", html);
    }

    [Fact]
    public void Process_WhenRequiredTrue_IncludesRequiredAttribute()
    {
        // Arrange
        var tagHelper = new InputTagHelper(
            _mockFormComponentProvider.Object,
            _mockIconProvider.Object
        )
        {
            For = nameof(TestModel.Name),
            Required = true
        };
        var output = CreateInputOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.Contains("required", html);
    }

    [Fact]
    public void Process_WithIconPrefix_RendersInputGroupWrapperWithIconBefore()
    {
        // Arrange
        var iconHtml = "<i class=\"bi bi-search\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Search, It.IsAny<string?>()))
            .Returns(iconHtml);

        var tagHelper = new InputTagHelper(
            _mockFormComponentProvider.Object,
            _mockIconProvider.Object
        )
        {
            For = nameof(TestModel.Name),
            IconPrefix = IconType.Search
        };
        var output = CreateInputOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.Contains("<div class=\"input-group\">", html);
        Assert.Contains("<span class=\"input-group-text\">", html);
        Assert.Contains(iconHtml, html);
        Assert.Contains("</div>", html);
        _mockIconProvider.Verify(x => x.GetIconHtml(IconType.Search, It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public void Process_WithIconSuffix_RendersInputGroupWrapperWithIconAfter()
    {
        // Arrange
        var iconHtml = "<i class=\"bi bi-calendar\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Info, It.IsAny<string?>()))
            .Returns(iconHtml);

        var tagHelper = new InputTagHelper(
            _mockFormComponentProvider.Object,
            _mockIconProvider.Object
        )
        {
            For = nameof(TestModel.Name),
            IconSuffix = IconType.Info
        };
        var output = CreateInputOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.Contains("<div class=\"input-group\">", html);
        Assert.Contains("<span class=\"input-group-text\">", html);
        Assert.Contains(iconHtml, html);
        Assert.Contains("</div>", html);
        _mockIconProvider.Verify(x => x.GetIconHtml(IconType.Info, It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public void Process_WithBothIconPrefixAndSuffix_RendersWrapperWithIconsOnBothSides()
    {
        // Arrange
        var prefixIconHtml = "<i class=\"bi bi-search\"></i>";
        var suffixIconHtml = "<i class=\"bi bi-x\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Search, It.IsAny<string?>()))
            .Returns(prefixIconHtml);
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Close, It.IsAny<string?>()))
            .Returns(suffixIconHtml);

        var tagHelper = new InputTagHelper(
            _mockFormComponentProvider.Object,
            _mockIconProvider.Object
        )
        {
            For = nameof(TestModel.Name),
            IconPrefix = IconType.Search,
            IconSuffix = IconType.Close
        };
        var output = CreateInputOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.Contains("<div class=\"input-group\">", html);
        Assert.Contains(prefixIconHtml, html);
        Assert.Contains(suffixIconHtml, html);
        Assert.Contains("</div>", html);
        // Verify prefix icon comes before input
        var prefixIndex = html.IndexOf(prefixIconHtml);
        var inputIndex = html.IndexOf("type=\"text\"");
        Assert.True(prefixIndex < inputIndex);
    }

    [Fact]
    public void Process_AppliesFormComponentConfigInputClass()
    {
        // Arrange
        var config = new FormComponentConfig { InputClass = "form-control custom-class" };
        _mockFormComponentProvider
            .Setup(x => x.GetConfiguration())
            .Returns(config);

        var tagHelper = new InputTagHelper(
            _mockFormComponentProvider.Object,
            _mockIconProvider.Object
        )
        {
            For = nameof(TestModel.Name)
        };
        var output = CreateInputOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.Contains("class=\"form-control custom-class\"", html);
    }

    [Fact]
    public void Process_GeneratesFieldIdFromForName()
    {
        // Arrange
        var tagHelper = new InputTagHelper(
            _mockFormComponentProvider.Object,
            _mockIconProvider.Object
        )
        {
            For = nameof(TestModel.FirstName)
        };
        var output = CreateInputOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        // Field ID should be lowercase
        Assert.Contains("id=\"field_firstname\"", html);
    }

    [Fact]
    public void Process_WithoutIconsAndWithoutPlaceholder_RendersSimpleInput()
    {
        // Arrange
        var tagHelper = new InputTagHelper(
            _mockFormComponentProvider.Object,
            _mockIconProvider.Object
        )
        {
            For = nameof(TestModel.Name),
            Type = "text"
        };
        var output = CreateInputOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        // Should be plain input without input-group wrapper
        Assert.StartsWith("<input", html);
        Assert.EndsWith("/>", html);
        Assert.DoesNotContain("<div", html);
    }

    [Fact]
    public void Process_DefaultTypeIsText()
    {
        // Arrange
        var tagHelper = new InputTagHelper(
            _mockFormComponentProvider.Object,
            _mockIconProvider.Object
        )
        {
            For = nameof(TestModel.Name)
            // Type not specified, should default to "text"
        };
        var output = CreateInputOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.Contains("type=\"text\"", html);
    }
}

// Test model for ModelExpression
public class TestModel
{
    public string Name { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string Email { get; set; } = "";
    public int Age { get; set; }
}

