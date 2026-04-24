import Cocoa

/// MenuBuilder is responsible for constructing the application menu bar
/// Supports building main menu and status menu with standard macOS menu items
class MenuBuilder {

    // MARK: - Properties

    private weak var appDelegate: NSApplicationDelegate?

    // MARK: - Initialization

    /// Initialize MenuBuilder with optional app delegate reference
    /// - Parameter appDelegate: The application delegate for handling menu actions
    init(appDelegate: NSApplicationDelegate? = nil) {
        self.appDelegate = appDelegate
    }

    // MARK: - Main Menu Building

    /// Build the main application menu bar
    /// - Returns: An NSMenu containing File, Edit, View, and Help menus
    func buildMainMenu() -> NSMenu {
        let mainMenu = NSMenu()

        // App Menu (auto-handled by macOS)
        mainMenu.addItem(buildAppMenu())

        // File Menu
        mainMenu.addItem(buildFileMenu())

        // Edit Menu
        mainMenu.addItem(buildEditMenu())

        // View Menu
        mainMenu.addItem(buildViewMenu())

        // Help Menu
        mainMenu.addItem(buildHelpMenu())

        return mainMenu
    }

    // MARK: - Private Menu Construction Methods

    /// Build the Application menu (File menu is typically where app title appears)
    /// - Returns: An NSMenuItem containing standard app menu items
    private func buildAppMenu() -> NSMenuItem {
        let appMenu = NSMenu()

        // About app
        appMenu.addItem(
            withTitle: "About SmartWorkz",
            action: #selector(NSApplication.orderFrontStandardAboutPanel),
            keyEquivalent: ""
        )

        appMenu.addItem(NSMenuItem.separator())

        // Preferences
        let preferencesItem = NSMenuItem(
            title: "Preferences...",
            action: #selector(AppDelegate.openPreferences(_:)),
            keyEquivalent: ","
        )
        preferencesItem.keyEquivalentModifierMask = [.command]
        appMenu.addItem(preferencesItem)

        appMenu.addItem(NSMenuItem.separator())

        // Services submenu
        let servicesMenu = NSMenu()
        let servicesItem = NSMenuItem(title: "Services", action: nil, keyEquivalent: "")
        servicesItem.submenu = servicesMenu
        appMenu.addItem(servicesItem)
        NSApplication.shared.servicesMenu = servicesMenu

        appMenu.addItem(NSMenuItem.separator())

        // Hide app
        let hideItem = NSMenuItem(
            title: "Hide SmartWorkz",
            action: #selector(NSApplication.hide(_:)),
            keyEquivalent: "h"
        )
        hideItem.keyEquivalentModifierMask = [.command]
        appMenu.addItem(hideItem)

        // Hide others
        let hideOthersItem = NSMenuItem(
            title: "Hide Others",
            action: #selector(NSApplication.hideOtherApplications(_:)),
            keyEquivalent: "h"
        )
        hideOthersItem.keyEquivalentModifierMask = [.command, .option]
        appMenu.addItem(hideOthersItem)

        // Show all
        appMenu.addItem(
            withTitle: "Show All",
            action: #selector(NSApplication.unhideAllApplications(_:)),
            keyEquivalent: ""
        )

        appMenu.addItem(NSMenuItem.separator())

        // Quit app
        let quitItem = NSMenuItem(
            title: "Quit SmartWorkz",
            action: #selector(NSApplication.terminate(_:)),
            keyEquivalent: "q"
        )
        quitItem.keyEquivalentModifierMask = [.command]
        appMenu.addItem(quitItem)

        let appMenuItem = NSMenuItem(title: "SmartWorkz", action: nil, keyEquivalent: "")
        appMenuItem.submenu = appMenu

        return appMenuItem
    }

