# Http Client API Reference

## Classes & Interfaces

### HttpClientHelper

- **Namespace:** `SmartWorkz.Shared.HttpClientHelper`
- **Summary:** HTTP client helper for making async HTTP requests with support for retry logic, timeouts, and JSON serialization.
            Provides fluent builder pattern for composing requests.

#### Methods & Properties

- **Get** - Creates a new HTTP GET request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Post** - Creates a new HTTP POST request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Put** - Creates a new HTTP PUT request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Delete** - Creates a new HTTP DELETE request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **WithBody** - Sets the request body.
  - Parameters:
    - `body`: The body object to serialize as JSON.
  - Returns: This instance for method chaining.
- **WithHeader** - Adds a single header to the request.
  - Parameters:
    - `key`: The header name.
    - `value`: The header value.
  - Returns: This instance for method chaining.
- **WithHeaders** - Sets all request headers, replacing existing headers.
  - Parameters:
    - `headers`: Dictionary of headers to set.
  - Returns: This instance for method chaining.
- **WithTimeout** - Sets the request timeout.
  - Parameters:
    - `milliseconds`: The timeout duration in milliseconds.
  - Returns: This instance for method chaining.
- **WithRetry** - Configures automatic retry logic for the request.
  - Parameters:
    - `maxAttempts`: Maximum number of retry attempts.
    - `backoffMs`: Initial backoff interval in milliseconds.
    - `strategy`: The backoff strategy to use.
  - Returns: This instance for method chaining.
- **ExecuteAsync** - Executes the HTTP request and returns a string response.
  - Parameters:
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.
- **ExecuteAsync``1** - Executes the HTTP request and returns a typed response.
  - Parameters:
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **GetAsync``1** - Sends a GET request and returns a typed response.
- **PostAsync``1** - Sends a POST request with JSON body and returns a typed response.
- **PutAsync``1** - Sends a PUT request with JSON body and returns a typed response.
- **DeleteAsync``1** - Sends a DELETE request and returns a typed response.
- **GetAsync** - Sends a GET request and returns a string response.
- **PostAsync** - Sends a POST request with JSON body and returns a string response.
- **CalculateBackoff** - Calculates the backoff delay in milliseconds based on the strategy and attempt number.
- **GetFibonacciNumber** - Gets the nth Fibonacci number (0-indexed).

### HttpRequest

- **Namespace:** `SmartWorkz.Shared.HttpRequest`
- **Summary:** Represents an HTTP request with configuration for URL, method, headers, body, timeout, and retry policy.

### HttpResponse`1

- **Namespace:** `SmartWorkz.Shared.HttpResponse`1`
- **Summary:** Represents an HTTP response with status code, typed data, error information, and response headers.

### HttpClientHelper

- **Namespace:** `SmartWorkz.Shared.HttpClientHelper`
- **Summary:** HTTP client helper for making async HTTP requests with support for retry logic, timeouts, and JSON serialization.
            Provides fluent builder pattern for composing requests.

#### Methods & Properties

- **Get** - Creates a new HTTP GET request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Post** - Creates a new HTTP POST request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Put** - Creates a new HTTP PUT request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Delete** - Creates a new HTTP DELETE request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **WithBody** - Sets the request body.
  - Parameters:
    - `body`: The body object to serialize as JSON.
  - Returns: This instance for method chaining.
- **WithHeader** - Adds a single header to the request.
  - Parameters:
    - `key`: The header name.
    - `value`: The header value.
  - Returns: This instance for method chaining.
- **WithHeaders** - Sets all request headers, replacing existing headers.
  - Parameters:
    - `headers`: Dictionary of headers to set.
  - Returns: This instance for method chaining.
- **WithTimeout** - Sets the request timeout.
  - Parameters:
    - `milliseconds`: The timeout duration in milliseconds.
  - Returns: This instance for method chaining.
- **WithRetry** - Configures automatic retry logic for the request.
  - Parameters:
    - `maxAttempts`: Maximum number of retry attempts.
    - `backoffMs`: Initial backoff interval in milliseconds.
    - `strategy`: The backoff strategy to use.
  - Returns: This instance for method chaining.
- **ExecuteAsync** - Executes the HTTP request and returns a string response.
  - Parameters:
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.
- **ExecuteAsync``1** - Executes the HTTP request and returns a typed response.
  - Parameters:
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **GetAsync``1** - Sends a GET request and returns a typed response.
- **PostAsync``1** - Sends a POST request with JSON body and returns a typed response.
- **PutAsync``1** - Sends a PUT request with JSON body and returns a typed response.
- **DeleteAsync``1** - Sends a DELETE request and returns a typed response.
- **GetAsync** - Sends a GET request and returns a string response.
- **PostAsync** - Sends a POST request with JSON body and returns a string response.
- **CalculateBackoff** - Calculates the backoff delay in milliseconds based on the strategy and attempt number.
- **GetFibonacciNumber** - Gets the nth Fibonacci number (0-indexed).

### HttpRequest

- **Namespace:** `SmartWorkz.Shared.HttpRequest`
- **Summary:** Represents an HTTP request with configuration for URL, method, headers, body, timeout, and retry policy.

### HttpResponse`1

