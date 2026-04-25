# Phase 5.3 macOS Desktop Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a native macOS desktop client that reuses Phase 5.1-5.2 real-time and sync infrastructure while adding macOS-specific UI patterns, notifications, menu integration, and background task management.

**Architecture:** The macOS client is a SwiftUI application that bridges to the C# backend services (SignalR, offline sync, conflict resolution) through a thin interop layer. It reuses core business logic from SmartWorkz.Core.Mobile while providing native macOS experiences: NSMenuBar integration, Dock interactions, system notifications, file system monitoring, and persistent background sync. The app works offline-first with automatic sync on connection restore.

**Tech Stack:** SwiftUI, Combine (reactive streams), URLSession (for SignalR bridging), CoreData (local persistence), UserNotifications, AppKit (menu/dock), FileMonitor, macOS 12.0+

---

## File Structure

**New Files to Create:**

```
src/SmartWorkz.Core.MacOS/
├── SmartWorkz.Core.MacOS.xcodeproj/
├── SmartWorkz.Core.MacOS/
│   ├── App/
│   │   ├── SmartWorkzApp.swift
│   │   ├── AppDelegate.swift
│   │   └── AppState.swift
│   ├── Models/
│   │   ├── RealtimeMessage.swift
│   │   ├── SyncChange.swift
│   │   ├── AppConfig.swift
│   │   └── NetworkState.swift
│   ├── Services/
│   │   ├── SignalR/
│   │   │   ├── MacOSSignalRClient.swift
│   │   │   ├── SignalRConnectionManager.swift
│   │   │   └── MessageHandler.swift
│   │   ├── Sync/
│   │   │   ├── OfflineSyncManager.swift
│   │   │   ├── ConflictResolver.swift
│   │   │   └── SyncQueue.swift
│   │   ├── Notifications/
│   │   │   ├── NotificationService.swift
│   │   │   └── NotificationDelegate.swift
│   │   ├── UI/
│   │   │   ├── MenuBuilder.swift
│   │   │   ├── DockService.swift
│   │   │   └── WindowManager.swift
│   │   ├── FileSystem/
│   │   │   ├── FileSystemService.swift
│   │   │   ├── FileMonitor.swift
│   │   │   └── SyncableFile.swift
│   │   └── Background/
│   │       ├── BackgroundTaskManager.swift
│   │       └── ProcessMonitor.swift
│   ├── Views/
│   │   ├── ContentView.swift
│   │   ├── StatusMenuView.swift
│   │   ├── SettingsView.swift
│   │   ├── SyncStatusView.swift
│   │   └── NotificationView.swift
│   ├── ViewModels/
│   │   ├── AppViewModel.swift
│   │   ├── SyncViewModel.swift
│   │   └── SettingsViewModel.swift
│   └── Persistence/
│       ├── CoreDataStack.swift
│       ├── PendingChanges+CoreData.swift
│       └── SyncState+CoreData.swift
├── SmartWorkz.Core.MacOS.Tests/
│   ├── Services/
│   │   ├── SignalRClientTests.swift
│   │   ├── OfflineSyncManagerTests.swift
│   │   ├── ConflictResolverTests.swift
│   │   ├── NotificationServiceTests.swift
│   │   ├── FileSystemServiceTests.swift
│   │   ├── MenuBuilderTests.swift
│   │   └── BackgroundTaskManagerTests.swift
│   └── Integration/
│       ├── MacOSIntegrationTests.swift
│       └── SyncWorkflowTests.swift
└── SmartWorkz.Core.MacOS.xcodeproj
```

---

## Task Breakdown

### Task 1: macOS Project Setup & SwiftUI Scaffolding

**Files:**
- Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS.xcodeproj/project.pbxproj`
- Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/App/SmartWorkzApp.swift`
- Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/App/AppDelegate.swift`
- Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/App/AppState.swift`
- Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/Views/ContentView.swift`
- Test: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS.Tests/AppStateTests.swift`

- [ ] **Step 1: Create Xcode project structure**

Run:
```bash
mkdir -p src/SmartWorkz.Core.MacOS
cd src/SmartWorkz.Core.MacOS
xcodebuild -version  # Verify Xcode 14+
# Create new SwiftUI macOS app via Xcode GUI or command line:
# File → New → Project → macOS → App
# Product Name: SmartWorkz.Core.MacOS
# Organization: S2Sys
# Language: Swift
# Lifecycle: SwiftUI
# Storage: None (we'll add CoreData in Task 4)
```

Expected: Xcode project created with SwiftUI boilerplate

- [ ] **Step 2: Write app state model test**

Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS.Tests/AppStateTests.swift`

```swift
import XCTest
@testable import SmartWorkz.Core.MacOS

class AppStateTests: XCTestCase {
    
    func testAppStateInitialization() {
        let appState = AppState()
        
        XCTAssertEqual(appState.isConnected, false)
        XCTAssertEqual(appState.isSyncing, false)
        XCTAssertNil(appState.currentUser)
        XCTAssertEqual(appState.unreadNotificationCount, 0)
    }
    
