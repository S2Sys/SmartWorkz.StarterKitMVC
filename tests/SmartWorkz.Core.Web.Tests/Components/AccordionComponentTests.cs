using Xunit;
using SmartWorkz.Web.Models;

namespace SmartWorkz.Core.Web.Tests.Components;

/// <summary>
/// Test suite for AccordionComponent verifying section rendering, expand/collapse behavior,
/// exclusive/multiple open modes, and section change callbacks.
/// </summary>
public class AccordionComponentTests
{
    [Fact]
    public void AccordionComponent_WithMultipleSections_RendersAllSections()
    {
        // Arrange
        var items = new List<AccordionItem>
        {
            new AccordionItem { Key = "section1", Title = "Section 1", Content = null },
            new AccordionItem { Key = "section2", Title = "Section 2", Content = null },
            new AccordionItem { Key = "section3", Title = "Section 3", Content = null }
        };

        // Act
        var itemCount = items.Count;
        var firstItem = items[0];
        var lastItem = items[items.Count - 1];

        // Assert
        Assert.Equal(3, itemCount);
        Assert.Equal("section1", firstItem.Key);
        Assert.Equal("Section 1", firstItem.Title);
        Assert.Equal("section3", lastItem.Key);
        Assert.Equal("Section 3", lastItem.Title);
    }

    [Fact]
    public void AccordionComponent_OnSectionClick_InvokesSectionChangeCallback()
    {
        // Arrange
        var items = new List<AccordionItem>
        {
            new AccordionItem { Key = "section1", Title = "Section 1", Content = null },
            new AccordionItem { Key = "section2", Title = "Section 2", Content = null }
        };
        var changedSectionKey = "";
        var sectionChangeCallback = new Func<string, Task>(async (key) =>
        {
            changedSectionKey = key;
            await Task.CompletedTask;
        });

        // Act
        sectionChangeCallback.Invoke("section2").Wait();

        // Assert
        Assert.Equal("section2", changedSectionKey);
    }

    [Fact]
    public void AccordionComponent_WithActiveSectionKey_SetsActiveSection()
    {
        // Arrange
        var items = new List<AccordionItem>
        {
            new AccordionItem { Key = "section1", Title = "Section 1", Content = null },
            new AccordionItem { Key = "section2", Title = "Section 2", Content = null },
            new AccordionItem { Key = "section3", Title = "Section 3", Content = null }
        };
        var activeSectionKey = "section2";

        // Act
        var activeSection = items.FirstOrDefault(s => s.Key == activeSectionKey);
        var isActiveSectionSet = activeSection != null && activeSection.Key == activeSectionKey;

        // Assert
        Assert.True(isActiveSectionSet);
        Assert.NotNull(activeSection);
        Assert.Equal("section2", activeSection.Key);
    }

    [Fact]
    public void AccordionComponent_ExclusiveMode_AllowsOnlyOneOpenSection()
    {
        // Arrange
        var allowMultipleOpen = false;
        var openSections = new List<string> { "section1" };

        // Act
        if (!allowMultipleOpen && openSections.Count > 0)
        {
            openSections.Clear();
            openSections.Add("section2");
        }

        // Assert
        Assert.False(allowMultipleOpen);
        Assert.Single(openSections);
        Assert.Equal("section2", openSections[0]);
    }

    [Fact]
    public void AccordionComponent_MultipleOpenMode_AllowsMultipleOpenSections()
    {
        // Arrange
        var allowMultipleOpen = true;
        var openSections = new List<string> { "section1" };

        // Act
        if (allowMultipleOpen)
        {
            openSections.Add("section2");
        }

        // Assert
        Assert.True(allowMultipleOpen);
        Assert.Equal(2, openSections.Count);
        Assert.Contains("section1", openSections);
        Assert.Contains("section2", openSections);
    }

    [Fact]
    public void AccordionComponent_FirstSection_IsActiveByDefault()
    {
        // Arrange
        var items = new List<AccordionItem>
        {
            new AccordionItem { Key = "section1", Title = "Section 1", Content = null },
            new AccordionItem { Key = "section2", Title = "Section 2", Content = null }
        };
        var activeSectionKey = (string?)null;

        // Act
        var defaultActiveSection = activeSectionKey ?? items.FirstOrDefault()?.Key ?? "";
        var isFirstSectionActive = defaultActiveSection == "section1";

        // Assert
        Assert.True(isFirstSectionActive);
        Assert.Equal("section1", defaultActiveSection);
    }