- **Namespace:** `SmartWorkz.Shared.HttpResponse`1`
- **Summary:** Represents an HTTP response with status code, typed data, error information, and response headers.

### HttpClientHelper

- **Namespace:** `SmartWorkz.Shared.HttpClientHelper`
- **Summary:** HTTP client helper for making async HTTP requests with support for retry logic, timeouts, and JSON serialization.
            Provides fluent builder pattern for composing requests.

#### Methods & Properties

- **Get** - Creates a new HTTP GET request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Post** - Creates a new HTTP POST request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Put** - Creates a new HTTP PUT request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Delete** - Creates a new HTTP DELETE request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **WithBody** - Sets the request body.
  - Parameters:
    - `body`: The body object to serialize as JSON.
  - Returns: This instance for method chaining.
- **WithHeader** - Adds a single header to the request.
  - Parameters:
    - `key`: The header name.
    - `value`: The header value.
  - Returns: This instance for method chaining.
- **WithHeaders** - Sets all request headers, replacing existing headers.
  - Parameters:
    - `headers`: Dictionary of headers to set.
  - Returns: This instance for method chaining.
- **WithTimeout** - Sets the request timeout.
  - Parameters:
    - `milliseconds`: The timeout duration in milliseconds.
  - Returns: This instance for method chaining.
- **WithRetry** - Configures automatic retry logic for the request.
  - Parameters:
    - `maxAttempts`: Maximum number of retry attempts.
    - `backoffMs`: Initial backoff interval in milliseconds.
    - `strategy`: The backoff strategy to use.
  - Returns: This instance for method chaining.
- **ExecuteAsync** - Executes the HTTP request and returns a string response.
  - Parameters:
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.
- **ExecuteAsync``1** - Executes the HTTP request and returns a typed response.
  - Parameters:
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **GetAsync``1** - Sends a GET request and returns a typed response.
- **PostAsync``1** - Sends a POST request with JSON body and returns a typed response.
- **PutAsync``1** - Sends a PUT request with JSON body and returns a typed response.
- **DeleteAsync``1** - Sends a DELETE request and returns a typed response.
- **GetAsync** - Sends a GET request and returns a string response.
- **PostAsync** - Sends a POST request with JSON body and returns a string response.
- **CalculateBackoff** - Calculates the backoff delay in milliseconds based on the strategy and attempt number.
- **GetFibonacciNumber** - Gets the nth Fibonacci number (0-indexed).

### HttpRequest

- **Namespace:** `SmartWorkz.Shared.HttpRequest`
- **Summary:** Represents an HTTP request with configuration for URL, method, headers, body, timeout, and retry policy.

### HttpResponse`1

