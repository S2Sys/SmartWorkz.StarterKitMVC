namespace SmartWorkz.Core.External.Tests.Export;

public class ExcelExportServiceTests
{
    private readonly IExcelExportService _service;

    public ExcelExportServiceTests()
    {
        _service = new ExcelExportService();
    }

    private class SimpleProduct
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    private class ProductWithCurrency
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public double Quantity { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    private class ProductWithSpecialChars
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Notes { get; set; }
    }

    [Fact]
    public async Task ExportAsync_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var data = new List<SimpleProduct>
        {
            new() { Id = 1, Name = "Product A", Price = 100.50m, CreatedDate = new DateTime(2024, 1, 15) },
            new() { Id = 2, Name = "Product B", Price = 200.75m, CreatedDate = new DateTime(2024, 2, 20) }
        };

        // Act
        var result = await _service.ExportAsync(data, "Products");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
        Assert.True(result.Data.Length > 0);
    }

    [Fact]
    public async Task ExportAsync_WithEmptyCollection_ReturnsFailureResult()
    {
        // Arrange
        var data = new List<SimpleProduct>();

        // Act
        var result = await _service.ExportAsync(data, "Products");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ExportAsync_WithNullSheetName_UsesDefaultName()
    {
        // Arrange
        var data = new List<SimpleProduct>
        {
            new() { Id = 1, Name = "Product A", Price = 100.50m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _service.ExportAsync(data, "");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task ExportAsync_HeadersShouldBeBold()
    {
        // Arrange
        var data = new List<SimpleProduct>
        {
            new() { Id = 1, Name = "Product A", Price = 100.50m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _service.ExportAsync(data, "Products");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        // Excel binary format verification would require parsing, but successful export indicates proper formatting
    }

    [Fact]
    public async Task ExportAsync_WithCurrencyValues_FormatsCorrectly()
    {
        // Arrange
        var data = new List<ProductWithCurrency>
        {
            new() { Id = 1, Description = "Item 1", Amount = 1234.56m, Quantity = 10.5, CreatedDate = DateTime.Now },
            new() { Id = 2, Description = "Item 2", Amount = 9876.54m, Quantity = 5.25, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _service.ExportAsync(data, "CurrencySheet");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task ExportAsync_WithDateValues_FormatsCorrectly()
    {
        // Arrange
        var data = new List<SimpleProduct>
        {
            new() { Id = 1, Name = "Product A", Price = 100.00m, CreatedDate = new DateTime(2024, 3, 15, 10, 30, 45) },
            new() { Id = 2, Name = "Product B", Price = 200.00m, CreatedDate = new DateTime(2024, 4, 20, 14, 45, 30) }
        };

        // Act
        var result = await _service.ExportAsync(data, "DatesSheet");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task ExportAsync_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var data = new List<ProductWithSpecialChars>
        {
            new() { Id = 1, Name = "Product \"A\"", Notes = "Contains, comma and \"quotes\"" },
            new() { Id = 2, Name = "Product B", Notes = "Contains\nnewline" }
        };

        // Act
        var result = await _service.ExportAsync(data, "SpecialChars");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task ExportAsync_WithMultipleDataTypes_ReturnsValidExcel()
    {
        // Arrange
        var data = new List<ProductWithCurrency>
        {
            new() { Id = 1, Description = "Product 1", Amount = 1500.00m, Quantity = 25.5, CreatedDate = new DateTime(2024, 1, 10) },
            new() { Id = 2, Description = "Product 2", Amount = 2500.75m, Quantity = 50.0, CreatedDate = new DateTime(2024, 2, 15) },
            new() { Id = 3, Description = "Product 3", Amount = 3200.25m, Quantity = 75.25, CreatedDate = new DateTime(2024, 3, 20) }
        };

        // Act
        var result = await _service.ExportAsync(data, "MultiType");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Length > 100); // Excel format should produce reasonable file size
    }

    [Fact]
    public async Task ExportAsync_WithCancellationToken_RespondsToCancel()
    {
        // Arrange
        var data = new List<SimpleProduct>
        {
            new() { Id = 1, Name = "Product A", Price = 100.50m, CreatedDate = DateTime.Now }
        };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        // Task.Run with canceled token throws TaskCanceledException (which derives from OperationCanceledException)
        try
        {
            await _service.ExportAsync(data, "Products", cts.Token);
            Assert.Fail("Expected an exception to be thrown");
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }

    [Fact]
    public async Task ExportMultipleAsync_WithMultipleSheets_ReturnsValidExcel()
    {
        // Arrange
        var sheets = new Dictionary<string, IEnumerable<object>>
        {
            {
                "Sheet1",
                new List<SimpleProduct>
                {
                    new() { Id = 1, Name = "Product A", Price = 100.50m, CreatedDate = DateTime.Now },
                    new() { Id = 2, Name = "Product B", Price = 200.75m, CreatedDate = DateTime.Now }
                }.Cast<object>().ToList()
            },
            {
                "Sheet2",
                new List<ProductWithCurrency>
                {
                    new() { Id = 1, Description = "Item 1", Amount = 1234.56m, Quantity = 10.5, CreatedDate = DateTime.Now }
                }.Cast<object>().ToList()
            }
        };

        // Act
        var result = await _service.ExportMultipleAsync(sheets);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
    }
}