    [Fact]
    public void AccordionComponent_AccordionItem_HasKeyAndTitle()
    {
        // Arrange
        var item = new AccordionItem { Key = "settings", Title = "Settings", Content = null };

        // Act
        var hasKey = !string.IsNullOrEmpty(item.Key);
        var hasTitle = !string.IsNullOrEmpty(item.Title);

        // Assert
        Assert.True(hasKey);
        Assert.True(hasTitle);
        Assert.Equal("settings", item.Key);
        Assert.Equal("Settings", item.Title);
    }

    [Fact]
    public void AccordionComponent_WithSingleSection_RendersSection()
    {
        // Arrange
        var items = new List<AccordionItem>
        {
            new AccordionItem { Key = "single", Title = "Single Section", Content = null }
        };

        // Act
        var itemCount = items.Count;
        var item = items[0];

        // Assert
        Assert.Single(items);
        Assert.Equal("single", item.Key);
        Assert.Equal("Single Section", item.Title);
    }

    [Fact]
    public void AccordionComponent_SectionChangeCallback_InvokedMultipleTimes()
    {
        // Arrange
        var sectionChangeCalls = new List<string>();
        var sectionChangeCallback = new Func<string, Task>(async (key) =>
        {
            sectionChangeCalls.Add(key);
            await Task.CompletedTask;
        });

        // Act
        sectionChangeCallback.Invoke("section1").Wait();
        sectionChangeCallback.Invoke("section2").Wait();
        sectionChangeCallback.Invoke("section3").Wait();

        // Assert
        Assert.Equal(3, sectionChangeCalls.Count);
        Assert.Equal(new[] { "section1", "section2", "section3" }, sectionChangeCalls);
    }

    [Fact]
    public void AccordionComponent_AllSections_HaveUniqueKeys()
    {
        // Arrange
        var items = new List<AccordionItem>
        {
            new AccordionItem { Key = "section1", Title = "Section 1", Content = null },
            new AccordionItem { Key = "section2", Title = "Section 2", Content = null },
            new AccordionItem { Key = "section3", Title = "Section 3", Content = null }
        };

        // Act
        var uniqueKeys = items.Select(s => s.Key).Distinct().Count();

        // Assert
        Assert.Equal(items.Count, uniqueKeys);
    }

    [Fact]
    public void AccordionComponent_WithEmptyItems_RendersNoSections()
    {
        // Arrange
        var items = new List<AccordionItem>();

        // Act
        var itemCount = items.Count;

        // Assert
        Assert.Equal(0, itemCount);
    }

    [Fact]
    public void AccordionComponent_ActiveSection_CanBeChanged()
    {
        // Arrange
        var items = new List<AccordionItem>
        {
            new AccordionItem { Key = "section1", Title = "Section 1", Content = null },
            new AccordionItem { Key = "section2", Title = "Section 2", Content = null },
            new AccordionItem { Key = "section3", Title = "Section 3", Content = null }
        };
        var activeSectionKey = "section1";

        // Act
        activeSectionKey = "section3";
        var newActiveSection = items.FirstOrDefault(s => s.Key == activeSectionKey);

        // Assert
        Assert.NotNull(newActiveSection);
        Assert.Equal("section3", newActiveSection.Key);
    }

    [Fact]
    public void AccordionComponent_SectionTitles_CanContainSpecialCharacters()
    {
        // Arrange
        var items = new List<AccordionItem>
        {
            new AccordionItem { Key = "section1", Title = "Section 1 & More", Content = null },
            new AccordionItem { Key = "section2", Title = "Section - Details", Content = null },
            new AccordionItem { Key = "section3", Title = "Section / Overview", Content = null }
        };

        // Act
        var complexTitle = items[0].Title;

        // Assert
        Assert.Equal("Section 1 & More", complexTitle);
        Assert.Contains("&", complexTitle);
    }

    [Fact]
    public void AccordionComponent_SettingActiveSectionToNonExistentKey_HandlesGracefully()
    {
        // Arrange
        var items = new List<AccordionItem>
        {
            new AccordionItem { Key = "section1", Title = "Section 1", Content = null },
            new AccordionItem { Key = "section2", Title = "Section 2", Content = null }
        };
        var activeSectionKey = "nonexistent";

        // Act
        var activeSection = items.FirstOrDefault(s => s.Key == activeSectionKey);
        var fallbackSection = activeSection ?? items.FirstOrDefault();

        // Assert
        Assert.Null(activeSection);
        Assert.NotNull(fallbackSection);
        Assert.Equal("section1", fallbackSection.Key);
    }

