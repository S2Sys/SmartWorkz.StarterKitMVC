import Foundation
import Combine

/// SignalRConnectionManager manages the connection lifecycle with thread-safe operations
/// Uses an Actor to ensure thread-safe state management
actor SignalRConnectionManager {
    // MARK: - Type Definitions

    /// Enum representing different connection states
    enum ConnectionState: Equatable {
        case disconnected
        case connecting
        case connected
        case reconnecting
        case error(String)

        var isConnected: Bool {
            switch self {
            case .connected:
                return true
            default:
                return false
            }
        }
    }

    // MARK: - Private Properties

    /// Current connection state
    private var connectionState: ConnectionState = .disconnected

    /// URLSessionWebSocketTask for WebSocket communication
    private var webSocket: URLSessionWebSocketTask?

    /// URL of the SignalR hub
    private var hubURL: URL?

    /// Message handler for routing messages
    private let messageHandler: MessageHandler

    /// Receive task for handling incoming messages
    private var receiveTask: Task<Void, Never>?

    // MARK: - Initialization

    init(messageHandler: MessageHandler) {
        self.messageHandler = messageHandler
    }

    // MARK: - Public Methods

    /// Get the current connection state
    /// - Returns: Current ConnectionState
    func getConnectionState() -> ConnectionState {
        return connectionState
    }

    /// Connect to the SignalR hub
    /// - Parameter url: The URL of the SignalR hub
    /// - Throws: URLError if connection fails
    func connectAsync(url: String) async throws {
        guard !url.isEmpty else {
            connectionState = .error("Invalid URL provided")
            throw URLError(.badURL)
        }

        guard let urlObject = URL(string: url) else {
            connectionState = .error("Failed to parse URL")
            throw URLError(.badURL)
        }

        connectionState = .connecting
        hubURL = urlObject

        // Create URLSessionWebSocketTask
        let session = URLSession(configuration: .default)
        let webSocketTask = session.webSocketTask(with: urlObject)
        webSocketTask.resume()

        self.webSocket = webSocketTask

        // Set state to connected after successful creation
        connectionState = .connected

        // Start receiving messages
        startReceivingMessages()
    }

    /// Disconnect from the SignalR hub
    func disconnectAsync() {
        if let webSocket = webSocket {
            webSocket.cancel(with: .goingAway, reason: nil)
            self.webSocket = nil
        }

        connectionState = .disconnected
        hubURL = nil

        // Cancel receive task
        receiveTask?.cancel()
        receiveTask = nil
    }

    /// Subscribe to a specific channel/method
    /// - Parameters:
    ///   - method: The method or channel name
    ///   - callback: The callback to invoke when messages arrive
    func subscribeToAsync(method: String, callback: @escaping (String) -> Void) {
        guard !method.isEmpty else { return }

        Task {
            await messageHandler.registerHandler(for: method, callback: callback)
        }
    }

    /// Send a message through the WebSocket
    /// - Parameters:
    ///   - message: The message content
    ///   - method: Optional method name for routing
    /// - Throws: URLSessionWebSocketTask.WebSocketError if sending fails
    func sendMessageAsync(message: String, method: String? = nil) async throws {
        guard let webSocket = webSocket else {
            connectionState = .error("Not connected to SignalR hub")
            throw URLError(.networkConnectionLost)
        }

        guard !message.isEmpty else {
            throw URLError(.badServerResponse)
        }

        let jsonMessage = createSignalRMessage(message, method: method)

        do {
            try await webSocket.send(.string(jsonMessage))
        } catch {
            connectionState = .error("Failed to send message: \(error.localizedDescription)")
            throw error
        }
    }

    // MARK: - Private Methods

    /// Start receiving messages from the WebSocket
    private func startReceivingMessages() {
        receiveTask = Task {
            while !Task.isCancelled {
                do {
                    guard let webSocket = webSocket else { break }

                    let message = try await webSocket.receive()

                    switch message {
                    case .string(let text):
                        await handleReceivedMessage(text)
                    case .data(let data):
                        if let text = String(data: data, encoding: .utf8) {
                            await handleReceivedMessage(text)
                        }
                    @unknown default:
                        break
                    }
                } catch {
                    if !Task.isCancelled {
                        connectionState = .error("WebSocket receive failed: \(error.localizedDescription)")
                    }
                    break
                }
            }
        }
    }

    /// Handle a received message from the WebSocket
    /// - Parameter messageContent: The message content as a string
    private func handleReceivedMessage(_ messageContent: String) {
        // Parse the SignalR message and route it through the message handler
        // For now, we'll call handlers with the raw message content
        // In production, this would parse SignalR protocol messages

        // Extract method name if present (simplified parsing)
        let method = extractMethodName(from: messageContent) ?? "default"
        messageHandler.handleMessage(for: method, message: messageContent)
    }

    /// Extract method name from a SignalR message (simplified)
    /// - Parameter message: The raw message content
    /// - Returns: The extracted method name or nil
    private func extractMethodName(from message: String) -> String? {
        // Simplified extraction - in production, use proper SignalR protocol parsing
        if let data = message.data(using: .utf8),
           let json = try? JSONSerialization.jsonObject(with: data) as? [String: Any],
           let method = json["method"] as? String {
            return method
        }
        return nil
    }

    /// Create a SignalR protocol message
    /// - Parameters:
    ///   - message: The message content
    ///   - method: Optional method name
    /// - Returns: JSON string suitable for SignalR
    private func createSignalRMessage(_ message: String, method: String?) -> String {
        var payload: [String: Any] = [
            "type": 1,
            "target": method ?? "send",
            "arguments": [message]
        ]

        if let jsonData = try? JSONSerialization.data(withJSONObject: payload),
           let jsonString = String(data: jsonData, encoding: .utf8) {
            return jsonString + "\u{1E}"
        }

        return message
    }
}
