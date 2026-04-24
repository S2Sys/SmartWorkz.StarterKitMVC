import XCTest
@testable import SmartWorkz_Core_MacOS

final class OfflineSyncManagerTests: XCTestCase {

    var sut: OfflineSyncManager!
    var mockCoreDataStack: CoreDataStack!

    override func setUp() {
        super.setUp()
        mockCoreDataStack = CoreDataStack.shared
        sut = OfflineSyncManager()
    }

    override func tearDown() {
        sut = nil
        super.tearDown()
    }

    // MARK: - testQueueChange
    /// Verify that changes can be queued and tracked
    func testQueueChange() {
        let change = SyncChange(
            changeId: "test-1",
            entityId: "entity-1",
            property: "name",
            oldValue: "old",
            newValue: "new",
            timestamp: Date(),
            changeType: .update
        )

        sut.queueChange(change)
        XCTAssertEqual(sut.pendingChangeCount(), 1, "pendingChangeCount should be 1 after queueing one change")
    }

    // MARK: - testQueueMultipleChanges
    /// Verify that multiple changes can be queued
    func testQueueMultipleChanges() {
        let changes = (1...5).map { i in
            SyncChange(
                changeId: "test-\(i)",
                entityId: "entity-\(i)",
                property: "property\(i)",
                oldValue: "old\(i)",
                newValue: "new\(i)",
                timestamp: Date(),
                changeType: .update
            )
        }

        changes.forEach { sut.queueChange($0) }
        XCTAssertEqual(sut.pendingChangeCount(), 5, "pendingChangeCount should be 5 after queueing 5 changes")
    }

    // MARK: - testSyncAsync
    /// Verify that sync processes pending changes asynchronously
    func testSyncAsync() {
        let change = SyncChange(
            changeId: "test-1",
            entityId: "entity-1",
            property: "name",
            oldValue: nil,
            newValue: "new",
            timestamp: Date(),
            changeType: .create
        )

        sut.queueChange(change)
        let expectation = expectation(description: "sync should complete")

        Task { @MainActor in
            await sut.syncAsync()
            expectation.fulfill()
        }

        waitForExpectations(timeout: 5.0)
        XCTAssertEqual(sut.pendingChangeCount(), 0, "pendingChangeCount should be 0 after sync")
    }

    // MARK: - testClearPendingChanges
    /// Verify that pending changes can be cleared
    func testClearPendingChanges() {
        let changes = (1...3).map { i in
            SyncChange(
                changeId: "test-\(i)",
                entityId: "entity-\(i)",
                property: nil,
                oldValue: nil,
                newValue: "value",
                timestamp: Date(),
                changeType: .create
            )
        }

        changes.forEach { sut.queueChange($0) }
        XCTAssertEqual(sut.pendingChangeCount(), 3)

        sut.clearPendingChanges()
        XCTAssertEqual(sut.pendingChangeCount(), 0, "pendingChangeCount should be 0 after clearing")
    }

    // MARK: - testPendingChangeCountInitialization
    /// Verify that pendingChangeCount starts at zero
    func testPendingChangeCountInitialization() {
        XCTAssertEqual(sut.pendingChangeCount(), 0, "pendingChangeCount should initialize to 0")
    }
}

final class SyncQueueTests: XCTestCase {

    var sut: SyncQueue!

    override func setUp() {
        super.setUp()
        sut = SyncQueue()
    }

    override func tearDown() {
        sut = nil
        super.tearDown()
    }

    // MARK: - testEnqueueAndDequeue
    /// Verify that changes can be enqueued and dequeued in FIFO order
    func testEnqueueAndDequeue() {
        let change1 = SyncChange(
            changeId: "test-1",
            entityId: "entity-1",
            property: "prop1",
            oldValue: nil,
            newValue: "val1",
            timestamp: Date(),
            changeType: .create
        )
        let change2 = SyncChange(
            changeId: "test-2",
            entityId: "entity-2",
            property: "prop2",
            oldValue: nil,
            newValue: "val2",
            timestamp: Date(),
            changeType: .create
        )

        sut.enqueue(change1)
        sut.enqueue(change2)

        let dequeued1 = sut.dequeue()
        XCTAssertEqual(dequeued1?.changeId, "test-1", "First dequeued item should be test-1")

        let dequeued2 = sut.dequeue()
        XCTAssertEqual(dequeued2?.changeId, "test-2", "Second dequeued item should be test-2")
    }

