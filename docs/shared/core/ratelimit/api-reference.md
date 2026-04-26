# RateLimit API Reference

## Classes & Interfaces

### IRateLimitService

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.IRateLimitService`
- **Summary:** Defines the contract for rate limiting service implementations.

#### Methods & Properties

- **IsRequestAllowedAsync** - Determines whether a request from the specified client is allowed based on rate limiting rules.
  - Parameters:
    - `clientId`: The unique identifier for the client making the request.
    - `maxRequests`: The maximum number of requests allowed within the window.
    - `windowSeconds`: The size of the time window in seconds.
  - Returns: True if the request is allowed; false if the rate limit has been exceeded.
- **GetStatusAsync** - Gets the current rate limiting status for a specific client.
  - Parameters:
    - `clientId`: The unique identifier for the client.
    - `maxRequests`: The maximum number of requests allowed within the window.
  - Returns: A RateLimitStatus object containing the current status information.
- **ResetAsync** - Resets the rate limit for a specific client, clearing all accumulated requests.
  - Parameters:
    - `clientId`: The unique identifier for the client.
  - Returns: True if the reset was successful; false otherwise.

### RateLimitService

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.RateLimitService`
- **Summary:** Implements token bucket rate limiting algorithm for controlling request rates.

#### Methods & Properties

- **IsRequestAllowedAsync** - Determines whether a request from the specified client is allowed based on rate limiting rules
            using the token bucket algorithm.
- **GetStatusAsync** - Gets the current rate limiting status for a specific client.
- **ResetAsync** - Resets the rate limit for a specific client, clearing all accumulated requests.

### RateLimitStatus

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.RateLimitStatus`
- **Summary:** Represents the current rate limiting status for a client.

### IRateLimitService

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.IRateLimitService`
- **Summary:** Defines the contract for rate limiting service implementations.

#### Methods & Properties

- **IsRequestAllowedAsync** - Determines whether a request from the specified client is allowed based on rate limiting rules.
  - Parameters:
    - `clientId`: The unique identifier for the client making the request.
    - `maxRequests`: The maximum number of requests allowed within the window.
    - `windowSeconds`: The size of the time window in seconds.
  - Returns: True if the request is allowed; false if the rate limit has been exceeded.
- **GetStatusAsync** - Gets the current rate limiting status for a specific client.
  - Parameters:
    - `clientId`: The unique identifier for the client.
    - `maxRequests`: The maximum number of requests allowed within the window.
  - Returns: A RateLimitStatus object containing the current status information.
- **ResetAsync** - Resets the rate limit for a specific client, clearing all accumulated requests.
  - Parameters:
    - `clientId`: The unique identifier for the client.
  - Returns: True if the reset was successful; false otherwise.

### RateLimitService

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.RateLimitService`
- **Summary:** Implements token bucket rate limiting algorithm for controlling request rates.

#### Methods & Properties

- **IsRequestAllowedAsync** - Determines whether a request from the specified client is allowed based on rate limiting rules
            using the token bucket algorithm.
- **GetStatusAsync** - Gets the current rate limiting status for a specific client.
- **ResetAsync** - Resets the rate limit for a specific client, clearing all accumulated requests.

### RateLimitStatus

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.RateLimitStatus`
- **Summary:** Represents the current rate limiting status for a client.

### IRateLimitService

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.IRateLimitService`
- **Summary:** Defines the contract for rate limiting service implementations.

#### Methods & Properties

- **IsRequestAllowedAsync** - Determines whether a request from the specified client is allowed based on rate limiting rules.
  - Parameters:
    - `clientId`: The unique identifier for the client making the request.
    - `maxRequests`: The maximum number of requests allowed within the window.
    - `windowSeconds`: The size of the time window in seconds.
  - Returns: True if the request is allowed; false if the rate limit has been exceeded.
- **GetStatusAsync** - Gets the current rate limiting status for a specific client.
  - Parameters:
    - `clientId`: The unique identifier for the client.
    - `maxRequests`: The maximum number of requests allowed within the window.
  - Returns: A RateLimitStatus object containing the current status information.
- **ResetAsync** - Resets the rate limit for a specific client, clearing all accumulated requests.
  - Parameters:
    - `clientId`: The unique identifier for the client.
  - Returns: True if the reset was successful; false otherwise.

### RateLimitService

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.RateLimitService`
- **Summary:** Implements token bucket rate limiting algorithm for controlling request rates.

