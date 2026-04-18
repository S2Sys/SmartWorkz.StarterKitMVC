using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using SmartWorkz.Core.Web.Services.Components;
using SmartWorkz.Core.Web.TagHelpers.Display;
using Xunit;

namespace SmartWorkz.Core.Web.Tests.TagHelpers.Display;

public class AlertTagHelperTests
{
    private readonly TagHelperContext _context;
    private readonly Mock<IIconProvider> _mockIconProvider;

    public AlertTagHelperTests()
    {
        var attributes = new TagHelperAttributeList();
        _context = new TagHelperContext(
            attributes,
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N")
        );
        _mockIconProvider = new Mock<IIconProvider>();
    }

    private TagHelperOutput CreateAlertOutput()
    {
        var attributes = new TagHelperAttributeList();
        return new TagHelperOutput(
            "alert",
            attributes,
            (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(
                new DefaultTagHelperContent()
            )
        );
    }

    [Fact]
    public void Process_WithTypeSuccess_RendersAlertSuccessClassAndSuccessIcon()
    {
        // Arrange
        var successIconHtml = "<i class=\"bi bi-check-circle-fill\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Success, "me-2 flex-shrink-0"))
            .Returns(successIconHtml);

        var tagHelper = new AlertTagHelper(_mockIconProvider.Object)
        {
            Type = "success"
        };
        var output = CreateAlertOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("alert-success", content);
        Assert.Contains(successIconHtml, content);
        _mockIconProvider.Verify(x => x.GetIconHtml(IconType.Success, "me-2 flex-shrink-0"), Times.Once);
    }

    [Fact]
    public void Process_WithTypeDanger_RendersAlertDangerClassAndErrorIcon()
    {
        // Arrange
        var errorIconHtml = "<i class=\"bi bi-exclamation-triangle-fill\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Error, "me-2 flex-shrink-0"))
            .Returns(errorIconHtml);

        var tagHelper = new AlertTagHelper(_mockIconProvider.Object)
        {
            Type = "danger"
        };
        var output = CreateAlertOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("alert-danger", content);
        Assert.Contains(errorIconHtml, content);
        _mockIconProvider.Verify(x => x.GetIconHtml(IconType.Error, "me-2 flex-shrink-0"), Times.Once);
    }

    [Fact]
    public void Process_WithTypeWarning_RendersAlertWarningClassAndWarningIcon()
    {
        // Arrange
        var warningIconHtml = "<i class=\"bi bi-exclamation-circle-fill\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Warning, "me-2 flex-shrink-0"))
            .Returns(warningIconHtml);

        var tagHelper = new AlertTagHelper(_mockIconProvider.Object)
        {
            Type = "warning"
        };
        var output = CreateAlertOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("alert-warning", content);
        Assert.Contains(warningIconHtml, content);
        _mockIconProvider.Verify(x => x.GetIconHtml(IconType.Warning, "me-2 flex-shrink-0"), Times.Once);
    }

    [Fact]
    public void Process_WithDefaultType_RendersAlertInfoClassAndInfoIcon()
    {
        // Arrange
        var infoIconHtml = "<i class=\"bi bi-info-circle-fill\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Info, "me-2 flex-shrink-0"))
            .Returns(infoIconHtml);

        var tagHelper = new AlertTagHelper(_mockIconProvider.Object)
        {
            Type = "info"
        };
        var output = CreateAlertOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("alert-info", content);
        Assert.Contains(infoIconHtml, content);
        _mockIconProvider.Verify(x => x.GetIconHtml(IconType.Info, "me-2 flex-shrink-0"), Times.Once);
    }

    [Fact]
    public void Process_WithUnknownType_DefaultsToAlertInfoClass()
    {
        // Arrange
        var infoIconHtml = "<i class=\"bi bi-info-circle-fill\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Info, "me-2 flex-shrink-0"))
            .Returns(infoIconHtml);

        var tagHelper = new AlertTagHelper(_mockIconProvider.Object)
        {
            Type = "unknown"
        };
        var output = CreateAlertOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("alert-info", content);
        Assert.Contains(infoIconHtml, content);
        _mockIconProvider.Verify(x => x.GetIconHtml(IconType.Info, "me-2 flex-shrink-0"), Times.Once);
    }

    [Fact]
    public void Process_WithMessage_RendersMessageInDiv()
    {
        // Arrange
        var infoIconHtml = "<i class=\"bi bi-info-circle-fill\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Info, "me-2 flex-shrink-0"))
            .Returns(infoIconHtml);

        var tagHelper = new AlertTagHelper(_mockIconProvider.Object)
        {
            Type = "info",
            Message = "This is a test message"
        };
        var output = CreateAlertOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("<div>This is a test message</div>", content);
        Assert.Contains(infoIconHtml, content);
    }

    [Fact]
    public void Process_WithoutMessage_DoesNotRenderMessageDiv()
    {
        // Arrange
        var infoIconHtml = "<i class=\"bi bi-info-circle-fill\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Info, "me-2 flex-shrink-0"))
            .Returns(infoIconHtml);

        var tagHelper = new AlertTagHelper(_mockIconProvider.Object)
        {
            Type = "info",
            Message = null
        };
        var output = CreateAlertOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.DoesNotContain("<div>", content);
    }

    [Fact]
    public void Process_WithDismissibleTrue_IncludesCloseButtonAndDismissibleClasses()
    {
        // Arrange
        var infoIconHtml = "<i class=\"bi bi-info-circle-fill\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Info, "me-2 flex-shrink-0"))
            .Returns(infoIconHtml);

        var tagHelper = new AlertTagHelper(_mockIconProvider.Object)
        {
            Type = "info",
            Dismissible = true
        };
        var output = CreateAlertOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("alert-dismissible", content);
        Assert.Contains("fade show", content);
        Assert.Contains("btn-close", content);
        Assert.Contains("data-bs-dismiss=\"alert\"", content);
    }

    [Fact]
    public void Process_WithDismissibleFalse_ExcludesCloseButtonAndDismissibleClasses()
    {
        // Arrange
        var infoIconHtml = "<i class=\"bi bi-info-circle-fill\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Info, "me-2 flex-shrink-0"))
            .Returns(infoIconHtml);

        var tagHelper = new AlertTagHelper(_mockIconProvider.Object)
        {
            Type = "info",
            Dismissible = false
        };
        var output = CreateAlertOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.DoesNotContain("alert-dismissible", content);
        Assert.DoesNotContain("fade show", content);
        Assert.DoesNotContain("btn-close", content);
    }

    [Fact]
    public void Process_IncludesAlertAndFlexClasses()
    {
        // Arrange
        var infoIconHtml = "<i class=\"bi bi-info-circle-fill\"></i>";
        _mockIconProvider
            .Setup(x => x.GetIconHtml(IconType.Info, "me-2 flex-shrink-0"))
            .Returns(infoIconHtml);

        var tagHelper = new AlertTagHelper(_mockIconProvider.Object)
        {
            Type = "info"
        };
        var output = CreateAlertOutput();

        // Act
        tagHelper.Process(_context, output);

        // Assert
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("class=\"alert", content);
        Assert.Contains("d-flex", content);
        Assert.Contains("align-items-center", content);
    }

    [Fact]
    public void Constructor_WithNullIconProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new AlertTagHelper(null!));
        Assert.Equal("iconProvider", exception.ParamName);
    }
}
