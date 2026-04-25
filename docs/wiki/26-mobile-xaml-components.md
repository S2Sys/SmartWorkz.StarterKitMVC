# Mobile XAML Components

## Overview

The Mobile XAML Components library provides six production-ready, reusable UI components for .NET MAUI applications. These components encapsulate common mobile patterns with built-in validation, styling, and platform-specific optimizations. Use this library when building forms, lists, dialogs, and other interactive screens to maintain visual consistency and reduce boilerplate code across iOS and Android platforms.

---

## Architecture

### Component Relationships

```
┌─────────────────────────────────────────────┐
│       Mobile XAML Components Library         │
└─────────────────────────────────────────────┘
                      │
        ┌─────────────┼─────────────┐
        │             │             │
    ┌───▼───┐    ┌────▼────┐   ┌──▼──┐
    │ Input │    │ Display │   │ UI  │
    │  Zone │    │  Zone   │   │     │
    └───┬───┘    └────┬────┘   └──┬──┘
        │             │           │
   ┌────┴────┐  ┌─────┴──┐  ┌────┴─────┐
   │Validated│  │ Smart  │  │  Loading  │
   │ Entry   │  │ List   │  │Indicator  │
   │         │  │ View   │  │           │
   │         │  └────────┘  └───────────┘
   └────┬────┘
        │
   ┌────┴──────┐
   │ Custom    │
   │ Picker    │
   └───────────┘


┌──────────────────────┐   ┌──────────────────────┐
│    Actions Zone      │   │   Appearance Zone    │
├──────────────────────┤   ├──────────────────────┤
│                      │   │                      │
│  • CustomButton      │   │  • AlertDialog       │
│    (4 variants)      │   │    (modal overlay)   │
│                      │   │                      │
└──────────────────────┘   └──────────────────────┘
```

### Component Property Overview

| Component | Properties | Purpose |
|-----------|-----------|---------|
| **CustomButton** | 1 (ButtonType) | Styled button with 4 variants (Primary, Secondary, Danger, Success) |
| **ValidatedEntry** | 7 (Label, Text, Placeholder, ErrorText, HasError, KeyboardType, Validator) | Entry with real-time validation and error display |
| **CustomPicker** | 5 (Label, SelectedItem, ItemsSource, DisplayMemberPath, SelectedValuePath) | Dropdown selector with binding support |
| **AlertDialog** | 8 (Title, Message, ConfirmText, CancelText, ConfirmCommand, CancelCommand, IsOpen) | Modal confirmation dialog with command binding |
| **SmartListView** | 3 (ItemsSource, SelectionCommand, ItemTemplate) | Modern list using CollectionView for performance |
| **LoadingIndicator** | 2 (IsLoading, Message) | Async operation spinner with status text |

### iOS HIG Color Palette

| Color | Hex | Usage | Component |
|-------|-----|-------|-----------|
| **Primary Blue** | #007AFF | Primary buttons, active states | CustomButton, LoadingIndicator |
| **Error Red** | #FF3B30 | Destructive actions, validation errors | CustomButton (Danger), ValidatedEntry (border) |
| **Success Green** | #34C759 | Positive confirmations | CustomButton (Success) |
| **Warning Yellow** | #FFCC00 | Alerts, attention-needed states | AlertDialog (warning) |
| **Secondary Orange** | #FF9500 | Secondary actions, info states | CustomButton (Secondary) |
| **Neutral Brown** | #A2845E | Subtle backgrounds, borders | General styling |

---

## Quick Start

Get one component working in 5 lines of XAML:

```xaml
<custombutton:CustomButton
    Text="Submit"
    ButtonType="Primary"
    Command="{Binding SubmitCommand}"
    Margin="10"
    Padding="12" />

<local:ValidatedEntry
    Label="Email"
    Placeholder="user@example.com"
    Text="{Binding Email}"
    Validator="{StaticResource EmailValidator}" />

<local:SmartListView
    ItemsSource="{Binding Products}"
    SelectionCommand="{Binding SelectProductCommand}" />

<local:AlertDialog
    Title="Confirm Delete"
    Message="Are you sure?"
    IsOpen="{Binding ShowDeleteConfirm}"
    ConfirmCommand="{Binding DeleteCommand}" />

<local:LoadingIndicator
    IsLoading="{Binding IsLoading}"
    Message="Processing..." />
```

---

## Configuration

### CustomButton — ButtonType Property

The CustomButton component supports four predefined visual variants via the ButtonType enumeration.

