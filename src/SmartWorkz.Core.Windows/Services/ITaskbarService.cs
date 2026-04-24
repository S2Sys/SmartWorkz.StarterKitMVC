namespace SmartWorkz.Windows.Services;

/// <summary>
/// Interface for Windows taskbar operations
/// </summary>
public interface ITaskbarService
{
    /// <summary>
    /// Set the taskbar progress value
    /// </summary>
    Task SetProgressAsync(int current, int total, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set the taskbar button badge
    /// </summary>
    Task SetBadgeAsync(string? badge, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activate the application window
    /// </summary>
    Task ActivateWindowAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for Windows system tray operations
/// </summary>
public interface ISystemTrayService
{
    /// <summary>
    /// Show the tray icon
    /// </summary>
    Task ShowIconAsync(string tooltip, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hide the tray icon
    /// </summary>
    Task HideIconAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update the tray tooltip
    /// </summary>
    Task UpdateTooltipAsync(string tooltip, CancellationToken cancellationToken = default);

    /// <summary>
    /// Observable for tray icon click events
    /// </summary>
    IObservable<DateTime> IconClicked { get; }
}
