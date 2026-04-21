using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using SmartWorkz.Web.Services.Components;
using SmartWorkz.Web.TagHelpers.Common;
using Xunit;

namespace SmartWorkz.Core.Web.Tests.TagHelpers.Common;

public class IconTagHelperTests
{
    private readonly TagHelperContext _context;
    private readonly Mock<IIconProvider> _mockIconProvider;

    public IconTagHelperTests()
    {
        var attributes = new TagHelperAttributeList();
        _context = new TagHelperContext(
            attributes,
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N")
        );
        _mockIconProvider = new Mock<IIconProvider>();
    }

    private TagHelperOutput CreateIconOutput()
    {
        var attributes = new TagHelperAttributeList();
        return new TagHelperOutput(
            "icon",
            attributes,
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(
                new DefaultTagHelperContent()
            )
        );
    }

    [Fact]
    public void Process_WithNameSuccess_RendersSuccessIconHtml()
    {
        // Arrange
        var expectedHtml = "<i class=\"bi bi-check-circle-fill\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Success, ""))
            .Returns(expectedHtml);

        var tagHelper = new IconTagHelper(_mockIconProvider.Object)
        {
            Name = "Success"
        };
        var output = CreateIconOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        Assert.Equal(expectedHtml, output.Content.GetContent());
        _mockIconProvider.Verify(x => x.GetIconHtml(IconType.Success, ""), Times.Once);
    }

    [Fact]
    public void Process_WithNameError_RendersErrorIconHtml()
    {
        // Arrange
        var expectedHtml = "<i class=\"bi bi-exclamation-triangle-fill\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Error, ""))
            .Returns(expectedHtml);

        var tagHelper = new IconTagHelper(_mockIconProvider.Object)
        {
            Name = "Error"
        };
        var output = CreateIconOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        Assert.Equal(expectedHtml, output.Content.GetContent());
        _mockIconProvider.Verify(x => x.GetIconHtml(IconType.Error, ""), Times.Once);
    }

    [Fact]
    public void Process_WithSizeSmall_AddsMeOneClassToIcon()
    {
        // Arrange
        var expectedHtml = "<i class=\"bi bi-check-circle-fill me-1\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Success, "me-1"))
            .Returns(expectedHtml);

        var tagHelper = new IconTagHelper(_mockIconProvider.Object)
        {
            Name = "Success",
            Size = "sm"
        };
        var output = CreateIconOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        Assert.Equal(expectedHtml, output.Content.GetContent());
        _mockIconProvider.Verify(x => x.GetIconHtml(IconType.Success, "me-1"), Times.Once);
    }

    [Fact]
    public void Process_WithSizeLarge_AddsFsHyphenFiveClassToIcon()
    {
        // Arrange
        var expectedHtml = "<i class=\"bi bi-check-circle-fill fs-5\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Success, "fs-5"))
            .Returns(expectedHtml);

        var tagHelper = new IconTagHelper(_mockIconProvider.Object)
        {
            Name = "Success",
            Size = "lg"
        };
        var output = CreateIconOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        Assert.Equal(expectedHtml, output.Content.GetContent());
        _mockIconProvider.Verify(x => x.GetIconHtml(IconType.Success, "fs-5"), Times.Once);
    }

    [Fact]
    public void Process_WithCustomCssClass_CombinesWithSizeClass()
    {
        // Arrange
        var expectedHtml = "<i class=\"bi bi-check-circle-fill custom-class me-1\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Success, "custom-class me-1"))
            .Returns(expectedHtml);

        var tagHelper = new IconTagHelper(_mockIconProvider.Object)
        {
            Name = "Success",
            Size = "sm",
            CssClass = "custom-class"
        };
        var output = CreateIconOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        Assert.Equal(expectedHtml, output.Content.GetContent());
        _mockIconProvider.Verify(x => x.GetIconHtml(IconType.Success, "custom-class me-1"), Times.Once);
    }

    [Fact]
    public void Process_WithCustomCssClassOnly_PreservesClass()
    {
        // Arrange
        var expectedHtml = "<i class=\"bi bi-check-circle-fill custom-class\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Success, "custom-class"))
            .Returns(expectedHtml);

        var tagHelper = new IconTagHelper(_mockIconProvider.Object)
        {
            Name = "Success",
            CssClass = "custom-class"
        };
        var output = CreateIconOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        Assert.Equal(expectedHtml, output.Content.GetContent());
        _mockIconProvider.Verify(x => x.GetIconHtml(IconType.Success, "custom-class"), Times.Once);
    }

    [Fact]
    public void Process_WithInvalidIconName_RendersHtmlComment()
    {
        // Arrange
        var tagHelper = new IconTagHelper(_mockIconProvider.Object)
        {
            Name = "InvalidName"
        };
        var output = CreateIconOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        Assert.Equal("<!-- Unknown icon: InvalidName -->", output.Content.GetContent());
        _mockIconProvider.Verify(x => x.GetIconHtml(It.IsAny<IconType>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Process_WithLowercaseIconName_ParsesCaseInsensitively()
    {
        // Arrange
        var expectedHtml = "<i class=\"bi bi-check-circle-fill\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Success, ""))
            .Returns(expectedHtml);

        var tagHelper = new IconTagHelper(_mockIconProvider.Object)
        {
            Name = "success"
        };
        var output = CreateIconOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        Assert.Equal(expectedHtml, output.Content.GetContent());
        _mockIconProvider.Verify(x => x.GetIconHtml(IconType.Success, ""), Times.Once);
    }

    [Fact]
    public void Process_WithUppercaseIconName_ParsesCaseInsensitively()
    {
        // Arrange
        var expectedHtml = "<i class=\"bi bi-check-circle-fill\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Success, ""))
            .Returns(expectedHtml);

        var tagHelper = new IconTagHelper(_mockIconProvider.Object)
        {
            Name = "SUCCESS"
        };
        var output = CreateIconOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        Assert.Equal(expectedHtml, output.Content.GetContent());
        _mockIconProvider.Verify(x => x.GetIconHtml(IconType.Success, ""), Times.Once);
    }

    [Fact]
    public void Constructor_WithNullIconProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new IconTagHelper(null!));
        Assert.Equal("iconProvider", exception.ParamName);
    }
}