| ButtonType | Background | Text Color | Usage |
|------------|-----------|-----------|-------|
| Primary | #007AFF (iOS Blue) | White | Main action (Save, Submit, OK) |
| Secondary | #E8E8E8 (Light Gray) | Black | Alternative action (Cancel, Skip) |
| Danger | #FF3B30 (iOS Red) | White | Destructive action (Delete, Reject) |
| Success | #34C759 (iOS Green) | White | Confirmation (Confirmed, Saved) |

```xaml
<!-- Primary Button (Default) -->
<custombutton:CustomButton Text="Save" ButtonType="Primary" />

<!-- Secondary Button -->
<custombutton:CustomButton Text="Cancel" ButtonType="Secondary" />

<!-- Danger Button (Destructive) -->
<custombutton:CustomButton Text="Delete" ButtonType="Danger" />

<!-- Success Button (Confirmation) -->
<custombutton:CustomButton Text="Confirmed" ButtonType="Success" />
```

### ValidatedEntry — Input Validation Properties

The ValidatedEntry component provides automatic validation, error display, and keyboard type selection. It includes a re-entrancy guard (`_isUpdating`) to prevent validator callbacks from creating circular updates.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Label | string | "" | Label text displayed above entry |
| Text | string | "" | Current entry text (bindable) |
| Placeholder | string | "" | Hint text shown when empty |
| ErrorText | string | "" | Error message displayed below entry |
| HasError | bool | false | Validation state flag |
| KeyboardType | Keyboard | Keyboard.Default | Mobile keyboard type (Email, Numeric, etc.) |
| Validator | Func<string, (bool, string)> | null | Validation function returning (isValid, errorMessage) |

```xaml
<local:ValidatedEntry
    Label="Full Name"
    Placeholder="Enter your name"
    Text="{Binding FullName}"
    KeyboardType="Default"
    Validator="{StaticResource NameValidator}"
    HasError="{Binding HasNameError, Mode=OneWayToSource}"
    ErrorText="{Binding NameErrorText}" />
```

### CustomPicker — Selection Properties

The CustomPicker component binds to any IEnumerable collection and supports display/value path customization for complex object selection.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Label | string | "" | Label text displayed above picker |
| SelectedItem | object | null | Currently selected item |
| ItemsSource | IEnumerable | null | Collection of items to display |
| DisplayMemberPath | string | "" | Property name used for display text |
| SelectedValuePath | string | "" | Property name used as selected value |

```xaml
<local:CustomPicker
    Label="Select Category"
    ItemsSource="{Binding Categories}"
    SelectedItem="{Binding SelectedCategory}"
    DisplayMemberPath="Name"
    SelectedValuePath="Id" />
```

### AlertDialog — Command-Based Dialog Properties

The AlertDialog component manages its own lifecycle with the IsOpen property and supports command binding for confirm/cancel actions. Event handlers are properly wired and unwired in constructor/destructor to prevent memory leaks.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Title | string | "Alert" | Dialog title text |
| Message | string | "" | Dialog message body |
| ConfirmText | string | "OK" | Confirm button label |
| CancelText | string | "Cancel" | Cancel button label |
| ConfirmCommand | ICommand | null | Command executed on confirm |
| CancelCommand | ICommand | null | Command executed on cancel |
| IsOpen | bool | false | Dialog visibility state |

```xaml
<local:AlertDialog
    Title="Delete Item"
    Message="This action cannot be undone."
    ConfirmText="Delete"
    CancelText="Keep Item"
    ConfirmCommand="{Binding DeleteCommand}"
    CancelCommand="{Binding CancelCommand}"
    IsOpen="{Binding ShowDeleteDialog}" />
```

### SmartListView — Modern List Display

The SmartListView uses CollectionView internally for better performance and memory management compared to the deprecated ListView. It supports single selection with command binding.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| ItemsSource | IEnumerable | null | Collection of items to display |
| SelectionCommand | ICommand | null | Command executed when item selected |
| ItemTemplate | DataTemplate | null | Custom template for list items |

**Important:** SmartListView uses `CollectionView`, not ListView. This provides:
- Virtualization for large lists (memory efficient)
- Better scrolling performance
- Modern layout controls

```xaml
<local:SmartListView
    ItemsSource="{Binding ProductList}"
    SelectionCommand="{Binding SelectProductCommand}"
    ItemTemplate="{StaticResource ProductItemTemplate}" />
```

### LoadingIndicator — Async Operation Overlay

