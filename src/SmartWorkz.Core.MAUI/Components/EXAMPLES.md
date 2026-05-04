# SmartWorkz Mobile Component Examples

Collection of complete, production-ready examples demonstrating how to combine mobile components for common app scenarios.

## Table of Contents

1. [Login Page](#login-page)
2. [User Profile Screen](#user-profile-screen)
3. [Product Catalog with Details](#product-catalog-with-details)
4. [Form with Validation](#form-with-validation)
5. [Settings Panel](#settings-panel)
6. [Shopping Cart](#shopping-cart)

## Login Page

Complete login screen using ValidatedEntry, CustomButton, LoadingIndicator, and AlertDialog.

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
            <VerticalStackLayout Padding="24" Spacing="20">
                <!-- Logo/Header -->
                <VerticalStackLayout HorizontalOptions="Center" Spacing="8">
                    <Label
                        FontSize="32"
                        FontAttributes="Bold"
                        Text="SmartWorkz"
                        TextColor="#007AFF"
                        HorizontalTextAlignment="Center" />
                    <Label
                        FontSize="14"
                        Text="Your App"
                        TextColor="#666666"
                        HorizontalTextAlignment="Center" />
                </VerticalStackLayout>

                <BoxView HeightRequest="1" Color="#E8E8E8" />

                <!-- Email Input -->
                <components:ValidatedEntry
                    Label="Email Address"
                    Text="{Binding Email, Mode=TwoWay}"
                    Placeholder="user@example.com"
                    KeyboardType="Email"
                    Validator="{Binding EmailValidator}"
                    HasError="{Binding EmailHasError}"
                    ErrorText="{Binding EmailError}" />

                <!-- Password Input -->
                <components:ValidatedEntry
                    Label="Password"
                    Text="{Binding Password, Mode=TwoWay}"
                    Placeholder="Enter your password"
                    KeyboardType="Default"
                    Validator="{Binding PasswordValidator}"
                    HasError="{Binding PasswordHasError}"
                    ErrorText="{Binding PasswordError}" />

                <!-- Forgot Password Link -->
                <HorizontalStackLayout HorizontalOptions="End" Margin="0,0,0,20">
                    <Label
                        Text="Forgot password?"
                        TextColor="#007AFF"
                        FontSize="12"
                        GestureRecognizers:GestureRecognizer.Tapped="{Binding ForgotPasswordCommand}" />
                </HorizontalStackLayout>

                <!-- Sign In Button -->
                <components:CustomButton
                    Text="Sign In"
                    Command="{Binding SignInCommand}"
                    ButtonType="Primary" />

                <!-- Divider -->
                <Label
                    Text="OR"
                    TextColor="#999999"
                    FontSize="12"
                    HorizontalTextAlignment="Center"
                    Margin="0,8,0,8" />

                <!-- Sign Up Button -->
                <components:CustomButton
                    Text="Create New Account"
                    Command="{Binding SignUpCommand}"
                    ButtonType="Secondary" />

                <!-- Social Login (Optional) -->
                <HorizontalStackLayout HorizontalOptions="Center" Spacing="16" Margin="0,20,0,0">
                    <components:CustomButton
                        Text="Google"
                        Command="{Binding GoogleSignInCommand}"
                        ButtonType="Secondary"
                        WidthRequest="100" />
                    <components:CustomButton
                        Text="Apple"
                        Command="{Binding AppleSignInCommand}"
                        ButtonType="Secondary"
                        WidthRequest="100" />
                </HorizontalStackLayout>
            </VerticalStackLayout>
        </ScrollView>

        <!-- Loading Indicator -->
        <components:LoadingIndicator
            IsLoading="{Binding IsLoading}"
            Message="Signing in..."
            AbsoluteLayout.LayoutBounds="0,0,1,1"
            AbsoluteLayout.LayoutFlags="All" />

        <!-- Error Dialog -->
        <components:AlertDialog
            IsVisible="{Binding ShowErrorDialog}"
            Title="Login Failed"
            Message="{Binding ErrorMessage}"
            ConfirmText="Try Again"
            ConfirmCommand="{Binding DismissErrorCommand}" />
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
    private string _emailError = "";
    private string _passwordError = "";
    private bool _emailHasError;
    private bool _passwordHasError;

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

    public string EmailError
    {
        get => _emailError;
        set { _emailError = value; OnPropertyChanged(); }
    }

    public string PasswordError
    {
        get => _passwordError;
        set { _passwordError = value; OnPropertyChanged(); }
    }

    public bool EmailHasError
    {
        get => _emailHasError;
        set { _emailHasError = value; OnPropertyChanged(); }
    }

    public bool PasswordHasError
    {
        get => _passwordHasError;
        set { _passwordHasError = value; OnPropertyChanged(); }
    }

    public Func<string, (bool, string)> EmailValidator { get; }
    public Func<string, (bool, string)> PasswordValidator { get; }
    public ICommand SignInCommand { get; }
    public ICommand SignUpCommand { get; }
    public ICommand ForgotPasswordCommand { get; }
    public ICommand DismissErrorCommand { get; }
    public ICommand GoogleSignInCommand { get; }
    public ICommand AppleSignInCommand { get; }

    public LoginViewModel()
    {
        EmailValidator = ValidateEmail;
        PasswordValidator = ValidatePassword;
        SignInCommand = new Command(OnSignIn);
        SignUpCommand = new Command(OnSignUp);
        ForgotPasswordCommand = new Command(OnForgotPassword);
        DismissErrorCommand = new Command(OnDismissError);
        GoogleSignInCommand = new Command(OnGoogleSignIn);
        AppleSignInCommand = new Command(OnAppleSignIn);
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

        if (password.Length < 6)
        {
            PasswordHasError = true;
            PasswordError = "Password is too short";
            return (false, "Too short");
        }

        return (true, "");
    }

    private async void OnSignIn()
    {
        ValidateEmail(Email);
        ValidatePassword(Password);

        if (EmailHasError || PasswordHasError)
        {
            ErrorMessage = "Please fix the errors above";
            ShowErrorDialog = true;
            return;
        }

        IsLoading = true;
        try
        {
            // Simulate API call
            await Task.Delay(2000);
            
            // Navigate to home page
            await Shell.Current.GoToAsync("home");
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

    private async void OnSignUp()
    {
        await Shell.Current.GoToAsync("register");
    }

    private async void OnForgotPassword()
    {
        await Shell.Current.GoToAsync("reset-password");
    }

    private void OnDismissError()
    {
        ShowErrorDialog = false;
    }

    private async void OnGoogleSignIn()
    {
        IsLoading = true;
        try
        {
            // Implement Google Sign-In
            await Task.Delay(2000);
            await Shell.Current.GoToAsync("home");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async void OnAppleSignIn()
    {
        IsLoading = true;
        try
        {
            // Implement Apple Sign-In
            await Task.Delay(2000);
            await Shell.Current.GoToAsync("home");
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

## User Profile Screen

User profile display and editing with ValidatedEntry and CustomButton.

### XAML View

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="SmartWorkz.Sample.ProfilePage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:components="clr-namespace:SmartWorkz.Mobile.Components"
    Title="Profile">
    <VerticalStackLayout Padding="16" Spacing="16">
        <ScrollView>
            <VerticalStackLayout Spacing="16">
                <!-- Profile Header -->
                <Frame
                    CornerRadius="12"
                    HasShadow="True"
                    Padding="0"
                    BorderColor="#E8E8E8">
                    <Grid RowDefinitions="Auto,Auto" ColumnDefinitions="Auto,*" Padding="16" RowSpacing="12">
                        <!-- Avatar -->
                        <Frame
                            Grid.Row="0"
                            Grid.Column="0"
                            CornerRadius="50"
                            HasShadow="False"
                            Padding="0"
                            WidthRequest="80"
                            HeightRequest="80"
                            BorderColor="#E8E8E8">
                            <Image Source="profile_placeholder.png" Aspect="AspectFill" />
                        </Frame>

                        <!-- User Info -->
                        <VerticalStackLayout
                            Grid.Row="0"
                            Grid.Column="1"
                            Padding="12,0,0,0"
                            VerticalOptions="Center"
                            Spacing="4">
                            <Label
                                FontSize="18"
                                FontAttributes="Bold"
                                Text="{Binding FullName}"
                                TextColor="#333333" />
                            <Label
                                FontSize="12"
                                Text="{Binding Email}"
                                TextColor="#666666" />
                            <Label
                                FontSize="12"
                                Text="{Binding MemberSince}"
                                TextColor="#999999" />
                        </VerticalStackLayout>

                        <!-- Edit Avatar Button -->
                        <components:CustomButton
                            Grid.Row="1"
                            Grid.Column="0"
                            Grid.ColumnSpan="2"
                            Text="Change Avatar"
                            Command="{Binding ChangeAvatarCommand}"
                            ButtonType="Secondary" />
                    </Grid>
                </Frame>

                <!-- Profile Form -->
                <Label
                    FontSize="16"
                    FontAttributes="Bold"
                    Text="Personal Information"
                    TextColor="#333333" />

                <components:ValidatedEntry
                    Label="First Name"
                    Text="{Binding FirstName, Mode=TwoWay}"
                    Placeholder="John"
                    Validator="{Binding FirstNameValidator}" />

                <components:ValidatedEntry
                    Label="Last Name"
                    Text="{Binding LastName, Mode=TwoWay}"
                    Placeholder="Doe"
                    Validator="{Binding LastNameValidator}" />

                <components:ValidatedEntry
                    Label="Email Address"
                    Text="{Binding Email, Mode=TwoWay}"
                    Placeholder="john@example.com"
                    KeyboardType="Email"
                    Validator="{Binding EmailValidator}" />

                <components:ValidatedEntry
                    Label="Phone Number"
                    Text="{Binding PhoneNumber, Mode=TwoWay}"
                    Placeholder="+1 (555) 123-4567"
                    KeyboardType="Telephone"
                    Validator="{Binding PhoneValidator}" />

                <Label
                    FontSize="16"
                    FontAttributes="Bold"
                    Text="Security"
                    TextColor="#333333"
                    Margin="0,16,0,0" />

                <components:CustomButton
                    Text="Change Password"
                    Command="{Binding ChangePasswordCommand}"
                    ButtonType="Secondary" />

                <!-- Action Buttons -->
                <StackLayout Spacing="8" Margin="0,20,0,0">
                    <components:CustomButton
                        Text="Save Changes"
                        Command="{Binding SaveCommand}"
                        ButtonType="Primary" />

                    <components:CustomButton
                        Text="Sign Out"
                        Command="{Binding SignOutCommand}"
                        ButtonType="Danger" />
                </StackLayout>
            </VerticalStackLayout>
        </ScrollView>

        <!-- Loading Indicator -->
        <components:LoadingIndicator
            IsLoading="{Binding IsLoading}"
            Message="Updating profile..." />
    </VerticalStackLayout>
</ContentPage>
```

---

## Product Catalog with Details

SmartListView for product browsing with AlertDialog for product details.

### XAML View

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="SmartWorkz.Sample.ProductCatalogPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:components="clr-namespace:SmartWorkz.Mobile.Components"
    Title="Products">
    <AbsoluteLayout>
        <ScrollView AbsoluteLayout.LayoutBounds="0,0,1,1" AbsoluteLayout.LayoutFlags="All">
            <VerticalStackLayout Padding="12" Spacing="16">
                <Label
                    FontSize="24"
                    FontAttributes="Bold"
                    Text="Our Products"
                    TextColor="#333333" />

                <SearchBar
                    Placeholder="Search products..."
                    Text="{Binding SearchTerm, Mode=TwoWay}"
                    SearchCommand="{Binding SearchCommand}" />

                <components:CustomPicker
                    Label="Category"
                    ItemsSource="{Binding Categories}"
                    SelectedItem="{Binding SelectedCategory, Mode=TwoWay}" />

                <components:SmartListView
                    ItemsSource="{Binding FilteredProducts}"
                    SelectionCommand="{Binding SelectProductCommand}" />

                <components:CustomButton
                    Text="Add to Cart"
                    Command="{Binding AddToCartCommand}"
                    ButtonType="Primary"
                    IsEnabled="{Binding HasSelectedProduct}" />
            </VerticalStackLayout>
        </ScrollView>

        <!-- Product Details Dialog -->
        <components:AlertDialog
            IsVisible="{Binding ShowProductDetails}"
            Title="{Binding SelectedProduct.Name}"
            Message="{Binding SelectedProductDescription}"
            ConfirmText="Add to Cart"
            CancelText="Close"
            ConfirmCommand="{Binding ConfirmAddToCartCommand}"
            CancelCommand="{Binding CloseDetailsCommand}" />

        <!-- Loading Indicator -->
        <components:LoadingIndicator
            IsLoading="{Binding IsLoading}"
            Message="Loading products..."
            AbsoluteLayout.LayoutBounds="0,0,1,1"
            AbsoluteLayout.LayoutFlags="All" />
    </AbsoluteLayout>
</ContentPage>
```

---

## Form with Validation

Complete form using multiple ValidatedEntry components with real-time validation.

### XAML View

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
                    Text="Join us today"
                    TextColor="#666666" />

                <!-- First Name -->
                <components:ValidatedEntry
                    Label="First Name"
                    Text="{Binding FirstName, Mode=TwoWay}"
                    Placeholder="John"
                    Validator="{Binding FirstNameValidator}"
                    HasError="{Binding FirstNameHasError}"
                    ErrorText="{Binding FirstNameError}" />

                <!-- Last Name -->
                <components:ValidatedEntry
                    Label="Last Name"
                    Text="{Binding LastName, Mode=TwoWay}"
                    Placeholder="Doe"
                    Validator="{Binding LastNameValidator}"
                    HasError="{Binding LastNameHasError}"
                    ErrorText="{Binding LastNameError}" />

                <!-- Email -->
                <components:ValidatedEntry
                    Label="Email Address"
                    Text="{Binding Email, Mode=TwoWay}"
                    Placeholder="john@example.com"
                    KeyboardType="Email"
                    Validator="{Binding EmailValidator}"
                    HasError="{Binding EmailHasError}"
                    ErrorText="{Binding EmailError}" />

                <!-- Password -->
                <components:ValidatedEntry
                    Label="Password"
                    Text="{Binding Password, Mode=TwoWay}"
                    Placeholder="Minimum 8 characters"
                    KeyboardType="Default"
                    Validator="{Binding PasswordValidator}"
                    HasError="{Binding PasswordHasError}"
                    ErrorText="{Binding PasswordError}" />

                <!-- Confirm Password -->
                <components:ValidatedEntry
                    Label="Confirm Password"
                    Text="{Binding ConfirmPassword, Mode=TwoWay}"
                    Placeholder="Re-enter password"
                    KeyboardType="Default"
                    Validator="{Binding ConfirmPasswordValidator}"
                    HasError="{Binding ConfirmPasswordHasError}"
                    ErrorText="{Binding ConfirmPasswordError}" />

                <!-- Country -->
                <components:CustomPicker
                    Label="Country"
                    ItemsSource="{Binding Countries}"
                    SelectedItem="{Binding SelectedCountry, Mode=TwoWay}" />

                <!-- Terms Checkbox -->
                <CheckBox
                    IsChecked="{Binding AgreeToTerms, Mode=TwoWay}"
                    Margin="0,8,0,0" />
                <Label
                    Text="I agree to the Terms of Service and Privacy Policy"
                    FontSize="12"
                    Margin="32,0,0,16"
                    TextColor="#666666" />

                <!-- Submit Button -->
                <components:CustomButton
                    Text="Create Account"
                    Command="{Binding RegisterCommand}"
                    ButtonType="Primary"
                    Margin="0,20,0,0" />

                <!-- Sign In Link -->
                <Label
                    Text="Already have an account? Sign In"
                    TextColor="#007AFF"
                    FontSize="12"
                    HorizontalTextAlignment="Center"
                    Margin="0,12,0,0">
                    <Label.GestureRecognizers>
                        <TapGestureRecognizer Command="{Binding NavigateToLoginCommand}" />
                    </Label.GestureRecognizers>
                </Label>
            </VerticalStackLayout>
        </ScrollView>

        <!-- Loading -->
        <components:LoadingIndicator
            IsLoading="{Binding IsLoading}"
            Message="Creating account..."
            AbsoluteLayout.LayoutBounds="0,0,1,1"
            AbsoluteLayout.LayoutFlags="All" />

        <!-- Error Dialog -->
        <components:AlertDialog
            IsVisible="{Binding ShowErrorDialog}"
            Title="Registration Error"
            Message="{Binding ErrorMessage}"
            ConfirmText="OK"
            ConfirmCommand="{Binding DismissErrorCommand}" />
    </AbsoluteLayout>
</ContentPage>
```

---

## Settings Panel

Settings page with various component types.

### XAML View

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="SmartWorkz.Sample.SettingsPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:components="clr-namespace:SmartWorkz.Mobile.Components"
    Title="Settings">
    <ScrollView>
        <VerticalStackLayout Padding="16" Spacing="20">
            <!-- Account Section -->
            <VerticalStackLayout Spacing="8">
                <Label
                    FontSize="18"
                    FontAttributes="Bold"
                    Text="Account"
                    TextColor="#333333" />

                <components:ValidatedEntry
                    Label="Display Name"
                    Text="{Binding DisplayName, Mode=TwoWay}"
                    Validator="{Binding NameValidator}" />

                <components:CustomButton
                    Text="Change Password"
                    Command="{Binding ChangePasswordCommand}"
                    ButtonType="Secondary" />
            </VerticalStackLayout>

            <!-- Preferences Section -->
            <VerticalStackLayout Spacing="8">
                <Label
                    FontSize="18"
                    FontAttributes="Bold"
                    Text="Preferences"
                    TextColor="#333333" />

                <components:CustomPicker
                    Label="Language"
                    ItemsSource="{Binding Languages}"
                    SelectedItem="{Binding SelectedLanguage, Mode=TwoWay}" />

                <components:CustomPicker
                    Label="Theme"
                    ItemsSource="{Binding Themes}"
                    SelectedItem="{Binding SelectedTheme, Mode=TwoWay}" />
            </VerticalStackLayout>

            <!-- Notifications Section -->
            <VerticalStackLayout Spacing="8">
                <Label
                    FontSize="18"
                    FontAttributes="Bold"
                    Text="Notifications"
                    TextColor="#333333" />

                <Frame CornerRadius="6" BorderColor="#E8E8E8" Padding="12">
                    <VerticalStackLayout Spacing="12">
                        <HorizontalStackLayout Spacing="12">
                            <CheckBox IsChecked="{Binding PushNotificationsEnabled, Mode=TwoWay}" />
                            <Label
                                Text="Push Notifications"
                                VerticalOptions="Center"
                                FontSize="14"
                                TextColor="#333333" />
                        </HorizontalStackLayout>

                        <HorizontalStackLayout Spacing="12">
                            <CheckBox IsChecked="{Binding EmailNotificationsEnabled, Mode=TwoWay}" />
                            <Label
                                Text="Email Notifications"
                                VerticalOptions="Center"
                                FontSize="14"
                                TextColor="#333333" />
                        </HorizontalStackLayout>

                        <HorizontalStackLayout Spacing="12">
                            <CheckBox IsChecked="{Binding SMSNotificationsEnabled, Mode=TwoWay}" />
                            <Label
                                Text="SMS Notifications"
                                VerticalOptions="Center"
                                FontSize="14"
                                TextColor="#333333" />
                        </HorizontalStackLayout>
                    </VerticalStackLayout>
                </Frame>
            </VerticalStackLayout>

            <!-- Action Buttons -->
            <VerticalStackLayout Spacing="8" Margin="0,20,0,0">
                <components:CustomButton
                    Text="Save Settings"
                    Command="{Binding SaveSettingsCommand}"
                    ButtonType="Primary" />

                <components:CustomButton
                    Text="Sign Out"
                    Command="{Binding SignOutCommand}"
                    ButtonType="Danger" />
            </VerticalStackLayout>
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

---

## Shopping Cart

Cart view with SmartListView, quantity adjustment, and loading states.

### XAML View

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="SmartWorkz.Sample.CartPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:components="clr-namespace:SmartWorkz.Mobile.Components"
    Title="Shopping Cart">
    <VerticalStackLayout Padding="12" Spacing="12">
        <!-- Items List -->
        <Label
            FontSize="18"
            FontAttributes="Bold"
            Text="{Binding ItemCountText}"
            TextColor="#333333" />

        <components:SmartListView
            ItemsSource="{Binding CartItems}"
            SelectionCommand="{Binding SelectItemCommand}" />

        <!-- Summary -->
        <Frame CornerRadius="8" BorderColor="#E8E8E8" Padding="16">
            <VerticalStackLayout Spacing="8">
                <HorizontalStackLayout Spacing="12">
                    <Label Text="Subtotal:" FontSize="14" TextColor="#666666" />
                    <Label Text="{Binding SubtotalText}" FontSize="14" FontAttributes="Bold" />
                </HorizontalStackLayout>

                <HorizontalStackLayout Spacing="12">
                    <Label Text="Tax:" FontSize="14" TextColor="#666666" />
                    <Label Text="{Binding TaxText}" FontSize="14" FontAttributes="Bold" />
                </HorizontalStackLayout>

                <BoxView HeightRequest="1" Color="#E8E8E8" />

                <HorizontalStackLayout Spacing="12">
                    <Label Text="Total:" FontSize="16" FontAttributes="Bold" TextColor="#333333" />
                    <Label Text="{Binding TotalText}" FontSize="16" FontAttributes="Bold" TextColor="#007AFF" />
                </HorizontalStackLayout>
            </VerticalStackLayout>
        </Frame>

        <!-- Checkout Button -->
        <components:CustomButton
            Text="Proceed to Checkout"
            Command="{Binding CheckoutCommand}"
            ButtonType="Primary"
            IsEnabled="{Binding HasItems}" />

        <!-- Continue Shopping Button -->
        <components:CustomButton
            Text="Continue Shopping"
            Command="{Binding ContinueShoppingCommand}"
            ButtonType="Secondary" />

        <!-- Empty State -->
        <VerticalStackLayout
            HorizontalOptions="Center"
            VerticalOptions="CenterAndExpand"
            IsVisible="{Binding IsEmpty}"
            Spacing="12">
            <Label
                FontSize="24"
                Text="Your cart is empty"
                TextColor="#999999"
                HorizontalTextAlignment="Center" />
            <components:CustomButton
                Text="Start Shopping"
                Command="{Binding StartShoppingCommand}"
                ButtonType="Primary" />
        </VerticalStackLayout>

        <!-- Loading -->
        <components:LoadingIndicator
            IsLoading="{Binding IsLoading}"
            Message="Updating cart..." />
    </VerticalStackLayout>
</ContentPage>
```

---

These examples demonstrate real-world scenarios and can be adapted to your specific application needs. Each example combines multiple components in a cohesive, production-ready pattern.
