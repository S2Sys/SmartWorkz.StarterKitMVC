namespace SmartWorkz.Mobile.Components;

/// <summary>
/// SmartListView is a modern list component using CollectionView for better performance.
///
/// Features:
/// - Bindable ItemsSource property (IEnumerable)
/// - Bindable SelectionCommand property (ICommand)
/// - Uses CollectionView (modern replacement for ListView)
/// - Single selection mode
/// - Frame wrapper for each item with shadow and rounded corners
/// </summary>
public partial class SmartListView : ContentView
{
    /// <summary>
    /// Defines the ItemsSource bindable property.
    /// </summary>
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(
            nameof(ItemsSource),
            typeof(System.Collections.IEnumerable),
            typeof(SmartListView),
            defaultValue: null,
            propertyChanged: OnItemsSourceChanged);

    /// <summary>
    /// Defines the SelectionCommand bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionCommandProperty =
        BindableProperty.Create(
            nameof(SelectionCommand),
            typeof(ICommand),
            typeof(SmartListView),
            defaultValue: null,
            propertyChanged: OnSelectionCommandChanged);

    /// <summary>
    /// Defines the ItemTemplate bindable property.
    /// </summary>
    public static readonly BindableProperty ItemTemplateProperty =
        BindableProperty.Create(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(SmartListView),
            defaultValue: null,
            propertyChanged: OnItemTemplateChanged);

    /// <summary>
    /// Gets or sets the items source for the list.
    /// </summary>
    public System.Collections.IEnumerable ItemsSource
    {
        get => (System.Collections.IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when an item is selected.
    /// </summary>
    public ICommand SelectionCommand
    {
        get => (ICommand)GetValue(SelectionCommandProperty);
        set => SetValue(SelectionCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template for items in the list.
    /// </summary>
    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the SmartListView class.
    /// </summary>
    public SmartListView()
    {
        InitializeComponent();
        WireUpSelectionHandling();
    }

    /// <summary>
    /// Handles items source property changes.
    /// </summary>
    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SmartListView listView && listView.CollectionViewControl is not null)
        {
            if (newValue == null)
            {
                listView.CollectionViewControl.ItemsSource = null;
            }
            else if (newValue is System.Collections.IEnumerable items)
            {
                listView.CollectionViewControl.ItemsSource = items;
            }
        }
    }

    /// <summary>
    /// Handles selection command property changes.
    /// </summary>
    private static void OnSelectionCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (SmartListView)bindable;
        view.WireUpSelectionHandling();
    }

    /// <summary>
    /// Handles item template property changes.
    /// </summary>
    private static void OnItemTemplateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SmartListView listView && listView.CollectionViewControl is not null && newValue is DataTemplate template)
        {
            listView.CollectionViewControl.ItemTemplate = template;
        }
    }

    /// <summary>
    /// Wires up the selection changed event to execute the selection command.
    /// </summary>
    private void WireUpSelectionHandling()
    {
        if (CollectionViewControl is not null)
        {
            CollectionViewControl.SelectionChangedCommand = new Command<object>(selectedItem =>
            {
                SelectionCommand?.Execute(selectedItem);
            });
        }
    }
}