#### Methods & Properties

- **IsRequestAllowedAsync** - Determines whether a request from the specified client is allowed based on rate limiting rules
            using the token bucket algorithm.
- **GetStatusAsync** - Gets the current rate limiting status for a specific client.
- **ResetAsync** - Resets the rate limit for a specific client, clearing all accumulated requests.

### RateLimitStatus

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.RateLimitStatus`
- **Summary:** Represents the current rate limiting status for a client.

### IRateLimitService

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.IRateLimitService`
- **Summary:** Defines the contract for rate limiting service implementations.

#### Methods & Properties

- **IsRequestAllowedAsync** - Determines whether a request from the specified client is allowed based on rate limiting rules.
  - Parameters:
    - `clientId`: The unique identifier for the client making the request.
    - `maxRequests`: The maximum number of requests allowed within the window.
    - `windowSeconds`: The size of the time window in seconds.
  - Returns: True if the request is allowed; false if the rate limit has been exceeded.
- **GetStatusAsync** - Gets the current rate limiting status for a specific client.
  - Parameters:
    - `clientId`: The unique identifier for the client.
    - `maxRequests`: The maximum number of requests allowed within the window.
  - Returns: A RateLimitStatus object containing the current status information.
- **ResetAsync** - Resets the rate limit for a specific client, clearing all accumulated requests.
  - Parameters:
    - `clientId`: The unique identifier for the client.
  - Returns: True if the reset was successful; false otherwise.

### RateLimitService

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.RateLimitService`
- **Summary:** Implements token bucket rate limiting algorithm for controlling request rates.

#### Methods & Properties

- **IsRequestAllowedAsync** - Determines whether a request from the specified client is allowed based on rate limiting rules
            using the token bucket algorithm.
- **GetStatusAsync** - Gets the current rate limiting status for a specific client.
- **ResetAsync** - Resets the rate limit for a specific client, clearing all accumulated requests.

### RateLimitStatus

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.RateLimitStatus`
- **Summary:** Represents the current rate limiting status for a client.

### IRateLimitService

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.IRateLimitService`
- **Summary:** Defines the contract for rate limiting service implementations.

#### Methods & Properties

- **IsRequestAllowedAsync** - Determines whether a request from the specified client is allowed based on rate limiting rules.
  - Parameters:
    - `clientId`: The unique identifier for the client making the request.
    - `maxRequests`: The maximum number of requests allowed within the window.
    - `windowSeconds`: The size of the time window in seconds.
  - Returns: True if the request is allowed; false if the rate limit has been exceeded.
- **GetStatusAsync** - Gets the current rate limiting status for a specific client.
  - Parameters:
    - `clientId`: The unique identifier for the client.
    - `maxRequests`: The maximum number of requests allowed within the window.
  - Returns: A RateLimitStatus object containing the current status information.
- **ResetAsync** - Resets the rate limit for a specific client, clearing all accumulated requests.
  - Parameters:
    - `clientId`: The unique identifier for the client.
  - Returns: True if the reset was successful; false otherwise.

### RateLimitService

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.RateLimitService`
- **Summary:** Implements token bucket rate limiting algorithm for controlling request rates.

#### Methods & Properties

- **IsRequestAllowedAsync** - Determines whether a request from the specified client is allowed based on rate limiting rules
            using the token bucket algorithm.
- **GetStatusAsync** - Gets the current rate limiting status for a specific client.
- **ResetAsync** - Resets the rate limit for a specific client, clearing all accumulated requests.

### RateLimitStatus

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.RateLimitStatus`
- **Summary:** Represents the current rate limiting status for a client.

### IRateLimitService

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.IRateLimitService`
- **Summary:** Defines the contract for rate limiting service implementations.

#### Methods & Properties

- **IsRequestAllowedAsync** - Determines whether a request from the specified client is allowed based on rate limiting rules.
  - Parameters:
    - `clientId`: The unique identifier for the client making the request.
    - `maxRequests`: The maximum number of requests allowed within the window.
    - `windowSeconds`: The size of the time window in seconds.
  - Returns: True if the request is allowed; false if the rate limit has been exceeded.
