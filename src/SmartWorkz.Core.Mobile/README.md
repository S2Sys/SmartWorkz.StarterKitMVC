# SmartWorkz.Core.Mobile

Cross-platform mobile library for MAUI applications providing unified APIs for device access, contacts, location, biometrics, connectivity, and real-time communication across iOS, Android, and macOS.

## Table of Contents

- [Purpose](#purpose)
- [Quick Start](#quick-start)
- [Platform Support Matrix](#platform-support-matrix)
- [Platform-Specific Setup](#platform-specific-setup)
- [ContactsService Usage Examples](#contactsservice-usage-examples)
- [Permission Handling](#permission-handling-per-platform)
- [Testing Mobile Services](#testing-mobile-services)
- [Troubleshooting](#troubleshooting)
- [Best Practices](#best-practices)

## Purpose

SmartWorkz.Core.Mobile provides a unified abstraction layer over platform-specific device capabilities. It enables developers to access contacts, location, camera, biometrics, Bluetooth, NFC, and real-time communication without writing platform-specific code for each target platform.

**Key Features:**
- Single API working across iOS, Android, and macOS (Catalyst)
- Automatic permission handling with check-and-request flow
- Platform-specific optimizations under the hood
- Async/await throughout
- Comprehensive error handling and logging
- Real-time communication via SignalR
- Offline-first sync capabilities

## Quick Start

### 1. Register Services

```csharp
// In your MauiProgram.cs
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        return builder
            .UseMauiApp<App>()
            .AddSmartWorkzCoreMobile(configureApi: config =>
            {
                config.BaseUrl = "https://api.example.com";
            })
            .Build();
    }
}
```

### 2. Inject and Use ContactsService

```csharp
public partial class ContactListViewModel
{
    private readonly IContactsService _contactsService;

    public ContactListViewModel(IContactsService contactsService)
    {
        _contactsService = contactsService;
    }

    public async Task LoadContacts()
    {
        var contacts = await _contactsService.GetAllContactsAsync();
        foreach (var contact in contacts)
        {
            Debug.WriteLine($"{contact.DisplayName} - {contact.Email}");
        }
    }
}
```

## Platform Support Matrix

| Feature | iOS 13+ | Android 8+ | macOS 12+ | Windows |
|---------|---------|-----------|----------|---------|
| **Contacts Access** | ✓ | ✓ | ✓ | ✗ |
| **Contact Search** | ✓ | ✓ | ✓ | ✗ |
| **Contact Picker** | ✗ | ✗ | ✗ | ✗ |
| **Location Services** | ✓ | ✓ | ✓ | ✗ |
| **Camera Access** | ✓ | ✓ | ✓ | ✗ |
| **Biometric Auth** | ✓ | ✓ | ✗ | ✗ |
| **Bluetooth** | ✓ | ✓ | ✗ | ✗ |
| **NFC** | ✓ | ✓ | ✗ | ✗ |
| **Push Notifications** | ✓ | ✓ | ✓ | ✗ |

## Platform-Specific Setup

### iOS Setup

**Requirements:**
- Xcode 15.0+
- iOS 13 minimum deployment target
- Apple Developer account for provisioning profiles

**Steps:**
1. In Xcode, configure `Info.plist` with required permissions:
   ```xml
   <key>NSContactsUsageDescription</key>
   <string>We need access to your contacts to suggest connections</string>
   <key>NSLocationWhenInUseUsageDescription</key>
   <string>We need your location to show nearby events</string>
   <key>NSCameraUsageDescription</key>
   <string>We need camera access to capture photos</string>
   ```

2. Set provisioning profile in project properties
3. Enable capabilities in Xcode (Contacts, Location, Camera, etc.)

### Android Setup

**Requirements:**
- Android SDK 8.0+ (API 26+)
- Gradle 8.0+

**Steps:**
1. Update `AndroidManifest.xml`:
   ```xml
   <uses-permission android:name="android.permission.READ_CONTACTS" />
   <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
   <uses-permission android:name="android.permission.CAMERA" />
   <uses-permission android:name="android.permission.INTERNET" />
   ```

2. For Android 6.0+, runtime permissions are handled automatically by IPermissionService
3. Ensure `targetSdkVersion` is 34 or higher in build.gradle

### macOS Setup

**Requirements:**
- macOS 12.0+
- Xcode 15.0+

**Steps:**
1. In Xcode, enable App Sandbox if required
2. Grant entitlements in Entitlements.plist:
   ```xml
   <key>com.apple.security.personal-information.contacts</key>
   <true/>
   <key>com.apple.security.personal-information.location</key>
   <true/>
   ```

3. Code-sign with appropriate certificate
4. Contacts framework is available on macOS 12+

### Windows Setup

Windows platform returns empty results for contact access. Consider alternative approaches:
- Use native Windows Contacts app via URI launch
- Store contacts in local database
- Sync from cloud service

## ContactsService Usage Examples

### Example 1: Display All Contacts

```csharp
public class ContactListPage
{
    private readonly IContactsService _contactsService;
    public ObservableCollection<Contact> Contacts { get; } = new();

    public ContactListPage(IContactsService contactsService)
    {
        _contactsService = contactsService;
        InitializeComponent();
    }

    private async void OnAppearing(object sender, EventArgs e)
    {
        try
        {
            var contacts = await _contactsService.GetAllContactsAsync();
            
            // Update UI on main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Contacts.Clear();
                foreach (var contact in contacts)
                {
                    Contacts.Add(contact);
                }
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load contacts: {ex.Message}", "OK");
        }
    }
}
```

### Example 2: Search Contacts by Name

```csharp
public class ContactSearchViewModel
{
    private readonly IContactsService _contactsService;
    private string _searchQuery;
    public ObservableCollection<Contact> SearchResults { get; } = new();

    public ContactSearchViewModel(IContactsService contactsService)
    {
        _contactsService = contactsService;
    }

    public async void OnSearchTextChanged(string query)
    {
        _searchQuery = query;
        
        if (string.IsNullOrWhiteSpace(query))
        {
            SearchResults.Clear();
            return;
        }

        try
        {
            var results = await _contactsService.SearchContactsAsync(query);
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SearchResults.Clear();
                foreach (var contact in results)
                {
                    SearchResults.Add(contact);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Search failed: {ex.Message}");
        }
    }
}
```

### Example 3: Handle Permission Status and Graceful Degradation

```csharp
public class ContactAccessViewModel
{
    private readonly IContactsService _contactsService;
    private readonly IPermissionService _permissionService;

    public ContactAccessViewModel(IContactsService contactsService, IPermissionService permissionService)
    {
        _contactsService = contactsService;
        _permissionService = permissionService;
    }

    public async Task<bool> EnsureContactsPermissionAsync()
    {
        // Check if service is available on this platform
        var isAvailable = await _contactsService.IsAvailableAsync();
        if (!isAvailable)
        {
            Debug.WriteLine("Contacts service not available on this platform");
            return false;
        }

        // Check current permission status
        var status = await _permissionService.CheckAsync(MobilePermission.Contacts);
        
        if (status == PermissionStatus.Granted)
        {
            return true;
        }

        if (status == PermissionStatus.DeniedAlways || status == PermissionStatus.Restricted)
        {
            Debug.WriteLine("Contacts permission permanently denied");
            return false;
        }

        // Request permission
        var result = await _permissionService.RequestAsync(MobilePermission.Contacts);
        return result == PermissionStatus.Granted;
    }

    public async Task<IReadOnlyList<Contact>> GetContactsWithPermissionFlow()
    {
        if (await EnsureContactsPermissionAsync())
        {
            return await _contactsService.GetAllContactsAsync();
        }

        return new List<Contact>();
    }
}
```

## Permission Handling Per Platform

### Android
- **Permission:** `android.permission.READ_CONTACTS`
- **Flow:** Check at runtime, request from user if not granted
- **API 6.0+:** Uses runtime permissions (handled by IPermissionService)
- **Response:** User can grant, deny, or deny always

**Example:**
```csharp
var status = await _permissionService.CheckAsync(MobilePermission.Contacts);
if (status != PermissionStatus.Granted)
{
    status = await _permissionService.RequestAsync(MobilePermission.Contacts);
}
```

### iOS
- **Permission:** Contacts framework entitlement in Info.plist
- **User Prompt:** System shows one-time permission dialog
- **Flow:** Check with `ContactStore.AuthorizationStatus`, request if needed
- **Status Values:** Authorized, Denied, NotDetermined, Restricted

### macOS
- **Entitlement:** `com.apple.security.personal-information.contacts`
- **Sandbox:** App Sandbox must be enabled
- **User Prompt:** System permission dialog on first access
- **Persistence:** User can manage in System Preferences > Security & Privacy

### Windows
- **Status:** Contacts service returns empty; not supported
- **Alternative:** Use native Windows Contact Picker or cloud sync
- **Service Check:** `IsAvailableAsync()` returns false on Windows

## Testing Mobile Services

### Unit Test Pattern with Mocked Permissions

```csharp
public class ContactsServiceTests
{
    private Mock<ILogger> _mockLogger;
    private Mock<IPermissionService> _mockPermissionService;
    private IContactsService _contactsService;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger>();
        _mockPermissionService = new Mock<IPermissionService>();
        _contactsService = new ContactsService(_mockLogger.Object, _mockPermissionService.Object);
    }

    [Test]
    public async Task GetAllContactsAsync_WithGrantedPermission_ReturnsContacts()
    {
        // Arrange
        _mockPermissionService
            .Setup(x => x.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _contactsService.GetAllContactsAsync();

        // Assert
        Assert.IsNotNull(result);
        _mockPermissionService.Verify(x => x.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetAllContactsAsync_WithDeniedPermission_ReturnsEmptyList()
    {
        // Arrange
        _mockPermissionService
            .Setup(x => x.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        _mockPermissionService
            .Setup(x => x.RequestAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Denied);

        // Act
        var result = await _contactsService.GetAllContactsAsync();

        // Assert
        Assert.IsEmpty(result);
    }

    [Test]
    public async Task SearchContactsAsync_WithValidQuery_CallsSearchMethod()
    {
        // Arrange
        _mockPermissionService
            .Setup(x => x.CheckAsync(MobilePermission.Contacts, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PermissionStatus.Granted);

        // Act
        var result = await _contactsService.SearchContactsAsync("John");

        // Assert
        Assert.IsNotNull(result);
    }
}
```

### Platform-Specific Testing

```csharp
#if __ANDROID__
[Test]
public async Task GetAllContactsAsync_Android_UsesContentProvider()
{
    // Android implementation tests would verify ContentResolver usage
    var result = await _contactsService.GetAllContactsAsync();
    Assert.IsNotNull(result);
}
#endif

#if __IOS__
[Test]
public async Task GetAllContactsAsync_iOS_UsesCNContactStore()
{
    // iOS implementation tests would verify CNContactStore usage
    var result = await _contactsService.GetAllContactsAsync();
    Assert.IsNotNull(result);
}
#endif
```

## Troubleshooting

### "Permission denied" when accessing contacts
- **Android:** Check manifest has `READ_CONTACTS` permission and user granted runtime permission
- **iOS:** Verify Info.plist contains `NSContactsUsageDescription`
- **macOS:** Confirm app has Contacts entitlement in Entitlements.plist

### Empty contacts list on iOS
- Ensure CNContactStore keys are properly fetched: `GivenName`, `FamilyName`, `EmailAddresses`, `PhoneNumbers`
- Check that permission dialog was accepted

### Contact picker returns null
- Contact picker is not available on any platform via IContactsService
- Implement custom contact selection UI or use platform-specific pickers

### Windows returns empty results
- Contact access is not supported on Windows platform
- Use alternative: Windows Contact app or cloud-based sync

### Slow performance on large contact lists
- Android: Contact queries block UI thread; wrap in background task
- Implement pagination for large result sets
- Consider local caching with periodic sync

## Best Practices

1. **Always check availability** before accessing contacts:
   ```csharp
   if (await _contactsService.IsAvailableAsync())
   {
       var contacts = await _contactsService.GetAllContactsAsync();
   }
   ```

2. **Handle permissions gracefully** with user-friendly messages:
   ```csharp
   var status = await _permissionService.RequestAsync(MobilePermission.Contacts);
   if (status == PermissionStatus.DeniedAlways)
   {
       // Show message directing user to Settings
   }
   ```

3. **Use async/await consistently** and never block main thread:
   ```csharp
   // Good
   var contacts = await _contactsService.GetAllContactsAsync();
   
   // Avoid
   var contacts = _contactsService.GetAllContactsAsync().Result;
   ```

4. **Implement offline fallback** for critical workflows:
   ```csharp
   var contacts = await _contactsService.GetAllContactsAsync();
   if (!contacts.Any())
   {
       // Load from cache or local database
   }
   ```

5. **Cache contacts when appropriate** to reduce repeated queries:
   ```csharp
   if (_cachedContacts == null)
   {
       _cachedContacts = await _contactsService.GetAllContactsAsync();
   }
   return _cachedContacts;
   ```

6. **Log permission and service issues** for debugging:
   ```csharp
   _logger.LogInformation("Requesting {Permission} permission", MobilePermission.Contacts);
   var status = await _permissionService.RequestAsync(MobilePermission.Contacts);
   _logger.LogInformation("Permission {Permission} result: {Status}", MobilePermission.Contacts, status);
   ```

7. **Test on real devices** as emulator behavior differs from production:
   - Android emulator may return mock contacts
   - iOS simulator requires actual Apple ID for some operations
   - macOS testing requires proper entitlements

8. **Handle cancellation tokens** properly:
   ```csharp
   using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
   var contacts = await _contactsService.GetAllContactsAsync(cts.Token);
   ```
