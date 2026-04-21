namespace SmartWorkz.Core.Tests.Data;

using SmartWorkz.Shared.Data;

public class XmlHelperTests
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

    private sealed class Company
    {
        public string Name { get; set; } = string.Empty;
        public Person? CEO { get; set; }
        public List<Person> Employees { get; set; } = [];
        public DateTime Founded { get; set; }
    }

    private sealed class SimpleRecord
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public bool IsActive { get; set; }
    }

    // Tests for Serialize

    [Fact]
    public void Serialize_WithValidObject_ReturnsSuccess()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe", Age = 30, Email = "john@example.com" };

        // Act
        var result = XmlHelper.Serialize(person);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Contains("FirstName", result.Data);
        Assert.Contains("John", result.Data);
        Assert.Contains("Doe", result.Data);
    }

    [Fact]
    public void Serialize_WithNullObject_ReturnsFail()
    {
        // Arrange
        Person? person = null;

        // Act
        var result = XmlHelper.Serialize(person!);

        // Assert
        Assert.False(result.Succeeded);
        var errorText = string.Join(" ", result.Errors) + " " + (result.Error?.Message ?? string.Empty);
        Assert.Contains("null", errorText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Serialize_WithCustomRootElement_UsesCustomRoot()
    {
        // Arrange
        var person = new Person { FirstName = "Jane", LastName = "Smith", Age = 28 };
        var options = new XmlOptions("Person");

        // Act
        var result = XmlHelper.Serialize(person, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Contains("<Person>", result.Data);
    }

    [Fact]
    public void Serialize_WithIncludeXmlDeclaration_IncludesDeclaration()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe", Age = 30 };
        var options = new XmlOptions { IncludeXmlDeclaration = true };

        // Act
        var result = XmlHelper.Serialize(person, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.StartsWith("<?xml", result.Data);
    }

    [Fact]
    public void Serialize_WithoutXmlDeclaration_OmitsDeclaration()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe", Age = 30 };
        var options = new XmlOptions { IncludeXmlDeclaration = false };

        // Act
        var result = XmlHelper.Serialize(person, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.False(result.Data.StartsWith("<?xml"));
    }

    [Fact]
    public void Serialize_WithIndentation_FormatsOutput()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe", Age = 30 };
        var options = new XmlOptions { Indent = true };

        // Act
        var result = XmlHelper.Serialize(person, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Contains("\n", result.Data);
    }

    [Fact]
    public void Serialize_WithSpecialCharacters_EscapesCorrectly()
    {
        // Arrange
        var person = new Person { FirstName = "John & Jane", LastName = "Smith <Dev>", Age = 30 };

        // Act
        var result = XmlHelper.Serialize(person);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Contains("&amp;", result.Data);
        Assert.Contains("&lt;", result.Data);
        Assert.Contains("&gt;", result.Data);
    }

    [Fact]
    public void Serialize_WithBasicTypes_SerializesAllTypes()
    {
        // Arrange
        var record = new SimpleRecord { Name = "Test", Value = 42, IsActive = true };

        // Act
        var result = XmlHelper.Serialize(record);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Contains("Test", result.Data);
        Assert.Contains("42", result.Data);
        Assert.Contains("true", result.Data);
    }

    [Fact]
    public void Serialize_WithNestedObject_SerializesHierarchy()
    {
        // Arrange
        var company = new Company
        {
            Name = "Acme Corp",
            CEO = new Person { FirstName = "John", LastName = "CEO", Age = 50 },
            Founded = new DateTime(2000, 1, 1)
        };

        // Act
        var result = XmlHelper.Serialize(company);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Contains("Acme Corp", result.Data);
        Assert.Contains("John", result.Data);
        Assert.Contains("CEO", result.Data);
    }

    [Fact]
    public void Serialize_WithList_SerializesAllItems()
    {
        // Arrange
        var company = new Company
        {
            Name = "Acme Corp",
            Employees = new List<Person>
            {
                new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
                new() { FirstName = "Bob", LastName = "Jones", Age = 35 }
            },
            Founded = new DateTime(2000, 1, 1)
        };

        // Act
        var result = XmlHelper.Serialize(company);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Contains("Alice", result.Data);
        Assert.Contains("Bob", result.Data);
        Assert.Contains("Item", result.Data);
    }

    [Fact]
    public void Serialize_WithDateTime_UsesIsoFormat()
    {
        // Arrange
        var company = new Company
        {
            Name = "Test",
            Founded = new DateTime(2020, 5, 15, 10, 30, 45, DateTimeKind.Utc)
        };

        // Act
        var result = XmlHelper.Serialize(company);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Contains("2020-05-15", result.Data);
    }

    // Tests for Deserialize

    [Fact]
    public void Deserialize_WithValidXml_ReturnsSuccess()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Root>
  <FirstName>John</FirstName>
  <LastName>Doe</LastName>
  <Age>30</Age>
  <Email>john@example.com</Email>
</Root>";

        // Act
        var result = XmlHelper.Deserialize<Person>(xml);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("John", result.Data.FirstName);
        Assert.Equal("Doe", result.Data.LastName);
        Assert.Equal(30, result.Data.Age);
        Assert.Equal("john@example.com", result.Data.Email);
    }

    [Fact]
    public void Deserialize_WithEmptyXml_ReturnsFail()
    {
        // Arrange
        var xml = string.Empty;

        // Act
        var result = XmlHelper.Deserialize<Person>(xml);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Deserialize_WithInvalidXml_ReturnsFail()
    {
        // Arrange
        var xml = "This is not valid XML";

        // Act
        var result = XmlHelper.Deserialize<Person>(xml);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Deserialize_WithBasicTypes_DeserializesAllTypes()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Root>
  <Name>Test</Name>
  <Value>42</Value>
  <IsActive>true</IsActive>
</Root>";

        // Act
        var result = XmlHelper.Deserialize<SimpleRecord>(xml);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("Test", result.Data.Name);
        Assert.Equal(42, result.Data.Value);
        Assert.True(result.Data.IsActive);
    }

    [Fact]
    public void Deserialize_WithNestedObject_DeserializesHierarchy()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Root>
  <Name>Acme Corp</Name>
  <CEO>
    <FirstName>John</FirstName>
    <LastName>CEO</LastName>
    <Age>50</Age>
  </CEO>
  <Founded>2000-01-01T00:00:00.0000000</Founded>
</Root>";

        // Act
        var result = XmlHelper.Deserialize<Company>(xml);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("Acme Corp", result.Data.Name);
        Assert.NotNull(result.Data.CEO);
        Assert.Equal("John", result.Data.CEO.FirstName);
        Assert.Equal("CEO", result.Data.CEO.LastName);
    }

    [Fact]
    public void Deserialize_WithList_DeserializesAllItems()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Root>
  <Name>Acme Corp</Name>
  <Employees>
    <Item>
      <FirstName>Alice</FirstName>
      <LastName>Smith</LastName>
      <Age>30</Age>
    </Item>
    <Item>
      <FirstName>Bob</FirstName>
      <LastName>Jones</LastName>
      <Age>35</Age>
    </Item>
  </Employees>
  <Founded>2000-01-01T00:00:00.0000000</Founded>
</Root>";

        // Act
        var result = XmlHelper.Deserialize<Company>(xml);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Employees.Count);
        Assert.Equal("Alice", result.Data.Employees[0].FirstName);
        Assert.Equal("Bob", result.Data.Employees[1].FirstName);
    }

    [Fact]
    public void Deserialize_WithMissingElements_SkipsMissingValues()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Root>
  <FirstName>John</FirstName>
  <LastName>Doe</LastName>
  <Age>30</Age>
