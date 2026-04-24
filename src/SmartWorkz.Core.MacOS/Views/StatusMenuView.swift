import SwiftUI

/// StatusMenuView displays connection status, sync status, and notification information
/// Designed for use in menu bar and status bar contexts
struct StatusMenuView: View {

    // MARK: - Environment

    @EnvironmentObject private var appState: AppState

    // MARK: - State

    @State private var isHovered: Bool = false

    // MARK: - Body

    var body: some View {
        VStack(alignment: .leading, spacing: 12) {
            // Header
            HStack(spacing: 8) {
                Image(systemName: "app.connected.to.app.below.fill")
                    .font(.system(size: 14, weight: .semibold))
                    .foregroundColor(.blue)

                Text("SmartWorkz Status")
                    .font(.system(size: 14, weight: .semibold))
                    .foregroundColor(.primary)

                Spacer()
            }
            .padding(.bottom, 4)

            Divider()

            // Connection Status
            HStack(spacing: 8) {
                Circle()
                    .fill(appState.isConnected ? Color.green : Color.red)
                    .frame(width: 8, height: 8)

                VStack(alignment: .leading, spacing: 2) {
                    Text("Connection")
                        .font(.system(size: 11, weight: .semibold))
                        .foregroundColor(.secondary)

                    Text(appState.isConnected ? "Connected" : "Disconnected")
                        .font(.system(size: 12, weight: .regular))
                        .foregroundColor(appState.isConnected ? .green : .red)
                }

                Spacer()
            }

            // Sync Status
            HStack(spacing: 8) {
                if appState.isSyncing {
                    ProgressView()
                        .scaleEffect(0.75, anchor: .center)
                        .frame(width: 8, height: 8)
                } else {
                    Circle()
                        .fill(Color.blue)
                        .frame(width: 8, height: 8)
                }

                VStack(alignment: .leading, spacing: 2) {
                    Text("Sync Status")
                        .font(.system(size: 11, weight: .semibold))
                        .foregroundColor(.secondary)

                    if appState.isSyncing {
                        Text("Synchronizing...")
                            .font(.system(size: 12, weight: .regular))
                            .foregroundColor(.blue)
                    } else if let lastSyncTime = appState.lastSyncTime {
                        Text("Last: \(formattedRelativeTime(lastSyncTime))")
                            .font(.system(size: 12, weight: .regular))
                            .foregroundColor(.secondary)
                    } else {
                        Text("Idle")
                            .font(.system(size: 12, weight: .regular))
                            .foregroundColor(.secondary)
                    }
                }

                Spacer()
            }

            // Notification Status
            HStack(spacing: 8) {
                Circle()
                    .fill(appState.unreadNotificationCount > 0 ? Color.orange : Color.gray)
                    .frame(width: 8, height: 8)

                VStack(alignment: .leading, spacing: 2) {
                    Text("Notifications")
                        .font(.system(size: 11, weight: .semibold))
                        .foregroundColor(.secondary)

                    if appState.unreadNotificationCount > 0 {
                        Text("\(appState.unreadNotificationCount) unread")
                            .font(.system(size: 12, weight: .regular))
                            .foregroundColor(.orange)
                    } else {
                        Text("All caught up")
                            .font(.system(size: 12, weight: .regular))
                            .foregroundColor(.secondary)
                    }
                }

                Spacer()
            }

            // User Profile (if logged in)
            if let user = appState.currentUser {
                Divider()

                HStack(spacing: 8) {
                    Circle()
                        .fill(Color.blue.opacity(0.3))
                        .frame(width: 24, height: 24)
                        .overlay(
                            Text(user.name.prefix(1).uppercased())
                                .font(.system(size: 10, weight: .semibold))
                                .foregroundColor(.white)
                        )

                    VStack(alignment: .leading, spacing: 2) {
                        Text(user.name)
                            .font(.system(size: 12, weight: .semibold))
                            .foregroundColor(.primary)

                        Text(user.email)
                            .font(.system(size: 10, weight: .regular))
                            .foregroundColor(.secondary)
                            .lineLimit(1)
                    }

                    Spacer()
                }
            }

            // Error message if present
            if let error = appState.connectionError {
                Divider()

                HStack(spacing: 6) {
                    Image(systemName: "exclamationmark.circle.fill")
                        .font(.system(size: 12, weight: .semibold))
                        .foregroundColor(.orange)

                    Text(error)
                        .font(.system(size: 11, weight: .regular))
                        .foregroundColor(.orange)
                        .lineLimit(2)

                    Spacer()
                }
                .padding(6)
                .background(Color.orange.opacity(0.1))
                .cornerRadius(4)
            }

            Divider()

            // Quick Actions
            HStack(spacing: 8) {
                Button(action: {}) {
                    Label("Sync", systemImage: "arrow.clockwise")
                        .font(.system(size: 11, weight: .regular))
                }
                .buttonStyle(.bordered)

                Button(action: {}) {
                    Label("Settings", systemImage: "gear")
                        .font(.system(size: 11, weight: .regular))
                }
                .buttonStyle(.bordered)

                Spacer()
            }

            // Footer: Last updated
            HStack {
                Text("Updated: \(formattedUpdateTime())")
                    .font(.system(size: 9, weight: .regular))
                    .foregroundColor(.secondary)

                Spacer()
            }
        }
        .padding(12)
        .frame(minWidth: 300)
        .background(Color(.controlBackgroundColor))
    }

