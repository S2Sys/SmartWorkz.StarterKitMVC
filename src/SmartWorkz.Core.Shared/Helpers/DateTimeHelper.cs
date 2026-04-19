namespace SmartWorkz.Core.Shared.Helpers;

public static class DateTimeHelper
{
    public static DateTime StartOfDay(this DateTime date) => date.Date;

    public static DateTime EndOfDay(this DateTime date) => date.Date.AddDays(1).AddTicks(-1);

    public static DateTime StartOfWeek(this DateTime date) => date.AddDays(-(int)date.DayOfWeek);

    public static DateTime EndOfWeek(this DateTime date) => date.StartOfWeek().AddDays(6).EndOfDay();

    public static DateTime StartOfMonth(this DateTime date) => new DateTime(date.Year, date.Month, 1);

    public static DateTime EndOfMonth(this DateTime date) => date.StartOfMonth().AddMonths(1).AddDays(-1).EndOfDay();

    public static DateTime StartOfYear(this DateTime date) => new DateTime(date.Year, 1, 1);

    public static DateTime EndOfYear(this DateTime date) => new DateTime(date.Year, 12, 31, 23, 59, 59).EndOfDay();

    public static bool IsToday(this DateTime date) => date.Date == DateTime.Today;

    public static bool IsYesterday(this DateTime date) => date.Date == DateTime.Today.AddDays(-1);

    public static bool IsTomorrow(this DateTime date) => date.Date == DateTime.Today.AddDays(1);

    public static bool IsThisWeek(this DateTime date) => date >= DateTime.Today.StartOfWeek() && date <= DateTime.Today.EndOfWeek();

    public static bool IsThisMonth(this DateTime date) => date.Year == DateTime.Today.Year && date.Month == DateTime.Today.Month;

    public static bool IsThisYear(this DateTime date) => date.Year == DateTime.Today.Year;

    public static string ToRelativeTime(this DateTime date)
    {
        var now = DateTime.UtcNow;
        var diff = now - date;
        return diff.TotalSeconds switch
        {
            < 60 => "just now",
            < 120 => "a minute ago",
            < 3600 => $"{(int)diff.TotalMinutes} minutes ago",
            < 7200 => "an hour ago",
            < 86400 => $"{(int)diff.TotalHours} hours ago",
            < 172800 => "yesterday",
            < 2592000 => $"{(int)diff.TotalDays} days ago",
            < 7776000 => $"{(int)(diff.TotalDays / 30)} months ago",
            _ => $"{(int)(diff.TotalDays / 365)} years ago"
        };
    }
}
