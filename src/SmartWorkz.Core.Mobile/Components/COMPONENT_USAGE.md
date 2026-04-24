# SmartWorkz Mobile Component Library

Complete documentation and usage examples for the SmartWorkz Mobile component library. All components are styled consistently and fully data-bindable for MVVM applications.

## Components Overview

The component library includes 6 production-ready components:

1. **CustomButton** - Styled button with multiple variants
2. **ValidatedEntry** - Text input with validation support
3. **CustomPicker** - Dropdown selector with styling
4. **SmartListView** - Modern list component
5. **LoadingIndicator** - Loading spinner with message
6. **AlertDialog** - Modal alert dialog

## Component Setup

### Installation

Add the component library to your MAUI app in `MauiProgram.cs`:

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureEssentials()
            .AddSmartWorkzComponentLibrary();
        
        return builder.Build();
    }
}
```

Make sure the `SmartWorkz.Mobile.Components` namespace is imported in your XAML files:

```xml
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:components="clr-namespace:SmartWorkz.Mobile.Components">
    <!-- Your XAML content here -->
</ContentPage>
```

## Component Details

### 1. CustomButton

A styled button component with support for multiple visual variants.

#### Features
- **Text Property**: Button display text
- **Command Property**: ICommand for click handling
- **ButtonType Property**: Enum with 4 variants (Primary, Secondary, Danger, Success)
- **Automatic Styling**: Colors and text colors change based on button type

#### Style Specifications
- **Padding**: 16 horizontal, 12 vertical
- **CornerRadius**: 8
- **FontSize**: 16
- **FontAttributes**: Bold
- **Color Variants**:
  - Primary: Blue (#007AFF) background, white text
  - Secondary: Light gray (#E8E8E8) background, black text
  - Danger: Red (#FF3B30) background, white text
  - Success: Green (#34C759) background, white text

#### XAML Usage Example

```xml
<!-- Primary Button (default) -->
<components:CustomButton
    Text="Submit"
    Command="{Binding SubmitCommand}"
    ButtonType="Primary" />

<!-- Secondary Button -->
<components:CustomButton
    Text="Cancel"
    Command="{Binding CancelCommand}"
    ButtonType="Secondary" />

<!-- Danger Button -->
<components:CustomButton
    Text="Delete"
    Command="{Binding DeleteCommand}"
    ButtonType="Danger" />

<!-- Success Button -->
<components:CustomButton
    Text="Save"
    Command="{Binding SaveCommand}"
    ButtonType="Success" />
```

#### C# Code-Behind Example

```csharp
public partial class MyPage : ContentPage
{
    public MyPage()
    {
        InitializeComponent();
        BindingContext = new MyViewModel();
    }
}

public class MyViewModel : INotifyPropertyChanged
{
    public ICommand SubmitCommand { get; }

    public MyViewModel()
    {
        SubmitCommand = new Command(OnSubmit);
    }

    private void OnSubmit()
    {
        // Handle submit
    }

    public event PropertyChangedEventHandler PropertyChanged;
}
```

---

### 2. ValidatedEntry

An enhanced entry component with built-in validation and error display.

#### Features
- **Label Property**: Label text displayed above the entry
- **Text Property**: The input text (two-way bindable)
- **Placeholder Property**: Placeholder text shown when empty
- **ErrorText Property**: Error message displayed below
- **HasError Property**: Boolean flag indicating error state
- **KeyboardType Property**: Keyboard type (Default, Email, Numeric, etc.)
- **Validator Property**: Validation function Func<string, (bool, string)>
- **Auto-Validation**: Validates on text change if validator is set

#### Styling
- **Border Color**: #D3D3D3 normal, #FF3B30 on error
- **CornerRadius**: 6
- **Padding**: 12 horizontal, 8 vertical
- **FontSize**: 14
- **Label FontSize**: 14 (Bold)
- **Error Label FontSize**: 12

#### XAML Usage Example

```xml
<!-- Email Entry with Validation -->
<components:ValidatedEntry
    Label="Email Address"
    Text="{Binding Email, Mode=TwoWay}"
    Placeholder="Enter your email"
    KeyboardType="Email"
    Validator="{Binding EmailValidator}"
    HasError="{Binding EmailHasError}"
    ErrorText="{Binding EmailError}" />

