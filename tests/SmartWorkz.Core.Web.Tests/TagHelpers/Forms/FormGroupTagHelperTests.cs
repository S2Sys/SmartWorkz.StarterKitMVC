using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using SmartWorkz.Core.Web.Services.Components;
using SmartWorkz.Core.Web.TagHelpers.Forms;
using Xunit;

namespace SmartWorkz.Core.Web.Tests.TagHelpers.Forms;

public class FormGroupTagHelperTests
{
    private readonly TagHelperContext _context;
    private readonly Mock<IFormComponentProvider> _mockFormComponentProvider;
    private readonly Mock<IAccessibilityService> _mockAccessibilityService;

    public FormGroupTagHelperTests()
    {
        var attributes = new TagHelperAttributeList();
        _context = new TagHelperContext(
            attributes,
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N")
        );
        _mockFormComponentProvider = new Mock<IFormComponentProvider>();
        _mockAccessibilityService = new Mock<IAccessibilityService>();

        // Setup default configuration
        var defaultConfig = new FormComponentConfig();
        _mockFormComponentProvider
            .Setup(x => x.GetConfiguration())
            .Returns(defaultConfig);

        // Setup accessibility service defaults
        _mockAccessibilityService
            .Setup(x => x.GenerateFieldId(It.IsAny<string>()))
            .Returns<string>(name => $"field_{name}");

        _mockAccessibilityService
            .Setup(x => x.GenerateHintId(It.IsAny<string>()))
            .Returns<string>(name => $"hint_{name}");
    }

    private TagHelperOutput CreateFormGroupOutput(string childContent = "")
    {
        var attributes = new TagHelperAttributeList();
        var content = new DefaultTagHelperContent();
        content.SetHtmlContent(childContent);

        return new TagHelperOutput(
            "form-group",
            attributes,
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(content)
        );
    }

    [Fact]
    public void Process_WithLabel_RendersLabelWithCorrectForAttribute()
    {
        // Arrange
        var output = CreateFormGroupOutput("<input type=\"text\" />");

        var tagHelper = new FormGroupTagHelper(
            _mockFormComponentProvider.Object,
            _mockAccessibilityService.Object
        )
        {
            Label = "Email Address"
        };

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var html = output.Content.GetContent();
        Assert.Contains("<label", html);
        Assert.Contains("for=\"field_Email Address\"", html);
        Assert.Contains("Email Address", html);
    }

    [Fact]
    public void Process_WithRequiredTrue_AddRedAsteriskToLabel()
    {
        // Arrange
        var output = CreateFormGroupOutput("<input type=\"text\" />");

        var tagHelper = new FormGroupTagHelper(
            _mockFormComponentProvider.Object,
            _mockAccessibilityService.Object
        )
        {
            Label = "Name",
            Required = true
        };

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains("<span class=\"text-danger\">*</span>", html);
    }

    [Fact]
    public void Process_WithHelpText_RendersHelpTextInSmallTag()
    {
        // Arrange
        var output = CreateFormGroupOutput("<input type=\"text\" />");

        var tagHelper = new FormGroupTagHelper(
            _mockFormComponentProvider.Object,
            _mockAccessibilityService.Object
        )
        {
            Label = "Username",
            HelpText = "Enter your unique username"
        };

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains("<small", html);
        Assert.Contains("id=\"hint_Username\"", html);
        Assert.Contains("class=\"form-text text-muted\"", html);
        Assert.Contains("Enter your unique username", html);
    }

    [Fact]
    public void Process_WithLabelAndHelpText_RendersBoth()
    {
        // Arrange
        var output = CreateFormGroupOutput("<input type=\"text\" />");

        var tagHelper = new FormGroupTagHelper(
            _mockFormComponentProvider.Object,
            _mockAccessibilityService.Object
        )
        {
            Label = "Phone Number",
            HelpText = "Use format: (123) 456-7890"
        };

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains("<label", html);
        Assert.Contains("Phone Number", html);
        Assert.Contains("<small", html);
        Assert.Contains("Use format: (123) 456-7890", html);
    }

    [Fact]
    public void Process_WithoutLabel_DoesNotRenderLabelElement()
    {
        // Arrange
        var output = CreateFormGroupOutput("<input type=\"text\" />");

        var tagHelper = new FormGroupTagHelper(
            _mockFormComponentProvider.Object,
            _mockAccessibilityService.Object
        );

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.DoesNotContain("<label", html);
    }

    [Fact]
    public void Process_WithoutHelpText_DoesNotRenderHelpText()
    {
        // Arrange
        var output = CreateFormGroupOutput("<input type=\"text\" />");

        var tagHelper = new FormGroupTagHelper(
            _mockFormComponentProvider.Object,
            _mockAccessibilityService.Object
        )
        {
            Label = "Field"
        };

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.DoesNotContain("<small", html);
    }

    [Fact]
    public void Process_AppliesFormGroupClassFromConfig()
    {
        // Arrange
        var output = CreateFormGroupOutput("<input type=\"text\" />");

        var tagHelper = new FormGroupTagHelper(
            _mockFormComponentProvider.Object,
            _mockAccessibilityService.Object
        );

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains("class=\"mb-3\"", html);
        Assert.StartsWith("<div", html);
    }

    [Fact]
    public void Process_GeneratesFieldIdFromForName()
    {
        // Arrange
        var output = CreateFormGroupOutput("<input type=\"text\" />");

        var tagHelper = new FormGroupTagHelper(
            _mockFormComponentProvider.Object,
            _mockAccessibilityService.Object
        )
        {
            Label = "Username"
        };

        // Act
        tagHelper.Process(_context, output);

        // Assert
        _mockAccessibilityService.Verify(
            x => x.GenerateFieldId("Username"),
            Times.Once
        );
    }

    [Fact]
    public void Process_WrapperDivContainsLabelBodyContentAndHelpText()
    {
        // Arrange
        var output = CreateFormGroupOutput("<input type=\"text\" id=\"field_username\" />");

        var tagHelper = new FormGroupTagHelper(
            _mockFormComponentProvider.Object,
            _mockAccessibilityService.Object
        )
        {
            Label = "Username",
            HelpText = "Choose a unique username"
        };

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var html = output.Content.GetContent();

        // Check for div wrapper
        Assert.StartsWith("<div", html);
        Assert.EndsWith("</div>", html);

        // Check order: label should come before input
        var labelIndex = html.IndexOf("<label");
        var inputIndex = html.IndexOf("<input");
        Assert.True(labelIndex < inputIndex, "Label should appear before input");

        // Check order: input should come before help text
        var smallIndex = html.IndexOf("<small");
        Assert.True(inputIndex < smallIndex, "Input should appear before help text");
    }

    [Fact]
    public void Process_RequiredFalse_DoesNotAddAsterisk()
    {
        // Arrange
        var output = CreateFormGroupOutput("<input type=\"text\" />");

        var tagHelper = new FormGroupTagHelper(
            _mockFormComponentProvider.Object,
            _mockAccessibilityService.Object
        )
        {
            Label = "Optional Field",
            Required = false
        };

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.DoesNotContain("<span class=\"text-danger\">*</span>", html);
    }
}
