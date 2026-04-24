namespace SmartWorkz.Windows.Services;

/// <summary>
/// Interface for Windows desktop notifications
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Show a toast notification
    /// </summary>
    Task ShowToastAsync(string title, string message, NotificationType type = NotificationType.Information, CancellationToken cancellationToken = default);

    /// <summary>
    /// Show a dialog notification
    /// </summary>
    Task ShowDialogAsync(string title, string message, string? buttonText = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear all active notifications
    /// </summary>
    Task ClearAllAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Type of notification to display
/// </summary>
public enum NotificationType
{
    Information,
    Success,
    Warning,
    Error
}
