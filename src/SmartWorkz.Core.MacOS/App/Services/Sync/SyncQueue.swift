import Foundation

final class SyncQueue {
    private var items: [SyncChange] = []
    private let lock = NSLock()

    // MARK: - Public Methods

    func enqueue(_ change: SyncChange) {
        lock.lock()
        defer { lock.unlock() }
        items.append(change)
    }

    func dequeue() -> SyncChange? {
        lock.lock()
        defer { lock.unlock() }
        guard !items.isEmpty else { return nil }
        return items.removeFirst()
    }

    func peek() -> SyncChange? {
        lock.lock()
        defer { lock.unlock() }
        return items.first
    }

    func count() -> Int {
        lock.lock()
        defer { lock.unlock() }
        return items.count
    }

    func clear() {
        lock.lock()
        defer { lock.unlock() }
        items.removeAll()
    }
}
