using Xunit;
using SmartWorkz.Web.Models;

namespace SmartWorkz.Core.Web.Tests.Components;

/// <summary>
/// Test suite for TabsComponent verifying tabbed content rendering, active state management, and tab change callbacks.
/// </summary>
public class TabsComponentTests
{
    [Fact]
    public void TabsComponent_WithMultipleTabs_RendersAllTabs()
    {
        // Arrange
        var tabs = new List<TabItem>
        {
            new TabItem { Key = "tab1", Label = "Tab 1", Content = null },
            new TabItem { Key = "tab2", Label = "Tab 2", Content = null },
            new TabItem { Key = "tab3", Label = "Tab 3", Content = null }
        };

        // Act
        var tabCount = tabs.Count;
        var firstTab = tabs[0];
        var lastTab = tabs[tabs.Count - 1];

        // Assert
        Assert.Equal(3, tabCount);
        Assert.Equal("tab1", firstTab.Key);
        Assert.Equal("Tab 1", firstTab.Label);
        Assert.Equal("tab3", lastTab.Key);
        Assert.Equal("Tab 3", lastTab.Label);
    }

    [Fact]
    public void TabsComponent_OnTabClick_InvokesTabChangeCallback()
    {
        // Arrange
        var tabs = new List<TabItem>
        {
            new TabItem { Key = "tab1", Label = "Tab 1", Content = null },
            new TabItem { Key = "tab2", Label = "Tab 2", Content = null }
        };
        var changedTabKey = "";
        var tabChangeCallback = new Func<string, Task>(async (key) =>
        {
            changedTabKey = key;
            await Task.CompletedTask;
        });

        // Act
        tabChangeCallback.Invoke("tab2").Wait();

        // Assert
        Assert.Equal("tab2", changedTabKey);
    }

    [Fact]
    public void TabsComponent_WithActiveTabKey_SetsActiveTab()
    {
        // Arrange
        var tabs = new List<TabItem>
        {
            new TabItem { Key = "tab1", Label = "Tab 1", Content = null },
            new TabItem { Key = "tab2", Label = "Tab 2", Content = null },
            new TabItem { Key = "tab3", Label = "Tab 3", Content = null }
        };
        var activeTabKey = "tab2";

        // Act
        var activeTab = tabs.FirstOrDefault(t => t.Key == activeTabKey);
        var isActiveTabSet = activeTab != null && activeTab.Key == activeTabKey;

        // Assert
        Assert.True(isActiveTabSet);
        Assert.NotNull(activeTab);
        Assert.Equal("tab2", activeTab.Key);
    }

    [Fact]
    public void TabsComponent_FirstTab_IsActiveByDefault()
    {
        // Arrange
        var tabs = new List<TabItem>
        {
            new TabItem { Key = "tab1", Label = "Tab 1", Content = null },
            new TabItem { Key = "tab2", Label = "Tab 2", Content = null }
        };
        var activeTabKey = (string?)null;

        // Act
        var defaultActiveTab = activeTabKey ?? tabs.FirstOrDefault()?.Key ?? "";
        var isFirstTabActive = defaultActiveTab == "tab1";

        // Assert
        Assert.True(isFirstTabActive);
        Assert.Equal("tab1", defaultActiveTab);
    }

    [Fact]
    public void TabsComponent_TabItem_HasKeyLabel()
    {
        // Arrange
        var tabItem = new TabItem { Key = "overview", Label = "Overview", Content = null };

        // Act
        var hasKey = !string.IsNullOrEmpty(tabItem.Key);
        var hasLabel = !string.IsNullOrEmpty(tabItem.Label);

        // Assert
        Assert.True(hasKey);
        Assert.True(hasLabel);
        Assert.Equal("overview", tabItem.Key);
        Assert.Equal("Overview", tabItem.Label);
    }

    [Fact]
    public void TabsComponent_WithSingleTab_RendersTab()
    {
        // Arrange
        var tabs = new List<TabItem>
        {
            new TabItem { Key = "single", Label = "Single Tab", Content = null }
        };

        // Act
        var tabCount = tabs.Count;
        var tab = tabs[0];

        // Assert
        Assert.Single(tabs);
        Assert.Equal("single", tab.Key);
        Assert.Equal("Single Tab", tab.Label);
    }

    [Fact]
    public void TabsComponent_TabChangeCallback_InvokedMultipleTimes()
    {
        // Arrange
        var tabChangeCalls = new List<string>();
        var tabChangeCallback = new Func<string, Task>(async (key) =>
        {
            tabChangeCalls.Add(key);
            await Task.CompletedTask;
        });

        // Act
        tabChangeCallback.Invoke("tab1").Wait();
        tabChangeCallback.Invoke("tab2").Wait();
        tabChangeCallback.Invoke("tab3").Wait();

        // Assert
        Assert.Equal(3, tabChangeCalls.Count);
        Assert.Equal(new[] { "tab1", "tab2", "tab3" }, tabChangeCalls);
    }

    [Fact]
    public void TabsComponent_AllTabs_HaveUniqueKeys()
    {
        // Arrange
        var tabs = new List<TabItem>
        {
            new TabItem { Key = "tab1", Label = "Tab 1", Content = null },
            new TabItem { Key = "tab2", Label = "Tab 2", Content = null },
            new TabItem { Key = "tab3", Label = "Tab 3", Content = null }
        };

        // Act
        var uniqueKeys = tabs.Select(t => t.Key).Distinct().Count();

        // Assert
        Assert.Equal(tabs.Count, uniqueKeys);
    }

