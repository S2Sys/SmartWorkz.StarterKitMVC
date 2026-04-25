# Middleware API Reference

## Classes & Interfaces

### GraphQLAuthenticationMiddleware

- **Namespace:** `SmartWorkz.Core.Web.GraphQL.Middleware.GraphQLAuthenticationMiddleware`
- **Summary:** GraphQL middleware for JWT authentication.
            Validates JWT tokens and attaches claims to the GraphQL request context.

#### Methods & Properties

- **ValidateToken** - Validates JWT token from Authorization header.
  - Parameters:
    - `token`: The JWT token to validate.
    - `issuer`: Optional issuer claim to validate.
    - `audience`: Optional audience claim to validate.
  - Returns: ClaimsPrincipal if token is valid; null otherwise.
- **ExtractBearerToken** - Extracts JWT token from Authorization header.
            Expected format: "Bearer {token}"

### GraphQLMiddlewareExtensions

- **Namespace:** `SmartWorkz.Core.Web.GraphQL.Middleware.GraphQLMiddlewareExtensions`
- **Summary:** Extension methods for GraphQL middleware integration.
            Provides methods to apply rate limiting and authentication to GraphQL endpoints.

#### Methods & Properties

- **UseGraphQLMiddleware** - Adds GraphQL middleware to the application pipeline.
            Applies rate limiting before GraphQL execution.
- **GraphQLMiddlewareDelegate** - The actual middleware delegate that handles GraphQL requests.
            Enforces rate limiting on /graphql endpoint.

### GraphQLRateLimitMiddleware

- **Namespace:** `SmartWorkz.Core.Web.GraphQL.Middleware.GraphQLRateLimitMiddleware`
- **Summary:** GraphQL rate limiting middleware.
            Enforces per-API-key rate limits on GraphQL requests.

#### Methods & Properties

- **ExtractApiKey** - Extracts API key from request headers.
            First checks X-API-Key header, then falls back to Authorization header.
- **AddRateLimitHeaders** - Adds rate limit headers to response.
- **IsRateLimitExceeded** - Checks if rate limit exceeded.
            Returns appropriate HTTP status code and headers.

### GraphQLRateLimitStore

- **Namespace:** `SmartWorkz.Core.Web.GraphQL.Middleware.GraphQLRateLimitStore`
- **Summary:** In-memory rate limit store for tracking API requests per key.
            Enforces 1000 requests per hour per API key.

#### Methods & Properties

- **IsAllowed** - Checks if a request is allowed for the given API key.
            Increments the request counter and returns true if within limit.
- **GetStatus** - Gets current limit status for an API key without incrementing.

