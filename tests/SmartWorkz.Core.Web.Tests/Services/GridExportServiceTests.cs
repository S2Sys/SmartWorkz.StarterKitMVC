using Xunit;
using SmartWorkz.Web.Services.Grid;
using SmartWorkz.Shared;

namespace SmartWorkz.Core.Web.Tests.Services;

public class GridExportServiceTests
{
    [Fact]
    public void ExportToCsv_ShouldGenerateValidCsv()
    {
        // Arrange
        var service = new GridExportService();
        var data = new List<ProductDto>
        {
            new() { Id = 1, Name = "Product A", Price = 100 },
            new() { Id = 2, Name = "Product B", Price = 200 }
        };
        var columns = new List<GridColumn>
        {
            new() { PropertyName = "Id", DisplayName = "ID" },
            new() { PropertyName = "Name", DisplayName = "Product Name" },
            new() { PropertyName = "Price", DisplayName = "Price" }
        };
        var options = new GridExportOptions { Format = "csv", FileName = "products" };

        // Act
        var result = service.ExportToCsv(data, columns, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("ID,Product Name,Price", result);
        Assert.Contains("1,Product A,100", result);
    }

    private class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}