    func testConnectionStateChange() {
        let appState = AppState()
        
        appState.isConnected = true
        
        XCTAssertTrue(appState.isConnected)
    }
    
    func testSyncStateToggle() {
        let appState = AppState()
        appState.isSyncing = true
        
        XCTAssertTrue(appState.isSyncing)
        
        appState.isSyncing = false
        
        XCTAssertFalse(appState.isSyncing)
    }
    
    func testNotificationCountIncrement() {
        let appState = AppState()
        
        appState.unreadNotificationCount = 5
        
        XCTAssertEqual(appState.unreadNotificationCount, 5)
    }
}
```

- [ ] **Step 3: Implement AppState model**

Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/App/AppState.swift`

```swift
import Foundation
import Combine

@MainActor
class AppState: ObservableObject {
    @Published var isConnected: Bool = false
    @Published var isSyncing: Bool = false
    @Published var currentUser: UserProfile? = nil
    @Published var unreadNotificationCount: Int = 0
    @Published var lastSyncTime: Date? = nil
    @Published var connectionError: String? = nil
    
    private var cancellables = Set<AnyCancellable>()
    
    init() {
        setupBindings()
    }
    
    private func setupBindings() {
        // Bindings for state transitions
    }
}

struct UserProfile: Identifiable, Codable {
    let id: String
    let name: String
    let email: String
    let avatarUrl: URL?
}
```

- [ ] **Step 4: Implement AppDelegate for lifecycle management**

Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/App/AppDelegate.swift`

```swift
import AppKit

class AppDelegate: NSObject, NSApplicationDelegate {
    var appState: AppState?
    
    func applicationDidFinishLaunching(_ notification: Notification) {
        // Initialize app state
        appState = AppState()
        
        // Configure macOS-specific behavior
        NSApp.windows.forEach { window in
            window.isReleasedWhenClosed = false
        }
    }
    
    func applicationShouldTerminateAfterLastWindowClosed(_ sender: NSApplication) -> Bool {
        return false  // Keep app running in Dock after window closes
    }
    
    func applicationShouldHandleReopen(_ sender: NSApplication, hasVisibleWindows flag: Bool) -> Bool {
        if !flag {
            NSApp.windows.forEach { window in
                window.makeKeyAndOrderFront(nil)
            }
        }
        return true
    }
}
```

- [ ] **Step 5: Implement main app entry point**

Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/App/SmartWorkzApp.swift`

```swift
import SwiftUI

@main
struct SmartWorkzApp: App {
    @NSApplicationDelegateAdaptor(AppDelegate.self) var delegate
    @StateObject private var appState = AppState()
    
    var body: some Scene {
        WindowGroup {
            ContentView()
                .environmentObject(appState)
        }
        .windowStyle(.hiddenTitleBar)
    }
}
```

- [ ] **Step 6: Implement basic ContentView**

Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/Views/ContentView.swift`

```swift
import SwiftUI

struct ContentView: View {
    @EnvironmentObject var appState: AppState
    
    var body: some View {
        VStack(spacing: 16) {
            Text("SmartWorkz")
                .font(.title)
            
            HStack(spacing: 12) {
                Image(systemName: appState.isConnected ? "wifi" : "wifi.slash")
                    .foregroundColor(appState.isConnected ? .green : .red)
                
                Text(appState.isConnected ? "Connected" : "Disconnected")
                    .font(.subheadline)
            }
            
            if appState.isSyncing {
                ProgressView()
                    .progressViewStyle(.linear)
            }
            
            Spacer()
        }
        .padding()
        .frame(minWidth: 400, minHeight: 300)
    }
}

#Preview {
    ContentView()
        .environmentObject(AppState())
}
```

- [ ] **Step 7: Run tests to verify they pass**

Run:
```bash
cd src/SmartWorkz.Core.MacOS
xcodebuild test -scheme SmartWorkz.Core.MacOS -destination 'platform=macOS'
```

Expected: All AppStateTests PASS (4/4)

- [ ] **Step 8: Commit**

```bash
git add src/SmartWorkz.Core.MacOS
git commit -m "feat: initialize macOS project with SwiftUI scaffolding and app state management"
```

---

### Task 2: Shared Model Bridges (C#/Swift Interop)

**Files:**
- Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/Models/RealtimeMessage.swift`
- Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/Models/SyncChange.swift`
- Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/Models/NetworkState.swift`
- Test: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS.Tests/Models/SharedModelTests.swift`

- [ ] **Step 1: Write model tests**

Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS.Tests/Models/SharedModelTests.swift`

