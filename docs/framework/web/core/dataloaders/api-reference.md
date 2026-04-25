# DataLoaders API Reference

## Classes & Interfaces

### ProductDataLoader

- **Namespace:** `SmartWorkz.Core.Web.GraphQL.DataLoaders.ProductDataLoader`
- **Summary:** DataLoader for batching Product queries to prevent N+1 query problems.
            Groups product lookups by ID and loads them in a single batch operation.
            Note: Full DataLoader implementation requires HotChocolate.DataLoader package
            and would integrate with a repository pattern for actual data loading.

#### Methods & Properties

- **GetConfig** - Gets the configuration for this DataLoader.
- **LoadProductsByIdsAsync** - Simulates batch loading of products.
            In a real implementation, this would fetch from a repository.

### UserDataLoader

- **Namespace:** `SmartWorkz.Core.Web.GraphQL.DataLoaders.UserDataLoader`
- **Summary:** DataLoader for batching User queries to prevent N+1 query problems.
            Groups user lookups by ID and loads them in a single batch operation.
            Note: Full DataLoader implementation requires HotChocolate.DataLoader package
            and would integrate with a repository pattern for actual data loading.

#### Methods & Properties

- **GetConfig** - Gets the configuration for this DataLoader.
- **LoadUsersByIdsAsync** - Simulates batch loading of users.
            In a real implementation, this would fetch from a repository.

