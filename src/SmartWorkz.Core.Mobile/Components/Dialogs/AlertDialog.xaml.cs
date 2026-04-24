namespace SmartWorkz.Mobile.Components;

/// <summary>
/// AlertDialog is a modal dialog component for displaying alerts and confirmations.
///
/// Features:
/// - Bindable Title property
/// - Bindable Message property
/// - Bindable ConfirmText property
/// - Bindable CancelText property
/// - Bindable ConfirmCommand property (ICommand)
/// - Bindable CancelCommand property (ICommand)
/// - Modal dialog with OK/Cancel buttons
/// - Overlay backdrop to dismiss dialog
/// </summary>
public partial class AlertDialog : ContentView
{
    /// <summary>
    /// Defines the Title bindable property.
    /// </summary>
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(AlertDialog),
            defaultValue: "Alert",
            propertyChanged: OnTitleChanged);

    /// <summary>
    /// Defines the Message bindable property.
    /// </summary>
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(
            nameof(Message),
            typeof(string),
            typeof(AlertDialog),
            defaultValue: "",
            propertyChanged: OnMessageChanged);

    /// <summary>
    /// Defines the ConfirmText bindable property.
    /// </summary>
    public static readonly BindableProperty ConfirmTextProperty =
        BindableProperty.Create(
            nameof(ConfirmText),
            typeof(string),
            typeof(AlertDialog),
            defaultValue: "OK",
            propertyChanged: OnConfirmTextChanged);

    /// <summary>
    /// Defines the CancelText bindable property.
    /// </summary>
    public static readonly BindableProperty CancelTextProperty =
        BindableProperty.Create(
            nameof(CancelText),
            typeof(string),
            typeof(AlertDialog),
            defaultValue: "Cancel",
            propertyChanged: OnCancelTextChanged);

    /// <summary>
    /// Defines the ConfirmCommand bindable property.
    /// </summary>
    public static readonly BindableProperty ConfirmCommandProperty =
        BindableProperty.Create(
            nameof(ConfirmCommand),
            typeof(ICommand),
            typeof(AlertDialog),
            defaultValue: null,
            propertyChanged: OnConfirmCommandChanged);

    /// <summary>
    /// Defines the CancelCommand bindable property.
    /// </summary>
    public static readonly BindableProperty CancelCommandProperty =
        BindableProperty.Create(
            nameof(CancelCommand),
            typeof(ICommand),
            typeof(AlertDialog),
            defaultValue: null,
            propertyChanged: OnCancelCommandChanged);

    /// <summary>
    /// Defines the IsOpen bindable property.
    /// </summary>
    public static readonly BindableProperty IsOpenProperty =
        BindableProperty.Create(
            nameof(IsOpen),
            typeof(bool),
            typeof(AlertDialog),
            defaultValue: false,
            propertyChanged: OnIsOpenChanged);

    /// <summary>
    /// Gets or sets the dialog title.
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the dialog message.
    /// </summary>
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>
    /// Gets or sets the confirm button text.
    /// </summary>
    public string ConfirmText
    {
        get => (string)GetValue(ConfirmTextProperty);
        set => SetValue(ConfirmTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the cancel button text.
    /// </summary>
    public string CancelText
    {
        get => (string)GetValue(CancelTextProperty);
        set => SetValue(CancelTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute on confirm.
    /// </summary>
    public ICommand ConfirmCommand
    {
        get => (ICommand)GetValue(ConfirmCommandProperty);
        set => SetValue(ConfirmCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute on cancel.
    /// </summary>
    public ICommand CancelCommand
    {
        get => (ICommand)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the dialog is open/visible.
    /// </summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the AlertDialog class.
    /// </summary>
    public AlertDialog()
    {
        InitializeComponent();
        WireUpButtonHandlers();

        // Handle cleanup when component is unloaded
        this.Unloaded += (sender, args) => UnwireButtonHandlers();
    }

    /// <summary>
    /// Handles title property changes.
    /// </summary>
    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AlertDialog dialog && dialog.TitleLabel is not null)
        {
            dialog.TitleLabel.Text = (string)newValue ?? "Alert";
        }
    }

    /// <summary>
    /// Handles message property changes.
    /// </summary>
    private static void OnMessageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AlertDialog dialog && dialog.MessageLabel is not null)
        {
            dialog.MessageLabel.Text = (string)newValue ?? "";
        }
    }

    /// <summary>
    /// Handles confirm text property changes.
    /// </summary>
    private static void OnConfirmTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AlertDialog dialog && dialog.ConfirmButton is not null)
        {
            dialog.ConfirmButton.Text = (string)newValue ?? "OK";
        }
    }

    /// <summary>
    /// Handles cancel text property changes.
    /// </summary>
    private static void OnCancelTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AlertDialog dialog && dialog.CancelButton is not null)
        {
            dialog.CancelButton.Text = (string)newValue ?? "Cancel";
        }
    }

    /// <summary>
    /// Handles confirm command property changes.
    /// </summary>
    private static void OnConfirmCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        // Command is available for binding
    }

    /// <summary>
    /// Handles cancel command property changes.
    /// </summary>
    private static void OnCancelCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        // Command is available for binding
    }

    /// <summary>
    /// Handles IsOpen property changes.
    /// </summary>
    private static void OnIsOpenChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AlertDialog dialog && dialog.DialogContainer is not null)
        {
            dialog.DialogContainer.IsVisible = (bool)newValue;
        }
    }

    /// <summary>
    /// Wires up button click handlers to execute commands.
    /// </summary>
    private void WireUpButtonHandlers()
    {
        if (ConfirmButton is not null)
            ConfirmButton.Clicked += OnConfirmButtonClicked;

        if (CancelButton is not null)
            CancelButton.Clicked += OnCancelButtonClicked;
    }

    /// <summary>
    /// Unwires button click handlers to prevent memory leaks.
    /// </summary>
    private void UnwireButtonHandlers()
    {
        if (ConfirmButton is not null)
            ConfirmButton.Clicked -= OnConfirmButtonClicked;

        if (CancelButton is not null)
            CancelButton.Clicked -= OnCancelButtonClicked;
    }

    /// <summary>
    /// Handles the confirm button clicked event.
    /// </summary>
    private void OnConfirmButtonClicked(object sender, EventArgs e)
    {
        if (ConfirmCommand?.CanExecute(null) == true)
        {
            ConfirmCommand.Execute(null);
        }

        IsOpen = false;
    }

    /// <summary>
    /// Handles the cancel button clicked event.
    /// </summary>
    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        if (CancelCommand?.CanExecute(null) == true)
        {
            CancelCommand.Execute(null);
        }

        IsOpen = false;
    }
}