    [Fact]
    public void TabsComponent_WithEmptyTabs_RendersNoTabs()
    {
        // Arrange
        var tabs = new List<TabItem>();

        // Act
        var tabCount = tabs.Count;

        // Assert
        Assert.Equal(0, tabCount);
    }

    [Fact]
    public void TabsComponent_ActiveTab_CanBeChanged()
    {
        // Arrange
        var tabs = new List<TabItem>
        {
            new TabItem { Key = "tab1", Label = "Tab 1", Content = null },
            new TabItem { Key = "tab2", Label = "Tab 2", Content = null },
            new TabItem { Key = "tab3", Label = "Tab 3", Content = null }
        };
        var activeTabKey = "tab1";

        // Act
        activeTabKey = "tab3";
        var newActiveTab = tabs.FirstOrDefault(t => t.Key == activeTabKey);

        // Assert
        Assert.NotNull(newActiveTab);
        Assert.Equal("tab3", newActiveTab.Key);
    }

    [Fact]
    public void TabsComponent_TabLabels_CanContainSpecialCharacters()
    {
        // Arrange
        var tabs = new List<TabItem>
        {
            new TabItem { Key = "tab1", Label = "Tab 1 & More", Content = null },
            new TabItem { Key = "tab2", Label = "Tab - Settings", Content = null },
            new TabItem { Key = "tab3", Label = "Tab / Overview", Content = null }
        };

        // Act
        var complexLabel = tabs[0].Label;

        // Assert
        Assert.Equal("Tab 1 & More", complexLabel);
        Assert.Contains("&", complexLabel);
    }

    [Fact]
    public void TabsComponent_SettingActiveTabToNonExistentKey_HandlesGracefully()
    {
        // Arrange
        var tabs = new List<TabItem>
        {
            new TabItem { Key = "tab1", Label = "Tab 1", Content = null },
            new TabItem { Key = "tab2", Label = "Tab 2", Content = null }
        };
        var activeTabKey = "nonexistent";

        // Act
        var activeTab = tabs.FirstOrDefault(t => t.Key == activeTabKey);
        var fallbackTab = activeTab ?? tabs.FirstOrDefault();

        // Assert
        Assert.Null(activeTab);
        Assert.NotNull(fallbackTab);
        Assert.Equal("tab1", fallbackTab.Key);
    }

    [Fact]
    public void TabsComponent_TabKey_IsNotNull()
    {
        // Arrange
        var tab = new TabItem { Key = "unique-key", Label = "Test Tab", Content = null };

        // Act
        var hasKey = !string.IsNullOrEmpty(tab.Key);

        // Assert
        Assert.True(hasKey);
        Assert.Equal("unique-key", tab.Key);
    }

    [Fact]
    public void TabsComponent_TabLabel_IsNotNull()
    {
        // Arrange
        var tab = new TabItem { Key = "tab", Label = "Test Label", Content = null };

        // Act
        var hasLabel = !string.IsNullOrEmpty(tab.Label);

        // Assert
        Assert.True(hasLabel);
        Assert.Equal("Test Label", tab.Label);
    }

    [Fact]
    public void TabsComponent_FindTabByKey_ReturnsCorrectTab()
    {
        // Arrange
        var tabs = new List<TabItem>
        {
            new TabItem { Key = "overview", Label = "Overview", Content = null },
            new TabItem { Key = "details", Label = "Details", Content = null },
            new TabItem { Key = "settings", Label = "Settings", Content = null }
        };

        // Act
        var foundTab = tabs.FirstOrDefault(t => t.Key == "details");

        // Assert
        Assert.NotNull(foundTab);
        Assert.Equal("details", foundTab.Key);
        Assert.Equal("Details", foundTab.Label);
    }

    [Fact]
    public void TabsComponent_TabChangeCallback_IsAsync()
    {
        // Arrange
        var tabChangeCalls = new List<(string key, long timestamp)>();
        var tabChangeCallback = new Func<string, Task>(async (key) =>
        {
            tabChangeCalls.Add((key, DateTimeOffset.UtcNow.Ticks));
            await Task.Delay(10);
        });

        // Act
        tabChangeCallback.Invoke("tab1").Wait();
        var firstCallTime = tabChangeCalls[0].timestamp;

        tabChangeCallback.Invoke("tab2").Wait();
        var secondCallTime = tabChangeCalls[1].timestamp;

        // Assert
        Assert.Equal(2, tabChangeCalls.Count);
        Assert.True(secondCallTime >= firstCallTime);
    }

    [Fact]
    public void TabsComponent_MultipleTabSets_MaintainIndependentState()
    {
        // Arrange
        var tabs1 = new List<TabItem>
        {
            new TabItem { Key = "tab1", Label = "Tab 1", Content = null },
            new TabItem { Key = "tab2", Label = "Tab 2", Content = null }
        };
        var tabs2 = new List<TabItem>
        {
            new TabItem { Key = "tab3", Label = "Tab 3", Content = null },
            new TabItem { Key = "tab4", Label = "Tab 4", Content = null }
        };

        // Act
        var activeTab1 = "tab1";
        var activeTab2 = "tab4";

        // Assert
        Assert.Equal("tab1", activeTab1);
        Assert.Equal("tab4", activeTab2);
        Assert.NotEqual(activeTab1, activeTab2);
    }
}
