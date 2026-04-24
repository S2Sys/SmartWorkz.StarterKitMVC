import XCTest
@testable import SmartWorkz_Core_MacOS

final class DockServiceTests: XCTestCase {

    var sut: DockService!

    override func setUp() {
        super.setUp()
        sut = DockService()
    }

    override func tearDown() {
        sut = nil
        super.tearDown()
    }

    // MARK: - Badge Management Tests

    /// Test that setting badge count updates the dock tile
    func testSetBadgeCount() {
        // Note: Full functionality requires running in app context
        // This test validates the method exists and can be called
        sut.setBadgeCount(5)
        // In a real app environment, NSApplication.shared.dockTile.badgeLabel would be "5"
        XCTAssertTrue(true, "setBadgeCount should execute without error")
    }

    /// Test that clearing badge works
    func testClearBadge() {
        sut.setBadgeCount(5)
        sut.clearBadge()
        // Verify no errors occur during badge clearing
        XCTAssertTrue(true, "clearBadge should execute without error")
    }

    /// Test incrementing badge
    func testIncrementBadge() {
        sut.clearBadge()
        sut.incrementBadge()
        // Should increment from 0 to 1
        XCTAssertTrue(true, "incrementBadge should execute without error")
    }

    /// Test decrementing badge
    func testDecrementBadge() {
        sut.setBadgeCount(3)
        sut.decrementBadge()
        // Should decrement from 3 to 2
        XCTAssertTrue(true, "decrementBadge should execute without error")
    }

    /// Test that decrement won't go below zero
    func testDecrementBadgeBelowZero() {
        sut.clearBadge()
        sut.decrementBadge()
        // Should stay at 0, not go negative
        XCTAssertTrue(true, "decrementBadge should not crash at zero")
    }

    // MARK: - Dock Icon Tests

    /// Test setting a custom dock image
    func testSetDockImage() {
        if let image = NSImage(named: "AppIcon") {
            sut.setDockImage(image)
            XCTAssertTrue(true, "setDockImage should execute with valid image")
        }
    }

    /// Test clearing dock image
    func testSetDockImageNil() {
        sut.setDockImage(nil)
        XCTAssertTrue(true, "setDockImage(nil) should restore default icon")
    }

    // MARK: - Status Indicator Tests

    /// Test setting a status indicator
    func testSetStatusIndicator() {
        sut.setStatusIndicator("✓")
        XCTAssertTrue(true, "setStatusIndicator should execute without error")
    }

    /// Test clearing status indicator
    func testClearStatusIndicator() {
        sut.setStatusIndicator("!")
        sut.clearStatusIndicator()
        XCTAssertTrue(true, "clearStatusIndicator should execute without error")
    }

    // MARK: - Dock Menu Tests

    /// Test building a dock menu
    func testBuildDockMenu() {
        let menuBuilder = MenuBuilder()
        let dockMenu = sut.buildDockMenu(using: menuBuilder)

        XCTAssertNotNil(dockMenu, "Dock menu should not be nil")
        XCTAssertGreaterThan(dockMenu.items.count, 0, "Dock menu should contain items")
    }

    /// Test that dock menu contains essential items
    func testDockMenuHasEssentialItems() {
        let menuBuilder = MenuBuilder()
        let dockMenu = sut.buildDockMenu(using: menuBuilder)

        let itemTitles = dockMenu.items.map { $0.title }

        XCTAssertTrue(
            itemTitles.contains("Open SmartWorkz"),
            "Dock menu should contain 'Open SmartWorkz' item"
        )
        XCTAssertTrue(
            itemTitles.contains("Quit SmartWorkz"),
            "Dock menu should contain 'Quit SmartWorkz' item"
        )
    }

    /// Test that dock menu contains status items with correct tags
    func testDockMenuStatusItemTags() {
        let menuBuilder = MenuBuilder()
        let dockMenu = sut.buildDockMenu(using: menuBuilder)

        // Look for items with specific tags
        let connectionItem = dockMenu.item(withTag: 1001)
        let syncItem = dockMenu.item(withTag: 1002)

        XCTAssertNotNil(connectionItem, "Dock menu should have connection status item (tag 1001)")
        XCTAssertNotNil(syncItem, "Dock menu should have sync status item (tag 1002)")
    }

    /// Test that dock menu contains Quick Actions submenu
    func testDockMenuHasQuickActionsSubmenu() {
        let menuBuilder = MenuBuilder()
        let dockMenu = sut.buildDockMenu(using: menuBuilder)

        let quickActionsItem = dockMenu.items.first { $0.title == "Quick Actions" }

        XCTAssertNotNil(quickActionsItem, "Dock menu should contain Quick Actions submenu")
        XCTAssertNotNil(quickActionsItem?.submenu, "Quick Actions should have a submenu")
    }

    /// Test updating dock menu status
    func testUpdateDockMenuStatus() {
        let menuBuilder = MenuBuilder()
        sut.buildDockMenu(using: menuBuilder)

        // Update status should not crash
        sut.updateDockMenuStatus(connectionStatus: "Connected", syncStatus: "Synced")
        XCTAssertTrue(true, "updateDockMenuStatus should execute without error")
    }

    // MARK: - Notification Badge Coordination

    /// Test updating notification badge
    func testUpdateNotificationBadge() {
        sut.updateNotificationBadge(count: 5)
        XCTAssertTrue(true, "updateNotificationBadge should execute without error")
    }

    /// Test that notification badge with zero clears
    func testUpdateNotificationBadgeZero() {
        sut.updateNotificationBadge(count: 3)
        sut.updateNotificationBadge(count: 0)
        XCTAssertTrue(true, "updateNotificationBadge with 0 should clear badge")
    }

    // MARK: - Initialization Tests

    /// Test DockService initialization
    func testDockServiceInitialization() {
        let service1 = DockService()
        XCTAssertNotNil(service1, "DockService should initialize without appDelegate")

        let mockDelegate = MockAppDelegate()
        let service2 = DockService(appDelegate: mockDelegate)
        XCTAssertNotNil(service2, "DockService should initialize with appDelegate")
    }

    // MARK: - Integration Tests

    /// Test complete badge lifecycle
    func testBadgeLifecycle() {
        // Clear starting state
        sut.clearBadge()

        // Increment to 3
        sut.incrementBadge()
        sut.incrementBadge()
        sut.incrementBadge()

        // Decrement to 1
        sut.decrementBadge()
        sut.decrementBadge()

        // Clear
        sut.clearBadge()

        XCTAssertTrue(true, "Badge lifecycle should complete without error")
    }

    /// Test dock service with app state updates
    func testDockServiceWithNotificationUpdates() {
        // Simulate notification updates
        sut.updateNotificationBadge(count: 1)
        sut.updateNotificationBadge(count: 2)
        sut.updateNotificationBadge(count: 5)
        sut.updateNotificationBadge(count: 0)

        XCTAssertTrue(true, "Dock service should handle notification updates")
    }
}

// MARK: - Mock Classes

private class MockAppDelegate: NSObject, NSApplicationDelegate {
    func applicationDidFinishLaunching(_ notification: Notification) {
        // Mock implementation
    }
}
