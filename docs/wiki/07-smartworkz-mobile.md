# SmartWorkz.Mobile — MAUI Components & Platform Services

## Assembly Reference

**Path:** `src/SmartWorkz.Core.MAUI/`  
**Namespace:** `SmartWorkz.Core.MAUI`  
**Target Framework:** .NET 9 MAUI (iOS/Android)  
**Dependencies:** Microsoft.Maui.Controls v9.0.0, Microsoft.Extensions.DependencyInjection

---

## Core MAUI Components (XML Documented)

### CustomButton — Branded Button Control

MAUI Button with built-in styling templates and states.

**ButtonStyle enum:** Primary (blue), Secondary (gray), Danger (red), Success (green)

**Usage:**

```xml
<controls:CustomButton 
    Text="Save" 
    ButtonStyle="Primary"
    Clicked="OnSaveClicked" />
```

### ValidatedEntry — Input Field with Validation

Entry field showing error messages inline.

**ValidationRules:** RequiredRule, EmailRule, PhoneRule, LengthRule, RegexRule

**Properties:** ValidationRule, ErrorMessage

### CustomPicker — Dropdown with Data Binding

Picker control with easy item source binding.

**Property:** ItemsSource (IEnumerable<string>)

### AlertDialog — Native Alert Wrapper

Displays native alerts (UIAlertController iOS, AlertDialog Android).

**Methods:**  
ShowAsync(title, message, buttonText="OK")  
ConfirmAsync(title, message, acceptText="Yes", cancelText="No") → bool

### SmartListView — Virtual List with Template Support

Performant list view for large datasets.

**Features:** Virtual scrolling, pull-to-refresh, grouping, selection callback

**Properties:** ItemsSource, SelectionMode, SelectedItem, ItemTemplate

### LoadingIndicator — Progress Spinner

Activity indicator with customizable appearance.

**Properties:** IsRunning, Color, Size (Large/Medium/Small)

---

## Platform-Specific Services (XML Documented)

### IOfflineMessageQueue — Persist Messages When Offline

Queues messages when device is offline, syncs when reconnected.

**Methods:**  
EnqueueAsync(message, topic="default")  
GetPendingAsync() → IEnumerable<QueuedMessage>  
MarkSyncedAsync(messageId)  
ClearAsync()

### IAutoReconnectService — Exponential Backoff Retry

Manages connection retry with exponential backoff.

**Configuration:**  
InitialDelay: 1 second  
MaxDelay: 5 minutes  
Backoff: 2x per attempt  
MaxAttempts: 10

**Properties:** IsConnected  
**Event:** ConnectionStateChanged

### IDeduplicationService — Prevent Duplicate Real-Time Messages

Tracks message IDs to prevent processing duplicates.

**Methods:**  
IsDuplicate(messageId, topic="default") → bool  
MarkProcessed(messageId, topic="default")  
ClearExpired()

---

## VoIP Push Notifications (iOS)

Handles CallKit integration for incoming calls.

**Class:** iOSVoIPPushDelegate : PKPushRegistryDelegate

**Methods:**  
DidUpdatePushCredentials() — Send token to backend  
DidReceiveIncomingPush() — Handle incoming call, update CallKit

---

## Background Refresh (iOS/Android)

Periodic sync in background without user interaction.

**iOS:** BGAppRefreshTaskRequest, BGTaskScheduler  
**Android:** PeriodicWorkRequest, WorkManager with Constraints

---

## DI Registration

```csharp
public static void AddSmartWorkzMobile(this IServiceCollection services)
{
    services.AddSingleton<IOfflineMessageQueue, OfflineMessageQueue>();
    services.AddSingleton<IAutoReconnectService, AutoReconnectService>();
    services.AddSingleton<IDeduplicationService, DeduplicationService>();
}
```

**In MauiProgram.cs:**

```csharp
var builder = MauiApp.CreateBuilder()
    .UseMauiApp<App>()
    .AddSmartWorkzMobile();
    
return builder.Build();
```

---

**Next:** [08-step-by-step-guide.md](./08-step-by-step-guide.md)
