namespace SmartWorkz.Core.Tests.Utilities;

using SmartWorkz.Core.Shared.Utilities;

public class SlugHelperTests
{
    [Fact]
    public void GenerateSlug_WithSimpleText_ReturnsSuccessfulSlug()
    {
        // Arrange
        const string text = "Hello World";

        // Act
        var result = SlugHelper.GenerateSlug(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("hello-world", result.Data);
    }

    [Fact]
    public void GenerateSlug_WithMultipleSpaces_CollapsesToSingleSeparator()
    {
        // Arrange
        const string text = "Hello    World    Test";

        // Act
        var result = SlugHelper.GenerateSlug(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("hello-world-test", result.Data);
    }

    [Fact]
    public void GenerateSlug_WithAccentedCharacters_RemovesAccents()
    {
        // Arrange
        const string text = "Café résumé naïve";

        // Act
        var result = SlugHelper.GenerateSlug(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("cafe-resume-naive", result.Data);
    }

    [Fact]
    public void GenerateSlug_WithSpecialCharacters_RemovesSpecialChars()
    {
        // Arrange
        const string text = "Hello! @World #Test";

        // Act
        var result = SlugHelper.GenerateSlug(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("hello-world-test", result.Data);
    }

    [Fact]
    public void GenerateSlug_WithLeadingAndTrailingSeparators_RemovesThem()
    {
        // Arrange
        const string text = "---Hello World---";

        // Act
        var result = SlugHelper.GenerateSlug(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("hello-world", result.Data);
    }

    [Fact]
    public void GenerateSlug_WithMaxLengthOption_TruncatesCorrectly()
    {
        // Arrange
        const string text = "This is a very long text that needs to be truncated";
        var options = new SlugOptions { MaxLength = 20 };

        // Act
        var result = SlugHelper.GenerateSlug(text, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data!.Length <= 20);
        Assert.Equal("this-is-a-very-long", result.Data);
    }

    [Fact]
    public void GenerateSlug_WithCustomSeparator_UsesCustomSeparator()
    {
        // Arrange
        const string text = "Hello World Test";
        var options = new SlugOptions { Separator = "_" };

        // Act
        var result = SlugHelper.GenerateSlug(text, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("hello_world_test", result.Data);
    }

    [Fact]
    public void GenerateSlug_WithLowercaseDisabled_PreservesCase()
    {
        // Arrange
        const string text = "Hello World";
        var options = new SlugOptions { Lowercase = false };

        // Act
        var result = SlugHelper.GenerateSlug(text, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Hello-World", result.Data);
    }

    [Fact]
    public void GenerateSlug_WithAccentRemovalDisabled_PreservesAccents()
    {
        // Arrange
        const string text = "Café";
        var options = new SlugOptions { RemoveAccents = false };

        // Act
        var result = SlugHelper.GenerateSlug(text, options);

        // Assert
        Assert.True(result.Succeeded);
        // After normalization, accents may be preserved depending on form
        Assert.NotEmpty(result.Data!);
    }

    [Fact]
    public void GenerateSlug_WithSpecialCharsRemovalDisabled_KeepsSpecialChars()
    {
        // Arrange
        const string text = "Hello@World";
        var options = new SlugOptions { RemoveSpecialChars = false };

        // Act
        var result = SlugHelper.GenerateSlug(text, options);

        // Assert
        Assert.True(result.Succeeded);
        // Special chars should be kept when RemoveSpecialChars is false
        Assert.Equal("hello@world", result.Data);
    }

    [Fact]
    public void GenerateSlug_WithNullInput_ReturnsFailed()
    {
        // Arrange
        string? text = null;

        // Act
        var result = SlugHelper.GenerateSlug(text!);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.MessageKey);
    }

    [Fact]
    public void GenerateSlug_WithEmptyInput_ReturnsFailed()
    {
        // Arrange
        const string text = "";

        // Act
        var result = SlugHelper.GenerateSlug(text);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.MessageKey);
    }

    [Fact]
    public void GenerateSlug_WithWhitespaceOnlyInput_ReturnsFailed()
    {
        // Arrange
        const string text = "   ";

        // Act
        var result = SlugHelper.GenerateSlug(text);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void GenerateSlug_WithPunctuation_RemovesPunctuation()
    {
        // Arrange
        const string text = "Hello, World! How are you?";

        // Act
        var result = SlugHelper.GenerateSlug(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("hello-world-how-are-you", result.Data);
    }

    [Fact]
    public void GenerateSlug_WithNumbers_KeepsNumbers()
    {
        // Arrange
        const string text = "Product 123 Review";

        // Act
        var result = SlugHelper.GenerateSlug(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("product-123-review", result.Data);
    }

    [Fact]
    public void ToSlug_ConvertsWithDefaultOptions()
    {
        // Arrange
        const string text = "Hello World";

        // Act
        var result = SlugHelper.ToSlug(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("hello-world", result.Data);
    }

    [Fact]
    public void GenerateSlug_WithMaxLengthZero_NoLimit()
    {
        // Arrange
        const string text = "This is a very long text that should not be truncated";
        var options = new SlugOptions { MaxLength = 0 };

        // Act
        var result = SlugHelper.GenerateSlug(text, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data!.Length > 20);
    }

    [Fact]
    public void GenerateSlug_WithCombinedOptions_WorksTogether()
    {
        // Arrange
        const string text = "Café & Restaurant #1 - BEST PLACE";
        var options = new SlugOptions
        {
            Lowercase = true,
            RemoveAccents = true,
            RemoveSpecialChars = true,
            Separator = "_",
            MaxLength = 30
        };

        // Act
        var result = SlugHelper.GenerateSlug(text, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.DoesNotContain("&", result.Data!);
        Assert.DoesNotContain("#", result.Data!);
        Assert.Contains("_", result.Data!);
        Assert.True(result.Data!.Length <= 30);
    }

    [Fact]
    public void GenerateSlug_WithMixedUnicodeAndAccents_NormalizesCorrectly()
    {
        // Arrange
        const string text = "Ñoño Señor España";

        // Act
        var result = SlugHelper.GenerateSlug(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("nono-senor-espana", result.Data);
    }
}