- **Namespace:** `SmartWorkz.Shared.HttpResponse`1`
- **Summary:** Represents an HTTP response with status code, typed data, error information, and response headers.

### HttpClientHelper

- **Namespace:** `SmartWorkz.Shared.HttpClientHelper`
- **Summary:** HTTP client helper for making async HTTP requests with support for retry logic, timeouts, and JSON serialization.
            Provides fluent builder pattern for composing requests.

#### Methods & Properties

- **Get** - Creates a new HTTP GET request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Post** - Creates a new HTTP POST request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Put** - Creates a new HTTP PUT request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Delete** - Creates a new HTTP DELETE request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **WithBody** - Sets the request body.
  - Parameters:
    - `body`: The body object to serialize as JSON.
  - Returns: This instance for method chaining.
- **WithHeader** - Adds a single header to the request.
  - Parameters:
    - `key`: The header name.
    - `value`: The header value.
  - Returns: This instance for method chaining.
- **WithHeaders** - Sets all request headers, replacing existing headers.
  - Parameters:
    - `headers`: Dictionary of headers to set.
  - Returns: This instance for method chaining.
- **WithTimeout** - Sets the request timeout.
  - Parameters:
    - `milliseconds`: The timeout duration in milliseconds.
  - Returns: This instance for method chaining.
- **WithRetry** - Configures automatic retry logic for the request.
  - Parameters:
    - `maxAttempts`: Maximum number of retry attempts.
    - `backoffMs`: Initial backoff interval in milliseconds.
    - `strategy`: The backoff strategy to use.
  - Returns: This instance for method chaining.
- **ExecuteAsync** - Executes the HTTP request and returns a string response.
  - Parameters:
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.
- **ExecuteAsync``1** - Executes the HTTP request and returns a typed response.
  - Parameters:
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **GetAsync``1** - Sends a GET request and returns a typed response.
- **PostAsync``1** - Sends a POST request with JSON body and returns a typed response.
- **PutAsync``1** - Sends a PUT request with JSON body and returns a typed response.
- **DeleteAsync``1** - Sends a DELETE request and returns a typed response.
- **GetAsync** - Sends a GET request and returns a string response.
- **PostAsync** - Sends a POST request with JSON body and returns a string response.
- **CalculateBackoff** - Calculates the backoff delay in milliseconds based on the strategy and attempt number.
- **GetFibonacciNumber** - Gets the nth Fibonacci number (0-indexed).

### HttpRequest

- **Namespace:** `SmartWorkz.Shared.HttpRequest`
- **Summary:** Represents an HTTP request with configuration for URL, method, headers, body, timeout, and retry policy.

### HttpResponse`1

- **Namespace:** `SmartWorkz.Shared.HttpResponse`1`
- **Summary:** Represents an HTTP response with status code, typed data, error information, and response headers.

### HttpClientHelper

- **Namespace:** `SmartWorkz.Shared.HttpClientHelper`
- **Summary:** HTTP client helper for making async HTTP requests with support for retry logic, timeouts, and JSON serialization.
            Provides fluent builder pattern for composing requests.

#### Methods & Properties

- **Get** - Creates a new HTTP GET request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Post** - Creates a new HTTP POST request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Put** - Creates a new HTTP PUT request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Delete** - Creates a new HTTP DELETE request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **WithBody** - Sets the request body.
  - Parameters:
    - `body`: The body object to serialize as JSON.
  - Returns: This instance for method chaining.
- **WithHeader** - Adds a single header to the request.
  - Parameters:
    - `key`: The header name.
    - `value`: The header value.
  - Returns: This instance for method chaining.
- **WithHeaders** - Sets all request headers, replacing existing headers.
  - Parameters:
    - `headers`: Dictionary of headers to set.
  - Returns: This instance for method chaining.
- **WithTimeout** - Sets the request timeout.
  - Parameters:
    - `milliseconds`: The timeout duration in milliseconds.
  - Returns: This instance for method chaining.
- **WithRetry** - Configures automatic retry logic for the request.
  - Parameters:
    - `maxAttempts`: Maximum number of retry attempts.
    - `backoffMs`: Initial backoff interval in milliseconds.
    - `strategy`: The backoff strategy to use.
  - Returns: This instance for method chaining.
- **ExecuteAsync** - Executes the HTTP request and returns a string response.
  - Parameters:
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.
- **ExecuteAsync``1** - Executes the HTTP request and returns a typed response.
  - Parameters:
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **GetAsync``1** - Sends a GET request and returns a typed response.
- **PostAsync``1** - Sends a POST request with JSON body and returns a typed response.
- **PutAsync``1** - Sends a PUT request with JSON body and returns a typed response.
- **DeleteAsync``1** - Sends a DELETE request and returns a typed response.
- **GetAsync** - Sends a GET request and returns a string response.
- **PostAsync** - Sends a POST request with JSON body and returns a string response.
- **CalculateBackoff** - Calculates the backoff delay in milliseconds based on the strategy and attempt number.
- **GetFibonacciNumber** - Gets the nth Fibonacci number (0-indexed).

### HttpRequest

- **Namespace:** `SmartWorkz.Shared.HttpRequest`
- **Summary:** Represents an HTTP request with configuration for URL, method, headers, body, timeout, and retry policy.

### HttpResponse`1

