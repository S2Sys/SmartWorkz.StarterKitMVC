namespace SmartWorkz.Core.Shared.Utilities;

/// <summary>
/// Provides utilities for common math operations.
/// </summary>
public static class MathHelper
{
    /// <summary>
    /// Calculates the percentage of a value.
    /// Example: Percentage(100, 20) returns 20 (20% of 100).
    /// </summary>
    public static decimal Percentage(decimal value, decimal percent)
        => (value * percent) / 100;

    /// <summary>
    /// Calculates the percentage change from oldValue to newValue.
    /// Positive result indicates increase, negative indicates decrease.
    /// </summary>
    public static decimal PercentageChange(decimal oldValue, decimal newValue)
    {
        if (oldValue == 0)
            return newValue == 0 ? 0 : 100;

        return ((newValue - oldValue) / oldValue) * 100;
    }

    /// <summary>
    /// Rounds a decimal value to the specified number of decimal places.
    /// </summary>
    public static decimal RoundTo(decimal value, int decimals)
    {
        if (decimals < 0)
            throw new ArgumentException("Decimals cannot be negative", nameof(decimals));

        return decimal.Round(value, decimals, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Clamps a value within a specified range [min, max].
    /// </summary>
    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0)
            return min;
        if (value.CompareTo(max) > 0)
            return max;
        return value;
    }

    /// <summary>
    /// Calculates the average of the provided decimal values.
    /// </summary>
    public static decimal Average(params decimal[] values)
    {
        if (values.Length == 0)
            throw new ArgumentException("At least one value is required", nameof(values));

        return values.Sum() / values.Length;
    }
}