- **GetStatusAsync** - Gets the current rate limiting status for a specific client.
  - Parameters:
    - `clientId`: The unique identifier for the client.
    - `maxRequests`: The maximum number of requests allowed within the window.
  - Returns: A RateLimitStatus object containing the current status information.
- **ResetAsync** - Resets the rate limit for a specific client, clearing all accumulated requests.
  - Parameters:
    - `clientId`: The unique identifier for the client.
  - Returns: True if the reset was successful; false otherwise.

### RateLimitService

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.RateLimitService`
- **Summary:** Implements token bucket rate limiting algorithm for controlling request rates.

#### Methods & Properties

- **IsRequestAllowedAsync** - Determines whether a request from the specified client is allowed based on rate limiting rules
            using the token bucket algorithm.
- **GetStatusAsync** - Gets the current rate limiting status for a specific client.
- **ResetAsync** - Resets the rate limit for a specific client, clearing all accumulated requests.

### RateLimitStatus

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.RateLimitStatus`
- **Summary:** Represents the current rate limiting status for a client.

### IRateLimitService

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.IRateLimitService`
- **Summary:** Defines the contract for rate limiting service implementations.

#### Methods & Properties

- **IsRequestAllowedAsync** - Determines whether a request from the specified client is allowed based on rate limiting rules.
  - Parameters:
    - `clientId`: The unique identifier for the client making the request.
    - `maxRequests`: The maximum number of requests allowed within the window.
    - `windowSeconds`: The size of the time window in seconds.
  - Returns: True if the request is allowed; false if the rate limit has been exceeded.
- **GetStatusAsync** - Gets the current rate limiting status for a specific client.
  - Parameters:
    - `clientId`: The unique identifier for the client.
    - `maxRequests`: The maximum number of requests allowed within the window.
  - Returns: A RateLimitStatus object containing the current status information.
- **ResetAsync** - Resets the rate limit for a specific client, clearing all accumulated requests.
  - Parameters:
    - `clientId`: The unique identifier for the client.
  - Returns: True if the reset was successful; false otherwise.

### RateLimitService

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.RateLimitService`
- **Summary:** Implements token bucket rate limiting algorithm for controlling request rates.

#### Methods & Properties

- **IsRequestAllowedAsync** - Determines whether a request from the specified client is allowed based on rate limiting rules
            using the token bucket algorithm.
- **GetStatusAsync** - Gets the current rate limiting status for a specific client.
- **ResetAsync** - Resets the rate limit for a specific client, clearing all accumulated requests.

### RateLimitStatus

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.RateLimitStatus`
- **Summary:** Represents the current rate limiting status for a client.

### IRateLimitService

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.IRateLimitService`
- **Summary:** Defines the contract for rate limiting service implementations.

#### Methods & Properties

- **IsRequestAllowedAsync** - Determines whether a request from the specified client is allowed based on rate limiting rules.
  - Parameters:
    - `clientId`: The unique identifier for the client making the request.
    - `maxRequests`: The maximum number of requests allowed within the window.
    - `windowSeconds`: The size of the time window in seconds.
  - Returns: True if the request is allowed; false if the rate limit has been exceeded.
- **GetStatusAsync** - Gets the current rate limiting status for a specific client.
  - Parameters:
    - `clientId`: The unique identifier for the client.
    - `maxRequests`: The maximum number of requests allowed within the window.
  - Returns: A RateLimitStatus object containing the current status information.
- **ResetAsync** - Resets the rate limit for a specific client, clearing all accumulated requests.
  - Parameters:
    - `clientId`: The unique identifier for the client.
  - Returns: True if the reset was successful; false otherwise.

### RateLimitService

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.RateLimitService`
- **Summary:** Implements token bucket rate limiting algorithm for controlling request rates.

#### Methods & Properties

- **IsRequestAllowedAsync** - Determines whether a request from the specified client is allowed based on rate limiting rules
            using the token bucket algorithm.
- **GetStatusAsync** - Gets the current rate limiting status for a specific client.
- **ResetAsync** - Resets the rate limit for a specific client, clearing all accumulated requests.

### RateLimitStatus

- **Namespace:** `SmartWorkz.Shared.Security.RateLimit.RateLimitStatus`
- **Summary:** Represents the current rate limiting status for a client.

