# Mobile MAUI Architecture & Core Components

## Overview

The Mobile MAUI project shares core infrastructure with Web and Admin projects while providing a native mobile experience for iOS, Android, macOS, and Windows.

## Architecture Layers

### 1. **Presentation Layer** (Mobile UI)

#### Pages
- **Auth Pages**
  - LoginPage - Email/password authentication
  - RegisterPage - User registration
  - ForgotPasswordPage - Password reset flow
  - ProfilePage - User profile management

- **Dashboard Pages**
  - DashboardPage - Main home screen with stats
  - StatisticsPage - Detailed analytics

- **Feature Pages**
  - ProductsPage - Product listing
  - ProductDetailPage - Individual product details
  - UsersPage - User management
  - SettingsPage - App & user settings

#### ViewModels (MVVM Pattern)
```
ViewModels/
├── Auth/
│   ├── LoginViewModel
│   ├── RegisterViewModel
│   └── AuthBaseViewModel
├── Dashboard/
│   ├── DashboardViewModel
│   └── StatisticsViewModel
├── Products/
│   ├── ProductsListViewModel
│   └── ProductDetailViewModel
└── BaseViewModel (implements INotifyPropertyChanged)
```

Each ViewModel:
- Implements `MVVM Community Toolkit` for property notifications
- Handles user input and business logic
- Communicates with services
- Manages state

### 2. **Service Layer**

#### Authentication Services
```csharp
IAuthService
├── LoginAsync(email, password) → Task<LoginResponse>
├── LogoutAsync() → Task
├── RegisterAsync(data) → Task<RegisterResponse>
├── RefreshTokenAsync() → Task<bool>
└── IsAuthenticatedAsync() → Task<bool>
```

#### API Services
```csharp
IApiService
├── GetAsync<T>(endpoint) → Task<T>
├── PostAsync<T>(endpoint, data) → Task<T>
├── PutAsync<T>(endpoint, data) → Task<T>
├── DeleteAsync(endpoint) → Task
└── SetAuthToken(token) → void
```

#### Cache Services
```csharp
ICacheService
├── GetAsync<T>(key) → Task<T>
├── SetAsync<T>(key, value, expiry) → Task
├── RemoveAsync(key) → Task
└── ClearAsync() → Task
```

#### Navigation Services
```csharp
INavigationService
├── NavigateToAsync<T>(parameter) → Task
├── GoBackAsync() → Task
├── NavigateToLoginAsync() → Task
└── ClearStackAsync() → Task
```

### 3. **Infrastructure Layer**

#### Dependency Injection
```csharp
// MauiProgram.cs Setup
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IApiService, ApiService>();
services.AddScoped<ICacheService, CacheService>();
services.AddScoped<INavigationService, NavigationService>();
```

#### Models (Shared with API)
```
Models/
├── Requests/
│   ├── LoginRequest
│   ├── RegisterRequest
│   └── UpdateUserRequest
└── Responses/
    ├── LoginResponse
    ├── UserDto
    └── ApiResponse<T>
```

#### Behaviors & Behaviors
```csharp
Infrastructure/Behaviors/
├── EmailValidationBehavior - Entry field validation
├── NumericValidationBehavior - Number input validation
└── PickerBehavior - Dropdown selection behavior

Infrastructure/Converters/
├── BoolToColorConverter - Conditional styling
├── EmptyStringConverter - Visibility logic
└── DateTimeConverter - Date formatting
```

### 4. **Core Features**

#### Authentication Flow
1. User enters credentials on LoginPage
2. LoginViewModel calls IAuthService.LoginAsync()
3. AuthService makes API call to backend
4. On success:
   - Token stored in secure storage
   - Navigation to Dashboard
   - IApiService configured with Bearer token
5. On failure:
   - Error message shown
   - User remains on LoginPage

#### API Communication
1. All services use IApiService
2. IApiService uses HttpClient with:
   - Base URL configuration
   - Bearer token in headers
   - Error handling & retry logic
3. Shared DTOs from Application layer
4. Automatic token refresh on 401 responses

#### Data Caching
1. Frequently accessed data cached locally
2. Cache invalidation on user actions
3. Offline-first experience where possible
4. Sync when connection restored

### 5. **Project References**

Mobile references the core layers:
```
SmartWorkz.Starter.Mobile
├── references: SmartWorkz.Starter.Application
│   ├── Services (IAuthService, etc.)
│   ├── DTOs & Models
│   └── Business Logic
├── references: SmartWorkz.Starter.Infrastructure
│   ├── Repository patterns
│   ├── Database context
│   └── External service integrations
└── references: SmartWorkz.Starter.Shared
    ├── Constants
    ├── Enums
    └── Common utilities
```

## File Structure

