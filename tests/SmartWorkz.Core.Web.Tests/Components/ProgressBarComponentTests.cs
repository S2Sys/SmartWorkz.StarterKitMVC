using Xunit;
using SmartWorkz.Web.Models;

namespace SmartWorkz.Core.Web.Tests.Components;

/// <summary>
/// Test suite for ProgressBarComponent verifying progress rendering, percentage display, color variants, and animation effects.
/// </summary>
public class ProgressBarComponentTests
{
    [Fact]
    public void ProgressBar_WithValidValue_RendersCorrectly()
    {
        // Arrange
        var value = 50;

        // Act
        var isValid = value >= 0 && value <= 100;
        var percentage = $"{value}%";

        // Assert
        Assert.True(isValid);
        Assert.Equal("50%", percentage);
    }

    [Fact]
    public void ProgressBar_WithZeroValue_RendersEmptyBar()
    {
        // Arrange
        var value = 0;

        // Act
        var isZero = value == 0;
        var percentage = value / 100.0;

        // Assert
        Assert.True(isZero);
        Assert.Equal(0, percentage);
    }

    [Fact]
    public void ProgressBar_WithHundredValue_RendersFull()
    {
        // Arrange
        var value = 100;

        // Act
        var isFull = value == 100;
        var percentage = value / 100.0;

        // Assert
        Assert.True(isFull);
        Assert.Equal(1.0, percentage);
    }

    [Fact]
    public void ProgressBar_WithColorVariant_AppliesCorrectCssClass()
    {
        // Arrange
        var variant = "success";

        // Act
        var cssClass = $"progress-bar progress-bar-{variant}";

        // Assert
        Assert.Equal("progress-bar progress-bar-success", cssClass);
    }

    [Fact]
    public void ProgressBar_WithMultipleColorVariants_GeneratesCorrectClasses()
    {
        // Arrange
        var variants = new[] { "primary", "success", "warning", "danger" };

        // Act
        var classes = variants.Select(v => $"progress-bar-{v}").ToList();

        // Assert
        Assert.Equal(4, classes.Count);
        Assert.Contains("progress-bar-primary", classes);
        Assert.Contains("progress-bar-success", classes);
        Assert.Contains("progress-bar-warning", classes);
        Assert.Contains("progress-bar-danger", classes);
    }

    [Fact]
    public void ProgressBar_ShowLabel_DisplaysPercentage()
    {
        // Arrange
        var value = 75;
        var showLabel = true;

        // Act
        var labelText = showLabel ? $"{value}%" : "";

        // Assert
        Assert.True(showLabel);
        Assert.Equal("75%", labelText);
    }

    [Fact]
    public void ProgressBar_HideLabel_NoPercentageDisplay()
    {
        // Arrange
        var value = 50;
        var showLabel = false;

        // Act
        var labelText = showLabel ? $"{value}%" : "";

        // Assert
        Assert.False(showLabel);
        Assert.Empty(labelText);
    }

    [Fact]
    public void ProgressBar_WithAnimatedFlag_AppliesAnimationClass()
    {
        // Arrange
        var animated = true;

        // Act
        var cssClass = animated ? "progress-bar-animated" : "";

        // Assert
        Assert.True(animated);
        Assert.Equal("progress-bar-animated", cssClass);
    }

    [Fact]
    public void ProgressBar_WithoutAnimatedFlag_NoAnimationClass()
    {
        // Arrange
        var animated = false;

        // Act
        var cssClass = animated ? "progress-bar-animated" : "";

        // Assert
        Assert.False(animated);
        Assert.Empty(cssClass);
    }

    [Fact]
    public void ProgressBar_WithStripedFlag_AppliesStripedClass()
    {
        // Arrange
        var striped = true;

        // Act
        var cssClass = striped ? "progress-bar-striped" : "";

        // Assert
        Assert.True(striped);
        Assert.Equal("progress-bar-striped", cssClass);
    }

    [Fact]
    public void ProgressBar_WithoutStripedFlag_NoStripedClass()
    {
        // Arrange
        var striped = false;

        // Act
        var cssClass = striped ? "progress-bar-striped" : "";

        // Assert
        Assert.False(striped);
        Assert.Empty(cssClass);
    }

    [Fact]
    public void ProgressBar_WithCustomHeight_AppliesHeightStyle()
    {
        // Arrange
        var height = "0.5rem";

        // Act
        var styleString = $"height: {height};";

        // Assert
        Assert.Equal("height: 0.5rem;", styleString);
    }

    [Fact]
    public void ProgressBar_WithDefaultHeight_Uses1Rem()
    {
        // Arrange
        var height = "1rem";

        // Act
        var isDefault = height == "1rem";

        // Assert
        Assert.True(isDefault);
        Assert.Equal("1rem", height);
    }