- **Namespace:** `SmartWorkz.Shared.HttpResponse`1`
- **Summary:** Represents an HTTP response with status code, typed data, error information, and response headers.

### HttpClientHelper

- **Namespace:** `SmartWorkz.Shared.HttpClientHelper`
- **Summary:** HTTP client helper for making async HTTP requests with support for retry logic, timeouts, and JSON serialization.
            Provides fluent builder pattern for composing requests.

#### Methods & Properties

- **Get** - Creates a new HTTP GET request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Post** - Creates a new HTTP POST request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Put** - Creates a new HTTP PUT request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Delete** - Creates a new HTTP DELETE request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **WithBody** - Sets the request body.
  - Parameters:
    - `body`: The body object to serialize as JSON.
  - Returns: This instance for method chaining.
- **WithHeader** - Adds a single header to the request.
  - Parameters:
    - `key`: The header name.
    - `value`: The header value.
  - Returns: This instance for method chaining.
- **WithHeaders** - Sets all request headers, replacing existing headers.
  - Parameters:
    - `headers`: Dictionary of headers to set.
  - Returns: This instance for method chaining.
- **WithTimeout** - Sets the request timeout.
  - Parameters:
    - `milliseconds`: The timeout duration in milliseconds.
  - Returns: This instance for method chaining.
- **WithRetry** - Configures automatic retry logic for the request.
  - Parameters:
    - `maxAttempts`: Maximum number of retry attempts.
    - `backoffMs`: Initial backoff interval in milliseconds.
    - `strategy`: The backoff strategy to use.
  - Returns: This instance for method chaining.
- **ExecuteAsync** - Executes the HTTP request and returns a string response.
  - Parameters:
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.
- **ExecuteAsync``1** - Executes the HTTP request and returns a typed response.
  - Parameters:
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **GetAsync``1** - Sends a GET request and returns a typed response.
- **PostAsync``1** - Sends a POST request with JSON body and returns a typed response.
- **PutAsync``1** - Sends a PUT request with JSON body and returns a typed response.
- **DeleteAsync``1** - Sends a DELETE request and returns a typed response.
- **GetAsync** - Sends a GET request and returns a string response.
- **PostAsync** - Sends a POST request with JSON body and returns a string response.
- **CalculateBackoff** - Calculates the backoff delay in milliseconds based on the strategy and attempt number.
- **GetFibonacciNumber** - Gets the nth Fibonacci number (0-indexed).

### HttpRequest

- **Namespace:** `SmartWorkz.Shared.HttpRequest`
- **Summary:** Represents an HTTP request with configuration for URL, method, headers, body, timeout, and retry policy.

### HttpResponse`1

- **Namespace:** `SmartWorkz.Shared.HttpResponse`1`
- **Summary:** Represents an HTTP response with status code, typed data, error information, and response headers.

### HttpClientHelper

- **Namespace:** `SmartWorkz.Shared.HttpClientHelper`
- **Summary:** HTTP client helper for making async HTTP requests with support for retry logic, timeouts, and JSON serialization.
            Provides fluent builder pattern for composing requests.

#### Methods & Properties

- **Get** - Creates a new HTTP GET request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Post** - Creates a new HTTP POST request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Put** - Creates a new HTTP PUT request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Delete** - Creates a new HTTP DELETE request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **WithBody** - Sets the request body.
  - Parameters:
    - `body`: The body object to serialize as JSON.
  - Returns: This instance for method chaining.
- **WithHeader** - Adds a single header to the request.
  - Parameters:
    - `key`: The header name.
    - `value`: The header value.
  - Returns: This instance for method chaining.
- **WithHeaders** - Sets all request headers, replacing existing headers.
  - Parameters:
    - `headers`: Dictionary of headers to set.
  - Returns: This instance for method chaining.
- **WithTimeout** - Sets the request timeout.
  - Parameters:
    - `milliseconds`: The timeout duration in milliseconds.
  - Returns: This instance for method chaining.
- **WithRetry** - Configures automatic retry logic for the request.
  - Parameters:
    - `maxAttempts`: Maximum number of retry attempts.
    - `backoffMs`: Initial backoff interval in milliseconds.
    - `strategy`: The backoff strategy to use.
  - Returns: This instance for method chaining.
- **ExecuteAsync** - Executes the HTTP request and returns a string response.
  - Parameters:
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.
- **ExecuteAsync``1** - Executes the HTTP request and returns a typed response.
  - Parameters:
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **GetAsync``1** - Sends a GET request and returns a typed response.
- **PostAsync``1** - Sends a POST request with JSON body and returns a typed response.
- **PutAsync``1** - Sends a PUT request with JSON body and returns a typed response.
- **DeleteAsync``1** - Sends a DELETE request and returns a typed response.
- **GetAsync** - Sends a GET request and returns a string response.
- **PostAsync** - Sends a POST request with JSON body and returns a string response.
- **CalculateBackoff** - Calculates the backoff delay in milliseconds based on the strategy and attempt number.
- **GetFibonacciNumber** - Gets the nth Fibonacci number (0-indexed).

### HttpRequest

- **Namespace:** `SmartWorkz.Shared.HttpRequest`
- **Summary:** Represents an HTTP request with configuration for URL, method, headers, body, timeout, and retry policy.

### HttpResponse`1

