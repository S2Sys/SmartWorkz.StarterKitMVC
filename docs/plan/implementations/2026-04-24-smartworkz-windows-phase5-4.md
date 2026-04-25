# Phase 5.4 Windows Desktop Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a native Windows desktop client that reuses Phase 5.1-5.2 real-time and sync infrastructure while adding Windows-specific UI patterns, notifications, taskbar integration, and background service management.

**Architecture:** The Windows client is a WinUI 3 application using MVVM pattern that leverages existing C# backend services (SignalR, offline sync, conflict resolution) without needing Swift bridges. It provides native Windows experiences: taskbar integration, system notifications, system tray support, file system monitoring, and background synchronization. The app works offline-first with automatic sync on connection restore, reusing the core C# sync engine from Phase 5.1-5.2.

**Tech Stack:** WinUI 3 (Windows App SDK), MVVM Community Toolkit, Windows App Notifications, Windows Services (background sync), File System Watcher, async/await patterns, xUnit for testing.

---

## File Structure

**New Files to Create:**

```
src/SmartWorkz.Core.Windows/
├── SmartWorkz.Core.Windows.csproj
├── App.xaml
├── App.xaml.cs
├── AppWindow.xaml
├── AppWindow.xaml.cs
├── ViewModels/
│   ├── MainViewModel.cs
│   ├── SyncViewModel.cs
│   ├── SettingsViewModel.cs
│   └── ViewModelBase.cs
├── Views/
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── SettingsWindow.xaml
│   ├── SettingsWindow.xaml.cs
│   ├── StatusPanel.xaml
│   ├── SyncStatusPanel.xaml
│   └── NotificationPanel.xaml
├── Services/
│   ├── ISignalRClientService.cs
│   ├── WindowsSignalRClientService.cs
│   ├── INotificationService.cs
│   ├── WindowsNotificationService.cs
│   ├── ITaskbarService.cs
│   ├── WindowsTaskbarService.cs
│   ├── ISystemTrayService.cs
│   ├── WindowsSystemTrayService.cs
│   ├── IFileSystemWatcherService.cs
│   ├── WindowsFileSystemWatcherService.cs
│   ├── IBackgroundSyncService.cs
│   └── WindowsBackgroundSyncService.cs
├── Models/
│   ├── ConnectionState.cs
│   ├── SyncStatus.cs
│   └── NotificationMessage.cs
├── Utilities/
│   ├── ServiceCollectionExtensions.cs
│   └── RelayCommand.cs
└── Resources/
    ├── AppResources.resw
    └── Strings/

tests/SmartWorkz.Core.Windows.Tests/
├── SmartWorkz.Core.Windows.Tests.csproj
├── Services/
│   ├── WindowsSignalRClientServiceTests.cs
│   ├── WindowsNotificationServiceTests.cs
│   ├── WindowsTaskbarServiceTests.cs
│   ├── WindowsSystemTrayServiceTests.cs
│   ├── WindowsFileSystemWatcherServiceTests.cs
│   └── WindowsBackgroundSyncServiceTests.cs
├── ViewModels/
│   ├── MainViewModelTests.cs
│   ├── SyncViewModelTests.cs
│   └── SettingsViewModelTests.cs
└── Integration/
    ├── WindowsIntegrationTests.cs
    └── SyncWorkflowTests.cs
```

---

## Tasks

### Task 1: Project Setup & MVVM Scaffolding
Create WinUI 3 project, MVVM base classes, MainViewModel with state management, basic XAML structure.

### Task 2: SignalR Integration for Windows
Implement WindowsSignalRClientService, connection management, message routing, reuse Phase 5.1 patterns.

### Task 3: Windows Notifications System
Implement toast notifications, notification actions, system tray notifications using Windows App SDK.

### Task 4: Taskbar & System Tray Integration
Implement taskbar badge counts, progress indicators, system tray menu and icon management.

### Task 5: File System Monitoring
Implement FileSystemWatcher, monitor file changes, trigger sync operations on file changes.

### Task 6: Background Sync Service
Implement background synchronization service, periodic sync timer, offline queue management.

### Task 7: Offline Sync ViewModel
Implement SyncViewModel for managing pending changes, sync coordination, error handling.

### Task 8: Integration Tests
Implement end-to-end workflow tests, sync scenario tests, component interaction verification.

### Task 9: Final Code Review & Quality Assurance
Run full test suite, verify compilation, code quality checks, documentation.

---

**Total Tasks:** 9  
**Estimated Effort:** 2-3 weeks  
**Test Coverage:** 25+ unit and integration tests  
**Code Quality:** TDD throughout, MVVM pattern, comprehensive error handling
