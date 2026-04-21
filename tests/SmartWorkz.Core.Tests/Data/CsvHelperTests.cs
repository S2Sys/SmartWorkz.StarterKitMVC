namespace SmartWorkz.Core.Tests.Data;

using SmartWorkz.Shared.Data;

public class CsvHelperTests
{
    // Test models
    private sealed class Person
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? Email { get; set; }
    }

    private sealed class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }

    private sealed class SimpleRecord
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    // Tests for CsvWriter

    [Fact]
    public void CsvWriter_WithValidObjects_ReturnsSuccess()
    {
        // Arrange
        var items = new List<Person>
        {
            new() { FirstName = "John", LastName = "Doe", Age = 30, Email = "john@example.com" },
            new() { FirstName = "Jane", LastName = "Smith", Age = 28, Email = "jane@example.com" }
        };

        // Act
        var result = CsvHelper.CsvWriter(items);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Contains("FirstName", result.Data);
        Assert.Contains("John", result.Data);
        Assert.Contains("Doe", result.Data);
    }

    [Fact]
    public void CsvWriter_WithEmptyList_ReturnsEmptyString()
    {
        // Arrange
        var items = new List<Person>();

        // Act
        var result = CsvHelper.CsvWriter(items);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public void CsvWriter_WithNullItems_ReturnsFail()
    {
        // Arrange
        IEnumerable<Person> items = null!;

        // Act
        var result = CsvHelper.CsvWriter(items);

        // Assert
        Assert.False(result.Succeeded);
        // Check for either the Errors list or the Error message
        var errorText = string.Join(" ", result.Errors) + " " + (result.Error?.Message ?? string.Empty);
        Assert.Contains("null", errorText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CsvWriter_WithDelimitedValues_QuotesFields()
    {
        // Arrange
        var items = new List<Person>
        {
            new() { FirstName = "John, Jr.", LastName = "Doe", Age = 30, Email = "john@example.com" }
        };

        // Act
        var result = CsvHelper.CsvWriter(items);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Contains("\"John, Jr.\"", result.Data);
    }

    [Fact]
    public void CsvWriter_WithQuoteCharInValue_EscapesQuotes()
    {
        // Arrange
        var items = new List<Person>
        {
            new() { FirstName = "John \"Jack\" Doe", LastName = "Smith", Age = 35, Email = "john@example.com" }
        };

        // Act
        var result = CsvHelper.CsvWriter(items);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        // Quotes should be doubled inside quoted fields
        Assert.Contains("\"John \"\"Jack\"\" Doe\"", result.Data);
    }

    [Fact]
    public void CsvWriter_WithNewlinesInValue_QuotesField()
    {
        // Arrange
        var items = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 99.99m, Description = "Line 1\nLine 2\nLine 3" }
        };

        // Act
        var result = CsvHelper.CsvWriter(items);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Contains("\"Line 1", result.Data);
    }

    [Fact]
    public void CsvWriter_WithCustomOptions_UsesDelimiter()
    {
        // Arrange
        var items = new List<SimpleRecord>
        {
            new() { Name = "Test", Value = 42 }
        };
        var options = new CsvOptions { Delimiter = ';' };

        // Act
        var result = CsvHelper.CsvWriter(items, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Contains("Name;Value", result.Data);
        Assert.Contains("Test;42", result.Data);
    }

    [Fact]
    public void CsvWriter_WithCustomQuoteChar_QuotesWithCustomChar()
    {
        // Arrange
        var items = new List<SimpleRecord>
        {
            new() { Name = "Test, Value", Value = 42 }
        };
        var options = new CsvOptions { QuoteChar = '\'' };

        // Act
        var result = CsvHelper.CsvWriter(items, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Contains("'Test, Value'", result.Data);
    }

    // Tests for CsvReader

    [Fact]
    public async Task CsvReader_WithValidCsv_ReturnsSuccess()
    {
        // Arrange
        var csv = "FirstName,LastName,Age,Email\nJohn,Doe,30,john@example.com\nJane,Smith,28,jane@example.com";

        // Act
        var result = await CsvHelper.CsvReader<Person>(csv);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal("John", result.Data[0].FirstName);
        Assert.Equal("Doe", result.Data[0].LastName);
        Assert.Equal(30, result.Data[0].Age);
    }

    [Fact]
    public async Task CsvReader_WithEmptyContent_ReturnsEmptyList()
    {
        // Arrange
        var csv = string.Empty;

        // Act
        var result = await CsvHelper.CsvReader<Person>(csv);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task CsvReader_WithQuotedFields_ParsesCorrectly()
    {
        // Arrange
        var csv = "FirstName,LastName,Age,Email\n\"John, Jr.\",Doe,30,john@example.com";

        // Act
        var result = await CsvHelper.CsvReader<Person>(csv);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("John, Jr.", result.Data[0].FirstName);
    }

    [Fact]
    public async Task CsvReader_WithEscapedQuotes_UnescapesCorrectly()
    {
        // Arrange
        var csv = "FirstName,LastName,Age,Email\n\"John \"\"Jack\"\" Doe\",Smith,35,john@example.com";

        // Act
        var result = await CsvHelper.CsvReader<Person>(csv);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("John \"Jack\" Doe", result.Data[0].FirstName);
    }

    [Fact]
    public async Task CsvReader_WithEmbeddedNewlines_ParsesCorrectly()
    {
        // Arrange
        var csv = "Id,Name,Price,Description\n1,\"Product 1\",99.99,\"Line 1\nLine 2\nLine 3\"";

        // Act
        var result = await CsvHelper.CsvReader<Product>(csv);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("Line 1\nLine 2\nLine 3", result.Data[0].Description);
    }

    [Fact]
    public async Task CsvReader_WithCustomDelimiter_ParsesCorrectly()
    {
        // Arrange
        var csv = "Name;Value\nTest;42";
        var options = new CsvOptions { Delimiter = ';' };

        // Act
        var result = await CsvHelper.CsvReader<SimpleRecord>(csv, null, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("Test", result.Data[0].Name);
        Assert.Equal(42, result.Data[0].Value);
    }

    [Fact]
    public async Task CsvReader_WithoutHeader_MapsByColumnOrder()
    {
        // Arrange
        var csv = "Test,42";
        var options = new CsvOptions { HasHeader = false };

        // Act
        var result = await CsvHelper.CsvReader<SimpleRecord>(csv, null, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("Test", result.Data[0].Name);
        Assert.Equal(42, result.Data[0].Value);
    }

    [Fact]
    public async Task CsvReader_WithTrimEnabled_TrimValues()
    {
        // Arrange
        var csv = "FirstName,LastName,Age,Email\n  John  ,  Doe  ,30,  john@example.com  ";
        var options = new CsvOptions { TrimValues = true };

        // Act
        var result = await CsvHelper.CsvReader<Person>(csv, null, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("John", result.Data[0].FirstName);
        Assert.Equal("Doe", result.Data[0].LastName);
    }

    [Fact]
    public async Task CsvReader_WithTrimDisabled_PreserveWhitespace()
    {
        // Arrange
        var csv = "FirstName,LastName,Age,Email\n  John  ,  Doe  ,30,  john@example.com  ";
        var options = new CsvOptions { TrimValues = false };

        // Act
        var result = await CsvHelper.CsvReader<Person>(csv, null, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("  John  ", result.Data[0].FirstName);
    }

    // Tests for CsvMapping

    [Fact]
    public void CsvMapping_CreateAuto_MapsAllProperties()
    {
        // Act
        var mapping = CsvMapping<Person>.CreateAuto();

        // Assert
        Assert.NotNull(mapping);
        Assert.Equal(4, mapping.Mappings.Count);
        Assert.True(mapping.Mappings.Values.Contains("FirstName"));
        Assert.True(mapping.Mappings.Values.Contains("LastName"));
        Assert.True(mapping.Mappings.Values.Contains("Age"));
        Assert.True(mapping.Mappings.Values.Contains("Email"));
    }

    [Fact]
    public void CsvMapping_Column_BuildsMapping()
    {
        // Arrange
        var mapping = new CsvMapping<Person>();

        // Act
        mapping
            .Column(p => p.FirstName, "Given Name")
            .Column(p => p.LastName, "Family Name")
            .Column(p => p.Age, "Years Old")
            .Column(p => p.Email, "Email Address");

        // Assert
        Assert.Equal(4, mapping.Mappings.Count);
        Assert.True(mapping.HeaderToProperty.ContainsKey("Given Name"));
        Assert.True(mapping.HeaderToProperty.ContainsKey("Family Name"));
    }

    [Fact]
    public async Task CsvReader_WithCustomMapping_UsesMapping()
    {
        // Arrange
        var csv = "Given Name,Family Name,Years Old,Email Address\nJohn,Doe,30,john@example.com";
        var mapping = new CsvMapping<Person>()
            .Column(p => p.FirstName, "Given Name")
            .Column(p => p.LastName, "Family Name")
            .Column(p => p.Age, "Years Old")
            .Column(p => p.Email, "Email Address");

        // Act
        var result = await CsvHelper.CsvReader(csv, mapping);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("John", result.Data[0].FirstName);
        Assert.Equal("Doe", result.Data[0].LastName);
    }

    [Fact]
    public async Task CsvReader_WithNullableFields_HandlesNullValues()
    {
        // Arrange
        var csv = "FirstName,LastName,Age,Email\nJohn,Doe,30,\nJane,Smith,28,jane@example.com";

        // Act
        var result = await CsvHelper.CsvReader<Person>(csv);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Null(result.Data[0].Email);
        Assert.NotNull(result.Data[1].Email);
    }

    [Fact]
    public void CsvWriter_RoundTrip_PreservesData()
    {
        // Arrange
        var original = new List<Person>
        {
            new() { FirstName = "John", LastName = "Doe", Age = 30, Email = "john@example.com" },
            new() { FirstName = "Jane", LastName = "Smith", Age = 28, Email = "jane@example.com" }
        };

        // Act
        var writeResult = CsvHelper.CsvWriter(original);
        var readResult = CsvHelper.CsvReader<Person>(writeResult.Data!).Result;

        // Assert
        Assert.True(writeResult.Succeeded);
        Assert.True(readResult.Succeeded);
        Assert.Equal(original.Count, readResult.Data!.Count);
        Assert.Equal(original[0].FirstName, readResult.Data[0].FirstName);
        Assert.Equal(original[0].LastName, readResult.Data[0].LastName);
        Assert.Equal(original[0].Age, readResult.Data[0].Age);
    }

    [Fact]
    public void CsvWriter_WithDecimalValues_PreservesFormat()
    {
        // Arrange
        var items = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 99.99m, Description = "Test" },
            new() { Id = 2, Name = "Product 2", Price = 19.50m, Description = "Another" }
        };

        // Act
        var result = CsvHelper.CsvWriter(items);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Contains("99.99", result.Data);
        Assert.Contains("19.5", result.Data);
    }

    [Fact]
    public async Task CsvReader_WithDecimalValues_ParsesCorrectly()
    {
        // Arrange
        var csv = "Id,Name,Price,Description\n1,Product 1,99.99,Test\n2,Product 2,19.50,Another";

        // Act
        var result = await CsvHelper.CsvReader<Product>(csv);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(99.99m, result.Data[0].Price);
        Assert.Equal(19.50m, result.Data[1].Price);
    }
}
