namespace SmartWorkz.Mobile.Components;

/// <summary>
/// ValidatedEntry is an enhanced entry component with validation support.
///
/// Features:
/// - Bindable Label property
/// - Bindable Text property
/// - Bindable Placeholder property
/// - Bindable ErrorText property
/// - Bindable HasError property (bool)
/// - Bindable KeyboardType property
/// - Bindable Validator function property
/// - Auto-validates on text change
/// - Displays error label and changes border color on validation failure
/// </summary>
public partial class ValidatedEntry : ContentView
{
    /// <summary>
    /// Flag to prevent reentrancy in OnTextChanged handler.
    /// </summary>
    private bool _isUpdating = false;

    /// <summary>
    /// Defines the Label bindable property.
    /// </summary>
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(
            nameof(Label),
            typeof(string),
            typeof(ValidatedEntry),
            defaultValue: "",
            propertyChanged: OnLabelChanged);

    /// <summary>
    /// Defines the Text bindable property.
    /// </summary>
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(ValidatedEntry),
            defaultValue: "",
            propertyChanged: OnTextChanged);

    /// <summary>
    /// Defines the Placeholder bindable property.
    /// </summary>
    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(string),
            typeof(ValidatedEntry),
            defaultValue: "",
            propertyChanged: OnPlaceholderChanged);

    /// <summary>
    /// Defines the ErrorText bindable property.
    /// </summary>
    public static readonly BindableProperty ErrorTextProperty =
        BindableProperty.Create(
            nameof(ErrorText),
            typeof(string),
            typeof(ValidatedEntry),
            defaultValue: "",
            propertyChanged: OnErrorTextChanged);

    /// <summary>
    /// Defines the HasError bindable property.
    /// </summary>
    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(
            nameof(HasError),
            typeof(bool),
            typeof(ValidatedEntry),
            defaultValue: false,
            propertyChanged: OnHasErrorChanged);

    /// <summary>
    /// Defines the KeyboardType bindable property.
    /// </summary>
    public static readonly BindableProperty KeyboardTypeProperty =
        BindableProperty.Create(
            nameof(KeyboardType),
            typeof(Keyboard),
            typeof(ValidatedEntry),
            defaultValue: Keyboard.Default,
            propertyChanged: OnKeyboardTypeChanged);

    /// <summary>
    /// Defines the Validator bindable property.
    /// Validator is a function that takes text and returns (isValid, errorMessage).
    /// </summary>
    public static readonly BindableProperty ValidatorProperty =
        BindableProperty.Create(
            nameof(Validator),
            typeof(Func<string, (bool, string)>),
            typeof(ValidatedEntry),
            defaultValue: null,
            propertyChanged: OnValidatorChanged);

    /// <summary>
    /// Gets or sets the label text displayed above the entry.
    /// </summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// Gets or sets the entry text.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the placeholder text displayed when entry is empty.
    /// </summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>
    /// Gets or sets the error message text displayed below the entry.
    /// </summary>
    public string ErrorText
    {
        get => (string)GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the entry has a validation error.
    /// </summary>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    /// <summary>
    /// Gets or sets the keyboard type for the entry.
    /// </summary>
    public Keyboard KeyboardType
    {
        get => (Keyboard)GetValue(KeyboardTypeProperty);
        set => SetValue(KeyboardTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the validator function.
    /// The function takes text and returns (isValid, errorMessage).
    /// </summary>
    public Func<string, (bool, string)> Validator
    {
        get => (Func<string, (bool, string)>)GetValue(ValidatorProperty);
        set => SetValue(ValidatorProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the ValidatedEntry class.
    /// </summary>
    public ValidatedEntry()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles label property changes.
    /// </summary>
    private static void OnLabelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ValidatedEntry entry && entry.LabelLabel is not null)
        {
            entry.LabelLabel.Text = (string)newValue ?? "";
        }
    }

    /// <summary>
    /// Handles text property changes and triggers validation.
    /// </summary>
    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ValidatedEntry entry && entry.EntryControl is not null)
        {
            if (entry._isUpdating)
                return;

            entry._isUpdating = true;
            try
            {
                entry.EntryControl.Text = (string)newValue ?? "";
                entry.ValidateText();
            }
            finally
            {
                entry._isUpdating = false;
            }
        }
    }

    /// <summary>
    /// Handles placeholder property changes.
    /// </summary>
    private static void OnPlaceholderChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ValidatedEntry entry && entry.EntryControl is not null)
        {
            entry.EntryControl.Placeholder = (string)newValue ?? "";
        }
    }

    /// <summary>
    /// Handles error text property changes.
    /// </summary>
    private static void OnErrorTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ValidatedEntry entry && entry.ErrorLabel is not null)
        {
            entry.ErrorLabel.Text = (string)newValue ?? "";
        }
    }

    /// <summary>
    /// Handles HasError property changes and updates visual state.
    /// </summary>
    private static void OnHasErrorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ValidatedEntry entry)
        {
            entry.UpdateErrorState((bool)newValue);
        }
    }

    /// <summary>
    /// Handles keyboard type property changes.
    /// </summary>
    private static void OnKeyboardTypeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ValidatedEntry entry && entry.EntryControl is not null)
        {
            entry.EntryControl.Keyboard = newValue as Keyboard ?? Keyboard.Default;
        }
    }

    /// <summary>
    /// Handles validator property changes.
    /// </summary>
    private static void OnValidatorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var entry = (ValidatedEntry)bindable;
        entry.ValidateText();
    }

    /// <summary>
    /// Validates the current text using the validator function.
    /// </summary>
    private void ValidateText()
    {
        if (Validator != null)
        {
            var (isValid, errorMessage) = Validator(Text);
            HasError = !isValid;
            ErrorText = errorMessage;
        }
    }

    /// <summary>
    /// Updates the error state visuals (border color and error label visibility).
    /// </summary>
    private void UpdateErrorState(bool hasError)
    {
        if (BorderFrame is not null)
        {
            BorderFrame.Stroke = hasError ? Color.FromArgb("#FF3B30") : Color.FromArgb("#D3D3D3");
        }

        if (ErrorLabel is not null)
        {
            ErrorLabel.IsVisible = hasError;
        }
    }
}
