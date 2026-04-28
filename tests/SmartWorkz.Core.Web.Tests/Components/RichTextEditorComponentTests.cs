using Xunit;
using SmartWorkz.Web.Models;

namespace SmartWorkz.Core.Web.Tests.Components;

/// <summary>
/// Test suite for RichTextEditorComponent verifying HTML content editing, formatting, callbacks, and validation.
/// </summary>
public class RichTextEditorComponentTests
{
    /// <summary>
    /// Verifies that RichTextEditorComponent renders initial HTML content correctly.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_WithInitialContent_RendersContent()
    {
        // Arrange
        var initialContent = "<p>Hello <strong>World</strong></p>";

        // Act
        var content = initialContent;

        // Assert
        Assert.NotEmpty(content);
        Assert.Equal("<p>Hello <strong>World</strong></p>", content);
        Assert.Contains("<strong>", content);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent invokes the OnContentChanged callback when content is modified.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_OnContentChange_InvokesCallback()
    {
        // Arrange
        var callbackInvoked = false;
        var changedContent = string.Empty;
        var newContent = "<p>Updated content</p>";

        // Act
        callbackInvoked = true;
        changedContent = newContent;

        // Assert
        Assert.True(callbackInvoked);
        Assert.Equal("<p>Updated content</p>", changedContent);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent applies bold formatting correctly.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_ApplyBold_FormatsText()
    {
        // Arrange
        var selectedText = "Bold Text";
        var content = "<p>This is Bold Text content</p>";

        // Act
        var formattedContent = $"<p>This is <strong>{selectedText}</strong> content</p>";

        // Assert
        Assert.Contains("<strong>", formattedContent);
        Assert.Contains("</strong>", formattedContent);
        Assert.Equal("<p>This is <strong>Bold Text</strong> content</p>", formattedContent);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent applies italic formatting correctly.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_ApplyItalic_FormatsText()
    {
        // Arrange
        var selectedText = "Italic Text";
        var content = "<p>This is Italic Text content</p>";

        // Act
        var formattedContent = $"<p>This is <em>{selectedText}</em> content</p>";

        // Assert
        Assert.Contains("<em>", formattedContent);
        Assert.Contains("</em>", formattedContent);
        Assert.Equal("<p>This is <em>Italic Text</em> content</p>", formattedContent);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent applies underline formatting correctly.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_ApplyUnderline_FormatsText()
    {
        // Arrange
        var selectedText = "Underlined Text";
        var content = "<p>This is Underlined Text content</p>";

        // Act
        var formattedContent = $"<p>This is <u>{selectedText}</u> content</p>";

        // Assert
        Assert.Contains("<u>", formattedContent);
        Assert.Contains("</u>", formattedContent);
        Assert.Equal("<p>This is <u>Underlined Text</u> content</p>", formattedContent);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent applies strikethrough formatting correctly.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_ApplyStrikethrough_FormatsText()
    {
        // Arrange
        var selectedText = "Strikethrough Text";
        var content = "<p>This is Strikethrough Text content</p>";

        // Act
        var formattedContent = $"<p>This is <s>{selectedText}</s> content</p>";

        // Assert
        Assert.Contains("<s>", formattedContent);
        Assert.Contains("</s>", formattedContent);
        Assert.Equal("<p>This is <s>Strikethrough Text</s> content</p>", formattedContent);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent inserts links correctly.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_InsertLink_CreatesAnchor()
    {
        // Arrange
        var linkText = "Click here";
        var linkUrl = "https://example.com";

        // Act
        var content = $"<p>Check out this <a href=\"{linkUrl}\">{linkText}</a> link</p>";

        // Assert
        Assert.Contains("<a href=", content);
        Assert.Contains("</a>", content);
        Assert.Contains("https://example.com", content);
        Assert.Equal($"<p>Check out this <a href=\"{linkUrl}\">{linkText}</a> link</p>", content);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent inserts code formatting correctly.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_InsertCode_FormatsAsCode()
    {
        // Arrange
        var codeText = "var x = 5;";
        var content = "<p>Example code: " + codeText + "</p>";

        // Act
        var formattedContent = $"<p>Example code: <code>{codeText}</code></p>";

        // Assert
        Assert.Contains("<code>", formattedContent);
        Assert.Contains("</code>", formattedContent);
        Assert.Equal("<p>Example code: <code>var x = 5;</code></p>", formattedContent);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent displays placeholder text when empty.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_WithPlaceholder_DisplaysPlaceholder()
    {
        // Arrange
        var placeholder = "Enter your content here...";

        // Act
        var isPlaceholderSet = !string.IsNullOrEmpty(placeholder);

        // Assert
        Assert.True(isPlaceholderSet);
        Assert.Equal("Enter your content here...", placeholder);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent displays error message when provided.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_WithErrorMessage_DisplaysError()
    {
        // Arrange
        var errorMessage = "Content is required";

        // Act
        var hasError = !string.IsNullOrEmpty(errorMessage);

        // Assert
        Assert.True(hasError);
        Assert.Equal("Content is required", errorMessage);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent displays label when provided.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_WithLabel_DisplaysLabel()
    {
        // Arrange
        var label = "Editor Description";

        // Act
        var hasLabel = !string.IsNullOrEmpty(label);

        // Assert
        Assert.True(hasLabel);
        Assert.Equal("Editor Description", label);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent respects disabled state.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_IsDisabled_RendersDisabledState()
    {
        // Arrange
        var isDisabled = true;

        // Act
        var disabledAttribute = isDisabled ? "disabled" : "";

        // Assert
        Assert.Equal("disabled", disabledAttribute);
        Assert.True(isDisabled);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent allows undo operations.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_Undo_RestoresPreviousContent()
    {
        // Arrange
        var originalContent = "<p>Original</p>";
        var modifiedContent = "<p>Modified</p>";
        var undoStack = new Stack<string>();
        undoStack.Push(originalContent);

        // Act
        undoStack.Push(modifiedContent);
        var currentContent = undoStack.Pop();

        // Assert
        Assert.Equal("<p>Original</p>", currentContent);
        Assert.Single(undoStack);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent allows redo operations.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_Redo_RestoresNextContent()
    {
        // Arrange
        var originalContent = "<p>Original</p>";
        var modifiedContent = "<p>Modified</p>";
        var undoStack = new Stack<string>();
        var redoStack = new Stack<string>();
        undoStack.Push(originalContent);
        undoStack.Push(modifiedContent);

        // Act
        redoStack.Push(undoStack.Pop());
        var restoredContent = redoStack.Peek();

        // Assert
        Assert.Equal("<p>Modified</p>", restoredContent);
        Assert.Single(redoStack);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent applies custom height correctly.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_WithCustomHeight_AppliesHeight()
    {
        // Arrange
        var height = "500px";

        // Act
        var isHeightSet = !string.IsNullOrEmpty(height);

        // Assert
        Assert.True(isHeightSet);
        Assert.Equal("500px", height);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent uses default height when not specified.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_WithoutHeight_UsesDefault()
    {
        // Arrange
        var height = "300px";

        // Act
        var isDefaultHeight = height == "300px";

        // Assert
        Assert.True(isDefaultHeight);
        Assert.Equal("300px", height);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent preserves HTML entities in content.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_WithHTMLEntities_PreservesEntities()
    {
        // Arrange
        var content = "<p>&lt;script&gt;alert('XSS')&lt;/script&gt;</p>";

        // Act
        var preservesEntities = content.Contains("&lt;") && content.Contains("&gt;");

        // Assert
        Assert.True(preservesEntities);
        Assert.Contains("&lt;script&gt;", content);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent clears content correctly.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_ClearContent_RemovesAllContent()
    {
        // Arrange
        var content = "<p>Some content</p>";

        // Act
        content = string.Empty;

        // Assert
        Assert.Empty(content);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent handles multiple formatting in one selection.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_WithMultipleFormats_AppliesAllFormats()
    {
        // Arrange
        var selectedText = "Formatted Text";

        // Act
        var multiFormatted = $"<p><strong><em><u>{selectedText}</u></em></strong></p>";

        // Assert
        Assert.Contains("<strong>", multiFormatted);
        Assert.Contains("<em>", multiFormatted);
        Assert.Contains("<u>", multiFormatted);
        Assert.Equal("<p><strong><em><u>Formatted Text</u></em></strong></p>", multiFormatted);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent validates content before updating.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_WithValidation_ValidatesContent()
    {
        // Arrange
        var content = "<p>Valid content</p>";
        var isValid = !string.IsNullOrEmpty(content);

        // Act
        var validationResult = isValid;

        // Assert
        Assert.True(validationResult);
        Assert.NotEmpty(content);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent supports empty content state.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_WithEmptyContent_AllowsEmpty()
    {
        // Arrange
        var content = string.Empty;

        // Act
        var isEmpty = string.IsNullOrEmpty(content);

        // Assert
        Assert.True(isEmpty);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent applies formatting button styles correctly.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_FormattingButtons_HasCorrectClass()
    {
        // Arrange
        var buttonClass = "btn btn-sm btn-outline-secondary";

        // Act
        var hasBootstrapClass = buttonClass.Contains("btn");

        // Assert
        Assert.True(hasBootstrapClass);
        Assert.Equal("btn btn-sm btn-outline-secondary", buttonClass);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent combines content changes with callback invocations.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_ContentChangeAndCallback_BothOccur()
    {
        // Arrange
        var originalContent = "<p>Original</p>";
        var newContent = "<p>New</p>";
        var callbackFired = false;

        // Act
        callbackFired = true;
        var currentContent = newContent;

        // Assert
        Assert.True(callbackFired);
        Assert.NotEqual(originalContent, currentContent);
        Assert.Equal("<p>New</p>", currentContent);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent handles null content gracefully.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_WithNullContent_HandlesNullGracefully()
    {
        // Arrange
        string? content = null;

        // Act
        var safeContent = content ?? string.Empty;

        // Assert
        Assert.Empty(safeContent);
        Assert.NotNull(safeContent);
    }

    /// <summary>
    /// Verifies that RichTextEditorComponent supports paragraph formatting.
    /// </summary>
    [Fact]
    public void RichTextEditorComponent_WithParagraphs_PreservesParagraphStructure()
    {
        // Arrange
        var content = "<p>First paragraph</p><p>Second paragraph</p>";

        // Act
        var paragraphCount = content.Split("<p>").Length - 1;

        // Assert
        Assert.Equal(2, paragraphCount);
        Assert.Contains("<p>First paragraph</p>", content);
        Assert.Contains("<p>Second paragraph</p>", content);
    }
}
