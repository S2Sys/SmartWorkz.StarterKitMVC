# SmartWorkz.Mobile — MAUI Components & Platform Services

## Assembly Reference

**Path:** `src/SmartWorkz.Core.Mobile/SmartWorkz.Core.Mobile/`  
**Namespace:** `SmartWorkz.Core.Mobile`  
**Target Framework:** .NET 9 MAUI (iOS/Android)  
**Dependencies:**
- `Microsoft.Maui.Controls` v9.0.0
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.Http.Resilience` (retry policies)

---

## Core Components

### CustomButton — Branded Button Control

MAUI Button with built-in styling templates and states.

```csharp
public partial class CustomButton : Button
{
    public static readonly BindableProperty ButtonStyleProperty = 
        BindableProperty.Create(nameof(ButtonStyle), typeof(ButtonStyle), typeof(CustomButton), ButtonStyle.Primary);
    
    public ButtonStyle ButtonStyle
    {
        get => (ButtonStyle)GetValue(ButtonStyleProperty);
        set => SetValue(ButtonStyleProperty, value);
    }
}
```

**Button Styles:**
- `Primary` — Blue, prominent CTA
- `Secondary` — Gray, less prominent
- `Danger` — Red, destructive action
- `Success` — Green, confirmation

**XAML Usage:**
```xml
<controls:CustomButton 
    Text="Save" 
    ButtonStyle="Primary"
    Clicked="OnSaveClicked" />
```

---

### ValidatedEntry — Input Field with Validation

Entry field that shows error messages inline.

```csharp
public partial class ValidatedEntry : Entry
{
    public static readonly BindableProperty ValidationRuleProperty = 
        BindableProperty.Create(nameof(ValidationRule), typeof(ValidationRule), typeof(ValidatedEntry));
    
    public ValidationRule? ValidationRule
    {
        get => (ValidationRule)GetValue(ValidationRuleProperty);
        set => SetValue(ValidationRuleProperty, value);
    }
    
    public string? ErrorMessage { get; set; }
}
```

**Validation Rules:**
- `RequiredRule` — Text must not be empty
- `EmailRule` — Text must match email pattern
- `PhoneRule` — Text must match phone pattern
- `LengthRule` — Text length between min/max
- `RegexRule` — Text must match regex

**XAML Usage:**
```xml
<controls:ValidatedEntry 
    Placeholder="Email"
    Keyboard="Email">
    <controls:ValidatedEntry.ValidationRule>
        <validations:EmailRule />
    </controls:ValidatedEntry.ValidationRule>
