namespace SmartWorkz.Mobile.Components;

/// <summary>
/// CustomPicker is a styled picker component with binding support.
///
/// Features:
/// - Bindable Label property
/// - Bindable SelectedItem property
/// - Bindable ItemsSource property (IEnumerable)
/// - Bindable DisplayMemberPath property
/// - Bindable SelectedValuePath property
/// - Consistent styling with rounded corners and borders
/// </summary>
public partial class CustomPicker : ContentView
{
    /// <summary>
    /// Defines the Label bindable property.
    /// </summary>
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(
            nameof(Label),
            typeof(string),
            typeof(CustomPicker),
            defaultValue: "",
            propertyChanged: OnLabelChanged);

    /// <summary>
    /// Defines the SelectedItem bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(
            nameof(SelectedItem),
            typeof(object),
            typeof(CustomPicker),
            defaultValue: null,
            propertyChanged: OnSelectedItemChanged);

    /// <summary>
    /// Defines the ItemsSource bindable property.
    /// </summary>
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(
            nameof(ItemsSource),
            typeof(System.Collections.IEnumerable),
            typeof(CustomPicker),
            defaultValue: null,
            propertyChanged: OnItemsSourceChanged);

    /// <summary>
    /// Defines the DisplayMemberPath bindable property.
    /// </summary>
    public static readonly BindableProperty DisplayMemberPathProperty =
        BindableProperty.Create(
            nameof(DisplayMemberPath),
            typeof(string),
            typeof(CustomPicker),
            defaultValue: "",
            propertyChanged: OnDisplayMemberPathChanged);

    /// <summary>
    /// Defines the SelectedValuePath bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedValuePathProperty =
        BindableProperty.Create(
            nameof(SelectedValuePath),
            typeof(string),
            typeof(CustomPicker),
            defaultValue: "",
            propertyChanged: OnSelectedValuePathChanged);

    /// <summary>
    /// Gets or sets the label text displayed above the picker.
    /// </summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected item.
    /// </summary>
    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Gets or sets the items source for the picker.
    /// </summary>
    public System.Collections.IEnumerable ItemsSource
    {
        get => (System.Collections.IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the property path to use for display text.
    /// </summary>
    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    /// <summary>
    /// Gets or sets the property path to use for the selected value.
    /// </summary>
    public string SelectedValuePath
    {
        get => (string)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the CustomPicker class.
    /// </summary>
    public CustomPicker()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles label property changes.
    /// </summary>
    private static void OnLabelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CustomPicker picker && picker.LabelLabel is not null)
        {
            picker.LabelLabel.Text = (string)newValue ?? "";
        }
    }

    /// <summary>
    /// Handles selected item property changes.
    /// </summary>
    private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CustomPicker picker && picker.PickerControl is not null)
        {
            picker.PickerControl.SelectedItem = newValue;
        }
    }

    /// <summary>
    /// Handles items source property changes.
    /// </summary>
    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CustomPicker picker && picker.PickerControl is not null)
        {
            if (newValue == null)
            {
                picker.PickerControl.ItemsSource = null;
            }
            else if (newValue is System.Collections.IEnumerable items)
            {
                picker.PickerControl.ItemsSource = items;
            }
        }
    }

    /// <summary>
    /// Handles display member path property changes.
    /// </summary>
    private static void OnDisplayMemberPathChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CustomPicker picker && picker.PickerControl is not null)
        {
            if (string.IsNullOrEmpty((string)newValue))
            {
                picker.PickerControl.ItemDisplayBinding = null;
            }
            else
            {
                picker.PickerControl.ItemDisplayBinding = new Binding((string)newValue);
            }
        }
    }

    /// <summary>
    /// Handles selected value path property changes.
    /// </summary>
    private static void OnSelectedValuePathChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var picker = (CustomPicker)bindable;
        if (picker.PickerControl is not null && newValue is string path)
        {
            picker.PickerControl.SelectedValuePath = path;
        }
    }
}
