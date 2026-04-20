namespace SmartWorkz.Core.Web.Tests.Services;

using SmartWorkz.Core.Web.Services.DataView;
using Xunit;

public class ListViewFormatterTests
{
    private readonly IListViewFormatter _formatter;

    public ListViewFormatterTests()
    {
        _formatter = new ListViewFormatter();
    }

    [Fact]
    public void FormatDate_WithValidDate_ReturnsFormattedDate()
    {
        // Arrange
        var date = new DateTime(2026, 4, 20);
        var expected = "Apr 20, 2026";

        // Act
        var result = _formatter.FormatDate(date);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FormatDate_WithNullDate_ReturnsDash()
    {
        // Arrange
        DateTime? date = null;
        var expected = "-";

        // Act
        var result = _formatter.FormatDate(date);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FormatCurrency_WithValidValue_ReturnsFormattedCurrency()
    {
        // Arrange
        decimal? value = 1234.56m;
        var expected = "$1,234.56";

        // Act
        var result = _formatter.FormatCurrency(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FormatCurrency_WithNullValue_ReturnsDash()
    {
        // Arrange
        decimal? value = null;
        var expected = "-";

        // Act
        var result = _formatter.FormatCurrency(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TruncateText_WithLongText_TruncatesAndAddsEllipsis()
    {
        // Arrange
        var text = "This is a very long text that should be truncated because it exceeds the maximum length of 50 characters";
        var maxLength = 50;
        var expected = text.Substring(0, maxLength) + "...";

        // Act
        var result = _formatter.TruncateText(text, maxLength);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TruncateText_WithShortText_ReturnsOriginalText()
    {
        // Arrange
        var text = "Short text";
        var maxLength = 100;
        var expected = "Short text";

        // Act
        var result = _formatter.TruncateText(text, maxLength);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FormatBoolean_WithTrue_ReturnsYes()
    {
        // Arrange
        bool? value = true;
        var expected = "Yes";

        // Act
        var result = _formatter.FormatBoolean(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FormatBoolean_WithFalse_ReturnsNo()
    {
        // Arrange
        bool? value = false;
        var expected = "No";

        // Act
        var result = _formatter.FormatBoolean(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FormatValue_WithDateTime_ReturnsFormattedDate()
    {
        // Arrange
        object? value = new DateTime(2026, 4, 20);
        var expected = "Apr 20, 2026";

        // Act
        var result = _formatter.FormatValue(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FormatValue_WithNull_ReturnsDash()
    {
        // Arrange
        object? value = null;
        var expected = "-";

        // Act
        var result = _formatter.FormatValue(value);

        // Assert
        Assert.Equal(expected, result);
    }
}
