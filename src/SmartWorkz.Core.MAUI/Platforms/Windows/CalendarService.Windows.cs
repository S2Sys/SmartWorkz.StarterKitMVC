namespace SmartWorkz.Mobile;

#if WINDOWS

public partial class CalendarService
{
    private partial async Task<string> CreateEventAsyncPlatform(string title, string description, DateTime startTime, DateTime endTime, CancellationToken ct) => string.Empty;
    private partial async Task UpdateEventAsyncPlatform(string eventId, string title, string description, DateTime startTime, DateTime endTime, CancellationToken ct) { }
    private partial async Task DeleteEventAsyncPlatform(string eventId, CancellationToken ct) { }
    private partial async Task<IEnumerable<CalendarEvent>> GetEventsAsyncPlatform(DateTime startDate, DateTime endDate, CancellationToken ct) => Enumerable.Empty<CalendarEvent>();
    private partial async Task SetReminderAsyncPlatform(string eventId, int minutesBefore, CancellationToken ct) { }
    private partial async Task<bool> IsCalendarAvailableAsyncPlatform(CancellationToken ct) => false;
    private partial async Task<bool> RequestCalendarPermissionAsyncPlatform(CancellationToken ct) => false;
}

#endif
