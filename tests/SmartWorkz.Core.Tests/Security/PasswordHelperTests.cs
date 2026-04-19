namespace SmartWorkz.Core.Tests.Security;

using SmartWorkz.Core.Shared.Security;

public class PasswordHelperTests
{
    [Fact]
    public async Task GeneratePassword_ProducesValidStrongPassword()
    {
        // Arrange & Act
        var result = await PasswordHelper.GeneratePassword();

        // Assert
        Assert.True(result.Succeeded);
        var password = result.Data!;
        var validationResult = PasswordHelper.ValidateStrength(password);
        Assert.True(validationResult.Succeeded);
        Assert.True(validationResult.Data!.IsValid);
    }

    [Fact]
    public async Task GeneratePassword_RespectsLengthParameter()
    {
        // Arrange
        int expectedLength = 16;

        // Act
        var result = await PasswordHelper.GeneratePassword(length: expectedLength);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(expectedLength, result.Data!.Length);
    }

    [Fact]
    public async Task GeneratePassword_IncludesSpecialCharsWhenRequested()
    {
        // Arrange
        const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        // Act
        var result = await PasswordHelper.GeneratePassword(includeSpecialChars: true);

        // Assert
        Assert.True(result.Succeeded);
        var password = result.Data!;
        var hasSpecialChar = password.Any(c => specialChars.Contains(c));
        Assert.True(hasSpecialChar, "Password should contain at least one special character");
    }

    [Fact]
    public async Task GeneratePassword_ExcludesSpecialCharsWhenNotRequested()
    {
        // Arrange
        const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        // Act
        var result = await PasswordHelper.GeneratePassword(includeSpecialChars: false);

        // Assert
        Assert.True(result.Succeeded);
        var password = result.Data!;
        var hasSpecialChar = password.Any(c => specialChars.Contains(c));
        Assert.False(hasSpecialChar, "Password should not contain special characters");
    }

    [Theory]
    [InlineData(7)]
    [InlineData(129)]
    public async Task GeneratePassword_RejectsInvalidLengths(int invalidLength)
    {
        // Act
        var result = await PasswordHelper.GeneratePassword(length: invalidLength);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void ValidateStrength_RejectsNullPassword()
    {
        // Act
        var result = PasswordHelper.ValidateStrength(null);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void ValidateStrength_RejectsWeakPassword()
    {
        // Arrange
        var weakPassword = "abc";

        // Act
        var result = PasswordHelper.ValidateStrength(weakPassword);

        // Assert
        Assert.True(result.Succeeded);
        var validationResult = result.Data!;
        Assert.False(validationResult.IsValid);
        Assert.NotEmpty(validationResult.FailedRequirements);
    }

    [Fact]
    public async Task ValidateStrength_AcceptsStrongPassword()
    {
        // Arrange
        var generateResult = await PasswordHelper.GeneratePassword();
        var password = generateResult.Data!;

        // Act
        var validationResult = PasswordHelper.ValidateStrength(password);

        // Assert
        Assert.True(validationResult.Succeeded);
        Assert.True(validationResult.Data!.IsValid);
        Assert.Empty(validationResult.Data.FailedRequirements);
    }

    [Fact]
    public void ValidateStrength_EnforcesMinLength()
    {
        // Arrange
        var policy = new PasswordPolicy { MinLength = 10 };
        var shortPassword = "short";

        // Act
        var result = PasswordHelper.ValidateStrength(shortPassword, policy);

        // Assert
        Assert.True(result.Succeeded);
        var validationResult = result.Data!;
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.FailedRequirements, m => m.Contains("at least 10 characters"));
    }

    [Fact]
    public void ValidateStrength_EnforcesRequireUppercase()
    {
        // Arrange
        var policy = new PasswordPolicy { RequireUppercase = true };
        var noUppercasePassword = "password123!";

        // Act
        var result = PasswordHelper.ValidateStrength(noUppercasePassword, policy);

        // Assert
        Assert.True(result.Succeeded);
        var validationResult = result.Data!;
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.FailedRequirements, m => m.Contains("uppercase"));
    }

    [Fact]
    public void ValidateStrength_EnforcesRequireNumbers()
    {
        // Arrange
        var policy = new PasswordPolicy { RequireNumbers = true };
        var noNumbersPassword = "Password!";

        // Act
        var result = PasswordHelper.ValidateStrength(noNumbersPassword, policy);

        // Assert
        Assert.True(result.Succeeded);
        var validationResult = result.Data!;
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.FailedRequirements, m => m.Contains("digit"));
    }

    [Fact]
    public void ValidateStrength_EnforcesRequireSpecialChars()
    {
        // Arrange
        var policy = new PasswordPolicy { RequireSpecialChars = true };
        var noSpecialPassword = "Password123";

        // Act
        var result = PasswordHelper.ValidateStrength(noSpecialPassword, policy);

        // Assert
        Assert.True(result.Succeeded);
        var validationResult = result.Data!;
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.FailedRequirements, m => m.Contains("special character"));
    }

    [Fact]
    public void ValidateStrength_WithCustomPolicy()
    {
        // Arrange
        var policy = new PasswordPolicy
        {
            MinLength = 12,
            RequireUppercase = true,
            RequireLowercase = true,
            RequireNumbers = true,
            RequireSpecialChars = true
        };
        var password = "ValidPass123!";

        // Act
        var result = PasswordHelper.ValidateStrength(password, policy);

        // Assert
        Assert.True(result.Succeeded);
        var validationResult = result.Data!;
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public void ValidateStrength_ReportsMultipleFailedRequirements()
    {
        // Arrange
        var policy = new PasswordPolicy
        {
            RequireUppercase = true,
            RequireLowercase = true,
            RequireNumbers = true,
            RequireSpecialChars = true
        };
        var weakPassword = "abc";

        // Act
        var result = PasswordHelper.ValidateStrength(weakPassword, policy);

        // Assert
        Assert.True(result.Succeeded);
        var validationResult = result.Data!;
        Assert.False(validationResult.IsValid);
        Assert.NotEmpty(validationResult.FailedRequirements);
        Assert.True(validationResult.FailedRequirements.Count > 1, "Should report multiple failed requirements");
    }

    [Fact]
    public void ValidateStrength_UsesDefaultPolicyWhenNoneProvided()
    {
        // Arrange
        var password = "Password123"; // Valid per default policy (uppercase, lowercase, numbers)

        // Act
        var result = PasswordHelper.ValidateStrength(password, null);

        // Assert
        Assert.True(result.Succeeded);
        var validationResult = result.Data!;
        Assert.True(validationResult.IsValid, "Password should be valid with default policy (no special chars required)");
    }
}
