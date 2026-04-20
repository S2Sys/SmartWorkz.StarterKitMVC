using Xunit;
using SmartWorkz.Core.Web.Services.DataView;

namespace SmartWorkz.Core.Web.Tests.Services;

public class ListViewFormatterTests
{
    private readonly ListViewFormatter _formatter = new();

    [Fact]
    public void FormatDate_WithValidDate_ReturnsFormattedString()
    {
        // Arrange
        var date = new DateTime(2026, 4, 20);

        // Act
        var result = _formatter.FormatDate(date);

        // Assert
        Assert.Equal("Apr 20, 2026", result);
    }

    [Fact]
    public void FormatDate_WithNullDate_ReturnsDash()
    {
        // Act
        var result = _formatter.FormatDate(null);

        // Assert
        Assert.Equal("-", result);
    }

    [Fact]
    public void FormatCurrency_WithValidValue_ReturnsFormattedString()
    {
        // Act
        var result = _formatter.FormatCurrency(99.99m);

        // Assert
        Assert.Equal("$99.99", result);
    }

    [Fact]
    public void FormatCurrency_WithNullValue_ReturnsDash()
    {
        // Act
        var result = _formatter.FormatCurrency(null);

        // Assert
        Assert.Equal("-", result);
    }

    [Fact]
    public void TruncateText_WithLongText_TruncatesWithEllipsis()
    {
        // Arrange
        var longText = "This is a very long text that should be truncated";

        // Act
        var result = _formatter.TruncateText(longText, 20);

        // Assert
        Assert.Equal("This is a very long ...", result);
    }

    [Fact]
    public void TruncateText_WithShortText_ReturnsUnchanged()
    {
        // Arrange
        var shortText = "Short";

        // Act
        var result = _formatter.TruncateText(shortText, 20);

        // Assert
        Assert.Equal("Short", result);
    }

    [Fact]
    public void FormatBoolean_WithTrue_ReturnsYes()
    {
        // Act
        var result = _formatter.FormatBoolean(true);

        // Assert
        Assert.Equal("Yes", result);
    }

    [Fact]
    public void FormatBoolean_WithFalse_ReturnsNo()
    {
        // Act
        var result = _formatter.FormatBoolean(false);

        // Assert
        Assert.Equal("No", result);
    }

    [Fact]
    public void FormatValue_WithDateTime_UsesDateFormatter()
    {
        // Arrange
        var date = new DateTime(2026, 4, 20);

        // Act
        var result = _formatter.FormatValue(date);

        // Assert
        Assert.Equal("Apr 20, 2026", result);
    }

    [Fact]
    public void FormatValue_WithDecimal_UsesCurrencyFormatter()
    {
        // Act
        var result = _formatter.FormatValue(49.50m);

        // Assert
        Assert.Equal("$49.50", result);
    }

    [Fact]
    public void FormatValue_WithNull_ReturnsDash()
    {
        // Act
        var result = _formatter.FormatValue(null);

        // Assert
        Assert.Equal("-", result);
    }
}
