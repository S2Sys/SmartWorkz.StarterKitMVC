using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Core.Web.TagHelpers.Common;
using Xunit;

namespace SmartWorkz.Core.Web.Tests.TagHelpers.Common;

public class ButtonTagHelperTests
{
    private readonly TagHelperContext _context;

    public ButtonTagHelperTests()
    {
        var attributes = new TagHelperAttributeList();
        _context = new TagHelperContext(
            attributes,
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N")
        );
    }

    private TagHelperOutput CreateButtonOutput()
    {
        var attributes = new TagHelperAttributeList();
        return new TagHelperOutput(
            "button",
            attributes,
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(
                new DefaultTagHelperContent()
            )
        );
    }

    private TagHelperOutput CreateAnchorOutput()
    {
        var attributes = new TagHelperAttributeList();
        return new TagHelperOutput(
            "a",
            attributes,
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(
                new DefaultTagHelperContent()
            )
        );
    }

    [Fact]
    public void Process_WithButtonElementAndPrimaryVariant_AddsBtnAndBtnPrimaryClasses()
    {
        // Arrange
        var tagHelper = new ButtonTagHelper { Variant = "primary" };
        var output = CreateButtonOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var classAttribute = output.Attributes["class"];
        var classValue = classAttribute.Value.ToString();
        Assert.Contains("btn", classValue);
        Assert.Contains("btn-primary", classValue);
    }

    [Fact]
    public void Process_WithAnchorElementAndDangerVariant_AddsBtnAndBtnDangerClasses()
    {
        // Arrange
        var tagHelper = new ButtonTagHelper { Variant = "danger" };
        var output = CreateAnchorOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var classAttribute = output.Attributes["class"];
        var classValue = classAttribute.Value.ToString();
        Assert.Contains("btn", classValue);
        Assert.Contains("btn-danger", classValue);
    }

    [Fact]
    public void Process_WhenVariantNotSpecified_DefaultsToSecondary()
    {
        // Arrange
        var tagHelper = new ButtonTagHelper();
        var output = CreateButtonOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var classAttribute = output.Attributes["class"];
        var classValue = classAttribute.Value.ToString();
        Assert.Contains("btn-secondary", classValue);
    }

    [Fact]
    public void Process_WithSizeSmall_AddsBtnSmClass()
    {
        // Arrange
        var tagHelper = new ButtonTagHelper { Size = "sm" };
        var output = CreateButtonOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var classAttribute = output.Attributes["class"];
        var classValue = classAttribute.Value.ToString();
        Assert.Contains("btn-sm", classValue);
    }

    [Fact]
    public void Process_WithSizeLarge_AddsBtnLgClass()
    {
        // Arrange
        var tagHelper = new ButtonTagHelper { Size = "lg" };
        var output = CreateButtonOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var classAttribute = output.Attributes["class"];
        var classValue = classAttribute.Value.ToString();
        Assert.Contains("btn-lg", classValue);
    }

    [Fact]
    public void Process_WhenIsLoadingTrue_AddsDisabledClassAndAttribute()
    {
        // Arrange
        var tagHelper = new ButtonTagHelper { IsLoading = true };
        var output = CreateButtonOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var classAttribute = output.Attributes["class"];
        var classValue = classAttribute.Value.ToString();
        Assert.Contains("disabled", classValue);

        var disabledAttribute = output.Attributes["disabled"];
        Assert.NotNull(disabledAttribute);
        Assert.Equal("disabled", disabledAttribute.Value.ToString());
    }

    [Fact]
    public void Process_WithExistingClass_PreservesAndAppends()
    {
        // Arrange
        var tagHelper = new ButtonTagHelper { Variant = "primary" };
        var output = CreateButtonOutput();
        output.Attributes.SetAttribute("class", "custom-class");

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var classAttribute = output.Attributes["class"];
        var classValue = classAttribute.Value.ToString();
        Assert.Contains("custom-class", classValue);
        Assert.Contains("btn", classValue);
        Assert.Contains("btn-primary", classValue);
    }

    [Fact]
    public void Process_WithUnknownVariant_DefaultsToSecondary()
    {
        // Arrange
        var tagHelper = new ButtonTagHelper { Variant = "unknown-variant" };
        var output = CreateButtonOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        var classAttribute = output.Attributes["class"];
        var classValue = classAttribute.Value.ToString();
        Assert.Contains("btn-secondary", classValue);
    }

    [Fact]
    public void Process_WithAllVariants_AllMapped()
    {
        // Test all 8 variants are correctly mapped
        var variants = new[] { "primary", "secondary", "danger", "success", "warning", "info", "light", "dark" };
        var expectedClasses = new[]
        {
            "btn-primary", "btn-secondary", "btn-danger", "btn-success",
            "btn-warning", "btn-info", "btn-light", "btn-dark"
        };

        for (int i = 0; i < variants.Length; i++)
        {
            // Arrange
            var helper = new ButtonTagHelper { Variant = variants[i] };
            var output = CreateButtonOutput();

            // Act
            helper.Process(_context, output);

            // Assert
            var classAttribute = output.Attributes["class"];
            var classValue = classAttribute.Value.ToString();
            Assert.Contains(expectedClasses[i], classValue);
        }
    }
}
