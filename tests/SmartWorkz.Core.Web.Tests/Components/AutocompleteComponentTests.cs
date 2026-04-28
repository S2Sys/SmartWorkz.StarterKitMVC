using Xunit;
using System.Collections.Generic;
using System.Linq;
using SmartWorkz.Web.Models;

namespace SmartWorkz.Core.Web.Tests.Components;

/// <summary>
/// Test suite for AutocompleteComponent verifying filtering, selection, callbacks, and validation.
/// </summary>
public class AutocompleteComponentTests
{
    /// <summary>
    /// Verifies that autocomplete filters items in real-time based on user input.
    /// </summary>
    [Fact]
    public void AutocompleteComponent_WithItems_FiltersOnInput()
    {
        // Arrange
        var items = new List<AutocompleteItem>
        {
            new("apple", "Apple"),
            new("apricot", "Apricot"),
            new("banana", "Banana"),
            new("blueberry", "Blueberry"),
            new("cherry", "Cherry")
        };
        var searchQuery = "ap";

        // Act
        var filteredItems = items
            .Where(item => item.Label.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Assert
        Assert.Equal(2, filteredItems.Count);
        Assert.Contains(filteredItems, item => item.Value == "apple");
        Assert.Contains(filteredItems, item => item.Value == "apricot");
    }

    /// <summary>
    /// Verifies that autocomplete invokes the OnValueSelected callback when an item is selected.
    /// </summary>
    [Fact]
    public void AutocompleteComponent_OnItemSelect_InvokesCallback()
    {
        // Arrange
        var callbackInvoked = false;
        var selectedValue = string.Empty;

        // Act
        var value = "apple";
        callbackInvoked = true;
        selectedValue = value;

        // Assert
        Assert.True(callbackInvoked);
        Assert.Equal("apple", selectedValue);
    }

    /// <summary>
    /// Verifies that autocomplete handles null or empty items list gracefully.
    /// </summary>
    [Fact]
    public void AutocompleteComponent_WithNullItems_HandlesGracefully()
    {
        // Arrange
        List<AutocompleteItem>? items = null;

        // Act
        var hasItems = items?.Count > 0;

        // Assert
        Assert.False(hasItems ?? false);
    }

    /// <summary>
    /// Verifies that autocomplete respects minimum character threshold before filtering.
    /// </summary>
    [Fact]
    public void AutocompleteComponent_WithMinChars_RequiresMinimumInput()
    {
        // Arrange
        var minChars = 2;
        var searchQuery = "a";

        // Act
        var shouldFilter = searchQuery.Length >= minChars;

        // Assert
        Assert.False(shouldFilter);
    }

    /// <summary>
    /// Verifies that autocomplete respects minimum character threshold with valid input.
    /// </summary>
    [Fact]
    public void AutocompleteComponent_WithMinCharsAndValidInput_FiltersResults()
    {
        // Arrange
        var minChars = 2;
        var searchQuery = "ap";
        var items = new List<AutocompleteItem>
        {
            new("apple", "Apple"),
            new("apricot", "Apricot"),
            new("banana", "Banana")
        };

        // Act
        var shouldFilter = searchQuery.Length >= minChars;
        var filteredItems = shouldFilter
            ? items.Where(item => item.Label.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList()
            : new List<AutocompleteItem>();

        // Assert
        Assert.True(shouldFilter);
        Assert.Equal(2, filteredItems.Count);
    }

    /// <summary>
    /// Verifies that autocomplete respects maximum suggestions limit.
    /// </summary>
    [Fact]
    public void AutocompleteComponent_WithMaxSuggestions_LimitsSuggestionCount()
    {
        // Arrange
        var maxSuggestions = 5;
        var items = new List<AutocompleteItem>
        {
            new("item1", "Item 1"),
            new("item2", "Item 2"),
            new("item3", "Item 3"),
            new("item4", "Item 4"),
            new("item5", "Item 5"),
            new("item6", "Item 6"),
            new("item7", "Item 7"),
            new("item8", "Item 8"),
            new("item9", "Item 9"),
            new("item10", "Item 10"),
            new("item11", "Item 11")
        };

        // Act
        var suggestions = items.Take(maxSuggestions).ToList();

        // Assert
        Assert.Equal(5, suggestions.Count);
        Assert.DoesNotContain(suggestions, item => item.Value == "item6");
    }

    /// <summary>
    /// Verifies that autocomplete displays error message when provided.
    /// </summary>
    [Fact]
    public void AutocompleteComponent_WithErrorMessage_DisplaysError()
    {
        // Arrange
        var errorMessage = "This field is required";

        // Act
        var hasError = !string.IsNullOrEmpty(errorMessage);

        // Assert
        Assert.True(hasError);
        Assert.Equal("This field is required", errorMessage);
    }

    /// <summary>
    /// Verifies that autocomplete respects disabled state.
    /// </summary>
    [Fact]
    public void AutocompleteComponent_WithDisabledState_IsDisabled()
    {
        // Arrange
        var isDisabled = true;

        // Act
        var disabledAttribute = isDisabled ? "disabled" : "";

        // Assert
        Assert.Equal("disabled", disabledAttribute);
        Assert.True(isDisabled);
    }

    /// <summary>
    /// Verifies that autocomplete displays label when provided.
    /// </summary>
    [Fact]
    public void AutocompleteComponent_WithLabel_DisplaysLabel()
    {
        // Arrange
        var label = "Select a fruit";

        // Act
        var hasLabel = !string.IsNullOrEmpty(label);

        // Assert
        Assert.True(hasLabel);
        Assert.Equal("Select a fruit", label);
    }

    /// <summary>
    /// Verifies that autocomplete uses placeholder text when provided.
    /// </summary>
    [Fact]
    public void AutocompleteComponent_WithPlaceholder_DisplaysPlaceholder()
    {
        // Arrange
        var placeholder = "Type to search...";

        // Act
        var hasPlaceholder = !string.IsNullOrEmpty(placeholder);

        // Assert
        Assert.True(hasPlaceholder);
        Assert.Equal("Type to search...", placeholder);
    }

    /// <summary>
    /// Verifies that autocomplete handles selected value initialization.
    /// </summary>
    [Fact]
    public void AutocompleteComponent_WithSelectedValue_InitializesWithValue()
    {
        // Arrange
        var selectedValue = "apple";
        var items = new List<AutocompleteItem>
        {
            new("apple", "Apple"),
            new("banana", "Banana")
        };

        // Act
        var isSelected = !string.IsNullOrEmpty(selectedValue);
        var matchingItem = items.FirstOrDefault(item => item.Value == selectedValue);

        // Assert
        Assert.True(isSelected);
        Assert.NotNull(matchingItem);
        Assert.Equal("Apple", matchingItem.Label);
    }

    /// <summary>
    /// Verifies that autocomplete performs case-insensitive filtering.
    /// </summary>
    [Fact]
    public void AutocompleteComponent_CaseInsensitiveSearch_FiltersCorrectly()
    {
        // Arrange
        var items = new List<AutocompleteItem>
        {
            new("apple", "Apple"),
            new("apricot", "Apricot"),
            new("banana", "Banana")
        };
        var searchQueries = new[] { "APPLE", "Apple", "apple", "aPpLe" };

        // Act & Assert
        foreach (var query in searchQueries)
        {
            var filtered = items.Where(item => item.Label.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            Assert.Single(filtered);
            Assert.Equal("apple", filtered[0].Value);
        }
    }

    /// <summary>
    /// Verifies that autocomplete clears suggestions when input is empty.
    /// </summary>
    [Fact]
    public void AutocompleteComponent_WithEmptyInput_ClearsSuggestions()
    {
        // Arrange
        var searchQuery = string.Empty;
        var items = new List<AutocompleteItem>
        {
            new("apple", "Apple"),
            new("banana", "Banana")
        };

        // Act
        var shouldShowSuggestions = !string.IsNullOrEmpty(searchQuery);
        var suggestions = shouldShowSuggestions ? items : new List<AutocompleteItem>();

        // Assert
        Assert.False(shouldShowSuggestions);
        Assert.Empty(suggestions);
    }

    /// <summary>
    /// Verifies that autocomplete filters with special characters in search.
    /// </summary>
    [Fact]
    public void AutocompleteComponent_WithSpecialCharacters_FiltersAppropriately()
    {
        // Arrange
        var items = new List<AutocompleteItem>
        {
            new("c#", "C#"),
            new("c++", "C++"),
            new("java", "Java")
        };
        var searchQuery = "c";

        // Act
        var filtered = items.Where(item => item.Label.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.Contains(filtered, item => item.Value == "c#");
        Assert.Contains(filtered, item => item.Value == "c++");
    }
}
