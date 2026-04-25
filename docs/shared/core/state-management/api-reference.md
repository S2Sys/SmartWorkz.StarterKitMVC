# State Management API Reference

## Classes & Interfaces

### StateGroups

- **Namespace:** `SmartWorkz.Core.Helpers.StateGroups`
- **Summary:** Helper class that manages valid state collections per entity type using a master/subset pattern.
            
             MASTER/SUBSET PATTERN:
             This class maintains a single source of truth (AllValidStates) that contains all 64 valid EntityState values.
             Each state collection (LifecycleStates, OrderStates, etc.) is a logical subset of AllValidStates that groups
             related states for a specific domain category or entity type.
            
             Benefits:
             - Single source of truth prevents state value inconsistencies
             - Type safety: states are strongly typed as EntityState enums
             - Immutable collections: returned as IReadOnlySet to prevent external modification
             - Easy validation: can verify entity states against correct subset for type
             - Clear organization: states grouped by business domain/entity category
            
             Usage:
             1. Get all valid states: StateGroups.AllValidStates
             2. Get states for category: StateGroups.OrderStates, StateGroups.UserAccountStates, etc.
             3. Get states for entity type: StateGroups.GetValidStatesFor(typeof(Order))
             4. Validate state: StateGroups.OrderStates.Contains(someState)

#### Methods & Properties

- **#cctor** - Static constructor initializes all state collections.
            All collections are implemented as immutable IReadOnlySet backed by HashSet internally.
- **GetValidStatesFor** - Gets the valid state subset for a specific entity type.
            
             Uses a pre-cached dictionary mapping of entity type names to their corresponding state collections.
             This provides O(1) lookup performance without reflection or repeated allocation.
            
             Supported entity type mappings:
             - User, ApplicationUser -> UserAccountStates
             - Order -> OrderStates
             - Product -> InventoryStates
             - Task, Job -> TaskJobStates
             - Subscription -> SubscriptionStates
             - And others (see TypeToStatesMap for complete list)
  - Parameters:
    - `entityType`: The entity type to get states for
  - Returns: IReadOnlySet of valid EntityState values for the given entity type

### StateGroups

- **Namespace:** `SmartWorkz.Core.Helpers.StateGroups`
- **Summary:** Helper class that manages valid state collections per entity type using a master/subset pattern.
            
             MASTER/SUBSET PATTERN:
             This class maintains a single source of truth (AllValidStates) that contains all 64 valid EntityState values.
             Each state collection (LifecycleStates, OrderStates, etc.) is a logical subset of AllValidStates that groups
             related states for a specific domain category or entity type.
            
             Benefits:
             - Single source of truth prevents state value inconsistencies
             - Type safety: states are strongly typed as EntityState enums
             - Immutable collections: returned as IReadOnlySet to prevent external modification
             - Easy validation: can verify entity states against correct subset for type
             - Clear organization: states grouped by business domain/entity category
            
             Usage:
             1. Get all valid states: StateGroups.AllValidStates
             2. Get states for category: StateGroups.OrderStates, StateGroups.UserAccountStates, etc.
             3. Get states for entity type: StateGroups.GetValidStatesFor(typeof(Order))
             4. Validate state: StateGroups.OrderStates.Contains(someState)

#### Methods & Properties

- **#cctor** - Static constructor initializes all state collections.
            All collections are implemented as immutable IReadOnlySet backed by HashSet internally.
- **GetValidStatesFor** - Gets the valid state subset for a specific entity type.
            
             Uses a pre-cached dictionary mapping of entity type names to their corresponding state collections.
             This provides O(1) lookup performance without reflection or repeated allocation.
            
             Supported entity type mappings:
             - User, ApplicationUser -> UserAccountStates
             - Order -> OrderStates
             - Product -> InventoryStates
             - Task, Job -> TaskJobStates
             - Subscription -> SubscriptionStates
             - And others (see TypeToStatesMap for complete list)
  - Parameters:
    - `entityType`: The entity type to get states for
  - Returns: IReadOnlySet of valid EntityState values for the given entity type

### StateGroups

- **Namespace:** `SmartWorkz.Core.Helpers.StateGroups`
- **Summary:** Helper class that manages valid state collections per entity type using a master/subset pattern.
            
             MASTER/SUBSET PATTERN:
             This class maintains a single source of truth (AllValidStates) that contains all 64 valid EntityState values.
             Each state collection (LifecycleStates, OrderStates, etc.) is a logical subset of AllValidStates that groups
             related states for a specific domain category or entity type.
            
             Benefits:
             - Single source of truth prevents state value inconsistencies
             - Type safety: states are strongly typed as EntityState enums
             - Immutable collections: returned as IReadOnlySet to prevent external modification
             - Easy validation: can verify entity states against correct subset for type
             - Clear organization: states grouped by business domain/entity category
            
             Usage:
             1. Get all valid states: StateGroups.AllValidStates
             2. Get states for category: StateGroups.OrderStates, StateGroups.UserAccountStates, etc.
             3. Get states for entity type: StateGroups.GetValidStatesFor(typeof(Order))
             4. Validate state: StateGroups.OrderStates.Contains(someState)

