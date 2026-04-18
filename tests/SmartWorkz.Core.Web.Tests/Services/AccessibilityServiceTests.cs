namespace SmartWorkz.Core.Web.Tests.Services;

using SmartWorkz.Core.Web.Services.Components;
using Xunit;

public class AccessibilityServiceTests
{
    private readonly IAccessibilityService _service;

    public AccessibilityServiceTests()
    {
        _service = new AccessibilityService();
    }

    [Fact]
    public void GenerateFieldId_WithSimpleFieldName_ReturnsCorrectFormat()
    {
        // Arrange
        var fieldName = "Email";
        var expected = "field_email";

        // Act
        var result = _service.GenerateFieldId(fieldName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateErrorId_WithFieldName_ReturnsCorrectFormat()
    {
        // Arrange
        var fieldName = "Password";
        var expected = "error_password";

        // Act
        var result = _service.GenerateErrorId(fieldName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateHintId_WithFieldName_ReturnsCorrectFormat()
    {
        // Arrange
        var fieldName = "Username";
        var expected = "hint_username";

        // Act
        var result = _service.GenerateHintId(fieldName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateAriaLabel_WithFieldName_ReturnsFieldName()
    {
        // Arrange
        var fieldName = "Email";
        var expected = "Email";

        // Act
        var result = _service.GenerateAriaLabel(fieldName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateAriaLabel_WithRequiredTrue_AppendsRequiredText()
    {
        // Arrange
        var fieldName = "Email";
        var expected = "Email (required)";

        // Act
        var result = _service.GenerateAriaLabel(fieldName, required: true);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateFieldId_WithSpecialCharacters_SanitizesAndConvertsToLowercase()
    {
        // Arrange
        var fieldName = "User.Email";
        var expected = "field_user_email";

        // Act
        var result = _service.GenerateFieldId(fieldName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateErrorId_WithMultipleSpecialCharacters_ReplacesWithUnderscores()
    {
        // Arrange
        var fieldName = "FirstName/LastName";
        var expected = "error_firstname_lastname";

        // Act
        var result = _service.GenerateErrorId(fieldName);

        // Assert
        Assert.Equal(expected, result);
    }
}
