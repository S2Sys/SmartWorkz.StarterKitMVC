import Foundation

final class ConflictResolver {

    // MARK: - Nested Enum

    enum ConflictResolutionStrategy {
        case lastWriteWins
        case clientWins
        case serverWins
    }

    // MARK: - Public Methods

    func detectConflict(_ localChange: SyncChange, _ remoteChange: SyncChange) -> Bool {
        // Conflict exists if both changes target the same entity and property
        return localChange.entityId == remoteChange.entityId &&
               localChange.property == remoteChange.property &&
               localChange.changeType == .update &&
               remoteChange.changeType == .update
    }

    func resolve(
        localChange: SyncChange,
        remoteChange: SyncChange,
        strategy: ConflictResolutionStrategy
    ) -> SyncChange {
        switch strategy {
        case .lastWriteWins:
            return localChange.timestamp > remoteChange.timestamp ? localChange : remoteChange

        case .clientWins:
            return localChange

        case .serverWins:
            return remoteChange
        }
    }
}