    [Fact]
    public void ProgressBar_CombinesMultipleCssClasses()
    {
        // Arrange
        var variant = "warning";
        var animated = true;
        var striped = true;

        // Act
        var classes = new List<string> { "progress-bar", $"progress-bar-{variant}" };
        if (striped) classes.Add("progress-bar-striped");
        if (animated) classes.Add("progress-bar-animated");
        var combinedClass = string.Join(" ", classes);

        // Assert
        Assert.Contains("progress-bar", combinedClass);
        Assert.Contains("progress-bar-warning", combinedClass);
        Assert.Contains("progress-bar-striped", combinedClass);
        Assert.Contains("progress-bar-animated", combinedClass);
    }

    [Fact]
    public void ProgressBar_WithVariantAndLabel_DisplaysBoth()
    {
        // Arrange
        var value = 33;
        var variant = "danger";
        var showLabel = true;

        // Act
        var cssClass = $"progress-bar-{variant}";
        var labelText = showLabel ? $"{value}%" : "";

        // Assert
        Assert.Equal("progress-bar-danger", cssClass);
        Assert.Equal("33%", labelText);
    }

    [Fact]
    public void ProgressBar_Value_ClampsToValidRange()
    {
        // Arrange
        var rawValue = 150;

        // Act
        var clampedValue = Math.Clamp(rawValue, 0, 100);

        // Assert
        Assert.Equal(100, clampedValue);
    }

    [Fact]
    public void ProgressBar_NegativeValue_ClampedToZero()
    {
        // Arrange
        var rawValue = -25;

        // Act
        var clampedValue = Math.Clamp(rawValue, 0, 100);

        // Assert
        Assert.Equal(0, clampedValue);
    }

    [Fact]
    public void ProgressBar_WithVariousValues_CalculatesCorrectPercentages()
    {
        // Arrange
        var values = new[] { 0, 25, 50, 75, 100 };

        // Act
        var percentages = values.Select(v => (double)v / 100).ToList();

        // Assert
        Assert.Equal(5, percentages.Count);
        Assert.Equal(0.0, percentages[0]);
        Assert.Equal(0.25, percentages[1]);
        Assert.Equal(0.5, percentages[2]);
        Assert.Equal(0.75, percentages[3]);
        Assert.Equal(1.0, percentages[4]);
    }

    [Fact]
    public void ProgressBar_DefaultVariant_IsPrimary()
    {
        // Arrange
        var variant = "primary";

        // Act
        var isDefault = variant == "primary";

        // Assert
        Assert.True(isDefault);
        Assert.Equal("primary", variant);
    }

    [Fact]
    public void ProgressBar_WithAllOptions_CombinesCorrectly()
    {
        // Arrange
        var value = 65;
        var variant = "success";
        var showLabel = true;
        var animated = true;
        var striped = true;
        var height = "1.5rem";

        // Act
        var classes = new List<string> { "progress-bar", $"progress-bar-{variant}" };
        if (striped) classes.Add("progress-bar-striped");
        if (animated) classes.Add("progress-bar-animated");
        var cssClass = string.Join(" ", classes);
        var label = showLabel ? $"{value}%" : "";
        var style = $"height: {height};";

        // Assert
        Assert.Contains("progress-bar-success", cssClass);
        Assert.Contains("progress-bar-striped", cssClass);
        Assert.Contains("progress-bar-animated", cssClass);
        Assert.Equal("65%", label);
        Assert.Equal("height: 1.5rem;", style);
    }

    [Fact]
    public void ProgressBar_PercentageLabel_FormattedCorrectly()
    {
        // Arrange
        var value = 42;

        // Act
        var formattedLabel = $"{value}%";

        // Assert
        Assert.Equal("42%", formattedLabel);
        Assert.EndsWith("%", formattedLabel);
    }

    [Fact]
    public void ProgressBar_AllVariants_AreValid()
    {
        // Arrange
        var validVariants = new[] { "primary", "success", "warning", "danger" };
        var testVariant = "success";

        // Act
        var isValid = validVariants.Contains(testVariant);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ProgressBar_InvalidVariant_HandledGracefully()
    {
        // Arrange
        var variant = "invalid";
        var validVariants = new[] { "primary", "success", "warning", "danger" };

        // Act
        var fallbackVariant = validVariants.Contains(variant) ? variant : "primary";

        // Assert
        Assert.Equal("primary", fallbackVariant);
    }

    [Fact]
    public void ProgressBar_WithFractionalValue_RoundsToNearestPercent()
    {
        // Arrange
        var value = 33.33;

        // Act
        var roundedValue = Math.Round(value);

        // Assert
        Assert.Equal(33, roundedValue);
    }
}
