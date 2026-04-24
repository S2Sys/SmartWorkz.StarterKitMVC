import Foundation
import Combine

/// UserProfile represents the currently logged-in user
struct UserProfile: Identifiable, Codable {
    let id: String
    let name: String
    let email: String
    let avatarURL: URL?

    init(id: String, name: String, email: String, avatarURL: URL? = nil) {
        self.id = id
        self.name = name
        self.email = email
        self.avatarURL = avatarURL
    }
}

/// AppState manages the global application state using Combine framework
/// Thread-safe with @MainActor annotation for UI updates
@MainActor
final class AppState: ObservableObject {
    // MARK: - Published Properties

    /// Indicates whether the app is connected to the server
    @Published var isConnected: Bool = false

    /// Indicates whether data synchronization is in progress
    @Published var isSyncing: Bool = false

    /// Currently logged-in user profile
    @Published var currentUser: UserProfile? = nil

    /// Count of unread notifications
    @Published var unreadNotificationCount: Int = 0

    /// Timestamp of the last successful synchronization
    @Published var lastSyncTime: Date? = nil

    /// Error message if connection fails
    @Published var connectionError: String? = nil

    // MARK: - Initialization

    init() {
        setupBindings()
    }

    // MARK: - Private Methods

    private func setupBindings() {
        // Future bindings for reactive updates can be added here
        // This is where Combine chains and subscriptions would be configured
    }

    // MARK: - Public Methods

    /// Reset the app state to initial values
    func reset() {
        isConnected = false
        isSyncing = false
        currentUser = nil
        unreadNotificationCount = 0
        lastSyncTime = nil
        connectionError = nil
    }

    /// Update the current user profile
    func setCurrentUser(_ user: UserProfile) {
        self.currentUser = user
    }

    /// Clear the current user (logout)
    func clearCurrentUser() {
        self.currentUser = nil
    }

    /// Update the last sync time to now
    func updateLastSyncTime() {
        self.lastSyncTime = Date()
    }

    /// Increment the unread notification count
    func incrementNotificationCount() {
        self.unreadNotificationCount += 1
    }

    /// Decrement the unread notification count (if > 0)
    func decrementNotificationCount() {
        if unreadNotificationCount > 0 {
            self.unreadNotificationCount -= 1
        }
    }

    /// Clear all notifications
    func clearNotifications() {
        self.unreadNotificationCount = 0
    }
}