    // MARK: - testPeek
    /// Verify that peek returns the front element without removing it
    func testPeek() {
        let change = SyncChange(
            changeId: "test-1",
            entityId: "entity-1",
            property: nil,
            oldValue: nil,
            newValue: "value",
            timestamp: Date(),
            changeType: .create
        )

        sut.enqueue(change)

        let peeked1 = sut.peek()
        XCTAssertEqual(peeked1?.changeId, "test-1", "Peeked item should be test-1")

        let peeked2 = sut.peek()
        XCTAssertEqual(peeked2?.changeId, "test-1", "Peeked item should still be test-1 (not removed)")

        let count = sut.count()
        XCTAssertEqual(count, 1, "Count should be 1 (item not removed by peek)")
    }

    // MARK: - testCount
    /// Verify that count returns the correct queue size
    func testCount() {
        XCTAssertEqual(sut.count(), 0, "Initial count should be 0")

        let changes = (1...3).map { i in
            SyncChange(
                changeId: "test-\(i)",
                entityId: "entity-\(i)",
                property: nil,
                oldValue: nil,
                newValue: "value\(i)",
                timestamp: Date(),
                changeType: .create
            )
        }

        changes.forEach { sut.enqueue($0) }
        XCTAssertEqual(sut.count(), 3, "Count should be 3 after enqueuing 3 items")

        sut.dequeue()
        XCTAssertEqual(sut.count(), 2, "Count should be 2 after dequeuing one item")
    }

    // MARK: - testClear
    /// Verify that clear removes all items from the queue
    func testClear() {
        let changes = (1...3).map { i in
            SyncChange(
                changeId: "test-\(i)",
                entityId: "entity-\(i)",
                property: nil,
                oldValue: nil,
                newValue: "value\(i)",
                timestamp: Date(),
                changeType: .create
            )
        }

        changes.forEach { sut.enqueue($0) }
        XCTAssertEqual(sut.count(), 3)

        sut.clear()
        XCTAssertEqual(sut.count(), 0, "Count should be 0 after clearing")
        XCTAssertNil(sut.dequeue(), "Dequeue should return nil after clearing")
    }

    // MARK: - testThreadSafety
    /// Verify that queue operations are thread-safe
    func testThreadSafety() {
        let expectation = expectation(description: "All threads complete")
        expectation.expectedFulfillmentCount = 10

        DispatchQueue.concurrentPerform(iterations: 10) { index in
            let change = SyncChange(
                changeId: "test-\(index)",
                entityId: "entity-\(index)",
                property: nil,
                oldValue: nil,
                newValue: "value\(index)",
                timestamp: Date(),
                changeType: .create
            )
            self.sut.enqueue(change)
            expectation.fulfill()
        }

        waitForExpectations(timeout: 5.0)
        XCTAssertEqual(sut.count(), 10, "Queue should contain all 10 enqueued items despite concurrent access")
    }
}

final class ConflictResolverTests: XCTestCase {

    var sut: ConflictResolver!

    override func setUp() {
        super.setUp()
        sut = ConflictResolver()
    }

    override func tearDown() {
        sut = nil
        super.tearDown()
    }

    // MARK: - testDetectConflict
    /// Verify that conflicts are detected between local and remote changes
    func testDetectConflict() {
        let localChange = SyncChange(
            changeId: "local-1",
            entityId: "entity-1",
            property: "name",
            oldValue: "original",
            newValue: "local-update",
            timestamp: Date(),
            changeType: .update
        )

        let remoteChange = SyncChange(
            changeId: "remote-1",
            entityId: "entity-1",
            property: "name",
            oldValue: "original",
            newValue: "remote-update",
            timestamp: Date().addingTimeInterval(-10),
            changeType: .update
        )

        let hasConflict = sut.detectConflict(localChange, remoteChange)
        XCTAssertTrue(hasConflict, "Should detect conflict when both modify same property")
    }

    // MARK: - testNoConflictDifferentProperties
    /// Verify that no conflict is detected for different properties
    func testNoConflictDifferentProperties() {
        let localChange = SyncChange(
            changeId: "local-1",
            entityId: "entity-1",
            property: "name",
            oldValue: nil,
            newValue: "Alice",
            timestamp: Date(),
            changeType: .update
        )

        let remoteChange = SyncChange(
            changeId: "remote-1",
            entityId: "entity-1",
            property: "email",
            oldValue: nil,
            newValue: "test@example.com",
            timestamp: Date().addingTimeInterval(-10),
            changeType: .update
        )

        let hasConflict = sut.detectConflict(localChange, remoteChange)
        XCTAssertFalse(hasConflict, "Should not detect conflict when properties differ")
    }

