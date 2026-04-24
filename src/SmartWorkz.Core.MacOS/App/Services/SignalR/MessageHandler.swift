import Foundation

/// MessageHandler manages message routing with thread-safe handler registration
/// Uses a dictionary to map method names to their corresponding callback handlers
actor MessageHandler {
    // MARK: - Type Definitions

    /// Type alias for message handler callbacks
    typealias MessageCallback = (String) -> Void

    // MARK: - Private Properties

    /// Thread-safe dictionary mapping method names to handler callbacks
    private var handlers: [String: MessageCallback] = [:]

    // MARK: - Initialization

    init() {
        // Initialize empty handlers dictionary
    }

    // MARK: - Public Methods

    /// Register a handler for a specific method/channel
    /// - Parameters:
    ///   - method: The method or channel name to listen for
    ///   - callback: The callback to invoke when a message is received
    func registerHandler(for method: String, callback: @escaping MessageCallback) {
        guard !method.isEmpty else { return }
        handlers[method] = callback
    }

    /// Handle an incoming message by routing to registered handlers
    /// - Parameters:
    ///   - method: The method or channel name
    ///   - message: The message content
    func handleMessage(for method: String, message: String) {
        guard !method.isEmpty else { return }

        if let callback = handlers[method] {
            callback(message)
        }
    }

    /// Get the current handler for a specific method (for testing)
    /// - Parameter method: The method name
    /// - Returns: The registered callback if one exists
    func getHandler(for method: String) -> MessageCallback? {
        return handlers[method]
    }

    /// Clear all registered handlers
    func clearHandlers() {
        handlers.removeAll()
    }

    /// Get count of registered handlers (for debugging/testing)
    /// - Returns: Number of registered handlers
    func handlerCount() -> Int {
        return handlers.count
    }
}
