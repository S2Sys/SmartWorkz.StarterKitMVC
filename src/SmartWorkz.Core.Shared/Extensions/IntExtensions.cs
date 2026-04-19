namespace SmartWorkz.Core.Shared.Extensions;

public static class IntExtensions
{
    public static bool IsPositive(this int value) => value > 0;
    public static bool IsNegative(this int value) => value < 0;
    public static bool IsEven(this int value) => value % 2 == 0;
    public static bool IsOdd(this int value) => value % 2 != 0;
    public static int Abs(this int value) => Math.Abs(value);
    public static int Clamp(this int value, int min, int max) => Math.Max(min, Math.Min(max, value));
    public static int Square(this int value) => value * value;
    public static bool IsBetween(this int value, int min, int max) => value >= min && value <= max;
}