    // MARK: - Helper Methods

    /// Format relative time for last sync
    /// - Parameter date: The date to format
    /// - Returns: A relative time string (e.g., "2m ago")
    private func formattedRelativeTime(_ date: Date) -> String {
        let interval = Date().timeIntervalSince(date)

        if interval < 60 {
            return "just now"
        } else if interval < 3600 {
            let minutes = Int(interval / 60)
            return "\(minutes)m ago"
        } else if interval < 86400 {
            let hours = Int(interval / 3600)
            return "\(hours)h ago"
        } else {
            let days = Int(interval / 86400)
            return "\(days)d ago"
        }
    }

    /// Format the current update time
    /// - Returns: A formatted time string
    private func formattedUpdateTime() -> String {
        let formatter = DateFormatter()
        formatter.dateFormat = "HH:mm:ss"
        return formatter.string(from: Date())
    }
}

// MARK: - Previews

#if DEBUG
#Preview("Connected Status") {
    let appState = AppState()
    appState.isConnected = true
    appState.isSyncing = false
    appState.currentUser = UserProfile(
        id: "user-1",
        name: "John Doe",
        email: "john@example.com"
    )
    appState.unreadNotificationCount = 2
    appState.lastSyncTime = Date(timeIntervalSinceNow: -300)

    return StatusMenuView()
        .environmentObject(appState)
}

#Preview("Syncing Status") {
    let appState = AppState()
    appState.isConnected = true
    appState.isSyncing = true
    appState.currentUser = UserProfile(
        id: "user-1",
        name: "Jane Smith",
        email: "jane@example.com"
    )
    appState.unreadNotificationCount = 0
    appState.lastSyncTime = Date(timeIntervalSinceNow: -3600)

    return StatusMenuView()
        .environmentObject(appState)
}

#Preview("Disconnected Status") {
    let appState = AppState()
    appState.isConnected = false
    appState.connectionError = "Failed to connect to server"
    appState.unreadNotificationCount = 5

    return StatusMenuView()
        .environmentObject(appState)
}

#Preview("With Notifications") {
    let appState = AppState()
    appState.isConnected = true
    appState.isSyncing = false
    appState.currentUser = UserProfile(
        id: "user-1",
        name: "Alice Johnson",
        email: "alice@example.com"
    )
    appState.unreadNotificationCount = 10
    appState.lastSyncTime = Date(timeIntervalSinceNow: -60)

    return StatusMenuView()
        .environmentObject(appState)
}
#endif
