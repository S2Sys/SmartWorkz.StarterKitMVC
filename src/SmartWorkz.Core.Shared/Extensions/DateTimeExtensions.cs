namespace SmartWorkz.Core.Shared.Extensions;

public static class DateTimeExtensions
{
    public static DateTime ToUtcKind(this DateTime dateTime)
        => dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();

    public static bool IsBetween(this DateTime dateTime, DateTime start, DateTime end)
        => dateTime >= start && dateTime <= end;
}
