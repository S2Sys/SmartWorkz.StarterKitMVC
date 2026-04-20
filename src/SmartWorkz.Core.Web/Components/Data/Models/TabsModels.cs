namespace SmartWorkz.Core.Web.Components.Data.Models;

/// <summary>Individual tab configuration.</summary>
public class TabItem
{
    /// <summary>Unique tab identifier.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Display title for the tab.</summary>
    public required string Title { get; set; }

    /// <summary>Icon CSS class (e.g., "fa-home", "fa-settings").</summary>
    public string? IconClass { get; set; }

    /// <summary>Badge/counter text (e.g., "3", "New").</summary>
    public string? BadgeText { get; set; }

    /// <summary>Badge color (success, warning, danger, info, primary).</summary>
    public string BadgeColor { get; set; } = "primary";

    /// <summary>Disable this tab from being selected.</summary>
    public bool IsDisabled { get; set; } = false;

    /// <summary>Show loading state in tab content.</summary>
    public bool IsLoading { get; set; } = false;

    /// <summary>Custom CSS for tab styling.</summary>
    public string? CssClass { get; set; }

    /// <summary>Tab content.</summary>
    public RenderFragment? Content { get; set; }

    /// <summary>Lazy load content only when tab is activated.</summary>
    public bool LazyLoad { get; set; } = false;

    /// <summary>Content loading state.</summary>
    public bool ContentLoaded { get; set; } = false;
}

/// <summary>Configuration for TabsComponent.</summary>
public class TabsOptions
{
    /// <summary>Tab layout direction (Horizontal or Vertical).</summary>
    public TabLayout Layout { get; set; } = TabLayout.Horizontal;

    /// <summary>Visual style (Tabs, Pills, Underline).</summary>
    public TabStyle Style { get; set; } = TabStyle.Tabs;

    /// <summary>Activate tab on click or hover.</summary>
    public TabActivation ActivateOn { get; set; } = TabActivation.Click;

    /// <summary>Allow closing/removing tabs dynamically.</summary>
    public bool AllowClosing { get; set; } = false;

    /// <summary>Show tab icons.</summary>
    public bool ShowIcons { get; set; } = true;

    /// <summary>Show badges on tabs.</summary>
    public bool ShowBadges { get; set; } = true;

    /// <summary>Animate tab transitions.</summary>
    public bool AnimateTransitions { get; set; } = true;

    /// <summary>Custom CSS for tab container.</summary>
    public string? CssClass { get; set; }

    /// <summary>Fill available width (responsive).</summary>
    public bool FillWidth { get; set; } = false;

    /// <summary>Show as mobile-friendly dropdown on small screens.</summary>
    public bool ResponsiveDropdown { get; set; } = true;

    /// <summary>Make tab scrollable if too many tabs.</summary>
    public bool Scrollable { get; set; } = false;
}

/// <summary>Tab layout direction.</summary>
public enum TabLayout
{
    /// <summary>Tabs arranged horizontally (traditional).</summary>
    Horizontal = 0,

    /// <summary>Tabs arranged vertically on the side.</summary>
    Vertical = 1
}

/// <summary>Tab visual styling.</summary>
public enum TabStyle
{
    /// <summary>Classic tab appearance with borders.</summary>
    Tabs = 0,

    /// <summary>Pill/capsule appearance.</summary>
    Pills = 1,

    /// <summary>Underline style with minimal borders.</summary>
    Underline = 2
}

/// <summary>When to activate a tab.</summary>
public enum TabActivation
{
    /// <summary>Activate on mouse click.</summary>
    Click = 0,

    /// <summary>Activate on mouse hover.</summary>
    Hover = 1
}

/// <summary>Event args for tab change.</summary>
public class TabChangedEventArgs
{
    public string PreviousTabId { get; set; } = string.Empty;
    public required string ActiveTabId { get; set; }
    public required TabItem ActiveTab { get; set; }
}

/// <summary>Event args for tab closing.</summary>
public class TabClosedEventArgs
{
    public required string ClosedTabId { get; set; }
    public required TabItem ClosedTab { get; set; }
    public bool Cancel { get; set; } = false;
}
