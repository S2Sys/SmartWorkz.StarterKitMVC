namespace SmartWorkz.Mobile.Components;

/// <summary>
/// Enum defining button types with different visual styles.
/// </summary>
public enum ButtonType
{
    /// <summary>Primary action button - blue background, white text</summary>
    Primary = 0,

    /// <summary>Secondary action button - light gray background, black text</summary>
    Secondary = 1,

    /// <summary>Danger/destructive action button - red background, white text</summary>
    Danger = 2,

    /// <summary>Success action button - green background, white text</summary>
    Success = 3
}

/// <summary>
/// CustomButton is a styled button component that supports multiple visual variants.
///
/// Features:
/// - Inherits Text and Command properties from Button
/// - Multiple button types with automatic styling
/// - Consistent styling across platforms
/// </summary>
public partial class CustomButton : Button
{
    /// <summary>
    /// Defines the ButtonType bindable property.
    /// </summary>
    public static readonly BindableProperty ButtonTypeProperty =
        BindableProperty.Create(
            nameof(ButtonType),
            typeof(ButtonType),
            typeof(CustomButton),
            defaultValue: ButtonType.Primary,
            propertyChanged: OnButtonTypeChanged);

    /// <summary>
    /// Gets or sets the button type (Primary, Secondary, Danger, Success).
    /// </summary>
    public ButtonType ButtonType
    {
        get => (ButtonType)GetValue(ButtonTypeProperty);
        set => SetValue(ButtonTypeProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the CustomButton class.
    /// </summary>
    public CustomButton()
    {
        InitializeComponent();
        ApplyButtonTypeStyle(ButtonType.Primary);
    }

    /// <summary>
    /// Handles button type property changes.
    /// </summary>
    private static void OnButtonTypeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CustomButton button && newValue is ButtonType buttonType)
        {
            button.ApplyButtonTypeStyle(buttonType);
        }
    }

    /// <summary>
    /// Applies styling based on button type.
    /// </summary>
    private void ApplyButtonTypeStyle(ButtonType type)
    {
        switch (type)
        {
            case ButtonType.Primary:
                BackgroundColor = Color.FromArgb("#007AFF");
                TextColor = Colors.White;
                break;

            case ButtonType.Secondary:
                BackgroundColor = Color.FromArgb("#E8E8E8");
                TextColor = Colors.Black;
                break;

            case ButtonType.Danger:
                BackgroundColor = Color.FromArgb("#FF3B30");
                TextColor = Colors.White;
                break;

            case ButtonType.Success:
                BackgroundColor = Color.FromArgb("#34C759");
                TextColor = Colors.White;
                break;
        }
    }
}
