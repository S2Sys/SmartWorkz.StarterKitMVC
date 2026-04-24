namespace SmartWorkz.Mobile.Components;

/// <summary>
/// LoadingIndicator is a component that displays a loading state with spinner and message.
///
/// Features:
/// - Bindable IsLoading property (bool)
/// - Bindable Message property (string)
/// - ActivityIndicator with primary color (#007AFF)
/// - Label showing loading message
/// - Visibility toggles with IsLoading property
/// </summary>
public partial class LoadingIndicator : ContentView
{
    /// <summary>
    /// Defines the IsLoading bindable property.
    /// </summary>
    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(
            nameof(IsLoading),
            typeof(bool),
            typeof(LoadingIndicator),
            defaultValue: false,
            propertyChanged: OnIsLoadingChanged);

    /// <summary>
    /// Defines the Message bindable property.
    /// </summary>
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(
            nameof(Message),
            typeof(string),
            typeof(LoadingIndicator),
            defaultValue: "Loading...",
            propertyChanged: OnMessageChanged);

    /// <summary>
    /// Gets or sets a value indicating whether the loading indicator is visible.
    /// </summary>
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>
    /// Gets or sets the loading message text.
    /// </summary>
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the LoadingIndicator class.
    /// </summary>
    public LoadingIndicator()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles IsLoading property changes and updates visibility.
    /// </summary>
    private static void OnIsLoadingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LoadingIndicator indicator)
        {
            bool isLoading = (bool)newValue;
            if (indicator.LoadingContainer is not null)
            {
                indicator.LoadingContainer.IsVisible = isLoading;
            }
            if (indicator.ActivityIndicatorControl is not null)
            {
                indicator.ActivityIndicatorControl.IsRunning = isLoading;
            }
        }
    }

    /// <summary>
    /// Handles Message property changes.
    /// </summary>
    private static void OnMessageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LoadingIndicator indicator && indicator.MessageLabel is not null)
        {
            indicator.MessageLabel.Text = (string)newValue ?? "Loading...";
        }
    }
}
