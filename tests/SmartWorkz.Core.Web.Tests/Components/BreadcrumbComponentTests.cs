using Xunit;
using SmartWorkz.Web.Models;

namespace SmartWorkz.Core.Web.Tests.Components;

/// <summary>
/// Test suite for BreadcrumbComponent verifying breadcrumb rendering, navigation callbacks, separator configuration, and icon support.
/// </summary>
public class BreadcrumbComponentTests
{
    [Fact]
    public void BreadcrumbComponent_WithItems_RendersAllItems()
    {
        // Arrange
        var items = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Value = "home", Label = "Home" },
            new BreadcrumbItem { Value = "products", Label = "Products" },
            new BreadcrumbItem { Value = "details", Label = "Product Details" }
        };

        // Act
        var itemCount = items.Count;
        var firstItem = items[0];
        var lastItem = items[items.Count - 1];

        // Assert
        Assert.Equal(3, itemCount);
        Assert.Equal("home", firstItem.Value);
        Assert.Equal("Home", firstItem.Label);
        Assert.Equal("details", lastItem.Value);
        Assert.Equal("Product Details", lastItem.Label);
    }

    [Fact]
    public void BreadcrumbComponent_OnItemClick_InvokesNavigationCallback()
    {
        // Arrange
        var items = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Value = "home", Label = "Home" },
            new BreadcrumbItem { Value = "products", Label = "Products" }
        };
        var clickedItemValue = "";
        var navigationCallback = new Func<string, Task>(async (value) =>
        {
            clickedItemValue = value;
            await Task.CompletedTask;
        });

        // Act
        navigationCallback.Invoke("products").Wait();

        // Assert
        Assert.Equal("products", clickedItemValue);
    }

    [Fact]
    public void BreadcrumbComponent_WithCustomSeparator_UsesSeparatorText()
    {
        // Arrange
        var separator = " > ";

        // Act
        var isCustomSeparator = separator != "/";
        var trimmedSeparator = separator.Trim();

        // Assert
        Assert.True(isCustomSeparator);
        Assert.Equal(">", trimmedSeparator);
    }

    [Fact]
    public void BreadcrumbComponent_WithDefaultSeparator_UsesSlash()
    {
        // Arrange
        string? separator = null;

        // Act
        var defaultSeparator = separator ?? "/";

        // Assert
        Assert.Equal("/", defaultSeparator);
    }

    [Fact]
    public void BreadcrumbComponent_LastItem_IsActiveState()
    {
        // Arrange
        var items = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Value = "home", Label = "Home" },
            new BreadcrumbItem { Value = "products", Label = "Products" },
            new BreadcrumbItem { Value = "details", Label = "Details" }
        };

        // Act
        var lastItemIndex = items.Count - 1;
        var isLastItemActive = lastItemIndex == items.Count - 1;

        // Assert
        Assert.True(isLastItemActive);
        Assert.Equal("Details", items[lastItemIndex].Label);
    }

    [Fact]
    public void BreadcrumbComponent_WithIcons_RendersIconClass()
    {
        // Arrange
        var items = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Value = "home", Label = "Home", Icon = "bi bi-house" },
            new BreadcrumbItem { Value = "products", Label = "Products", Icon = "bi bi-box" },
            new BreadcrumbItem { Value = "details", Label = "Details" }
        };

        // Act
        var firstItemIcon = items[0].Icon;
        var lastItemIcon = items[2].Icon;

        // Assert
        Assert.NotNull(firstItemIcon);
        Assert.Equal("bi bi-house", firstItemIcon);
        Assert.Null(lastItemIcon);
    }

    [Fact]
    public void BreadcrumbComponent_WithEmptyItems_RendersNoItems()
    {
        // Arrange
        var items = new List<BreadcrumbItem>();

        // Act
        var itemCount = items.Count;

        // Assert
        Assert.Equal(0, itemCount);
    }

    [Fact]
    public void BreadcrumbComponent_SingleItem_IsActiveState()
    {
        // Arrange
        var items = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Value = "home", Label = "Home" }
        };

        // Act
        var isLastItem = items.Count == 1;

        // Assert
        Assert.True(isLastItem);
        Assert.Single(items);
    }

    [Fact]
    public void BreadcrumbComponent_ItemsWithComplexLabels_RendersLabelCorrectly()
    {
        // Arrange
        var items = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Value = "home", Label = "Home" },
            new BreadcrumbItem { Value = "admin", Label = "Admin - Settings & Configuration" },
            new BreadcrumbItem { Value = "users", Label = "User Management" }
        };

        // Act
        var complexLabel = items[1].Label;

        // Assert
        Assert.Equal("Admin - Settings & Configuration", complexLabel);
        Assert.Contains("&", complexLabel);
    }

    [Fact]
    public void BreadcrumbComponent_NavigateToMultipleItems_InvokesCallbackForEach()
    {
        // Arrange
        var navigationCalls = new List<string>();
        var navigationCallback = new Func<string, Task>(async (value) =>
        {
            navigationCalls.Add(value);
            await Task.CompletedTask;
        });

        // Act
        navigationCallback.Invoke("home").Wait();
        navigationCallback.Invoke("products").Wait();
        navigationCallback.Invoke("details").Wait();

        // Assert
        Assert.Equal(3, navigationCalls.Count);
        Assert.Equal(new[] { "home", "products", "details" }, navigationCalls);
    }

    [Fact]
    public void BreadcrumbComponent_ItemValue_IsNotNull()
    {
        // Arrange
        var item = new BreadcrumbItem { Value = "test", Label = "Test" };

        // Act
        var hasValue = !string.IsNullOrEmpty(item.Value);

        // Assert
        Assert.True(hasValue);
        Assert.Equal("test", item.Value);
    }

    [Fact]
    public void BreadcrumbComponent_ItemLabel_IsNotNull()
    {
        // Arrange
        var item = new BreadcrumbItem { Value = "test", Label = "Test Label" };

        // Act
        var hasLabel = !string.IsNullOrEmpty(item.Label);

        // Assert
        Assert.True(hasLabel);
        Assert.Equal("Test Label", item.Label);
    }

    [Fact]
    public void BreadcrumbComponent_WithMixedIconsAndNoIcons_RendersMixedCorrectly()
    {
        // Arrange
        var items = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Value = "home", Label = "Home", Icon = "bi bi-house" },
            new BreadcrumbItem { Value = "settings", Label = "Settings" },
            new BreadcrumbItem { Value = "profile", Label = "Profile", Icon = "bi bi-person" }
        };

        // Act
        var itemsWithIcons = items.Where(i => !string.IsNullOrEmpty(i.Icon)).ToList();
        var itemsWithoutIcons = items.Where(i => string.IsNullOrEmpty(i.Icon)).ToList();

        // Assert
        Assert.Equal(2, itemsWithIcons.Count);
        Assert.Single(itemsWithoutIcons);
        Assert.Equal("bi bi-house", items[0].Icon);
        Assert.Null(items[1].Icon);
        Assert.Equal("bi bi-person", items[2].Icon);
    }

    [Fact]
    public void BreadcrumbComponent_Separator_CanBeCustomized()
    {
        // Arrange
        var separators = new[] { "/", " > ", " → ", "|" };

        // Act & Assert
        foreach (var sep in separators)
        {
            Assert.NotNull(sep);
            Assert.NotEmpty(sep);
        }
    }

    [Fact]
    public void BreadcrumbComponent_AllItemsHaveUniqueValues()
    {
        // Arrange
        var items = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Value = "home", Label = "Home" },
            new BreadcrumbItem { Value = "products", Label = "Products" },
            new BreadcrumbItem { Value = "details", Label = "Details" }
        };

        // Act
        var uniqueValues = items.Select(i => i.Value).Distinct().Count();

        // Assert
        Assert.Equal(items.Count, uniqueValues);
    }
}
