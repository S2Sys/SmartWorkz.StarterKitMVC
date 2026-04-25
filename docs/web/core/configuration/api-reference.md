# Configuration API Reference

## Classes & Interfaces

### GraphQLSetup

- **Namespace:** `SmartWorkz.Core.Web.GraphQL.Configuration.GraphQLSetup`
- **Summary:** Extension methods for configuring GraphQL services and middleware.
            Provides setup for HotChocolate GraphQL endpoint with authentication and rate limiting.

#### Methods & Properties

- **AddGraphQLServices** - Adds GraphQL services to the dependency injection container.
            Configures HotChocolate with Query type, pagination, and error handling.
- **MapGraphQLEndpoint** - Maps GraphQL endpoint and configures middleware.
            Sets up /graphql endpoint with authentication and rate limiting.
            Note: The consuming application must call app.MapGraphQL("/graphql") directly
            since this library doesn't have HotChocolate.AspNetCore as a dependency.

### RelayConnectionTypes

- **Namespace:** `SmartWorkz.Core.Web.GraphQL.Configuration.RelayConnectionTypes`
- **Summary:** Relay-based connection types for cursor-based pagination.

