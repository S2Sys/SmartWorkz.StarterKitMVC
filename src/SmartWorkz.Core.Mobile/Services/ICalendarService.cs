namespace SmartWorkz.Mobile;

/// <summary>
/// Service for managing calendar events and reminders on mobile devices.
/// Handles event creation, modification, deletion, and access to device calendars.
/// </summary>
public interface ICalendarService
{
    /// <summary>
    /// Creates a new calendar event.
    /// </summary>
    /// <param name="title">Event title</param>
    /// <param name="description">Event description</param>
    /// <param name="startTime">Event start time</param>
    /// <param name="endTime">Event end time</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Event ID</returns>
    Task<string> CreateEventAsync(string title, string description, DateTime startTime, DateTime endTime, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing calendar event.
    /// </summary>
    /// <param name="eventId">ID of event to update</param>
    /// <param name="title">Updated event title</param>
    /// <param name="description">Updated event description</param>
    /// <param name="startTime">Updated event start time</param>
    /// <param name="endTime">Updated event end time</param>
    /// <param name="ct">Cancellation token</param>
    Task UpdateEventAsync(string eventId, string title, string description, DateTime startTime, DateTime endTime, CancellationToken ct = default);

    /// <summary>
    /// Deletes a calendar event.
    /// </summary>
    /// <param name="eventId">ID of event to delete</param>
    /// <param name="ct">Cancellation token</param>
    Task DeleteEventAsync(string eventId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves events within a specified date range.
    /// </summary>
    /// <param name="startDate">Start of date range</param>
    /// <param name="endDate">End of date range</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of calendar events</returns>
    Task<IEnumerable<CalendarEvent>> GetEventsAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

    /// <summary>
    /// Gets today's events.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of today's events</returns>
    Task<IEnumerable<CalendarEvent>> GetTodayEventsAsync(CancellationToken ct = default);

    /// <summary>
    /// Sets a reminder for an event.
    /// </summary>
    /// <param name="eventId">ID of event to set reminder for</param>
    /// <param name="minutesBefore">Minutes before event to trigger reminder</param>
    /// <param name="ct">Cancellation token</param>
    Task SetReminderAsync(string eventId, int minutesBefore, CancellationToken ct = default);

    /// <summary>
    /// Checks if calendar access is available and permitted.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if calendar is accessible</returns>
    Task<bool> IsCalendarAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Requests calendar access permission from the user.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if user granted permission</returns>
    Task<bool> RequestCalendarPermissionAsync(CancellationToken ct = default);
}

/// <summary>
/// Represents a calendar event.
/// </summary>
public class CalendarEvent
{
    /// <summary>Gets or sets the unique identifier of the event.</summary>
    public string? Id { get; set; }

    /// <summary>Gets or sets the event title.</summary>
    public string? Title { get; set; }

    /// <summary>Gets or sets the event description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the event start time.</summary>
    public DateTime StartTime { get; set; }

    /// <summary>Gets or sets the event end time.</summary>
    public DateTime EndTime { get; set; }

    /// <summary>Gets or sets the event location.</summary>
    public string? Location { get; set; }

    /// <summary>Gets or sets whether this is an all-day event.</summary>
    public bool IsAllDay { get; set; }
}
