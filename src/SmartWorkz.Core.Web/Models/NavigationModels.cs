using Microsoft.AspNetCore.Components;

namespace SmartWorkz.Web.Models;

/// <summary>
/// Represents a single navigation item in the sidebar menu.
/// Supports multi-level nesting with optional custom content.
/// </summary>
public record NavItem
{
    /// <summary>
    /// Gets or sets the unique key identifier for the navigation item.
    /// Used to identify which item was clicked in the OnNavigate callback.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display label for the navigation item.
    /// Shown in the sidebar menu.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Bootstrap icon class for the navigation item.
    /// Example: "bi-house", "bi-person", "bi-gear".
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of child navigation items for multi-level nesting.
    /// When populated, a collapsible submenu is displayed.
    /// </summary>
    public List<NavItem>? Children { get; set; }

    /// <summary>
    /// Gets or sets custom content to render instead of the default label.
    /// Allows for rich HTML or component content in navigation items.
    /// </summary>
    public RenderFragment? ChildContent { get; set; }
}
