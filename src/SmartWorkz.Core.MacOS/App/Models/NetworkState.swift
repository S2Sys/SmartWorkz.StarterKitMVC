import Foundation

enum NetworkState: Equatable {
    case disconnected
    case connecting
    case connected
    case reconnecting
    case error(String)

    // MARK: - Computed Properties

    var isConnected: Bool {
        switch self {
        case .connected:
            return true
        default:
            return false
        }
    }

    var displayName: String {
        switch self {
        case .disconnected:
            return "Disconnected"
        case .connecting:
            return "Connecting"
        case .connected:
            return "Connected"
        case .reconnecting:
            return "Reconnecting"
        case .error(let message):
            return "Error: \(message)"
        }
    }
}
