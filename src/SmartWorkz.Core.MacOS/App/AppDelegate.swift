import Cocoa

/// AppDelegate handles macOS application lifecycle events
final class AppDelegate: NSObject, NSApplicationDelegate {

    /// Reference to the shared app state
    @MainActor static var appState: AppState?

    // MARK: - NSApplicationDelegate Lifecycle Methods

    /// Called when the application has finished launching
    func applicationDidFinishLaunching(_ notification: Notification) {
        // Initialize app state on main thread
        Task { @MainActor in
            if AppDelegate.appState == nil {
                AppDelegate.appState = AppState()
            }

            // Perform any startup initialization
            setupApplication()
        }
    }

    /// Determines whether the application should terminate after the last window is closed
    /// Returns false to keep the app in the Dock even when all windows are closed
    func applicationShouldTerminateAfterLastWindowClosed(_ sender: NSApplication) -> Bool {
        return false
    }

    /// Called when the user attempts to reopen the application
    /// This happens when clicking the app icon in the Dock
    func applicationShouldHandleReopen(_ sender: NSApplication, hasVisibleWindows flag: Bool) -> Bool {
        // If no windows are visible, create/show the main window
        if !flag {
            // Find and show the main window
            if let window = NSApplication.shared.windows.first {
                window.makeKeyAndOrderFront(self)
            }
        }
        return true
    }

    /// Called when the application will terminate
    func applicationWillTerminate(_ notification: Notification) {
        // Cleanup operations
        cleanup()
    }

    // MARK: - Private Methods

    /// Setup application resources and state
    private func setupApplication() {
        // Configure app state with initial values
        // Connect to backend, load user session, etc.
    }

    /// Cleanup before app termination
    private func cleanup() {
        // Save state, close connections, release resources
    }
}
