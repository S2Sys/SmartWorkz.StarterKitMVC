import Cocoa
import SwiftUI

/// WindowManager handles the creation and lifecycle management of application windows
/// Manages main window, settings window, and other application windows
class WindowManager {

    // MARK: - Properties

    private var mainWindow: NSWindow?
    private var settingsWindow: NSWindow?
    private var activeWindows: Set<NSWindow> = []

    private weak var appDelegate: NSApplicationDelegate?
    private weak var appState: AppState?

    // MARK: - Initialization

    /// Initialize WindowManager with optional app delegate and app state
    /// - Parameters:
    ///   - appDelegate: The application delegate
    ///   - appState: The application state object
    init(appDelegate: NSApplicationDelegate? = nil, appState: AppState? = nil) {
        self.appDelegate = appDelegate
        self.appState = appState
    }

    // MARK: - Main Window Management

    /// Create the main application window with ContentView
    /// - Returns: The created NSWindow, or existing one if already created
    func createMainWindow() -> NSWindow {
        if let existingWindow = mainWindow {
            return existingWindow
        }

        let contentView = ContentView()
            .environmentObject(appState ?? AppState())

        let hostingController = NSHostingController(rootView: contentView)

        let window = NSWindow(
            contentViewController: hostingController
        )

        // Configure window properties
        window.title = "SmartWorkz"
        window.setFrameAutosaveName("MainWindow")

        // Set window size and center on screen
        let frame = NSRect(x: 100, y: 100, width: 600, height: 700)
        window.setFrame(frame, display: true)

        // Set window style mask
        window.styleMask = [.titled, .closable, .miniaturizable, .resizable]

        // Set minimum size
        window.minSize = NSSize(width: 500, height: 600)

        // Appearance
        window.appearance = NSAppearance(named: .aqua)

        // Delegate for window events
        window.delegate = WindowDelegate()

        self.mainWindow = window
        activeWindows.insert(window)

        return window
    }

    /// Show the main window, creating it if necessary
    func showMainWindow() {
        let window = createMainWindow()
        window.makeKeyAndOrderFront(nil)
        NSApplication.shared.activate(ignoringOtherApps: true)
    }

    /// Hide the main window (does not close it)
    func hideMainWindow() {
        if let window = mainWindow, window.isVisible {
            window.orderOut(nil)
        }
    }

    /// Close the main window
    func closeMainWindow() {
        if let window = mainWindow {
            window.close()
            mainWindow = nil
            activeWindows.remove(window)
        }
    }

    /// Check if main window is visible
    /// - Returns: True if the main window exists and is visible
    func isMainWindowVisible() -> Bool {
        return mainWindow?.isVisible ?? false
    }

    // MARK: - Settings Window Management

    /// Create the settings/preferences window
    /// - Returns: The created NSWindow for settings
    func createSettingsWindow() -> NSWindow {
        if let existingWindow = settingsWindow {
            return existingWindow
        }

        let settingsView = SettingsView()
            .environmentObject(appState ?? AppState())

        let hostingController = NSHostingController(rootView: settingsView)

        let window = NSWindow(
            contentViewController: hostingController
        )

        // Configure window properties
        window.title = "SmartWorkz - Settings"
        window.setFrameAutosaveName("SettingsWindow")

        // Set window size and center on screen
        let frame = NSRect(x: 200, y: 200, width: 500, height: 600)
        window.setFrame(frame, display: true)

        // Set window style mask (no resize for settings)
        window.styleMask = [.titled, .closable]

        // Set fixed size
        window.minSize = NSSize(width: 500, height: 600)
        window.maxSize = NSSize(width: 500, height: 600)

        // Make it a floating window
        window.level = .floating

        // Delegate for window events
        window.delegate = WindowDelegate()

        self.settingsWindow = window
        activeWindows.insert(window)

        return window
    }

    /// Show the settings window
    func showSettingsWindow() {
        let window = createSettingsWindow()
        window.makeKeyAndOrderFront(nil)
        NSApplication.shared.activate(ignoringOtherApps: true)
    }

