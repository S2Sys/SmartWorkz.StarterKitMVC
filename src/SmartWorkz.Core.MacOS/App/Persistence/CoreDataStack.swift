import CoreData
import Foundation

final class CoreDataStack {
    static let shared = CoreDataStack()

    let persistentContainer: NSPersistentContainer
    var viewContext: NSManagedObjectContext {
        persistentContainer.viewContext
    }

    private init() {
        persistentContainer = NSPersistentContainer(name: "SmartWorkz")

        // Configure the persistent store
        let description = NSPersistentStoreDescription()
        description.shouldDeletePersistentStoreOnModelMismatch = false
        persistentContainer.persistentStoreDescriptions = [description]

        persistentContainer.loadPersistentStores { _, error in
            if let error = error {
                fatalError("Failed to load persistent stores: \(error)")
            }
        }

        // Configure view context
        viewContext.automaticallyMergesChangesFromParent = true
        viewContext.mergePolicy = NSMergeByPropertyObjectTrumpMergePolicy
    }

    // MARK: - Public Methods

    func saveContext() throws {
        let context = viewContext
        if context.hasChanges {
            try context.save()
        }
    }

    // MARK: - Testing Support

    static func resetForTesting() {
        // Reset the singleton for testing
        _shared = nil
    }

    private static var _shared: CoreDataStack?

    static let testShared: CoreDataStack = {
        let container = NSPersistentContainer(name: "SmartWorkz")

        let description = NSPersistentStoreDescription()
        description.url = URL(fileURLWithPath: "/dev/null")
        container.persistentStoreDescriptions = [description]

        container.loadPersistentStores { _, error in
            if let error = error {
                fatalError("Failed to load persistent stores: \(error)")
            }
        }

        let instance = CoreDataStack()
        return instance
    }()
}
