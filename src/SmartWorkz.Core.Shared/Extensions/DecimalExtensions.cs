namespace SmartWorkz.Shared;

public static class DecimalExtensions
{
    public static bool IsPositive(this decimal value) => value > 0;
    public static bool IsNegative(this decimal value) => value < 0;
    public static decimal Abs(this decimal value) => Math.Abs(value);
    public static decimal Round(this decimal value, int decimals = 2) => Math.Round(value, decimals);
    public static decimal Clamp(this decimal value, decimal min, decimal max)
        => value < min ? min : value > max ? max : value;
    public static bool IsBetween(this decimal value, decimal min, decimal max)
        => value >= min && value <= max;
    public static bool IsZero(this decimal value) => value == 0m;
}
