import XCTest
import Combine
@testable import SmartWorkz_Core_MacOS

final class SignalRClientTests: XCTestCase {
    var sut: MacOSSignalRClient!
    var cancellables: Set<AnyCancellable>!

    override func setUp() {
        super.setUp()
        sut = MacOSSignalRClient()
        cancellables = []
    }

    override func tearDown() {
        sut = nil
        cancellables = nil
        super.tearDown()
    }

    // MARK: - testClientInitialization
    /// Verify that MacOSSignalRClient initializes with correct default values
    func testClientInitialization() {
        XCTAssertNotNil(sut, "MacOSSignalRClient should initialize successfully")
        XCTAssertFalse(sut.isConnected, "isConnected should default to false")

        // Verify state publisher exists and can be subscribed to
        var stateReceived = false
        sut.statePublisher
            .sink { _ in
                stateReceived = true
            }
            .store(in: &cancellables)

        XCTAssertTrue(stateReceived, "statePublisher should emit initial state")
    }

    // MARK: - testConnectUpdatesState
    /// Verify that connecting updates the connection state
    func testConnectUpdatesState() {
        let expectation = XCTestExpectation(description: "Connect should update isConnected state")

        // Subscribe to state changes
        sut.statePublisher
            .dropFirst() // Skip initial state
            .sink { state in
                if state == .connected {
                    expectation.fulfill()
                }
            }
            .store(in: &cancellables)

        // Attempt to connect
        Task {
            await sut.connect(url: "ws://localhost:5000/signalr")
        }

        // Wait for state update
        wait(for: [expectation], timeout: 2.0)
        XCTAssertTrue(sut.isConnected, "isConnected should be true after successful connection")
    }

    // MARK: - testSubscribeToChannel
    /// Verify that subscribing to a channel works correctly
    func testSubscribeToChannel() {
        let expectation = XCTestExpectation(description: "Subscribe should register handler for channel")

        let channelName = "testChannel"
        let testMessage = "Hello World"

        // Subscribe to channel with handler
        Task {
            await sut.subscribe(to: channelName) { message in
                if message == testMessage {
                    expectation.fulfill()
                }
            }

            // Simulate receiving a message (for testing purposes)
            await sut.simulateMessage(testMessage, on: channelName)
        }

        wait(for: [expectation], timeout: 2.0)
    }

    // MARK: - testMessageReceived
    /// Verify that messages are properly routed through the message publisher
    func testMessageReceived() {
        let expectation = XCTestExpectation(description: "Message should be emitted through messagePublisher")

        let messageId = UUID().uuidString
        let testMessage = RealtimeMessage(
            messageId: messageId,
            channel: "notifications",
            method: "receiveNotification",
            payload: "Test payload",
            receivedAt: Date(),
            userId: "testUser123"
        )

        // Subscribe to message publisher
        sut.messagePublisher
            .sink { message in
                if message.messageId == messageId {
                    expectation.fulfill()
                }
            }
            .store(in: &cancellables)

        // Simulate receiving a message
        Task {
            await sut.simulateReceiveMessage(testMessage)
        }

        wait(for: [expectation], timeout: 2.0)
    }
}
