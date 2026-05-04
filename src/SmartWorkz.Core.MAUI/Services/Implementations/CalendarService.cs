namespace SmartWorkz.Mobile;

using ILogger = Microsoft.Extensions.Logging.ILogger;

#if !WINDOWS
public partial class CalendarService : ICalendarService
#else
public class CalendarService : ICalendarService
#endif
{
    private readonly ILogger _logger;

    public CalendarService(ILogger logger)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public async Task<string> CreateEventAsync(string title, string description, DateTime startTime, DateTime endTime, CancellationToken ct = default)
    {
        Guard.NotEmpty(title, nameof(title));
        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time");
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        return string.Empty;
        #else
        try
        {
            return await CreateEventAsyncPlatform(title, description, startTime, endTime, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create event");
            return string.Empty;
        }
        #endif
    }

    public async Task UpdateEventAsync(string eventId, string title, string description, DateTime startTime, DateTime endTime, CancellationToken ct = default)
    {
        Guard.NotEmpty(eventId, nameof(eventId));
        Guard.NotEmpty(title, nameof(title));
        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time");
        ct.ThrowIfCancellationRequested();

        #if !WINDOWS
        try
        {
            await UpdateEventAsyncPlatform(eventId, title, description, startTime, endTime, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update event");
        }
        #endif
    }

    public async Task DeleteEventAsync(string eventId, CancellationToken ct = default)
    {
        Guard.NotEmpty(eventId, nameof(eventId));
        ct.ThrowIfCancellationRequested();

        #if !WINDOWS
        try
        {
            await DeleteEventAsyncPlatform(eventId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete event");
        }
        #endif
    }

    public async Task<IEnumerable<CalendarEvent>> GetEventsAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date must be after start date");
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        return Enumerable.Empty<CalendarEvent>();
        #else
        try
        {
            return await GetEventsAsyncPlatform(startDate, endDate, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get events");
            return Enumerable.Empty<CalendarEvent>();
        }
        #endif
    }

    public async Task<IEnumerable<CalendarEvent>> GetTodayEventsAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var today = DateTime.Today;
        return await GetEventsAsync(today, today.AddDays(1), ct);
    }

    public async Task SetReminderAsync(string eventId, int minutesBefore, CancellationToken ct = default)
    {
        Guard.NotEmpty(eventId, nameof(eventId));
        if (minutesBefore < 0)
            throw new ArgumentException("Minutes before cannot be negative");
        ct.ThrowIfCancellationRequested();

        #if !WINDOWS
        try
        {
            await SetReminderAsyncPlatform(eventId, minutesBefore, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set reminder");
        }
        #endif
    }

    public async Task<bool> IsCalendarAvailableAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        return false;
        #else
        try
        {
            return await IsCalendarAvailableAsyncPlatform(ct);
        }
        catch
        {
            return false;
        }
        #endif
    }

    public async Task<bool> RequestCalendarPermissionAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        return false;
        #else
        try
        {
            return await RequestCalendarPermissionAsyncPlatform(ct);
        }
        catch
        {
            return false;
        }
        #endif
    }

    #if !WINDOWS
    private partial Task<string> CreateEventAsyncPlatform(string title, string description, DateTime startTime, DateTime endTime, CancellationToken ct);
    private partial Task UpdateEventAsyncPlatform(string eventId, string title, string description, DateTime startTime, DateTime endTime, CancellationToken ct);
    private partial Task DeleteEventAsyncPlatform(string eventId, CancellationToken ct);
    private partial Task<IEnumerable<CalendarEvent>> GetEventsAsyncPlatform(DateTime startDate, DateTime endDate, CancellationToken ct);
    private partial Task SetReminderAsyncPlatform(string eventId, int minutesBefore, CancellationToken ct);
    private partial Task<bool> IsCalendarAvailableAsyncPlatform(CancellationToken ct);
    private partial Task<bool> RequestCalendarPermissionAsyncPlatform(CancellationToken ct);
    #endif
}
