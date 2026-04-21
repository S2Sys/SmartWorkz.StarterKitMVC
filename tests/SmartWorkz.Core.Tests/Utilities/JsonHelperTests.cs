namespace SmartWorkz.Core.Tests.Utilities;

using SmartWorkz.Shared;

public class JsonHelperTests
{
    #region Test Models

    public class TestUser
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }

    #endregion

    #region Serialize Tests

    [Fact]
    public void Serialize_WithObject_ReturnsValidJson()
    {
        // Arrange
        var user = new TestUser { Id = 1, Name = "John", Email = "john@example.com" };

        // Act
        var result = JsonHelper.Serialize(user);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotEmpty(result.Data!);
        Assert.Contains("john@example.com", result.Data!);
    }

    [Fact]
    public void Serialize_WithIndentTrue_ReturnsPrettyJson()
    {
        // Arrange
        var user = new TestUser { Id = 1, Name = "John", Email = "john@example.com" };

        // Act
        var result = JsonHelper.Serialize(user, indent: true);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Contains("\n", result.Data!);
    }

    [Fact]
    public void Serialize_WithIndentFalse_ReturnsCompactJson()
    {
        // Arrange
        var user = new TestUser { Id = 1, Name = "John", Email = "john@example.com" };

        // Act
        var result = JsonHelper.Serialize(user, indent: false);

        // Assert
        Assert.True(result.Succeeded);
        Assert.DoesNotContain("\n", result.Data!);
    }

    [Fact]
    public void Serialize_WithNull_ReturnsNullString()
    {
        // Act
        var result = JsonHelper.Serialize<TestUser>(null!);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("null", result.Data);
    }

    [Fact]
    public void Serialize_WithComplexObject_SerializesCorrectly()
    {
        // Arrange
        var users = new[]
        {
            new TestUser { Id = 1, Name = "John", Email = "john@example.com" },
            new TestUser { Id = 2, Name = "Jane", Email = "jane@example.com" }
        };

        // Act
        var result = JsonHelper.Serialize(users);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Contains("John", result.Data!);
        Assert.Contains("Jane", result.Data!);
    }

    #endregion

    #region Deserialize Tests

    [Fact]
    public void Deserialize_WithValidJson_ReturnsObject()
    {
        // Arrange
        var json = """{"id":1,"name":"John","email":"john@example.com"}""";

        // Act
        var result = JsonHelper.Deserialize<TestUser>(json);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Data!.Id);
        Assert.Equal("John", result.Data!.Name);
        Assert.Equal("john@example.com", result.Data!.Email);
    }

    [Fact]
    public void Deserialize_WithPrettyJson_ReturnsObject()
    {
        // Arrange
        var json = """
            {
              "id": 1,
              "name": "John",
              "email": "john@example.com"
            }
            """;

        // Act
        var result = JsonHelper.Deserialize<TestUser>(json);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Data!.Id);
        Assert.Equal("John", result.Data!.Name);
    }

    [Fact]
    public void Deserialize_WithEmptyJson_ReturnsFail()
    {
        // Act
        var result = JsonHelper.Deserialize<TestUser>("");

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Deserialize_WithInvalidJson_ReturnsFail()
    {
        // Arrange
        var json = "{invalid json}";

        // Act
        var result = JsonHelper.Deserialize<TestUser>(json);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Invalid", result.MessageKey ?? "");
    }

    [Fact]
    public void Deserialize_WithCasInsensitive_DeserializesCorrectly()
    {
        // Arrange
        var json = """{"ID":1,"NAME":"John","EMAIL":"john@example.com"}""";

        // Act
        var result = JsonHelper.Deserialize<TestUser>(json);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("John", result.Data!.Name);
    }

    #endregion

    #region IsPrettyJson Tests

    [Fact]
    public void IsPrettyJson_WithCompactJson_ReturnsFalse()
    {
        // Arrange
        var json = """{"id":1,"name":"John"}""";

        // Act
        var result = JsonHelper.IsPrettyJson(json);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsPrettyJson_WithPrettyJson_ReturnsTrue()
    {
        // Arrange
        var json = """
            {
              "id": 1,
              "name": "John"
            }
            """;

        // Act
        var result = JsonHelper.IsPrettyJson(json);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsPrettyJson_WithEmptyString_ReturnsFalse()
    {
        // Act
        var result = JsonHelper.IsPrettyJson("");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsPrettyJson_WithNull_ReturnsFalse()
    {
        // Act
        var result = JsonHelper.IsPrettyJson(null!);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Round-trip Tests

    [Fact]
    public void SerializeAndDeserialize_WithValidObject_RoundTripSucceeds()
    {
        // Arrange
        var originalUser = new TestUser { Id = 42, Name = "Alice", Email = "alice@example.com" };

        // Act
        var serializeResult = JsonHelper.Serialize(originalUser);
        var deserializeResult = JsonHelper.Deserialize<TestUser>(serializeResult.Data!);

        // Assert
        Assert.True(serializeResult.Succeeded);
        Assert.True(deserializeResult.Succeeded);
        Assert.Equal(originalUser.Id, deserializeResult.Data!.Id);
        Assert.Equal(originalUser.Name, deserializeResult.Data!.Name);
        Assert.Equal(originalUser.Email, deserializeResult.Data!.Email);
    }

    #endregion
}

