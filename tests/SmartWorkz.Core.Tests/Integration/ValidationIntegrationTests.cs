using SmartWorkz.Shared;

namespace SmartWorkz.Core.Tests.Integration;

/// <summary>
/// Integration tests for Validation module interactions with services.
/// Tests validation rules and error message generation.
/// </summary>
public class ValidationIntegrationTests
{
    /// <summary>
    /// Test 1: ValidationFailure contains proper information.
    /// </summary>
    [Fact]
    public void ValidationFailure_WithPropertyAndMessage_Succeeds()
    {
        // Arrange
        var propertyName = "Email";
        var message = "Email is invalid";

        // Act
        var failure = new ValidationFailure(propertyName, message);

        // Assert
        Assert.Equal(propertyName, failure.PropertyName);
        Assert.Equal(message, failure.Message);
        Assert.NotNull(failure.ToString());
    }

    /// <summary>
    /// Test 2: ValidationResult with multiple failures.
    /// </summary>
    [Fact]
    public void ValidationResult_WithMultipleFailures_Succeeds()
    {
        // Arrange
        var failures = new[]
        {
            new ValidationFailure("Name", "Name is required"),
            new ValidationFailure("Email", "Email format is invalid")
        };

        // Act
        var result = new ValidationResult(failures);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Failures.Count);
        Assert.NotEmpty(result.Failures);
    }

    /// <summary>
    /// Test 3: ValidationResult success has no failures.
    /// </summary>
    [Fact]
    public void ValidationResult_Success_HasNoFailures()
    {
        // Act
        var result = ValidationResult.Success();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Failures);
    }

    /// <summary>
    /// Test 4: ValidationResult with single failure.
    /// </summary>
    [Fact]
    public void ValidationResult_WithSingleFailure_Succeeds()
    {
        // Arrange
        var failure = new ValidationFailure("Username", "Username is required");

        // Act
        var result = ValidationResult.Failure(failure);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Failures);
        Assert.Equal("Username", result.Failures.First().PropertyName);
    }

    /// <summary>
    /// Test 5: ValidationFailure toString method.
    /// </summary>
    [Fact]
    public void ValidationFailure_ToString_FormatsCorrectly()
    {
        // Arrange
        var propertyName = "Password";
        var message = "Password too short";
        var failure = new ValidationFailure(propertyName, message);

        // Act
        var result = failure.ToString();

        // Assert
        Assert.Contains(propertyName, result);
        Assert.Contains(message, result);
        Assert.NotEmpty(result);
    }

    /// <summary>
    /// Test 6: ValidationResult can be created from params array.
    /// </summary>
    [Fact]
    public void ValidationResult_FromParamsArray_Succeeds()
    {
        // Arrange
        var failure1 = new ValidationFailure("Field1", "Error1");
        var failure2 = new ValidationFailure("Field2", "Error2");
        var failure3 = new ValidationFailure("Field3", "Error3");

        // Act
        var result = ValidationResult.Failure(failure1, failure2, failure3);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(3, result.Failures.Count);
    }
}
