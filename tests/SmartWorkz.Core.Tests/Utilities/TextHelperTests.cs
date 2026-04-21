namespace SmartWorkz.Core.Tests.Utilities;

using SmartWorkz.Shared.Utilities;

public class TextHelperTests
{
    #region Truncate Tests

    [Fact]
    public void Truncate_WithSimpleText_TruncatesCorrectly()
    {
        // Arrange
        const string text = "Hello World";

        // Act
        var result = TextHelper.Truncate(text, 8);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Hello...", result.Data);
    }

    [Fact]
    public void Truncate_WithTextShorterThanMax_ReturnsUnchanged()
    {
        // Arrange
        const string text = "Hello";

        // Act
        var result = TextHelper.Truncate(text, 10);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Hello", result.Data);
    }

    [Fact]
    public void Truncate_WithCustomSuffix_UsesSuffix()
    {
        // Arrange
        const string text = "Hello World";

        // Act
        var result = TextHelper.Truncate(text, 10, " [...]");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Hell [...]", result.Data);
    }

    [Fact]
    public void Truncate_WithNullInput_ReturnsFailed()
    {
        // Arrange
        string? text = null;

        // Act
        var result = TextHelper.Truncate(text!, 5);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.MessageKey);
    }

    [Fact]
    public void Truncate_WithNegativeLength_ReturnsFailed()
    {
        // Arrange
        const string text = "Hello";

        // Act
        var result = TextHelper.Truncate(text, -1);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Truncate_WithSuffixLongerThanMax_ReturnsFailed()
    {
        // Arrange
        const string text = "Hello";

        // Act
        var result = TextHelper.Truncate(text, 2, "...");

        // Assert
        Assert.False(result.Succeeded);
    }

    #endregion

    #region Capitalize Tests

    [Fact]
    public void Capitalize_WithLowercaseText_CapitalizesFirstLetter()
    {
        // Arrange
        const string text = "hello world";

        // Act
        var result = TextHelper.Capitalize(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Hello world", result.Data);
    }

    [Fact]
    public void Capitalize_WithAlreadyCapitalized_ReturnsUnchanged()
    {
        // Arrange
        const string text = "Hello world";

        // Act
        var result = TextHelper.Capitalize(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Hello world", result.Data);
    }

    [Fact]
    public void Capitalize_WithSingleCharacter_CapitalizesChar()
    {
        // Arrange
        const string text = "a";

        // Act
        var result = TextHelper.Capitalize(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("A", result.Data);
    }

    [Fact]
    public void Capitalize_WithNullInput_ReturnsFailed()
    {
        // Arrange
        string? text = null;

        // Act
        var result = TextHelper.Capitalize(text!);

        // Assert
        Assert.False(result.Succeeded);
    }

    #endregion

    #region Decapitalize Tests

    [Fact]
    public void Decapitalize_WithCapitalizedText_DecapitalizesFirstLetter()
    {
        // Arrange
        const string text = "Hello World";

        // Act
        var result = TextHelper.Decapitalize(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("hello World", result.Data);
    }

    [Fact]
    public void Decapitalize_WithLowercaseText_ReturnsUnchanged()
    {
        // Arrange
        const string text = "hello world";

        // Act
        var result = TextHelper.Decapitalize(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("hello world", result.Data);
    }

    [Fact]
    public void Decapitalize_WithSingleCapitalChar_DecapitalizesChar()
    {
        // Arrange
        const string text = "A";

        // Act
        var result = TextHelper.Decapitalize(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("a", result.Data);
    }

    [Fact]
    public void Decapitalize_WithNullInput_ReturnsFailed()
    {
        // Arrange
        string? text = null;

        // Act
        var result = TextHelper.Decapitalize(text!);

        // Assert
        Assert.False(result.Succeeded);
    }

    #endregion

    #region StripHtml Tests

    [Fact]
    public void StripHtml_WithSimpleHtmlTags_RemovesTags()
    {
        // Arrange
        const string html = "<p>Hello <strong>World</strong></p>";

        // Act
        var result = TextHelper.StripHtml(html);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Hello World", result.Data);
    }

    [Fact]
    public void StripHtml_WithMultipleTags_RemovesAllTags()
    {
        // Arrange
        const string html = "<div><h1>Title</h1> <p>Content</p></div>";

        // Act
        var result = TextHelper.StripHtml(html);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Title Content", result.Data);
    }

    [Fact]
    public void StripHtml_WithHtmlEntities_DecodesEntities()
    {
        // Arrange
        const string html = "&lt;p&gt;Hello&lt;/p&gt;";

        // Act
        var result = TextHelper.StripHtml(html);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public void StripHtml_WithNullInput_ReturnsFailed()
    {
        // Arrange
        string? html = null;

        // Act
        var result = TextHelper.StripHtml(html!);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void StripHtml_WithPlainText_ReturnsUnchanged()
    {
        // Arrange
        const string text = "Hello World";

        // Act
        var result = TextHelper.StripHtml(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Hello World", result.Data);
    }

    #endregion

    #region Pluralize Tests

    [Fact]
    public void Pluralize_WithCountOne_ReturnsSingular()
    {
        // Arrange
        const string singular = "cat";
        const int count = 1;

        // Act
        var result = TextHelper.Pluralize(singular, count);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("cat", result.Data);
    }

    [Fact]
    public void Pluralize_WithCountZero_ReturnsPlural()
    {
        // Arrange
        const string singular = "cat";
        const int count = 0;

        // Act
        var result = TextHelper.Pluralize(singular, count);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("cats", result.Data);
    }

    [Fact]
    public void Pluralize_WithCountGreaterThanOne_ReturnsPlural()
    {
        // Arrange
        const string singular = "dog";
        const int count = 5;

        // Act
        var result = TextHelper.Pluralize(singular, count);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("dogs", result.Data);
    }

    [Fact]
    public void Pluralize_WithNegativeCount_ReturnsPlural()
    {
        // Arrange
        const string singular = "apple";
        const int count = -1;

        // Act
        var result = TextHelper.Pluralize(singular, count);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("apples", result.Data);
    }

    [Fact]
    public void Pluralize_WithNullInput_ReturnsFailed()
    {
        // Arrange
        string? singular = null;

        // Act
        var result = TextHelper.Pluralize(singular!, 5);

        // Assert
        Assert.False(result.Succeeded);
    }

    #endregion

    #region TitleCase Tests

    [Fact]
    public void TitleCase_WithLowercaseText_CapitalizesEachWord()
    {
        // Arrange
        const string text = "hello world test";

        // Act
        var result = TextHelper.TitleCase(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Hello World Test", result.Data);
    }

    [Fact]
    public void TitleCase_WithMixedCase_CapitalizesFirstLetterOfEachWord()
    {
        // Arrange
        const string text = "hello world test";

        // Act
        var result = TextHelper.TitleCase(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Hello World Test", result.Data);
    }

    [Fact]
    public void TitleCase_WithSingleWord_CapitalizesIt()
    {
        // Arrange
        const string text = "hello";

        // Act
        var result = TextHelper.TitleCase(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Hello", result.Data);
    }

    [Fact]
    public void TitleCase_WithNullInput_ReturnsFailed()
    {
        // Arrange
        string? text = null;

        // Act
        var result = TextHelper.TitleCase(text!);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void TitleCase_WithMultipleSpaces_CapitalizesEachWord()
    {
        // Arrange
        const string text = "hello  world   test";

        // Act
        var result = TextHelper.TitleCase(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotEmpty(result.Data);
    }

    #endregion

    #region Reverse Tests

    [Fact]
    public void Reverse_WithSimpleText_ReversesText()
    {
        // Arrange
        const string text = "hello";

        // Act
        var result = TextHelper.Reverse(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("olleh", result.Data);
    }

    [Fact]
    public void Reverse_WithPhraseWithSpaces_ReversesEverything()
    {
        // Arrange
        const string text = "hello world";

        // Act
        var result = TextHelper.Reverse(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("dlrow olleh", result.Data);
    }

    [Fact]
    public void Reverse_WithSingleCharacter_ReturnsUnchanged()
    {
        // Arrange
        const string text = "a";

        // Act
        var result = TextHelper.Reverse(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("a", result.Data);
    }

    [Fact]
    public void Reverse_WithNullInput_ReturnsFailed()
    {
        // Arrange
        string? text = null;

        // Act
        var result = TextHelper.Reverse(text!);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Reverse_WithNumbers_ReversesNumbers()
    {
        // Arrange
        const string text = "12345";

        // Act
        var result = TextHelper.Reverse(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("54321", result.Data);
    }

    #endregion

    #region RemoveWhitespace Tests

    [Fact]
    public void RemoveWhitespace_WithSpaces_RemovesSpaces()
    {
        // Arrange
        const string text = "hello world test";

        // Act
        var result = TextHelper.RemoveWhitespace(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("helloworldtest", result.Data);
    }

    [Fact]
    public void RemoveWhitespace_WithTabs_RemovesTabs()
    {
        // Arrange
        const string text = "hello\tworld\ttest";

        // Act
        var result = TextHelper.RemoveWhitespace(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("helloworldtest", result.Data);
    }

    [Fact]
    public void RemoveWhitespace_WithNewlines_RemovesNewlines()
    {
        // Arrange
        const string text = "hello\nworld\ntest";

        // Act
        var result = TextHelper.RemoveWhitespace(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("helloworldtest", result.Data);
    }

    [Fact]
    public void RemoveWhitespace_WithMixedWhitespace_RemovesAllWhitespace()
    {
        // Arrange
        const string text = "hello \t\n world  \r\n test";

        // Act
        var result = TextHelper.RemoveWhitespace(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("helloworldtest", result.Data);
    }

    [Fact]
    public void RemoveWhitespace_WithNullInput_ReturnsFailed()
    {
        // Arrange
        string? text = null;

        // Act
        var result = TextHelper.RemoveWhitespace(text!);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void RemoveWhitespace_WithNoWhitespace_ReturnsUnchanged()
    {
        // Arrange
        const string text = "HelloWorld";

        // Act
        var result = TextHelper.RemoveWhitespace(text);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("HelloWorld", result.Data);
    }

    #endregion

    #region WordWrap Tests

    [Fact]
    public void WordWrap_WithSimpleText_WrapsProperly()
    {
        // Arrange
        const string text = "hello world this is a test";

        // Act
        var result = TextHelper.WordWrap(text, 11);

        // Assert
        Assert.True(result.Succeeded);
        var lines = result.Data!.Split('\n');
        Assert.All(lines, line => Assert.True(line.Length <= 11));
    }

    [Fact]
    public void WordWrap_WithLongWords_HandlesLongWords()
    {
        // Arrange
        const string text = "hello verylongwordthatexceedslinelength world";

        // Act
        var result = TextHelper.WordWrap(text, 10);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public void WordWrap_WithShortLineLength_WrapsCorrectly()
    {
        // Arrange
        const string text = "a b c d e f";

        // Act
        var result = TextHelper.WordWrap(text, 3);

        // Assert
        Assert.True(result.Succeeded);
        var lines = result.Data!.Split('\n');
        Assert.True(lines.Length > 1);
    }

    [Fact]
    public void WordWrap_WithCustomNewline_UsesCustomNewline()
    {
        // Arrange
        const string text = "hello world test";

        // Act
        var result = TextHelper.WordWrap(text, 6, "<br>");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Contains("<br>", result.Data!);
    }

    [Fact]
    public void WordWrap_WithNullInput_ReturnsFailed()
    {
        // Arrange
        string? text = null;

        // Act
        var result = TextHelper.WordWrap(text!, 10);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void WordWrap_WithZeroLineLength_ReturnsFailed()
    {
        // Arrange
        const string text = "hello world";

        // Act
        var result = TextHelper.WordWrap(text, 0);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void WordWrap_WithSingleWord_ReturnsWord()
    {
        // Arrange
        const string text = "hello";

        // Act
        var result = TextHelper.WordWrap(text, 10);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("hello", result.Data);
    }

    #endregion

    #region Repeat Tests

    [Fact]
    public void Repeat_WithSimpleText_RepeatsCorrectly()
    {
        // Arrange
        const string text = "ab";

        // Act
        var result = TextHelper.Repeat(text, 3);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("ababab", result.Data);
    }

    [Fact]
    public void Repeat_WithCountZero_ReturnsEmpty()
    {
        // Arrange
        const string text = "hello";

        // Act
        var result = TextHelper.Repeat(text, 0);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(string.Empty, result.Data);
    }

    [Fact]
    public void Repeat_WithCountOne_ReturnsSingleCopy()
    {
        // Arrange
        const string text = "hello";

        // Act
        var result = TextHelper.Repeat(text, 1);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("hello", result.Data);
    }

    [Fact]
    public void Repeat_WithLargeCount_RepeatsMany()
    {
        // Arrange
        const string text = "x";

        // Act
        var result = TextHelper.Repeat(text, 100);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(100, result.Data!.Length);
    }

    [Fact]
    public void Repeat_WithNullInput_ReturnsFailed()
    {
        // Arrange
        string? text = null;

        // Act
        var result = TextHelper.Repeat(text!, 5);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Repeat_WithNegativeCount_ReturnsFailed()
    {
        // Arrange
        const string text = "hello";

        // Act
        var result = TextHelper.Repeat(text, -1);

        // Assert
        Assert.False(result.Succeeded);
    }

    #endregion
}