    /// Build the File menu
    /// - Returns: An NSMenuItem containing file-related operations
    private func buildFileMenu() -> NSMenuItem {
        let fileMenu = NSMenu(title: "File")

        // New window
        let newWindowItem = NSMenuItem(
            title: "New Window",
            action: #selector(AppDelegate.newWindow(_:)),
            keyEquivalent: "n"
        )
        newWindowItem.keyEquivalentModifierMask = [.command]
        fileMenu.addItem(newWindowItem)

        // Open recent
        let openRecentItem = NSMenuItem(title: "Open Recent", action: nil, keyEquivalent: "")
        openRecentItem.submenu = NSMenu()
        fileMenu.addItem(openRecentItem)

        fileMenu.addItem(NSMenuItem.separator())

        // Close window
        let closeItem = NSMenuItem(
            title: "Close",
            action: #selector(NSWindow.performClose(_:)),
            keyEquivalent: "w"
        )
        closeItem.keyEquivalentModifierMask = [.command]
        fileMenu.addItem(closeItem)

        let fileMenuItem = NSMenuItem(title: "File", action: nil, keyEquivalent: "")
        fileMenuItem.submenu = fileMenu

        return fileMenuItem
    }

    /// Build the Edit menu
    /// - Returns: An NSMenuItem containing standard editing operations
    private func buildEditMenu() -> NSMenuItem {
        let editMenu = NSMenu(title: "Edit")

        // Undo
        let undoItem = NSMenuItem(
            title: "Undo",
            action: Selector(("undo:")),
            keyEquivalent: "z"
        )
        undoItem.keyEquivalentModifierMask = [.command]
        editMenu.addItem(undoItem)

        // Redo
        let redoItem = NSMenuItem(
            title: "Redo",
            action: Selector(("redo:")),
            keyEquivalent: "z"
        )
        redoItem.keyEquivalentModifierMask = [.command, .shift]
        editMenu.addItem(redoItem)

        editMenu.addItem(NSMenuItem.separator())

        // Cut
        let cutItem = NSMenuItem(
            title: "Cut",
            action: #selector(NSResponder.cut(_:)),
            keyEquivalent: "x"
        )
        cutItem.keyEquivalentModifierMask = [.command]
        editMenu.addItem(cutItem)

        // Copy
        let copyItem = NSMenuItem(
            title: "Copy",
            action: #selector(NSResponder.copy(_:)),
            keyEquivalent: "c"
        )
        copyItem.keyEquivalentModifierMask = [.command]
        editMenu.addItem(copyItem)

        // Paste
        let pasteItem = NSMenuItem(
            title: "Paste",
            action: #selector(NSResponder.paste(_:)),
            keyEquivalent: "v"
        )
        pasteItem.keyEquivalentModifierMask = [.command]
        editMenu.addItem(pasteItem)

        // Select All
        let selectAllItem = NSMenuItem(
            title: "Select All",
            action: #selector(NSResponder.selectAll(_:)),
            keyEquivalent: "a"
        )
        selectAllItem.keyEquivalentModifierMask = [.command]
        editMenu.addItem(selectAllItem)

        let editMenuItem = NSMenuItem(title: "Edit", action: nil, keyEquivalent: "")
        editMenuItem.submenu = editMenu

        return editMenuItem
    }

    /// Build the View menu
    /// - Returns: An NSMenuItem containing view-related options
    private func buildViewMenu() -> NSMenuItem {
        let viewMenu = NSMenu(title: "View")

        // Toggle fullscreen
        let fullscreenItem = NSMenuItem(
            title: "Toggle Full Screen",
            action: #selector(NSWindow.toggleFullScreen(_:)),
            keyEquivalent: "f"
        )
        fullscreenItem.keyEquivalentModifierMask = [.command, .control]
        viewMenu.addItem(fullscreenItem)

        viewMenu.addItem(NSMenuItem.separator())

        // Show sidebar (future feature)
        let sidebarItem = NSMenuItem(
            title: "Show Sidebar",
            action: #selector(AppDelegate.toggleSidebar(_:)),
            keyEquivalent: "s"
        )
        sidebarItem.keyEquivalentModifierMask = [.command, .option]
        viewMenu.addItem(sidebarItem)

        let viewMenuItem = NSMenuItem(title: "View", action: nil, keyEquivalent: "")
        viewMenuItem.submenu = viewMenu

        return viewMenuItem
    }