#### Methods & Properties

- **#cctor** - Static constructor initializes all state collections.
            All collections are implemented as immutable IReadOnlySet backed by HashSet internally.
- **GetValidStatesFor** - Gets the valid state subset for a specific entity type.
            
             Uses a pre-cached dictionary mapping of entity type names to their corresponding state collections.
             This provides O(1) lookup performance without reflection or repeated allocation.
            
             Supported entity type mappings:
             - User, ApplicationUser -> UserAccountStates
             - Order -> OrderStates
             - Product -> InventoryStates
             - Task, Job -> TaskJobStates
             - Subscription -> SubscriptionStates
             - And others (see TypeToStatesMap for complete list)
  - Parameters:
    - `entityType`: The entity type to get states for
  - Returns: IReadOnlySet of valid EntityState values for the given entity type

### StateGroups

- **Namespace:** `SmartWorkz.Core.Helpers.StateGroups`
- **Summary:** Helper class that manages valid state collections per entity type using a master/subset pattern.
            
             MASTER/SUBSET PATTERN:
             This class maintains a single source of truth (AllValidStates) that contains all 64 valid EntityState values.
             Each state collection (LifecycleStates, OrderStates, etc.) is a logical subset of AllValidStates that groups
             related states for a specific domain category or entity type.
            
             Benefits:
             - Single source of truth prevents state value inconsistencies
             - Type safety: states are strongly typed as EntityState enums
             - Immutable collections: returned as IReadOnlySet to prevent external modification
             - Easy validation: can verify entity states against correct subset for type
             - Clear organization: states grouped by business domain/entity category
            
             Usage:
             1. Get all valid states: StateGroups.AllValidStates
             2. Get states for category: StateGroups.OrderStates, StateGroups.UserAccountStates, etc.
             3. Get states for entity type: StateGroups.GetValidStatesFor(typeof(Order))
             4. Validate state: StateGroups.OrderStates.Contains(someState)

#### Methods & Properties

- **#cctor** - Static constructor initializes all state collections.
            All collections are implemented as immutable IReadOnlySet backed by HashSet internally.
- **GetValidStatesFor** - Gets the valid state subset for a specific entity type.
            
             Uses a pre-cached dictionary mapping of entity type names to their corresponding state collections.
             This provides O(1) lookup performance without reflection or repeated allocation.
            
             Supported entity type mappings:
             - User, ApplicationUser -> UserAccountStates
             - Order -> OrderStates
             - Product -> InventoryStates
             - Task, Job -> TaskJobStates
             - Subscription -> SubscriptionStates
             - And others (see TypeToStatesMap for complete list)
  - Parameters:
    - `entityType`: The entity type to get states for
  - Returns: IReadOnlySet of valid EntityState values for the given entity type

### StateGroups

- **Namespace:** `SmartWorkz.Core.Helpers.StateGroups`
- **Summary:** Helper class that manages valid state collections per entity type using a master/subset pattern.
            
             MASTER/SUBSET PATTERN:
             This class maintains a single source of truth (AllValidStates) that contains all 64 valid EntityState values.
             Each state collection (LifecycleStates, OrderStates, etc.) is a logical subset of AllValidStates that groups
             related states for a specific domain category or entity type.
            
             Benefits:
             - Single source of truth prevents state value inconsistencies
             - Type safety: states are strongly typed as EntityState enums
             - Immutable collections: returned as IReadOnlySet to prevent external modification
             - Easy validation: can verify entity states against correct subset for type
             - Clear organization: states grouped by business domain/entity category
            
             Usage:
             1. Get all valid states: StateGroups.AllValidStates
             2. Get states for category: StateGroups.OrderStates, StateGroups.UserAccountStates, etc.
             3. Get states for entity type: StateGroups.GetValidStatesFor(typeof(Order))
             4. Validate state: StateGroups.OrderStates.Contains(someState)

#### Methods & Properties

- **#cctor** - Static constructor initializes all state collections.
            All collections are implemented as immutable IReadOnlySet backed by HashSet internally.
- **GetValidStatesFor** - Gets the valid state subset for a specific entity type.
            
             Uses a pre-cached dictionary mapping of entity type names to their corresponding state collections.
             This provides O(1) lookup performance without reflection or repeated allocation.
            
             Supported entity type mappings:
             - User, ApplicationUser -> UserAccountStates
             - Order -> OrderStates
             - Product -> InventoryStates
             - Task, Job -> TaskJobStates
             - Subscription -> SubscriptionStates
             - And others (see TypeToStatesMap for complete list)
  - Parameters:
    - `entityType`: The entity type to get states for
  - Returns: IReadOnlySet of valid EntityState values for the given entity type

