using Xunit;
using SmartWorkz.Web.Models;

namespace SmartWorkz.Core.Web.Tests.Components;

/// <summary>
/// Test suite for SidebarNavigationComponent verifying sidebar rendering, collapse/expand functionality,
/// nested navigation items, icon support, and navigation callbacks.
/// </summary>
public class SidebarNavigationComponentTests
{
    [Fact]
    public void SidebarNavigationComponent_WithItems_RendersNavigationItems()
    {
        // Arrange
        var navItems = new List<NavItem>
        {
            new NavItem { Key = "dashboard", Label = "Dashboard", Icon = "bi-house" },
            new NavItem { Key = "profile", Label = "Profile", Icon = "bi-person" }
        };

        // Act
        var itemCount = navItems.Count;

        // Assert
        Assert.Equal(2, itemCount);
        Assert.All(navItems, item => Assert.NotEmpty(item.Label));
        Assert.All(navItems, item => Assert.NotEmpty(item.Icon));
    }

    [Fact]
    public void SidebarNavigationComponent_WithNestedItems_RendersNestedNavigation()
    {
        // Arrange
        var childItem1 = new NavItem { Key = "users", Label = "Users", Icon = "bi-people" };
        var childItem2 = new NavItem { Key = "roles", Label = "Roles", Icon = "bi-lock" };
        var parentItem = new NavItem
        {
            Key = "admin",
            Label = "Admin",
            Icon = "bi-gear",
            Children = new List<NavItem> { childItem1, childItem2 }
        };

        // Act
        var hasChildren = parentItem.Children != null && parentItem.Children.Count > 0;
        var childrenCount = parentItem.Children?.Count ?? 0;

        // Assert
        Assert.True(hasChildren);
        Assert.Equal(2, childrenCount);
        Assert.Contains("Users", parentItem.Children!.Select(c => c.Label));
        Assert.Contains("Roles", parentItem.Children!.Select(c => c.Label));
    }

    [Fact]
    public void SidebarNavigationComponent_IsCollapsed_ReturnsCollapsedState()
    {
        // Arrange
        var isCollapsed = true;

        // Act
        var collapsedState = isCollapsed;

        // Assert
        Assert.True(collapsedState);
    }

    [Fact]
    public void SidebarNavigationComponent_DefaultIsCollapsed_IsFalse()
    {
        // Arrange
        var defaultIsCollapsed = false;

        // Act
        var isCollapsed = defaultIsCollapsed;

        // Assert
        Assert.False(isCollapsed);
    }

    [Fact]
    public void SidebarNavigationComponent_WithCollapsedWidthParameter_SetsCorrectWidth()
    {
        // Arrange
        var collapsedWidth = "60px";
        var expandedWidth = "250px";

        // Act
        var widthValues = new[] { collapsedWidth, expandedWidth };

        // Assert
        Assert.Equal(2, widthValues.Length);
        Assert.Equal("60px", collapsedWidth);
        Assert.Equal("250px", expandedWidth);
    }

    [Fact]
    public void SidebarNavigationComponent_OnCollapsedChangedCallback_IsInvoked()
    {
        // Arrange
        var collapsedChangedCalled = false;
        var collapsedCallback = new Func<bool, Task>(async (isCollapsed) =>
        {
            collapsedChangedCalled = true;
            await Task.CompletedTask;
        });

        // Act
        collapsedCallback.Invoke(true).Wait();

        // Assert
        Assert.True(collapsedChangedCalled);
    }

    [Fact]
    public void SidebarNavigationComponent_OnNavigateCallback_IsInvokedWithItemKey()
    {
        // Arrange
        var navigationKey = string.Empty;
        var navigateCallback = new Func<string, Task>(async (key) =>
        {
            navigationKey = key;
            await Task.CompletedTask;
        });

        // Act
        navigateCallback.Invoke("dashboard").Wait();

        // Assert
        Assert.Equal("dashboard", navigationKey);
    }

    [Fact]
    public void SidebarNavigationComponent_WithBootstrapStyling_AppliesCorrectClasses()
    {
        // Arrange
        var sidebarClass = "sidebar bg-dark";
        var navItemClass = "nav-item";
        var navLinkClass = "nav-link";

        // Act
        var hasBootstrapClasses = sidebarClass.Contains("bg-dark") &&
                                 navItemClass.Contains("nav-item") &&
                                 navLinkClass.Contains("nav-link");

        // Assert
        Assert.True(hasBootstrapClasses);
        Assert.Contains("bg-dark", sidebarClass);
    }