    /// Build the Help menu
    /// - Returns: An NSMenuItem containing help-related items
    private func buildHelpMenu() -> NSMenuItem {
        let helpMenu = NSMenu(title: "Help")

        // SmartWorkz Help
        let helpItem = NSMenuItem(
            title: "SmartWorkz Help",
            action: #selector(AppDelegate.showHelp(_:)),
            keyEquivalent: ""
        )
        helpMenu.addItem(helpItem)

        helpMenu.addItem(NSMenuItem.separator())

        // Report issue
        let reportItem = NSMenuItem(
            title: "Report an Issue",
            action: #selector(AppDelegate.reportIssue(_:)),
            keyEquivalent: ""
        )
        helpMenu.addItem(reportItem)

        // Documentation
        let docItem = NSMenuItem(
            title: "Documentation",
            action: #selector(AppDelegate.openDocumentation(_:)),
            keyEquivalent: ""
        )
        helpMenu.addItem(docItem)

        let helpMenuItem = NSMenuItem(title: "Help", action: nil, keyEquivalent: "")
        helpMenuItem.submenu = helpMenu

        return helpMenuItem
    }

    // MARK: - Status Menu Building

    /// Build a status menu for the status bar (menu bar)
    /// - Returns: An NSMenu for use in NSStatusBar
    func buildStatusMenu() -> NSMenu {
        let statusMenu = NSMenu()

        // Open app
        statusMenu.addItem(
            withTitle: "Open SmartWorkz",
            action: #selector(AppDelegate.showMainWindow(_:)),
            keyEquivalent: ""
        )

        statusMenu.addItem(NSMenuItem.separator())

        // Connection status (informational)
        let statusItem = NSMenuItem(
            title: "Status: Ready",
            action: nil,
            keyEquivalent: ""
        )
        statusItem.isEnabled = false
        statusMenu.addItem(statusItem)

        // Sync status
        let syncItem = NSMenuItem(
            title: "Last Sync: Never",
            action: nil,
            keyEquivalent: ""
        )
        syncItem.isEnabled = false
        statusMenu.addItem(syncItem)

        statusMenu.addItem(NSMenuItem.separator())

        // Preferences
        statusMenu.addItem(
            withTitle: "Preferences...",
            action: #selector(AppDelegate.openPreferences(_:)),
            keyEquivalent: ""
        )

        statusMenu.addItem(NSMenuItem.separator())

        // Quit
        statusMenu.addItem(
            withTitle: "Quit",
            action: #selector(NSApplication.terminate(_:)),
            keyEquivalent: ""
        )

        return statusMenu
    }
}

// MARK: - AppDelegate Extensions for Menu Actions

extension AppDelegate {

    /// Handle opening preferences
    @objc func openPreferences(_ sender: Any?) {
        // Implementation handled by WindowManager
    }

    /// Handle creating a new window
    @objc func newWindow(_ sender: Any?) {
        // Implementation handled by WindowManager
    }

    /// Handle showing main window
    @objc func showMainWindow(_ sender: Any?) {
        // Implementation handled by WindowManager
    }

    /// Handle toggling sidebar
    @objc func toggleSidebar(_ sender: Any?) {
        // Implementation handled by WindowManager
    }

    /// Handle showing help
    @objc func showHelp(_ sender: Any?) {
        NSWorkspace.shared.open(URL(string: "https://smartworkz.example.com/help")!)
    }

    /// Handle reporting issue
    @objc func reportIssue(_ sender: Any?) {
        NSWorkspace.shared.open(URL(string: "https://smartworkz.example.com/report-issue")!)
    }

    /// Handle opening documentation
    @objc func openDocumentation(_ sender: Any?) {
        NSWorkspace.shared.open(URL(string: "https://smartworkz.example.com/docs")!)
    }
}