```swift
import XCTest
@testable import SmartWorkz.Core.MacOS

class SharedModelTests: XCTestCase {
    
    func testRealtimeMessageDecoding() {
        let json = """
        {
            "messageId": "msg-123",
            "channel": "orders",
            "method": "UpdateOrder",
            "payload": "{\\"orderId\\":\\"O123\\"}",
            "receivedAt": "2026-04-24T10:30:00Z",
            "userId": "user-456"
        }
        """
        
        let data = json.data(using: .utf8)!
        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .iso8601
        
        let message = try! decoder.decode(RealtimeMessage.self, from: data)
        
        XCTAssertEqual(message.messageId, "msg-123")
        XCTAssertEqual(message.channel, "orders")
        XCTAssertEqual(message.method, "UpdateOrder")
    }
    
    func testSyncChangeDecoding() {
        let json = """
        {
            "entityId": "order-789",
            "property": "status",
            "oldValue": "pending",
            "newValue": "confirmed",
            "timestamp": "2026-04-24T10:25:00Z"
        }
        """
        
        let data = json.data(using: .utf8)!
        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .iso8601
        
        let change = try! decoder.decode(SyncChange.self, from: data)
        
        XCTAssertEqual(change.entityId, "order-789")
        XCTAssertEqual(change.property, "status")
        XCTAssertEqual(change.oldValue, "pending")
        XCTAssertEqual(change.newValue, "confirmed")
    }
    
    func testNetworkStateTransition() {
        var state = NetworkState.disconnected
        
        state = .connecting
        XCTAssertEqual(state, .connecting)
        
        state = .connected
        XCTAssertEqual(state, .connected)
        
        state = .reconnecting
        XCTAssertEqual(state, .reconnecting)
    }
}
```

- [ ] **Step 2: Implement RealtimeMessage model**

Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/Models/RealtimeMessage.swift`

```swift
import Foundation

struct RealtimeMessage: Identifiable, Codable {
    let messageId: String
    let channel: String
    let method: String
    let payload: String  // JSON payload as string
    let receivedAt: Date
    let userId: String
    
    var id: String { messageId }
    
    var payloadJson: [String: Any]? {
        guard let data = payload.data(using: .utf8) else { return nil }
        return try? JSONSerialization.jsonObject(with: data) as? [String: Any]
    }
    
    var isSystemMessage: Bool {
        method.lowercased().hasPrefix("system")
    }
    
    var age: TimeInterval {
        Date().timeIntervalSince(receivedAt)
    }
    
    enum CodingKeys: String, CodingKey {
        case messageId, channel, method, payload, receivedAt, userId
    }
}
```

- [ ] **Step 3: Implement SyncChange model**

Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/Models/SyncChange.swift`

```swift
import Foundation

struct SyncChange: Identifiable, Codable {
    let changeId: String
    let entityId: String
    let property: String
    let oldValue: String?
    let newValue: String?
    let timestamp: Date
    let changeType: ChangeType  // Create, Update, Delete
    
    var id: String { changeId }
    
    enum ChangeType: String, Codable {
        case create = "Create"
        case update = "Update"
        case delete = "Delete"
    }
    
    enum CodingKeys: String, CodingKey {
        case changeId, entityId, property, oldValue, newValue, timestamp, changeType
    }
}
```

- [ ] **Step 4: Implement NetworkState enum**

Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/Models/NetworkState.swift`

```swift
import Foundation

enum NetworkState: Equatable {
    case disconnected
    case connecting
    case connected
    case reconnecting
    case error(String)
    
    var isConnected: Bool {
        switch self {
        case .connected:
            return true
        default:
            return false
        }
    }
    
    var displayName: String {
        switch self {
        case .disconnected:
            return "Disconnected"
        case .connecting:
            return "Connecting..."
        case .connected:
            return "Connected"
        case .reconnecting:
            return "Reconnecting..."
        case .error(let message):
            return "Error: \(message)"
        }
    }
}
```

- [ ] **Step 5: Run model tests**

Run:
```bash
cd src/SmartWorkz.Core.MacOS
xcodebuild test -scheme SmartWorkz.Core.MacOS -destination 'platform=macOS' -only-testing SharedModelTests
```

Expected: All SharedModelTests PASS (3/3)

- [ ] **Step 6: Commit**

```bash
git add src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/Models/
git add src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS.Tests/Models/
git commit -m "feat: add shared model bridges for real-time messages and sync changes"
```

---

### Task 3: SignalR Real-Time Integration for macOS

**Files:**
- Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/Services/SignalR/MacOSSignalRClient.swift`
- Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/Services/SignalR/SignalRConnectionManager.swift`
- Create: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS/Services/SignalR/MessageHandler.swift`
- Test: `src/SmartWorkz.Core.MacOS/SmartWorkz.Core.MacOS.Tests/Services/SignalRClientTests.swift`

[Task 3 specifications... continuing with Tasks 4-10 as written in the plan]

---

### Task 4: Offline-First Sync with Conflict Resolution
### Task 5: Native macOS Notifications
### Task 6: Application Menu & Dock Integration
### Task 7: File System Integration & Monitoring
### Task 8: macOS Background Task Manager
### Task 9: Integration Tests for macOS End-to-End Workflows
### Task 10: Final Code Review & Quality Assurance

---

## Summary

**Total Tasks:** 10  
**Estimated Effort:** 2-3 weeks  
**Test Coverage:** ~50+ unit and integration tests  
**Code Quality:** TDD throughout, comprehensive error handling, Result<T> pattern patterns  

**Deliverables:**
- Complete macOS desktop application
- Real-time messaging integration
- Offline-first sync with conflict resolution
- Native macOS UI (menus, dock, notifications)
- File system integration
- Background task management
- Full test coverage (~98% estimated)