The LoadingIndicator displays an activity spinner with message text, typically used to block interaction while async operations complete.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| IsLoading | bool | false | Show/hide the loading overlay |
| Message | string | "Loading..." | Status message displayed below spinner |

```xaml
<local:LoadingIndicator
    IsLoading="{Binding IsProcessing}"
    Message="Uploading file..." />
```

---

## Usage Examples

### Example 1: CustomButton with All Four Variants

Display all button types in a vertical stack, demonstrating color usage:

```xaml
<VerticalStackLayout Spacing="10" Padding="20">
    <Label Text="Button Variants" FontSize="18" FontAttributes="Bold" />
    
    <custombutton:CustomButton
        Text="Primary Action"
        ButtonType="Primary"
        Command="{Binding PrimaryCommand}"
        Padding="12"
        CornerRadius="8" />
    
    <custombutton:CustomButton
        Text="Secondary Action"
        ButtonType="Secondary"
        Command="{Binding SecondaryCommand}"
        Padding="12"
        CornerRadius="8" />
    
    <custombutton:CustomButton
        Text="Confirm & Save"
        ButtonType="Success"
        Command="{Binding SaveCommand}"
        Padding="12"
        CornerRadius="8" />
    
    <custombutton:CustomButton
        Text="Delete Account"
        ButtonType="Danger"
        Command="{Binding DeleteAccountCommand}"
        Padding="12"
        CornerRadius="8" />
</VerticalStackLayout>
```

### Example 2: ValidatedEntry with Email Validation

Demonstrate real-time validation with error display and re-entrancy protection:

```xaml
<VerticalStackLayout Spacing="8" Padding="20">
    <local:ValidatedEntry
        Label="Email Address"
        Placeholder="user@example.com"
        Text="{Binding Email}"
        KeyboardType="Email"
        Validator="{StaticResource EmailValidator}"
        HasError="{Binding HasEmailError, Mode=OneWayToSource}"
        ErrorText="{Binding EmailErrorText}" />
    
    <custombutton:CustomButton
        Text="Sign Up"
        ButtonType="Primary"
        Command="{Binding SignUpCommand}"
        IsEnabled="{Binding IsValidEmail}" />
</VerticalStackLayout>
```

With the validator function in code-behind or ViewModel:

```csharp
private Func<string, (bool, string)> EmailValidator = email =>
{
    if (string.IsNullOrWhiteSpace(email))
        return (false, "Email is required");
    
    if (!System.Text.RegularExpressions.Regex.IsMatch(
        email, 
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        return (false, "Please enter a valid email address");
    
    return (true, "");
};
```

**Re-entrancy Guard Warning:** The ValidatedEntry component has an internal `_isUpdating` flag to prevent circular updates. Do NOT update the Text property from within your Validator function:

```csharp
// WRONG - Creates re-entrancy issue:
private Func<string, (bool, string)> BadValidator = email =>
{
    entry.Text = email.ToUpper(); // CIRCULAR!
    return (true, "");
};

// CORRECT - Just validate:
private Func<string, (bool, string)> GoodValidator = email =>
{
    var isValid = email.Contains("@");
    return (isValid, isValid ? "" : "Invalid email");
};
```

### Example 3: AlertDialog Delete Confirmation Pattern

Implement a common delete confirmation dialog with undo capability:

```xaml
<Grid RowDefinitions="*, Auto">
    <local:SmartListView
        ItemsSource="{Binding Items}"
        SelectionCommand="{Binding SelectItemCommand}" />
    
    <local:AlertDialog
        Grid.Row="1"
        Title="Delete Item"
        Message="This item will be permanently deleted. This action cannot be undone."
        ConfirmText="Delete"
        CancelText="Keep Item"
        ConfirmCommand="{Binding ConfirmDeleteCommand}"
        CancelCommand="{Binding CancelDeleteCommand}"
        IsOpen="{Binding ShowDeleteConfirmation}" />
</Grid>
```

ViewModel:

```csharp
public class ItemListViewModel
{
    private Item _itemToDelete;
    
    public ICommand SelectItemCommand => new Command<Item>(item =>
    {
        _itemToDelete = item;
        ShowDeleteConfirmation = true;
    });
    
    public ICommand ConfirmDeleteCommand => new Command(async () =>
    {
        if (_itemToDelete != null)
        {
            await _itemRepository.DeleteAsync(_itemToDelete.Id);
            Items.Remove(_itemToDelete);
            ShowDeleteConfirmation = false;
        }
    });
    
    public ICommand CancelDeleteCommand => new Command(() =>
    {
        ShowDeleteConfirmation = false;
        _itemToDelete = null;
    });
}
```

