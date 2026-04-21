namespace SmartWorkz.Web;

/// <summary>
/// Enum representing common Bootstrap icons used throughout the application.
/// </summary>
public enum IconType
{
    // Status icons
    Success,
    Error,
    Warning,
    Info,
    CheckCircle,
    ExclamationTriangle,
    ExclamationCircle,
    InformationCircle,

    // Action icons
    Search,
    Menu,
    Close,
    ChevronLeft,
    ChevronRight,
    ChevronUp,
    ChevronDown,

    // Navigation icons
    User,
    Settings,
    Home,
    Logout,

    // Form icons
    Plus,
    Minus,
    Edit,
    Delete,
    Save,
}

/// <summary>
/// Service interface for providing icon-related utilities.
/// Centralizes icon management and provides methods to get icon CSS classes and HTML markup.
/// </summary>
public interface IIconProvider
{
    /// <summary>
    /// Gets the Bootstrap icon CSS class for the specified icon type.
    /// </summary>
    /// <param name="iconType">The type of icon to retrieve.</param>
    /// <returns>A string containing the icon CSS classes (e.g., "bi bi-check-circle-fill").</returns>
    string GetIconClass(IconType iconType);

    /// <summary>
    /// Gets the Bootstrap icon CSS class for the specified icon type with an optional size modifier.
    /// </summary>
    /// <param name="iconType">The type of icon to retrieve.</param>
    /// <param name="sizeClass">Optional CSS class for sizing (e.g., "fs-5", "fs-6").</param>
    /// <returns>A string containing the icon CSS classes and size class if provided (e.g., "bi bi-check-circle-fill fs-5").</returns>
    string GetIconClass(IconType iconType, string sizeClass);

    /// <summary>
    /// Gets the complete HTML markup for the specified icon.
    /// </summary>
    /// <param name="iconType">The type of icon to retrieve.</param>
    /// <param name="cssClass">Optional CSS class to apply to the icon element.</param>
    /// <returns>HTML string containing the icon element (e.g., "<i class=\"bi bi-check-circle-fill\"></i>").</returns>
    string GetIconHtml(IconType iconType, string? cssClass = null);
}