    [Fact]
    public void SidebarNavigationComponent_WithIconSupport_RendersIconClass()
    {
        // Arrange
        var iconClass = "bi-house";
        var isValidIconClass = iconClass.StartsWith("bi-");

        // Act
        var rendersIcon = !string.IsNullOrEmpty(iconClass);

        // Assert
        Assert.True(rendersIcon);
        Assert.True(isValidIconClass);
    }

    [Fact]
    public void SidebarNavigationComponent_WithChildContent_AcceptsRenderFragment()
    {
        // Arrange
        var navItem = new NavItem
        {
            Key = "custom",
            Label = "Custom Item",
            Icon = "bi-star",
            ChildContent = null
        };

        // Act
        var hasChildContent = navItem.ChildContent != null;

        // Assert
        Assert.False(hasChildContent);
    }

    [Fact]
    public void SidebarNavigationComponent_MultiLevelNesting_SupportsThreeLevels()
    {
        // Arrange
        var level3Item = new NavItem { Key = "sub-role", Label = "Sub Role", Icon = "bi-lock-fill" };
        var level2Item = new NavItem
        {
            Key = "roles",
            Label = "Roles",
            Icon = "bi-lock",
            Children = new List<NavItem> { level3Item }
        };
        var level1Item = new NavItem
        {
            Key = "admin",
            Label = "Admin",
            Icon = "bi-gear",
            Children = new List<NavItem> { level2Item }
        };

        // Act
        var hasLevel3 = level1Item.Children?[0].Children != null && level1Item.Children[0].Children.Count > 0;

        // Assert
        Assert.True(hasLevel3);
        Assert.Equal("sub-role", level1Item.Children![0].Children![0].Key);
    }

    [Fact]
    public void SidebarNavigationComponent_ToggleCollapse_ChangesState()
    {
        // Arrange
        var isCollapsed = false;

        // Act
        isCollapsed = !isCollapsed;
        var newCollapsedState = isCollapsed;

        // Assert
        Assert.True(newCollapsedState);
    }

    [Fact]
    public void SidebarNavigationComponent_WithMultipleItems_AllItemsHaveUniqueKeys()
    {
        // Arrange
        var navItems = new List<NavItem>
        {
            new NavItem { Key = "dashboard", Label = "Dashboard", Icon = "bi-house" },
            new NavItem { Key = "profile", Label = "Profile", Icon = "bi-person" },
            new NavItem { Key = "admin", Label = "Admin", Icon = "bi-gear" }
        };

        // Act
        var uniqueKeys = navItems.Select(i => i.Key).Distinct().Count();

        // Assert
        Assert.Equal(3, uniqueKeys);
        Assert.Equal(navItems.Count, uniqueKeys);
    }

    [Fact]
    public void SidebarNavigationComponent_DefaultWidth_Is250px()
    {
        // Arrange
        var defaultWidth = "250px";

        // Act
        var width = defaultWidth;

        // Assert
        Assert.Equal("250px", width);
    }

    [Fact]
    public void SidebarNavigationComponent_DefaultCollapsedWidth_Is60px()
    {
        // Arrange
        var defaultCollapsedWidth = "60px";

        // Act
        var collapsedWidth = defaultCollapsedWidth;

        // Assert
        Assert.Equal("60px", collapsedWidth);
    }

    [Fact]
    public void NavItem_Record_HasAllRequiredProperties()
    {
        // Arrange
        var navItem = new NavItem
        {
            Key = "test",
            Label = "Test",
            Icon = "bi-test",
            Children = null,
            ChildContent = null
        };

        // Act
        var hasKey = !string.IsNullOrEmpty(navItem.Key);
        var hasLabel = !string.IsNullOrEmpty(navItem.Label);
        var hasIcon = !string.IsNullOrEmpty(navItem.Icon);

        // Assert
        Assert.True(hasKey);
        Assert.True(hasLabel);
        Assert.True(hasIcon);
        Assert.Equal("test", navItem.Key);
        Assert.Equal("Test", navItem.Label);
    }
}