### Example 4: SmartListView with ObservableCollection

Demonstrate proper binding with ObservableCollection for real-time updates:

```xaml
<local:SmartListView
    ItemsSource="{Binding Products}"
    SelectionCommand="{Binding SelectProductCommand}">
    <local:SmartListView.ItemTemplate>
        <DataTemplate x:DataType="local:ProductViewModel">
            <VerticalStackLayout Padding="12" Spacing="4">
                <Label Text="{Binding Name}" FontSize="16" FontAttributes="Bold" />
                <Label Text="{Binding Price, StringFormat='${0:F2}'}" FontSize="14" TextColor="Gray" />
                <Label Text="{Binding Description}" FontSize="12" LineBreakMode="TailTruncation" />
            </VerticalStackLayout>
        </DataTemplate>
    </local:SmartListView.ItemTemplate>
</local:SmartListView>
```

ViewModel:

```csharp
public partial class ProductListViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ProductViewModel> products;
    
    [RelayCommand]
    private async Task SelectProduct(ProductViewModel product)
    {
        if (product == null) return;
        
        await Shell.Current.GoToAsync($"product/{product.Id}");
    }
    
    public ProductListViewModel(IProductRepository repository)
    {
        var items = await repository.GetAllAsync();
        Products = new ObservableCollection<ProductViewModel>(
            items.Select(p => new ProductViewModel(p))
        );
    }
}
```

**Key Point:** Use `ObservableCollection<T>` with SmartListView. The component uses CollectionView internally, which monitors the collection for changes. When you add/remove items from ObservableCollection, the list updates automatically.

### Example 5: LoadingIndicator Wrapping Async Operation

Demonstrate wrapping an async file upload with loading state:

```xaml
<Grid RowDefinitions="*, Auto" Padding="20">
    <VerticalStackLayout
        Grid.Row="0"
        Spacing="20"
        VerticalOptions="Center">
        
        <Label Text="Upload File" FontSize="20" FontAttributes="Bold" />
        
        <custombutton:CustomButton
            Text="Choose File"
            ButtonType="Primary"
            Command="{Binding ChooseFileCommand}"
            IsEnabled="{Binding IsNotLoading}" />
        
        <Label
            Text="{Binding SelectedFileName}"
            FontSize="14"
            IsVisible="{Binding HasFileSelected}" />
    </VerticalStackLayout>
    
    <local:LoadingIndicator
        Grid.RowSpan="2"
        IsLoading="{Binding IsLoading}"
        Message="Uploading file..." />
</Grid>
```

ViewModel:

```csharp
public partial class FileUploadViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading;
    
    [ObservableProperty]
    private string selectedFileName;
    
    [ObservableProperty]
    private bool hasFileSelected;
    
    public bool IsNotLoading => !IsLoading;
    
    [RelayCommand]
    private async Task ChooseFile()
    {
        var result = await FilePicker.Default.PickAsync();
        if (result == null) return;
        
        SelectedFileName = result.FileName;
        HasFileSelected = true;
        
        IsLoading = true;
        try
        {
            using var stream = await result.OpenReadAsync();
            await _fileService.UploadAsync(stream, result.FileName);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

---

## API Reference

### CustomButton

**Namespace:** `SmartWorkz.Mobile.Components`

**Base Class:** Button

**BindableProperties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| ButtonType | ButtonType | Primary | Visual variant (Primary, Secondary, Danger, Success) |

**ButtonType Enum:**
- `Primary` (0) — Blue background (#007AFF), white text
- `Secondary` (1) — Light gray background (#E8E8E8), black text
- `Danger` (2) — Red background (#FF3B30), white text
- `Success` (3) — Green background (#34C759), white text

**Code-behind Events:**
- `OnButtonTypeChanged(BindableObject, object, object)` — Applies style when ButtonType changes

---

### ValidatedEntry

**Namespace:** `SmartWorkz.Mobile.Components`

**Base Class:** ContentView

**BindableProperties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Label | string | "" | Label displayed above entry |
| Text | string | "" | Entry text content |
| Placeholder | string | "" | Hint text |
| ErrorText | string | "" | Error message below entry |
| HasError | bool | false | Validation state |
| KeyboardType | Keyboard | Default | Mobile keyboard type |
| Validator | Func<string, (bool, string)> | null | Validation function |

**Internal Properties:**
- `_isUpdating` (bool, private) — Re-entrancy guard flag

**Events:**
- `OnTextChanged(BindableObject, object, object)` — Triggers ValidateText() with re-entrancy check
- `OnValidatorChanged(BindableObject, object, object)` — Re-validates when validator updates

**Private Methods:**
- `ValidateText()` — Calls Validator and updates HasError/ErrorText
- `UpdateErrorState(bool)` — Updates border color and error label visibility

---

### CustomPicker

**Namespace:** `SmartWorkz.Mobile.Components`

**Base Class:** ContentView

**BindableProperties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Label | string | "" | Picker label |
| SelectedItem | object | null | Currently selected item |
| ItemsSource | IEnumerable | null | Items collection |
| DisplayMemberPath | string | "" | Property for display text |
| SelectedValuePath | string | "" | Property for value |

---

### AlertDialog

**Namespace:** `SmartWorkz.Mobile.Components`

**Base Class:** ContentView

**BindableProperties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Title | string | "Alert" | Dialog title |
| Message | string | "" | Dialog message |
| ConfirmText | string | "OK" | Confirm button text |
| CancelText | string | "Cancel" | Cancel button text |
| ConfirmCommand | ICommand | null | Confirm action command |
| CancelCommand | ICommand | null | Cancel action command |
| IsOpen | bool | false | Dialog visibility |

**Lifecycle:**
- Constructor wires button click handlers via `WireUpButtonHandlers()`
- `Unloaded` event unwires handlers via `UnwireButtonHandlers()` to prevent memory leaks

---

### SmartListView

**Namespace:** `SmartWorkz.Mobile.Components`

**Base Class:** ContentView

**BindableProperties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| ItemsSource | IEnumerable | null | Items to display |
| SelectionCommand | ICommand | null | Item selection command |
| ItemTemplate | DataTemplate | null | Custom item template |

**Internal Structure:**
- Uses CollectionView (not ListView) for virtualization and performance
- Single selection mode with SelectionChangedCommand binding

---

### LoadingIndicator

**Namespace:** `SmartWorkz.Mobile.Components`

**Base Class:** ContentView

**BindableProperties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| IsLoading | bool | false | Show/hide spinner |
| Message | string | "Loading..." | Status message |

**Internal Properties:**
- ActivityIndicator with Color = #007AFF (iOS Primary Blue)
- Label for message text

---

## Integration Notes

### Cross-References

- **[Template Engine](./13-template-engine.md)** — Use for dynamic form labels and validation messages in templates
- **[Cache Attribute](./12-cache-attribute.md)** — Cache picker ItemsSource and list data for performance
- **[MAUI Patterns](./maui-patterns.md)** — Standard MAUI conventions used in all components
- **[Data Binding Guide](./data-binding-guide.md)** — Two-way binding examples for ValidatedEntry and SmartListView

### MAUI Data Binding Integration

All components use standard MAUI BindableProperty pattern:

```xaml
xmlns:local="clr-namespace:SmartWorkz.Mobile.Components"

<local:ValidatedEntry
    Label="Name"
    Text="{Binding UserName, Mode=TwoWay}"
    Validator="{StaticResource RequiredValidator}" />

<local:CustomButton
    Text="Submit"
    Command="{Binding SubmitCommand}"
    CommandParameter="{Binding .}" />
```

### Command Parameter Passing

For SmartListView and AlertDialog, pass selected items or context to commands:

```xaml
<!-- SmartListView passes selected item to command -->
<local:SmartListView
    ItemsSource="{Binding Items}"
    SelectionCommand="{Binding ItemSelectedCommand}" />

<!-- AlertDialog passes true/false to confirm/cancel commands -->
<local:AlertDialog
    ConfirmCommand="{Binding DeleteConfirmCommand}"
    CancelCommand="{Binding DeleteCancelCommand}" />
```

---

## Troubleshooting

### Issue 1: SmartListView Not Updating When Items Change

**Problem:** You add items to your collection, but the SmartListView doesn't show the new items.

**Root Cause:** SmartListView uses `CollectionView` internally, which requires `ObservableCollection<T>` to detect changes. Regular `List<T>` won't notify the view of updates.

**Solution:** Use `ObservableCollection<T>` instead of `List<T>`:

```csharp
// WRONG - List won't notify SmartListView of changes:
public List<Product> Products { get; set; } = new();
Products.Add(newProduct); // SmartListView won't update!

