import Cocoa

/// DockService manages the application's dock appearance and interactions
/// Handles badge counts, dock icons, and dock context menus
class DockService {

    // MARK: - Properties

    private weak var appDelegate: NSApplicationDelegate?
    private var statusMenu: NSMenu?

    // MARK: - Initialization

    /// Initialize DockService with optional app delegate
    /// - Parameter appDelegate: The application delegate for action handling
    init(appDelegate: NSApplicationDelegate? = nil) {
        self.appDelegate = appDelegate
    }

    // MARK: - Badge Management

    /// Set the badge count displayed on the dock icon
    /// - Parameter count: The number to display (0 to hide badge)
    func setBadgeCount(_ count: Int) {
        DispatchQueue.main.async {
            if count > 0 {
                NSApplication.shared.dockTile.badgeLabel = String(count)
            } else {
                NSApplication.shared.dockTile.badgeLabel = nil
            }
        }
    }

    /// Increment the current badge count
    func incrementBadge() {
        let currentLabel = NSApplication.shared.dockTile.badgeLabel ?? "0"
        let currentCount = Int(currentLabel) ?? 0
        setBadgeCount(currentCount + 1)
    }

    /// Decrement the current badge count
    func decrementBadge() {
        let currentLabel = NSApplication.shared.dockTile.badgeLabel ?? "0"
        let currentCount = Int(currentLabel) ?? 0
        if currentCount > 0 {
            setBadgeCount(currentCount - 1)
        }
    }

    /// Clear the badge (set to 0)
    func clearBadge() {
        setBadgeCount(0)
    }

    // MARK: - Dock Icon Management

    /// Set a custom dock icon image
    /// - Parameter image: The NSImage to use as dock icon (nil to restore default)
    func setDockImage(_ image: NSImage?) {
        DispatchQueue.main.async {
            if let image = image {
                NSApplication.shared.applicationIconImage = image
            } else {
                // Restore default icon by reloading from bundle
                if let defaultIcon = NSImage(named: "AppIcon") {
                    NSApplication.shared.applicationIconImage = defaultIcon
                }
            }
        }
    }

    /// Set a status indicator on the dock icon (small overlay image)
    /// - Parameter status: A short status indicator (e.g., "✓", "!", "●")
    func setStatusIndicator(_ status: String) {
        DispatchQueue.main.async {
            let dockTile = NSApplication.shared.dockTile

            // Create a view for the dock tile with custom content
            let dockView = DockTileView(status: status)
            dockTile.contentView = dockView
            dockTile.display()
        }
    }

    /// Clear the status indicator from dock icon
    func clearStatusIndicator() {
        DispatchQueue.main.async {
            NSApplication.shared.dockTile.contentView = nil
            NSApplication.shared.dockTile.display()
        }
    }

    // MARK: - Dock Menu Management

    /// Build and set the dock context menu
    /// - Parameter menuBuilder: MenuBuilder instance for menu construction
    func buildDockMenu(using menuBuilder: MenuBuilder) -> NSMenu {
        let dockMenu = NSMenu()

        // Open app
        dockMenu.addItem(
            withTitle: "Open SmartWorkz",
            action: #selector(AppDelegate.showMainWindow(_:)),
            keyEquivalent: ""
        )

        dockMenu.addItem(NSMenuItem.separator())

        // Connection status (informational)
        let connectionItem = NSMenuItem(
            title: "Connection: Disconnected",
            action: nil,
            keyEquivalent: ""
        )
        connectionItem.isEnabled = false
        connectionItem.tag = 1001 // Tag for easy identification
        dockMenu.addItem(connectionItem)

        // Sync status (informational)
        let syncItem = NSMenuItem(
            title: "Sync: Idle",
            action: nil,
            keyEquivalent: ""
        )
        syncItem.isEnabled = false
        syncItem.tag = 1002 // Tag for easy identification
        dockMenu.addItem(syncItem)

        dockMenu.addItem(NSMenuItem.separator())

        // Quick actions submenu
        let quickActionsMenu = NSMenu(title: "Quick Actions")
        quickActionsMenu.addItem(
            withTitle: "Check Notifications",
            action: #selector(AppDelegate.checkNotifications(_:)),
            keyEquivalent: ""
        )
        quickActionsMenu.addItem(
            withTitle: "Sync Now",
            action: #selector(AppDelegate.syncNow(_:)),
            keyEquivalent: ""
        )
        quickActionsMenu.addItem(NSMenuItem.separator())
        quickActionsMenu.addItem(
            withTitle: "View Settings",
            action: #selector(AppDelegate.openPreferences(_:)),
            keyEquivalent: ""
        )

        let quickActionsItem = NSMenuItem(title: "Quick Actions", action: nil, keyEquivalent: "")
        quickActionsItem.submenu = quickActionsMenu
        dockMenu.addItem(quickActionsItem)

        dockMenu.addItem(NSMenuItem.separator())

        // Quit
        dockMenu.addItem(
            withTitle: "Quit SmartWorkz",
            action: #selector(NSApplication.terminate(_:)),
            keyEquivalent: ""
        )

        self.statusMenu = dockMenu
        return dockMenu
    }

    /// Update status items in the dock menu
    /// - Parameter connectionStatus: Current connection status
    /// - Parameter syncStatus: Current sync status
    func updateDockMenuStatus(connectionStatus: String, syncStatus: String) {
        guard let menu = statusMenu else { return }

        DispatchQueue.main.async {
            // Update connection status
            if let connectionItem = menu.item(withTag: 1001) {
                connectionItem.title = "Connection: \(connectionStatus)"
            }

            // Update sync status
            if let syncItem = menu.item(withTag: 1002) {
                syncItem.title = "Sync: \(syncStatus)"
            }
        }
    }

    // MARK: - Notification Badge Coordination

    /// Update dock badge based on notification count
    /// - Parameter count: Number of unread notifications
    func updateNotificationBadge(count: Int) {
        setBadgeCount(count)
    }
}

// MARK: - DockTileView

/// Custom view for dock tile to display status indicators
private class DockTileView: NSView {

    let statusLabel: NSTextField

    init(status: String) {
        self.statusLabel = NSTextField()
        super.init(frame: NSRect(x: 0, y: 0, width: 128, height: 128))

        self.wantsLayer = true
        self.layer?.backgroundColor = NSColor.clear.cgColor

        statusLabel.stringValue = status
        statusLabel.font = NSFont.systemFont(ofSize: 60, weight: .bold)
        statusLabel.textColor = .white
        statusLabel.alignment = .center
        statusLabel.isEditable = false
        statusLabel.isSelectable = false
        statusLabel.drawsBackground = false
        statusLabel.isBezeled = false

        // Position in bottom-right corner
        statusLabel.frame = NSRect(x: 60, y: 0, width: 68, height: 68)

        self.addSubview(statusLabel)
    }

    required init?(coder: NSCoder) {
        fatalError("init(coder:) has not been implemented")
    }
}

// MARK: - AppDelegate Extensions for Dock Menu Actions

extension AppDelegate {

    /// Handle checking notifications
    @objc func checkNotifications(_ sender: Any?) {
        // Implementation: Show notifications window or badge
    }

    /// Handle sync now action
    @objc func syncNow(_ sender: Any?) {
        // Implementation: Trigger immediate sync
    }
}
