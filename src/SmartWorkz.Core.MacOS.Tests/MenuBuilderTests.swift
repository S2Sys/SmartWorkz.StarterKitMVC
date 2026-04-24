import XCTest
@testable import SmartWorkz_Core_MacOS

final class MenuBuilderTests: XCTestCase {

    var sut: MenuBuilder!

    override func setUp() {
        super.setUp()
        sut = MenuBuilder()
    }

    override func tearDown() {
        sut = nil
        super.tearDown()
    }

    // MARK: - Main Menu Building Tests

    /// Test that buildMainMenu creates a valid menu with all major sections
    func testBuildMainMenuStructure() {
        let mainMenu = sut.buildMainMenu()

        // Main menu should have 5 items: App, File, Edit, View, Help
        XCTAssertGreaterThanOrEqual(
            mainMenu.items.count,
            5,
            "Main menu should contain at least 5 items (App, File, Edit, View, Help)"
        )

        // Verify menu items are not nil
        XCTAssertNotNil(mainMenu.items.first, "First menu item (App menu) should not be nil")
        XCTAssertFalse(mainMenu.items.isEmpty, "Main menu should not be empty")
    }

    /// Test that buildMainMenu creates proper menu with File menu
    func testBuildMainMenuHasFileMenu() {
        let mainMenu = sut.buildMainMenu()

        // Look for File menu
        var hasFileMenu = false
        for item in mainMenu.items {
            if item.title == "File" && item.submenu != nil {
                hasFileMenu = true
                break
            }
        }

        XCTAssertTrue(hasFileMenu, "Main menu should contain a File menu")
    }

    /// Test that buildMainMenu creates proper menu with Edit menu
    func testBuildMainMenuHasEditMenu() {
        let mainMenu = sut.buildMainMenu()

        // Look for Edit menu
        var hasEditMenu = false
        for item in mainMenu.items {
            if item.title == "Edit" && item.submenu != nil {
                hasEditMenu = true
                break
            }
        }

        XCTAssertTrue(hasEditMenu, "Main menu should contain an Edit menu")
    }

    /// Test that buildMainMenu creates proper menu with View menu
    func testBuildMainMenuHasViewMenu() {
        let mainMenu = sut.buildMainMenu()

        // Look for View menu
        var hasViewMenu = false
        for item in mainMenu.items {
            if item.title == "View" && item.submenu != nil {
                hasViewMenu = true
                break
            }
        }

        XCTAssertTrue(hasViewMenu, "Main menu should contain a View menu")
    }

    /// Test that buildMainMenu creates proper menu with Help menu
    func testBuildMainMenuHasHelpMenu() {
        let mainMenu = sut.buildMainMenu()

        // Look for Help menu
        var hasHelpMenu = false
        for item in mainMenu.items {
            if item.title == "Help" && item.submenu != nil {
                hasHelpMenu = true
                break
            }
        }

        XCTAssertTrue(hasHelpMenu, "Main menu should contain a Help menu")
    }

    // MARK: - File Menu Tests

    /// Test that File menu contains essential items
    func testFileMenuHasEssentialItems() {
        let mainMenu = sut.buildMainMenu()

        // Find File menu
        guard let fileMenuItem = mainMenu.items.first(where: { $0.title == "File" }),
              let fileMenu = fileMenuItem.submenu else {
            XCTFail("File menu not found")
            return
        }

        // Check for required items
        let itemTitles = fileMenu.items.map { $0.title }

        XCTAssertTrue(
            itemTitles.contains("New Window"),
            "File menu should contain 'New Window' item"
        )
        XCTAssertTrue(
            itemTitles.contains("Close"),
            "File menu should contain 'Close' item"
        )
    }

    /// Test File menu items have correct key equivalents
    func testFileMenuKeyEquivalents() {
        let mainMenu = sut.buildMainMenu()

        guard let fileMenuItem = mainMenu.items.first(where: { $0.title == "File" }),
              let fileMenu = fileMenuItem.submenu else {
            XCTFail("File menu not found")
            return
        }

        // Check for keyboard shortcuts
        if let newWindowItem = fileMenu.items.first(where: { $0.title == "New Window" }) {
            XCTAssertEqual(
                newWindowItem.keyEquivalent,
                "n",
                "New Window should have 'n' as key equivalent"
            )
        }

        if let closeItem = fileMenu.items.first(where: { $0.title == "Close" }) {
            XCTAssertEqual(
                closeItem.keyEquivalent,
                "w",
                "Close should have 'w' as key equivalent"
            )
        }
    }

    // MARK: - Edit Menu Tests

    /// Test that Edit menu contains essential items
    func testEditMenuHasEssentialItems() {
        let mainMenu = sut.buildMainMenu()

        guard let editMenuItem = mainMenu.items.first(where: { $0.title == "Edit" }),
              let editMenu = editMenuItem.submenu else {
            XCTFail("Edit menu not found")
            return
        }

        let itemTitles = editMenu.items.map { $0.title }

        XCTAssertTrue(
            itemTitles.contains("Cut"),
            "Edit menu should contain 'Cut' item"
        )
        XCTAssertTrue(
            itemTitles.contains("Copy"),
            "Edit menu should contain 'Copy' item"
        )
        XCTAssertTrue(
            itemTitles.contains("Paste"),
            "Edit menu should contain 'Paste' item"
        )
        XCTAssertTrue(
            itemTitles.contains("Select All"),
            "Edit menu should contain 'Select All' item"
        )
    }

