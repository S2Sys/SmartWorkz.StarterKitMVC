# State Management Usage Guide

## Overview

Helper class that manages valid state collections per entity type using a master/subset pattern.
            
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

## API Reference

See [API Reference](./api-reference.md) for complete documentation.

