using Xunit;
using SmartWorkz.Web.Services.Grid;
using SmartWorkz.Shared;
using SmartWorkz.Shared;

namespace SmartWorkz.Core.Web.Tests.Components;

public class GridComponentTests
{
    [Fact]
    public void GridComponent_ShouldRenderWithColumns()
    {
        // Arrange
        var columns = new List<GridColumn>
        {
            new() { PropertyName = "Id", DisplayName = "ID", IsSortable = true },
            new() { PropertyName = "Name", DisplayName = "Product Name", IsSortable = true },
            new() { PropertyName = "Price", DisplayName = "Price", IsFilterable = true, FilterType = "range" }
        };

        // Act
        var columnCount = columns.Count;

        // Assert
        Assert.Equal(3, columnCount);
        Assert.True(columns[0].IsSortable);
        Assert.True(columns[1].IsSortable);
        Assert.True(columns[2].IsFilterable);
    }

    [Fact]
    public void GridDataProvider_ShouldApplyInMemorySorting()
    {
        // Arrange
        var data = new List<ProductDto>
        {
            new() { Id = 3, Name = "C Product", Price = 300 },
            new() { Id = 1, Name = "A Product", Price = 100 },
            new() { Id = 2, Name = "B Product", Price = 200 }
        };
        var request = new GridRequest(SortBy: "Name", Page: 1, PageSize: 10);

        // Act
        var result = GridDataProvider.ApplyGridLogic(data, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal("A Product", result.Items.First().Name);
        Assert.Equal("C Product", result.Items.Last().Name);
    }

    [Fact]
    public void GridDataProvider_ShouldApplyPaging()
    {
        // Arrange
        var data = Enumerable.Range(1, 100)
            .Select(i => new ProductDto { Id = i, Name = $"Product {i}", Price = i * 10 })
            .ToList();
        var request = new GridRequest(Page: 2, PageSize: 25);

        // Act
        var result = GridDataProvider.ApplyGridLogic(data, request);

        // Assert
        Assert.Equal(100, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(25, result.PageSize);
        Assert.Equal(4, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
        Assert.Equal(25, result.Items.Count);
        Assert.Equal(26, result.Items.First().Id);  // First item of page 2
    }

    [Fact]
    public void GridExportService_ShouldExportSelectedColumnsOnly()
    {
        // Arrange
        var service = new GridExportService();
        var data = new List<ProductDto>
        {
            new() { Id = 1, Name = "Product A", Price = 100 }
        };
        var columns = new List<GridColumn>
        {
            new() { PropertyName = "Id", DisplayName = "ID" },
            new() { PropertyName = "Name", DisplayName = "Product Name" },
            new() { PropertyName = "Price", DisplayName = "Price" }
        };
        var options = new GridExportOptions
        {
            IncludeColumns = ["Id", "Name"],
            IncludeHeaders = true
        };

        // Act
        var csv = service.ExportToCsv(data, columns, options);

        // Assert
        Assert.Contains("ID,Product Name", csv);
        Assert.DoesNotContain("Price", csv);
    }

    private class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}