    /// Close the settings window
    func closeSettingsWindow() {
        if let window = settingsWindow {
            window.close()
            settingsWindow = nil
            activeWindows.remove(window)
        }
    }

    /// Check if settings window is visible
    /// - Returns: True if the settings window exists and is visible
    func isSettingsWindowVisible() -> Bool {
        return settingsWindow?.isVisible ?? false
    }

    // MARK: - Window Ordering and Focus

    /// Bring all application windows to front
    func bringAllWindowsToFront() {
        for window in activeWindows where window.isVisible {
            window.makeKeyAndOrderFront(nil)
        }
    }

    /// Minimize all windows
    func minimizeAllWindows() {
        for window in activeWindows where window.isVisible {
            window.miniaturize(nil)
        }
    }

    /// Close all windows except the main window
    func closeSecondaryWindows() {
        if let settingsWindow = settingsWindow {
            settingsWindow.close()
            self.settingsWindow = nil
            activeWindows.remove(settingsWindow)
        }
    }

    // MARK: - Query Methods

    /// Get all active windows
    /// - Returns: Array of currently active windows
    func getAllWindows() -> [NSWindow] {
        return Array(activeWindows)
    }

    /// Get the main window if it exists
    /// - Returns: The main window or nil
    func getMainWindow() -> NSWindow? {
        return mainWindow
    }

    /// Get the settings window if it exists
    /// - Returns: The settings window or nil
    func getSettingsWindow() -> NSWindow? {
        return settingsWindow
    }

    /// Remove a window from tracking
    /// - Parameter window: The window to remove
    func removeWindow(_ window: NSWindow) {
        activeWindows.remove(window)
        if window == mainWindow {
            mainWindow = nil
        } else if window == settingsWindow {
            settingsWindow = nil
        }
    }
}

// MARK: - WindowDelegate

/// Delegate for handling window lifecycle events
private class WindowDelegate: NSObject, NSWindowDelegate {

    /// Called when window will close
    func windowWillClose(_ notification: Notification) {
        if let window = notification.object as? NSWindow {
            // The WindowManager will be notified indirectly
        }
    }

    /// Called when window will miniaturize
    func windowWillMiniaturize(_ notification: Notification) {
        // Handle miniaturization if needed
    }

    /// Called when window will deminiaturize
    func windowDidDeminiaturize(_ notification: Notification) {
        // Handle deminiaturization if needed
    }
}

// MARK: - AppDelegate Extensions for Window Actions

extension AppDelegate {

    /// Handle showing main window from menu
    @objc func showMainWindowFromMenu(_ sender: Any?) {
        // Implementation would use WindowManager
    }

    /// Handle opening preferences from menu
    @objc func openPreferencesFromMenu(_ sender: Any?) {
        // Implementation would use WindowManager
    }
}

// MARK: - SettingsView (Placeholder)

/// Placeholder Settings view for SwiftUI
struct SettingsView: View {

    @EnvironmentObject private var appState: AppState

    var body: some View {
        VStack(spacing: 20) {
            Text("Settings")
                .font(.title)
                .padding()

            Form {
                Section(header: Text("Connection")) {
                    Toggle("Auto-connect on launch", isOn: .constant(true))
                    Toggle("Show connection status", isOn: .constant(true))
                }

                Section(header: Text("Notifications")) {
                    Toggle("Enable notifications", isOn: .constant(true))
                    Toggle("Show badge count", isOn: .constant(true))
                }

                Section(header: Text("Sync")) {
                    Toggle("Auto-sync", isOn: .constant(true))
                    Stepper("Sync interval (minutes)", value: .constant(5), in: 1...60)
                }
            }
            .padding()

            Spacer()

            HStack(spacing: 12) {
                Button(action: {}) {
                    Text("Reset to Defaults")
                }

                Spacer()

                Button(action: {}) {
                    Text("Done")
                }
                .keyboardShortcut(.defaultAction)
            }
            .padding()
        }
        .frame(minWidth: 500, minHeight: 600)
    }
}