- **Namespace:** `SmartWorkz.Shared.HttpResponse`1`
- **Summary:** Represents an HTTP response with status code, typed data, error information, and response headers.

### HttpClientHelper

- **Namespace:** `SmartWorkz.Shared.HttpClientHelper`
- **Summary:** HTTP client helper for making async HTTP requests with support for retry logic, timeouts, and JSON serialization.
            Provides fluent builder pattern for composing requests.

#### Methods & Properties

- **Get** - Creates a new HTTP GET request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Post** - Creates a new HTTP POST request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Put** - Creates a new HTTP PUT request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **Delete** - Creates a new HTTP DELETE request builder.
  - Parameters:
    - `url`: The request URL.
  - Returns: A configured HttpClientHelper instance.
- **WithBody** - Sets the request body.
  - Parameters:
    - `body`: The body object to serialize as JSON.
  - Returns: This instance for method chaining.
- **WithHeader** - Adds a single header to the request.
  - Parameters:
    - `key`: The header name.
    - `value`: The header value.
  - Returns: This instance for method chaining.
- **WithHeaders** - Sets all request headers, replacing existing headers.
  - Parameters:
    - `headers`: Dictionary of headers to set.
  - Returns: This instance for method chaining.
- **WithTimeout** - Sets the request timeout.
  - Parameters:
    - `milliseconds`: The timeout duration in milliseconds.
  - Returns: This instance for method chaining.
- **WithRetry** - Configures automatic retry logic for the request.
  - Parameters:
    - `maxAttempts`: Maximum number of retry attempts.
    - `backoffMs`: Initial backoff interval in milliseconds.
    - `strategy`: The backoff strategy to use.
  - Returns: This instance for method chaining.
- **ExecuteAsync** - Executes the HTTP request and returns a string response.
  - Parameters:
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.
- **ExecuteAsync``1** - Executes the HTTP request and returns a typed response.
  - Parameters:
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **GetAsync``1** - Sends a GET request and returns a typed response.
- **PostAsync``1** - Sends a POST request with JSON body and returns a typed response.
- **PutAsync``1** - Sends a PUT request with JSON body and returns a typed response.
- **DeleteAsync``1** - Sends a DELETE request and returns a typed response.
- **GetAsync** - Sends a GET request and returns a string response.
- **PostAsync** - Sends a POST request with JSON body and returns a string response.
- **CalculateBackoff** - Calculates the backoff delay in milliseconds based on the strategy and attempt number.
- **GetFibonacciNumber** - Gets the nth Fibonacci number (0-indexed).

### HttpRequest

- **Namespace:** `SmartWorkz.Shared.HttpRequest`
- **Summary:** Represents an HTTP request with configuration for URL, method, headers, body, timeout, and retry policy.

### HttpResponse`1

- **Namespace:** `SmartWorkz.Shared.HttpResponse`1`
- **Summary:** Represents an HTTP response with status code, typed data, error information, and response headers.

### HttpContentExtensions

- **Namespace:** `SmartWorkz.Extensions.HttpContentExtensions`
- **Summary:** Extension methods for HttpContent to simplify JSON deserialization.

#### Methods & Properties

- **ReadAsAsync``1** - Reads and deserializes JSON content asynchronously.
  - Parameters:
    - `content`: The HTTP content.
    - `cancellationToken`: Cancellation token.
  - Returns: Deserialized object of type T.

### HttpContentExtensions

- **Namespace:** `SmartWorkz.Extensions.HttpContentExtensions`
- **Summary:** Extension methods for HttpContent to simplify JSON deserialization.

#### Methods & Properties

- **ReadAsAsync``1** - Reads and deserializes JSON content asynchronously.
  - Parameters:
    - `content`: The HTTP content.
    - `cancellationToken`: Cancellation token.
  - Returns: Deserialized object of type T.

