namespace SmartWorkz.StarterKitMVC.Shared.Extensions;

/// <summary>
/// Extension methods for <see cref="DateTime"/> manipulation.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts the DateTime to UTC kind without changing the actual time value.
    /// </summary>
    /// <param name="value">The DateTime to convert.</param>
    /// <returns>DateTime with UTC kind specified.</returns>
    /// <example>
    /// <code>
    /// var localTime = DateTime.Now;
    /// var utcTime = localTime.ToUtcKind();
    /// Console.WriteLine(utcTime.Kind); // Utc
    /// </code>
    /// </example>
    public static DateTime ToUtcKind(this DateTime value) =>
        DateTime.SpecifyKind(value, DateTimeKind.Utc);

    /// <summary>
    /// Checks if the DateTime falls between two dates (inclusive).
    /// </summary>
    /// <param name="value">The DateTime to check.</param>
    /// <param name="from">Start of the range.</param>
    /// <param name="to">End of the range.</param>
    /// <returns>True if within range; otherwise false.</returns>
    /// <example>
    /// <code>
    /// var date = new DateTime(2024, 6, 15);
    /// var start = new DateTime(2024, 1, 1);
    /// var end = new DateTime(2024, 12, 31);
    /// 
    /// if (date.IsBetween(start, end))
    ///     Console.WriteLine("Date is in 2024!");
    /// </code>
    /// </example>
    public static bool IsBetween(this DateTime value, DateTime from, DateTime to) =>
        value >= from && value <= to;
}
