namespace SmartWorkz.Core.Web.Tests.Services;

using SmartWorkz.Core.Web.Services.Components;
using Xunit;

public class ValidationMessageProviderTests
{
    private readonly IValidationMessageProvider _provider;

    public ValidationMessageProviderTests()
    {
        _provider = new ValidationMessageProvider();
    }

    [Fact]
    public void GetMessage_WithKnownErrorType_ReturnsCorrectMessage()
    {
        // Arrange
        var errorType = "required";
        var expectedMessage = "This field is required";

        // Act
        var result = _provider.GetMessage(errorType);

        // Assert
        Assert.Equal(expectedMessage, result);
    }

    [Fact]
    public void GetMessage_WithUnknownErrorType_ReturnsFallbackMessage()
    {
        // Arrange
        var errorType = "unknownValidationError";

        // Act
        var result = _provider.GetMessage(errorType);

        // Assert
        Assert.StartsWith("Validation error:", result);
        Assert.Contains(errorType, result);
    }

    [Fact]
    public void GetMessage_WithPropertyName_IncludesPropertyNameInResult()
    {
        // Arrange
        var errorType = "required";
        var propertyName = "Email";
        var expectedMessage = "Email: This field is required";

        // Act
        var result = _provider.GetMessage(errorType, propertyName);

        // Assert
        Assert.Equal(expectedMessage, result);
    }

    [Fact]
    public void GetMessage_WithNullPropertyName_ReturnsMessageOnly()
    {
        // Arrange
        var errorType = "email";
        var expectedMessage = "Please enter a valid email address";

        // Act
        var result = _provider.GetMessage(errorType, null);

        // Assert
        Assert.Equal(expectedMessage, result);
    }

    [Fact]
    public void RegisterMessage_WithCustomMessage_AllowsCustomMessageRegistration()
    {
        // Arrange
        var errorType = "customError";
        var customMessage = "This is a custom validation error";

        // Act
        _provider.RegisterMessage(errorType, customMessage);
        var result = _provider.GetMessage(errorType);

        // Assert
        Assert.Equal(customMessage, result);
    }

    [Fact]
    public void RegisterMessage_WithExistingErrorType_OverridesExistingMessage()
    {
        // Arrange
        var errorType = "required";
        var newMessage = "This field cannot be left blank";

        // Act
        _provider.RegisterMessage(errorType, newMessage);
        var result = _provider.GetMessage(errorType);

        // Assert
        Assert.Equal(newMessage, result);
    }

    [Fact]
    public void GetMessage_WithCaseInsensitiveErrorType_ReturnsCorrectMessage()
    {
        // Arrange
        var errorType = "REQUIRED";

        // Act
        var result = _provider.GetMessage(errorType);

        // Assert
        Assert.Equal("This field is required", result);
    }

    [Fact]
    public void GetMessage_WithEmptyPropertyName_ReturnsMessageOnly()
    {
        // Arrange
        var errorType = "email";

        // Act
        var result = _provider.GetMessage(errorType, "");

        // Assert
        Assert.Equal("Please enter a valid email address", result);
    }

    [Fact]
    public void GetMessage_WithAllDefaultErrorTypes_ReturnsValidMessages()
    {
        // Arrange
        var errorTypes = new[] { "required", "email", "minlength", "maxlength", "pattern", "min", "max", "unique", "invalid", "match", "regex", "url", "number", "date" };

        // Act & Assert - verify each error type returns a non-empty message
        foreach (var errorType in errorTypes)
        {
            var result = _provider.GetMessage(errorType);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.DoesNotContain("Validation error:", result); // Should not be fallback message
        }
    }
}
