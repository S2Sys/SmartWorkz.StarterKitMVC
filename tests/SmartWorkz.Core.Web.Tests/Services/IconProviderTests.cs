namespace SmartWorkz.Core.Web.Tests.Services;

using SmartWorkz.Web.Services.Components;
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

    [Fact]
    public void GetIconClass_WithInvalidIconType_ThrowsArgumentException()
    {
        // Arrange
        var invalidIconType = (IconType)(-1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _iconProvider.GetIconClass(invalidIconType));
        Assert.Contains("Unknown icon type", exception.Message);
    }

    [Fact]
    public void GetIconHtml_WithMaliciousCssClass_EscapesHtml()
    {
        // Arrange
        var iconType = IconType.Success;
        var maliciousCssClass = "\" onload=\"alert('XSS')";
        var expectedHtml = "<i class=\"bi bi-check-circle-fill &quot; onload=&quot;alert(&#39;XSS&#39;)\"></i>";

        // Act
        var result = _iconProvider.GetIconHtml(iconType, maliciousCssClass);

        // Assert
        Assert.Equal(expectedHtml, result);
    }

    [Fact]
    public void GetIconHtml_WithNullCssClass_ReturnsValidHtml()
    {
        // Arrange
        var iconType = IconType.Success;
        var expectedHtml = "<i class=\"bi bi-check-circle-fill\"></i>";

        // Act
        var result = _iconProvider.GetIconHtml(iconType, null);

        // Assert
        Assert.Equal(expectedHtml, result);
    }

    [Fact]
    public void GetIconClass_WithWhitespaceSizeClass_ReturnsBaseClassOnly()
    {
        // Arrange
        var iconType = IconType.Success;
        var whitespaceSize = "   ";

        // Act
        var result = _iconProvider.GetIconClass(iconType, whitespaceSize);

        // Assert
        Assert.Equal("bi bi-check-circle-fill", result);
    }

    [Fact]
    public void GetIconClass_WithWarningType_ReturnsExclamationTriangleClass()
    {
        // Arrange
        var iconType = IconType.Warning;

        // Act
        var result = _iconProvider.GetIconClass(iconType);

        // Assert
        Assert.Equal("bi bi-exclamation-triangle", result);
    }

    [Fact]
    public void GetIconClass_WithInfoType_ReturnsInfoCircleClass()
    {
        // Arrange
        var iconType = IconType.Info;

        // Act
        var result = _iconProvider.GetIconClass(iconType);

        // Assert
        Assert.Equal("bi bi-info-circle", result);
    }

    [Fact]
    public void GetIconClass_WithDeleteType_ReturnsTrashClass()
    {
        // Arrange
        var iconType = IconType.Delete;

        // Act
        var result = _iconProvider.GetIconClass(iconType);

        // Assert
        Assert.Equal("bi bi-trash", result);
    }

    [Fact]
    public void GetIconClass_WithExclamationTriangleType_ReturnsSeparateFromErrorType()
    {
        // Arrange
        var errorIconType = IconType.Error;
        var exclamationTriangleType = IconType.ExclamationTriangle;

        // Act
        var errorResult = _iconProvider.GetIconClass(errorIconType);
        var exclamationTriangleResult = _iconProvider.GetIconClass(exclamationTriangleType);

        // Assert - they should be different (Error is filled, ExclamationTriangle is not)
        Assert.Equal("bi bi-exclamation-triangle-fill", errorResult);
        Assert.Equal("bi bi-exclamation-triangle", exclamationTriangleResult);
        Assert.NotEqual(errorResult, exclamationTriangleResult);
    }

    [Fact]
    public void GetIconClass_WithInformationCircleType_ReturnsSeparateFromInfoType()
    {
        // Arrange
        var infoType = IconType.Info;
        var infoCircleType = IconType.InformationCircle;

        // Act
        var infoResult = _iconProvider.GetIconClass(infoType);
        var infoCircleResult = _iconProvider.GetIconClass(infoCircleType);

        // Assert - they should be different (Info is unfilled, InformationCircle is filled)
        Assert.Equal("bi bi-info-circle", infoResult);
        Assert.Equal("bi bi-info-circle-fill", infoCircleResult);
        Assert.NotEqual(infoResult, infoCircleResult);
    }
}

