namespace SmartWorkz.Core.Web.Tests.Services;

using SmartWorkz.Core.Web.Services.Components;
using Xunit;

public class IconProviderTests
{
    private readonly IconProvider _iconProvider;

    public IconProviderTests()
    {
        _iconProvider = new IconProvider();
    }

    [Fact]
    public void GetIconClass_WithSuccessType_ReturnsCheckCircleFillClass()
    {
        // Arrange
        var iconType = IconType.Success;

        // Act
        var result = _iconProvider.GetIconClass(iconType);

        // Assert
        Assert.Equal("bi bi-check-circle-fill", result);
    }

    [Fact]
    public void GetIconClass_WithErrorType_ReturnsExclamationTriangleFillClass()
    {
        // Arrange
        var iconType = IconType.Error;

        // Act
        var result = _iconProvider.GetIconClass(iconType);

        // Assert
        Assert.Equal("bi bi-exclamation-triangle-fill", result);
    }

    [Fact]
    public void GetIconClass_WithSizeModifier_IncludesSizeClass()
    {
        // Arrange
        var iconType = IconType.Success;
        var sizeClass = "fs-5";

        // Act
        var result = _iconProvider.GetIconClass(iconType, sizeClass);

        // Assert
        Assert.Equal("bi bi-check-circle-fill fs-5", result);
    }

    [Fact]
    public void GetIconHtml_ReturnsFormattedHtmlString()
    {
        // Arrange
        var iconType = IconType.Success;
        var expectedHtml = "<i class=\"bi bi-check-circle-fill\"></i>";

        // Act
        var result = _iconProvider.GetIconHtml(iconType);

        // Assert
        Assert.Equal(expectedHtml, result);
    }

    [Fact]
    public void GetIconHtml_WithCustomClass_IncludesCustomClass()
    {
        // Arrange
        var iconType = IconType.Success;
        var customClass = "text-success";
        var expectedHtml = "<i class=\"bi bi-check-circle-fill text-success\"></i>";

        // Act
        var result = _iconProvider.GetIconHtml(iconType, customClass);

        // Assert
        Assert.Equal(expectedHtml, result);
    }
}
