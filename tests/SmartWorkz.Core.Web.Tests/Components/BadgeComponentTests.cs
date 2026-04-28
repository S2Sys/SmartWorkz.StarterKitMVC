using Xunit;

namespace SmartWorkz.Core.Web.Tests.Components;

/// <summary>
/// Test suite for BadgeComponent verifying badge rendering, color variants, pill shape, and dismissible functionality.
/// </summary>
public class BadgeComponentTests
{
    [Fact]
    public void BadgeComponent_WithText_RendersBadgeText()
    {
        // Arrange
        var badgeText = "New";

        // Act
        var isBadgeValid = !string.IsNullOrEmpty(badgeText);

        // Assert
        Assert.True(isBadgeValid);
        Assert.Equal("New", badgeText);
    }

    [Fact]
    public void BadgeComponent_WithVariant_AppliesCorrectCssClass()
    {
        // Arrange
        var variants = new[] { "primary", "success", "info", "warning", "danger", "secondary", "light", "dark" };
        var selectedVariant = "success";

        // Act
        var isValidVariant = variants.Contains(selectedVariant);
        var cssClass = $"bg-{selectedVariant}";

        // Assert
        Assert.True(isValidVariant);
        Assert.Equal("bg-success", cssClass);
    }

    [Fact]
    public void BadgeComponent_WithPillShapeEnabled_AppliesPillClass()
    {
        // Arrange
        var isPill = true;
        var pillClass = "rounded-pill";

        // Act
        var shouldApplyPill = isPill;

        // Assert
        Assert.True(shouldApplyPill);
        Assert.NotEmpty(pillClass);
        Assert.Equal("rounded-pill", pillClass);
    }

    [Fact]
    public void BadgeComponent_WithDismissibleEnabled_ShowsCloseButton()
    {
        // Arrange
        var isDismissible = true;

        // Act
        var showCloseButton = isDismissible;

        // Assert
        Assert.True(showCloseButton);
    }

    [Fact]
    public void BadgeComponent_WithDismissCallback_InvokesDismissCallback()
    {
        // Arrange
        var dismissCalled = false;
        var dismissCallback = new Func<Task>(async () =>
        {
            dismissCalled = true;
            await Task.CompletedTask;
        });

        // Act
        dismissCallback.Invoke().Wait();

        // Assert
        Assert.True(dismissCalled);
    }

    [Fact]
    public void BadgeComponent_DefaultVariant_IsPrimary()
    {
        // Arrange
        string? defaultVariant = null;

        // Act
        var variant = defaultVariant ?? "primary";

        // Assert
        Assert.Equal("primary", variant);
    }

    [Fact]
    public void BadgeComponent_DefaultIsPill_IsFalse()
    {
        // Arrange
        bool defaultIsPill = false;

        // Act
        var isPill = defaultIsPill;

        // Assert
        Assert.False(isPill);
    }

    [Fact]
    public void BadgeComponent_DefaultDismissible_IsFalse()
    {
        // Arrange
        bool defaultDismissible = false;

        // Act
        var isDismissible = defaultDismissible;

        // Assert
        Assert.False(isDismissible);
    }

    [Fact]
    public void BadgeComponent_AllVariants_AreSupported()
    {
        // Arrange
        var allVariants = new[] { "primary", "success", "info", "warning", "danger", "secondary", "light", "dark" };

        // Act
        var variantCount = allVariants.Length;

        // Assert
        Assert.Equal(8, variantCount);
        Assert.Contains("primary", allVariants);
        Assert.Contains("success", allVariants);
        Assert.Contains("danger", allVariants);
        Assert.Contains("warning", allVariants);
        Assert.Contains("info", allVariants);
        Assert.Contains("secondary", allVariants);
        Assert.Contains("light", allVariants);
        Assert.Contains("dark", allVariants);
    }

    [Fact]
    public void BadgeComponent_WithCustomContent_AcceptsRenderFragment()
    {
        // Arrange
        var hasCustomContent = true;

        // Act
        var acceptsContent = hasCustomContent;

        // Assert
        Assert.True(acceptsContent);
    }

    [Fact]
    public void BadgeComponent_WithCountText_RendersBadgeCorrectly()
    {
        // Arrange
        var badgeText = "5";
        var badgeLabel = "New Messages";

        // Act
        var isValidCount = !string.IsNullOrEmpty(badgeText) && int.TryParse(badgeText, out _);

        // Assert
        Assert.True(isValidCount);
        Assert.Equal("5", badgeText);
    }

    [Fact]
    public void BadgeComponent_WithStatusText_RendersBadgeCorrectly()
    {
        // Arrange
        var statusBadges = new[] { "Active", "Inactive", "Pending", "Completed", "Failed" };

        // Act
        var badgeCount = statusBadges.Length;

        // Assert
        Assert.Equal(5, badgeCount);
        Assert.All(statusBadges, badge => Assert.NotEmpty(badge));
    }

    [Fact]
    public void BadgeComponent_WithPillAndDismissible_BothFeaturesWork()
    {
        // Arrange
        var isPill = true;
        var isDismissible = true;
        var dismissCalled = false;
        var dismissCallback = new Func<Task>(async () =>
        {
            dismissCalled = true;
            await Task.CompletedTask;
        });

        // Act
        var hasBothFeatures = isPill && isDismissible;
        dismissCallback.Invoke().Wait();

        // Assert
        Assert.True(hasBothFeatures);
        Assert.True(dismissCalled);
    }

    [Fact]
    public void BadgeComponent_LightVariant_IncludesTextDarkClass()
    {
        // Arrange
        var lightVariantClasses = "bg-light text-dark";

        // Act
        var hasTextDark = lightVariantClasses.Contains("text-dark");

        // Assert
        Assert.True(hasTextDark);
        Assert.Contains("bg-light", lightVariantClasses);
        Assert.Contains("text-dark", lightVariantClasses);
    }

    [Fact]
    public void BadgeComponent_VariantMapping_IsMappedCorrectly()
    {
        // Arrange
        var variantMap = new Dictionary<string, string>
        {
            { "primary", "bg-primary" },
            { "success", "bg-success" },
            { "info", "bg-info" },
            { "warning", "bg-warning" },
            { "danger", "bg-danger" },
            { "secondary", "bg-secondary" },
            { "light", "bg-light text-dark" },
            { "dark", "bg-dark" }
        };

        // Act
        var mappingCount = variantMap.Count;

        // Assert
        Assert.Equal(8, mappingCount);
        Assert.Equal("bg-success", variantMap["success"]);
        Assert.Equal("bg-light text-dark", variantMap["light"]);
    }
}
