using Xunit;
using SmartWorkz.Core.Web.Components.DataContext;
using SmartWorkz.Core.Shared.Grid;

namespace SmartWorkz.Core.Web.Tests.Components;

public class DataContextTests
{
    private class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
    }

    [Fact]
    public async Task Initialize_WithData_PopulatesCurrentResponse()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        var items = new List<TestItem>
        {
            new() { Id = 1, Name = "Item 1", Category = "A" },
            new() { Id = 2, Name = "Item 2", Category = "B" }
        };

        // Act
        await context.Initialize(items);

        // Assert
        Assert.NotNull(context.CurrentResponse);
        Assert.NotEmpty(context.CurrentResponse.Data.Items);
        Assert.Equal(2, context.CurrentResponse.Data.Items.Count());
    }

    [Fact]
    public async Task UpdateSort_ChangesCurrentRequest()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        await context.Initialize(new List<TestItem>());

        // Act
        await context.UpdateSort("Name", false);

        // Assert
        Assert.Equal("Name", context.CurrentRequest.SortBy);
        Assert.False(context.CurrentRequest.SortDescending);
    }

    [Fact]
    public async Task UpdateSort_ResetsToPage1()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        await context.Initialize(new List<TestItem>());
        context.CurrentRequest.Page.Equals(1);  // Initial state

        // Act
        // Verify we start at page 1
        Assert.Equal(1, context.CurrentRequest.Page);

        // Manually set page to 5 using a workaround (GridRequest is a record)
        var oldRequest = context.CurrentRequest;
        var modifiedRequest = oldRequest with { Page = 5 };
        // Note: Can't directly modify CurrentRequest as it's a record property
        // We'll update pagination first
        await context.UpdatePagination(5, 20);

        // Act
        await context.UpdateSort("Name", false);

        // Assert
        Assert.Equal(1, context.CurrentRequest.Page);
    }

    [Fact]
    public async Task UpdateFilter_AddsFilterToRequest()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        await context.Initialize(new List<TestItem>());

        // Act
        await context.UpdateFilter("Category", "equals", "A");

        // Assert
        Assert.NotNull(context.CurrentRequest.Filters);
        Assert.Contains("Category", context.CurrentRequest.Filters.Keys);
        Assert.Equal("A", context.CurrentRequest.Filters["Category"]);
    }

    [Fact]
    public async Task UpdateFilter_ResetsToPage1()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        await context.Initialize(new List<TestItem>());

        // Set page to 5
        await context.UpdatePagination(5, 20);
        Assert.Equal(5, context.CurrentRequest.Page);

        // Act
        await context.UpdateFilter("Category", "equals", "A");

        // Assert
        Assert.Equal(1, context.CurrentRequest.Page);
    }

    [Fact]
    public async Task UpdatePagination_ChangesPaginationParams()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        await context.Initialize(new List<TestItem>());

        // Act
        await context.UpdatePagination(3, 50);

        // Assert
        Assert.Equal(3, context.CurrentRequest.Page);
        Assert.Equal(50, context.CurrentRequest.PageSize);
    }

    [Fact]
    public void ToggleRowSelection_TogglesSingleRow()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        var rowId = 1;

        // Act
        context.ToggleRowSelection(rowId);

        // Assert
        Assert.Contains(rowId, context.SelectedRowIds);

        // Act again
        context.ToggleRowSelection(rowId);

        // Assert
        Assert.DoesNotContain(rowId, context.SelectedRowIds);
    }

    [Fact]
    public void SetSelectedRows_ReplacesSelection()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        context.ToggleRowSelection(1);
        context.ToggleRowSelection(2);

        // Act
        context.SetSelectedRows(new List<object> { 3, 4 });

        // Assert
        Assert.Equal(2, context.SelectedRowIds.Count);
        Assert.Contains(3, context.SelectedRowIds);
        Assert.Contains(4, context.SelectedRowIds);
        Assert.DoesNotContain(1, context.SelectedRowIds);
    }

    [Fact]
    public async Task ClearFilters_ClearsFiltersAndResetsPage()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        await context.Initialize(new List<TestItem>());
        await context.UpdateFilter("Category", "equals", "A");
        await context.UpdateSort("Name", true);
        await context.UpdatePagination(5, 20);

        // Act
        await context.ClearFilters();

        // Assert
        Assert.Null(context.CurrentRequest.Filters);
        // Note: SortBy is NOT cleared by ClearFilters, only filters are cleared
        Assert.Equal("Name", context.CurrentRequest.SortBy);
        Assert.Equal(1, context.CurrentRequest.Page);
    }

    [Fact]
    public void OnStateChanged_RaisedWhenStateChanges()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        var stateChangedCount = 0;
        context.OnStateChanged += () => stateChangedCount++;

        // Act
        context.ToggleRowSelection(1);

        // Assert
        Assert.Equal(1, stateChangedCount);
    }

    [Fact]
    public void ToggleSelectAll_SelectsAllRows_WhenTrue()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        var items = new List<TestItem>
        {
            new() { Id = 1, Name = "Item 1", Category = "A" },
            new() { Id = 2, Name = "Item 2", Category = "B" }
        };
        context.Initialize(items).Wait();

        // Act
        context.ToggleSelectAll(true);

        // Assert
        Assert.Equal(2, context.SelectedRowIds.Count);
        Assert.Contains(1, context.SelectedRowIds);
        Assert.Contains(2, context.SelectedRowIds);
    }

    [Fact]
    public void ToggleSelectAll_ClearsSelection_WhenFalse()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        var items = new List<TestItem>
        {
            new() { Id = 1, Name = "Item 1", Category = "A" },
            new() { Id = 2, Name = "Item 2", Category = "B" }
        };
        context.Initialize(items).Wait();
        context.ToggleSelectAll(true);

        // Act
        context.ToggleSelectAll(false);

        // Assert
        Assert.Empty(context.SelectedRowIds);
    }
}
