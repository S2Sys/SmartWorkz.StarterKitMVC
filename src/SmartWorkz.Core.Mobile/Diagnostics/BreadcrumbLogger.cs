namespace SmartWorkz.Mobile.Diagnostics;

/// <summary>
/// Represents a single breadcrumb in the event trail.
/// </summary>
public class Breadcrumb
{
    /// <summary>
    /// Gets the message describing the breadcrumb event.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets the category of the breadcrumb (e.g., "ui.click", "navigation", "network").
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets the severity level of the breadcrumb (debug, info, warning, error).
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Gets the timestamp when the breadcrumb was recorded.
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Provides thread-safe breadcrumb logging for tracking event trails leading up to crashes.
/// </summary>
public class BreadcrumbLogger
{
    private readonly List<Breadcrumb> _breadcrumbs = new();
    private readonly int _maxBreadcrumbs;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BreadcrumbLogger"/> class.
    /// </summary>
    /// <param name="maxBreadcrumbs">The maximum number of breadcrumbs to retain (default 50).</param>
    public BreadcrumbLogger(int maxBreadcrumbs = 50)
    {
        if (maxBreadcrumbs <= 0)
        {
            throw new ArgumentException("Maximum breadcrumbs must be greater than zero.", nameof(maxBreadcrumbs));
        }
        _maxBreadcrumbs = maxBreadcrumbs;
    }

    /// <summary>
    /// Adds a breadcrumb to the event trail in a thread-safe manner.
    /// </summary>
    /// <param name="message">The breadcrumb message describing the event.</param>
    /// <param name="category">The category of the breadcrumb (e.g., "ui.click", "navigation", "network").</param>
    /// <param name="level">The severity level of the breadcrumb (debug, info, warning, error).</param>
    public void AddBreadcrumb(string message, string category, string level)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));
        ArgumentException.ThrowIfNullOrWhiteSpace(category, nameof(category));
        ArgumentException.ThrowIfNullOrWhiteSpace(level, nameof(level));

        lock (_lock)
        {
            var breadcrumb = new Breadcrumb
            {
                Message = message,
                Category = category,
                Level = level,
                Timestamp = DateTime.UtcNow
            };

            _breadcrumbs.Add(breadcrumb);

            // Remove oldest breadcrumb if we exceed maximum
            if (_breadcrumbs.Count > _maxBreadcrumbs)
            {
                _breadcrumbs.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Gets a snapshot of all current breadcrumbs.
    /// </summary>
    /// <returns>A read-only list of all breadcrumbs in the current trail.</returns>
    public IReadOnlyList<Breadcrumb> GetBreadcrumbs()
    {
        lock (_lock)
        {
            return new List<Breadcrumb>(_breadcrumbs).AsReadOnly();
        }
    }

    /// <summary>
    /// Gets the number of breadcrumbs currently in the trail.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _breadcrumbs.Count;
            }
        }
    }

    /// <summary>
    /// Clears all breadcrumbs from the event trail.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _breadcrumbs.Clear();
        }
    }
}
