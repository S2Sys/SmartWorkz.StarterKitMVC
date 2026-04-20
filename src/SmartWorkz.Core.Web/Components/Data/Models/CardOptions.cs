namespace SmartWorkz.Core.Web.Components.Data.Models;

/// <summary>Configuration options for CardComponent.</summary>
public class CardOptions
{
    /// <summary>Card title/header text.</summary>
    public string? Title { get; set; }

    /// <summary>Card subtitle (shown under title).</summary>
    public string? Subtitle { get; set; }

    /// <summary>URL to header image.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Icon to display in header (e.g., "fa-star").</summary>
    public string? IconClass { get; set; }

    /// <summary>Card elevation/shadow level (0-3).</summary>
    public int ElevationLevel { get; set; } = 1;

    /// <summary>Card CSS classes for customization.</summary>
    public string? CssClass { get; set; }

    /// <summary>Enable click/hover effects.</summary>
    public bool IsClickable { get; set; } = false;

    /// <summary>Show loading skeleton while content loads.</summary>
    public bool IsLoading { get; set; } = false;

    /// <summary>Badge text (e.g., "New", "Featured").</summary>
    public string? BadgeText { get; set; }

    /// <summary>Badge color (success, warning, danger, info).</summary>
    public string BadgeColor { get; set; } = "success";
}

/// <summary>Statistics card data for DashboardComponent.</summary>
public class StatCardData
{
    /// <summary>Metric label (e.g., "Total Users").</summary>
    public required string Label { get; set; }

    /// <summary>Current value to display.</summary>
    public required string Value { get; set; }

    /// <summary>Trend percentage (positive or negative).</summary>
    public decimal? Trend { get; set; }

    /// <summary>True if trend is upward (green), false if downward (red).</summary>
    public bool TrendUp { get; set; } = true;

    /// <summary>Icon to display (e.g., "fa-users").</summary>
    public string? Icon { get; set; }

    /// <summary>Card background color (primary, success, danger, warning, info).</summary>
    public string Color { get; set; } = "primary";

    /// <summary>Custom CSS for stat value formatting.</summary>
    public string? ValueClass { get; set; } = "h3 fw-bold";
}
