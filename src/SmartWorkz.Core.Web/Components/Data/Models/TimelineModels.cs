namespace SmartWorkz.Core.Web.Components.Data.Models;

/// <summary>Event item for TimelineComponent.</summary>
public class TimelineEvent
{
    /// <summary>Unique identifier.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Event title or heading.</summary>
    public required string Title { get; set; }

    /// <summary>Event description or body text.</summary>
    public string? Description { get; set; }

    /// <summary>Timestamp for the event.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Icon CSS class (e.g., "fa-check", "fa-clock").</summary>
    public string? IconClass { get; set; }

    /// <summary>Icon background color (success, warning, danger, info, primary).</summary>
    public string IconColor { get; set; } = "primary";

    /// <summary>Custom CSS class for styling.</summary>
    public string? CssClass { get; set; }

    /// <summary>Additional data/metadata attached to event.</summary>
    public object? Data { get; set; }

    /// <summary>Badge text (e.g., "New", "Important").</summary>
    public string? BadgeText { get; set; }

    /// <summary>User/actor who triggered the event.</summary>
    public string? Actor { get; set; }

    /// <summary>Avatar URL for the actor.</summary>
    public string? AvatarUrl { get; set; }
}

/// <summary>Configuration for TimelineComponent.</summary>
public class TimelineOptions
{
    /// <summary>Layout direction (Vertical or Horizontal).</summary>
    public TimelineLayout Layout { get; set; } = TimelineLayout.Vertical;

    /// <summary>Position of timeline bar (Left, Right, or Center).</summary>
    public TimelinePosition Position { get; set; } = TimelinePosition.Left;

    /// <summary>Enable connections between events.</summary>
    public bool ShowConnections { get; set; } = true;

    /// <summary>Size of timeline indicators (Small, Medium, Large).</summary>
    public TimelineSize IndicatorSize { get; set; } = TimelineSize.Medium;

    /// <summary>Format string for displaying timestamps.</summary>
    public string DateFormat { get; set; } = "MMM dd, yyyy";

    /// <summary>Include time in timestamp (not just date).</summary>
    public bool IncludeTime { get; set; } = false;

    /// <summary>Reverse chronological order (newest first).</summary>
    public bool ReverseOrder { get; set; } = false;

    /// <summary>Show avatars for events.</summary>
    public bool ShowAvatars { get; set; } = false;

    /// <summary>Animate on scroll into view.</summary>
    public bool AnimateOnScroll { get; set; } = true;
}

/// <summary>Timeline layout direction.</summary>
public enum TimelineLayout
{
    /// <summary>Events stacked vertically.</summary>
    Vertical = 0,

    /// <summary>Events arranged horizontally.</summary>
    Horizontal = 1
}

/// <summary>Timeline position relative to content.</summary>
public enum TimelinePosition
{
    /// <summary>Timeline bar on the left.</summary>
    Left = 0,

    /// <summary>Timeline bar in the center (events alternate sides).</summary>
    Center = 1,

    /// <summary>Timeline bar on the right.</summary>
    Right = 2
}

/// <summary>Size of timeline event indicators.</summary>
public enum TimelineSize
{
    /// <summary>Small indicators (24px).</summary>
    Small = 0,

    /// <summary>Medium indicators (40px).</summary>
    Medium = 1,

    /// <summary>Large indicators (56px).</summary>
    Large = 2
}
