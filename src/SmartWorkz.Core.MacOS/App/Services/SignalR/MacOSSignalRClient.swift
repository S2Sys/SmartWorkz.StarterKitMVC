import Foundation
import Combine
import SwiftUI

/// MacOSSignalRClient is the main client wrapper for SignalR real-time integration
/// Thread-safe MainActor ObservableObject that manages SignalRConnectionManager and MessageHandler
@MainActor
final class MacOSSignalRClient: ObservableObject {
    // MARK: - Published Properties

    /// Current connection state
    @Published private(set) var connectionState: SignalRConnectionManager.ConnectionState = .disconnected

    /// Whether the client is connected
    @Published private(set) var isConnected: Bool = false

    /// Publisher for state changes
    let statePublisher: AnyPublisher<SignalRConnectionManager.ConnectionState, Never>

    /// Publisher for received messages
    let messagePublisher: AnyPublisher<RealtimeMessage, Never>

    // MARK: - Private Properties

    /// Internal subject for state changes
    private let stateSubject = PassthroughSubject<SignalRConnectionManager.ConnectionState, Never>()

    /// Internal subject for received messages
    private let messageSubject = PassthroughSubject<RealtimeMessage, Never>()

    /// Connection manager instance
    private let connectionManager: SignalRConnectionManager

    /// Message handler instance
    private let messageHandler: MessageHandler

    /// Cancellables for managing subscriptions
    private var cancellables = Set<AnyCancellable>()

    // MARK: - Initialization

    init() {
        let messageHandler = MessageHandler()
        self.messageHandler = messageHandler
        self.connectionManager = SignalRConnectionManager(messageHandler: messageHandler)

        // Setup publishers with proper type erasure
        self.statePublisher = stateSubject.eraseToAnyPublisher()
        self.messagePublisher = messageSubject.eraseToAnyPublisher()

        // Emit initial state
        stateSubject.send(.disconnected)

        setupStateBinding()
    }

    // MARK: - Public Methods

    /// Connect to the SignalR hub
    /// - Parameter url: The URL of the SignalR hub
    func connect(url: String) async {
        guard !url.isEmpty else {
            await updateState(.error("Invalid URL"))
            return
        }

        do {
            try await connectionManager.connectAsync(url: url)
            let state = await connectionManager.getConnectionState()
            await updateState(state)
        } catch {
            await updateState(.error(error.localizedDescription))
        }
    }

    /// Disconnect from the SignalR hub
    func disconnect() async {
        await connectionManager.disconnectAsync()
        await updateState(.disconnected)
    }

    /// Subscribe to a channel/method
    /// - Parameters:
    ///   - channel: The channel name to subscribe to
    ///   - handler: The callback to invoke when messages arrive
    func subscribe(to channel: String, handler: @escaping (String) -> Void) async {
        guard !channel.isEmpty else { return }

        await connectionManager.subscribeToAsync(method: channel, callback: handler)
    }

    /// Send a message through the SignalR connection
    /// - Parameters:
    ///   - message: The message content
    ///   - channel: Optional channel/method name
    func sendMessage(_ message: String, to channel: String? = nil) async {
        guard !message.isEmpty else { return }

        do {
            try await connectionManager.sendMessageAsync(message: message, method: channel)
        } catch {
            await updateState(.error("Failed to send message: \(error.localizedDescription)"))
        }
    }

    /// Receive a message (internal method for testing/simulation)
    /// - Parameter message: The RealtimeMessage to process
    func receiveMessage(_ message: RealtimeMessage) async {
        messageSubject.send(message)
    }

    // MARK: - Internal Testing Methods

    /// Simulate receiving a message (for testing)
    /// - Parameters:
    ///   - message: The message content
    ///   - channel: The channel name
    func simulateMessage(_ message: String, on channel: String) async {
        // Register a handler for the channel
        await connectionManager.subscribeToAsync(method: channel) { message in
            // Handler would be invoked when message is received
        }
    }

    /// Simulate receiving a complete RealtimeMessage (for testing)
    /// - Parameter message: The RealtimeMessage to simulate
    func simulateReceiveMessage(_ message: RealtimeMessage) async {
        await receiveMessage(message)
    }

    // MARK: - Private Methods

    /// Update the connection state and isConnected flag
    /// - Parameter newState: The new connection state
    private func updateState(_ newState: SignalRConnectionManager.ConnectionState) async {
        self.connectionState = newState
        self.isConnected = newState.isConnected

        stateSubject.send(newState)
    }

    /// Setup state binding for reactive updates
    private func setupStateBinding() {
        // State changes are published through stateSubject
        // This method can be extended for additional reactive behavior
    }
}
