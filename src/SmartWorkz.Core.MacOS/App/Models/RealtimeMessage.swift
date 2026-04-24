import Foundation

struct RealtimeMessage: Identifiable, Codable {
    let messageId: String
    let channel: String
    let method: String
    let payload: String
    let receivedAt: Date
    let userId: String

    // MARK: - Identifiable

    var id: String {
        messageId
    }

    // MARK: - Computed Properties

    var payloadJson: [String: Any]? {
        guard let data = payload.data(using: .utf8) else {
            return nil
        }
        return try? JSONSerialization.jsonObject(with: data, options: []) as? [String: Any]
    }

    var isSystemMessage: Bool {
        method.lowercased().hasPrefix("system")
    }

    var age: TimeInterval {
        Date().timeIntervalSince(receivedAt)
    }

    // MARK: - Codable

    enum CodingKeys: String, CodingKey {
        case messageId = "messageId"
        case channel = "channel"
        case method = "method"
        case payload = "payload"
        case receivedAt = "receivedAt"
        case userId = "userId"
    }
}