</Root>";

        // Act
        var result = XmlHelper.Deserialize<Person>(xml);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("John", result.Data.FirstName);
        Assert.Null(result.Data.Email);
    }

    [Fact]
    public void Deserialize_RoundTrip_PreservesData()
    {
        // Arrange
        var original = new Person { FirstName = "John", LastName = "Doe", Age = 30, Email = "john@example.com" };

        // Act
        var serializeResult = XmlHelper.Serialize(original);
        var deserializeResult = XmlHelper.Deserialize<Person>(serializeResult.Data!);

        // Assert
        Assert.True(serializeResult.Succeeded);
        Assert.True(deserializeResult.Succeeded);
        Assert.Equal(original.FirstName, deserializeResult.Data!.FirstName);
        Assert.Equal(original.LastName, deserializeResult.Data!.LastName);
        Assert.Equal(original.Age, deserializeResult.Data!.Age);
        Assert.Equal(original.Email, deserializeResult.Data!.Email);
    }

    [Fact]
    public void Deserialize_WithSpecialCharacters_UnescapesCorrectly()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Root>
  <FirstName>John &amp; Jane</FirstName>
  <LastName>Smith &lt;Dev&gt;</LastName>
  <Age>30</Age>
</Root>";

        // Act
        var result = XmlHelper.Deserialize<Person>(xml);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("John & Jane", result.Data.FirstName);
        Assert.Equal("Smith <Dev>", result.Data.LastName);
    }

    // Tests for Query

    [Fact]
    public void Query_WithSimplePath_ReturnsElements()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Root>
  <FirstName>John</FirstName>
  <LastName>Doe</LastName>
  <Age>30</Age>
