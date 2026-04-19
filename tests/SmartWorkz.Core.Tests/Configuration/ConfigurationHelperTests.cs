using Microsoft.Extensions.Configuration;
using SmartWorkz.Core.Shared.Configuration;

namespace SmartWorkz.Core.Tests.Configuration;

public class ConfigurationHelperTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ConfigurationHelper(null!));
        Assert.Equal("configuration", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidConfiguration_ShouldInitializeSuccessfully()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["TestKey"] = "TestValue" })
            .Build();

        // Act
        var helper = new ConfigurationHelper(config);

        // Assert
        Assert.NotNull(helper);
    }

    #endregion

    #region GetRequired Tests

    [Fact]
    public void GetRequired_WithExistingStringKey_ShouldReturnValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["StringKey"] = "MyStringValue" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.GetRequired<string>("StringKey");

        // Assert
        Assert.Equal("MyStringValue", result);
    }

    [Fact]
    public void GetRequired_WithExistingIntKey_ShouldReturnIntValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["IntKey"] = "42" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.GetRequired<int>("IntKey");

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetRequired_WithExistingBoolKey_ShouldReturnBoolValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["BoolKey"] = "true" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.GetRequired<bool>("BoolKey");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetRequired_WithExistingDecimalKey_ShouldReturnDecimalValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["DecimalKey"] = "123.45" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.GetRequired<decimal>("DecimalKey");

        // Assert
        Assert.Equal(123.45m, result);
    }

    [Fact]
    public void GetRequired_WithExistingLongKey_ShouldReturnLongValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["LongKey"] = "9223372036854775807" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.GetRequired<long>("LongKey");

        // Assert
        Assert.Equal(9223372036854775807L, result);
    }

    [Fact]
    public void GetRequired_WithExistingDoubleKey_ShouldReturnDoubleValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["DoubleKey"] = "3.14159" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.GetRequired<double>("DoubleKey");

        // Assert
        Assert.Equal(3.14159, result);
    }

    [Fact]
    public void GetRequired_WithExistingDateTimeKey_ShouldReturnDateTimeValue()
    {
        // Arrange
        var dateTimeString = "2025-04-19T10:30:00";
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["DateTimeKey"] = dateTimeString })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.GetRequired<DateTime>("DateTimeKey");

        // Assert
        Assert.Equal(DateTime.Parse(dateTimeString), result);
    }

    [Fact]
    public void GetRequired_WithExistingEnumKey_ShouldReturnEnumValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["EnumKey"] = "Red" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.GetRequired<TestColor>("EnumKey");

        // Assert
        Assert.Equal(TestColor.Red, result);
    }

    [Fact]
    public void GetRequired_WithEnumKeyIgnoreCase_ShouldReturnEnumValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["EnumKey"] = "red" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.GetRequired<TestColor>("EnumKey");

        // Assert
        Assert.Equal(TestColor.Red, result);
    }

    [Fact]
    public void GetRequired_WithMissingKey_ShouldThrowConfigurationValidationException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>())
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act & Assert
        var exception = Assert.Throws<ConfigurationValidationException>(() => helper.GetRequired<string>("MissingKey"));
        Assert.Contains("Required configuration key 'MissingKey' not found or empty.", exception.Message);
    }

    [Fact]
    public void GetRequired_WithEmptyStringValue_ShouldThrowConfigurationValidationException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["EmptyKey"] = "" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act & Assert
        var exception = Assert.Throws<ConfigurationValidationException>(() => helper.GetRequired<string>("EmptyKey"));
        Assert.Contains("Required configuration key 'EmptyKey' not found or empty.", exception.Message);
    }

    [Fact]
    public void GetRequired_WithWhitespaceValue_ShouldThrowConfigurationValidationException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["WhitespaceKey"] = "   " })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act & Assert
        var exception = Assert.Throws<ConfigurationValidationException>(() => helper.GetRequired<string>("WhitespaceKey"));
        Assert.Contains("Required configuration key 'WhitespaceKey' not found or empty.", exception.Message);
    }

    [Fact]
    public void GetRequired_WithInvalidIntConversion_ShouldThrowConfigurationValidationException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["InvalidInt"] = "not-a-number" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act & Assert
        var exception = Assert.Throws<ConfigurationValidationException>(() => helper.GetRequired<int>("InvalidInt"));
        Assert.Contains("InvalidInt", exception.Message);
    }

    [Fact]
    public void GetRequired_WithInvalidBoolConversion_ShouldThrowConfigurationValidationException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["InvalidBool"] = "maybe" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act & Assert
        var exception = Assert.Throws<ConfigurationValidationException>(() => helper.GetRequired<bool>("InvalidBool"));
        Assert.Contains("InvalidBool", exception.Message);
    }

    [Fact]
    public void GetRequired_WithInvalidDecimalConversion_ShouldThrowConfigurationValidationException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["InvalidDecimal"] = "not-a-decimal" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act & Assert
        var exception = Assert.Throws<ConfigurationValidationException>(() => helper.GetRequired<decimal>("InvalidDecimal"));
        Assert.Contains("InvalidDecimal", exception.Message);
    }

    [Fact]
    public void GetRequired_WithUnsupportedType_ShouldThrowConfigurationValidationException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["UnsupportedType"] = "value" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act & Assert
        var exception = Assert.Throws<ConfigurationValidationException>(() => helper.GetRequired<Guid>("UnsupportedType"));
        Assert.Contains("Type conversion not supported for type 'Guid'.", exception.Message);
    }

    #endregion

    #region GetOptional Tests

    [Fact]
    public void GetOptional_WithExistingStringKey_ShouldReturnValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["StringKey"] = "MyValue" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.GetOptional<string>("StringKey", "DefaultValue");

        // Assert
        Assert.Equal("MyValue", result);
    }

    [Fact]
    public void GetOptional_WithExistingIntKey_ShouldReturnValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["IntKey"] = "100" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.GetOptional<int>("IntKey", 999);

        // Assert
        Assert.Equal(100, result);
    }

    [Fact]
    public void GetOptional_WithMissingKey_ShouldReturnDefaultValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>())
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.GetOptional<string>("MissingKey", "DefaultValue");

        // Assert
        Assert.Equal("DefaultValue", result);
    }

    [Fact]
    public void GetOptional_WithEmptyValue_ShouldReturnDefaultValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["EmptyKey"] = "" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.GetOptional<string>("EmptyKey", "DefaultValue");

        // Assert
        Assert.Equal("DefaultValue", result);
    }

    [Fact]
    public void GetOptional_WithWhitespaceValue_ShouldReturnDefaultValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["WhitespaceKey"] = "   " })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.GetOptional<string>("WhitespaceKey", "DefaultValue");

        // Assert
        Assert.Equal("DefaultValue", result);
    }

    [Fact]
    public void GetOptional_WithInvalidIntConversion_ShouldReturnDefaultValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["InvalidInt"] = "not-a-number" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.GetOptional<int>("InvalidInt", 999);

        // Assert
        Assert.Equal(999, result);
    }

    [Fact]
    public void GetOptional_WithInvalidBoolConversion_ShouldReturnDefaultValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["InvalidBool"] = "maybe" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.GetOptional<bool>("InvalidBool", false);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region TryGet Tests

    [Fact]
    public void TryGet_WithExistingKey_ShouldReturnSuccessResult()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["StringKey"] = "MyValue" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.TryGet<string>("StringKey");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("MyValue", result.Data);
    }

    [Fact]
    public void TryGet_WithExistingIntKey_ShouldReturnSuccessResult()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["IntKey"] = "42" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.TryGet<int>("IntKey");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(42, result.Data);
    }

    [Fact]
    public void TryGet_WithMissingKey_ShouldReturnFailResult()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>())
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.TryGet<string>("MissingKey");

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Contains("MissingKey", result.Error.Message);
    }

    [Fact]
    public void TryGet_WithInvalidIntConversion_ShouldReturnFailResult()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["InvalidInt"] = "not-a-number" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.TryGet<int>("InvalidInt");

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void TryGet_WithEmptyValue_ShouldReturnFailResult()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["EmptyKey"] = "" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.TryGet<string>("EmptyKey");

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    #endregion

    #region Exists Tests

    [Fact]
    public void Exists_WithExistingKey_ShouldReturnTrue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["ExistingKey"] = "value" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.Exists("ExistingKey");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Exists_WithMissingKey_ShouldReturnFalse()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>())
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.Exists("MissingKey");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Exists_WithEmptyValue_ShouldReturnFalse()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["EmptyKey"] = "" })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.Exists("EmptyKey");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Exists_WithWhitespaceValue_ShouldReturnFalse()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["WhitespaceKey"] = "   " })
            .Build();
        var helper = new ConfigurationHelper(config);

        // Act
        var result = helper.Exists("WhitespaceKey");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Test Helper Enum

    public enum TestColor
    {
        Red,
        Green,
        Blue
    }

    #endregion
}
