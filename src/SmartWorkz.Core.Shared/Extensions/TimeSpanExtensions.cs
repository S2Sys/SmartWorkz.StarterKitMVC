namespace SmartWorkz.Core.Shared.Extensions;

public static class TimeSpanExtensions
{
    public static bool IsZero(this TimeSpan value) => value == TimeSpan.Zero;
    public static bool IsPositive(this TimeSpan value) => value > TimeSpan.Zero;
    public static bool IsNegative(this TimeSpan value) => value < TimeSpan.Zero;
    public static int TotalDays(this TimeSpan value) => (int)value.TotalDays;
    public static int TotalHours(this TimeSpan value) => (int)value.TotalHours;
    public static int TotalMinutes(this TimeSpan value) => (int)value.TotalMinutes;
    public static int TotalSeconds(this TimeSpan value) => (int)value.TotalSeconds;
    public static DateTime FromNow(this TimeSpan value) => DateTime.UtcNow.Add(value);
    public static DateTime FromNow(this TimeSpan value, DateTime baseTime) => baseTime.Add(value);
    public static DateTime Ago(this TimeSpan value) => DateTime.UtcNow.Subtract(value);
    public static DateTime Ago(this TimeSpan value, DateTime baseTime) => baseTime.Subtract(value);
}
