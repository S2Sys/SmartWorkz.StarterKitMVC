import XCTest
@testable import SmartWorkz_Core_MacOS

final class WindowManagerTests: XCTestCase {

    var sut: WindowManager!
    var mockAppState: AppState!

    override func setUp() {
        super.setUp()
        mockAppState = AppState()
        sut = WindowManager(appState: mockAppState)
    }

    override func tearDown() {
        sut.closeMainWindow()
        sut.closeSettingsWindow()
        sut = nil
        mockAppState = nil
        super.tearDown()
    }

    // MARK: - Main Window Tests

    /// Test creating the main window
    func testCreateMainWindow() {
        let window = sut.createMainWindow()

        XCTAssertNotNil(window, "Main window should be created successfully")
        XCTAssertEqual(window.title, "SmartWorkz", "Window title should be 'SmartWorkz'")
    }

    /// Test that main window caches properly
    func testMainWindowCaching() {
        let window1 = sut.createMainWindow()
        let window2 = sut.createMainWindow()

        XCTAssertEqual(
            ObjectIdentifier(window1),
            ObjectIdentifier(window2),
            "createMainWindow should return the same window instance on subsequent calls"
        )
    }

    /// Test showing main window
    func testShowMainWindow() {
        sut.showMainWindow()
        let window = sut.getMainWindow()

        XCTAssertNotNil(window, "Main window should exist after showing")
        XCTAssertTrue(sut.isMainWindowVisible(), "Main window should be visible after showMainWindow")
    }

    /// Test hiding main window
    func testHideMainWindow() {
        sut.showMainWindow()
        XCTAssertTrue(sut.isMainWindowVisible(), "Window should be visible before hiding")

        sut.hideMainWindow()
        XCTAssertFalse(sut.isMainWindowVisible(), "Window should be hidden after hideMainWindow")
    }

    /// Test closing main window
    func testCloseMainWindow() {
        sut.createMainWindow()
        XCTAssertNotNil(sut.getMainWindow(), "Main window should exist before closing")

        sut.closeMainWindow()
        XCTAssertNil(sut.getMainWindow(), "Main window should be nil after closing")
    }

    /// Test main window visibility query
    func testIsMainWindowVisible() {
        // Initially should be false
        XCTAssertFalse(sut.isMainWindowVisible(), "Main window should not be visible initially")

        // After showing
        sut.showMainWindow()
        XCTAssertTrue(sut.isMainWindowVisible(), "Main window should be visible after show")

        // After hiding
        sut.hideMainWindow()
        XCTAssertFalse(sut.isMainWindowVisible(), "Main window should not be visible after hide")
    }

    // MARK: - Settings Window Tests

    /// Test creating settings window
    func testCreateSettingsWindow() {
        let window = sut.createSettingsWindow()

        XCTAssertNotNil(window, "Settings window should be created successfully")
        XCTAssertEqual(window.title, "SmartWorkz - Settings", "Window title should be 'SmartWorkz - Settings'")
    }

    /// Test that settings window caches properly
    func testSettingsWindowCaching() {
        let window1 = sut.createSettingsWindow()
        let window2 = sut.createSettingsWindow()

        XCTAssertEqual(
            ObjectIdentifier(window1),
            ObjectIdentifier(window2),
            "createSettingsWindow should return the same window instance on subsequent calls"
        )
    }

    /// Test showing settings window
    func testShowSettingsWindow() {
        sut.showSettingsWindow()
        let window = sut.getSettingsWindow()

        XCTAssertNotNil(window, "Settings window should exist after showing")
        XCTAssertTrue(sut.isSettingsWindowVisible(), "Settings window should be visible after show")
    }

    /// Test closing settings window
    func testCloseSettingsWindow() {
        sut.createSettingsWindow()
        XCTAssertNotNil(sut.getSettingsWindow(), "Settings window should exist before closing")

        sut.closeSettingsWindow()
        XCTAssertNil(sut.getSettingsWindow(), "Settings window should be nil after closing")
    }

    /// Test settings window visibility query
    func testIsSettingsWindowVisible() {
        // Initially should be false
        XCTAssertFalse(sut.isSettingsWindowVisible(), "Settings window should not be visible initially")

        // After showing
        sut.showSettingsWindow()
        XCTAssertTrue(sut.isSettingsWindowVisible(), "Settings window should be visible after show")

        // After closing
        sut.closeSettingsWindow()
        XCTAssertFalse(sut.isSettingsWindowVisible(), "Settings window should not be visible after close")
    }

    // MARK: - Window Query Tests

    /// Test getting main window
    func testGetMainWindow() {
        XCTAssertNil(sut.getMainWindow(), "Main window should be nil before creation")

        let window = sut.createMainWindow()
        XCTAssertEqual(
            ObjectIdentifier(sut.getMainWindow()!),
            ObjectIdentifier(window),
            "getMainWindow should return the created window"
        )
    }

