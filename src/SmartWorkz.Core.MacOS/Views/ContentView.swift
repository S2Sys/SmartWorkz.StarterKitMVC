import SwiftUI

/// Main content view for SmartWorkz macOS application
struct ContentView: View {

    // MARK: - Environment

    @EnvironmentObject private var appState: AppState

    // MARK: - Body

    var body: some View {
        VStack(spacing: 20) {
            // Header: App Title
            VStack(spacing: 8) {
                Text("SmartWorkz")
                    .font(.system(size: 32, weight: .bold, design: .default))
                    .foregroundColor(.primary)

                Text("macOS Desktop Client")
                    .font(.system(size: 14, weight: .regular, design: .default))
                    .foregroundColor(.secondary)
            }
            .padding(.top, 20)

            Divider()

            // Connection Status Section
            VStack(alignment: .leading, spacing: 12) {
                HStack(spacing: 8) {
                    Image(systemName: appState.isConnected ? "wifi" : "wifi.slash")
                        .font(.system(size: 16, weight: .semibold))
                        .foregroundColor(appState.isConnected ? .green : .red)

                    Text(appState.isConnected ? "Connected" : "Disconnected")
                        .font(.system(size: 14, weight: .regular))
                        .foregroundColor(appState.isConnected ? .green : .red)

                    Spacer()
                }

                // Error message if present
                if let error = appState.connectionError {
                    HStack(spacing: 8) {
                        Image(systemName: "exclamationmark.circle.fill")
                            .foregroundColor(.orange)

                        Text(error)
                            .font(.system(size: 12, weight: .regular))
                            .foregroundColor(.orange)
                            .lineLimit(2)
                    }
                    .padding(8)
                    .background(Color.orange.opacity(0.1))
                    .cornerRadius(6)
                }
            }
            .padding(.horizontal)

            // Sync Progress Section
            if appState.isSyncing {
                VStack(alignment: .leading, spacing: 12) {
                    HStack(spacing: 8) {
                        ProgressView()
                            .scaleEffect(0.9, anchor: .center)

                        Text("Synchronizing...")
                            .font(.system(size: 14, weight: .regular))
                            .foregroundColor(.primary)

                        Spacer()
                    }

                    if let lastSyncTime = appState.lastSyncTime {
                        Text("Last sync: \(formattedDate(lastSyncTime))")
                            .font(.system(size: 12, weight: .regular))
                            .foregroundColor(.secondary)
                    }
                }
                .padding(12)
                .background(Color.blue.opacity(0.1))
                .cornerRadius(6)
                .padding(.horizontal)
            } else if let lastSyncTime = appState.lastSyncTime {
                HStack(spacing: 8) {
                    Image(systemName: "checkmark.circle.fill")
                        .foregroundColor(.green)

                    Text("Last sync: \(formattedDate(lastSyncTime))")
                        .font(.system(size: 12, weight: .regular))
                        .foregroundColor(.secondary)

                    Spacer()
                }
                .padding(.horizontal)
            }

            // User Profile Section
            if let user = appState.currentUser {
                VStack(alignment: .leading, spacing: 8) {
                    Text("Logged in as")
                        .font(.system(size: 12, weight: .semibold))
                        .foregroundColor(.secondary)

                    HStack(spacing: 12) {
                        Circle()
                            .fill(Color.blue.opacity(0.3))
                            .frame(width: 40, height: 40)
                            .overlay(
                                Text(user.name.prefix(1).uppercased())
                                    .font(.system(size: 16, weight: .semibold))
                                    .foregroundColor(.white)
                            )

                        VStack(alignment: .leading, spacing: 2) {
                            Text(user.name)
                                .font(.system(size: 14, weight: .semibold))
                                .foregroundColor(.primary)

                            Text(user.email)
                                .font(.system(size: 12, weight: .regular))
                                .foregroundColor(.secondary)
                        }

                        Spacer()
                    }
                }
                .padding(12)
                .background(Color.gray.opacity(0.05))
                .cornerRadius(6)
                .padding(.horizontal)
            }

            // Notifications Badge
            HStack(spacing: 12) {
                if appState.unreadNotificationCount > 0 {
                    HStack(spacing: 8) {
                        Image(systemName: "bell.badge.fill")
                            .foregroundColor(.orange)

                        Text("\(appState.unreadNotificationCount) notification\(appState.unreadNotificationCount == 1 ? "" : "s")")
                            .font(.system(size: 12, weight: .regular))
                            .foregroundColor(.orange)
                    }
                } else {
                    HStack(spacing: 8) {
                        Image(systemName: "bell")
                            .foregroundColor(.gray)

                        Text("No new notifications")
                            .font(.system(size: 12, weight: .regular))
                            .foregroundColor(.secondary)
                    }
                }

                Spacer()
            }
            .padding(.horizontal)

            Spacer()

            // Footer: Version Info
            Text("v1.0.0")
                .font(.system(size: 11, weight: .regular))
                .foregroundColor(.secondary)
                .padding(.bottom, 12)
        }
        .frame(maxWidth: .infinity, maxHeight: .infinity)
        .background(Color(.controlBackgroundColor))
    }

    // MARK: - Helper Methods

    private func formattedDate(_ date: Date) -> String {
        let formatter = DateFormatter()
        formatter.dateStyle = .medium
        formatter.timeStyle = .short
        return formatter.string(from: date)
    }
}

#if DEBUG
#Preview("Connected State") {
    let appState = AppState()
    appState.isConnected = true
    appState.currentUser = UserProfile(
        id: "user-123",
        name: "John Doe",
        email: "john@example.com",
        avatarURL: nil
    )
    appState.unreadNotificationCount = 3
    appState.lastSyncTime = Date()

    return ContentView()
        .environmentObject(appState)
        .frame(width: 500, height: 600)
}

#Preview("Disconnected State") {
    let appState = AppState()
    appState.isConnected = false
    appState.connectionError = "Failed to connect to server"

    return ContentView()
        .environmentObject(appState)
        .frame(width: 500, height: 400)
}

#Preview("Syncing State") {
    let appState = AppState()
    appState.isConnected = true
    appState.isSyncing = true
    appState.currentUser = UserProfile(
        id: "user-123",
        name: "Jane Smith",
        email: "jane@example.com",
        avatarURL: nil
    )
    appState.lastSyncTime = Date()

    return ContentView()
        .environmentObject(appState)
        .frame(width: 500, height: 500)
}
#endif
