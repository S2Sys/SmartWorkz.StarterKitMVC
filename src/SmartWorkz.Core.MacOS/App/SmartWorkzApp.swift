import SwiftUI

/// Main application entry point for SmartWorkz macOS client
@main
struct SmartWorkzApp: App {

    // MARK: - App Delegate Setup

    @NSApplicationDelegateAdaptor(AppDelegate.self) var appDelegate

    // MARK: - State Management

    @StateObject private var appState = AppState()

    // MARK: - Scene Definition

    var body: some Scene {
        WindowGroup {
            ContentView()
                .environmentObject(appState)
                .frame(minWidth: 400, minHeight: 300)
        }
        .windowStyle(.hiddenTitleBar)
        .commands {
            CommandGroup(replacing: .appSettings) {
                Button("Preferences...") {
                    // Open preferences window
                }
                .keyboardShortcut(",", modifiers: .command)
            }
        }

        #if os(macOS)
        Settings {
            Text("Settings")
        }
        #endif
    }
}

#if DEBUG
#Preview {
    let testAppState = AppState()
    testAppState.isConnected = true
    testAppState.currentUser = UserProfile(
        id: "user-123",
        name: "John Doe",
        email: "john@example.com",
        avatarURL: nil
    )

    return ContentView()
        .environmentObject(testAppState)
        .frame(width: 500, height: 400)
}
#endif