    /// Test getting settings window
    func testGetSettingsWindow() {
        XCTAssertNil(sut.getSettingsWindow(), "Settings window should be nil before creation")

        let window = sut.createSettingsWindow()
        XCTAssertEqual(
            ObjectIdentifier(sut.getSettingsWindow()!),
            ObjectIdentifier(window),
            "getSettingsWindow should return the created window"
        )
    }

    /// Test getting all windows
    func testGetAllWindows() {
        // Initially empty
        XCTAssertEqual(sut.getAllWindows().count, 0, "Should have no windows initially")

        // After creating main window
        sut.createMainWindow()
        XCTAssertEqual(sut.getAllWindows().count, 1, "Should have 1 window after creating main")

        // After creating settings window
        sut.createSettingsWindow()
        XCTAssertEqual(sut.getAllWindows().count, 2, "Should have 2 windows after creating settings")
    }

    // MARK: - Window Management Tests

    /// Test closing secondary windows
    func testCloseSecondaryWindows() {
        sut.createMainWindow()
        sut.createSettingsWindow()

        XCTAssertEqual(sut.getAllWindows().count, 2, "Should have 2 windows before closing secondary")

        sut.closeSecondaryWindows()

        XCTAssertNotNil(sut.getMainWindow(), "Main window should still exist")
        XCTAssertNil(sut.getSettingsWindow(), "Settings window should be closed")
    }

    /// Test removing window
    func testRemoveWindow() {
        let window = sut.createMainWindow()
        XCTAssertEqual(sut.getAllWindows().count, 1, "Should have 1 window")

        sut.removeWindow(window)
        XCTAssertEqual(sut.getAllWindows().count, 0, "Should have 0 windows after removal")
        XCTAssertNil(sut.getMainWindow(), "Main window should be removed")
    }

    // MARK: - Window Size and Configuration Tests

    /// Test main window has correct minimum size
    func testMainWindowMinimumSize() {
        let window = sut.createMainWindow()

        XCTAssertGreaterThan(window.minSize.width, 0, "Window should have minimum width")
        XCTAssertGreaterThan(window.minSize.height, 0, "Window should have minimum height")
        XCTAssertEqual(window.minSize.width, 500, "Window minimum width should be 500")
        XCTAssertEqual(window.minSize.height, 600, "Window minimum height should be 600")
    }

    /// Test settings window is fixed size
    func testSettingsWindowFixedSize() {
        let window = sut.createSettingsWindow()

        XCTAssertEqual(window.minSize.width, window.maxSize.width, "Settings window width should be fixed")
        XCTAssertEqual(window.minSize.height, window.maxSize.height, "Settings window height should be fixed")
    }

    /// Test main window style mask
    func testMainWindowStyleMask() {
        let window = sut.createMainWindow()

        let expectedMasks: NSWindow.StyleMask = [.titled, .closable, .miniaturizable, .resizable]
        XCTAssertTrue(
            window.styleMask.contains(.titled),
            "Main window should have titled style"
        )
        XCTAssertTrue(
            window.styleMask.contains(.closable),
            "Main window should be closable"
        )
        XCTAssertTrue(
            window.styleMask.contains(.resizable),
            "Main window should be resizable"
        )
    }

    // MARK: - Lifecycle Tests

    /// Test window lifecycle
    func testWindowLifecycle() {
        // Create -> Show -> Hide -> Close
        let window = sut.createMainWindow()
        XCTAssertNotNil(window, "Window should be created")

        sut.showMainWindow()
        XCTAssertTrue(sut.isMainWindowVisible(), "Window should be visible after show")

        sut.hideMainWindow()
        XCTAssertFalse(sut.isMainWindowVisible(), "Window should be hidden after hide")

        sut.closeMainWindow()
        XCTAssertNil(sut.getMainWindow(), "Window should be removed after close")
    }

    /// Test multiple show/hide cycles
    func testMultipleShowHideCycles() {
        sut.createMainWindow()

        for _ in 0..<3 {
            sut.showMainWindow()
            XCTAssertTrue(sut.isMainWindowVisible(), "Window should be visible")

            sut.hideMainWindow()
            XCTAssertFalse(sut.isMainWindowVisible(), "Window should be hidden")
        }

        XCTAssertTrue(true, "Window should handle multiple show/hide cycles")
    }

    // MARK: - Initialization Tests

    /// Test WindowManager initialization
    func testWindowManagerInitialization() {
        let manager1 = WindowManager()
        XCTAssertNotNil(manager1, "WindowManager should initialize without parameters")

        let appState = AppState()
        let manager2 = WindowManager(appState: appState)
        XCTAssertNotNil(manager2, "WindowManager should initialize with appState")
    }

    /// Test WindowManager with app state reference
    func testWindowManagerWithAppState() {
        let appState = AppState()
        appState.isConnected = true

        let manager = WindowManager(appState: appState)
        let window = manager.createMainWindow()

        XCTAssertNotNil(window, "Window should be created with app state")
    }
}