// CORRECT - ObservableCollection notifies automatically:
public ObservableCollection<Product> Products { get; set; } = new();
Products.Add(newProduct); // SmartListView updates immediately
```

### Issue 2: ValidatedEntry Text Updates Trigger Validator Twice or Hang

**Problem:** Setting Text from your ViewModel causes validator to run, which attempts to modify Text again, creating a loop.

**Root Cause:** The `_isUpdating` re-entrancy guard prevents infinite loops, but it also prevents intentional Text updates from executing the validator if triggered from within the validator itself.

**Solution:** Don't modify Text from within your Validator. Use separate properties:

```csharp
// WRONG - Don't update Text inside Validator:
private Func<string, (bool, string)> BadValidator = value =>
{
    entry.Text = value.Trim(); // Re-entrancy guard blocks this!
    return (true, "");
};

// CORRECT - Just validate:
private Func<string, (bool, string)> GoodValidator = value =>
{
    var trimmed = value.Trim();
    var isValid = trimmed.Length >= 3;
    return (isValid, isValid ? "" : "Minimum 3 characters");
};

// If you need to normalize, do it in Text binding or binding converter:
public string Email
{
    get => _email;
    set => _email = value?.Trim().ToLower() ?? "";
}
```

### Issue 3: AlertDialog Event Handlers Cause Memory Leaks

**Problem:** Your application uses more memory over time, and dialogs appear to accumulate in memory.

**Root Cause:** Button event handlers aren't properly unwired when the component is destroyed, keeping references alive.

**Solution:** AlertDialog automatically wires/unwires handlers:

```csharp
public AlertDialog()
{
    InitializeComponent();
    WireUpButtonHandlers(); // Wires on creation
    this.Unloaded += (sender, args) => UnwireButtonHandlers(); // Unwires on destruction
}
```

If you create AlertDialog components dynamically, ensure they're properly removed from the visual tree:

```xaml
<!-- CORRECT - Component removed when binding changes -->
<local:AlertDialog
    IsVisible="{Binding ShowDialog}"
    Title="Confirm"
    ConfirmCommand="{Binding ConfirmCommand}" />

<!-- WRONG - Component remains in tree even when hidden -->
<local:AlertDialog
    Opacity="{Binding ShowDialog ? 1 : 0}"
    Title="Confirm"
    ConfirmCommand="{Binding ConfirmCommand}" />
```

### Issue 4: Custom Button Styles and Color Overrides Not Working

**Problem:** You set BackgroundColor in XAML, but the button still uses the ButtonType color.

**Root Cause:** The `OnButtonTypeChanged` callback runs after XAML parsing and overwrites any inline color settings.

**Solution:** Don't set BackgroundColor/TextColor in XAML if using ButtonType. Create a custom button subclass or use a data trigger:

```xaml
<!-- WRONG - ButtonType style overwrites BackgroundColor -->
<custombutton:CustomButton
    Text="Custom"
    ButtonType="Primary"
    BackgroundColor="Purple" /> <!-- Ignored! -->

<!-- CORRECT - Use ButtonType only -->
<custombutton:CustomButton
    Text="Custom"
    ButtonType="Primary" />

<!-- Or create custom button subclass without ButtonType -->
<Button
    Text="Custom"
    BackgroundColor="Purple"
    TextColor="White" />
```

If you need custom colors for a ButtonType, create a subclass:

```csharp
public class CustomColorButton : CustomButton
{
    public static readonly BindableProperty CustomBackgroundColorProperty =
        BindableProperty.Create(
            nameof(CustomBackgroundColor),
            typeof(Color),
            typeof(CustomColorButton),
            defaultValue: null,
            propertyChanged: OnCustomColorChanged);
    
    public Color CustomBackgroundColor
    {
        get => (Color)GetValue(CustomBackgroundColorProperty);
        set => SetValue(CustomBackgroundColorProperty, value);
    }
    
    private static void OnCustomColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CustomColorButton button && newValue is Color color)
        {
            button.BackgroundColor = color;
        }
    }
}
```

---

## Best Practices

1. **Always use ObservableCollection<T> with SmartListView** — Regular lists won't notify of changes
2. **Validate in the Validator function only** — Don't modify Text property from within the validator
3. **Unwire AlertDialog handlers** — Component does this automatically, but verify in custom scenarios
4. **Use ButtonType for consistency** — Don't override BackgroundColor/TextColor directly
5. **Test on both iOS and Android** — Components use iOS HIG colors; Android may need theme customization
6. **Cache picker data** — Use [Cache Attribute](./12-cache-attribute.md) for large ItemsSource collections
7. **Monitor LoadingIndicator lifecycle** — Ensure IsLoading is reset even if async operations fail