```
SmartWorkz.Starter.Mobile/
├── Pages/
│   ├── Auth/
│   │   ├── LoginPage.xaml
│   │   ├── LoginPage.xaml.cs
│   │   ├── RegisterPage.xaml
│   │   └── RegisterPage.xaml.cs
│   ├── Dashboard/
│   │   ├── DashboardPage.xaml
│   │   └── DashboardPage.xaml.cs
│   └── ...
├── ViewModels/
│   ├── BaseViewModel.cs
│   ├── Auth/
│   │   ├── LoginViewModel.cs
│   │   └── AuthBaseViewModel.cs
│   └── ...
├── Services/
│   ├── Auth/
│   │   └── AuthService.cs
│   ├── API/
│   │   └── ApiService.cs
│   ├── Cache/
│   │   └── CacheService.cs
│   └── Navigation/
│       └── NavigationService.cs
├── Models/
│   ├── Requests/
│   └── Responses/
├── Infrastructure/
│   ├── DI/
│   │   └── ServiceCollectionExtensions.cs
│   ├── Configuration/
│   │   └── AppConfiguration.cs
│   ├── Converters/
│   └── Behaviors/
├── Resources/
│   ├── Styles/
│   │   └── Colors.xaml
│   ├── Images/
│   ├── Fonts/
│   └── Raw/
├── Platforms/
│   ├── Android/
│   ├── iOS/
│   ├── MacCatalyst/
│   └── Windows/
├── AppShell.xaml
├── App.xaml.cs
└── MauiProgram.cs
```

## Implementation Checklist

### Phase 1: Core Setup ✓
- [ ] Create project structure
- [ ] Add NuGet packages
- [ ] Configure MauiProgram
- [ ] Set up DI

### Phase 2: Authentication
- [ ] Implement IAuthService
- [ ] Create LoginPage & LoginViewModel
- [ ] Secure token storage
- [ ] Auto-login on app start

### Phase 3: API Integration
- [ ] Implement IApiService
- [ ] Configure HttpClient
- [ ] Error handling & logging
- [ ] Token refresh mechanism

### Phase 4: Core Features
- [ ] Dashboard page & ViewModel
- [ ] Product listing
- [ ] User management
- [ ] Settings page

### Phase 5: UX Polish
- [ ] Styling & themes
- [ ] Loading states
- [ ] Error messages
- [ ] Offline mode

### Phase 6: Testing
- [ ] Unit tests for ViewModels
- [ ] Integration tests for Services
- [ ] UI tests

## Best Practices

### ViewModels
```csharp
public class LoginViewModel : BaseViewModel
{
    private string _email;
    private string _password;
    
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }
    
    public IAsyncRelayCommand LoginCommand { get; }
    
    public LoginViewModel(IAuthService authService)
    {
        LoginCommand = new AsyncRelayCommand(OnLoginAsync);
    }
    
    private async Task OnLoginAsync()
    {
        if (IsBusy) return;
        
        try
        {
            IsBusy = true;
            var result = await _authService.LoginAsync(Email, Password);
            if (result) await Shell.Current.GoToAsync("dashboard");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

### Services
```csharp
public class AuthService : IAuthService
{
    private readonly IApiService _api;
    private readonly ISecureStorage _storage;
    
    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await _api.PostAsync<LoginResponse>(
            "api/auth/login",
            new LoginRequest { Email = email, Password = password }
        );
        
        if (response?.Token != null)
        {
            await _storage.SetAsync("authToken", response.Token);
            return true;
        }
        
        return false;
    }
}
```

### Pages
```xaml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
    <VerticalStackLayout Padding="20" Spacing="10">
        <Entry 
            Placeholder="Email"
            Text="{Binding Email}"
            Keyboard="Email" />
        
        <Entry
            Placeholder="Password"
            Text="{Binding Password}"
            IsPassword="True" />
        
        <Button
            Text="Login"
            Command="{Binding LoginCommand}"
            IsEnabled="{Binding IsNotBusy}" />
    </VerticalStackLayout>
</ContentPage>
```

## Dependencies

### NuGet Packages
- `Microsoft.Maui.Controls` (v9.0.0)
- `CommunityToolkit.Mvvm` (v8.2.2)
- `Microsoft.Extensions.DependencyInjection` (v9.0.0)
- `Microsoft.Extensions.Http` (v9.0.0)
- `Microsoft.Extensions.Configuration` (v9.0.0)

### Local Projects
- `SmartWorkz.Starter.Application` - Business logic & services
- `SmartWorkz.Starter.Infrastructure` - Data access & external APIs
- `SmartWorkz.Starter.Shared` - Constants & utilities

## Getting Started

1. **Run setup script:**
   ```powershell
   .\rename-and-extend.ps1 -AddMobile
   ```

2. **Build project:**
   ```powershell
   dotnet build
   ```

3. **Implement core services:**
   - AuthService with real API calls
   - ApiService with HttpClient setup
   - CacheService for local storage

4. **Create pages:**
   - Implement XAML pages
   - Connect ViewModels
   - Add routing via AppShell

5. **Test & deploy:**
   - Run on Android/iOS emulators
   - Test API integration
   - Package for distribution

## References

- [MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [MVVM Community Toolkit](https://github.com/CommunityToolkit/dotnet)
- [Project Architecture](./RENAME_AND_EXTEND_GUIDE.md)