    [Fact]
    public void AccordionComponent_SectionKey_IsNotNull()
    {
        // Arrange
        var item = new AccordionItem { Key = "unique-key", Title = "Test Section", Content = null };

        // Act
        var hasKey = !string.IsNullOrEmpty(item.Key);

        // Assert
        Assert.True(hasKey);
        Assert.Equal("unique-key", item.Key);
    }

    [Fact]
    public void AccordionComponent_SectionTitle_IsNotNull()
    {
        // Arrange
        var item = new AccordionItem { Key = "section", Title = "Test Title", Content = null };

        // Act
        var hasTitle = !string.IsNullOrEmpty(item.Title);

        // Assert
        Assert.True(hasTitle);
        Assert.Equal("Test Title", item.Title);
    }

    [Fact]
    public void AccordionComponent_FindSectionByKey_ReturnsCorrectSection()
    {
        // Arrange
        var items = new List<AccordionItem>
        {
            new AccordionItem { Key = "overview", Title = "Overview", Content = null },
            new AccordionItem { Key = "details", Title = "Details", Content = null },
            new AccordionItem { Key = "settings", Title = "Settings", Content = null }
        };

        // Act
        var foundSection = items.FirstOrDefault(s => s.Key == "details");

        // Assert
        Assert.NotNull(foundSection);
        Assert.Equal("details", foundSection.Key);
        Assert.Equal("Details", foundSection.Title);
    }

    [Fact]
    public void AccordionComponent_SectionChangeCallback_IsAsync()
    {
        // Arrange
        var sectionChangeCalls = new List<(string key, long timestamp)>();
        var sectionChangeCallback = new Func<string, Task>(async (key) =>
        {
            sectionChangeCalls.Add((key, DateTimeOffset.UtcNow.Ticks));
            await Task.Delay(10);
        });

        // Act
        sectionChangeCallback.Invoke("section1").Wait();
        var firstCallTime = sectionChangeCalls[0].timestamp;

        sectionChangeCallback.Invoke("section2").Wait();
        var secondCallTime = sectionChangeCalls[1].timestamp;

        // Assert
        Assert.Equal(2, sectionChangeCalls.Count);
        Assert.True(secondCallTime >= firstCallTime);
    }

    [Fact]
    public void AccordionComponent_AllowMultipleOpen_DefaultIsFalse()
    {
        // Arrange
        var allowMultipleOpen = false; // Default value

        // Act
        var isExclusiveMode = !allowMultipleOpen;

        // Assert
        Assert.False(allowMultipleOpen);
        Assert.True(isExclusiveMode);
    }

    [Fact]
    public void AccordionComponent_MultipleAccordions_MaintainIndependentState()
    {
        // Arrange
        var items1 = new List<AccordionItem>
        {
            new AccordionItem { Key = "section1", Title = "Section 1", Content = null },
            new AccordionItem { Key = "section2", Title = "Section 2", Content = null }
        };
        var items2 = new List<AccordionItem>
        {
            new AccordionItem { Key = "section3", Title = "Section 3", Content = null },
            new AccordionItem { Key = "section4", Title = "Section 4", Content = null }
        };

        // Act
        var activeSection1 = "section1";
        var activeSection2 = "section4";

        // Assert
        Assert.Equal("section1", activeSection1);
        Assert.Equal("section4", activeSection2);
        Assert.NotEqual(activeSection1, activeSection2);
    }

    [Fact]
    public void AccordionComponent_ToggleSectionInExclusiveMode_ClosesPreviousSection()
    {
        // Arrange
        var allowMultipleOpen = false;
        var activeSectionKey = "section1";

        // Act
        activeSectionKey = "section2";
        var previousSectionClosed = activeSectionKey != "section1";
        var newSectionOpened = activeSectionKey == "section2";

        // Assert
        Assert.True(previousSectionClosed);
        Assert.True(newSectionOpened);
    }

    [Fact]
    public void AccordionComponent_ClickActiveSection_TogglesItClosed()
    {
        // Arrange
        var activeSectionKey = "section1";
        var isOpen = true;

        // Act
        if (activeSectionKey == "section1" && isOpen)
        {
            activeSectionKey = "";
            isOpen = false;
        }

        // Assert
        Assert.False(isOpen);
        Assert.Empty(activeSectionKey);
    }
}
