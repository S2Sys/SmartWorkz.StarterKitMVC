import Foundation

struct SyncChange: Identifiable, Codable {
    let changeId: String
    let entityId: String
    let property: String?
    let oldValue: String?
    let newValue: String?
    let timestamp: Date
    let changeType: ChangeType

    // MARK: - Nested Enum

    enum ChangeType: String, Codable {
        case create = "Create"
        case update = "Update"
        case delete = "Delete"
    }

    // MARK: - Identifiable

    var id: String {
        changeId
    }

    // MARK: - Codable

    enum CodingKeys: String, CodingKey {
        case changeId = "changeId"
        case entityId = "entityId"
        case property = "property"
        case oldValue = "oldValue"
        case newValue = "newValue"
        case timestamp = "timestamp"
        case changeType = "changeType"
    }
}
