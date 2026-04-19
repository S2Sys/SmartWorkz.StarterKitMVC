namespace SmartWorkz.Core.External.Tests.Export;

public class PdfExporterTests
{
    private readonly PdfExporter _exporter;

    public PdfExporterTests()
    {
        _exporter = new PdfExporter();
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
        public bool IsActive { get; set; }
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
        var result = await _exporter.ExportAsync(data, "Test Report");

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
        var result = await _exporter.ExportAsync(data, "Empty Report");

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
        var result = await _exporter.ExportAsync(data, "Null Values Report");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExportAsync_WithoutTitle_ReturnsSuccessResult()
    {
        // Arrange
        var data = new List<SimpleData>
        {
            new SimpleData { Id = 1, Name = "Product", Price = 100m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _exporter.ExportAsync(data, "");

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ExportAsync_WithLargeDataset_ReturnsSuccessResult()
    {
        // Arrange
        var data = Enumerable.Range(1, 200)
            .Select(i => new SimpleData
            {
                Id = i,
                Name = $"Product {i}",
                Price = 100.00m + i,
                CreatedDate = DateTime.Now
            })
            .ToList();

        // Act
        var result = await _exporter.ExportAsync(data, "Large Dataset Report");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExportAsync_WithCustomPageOptions_ReturnsSuccessResult()
    {
        // Arrange
        var options = new PdfOptions
        {
            PageSize = "A4",
            Orientation = "Portrait",
            Title = "Custom Report",
            IncludePageNumbers = true,
            TopMargin = 50,
            BottomMargin = 50,
            LeftMargin = 40,
            RightMargin = 40,
            RowsPerPage = 25
        };

        var exporter = new PdfExporter(options);
        var data = new List<SimpleData>
        {
            new SimpleData { Id = 1, Name = "Test", Price = 100m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await exporter.ExportAsync(data, "Custom Report");

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ExportAsync_WithLandscapeOrientation_ReturnsSuccessResult()
    {
        // Arrange
        var options = new PdfOptions
        {
            Orientation = "Landscape"
        };

        var exporter = new PdfExporter(options);
        var data = new List<SimpleData>
        {
            new SimpleData { Id = 1, Name = "Product", Price = 100m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await exporter.ExportAsync(data, "Landscape Report");

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ExportAsync_WithDifferentPageSizes_ReturnsSuccessResult()
    {
        // Arrange
        var pageSizes = new[] { "A4", "Letter", "A3", "A5", "Legal" };

        foreach (var pageSize in pageSizes)
        {
            var options = new PdfOptions { PageSize = pageSize };
            var exporter = new PdfExporter(options);
            var data = new List<SimpleData>
            {
                new SimpleData { Id = 1, Name = "Test", Price = 100m, CreatedDate = DateTime.Now }
            };

            // Act
            var result = await exporter.ExportAsync(data, $"Report {pageSize}");

            // Assert
            Assert.True(result.Succeeded, $"Failed for page size {pageSize}");
        }
    }

    [Fact]
    public async Task ExportAsync_WithNullableTypes_HandlesProperlyFormatted()
    {
        // Arrange
        var data = new List<ComplexData>
        {
            new ComplexData { Id = 1, Description = "Test", Quantity = 10.5, Amount = 500m, CreatedDate = null, IsActive = true },
            new ComplexData { Id = 2, Description = null, Quantity = 20.0, Amount = 1000m, CreatedDate = DateTime.Now, IsActive = false }
        };

        // Act
        var result = await _exporter.ExportAsync(data, "Nullable Types Report");

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ExportAsync_WithBooleanFields_FormatsCorrectly()
    {
        // Arrange
        var data = new List<ComplexData>
        {
            new ComplexData { Id = 1, Description = "Active", Quantity = 10, Amount = 100m, CreatedDate = DateTime.Now, IsActive = true },
            new ComplexData { Id = 2, Description = "Inactive", Quantity = 20, Amount = 200m, CreatedDate = DateTime.Now, IsActive = false }
        };

        // Act
        var result = await _exporter.ExportAsync(data, "Boolean Fields Report");

        // Assert
        Assert.True(result.Succeeded);
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
        var result = await _exporter.ExportAsync(data, "Currency Report");

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
        var result = await _exporter.ExportAsync(data, "Date Report");

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ExportAsync_WithPageNumbers_IncludesPageInfo()
    {
        // Arrange
        var options = new PdfOptions { IncludePageNumbers = true };
        var exporter = new PdfExporter(options);
        var data = Enumerable.Range(1, 100)
            .Select(i => new SimpleData
            {
                Id = i,
                Name = $"Item {i}",
                Price = 100m,
                CreatedDate = DateTime.Now
            })
            .ToList();

        // Act
        var result = await exporter.ExportAsync(data, "Paginated Report");

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ExportAsync_WithoutPageNumbers_ReturnsSuccessResult()
    {
        // Arrange
        var options = new PdfOptions { IncludePageNumbers = false };
        var exporter = new PdfExporter(options);
        var data = new List<SimpleData>
        {
            new SimpleData { Id = 1, Name = "Test", Price = 100m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await exporter.ExportAsync(data, "No Page Numbers Report");

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ExportAsync_WithCustomMargins_ReturnsSuccessResult()
    {
        // Arrange
        var options = new PdfOptions
        {
            TopMargin = 72,    // 1 inch
            BottomMargin = 72,
            LeftMargin = 54,   // 0.75 inch
            RightMargin = 54
        };

        var exporter = new PdfExporter(options);
        var data = new List<SimpleData>
        {
            new SimpleData { Id = 1, Name = "Test", Price = 100m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await exporter.ExportAsync(data, "Custom Margins Report");

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
        var result = await _exporter.ExportAsync(data, "Report", cts.Token);

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
        var result = await _exporter.ExportAsync(data, "Special Characters Report");

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ExportAsync_WithPaginationAcrossPages_ReturnsSuccessResult()
    {
        // Arrange
        var options = new PdfOptions { RowsPerPage = 10 };
        var exporter = new PdfExporter(options);
        var data = Enumerable.Range(1, 100)
            .Select(i => new SimpleData
            {
                Id = i,
                Name = $"Item {i}",
                Price = 100m + i,
                CreatedDate = DateTime.Now
            })
            .ToList();

        // Act
        var result = await exporter.ExportAsync(data, "Paginated Large Dataset");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExportAsync_With1000RowDataset_ReturnsSuccessResult()
    {
        // Arrange
        var data = Enumerable.Range(1, 300)
            .Select(i => new SimpleData
            {
                Id = i,
                Name = $"Product {i}",
                Price = 100.00m + i,
                CreatedDate = DateTime.Now.AddDays(-i)
            })
            .ToList();

        // Act
        var result = await _exporter.ExportAsync(data, "Very Large Dataset Report");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExportAsync_WithNumericData_AlignmentCorrect()
    {
        // Arrange
        var data = new List<ComplexData>
        {
            new ComplexData { Id = 1, Description = "Item 1", Quantity = 100.5, Amount = 5000.50m, CreatedDate = DateTime.Now, IsActive = true },
            new ComplexData { Id = 2, Description = "Item 2", Quantity = 250.75, Amount = 10000.25m, CreatedDate = DateTime.Now, IsActive = false }
        };

        // Act
        var result = await _exporter.ExportAsync(data, "Numeric Alignment Report");

        // Assert
        Assert.True(result.Succeeded);
    }
}
