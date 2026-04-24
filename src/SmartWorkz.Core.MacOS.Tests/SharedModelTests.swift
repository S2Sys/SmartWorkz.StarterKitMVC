import XCTest
@testable import SmartWorkz_Core_MacOS

final class SharedModelTests: XCTestCase {

    // MARK: - RealtimeMessage Tests

    func testRealtimeMessageDecoding() {
        // Arrange
        let json = """
        {
            "messageId": "msg-123",
            "channel": "orders",
            "method": "UpdateOrder",
            "payload": "{\\"orderId\\":\\"O123\\",\\"status\\":\\"confirmed\\"}",
            "receivedAt": "2026-04-24T10:30:00Z",
            "userId": "user-456"
        }
        """.data(using: .utf8)!

        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .iso8601

        // Act
        let message = try! decoder.decode(RealtimeMessage.self, from: json)

        // Assert
        XCTAssertEqual(message.messageId, "msg-123")
        XCTAssertEqual(message.channel, "orders")
        XCTAssertEqual(message.method, "UpdateOrder")
        XCTAssertEqual(message.userId, "user-456")
        XCTAssertEqual(message.id, "msg-123")
        XCTAssertFalse(message.isSystemMessage)

        // Test payloadJson computed property
        let payloadDict = message.payloadJson
        XCTAssertNotNil(payloadDict)
        XCTAssertEqual(payloadDict?["orderId"] as? String, "O123")
        XCTAssertEqual(payloadDict?["status"] as? String, "confirmed")

        // Test age computed property
        let age = message.age
        XCTAssertGreaterThanOrEqual(age, 0)
    }

    func testRealtimeMessageSystemMessage() {
        // Arrange
        let json = """
        {
            "messageId": "msg-sys-456",
            "channel": "system",
            "method": "SystemHealthCheck",
            "payload": "{\\"status\\":\\"ok\\"}",
            "receivedAt": "2026-04-24T10:30:00Z",
            "userId": "system"
        }
        """.data(using: .utf8)!

        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .iso8601

        // Act
        let message = try! decoder.decode(RealtimeMessage.self, from: json)

        // Assert
        XCTAssertTrue(message.isSystemMessage)
        XCTAssertEqual(message.method, "SystemHealthCheck")
    }

    // MARK: - SyncChange Tests

    func testSyncChangeDecoding() {
        // Arrange
        let json = """
        {
            "changeId": "chg-789",
            "entityId": "order-789",
            "property": "status",
            "oldValue": "pending",
            "newValue": "confirmed",
            "timestamp": "2026-04-24T10:25:00Z",
            "changeType": "Update"
        }
        """.data(using: .utf8)!

        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .iso8601

        // Act
        let change = try! decoder.decode(SyncChange.self, from: json)

        // Assert
        XCTAssertEqual(change.changeId, "chg-789")
        XCTAssertEqual(change.entityId, "order-789")
        XCTAssertEqual(change.property, "status")
        XCTAssertEqual(change.oldValue, "pending")
        XCTAssertEqual(change.newValue, "confirmed")
        XCTAssertEqual(change.id, "chg-789")
        XCTAssertEqual(change.changeType, .update)
    }

    func testSyncChangeAllTypes() {
        // Test Create
        let createJson = """
        {
            "changeId": "chg-create",
            "entityId": "new-order",
            "property": "id",
            "oldValue": null,
            "newValue": "new-order",
            "timestamp": "2026-04-24T10:25:00Z",
            "changeType": "Create"
        }
        """.data(using: .utf8)!

        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .iso8601

        let createChange = try! decoder.decode(SyncChange.self, from: createJson)
        XCTAssertEqual(createChange.changeType, .create)
        XCTAssertNil(createChange.oldValue)

        // Test Delete
        let deleteJson = """
        {
            "changeId": "chg-delete",
            "entityId": "removed-order",
            "property": null,
            "oldValue": "removed-order",
            "newValue": null,
            "timestamp": "2026-04-24T10:25:00Z",
            "changeType": "Delete"
        }
        """.data(using: .utf8)!

        let deleteChange = try! decoder.decode(SyncChange.self, from: deleteJson)
        XCTAssertEqual(deleteChange.changeType, .delete)
        XCTAssertNil(deleteChange.newValue)
    }

    // MARK: - NetworkState Tests

    func testNetworkStateTransition() {
        // Arrange & Act
        let disconnected = NetworkState.disconnected
        let connecting = NetworkState.connecting
        let connected = NetworkState.connected
        let reconnecting = NetworkState.reconnecting
        let error = NetworkState.error("Connection timeout")

        // Assert
        XCTAssertFalse(disconnected.isConnected)
        XCTAssertFalse(connecting.isConnected)
        XCTAssertTrue(connected.isConnected)
        XCTAssertFalse(reconnecting.isConnected)
        XCTAssertFalse(error.isConnected)

        // Test displayName
        XCTAssertEqual(disconnected.displayName, "Disconnected")
        XCTAssertEqual(connecting.displayName, "Connecting")
        XCTAssertEqual(connected.displayName, "Connected")
        XCTAssertEqual(reconnecting.displayName, "Reconnecting")
        XCTAssertEqual(error.displayName, "Error: Connection timeout")

        // Test equality
        XCTAssertEqual(connected, NetworkState.connected)
        XCTAssertNotEqual(connected, disconnected)
    }
}
