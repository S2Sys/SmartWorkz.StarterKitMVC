namespace SmartWorkz.Core.Shared.Utilities;

/// <summary>
/// Provides utilities for date and time operations.
/// </summary>
public static class DateHelper
{
    /// <summary>
    /// Calculates the age in years from a birth date to today.
    /// </summary>
    public static Result<int> GetAge(DateTime birthDate)
    {
        try
        {
            if (birthDate > DateTime.UtcNow)
                return Result.Fail<int>("Error.InvalidBirthDate", "Birth date cannot be in the future");

            int age = DateTime.UtcNow.Year - birthDate.Year;
            if (birthDate.Date > DateTime.UtcNow.AddYears(-age))
                age--;

            if (age < 0)
                return Result.Fail<int>("Error.InvalidAge", "Age cannot be negative");

            return Result.Ok(age);
        }
        catch (Exception ex)
        {
            return Result.Fail<int>("Error.GetAge", ex.Message);
        }
    }

    /// <summary>
    /// Returns a human-readable relative time string (e.g., "2 days ago", "in 3 hours").
    /// </summary>
    public static Result<string> GetRelativeTime(DateTime date)
    {
        try
        {
            var now = DateTime.UtcNow;
            var span = now - date;

            if (span.TotalSeconds < 60)
                return Result.Ok("just now");

            if (span.TotalMinutes < 60)
            {
                int minutes = (int)span.TotalMinutes;
                return Result.Ok(minutes == 1 ? "1 minute ago" : $"{minutes} minutes ago");
            }

            if (span.TotalHours < 24)
            {
                int hours = (int)span.TotalHours;
                return Result.Ok(hours == 1 ? "1 hour ago" : $"{hours} hours ago");
            }

            if (span.TotalDays < 7)
            {
                int days = (int)span.TotalDays;
                return Result.Ok(days == 1 ? "1 day ago" : $"{days} days ago");
            }

            if (span.TotalDays < 30)
            {
                int weeks = (int)(span.TotalDays / 7);
                return Result.Ok(weeks == 1 ? "1 week ago" : $"{weeks} weeks ago");
            }

            if (span.TotalDays < 365)
            {
                int months = (int)(span.TotalDays / 30);
                return Result.Ok(months == 1 ? "1 month ago" : $"{months} months ago");
            }

            int years = (int)(span.TotalDays / 365);
            return Result.Ok(years == 1 ? "1 year ago" : $"{years} years ago");
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Error.GetRelativeTime", ex.Message);
        }
    }

    /// <summary>
    /// Returns the start of the day (00:00:00) for the given date.
    /// </summary>
    public static DateTime StartOfDay(DateTime date)
        => date.Date;

    /// <summary>
    /// Returns the end of the day (23:59:59.999) for the given date.
    /// </summary>
    public static DateTime EndOfDay(DateTime date)
        => date.Date.AddDays(1).AddTicks(-1);

    /// <summary>
    /// Determines if the given date falls on a weekend (Saturday or Sunday).
    /// </summary>
    public static bool IsWeekend(DateTime date)
        => date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    /// <summary>
    /// Returns the name of the day of week (e.g., "Monday", "Tuesday").
    /// </summary>
    public static string GetDayOfWeekName(DateTime date)
        => date.DayOfWeek.ToString();

    /// <summary>
    /// Calculates the number of days between two dates (inclusive of the from date, exclusive of the to date).
    /// </summary>
    public static int DaysBetween(DateTime from, DateTime to)
        => (int)(to.Date - from.Date).TotalDays;
}
