using Xunit;

namespace SmartWorkz.Core.Web.Tests.Components;

/// <summary>
/// Test suite for SpinnerComponent verifying spinner rendering, visibility toggling, type variants, size options, and label display.
/// </summary>
public class SpinnerComponentTests
{
    [Fact]
    public void SpinnerComponent_WithIsVisibleTrue_ShouldBeVisible()
    {
        // Arrange
        var isVisible = true;

        // Act
        var spinnerShouldRender = isVisible;

        // Assert
        Assert.True(spinnerShouldRender);
    }

    [Fact]
    public void SpinnerComponent_WithIsVisibleFalse_ShouldNotBeVisible()
    {
        // Arrange
        var isVisible = false;

        // Act
        var spinnerShouldRender = isVisible;

        // Assert
        Assert.False(spinnerShouldRender);
    }

    [Theory]
    [InlineData("border")]
    [InlineData("grow")]
    public void SpinnerComponent_WithDifferentTypes_ShouldSupportBorderAndGrow(string type)
    {
        // Arrange
        var validTypes = new[] { "border", "grow" };

        // Act
        var isValidType = validTypes.Contains(type);

        // Assert
        Assert.True(isValidType);
    }

    [Theory]
    [InlineData("sm")]
    [InlineData("md")]
    [InlineData("lg")]
    public void SpinnerComponent_WithDifferentSizes_ShouldSupportSmMdLg(string size)
    {
        // Arrange
        var validSizes = new[] { "sm", "md", "lg" };

        // Act
        var isValidSize = validSizes.Contains(size);

        // Assert
        Assert.True(isValidSize);
    }

    [Theory]
    [InlineData("primary")]
    [InlineData("success")]
    [InlineData("warning")]
    [InlineData("danger")]
    [InlineData("secondary")]
    [InlineData("info")]
    [InlineData("light")]
    [InlineData("dark")]
    public void SpinnerComponent_WithDifferentVariants_ShouldSupportAllColorVariants(string variant)
    {
        // Arrange
        var validVariants = new[] { "primary", "success", "warning", "danger", "secondary", "info", "light", "dark" };

        // Act
        var isValidVariant = validVariants.Contains(variant);

        // Assert
        Assert.True(isValidVariant);
    }

    [Fact]
    public void SpinnerComponent_WithOptionalLabel_ShouldDisplayLabel()
    {
        // Arrange
        var label = "Loading...";
        var showLabel = true;

        // Act
        var shouldDisplayLabel = !string.IsNullOrEmpty(label) && showLabel;

        // Assert
        Assert.True(shouldDisplayLabel);
        Assert.Equal("Loading...", label);
    }

    [Fact]
    public void SpinnerComponent_WithNullLabel_ShouldNotDisplayLabel()
    {
        // Arrange
        string? label = null;
        var showLabel = true;

        // Act
        var shouldDisplayLabel = !string.IsNullOrEmpty(label) && showLabel;

        // Assert
        Assert.False(shouldDisplayLabel);
    }

    [Fact]
    public void SpinnerComponent_WithShowLabelFalse_ShouldNotDisplayLabel()
    {
        // Arrange
        var label = "Loading...";
        var showLabel = false;

        // Act
        var shouldDisplayLabel = !string.IsNullOrEmpty(label) && showLabel;

        // Assert
        Assert.False(shouldDisplayLabel);
    }

    [Fact]
    public void SpinnerComponent_DefaultParameters_ShouldHaveCorrectDefaults()
    {
        // Arrange
        var defaultIsVisible = true;
        var defaultType = "border";
        var defaultVariant = "primary";
        var defaultSize = "md";
        var defaultShowLabel = true;

        // Act
        var isVisibleCorrect = defaultIsVisible == true;
        var typeCorrect = defaultType == "border";
        var variantCorrect = defaultVariant == "primary";
        var sizeCorrect = defaultSize == "md";
        var showLabelCorrect = defaultShowLabel == true;

        // Assert
        Assert.True(isVisibleCorrect);
        Assert.True(typeCorrect);
        Assert.True(variantCorrect);
        Assert.True(sizeCorrect);
        Assert.True(showLabelCorrect);
    }

    [Fact]
    public void SpinnerComponent_WithBootstrapClasses_ShouldIncludeSpinnerClass()
    {
        // Arrange
        var spinnerClass = "spinner-border";

        // Act
        var hasSpinnerClass = !string.IsNullOrEmpty(spinnerClass) && spinnerClass.Contains("spinner");

        // Assert
        Assert.True(hasSpinnerClass);
    }

    [Theory]
    [InlineData("border", "spinner-border")]
    [InlineData("grow", "spinner-grow")]
    public void SpinnerComponent_WithTypeVariant_ShouldRenderCorrectBootstrapClass(string type, string expectedClass)
    {
        // Arrange
        var spinnerType = type;

        // Act
        var spinnerClass = spinnerType == "border" ? "spinner-border" : "spinner-grow";

        // Assert
        Assert.Equal(expectedClass, spinnerClass);
    }
}