<!-- Password Entry -->
<components:ValidatedEntry
    Label="Password"
    Text="{Binding Password, Mode=TwoWay}"
    Placeholder="Enter password"
    KeyboardType="Default"
    Validator="{Binding PasswordValidator}"
    HasError="{Binding PasswordHasError}"
    ErrorText="{Binding PasswordError}" />
```

#### C# Code-Behind Example

```csharp
public class LoginViewModel : INotifyPropertyChanged
{
    private string _email = "";
    private string _password = "";

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); }
    }

    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    // Validator function that returns (isValid, errorMessage)
    public Func<string, (bool, string)> EmailValidator { get; }

    public LoginViewModel()
    {
        EmailValidator = ValidateEmail;
    }

    private (bool, string) ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email is required");

        if (!email.Contains("@"))
            return (false, "Invalid email format");

        return (true, "");
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

---

### 3. CustomPicker

A styled dropdown/picker component with data binding support.

#### Features
- **Label Property**: Label text displayed above picker
- **SelectedItem Property**: Currently selected item (two-way bindable)
- **ItemsSource Property**: IEnumerable collection of items
- **DisplayMemberPath Property**: Property path for display text
- **SelectedValuePath Property**: Property path for item identification
- **Consistent Styling**: Rounded corners and borders

#### Styling
- **Border Color**: #D3D3D3
- **CornerRadius**: 6
- **Padding**: 12 horizontal, 8 vertical
- **FontSize**: 14

#### XAML Usage Example

```xml
<!-- Simple String Items -->
<components:CustomPicker
    Label="Select Category"
    ItemsSource="{Binding Categories}"
    SelectedItem="{Binding SelectedCategory, Mode=TwoWay}" />

<!-- Complex Objects with Display Binding -->
<components:CustomPicker
    Label="Select Product"
    ItemsSource="{Binding Products}"
    SelectedItem="{Binding SelectedProduct, Mode=TwoWay}"
    DisplayMemberPath="Name"
    SelectedValuePath="Id" />
```

#### C# Code-Behind Example

```csharp
public class ProductViewModel : INotifyPropertyChanged
{
    private List<string> _categories;
    private string _selectedCategory;

    public List<string> Categories
    {
        get => _categories;
        set { _categories = value; OnPropertyChanged(); }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set { _selectedCategory = value; OnPropertyChanged(); }
    }

    public ProductViewModel()
    {
        Categories = new List<string> { "Electronics", "Clothing", "Books" };
    }

    public event PropertyChangedEventHandler PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string name = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

---

### 4. SmartListView

A modern list component using CollectionView for better performance.

#### Features
- **ItemsSource Property**: IEnumerable collection
- **SelectionCommand Property**: ICommand executed on item selection
- **Single Selection Mode**: Only one item can be selected at a time
- **Modern Control**: Uses CollectionView (better than ListView)
- **Styled Items**: Each item wrapped in a Frame with shadow and rounded corners

#### Styling
- **Item CornerRadius**: 8
- **Item HasShadow**: True
- **Item Margin**: 0 top/bottom, 8 left/right
- **Item Padding**: 16
- **FontSize**: 14

#### XAML Usage Example

```xml
<!-- Basic List -->
<components:SmartListView
    ItemsSource="{Binding Items}"
    SelectionCommand="{Binding SelectItemCommand}" />

<!-- List in a Page Layout -->
<VerticalStackLayout>
    <Label Text="Select an item:" FontSize="16" FontAttributes="Bold" />
    <components:SmartListView
        ItemsSource="{Binding Orders}"
        SelectionCommand="{Binding ViewOrderCommand}" />
</VerticalStackLayout>
```

#### C# Code-Behind Example

```csharp
public class OrdersViewModel : INotifyPropertyChanged
{
    private List<string> _items;
    public List<string> Items
    {
        get => _items;
        set { _items = value; OnPropertyChanged(); }
    }

    public ICommand SelectItemCommand { get; }

    public OrdersViewModel()
    {
        Items = new List<string> { "Order 1", "Order 2", "Order 3" };
        SelectItemCommand = new Command<object>(OnItemSelected);
    }

