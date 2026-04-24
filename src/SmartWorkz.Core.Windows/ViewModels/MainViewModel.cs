namespace SmartWorkz.Windows.ViewModels;

/// <summary>
/// Main view model for the application shell/window.
/// Manages application state including connection status, sync status, and user session.
/// Implements MVVM pattern with data binding support for WinUI 3.
/// </summary>
public class MainViewModel : ViewModelBase
{
    private string _title = "SmartWorkz Desktop";
    private bool _isConnected;
    private bool _isSyncing;
    private int _unreadNotificationCount;
    private UserInfo? _currentUser;

    /// <summary>
    /// Gets or sets the window title displayed in the titlebar.
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the app is connected to the backend service.
    /// </summary>
    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether a synchronization operation is in progress.
    /// </summary>
    public bool IsSyncing
    {
        get => _isSyncing;
        set => SetProperty(ref _isSyncing, value);
    }

    /// <summary>
    /// Gets or sets the count of unread notifications for the current user.
    /// </summary>
    public int UnreadNotificationCount
    {
        get => _unreadNotificationCount;
        set => SetProperty(ref _unreadNotificationCount, value);
    }

    /// <summary>
    /// Gets or sets the currently authenticated user.
    /// Null when no user is authenticated.
    /// </summary>
    public UserInfo? CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

    /// <summary>
    /// Initializes a new instance of the MainViewModel class.
    /// </summary>
    public MainViewModel()
    {
        // Initialize with default values
        _title = "SmartWorkz Desktop";
        _isConnected = false;
        _isSyncing = false;
        _unreadNotificationCount = 0;
        _currentUser = null;
    }
}

/// <summary>
/// Represents user information for the current session.
/// </summary>
public class UserInfo
{
    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the user's display name.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;
}