    // MARK: - testLastWriteWinsStrategy
    /// Verify that last-write-wins strategy resolves conflicts based on timestamp
    func testLastWriteWinsStrategy() {
        let olderChange = SyncChange(
            changeId: "old-1",
            entityId: "entity-1",
            property: "status",
            oldValue: nil,
            newValue: "pending",
            timestamp: Date().addingTimeInterval(-100),
            changeType: .update
        )

        let newerChange = SyncChange(
            changeId: "new-1",
            entityId: "entity-1",
            property: "status",
            oldValue: nil,
            newValue: "completed",
            timestamp: Date(),
            changeType: .update
        )

        let resolved = sut.resolve(
            localChange: olderChange,
            remoteChange: newerChange,
            strategy: .lastWriteWins
        )

        XCTAssertEqual(resolved.changeId, "new-1", "lastWriteWins should select the newer change")
        XCTAssertEqual(resolved.newValue, "completed")
    }

    // MARK: - testClientWinsStrategy
    /// Verify that client-wins strategy prefers local changes
    func testClientWinsStrategy() {
        let localChange = SyncChange(
            changeId: "local-1",
            entityId: "entity-1",
            property: "priority",
            oldValue: nil,
            newValue: "high",
            timestamp: Date().addingTimeInterval(-50),
            changeType: .update
        )

        let remoteChange = SyncChange(
            changeId: "remote-1",
            entityId: "entity-1",
            property: "priority",
            oldValue: nil,
            newValue: "low",
            timestamp: Date(),
            changeType: .update
        )

        let resolved = sut.resolve(
            localChange: localChange,
            remoteChange: remoteChange,
            strategy: .clientWins
        )

        XCTAssertEqual(resolved.changeId, "local-1", "clientWins should always select local change")
        XCTAssertEqual(resolved.newValue, "high")
    }

    // MARK: - testServerWinsStrategy
    /// Verify that server-wins strategy prefers remote changes
    func testServerWinsStrategy() {
        let localChange = SyncChange(
            changeId: "local-1",
            entityId: "entity-1",
            property: "version",
            oldValue: nil,
            newValue: "1.0",
            timestamp: Date(),
            changeType: .update
        )

        let remoteChange = SyncChange(
            changeId: "remote-1",
            entityId: "entity-1",
            property: "version",
            oldValue: nil,
            newValue: "2.0",
            timestamp: Date().addingTimeInterval(-100),
            changeType: .update
        )

        let resolved = sut.resolve(
            localChange: localChange,
            remoteChange: remoteChange,
            strategy: .serverWins
        )

        XCTAssertEqual(resolved.changeId, "remote-1", "serverWins should always select remote change")
        XCTAssertEqual(resolved.newValue, "2.0")
    }
}

final class CoreDataStackTests: XCTestCase {

    override func tearDown() {
        // Reset singleton between tests
        CoreDataStack.resetForTesting()
        super.tearDown()
    }

    // MARK: - testSingletonInitialization
    /// Verify that CoreDataStack is a singleton
    func testSingletonInitialization() {
        let stack1 = CoreDataStack.shared
        let stack2 = CoreDataStack.shared

        XCTAssertTrue(stack1 === stack2, "CoreDataStack should be a singleton")
    }

    // MARK: - testPersistentContainerExists
    /// Verify that NSPersistentContainer is initialized
    func testPersistentContainerExists() {
        let stack = CoreDataStack.shared
        XCTAssertNotNil(stack.persistentContainer, "persistentContainer should not be nil")
    }

    // MARK: - testViewContextExists
    /// Verify that viewContext is available and configured
    func testViewContextExists() {
        let stack = CoreDataStack.shared
        let context = stack.viewContext

        XCTAssertNotNil(context, "viewContext should not be nil")
        XCTAssertEqual(context.automaticallyMergesChangesFromParent, true, "viewContext should auto-merge changes")
    }

    // MARK: - testMergePolicyConfiguration
    /// Verify that merge policy is set correctly
    func testMergePolicyConfiguration() {
        let stack = CoreDataStack.shared
        let context = stack.viewContext

        XCTAssertNotNil(context.mergePolicy, "Merge policy should be configured")
        // The merge policy should be one of the standard Core Data merge policies
        let mergePolicy = context.mergePolicy
        XCTAssertTrue(
            mergePolicy is NSMergeByPropertyObjectTrumpMergePolicy ||
            mergePolicy is NSMergeByPropertyStoreTrumpMergePolicy ||
            mergePolicy is NSOverwriteMergePolicy ||
            mergePolicy is NSRollbackMergePolicy,
            "Merge policy should be a standard Core Data merge policy"
        )
    }
}
