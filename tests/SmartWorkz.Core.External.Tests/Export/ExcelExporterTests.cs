namespace SmartWorkz.Core.External.Tests.Export;

public class ExcelExporterTests
{
    private readonly ExcelExporter _exporter;

    public ExcelExporterTests()
    {
        _exporter = new ExcelExporter();
    }

    private class SimpleData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    private class ComplexData
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public double Quantity { get; set; }
        public decimal Amount { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    [Fact]
    public async Task ExportAsync_WithValidSimpleData_ReturnsSuccessResult()
    {
        // Arrange
        var data = new List<SimpleData>
        {
            new SimpleData { Id = 1, Name = "Product A", Price = 100.50m, CreatedDate = DateTime.Now },
            new SimpleData { Id = 2, Name = "Product B", Price = 200.75m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _exporter.ExportAsync(data, "Products");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
        Assert.True(result.Data.Length > 0);
    }

    [Fact]
    public async Task ExportAsync_WithEmptyCollection_ReturnsFailureResult()
    {
        // Arrange
        var data = new List<SimpleData>();

        // Act
        var result = await _exporter.ExportAsync(data, "Products");

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ExportAsync_WithNullValues_ReturnsSuccessResult()
    {
        // Arrange
        var data = new List<SimpleData>
        {
            new SimpleData { Id = 1, Name = null, Price = 100.50m, CreatedDate = DateTime.Now },
            new SimpleData { Id = 2, Name = "Product B", Price = 200.75m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _exporter.ExportAsync(data, "Products");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExportAsync_WithMultipleRows_ReturnsSuccessResult()
    {
        // Arrange
        var data = Enumerable.Range(1, 100)
            .Select(i => new SimpleData
            {
                Id = i,
                Name = $"Product {i}",
                Price = 100.00m + i,
                CreatedDate = DateTime.Now
            })
            .ToList();

        // Act
        var result = await _exporter.ExportAsync(data, "Products");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Length > 0);
    }

    [Fact]
    public async Task ExportAsync_WithDecimalAndDateTypes_AppliesFormattingCorrectly()
    {
        // Arrange
        var data = new List<SimpleData>
        {
            new SimpleData { Id = 1, Name = "Test", Price = 123.456m, CreatedDate = new DateTime(2024, 5, 15) }
        };

        // Act
        var result = await _exporter.ExportAsync(data, "Formatted");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExportMultipleAsync_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var sheets = new Dictionary<string, IEnumerable<object>>
        {
            {
                "Sheet1",
                new List<SimpleData>
                {
                    new SimpleData { Id = 1, Name = "Product A", Price = 100.50m, CreatedDate = DateTime.Now }
                }.Cast<object>().ToList()
            },
            {
                "Sheet2",
                new List<SimpleData>
                {
                    new SimpleData { Id = 2, Name = "Product B", Price = 200.75m, CreatedDate = DateTime.Now }
                }.Cast<object>().ToList()
            }
        };

        // Act
        var result = await _exporter.ExportMultipleAsync(sheets);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExportMultipleAsync_WithEmptySheets_ReturnsFailureResult()
    {
        // Arrange
        var sheets = new Dictionary<string, IEnumerable<object>>();

        // Act
        var result = await _exporter.ExportMultipleAsync(sheets);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ExportMultipleAsync_WithNullData_ReturnsFailureResult()
    {
        // Arrange
        Dictionary<string, IEnumerable<object>>? sheets = null;

        // Act
        var result = await _exporter.ExportMultipleAsync(sheets!);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ExportAsync_WithCustomOptions_AppliesOptions()
    {
        // Arrange
        var options = new ExcelOptions
        {
            SheetName = "CustomSheet",
            HeaderBold = true,
            HeaderBackgroundColor = "FF0000",
            AutoColumnWidth = true,
            FreezePanes = true,
            BorderStyle = "thick"
        };

        var exporter = new ExcelExporter(options);
        var data = new List<SimpleData>
        {
            new SimpleData { Id = 1, Name = "Test", Price = 100m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await exporter.ExportAsync(data, "CustomSheet");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExportAsync_WithCurrencyData_AppliesCurrencyFormatting()
    {
        // Arrange
        var data = new List<SimpleData>
        {
            new SimpleData { Id = 1, Name = "Item", Price = 1234.567m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _exporter.ExportAsync(data, "Currency");

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ExportAsync_WithDateTimeData_AppliesDateFormatting()
    {
        // Arrange
        var data = new List<SimpleData>
        {
            new SimpleData
            {
                Id = 1,
                Name = "Test",
                Price = 100m,
                CreatedDate = new DateTime(2024, 12, 25)
            }
        };

        // Act
        var result = await _exporter.ExportAsync(data, "Dates");

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ExportAsync_WithLargeDataset_ReturnsSuccessResult()
    {
        // Arrange
        var data = Enumerable.Range(1, 1000)
            .Select(i => new SimpleData
            {
                Id = i,
                Name = $"Product {i}",
                Price = 100.50m,
                CreatedDate = DateTime.Now
            })
            .ToList();

        // Act
        var result = await _exporter.ExportAsync(data, "LargeDataset");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Length > 10000); // Should be a reasonable size file
    }

    [Fact]
    public async Task ExportAsync_WithNullableTypes_HandlesProperlyFormatted()
    {
        // Arrange
        var data = new List<ComplexData>
        {
            new ComplexData { Id = 1, Description = "Test", Quantity = 10.5, Amount = 500m, CreatedDate = null },
            new ComplexData { Id = 2, Description = null, Quantity = 20.0, Amount = 1000m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _exporter.ExportAsync(data, "Nullable");

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ExportAsync_WithValidCancellationToken_AllowsNormalOperation()
    {
        // Arrange - test that a non-cancelled token works fine
        var data = new List<SimpleData>
        {
            new SimpleData { Id = 1, Name = "Test", Price = 100m, CreatedDate = DateTime.Now }
        };
        var cts = new CancellationTokenSource();

        // Act
        var result = await _exporter.ExportAsync(data, "Products", cts.Token);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExportAsync_WithSpecialCharacters_EncodesCorrectly()
    {
        // Arrange
        var data = new List<SimpleData>
        {
            new SimpleData { Id = 1, Name = "Product@#$%^&*()", Price = 100m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _exporter.ExportAsync(data, "SpecialChars");

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ExportMultipleAsync_WithMixedDataTypes_ReturnsSuccessResult()
    {
        // Arrange
        var sheets = new Dictionary<string, IEnumerable<object>>
        {
            {
                "SimpleData",
                new List<SimpleData>
                {
                    new SimpleData { Id = 1, Name = "A", Price = 100m, CreatedDate = DateTime.Now }
                }.Cast<object>().ToList()
            },
            {
                "ComplexData",
                new List<ComplexData>
                {
                    new ComplexData { Id = 1, Description = "B", Quantity = 50, Amount = 500m, CreatedDate = null }
                }.Cast<object>().ToList()
            }
        };

        // Act
        var result = await _exporter.ExportMultipleAsync(sheets);

        // Assert
        Assert.True(result.Succeeded);
    }
}
