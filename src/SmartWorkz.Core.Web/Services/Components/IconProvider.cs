namespace SmartWorkz.Web;

using System.Net;

/// <summary>
/// Implementation of IIconProvider that provides Bootstrap icon CSS classes and HTML markup.
/// </summary>
public class IconProvider : IIconProvider
{
    private static readonly Dictionary<IconType, string> IconMapping = new()
    {
        // Status icons
        { IconType.Success, "bi-check-circle-fill" },
        { IconType.Error, "bi-exclamation-triangle-fill" },
        { IconType.Warning, "bi-exclamation-triangle" },
        { IconType.Info, "bi-info-circle" },
        { IconType.CheckCircle, "bi-check-circle" },
        { IconType.ExclamationTriangle, "bi-exclamation-triangle" },
        { IconType.ExclamationCircle, "bi-exclamation-circle" },
        { IconType.InformationCircle, "bi-info-circle-fill" },

        // Action icons
        { IconType.Search, "bi-search" },
        { IconType.Menu, "bi-list" },
        { IconType.Close, "bi-x-circle-fill" },
        { IconType.ChevronLeft, "bi-chevron-left" },
        { IconType.ChevronRight, "bi-chevron-right" },
        { IconType.ChevronUp, "bi-chevron-up" },
        { IconType.ChevronDown, "bi-chevron-down" },

        // Navigation icons
        { IconType.User, "bi-person-circle" },
        { IconType.Settings, "bi-gear" },
        { IconType.Home, "bi-house" },
        { IconType.Logout, "bi-box-arrow-right" },

        // Form icons
        { IconType.Plus, "bi-plus" },
        { IconType.Minus, "bi-dash" },
        { IconType.Edit, "bi-pencil-square" },
        { IconType.Delete, "bi-trash" },
        { IconType.Save, "bi-floppy" },
    };

    /// <summary>
    /// Gets the Bootstrap icon CSS class for the specified icon type.
    /// </summary>
    /// <param name="iconType">The type of icon to retrieve.</param>
    /// <returns>A string containing the icon CSS classes (e.g., "bi bi-check-circle-fill").</returns>
    /// <exception cref="ArgumentException">Thrown when the icon type is unknown.</exception>
    public string GetIconClass(IconType iconType)
    {
        if (!IconMapping.TryGetValue(iconType, out var iconClass))
        {
            throw new ArgumentException($"Unknown icon type: {iconType}", nameof(iconType));
        }

        return $"bi {iconClass}";
    }

    /// <summary>
    /// Gets the Bootstrap icon CSS class for the specified icon type with an optional size modifier.
    /// </summary>
    /// <param name="iconType">The type of icon to retrieve.</param>
    /// <param name="sizeClass">Optional CSS class for sizing (e.g., "fs-5", "fs-6").</param>
    /// <returns>A string containing the icon CSS classes and size class if provided.</returns>
    /// <exception cref="ArgumentException">Thrown when the icon type is unknown.</exception>
    public string GetIconClass(IconType iconType, string sizeClass)
    {
        var baseClass = GetIconClass(iconType);

        if (string.IsNullOrWhiteSpace(sizeClass))
        {
            return baseClass;
        }

        return $"{baseClass} {sizeClass}";
    }

    /// <summary>
    /// Gets the complete HTML markup for the specified icon.
    /// </summary>
    /// <param name="iconType">The type of icon to retrieve.</param>
    /// <param name="cssClass">Optional CSS class to apply to the icon element.</param>
    /// <returns>HTML string containing the icon element.</returns>
    /// <exception cref="ArgumentException">Thrown when the icon type is unknown.</exception>
    public string GetIconHtml(IconType iconType, string? cssClass = null)
    {
        var iconClass = GetIconClass(iconType);

        if (string.IsNullOrWhiteSpace(cssClass))
        {
            return $"<i class=\"{iconClass}\"></i>";
        }

        var encodedCssClass = WebUtility.HtmlEncode(cssClass);
        return $"<i class=\"{iconClass} {encodedCssClass}\"></i>";
    }
}