    private void OnItemSelected(object item)
    {
        if (item is string selectedItem)
        {
            // Handle selection
            Debug.WriteLine($"Selected: {selectedItem}");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

---

### 5. LoadingIndicator

A loading spinner component with optional message display.

#### Features
- **IsLoading Property**: Boolean controlling visibility and spinner animation
- **Message Property**: Loading message text
- **ActivityIndicator**: Animated spinner
- **Auto-Hide**: Automatically hides when IsLoading is false
- **Centered Layout**: Vertically and horizontally centered

#### Styling
- **Spinner Color**: #007AFF (primary blue)
- **Scale**: 1.5x default size
- **Message FontSize**: 14
- **Spacing**: 12 between spinner and message

#### XAML Usage Example

```xml
<!-- Simple Loading Indicator -->
<components:LoadingIndicator
    IsLoading="{Binding IsLoading}"
    Message="Please wait..." />

<!-- In an Overlay Layout -->
<AbsoluteLayout>
    <ScrollView AbsoluteLayout.LayoutBounds="0,0,1,1" AbsoluteLayout.LayoutFlags="All">
        <!-- Your content here -->
    </ScrollView>
    <components:LoadingIndicator
        IsLoading="{Binding IsLoading}"
        Message="Loading data..."
        AbsoluteLayout.LayoutBounds="0,0,1,1"
        AbsoluteLayout.LayoutFlags="All" />
</AbsoluteLayout>
```

#### C# Code-Behind Example

```csharp
public class DataViewModel : INotifyPropertyChanged
{
    private bool _isLoading;
    private string _message = "Loading...";

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public string Message
    {
        get => _message;
        set { _message = value; OnPropertyChanged(); }
    }

    public ICommand LoadDataCommand { get; }

    public DataViewModel()
    {
        LoadDataCommand = new Command(OnLoadData);
    }

    private async void OnLoadData()
    {
        IsLoading = true;
        Message = "Fetching data...";
        
        try
        {
            await Task.Delay(2000); // Simulate network call
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

---

### 6. AlertDialog

A modal dialog component for alerts and confirmations.

#### Features
- **Title Property**: Dialog title
- **Message Property**: Dialog message
- **ConfirmText Property**: Confirm button label
- **CancelText Property**: Cancel button label
- **ConfirmCommand Property**: ICommand executed on confirm
- **CancelCommand Property**: ICommand executed on cancel
- **Modal Overlay**: Semi-transparent background
- **Centered Layout**: Dialog centered on screen

#### Styling
- **Backdrop Color**: Black with 50% opacity (#80000000)
- **Dialog Background**: White
- **CornerRadius**: 12
- **Title FontSize**: 18 (Bold)
- **Message FontSize**: 14
- **Button CornerRadius**: 6
- **Confirm Button Color**: #007AFF (blue)
- **Cancel Button Color**: #E8E8E8 (gray)

#### XAML Usage Example

```xml
<!-- Confirmation Dialog -->
<components:AlertDialog
    Title="Confirm Action"
    Message="Are you sure you want to delete this item?"
    ConfirmText="Delete"
    CancelText="Cancel"
    ConfirmCommand="{Binding DeleteCommand}"
    CancelCommand="{Binding CancelCommand}" />

<!-- Simple Alert -->
<components:AlertDialog
    Title="Success"
    Message="Your changes have been saved."
    ConfirmText="OK"
    CancelText="Dismiss"
    ConfirmCommand="{Binding AcknowledgeCommand}"
    CancelCommand="{Binding DismissCommand}" />
```

#### C# Code-Behind Example

```csharp
public class DialogViewModel : INotifyPropertyChanged
{
    public ICommand DeleteCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand AcknowledgeCommand { get; }

    public DialogViewModel()
    {
        DeleteCommand = new Command(OnDelete);
        CancelCommand = new Command(OnCancel);
        AcknowledgeCommand = new Command(OnAcknowledge);
    }

    private void OnDelete()
    {
        // Handle delete
        Debug.WriteLine("Item deleted");
    }

    private void OnCancel()
    {
        // Handle cancel
        Debug.WriteLine("Action cancelled");
    }

    private void OnAcknowledge()
    {
        // Handle acknowledgement
        Debug.WriteLine("Acknowledged");
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

---

## Complete Example: Login Form

Here's a complete example using multiple components together:

### XAML View

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="SmartWorkz.Sample.LoginPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:components="clr-namespace:SmartWorkz.Mobile.Components"
    BackgroundColor="White"
    Title="Login">
    <AbsoluteLayout>
        <ScrollView AbsoluteLayout.LayoutBounds="0,0,1,1" AbsoluteLayout.LayoutFlags="All">
            <VerticalStackLayout Padding="20" Spacing="16">
                <Label
                    FontSize="24"
                    FontAttributes="Bold"
                    Text="Sign In"
                    TextColor="#333333" />

                <components:ValidatedEntry
                    Label="Email"
                    Text="{Binding Email, Mode=TwoWay}"
                    Placeholder="Enter your email"
                    KeyboardType="Email"
                    Validator="{Binding EmailValidator}" />

                <components:ValidatedEntry
                    Label="Password"
                    Text="{Binding Password, Mode=TwoWay}"
                    Placeholder="Enter your password"
                    KeyboardType="Default"
                    Validator="{Binding PasswordValidator}" />

                <components:CustomButton
                    Text="Sign In"
                    Command="{Binding LoginCommand}"
                    ButtonType="Primary"
                    Margin="0,20,0,0" />

                <components:CustomButton
                    Text="Create Account"
                    Command="{Binding SignUpCommand}"
                    ButtonType="Secondary" />
            </VerticalStackLayout>
        </ScrollView>

        <components:LoadingIndicator
            IsLoading="{Binding IsLoading}"
            Message="Signing in..."
            AbsoluteLayout.LayoutBounds="0,0,1,1"
            AbsoluteLayout.LayoutFlags="All" />

        <components:AlertDialog
            IsVisible="{Binding ShowErrorDialog}"
            Title="Login Failed"
            Message="{Binding ErrorMessage}"
            ConfirmText="Try Again"
            ConfirmCommand="{Binding AcknowledgeErrorCommand}" />
    </AbsoluteLayout>
</ContentPage>
```

### C# ViewModel

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

public class LoginViewModel : INotifyPropertyChanged
{
    private string _email = "";
    private string _password = "";
    private bool _isLoading;
    private bool _showErrorDialog;
    private string _errorMessage = "";

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); }
    }

    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public bool ShowErrorDialog
    {
        get => _showErrorDialog;
        set { _showErrorDialog = value; OnPropertyChanged(); }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public Func<string, (bool, string)> EmailValidator { get; }
    public Func<string, (bool, string)> PasswordValidator { get; }
    public ICommand LoginCommand { get; }
    public ICommand SignUpCommand { get; }
    public ICommand AcknowledgeErrorCommand { get; }

    public LoginViewModel()
    {
        EmailValidator = ValidateEmail;
        PasswordValidator = ValidatePassword;
        LoginCommand = new Command(OnLogin);
        SignUpCommand = new Command(OnSignUp);
        AcknowledgeErrorCommand = new Command(OnAcknowledgeError);
    }

    private (bool, string) ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email is required");
        if (!email.Contains("@"))
            return (false, "Invalid email format");
        return (true, "");
    }

    private (bool, string) ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password is required");
        if (password.Length < 6)
            return (false, "Password must be at least 6 characters");
        return (true, "");
    }

    private async void OnLogin()
    {
        IsLoading = true;
        try
        {
            // Simulate API call
            await Task.Delay(2000);
            // Navigate to main page
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            ShowErrorDialog = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnSignUp()
    {
        // Navigate to signup page
    }

    private void OnAcknowledgeError()
    {
        ShowErrorDialog = false;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

---

## Color Palette Reference

- **Primary Blue**: #007AFF
- **Secondary Gray**: #E8E8E8
- **Danger Red**: #FF3B30
- **Success Green**: #34C759
- **Text Dark**: #333333
- **Text Medium**: #666666
- **Border Light**: #D3D3D3
- **Overlay Dark**: #80000000 (50% black)

## Complete Examples

### Example 1: Registration Form

A complete registration form using ValidatedEntry, CustomButton, CustomPicker, and checkbox functionality.

#### XAML View

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="SmartWorkz.Sample.RegistrationPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:components="clr-namespace:SmartWorkz.Mobile.Components"
    BackgroundColor="White"
    Title="Register">
    <AbsoluteLayout>
        <ScrollView AbsoluteLayout.LayoutBounds="0,0,1,1" AbsoluteLayout.LayoutFlags="All">
            <VerticalStackLayout Padding="20" Spacing="12">
                <Label
                    FontSize="28"
                    FontAttributes="Bold"
                    Text="Create Account"
                    TextColor="#333333" />
                
                <Label
                    FontSize="14"
                    Text="Join our community today"
                    TextColor="#666666" />

                <components:ValidatedEntry
                    Label="First Name"
                    Text="{Binding FirstName, Mode=TwoWay}"
                    Placeholder="John"
                    KeyboardType="Default"
                    Validator="{Binding FirstNameValidator}"
                    HasError="{Binding FirstNameHasError}"
                    ErrorText="{Binding FirstNameError}" />

                <components:ValidatedEntry
                    Label="Last Name"
                    Text="{Binding LastName, Mode=TwoWay}"
                    Placeholder="Doe"
                    KeyboardType="Default"
                    Validator="{Binding LastNameValidator}"
                    HasError="{Binding LastNameHasError}"
                    ErrorText="{Binding LastNameError}" />

                <components:ValidatedEntry
                    Label="Email Address"
                    Text="{Binding Email, Mode=TwoWay}"
                    Placeholder="john@example.com"
                    KeyboardType="Email"
                    Validator="{Binding EmailValidator}"
                    HasError="{Binding EmailHasError}"
                    ErrorText="{Binding EmailError}" />

                <components:CustomPicker
                    Label="Country"
                    ItemsSource="{Binding Countries}"
                    SelectedItem="{Binding SelectedCountry, Mode=TwoWay}" />

                <components:ValidatedEntry
                    Label="Password"
                    Text="{Binding Password, Mode=TwoWay}"
                    Placeholder="Minimum 8 characters"
                    KeyboardType="Default"
                    Validator="{Binding PasswordValidator}"
                    HasError="{Binding PasswordHasError}"
                    ErrorText="{Binding PasswordError}" />

                <CheckBox
                    IsChecked="{Binding AgreeToTerms, Mode=TwoWay}"
                    Margin="0,8,0,0" />
                <Label
                    Text="I agree to the Terms of Service"
                    FontSize="12"
                    Margin="32,0,0,16"
                    TextColor="#666666" />

                <components:CustomButton
                    Text="Create Account"
                    Command="{Binding RegisterCommand}"
                    ButtonType="Primary"
                    Margin="0,20,0,0" />

                <components:CustomButton
                    Text="Already have an account? Sign In"
                    Command="{Binding NavigateToLoginCommand}"
                    ButtonType="Secondary" />
            </VerticalStackLayout>
        </ScrollView>

        <components:LoadingIndicator
            IsLoading="{Binding IsLoading}"
            Message="Creating your account..."
            AbsoluteLayout.LayoutBounds="0,0,1,1"
            AbsoluteLayout.LayoutFlags="All" />

        <components:AlertDialog
            IsVisible="{Binding ShowErrorDialog}"
            Title="Registration Failed"
            Message="{Binding ErrorMessage}"
            ConfirmText="OK"
            ConfirmCommand="{Binding DismissErrorCommand}" />
    </AbsoluteLayout>
</ContentPage>
```

#### C# ViewModel

```csharp
public class RegistrationViewModel : INotifyPropertyChanged
{
    private string _firstName = "";
    private string _lastName = "";
    private string _email = "";
    private string _password = "";
    private string _selectedCountry;
    private bool _agreeToTerms;
    private bool _isLoading;
    private bool _showErrorDialog;
    private string _errorMessage = "";

    private string _firstNameError = "";
    private string _lastNameError = "";
    private string _emailError = "";
    private string _passwordError = "";
    private bool _firstNameHasError;
    private bool _lastNameHasError;
    private bool _emailHasError;
    private bool _passwordHasError;

    public string FirstName
    {
        get => _firstName;
        set { _firstName = value; OnPropertyChanged(); }
    }

    public string LastName
    {
        get => _lastName;
        set { _lastName = value; OnPropertyChanged(); }
    }

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); }
    }

    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    public string SelectedCountry
    {
        get => _selectedCountry;
        set { _selectedCountry = value; OnPropertyChanged(); }
    }

    public bool AgreeToTerms
    {
        get => _agreeToTerms;
        set { _agreeToTerms = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public bool ShowErrorDialog
    {
        get => _showErrorDialog;
        set { _showErrorDialog = value; OnPropertyChanged(); }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public List<string> Countries { get; } = new()
    {
        "United States", "Canada", "United Kingdom", "Australia", "Germany", "France", "Japan"
    };

    public ICommand RegisterCommand { get; }
    public ICommand NavigateToLoginCommand { get; }
    public ICommand DismissErrorCommand { get; }

    public Func<string, (bool, string)> FirstNameValidator { get; }
    public Func<string, (bool, string)> LastNameValidator { get; }
    public Func<string, (bool, string)> EmailValidator { get; }
    public Func<string, (bool, string)> PasswordValidator { get; }

    public bool FirstNameHasError
    {
        get => _firstNameHasError;
        set { _firstNameHasError = value; OnPropertyChanged(); }
    }

    public string FirstNameError
    {
        get => _firstNameError;
        set { _firstNameError = value; OnPropertyChanged(); }
    }

    // Similar properties for other fields...

    public RegistrationViewModel()
    {
        FirstNameValidator = ValidateFirstName;
        LastNameValidator = ValidateLastName;
        EmailValidator = ValidateEmail;
        PasswordValidator = ValidatePassword;

        RegisterCommand = new Command(OnRegister);
        NavigateToLoginCommand = new Command(OnNavigateToLogin);
        DismissErrorCommand = new Command(OnDismissError);
    }

    private (bool, string) ValidateFirstName(string name)
    {
        FirstNameHasError = false;
        FirstNameError = "";

        if (string.IsNullOrWhiteSpace(name))
        {
            FirstNameHasError = true;
            FirstNameError = "First name is required";
            return (false, "Required");
        }

        if (name.Length < 2)
        {
            FirstNameHasError = true;
            FirstNameError = "Minimum 2 characters";
            return (false, "Too short");
        }

        return (true, "");
    }

    private (bool, string) ValidateLastName(string name)
    {
        LastNameHasError = false;
        LastNameError = "";

        if (string.IsNullOrWhiteSpace(name))
        {
            LastNameHasError = true;
            LastNameError = "Last name is required";
            return (false, "Required");
        }

        return (true, "");
    }

    private (bool, string) ValidateEmail(string email)
    {
        EmailHasError = false;
        EmailError = "";

        if (string.IsNullOrWhiteSpace(email))
        {
            EmailHasError = true;
            EmailError = "Email is required";
            return (false, "Required");
        }

        if (!email.Contains("@") || !email.Contains("."))
        {
            EmailHasError = true;
            EmailError = "Invalid email format";
            return (false, "Invalid");
        }

        return (true, "");
    }

    private (bool, string) ValidatePassword(string password)
    {
        PasswordHasError = false;
        PasswordError = "";

        if (string.IsNullOrWhiteSpace(password))
        {
            PasswordHasError = true;
            PasswordError = "Password is required";
            return (false, "Required");
        }

        if (password.Length < 8)
        {
            PasswordHasError = true;
            PasswordError = "Minimum 8 characters";
            return (false, "Too short");
        }

        return (true, "");
    }

    private async void OnRegister()
    {
        if (!ValidateAllFields())
        {
            ErrorMessage = "Please fix the errors above";
            ShowErrorDialog = true;
            return;
        }

        IsLoading = true;
        try
        {
            // Call API to register
            await Task.Delay(2000); // Simulate network call
            await Shell.Current.GoToAsync("login");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            ShowErrorDialog = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool ValidateAllFields()
    {
        ValidateFirstName(FirstName);
        ValidateLastName(LastName);
        ValidateEmail(Email);
        ValidatePassword(Password);

        return !FirstNameHasError && !LastNameHasError && !EmailHasError && !PasswordHasError;
    }

    private async void OnNavigateToLogin()
    {
        await Shell.Current.GoToAsync("login");
    }

    private void OnDismissError()
    {
        ShowErrorDialog = false;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### Example 2: Product List with SmartListView

Display a list of products with selection handling.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="SmartWorkz.Sample.ProductsPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:components="clr-namespace:SmartWorkz.Mobile.Components"
    Title="Products">
    <VerticalStackLayout Padding="12" Spacing="8">
        <Label
            FontSize="24"
            FontAttributes="Bold"
            Text="Our Products"
            TextColor="#333333" />

        <components:SmartListView
            ItemsSource="{Binding Products}"
            SelectionCommand="{Binding SelectProductCommand}" />

        <components:CustomButton
            Text="Add to Cart"
            Command="{Binding AddToCartCommand}"
            ButtonType="Primary"
            IsEnabled="{Binding HasSelectedProduct}" />

        <components:AlertDialog
            IsVisible="{Binding ShowProductDetails}"
            Title="{Binding SelectedProduct.Name}"
            Message="{Binding SelectedProduct.Description}"
            ConfirmText="Add to Cart"
            CancelText="Close"
            ConfirmCommand="{Binding ConfirmAddCommand}"
            CancelCommand="{Binding CancelCommand}" />
    </VerticalStackLayout>
</ContentPage>
```

### Example 3: Settings Page

Settings page with multiple component types.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="SmartWorkz.Sample.SettingsPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:components="clr-namespace:SmartWorkz.Mobile.Components"
    Title="Settings">
    <ScrollView>
        <VerticalStackLayout Padding="16" Spacing="16">
            <Label
                FontSize="20"
                FontAttributes="Bold"
                Text="Account Settings"
                TextColor="#333333" />

            <components:ValidatedEntry
                Label="Full Name"
                Text="{Binding FullName, Mode=TwoWay}"
                Placeholder="Enter your name"
                Validator="{Binding NameValidator}" />

            <components:ValidatedEntry
                Label="Phone Number"
                Text="{Binding PhoneNumber, Mode=TwoWay}"
                Placeholder="+1 (555) 000-0000"
                KeyboardType="Telephone"
                Validator="{Binding PhoneValidator}" />

            <components:CustomPicker
                Label="Language"
                ItemsSource="{Binding Languages}"
                SelectedItem="{Binding SelectedLanguage, Mode=TwoWay}" />

            <Label
                FontSize="16"
                FontAttributes="Bold"
                Text="Notifications"
                TextColor="#333333"
                Margin="0,16,0,8" />

            <CheckBox
                IsChecked="{Binding EmailNotifications, Mode=TwoWay}"
                Margin="0,4,0,0" />
            <Label
                Text="Email Notifications"
                FontSize="12"
                Margin="32,0,0,12"
                TextColor="#666666" />

            <CheckBox
                IsChecked="{Binding PushNotifications, Mode=TwoWay}"
                Margin="0,4,0,0" />
            <Label
                Text="Push Notifications"
                FontSize="12"
                Margin="32,0,0,12"
                TextColor="#666666" />

            <components:CustomButton
                Text="Save Changes"
                Command="{Binding SaveSettingsCommand}"
                ButtonType="Primary"
                Margin="0,20,0,0" />

            <components:CustomButton
                Text="Sign Out"
                Command="{Binding SignOutCommand}"
                ButtonType="Danger" />

            <components:LoadingIndicator
                IsLoading="{Binding IsLoading}"
                Message="Saving..." />
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

## Best Practices

### 1. MVVM Pattern
Always use proper MVVM with ViewModels and data binding:

```csharp
// Good: Proper MVVM
BindingContext = new MyViewModel();

// Bad: Code-behind event handlers
Button.Clicked += OnButtonClicked;
```

### 2. INotifyPropertyChanged Implementation
Properly implement property notifications for binding:

```csharp
private string _name;
public string Name
{
    get => _name;
    set { _name = value; OnPropertyChanged(); }
}
```

### 3. Validation on Every Input
Use validators for all user input:

```csharp
<components:ValidatedEntry
    Text="{Binding Email, Mode=TwoWay}"
    Validator="{Binding EmailValidator}"
    HasError="{Binding EmailHasError}"
    ErrorText="{Binding EmailError}" />
```

### 4. Loading States
Show loading indicators during async operations:

```csharp
IsLoading = true;
try
{
    await ApiService.SaveAsync(data);
}
finally
{
    IsLoading = false;
}
```

### 5. Error Messages
Provide clear, user-friendly error messages:

```csharp
private (bool, string) ValidateEmail(string email)
{
    if (string.IsNullOrWhiteSpace(email))
        return (false, "Email is required");

    if (!email.Contains("@"))
        return (false, "Invalid email format");

    return (true, "");
}
```

### 6. Keyboard Types
Use appropriate keyboard types for better UX:

```xml
<components:ValidatedEntry KeyboardType="Email" />
<components:ValidatedEntry KeyboardType="Telephone" />
<components:ValidatedEntry KeyboardType="Numeric" />
```

### 7. Responsive Layout
Test on multiple device sizes:

```xml
<VerticalStackLayout Padding="16" Spacing="12">
    <!-- Components automatically adapt -->
</VerticalStackLayout>
```

### 8. Command Execution
Disable buttons during long operations:

```csharp
RegisterCommand = new Command(OnRegister, CanRegister);
```

## Troubleshooting

### Component Not Rendering
**Issue:** Component doesn't appear on screen.

**Solutions:**
- Ensure `AddSmartWorkzComponentLibrary()` is called in `MauiProgram.cs`
- Verify namespace is imported in XAML: `xmlns:components="clr-namespace:SmartWorkz.Mobile.Components"`
- Check that `BindingContext` is properly set in code-behind
- Look for errors in Visual Studio output window

### Validation Not Working
**Issue:** Validator function not being called or not displaying errors.

**Solutions:**
- Ensure `Validator` property is bound to function (e.g., `{Binding EmailValidator}`)
- Check that validator returns `(bool, string)` tuple correctly
- Verify `OnPropertyChanged()` is called when value changes
- Ensure `HasError` and `ErrorText` properties are bound

### Styling Issues
**Issue:** Colors not applying or components look wrong.

**Solutions:**
- Verify hex color codes are valid (e.g., `#007AFF`)
- Ensure padding/margin values are numeric
- Check `CornerRadius` is supported on platform
- Test on actual device, not just emulator

### Command Not Executing
**Issue:** Button commands not executing when clicked.

**Solutions:**
- Verify command is properly bound: `Command="{Binding MyCommand}"`
- Check command is initialized in ViewModel constructor
- Ensure command has proper `CanExecute` implementation
- Look for exceptions in debug output

### Binding Not Working
**Issue:** Property changes don't update UI.

**Solutions:**
- Implement `INotifyPropertyChanged` in ViewModel
- Call `OnPropertyChanged()` when property changes
- Use `Mode=TwoWay` for two-way binding
- Verify property names match exactly in XAML

### Memory Leaks
**Issue:** Application slows down over time.

**Solutions:**
- Unsubscribe from events in `OnDisappearing()`
- Dispose resources properly in ViewModels
- Avoid keeping references to views in ViewModels
- Use weak event patterns for long-lived events

## Styling Customization

### Color Scheme Override
Create custom styles in `App.xaml`:

```xml
<Color x:Key="PrimaryColor">#007AFF</Color>
<Color x:Key="DangerColor">#FF3B30</Color>
<Color x:Key="SuccessColor">#34C759</Color>

<Style TargetType="components:CustomButton">
    <Setter Property="BackgroundColor" Value="{StaticResource PrimaryColor}" />
</Style>
```

### Font Customization
Apply custom fonts:

```xml
<Style TargetType="Label">
    <Setter Property="FontFamily" Value="CustomFont" />
    <Setter Property="FontSize" Value="14" />
</Style>
```

## Platform-Specific Notes

### iOS
- Safe area handling for notched devices
- Native keyboard handling for input types
- Haptic feedback support

### Android
- Material Design compliance
- Keyboard handling variations
- Navigation bar considerations

## Support and Contributions

For issues or feature requests, refer to the SmartWorkz project documentation.

## Performance Tips

1. Use `CollectionView` with virtual scrolling for large lists
2. Implement data virtualization for SmartListView
3. Cache validators and commands
4. Use async/await properly for network calls
5. Implement proper lifecycle management in ViewModels
