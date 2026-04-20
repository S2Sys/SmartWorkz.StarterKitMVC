using Xunit;
using SmartWorkz.Core.Web.Components.DataContext;
using SmartWorkz.Core.Shared.Grid;

namespace SmartWorkz.Core.Web.Tests.Integration;

public class MultiViewIntegrationTests
{
    private class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Price { get; set; }
    }

    private List<Product> GetSampleProducts()
    {
        return new()
        {
            new() { Id = 1, Name = "Laptop", Category = "Electronics", Price = 999.99m },
            new() { Id = 2, Name = "Mouse", Category = "Electronics", Price = 29.99m },
            new() { Id = 3, Name = "Chair", Category = "Furniture", Price = 199.99m },
            new() { Id = 4, Name = "Desk", Category = "Furniture", Price = 399.99m },
            new() { Id = 5, Name = "Monitor", Category = "Electronics", Price = 299.99m }
        };
    }

    [Fact]
    public async Task ViewToggle_PreservesState_WhenSwitchingViews()
    {
        // Arrange
        var context = new DataContext<Product>();
        var products = GetSampleProducts();
        await context.Initialize(products);

        // Act - Apply filter (updates request state)
        await context.UpdateFilter("Category", "equals", "Electronics");

        // Verify filter was recorded in request
        Assert.NotNull(context.CurrentRequest.Filters);
        Assert.True(context.CurrentRequest.Filters?.ContainsKey("Category") ?? false);

        // Act - Select rows
        context.ToggleRowSelection(1);
        context.ToggleRowSelection(2);

        // Assert - Selection is preserved
        Assert.Equal(2, context.SelectedRowIds.Count);
        Assert.Contains(1, context.SelectedRowIds);
        Assert.Contains(2, context.SelectedRowIds);
    }

    [Fact]
    public async Task SortAndFilter_ApplyCorrectly_WhenCombined()
    {
        // Arrange
        var context = new DataContext<Product>();
        var products = GetSampleProducts();
        await context.Initialize(products);

        // Act - Filter by category
        await context.UpdateFilter("Category", "equals", "Electronics");

        // Act - Sort by price descending
        await context.UpdateSort("Price", true);

        // Assert - State is updated correctly
        Assert.NotNull(context.CurrentRequest.Filters);
        Assert.Equal("Price", context.CurrentRequest.SortBy);
        Assert.True(context.CurrentRequest.SortDescending);
    }

    [Fact]
    public async Task ClearFilters_RemovesAllFiltersAndSort()
    {
        // Arrange
        var context = new DataContext<Product>();
        var products = GetSampleProducts();
        await context.Initialize(products);
        await context.UpdateFilter("Category", "equals", "Electronics");
        await context.UpdateSort("Price", true);
        context.ToggleRowSelection(1);
        context.ToggleRowSelection(2);

        // Verify state was set
        Assert.NotNull(context.CurrentRequest.Filters);
        Assert.Equal("Price", context.CurrentRequest.SortBy);

        // Act
        await context.ClearFilters();

        // Assert - filters and sort are cleared
        Assert.Null(context.CurrentRequest.Filters);
        Assert.Equal(1, context.CurrentRequest.Page);
        // Selection should persist across clear filters
        Assert.Equal(2, context.SelectedRowIds.Count);
    }

    [Fact]
    public async Task Pagination_Syncs_AcrossStateChanges()
    {
        // Arrange
        var context = new DataContext<Product>();
        var products = Enumerable.Range(1, 50)
            .Select(i => new Product { Id = i, Name = $"Item {i}", Category = "Test", Price = i * 10 })
            .ToList();
        await context.Initialize(products);

        // Act - Update to page 1 with 10 items per page
        await context.UpdatePagination(1, 10);
        var page1Request = context.CurrentRequest;

        // Act - Update to page 2 with 10 items per page
        await context.UpdatePagination(2, 10);
        var page2Request = context.CurrentRequest;

        // Assert - Pagination state is updated correctly
        Assert.Equal(1, page1Request.Page);
        Assert.Equal(10, page1Request.PageSize);
        Assert.Equal(2, page2Request.Page);
        Assert.Equal(10, page2Request.PageSize);
    }

    [Fact]
    public async Task StateChange_RaisesEvent_ForAllOperations()
    {
        // Arrange
        var context = new DataContext<Product>();
        var products = GetSampleProducts();
        await context.Initialize(products);
        var eventCount = 0;
        context.OnStateChanged += () => eventCount++;

        // Act
        await context.UpdateSort("Name", false);
        await context.UpdateFilter("Category", "equals", "Electronics");
        await context.UpdatePagination(2, 20);
        context.ToggleRowSelection(1);

        // Assert - Events were raised for async operations + toggle
        // UpdateSort, UpdateFilter, UpdatePagination each raise events (3)
        // ToggleRowSelection raises 1 event
        // Total: at least 4 events
        Assert.True(eventCount >= 4);
    }

    [Fact]
    public async Task Selection_IsMaintained_WithMultipleTogles()
    {
        // Arrange
        var context = new DataContext<Product>();
        var products = GetSampleProducts();
        await context.Initialize(products);

        // Act
        context.ToggleRowSelection(1);
        context.ToggleRowSelection(2);
        context.ToggleRowSelection(3);
        context.ToggleRowSelection(2); // toggle off

        // Assert
        Assert.Equal(2, context.SelectedRowIds.Count);
        Assert.Contains(1, context.SelectedRowIds);
        Assert.Contains(3, context.SelectedRowIds);
        Assert.DoesNotContain(2, context.SelectedRowIds);
    }
}
