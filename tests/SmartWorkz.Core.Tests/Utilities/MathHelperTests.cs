namespace SmartWorkz.Core.Tests.Utilities;

using SmartWorkz.Shared.Utilities;

public class MathHelperTests
{
    #region Percentage Tests

    [Fact]
    public void Percentage_WithBasicValues_ReturnsCorrectPercentage()
    {
        // Act
        var result = MathHelper.Percentage(100, 20);

        // Assert
        Assert.Equal(20, result);
    }

    [Fact]
    public void Percentage_With50Percent_ReturnsHalf()
    {
        // Act
        var result = MathHelper.Percentage(200, 50);

        // Assert
        Assert.Equal(100, result);
    }

    [Fact]
    public void Percentage_WithDecimalValues_ReturnsCorrect()
    {
        // Act
        var result = MathHelper.Percentage(99.5m, 10);

        // Assert
        Assert.Equal(9.95m, result);
    }

    [Fact]
    public void Percentage_WithZeroValue_ReturnsZero()
    {
        // Act
        var result = MathHelper.Percentage(0, 50);

        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region PercentageChange Tests

    [Fact]
    public void PercentageChange_WithIncrease_ReturnsPositive()
    {
        // Act
        var result = MathHelper.PercentageChange(100, 120);

        // Assert
        Assert.Equal(20, result);
    }

    [Fact]
    public void PercentageChange_WithDecrease_ReturnsNegative()
    {
        // Act
        var result = MathHelper.PercentageChange(100, 80);

        // Assert
        Assert.Equal(-20, result);
    }

    [Fact]
    public void PercentageChange_WithNoChange_ReturnsZero()
    {
        // Act
        var result = MathHelper.PercentageChange(100, 100);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void PercentageChange_WithZeroOldValue_ReturnsHundred()
    {
        // Act
        var result = MathHelper.PercentageChange(0, 50);

        // Assert
        Assert.Equal(100, result);
    }

    [Fact]
    public void PercentageChange_WithZeroToZero_ReturnsZero()
    {
        // Act
        var result = MathHelper.PercentageChange(0, 0);

        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region RoundTo Tests

    [Fact]
    public void RoundTo_WithTwoDecimals_RoundsCorrectly()
    {
        // Act
        var result = MathHelper.RoundTo(3.14159m, 2);

        // Assert
        Assert.Equal(3.14m, result);
    }

    [Fact]
    public void RoundTo_WithZeroDecimals_RoundsToInteger()
    {
        // Act
        var result = MathHelper.RoundTo(3.7m, 0);

        // Assert
        Assert.Equal(4, result);
    }

    [Fact]
    public void RoundTo_WithHigherDecimals_PreservesAllDecimal()
    {
        // Act
        var result = MathHelper.RoundTo(3.14159m, 5);

        // Assert
        Assert.Equal(3.14159m, result);
    }

    [Fact]
    public void RoundTo_WithNegativeDecimals_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => MathHelper.RoundTo(3.14m, -1));
    }

    #endregion

    #region Clamp Tests

    [Fact]
    public void Clamp_WithValueInRange_ReturnsValue()
    {
        // Act
        var result = MathHelper.Clamp(5, 0, 10);

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void Clamp_WithValueBelowMin_ReturnsMin()
    {
        // Act
        var result = MathHelper.Clamp(-5, 0, 10);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Clamp_WithValueAboveMax_ReturnsMax()
    {
        // Act
        var result = MathHelper.Clamp(15, 0, 10);

        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public void Clamp_WithDecimalValues_ClampCorrectly()
    {
        // Act
        var result = MathHelper.Clamp(5.5m, 0m, 10m);

        // Assert
        Assert.Equal(5.5m, result);
    }

    [Fact]
    public void Clamp_WithStringValues_ClampCorrectly()
    {
        // Act
        var result = MathHelper.Clamp("b", "a", "c");

        // Assert
        Assert.Equal("b", result);
    }

    #endregion

    #region Average Tests

    [Fact]
    public void Average_WithMultipleValues_ReturnsCorrectAverage()
    {
        // Act
        var result = MathHelper.Average(10, 20, 30);

        // Assert
        Assert.Equal(20, result);
    }

    [Fact]
    public void Average_WithSingleValue_ReturnsThatValue()
    {
        // Act
        var result = MathHelper.Average(42);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Average_WithDecimalValues_ReturnsCorrect()
    {
        // Act
        var result = MathHelper.Average(10.5m, 20.5m, 30m);

        // Assert
        Assert.Equal(20.333333333333333333333333333m, result);
    }

    [Fact]
    public void Average_WithEmptyArray_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => MathHelper.Average());
    }

    #endregion
}

