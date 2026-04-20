namespace SmartWorkz.Core.Web.Components.Data.Models;

/// <summary>Individual accordion item configuration.</summary>
public class AccordionItem
{
    /// <summary>Unique item identifier.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Header/title text.</summary>
    public required string Title { get; set; }

    /// <summary>Icon CSS class for header (e.g., "fa-info-circle").</summary>
    public string? IconClass { get; set; }

    /// <summary>Initially expanded state.</summary>
    public bool IsExpanded { get; set; } = false;

    /// <summary>Disable this accordion item.</summary>
    public bool IsDisabled { get; set; } = false;

    /// <summary>Show loading state while content loads.</summary>
    public bool IsLoading { get; set; } = false;

    /// <summary>Custom CSS class for styling.</summary>
    public string? CssClass { get; set; }

    /// <summary>Accordion content.</summary>
    public RenderFragment? Content { get; set; }

    /// <summary>Custom header template (overrides default).</summary>
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>Lazy load content only when expanded.</summary>
    public bool LazyLoad { get; set; } = false;

    /// <summary>Content loading state.</summary>
    public bool ContentLoaded { get; set; } = false;
}

/// <summary>Configuration for AccordionComponent.</summary>
public class AccordionOptions
{
    /// <summary>Allow multiple items expanded simultaneously.</summary>
    public bool AllowMultiple { get; set; } = false;

    /// <summary>Auto-collapse siblings when opening (only if AllowMultiple is false).</summary>
    public bool AutoCollapseSiblings { get; set; } = true;

    /// <summary>Animate expand/collapse transitions.</summary>
    public bool AnimateTransitions { get; set; } = true;

    /// <summary>Animation duration in milliseconds.</summary>
    public int AnimationDuration { get; set; } = 300;

    /// <summary>Show expand/collapse icons.</summary>
    public bool ShowToggleIcon { get; set; } = true;

    /// <summary>Icon to show when collapsed.</summary>
    public string CollapsedIcon { get; set; } = "fa-chevron-right";

    /// <summary>Icon to show when expanded.</summary>
    public string ExpandedIcon { get; set; } = "fa-chevron-down";

    /// <summary>Custom CSS class for accordion container.</summary>
    public string? CssClass { get; set; }

    /// <summary>Flush mode (no borders, seamless integration).</summary>
    public bool IsFlush { get; set; } = false;

    /// <summary>Make accordion background darker.</summary>
    public bool HasDarkBackground { get; set; } = false;

    /// <summary>Close button to remove item dynamically.</summary>
    public bool AllowRemoving { get; set; } = false;
}

/// <summary>Event args for accordion item expansion.</summary>
public class AccordionExpandedEventArgs
{
    public required string ItemId { get; set; }
    public required AccordionItem Item { get; set; }
    public DateTime ExpandedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Event args for accordion item collapse.</summary>
public class AccordionCollapsedEventArgs
{
    public required string ItemId { get; set; }
    public required AccordionItem Item { get; set; }
    public DateTime CollapsedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Event args for accordion item removal.</summary>
public class AccordionItemRemovedEventArgs
{
    public required string ItemId { get; set; }
    public required AccordionItem Item { get; set; }
    public bool Cancel { get; set; } = false;
}