    /// Test Edit menu standard key equivalents
    func testEditMenuKeyEquivalents() {
        let mainMenu = sut.buildMainMenu()

        guard let editMenuItem = mainMenu.items.first(where: { $0.title == "Edit" }),
              let editMenu = editMenuItem.submenu else {
            XCTFail("Edit menu not found")
            return
        }

        if let cutItem = editMenu.items.first(where: { $0.title == "Cut" }) {
            XCTAssertEqual(cutItem.keyEquivalent, "x", "Cut should have 'x' as key equivalent")
        }

        if let copyItem = editMenu.items.first(where: { $0.title == "Copy" }) {
            XCTAssertEqual(copyItem.keyEquivalent, "c", "Copy should have 'c' as key equivalent")
        }

        if let pasteItem = editMenu.items.first(where: { $0.title == "Paste" }) {
            XCTAssertEqual(pasteItem.keyEquivalent, "v", "Paste should have 'v' as key equivalent")
        }

        if let selectAllItem = editMenu.items.first(where: { $0.title == "Select All" }) {
            XCTAssertEqual(
                selectAllItem.keyEquivalent,
                "a",
                "Select All should have 'a' as key equivalent"
            )
        }
    }

    // MARK: - Status Menu Tests

    /// Test that buildStatusMenu creates a valid menu
    func testBuildStatusMenuStructure() {
        let statusMenu = sut.buildStatusMenu()

        XCTAssertNotNil(statusMenu, "Status menu should not be nil")
        XCTAssertGreaterThan(
            statusMenu.items.count,
            0,
            "Status menu should contain items"
        )
    }

    /// Test that Status menu contains essential items
    func testStatusMenuHasEssentialItems() {
        let statusMenu = sut.buildStatusMenu()

        let itemTitles = statusMenu.items.map { $0.title }

        XCTAssertTrue(
            itemTitles.contains("Open SmartWorkz"),
            "Status menu should contain 'Open SmartWorkz' item"
        )
        XCTAssertTrue(
            itemTitles.contains("Quit"),
            "Status menu should contain 'Quit' item"
        )
    }

    /// Test that Status menu has informational items disabled
    func testStatusMenuInformationalItems() {
        let statusMenu = sut.buildStatusMenu()

        let statusItems = statusMenu.items.filter { $0.title.contains("Status:") }
        let syncItems = statusMenu.items.filter { $0.title.contains("Sync:") }

        // These items should be disabled (informational only)
        for item in statusItems {
            XCTAssertFalse(item.isEnabled, "Status informational items should be disabled")
        }

        for item in syncItems {
            XCTAssertFalse(item.isEnabled, "Sync informational items should be disabled")
        }
    }

    // MARK: - Menu Separator Tests

    /// Test that menus contain separators
    func testMainMenuContainsSeparators() {
        let mainMenu = sut.buildMainMenu()

        let separators = mainMenu.items.filter { $0.isSeparatorItem }

        XCTAssertGreaterThan(
            separators.count,
            0,
            "Main menu should contain separators for organization"
        )
    }

    /// Test that File menu contains separators
    func testFileMenuContainsSeparators() {
        let mainMenu = sut.buildMainMenu()

        guard let fileMenuItem = mainMenu.items.first(where: { $0.title == "File" }),
              let fileMenu = fileMenuItem.submenu else {
            XCTFail("File menu not found")
            return
        }

        let separators = fileMenu.items.filter { $0.isSeparatorItem }

        XCTAssertGreaterThan(
            separators.count,
            0,
            "File menu should contain separators"
        )
    }

    // MARK: - Menu Item Integrity Tests

    /// Test that all menu items have valid titles
    func testAllMenuItemsHaveValidTitles() {
        let mainMenu = sut.buildMainMenu()

        for item in mainMenu.items {
            if !item.isSeparatorItem {
                XCTAssertFalse(
                    item.title.trimmingCharacters(in: .whitespaces).isEmpty,
                    "Menu items should have non-empty titles"
                )
            }
        }
    }

    /// Test that menu items with actions are enabled
    func testMenuItemsWithActionsAreAccessible() {
        let mainMenu = sut.buildMainMenu()

        // Find first File menu
        guard let fileMenuItem = mainMenu.items.first(where: { $0.title == "File" }),
              let fileMenu = fileMenuItem.submenu else {
            XCTFail("File menu not found")
            return
        }

        // Items with actions (not just submenus) should be interactable
        let itemsWithActions = fileMenu.items.filter { $0.action != nil && !$0.isSeparatorItem }

        XCTAssertGreaterThan(
            itemsWithActions.count,
            0,
            "File menu should have items with actions"
        )
    }

    // MARK: - Initialization Tests

    /// Test MenuBuilder initialization
    func testMenuBuilderInitialization() {
        let builder1 = MenuBuilder()
        XCTAssertNotNil(builder1, "MenuBuilder should initialize successfully without appDelegate")

        // Create a mock app delegate
        let mockDelegate = MockAppDelegate()
        let builder2 = MenuBuilder(appDelegate: mockDelegate)
        XCTAssertNotNil(builder2, "MenuBuilder should initialize successfully with appDelegate")
    }

    /// Test that building menu multiple times returns new instances
    func testBuildMainMenuReturnsNewInstance() {
        let menu1 = sut.buildMainMenu()
        let menu2 = sut.buildMainMenu()

        // Each call should return a new menu instance
        XCTAssertNotEqual(
            ObjectIdentifier(menu1),
            ObjectIdentifier(menu2),
            "Each call to buildMainMenu should return a new menu instance"
        )
    }
}

// MARK: - Mock Classes

/// Mock app delegate for testing
private class MockAppDelegate: NSObject, NSApplicationDelegate {
    func applicationDidFinishLaunching(_ notification: Notification) {
        // Mock implementation
    }
}
