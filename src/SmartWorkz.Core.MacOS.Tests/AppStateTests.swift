import XCTest
@testable import SmartWorkz_Core_MacOS

final class AppStateTests: XCTestCase {

    var sut: AppState!

    override func setUp() {
        super.setUp()
        sut = AppState()
    }

    override func tearDown() {
        sut = nil
        super.tearDown()
    }

    // MARK: - testAppStateInitialization
    /// Verify that AppState initializes with correct default values
    func testAppStateInitialization() {
        XCTAssertFalse(sut.isConnected, "isConnected should default to false")
        XCTAssertFalse(sut.isSyncing, "isSyncing should default to false")
        XCTAssertNil(sut.currentUser, "currentUser should default to nil")
        XCTAssertEqual(sut.unreadNotificationCount, 0, "unreadNotificationCount should default to 0")
        XCTAssertNil(sut.lastSyncTime, "lastSyncTime should default to nil")
        XCTAssertNil(sut.connectionError, "connectionError should default to nil")
    }

    // MARK: - testConnectionStateChange
    /// Verify that connection state can be updated
    func testConnectionStateChange() {
        // Initially disconnected
        XCTAssertFalse(sut.isConnected)

        // Connect
        sut.isConnected = true
        XCTAssertTrue(sut.isConnected, "isConnected should be true after setting to true")

        // Disconnect
        sut.isConnected = false
        XCTAssertFalse(sut.isConnected, "isConnected should be false after setting to false")
    }

    // MARK: - testSyncStateToggle
    /// Verify that sync state can be toggled
    func testSyncStateToggle() {
        // Initially not syncing
        XCTAssertFalse(sut.isSyncing)

        // Start syncing
        sut.isSyncing = true
        XCTAssertTrue(sut.isSyncing, "isSyncing should be true after setting to true")

        // Stop syncing
        sut.isSyncing = false
        XCTAssertFalse(sut.isSyncing, "isSyncing should be false after setting to false")
    }

    // MARK: - testNotificationCountIncrement
    /// Verify that notification count can be incremented
    func testNotificationCountIncrement() {
        // Initially zero notifications
        XCTAssertEqual(sut.unreadNotificationCount, 0)

        // Increment to 1
        sut.unreadNotificationCount = 1
        XCTAssertEqual(sut.unreadNotificationCount, 1, "unreadNotificationCount should be 1 after incrementing")

        // Increment to 5
        sut.unreadNotificationCount = 5
        XCTAssertEqual(sut.unreadNotificationCount, 5, "unreadNotificationCount should be 5 after setting")

        // Reset to 0
        sut.unreadNotificationCount = 0
        XCTAssertEqual(sut.unreadNotificationCount, 0, "unreadNotificationCount should be 0 after reset")
    }
}