</Root>";

        // Act
        var result = XmlHelper.Query(xml, "//FirstName");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("John", result.Data[0]);
    }

    [Fact]
    public void Query_WithRootPath_ReturnsElements()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Root>
  <FirstName>John</FirstName>
  <LastName>Doe</LastName>
</Root>";

        // Act
        var result = XmlHelper.Query(xml, "/Root/FirstName");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("John", result.Data[0]);
    }

    [Fact]
    public void Query_WithWildcard_ReturnsMultipleElements()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Root>
  <Person>
    <Name>John</Name>
  </Person>
  <Person>
    <Name>Jane</Name>
  </Person>
</Root>";

        // Act
        var result = XmlHelper.Query(xml, "//Person/Name");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Contains("John", result.Data);
        Assert.Contains("Jane", result.Data);
    }

    [Fact]
    public void Query_WithEmptyXml_ReturnsFail()
    {
        // Arrange
        var xml = string.Empty;

        // Act
        var result = XmlHelper.Query(xml, "//Element");

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Query_WithEmptyExpression_ReturnsFail()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Root>
  <Element>Value</Element>
</Root>";

        // Act
        var result = XmlHelper.Query(xml, string.Empty);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Query_WithInvalidXml_ReturnsFail()
    {
        // Arrange
        var xml = "Not valid XML";

        // Act
        var result = XmlHelper.Query(xml, "//Element");

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Query_WithComplexXPath_ReturnsFilteredResults()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Root>
  <Items>
    <Item>
      <Name>Product A</Name>
      <Price>100</Price>
    </Item>
    <Item>
      <Name>Product B</Name>
      <Price>200</Price>
    </Item>
  </Items>
</Root>";

        // Act
        var result = XmlHelper.Query(xml, "//Item/Name");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Contains("Product A", result.Data);
        Assert.Contains("Product B", result.Data);
    }

    [Fact]
    public void Query_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Root>
  <FirstName>John</FirstName>
</Root>";

        // Act
        var result = XmlHelper.Query(xml, "//NonExistentElement");

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public void Serialize_WithDecimalValues_PreservesFormat()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Product", Price = 99.99m, Description = "Test" };

        // Act
        var result = XmlHelper.Serialize(product);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Contains("99.99", result.Data);
    }

    [Fact]
    public void Deserialize_WithDecimalValues_ParsesCorrectly()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Root>
  <Id>1</Id>
  <Name>Product</Name>
  <Price>99.99</Price>
  <Description>Test</Description>
</Root>";

        // Act
        var result = XmlHelper.Deserialize<Product>(xml);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(99.99m, result.Data.Price);
    }

    [Fact]
    public void Serialize_ComplexObject_RoundTrip_PreservesData()
    {
        // Arrange
        var original = new Company
        {
            Name = "Acme Corp",
            CEO = new Person { FirstName = "John", LastName = "CEO", Age = 50, Email = "john@acme.com" },
            Employees = new List<Person>
            {
                new() { FirstName = "Alice", LastName = "Smith", Age = 30, Email = "alice@acme.com" },
                new() { FirstName = "Bob", LastName = "Jones", Age = 35, Email = "bob@acme.com" }
            },
            Founded = new DateTime(2000, 1, 1)
        };

        // Act
        var serializeResult = XmlHelper.Serialize(original);
        var deserializeResult = XmlHelper.Deserialize<Company>(serializeResult.Data!);

        // Assert
        Assert.True(serializeResult.Succeeded);
        Assert.True(deserializeResult.Succeeded);
        Assert.Equal(original.Name, deserializeResult.Data!.Name);
        Assert.NotNull(deserializeResult.Data.CEO);
        Assert.Equal(original.CEO.FirstName, deserializeResult.Data.CEO.FirstName);
        Assert.Equal(original.Employees.Count, deserializeResult.Data.Employees.Count);
    }
}