</controls:ValidatedEntry>
```

---

### CustomPicker — Dropdown with Data Binding

Picker control with easy item source binding.

```csharp
public partial class CustomPicker : Picker
{
    public static readonly BindableProperty ItemsSourceProperty = 
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<string>), typeof(CustomPicker));
    
    public IEnumerable<string> ItemsSource
    {
        get => (IEnumerable<string>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
}
```

**Usage:**
```xml
<controls:CustomPicker 
    Title="Select Status"
    ItemsSource="{Binding Statuses}"
    SelectedIndex="{Binding SelectedStatus}" />
```

---

### AlertDialog — Native Alert Wrapper

Displays native alert dialogs (UIAlertController on iOS, AlertDialog on Android).

```csharp
public static class AlertDialog
{
    public static async Task ShowAsync(
        string title,
        string message,
        string buttonText = "OK");
    
    public static async Task<bool> ConfirmAsync(
        string title,
        string message,
        string acceptText = "Yes",
        string cancelText = "No");
}
```

**Usage:**
```csharp
await AlertDialog.ShowAsync("Success", "Item saved successfully.");

var confirmed = await AlertDialog.ConfirmAsync(
    "Delete?",
    "Are you sure?",
    "Delete",
    "Cancel");
```

---

### SmartListView — Virtual List with Template Support

Performant list view for large datasets with data template binding.

```csharp
<controls:SmartListView 
    ItemsSource="{Binding Items}"
    SelectionMode="Single"
    SelectedItem="{Binding SelectedItem}">
    <controls:SmartListView.ItemTemplate>
        <DataTemplate>
            <StackLayout Padding="10">
                <Label Text="{Binding Name}" FontSize="16" FontAttributes="Bold" />
                <Label Text="{Binding Description}" FontSize="12" Opacity="0.7" />
            </StackLayout>
        </DataTemplate>
    </controls:SmartListView.ItemTemplate>
</controls:SmartListView>
```

**Features:**
- Virtual scrolling (renders only visible items)
- Pull-to-refresh support
- Grouping with templates
- Selection changed callback

---

### LoadingIndicator — Progress & Spinner

Activity indicator with customizable appearance.

```csharp
<controls:LoadingIndicator 
    IsRunning="{Binding IsLoading}"
    IsVisible="{Binding IsLoading}"
    Color="Blue"
    Size="Large" />
```

**Properties:**
- `IsRunning` — Animate spinner
- `Color` — Spinner color
- `Size` — Large, Medium, Small

---

## Platform-Specific Services

### OfflineMessageQueue — Persist Messages When Offline

Queues messages when the device is offline, syncs when reconnected.

```csharp
public interface IOfflineMessageQueue
{
    Task EnqueueAsync(string message, string topic = "default");
    Task<IEnumerable<QueuedMessage>> GetPendingAsync();
    Task MarkSyncedAsync(string messageId);
    Task ClearAsync();
}
```

**Usage:**
```csharp
public class OrderService
{
    public async Task SaveOrderAsync(Order order)
    {
        try
        {
            await _apiClient.PostAsync("/orders", order);
        }
        catch (HttpRequestException)
        {
            // Device offline; queue for later
            await _messageQueue.EnqueueAsync(
                JsonSerializer.Serialize(order),
                "orders");
        }
    }
}
```

---

### AutoReconnectService — Exponential Backoff Retry

Manages connection retry logic with exponential backoff.

```csharp
public interface IAutoReconnectService
{
    Task ConnectAsync();
    bool IsConnected { get; }
    event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;
}
```

**Configuration:**
- **InitialDelay:** 1 second
- **MaxDelay:** 5 minutes
- **Backoff:** 2x per attempt
- **Max Attempts:** 10

**Usage:**
```csharp
_reconnectService.ConnectionStateChanged += (s, e) =>
{
    if (e.IsConnected)
    {
        _syncService.SyncOfflineMessages();
    }
};
```

---

### DeduplicationService — Prevent Duplicate Real-Time Messages

Tracks message IDs to prevent processing duplicates in real-time scenarios.

```csharp
public interface IDeduplicationService
{
    bool IsDuplicate(string messageId, string topic = "default");
    void MarkProcessed(string messageId, string topic = "default");
    void ClearExpired();
}
```

**Usage:**
```csharp
public async Task OnMessageReceivedAsync(WebSocketMessage message)
{
    if (_deduplicationService.IsDuplicate(message.Id))
        return;  // Skip duplicate
    
    await ProcessMessageAsync(message);
    _deduplicationService.MarkProcessed(message.Id);
}
```

---

## VoIP Push Notifications (iOS)

Handles CallKit integration for incoming calls.

```csharp
[Register("iOSVoIPPushDelegate")]
public class iOSVoIPPushDelegate : PKPushRegistryDelegate
{
    public override void DidUpdatePushCredentials(
        PKPushRegistry registry,
        PKPushCredentials credentials,
        string pushType)
    {
        // Send token to backend for VoIP push registration
    }
    
    public override void DidReceiveIncomingPush(
        PKPushRegistry registry,
        PKPushPayload payload,
        string pushType,
        Action completion)
    {
        // Handle incoming call, update CallKit
    }
}
```

---

## Background Refresh (iOS/Android)

Periodic sync in the background without user interaction.

```csharp
public class BackgroundRefreshService
{
    public static void SchedulePeriodicSync(TimeSpan interval)
    {
        #if __IOS__
            var request = new BGAppRefreshTaskRequest 
            { 
                Identifier = "com.smartworkz.bg-sync",
                EarliestBeginDate = NSDate.FromTimeIntervalSinceNow(interval.TotalSeconds)
            };
            BGTaskScheduler.SharedScheduler.SubmitTaskRequest(request, out NSError error);
        #elif __ANDROID__
            var constraints = new Constraints.Builder()
                .SetRequiresDeviceIdle(false)
                .SetRequiresBatteryNotLow(true)
                .Build();
            
            var request = new PeriodicWorkRequest.Builder(
                typeof(SyncWorker),
                interval)
                .SetConstraints(constraints)
                .Build();
            
            WorkManager.GetInstance(Application.Context)
                .EnqueueUniquePeriodicWork(
                    "sync",
                    ExistingPeriodicWorkPolicy.KeepExisting,
                    request);
        #endif
    }
}
```

---

## DI Registration

```csharp
public static void AddSmartWorkzMobile(this IServiceCollection services)
{
    services.AddSingleton<IOfflineMessageQueue, OfflineMessageQueue>();
    services.AddSingleton<IAutoReconnectService, AutoReconnectService>();
    services.AddSingleton<IDeduplicationService, DeduplicationService>();
    services.AddSingleton<IBackgroundRefreshService, BackgroundRefreshService>();
}
```

**In MauiProgram.cs:**
```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"))
            .AddSmartWorkzMobile();
        
        return builder.Build();
    }
}
```

---

**Next:** [Step-by-Step Guide — Build Your First Feature](08-step-by-step-guide.md)
