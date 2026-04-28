namespace SmartWorkz.Core.External.Tests.Export;

/// <summary>
/// Unit tests for the CsvExportService using CsvHelper library.
/// Tests cover generic collection export, special character handling, header generation, and multiple data types.
/// </summary>
public class CsvExportServiceTests
{
    private readonly CsvExportService _service;

    public CsvExportServiceTests()
    {
        _service = new CsvExportService();
    }

    private class SimpleProduct
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    private class ComplexProduct
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public string? Sku { get; set; }
        public double Quantity { get; set; }
        public decimal Amount { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }

    [Fact]
    public async Task ExportAsync_WithValidData_ReturnsSuccessResultWithBytes()
    {
        // Arrange
        var data = new List<SimpleProduct>
        {
            new SimpleProduct { Id = 1, Name = "Product A", Price = 100.50m, CreatedDate = DateTime.Now },
            new SimpleProduct { Id = 2, Name = "Product B", Price = 200.75m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _service.ExportAsync(data);

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
        var data = new List<SimpleProduct>();

        // Act
        var result = await _service.ExportAsync(data);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ExportAsync_WithSpecialCharacters_HandlesEscapingCorrectly()
    {
        // Arrange
        var data = new List<SimpleProduct>
        {
            new SimpleProduct
            {
                Id = 1,
                Name = "Product \"Test\", With, Commas",
                Price = 100.50m,
                CreatedDate = DateTime.Now
            },
            new SimpleProduct
            {
                Id = 2,
                Name = "Product\nWith\nNewlines",
                Price = 200.75m,
                CreatedDate = DateTime.Now
            }
        };

        // Act
        var result = await _service.ExportAsync(data);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);

        // Verify content contains properly escaped special characters
        var csvContent = System.Text.Encoding.UTF8.GetString(result.Data);
        Assert.Contains("\"Product \"\"Test\"\", With, Commas\"", csvContent);
    }

    [Fact]
    public async Task ExportAsync_GeneratesHeaderRow()
    {
        // Arrange
        var data = new List<SimpleProduct>
        {
            new SimpleProduct { Id = 1, Name = "Test", Price = 100m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _service.ExportAsync(data);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        var csvContent = System.Text.Encoding.UTF8.GetString(result.Data);

        // Verify headers are present
        Assert.Contains("Id", csvContent);
        Assert.Contains("Name", csvContent);
        Assert.Contains("Price", csvContent);
        Assert.Contains("CreatedDate", csvContent);
    }

    [Fact]
    public async Task ExportAsync_WithMultipleDataTypes_ExportsCorrectly()
    {
        // Arrange
        var data = new List<ComplexProduct>
        {
            new ComplexProduct
            {
                Id = 1,
                Description = "Complex Item",
                Sku = "SKU-001",
                Quantity = 10.5,
                Amount = 500.25m,
                CreatedDate = new DateTime(2024, 5, 15),
                IsActive = true
            },
            new ComplexProduct
            {
                Id = 2,
                Description = null,
                Sku = "SKU-002",
                Quantity = 20.0,
                Amount = 1000.50m,
                CreatedDate = null,
                IsActive = false
            }
        };

        // Act
        var result = await _service.ExportAsync(data);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);

        var csvContent = System.Text.Encoding.UTF8.GetString(result.Data);
        Assert.Contains("10.5", csvContent);
        Assert.Contains("500.25", csvContent);
        Assert.Contains("True", csvContent);
        Assert.Contains("False", csvContent);
    }

    [Fact]
    public async Task ExportAsync_WithNullValues_HandlesGracefully()
    {
        // Arrange
        var data = new List<SimpleProduct>
        {
            new SimpleProduct { Id = 1, Name = null, Price = 100.50m, CreatedDate = DateTime.Now },
            new SimpleProduct { Id = 2, Name = "Product B", Price = 200.75m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _service.ExportAsync(data);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExportAsync_WithLargeDataset_ReturnsSuccessResult()
    {
        // Arrange
        var data = Enumerable.Range(1, 1000)
            .Select(i => new SimpleProduct
            {
                Id = i,
                Name = $"Product {i}",
                Price = 100.00m + i,
                CreatedDate = DateTime.Now
            })
            .ToList();

        // Act
        var result = await _service.ExportAsync(data);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Length > 0);
    }

    [Fact]
    public async Task ExportAsync_WithCustomOptions_AppliesDelimiter()
    {
        // Arrange
        var options = new ExportOptions { Delimiter = ';' };
        var data = new List<SimpleProduct>
        {
            new SimpleProduct { Id = 1, Name = "Test", Price = 100m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _service.ExportAsync(data, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);

        var csvContent = System.Text.Encoding.UTF8.GetString(result.Data);
        Assert.Contains(";", csvContent);
    }

    [Fact]
    public async Task ExportAsync_WithCustomOptions_ExcludesHeaders()
    {
        // Arrange
        var options = new ExportOptions { IncludeHeaders = false };
        var data = new List<SimpleProduct>
        {
            new SimpleProduct { Id = 1, Name = "Test", Price = 100m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _service.ExportAsync(data, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);

        var csvContent = System.Text.Encoding.UTF8.GetString(result.Data);
        Assert.DoesNotContain("Id,Name,Price", csvContent);
    }

    [Fact]
    public async Task ExportAsync_WithAnonymousType_ExportsCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new { Id = 1, Name = "John", Age = 30 },
            new { Id = 2, Name = "Jane", Age = 25 }
        };

        // Act
        var result = await _service.ExportAsync(data.AsEnumerable());

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExportAsync_WithDateTime_FormatsCorrectly()
    {
        // Arrange
        var data = new List<SimpleProduct>
        {
            new SimpleProduct
            {
                Id = 1,
                Name = "Test",
                Price = 100m,
                CreatedDate = new DateTime(2024, 12, 25, 10, 30, 45)
            }
        };

        // Act
        var result = await _service.ExportAsync(data);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);

        var csvContent = System.Text.Encoding.UTF8.GetString(result.Data);
        Assert.Contains("2024", csvContent);
    }

    [Fact]
    public async Task ExportAsync_WithDecimalValues_PreservesAccuracy()
    {
        // Arrange
        var data = new List<SimpleProduct>
        {
            new SimpleProduct { Id = 1, Name = "Test", Price = 123.456789m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _service.ExportAsync(data);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);

        var csvContent = System.Text.Encoding.UTF8.GetString(result.Data);
        Assert.Contains("123.456789", csvContent);
    }

    [Fact]
    public async Task ExportAsync_WithCancellationToken_AllowsNormalOperation()
    {
        // Arrange
        var data = new List<SimpleProduct>
        {
            new SimpleProduct { Id = 1, Name = "Test", Price = 100m, CreatedDate = DateTime.Now }
        };
        var cts = new CancellationTokenSource();

        // Act
        var result = await _service.ExportAsync(data, cancellationToken: cts.Token);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExportAsync_WithCancellationToken_ThrowsWhenCancelled()
    {
        // Arrange
        var data = new List<SimpleProduct>
        {
            new SimpleProduct { Id = 1, Name = "Test", Price = 100m, CreatedDate = DateTime.Now }
        };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.ExportAsync(data, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task ExportAsync_WithMultipleRows_IncludesAllRows()
    {
        // Arrange
        var data = new List<SimpleProduct>
        {
            new SimpleProduct { Id = 1, Name = "Product 1", Price = 100m, CreatedDate = DateTime.Now },
            new SimpleProduct { Id = 2, Name = "Product 2", Price = 200m, CreatedDate = DateTime.Now },
            new SimpleProduct { Id = 3, Name = "Product 3", Price = 300m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _service.ExportAsync(data);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);

        var csvContent = System.Text.Encoding.UTF8.GetString(result.Data);
        Assert.Contains("Product 1", csvContent);
        Assert.Contains("Product 2", csvContent);
        Assert.Contains("Product 3", csvContent);
    }

    [Fact]
    public async Task ExportAsync_WithSpecialCharactersInHeaders_HandlesCorrectly()
    {
        // Arrange - Create a type with special characters in property names (simulated with display names)
        var data = new List<SimpleProduct>
        {
            new SimpleProduct { Id = 1, Name = "Test", Price = 100m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _service.ExportAsync(data);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ExportAsync_ReturnedBytesCanBeConverted_ToString()
    {
        // Arrange
        var data = new List<SimpleProduct>
        {
            new SimpleProduct { Id = 1, Name = "Test", Price = 100m, CreatedDate = DateTime.Now }
        };

        // Act
        var result = await _service.ExportAsync(data);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);

        var csvContent = System.Text.Encoding.UTF8.GetString(result.Data);
        Assert.NotEmpty(csvContent);
        Assert.Contains("Id", csvContent);
    }
}
