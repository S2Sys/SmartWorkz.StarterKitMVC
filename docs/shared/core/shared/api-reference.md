# Shared API Reference

## Classes & Interfaces

### AuditEntry

- **Namespace:** `SmartWorkz.Shared.AuditEntry`
- **Summary:** Immutable audit log entry for tracking entity changes and domain events.
            Records who did what, when, where, and why for compliance and debugging.

### AuditEventSubscriber

- **Namespace:** `SmartWorkz.Shared.AuditEventSubscriber`
- **Summary:** Subscribes to domain events and records them in the audit trail.
            Enables automatic audit capture without requiring explicit audit calls in business logic.

#### Methods & Properties

- **OnEventPublishedAsync** - Record a domain event in the audit trail.
  - Parameters:
    - `evt`: The domain event to record.
    - `userId`: User ID who triggered the event (optional for system events).
    - `ipAddress`: IP address of the request originator (optional).
    - `cancellationToken`: Cancellation token.

### AuditStartupExtensions

- **Namespace:** `SmartWorkz.Shared.AuditStartupExtensions`
- **Summary:** Dependency injection and schema setup for audit trail functionality.

#### Methods & Properties

- **AddAuditTrail** - Register IAuditTrail with SQL Server implementation.
- **CreateAuditTrailSchema** - Create the AuditTrail table and indexes if they don't exist.
            Call this during application startup or migration.

### IAuditTrail

- **Namespace:** `SmartWorkz.Shared.IAuditTrail`
- **Summary:** Service for recording and querying immutable audit entries.
            Abstracts the persistence mechanism for audit trails.

#### Methods & Properties

- **RecordAsync** - Record an audit entry (immutable append-only).
  - Parameters:
    - `entry`: The audit entry to record.
    - `cancellationToken`: Cancellation token.
- **GetEntriesAsync** - Get all audit entries for a specific entity instance.
  - Parameters:
    - `entityType`: Type of entity (e.g., "Order").
    - `entityId`: Entity instance ID.
    - `cancellationToken`: Cancellation token.
- **GetEntriesByActionAsync** - Get audit entries by action type (Created, Updated, Deleted, etc.).
  - Parameters:
    - `action`: The action to filter by.
    - `since`: Optional: only entries after this date.
    - `cancellationToken`: Cancellation token.
- **GetEntriesByUserAsync** - Get audit entries for a specific user.
  - Parameters:
    - `userId`: User ID who performed actions.
    - `since`: Optional: only entries after this date.
    - `cancellationToken`: Cancellation token.
- **SearchAsync** - Search audit trail with multiple filter criteria.
            All criteria are AND'd together (null criteria are ignored).
  - Parameters:
    - `entityType`: Optional entity type filter.
    - `action`: Optional action filter.
    - `userId`: Optional user ID filter.
    - `since`: Optional timestamp filter (inclusive).
    - `cancellationToken`: Cancellation token.

### SqlAuditTrail

- **Namespace:** `SmartWorkz.Shared.SqlAuditTrail`
- **Summary:** SQL Server implementation of IAuditTrail for immutable audit log persistence.
            Appends audit entries to a single table with indexes for efficient querying.

#### Methods & Properties

- **RecordAsync** - 
- **GetEntriesAsync** - 
- **GetEntriesByActionAsync** - 
- **GetEntriesByUserAsync** - 
- **SearchAsync** - 

### ValueConverter`1

- **Namespace:** `SmartWorkz.Shared.ValueConverter`1`
- **Summary:** Abstract base class for type conversion between domain objects and DTOs.
            Enables loose coupling between layers by centralizing conversion logic.

#### Methods & Properties

- **Convert``1** - Convert a single source object to target type.
- **Convert** - Convert a single source object using dynamic target type resolution.
- **ConvertList``1** - Convert a collection of source objects to target type.
- **ConvertList** - Convert a collection using dynamic target type resolution.
- **ConvertFromList``2** - Convert from a collection of different source types.

### CacheEntry`1

- **Namespace:** `SmartWorkz.Shared.CacheEntry`1`
- **Summary:** Represents a cached entry with data, expiration time, and metadata.

#### Methods & Properties

- **#ctor** - Creates a new CacheEntry instance.
- **#ctor** - Creates a new CacheEntry instance with data and expiration.
- **RenewExpiry** - Renews the expiry time based on the cache strategy and TTL.

### CacheEntryWrapper

- **Namespace:** `SmartWorkz.Shared.CacheEntryWrapper`
- **Summary:** Non-generic wrapper for CacheEntry to store in the cache dictionary.

### CacheOptions

- **Namespace:** `SmartWorkz.Shared.CacheOptions`
- **Summary:** Configuration options for cache operations.

#### Methods & Properties

- **#ctor** - Creates a new CacheOptions instance with default values.
- **#ctor** - Creates a new CacheOptions instance with specified TTL.
- **#ctor** - Creates a new CacheOptions instance with specified TTL and cache strategy.
- **#ctor** - Creates a new CacheOptions instance with all parameters.

### CacheStrategy

- **Namespace:** `SmartWorkz.Shared.CacheStrategy`
- **Summary:** Enumeration of cache expiration strategies.

### ICacheService

- **Namespace:** `SmartWorkz.Shared.ICacheService`
- **Summary:** Service for caching with tenant isolation and L1/L2 hybrid support.
            Implementations may use memory cache (L1) and distributed cache (L2).
            All cache operations are tenant-scoped with automatic key prefixing.

#### Methods & Properties

- **GetAsync``1** - Gets a cached value by key with tenant isolation.
  - Parameters:
    - `key`: Cache key (will be tenant-scoped automatically).
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Result containing cached value, or failure if not found or expired.
- **SetAsync``1** - Sets a value in cache with optional TTL for the specified tenant.
  - Parameters:
    - `key`: Cache key (will be tenant-scoped automatically).
    - `value`: Value to cache.
    - `ttlMinutes`: Time to live in minutes. If null, value never expires.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveAsync** - Removes a cached value by key for the specified tenant.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveByPrefixAsync** - Removes all cached values matching a key prefix for the specified tenant.
            Example: RemoveByPrefixAsync("user:") removes all "user:*" entries for that tenant.
  - Parameters:
    - `prefix`: Key prefix to match (may include wildcard suffix like "user:*").
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **ExistsAsync** - Checks if a key exists in cache for the specified tenant.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: True if key exists and is not expired; false otherwise.
- **ClearAsync** - Clears all cache entries for the specified tenant.
            Does not affect entries for other tenants.
  - Parameters:
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.

### ICacheStore

- **Namespace:** `SmartWorkz.Shared.ICacheStore`
- **Summary:** Abstraction for a cache store with support for various operations including TTL and expiration strategies.

#### Methods & Properties

- **GetAsync``1** - Retrieves a value from the cache.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result containing the cached value or null if not found or expired.
- **SetAsync``1** - Sets a value in the cache with optional TTL.
  - Parameters:
    - `key`: The cache key.
    - `value`: The value to cache.
    - `ttlMinutes`: Optional time-to-live in minutes. If null, uses default or no expiration.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **SetAsync``1** - Sets a value in the cache with cache options.
  - Parameters:
    - `key`: The cache key.
    - `value`: The value to cache.
    - `options`: Cache options including TTL, strategy, and sliding expiration.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **RemoveAsync** - Removes a value from the cache.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **RemoveByPrefixAsync** - Removes all values from the cache that match the specified key prefix.
  - Parameters:
    - `keyPrefix`: The prefix to match.
    - `ct`: Cancellation token.
  - Returns: A Result containing the number of entries removed.
- **ExistsAsync** - Checks if a key exists in the cache and is not expired.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result indicating whether the key exists and is valid.
- **ClearAsync** - Clears all entries from the cache.
  - Parameters:
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.

### MemoryCacheService

- **Namespace:** `SmartWorkz.Shared.MemoryCacheService`
- **Summary:** In-memory L1 cache service implementation with thread-safe operations and tenant isolation.
            Suitable for single-process deployments with TTL and expiration support.

#### Methods & Properties

- **BuildKey** - Builds a tenant-scoped cache key.
  - Parameters:
    - `key`: Original cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
  - Returns: Tenant-scoped key in format "{tenantId}:{key}".
- **GetAsync``1** - Gets a cached value by key with tenant isolation. Returns failure if not found or expired.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Result containing cached value, or failure if not found or expired.
- **SetAsync``1** - Sets a cached value with optional TTL expiration and tenant isolation.
  - Parameters:
    - `key`: Cache key.
    - `value`: Value to cache.
    - `ttlMinutes`: Time to live in minutes. If null, no expiration.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveAsync** - Removes a single cache entry with tenant isolation.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveByPrefixAsync** - Removes all cache entries matching a prefix pattern with tenant isolation.
            Example: RemoveByPrefixAsync("user:*", "tenant1") removes "tenant1:user:*" entries.
  - Parameters:
    - `prefix`: Key prefix to match.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **ExistsAsync** - Checks if a key exists in the cache with tenant isolation (ignores expiration check).
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: True if key exists and is not expired; false otherwise.
- **ClearAsync** - Clears all cache entries for the specified tenant (or "default" if not specified).
            Does not clear entries from other tenants.
  - Parameters:
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.

### MemoryCacheStore

- **Namespace:** `SmartWorkz.Shared.MemoryCacheStore`
- **Summary:** In-memory implementation of ICacheStore with TTL support and thread-safe operations.

#### Methods & Properties

- **#ctor** - Creates a new instance of MemoryCacheStore with default options.
- **#ctor** - Creates a new instance of MemoryCacheStore with specified default options.
- **GetAsync``1** - Retrieves a value from the cache.
- **SetAsync``1** - Sets a value in the cache with optional TTL.
- **SetAsync``1** - Sets a value in the cache with cache options.
- **RemoveAsync** - Removes a value from the cache.
- **RemoveByPrefixAsync** - Removes all values from the cache that match the specified key prefix.
- **ExistsAsync** - Checks if a key exists in the cache and is not expired.
- **ClearAsync** - Clears all entries from the cache.
- **CleanupExpiredEntries** - Performs cleanup of expired entries. This is useful for periodic maintenance.

### ISmsService

- **Namespace:** `SmartWorkz.Shared.ISmsService`
- **Summary:** Defines a contract for SMS communication services.
            Provides methods for sending SMS messages to single or multiple recipients.

#### Methods & Properties

- **SendAsync** - Sends an SMS message to a single recipient.
  - Parameters:
    - `phoneNumber`: The recipient phone number (E.164 format recommended)
    - `message`: The SMS message content
    - `cancellationToken`: Cancellation token
  - Returns: Result containing the SMS ID if successful
- **SendBatchAsync** - Sends an SMS message to multiple recipients (batch).
  - Parameters:
    - `phoneNumbers`: Collection of recipient phone numbers
    - `message`: The SMS message content sent to all recipients
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of SMS IDs if successful

### IWebSocketClient

- **Namespace:** `SmartWorkz.Shared.IWebSocketClient`
- **Summary:** Abstraction for WebSocket client operations.

#### Methods & Properties

- **SendAsync** - Sends a message asynchronously through the WebSocket connection.
- **ReceiveAsync** - Receives a message asynchronously from the WebSocket connection.
            Returns null if the connection is closed.
- **CloseAsync** - Closes the WebSocket connection gracefully.

### WebSocketClient

- **Namespace:** `SmartWorkz.Shared.WebSocketClient`
- **Summary:** Sealed implementation of IWebSocketClient using System.Net.WebSockets.

#### Methods & Properties

- **ConnectAsync** - Connects to a WebSocket server at the specified URI.
- **SendAsync** - Sends a message asynchronously through the WebSocket connection.
- **ReceiveAsync** - Receives a message asynchronously from the WebSocket connection.
            Returns null if the connection is closed.
- **CloseAsync** - Closes the WebSocket connection gracefully.

### ConfigurationHelper

- **Namespace:** `SmartWorkz.Shared.ConfigurationHelper`
- **Summary:** Provides typed access to configuration values with automatic type conversion and validation.
            
             This sealed class implements IConfigurationHelper to provide a strongly-typed interface
             for accessing configuration values. It supports automatic type conversion for common types
             including strings, numeric types, booleans, DateTimes, and enums.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ConfigurationHelper class.
  - Parameters:
    - `configuration`: The configuration source to read from.
- **GetRequired``1** - Gets a required configuration value and converts it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: The configuration value converted to type T.
- **GetOptional``1** - Gets an optional configuration value and converts it to the specified type,
            returning a default value if the key is not found or conversion fails.
  - Parameters:
    - `key`: The configuration key to retrieve.
    - `defaultValue`: The value to return if the key is not found or conversion fails.
  - Returns: The configuration value converted to type T, or the default value if retrieval or conversion fails.
- **TryGet``1** - Attempts to get a configuration value and convert it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: A Result<T> containing the converted value on success, or a failure result
            if the key is not found or conversion fails.
- **Exists** - Checks whether a configuration key exists and is not empty.
  - Parameters:
    - `key`: The configuration key to check.
  - Returns: True if the key exists and is not null or whitespace; otherwise false.
- **ConvertValue``1** - Converts a string value to the specified type using invariant culture for numeric types.
  - Parameters:
    - `value`: The string value to convert.
  - Returns: The converted value of type T.

### ConfigurationValidationException

- **Namespace:** `SmartWorkz.Shared.ConfigurationValidationException`
- **Summary:** Exception thrown when configuration validation fails, indicating that a required
            configuration key is missing, empty, or cannot be converted to the requested type.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ConfigurationValidationException class with a specified message.
  - Parameters:
    - `message`: The error message that explains the reason for the exception.
- **#ctor** - Initializes a new instance of the ConfigurationValidationException class with a specified message
            and a reference to the inner exception that is the cause of this exception.
  - Parameters:
    - `message`: The error message that explains the reason for the exception.
    - `innerException`: The exception that is the cause of the current exception.

### IConfigurationHelper

- **Namespace:** `SmartWorkz.Shared.IConfigurationHelper`
- **Summary:** Provides typed access to configuration values with automatic type conversion and validation.

#### Methods & Properties

- **GetRequired``1** - Gets a required configuration value and converts it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: The configuration value converted to type T.
- **GetOptional``1** - Gets an optional configuration value and converts it to the specified type,
            returning a default value if the key is not found or conversion fails.
  - Parameters:
    - `key`: The configuration key to retrieve.
    - `defaultValue`: The value to return if the key is not found or conversion fails.
  - Returns: The configuration value converted to type T, or the default value if retrieval or conversion fails.
- **TryGet``1** - Attempts to get a configuration value and convert it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: A Result<T> containing the converted value on success, or a failure result
            if the key is not found or conversion fails.
- **Exists** - Checks whether a configuration key exists and is not empty.
  - Parameters:
    - `key`: The configuration key to check.
  - Returns: True if the key exists and is not null or whitespace; otherwise false.

### SharedConstants

- **Namespace:** `SmartWorkz.Shared.SharedConstants`
- **Summary:** Shared configuration constants used throughout SmartWorkz.Shared.
            Enables centralized management of default values and limits.

### ICommand

- **Namespace:** `SmartWorkz.Shared.ICommand`
- **Summary:** Marker interface for command objects representing intent to change state.

### ICommandHandler`1

- **Namespace:** `SmartWorkz.Shared.ICommandHandler`1`
- **Summary:** Handler for processing a specific command type.

#### Methods & Properties

- **HandleAsync** - Handles the specified command asynchronously.
  - Parameters:
    - `command`: The command to handle.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task that represents the asynchronous handle operation.

### IQuery`1

- **Namespace:** `SmartWorkz.Shared.IQuery`1`
- **Summary:** Marker interface for query objects that return a result without modifying state.

### IQueryHandler`2

- **Namespace:** `SmartWorkz.Shared.IQueryHandler`2`
- **Summary:** Handler for processing a specific query type and returning results.

#### Methods & Properties

- **HandleAsync** - Handles the specified query asynchronously and returns the result.
  - Parameters:
    - `query`: The query to handle.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task that represents the asynchronous handle operation and contains the query result.

### MediatorCommandDispatcher

- **Namespace:** `SmartWorkz.Shared.MediatorCommandDispatcher`
- **Summary:** Routes commands to their appropriate handlers via dependency injection.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `serviceProvider`: The service provider for resolving handlers.
- **DispatchAsync``1** - Dispatches the specified command to its handler asynchronously.
  - Parameters:
    - `command`: The command to dispatch.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task representing the asynchronous dispatch operation.

### AdoHelper

- **Namespace:** `SmartWorkz.Shared.AdoHelper`
- **Summary:** ADO.NET helper for executing queries and managing connections.
            Works with any IDbProvider implementation.

#### Methods & Properties

- **ExecuteScalarAsync``1** - Execute scalar query (returns single value).
- **ExecuteNonQueryAsync** - Execute non-query command (INSERT, UPDATE, DELETE).
- **ExecuteQueryAsync``1** - Execute query and map results to objects.
- **ExecuteStoredProcedureAsync** - Execute stored procedure.
- **ExecuteQueryMultipleAsync``2** - Execute query returning multiple result sets (2 sets).
- **ExecuteQueryMultipleAsync``3** - Execute query returning multiple result sets (3 sets).
- **ExecuteQueryMultipleAsync``4** - Execute query returning multiple result sets (4 sets).
- **ExecuteTransactionAsync** - Execute transaction with multiple commands.

### CsvHelper

- **Namespace:** `SmartWorkz.Shared.CsvHelper`
- **Summary:** Provides static methods for reading and writing CSV data with support for column mapping,
            quoted fields, embedded delimiters, and newlines.
            RFC 4180 compliant CSV parsing and writing.
            Sealed class to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **CsvWriter``1** - Serializes a collection of objects to CSV format.
  - Parameters:
    - `items`: The collection of objects to serialize.
    - `options`: CSV options. If null, defaults are used.
  - Returns: A Result containing the CSV string if successful; otherwise a failure.
- **CsvReader``1** - Asynchronously deserializes CSV content to a collection of objects.
  - Parameters:
    - `content`: The CSV content string.
    - `mapping`: Column mapping configuration. If null, property names are used as headers.
    - `options`: CSV options. If null, defaults are used.
  - Returns: A Result containing the deserialized list if successful; otherwise a failure.
- **ParseCsvLines** - Parses CSV content into a list of records (each record is a list of field values).
            Handles quoted fields with embedded delimiters and newlines.
- **WriteRecord** - Writes a single CSV record (list of field values) to the string builder.
            Handles quoting of fields with special characters.
- **ConvertValue** - Converts a string value to the specified type.
- **IsNullableType** - Determines if a type is nullable (Nullable<T> or reference type).

### CsvMapping`1

- **Namespace:** `SmartWorkz.Shared.CsvMapping`1`
- **Summary:** Defines column mapping for CSV operations using a fluent API.
            Supports mapping object properties to CSV columns with custom headers.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **Column``1** - Adds a column mapping for the specified property.
  - Parameters:
    - `propertyExpression`: Expression selecting the property to map.
    - `csvHeader`: The CSV column header name.
  - Returns: This instance for method chaining.
- **ExtractPropertyInfo``1** - Extracts property information from a lambda expression.
  - Parameters:
    - `expression`: The lambda expression.
  - Returns: The PropertyInfo if the expression resolves to a property; otherwise null.
- **CreateAuto** - Creates a mapping automatically from all public properties of type T.
            Property names are used as CSV headers.
  - Returns: A new CsvMapping instance with all properties mapped.

### CsvOptions

- **Namespace:** `SmartWorkz.Shared.CsvOptions`
- **Summary:** Configuration options for CSV read/write operations.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **#ctor** - Creates a default instance of CsvOptions.
- **#ctor** - Creates an instance of CsvOptions with specified delimiter and quote character.
  - Parameters:
    - `delimiter`: The field delimiter character.
    - `quoteChar`: The quote character for quoted fields.

### DbProviderFactory

- **Namespace:** `SmartWorkz.Shared.DbProviderFactory`
- **Summary:** Factory for creating database provider instances.
            Resolves provider name from connection string or explicit specification.

#### Methods & Properties

- **Register** - Register custom provider implementation.
- **GetProvider** - Get provider by name.
- **GetProvider** - Get provider by enum value.
- **GetProviderFromConnectionString** - Get provider from connection string (detects provider automatically).

### IDbProvider

- **Namespace:** `SmartWorkz.Shared.IDbProvider`
- **Summary:** Abstraction for database provider-specific operations.
            Supports multiple providers: SQL Server, MySQL, PostgreSQL, SQLite, Oracle.

#### Methods & Properties

- **CreateConnection** - Create connection with connection string.
- **GetParameterPrefix** - Get parameter prefix for this provider (@, :, $).
- **GetLastInsertIdSql** - Get SQL for last inserted ID based on provider.
- **GetPaginationSql** - Get SQL for pagination based on provider.
- **FormatIdentifier** - Format table/column name for provider (e.g., [brackets] for SQL Server).
- **TestConnectionAsync** - Test connection validity.

### DatabaseProvider

- **Namespace:** `SmartWorkz.Shared.DatabaseProvider`
- **Summary:** Enum of supported database providers.

### QueryMultipleHelper

- **Namespace:** `SmartWorkz.Shared.QueryMultipleHelper`
- **Summary:** Helper for executing multiple queries in a single database roundtrip.
            Eliminates N+1 query problems by batching queries together.

#### Methods & Properties

- **QueryMultipleAsync``2** - Execute multiple queries and return results as tuple.
             Single roundtrip, single SQL execution, improved performance.
- **QueryMultipleAsync``3** - Execute 3 queries in single roundtrip.
- **QueryMultipleAsync``4** - Execute 4 queries in single roundtrip.
- **QueryMultipleAsync``5** - Execute 5 queries in single roundtrip.

### QueryResult`1

- **Namespace:** `SmartWorkz.Shared.QueryResult`1`
- **Summary:** Result wrapper for query operations.

### XmlHelper

- **Namespace:** `SmartWorkz.Shared.XmlHelper`
- **Summary:** Provides static methods for XML serialization, deserialization, and XPath queries.
            Uses System.Xml.Linq for manipulation and reflection for property mapping.
            Sealed class to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **Serialize``1** - Serializes an object to an XML string using reflection.
  - Parameters:
    - `obj`: The object to serialize.
    - `options`: XML options. If null, defaults are used.
  - Returns: A Result containing the XML string if successful; otherwise a failure.
- **Deserialize``1** - Deserializes an XML string to an object of type T using reflection.
  - Parameters:
    - `xml`: The XML string to deserialize.
    - `options`: XML options. If null, defaults are used.
  - Returns: A Result containing the deserialized object if successful; otherwise a failure.
- **Query** - Executes an XPath query on an XML string and returns matching element values.
  - Parameters:
    - `xml`: The XML string to query.
    - `xpathExpression`: The XPath expression to execute.
  - Returns: A Result containing a list of matched values if successful; otherwise a failure.
- **SerializeObject** - Recursively serializes an object's properties into an XML element.
- **DeserializeObject** - Recursively deserializes an XML element into an object's properties.
- **IsBasicType** - Determines if a type is a basic/primitive type supported by XML.
- **IsGenericList** - Determines if a type is a generic List<T>.
- **IsComplexType** - Determines if a type is a complex (non-primitive) type.
- **ConvertToXmlValue** - Converts a value to its XML-safe string representation.
- **ConvertFromXmlValue** - Converts an XML string value to the specified type.

### XmlOptions

- **Namespace:** `SmartWorkz.Shared.XmlOptions`
- **Summary:** Configuration options for XML serialization, deserialization, and query operations.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **#ctor** - Creates a default instance of XmlOptions.
- **#ctor** - Creates an instance of XmlOptions with a specified root element name.
  - Parameters:
    - `rootElement`: The name of the root element.
- **#ctor** - Creates an instance of XmlOptions with specified configuration.
  - Parameters:
    - `rootElement`: The name of the root element.
    - `includeXmlDeclaration`: Whether to include the XML declaration.
    - `indent`: Whether to indent the output.

### ApplicationHealth

- **Namespace:** `SmartWorkz.Shared.ApplicationHealth`
- **Summary:** Represents the overall health status of the application.

### CorrelationContext

- **Namespace:** `SmartWorkz.Shared.CorrelationContext`
- **Summary:** A sealed implementation of  for distributed request tracing.

#### Methods & Properties

- **#ctor** - Initializes a new instance with a generated correlation ID.
- **#ctor** - Initializes a new instance with a specified correlation ID.
  - Parameters:
    - `correlationId`: The correlation ID to use
- **#ctor** - Initializes a child context from a parent context.

### CpuUsage

- **Namespace:** `SmartWorkz.Shared.CpuUsage`
- **Summary:** Represents CPU usage information.

### DiagnosticsHelper

- **Namespace:** `SmartWorkz.Shared.DiagnosticsHelper`
- **Summary:** Sealed helper class for system diagnostics and application health monitoring.
            Provides methods to gather system information, CPU/memory/disk usage, and determine application health.

#### Methods & Properties

- **Initialize** - Initializes the application start time (called once at application startup).
- **GetSystemInfo** - Gets comprehensive system information including CPU, memory, disk, and processor count.
  - Returns: A Result containing SystemInfo or error details.
- **GetMemoryUsage** - Gets memory usage statistics for the current process and system.
  - Returns: A Result containing MemoryUsage or error details.
- **GetCpuUsage** - Gets CPU utilization percentage.
  - Returns: A Result containing CpuUsage or error details.
- **GetDiskSpace** - Gets disk space information for a specific drive.
  - Parameters:
    - `drive`: The drive letter (e.g., "C:", "D:"). Defaults to "C:".
  - Returns: A Result containing DiskSpace or error details.
- **GetUptime** - Gets the application uptime since the last Initialize() call or application start.
  - Returns: A Result containing the uptime as a TimeSpan or error details.
- **GetApplicationHealth** - Gets the overall health status of the application based on system metrics.
  - Returns: A Result containing ApplicationHealth or error details.
- **IsHealthy** - Determines if the application is considered healthy based on the provided health status.
  - Parameters:
    - `health`: The ApplicationHealth object to evaluate.
  - Returns: True if the status is Healthy, false otherwise.
- **InitializeCpuCounter** - Initializes the CPU performance counter (called once).
- **GetMemoryUsageInternal** - Internal method to get memory usage statistics.
- **GetCpuUsageInternal** - Internal method to get CPU usage percentage.
- **GetDiskSpaceInternal** - Internal method to get disk space information.

### DiskSpace

- **Namespace:** `SmartWorkz.Shared.DiskSpace`
- **Summary:** Represents disk space information for a drive.

### HealthCheck

- **Namespace:** `SmartWorkz.Shared.HealthCheck`
- **Summary:** Represents a single health check result.

### HealthStatus

- **Namespace:** `SmartWorkz.Shared.HealthStatus`
- **Summary:** Represents the health status of the application.

### ICorrelationContext

- **Namespace:** `SmartWorkz.Shared.ICorrelationContext`
- **Summary:** Defines a correlation context for distributed request tracing across systems.

#### Methods & Properties

- **SetProperty** - Adds or updates a property in the correlation context.
- **TryGetProperty** - Attempts to retrieve a property from the correlation context.
- **CreateChildContext** - Creates a child correlation context for nested operations (for async/distributed flows).

### MemoryUsage

- **Namespace:** `SmartWorkz.Shared.MemoryUsage`
- **Summary:** Represents memory usage information.

### MetricsHelper

- **Namespace:** `SmartWorkz.Shared.MetricsHelper`
- **Summary:** Provides utilities for collecting and tracking performance metrics.

#### Methods & Properties

- **StartTimer** - Starts a timer and returns an IDisposable that logs elapsed time on disposal.
- **TrackExecution``1** - Tracks the execution time and result of a function.
- **MeasureMemory** - Captures memory usage before and after a block of code execution.

### SystemInfo

- **Namespace:** `SmartWorkz.Shared.SystemInfo`
- **Summary:** Represents system information including CPU, memory, and disk details.

### EventStoreSnapshot

- **Namespace:** `SmartWorkz.Shared.EventStoreSnapshot`
- **Summary:** Represents a snapshot of an aggregate's state at a specific version.
            Snapshots optimize event sourcing by reducing the number of events needed for reconstruction.

### IEventStore

- **Namespace:** `SmartWorkz.Shared.IEventStore`
- **Summary:** Abstraction for an immutable event store that persists domain events.
            Enables event sourcing patterns for temporal queries, audit trails, and event replay.

#### Methods & Properties

- **AppendEventsAsync** - Appends events to the event stream for an aggregate.
            Events are immutable and persist as an append-only log.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `events`: The domain events to append
    - `cancellationToken`: Cancellation token
  - Returns: Task representing the asynchronous operation
- **GetEventsAsync** - Retrieves all events for an aggregate in chronological order.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: Collection of domain events for the aggregate, empty if none exist
- **GetEventsSinceAsync** - Retrieves events for an aggregate after a specific version.
            Useful for incremental event replay and event streaming.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `version`: The version after which to retrieve events
    - `cancellationToken`: Cancellation token
  - Returns: Collection of events after the specified version, empty if none exist
- **GetSnapshotAsync** - Retrieves the latest snapshot for an aggregate if one exists.
            Snapshots optimize aggregate reconstruction by storing intermediate state.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: Snapshot data if exists; null if no snapshot is available
- **SaveSnapshotAsync** - Saves a snapshot of aggregate state at a specific version.
            Snapshots reduce the number of events needed to replay an aggregate.
  - Parameters:
    - `snapshot`: The snapshot to save
    - `cancellationToken`: Cancellation token
  - Returns: Task representing the asynchronous operation
- **GetAggregateAsync``1** - Reconstructs an aggregate from its event history.
            Optionally uses snapshots for performance optimization.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: The reconstructed aggregate instance, or null if no events exist

### SqlEventStore

- **Namespace:** `SmartWorkz.Shared.SqlEventStore`
- **Summary:** SQL Server implementation of the event store using Dapper for data access.
            Provides immutable append-only event log with snapshot support for optimization.
            Implements optimistic concurrency control using version numbers.

#### Methods & Properties

- **AppendEventsAsync** - Appends events to the event stream for an aggregate.
- **GetEventsAsync** - Retrieves all events for an aggregate in chronological order.
- **GetEventsSinceAsync** - Retrieves events for an aggregate after a specific version.
- **GetSnapshotAsync** - Retrieves the latest snapshot for an aggregate if one exists.
- **SaveSnapshotAsync** - Saves a snapshot of aggregate state at a specific version.
- **GetAggregateAsync``1** - Reconstructs an aggregate from its event history.
            Optionally uses snapshots for performance optimization.
- **GetCurrentVersionAsync** - Gets the current version number for an aggregate.
- **DeserializeEvent** - Deserializes a stored event record back to IDomainEvent.

### IDomainEvent

- **Namespace:** `SmartWorkz.Shared.IDomainEvent`
- **Summary:** Base interface for domain events in event-driven architecture.
            Provides core event metadata for tracking and publishing.

### IEventPublisher

- **Namespace:** `SmartWorkz.Shared.IEventPublisher`
- **Summary:** Publishes domain events for event-driven architecture.

#### Methods & Properties

- **PublishAsync``1** - Publishes a single domain event.
  - Parameters:
    - `event`: Event to publish.
    - `cancellationToken`: Cancellation token.
- **PublishAsync``1** - Publishes multiple domain events.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token.

### IEventSubscriber

- **Namespace:** `SmartWorkz.Shared.IEventSubscriber`
- **Summary:** Registers event handlers for domain events.

#### Methods & Properties

- **Subscribe``1** - Subscribes a handler to an event type.
            Multiple handlers can subscribe to the same event.
  - Parameters:
    - `handler`: Async handler function. Receives event and cancellation token.

### InMemoryEventPublisher

- **Namespace:** `SmartWorkz.Shared.InMemoryEventPublisher`
- **Summary:** In-memory event publisher that executes all registered handlers sequentially.
            Provides synchronous event delivery with exception handling and result reporting.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the InMemoryEventPublisher with a subscriber.
  - Parameters:
    - `subscriber`: The event subscriber containing registered handlers.
- **PublishAsync``1** - Publishes a single domain event to all registered handlers.
            Handlers are invoked sequentially in registration order.
            If any handler throws an exception, it is caught and a failure Result is returned.
            Other handlers will attempt to execute even if a previous handler fails.
  - Parameters:
    - `event`: The event instance to publish.
    - `cancellationToken`: Cancellation token to cancel handler execution.
  - Returns: A task representing the asynchronous operation.
- **PublishAsync``1** - Publishes multiple domain events to all registered handlers.
            Events are published sequentially in the order provided.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token to cancel handler execution.
  - Returns: A task representing the asynchronous batch operation.

### InMemoryEventSubscriber

- **Namespace:** `SmartWorkz.Shared.InMemoryEventSubscriber`
- **Summary:** In-memory event subscriber that maintains a registry of event handlers.
            Supports multiple handlers per event type using thread-safe concurrent collections.

#### Methods & Properties

- **Subscribe``1** - Subscribes a handler to an event type.
            Multiple handlers can be registered for the same event type and will execute sequentially.
  - Parameters:
    - `handler`: Async handler function that receives event and cancellation token.
- **GetHandlers** - Gets all registered handlers for a given event type.
            Returns an empty list if no handlers are registered for the type.
  - Parameters:
    - `eventType`: The event type to retrieve handlers for.
  - Returns: List of registered handlers (delegates).

### MassTransitEventPublisher

- **Namespace:** `SmartWorkz.Shared.MassTransitEventPublisher`
- **Summary:** Distributed event publisher using MassTransit message bus.
            Supports both single and batch event publishing with async/await patterns.
            Suitable for production environments with message broker backend (RabbitMQ, Azure Service Bus, etc).

#### Methods & Properties

- **#ctor** - Initializes a new instance of MassTransitEventPublisher.
  - Parameters:
    - `publishEndpoint`: MassTransit publish endpoint for message distribution.
    - `logger`: Logger for event publication tracking.
- **PublishAsync``1** - Publishes a single domain event to the message bus asynchronously.
  - Parameters:
    - `event`: Event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous publish operation.
- **PublishAsync``1** - Publishes multiple domain events to the message bus asynchronously.
            Events are published sequentially in the order provided.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous batch publish operation.

### PublisherType

- **Namespace:** `SmartWorkz.Shared.PublisherType`
- **Summary:** Specifies the publisher type for event publishing.

### ServiceCollectionExtensions

- **Namespace:** `SmartWorkz.Shared.ServiceCollectionExtensions`
- **Summary:** Extension methods for IServiceCollection to register Core.Shared services.

#### Methods & Properties

- **AddCoreSharedServices** - Adds Core.Shared services including TemplateEngine for template rendering.
- **AddEventPublishing** - Adds event publishing services to the dependency injection container.
            Supports switching between in-memory and MassTransit publishers based on application needs.
  - Parameters:
    - `services`: The service collection.
    - `publisherType`: The publisher type to use (defaults to InMemory).
  - Returns: The service collection for method chaining.

### DefaultFeatureFlagService

- **Namespace:** `SmartWorkz.Shared.DefaultFeatureFlagService`
- **Summary:** Global (non-tenant) feature flag service with in-memory storage.
            Thread-safe implementation suitable for single-process deployments.
            Use for organization-wide feature toggles; use ITenantFeatureFlags for tenant-scoped flags.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature flag is enabled.
            Returns false for unknown flags (does not throw).
  - Parameters:
    - `flagName`: The name of the feature flag to check.
    - `cancellationToken`: Cancellation token.
  - Returns: True if the flag exists and is enabled; false otherwise.
- **GetEnabledFeaturesAsync** - Gets all enabled feature flags.
            Returns empty list if no flags are enabled.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: A read-only list of enabled feature flag names.
- **EnableFlag** - Enables a feature flag.
            Creates the flag if it doesn't exist.
  - Parameters:
    - `flagName`: The name of the feature flag to enable.
- **DisableFlag** - Disables a feature flag.
            Creates the flag as disabled if it doesn't exist.
  - Parameters:
    - `flagName`: The name of the feature flag to disable.

### IFeatureFlagService

- **Namespace:** `SmartWorkz.Shared.IFeatureFlagService`
- **Summary:** Global feature flag service for cross-tenant feature control.
            Use for organization-wide feature toggles (not tenant-specific).
            For tenant-scoped flags, use ITenantFeatureFlags.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a global feature is enabled.
  - Parameters:
    - `flagName`: Feature flag name (e.g., "NEW_DASHBOARD", "BETA_REPORTING").
    - `cancellationToken`: Cancellation token.
  - Returns: True if feature is enabled globally.
- **GetEnabledFeaturesAsync** - Gets all enabled global features.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: List of enabled feature flag names.

### IFileStorageService

- **Namespace:** `SmartWorkz.Shared.IFileStorageService`
- **Summary:** Interface for file storage operations supporting both local and cloud providers.

#### Methods & Properties

- **UploadAsync** - Uploads a file to storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `content`: The file content stream.
    - `metadata`: The file metadata.
    - `cancellationToken`: The cancellation token.
  - Returns: The full path or URI of the uploaded file.
- **DownloadAsync** - Downloads a file from storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: A stream containing the file content. Caller must dispose using 'using' statement.
- **DeleteAsync** - Deletes a file from storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
- **ExistsAsync** - Checks if a file exists in storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: True if the file exists, false otherwise.
- **GetMetadataAsync** - Gets metadata for a file in storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: FileMetadata if file exists, null otherwise.
- **ListAsync** - Lists files in a directory or container prefix.
  - Parameters:
    - `folderPath`: The relative folder path or blob prefix.
    - `cancellationToken`: The cancellation token.
  - Returns: A read-only collection of FileMetadata for files in the directory/prefix.
- **GenerateTemporaryUrlAsync** - Generates a temporary download URL for a file.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `expiration`: The expiration duration from now.
    - `cancellationToken`: The cancellation token.
  - Returns: A URL that can be used to download the file. For local storage, returns the full file path.

### GridColumn

- **Namespace:** `SmartWorkz.Shared.GridColumn`
- **Summary:** Defines a single column in a grid, including display options, sorting, filtering, and rendering hints.

### GridExportOptions

- **Namespace:** `SmartWorkz.Shared.GridExportOptions`
- **Summary:** Configuration for grid data export (CSV, Excel).

### GridRequest

- **Namespace:** `SmartWorkz.Shared.GridRequest`
- **Summary:** Request parameters for grid data fetching, extending PagedQuery with filtering support.

#### Methods & Properties

- **#ctor** - Request parameters for grid data fetching, extending PagedQuery with filtering support.

### GridResponse`1

- **Namespace:** `SmartWorkz.Shared.GridResponse`1`
- **Summary:** Response from a grid data request, including paged data, column metadata, and filter options.

### IGridDataProvider

- **Namespace:** `SmartWorkz.Shared.IGridDataProvider`
- **Summary:** Abstraction for grid data fetching. Implementations handle API calls or in-memory queries.
            Enables platform independence: Web uses HTTP, MAUI uses direct API client, Desktop uses local DB.

#### Methods & Properties

- **GetDataAsync``1** - Fetch paged grid data based on request (sorting, filtering, pagination).
  - Parameters:
    - `request`: Grid request with sorting, paging, and filter criteria.
    - `cancellationToken`: Cancellation token for async operations.
  - Returns: Result containing GridResponse or error details.

### Guard

- **Namespace:** `SmartWorkz.Shared.Guard`
- **Summary:** Static guard clauses for argument validation at method entry points.
             Throw immediately on invalid input — fail fast, fail loudly.
            
             Usage:
               Guard.NotNull(userId, nameof(userId));
               Guard.NotEmpty(name, nameof(name));
               Guard.InRange(pageSize, 1, 100, nameof(pageSize));
            
             These replace the ValidationExtensions.EnsureNotNull() extension method
             and the scattered ArgumentNullException throws throughout the codebase.

#### Methods & Properties

- **NotNull``1** - Throws ArgumentNullException if value is null.
- **NotNull``1** - Throws ArgumentNullException if value is null (struct/nullable).
- **NotEmpty** - Throws ArgumentException if string is null, empty, or whitespace.
- **NotEmpty``1** - Throws ArgumentException if collection is null or has no elements.
- **NotDefault``1** - Throws ArgumentException if value equals the default for its type (0, null, Guid.Empty).
- **InRange``1** - Throws ArgumentOutOfRangeException if value is outside [min, max].
- **Requires** - Throws ArgumentException if condition is false.

### EncryptionHelper

- **Namespace:** `SmartWorkz.Shared.EncryptionHelper`
- **Summary:** Cryptographic utilities for hashing and encryption.
            Uses PBKDF2 for password hashing and AES-256 for data encryption.

#### Methods & Properties

- **HashPassword** - Hash password using PBKDF2 with SHA256.
- **VerifyPassword** - Verify password against hash.
- **Encrypt** - Encrypt text using AES-256-GCM with provided key.
- **Decrypt** - Decrypt text using AES-256-GCM with provided key.
- **GenerateRandomString** - Generate cryptographically secure random string.
- **GenerateEncryptionKey** - Generate random encryption key (Base64 encoded).
- **ComputeSha256** - Compute SHA256 hash of text for integrity checking.

### JsonHelper

- **Namespace:** `SmartWorkz.Shared.JsonHelper`
- **Summary:** JSON serialization utilities using System.Text.Json.
            Provides consistent serialization options across the application.

#### Methods & Properties

- **Serialize``1** - Serialize object to JSON string.
- **Serialize** - Serialize object to JSON string with dynamic type.
- **Deserialize``1** - Deserialize JSON string to object.
- **Deserialize** - Deserialize JSON string to object with dynamic type.
- **DeserializeAsync``1** - Deserialize JSON asynchronously from stream.
- **SerializeAsync``1** - Serialize asynchronously to stream.
- **IsValidJson** - Check if string is valid JSON.
- **GetValueByPath** - Parse JSON and extract value at specified path (dot notation).

### IHttpClient

- **Namespace:** `SmartWorkz.Shared.IHttpClient`
- **Summary:** Abstraction for HTTP client operations with support for async/await and cancellation.
            Implementations should handle retries, timeouts, and error responses gracefully.

#### Methods & Properties

- **GetAsync``1** - Sends a GET request and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **PostAsync``1** - Sends a POST request with JSON body and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **PutAsync``1** - Sends a PUT request with JSON body and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **DeleteAsync``1** - Sends a DELETE request and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **GetAsync** - Sends a GET request and returns a string response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.
- **PostAsync** - Sends a POST request with JSON body and returns a string response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.

### RetryStrategy

- **Namespace:** `SmartWorkz.Shared.RetryStrategy`
- **Summary:** Specifies the backoff strategy to use when retrying failed HTTP requests.

### RetryPolicy

- **Namespace:** `SmartWorkz.Shared.RetryPolicy`
- **Summary:** Configures automatic retry behavior for failed HTTP requests.

### AuditRecord

- **Namespace:** `SmartWorkz.Shared.AuditRecord`
- **Summary:** Represents an immutable audit record with all relevant audit information.

#### Methods & Properties

- **#ctor** - Represents an immutable audit record with all relevant audit information.
  - Parameters:
    - `Id`: The unique identifier of the audit record
    - `EntityType`: The type of entity being audited (e.g., "User", "BlogPost")
    - `EntityId`: The identifier of the audited entity
    - `Action`: The action performed (Create, Update, Delete, etc.)
    - `UserId`: The identifier of the user who performed the action
    - `PerformedAt`: The timestamp when the action was performed
    - `Metadata`: Optional metadata dictionary containing additional context

### EnrichedLogger

- **Namespace:** `SmartWorkz.Shared.EnrichedLogger`
- **Summary:** Enriched logger wrapper around ILogger that provides structured logging methods
            for domain events, commands, sagas, file operations, and background jobs.
            Uses structured properties instead of string interpolation for better queryability.

#### Methods & Properties

- **#ctor** - Creates a new instance of EnrichedLogger.
  - Parameters:
    - `logger`: The underlying ILogger instance
- **LogCommandExecuted** - Logs command execution with duration and other metrics.
  - Parameters:
    - `commandType`: The type of command being executed
    - `duration`: How long the command took to execute
- **LogCommandExecutionError** - Logs a command execution error with exception details.
  - Parameters:
    - `commandType`: The type of command that failed
    - `exception`: The exception that occurred
- **LogCommandValidationError** - Logs a command with validation errors.
  - Parameters:
    - `commandType`: The type of command
    - `errors`: Dictionary of validation errors
- **LogEventPublished** - Logs an event publication with metadata.
  - Parameters:
    - `eventType`: The type of event being published
    - `eventId`: The unique identifier of the event
- **LogEventPublishedWithContext** - Logs an event with additional context properties.
  - Parameters:
    - `eventType`: The type of event
    - `eventId`: The event identifier
    - `context`: Additional context data
- **LogEventSubscribed** - Logs an event subscription.
  - Parameters:
    - `eventType`: The type of event being subscribed to
    - `subscriberType`: The subscriber type
- **LogSagaStarted** - Logs the start of a saga with its initial state.
  - Parameters:
    - `sagaId`: The unique saga identifier
    - `state`: The initial saga state
- **LogSagaStateTransition** - Logs a saga state transition.
  - Parameters:
    - `sagaId`: The saga identifier
    - `fromState`: The previous state
    - `toState`: The new state
- **LogSagaCompleted** - Logs the completion of a saga.
  - Parameters:
    - `sagaId`: The saga identifier
    - `duration`: How long the saga took to complete
- **LogSagaFailed** - Logs a saga failure.
  - Parameters:
    - `sagaId`: The saga identifier
    - `exception`: The exception that caused the failure
- **LogFileOperation** - Logs file operations such as upload, download, delete.
  - Parameters:
    - `operation`: The type of operation (Upload, Download, Delete, etc.)
    - `filePath`: The file path or URI
- **LogFileOperationWithSize** - Logs a file operation with size information.
  - Parameters:
    - `operation`: The type of operation
    - `filePath`: The file path
    - `sizeBytes`: The file size in bytes
- **LogFileOperationError** - Logs a file operation error.
  - Parameters:
    - `operation`: The operation that failed
    - `filePath`: The file path
    - `exception`: The exception that occurred
- **LogJobQueued** - Logs when a background job is queued.
  - Parameters:
    - `jobId`: The unique job identifier
    - `jobType`: The type of job being queued
- **LogJobStarted** - Logs when a background job starts processing.
  - Parameters:
    - `jobId`: The job identifier
    - `jobType`: The job type
- **LogJobCompleted** - Logs successful job completion.
  - Parameters:
    - `jobId`: The job identifier
    - `duration`: How long the job took to complete
- **LogJobFailed** - Logs a job failure.
  - Parameters:
    - `jobId`: The job identifier
    - `exception`: The exception that caused the failure
- **LogJobRetry** - Logs job retry attempt.
  - Parameters:
    - `jobId`: The job identifier
    - `attemptNumber`: The current attempt number
    - `maxRetries`: The maximum number of retries
- **LogWithContext** - Logs a message with structured context properties.
  - Parameters:
    - `operationName`: The name of the operation
    - `context`: Dictionary of contextual properties
- **LogPerformanceMetrics** - Logs performance metrics for an operation.
  - Parameters:
    - `operationName`: The operation name
    - `duration`: The operation duration
    - `resultStatus`: The result status (Success, Failure, etc.)
- **LogCorrelation** - Logs a correlation ID for request tracing.
  - Parameters:
    - `correlationId`: The correlation identifier
    - `userId`: Optional user identifier
    - `requestPath`: Optional request path
- **LogUnhandledException** - Logs unhandled exceptions as critical errors.
  - Parameters:
    - `exception`: The exception that occurred
    - `operationName`: The operation that failed

### IAuditLogger

- **Namespace:** `SmartWorkz.Shared.IAuditLogger`
- **Summary:** Interface for structured audit logging with metadata support.

#### Methods & Properties

- **LogAuditAsync** - Logs an audit event with structured metadata.
  - Parameters:
    - `entityType`: The entity type being audited (e.g., "User", "BlogPost")
    - `entityId`: The unique identifier of the entity
    - `action`: The action performed (Create, Update, Delete, etc.)
    - `metadata`: Optional metadata dictionary for additional context
    - `cancellationToken`: Cancellation token
  - Returns: Result indicating success or failure
- **GetAuditHistoryAsync** - Retrieves audit logs for a specific entity.
  - Parameters:
    - `entityType`: The entity type
    - `entityId`: The entity identifier
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of audit records
- **GetUserActivityAsync** - Retrieves audit logs for a specific user across all entities.
  - Parameters:
    - `userId`: The user identifier
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of audit records

### ILogger

- **Namespace:** `SmartWorkz.Shared.ILogger`
- **Summary:** Abstraction for application logging.
            Decouples from specific logging frameworks (Serilog, NLog, etc.).

### LogLevel

- **Namespace:** `SmartWorkz.Shared.LogLevel`
- **Summary:** Log level severity.

### ILoggerFactory

- **Namespace:** `SmartWorkz.Shared.ILoggerFactory`
- **Summary:** Factory for creating logger instances by category/source.

### IMapper

- **Namespace:** `SmartWorkz.Shared.IMapper`
- **Summary:** Mapping service abstraction for transforming objects between types.
            Supports registration of mapping profiles and bidirectional conversions.

#### Methods & Properties

- **Map``2** - Map source object to target type.
- **Map** - Map source object to target type using dynamic type.
- **MapAsync``2** - Map asynchronously with potential async operations in profile.
- **MapCollection``2** - Map collection of sources to targets.
- **MapCollectionAsync``2** - Map collection asynchronously.
- **RegisterProfile``2** - Register a mapping profile.

### IMapperProfile

- **Namespace:** `SmartWorkz.Shared.IMapperProfile`
- **Summary:** Profile for defining mapping rules between types.
            Implemented by concrete profiles that configure source-to-target transformations.

### IMapperProfile`2

- **Namespace:** `SmartWorkz.Shared.IMapperProfile`2`
- **Summary:** Typed mapper profile for strong typing.

#### Methods & Properties

- **Map** - Transform source to target synchronously.
- **MapAsync** - Transform source to target asynchronously.

### SimpleMapper

- **Namespace:** `SmartWorkz.Shared.SimpleMapper`
- **Summary:** A simple in-memory mapper that supports registering and executing mapping profiles.

### IMetricsCollector

- **Namespace:** `SmartWorkz.Shared.IMetricsCollector`
- **Summary:** Abstraction for collecting application metrics and performance data.
            Enables tracking of operation duration, throughput, error rates, and custom metrics.
            Implementations integrate with OpenTelemetry for export to Prometheus/Grafana.

#### Methods & Properties

- **RecordOperationDuration** - Record operation duration in milliseconds.
  - Parameters:
    - `operationName`: Name of the operation being measured.
    - `durationMs`: Duration in milliseconds.
    - `status`: Optional status (e.g., "success", "error").
    - `tags`: Optional metadata tags for grouping and filtering.
- **RecordOperationCount** - Record operation count (increments counter).
  - Parameters:
    - `operationName`: Name of the operation.
    - `count`: Number to increment by (default 1).
    - `status`: Optional status label.
    - `tags`: Optional metadata tags.
- **RecordGaugeValue** - Record a gauge value (e.g., queue depth, memory usage).
  - Parameters:
    - `metricName`: Name of the gauge metric.
    - `value`: The gauge value to record.
    - `tags`: Optional metadata tags.
- **RecordError** - Record error/exception occurrence.
  - Parameters:
    - `operationName`: Name of the operation that failed.
    - `ex`: The exception that occurred.
    - `tags`: Optional metadata tags.
- **IncrementCounter** - Increment a custom counter.
  - Parameters:
    - `counterName`: Name of the counter.
    - `increment`: Amount to increment (default 1).
    - `tags`: Optional metadata tags.

### MetricsMiddleware

- **Namespace:** `SmartWorkz.Shared.MetricsMiddleware`
- **Summary:** ASP.NET Core middleware for automatic HTTP request/response metrics collection.
             Records operation duration, status, and errors for all HTTP requests.
            
             Usage:
                 app.UseMiddleware<MetricsMiddleware>();

### MetricsStartupExtensions

- **Namespace:** `SmartWorkz.Shared.MetricsStartupExtensions`
- **Summary:** Extension methods for registering application metrics in dependency injection.

#### Methods & Properties

- **AddApplicationMetrics** - Registers IMetricsCollector with OpenTelemetry implementation.
  - Parameters:
    - `services`: The service collection to register with.
  - Returns: The service collection for method chaining.

### OpenTelemetryMetricsCollector

- **Namespace:** `SmartWorkz.Shared.OpenTelemetryMetricsCollector`
- **Summary:** OpenTelemetry-based implementation of IMetricsCollector.
            Collects metrics using System.Diagnostics.Metrics for export to Prometheus/Grafana.

### DefaultTenantFeatureFlags

- **Namespace:** `SmartWorkz.Shared.DefaultTenantFeatureFlags`
- **Summary:** In-memory feature flag provider for tenant-scoped feature control.
            
             Uses ConcurrentDictionary to store tenant-specific flags:
             - Key: tenant ID
             - Value: HashSet of enabled feature flag names
            
             Thread-safe for concurrent operations. Suitable for in-process caching
             or dev/test scenarios. For distributed systems, integrate with a
             centralized feature flag service (Unleash, LaunchDarkly, etc.).

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature is enabled for the specified tenant.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").
    - `cancellationToken`: Cancellation token.
  - Returns: Task containing true if the feature is enabled for this tenant,
            false if the tenant doesn't exist or the flag is not enabled.
- **GetEnabledFeaturesAsync** - Gets all enabled features for a tenant.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Task containing a read-only list of enabled feature flag names.
            Returns an empty list if the tenant doesn't exist or has no enabled flags.
- **EnableFlag** - Enables a feature flag for a tenant.
            Idempotent — calling multiple times with the same flag is safe.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name.
- **DisableFlag** - Disables a feature flag for a tenant.
            Idempotent — calling multiple times with the same flag is safe.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name.

### ITenantContext

- **Namespace:** `SmartWorkz.Shared.ITenantContext`
- **Summary:** Scoped service providing current tenant ID for multi-tenant applications.
            Resolved from request context or claims principal.

#### Methods & Properties

- **GetTenantId** - Gets the current tenant identifier.
  - Returns: Tenant ID, or null if operating in single-tenant context.
- **SetTenantId** - Sets the current tenant identifier (rarely used; typically set from request context).
  - Parameters:
    - `tenantId`: Tenant ID to set.

### ITenantFeatureFlags

- **Namespace:** `SmartWorkz.Shared.ITenantFeatureFlags`
- **Summary:** Feature flag provider scoped to a specific tenant.
            Allows per-tenant feature control.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature is enabled for the specified tenant.
  - Parameters:
    - `tenantId`: Tenant ID.
    - `flagName`: Feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").
    - `cancellationToken`: Cancellation token.
  - Returns: True if feature is enabled for this tenant.
- **GetEnabledFeaturesAsync** - Gets all enabled features for a tenant.
  - Parameters:
    - `tenantId`: Tenant ID.
    - `cancellationToken`: Cancellation token.
  - Returns: List of enabled feature flag names.

### TenantContext

- **Namespace:** `SmartWorkz.Shared.TenantContext`
- **Summary:** Scoped tenant context using AsyncLocal for proper isolation across async boundaries.
            
             AsyncLocal ensures:
             - Thread-safe storage per async execution context
             - Isolation between concurrent requests (each gets its own context)
             - Proper inheritance to child tasks (when awaited)
            
             Survives async/await boundaries unlike ThreadLocal, making it suitable for async methods.

#### Methods & Properties

- **GetTenantId** - Gets the current tenant identifier.
  - Returns: The current tenant ID, or "default" if not set.
- **SetTenantId** - Sets the current tenant identifier.
  - Parameters:
    - `tenantId`: The tenant ID to set. Cannot be null or empty.

### FirebaseCloudMessagingService

- **Namespace:** `SmartWorkz.Shared.FirebaseCloudMessagingService`
- **Summary:** Firebase Cloud Messaging service implementation for sending push notifications.
            Supports single/batch user notifications, topic-based broadcasting, and multi-platform delivery (Android, iOS, Web).

#### Methods & Properties

- **#ctor** - Initializes a new instance of the FirebaseCloudMessagingService.
  - Parameters:
    - `logger`: Logger for diagnostic and error information.
- **SendAsync** - Sends a simple push notification to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends simple push notifications to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase device tokens).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends a rich push notification with metadata to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends a rich push notification to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase device tokens).
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SendToTopicAsync** - Sends a rich push notification to all users subscribed to a topic (broadcast).
  - Parameters:
    - `topic`: Topic name (e.g., "news", "promotions").
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SubscribeToTopicAsync** - Subscribes a user to a topic for broadcast notifications.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `topic`: Topic name to subscribe to.
    - `cancellationToken`: Cancellation token.
- **UnsubscribeFromTopicAsync** - Unsubscribes a user from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `topic`: Topic name to unsubscribe from.
    - `cancellationToken`: Cancellation token.

### IPushNotificationService

- **Namespace:** `SmartWorkz.Shared.IPushNotificationService`
- **Summary:** Service for sending push notifications using Firebase Cloud Messaging.

#### Methods & Properties

- **SendAsync** - Sends a simple push notification to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a simple push notification to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase tokens).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a rich push notification with metadata to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a rich push notification with metadata to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase tokens).
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendToTopicAsync** - Sends a push notification to all users subscribed to a topic.
  - Parameters:
    - `topic`: The topic name.
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SubscribeToTopicAsync** - Subscribes a user to receive notifications from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `topic`: The topic name.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous subscription operation.
- **UnsubscribeFromTopicAsync** - Unsubscribes a user from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `topic`: The topic name.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous unsubscription operation.

### PushNotificationPayload

- **Namespace:** `SmartWorkz.Shared.PushNotificationPayload`
- **Summary:** Represents the payload data for a push notification.

### PushNotificationAction

- **Namespace:** `SmartWorkz.Shared.PushNotificationAction`
- **Summary:** Represents an action that can be performed from a push notification.

### PagedList`1

- **Namespace:** `SmartWorkz.Shared.PagedList`1`
- **Summary:** A page of items with metadata.
             Replaces PaginationResponse<T> in StarterKitMVC.Shared.DTOs.
            
             Migration path: PaginationResponse<T> has the same fields under different names.
             PagedList<T>.Create() is a drop-in replacement for PaginationResponse<T>.Create().

#### Methods & Properties

- **Empty** - Create an empty result set (e.g., when no rows match).
- **Map``1** - Project items to a different type without changing pagination metadata.

### PagedQuery

- **Namespace:** `SmartWorkz.Shared.PagedQuery`
- **Summary:** Standard request parameters for any paginated query.
             Replaces the existing PaginationRequest record in StarterKitMVC.Shared.DTOs.
            
             Migration: PaginationRequest is structurally identical — alias it or replace it.

#### Methods & Properties

- **#ctor** - Standard request parameters for any paginated query.
             Replaces the existing PaginationRequest record in StarterKitMVC.Shared.DTOs.
            
             Migration: PaginationRequest is structurally identical — alias it or replace it.
- **Normalize** - Clamp page and pageSize to safe bounds.

### IEntity`1

- **Namespace:** `SmartWorkz.Shared.IEntity`1`
- **Summary:** Marks a class as a domain entity with a typed primary key.

### CircuitBreaker

- **Namespace:** `SmartWorkz.Shared.CircuitBreaker`
- **Summary:** A thread-safe implementation of the circuit breaker pattern for handling failing dependencies gracefully.
            
             The circuit breaker operates in three states:
             - Closed: Normal operation. Requests pass through. Failures are tracked.
             - Open: Failing. All requests are rejected immediately to prevent cascading failures.
             - HalfOpen: Testing recovery. Limited requests are allowed to test if the dependency has recovered.
            
             State transitions:
             - Closed → Open: When ConsecutiveFailures >= FailureThreshold
             - Open → HalfOpen: Automatically when (DateTime.UtcNow - LastFailureTime) >= TimeoutMilliseconds
             - HalfOpen → Closed: When SuccessCount >= SuccessThreshold
             - HalfOpen → Open: When RecordFailure() is called in HalfOpen state
             - Closed → Closed: When RecordSuccess() is called (resets failure counter)

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `options`: The circuit breaker configuration options.
- **RecordSuccess** - Records a successful operation and updates state transitions accordingly.
            In Closed state: resets the failure counter.
            In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
- **RecordFailure** - Records a failed operation and updates state transitions accordingly.
            In Closed state: increments failure counter and transitions to Open if threshold is met.
            In HalfOpen state: immediately transitions back to Open.
- **Reset** - Resets the circuit breaker to the Closed state, clearing all failure and success counters.

### CircuitBreakerOptions

- **Namespace:** `SmartWorkz.Shared.CircuitBreakerOptions`
- **Summary:** Configuration options for the circuit breaker.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if options are valid; otherwise false.

### CircuitBreakerState

- **Namespace:** `SmartWorkz.Shared.CircuitBreakerState`
- **Summary:** Defines the state of a circuit breaker in the state machine pattern.

### ICircuitBreaker

- **Namespace:** `SmartWorkz.Shared.ICircuitBreaker`
- **Summary:** Defines the contract for a circuit breaker that implements the state machine pattern
            to handle failing dependencies gracefully.

#### Methods & Properties

- **RecordSuccess** - Records a successful operation and updates state transitions accordingly.
            In Closed state: resets the failure counter.
            In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
- **RecordFailure** - Records a failed operation and updates state transitions accordingly.
            In Closed state: increments failure counter and transitions to Open if threshold is met.
            In HalfOpen state: immediately transitions back to Open.
- **Reset** - Resets the circuit breaker to the Closed state, clearing all failure and success counters.

### IRateLimiter

- **Namespace:** `SmartWorkz.Shared.IRateLimiter`
- **Summary:** Defines the contract for a thread-safe rate limiter.

#### Methods & Properties

- **TryAcquireAsync** - Attempts to acquire tokens for a request identified by the given identifier.
  - Parameters:
    - `identifier`: The identifier for rate limiting (e.g., IP address, user ID).
    - `cost`: The number of tokens to acquire (default: 1).
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A result containing true if tokens were acquired successfully; otherwise false.
            On failure, the Error will include retry-after information.
- **GetAvailableTokensAsync** - Gets the number of available tokens for a given identifier.
  - Parameters:
    - `identifier`: The identifier to check.
  - Returns: The number of available tokens.
- **ResetAsync** - Resets the rate limit state for a given identifier.
  - Parameters:
    - `identifier`: The identifier to reset.
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **ClearAsync** - Clears all rate limit state.
  - Parameters:
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.

### RateLimiter

- **Namespace:** `SmartWorkz.Shared.RateLimiter`
- **Summary:** Thread-safe token bucket rate limiter implementation.
            
             This class maintains a per-identifier token bucket that refills at a constant rate.
             Tokens are consumed when requests are made; if insufficient tokens exist, the request is denied.
            
             Thread-safe operations use ConcurrentDictionary and locks on individual buckets to ensure
             consistent state without global locking bottlenecks.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the RateLimiter class.
  - Parameters:
    - `options`: Configuration options for the rate limiter.
- **TryAcquireAsync** - Attempts to acquire tokens for a request identified by the given identifier.
  - Parameters:
    - `identifier`: The identifier for rate limiting (e.g., IP address, user ID).
    - `cost`: The number of tokens to acquire (default: 1).
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A result containing true if tokens were acquired successfully; otherwise false.
            On failure, the Error will include retry-after information.
- **GetAvailableTokensAsync** - Gets the number of available tokens for a given identifier.
  - Parameters:
    - `identifier`: The identifier to check.
  - Returns: The number of available tokens.
- **ResetAsync** - Resets the rate limit state for a given identifier.
  - Parameters:
    - `identifier`: The identifier to reset.
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **ClearAsync** - Clears all rate limit state.
  - Parameters:
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **TokenBucket.TryAcquire** - Tries to acquire the specified number of tokens.
- **TokenBucket.GetAvailableTokens** - Gets the current number of available tokens.
- **TokenBucket.GetRetryAfterMilliseconds** - Gets the number of milliseconds to wait before retrying.
- **TokenBucket.RefillTokens** - Refills the token bucket based on elapsed time.

### RateLimiterOptions

- **Namespace:** `SmartWorkz.Shared.RateLimiterOptions`
- **Summary:** Configuration options for the rate limiter.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if options are valid; otherwise false.

### RateLimiterStrategy

- **Namespace:** `SmartWorkz.Shared.RateLimiterStrategy`
- **Summary:** Specifies the strategy used by the rate limiter to control request flow.

### ApiError

- **Namespace:** `SmartWorkz.Shared.ApiError`
- **Summary:** Structured error representation for API responses.
            Provides code, message, and optional field-level error details.

#### Methods & Properties

- **FromError** - Create from core Error type.
- **FromValidationErrors** - Create from validation errors.
- **FromException** - Create from exception.

### ApiResponse

- **Namespace:** `SmartWorkz.Shared.ApiResponse`
- **Summary:** Generic API response envelope that wraps result data with metadata.
            Non-generic convenience version for non-data responses.

#### Methods & Properties

- **Ok** - Success response without data.
- **Fail** - Failure response with error details.
- **FromResult** - Create from core Result pattern.

### ApiResponse`1

- **Namespace:** `SmartWorkz.Shared.ApiResponse`1`
- **Summary:** Typed API response envelope with data payload.
            Includes optional pagination metadata for list responses.

#### Methods & Properties

- **Ok** - Success response with data.
- **OkPaginated** - Success response with paginated data.
- **Fail** - Failure response with error.

### ProblemDetailsResponse

- **Namespace:** `SmartWorkz.Shared.ProblemDetailsResponse`
- **Summary:** Implements RFC 7807 Problem Details for HTTP APIs standard response format.
            Provides a standardized way to represent error details in API responses.

#### Methods & Properties

- **ValidationError** - Factory method for 400 Bad Request error with validation details.
- **Unauthorized** - Factory method for 401 Unauthorized error.
- **Forbidden** - Factory method for 403 Forbidden error.
- **NotFound** - Factory method for 404 Not Found error.
- **Conflict** - Factory method for 409 Conflict error.
- **InternalServerError** - Factory method for 500 Internal Server Error.
- **Custom** - Factory method for custom problem details.

### Error

- **Namespace:** `SmartWorkz.Shared.Error`
- **Summary:** Represents a structured error with a machine-readable code and human-readable message.
            
             This is the canonical Error type. It replaces:
             - SmartWorkz.StarterKitMVC.Shared.Primitives.Error (record struct)
             - The ad-hoc string errors in Models.Result
            
             Code examples: "USER_NOT_FOUND", "VALIDATION.EMAIL_REQUIRED", "AUTH.INVALID_CREDENTIALS"
             MessageKey maps to localization resource keys for UI display.

### Result

- **Namespace:** `SmartWorkz.Shared.Result`
- **Summary:** Represents the outcome of an operation that does not return a value.
            
             This unifies:
             - SmartWorkz.StarterKitMVC.Shared.Models.Result (Succeeded + MessageKey + Errors[])
             - SmartWorkz.StarterKitMVC.Shared.Primitives.Result (IsSuccess + Error struct)
            
             Design choice — class over struct:
             1. Result<T> inherits from Result to reuse Succeeded/Errors without duplication.
                Structs cannot use inheritance this way.
             2. Services return Result from interface methods — class semantics (null check) are
                simpler than boxing/unboxing structs across interface boundaries.
             3. Errors[] supports field-level validation messages that ModelState.AddErrors() consumes.
                A single Error struct cannot carry multiple field errors.
            
             The Primitives.Result struct in StarterKitMVC.Shared remains valid for pure functions
             where you want zero-allocation returns. This class is for service layer contracts.

#### Methods & Properties

- **Fail** - Failure with a localization message key and optional field-level error strings.
- **Fail** - Failure from a structured Error (bridges the Primitives.Error pattern).
- **Ok``1** - Factory for a typed result. Use in services that return data.

### Result`1

- **Namespace:** `SmartWorkz.Shared.Result`1`
- **Summary:** Result with a typed payload. Data is only valid when Succeeded = true.
            
             Usage:
               Result<UserDto> result = await _userService.GetByIdAsync(id);
               if (!result.Succeeded) return RedirectToPage("Error");
               var user = result.Data!;

### ResultExtensions

- **Namespace:** `SmartWorkz.Shared.ResultExtensions`
- **Summary:** Functional helpers for chaining Result operations.
            Keeps service code flat — avoids nested if (!result.Succeeded) blocks.

#### Methods & Properties

- **Map``2** - Transform the Data value if the result succeeded.
- **BindAsync``2** - Chain a second operation that also returns Result.
- **OnSuccess``1** - Execute a side-effect action on success, then return the original result.
- **OnFailure``1** - Execute a side-effect action on failure, then return the original result.

### ISagaDefinition`1

- **Namespace:** `SmartWorkz.Shared.ISagaDefinition`1`
- **Summary:** Defines the blueprint for a saga orchestration.
            A saga is a pattern for managing distributed transactions and long-running processes
            by coordinating multiple steps with built-in compensation mechanisms.

#### Methods & Properties

- **DefineStep``1** - Defines a step in the saga that will be executed when a specific event type is received.
            Steps are executed sequentially in the order they were defined.
  - Parameters:
    - `handler`: The async handler function that processes the event and updates the saga state.
            Returns a StepResult indicating success or failure.
- **OnFailure** - Defines the failure handler that will be called if any step fails.
            Used for compensation logic and saga-level error handling.
  - Parameters:
    - `compensationHandler`: The async handler that receives the current saga state and the exception that occurred.
            Responsible for compensation/rollback logic.
- **BuildAsync** - Builds and returns the saga definition for execution.
            Can be used for async initialization or validation.
  - Returns: A task that completes with the configured saga definition.
- **GetSteps** - Gets the list of saga steps in execution order.
  - Returns: A read-only list of saga step handlers.
- **GetFailureHandler** - Gets the failure compensation handler if defined.
  - Returns: The failure handler function, or null if not defined.

### SagaOrchestrator

- **Namespace:** `SmartWorkz.Shared.SagaOrchestrator`
- **Summary:** Orchestrates the execution of sagas, managing step sequencing, error handling,
            and compensation/rollback logic for complex distributed processes.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the SagaOrchestrator class.
  - Parameters:
    - `logger`: Logger for saga execution tracking and debugging.
- **ExecuteSagaAsync``1** - Executes a saga definition with the provided initial state and triggering event.
            Manages step execution, error handling, and compensation logic.
  - Parameters:
    - `sagaDefinition`: The saga definition blueprint to execute.
    - `initialState`: The initial saga state.
    - `event`: The domain event triggering the saga.
    - `cancellationToken`: Optional cancellation token.
  - Returns: A task representing the saga execution.
- **ExecuteSagaStepsAsync``1** - Executes saga steps by using reflection to access internal step definitions.
- **CompensateExecutedStepsAsync``1** - Executes compensation handlers for all executed steps in reverse order.
            Uses stored compensation handlers to avoid re-executing steps.
- **ExecuteFailureHandlerAsync``1** - Executes the saga-level failure handler if one is defined.

### SagaStatus

- **Namespace:** `SmartWorkz.Shared.SagaStatus`
- **Summary:** Represents the status of a saga execution.

### SagaState

- **Namespace:** `SmartWorkz.Shared.SagaState`
- **Summary:** Base class for saga state objects.
            Provides common tracking properties for saga execution flow.

### StepResult

- **Namespace:** `SmartWorkz.Shared.StepResult`
- **Summary:** Represents the result of executing a single saga step.
            Provides success/failure status and optional compensation logic for rollback.

#### Methods & Properties

- **Success** - Creates a successful step result.
  - Returns: A StepResult indicating success.
- **Failure** - Creates a failed step result with an optional compensation handler.
  - Parameters:
    - `failureReason`: The reason for the step failure.
    - `compensationHandler`: Optional handler to compensate/rollback this step if a later step fails.
  - Returns: A StepResult indicating failure.
- **FromException** - Creates a failed step result for an exception with optional compensation.
  - Parameters:
    - `exception`: The exception that caused the failure.
    - `compensationHandler`: Optional compensation handler.
  - Returns: A StepResult indicating failure.

### CryptHelper

- **Namespace:** `SmartWorkz.Shared.CryptHelper`
- **Summary:** Provides AES-256-CBC encryption and decryption utilities with secure key and IV generation.
            
             All operations support both string and byte array inputs/outputs.
             Keys are normalized to 32 bytes (256 bits) via padding/trimming as needed.
             IVs are auto-generated if not provided and embedded in the ciphertext (IV:Ciphertext format).

#### Methods & Properties

- **EncryptString** - Encrypts plaintext using AES-256-CBC with a Base64-encoded output.
  - Parameters:
    - `plaintext`: The plaintext to encrypt.
    - `key`: The encryption key (will be normalized to 32 bytes).
    - `iv`: Optional IV; auto-generated if null.
  - Returns: A Result containing Base64-encoded ciphertext in "IV:Ciphertext" format or an error.
- **EncryptBytes** - Encrypts byte data using AES-256-CBC.
  - Parameters:
    - `plaintext`: The plaintext bytes to encrypt.
    - `key`: The encryption key (will be normalized to 32 bytes).
    - `iv`: Optional IV; auto-generated if null.
  - Returns: A Result containing encrypted bytes with embedded IV (IV || Ciphertext) or an error.
- **DecryptString** - Decrypts Base64-encoded ciphertext using AES-256-CBC.
  - Parameters:
    - `ciphertext`: The Base64-encoded ciphertext in "IV:Ciphertext" format.
    - `key`: The decryption key (will be normalized to 32 bytes).
  - Returns: A Result containing the decrypted plaintext or an error.
- **DecryptBytes** - Decrypts byte data using AES-256-CBC.
  - Parameters:
    - `ciphertext`: The encrypted bytes with embedded IV (IV || Ciphertext).
    - `key`: The decryption key (will be normalized to 32 bytes).
  - Returns: A Result containing decrypted bytes or an error.
- **GenerateKey** - Generates a random cryptographic key of the specified size.
  - Parameters:
    - `keySize`: The key size in bytes (default 32 for AES-256). Must be 16, 24, or 32.
  - Returns: A Result containing Base64-encoded random key or an error.
- **GenerateIv** - Generates a random cryptographic IV (Initialization Vector).
  - Returns: A Result containing Base64-encoded random IV or an error.
- **GenerateRandomBytes** - Generates cryptographically secure random bytes.
- **NormalizeKey** - Normalizes a key to exactly 32 bytes (256 bits).
            If the key is shorter, it's padded with zeros. If longer, it's trimmed.

### CryptOptions

- **Namespace:** `SmartWorkz.Shared.CryptOptions`
- **Summary:** Configuration options for AES cryptographic operations.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if valid, false otherwise.

### HashHelper

- **Namespace:** `SmartWorkz.Shared.HashHelper`
- **Summary:** Provides utilities for cryptographic hash operations (SHA256 and MD5).

#### Methods & Properties

- **Sha256** - Computes the SHA256 hash of a string and returns it as a hexadecimal string.
- **Sha256Bytes** - Computes the SHA256 hash of a byte array and returns the hash as a byte array.
- **Md5** - Computes the MD5 hash of a string and returns it as a hexadecimal string.
            Note: MD5 is cryptographically broken; use SHA256 for security-critical applications.
- **VerifyHash** - Verifies that a text matches its SHA256 hash.

### HmacAlgorithm

- **Namespace:** `SmartWorkz.Shared.HmacAlgorithm`
- **Summary:** Specifies the HMAC algorithm to use for message signing and verification.

### HmacHelper

- **Namespace:** `SmartWorkz.Shared.HmacHelper`
- **Summary:** Provides HMAC-SHA256/SHA512 message signing and verification for API requests and webhook verification.
            Implements constant-time comparison to prevent timing attacks.

#### Methods & Properties

- **Sign** - Signs a message using HMAC with the specified algorithm and returns a Base64-encoded hex digest.
  - Parameters:
    - `message`: The message to sign.
    - `secret`: The secret key for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing the Base64-encoded signature or an error.
- **SignBytes** - Signs a message using HMAC with the specified algorithm and returns the raw byte digest.
  - Parameters:
    - `message`: The message bytes to sign.
    - `secret`: The secret key for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing the byte signature or an error.
- **Verify** - Verifies a message signature using HMAC with constant-time comparison to prevent timing attacks.
  - Parameters:
    - `message`: The original message that was signed.
    - `signature`: The Base64-encoded signature to verify.
    - `secret`: The secret key used for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing true if the signature is valid, false if not, or an error.
- **VerifyBytes** - Verifies a message signature using HMAC with raw byte inputs and constant-time comparison.
  - Parameters:
    - `message`: The original message bytes that were signed.
    - `signature`: The byte signature to verify.
    - `secret`: The secret key used for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing true if the signature is valid, false if not, or an error.
- **SignBytes** - Internal method to compute HMAC signature from raw bytes.
- **CreateHmac** - Creates the appropriate HMAC instance based on the algorithm.

### InputSanitizer

- **Namespace:** `SmartWorkz.Shared.InputSanitizer`
- **Summary:** Input sanitization to prevent XSS, SQL injection, and path traversal attacks.

#### Methods & Properties

- **SanitizeHtml** - Sanitize HTML by removing dangerous tags and attributes.
- **EscapeHtml** - Escape HTML special characters to prevent XSS.
- **SanitizeSql** - Sanitize string to prevent SQL injection (basic, not a replacement for parameterized queries).
- **SanitizeFilePath** - Sanitize file path to prevent directory traversal attacks.
- **SanitizeUrl** - Sanitize and validate URL.
- **EscapeJson** - Escape string for safe JSON inclusion.
- **IsValidEmail** - Validate email format (basic check, server-side SMTP validation recommended).
- **RemoveControlCharacters** - Remove null bytes and control characters.

### JwtSettings

- **Namespace:** `SmartWorkz.Shared.JwtSettings`
- **Summary:** Settings for JWT token generation and validation.

#### Methods & Properties

- **Validate** - Validate settings: Secret >= 32 chars, other fields non-empty.

### JwtClaims

- **Namespace:** `SmartWorkz.Shared.JwtClaims`
- **Summary:** JWT claims that can be included in a token.

#### Methods & Properties

- **GetClaimValue** - Get claim value by type (supports standard claims + custom).

### JwtTokenValidationResult

- **Namespace:** `SmartWorkz.Shared.JwtTokenValidationResult`
- **Summary:** Result of JWT token validation.

### JwtHelper

- **Namespace:** `SmartWorkz.Shared.JwtHelper`
- **Summary:** Provides JWT token generation, validation, and refresh functionality.

#### Methods & Properties

- **GenerateTokenInternal** - Internal token generation logic shared by GenerateToken and GenerateRefreshToken.
  - Parameters:
    - `claims`: The claims to include in the token.
    - `settings`: The JWT settings for signing and configuration.
    - `isRefreshToken`: If true, uses RefreshTokenExpiryDays; otherwise uses ExpiryMinutes.
  - Returns: A Result containing the signed token or an error.
- **GenerateToken** - Generates a JWT access token with the specified claims and settings.
  - Parameters:
    - `claims`: The claims to include in the token.
    - `settings`: The JWT settings for signing and configuration.
  - Returns: A Result containing the signed token or an error.
- **ValidateToken** - Validates a JWT token and extracts claims if valid.
  - Parameters:
    - `token`: The token to validate.
    - `settings`: The JWT settings for validation.
  - Returns: A Result containing the validation result.
- **RefreshToken** - Refreshes an access token using a refresh token.
  - Parameters:
    - `refreshToken`: The refresh token.
    - `settings`: The JWT settings.
  - Returns: A Result containing the new access token or an error.
- **GenerateRefreshToken** - Generates a refresh token with extended expiry.
  - Parameters:
    - `claims`: The claims to include in the refresh token.
    - `settings`: The JWT settings.
  - Returns: A Result containing the refresh token or an error.
- **ToBase64Url** - Encodes bytes to Base64Url format (no padding, + → -, / → _).
- **FromBase64Url** - Decodes Base64Url format to bytes.

### PasswordHelper

- **Namespace:** `SmartWorkz.Shared.PasswordHelper`
- **Summary:** Provides secure password generation and validation using cryptographically secure random number generation.

#### Methods & Properties

- **GeneratePassword** - Generates a cryptographically secure random password.
  - Parameters:
    - `length`: Length of the password (8-128, default 12).
    - `includeSpecialChars`: Whether to include special characters.
  - Returns: A Result containing the generated password or an error.
- **ValidateStrength** - Validates the strength of a password against a policy.
  - Parameters:
    - `password`: The password to validate.
    - `policy`: The policy to validate against (uses default if null).
  - Returns: A Result containing the validation result.
- **GetRandomChar** - Gets a random character from the specified character set using cryptographic randomness.
- **Shuffle** - Performs Fisher-Yates shuffle on the character array.
- **CheckPasswordLength** - Checks if password meets minimum length requirement.
- **CheckUppercase** - Checks if password contains at least one uppercase letter.
- **CheckLowercase** - Checks if password contains at least one lowercase letter.
- **CheckNumbers** - Checks if password contains at least one digit.
- **CheckSpecialChars** - Checks if password contains at least one special character.

### PasswordPolicy

- **Namespace:** `SmartWorkz.Shared.PasswordPolicy`
- **Summary:** Policy for password validation requirements.

#### Methods & Properties

- **Validate** - Validates the policy invariants.

### PasswordValidationResult

- **Namespace:** `SmartWorkz.Shared.PasswordValidationResult`
- **Summary:** Result of password validation against a policy.

### ITemplateEngine

- **Namespace:** `SmartWorkz.Shared.ITemplateEngine`
- **Summary:** Defines operations for rendering templates with placeholder substitution.

#### Methods & Properties

- **Render** - Renders a template string by replacing placeholders with values from a dictionary.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `values`: Dictionary of key-value pairs to substitute into the template.
  - Returns: The rendered content with placeholders replaced. Unmatched placeholders remain unchanged.
- **Render** - Renders a template string by extracting properties from an object model.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `model`: The model object whose public properties are used for placeholder substitution.
  - Returns: The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.
- **RenderFileAsync** - Asynchronously renders a template file by replacing placeholders with values from a dictionary.
  - Parameters:
    - `filePath`: The path to the template file.
    - `values`: Dictionary of key-value pairs to substitute into the template.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **RenderFileAsync** - Asynchronously renders a template file by extracting properties from an object model.
  - Parameters:
    - `filePath`: The path to the template file.
    - `model`: The model object whose public properties are used for placeholder substitution.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **LoadDirectoryAsync** - Asynchronously loads all template files from a directory into a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.
- **RenderDirectoryAsync** - Asynchronously loads and renders all template files from a directory using values from a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `values`: Dictionary of key-value pairs to substitute into each template.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.

### TemplateEngine

- **Namespace:** `SmartWorkz.Shared.TemplateEngine`
- **Summary:** Provides template rendering services with support for placeholder substitution.

#### Methods & Properties

- **PlaceholderRegex** - 
- **Render** - Renders a template string by replacing placeholders with values from a dictionary.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `values`: Dictionary of key-value pairs to substitute into the template.
  - Returns: The rendered content with placeholders replaced. Unmatched placeholders and null/empty content remain unchanged.
- **Render** - Renders a template string by extracting properties from an object model.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `model`: The model object whose public properties are used for placeholder substitution.
  - Returns: The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.
- **RenderFileAsync** - Asynchronously renders a template file by replacing placeholders with values from a dictionary.
  - Parameters:
    - `filePath`: The path to the template file.
    - `values`: Dictionary of key-value pairs to substitute into the template.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **RenderFileAsync** - Asynchronously renders a template file by extracting properties from an object model.
  - Parameters:
    - `filePath`: The path to the template file.
    - `model`: The model object whose public properties are used for placeholder substitution.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **LoadDirectoryAsync** - Asynchronously loads all template files from a directory into a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.
- **RenderDirectoryAsync** - Asynchronously loads and renders all template files from a directory using values from a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `values`: Dictionary of key-value pairs to substitute into each template.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.
- **ReflectModel** - Reflects over a model object and builds a case-insensitive dictionary of public properties
            mapped to their string values. Uses cached property metadata for performance.
- **ValidateFilePath** - Validates a file path to prevent directory traversal attacks.
  - Parameters:
    - `filePath`: The file path to validate.
  - Returns: A result indicating if the path is valid and safe.

### CompressHelper

- **Namespace:** `SmartWorkz.Shared.CompressHelper`
- **Summary:** Provides utilities for GZip compression and decompression.

#### Methods & Properties

- **CompressString** - Compresses a string using GZip compression.
- **DecompressString** - Decompresses a GZip-compressed byte array back to a string.
- **CompressBytes** - Compresses a byte array using GZip compression.
- **DecompressBytes** - Decompresses a GZip-compressed byte array.

### DateHelper

- **Namespace:** `SmartWorkz.Shared.DateHelper`
- **Summary:** Provides utilities for date and time operations.

#### Methods & Properties

- **GetAge** - Calculates the age in years from a birth date to today.
- **GetRelativeTime** - Returns a human-readable relative time string (e.g., "2 days ago", "in 3 hours").
- **StartOfDay** - Returns the start of the day (00:00:00) for the given date.
- **EndOfDay** - Returns the end of the day (23:59:59.999) for the given date.
- **IsWeekend** - Determines if the given date falls on a weekend (Saturday or Sunday).
- **GetDayOfWeekName** - Returns the name of the day of week (e.g., "Monday", "Tuesday").
- **DaysBetween** - Calculates the number of days between two dates (inclusive of the from date, exclusive of the to date).

### EnumHelper

- **Namespace:** `SmartWorkz.Shared.EnumHelper`
- **Summary:** Provides utilities for enum operations including reflection and description retrieval.

#### Methods & Properties

- **GetDescription** - Gets the description of an enum value from its [Description] attribute.
            Falls back to the enum name if no description is found.
- **GetValue``1** - Attempts to get an enum value by its name.
- **GetAllValues``1** - Returns all values of the specified enum type as a list.
- **GetName** - Gets the name of an enum value.

### MathHelper

- **Namespace:** `SmartWorkz.Shared.MathHelper`
- **Summary:** Provides utilities for common math operations.

#### Methods & Properties

- **Percentage** - Calculates the percentage of a value.
            Example: Percentage(100, 20) returns 20 (20% of 100).
- **PercentageChange** - Calculates the percentage change from oldValue to newValue.
            Positive result indicates increase, negative indicates decrease.
- **RoundTo** - Rounds a decimal value to the specified number of decimal places.
- **Clamp``1** - Clamps a value within a specified range [min, max].
- **Average** - Calculates the average of the provided decimal values.

### SlugHelper

- **Namespace:** `SmartWorkz.Shared.SlugHelper`
- **Summary:** Helper for generating URL-friendly slugs from text input.

#### Methods & Properties

- **GenerateSlug** - Generates a URL-friendly slug from the given text with optional configuration.
  - Parameters:
    - `text`: The input text to convert to a slug.
    - `options`: Configuration options. If null, default options are used.
  - Returns: A Result containing the generated slug or an error.
- **ToSlug** - Generates a URL-friendly slug from the given text using default options.
            Convenience method equivalent to GenerateSlug(text, null).
  - Parameters:
    - `text`: The input text to convert to a slug.
  - Returns: A Result containing the generated slug or an error.
- **RemoveAccents** - Removes accented characters from text by decomposing them and filtering out combining marks.
            For example: "café" → "cafe", "naïve" → "naive", "Señor" → "Senor".
  - Parameters:
    - `input`: The input text potentially containing accented characters.
  - Returns: The text with accented characters converted to their base forms.
- **ReplaceSpecialCharacters** - Replaces special characters and spaces with the specified separator.
            Keeps only alphanumeric characters and the separator.
  - Parameters:
    - `input`: The input text.
    - `separator`: The separator to use for special characters and spaces.
  - Returns: The text with special characters replaced by the separator.

### SlugOptions

- **Namespace:** `SmartWorkz.Shared.SlugOptions`
- **Summary:** Options for configuring slug generation behavior in .

### TextHelper

- **Namespace:** `SmartWorkz.Shared.TextHelper`
- **Summary:** Sealed class providing advanced text processing and formatting utilities.
            All methods return Result<string> for consistent error handling.

#### Methods & Properties

- **Truncate** - Truncates text to a maximum length and appends a suffix (default "...").
  - Parameters:
    - `text`: The input text to truncate.
    - `maxLength`: The maximum length including the suffix.
    - `suffix`: The suffix to append when truncating. Defaults to "...".
  - Returns: A Result containing the truncated text or an error.
- **Capitalize** - Capitalizes the first letter of the text while preserving the rest.
  - Parameters:
    - `text`: The input text to capitalize.
  - Returns: A Result containing the capitalized text or an error.
- **Decapitalize** - Decapitalizes the first letter of the text while preserving the rest.
  - Parameters:
    - `text`: The input text to decapitalize.
  - Returns: A Result containing the decapitalized text or an error.
- **StripHtml** - Removes HTML tags from the input string using regex.
  - Parameters:
    - `html`: The HTML string to process.
  - Returns: A Result containing the plain text with HTML tags removed or an error.
- **Pluralize** - Pluralizes a word based on count using a simple heuristic.
            If count == 1, returns singular form. Otherwise appends 's'.
  - Parameters:
    - `singular`: The singular form of the word.
    - `count`: The count to determine plural form.
  - Returns: A Result containing the appropriately pluralized word or an error.
- **TitleCase** - Converts text to title case by capitalizing the first letter of each word.
  - Parameters:
    - `text`: The input text to convert.
  - Returns: A Result containing the title-cased text or an error.
- **Reverse** - Reverses the input string.
  - Parameters:
    - `text`: The input text to reverse.
  - Returns: A Result containing the reversed text or an error.
- **RemoveWhitespace** - Removes all whitespace characters from the input string.
  - Parameters:
    - `text`: The input text to process.
  - Returns: A Result containing the text with all whitespace removed or an error.
- **WordWrap** - Wraps text at a specified line length while preserving word boundaries.
  - Parameters:
    - `text`: The input text to wrap.
    - `lineLength`: The maximum length of each line.
    - `newline`: The newline character(s) to use. Defaults to "\n".
  - Returns: A Result containing the word-wrapped text or an error.
- **Repeat** - Repeats the input string the specified number of times.
  - Parameters:
    - `text`: The input text to repeat.
    - `count`: The number of times to repeat the text.
  - Returns: A Result containing the repeated text or an error.

### CompositeValidator`1

- **Namespace:** `SmartWorkz.Shared.CompositeValidator`1`
- **Summary:** Combines multiple validators into a single validator.
            Useful for composing validators from different sources.

### IValidationRule`2

- **Namespace:** `SmartWorkz.Shared.IValidationRule`2`
- **Summary:** Single validation rule for a property.

#### Methods & Properties

- **ValidateAsync** - Validate property and return results.

### ValidationRule`2

- **Namespace:** `SmartWorkz.Shared.ValidationRule`2`
- **Summary:** Base implementation for custom validation rules.

### ValidationRules

- **Namespace:** `SmartWorkz.Shared.ValidationRules`
- **Summary:** Pre-built validation rules for common scenarios.

### ValidatorBuilder`1

- **Namespace:** `SmartWorkz.Shared.ValidatorBuilder`1`
- **Summary:** Fluent validator builder for defining validation rules.
            Provides an alternative to ValidatorBase for more concise validator definitions.

#### Methods & Properties

- **RuleFor``1** - Add a rule for a property using fluent API.
- **ValidateAsync** - Validate instance against all rules.

### IWebhookRegistry

- **Namespace:** `SmartWorkz.Shared.IWebhookRegistry`
- **Summary:** Abstraction for managing webhook subscriptions and registrations.
            Supports CRUD operations and subscription queries.

#### Methods & Properties

- **RegisterAsync** - Register a new webhook subscription.
  - Parameters:
    - `url`: The webhook endpoint URL.
    - `events`: Array of event names to subscribe to.
    - `secret`: Optional HMAC-SHA256 secret for signature verification.
    - `cancellationToken`: Cancellation token.
  - Returns: The ID of the newly registered subscription.
- **UnregisterAsync** - Unregister and remove a webhook subscription.
  - Parameters:
    - `subscriptionId`: The subscription ID to unregister.
    - `cancellationToken`: Cancellation token.
- **GetSubscriptionsForEventAsync** - Get all active subscriptions for a specific event.
  - Parameters:
    - `eventName`: The event name to filter by.
    - `cancellationToken`: Cancellation token.
  - Returns: Collection of subscriptions interested in this event.
- **GetActiveSubscriptionsAsync** - Get all currently active subscriptions.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: Collection of all active subscriptions.
- **UpdateSubscriptionStatusAsync** - Update the status and failure tracking of a subscription.
  - Parameters:
    - `subscriptionId`: The subscription ID to update.
    - `isActive`: Whether the subscription should remain active.
    - `failureCount`: Number of consecutive failures (null to leave unchanged).
    - `failureReason`: Reason for failure (null to clear).
    - `cancellationToken`: Cancellation token.

### SqlWebhookRegistry

- **Namespace:** `SmartWorkz.Shared.SqlWebhookRegistry`
- **Summary:** SQL Server implementation of IWebhookRegistry.
            Persists webhook subscriptions to the database with support for querying and status updates.

### WebhookDeliveryService

- **Namespace:** `SmartWorkz.Shared.WebhookDeliveryService`
- **Summary:** Service for publishing domain events to registered webhook endpoints.
            Implements exponential backoff retry logic, HMAC signature verification, and failure tracking.

#### Methods & Properties

- **PublishEventAsync** - Publish an event to all subscribed webhook endpoints.
  - Parameters:
    - `eventName`: The name of the event being published.
    - `payload`: The event payload to send.
    - `cancellationToken`: Cancellation token.
- **DeliverAsync** - Deliver an event to a single webhook endpoint with exponential backoff retry logic.
- **GenerateSignature** - Generate HMAC-SHA256 signature for webhook payload verification.

### AuditEntry

- **Namespace:** `SmartWorkz.Shared.AuditEntry`
- **Summary:** Immutable audit log entry for tracking entity changes and domain events.
            Records who did what, when, where, and why for compliance and debugging.

### AuditEventSubscriber

- **Namespace:** `SmartWorkz.Shared.AuditEventSubscriber`
- **Summary:** Subscribes to domain events and records them in the audit trail.
            Enables automatic audit capture without requiring explicit audit calls in business logic.

#### Methods & Properties

- **OnEventPublishedAsync** - Record a domain event in the audit trail.
  - Parameters:
    - `evt`: The domain event to record.
    - `userId`: User ID who triggered the event (optional for system events).
    - `ipAddress`: IP address of the request originator (optional).
    - `cancellationToken`: Cancellation token.

### AuditStartupExtensions

- **Namespace:** `SmartWorkz.Shared.AuditStartupExtensions`
- **Summary:** Dependency injection and schema setup for audit trail functionality.

#### Methods & Properties

- **AddAuditTrail** - Register IAuditTrail with SQL Server implementation.
- **CreateAuditTrailSchema** - Create the AuditTrail table and indexes if they don't exist.
            Call this during application startup or migration.

### IAuditTrail

- **Namespace:** `SmartWorkz.Shared.IAuditTrail`
- **Summary:** Service for recording and querying immutable audit entries.
            Abstracts the persistence mechanism for audit trails.

#### Methods & Properties

- **RecordAsync** - Record an audit entry (immutable append-only).
  - Parameters:
    - `entry`: The audit entry to record.
    - `cancellationToken`: Cancellation token.
- **GetEntriesAsync** - Get all audit entries for a specific entity instance.
  - Parameters:
    - `entityType`: Type of entity (e.g., "Order").
    - `entityId`: Entity instance ID.
    - `cancellationToken`: Cancellation token.
- **GetEntriesByActionAsync** - Get audit entries by action type (Created, Updated, Deleted, etc.).
  - Parameters:
    - `action`: The action to filter by.
    - `since`: Optional: only entries after this date.
    - `cancellationToken`: Cancellation token.
- **GetEntriesByUserAsync** - Get audit entries for a specific user.
  - Parameters:
    - `userId`: User ID who performed actions.
    - `since`: Optional: only entries after this date.
    - `cancellationToken`: Cancellation token.
- **SearchAsync** - Search audit trail with multiple filter criteria.
            All criteria are AND'd together (null criteria are ignored).
  - Parameters:
    - `entityType`: Optional entity type filter.
    - `action`: Optional action filter.
    - `userId`: Optional user ID filter.
    - `since`: Optional timestamp filter (inclusive).
    - `cancellationToken`: Cancellation token.

### SqlAuditTrail

- **Namespace:** `SmartWorkz.Shared.SqlAuditTrail`
- **Summary:** SQL Server implementation of IAuditTrail for immutable audit log persistence.
            Appends audit entries to a single table with indexes for efficient querying.

#### Methods & Properties

- **RecordAsync** - 
- **GetEntriesAsync** - 
- **GetEntriesByActionAsync** - 
- **GetEntriesByUserAsync** - 
- **SearchAsync** - 

### ValueConverter`1

- **Namespace:** `SmartWorkz.Shared.ValueConverter`1`
- **Summary:** Abstract base class for type conversion between domain objects and DTOs.
            Enables loose coupling between layers by centralizing conversion logic.

#### Methods & Properties

- **Convert``1** - Convert a single source object to target type.
- **Convert** - Convert a single source object using dynamic target type resolution.
- **ConvertList``1** - Convert a collection of source objects to target type.
- **ConvertList** - Convert a collection using dynamic target type resolution.
- **ConvertFromList``2** - Convert from a collection of different source types.

### CacheEntry`1

- **Namespace:** `SmartWorkz.Shared.CacheEntry`1`
- **Summary:** Represents a cached entry with data, expiration time, and metadata.

#### Methods & Properties

- **#ctor** - Creates a new CacheEntry instance.
- **#ctor** - Creates a new CacheEntry instance with data and expiration.
- **RenewExpiry** - Renews the expiry time based on the cache strategy and TTL.

### CacheEntryWrapper

- **Namespace:** `SmartWorkz.Shared.CacheEntryWrapper`
- **Summary:** Non-generic wrapper for CacheEntry to store in the cache dictionary.

### CacheOptions

- **Namespace:** `SmartWorkz.Shared.CacheOptions`
- **Summary:** Configuration options for cache operations.

#### Methods & Properties

- **#ctor** - Creates a new CacheOptions instance with default values.
- **#ctor** - Creates a new CacheOptions instance with specified TTL.
- **#ctor** - Creates a new CacheOptions instance with specified TTL and cache strategy.
- **#ctor** - Creates a new CacheOptions instance with all parameters.

### CacheStrategy

- **Namespace:** `SmartWorkz.Shared.CacheStrategy`
- **Summary:** Enumeration of cache expiration strategies.

### ICacheService

- **Namespace:** `SmartWorkz.Shared.ICacheService`
- **Summary:** Service for caching with tenant isolation and L1/L2 hybrid support.
            Implementations may use memory cache (L1) and distributed cache (L2).
            All cache operations are tenant-scoped with automatic key prefixing.

#### Methods & Properties

- **GetAsync``1** - Gets a cached value by key with tenant isolation.
  - Parameters:
    - `key`: Cache key (will be tenant-scoped automatically).
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Result containing cached value, or failure if not found or expired.
- **SetAsync``1** - Sets a value in cache with optional TTL for the specified tenant.
  - Parameters:
    - `key`: Cache key (will be tenant-scoped automatically).
    - `value`: Value to cache.
    - `ttlMinutes`: Time to live in minutes. If null, value never expires.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveAsync** - Removes a cached value by key for the specified tenant.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveByPrefixAsync** - Removes all cached values matching a key prefix for the specified tenant.
            Example: RemoveByPrefixAsync("user:") removes all "user:*" entries for that tenant.
  - Parameters:
    - `prefix`: Key prefix to match (may include wildcard suffix like "user:*").
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **ExistsAsync** - Checks if a key exists in cache for the specified tenant.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: True if key exists and is not expired; false otherwise.
- **ClearAsync** - Clears all cache entries for the specified tenant.
            Does not affect entries for other tenants.
  - Parameters:
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.

### ICacheStore

- **Namespace:** `SmartWorkz.Shared.ICacheStore`
- **Summary:** Abstraction for a cache store with support for various operations including TTL and expiration strategies.

#### Methods & Properties

- **GetAsync``1** - Retrieves a value from the cache.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result containing the cached value or null if not found or expired.
- **SetAsync``1** - Sets a value in the cache with optional TTL.
  - Parameters:
    - `key`: The cache key.
    - `value`: The value to cache.
    - `ttlMinutes`: Optional time-to-live in minutes. If null, uses default or no expiration.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **SetAsync``1** - Sets a value in the cache with cache options.
  - Parameters:
    - `key`: The cache key.
    - `value`: The value to cache.
    - `options`: Cache options including TTL, strategy, and sliding expiration.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **RemoveAsync** - Removes a value from the cache.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **RemoveByPrefixAsync** - Removes all values from the cache that match the specified key prefix.
  - Parameters:
    - `keyPrefix`: The prefix to match.
    - `ct`: Cancellation token.
  - Returns: A Result containing the number of entries removed.
- **ExistsAsync** - Checks if a key exists in the cache and is not expired.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result indicating whether the key exists and is valid.
- **ClearAsync** - Clears all entries from the cache.
  - Parameters:
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.

### MemoryCacheService

- **Namespace:** `SmartWorkz.Shared.MemoryCacheService`
- **Summary:** In-memory L1 cache service implementation with thread-safe operations and tenant isolation.
            Suitable for single-process deployments with TTL and expiration support.

#### Methods & Properties

- **BuildKey** - Builds a tenant-scoped cache key.
  - Parameters:
    - `key`: Original cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
  - Returns: Tenant-scoped key in format "{tenantId}:{key}".
- **GetAsync``1** - Gets a cached value by key with tenant isolation. Returns failure if not found or expired.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Result containing cached value, or failure if not found or expired.
- **SetAsync``1** - Sets a cached value with optional TTL expiration and tenant isolation.
  - Parameters:
    - `key`: Cache key.
    - `value`: Value to cache.
    - `ttlMinutes`: Time to live in minutes. If null, no expiration.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveAsync** - Removes a single cache entry with tenant isolation.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveByPrefixAsync** - Removes all cache entries matching a prefix pattern with tenant isolation.
            Example: RemoveByPrefixAsync("user:*", "tenant1") removes "tenant1:user:*" entries.
  - Parameters:
    - `prefix`: Key prefix to match.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **ExistsAsync** - Checks if a key exists in the cache with tenant isolation (ignores expiration check).
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: True if key exists and is not expired; false otherwise.
- **ClearAsync** - Clears all cache entries for the specified tenant (or "default" if not specified).
            Does not clear entries from other tenants.
  - Parameters:
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.

### MemoryCacheStore

- **Namespace:** `SmartWorkz.Shared.MemoryCacheStore`
- **Summary:** In-memory implementation of ICacheStore with TTL support and thread-safe operations.

#### Methods & Properties

- **#ctor** - Creates a new instance of MemoryCacheStore with default options.
- **#ctor** - Creates a new instance of MemoryCacheStore with specified default options.
- **GetAsync``1** - Retrieves a value from the cache.
- **SetAsync``1** - Sets a value in the cache with optional TTL.
- **SetAsync``1** - Sets a value in the cache with cache options.
- **RemoveAsync** - Removes a value from the cache.
- **RemoveByPrefixAsync** - Removes all values from the cache that match the specified key prefix.
- **ExistsAsync** - Checks if a key exists in the cache and is not expired.
- **ClearAsync** - Clears all entries from the cache.
- **CleanupExpiredEntries** - Performs cleanup of expired entries. This is useful for periodic maintenance.

### ISmsService

- **Namespace:** `SmartWorkz.Shared.ISmsService`
- **Summary:** Defines a contract for SMS communication services.
            Provides methods for sending SMS messages to single or multiple recipients.

#### Methods & Properties

- **SendAsync** - Sends an SMS message to a single recipient.
  - Parameters:
    - `phoneNumber`: The recipient phone number (E.164 format recommended)
    - `message`: The SMS message content
    - `cancellationToken`: Cancellation token
  - Returns: Result containing the SMS ID if successful
- **SendBatchAsync** - Sends an SMS message to multiple recipients (batch).
  - Parameters:
    - `phoneNumbers`: Collection of recipient phone numbers
    - `message`: The SMS message content sent to all recipients
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of SMS IDs if successful

### IWebSocketClient

- **Namespace:** `SmartWorkz.Shared.IWebSocketClient`
- **Summary:** Abstraction for WebSocket client operations.

#### Methods & Properties

- **SendAsync** - Sends a message asynchronously through the WebSocket connection.
- **ReceiveAsync** - Receives a message asynchronously from the WebSocket connection.
            Returns null if the connection is closed.
- **CloseAsync** - Closes the WebSocket connection gracefully.

### WebSocketClient

- **Namespace:** `SmartWorkz.Shared.WebSocketClient`
- **Summary:** Sealed implementation of IWebSocketClient using System.Net.WebSockets.

#### Methods & Properties

- **ConnectAsync** - Connects to a WebSocket server at the specified URI.
- **SendAsync** - Sends a message asynchronously through the WebSocket connection.
- **ReceiveAsync** - Receives a message asynchronously from the WebSocket connection.
            Returns null if the connection is closed.
- **CloseAsync** - Closes the WebSocket connection gracefully.

### ConfigurationHelper

- **Namespace:** `SmartWorkz.Shared.ConfigurationHelper`
- **Summary:** Provides typed access to configuration values with automatic type conversion and validation.
            
             This sealed class implements IConfigurationHelper to provide a strongly-typed interface
             for accessing configuration values. It supports automatic type conversion for common types
             including strings, numeric types, booleans, DateTimes, and enums.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ConfigurationHelper class.
  - Parameters:
    - `configuration`: The configuration source to read from.
- **GetRequired``1** - Gets a required configuration value and converts it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: The configuration value converted to type T.
- **GetOptional``1** - Gets an optional configuration value and converts it to the specified type,
            returning a default value if the key is not found or conversion fails.
  - Parameters:
    - `key`: The configuration key to retrieve.
    - `defaultValue`: The value to return if the key is not found or conversion fails.
  - Returns: The configuration value converted to type T, or the default value if retrieval or conversion fails.
- **TryGet``1** - Attempts to get a configuration value and convert it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: A Result<T> containing the converted value on success, or a failure result
            if the key is not found or conversion fails.
- **Exists** - Checks whether a configuration key exists and is not empty.
  - Parameters:
    - `key`: The configuration key to check.
  - Returns: True if the key exists and is not null or whitespace; otherwise false.
- **ConvertValue``1** - Converts a string value to the specified type using invariant culture for numeric types.
  - Parameters:
    - `value`: The string value to convert.
  - Returns: The converted value of type T.

### ConfigurationValidationException

- **Namespace:** `SmartWorkz.Shared.ConfigurationValidationException`
- **Summary:** Exception thrown when configuration validation fails, indicating that a required
            configuration key is missing, empty, or cannot be converted to the requested type.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ConfigurationValidationException class with a specified message.
  - Parameters:
    - `message`: The error message that explains the reason for the exception.
- **#ctor** - Initializes a new instance of the ConfigurationValidationException class with a specified message
            and a reference to the inner exception that is the cause of this exception.
  - Parameters:
    - `message`: The error message that explains the reason for the exception.
    - `innerException`: The exception that is the cause of the current exception.

### IConfigurationHelper

- **Namespace:** `SmartWorkz.Shared.IConfigurationHelper`
- **Summary:** Provides typed access to configuration values with automatic type conversion and validation.

#### Methods & Properties

- **GetRequired``1** - Gets a required configuration value and converts it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: The configuration value converted to type T.
- **GetOptional``1** - Gets an optional configuration value and converts it to the specified type,
            returning a default value if the key is not found or conversion fails.
  - Parameters:
    - `key`: The configuration key to retrieve.
    - `defaultValue`: The value to return if the key is not found or conversion fails.
  - Returns: The configuration value converted to type T, or the default value if retrieval or conversion fails.
- **TryGet``1** - Attempts to get a configuration value and convert it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: A Result<T> containing the converted value on success, or a failure result
            if the key is not found or conversion fails.
- **Exists** - Checks whether a configuration key exists and is not empty.
  - Parameters:
    - `key`: The configuration key to check.
  - Returns: True if the key exists and is not null or whitespace; otherwise false.

### SharedConstants

- **Namespace:** `SmartWorkz.Shared.SharedConstants`
- **Summary:** Shared configuration constants used throughout SmartWorkz.Shared.
            Enables centralized management of default values and limits.

### ICommand

- **Namespace:** `SmartWorkz.Shared.ICommand`
- **Summary:** Marker interface for command objects representing intent to change state.

### ICommandHandler`1

- **Namespace:** `SmartWorkz.Shared.ICommandHandler`1`
- **Summary:** Handler for processing a specific command type.

#### Methods & Properties

- **HandleAsync** - Handles the specified command asynchronously.
  - Parameters:
    - `command`: The command to handle.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task that represents the asynchronous handle operation.

### IQuery`1

- **Namespace:** `SmartWorkz.Shared.IQuery`1`
- **Summary:** Marker interface for query objects that return a result without modifying state.

### IQueryHandler`2

- **Namespace:** `SmartWorkz.Shared.IQueryHandler`2`
- **Summary:** Handler for processing a specific query type and returning results.

#### Methods & Properties

- **HandleAsync** - Handles the specified query asynchronously and returns the result.
  - Parameters:
    - `query`: The query to handle.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task that represents the asynchronous handle operation and contains the query result.

### MediatorCommandDispatcher

- **Namespace:** `SmartWorkz.Shared.MediatorCommandDispatcher`
- **Summary:** Routes commands to their appropriate handlers via dependency injection.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `serviceProvider`: The service provider for resolving handlers.
- **DispatchAsync``1** - Dispatches the specified command to its handler asynchronously.
  - Parameters:
    - `command`: The command to dispatch.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task representing the asynchronous dispatch operation.

### AdoHelper

- **Namespace:** `SmartWorkz.Shared.AdoHelper`
- **Summary:** ADO.NET helper for executing queries and managing connections.
            Works with any IDbProvider implementation.

#### Methods & Properties

- **ExecuteScalarAsync``1** - Execute scalar query (returns single value).
- **ExecuteNonQueryAsync** - Execute non-query command (INSERT, UPDATE, DELETE).
- **ExecuteQueryAsync``1** - Execute query and map results to objects.
- **ExecuteStoredProcedureAsync** - Execute stored procedure.
- **ExecuteQueryMultipleAsync``2** - Execute query returning multiple result sets (2 sets).
- **ExecuteQueryMultipleAsync``3** - Execute query returning multiple result sets (3 sets).
- **ExecuteQueryMultipleAsync``4** - Execute query returning multiple result sets (4 sets).
- **ExecuteTransactionAsync** - Execute transaction with multiple commands.

### CsvHelper

- **Namespace:** `SmartWorkz.Shared.CsvHelper`
- **Summary:** Provides static methods for reading and writing CSV data with support for column mapping,
            quoted fields, embedded delimiters, and newlines.
            RFC 4180 compliant CSV parsing and writing.
            Sealed class to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **CsvWriter``1** - Serializes a collection of objects to CSV format.
  - Parameters:
    - `items`: The collection of objects to serialize.
    - `options`: CSV options. If null, defaults are used.
  - Returns: A Result containing the CSV string if successful; otherwise a failure.
- **CsvReader``1** - Asynchronously deserializes CSV content to a collection of objects.
  - Parameters:
    - `content`: The CSV content string.
    - `mapping`: Column mapping configuration. If null, property names are used as headers.
    - `options`: CSV options. If null, defaults are used.
  - Returns: A Result containing the deserialized list if successful; otherwise a failure.
- **ParseCsvLines** - Parses CSV content into a list of records (each record is a list of field values).
            Handles quoted fields with embedded delimiters and newlines.
- **WriteRecord** - Writes a single CSV record (list of field values) to the string builder.
            Handles quoting of fields with special characters.
- **ConvertValue** - Converts a string value to the specified type.
- **IsNullableType** - Determines if a type is nullable (Nullable<T> or reference type).

### CsvMapping`1

- **Namespace:** `SmartWorkz.Shared.CsvMapping`1`
- **Summary:** Defines column mapping for CSV operations using a fluent API.
            Supports mapping object properties to CSV columns with custom headers.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **Column``1** - Adds a column mapping for the specified property.
  - Parameters:
    - `propertyExpression`: Expression selecting the property to map.
    - `csvHeader`: The CSV column header name.
  - Returns: This instance for method chaining.
- **ExtractPropertyInfo``1** - Extracts property information from a lambda expression.
  - Parameters:
    - `expression`: The lambda expression.
  - Returns: The PropertyInfo if the expression resolves to a property; otherwise null.
- **CreateAuto** - Creates a mapping automatically from all public properties of type T.
            Property names are used as CSV headers.
  - Returns: A new CsvMapping instance with all properties mapped.

### CsvOptions

- **Namespace:** `SmartWorkz.Shared.CsvOptions`
- **Summary:** Configuration options for CSV read/write operations.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **#ctor** - Creates a default instance of CsvOptions.
- **#ctor** - Creates an instance of CsvOptions with specified delimiter and quote character.
  - Parameters:
    - `delimiter`: The field delimiter character.
    - `quoteChar`: The quote character for quoted fields.

### DbProviderFactory

- **Namespace:** `SmartWorkz.Shared.DbProviderFactory`
- **Summary:** Factory for creating database provider instances.
            Resolves provider name from connection string or explicit specification.

#### Methods & Properties

- **Register** - Register custom provider implementation.
- **GetProvider** - Get provider by name.
- **GetProvider** - Get provider by enum value.
- **GetProviderFromConnectionString** - Get provider from connection string (detects provider automatically).

### IDbProvider

- **Namespace:** `SmartWorkz.Shared.IDbProvider`
- **Summary:** Abstraction for database provider-specific operations.
            Supports multiple providers: SQL Server, MySQL, PostgreSQL, SQLite, Oracle.

#### Methods & Properties

- **CreateConnection** - Create connection with connection string.
- **GetParameterPrefix** - Get parameter prefix for this provider (@, :, $).
- **GetLastInsertIdSql** - Get SQL for last inserted ID based on provider.
- **GetPaginationSql** - Get SQL for pagination based on provider.
- **FormatIdentifier** - Format table/column name for provider (e.g., [brackets] for SQL Server).
- **TestConnectionAsync** - Test connection validity.

### DatabaseProvider

- **Namespace:** `SmartWorkz.Shared.DatabaseProvider`
- **Summary:** Enum of supported database providers.

### QueryMultipleHelper

- **Namespace:** `SmartWorkz.Shared.QueryMultipleHelper`
- **Summary:** Helper for executing multiple queries in a single database roundtrip.
            Eliminates N+1 query problems by batching queries together.

#### Methods & Properties

- **QueryMultipleAsync``2** - Execute multiple queries and return results as tuple.
             Single roundtrip, single SQL execution, improved performance.
- **QueryMultipleAsync``3** - Execute 3 queries in single roundtrip.
- **QueryMultipleAsync``4** - Execute 4 queries in single roundtrip.
- **QueryMultipleAsync``5** - Execute 5 queries in single roundtrip.

### QueryResult`1

- **Namespace:** `SmartWorkz.Shared.QueryResult`1`
- **Summary:** Result wrapper for query operations.

### XmlHelper

- **Namespace:** `SmartWorkz.Shared.XmlHelper`
- **Summary:** Provides static methods for XML serialization, deserialization, and XPath queries.
            Uses System.Xml.Linq for manipulation and reflection for property mapping.
            Sealed class to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **Serialize``1** - Serializes an object to an XML string using reflection.
  - Parameters:
    - `obj`: The object to serialize.
    - `options`: XML options. If null, defaults are used.
  - Returns: A Result containing the XML string if successful; otherwise a failure.
- **Deserialize``1** - Deserializes an XML string to an object of type T using reflection.
  - Parameters:
    - `xml`: The XML string to deserialize.
    - `options`: XML options. If null, defaults are used.
  - Returns: A Result containing the deserialized object if successful; otherwise a failure.
- **Query** - Executes an XPath query on an XML string and returns matching element values.
  - Parameters:
    - `xml`: The XML string to query.
    - `xpathExpression`: The XPath expression to execute.
  - Returns: A Result containing a list of matched values if successful; otherwise a failure.
- **SerializeObject** - Recursively serializes an object's properties into an XML element.
- **DeserializeObject** - Recursively deserializes an XML element into an object's properties.
- **IsBasicType** - Determines if a type is a basic/primitive type supported by XML.
- **IsGenericList** - Determines if a type is a generic List<T>.
- **IsComplexType** - Determines if a type is a complex (non-primitive) type.
- **ConvertToXmlValue** - Converts a value to its XML-safe string representation.
- **ConvertFromXmlValue** - Converts an XML string value to the specified type.

### XmlOptions

- **Namespace:** `SmartWorkz.Shared.XmlOptions`
- **Summary:** Configuration options for XML serialization, deserialization, and query operations.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **#ctor** - Creates a default instance of XmlOptions.
- **#ctor** - Creates an instance of XmlOptions with a specified root element name.
  - Parameters:
    - `rootElement`: The name of the root element.
- **#ctor** - Creates an instance of XmlOptions with specified configuration.
  - Parameters:
    - `rootElement`: The name of the root element.
    - `includeXmlDeclaration`: Whether to include the XML declaration.
    - `indent`: Whether to indent the output.

### ApplicationHealth

- **Namespace:** `SmartWorkz.Shared.ApplicationHealth`
- **Summary:** Represents the overall health status of the application.

### CorrelationContext

- **Namespace:** `SmartWorkz.Shared.CorrelationContext`
- **Summary:** A sealed implementation of  for distributed request tracing.

#### Methods & Properties

- **#ctor** - Initializes a new instance with a generated correlation ID.
- **#ctor** - Initializes a new instance with a specified correlation ID.
  - Parameters:
    - `correlationId`: The correlation ID to use
- **#ctor** - Initializes a child context from a parent context.

### CpuUsage

- **Namespace:** `SmartWorkz.Shared.CpuUsage`
- **Summary:** Represents CPU usage information.

### DiagnosticsHelper

- **Namespace:** `SmartWorkz.Shared.DiagnosticsHelper`
- **Summary:** Sealed helper class for system diagnostics and application health monitoring.
            Provides methods to gather system information, CPU/memory/disk usage, and determine application health.

#### Methods & Properties

- **Initialize** - Initializes the application start time (called once at application startup).
- **GetSystemInfo** - Gets comprehensive system information including CPU, memory, disk, and processor count.
  - Returns: A Result containing SystemInfo or error details.
- **GetMemoryUsage** - Gets memory usage statistics for the current process and system.
  - Returns: A Result containing MemoryUsage or error details.
- **GetCpuUsage** - Gets CPU utilization percentage.
  - Returns: A Result containing CpuUsage or error details.
- **GetDiskSpace** - Gets disk space information for a specific drive.
  - Parameters:
    - `drive`: The drive letter (e.g., "C:", "D:"). Defaults to "C:".
  - Returns: A Result containing DiskSpace or error details.
- **GetUptime** - Gets the application uptime since the last Initialize() call or application start.
  - Returns: A Result containing the uptime as a TimeSpan or error details.
- **GetApplicationHealth** - Gets the overall health status of the application based on system metrics.
  - Returns: A Result containing ApplicationHealth or error details.
- **IsHealthy** - Determines if the application is considered healthy based on the provided health status.
  - Parameters:
    - `health`: The ApplicationHealth object to evaluate.
  - Returns: True if the status is Healthy, false otherwise.
- **InitializeCpuCounter** - Initializes the CPU performance counter (called once).
- **GetMemoryUsageInternal** - Internal method to get memory usage statistics.
- **GetCpuUsageInternal** - Internal method to get CPU usage percentage.
- **GetDiskSpaceInternal** - Internal method to get disk space information.

### DiskSpace

- **Namespace:** `SmartWorkz.Shared.DiskSpace`
- **Summary:** Represents disk space information for a drive.

### HealthCheck

- **Namespace:** `SmartWorkz.Shared.HealthCheck`
- **Summary:** Represents a single health check result.

### HealthStatus

- **Namespace:** `SmartWorkz.Shared.HealthStatus`
- **Summary:** Represents the health status of the application.

### ICorrelationContext

- **Namespace:** `SmartWorkz.Shared.ICorrelationContext`
- **Summary:** Defines a correlation context for distributed request tracing across systems.

#### Methods & Properties

- **SetProperty** - Adds or updates a property in the correlation context.
- **TryGetProperty** - Attempts to retrieve a property from the correlation context.
- **CreateChildContext** - Creates a child correlation context for nested operations (for async/distributed flows).

### MemoryUsage

- **Namespace:** `SmartWorkz.Shared.MemoryUsage`
- **Summary:** Represents memory usage information.

### MetricsHelper

- **Namespace:** `SmartWorkz.Shared.MetricsHelper`
- **Summary:** Provides utilities for collecting and tracking performance metrics.

#### Methods & Properties

- **StartTimer** - Starts a timer and returns an IDisposable that logs elapsed time on disposal.
- **TrackExecution``1** - Tracks the execution time and result of a function.
- **MeasureMemory** - Captures memory usage before and after a block of code execution.

### SystemInfo

- **Namespace:** `SmartWorkz.Shared.SystemInfo`
- **Summary:** Represents system information including CPU, memory, and disk details.

### EventStoreSnapshot

- **Namespace:** `SmartWorkz.Shared.EventStoreSnapshot`
- **Summary:** Represents a snapshot of an aggregate's state at a specific version.
            Snapshots optimize event sourcing by reducing the number of events needed for reconstruction.

### IEventStore

- **Namespace:** `SmartWorkz.Shared.IEventStore`
- **Summary:** Abstraction for an immutable event store that persists domain events.
            Enables event sourcing patterns for temporal queries, audit trails, and event replay.

#### Methods & Properties

- **AppendEventsAsync** - Appends events to the event stream for an aggregate.
            Events are immutable and persist as an append-only log.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `events`: The domain events to append
    - `cancellationToken`: Cancellation token
  - Returns: Task representing the asynchronous operation
- **GetEventsAsync** - Retrieves all events for an aggregate in chronological order.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: Collection of domain events for the aggregate, empty if none exist
- **GetEventsSinceAsync** - Retrieves events for an aggregate after a specific version.
            Useful for incremental event replay and event streaming.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `version`: The version after which to retrieve events
    - `cancellationToken`: Cancellation token
  - Returns: Collection of events after the specified version, empty if none exist
- **GetSnapshotAsync** - Retrieves the latest snapshot for an aggregate if one exists.
            Snapshots optimize aggregate reconstruction by storing intermediate state.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: Snapshot data if exists; null if no snapshot is available
- **SaveSnapshotAsync** - Saves a snapshot of aggregate state at a specific version.
            Snapshots reduce the number of events needed to replay an aggregate.
  - Parameters:
    - `snapshot`: The snapshot to save
    - `cancellationToken`: Cancellation token
  - Returns: Task representing the asynchronous operation
- **GetAggregateAsync``1** - Reconstructs an aggregate from its event history.
            Optionally uses snapshots for performance optimization.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: The reconstructed aggregate instance, or null if no events exist

### SqlEventStore

- **Namespace:** `SmartWorkz.Shared.SqlEventStore`
- **Summary:** SQL Server implementation of the event store using Dapper for data access.
            Provides immutable append-only event log with snapshot support for optimization.
            Implements optimistic concurrency control using version numbers.

#### Methods & Properties

- **AppendEventsAsync** - Appends events to the event stream for an aggregate.
- **GetEventsAsync** - Retrieves all events for an aggregate in chronological order.
- **GetEventsSinceAsync** - Retrieves events for an aggregate after a specific version.
- **GetSnapshotAsync** - Retrieves the latest snapshot for an aggregate if one exists.
- **SaveSnapshotAsync** - Saves a snapshot of aggregate state at a specific version.
- **GetAggregateAsync``1** - Reconstructs an aggregate from its event history.
            Optionally uses snapshots for performance optimization.
- **GetCurrentVersionAsync** - Gets the current version number for an aggregate.
- **DeserializeEvent** - Deserializes a stored event record back to IDomainEvent.

### IDomainEvent

- **Namespace:** `SmartWorkz.Shared.IDomainEvent`
- **Summary:** Base interface for domain events in event-driven architecture.
            Provides core event metadata for tracking and publishing.

### IEventPublisher

- **Namespace:** `SmartWorkz.Shared.IEventPublisher`
- **Summary:** Publishes domain events for event-driven architecture.

#### Methods & Properties

- **PublishAsync``1** - Publishes a single domain event.
  - Parameters:
    - `event`: Event to publish.
    - `cancellationToken`: Cancellation token.
- **PublishAsync``1** - Publishes multiple domain events.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token.

### IEventSubscriber

- **Namespace:** `SmartWorkz.Shared.IEventSubscriber`
- **Summary:** Registers event handlers for domain events.

#### Methods & Properties

- **Subscribe``1** - Subscribes a handler to an event type.
            Multiple handlers can subscribe to the same event.
  - Parameters:
    - `handler`: Async handler function. Receives event and cancellation token.

### InMemoryEventPublisher

- **Namespace:** `SmartWorkz.Shared.InMemoryEventPublisher`
- **Summary:** In-memory event publisher that executes all registered handlers sequentially.
            Provides synchronous event delivery with exception handling and result reporting.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the InMemoryEventPublisher with a subscriber.
  - Parameters:
    - `subscriber`: The event subscriber containing registered handlers.
- **PublishAsync``1** - Publishes a single domain event to all registered handlers.
            Handlers are invoked sequentially in registration order.
            If any handler throws an exception, it is caught and a failure Result is returned.
            Other handlers will attempt to execute even if a previous handler fails.
  - Parameters:
    - `event`: The event instance to publish.
    - `cancellationToken`: Cancellation token to cancel handler execution.
  - Returns: A task representing the asynchronous operation.
- **PublishAsync``1** - Publishes multiple domain events to all registered handlers.
            Events are published sequentially in the order provided.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token to cancel handler execution.
  - Returns: A task representing the asynchronous batch operation.

### InMemoryEventSubscriber

- **Namespace:** `SmartWorkz.Shared.InMemoryEventSubscriber`
- **Summary:** In-memory event subscriber that maintains a registry of event handlers.
            Supports multiple handlers per event type using thread-safe concurrent collections.

#### Methods & Properties

- **Subscribe``1** - Subscribes a handler to an event type.
            Multiple handlers can be registered for the same event type and will execute sequentially.
  - Parameters:
    - `handler`: Async handler function that receives event and cancellation token.
- **GetHandlers** - Gets all registered handlers for a given event type.
            Returns an empty list if no handlers are registered for the type.
  - Parameters:
    - `eventType`: The event type to retrieve handlers for.
  - Returns: List of registered handlers (delegates).

### MassTransitEventPublisher

- **Namespace:** `SmartWorkz.Shared.MassTransitEventPublisher`
- **Summary:** Distributed event publisher using MassTransit message bus.
            Supports both single and batch event publishing with async/await patterns.
            Suitable for production environments with message broker backend (RabbitMQ, Azure Service Bus, etc).

#### Methods & Properties

- **#ctor** - Initializes a new instance of MassTransitEventPublisher.
  - Parameters:
    - `publishEndpoint`: MassTransit publish endpoint for message distribution.
    - `logger`: Logger for event publication tracking.
- **PublishAsync``1** - Publishes a single domain event to the message bus asynchronously.
  - Parameters:
    - `event`: Event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous publish operation.
- **PublishAsync``1** - Publishes multiple domain events to the message bus asynchronously.
            Events are published sequentially in the order provided.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous batch publish operation.

### PublisherType

- **Namespace:** `SmartWorkz.Shared.PublisherType`
- **Summary:** Specifies the publisher type for event publishing.

### ServiceCollectionExtensions

- **Namespace:** `SmartWorkz.Shared.ServiceCollectionExtensions`
- **Summary:** Extension methods for IServiceCollection to register Core.Shared services.

#### Methods & Properties

- **AddCoreSharedServices** - Adds Core.Shared services including TemplateEngine for template rendering.
- **AddEventPublishing** - Adds event publishing services to the dependency injection container.
            Supports switching between in-memory and MassTransit publishers based on application needs.
  - Parameters:
    - `services`: The service collection.
    - `publisherType`: The publisher type to use (defaults to InMemory).
  - Returns: The service collection for method chaining.

### DefaultFeatureFlagService

- **Namespace:** `SmartWorkz.Shared.DefaultFeatureFlagService`
- **Summary:** Global (non-tenant) feature flag service with in-memory storage.
            Thread-safe implementation suitable for single-process deployments.
            Use for organization-wide feature toggles; use ITenantFeatureFlags for tenant-scoped flags.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature flag is enabled.
            Returns false for unknown flags (does not throw).
  - Parameters:
    - `flagName`: The name of the feature flag to check.
    - `cancellationToken`: Cancellation token.
  - Returns: True if the flag exists and is enabled; false otherwise.
- **GetEnabledFeaturesAsync** - Gets all enabled feature flags.
            Returns empty list if no flags are enabled.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: A read-only list of enabled feature flag names.
- **EnableFlag** - Enables a feature flag.
            Creates the flag if it doesn't exist.
  - Parameters:
    - `flagName`: The name of the feature flag to enable.
- **DisableFlag** - Disables a feature flag.
            Creates the flag as disabled if it doesn't exist.
  - Parameters:
    - `flagName`: The name of the feature flag to disable.

### IFeatureFlagService

- **Namespace:** `SmartWorkz.Shared.IFeatureFlagService`
- **Summary:** Global feature flag service for cross-tenant feature control.
            Use for organization-wide feature toggles (not tenant-specific).
            For tenant-scoped flags, use ITenantFeatureFlags.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a global feature is enabled.
  - Parameters:
    - `flagName`: Feature flag name (e.g., "NEW_DASHBOARD", "BETA_REPORTING").
    - `cancellationToken`: Cancellation token.
  - Returns: True if feature is enabled globally.
- **GetEnabledFeaturesAsync** - Gets all enabled global features.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: List of enabled feature flag names.

### IFileStorageService

- **Namespace:** `SmartWorkz.Shared.IFileStorageService`
- **Summary:** Interface for file storage operations supporting both local and cloud providers.

#### Methods & Properties

- **UploadAsync** - Uploads a file to storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `content`: The file content stream.
    - `metadata`: The file metadata.
    - `cancellationToken`: The cancellation token.
  - Returns: The full path or URI of the uploaded file.
- **DownloadAsync** - Downloads a file from storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: A stream containing the file content. Caller must dispose using 'using' statement.
- **DeleteAsync** - Deletes a file from storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
- **ExistsAsync** - Checks if a file exists in storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: True if the file exists, false otherwise.
- **GetMetadataAsync** - Gets metadata for a file in storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: FileMetadata if file exists, null otherwise.
- **ListAsync** - Lists files in a directory or container prefix.
  - Parameters:
    - `folderPath`: The relative folder path or blob prefix.
    - `cancellationToken`: The cancellation token.
  - Returns: A read-only collection of FileMetadata for files in the directory/prefix.
- **GenerateTemporaryUrlAsync** - Generates a temporary download URL for a file.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `expiration`: The expiration duration from now.
    - `cancellationToken`: The cancellation token.
  - Returns: A URL that can be used to download the file. For local storage, returns the full file path.

### GridColumn

- **Namespace:** `SmartWorkz.Shared.GridColumn`
- **Summary:** Defines a single column in a grid, including display options, sorting, filtering, and rendering hints.

### GridExportOptions

- **Namespace:** `SmartWorkz.Shared.GridExportOptions`
- **Summary:** Configuration for grid data export (CSV, Excel).

### GridRequest

- **Namespace:** `SmartWorkz.Shared.GridRequest`
- **Summary:** Request parameters for grid data fetching, extending PagedQuery with filtering support.

#### Methods & Properties

- **#ctor** - Request parameters for grid data fetching, extending PagedQuery with filtering support.

### GridResponse`1

- **Namespace:** `SmartWorkz.Shared.GridResponse`1`
- **Summary:** Response from a grid data request, including paged data, column metadata, and filter options.

### IGridDataProvider

- **Namespace:** `SmartWorkz.Shared.IGridDataProvider`
- **Summary:** Abstraction for grid data fetching. Implementations handle API calls or in-memory queries.
            Enables platform independence: Web uses HTTP, MAUI uses direct API client, Desktop uses local DB.

#### Methods & Properties

- **GetDataAsync``1** - Fetch paged grid data based on request (sorting, filtering, pagination).
  - Parameters:
    - `request`: Grid request with sorting, paging, and filter criteria.
    - `cancellationToken`: Cancellation token for async operations.
  - Returns: Result containing GridResponse or error details.

### Guard

- **Namespace:** `SmartWorkz.Shared.Guard`
- **Summary:** Static guard clauses for argument validation at method entry points.
             Throw immediately on invalid input — fail fast, fail loudly.
            
             Usage:
               Guard.NotNull(userId, nameof(userId));
               Guard.NotEmpty(name, nameof(name));
               Guard.InRange(pageSize, 1, 100, nameof(pageSize));
            
             These replace the ValidationExtensions.EnsureNotNull() extension method
             and the scattered ArgumentNullException throws throughout the codebase.

#### Methods & Properties

- **NotNull``1** - Throws ArgumentNullException if value is null.
- **NotNull``1** - Throws ArgumentNullException if value is null (struct/nullable).
- **NotEmpty** - Throws ArgumentException if string is null, empty, or whitespace.
- **NotEmpty``1** - Throws ArgumentException if collection is null or has no elements.
- **NotDefault``1** - Throws ArgumentException if value equals the default for its type (0, null, Guid.Empty).
- **InRange``1** - Throws ArgumentOutOfRangeException if value is outside [min, max].
- **Requires** - Throws ArgumentException if condition is false.

### EncryptionHelper

- **Namespace:** `SmartWorkz.Shared.EncryptionHelper`
- **Summary:** Cryptographic utilities for hashing and encryption.
            Uses PBKDF2 for password hashing and AES-256 for data encryption.

#### Methods & Properties

- **HashPassword** - Hash password using PBKDF2 with SHA256.
- **VerifyPassword** - Verify password against hash.
- **Encrypt** - Encrypt text using AES-256-GCM with provided key.
- **Decrypt** - Decrypt text using AES-256-GCM with provided key.
- **GenerateRandomString** - Generate cryptographically secure random string.
- **GenerateEncryptionKey** - Generate random encryption key (Base64 encoded).
- **ComputeSha256** - Compute SHA256 hash of text for integrity checking.

### JsonHelper

- **Namespace:** `SmartWorkz.Shared.JsonHelper`
- **Summary:** JSON serialization utilities using System.Text.Json.
            Provides consistent serialization options across the application.

#### Methods & Properties

- **Serialize``1** - Serialize object to JSON string.
- **Serialize** - Serialize object to JSON string with dynamic type.
- **Deserialize``1** - Deserialize JSON string to object.
- **Deserialize** - Deserialize JSON string to object with dynamic type.
- **DeserializeAsync``1** - Deserialize JSON asynchronously from stream.
- **SerializeAsync``1** - Serialize asynchronously to stream.
- **IsValidJson** - Check if string is valid JSON.
- **GetValueByPath** - Parse JSON and extract value at specified path (dot notation).

### IHttpClient

- **Namespace:** `SmartWorkz.Shared.IHttpClient`
- **Summary:** Abstraction for HTTP client operations with support for async/await and cancellation.
            Implementations should handle retries, timeouts, and error responses gracefully.

#### Methods & Properties

- **GetAsync``1** - Sends a GET request and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **PostAsync``1** - Sends a POST request with JSON body and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **PutAsync``1** - Sends a PUT request with JSON body and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **DeleteAsync``1** - Sends a DELETE request and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **GetAsync** - Sends a GET request and returns a string response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.
- **PostAsync** - Sends a POST request with JSON body and returns a string response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.

### RetryStrategy

- **Namespace:** `SmartWorkz.Shared.RetryStrategy`
- **Summary:** Specifies the backoff strategy to use when retrying failed HTTP requests.

### RetryPolicy

- **Namespace:** `SmartWorkz.Shared.RetryPolicy`
- **Summary:** Configures automatic retry behavior for failed HTTP requests.

### AuditRecord

- **Namespace:** `SmartWorkz.Shared.AuditRecord`
- **Summary:** Represents an immutable audit record with all relevant audit information.

#### Methods & Properties

- **#ctor** - Represents an immutable audit record with all relevant audit information.
  - Parameters:
    - `Id`: The unique identifier of the audit record
    - `EntityType`: The type of entity being audited (e.g., "User", "BlogPost")
    - `EntityId`: The identifier of the audited entity
    - `Action`: The action performed (Create, Update, Delete, etc.)
    - `UserId`: The identifier of the user who performed the action
    - `PerformedAt`: The timestamp when the action was performed
    - `Metadata`: Optional metadata dictionary containing additional context

### EnrichedLogger

- **Namespace:** `SmartWorkz.Shared.EnrichedLogger`
- **Summary:** Enriched logger wrapper around ILogger that provides structured logging methods
            for domain events, commands, sagas, file operations, and background jobs.
            Uses structured properties instead of string interpolation for better queryability.

#### Methods & Properties

- **#ctor** - Creates a new instance of EnrichedLogger.
  - Parameters:
    - `logger`: The underlying ILogger instance
- **LogCommandExecuted** - Logs command execution with duration and other metrics.
  - Parameters:
    - `commandType`: The type of command being executed
    - `duration`: How long the command took to execute
- **LogCommandExecutionError** - Logs a command execution error with exception details.
  - Parameters:
    - `commandType`: The type of command that failed
    - `exception`: The exception that occurred
- **LogCommandValidationError** - Logs a command with validation errors.
  - Parameters:
    - `commandType`: The type of command
    - `errors`: Dictionary of validation errors
- **LogEventPublished** - Logs an event publication with metadata.
  - Parameters:
    - `eventType`: The type of event being published
    - `eventId`: The unique identifier of the event
- **LogEventPublishedWithContext** - Logs an event with additional context properties.
  - Parameters:
    - `eventType`: The type of event
    - `eventId`: The event identifier
    - `context`: Additional context data
- **LogEventSubscribed** - Logs an event subscription.
  - Parameters:
    - `eventType`: The type of event being subscribed to
    - `subscriberType`: The subscriber type
- **LogSagaStarted** - Logs the start of a saga with its initial state.
  - Parameters:
    - `sagaId`: The unique saga identifier
    - `state`: The initial saga state
- **LogSagaStateTransition** - Logs a saga state transition.
  - Parameters:
    - `sagaId`: The saga identifier
    - `fromState`: The previous state
    - `toState`: The new state
- **LogSagaCompleted** - Logs the completion of a saga.
  - Parameters:
    - `sagaId`: The saga identifier
    - `duration`: How long the saga took to complete
- **LogSagaFailed** - Logs a saga failure.
  - Parameters:
    - `sagaId`: The saga identifier
    - `exception`: The exception that caused the failure
- **LogFileOperation** - Logs file operations such as upload, download, delete.
  - Parameters:
    - `operation`: The type of operation (Upload, Download, Delete, etc.)
    - `filePath`: The file path or URI
- **LogFileOperationWithSize** - Logs a file operation with size information.
  - Parameters:
    - `operation`: The type of operation
    - `filePath`: The file path
    - `sizeBytes`: The file size in bytes
- **LogFileOperationError** - Logs a file operation error.
  - Parameters:
    - `operation`: The operation that failed
    - `filePath`: The file path
    - `exception`: The exception that occurred
- **LogJobQueued** - Logs when a background job is queued.
  - Parameters:
    - `jobId`: The unique job identifier
    - `jobType`: The type of job being queued
- **LogJobStarted** - Logs when a background job starts processing.
  - Parameters:
    - `jobId`: The job identifier
    - `jobType`: The job type
- **LogJobCompleted** - Logs successful job completion.
  - Parameters:
    - `jobId`: The job identifier
    - `duration`: How long the job took to complete
- **LogJobFailed** - Logs a job failure.
  - Parameters:
    - `jobId`: The job identifier
    - `exception`: The exception that caused the failure
- **LogJobRetry** - Logs job retry attempt.
  - Parameters:
    - `jobId`: The job identifier
    - `attemptNumber`: The current attempt number
    - `maxRetries`: The maximum number of retries
- **LogWithContext** - Logs a message with structured context properties.
  - Parameters:
    - `operationName`: The name of the operation
    - `context`: Dictionary of contextual properties
- **LogPerformanceMetrics** - Logs performance metrics for an operation.
  - Parameters:
    - `operationName`: The operation name
    - `duration`: The operation duration
    - `resultStatus`: The result status (Success, Failure, etc.)
- **LogCorrelation** - Logs a correlation ID for request tracing.
  - Parameters:
    - `correlationId`: The correlation identifier
    - `userId`: Optional user identifier
    - `requestPath`: Optional request path
- **LogUnhandledException** - Logs unhandled exceptions as critical errors.
  - Parameters:
    - `exception`: The exception that occurred
    - `operationName`: The operation that failed

### IAuditLogger

- **Namespace:** `SmartWorkz.Shared.IAuditLogger`
- **Summary:** Interface for structured audit logging with metadata support.

#### Methods & Properties

- **LogAuditAsync** - Logs an audit event with structured metadata.
  - Parameters:
    - `entityType`: The entity type being audited (e.g., "User", "BlogPost")
    - `entityId`: The unique identifier of the entity
    - `action`: The action performed (Create, Update, Delete, etc.)
    - `metadata`: Optional metadata dictionary for additional context
    - `cancellationToken`: Cancellation token
  - Returns: Result indicating success or failure
- **GetAuditHistoryAsync** - Retrieves audit logs for a specific entity.
  - Parameters:
    - `entityType`: The entity type
    - `entityId`: The entity identifier
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of audit records
- **GetUserActivityAsync** - Retrieves audit logs for a specific user across all entities.
  - Parameters:
    - `userId`: The user identifier
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of audit records

### ILogger

- **Namespace:** `SmartWorkz.Shared.ILogger`
- **Summary:** Abstraction for application logging.
            Decouples from specific logging frameworks (Serilog, NLog, etc.).

### LogLevel

- **Namespace:** `SmartWorkz.Shared.LogLevel`
- **Summary:** Log level severity.

### ILoggerFactory

- **Namespace:** `SmartWorkz.Shared.ILoggerFactory`
- **Summary:** Factory for creating logger instances by category/source.

### IMapper

- **Namespace:** `SmartWorkz.Shared.IMapper`
- **Summary:** Mapping service abstraction for transforming objects between types.
            Supports registration of mapping profiles and bidirectional conversions.

#### Methods & Properties

- **Map``2** - Map source object to target type.
- **Map** - Map source object to target type using dynamic type.
- **MapAsync``2** - Map asynchronously with potential async operations in profile.
- **MapCollection``2** - Map collection of sources to targets.
- **MapCollectionAsync``2** - Map collection asynchronously.
- **RegisterProfile``2** - Register a mapping profile.

### IMapperProfile

- **Namespace:** `SmartWorkz.Shared.IMapperProfile`
- **Summary:** Profile for defining mapping rules between types.
            Implemented by concrete profiles that configure source-to-target transformations.

### IMapperProfile`2

- **Namespace:** `SmartWorkz.Shared.IMapperProfile`2`
- **Summary:** Typed mapper profile for strong typing.

#### Methods & Properties

- **Map** - Transform source to target synchronously.
- **MapAsync** - Transform source to target asynchronously.

### SimpleMapper

- **Namespace:** `SmartWorkz.Shared.SimpleMapper`
- **Summary:** A simple in-memory mapper that supports registering and executing mapping profiles.

### IMetricsCollector

- **Namespace:** `SmartWorkz.Shared.IMetricsCollector`
- **Summary:** Abstraction for collecting application metrics and performance data.
            Enables tracking of operation duration, throughput, error rates, and custom metrics.
            Implementations integrate with OpenTelemetry for export to Prometheus/Grafana.

#### Methods & Properties

- **RecordOperationDuration** - Record operation duration in milliseconds.
  - Parameters:
    - `operationName`: Name of the operation being measured.
    - `durationMs`: Duration in milliseconds.
    - `status`: Optional status (e.g., "success", "error").
    - `tags`: Optional metadata tags for grouping and filtering.
- **RecordOperationCount** - Record operation count (increments counter).
  - Parameters:
    - `operationName`: Name of the operation.
    - `count`: Number to increment by (default 1).
    - `status`: Optional status label.
    - `tags`: Optional metadata tags.
- **RecordGaugeValue** - Record a gauge value (e.g., queue depth, memory usage).
  - Parameters:
    - `metricName`: Name of the gauge metric.
    - `value`: The gauge value to record.
    - `tags`: Optional metadata tags.
- **RecordError** - Record error/exception occurrence.
  - Parameters:
    - `operationName`: Name of the operation that failed.
    - `ex`: The exception that occurred.
    - `tags`: Optional metadata tags.
- **IncrementCounter** - Increment a custom counter.
  - Parameters:
    - `counterName`: Name of the counter.
    - `increment`: Amount to increment (default 1).
    - `tags`: Optional metadata tags.

### MetricsMiddleware

- **Namespace:** `SmartWorkz.Shared.MetricsMiddleware`
- **Summary:** ASP.NET Core middleware for automatic HTTP request/response metrics collection.
             Records operation duration, status, and errors for all HTTP requests.
            
             Usage:
                 app.UseMiddleware<MetricsMiddleware>();

### MetricsStartupExtensions

- **Namespace:** `SmartWorkz.Shared.MetricsStartupExtensions`
- **Summary:** Extension methods for registering application metrics in dependency injection.

#### Methods & Properties

- **AddApplicationMetrics** - Registers IMetricsCollector with OpenTelemetry implementation.
  - Parameters:
    - `services`: The service collection to register with.
  - Returns: The service collection for method chaining.

### OpenTelemetryMetricsCollector

- **Namespace:** `SmartWorkz.Shared.OpenTelemetryMetricsCollector`
- **Summary:** OpenTelemetry-based implementation of IMetricsCollector.
            Collects metrics using System.Diagnostics.Metrics for export to Prometheus/Grafana.

### DefaultTenantFeatureFlags

- **Namespace:** `SmartWorkz.Shared.DefaultTenantFeatureFlags`
- **Summary:** In-memory feature flag provider for tenant-scoped feature control.
            
             Uses ConcurrentDictionary to store tenant-specific flags:
             - Key: tenant ID
             - Value: HashSet of enabled feature flag names
            
             Thread-safe for concurrent operations. Suitable for in-process caching
             or dev/test scenarios. For distributed systems, integrate with a
             centralized feature flag service (Unleash, LaunchDarkly, etc.).

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature is enabled for the specified tenant.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").
    - `cancellationToken`: Cancellation token.
  - Returns: Task containing true if the feature is enabled for this tenant,
            false if the tenant doesn't exist or the flag is not enabled.
- **GetEnabledFeaturesAsync** - Gets all enabled features for a tenant.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Task containing a read-only list of enabled feature flag names.
            Returns an empty list if the tenant doesn't exist or has no enabled flags.
- **EnableFlag** - Enables a feature flag for a tenant.
            Idempotent — calling multiple times with the same flag is safe.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name.
- **DisableFlag** - Disables a feature flag for a tenant.
            Idempotent — calling multiple times with the same flag is safe.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name.

### ITenantContext

- **Namespace:** `SmartWorkz.Shared.ITenantContext`
- **Summary:** Scoped service providing current tenant ID for multi-tenant applications.
            Resolved from request context or claims principal.

#### Methods & Properties

- **GetTenantId** - Gets the current tenant identifier.
  - Returns: Tenant ID, or null if operating in single-tenant context.
- **SetTenantId** - Sets the current tenant identifier (rarely used; typically set from request context).
  - Parameters:
    - `tenantId`: Tenant ID to set.

### ITenantFeatureFlags

- **Namespace:** `SmartWorkz.Shared.ITenantFeatureFlags`
- **Summary:** Feature flag provider scoped to a specific tenant.
            Allows per-tenant feature control.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature is enabled for the specified tenant.
  - Parameters:
    - `tenantId`: Tenant ID.
    - `flagName`: Feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").
    - `cancellationToken`: Cancellation token.
  - Returns: True if feature is enabled for this tenant.
- **GetEnabledFeaturesAsync** - Gets all enabled features for a tenant.
  - Parameters:
    - `tenantId`: Tenant ID.
    - `cancellationToken`: Cancellation token.
  - Returns: List of enabled feature flag names.

### TenantContext

- **Namespace:** `SmartWorkz.Shared.TenantContext`
- **Summary:** Scoped tenant context using AsyncLocal for proper isolation across async boundaries.
            
             AsyncLocal ensures:
             - Thread-safe storage per async execution context
             - Isolation between concurrent requests (each gets its own context)
             - Proper inheritance to child tasks (when awaited)
            
             Survives async/await boundaries unlike ThreadLocal, making it suitable for async methods.

#### Methods & Properties

- **GetTenantId** - Gets the current tenant identifier.
  - Returns: The current tenant ID, or "default" if not set.
- **SetTenantId** - Sets the current tenant identifier.
  - Parameters:
    - `tenantId`: The tenant ID to set. Cannot be null or empty.

### FirebaseCloudMessagingService

- **Namespace:** `SmartWorkz.Shared.FirebaseCloudMessagingService`
- **Summary:** Firebase Cloud Messaging service implementation for sending push notifications.
            Supports single/batch user notifications, topic-based broadcasting, and multi-platform delivery (Android, iOS, Web).

#### Methods & Properties

- **#ctor** - Initializes a new instance of the FirebaseCloudMessagingService.
  - Parameters:
    - `logger`: Logger for diagnostic and error information.
- **SendAsync** - Sends a simple push notification to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends simple push notifications to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase device tokens).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends a rich push notification with metadata to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends a rich push notification to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase device tokens).
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SendToTopicAsync** - Sends a rich push notification to all users subscribed to a topic (broadcast).
  - Parameters:
    - `topic`: Topic name (e.g., "news", "promotions").
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SubscribeToTopicAsync** - Subscribes a user to a topic for broadcast notifications.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `topic`: Topic name to subscribe to.
    - `cancellationToken`: Cancellation token.
- **UnsubscribeFromTopicAsync** - Unsubscribes a user from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `topic`: Topic name to unsubscribe from.
    - `cancellationToken`: Cancellation token.

### IPushNotificationService

- **Namespace:** `SmartWorkz.Shared.IPushNotificationService`
- **Summary:** Service for sending push notifications using Firebase Cloud Messaging.

#### Methods & Properties

- **SendAsync** - Sends a simple push notification to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a simple push notification to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase tokens).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a rich push notification with metadata to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a rich push notification with metadata to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase tokens).
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendToTopicAsync** - Sends a push notification to all users subscribed to a topic.
  - Parameters:
    - `topic`: The topic name.
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SubscribeToTopicAsync** - Subscribes a user to receive notifications from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `topic`: The topic name.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous subscription operation.
- **UnsubscribeFromTopicAsync** - Unsubscribes a user from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `topic`: The topic name.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous unsubscription operation.

### PushNotificationPayload

- **Namespace:** `SmartWorkz.Shared.PushNotificationPayload`
- **Summary:** Represents the payload data for a push notification.

### PushNotificationAction

- **Namespace:** `SmartWorkz.Shared.PushNotificationAction`
- **Summary:** Represents an action that can be performed from a push notification.

### PagedList`1

- **Namespace:** `SmartWorkz.Shared.PagedList`1`
- **Summary:** A page of items with metadata.
             Replaces PaginationResponse<T> in StarterKitMVC.Shared.DTOs.
            
             Migration path: PaginationResponse<T> has the same fields under different names.
             PagedList<T>.Create() is a drop-in replacement for PaginationResponse<T>.Create().

#### Methods & Properties

- **Empty** - Create an empty result set (e.g., when no rows match).
- **Map``1** - Project items to a different type without changing pagination metadata.

### PagedQuery

- **Namespace:** `SmartWorkz.Shared.PagedQuery`
- **Summary:** Standard request parameters for any paginated query.
             Replaces the existing PaginationRequest record in StarterKitMVC.Shared.DTOs.
            
             Migration: PaginationRequest is structurally identical — alias it or replace it.

#### Methods & Properties

- **#ctor** - Standard request parameters for any paginated query.
             Replaces the existing PaginationRequest record in StarterKitMVC.Shared.DTOs.
            
             Migration: PaginationRequest is structurally identical — alias it or replace it.
- **Normalize** - Clamp page and pageSize to safe bounds.

### IEntity`1

- **Namespace:** `SmartWorkz.Shared.IEntity`1`
- **Summary:** Marks a class as a domain entity with a typed primary key.

### CircuitBreaker

- **Namespace:** `SmartWorkz.Shared.CircuitBreaker`
- **Summary:** A thread-safe implementation of the circuit breaker pattern for handling failing dependencies gracefully.
            
             The circuit breaker operates in three states:
             - Closed: Normal operation. Requests pass through. Failures are tracked.
             - Open: Failing. All requests are rejected immediately to prevent cascading failures.
             - HalfOpen: Testing recovery. Limited requests are allowed to test if the dependency has recovered.
            
             State transitions:
             - Closed → Open: When ConsecutiveFailures >= FailureThreshold
             - Open → HalfOpen: Automatically when (DateTime.UtcNow - LastFailureTime) >= TimeoutMilliseconds
             - HalfOpen → Closed: When SuccessCount >= SuccessThreshold
             - HalfOpen → Open: When RecordFailure() is called in HalfOpen state
             - Closed → Closed: When RecordSuccess() is called (resets failure counter)

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `options`: The circuit breaker configuration options.
- **RecordSuccess** - Records a successful operation and updates state transitions accordingly.
            In Closed state: resets the failure counter.
            In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
- **RecordFailure** - Records a failed operation and updates state transitions accordingly.
            In Closed state: increments failure counter and transitions to Open if threshold is met.
            In HalfOpen state: immediately transitions back to Open.
- **Reset** - Resets the circuit breaker to the Closed state, clearing all failure and success counters.

### CircuitBreakerOptions

- **Namespace:** `SmartWorkz.Shared.CircuitBreakerOptions`
- **Summary:** Configuration options for the circuit breaker.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if options are valid; otherwise false.

### CircuitBreakerState

- **Namespace:** `SmartWorkz.Shared.CircuitBreakerState`
- **Summary:** Defines the state of a circuit breaker in the state machine pattern.

### ICircuitBreaker

- **Namespace:** `SmartWorkz.Shared.ICircuitBreaker`
- **Summary:** Defines the contract for a circuit breaker that implements the state machine pattern
            to handle failing dependencies gracefully.

#### Methods & Properties

- **RecordSuccess** - Records a successful operation and updates state transitions accordingly.
            In Closed state: resets the failure counter.
            In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
- **RecordFailure** - Records a failed operation and updates state transitions accordingly.
            In Closed state: increments failure counter and transitions to Open if threshold is met.
            In HalfOpen state: immediately transitions back to Open.
- **Reset** - Resets the circuit breaker to the Closed state, clearing all failure and success counters.

### IRateLimiter

- **Namespace:** `SmartWorkz.Shared.IRateLimiter`
- **Summary:** Defines the contract for a thread-safe rate limiter.

#### Methods & Properties

- **TryAcquireAsync** - Attempts to acquire tokens for a request identified by the given identifier.
  - Parameters:
    - `identifier`: The identifier for rate limiting (e.g., IP address, user ID).
    - `cost`: The number of tokens to acquire (default: 1).
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A result containing true if tokens were acquired successfully; otherwise false.
            On failure, the Error will include retry-after information.
- **GetAvailableTokensAsync** - Gets the number of available tokens for a given identifier.
  - Parameters:
    - `identifier`: The identifier to check.
  - Returns: The number of available tokens.
- **ResetAsync** - Resets the rate limit state for a given identifier.
  - Parameters:
    - `identifier`: The identifier to reset.
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **ClearAsync** - Clears all rate limit state.
  - Parameters:
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.

### RateLimiter

- **Namespace:** `SmartWorkz.Shared.RateLimiter`
- **Summary:** Thread-safe token bucket rate limiter implementation.
            
             This class maintains a per-identifier token bucket that refills at a constant rate.
             Tokens are consumed when requests are made; if insufficient tokens exist, the request is denied.
            
             Thread-safe operations use ConcurrentDictionary and locks on individual buckets to ensure
             consistent state without global locking bottlenecks.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the RateLimiter class.
  - Parameters:
    - `options`: Configuration options for the rate limiter.
- **TryAcquireAsync** - Attempts to acquire tokens for a request identified by the given identifier.
  - Parameters:
    - `identifier`: The identifier for rate limiting (e.g., IP address, user ID).
    - `cost`: The number of tokens to acquire (default: 1).
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A result containing true if tokens were acquired successfully; otherwise false.
            On failure, the Error will include retry-after information.
- **GetAvailableTokensAsync** - Gets the number of available tokens for a given identifier.
  - Parameters:
    - `identifier`: The identifier to check.
  - Returns: The number of available tokens.
- **ResetAsync** - Resets the rate limit state for a given identifier.
  - Parameters:
    - `identifier`: The identifier to reset.
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **ClearAsync** - Clears all rate limit state.
  - Parameters:
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **TokenBucket.TryAcquire** - Tries to acquire the specified number of tokens.
- **TokenBucket.GetAvailableTokens** - Gets the current number of available tokens.
- **TokenBucket.GetRetryAfterMilliseconds** - Gets the number of milliseconds to wait before retrying.
- **TokenBucket.RefillTokens** - Refills the token bucket based on elapsed time.

### RateLimiterOptions

- **Namespace:** `SmartWorkz.Shared.RateLimiterOptions`
- **Summary:** Configuration options for the rate limiter.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if options are valid; otherwise false.

### RateLimiterStrategy

- **Namespace:** `SmartWorkz.Shared.RateLimiterStrategy`
- **Summary:** Specifies the strategy used by the rate limiter to control request flow.

### ApiError

- **Namespace:** `SmartWorkz.Shared.ApiError`
- **Summary:** Structured error representation for API responses.
            Provides code, message, and optional field-level error details.

#### Methods & Properties

- **FromError** - Create from core Error type.
- **FromValidationErrors** - Create from validation errors.
- **FromException** - Create from exception.

### ApiResponse

- **Namespace:** `SmartWorkz.Shared.ApiResponse`
- **Summary:** Generic API response envelope that wraps result data with metadata.
            Non-generic convenience version for non-data responses.

#### Methods & Properties

- **Ok** - Success response without data.
- **Fail** - Failure response with error details.
- **FromResult** - Create from core Result pattern.

### ApiResponse`1

- **Namespace:** `SmartWorkz.Shared.ApiResponse`1`
- **Summary:** Typed API response envelope with data payload.
            Includes optional pagination metadata for list responses.

#### Methods & Properties

- **Ok** - Success response with data.
- **OkPaginated** - Success response with paginated data.
- **Fail** - Failure response with error.

### ProblemDetailsResponse

- **Namespace:** `SmartWorkz.Shared.ProblemDetailsResponse`
- **Summary:** Implements RFC 7807 Problem Details for HTTP APIs standard response format.
            Provides a standardized way to represent error details in API responses.

#### Methods & Properties

- **ValidationError** - Factory method for 400 Bad Request error with validation details.
- **Unauthorized** - Factory method for 401 Unauthorized error.
- **Forbidden** - Factory method for 403 Forbidden error.
- **NotFound** - Factory method for 404 Not Found error.
- **Conflict** - Factory method for 409 Conflict error.
- **InternalServerError** - Factory method for 500 Internal Server Error.
- **Custom** - Factory method for custom problem details.

### Error

- **Namespace:** `SmartWorkz.Shared.Error`
- **Summary:** Represents a structured error with a machine-readable code and human-readable message.
            
             This is the canonical Error type. It replaces:
             - SmartWorkz.StarterKitMVC.Shared.Primitives.Error (record struct)
             - The ad-hoc string errors in Models.Result
            
             Code examples: "USER_NOT_FOUND", "VALIDATION.EMAIL_REQUIRED", "AUTH.INVALID_CREDENTIALS"
             MessageKey maps to localization resource keys for UI display.

### Result

- **Namespace:** `SmartWorkz.Shared.Result`
- **Summary:** Represents the outcome of an operation that does not return a value.
            
             This unifies:
             - SmartWorkz.StarterKitMVC.Shared.Models.Result (Succeeded + MessageKey + Errors[])
             - SmartWorkz.StarterKitMVC.Shared.Primitives.Result (IsSuccess + Error struct)
            
             Design choice — class over struct:
             1. Result<T> inherits from Result to reuse Succeeded/Errors without duplication.
                Structs cannot use inheritance this way.
             2. Services return Result from interface methods — class semantics (null check) are
                simpler than boxing/unboxing structs across interface boundaries.
             3. Errors[] supports field-level validation messages that ModelState.AddErrors() consumes.
                A single Error struct cannot carry multiple field errors.
            
             The Primitives.Result struct in StarterKitMVC.Shared remains valid for pure functions
             where you want zero-allocation returns. This class is for service layer contracts.

#### Methods & Properties

- **Fail** - Failure with a localization message key and optional field-level error strings.
- **Fail** - Failure from a structured Error (bridges the Primitives.Error pattern).
- **Ok``1** - Factory for a typed result. Use in services that return data.

### Result`1

- **Namespace:** `SmartWorkz.Shared.Result`1`
- **Summary:** Result with a typed payload. Data is only valid when Succeeded = true.
            
             Usage:
               Result<UserDto> result = await _userService.GetByIdAsync(id);
               if (!result.Succeeded) return RedirectToPage("Error");
               var user = result.Data!;

### ResultExtensions

- **Namespace:** `SmartWorkz.Shared.ResultExtensions`
- **Summary:** Functional helpers for chaining Result operations.
            Keeps service code flat — avoids nested if (!result.Succeeded) blocks.

#### Methods & Properties

- **Map``2** - Transform the Data value if the result succeeded.
- **BindAsync``2** - Chain a second operation that also returns Result.
- **OnSuccess``1** - Execute a side-effect action on success, then return the original result.
- **OnFailure``1** - Execute a side-effect action on failure, then return the original result.

### ISagaDefinition`1

- **Namespace:** `SmartWorkz.Shared.ISagaDefinition`1`
- **Summary:** Defines the blueprint for a saga orchestration.
            A saga is a pattern for managing distributed transactions and long-running processes
            by coordinating multiple steps with built-in compensation mechanisms.

#### Methods & Properties

- **DefineStep``1** - Defines a step in the saga that will be executed when a specific event type is received.
            Steps are executed sequentially in the order they were defined.
  - Parameters:
    - `handler`: The async handler function that processes the event and updates the saga state.
            Returns a StepResult indicating success or failure.
- **OnFailure** - Defines the failure handler that will be called if any step fails.
            Used for compensation logic and saga-level error handling.
  - Parameters:
    - `compensationHandler`: The async handler that receives the current saga state and the exception that occurred.
            Responsible for compensation/rollback logic.
- **BuildAsync** - Builds and returns the saga definition for execution.
            Can be used for async initialization or validation.
  - Returns: A task that completes with the configured saga definition.
- **GetSteps** - Gets the list of saga steps in execution order.
  - Returns: A read-only list of saga step handlers.
- **GetFailureHandler** - Gets the failure compensation handler if defined.
  - Returns: The failure handler function, or null if not defined.

### SagaOrchestrator

- **Namespace:** `SmartWorkz.Shared.SagaOrchestrator`
- **Summary:** Orchestrates the execution of sagas, managing step sequencing, error handling,
            and compensation/rollback logic for complex distributed processes.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the SagaOrchestrator class.
  - Parameters:
    - `logger`: Logger for saga execution tracking and debugging.
- **ExecuteSagaAsync``1** - Executes a saga definition with the provided initial state and triggering event.
            Manages step execution, error handling, and compensation logic.
  - Parameters:
    - `sagaDefinition`: The saga definition blueprint to execute.
    - `initialState`: The initial saga state.
    - `event`: The domain event triggering the saga.
    - `cancellationToken`: Optional cancellation token.
  - Returns: A task representing the saga execution.
- **ExecuteSagaStepsAsync``1** - Executes saga steps by using reflection to access internal step definitions.
- **CompensateExecutedStepsAsync``1** - Executes compensation handlers for all executed steps in reverse order.
            Uses stored compensation handlers to avoid re-executing steps.
- **ExecuteFailureHandlerAsync``1** - Executes the saga-level failure handler if one is defined.

### SagaStatus

- **Namespace:** `SmartWorkz.Shared.SagaStatus`
- **Summary:** Represents the status of a saga execution.

### SagaState

- **Namespace:** `SmartWorkz.Shared.SagaState`
- **Summary:** Base class for saga state objects.
            Provides common tracking properties for saga execution flow.

### StepResult

- **Namespace:** `SmartWorkz.Shared.StepResult`
- **Summary:** Represents the result of executing a single saga step.
            Provides success/failure status and optional compensation logic for rollback.

#### Methods & Properties

- **Success** - Creates a successful step result.
  - Returns: A StepResult indicating success.
- **Failure** - Creates a failed step result with an optional compensation handler.
  - Parameters:
    - `failureReason`: The reason for the step failure.
    - `compensationHandler`: Optional handler to compensate/rollback this step if a later step fails.
  - Returns: A StepResult indicating failure.
- **FromException** - Creates a failed step result for an exception with optional compensation.
  - Parameters:
    - `exception`: The exception that caused the failure.
    - `compensationHandler`: Optional compensation handler.
  - Returns: A StepResult indicating failure.

### CryptHelper

- **Namespace:** `SmartWorkz.Shared.CryptHelper`
- **Summary:** Provides AES-256-CBC encryption and decryption utilities with secure key and IV generation.
            
             All operations support both string and byte array inputs/outputs.
             Keys are normalized to 32 bytes (256 bits) via padding/trimming as needed.
             IVs are auto-generated if not provided and embedded in the ciphertext (IV:Ciphertext format).

#### Methods & Properties

- **EncryptString** - Encrypts plaintext using AES-256-CBC with a Base64-encoded output.
  - Parameters:
    - `plaintext`: The plaintext to encrypt.
    - `key`: The encryption key (will be normalized to 32 bytes).
    - `iv`: Optional IV; auto-generated if null.
  - Returns: A Result containing Base64-encoded ciphertext in "IV:Ciphertext" format or an error.
- **EncryptBytes** - Encrypts byte data using AES-256-CBC.
  - Parameters:
    - `plaintext`: The plaintext bytes to encrypt.
    - `key`: The encryption key (will be normalized to 32 bytes).
    - `iv`: Optional IV; auto-generated if null.
  - Returns: A Result containing encrypted bytes with embedded IV (IV || Ciphertext) or an error.
- **DecryptString** - Decrypts Base64-encoded ciphertext using AES-256-CBC.
  - Parameters:
    - `ciphertext`: The Base64-encoded ciphertext in "IV:Ciphertext" format.
    - `key`: The decryption key (will be normalized to 32 bytes).
  - Returns: A Result containing the decrypted plaintext or an error.
- **DecryptBytes** - Decrypts byte data using AES-256-CBC.
  - Parameters:
    - `ciphertext`: The encrypted bytes with embedded IV (IV || Ciphertext).
    - `key`: The decryption key (will be normalized to 32 bytes).
  - Returns: A Result containing decrypted bytes or an error.
- **GenerateKey** - Generates a random cryptographic key of the specified size.
  - Parameters:
    - `keySize`: The key size in bytes (default 32 for AES-256). Must be 16, 24, or 32.
  - Returns: A Result containing Base64-encoded random key or an error.
- **GenerateIv** - Generates a random cryptographic IV (Initialization Vector).
  - Returns: A Result containing Base64-encoded random IV or an error.
- **GenerateRandomBytes** - Generates cryptographically secure random bytes.
- **NormalizeKey** - Normalizes a key to exactly 32 bytes (256 bits).
            If the key is shorter, it's padded with zeros. If longer, it's trimmed.

### CryptOptions

- **Namespace:** `SmartWorkz.Shared.CryptOptions`
- **Summary:** Configuration options for AES cryptographic operations.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if valid, false otherwise.

### HashHelper

- **Namespace:** `SmartWorkz.Shared.HashHelper`
- **Summary:** Provides utilities for cryptographic hash operations (SHA256 and MD5).

#### Methods & Properties

- **Sha256** - Computes the SHA256 hash of a string and returns it as a hexadecimal string.
- **Sha256Bytes** - Computes the SHA256 hash of a byte array and returns the hash as a byte array.
- **Md5** - Computes the MD5 hash of a string and returns it as a hexadecimal string.
            Note: MD5 is cryptographically broken; use SHA256 for security-critical applications.
- **VerifyHash** - Verifies that a text matches its SHA256 hash.

### HmacAlgorithm

- **Namespace:** `SmartWorkz.Shared.HmacAlgorithm`
- **Summary:** Specifies the HMAC algorithm to use for message signing and verification.

### HmacHelper

- **Namespace:** `SmartWorkz.Shared.HmacHelper`
- **Summary:** Provides HMAC-SHA256/SHA512 message signing and verification for API requests and webhook verification.
            Implements constant-time comparison to prevent timing attacks.

#### Methods & Properties

- **Sign** - Signs a message using HMAC with the specified algorithm and returns a Base64-encoded hex digest.
  - Parameters:
    - `message`: The message to sign.
    - `secret`: The secret key for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing the Base64-encoded signature or an error.
- **SignBytes** - Signs a message using HMAC with the specified algorithm and returns the raw byte digest.
  - Parameters:
    - `message`: The message bytes to sign.
    - `secret`: The secret key for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing the byte signature or an error.
- **Verify** - Verifies a message signature using HMAC with constant-time comparison to prevent timing attacks.
  - Parameters:
    - `message`: The original message that was signed.
    - `signature`: The Base64-encoded signature to verify.
    - `secret`: The secret key used for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing true if the signature is valid, false if not, or an error.
- **VerifyBytes** - Verifies a message signature using HMAC with raw byte inputs and constant-time comparison.
  - Parameters:
    - `message`: The original message bytes that were signed.
    - `signature`: The byte signature to verify.
    - `secret`: The secret key used for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing true if the signature is valid, false if not, or an error.
- **SignBytes** - Internal method to compute HMAC signature from raw bytes.
- **CreateHmac** - Creates the appropriate HMAC instance based on the algorithm.

### InputSanitizer

- **Namespace:** `SmartWorkz.Shared.InputSanitizer`
- **Summary:** Input sanitization to prevent XSS, SQL injection, and path traversal attacks.

#### Methods & Properties

- **SanitizeHtml** - Sanitize HTML by removing dangerous tags and attributes.
- **EscapeHtml** - Escape HTML special characters to prevent XSS.
- **SanitizeSql** - Sanitize string to prevent SQL injection (basic, not a replacement for parameterized queries).
- **SanitizeFilePath** - Sanitize file path to prevent directory traversal attacks.
- **SanitizeUrl** - Sanitize and validate URL.
- **EscapeJson** - Escape string for safe JSON inclusion.
- **IsValidEmail** - Validate email format (basic check, server-side SMTP validation recommended).
- **RemoveControlCharacters** - Remove null bytes and control characters.

### JwtSettings

- **Namespace:** `SmartWorkz.Shared.JwtSettings`
- **Summary:** Settings for JWT token generation and validation.

#### Methods & Properties

- **Validate** - Validate settings: Secret >= 32 chars, other fields non-empty.

### JwtClaims

- **Namespace:** `SmartWorkz.Shared.JwtClaims`
- **Summary:** JWT claims that can be included in a token.

#### Methods & Properties

- **GetClaimValue** - Get claim value by type (supports standard claims + custom).

### JwtTokenValidationResult

- **Namespace:** `SmartWorkz.Shared.JwtTokenValidationResult`
- **Summary:** Result of JWT token validation.

### JwtHelper

- **Namespace:** `SmartWorkz.Shared.JwtHelper`
- **Summary:** Provides JWT token generation, validation, and refresh functionality.

#### Methods & Properties

- **GenerateTokenInternal** - Internal token generation logic shared by GenerateToken and GenerateRefreshToken.
  - Parameters:
    - `claims`: The claims to include in the token.
    - `settings`: The JWT settings for signing and configuration.
    - `isRefreshToken`: If true, uses RefreshTokenExpiryDays; otherwise uses ExpiryMinutes.
  - Returns: A Result containing the signed token or an error.
- **GenerateToken** - Generates a JWT access token with the specified claims and settings.
  - Parameters:
    - `claims`: The claims to include in the token.
    - `settings`: The JWT settings for signing and configuration.
  - Returns: A Result containing the signed token or an error.
- **ValidateToken** - Validates a JWT token and extracts claims if valid.
  - Parameters:
    - `token`: The token to validate.
    - `settings`: The JWT settings for validation.
  - Returns: A Result containing the validation result.
- **RefreshToken** - Refreshes an access token using a refresh token.
  - Parameters:
    - `refreshToken`: The refresh token.
    - `settings`: The JWT settings.
  - Returns: A Result containing the new access token or an error.
- **GenerateRefreshToken** - Generates a refresh token with extended expiry.
  - Parameters:
    - `claims`: The claims to include in the refresh token.
    - `settings`: The JWT settings.
  - Returns: A Result containing the refresh token or an error.
- **ToBase64Url** - Encodes bytes to Base64Url format (no padding, + → -, / → _).
- **FromBase64Url** - Decodes Base64Url format to bytes.

### PasswordHelper

- **Namespace:** `SmartWorkz.Shared.PasswordHelper`
- **Summary:** Provides secure password generation and validation using cryptographically secure random number generation.

#### Methods & Properties

- **GeneratePassword** - Generates a cryptographically secure random password.
  - Parameters:
    - `length`: Length of the password (8-128, default 12).
    - `includeSpecialChars`: Whether to include special characters.
  - Returns: A Result containing the generated password or an error.
- **ValidateStrength** - Validates the strength of a password against a policy.
  - Parameters:
    - `password`: The password to validate.
    - `policy`: The policy to validate against (uses default if null).
  - Returns: A Result containing the validation result.
- **GetRandomChar** - Gets a random character from the specified character set using cryptographic randomness.
- **Shuffle** - Performs Fisher-Yates shuffle on the character array.
- **CheckPasswordLength** - Checks if password meets minimum length requirement.
- **CheckUppercase** - Checks if password contains at least one uppercase letter.
- **CheckLowercase** - Checks if password contains at least one lowercase letter.
- **CheckNumbers** - Checks if password contains at least one digit.
- **CheckSpecialChars** - Checks if password contains at least one special character.

### PasswordPolicy

- **Namespace:** `SmartWorkz.Shared.PasswordPolicy`
- **Summary:** Policy for password validation requirements.

#### Methods & Properties

- **Validate** - Validates the policy invariants.

### PasswordValidationResult

- **Namespace:** `SmartWorkz.Shared.PasswordValidationResult`
- **Summary:** Result of password validation against a policy.

### ITemplateEngine

- **Namespace:** `SmartWorkz.Shared.ITemplateEngine`
- **Summary:** Defines operations for rendering templates with placeholder substitution.

#### Methods & Properties

- **Render** - Renders a template string by replacing placeholders with values from a dictionary.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `values`: Dictionary of key-value pairs to substitute into the template.
  - Returns: The rendered content with placeholders replaced. Unmatched placeholders remain unchanged.
- **Render** - Renders a template string by extracting properties from an object model.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `model`: The model object whose public properties are used for placeholder substitution.
  - Returns: The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.
- **RenderFileAsync** - Asynchronously renders a template file by replacing placeholders with values from a dictionary.
  - Parameters:
    - `filePath`: The path to the template file.
    - `values`: Dictionary of key-value pairs to substitute into the template.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **RenderFileAsync** - Asynchronously renders a template file by extracting properties from an object model.
  - Parameters:
    - `filePath`: The path to the template file.
    - `model`: The model object whose public properties are used for placeholder substitution.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **LoadDirectoryAsync** - Asynchronously loads all template files from a directory into a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.
- **RenderDirectoryAsync** - Asynchronously loads and renders all template files from a directory using values from a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `values`: Dictionary of key-value pairs to substitute into each template.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.

### TemplateEngine

- **Namespace:** `SmartWorkz.Shared.TemplateEngine`
- **Summary:** Provides template rendering services with support for placeholder substitution.

#### Methods & Properties

- **PlaceholderRegex** - 
- **Render** - Renders a template string by replacing placeholders with values from a dictionary.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `values`: Dictionary of key-value pairs to substitute into the template.
  - Returns: The rendered content with placeholders replaced. Unmatched placeholders and null/empty content remain unchanged.
- **Render** - Renders a template string by extracting properties from an object model.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `model`: The model object whose public properties are used for placeholder substitution.
  - Returns: The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.
- **RenderFileAsync** - Asynchronously renders a template file by replacing placeholders with values from a dictionary.
  - Parameters:
    - `filePath`: The path to the template file.
    - `values`: Dictionary of key-value pairs to substitute into the template.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **RenderFileAsync** - Asynchronously renders a template file by extracting properties from an object model.
  - Parameters:
    - `filePath`: The path to the template file.
    - `model`: The model object whose public properties are used for placeholder substitution.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **LoadDirectoryAsync** - Asynchronously loads all template files from a directory into a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.
- **RenderDirectoryAsync** - Asynchronously loads and renders all template files from a directory using values from a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `values`: Dictionary of key-value pairs to substitute into each template.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.
- **ReflectModel** - Reflects over a model object and builds a case-insensitive dictionary of public properties
            mapped to their string values. Uses cached property metadata for performance.
- **ValidateFilePath** - Validates a file path to prevent directory traversal attacks.
  - Parameters:
    - `filePath`: The file path to validate.
  - Returns: A result indicating if the path is valid and safe.

### CompressHelper

- **Namespace:** `SmartWorkz.Shared.CompressHelper`
- **Summary:** Provides utilities for GZip compression and decompression.

#### Methods & Properties

- **CompressString** - Compresses a string using GZip compression.
- **DecompressString** - Decompresses a GZip-compressed byte array back to a string.
- **CompressBytes** - Compresses a byte array using GZip compression.
- **DecompressBytes** - Decompresses a GZip-compressed byte array.

### DateHelper

- **Namespace:** `SmartWorkz.Shared.DateHelper`
- **Summary:** Provides utilities for date and time operations.

#### Methods & Properties

- **GetAge** - Calculates the age in years from a birth date to today.
- **GetRelativeTime** - Returns a human-readable relative time string (e.g., "2 days ago", "in 3 hours").
- **StartOfDay** - Returns the start of the day (00:00:00) for the given date.
- **EndOfDay** - Returns the end of the day (23:59:59.999) for the given date.
- **IsWeekend** - Determines if the given date falls on a weekend (Saturday or Sunday).
- **GetDayOfWeekName** - Returns the name of the day of week (e.g., "Monday", "Tuesday").
- **DaysBetween** - Calculates the number of days between two dates (inclusive of the from date, exclusive of the to date).

### EnumHelper

- **Namespace:** `SmartWorkz.Shared.EnumHelper`
- **Summary:** Provides utilities for enum operations including reflection and description retrieval.

#### Methods & Properties

- **GetDescription** - Gets the description of an enum value from its [Description] attribute.
            Falls back to the enum name if no description is found.
- **GetValue``1** - Attempts to get an enum value by its name.
- **GetAllValues``1** - Returns all values of the specified enum type as a list.
- **GetName** - Gets the name of an enum value.

### MathHelper

- **Namespace:** `SmartWorkz.Shared.MathHelper`
- **Summary:** Provides utilities for common math operations.

#### Methods & Properties

- **Percentage** - Calculates the percentage of a value.
            Example: Percentage(100, 20) returns 20 (20% of 100).
- **PercentageChange** - Calculates the percentage change from oldValue to newValue.
            Positive result indicates increase, negative indicates decrease.
- **RoundTo** - Rounds a decimal value to the specified number of decimal places.
- **Clamp``1** - Clamps a value within a specified range [min, max].
- **Average** - Calculates the average of the provided decimal values.

### SlugHelper

- **Namespace:** `SmartWorkz.Shared.SlugHelper`
- **Summary:** Helper for generating URL-friendly slugs from text input.

#### Methods & Properties

- **GenerateSlug** - Generates a URL-friendly slug from the given text with optional configuration.
  - Parameters:
    - `text`: The input text to convert to a slug.
    - `options`: Configuration options. If null, default options are used.
  - Returns: A Result containing the generated slug or an error.
- **ToSlug** - Generates a URL-friendly slug from the given text using default options.
            Convenience method equivalent to GenerateSlug(text, null).
  - Parameters:
    - `text`: The input text to convert to a slug.
  - Returns: A Result containing the generated slug or an error.
- **RemoveAccents** - Removes accented characters from text by decomposing them and filtering out combining marks.
            For example: "café" → "cafe", "naïve" → "naive", "Señor" → "Senor".
  - Parameters:
    - `input`: The input text potentially containing accented characters.
  - Returns: The text with accented characters converted to their base forms.
- **ReplaceSpecialCharacters** - Replaces special characters and spaces with the specified separator.
            Keeps only alphanumeric characters and the separator.
  - Parameters:
    - `input`: The input text.
    - `separator`: The separator to use for special characters and spaces.
  - Returns: The text with special characters replaced by the separator.

### SlugOptions

- **Namespace:** `SmartWorkz.Shared.SlugOptions`
- **Summary:** Options for configuring slug generation behavior in .

### TextHelper

- **Namespace:** `SmartWorkz.Shared.TextHelper`
- **Summary:** Sealed class providing advanced text processing and formatting utilities.
            All methods return Result<string> for consistent error handling.

#### Methods & Properties

- **Truncate** - Truncates text to a maximum length and appends a suffix (default "...").
  - Parameters:
    - `text`: The input text to truncate.
    - `maxLength`: The maximum length including the suffix.
    - `suffix`: The suffix to append when truncating. Defaults to "...".
  - Returns: A Result containing the truncated text or an error.
- **Capitalize** - Capitalizes the first letter of the text while preserving the rest.
  - Parameters:
    - `text`: The input text to capitalize.
  - Returns: A Result containing the capitalized text or an error.
- **Decapitalize** - Decapitalizes the first letter of the text while preserving the rest.
  - Parameters:
    - `text`: The input text to decapitalize.
  - Returns: A Result containing the decapitalized text or an error.
- **StripHtml** - Removes HTML tags from the input string using regex.
  - Parameters:
    - `html`: The HTML string to process.
  - Returns: A Result containing the plain text with HTML tags removed or an error.
- **Pluralize** - Pluralizes a word based on count using a simple heuristic.
            If count == 1, returns singular form. Otherwise appends 's'.
  - Parameters:
    - `singular`: The singular form of the word.
    - `count`: The count to determine plural form.
  - Returns: A Result containing the appropriately pluralized word or an error.
- **TitleCase** - Converts text to title case by capitalizing the first letter of each word.
  - Parameters:
    - `text`: The input text to convert.
  - Returns: A Result containing the title-cased text or an error.
- **Reverse** - Reverses the input string.
  - Parameters:
    - `text`: The input text to reverse.
  - Returns: A Result containing the reversed text or an error.
- **RemoveWhitespace** - Removes all whitespace characters from the input string.
  - Parameters:
    - `text`: The input text to process.
  - Returns: A Result containing the text with all whitespace removed or an error.
- **WordWrap** - Wraps text at a specified line length while preserving word boundaries.
  - Parameters:
    - `text`: The input text to wrap.
    - `lineLength`: The maximum length of each line.
    - `newline`: The newline character(s) to use. Defaults to "\n".
  - Returns: A Result containing the word-wrapped text or an error.
- **Repeat** - Repeats the input string the specified number of times.
  - Parameters:
    - `text`: The input text to repeat.
    - `count`: The number of times to repeat the text.
  - Returns: A Result containing the repeated text or an error.

### CompositeValidator`1

- **Namespace:** `SmartWorkz.Shared.CompositeValidator`1`
- **Summary:** Combines multiple validators into a single validator.
            Useful for composing validators from different sources.

### IValidationRule`2

- **Namespace:** `SmartWorkz.Shared.IValidationRule`2`
- **Summary:** Single validation rule for a property.

#### Methods & Properties

- **ValidateAsync** - Validate property and return results.

### ValidationRule`2

- **Namespace:** `SmartWorkz.Shared.ValidationRule`2`
- **Summary:** Base implementation for custom validation rules.

### ValidationRules

- **Namespace:** `SmartWorkz.Shared.ValidationRules`
- **Summary:** Pre-built validation rules for common scenarios.

### ValidatorBuilder`1

- **Namespace:** `SmartWorkz.Shared.ValidatorBuilder`1`
- **Summary:** Fluent validator builder for defining validation rules.
            Provides an alternative to ValidatorBase for more concise validator definitions.

#### Methods & Properties

- **RuleFor``1** - Add a rule for a property using fluent API.
- **ValidateAsync** - Validate instance against all rules.

### IWebhookRegistry

- **Namespace:** `SmartWorkz.Shared.IWebhookRegistry`
- **Summary:** Abstraction for managing webhook subscriptions and registrations.
            Supports CRUD operations and subscription queries.

#### Methods & Properties

- **RegisterAsync** - Register a new webhook subscription.
  - Parameters:
    - `url`: The webhook endpoint URL.
    - `events`: Array of event names to subscribe to.
    - `secret`: Optional HMAC-SHA256 secret for signature verification.
    - `cancellationToken`: Cancellation token.
  - Returns: The ID of the newly registered subscription.
- **UnregisterAsync** - Unregister and remove a webhook subscription.
  - Parameters:
    - `subscriptionId`: The subscription ID to unregister.
    - `cancellationToken`: Cancellation token.
- **GetSubscriptionsForEventAsync** - Get all active subscriptions for a specific event.
  - Parameters:
    - `eventName`: The event name to filter by.
    - `cancellationToken`: Cancellation token.
  - Returns: Collection of subscriptions interested in this event.
- **GetActiveSubscriptionsAsync** - Get all currently active subscriptions.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: Collection of all active subscriptions.
- **UpdateSubscriptionStatusAsync** - Update the status and failure tracking of a subscription.
  - Parameters:
    - `subscriptionId`: The subscription ID to update.
    - `isActive`: Whether the subscription should remain active.
    - `failureCount`: Number of consecutive failures (null to leave unchanged).
    - `failureReason`: Reason for failure (null to clear).
    - `cancellationToken`: Cancellation token.

### SqlWebhookRegistry

- **Namespace:** `SmartWorkz.Shared.SqlWebhookRegistry`
- **Summary:** SQL Server implementation of IWebhookRegistry.
            Persists webhook subscriptions to the database with support for querying and status updates.

### WebhookDeliveryService

- **Namespace:** `SmartWorkz.Shared.WebhookDeliveryService`
- **Summary:** Service for publishing domain events to registered webhook endpoints.
            Implements exponential backoff retry logic, HMAC signature verification, and failure tracking.

#### Methods & Properties

- **PublishEventAsync** - Publish an event to all subscribed webhook endpoints.
  - Parameters:
    - `eventName`: The name of the event being published.
    - `payload`: The event payload to send.
    - `cancellationToken`: Cancellation token.
- **DeliverAsync** - Deliver an event to a single webhook endpoint with exponential backoff retry logic.
- **GenerateSignature** - Generate HMAC-SHA256 signature for webhook payload verification.

### AuditEntry

- **Namespace:** `SmartWorkz.Shared.AuditEntry`
- **Summary:** Immutable audit log entry for tracking entity changes and domain events.
            Records who did what, when, where, and why for compliance and debugging.

### AuditEventSubscriber

- **Namespace:** `SmartWorkz.Shared.AuditEventSubscriber`
- **Summary:** Subscribes to domain events and records them in the audit trail.
            Enables automatic audit capture without requiring explicit audit calls in business logic.

#### Methods & Properties

- **OnEventPublishedAsync** - Record a domain event in the audit trail.
  - Parameters:
    - `evt`: The domain event to record.
    - `userId`: User ID who triggered the event (optional for system events).
    - `ipAddress`: IP address of the request originator (optional).
    - `cancellationToken`: Cancellation token.

### AuditStartupExtensions

- **Namespace:** `SmartWorkz.Shared.AuditStartupExtensions`
- **Summary:** Dependency injection and schema setup for audit trail functionality.

#### Methods & Properties

- **AddAuditTrail** - Register IAuditTrail with SQL Server implementation.
- **CreateAuditTrailSchema** - Create the AuditTrail table and indexes if they don't exist.
            Call this during application startup or migration.

### IAuditTrail

- **Namespace:** `SmartWorkz.Shared.IAuditTrail`
- **Summary:** Service for recording and querying immutable audit entries.
            Abstracts the persistence mechanism for audit trails.

#### Methods & Properties

- **RecordAsync** - Record an audit entry (immutable append-only).
  - Parameters:
    - `entry`: The audit entry to record.
    - `cancellationToken`: Cancellation token.
- **GetEntriesAsync** - Get all audit entries for a specific entity instance.
  - Parameters:
    - `entityType`: Type of entity (e.g., "Order").
    - `entityId`: Entity instance ID.
    - `cancellationToken`: Cancellation token.
- **GetEntriesByActionAsync** - Get audit entries by action type (Created, Updated, Deleted, etc.).
  - Parameters:
    - `action`: The action to filter by.
    - `since`: Optional: only entries after this date.
    - `cancellationToken`: Cancellation token.
- **GetEntriesByUserAsync** - Get audit entries for a specific user.
  - Parameters:
    - `userId`: User ID who performed actions.
    - `since`: Optional: only entries after this date.
    - `cancellationToken`: Cancellation token.
- **SearchAsync** - Search audit trail with multiple filter criteria.
            All criteria are AND'd together (null criteria are ignored).
  - Parameters:
    - `entityType`: Optional entity type filter.
    - `action`: Optional action filter.
    - `userId`: Optional user ID filter.
    - `since`: Optional timestamp filter (inclusive).
    - `cancellationToken`: Cancellation token.

### SqlAuditTrail

- **Namespace:** `SmartWorkz.Shared.SqlAuditTrail`
- **Summary:** SQL Server implementation of IAuditTrail for immutable audit log persistence.
            Appends audit entries to a single table with indexes for efficient querying.

#### Methods & Properties

- **RecordAsync** - 
- **GetEntriesAsync** - 
- **GetEntriesByActionAsync** - 
- **GetEntriesByUserAsync** - 
- **SearchAsync** - 

### ValueConverter`1

- **Namespace:** `SmartWorkz.Shared.ValueConverter`1`
- **Summary:** Abstract base class for type conversion between domain objects and DTOs.
            Enables loose coupling between layers by centralizing conversion logic.

#### Methods & Properties

- **Convert``1** - Convert a single source object to target type.
- **Convert** - Convert a single source object using dynamic target type resolution.
- **ConvertList``1** - Convert a collection of source objects to target type.
- **ConvertList** - Convert a collection using dynamic target type resolution.
- **ConvertFromList``2** - Convert from a collection of different source types.

### CacheEntry`1

- **Namespace:** `SmartWorkz.Shared.CacheEntry`1`
- **Summary:** Represents a cached entry with data, expiration time, and metadata.

#### Methods & Properties

- **#ctor** - Creates a new CacheEntry instance.
- **#ctor** - Creates a new CacheEntry instance with data and expiration.
- **RenewExpiry** - Renews the expiry time based on the cache strategy and TTL.

### CacheEntryWrapper

- **Namespace:** `SmartWorkz.Shared.CacheEntryWrapper`
- **Summary:** Non-generic wrapper for CacheEntry to store in the cache dictionary.

### CacheOptions

- **Namespace:** `SmartWorkz.Shared.CacheOptions`
- **Summary:** Configuration options for cache operations.

#### Methods & Properties

- **#ctor** - Creates a new CacheOptions instance with default values.
- **#ctor** - Creates a new CacheOptions instance with specified TTL.
- **#ctor** - Creates a new CacheOptions instance with specified TTL and cache strategy.
- **#ctor** - Creates a new CacheOptions instance with all parameters.

### CacheStrategy

- **Namespace:** `SmartWorkz.Shared.CacheStrategy`
- **Summary:** Enumeration of cache expiration strategies.

### ICacheService

- **Namespace:** `SmartWorkz.Shared.ICacheService`
- **Summary:** Service for caching with tenant isolation and L1/L2 hybrid support.
            Implementations may use memory cache (L1) and distributed cache (L2).
            All cache operations are tenant-scoped with automatic key prefixing.

#### Methods & Properties

- **GetAsync``1** - Gets a cached value by key with tenant isolation.
  - Parameters:
    - `key`: Cache key (will be tenant-scoped automatically).
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Result containing cached value, or failure if not found or expired.
- **SetAsync``1** - Sets a value in cache with optional TTL for the specified tenant.
  - Parameters:
    - `key`: Cache key (will be tenant-scoped automatically).
    - `value`: Value to cache.
    - `ttlMinutes`: Time to live in minutes. If null, value never expires.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveAsync** - Removes a cached value by key for the specified tenant.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveByPrefixAsync** - Removes all cached values matching a key prefix for the specified tenant.
            Example: RemoveByPrefixAsync("user:") removes all "user:*" entries for that tenant.
  - Parameters:
    - `prefix`: Key prefix to match (may include wildcard suffix like "user:*").
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **ExistsAsync** - Checks if a key exists in cache for the specified tenant.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: True if key exists and is not expired; false otherwise.
- **ClearAsync** - Clears all cache entries for the specified tenant.
            Does not affect entries for other tenants.
  - Parameters:
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.

### ICacheStore

- **Namespace:** `SmartWorkz.Shared.ICacheStore`
- **Summary:** Abstraction for a cache store with support for various operations including TTL and expiration strategies.

#### Methods & Properties

- **GetAsync``1** - Retrieves a value from the cache.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result containing the cached value or null if not found or expired.
- **SetAsync``1** - Sets a value in the cache with optional TTL.
  - Parameters:
    - `key`: The cache key.
    - `value`: The value to cache.
    - `ttlMinutes`: Optional time-to-live in minutes. If null, uses default or no expiration.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **SetAsync``1** - Sets a value in the cache with cache options.
  - Parameters:
    - `key`: The cache key.
    - `value`: The value to cache.
    - `options`: Cache options including TTL, strategy, and sliding expiration.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **RemoveAsync** - Removes a value from the cache.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **RemoveByPrefixAsync** - Removes all values from the cache that match the specified key prefix.
  - Parameters:
    - `keyPrefix`: The prefix to match.
    - `ct`: Cancellation token.
  - Returns: A Result containing the number of entries removed.
- **ExistsAsync** - Checks if a key exists in the cache and is not expired.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result indicating whether the key exists and is valid.
- **ClearAsync** - Clears all entries from the cache.
  - Parameters:
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.

### MemoryCacheService

- **Namespace:** `SmartWorkz.Shared.MemoryCacheService`
- **Summary:** In-memory L1 cache service implementation with thread-safe operations and tenant isolation.
            Suitable for single-process deployments with TTL and expiration support.

#### Methods & Properties

- **BuildKey** - Builds a tenant-scoped cache key.
  - Parameters:
    - `key`: Original cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
  - Returns: Tenant-scoped key in format "{tenantId}:{key}".
- **GetAsync``1** - Gets a cached value by key with tenant isolation. Returns failure if not found or expired.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Result containing cached value, or failure if not found or expired.
- **SetAsync``1** - Sets a cached value with optional TTL expiration and tenant isolation.
  - Parameters:
    - `key`: Cache key.
    - `value`: Value to cache.
    - `ttlMinutes`: Time to live in minutes. If null, no expiration.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveAsync** - Removes a single cache entry with tenant isolation.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveByPrefixAsync** - Removes all cache entries matching a prefix pattern with tenant isolation.
            Example: RemoveByPrefixAsync("user:*", "tenant1") removes "tenant1:user:*" entries.
  - Parameters:
    - `prefix`: Key prefix to match.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **ExistsAsync** - Checks if a key exists in the cache with tenant isolation (ignores expiration check).
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: True if key exists and is not expired; false otherwise.
- **ClearAsync** - Clears all cache entries for the specified tenant (or "default" if not specified).
            Does not clear entries from other tenants.
  - Parameters:
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.

### MemoryCacheStore

- **Namespace:** `SmartWorkz.Shared.MemoryCacheStore`
- **Summary:** In-memory implementation of ICacheStore with TTL support and thread-safe operations.

#### Methods & Properties

- **#ctor** - Creates a new instance of MemoryCacheStore with default options.
- **#ctor** - Creates a new instance of MemoryCacheStore with specified default options.
- **GetAsync``1** - Retrieves a value from the cache.
- **SetAsync``1** - Sets a value in the cache with optional TTL.
- **SetAsync``1** - Sets a value in the cache with cache options.
- **RemoveAsync** - Removes a value from the cache.
- **RemoveByPrefixAsync** - Removes all values from the cache that match the specified key prefix.
- **ExistsAsync** - Checks if a key exists in the cache and is not expired.
- **ClearAsync** - Clears all entries from the cache.
- **CleanupExpiredEntries** - Performs cleanup of expired entries. This is useful for periodic maintenance.

### ISmsService

- **Namespace:** `SmartWorkz.Shared.ISmsService`
- **Summary:** Defines a contract for SMS communication services.
            Provides methods for sending SMS messages to single or multiple recipients.

#### Methods & Properties

- **SendAsync** - Sends an SMS message to a single recipient.
  - Parameters:
    - `phoneNumber`: The recipient phone number (E.164 format recommended)
    - `message`: The SMS message content
    - `cancellationToken`: Cancellation token
  - Returns: Result containing the SMS ID if successful
- **SendBatchAsync** - Sends an SMS message to multiple recipients (batch).
  - Parameters:
    - `phoneNumbers`: Collection of recipient phone numbers
    - `message`: The SMS message content sent to all recipients
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of SMS IDs if successful

### IWebSocketClient

- **Namespace:** `SmartWorkz.Shared.IWebSocketClient`
- **Summary:** Abstraction for WebSocket client operations.

#### Methods & Properties

- **SendAsync** - Sends a message asynchronously through the WebSocket connection.
- **ReceiveAsync** - Receives a message asynchronously from the WebSocket connection.
            Returns null if the connection is closed.
- **CloseAsync** - Closes the WebSocket connection gracefully.

### WebSocketClient

- **Namespace:** `SmartWorkz.Shared.WebSocketClient`
- **Summary:** Sealed implementation of IWebSocketClient using System.Net.WebSockets.

#### Methods & Properties

- **ConnectAsync** - Connects to a WebSocket server at the specified URI.
- **SendAsync** - Sends a message asynchronously through the WebSocket connection.
- **ReceiveAsync** - Receives a message asynchronously from the WebSocket connection.
            Returns null if the connection is closed.
- **CloseAsync** - Closes the WebSocket connection gracefully.

### ConfigurationHelper

- **Namespace:** `SmartWorkz.Shared.ConfigurationHelper`
- **Summary:** Provides typed access to configuration values with automatic type conversion and validation.
            
             This sealed class implements IConfigurationHelper to provide a strongly-typed interface
             for accessing configuration values. It supports automatic type conversion for common types
             including strings, numeric types, booleans, DateTimes, and enums.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ConfigurationHelper class.
  - Parameters:
    - `configuration`: The configuration source to read from.
- **GetRequired``1** - Gets a required configuration value and converts it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: The configuration value converted to type T.
- **GetOptional``1** - Gets an optional configuration value and converts it to the specified type,
            returning a default value if the key is not found or conversion fails.
  - Parameters:
    - `key`: The configuration key to retrieve.
    - `defaultValue`: The value to return if the key is not found or conversion fails.
  - Returns: The configuration value converted to type T, or the default value if retrieval or conversion fails.
- **TryGet``1** - Attempts to get a configuration value and convert it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: A Result<T> containing the converted value on success, or a failure result
            if the key is not found or conversion fails.
- **Exists** - Checks whether a configuration key exists and is not empty.
  - Parameters:
    - `key`: The configuration key to check.
  - Returns: True if the key exists and is not null or whitespace; otherwise false.
- **ConvertValue``1** - Converts a string value to the specified type using invariant culture for numeric types.
  - Parameters:
    - `value`: The string value to convert.
  - Returns: The converted value of type T.

### ConfigurationValidationException

- **Namespace:** `SmartWorkz.Shared.ConfigurationValidationException`
- **Summary:** Exception thrown when configuration validation fails, indicating that a required
            configuration key is missing, empty, or cannot be converted to the requested type.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ConfigurationValidationException class with a specified message.
  - Parameters:
    - `message`: The error message that explains the reason for the exception.
- **#ctor** - Initializes a new instance of the ConfigurationValidationException class with a specified message
            and a reference to the inner exception that is the cause of this exception.
  - Parameters:
    - `message`: The error message that explains the reason for the exception.
    - `innerException`: The exception that is the cause of the current exception.

### IConfigurationHelper

- **Namespace:** `SmartWorkz.Shared.IConfigurationHelper`
- **Summary:** Provides typed access to configuration values with automatic type conversion and validation.

#### Methods & Properties

- **GetRequired``1** - Gets a required configuration value and converts it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: The configuration value converted to type T.
- **GetOptional``1** - Gets an optional configuration value and converts it to the specified type,
            returning a default value if the key is not found or conversion fails.
  - Parameters:
    - `key`: The configuration key to retrieve.
    - `defaultValue`: The value to return if the key is not found or conversion fails.
  - Returns: The configuration value converted to type T, or the default value if retrieval or conversion fails.
- **TryGet``1** - Attempts to get a configuration value and convert it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: A Result<T> containing the converted value on success, or a failure result
            if the key is not found or conversion fails.
- **Exists** - Checks whether a configuration key exists and is not empty.
  - Parameters:
    - `key`: The configuration key to check.
  - Returns: True if the key exists and is not null or whitespace; otherwise false.

### SharedConstants

- **Namespace:** `SmartWorkz.Shared.SharedConstants`
- **Summary:** Shared configuration constants used throughout SmartWorkz.Shared.
            Enables centralized management of default values and limits.

### ICommand

- **Namespace:** `SmartWorkz.Shared.ICommand`
- **Summary:** Marker interface for command objects representing intent to change state.

### ICommandHandler`1

- **Namespace:** `SmartWorkz.Shared.ICommandHandler`1`
- **Summary:** Handler for processing a specific command type.

#### Methods & Properties

- **HandleAsync** - Handles the specified command asynchronously.
  - Parameters:
    - `command`: The command to handle.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task that represents the asynchronous handle operation.

### IQuery`1

- **Namespace:** `SmartWorkz.Shared.IQuery`1`
- **Summary:** Marker interface for query objects that return a result without modifying state.

### IQueryHandler`2

- **Namespace:** `SmartWorkz.Shared.IQueryHandler`2`
- **Summary:** Handler for processing a specific query type and returning results.

#### Methods & Properties

- **HandleAsync** - Handles the specified query asynchronously and returns the result.
  - Parameters:
    - `query`: The query to handle.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task that represents the asynchronous handle operation and contains the query result.

### MediatorCommandDispatcher

- **Namespace:** `SmartWorkz.Shared.MediatorCommandDispatcher`
- **Summary:** Routes commands to their appropriate handlers via dependency injection.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `serviceProvider`: The service provider for resolving handlers.
- **DispatchAsync``1** - Dispatches the specified command to its handler asynchronously.
  - Parameters:
    - `command`: The command to dispatch.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task representing the asynchronous dispatch operation.

### AdoHelper

- **Namespace:** `SmartWorkz.Shared.AdoHelper`
- **Summary:** ADO.NET helper for executing queries and managing connections.
            Works with any IDbProvider implementation.

#### Methods & Properties

- **ExecuteScalarAsync``1** - Execute scalar query (returns single value).
- **ExecuteNonQueryAsync** - Execute non-query command (INSERT, UPDATE, DELETE).
- **ExecuteQueryAsync``1** - Execute query and map results to objects.
- **ExecuteStoredProcedureAsync** - Execute stored procedure.
- **ExecuteQueryMultipleAsync``2** - Execute query returning multiple result sets (2 sets).
- **ExecuteQueryMultipleAsync``3** - Execute query returning multiple result sets (3 sets).
- **ExecuteQueryMultipleAsync``4** - Execute query returning multiple result sets (4 sets).
- **ExecuteTransactionAsync** - Execute transaction with multiple commands.

### CsvHelper

- **Namespace:** `SmartWorkz.Shared.CsvHelper`
- **Summary:** Provides static methods for reading and writing CSV data with support for column mapping,
            quoted fields, embedded delimiters, and newlines.
            RFC 4180 compliant CSV parsing and writing.
            Sealed class to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **CsvWriter``1** - Serializes a collection of objects to CSV format.
  - Parameters:
    - `items`: The collection of objects to serialize.
    - `options`: CSV options. If null, defaults are used.
  - Returns: A Result containing the CSV string if successful; otherwise a failure.
- **CsvReader``1** - Asynchronously deserializes CSV content to a collection of objects.
  - Parameters:
    - `content`: The CSV content string.
    - `mapping`: Column mapping configuration. If null, property names are used as headers.
    - `options`: CSV options. If null, defaults are used.
  - Returns: A Result containing the deserialized list if successful; otherwise a failure.
- **ParseCsvLines** - Parses CSV content into a list of records (each record is a list of field values).
            Handles quoted fields with embedded delimiters and newlines.
- **WriteRecord** - Writes a single CSV record (list of field values) to the string builder.
            Handles quoting of fields with special characters.
- **ConvertValue** - Converts a string value to the specified type.
- **IsNullableType** - Determines if a type is nullable (Nullable<T> or reference type).

### CsvMapping`1

- **Namespace:** `SmartWorkz.Shared.CsvMapping`1`
- **Summary:** Defines column mapping for CSV operations using a fluent API.
            Supports mapping object properties to CSV columns with custom headers.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **Column``1** - Adds a column mapping for the specified property.
  - Parameters:
    - `propertyExpression`: Expression selecting the property to map.
    - `csvHeader`: The CSV column header name.
  - Returns: This instance for method chaining.
- **ExtractPropertyInfo``1** - Extracts property information from a lambda expression.
  - Parameters:
    - `expression`: The lambda expression.
  - Returns: The PropertyInfo if the expression resolves to a property; otherwise null.
- **CreateAuto** - Creates a mapping automatically from all public properties of type T.
            Property names are used as CSV headers.
  - Returns: A new CsvMapping instance with all properties mapped.

### CsvOptions

- **Namespace:** `SmartWorkz.Shared.CsvOptions`
- **Summary:** Configuration options for CSV read/write operations.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **#ctor** - Creates a default instance of CsvOptions.
- **#ctor** - Creates an instance of CsvOptions with specified delimiter and quote character.
  - Parameters:
    - `delimiter`: The field delimiter character.
    - `quoteChar`: The quote character for quoted fields.

### DbProviderFactory

- **Namespace:** `SmartWorkz.Shared.DbProviderFactory`
- **Summary:** Factory for creating database provider instances.
            Resolves provider name from connection string or explicit specification.

#### Methods & Properties

- **Register** - Register custom provider implementation.
- **GetProvider** - Get provider by name.
- **GetProvider** - Get provider by enum value.
- **GetProviderFromConnectionString** - Get provider from connection string (detects provider automatically).

### IDbProvider

- **Namespace:** `SmartWorkz.Shared.IDbProvider`
- **Summary:** Abstraction for database provider-specific operations.
            Supports multiple providers: SQL Server, MySQL, PostgreSQL, SQLite, Oracle.

#### Methods & Properties

- **CreateConnection** - Create connection with connection string.
- **GetParameterPrefix** - Get parameter prefix for this provider (@, :, $).
- **GetLastInsertIdSql** - Get SQL for last inserted ID based on provider.
- **GetPaginationSql** - Get SQL for pagination based on provider.
- **FormatIdentifier** - Format table/column name for provider (e.g., [brackets] for SQL Server).
- **TestConnectionAsync** - Test connection validity.

### DatabaseProvider

- **Namespace:** `SmartWorkz.Shared.DatabaseProvider`
- **Summary:** Enum of supported database providers.

### QueryMultipleHelper

- **Namespace:** `SmartWorkz.Shared.QueryMultipleHelper`
- **Summary:** Helper for executing multiple queries in a single database roundtrip.
            Eliminates N+1 query problems by batching queries together.

#### Methods & Properties

- **QueryMultipleAsync``2** - Execute multiple queries and return results as tuple.
             Single roundtrip, single SQL execution, improved performance.
- **QueryMultipleAsync``3** - Execute 3 queries in single roundtrip.
- **QueryMultipleAsync``4** - Execute 4 queries in single roundtrip.
- **QueryMultipleAsync``5** - Execute 5 queries in single roundtrip.

### QueryResult`1

- **Namespace:** `SmartWorkz.Shared.QueryResult`1`
- **Summary:** Result wrapper for query operations.

### XmlHelper

- **Namespace:** `SmartWorkz.Shared.XmlHelper`
- **Summary:** Provides static methods for XML serialization, deserialization, and XPath queries.
            Uses System.Xml.Linq for manipulation and reflection for property mapping.
            Sealed class to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **Serialize``1** - Serializes an object to an XML string using reflection.
  - Parameters:
    - `obj`: The object to serialize.
    - `options`: XML options. If null, defaults are used.
  - Returns: A Result containing the XML string if successful; otherwise a failure.
- **Deserialize``1** - Deserializes an XML string to an object of type T using reflection.
  - Parameters:
    - `xml`: The XML string to deserialize.
    - `options`: XML options. If null, defaults are used.
  - Returns: A Result containing the deserialized object if successful; otherwise a failure.
- **Query** - Executes an XPath query on an XML string and returns matching element values.
  - Parameters:
    - `xml`: The XML string to query.
    - `xpathExpression`: The XPath expression to execute.
  - Returns: A Result containing a list of matched values if successful; otherwise a failure.
- **SerializeObject** - Recursively serializes an object's properties into an XML element.
- **DeserializeObject** - Recursively deserializes an XML element into an object's properties.
- **IsBasicType** - Determines if a type is a basic/primitive type supported by XML.
- **IsGenericList** - Determines if a type is a generic List<T>.
- **IsComplexType** - Determines if a type is a complex (non-primitive) type.
- **ConvertToXmlValue** - Converts a value to its XML-safe string representation.
- **ConvertFromXmlValue** - Converts an XML string value to the specified type.

### XmlOptions

- **Namespace:** `SmartWorkz.Shared.XmlOptions`
- **Summary:** Configuration options for XML serialization, deserialization, and query operations.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **#ctor** - Creates a default instance of XmlOptions.
- **#ctor** - Creates an instance of XmlOptions with a specified root element name.
  - Parameters:
    - `rootElement`: The name of the root element.
- **#ctor** - Creates an instance of XmlOptions with specified configuration.
  - Parameters:
    - `rootElement`: The name of the root element.
    - `includeXmlDeclaration`: Whether to include the XML declaration.
    - `indent`: Whether to indent the output.

### ApplicationHealth

- **Namespace:** `SmartWorkz.Shared.ApplicationHealth`
- **Summary:** Represents the overall health status of the application.

### CorrelationContext

- **Namespace:** `SmartWorkz.Shared.CorrelationContext`
- **Summary:** A sealed implementation of  for distributed request tracing.

#### Methods & Properties

- **#ctor** - Initializes a new instance with a generated correlation ID.
- **#ctor** - Initializes a new instance with a specified correlation ID.
  - Parameters:
    - `correlationId`: The correlation ID to use
- **#ctor** - Initializes a child context from a parent context.

### CpuUsage

- **Namespace:** `SmartWorkz.Shared.CpuUsage`
- **Summary:** Represents CPU usage information.

### DiagnosticsHelper

- **Namespace:** `SmartWorkz.Shared.DiagnosticsHelper`
- **Summary:** Sealed helper class for system diagnostics and application health monitoring.
            Provides methods to gather system information, CPU/memory/disk usage, and determine application health.

#### Methods & Properties

- **Initialize** - Initializes the application start time (called once at application startup).
- **GetSystemInfo** - Gets comprehensive system information including CPU, memory, disk, and processor count.
  - Returns: A Result containing SystemInfo or error details.
- **GetMemoryUsage** - Gets memory usage statistics for the current process and system.
  - Returns: A Result containing MemoryUsage or error details.
- **GetCpuUsage** - Gets CPU utilization percentage.
  - Returns: A Result containing CpuUsage or error details.
- **GetDiskSpace** - Gets disk space information for a specific drive.
  - Parameters:
    - `drive`: The drive letter (e.g., "C:", "D:"). Defaults to "C:".
  - Returns: A Result containing DiskSpace or error details.
- **GetUptime** - Gets the application uptime since the last Initialize() call or application start.
  - Returns: A Result containing the uptime as a TimeSpan or error details.
- **GetApplicationHealth** - Gets the overall health status of the application based on system metrics.
  - Returns: A Result containing ApplicationHealth or error details.
- **IsHealthy** - Determines if the application is considered healthy based on the provided health status.
  - Parameters:
    - `health`: The ApplicationHealth object to evaluate.
  - Returns: True if the status is Healthy, false otherwise.
- **InitializeCpuCounter** - Initializes the CPU performance counter (called once).
- **GetMemoryUsageInternal** - Internal method to get memory usage statistics.
- **GetCpuUsageInternal** - Internal method to get CPU usage percentage.
- **GetDiskSpaceInternal** - Internal method to get disk space information.

### DiskSpace

- **Namespace:** `SmartWorkz.Shared.DiskSpace`
- **Summary:** Represents disk space information for a drive.

### HealthCheck

- **Namespace:** `SmartWorkz.Shared.HealthCheck`
- **Summary:** Represents a single health check result.

### HealthStatus

- **Namespace:** `SmartWorkz.Shared.HealthStatus`
- **Summary:** Represents the health status of the application.

### ICorrelationContext

- **Namespace:** `SmartWorkz.Shared.ICorrelationContext`
- **Summary:** Defines a correlation context for distributed request tracing across systems.

#### Methods & Properties

- **SetProperty** - Adds or updates a property in the correlation context.
- **TryGetProperty** - Attempts to retrieve a property from the correlation context.
- **CreateChildContext** - Creates a child correlation context for nested operations (for async/distributed flows).

### MemoryUsage

- **Namespace:** `SmartWorkz.Shared.MemoryUsage`
- **Summary:** Represents memory usage information.

### MetricsHelper

- **Namespace:** `SmartWorkz.Shared.MetricsHelper`
- **Summary:** Provides utilities for collecting and tracking performance metrics.

#### Methods & Properties

- **StartTimer** - Starts a timer and returns an IDisposable that logs elapsed time on disposal.
- **TrackExecution``1** - Tracks the execution time and result of a function.
- **MeasureMemory** - Captures memory usage before and after a block of code execution.

### SystemInfo

- **Namespace:** `SmartWorkz.Shared.SystemInfo`
- **Summary:** Represents system information including CPU, memory, and disk details.

### EventStoreSnapshot

- **Namespace:** `SmartWorkz.Shared.EventStoreSnapshot`
- **Summary:** Represents a snapshot of an aggregate's state at a specific version.
            Snapshots optimize event sourcing by reducing the number of events needed for reconstruction.

### IEventStore

- **Namespace:** `SmartWorkz.Shared.IEventStore`
- **Summary:** Abstraction for an immutable event store that persists domain events.
            Enables event sourcing patterns for temporal queries, audit trails, and event replay.

#### Methods & Properties

- **AppendEventsAsync** - Appends events to the event stream for an aggregate.
            Events are immutable and persist as an append-only log.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `events`: The domain events to append
    - `cancellationToken`: Cancellation token
  - Returns: Task representing the asynchronous operation
- **GetEventsAsync** - Retrieves all events for an aggregate in chronological order.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: Collection of domain events for the aggregate, empty if none exist
- **GetEventsSinceAsync** - Retrieves events for an aggregate after a specific version.
            Useful for incremental event replay and event streaming.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `version`: The version after which to retrieve events
    - `cancellationToken`: Cancellation token
  - Returns: Collection of events after the specified version, empty if none exist
- **GetSnapshotAsync** - Retrieves the latest snapshot for an aggregate if one exists.
            Snapshots optimize aggregate reconstruction by storing intermediate state.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: Snapshot data if exists; null if no snapshot is available
- **SaveSnapshotAsync** - Saves a snapshot of aggregate state at a specific version.
            Snapshots reduce the number of events needed to replay an aggregate.
  - Parameters:
    - `snapshot`: The snapshot to save
    - `cancellationToken`: Cancellation token
  - Returns: Task representing the asynchronous operation
- **GetAggregateAsync``1** - Reconstructs an aggregate from its event history.
            Optionally uses snapshots for performance optimization.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: The reconstructed aggregate instance, or null if no events exist

### SqlEventStore

- **Namespace:** `SmartWorkz.Shared.SqlEventStore`
- **Summary:** SQL Server implementation of the event store using Dapper for data access.
            Provides immutable append-only event log with snapshot support for optimization.
            Implements optimistic concurrency control using version numbers.

#### Methods & Properties

- **AppendEventsAsync** - Appends events to the event stream for an aggregate.
- **GetEventsAsync** - Retrieves all events for an aggregate in chronological order.
- **GetEventsSinceAsync** - Retrieves events for an aggregate after a specific version.
- **GetSnapshotAsync** - Retrieves the latest snapshot for an aggregate if one exists.
- **SaveSnapshotAsync** - Saves a snapshot of aggregate state at a specific version.
- **GetAggregateAsync``1** - Reconstructs an aggregate from its event history.
            Optionally uses snapshots for performance optimization.
- **GetCurrentVersionAsync** - Gets the current version number for an aggregate.
- **DeserializeEvent** - Deserializes a stored event record back to IDomainEvent.

### IDomainEvent

- **Namespace:** `SmartWorkz.Shared.IDomainEvent`
- **Summary:** Base interface for domain events in event-driven architecture.
            Provides core event metadata for tracking and publishing.

### IEventPublisher

- **Namespace:** `SmartWorkz.Shared.IEventPublisher`
- **Summary:** Publishes domain events for event-driven architecture.

#### Methods & Properties

- **PublishAsync``1** - Publishes a single domain event.
  - Parameters:
    - `event`: Event to publish.
    - `cancellationToken`: Cancellation token.
- **PublishAsync``1** - Publishes multiple domain events.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token.

### IEventSubscriber

- **Namespace:** `SmartWorkz.Shared.IEventSubscriber`
- **Summary:** Registers event handlers for domain events.

#### Methods & Properties

- **Subscribe``1** - Subscribes a handler to an event type.
            Multiple handlers can subscribe to the same event.
  - Parameters:
    - `handler`: Async handler function. Receives event and cancellation token.

### InMemoryEventPublisher

- **Namespace:** `SmartWorkz.Shared.InMemoryEventPublisher`
- **Summary:** In-memory event publisher that executes all registered handlers sequentially.
            Provides synchronous event delivery with exception handling and result reporting.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the InMemoryEventPublisher with a subscriber.
  - Parameters:
    - `subscriber`: The event subscriber containing registered handlers.
- **PublishAsync``1** - Publishes a single domain event to all registered handlers.
            Handlers are invoked sequentially in registration order.
            If any handler throws an exception, it is caught and a failure Result is returned.
            Other handlers will attempt to execute even if a previous handler fails.
  - Parameters:
    - `event`: The event instance to publish.
    - `cancellationToken`: Cancellation token to cancel handler execution.
  - Returns: A task representing the asynchronous operation.
- **PublishAsync``1** - Publishes multiple domain events to all registered handlers.
            Events are published sequentially in the order provided.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token to cancel handler execution.
  - Returns: A task representing the asynchronous batch operation.

### InMemoryEventSubscriber

- **Namespace:** `SmartWorkz.Shared.InMemoryEventSubscriber`
- **Summary:** In-memory event subscriber that maintains a registry of event handlers.
            Supports multiple handlers per event type using thread-safe concurrent collections.

#### Methods & Properties

- **Subscribe``1** - Subscribes a handler to an event type.
            Multiple handlers can be registered for the same event type and will execute sequentially.
  - Parameters:
    - `handler`: Async handler function that receives event and cancellation token.
- **GetHandlers** - Gets all registered handlers for a given event type.
            Returns an empty list if no handlers are registered for the type.
  - Parameters:
    - `eventType`: The event type to retrieve handlers for.
  - Returns: List of registered handlers (delegates).

### MassTransitEventPublisher

- **Namespace:** `SmartWorkz.Shared.MassTransitEventPublisher`
- **Summary:** Distributed event publisher using MassTransit message bus.
            Supports both single and batch event publishing with async/await patterns.
            Suitable for production environments with message broker backend (RabbitMQ, Azure Service Bus, etc).

#### Methods & Properties

- **#ctor** - Initializes a new instance of MassTransitEventPublisher.
  - Parameters:
    - `publishEndpoint`: MassTransit publish endpoint for message distribution.
    - `logger`: Logger for event publication tracking.
- **PublishAsync``1** - Publishes a single domain event to the message bus asynchronously.
  - Parameters:
    - `event`: Event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous publish operation.
- **PublishAsync``1** - Publishes multiple domain events to the message bus asynchronously.
            Events are published sequentially in the order provided.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous batch publish operation.

### PublisherType

- **Namespace:** `SmartWorkz.Shared.PublisherType`
- **Summary:** Specifies the publisher type for event publishing.

### ServiceCollectionExtensions

- **Namespace:** `SmartWorkz.Shared.ServiceCollectionExtensions`
- **Summary:** Extension methods for IServiceCollection to register Core.Shared services.

#### Methods & Properties

- **AddCoreSharedServices** - Adds Core.Shared services including TemplateEngine for template rendering.
- **AddEventPublishing** - Adds event publishing services to the dependency injection container.
            Supports switching between in-memory and MassTransit publishers based on application needs.
  - Parameters:
    - `services`: The service collection.
    - `publisherType`: The publisher type to use (defaults to InMemory).
  - Returns: The service collection for method chaining.

### DefaultFeatureFlagService

- **Namespace:** `SmartWorkz.Shared.DefaultFeatureFlagService`
- **Summary:** Global (non-tenant) feature flag service with in-memory storage.
            Thread-safe implementation suitable for single-process deployments.
            Use for organization-wide feature toggles; use ITenantFeatureFlags for tenant-scoped flags.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature flag is enabled.
            Returns false for unknown flags (does not throw).
  - Parameters:
    - `flagName`: The name of the feature flag to check.
    - `cancellationToken`: Cancellation token.
  - Returns: True if the flag exists and is enabled; false otherwise.
- **GetEnabledFeaturesAsync** - Gets all enabled feature flags.
            Returns empty list if no flags are enabled.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: A read-only list of enabled feature flag names.
- **EnableFlag** - Enables a feature flag.
            Creates the flag if it doesn't exist.
  - Parameters:
    - `flagName`: The name of the feature flag to enable.
- **DisableFlag** - Disables a feature flag.
            Creates the flag as disabled if it doesn't exist.
  - Parameters:
    - `flagName`: The name of the feature flag to disable.

### IFeatureFlagService

- **Namespace:** `SmartWorkz.Shared.IFeatureFlagService`
- **Summary:** Global feature flag service for cross-tenant feature control.
            Use for organization-wide feature toggles (not tenant-specific).
            For tenant-scoped flags, use ITenantFeatureFlags.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a global feature is enabled.
  - Parameters:
    - `flagName`: Feature flag name (e.g., "NEW_DASHBOARD", "BETA_REPORTING").
    - `cancellationToken`: Cancellation token.
  - Returns: True if feature is enabled globally.
- **GetEnabledFeaturesAsync** - Gets all enabled global features.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: List of enabled feature flag names.

### IFileStorageService

- **Namespace:** `SmartWorkz.Shared.IFileStorageService`
- **Summary:** Interface for file storage operations supporting both local and cloud providers.

#### Methods & Properties

- **UploadAsync** - Uploads a file to storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `content`: The file content stream.
    - `metadata`: The file metadata.
    - `cancellationToken`: The cancellation token.
  - Returns: The full path or URI of the uploaded file.
- **DownloadAsync** - Downloads a file from storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: A stream containing the file content. Caller must dispose using 'using' statement.
- **DeleteAsync** - Deletes a file from storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
- **ExistsAsync** - Checks if a file exists in storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: True if the file exists, false otherwise.
- **GetMetadataAsync** - Gets metadata for a file in storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: FileMetadata if file exists, null otherwise.
- **ListAsync** - Lists files in a directory or container prefix.
  - Parameters:
    - `folderPath`: The relative folder path or blob prefix.
    - `cancellationToken`: The cancellation token.
  - Returns: A read-only collection of FileMetadata for files in the directory/prefix.
- **GenerateTemporaryUrlAsync** - Generates a temporary download URL for a file.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `expiration`: The expiration duration from now.
    - `cancellationToken`: The cancellation token.
  - Returns: A URL that can be used to download the file. For local storage, returns the full file path.

### GridColumn

- **Namespace:** `SmartWorkz.Shared.GridColumn`
- **Summary:** Defines a single column in a grid, including display options, sorting, filtering, and rendering hints.

### GridExportOptions

- **Namespace:** `SmartWorkz.Shared.GridExportOptions`
- **Summary:** Configuration for grid data export (CSV, Excel).

### GridRequest

- **Namespace:** `SmartWorkz.Shared.GridRequest`
- **Summary:** Request parameters for grid data fetching, extending PagedQuery with filtering support.

#### Methods & Properties

- **#ctor** - Request parameters for grid data fetching, extending PagedQuery with filtering support.

### GridResponse`1

- **Namespace:** `SmartWorkz.Shared.GridResponse`1`
- **Summary:** Response from a grid data request, including paged data, column metadata, and filter options.

### IGridDataProvider

- **Namespace:** `SmartWorkz.Shared.IGridDataProvider`
- **Summary:** Abstraction for grid data fetching. Implementations handle API calls or in-memory queries.
            Enables platform independence: Web uses HTTP, MAUI uses direct API client, Desktop uses local DB.

#### Methods & Properties

- **GetDataAsync``1** - Fetch paged grid data based on request (sorting, filtering, pagination).
  - Parameters:
    - `request`: Grid request with sorting, paging, and filter criteria.
    - `cancellationToken`: Cancellation token for async operations.
  - Returns: Result containing GridResponse or error details.

### Guard

- **Namespace:** `SmartWorkz.Shared.Guard`
- **Summary:** Static guard clauses for argument validation at method entry points.
             Throw immediately on invalid input — fail fast, fail loudly.
            
             Usage:
               Guard.NotNull(userId, nameof(userId));
               Guard.NotEmpty(name, nameof(name));
               Guard.InRange(pageSize, 1, 100, nameof(pageSize));
            
             These replace the ValidationExtensions.EnsureNotNull() extension method
             and the scattered ArgumentNullException throws throughout the codebase.

#### Methods & Properties

- **NotNull``1** - Throws ArgumentNullException if value is null.
- **NotNull``1** - Throws ArgumentNullException if value is null (struct/nullable).
- **NotEmpty** - Throws ArgumentException if string is null, empty, or whitespace.
- **NotEmpty``1** - Throws ArgumentException if collection is null or has no elements.
- **NotDefault``1** - Throws ArgumentException if value equals the default for its type (0, null, Guid.Empty).
- **InRange``1** - Throws ArgumentOutOfRangeException if value is outside [min, max].
- **Requires** - Throws ArgumentException if condition is false.

### EncryptionHelper

- **Namespace:** `SmartWorkz.Shared.EncryptionHelper`
- **Summary:** Cryptographic utilities for hashing and encryption.
            Uses PBKDF2 for password hashing and AES-256 for data encryption.

#### Methods & Properties

- **HashPassword** - Hash password using PBKDF2 with SHA256.
- **VerifyPassword** - Verify password against hash.
- **Encrypt** - Encrypt text using AES-256-GCM with provided key.
- **Decrypt** - Decrypt text using AES-256-GCM with provided key.
- **GenerateRandomString** - Generate cryptographically secure random string.
- **GenerateEncryptionKey** - Generate random encryption key (Base64 encoded).
- **ComputeSha256** - Compute SHA256 hash of text for integrity checking.

### JsonHelper

- **Namespace:** `SmartWorkz.Shared.JsonHelper`
- **Summary:** JSON serialization utilities using System.Text.Json.
            Provides consistent serialization options across the application.

#### Methods & Properties

- **Serialize``1** - Serialize object to JSON string.
- **Serialize** - Serialize object to JSON string with dynamic type.
- **Deserialize``1** - Deserialize JSON string to object.
- **Deserialize** - Deserialize JSON string to object with dynamic type.
- **DeserializeAsync``1** - Deserialize JSON asynchronously from stream.
- **SerializeAsync``1** - Serialize asynchronously to stream.
- **IsValidJson** - Check if string is valid JSON.
- **GetValueByPath** - Parse JSON and extract value at specified path (dot notation).

### IHttpClient

- **Namespace:** `SmartWorkz.Shared.IHttpClient`
- **Summary:** Abstraction for HTTP client operations with support for async/await and cancellation.
            Implementations should handle retries, timeouts, and error responses gracefully.

#### Methods & Properties

- **GetAsync``1** - Sends a GET request and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **PostAsync``1** - Sends a POST request with JSON body and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **PutAsync``1** - Sends a PUT request with JSON body and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **DeleteAsync``1** - Sends a DELETE request and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **GetAsync** - Sends a GET request and returns a string response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.
- **PostAsync** - Sends a POST request with JSON body and returns a string response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.

### RetryStrategy

- **Namespace:** `SmartWorkz.Shared.RetryStrategy`
- **Summary:** Specifies the backoff strategy to use when retrying failed HTTP requests.

### RetryPolicy

- **Namespace:** `SmartWorkz.Shared.RetryPolicy`
- **Summary:** Configures automatic retry behavior for failed HTTP requests.

### AuditRecord

- **Namespace:** `SmartWorkz.Shared.AuditRecord`
- **Summary:** Represents an immutable audit record with all relevant audit information.

#### Methods & Properties

- **#ctor** - Represents an immutable audit record with all relevant audit information.
  - Parameters:
    - `Id`: The unique identifier of the audit record
    - `EntityType`: The type of entity being audited (e.g., "User", "BlogPost")
    - `EntityId`: The identifier of the audited entity
    - `Action`: The action performed (Create, Update, Delete, etc.)
    - `UserId`: The identifier of the user who performed the action
    - `PerformedAt`: The timestamp when the action was performed
    - `Metadata`: Optional metadata dictionary containing additional context

### EnrichedLogger

- **Namespace:** `SmartWorkz.Shared.EnrichedLogger`
- **Summary:** Enriched logger wrapper around ILogger that provides structured logging methods
            for domain events, commands, sagas, file operations, and background jobs.
            Uses structured properties instead of string interpolation for better queryability.

#### Methods & Properties

- **#ctor** - Creates a new instance of EnrichedLogger.
  - Parameters:
    - `logger`: The underlying ILogger instance
- **LogCommandExecuted** - Logs command execution with duration and other metrics.
  - Parameters:
    - `commandType`: The type of command being executed
    - `duration`: How long the command took to execute
- **LogCommandExecutionError** - Logs a command execution error with exception details.
  - Parameters:
    - `commandType`: The type of command that failed
    - `exception`: The exception that occurred
- **LogCommandValidationError** - Logs a command with validation errors.
  - Parameters:
    - `commandType`: The type of command
    - `errors`: Dictionary of validation errors
- **LogEventPublished** - Logs an event publication with metadata.
  - Parameters:
    - `eventType`: The type of event being published
    - `eventId`: The unique identifier of the event
- **LogEventPublishedWithContext** - Logs an event with additional context properties.
  - Parameters:
    - `eventType`: The type of event
    - `eventId`: The event identifier
    - `context`: Additional context data
- **LogEventSubscribed** - Logs an event subscription.
  - Parameters:
    - `eventType`: The type of event being subscribed to
    - `subscriberType`: The subscriber type
- **LogSagaStarted** - Logs the start of a saga with its initial state.
  - Parameters:
    - `sagaId`: The unique saga identifier
    - `state`: The initial saga state
- **LogSagaStateTransition** - Logs a saga state transition.
  - Parameters:
    - `sagaId`: The saga identifier
    - `fromState`: The previous state
    - `toState`: The new state
- **LogSagaCompleted** - Logs the completion of a saga.
  - Parameters:
    - `sagaId`: The saga identifier
    - `duration`: How long the saga took to complete
- **LogSagaFailed** - Logs a saga failure.
  - Parameters:
    - `sagaId`: The saga identifier
    - `exception`: The exception that caused the failure
- **LogFileOperation** - Logs file operations such as upload, download, delete.
  - Parameters:
    - `operation`: The type of operation (Upload, Download, Delete, etc.)
    - `filePath`: The file path or URI
- **LogFileOperationWithSize** - Logs a file operation with size information.
  - Parameters:
    - `operation`: The type of operation
    - `filePath`: The file path
    - `sizeBytes`: The file size in bytes
- **LogFileOperationError** - Logs a file operation error.
  - Parameters:
    - `operation`: The operation that failed
    - `filePath`: The file path
    - `exception`: The exception that occurred
- **LogJobQueued** - Logs when a background job is queued.
  - Parameters:
    - `jobId`: The unique job identifier
    - `jobType`: The type of job being queued
- **LogJobStarted** - Logs when a background job starts processing.
  - Parameters:
    - `jobId`: The job identifier
    - `jobType`: The job type
- **LogJobCompleted** - Logs successful job completion.
  - Parameters:
    - `jobId`: The job identifier
    - `duration`: How long the job took to complete
- **LogJobFailed** - Logs a job failure.
  - Parameters:
    - `jobId`: The job identifier
    - `exception`: The exception that caused the failure
- **LogJobRetry** - Logs job retry attempt.
  - Parameters:
    - `jobId`: The job identifier
    - `attemptNumber`: The current attempt number
    - `maxRetries`: The maximum number of retries
- **LogWithContext** - Logs a message with structured context properties.
  - Parameters:
    - `operationName`: The name of the operation
    - `context`: Dictionary of contextual properties
- **LogPerformanceMetrics** - Logs performance metrics for an operation.
  - Parameters:
    - `operationName`: The operation name
    - `duration`: The operation duration
    - `resultStatus`: The result status (Success, Failure, etc.)
- **LogCorrelation** - Logs a correlation ID for request tracing.
  - Parameters:
    - `correlationId`: The correlation identifier
    - `userId`: Optional user identifier
    - `requestPath`: Optional request path
- **LogUnhandledException** - Logs unhandled exceptions as critical errors.
  - Parameters:
    - `exception`: The exception that occurred
    - `operationName`: The operation that failed

### IAuditLogger

- **Namespace:** `SmartWorkz.Shared.IAuditLogger`
- **Summary:** Interface for structured audit logging with metadata support.

#### Methods & Properties

- **LogAuditAsync** - Logs an audit event with structured metadata.
  - Parameters:
    - `entityType`: The entity type being audited (e.g., "User", "BlogPost")
    - `entityId`: The unique identifier of the entity
    - `action`: The action performed (Create, Update, Delete, etc.)
    - `metadata`: Optional metadata dictionary for additional context
    - `cancellationToken`: Cancellation token
  - Returns: Result indicating success or failure
- **GetAuditHistoryAsync** - Retrieves audit logs for a specific entity.
  - Parameters:
    - `entityType`: The entity type
    - `entityId`: The entity identifier
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of audit records
- **GetUserActivityAsync** - Retrieves audit logs for a specific user across all entities.
  - Parameters:
    - `userId`: The user identifier
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of audit records

### ILogger

- **Namespace:** `SmartWorkz.Shared.ILogger`
- **Summary:** Abstraction for application logging.
            Decouples from specific logging frameworks (Serilog, NLog, etc.).

### LogLevel

- **Namespace:** `SmartWorkz.Shared.LogLevel`
- **Summary:** Log level severity.

### ILoggerFactory

- **Namespace:** `SmartWorkz.Shared.ILoggerFactory`
- **Summary:** Factory for creating logger instances by category/source.

### IMapper

- **Namespace:** `SmartWorkz.Shared.IMapper`
- **Summary:** Mapping service abstraction for transforming objects between types.
            Supports registration of mapping profiles and bidirectional conversions.

#### Methods & Properties

- **Map``2** - Map source object to target type.
- **Map** - Map source object to target type using dynamic type.
- **MapAsync``2** - Map asynchronously with potential async operations in profile.
- **MapCollection``2** - Map collection of sources to targets.
- **MapCollectionAsync``2** - Map collection asynchronously.
- **RegisterProfile``2** - Register a mapping profile.

### IMapperProfile

- **Namespace:** `SmartWorkz.Shared.IMapperProfile`
- **Summary:** Profile for defining mapping rules between types.
            Implemented by concrete profiles that configure source-to-target transformations.

### IMapperProfile`2

- **Namespace:** `SmartWorkz.Shared.IMapperProfile`2`
- **Summary:** Typed mapper profile for strong typing.

#### Methods & Properties

- **Map** - Transform source to target synchronously.
- **MapAsync** - Transform source to target asynchronously.

### SimpleMapper

- **Namespace:** `SmartWorkz.Shared.SimpleMapper`
- **Summary:** A simple in-memory mapper that supports registering and executing mapping profiles.

### IMetricsCollector

- **Namespace:** `SmartWorkz.Shared.IMetricsCollector`
- **Summary:** Abstraction for collecting application metrics and performance data.
            Enables tracking of operation duration, throughput, error rates, and custom metrics.
            Implementations integrate with OpenTelemetry for export to Prometheus/Grafana.

#### Methods & Properties

- **RecordOperationDuration** - Record operation duration in milliseconds.
  - Parameters:
    - `operationName`: Name of the operation being measured.
    - `durationMs`: Duration in milliseconds.
    - `status`: Optional status (e.g., "success", "error").
    - `tags`: Optional metadata tags for grouping and filtering.
- **RecordOperationCount** - Record operation count (increments counter).
  - Parameters:
    - `operationName`: Name of the operation.
    - `count`: Number to increment by (default 1).
    - `status`: Optional status label.
    - `tags`: Optional metadata tags.
- **RecordGaugeValue** - Record a gauge value (e.g., queue depth, memory usage).
  - Parameters:
    - `metricName`: Name of the gauge metric.
    - `value`: The gauge value to record.
    - `tags`: Optional metadata tags.
- **RecordError** - Record error/exception occurrence.
  - Parameters:
    - `operationName`: Name of the operation that failed.
    - `ex`: The exception that occurred.
    - `tags`: Optional metadata tags.
- **IncrementCounter** - Increment a custom counter.
  - Parameters:
    - `counterName`: Name of the counter.
    - `increment`: Amount to increment (default 1).
    - `tags`: Optional metadata tags.

### MetricsMiddleware

- **Namespace:** `SmartWorkz.Shared.MetricsMiddleware`
- **Summary:** ASP.NET Core middleware for automatic HTTP request/response metrics collection.
             Records operation duration, status, and errors for all HTTP requests.
            
             Usage:
                 app.UseMiddleware<MetricsMiddleware>();

### MetricsStartupExtensions

- **Namespace:** `SmartWorkz.Shared.MetricsStartupExtensions`
- **Summary:** Extension methods for registering application metrics in dependency injection.

#### Methods & Properties

- **AddApplicationMetrics** - Registers IMetricsCollector with OpenTelemetry implementation.
  - Parameters:
    - `services`: The service collection to register with.
  - Returns: The service collection for method chaining.

### OpenTelemetryMetricsCollector

- **Namespace:** `SmartWorkz.Shared.OpenTelemetryMetricsCollector`
- **Summary:** OpenTelemetry-based implementation of IMetricsCollector.
            Collects metrics using System.Diagnostics.Metrics for export to Prometheus/Grafana.

### DefaultTenantFeatureFlags

- **Namespace:** `SmartWorkz.Shared.DefaultTenantFeatureFlags`
- **Summary:** In-memory feature flag provider for tenant-scoped feature control.
            
             Uses ConcurrentDictionary to store tenant-specific flags:
             - Key: tenant ID
             - Value: HashSet of enabled feature flag names
            
             Thread-safe for concurrent operations. Suitable for in-process caching
             or dev/test scenarios. For distributed systems, integrate with a
             centralized feature flag service (Unleash, LaunchDarkly, etc.).

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature is enabled for the specified tenant.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").
    - `cancellationToken`: Cancellation token.
  - Returns: Task containing true if the feature is enabled for this tenant,
            false if the tenant doesn't exist or the flag is not enabled.
- **GetEnabledFeaturesAsync** - Gets all enabled features for a tenant.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Task containing a read-only list of enabled feature flag names.
            Returns an empty list if the tenant doesn't exist or has no enabled flags.
- **EnableFlag** - Enables a feature flag for a tenant.
            Idempotent — calling multiple times with the same flag is safe.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name.
- **DisableFlag** - Disables a feature flag for a tenant.
            Idempotent — calling multiple times with the same flag is safe.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name.

### ITenantContext

- **Namespace:** `SmartWorkz.Shared.ITenantContext`
- **Summary:** Scoped service providing current tenant ID for multi-tenant applications.
            Resolved from request context or claims principal.

#### Methods & Properties

- **GetTenantId** - Gets the current tenant identifier.
  - Returns: Tenant ID, or null if operating in single-tenant context.
- **SetTenantId** - Sets the current tenant identifier (rarely used; typically set from request context).
  - Parameters:
    - `tenantId`: Tenant ID to set.

### ITenantFeatureFlags

- **Namespace:** `SmartWorkz.Shared.ITenantFeatureFlags`
- **Summary:** Feature flag provider scoped to a specific tenant.
            Allows per-tenant feature control.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature is enabled for the specified tenant.
  - Parameters:
    - `tenantId`: Tenant ID.
    - `flagName`: Feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").
    - `cancellationToken`: Cancellation token.
  - Returns: True if feature is enabled for this tenant.
- **GetEnabledFeaturesAsync** - Gets all enabled features for a tenant.
  - Parameters:
    - `tenantId`: Tenant ID.
    - `cancellationToken`: Cancellation token.
  - Returns: List of enabled feature flag names.

### TenantContext

- **Namespace:** `SmartWorkz.Shared.TenantContext`
- **Summary:** Scoped tenant context using AsyncLocal for proper isolation across async boundaries.
            
             AsyncLocal ensures:
             - Thread-safe storage per async execution context
             - Isolation between concurrent requests (each gets its own context)
             - Proper inheritance to child tasks (when awaited)
            
             Survives async/await boundaries unlike ThreadLocal, making it suitable for async methods.

#### Methods & Properties

- **GetTenantId** - Gets the current tenant identifier.
  - Returns: The current tenant ID, or "default" if not set.
- **SetTenantId** - Sets the current tenant identifier.
  - Parameters:
    - `tenantId`: The tenant ID to set. Cannot be null or empty.

### FirebaseCloudMessagingService

- **Namespace:** `SmartWorkz.Shared.FirebaseCloudMessagingService`
- **Summary:** Firebase Cloud Messaging service implementation for sending push notifications.
            Supports single/batch user notifications, topic-based broadcasting, and multi-platform delivery (Android, iOS, Web).

#### Methods & Properties

- **#ctor** - Initializes a new instance of the FirebaseCloudMessagingService.
  - Parameters:
    - `logger`: Logger for diagnostic and error information.
- **SendAsync** - Sends a simple push notification to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends simple push notifications to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase device tokens).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends a rich push notification with metadata to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends a rich push notification to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase device tokens).
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SendToTopicAsync** - Sends a rich push notification to all users subscribed to a topic (broadcast).
  - Parameters:
    - `topic`: Topic name (e.g., "news", "promotions").
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SubscribeToTopicAsync** - Subscribes a user to a topic for broadcast notifications.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `topic`: Topic name to subscribe to.
    - `cancellationToken`: Cancellation token.
- **UnsubscribeFromTopicAsync** - Unsubscribes a user from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `topic`: Topic name to unsubscribe from.
    - `cancellationToken`: Cancellation token.

### IPushNotificationService

- **Namespace:** `SmartWorkz.Shared.IPushNotificationService`
- **Summary:** Service for sending push notifications using Firebase Cloud Messaging.

#### Methods & Properties

- **SendAsync** - Sends a simple push notification to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a simple push notification to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase tokens).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a rich push notification with metadata to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a rich push notification with metadata to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase tokens).
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendToTopicAsync** - Sends a push notification to all users subscribed to a topic.
  - Parameters:
    - `topic`: The topic name.
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SubscribeToTopicAsync** - Subscribes a user to receive notifications from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `topic`: The topic name.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous subscription operation.
- **UnsubscribeFromTopicAsync** - Unsubscribes a user from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `topic`: The topic name.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous unsubscription operation.

### PushNotificationPayload

- **Namespace:** `SmartWorkz.Shared.PushNotificationPayload`
- **Summary:** Represents the payload data for a push notification.

### PushNotificationAction

- **Namespace:** `SmartWorkz.Shared.PushNotificationAction`
- **Summary:** Represents an action that can be performed from a push notification.

### PagedList`1

- **Namespace:** `SmartWorkz.Shared.PagedList`1`
- **Summary:** A page of items with metadata.
             Replaces PaginationResponse<T> in StarterKitMVC.Shared.DTOs.
            
             Migration path: PaginationResponse<T> has the same fields under different names.
             PagedList<T>.Create() is a drop-in replacement for PaginationResponse<T>.Create().

#### Methods & Properties

- **Empty** - Create an empty result set (e.g., when no rows match).
- **Map``1** - Project items to a different type without changing pagination metadata.

### PagedQuery

- **Namespace:** `SmartWorkz.Shared.PagedQuery`
- **Summary:** Standard request parameters for any paginated query.
             Replaces the existing PaginationRequest record in StarterKitMVC.Shared.DTOs.
            
             Migration: PaginationRequest is structurally identical — alias it or replace it.

#### Methods & Properties

- **#ctor** - Standard request parameters for any paginated query.
             Replaces the existing PaginationRequest record in StarterKitMVC.Shared.DTOs.
            
             Migration: PaginationRequest is structurally identical — alias it or replace it.
- **Normalize** - Clamp page and pageSize to safe bounds.

### IEntity`1

- **Namespace:** `SmartWorkz.Shared.IEntity`1`
- **Summary:** Marks a class as a domain entity with a typed primary key.

### CircuitBreaker

- **Namespace:** `SmartWorkz.Shared.CircuitBreaker`
- **Summary:** A thread-safe implementation of the circuit breaker pattern for handling failing dependencies gracefully.
            
             The circuit breaker operates in three states:
             - Closed: Normal operation. Requests pass through. Failures are tracked.
             - Open: Failing. All requests are rejected immediately to prevent cascading failures.
             - HalfOpen: Testing recovery. Limited requests are allowed to test if the dependency has recovered.
            
             State transitions:
             - Closed → Open: When ConsecutiveFailures >= FailureThreshold
             - Open → HalfOpen: Automatically when (DateTime.UtcNow - LastFailureTime) >= TimeoutMilliseconds
             - HalfOpen → Closed: When SuccessCount >= SuccessThreshold
             - HalfOpen → Open: When RecordFailure() is called in HalfOpen state
             - Closed → Closed: When RecordSuccess() is called (resets failure counter)

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `options`: The circuit breaker configuration options.
- **RecordSuccess** - Records a successful operation and updates state transitions accordingly.
            In Closed state: resets the failure counter.
            In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
- **RecordFailure** - Records a failed operation and updates state transitions accordingly.
            In Closed state: increments failure counter and transitions to Open if threshold is met.
            In HalfOpen state: immediately transitions back to Open.
- **Reset** - Resets the circuit breaker to the Closed state, clearing all failure and success counters.

### CircuitBreakerOptions

- **Namespace:** `SmartWorkz.Shared.CircuitBreakerOptions`
- **Summary:** Configuration options for the circuit breaker.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if options are valid; otherwise false.

### CircuitBreakerState

- **Namespace:** `SmartWorkz.Shared.CircuitBreakerState`
- **Summary:** Defines the state of a circuit breaker in the state machine pattern.

### ICircuitBreaker

- **Namespace:** `SmartWorkz.Shared.ICircuitBreaker`
- **Summary:** Defines the contract for a circuit breaker that implements the state machine pattern
            to handle failing dependencies gracefully.

#### Methods & Properties

- **RecordSuccess** - Records a successful operation and updates state transitions accordingly.
            In Closed state: resets the failure counter.
            In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
- **RecordFailure** - Records a failed operation and updates state transitions accordingly.
            In Closed state: increments failure counter and transitions to Open if threshold is met.
            In HalfOpen state: immediately transitions back to Open.
- **Reset** - Resets the circuit breaker to the Closed state, clearing all failure and success counters.

### IRateLimiter

- **Namespace:** `SmartWorkz.Shared.IRateLimiter`
- **Summary:** Defines the contract for a thread-safe rate limiter.

#### Methods & Properties

- **TryAcquireAsync** - Attempts to acquire tokens for a request identified by the given identifier.
  - Parameters:
    - `identifier`: The identifier for rate limiting (e.g., IP address, user ID).
    - `cost`: The number of tokens to acquire (default: 1).
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A result containing true if tokens were acquired successfully; otherwise false.
            On failure, the Error will include retry-after information.
- **GetAvailableTokensAsync** - Gets the number of available tokens for a given identifier.
  - Parameters:
    - `identifier`: The identifier to check.
  - Returns: The number of available tokens.
- **ResetAsync** - Resets the rate limit state for a given identifier.
  - Parameters:
    - `identifier`: The identifier to reset.
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **ClearAsync** - Clears all rate limit state.
  - Parameters:
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.

### RateLimiter

- **Namespace:** `SmartWorkz.Shared.RateLimiter`
- **Summary:** Thread-safe token bucket rate limiter implementation.
            
             This class maintains a per-identifier token bucket that refills at a constant rate.
             Tokens are consumed when requests are made; if insufficient tokens exist, the request is denied.
            
             Thread-safe operations use ConcurrentDictionary and locks on individual buckets to ensure
             consistent state without global locking bottlenecks.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the RateLimiter class.
  - Parameters:
    - `options`: Configuration options for the rate limiter.
- **TryAcquireAsync** - Attempts to acquire tokens for a request identified by the given identifier.
  - Parameters:
    - `identifier`: The identifier for rate limiting (e.g., IP address, user ID).
    - `cost`: The number of tokens to acquire (default: 1).
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A result containing true if tokens were acquired successfully; otherwise false.
            On failure, the Error will include retry-after information.
- **GetAvailableTokensAsync** - Gets the number of available tokens for a given identifier.
  - Parameters:
    - `identifier`: The identifier to check.
  - Returns: The number of available tokens.
- **ResetAsync** - Resets the rate limit state for a given identifier.
  - Parameters:
    - `identifier`: The identifier to reset.
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **ClearAsync** - Clears all rate limit state.
  - Parameters:
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **TokenBucket.TryAcquire** - Tries to acquire the specified number of tokens.
- **TokenBucket.GetAvailableTokens** - Gets the current number of available tokens.
- **TokenBucket.GetRetryAfterMilliseconds** - Gets the number of milliseconds to wait before retrying.
- **TokenBucket.RefillTokens** - Refills the token bucket based on elapsed time.

### RateLimiterOptions

- **Namespace:** `SmartWorkz.Shared.RateLimiterOptions`
- **Summary:** Configuration options for the rate limiter.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if options are valid; otherwise false.

### RateLimiterStrategy

- **Namespace:** `SmartWorkz.Shared.RateLimiterStrategy`
- **Summary:** Specifies the strategy used by the rate limiter to control request flow.

### ApiError

- **Namespace:** `SmartWorkz.Shared.ApiError`
- **Summary:** Structured error representation for API responses.
            Provides code, message, and optional field-level error details.

#### Methods & Properties

- **FromError** - Create from core Error type.
- **FromValidationErrors** - Create from validation errors.
- **FromException** - Create from exception.

### ApiResponse

- **Namespace:** `SmartWorkz.Shared.ApiResponse`
- **Summary:** Generic API response envelope that wraps result data with metadata.
            Non-generic convenience version for non-data responses.

#### Methods & Properties

- **Ok** - Success response without data.
- **Fail** - Failure response with error details.
- **FromResult** - Create from core Result pattern.

### ApiResponse`1

- **Namespace:** `SmartWorkz.Shared.ApiResponse`1`
- **Summary:** Typed API response envelope with data payload.
            Includes optional pagination metadata for list responses.

#### Methods & Properties

- **Ok** - Success response with data.
- **OkPaginated** - Success response with paginated data.
- **Fail** - Failure response with error.

### ProblemDetailsResponse

- **Namespace:** `SmartWorkz.Shared.ProblemDetailsResponse`
- **Summary:** Implements RFC 7807 Problem Details for HTTP APIs standard response format.
            Provides a standardized way to represent error details in API responses.

#### Methods & Properties

- **ValidationError** - Factory method for 400 Bad Request error with validation details.
- **Unauthorized** - Factory method for 401 Unauthorized error.
- **Forbidden** - Factory method for 403 Forbidden error.
- **NotFound** - Factory method for 404 Not Found error.
- **Conflict** - Factory method for 409 Conflict error.
- **InternalServerError** - Factory method for 500 Internal Server Error.
- **Custom** - Factory method for custom problem details.

### Error

- **Namespace:** `SmartWorkz.Shared.Error`
- **Summary:** Represents a structured error with a machine-readable code and human-readable message.
            
             This is the canonical Error type. It replaces:
             - SmartWorkz.StarterKitMVC.Shared.Primitives.Error (record struct)
             - The ad-hoc string errors in Models.Result
            
             Code examples: "USER_NOT_FOUND", "VALIDATION.EMAIL_REQUIRED", "AUTH.INVALID_CREDENTIALS"
             MessageKey maps to localization resource keys for UI display.

### Result

- **Namespace:** `SmartWorkz.Shared.Result`
- **Summary:** Represents the outcome of an operation that does not return a value.
            
             This unifies:
             - SmartWorkz.StarterKitMVC.Shared.Models.Result (Succeeded + MessageKey + Errors[])
             - SmartWorkz.StarterKitMVC.Shared.Primitives.Result (IsSuccess + Error struct)
            
             Design choice — class over struct:
             1. Result<T> inherits from Result to reuse Succeeded/Errors without duplication.
                Structs cannot use inheritance this way.
             2. Services return Result from interface methods — class semantics (null check) are
                simpler than boxing/unboxing structs across interface boundaries.
             3. Errors[] supports field-level validation messages that ModelState.AddErrors() consumes.
                A single Error struct cannot carry multiple field errors.
            
             The Primitives.Result struct in StarterKitMVC.Shared remains valid for pure functions
             where you want zero-allocation returns. This class is for service layer contracts.

#### Methods & Properties

- **Fail** - Failure with a localization message key and optional field-level error strings.
- **Fail** - Failure from a structured Error (bridges the Primitives.Error pattern).
- **Ok``1** - Factory for a typed result. Use in services that return data.

### Result`1

- **Namespace:** `SmartWorkz.Shared.Result`1`
- **Summary:** Result with a typed payload. Data is only valid when Succeeded = true.
            
             Usage:
               Result<UserDto> result = await _userService.GetByIdAsync(id);
               if (!result.Succeeded) return RedirectToPage("Error");
               var user = result.Data!;

### ResultExtensions

- **Namespace:** `SmartWorkz.Shared.ResultExtensions`
- **Summary:** Functional helpers for chaining Result operations.
            Keeps service code flat — avoids nested if (!result.Succeeded) blocks.

#### Methods & Properties

- **Map``2** - Transform the Data value if the result succeeded.
- **BindAsync``2** - Chain a second operation that also returns Result.
- **OnSuccess``1** - Execute a side-effect action on success, then return the original result.
- **OnFailure``1** - Execute a side-effect action on failure, then return the original result.

### ISagaDefinition`1

- **Namespace:** `SmartWorkz.Shared.ISagaDefinition`1`
- **Summary:** Defines the blueprint for a saga orchestration.
            A saga is a pattern for managing distributed transactions and long-running processes
            by coordinating multiple steps with built-in compensation mechanisms.

#### Methods & Properties

- **DefineStep``1** - Defines a step in the saga that will be executed when a specific event type is received.
            Steps are executed sequentially in the order they were defined.
  - Parameters:
    - `handler`: The async handler function that processes the event and updates the saga state.
            Returns a StepResult indicating success or failure.
- **OnFailure** - Defines the failure handler that will be called if any step fails.
            Used for compensation logic and saga-level error handling.
  - Parameters:
    - `compensationHandler`: The async handler that receives the current saga state and the exception that occurred.
            Responsible for compensation/rollback logic.
- **BuildAsync** - Builds and returns the saga definition for execution.
            Can be used for async initialization or validation.
  - Returns: A task that completes with the configured saga definition.
- **GetSteps** - Gets the list of saga steps in execution order.
  - Returns: A read-only list of saga step handlers.
- **GetFailureHandler** - Gets the failure compensation handler if defined.
  - Returns: The failure handler function, or null if not defined.

### SagaOrchestrator

- **Namespace:** `SmartWorkz.Shared.SagaOrchestrator`
- **Summary:** Orchestrates the execution of sagas, managing step sequencing, error handling,
            and compensation/rollback logic for complex distributed processes.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the SagaOrchestrator class.
  - Parameters:
    - `logger`: Logger for saga execution tracking and debugging.
- **ExecuteSagaAsync``1** - Executes a saga definition with the provided initial state and triggering event.
            Manages step execution, error handling, and compensation logic.
  - Parameters:
    - `sagaDefinition`: The saga definition blueprint to execute.
    - `initialState`: The initial saga state.
    - `event`: The domain event triggering the saga.
    - `cancellationToken`: Optional cancellation token.
  - Returns: A task representing the saga execution.
- **ExecuteSagaStepsAsync``1** - Executes saga steps by using reflection to access internal step definitions.
- **CompensateExecutedStepsAsync``1** - Executes compensation handlers for all executed steps in reverse order.
            Uses stored compensation handlers to avoid re-executing steps.
- **ExecuteFailureHandlerAsync``1** - Executes the saga-level failure handler if one is defined.

### SagaStatus

- **Namespace:** `SmartWorkz.Shared.SagaStatus`
- **Summary:** Represents the status of a saga execution.

### SagaState

- **Namespace:** `SmartWorkz.Shared.SagaState`
- **Summary:** Base class for saga state objects.
            Provides common tracking properties for saga execution flow.

### StepResult

- **Namespace:** `SmartWorkz.Shared.StepResult`
- **Summary:** Represents the result of executing a single saga step.
            Provides success/failure status and optional compensation logic for rollback.

#### Methods & Properties

- **Success** - Creates a successful step result.
  - Returns: A StepResult indicating success.
- **Failure** - Creates a failed step result with an optional compensation handler.
  - Parameters:
    - `failureReason`: The reason for the step failure.
    - `compensationHandler`: Optional handler to compensate/rollback this step if a later step fails.
  - Returns: A StepResult indicating failure.
- **FromException** - Creates a failed step result for an exception with optional compensation.
  - Parameters:
    - `exception`: The exception that caused the failure.
    - `compensationHandler`: Optional compensation handler.
  - Returns: A StepResult indicating failure.

### CryptHelper

- **Namespace:** `SmartWorkz.Shared.CryptHelper`
- **Summary:** Provides AES-256-CBC encryption and decryption utilities with secure key and IV generation.
            
             All operations support both string and byte array inputs/outputs.
             Keys are normalized to 32 bytes (256 bits) via padding/trimming as needed.
             IVs are auto-generated if not provided and embedded in the ciphertext (IV:Ciphertext format).

#### Methods & Properties

- **EncryptString** - Encrypts plaintext using AES-256-CBC with a Base64-encoded output.
  - Parameters:
    - `plaintext`: The plaintext to encrypt.
    - `key`: The encryption key (will be normalized to 32 bytes).
    - `iv`: Optional IV; auto-generated if null.
  - Returns: A Result containing Base64-encoded ciphertext in "IV:Ciphertext" format or an error.
- **EncryptBytes** - Encrypts byte data using AES-256-CBC.
  - Parameters:
    - `plaintext`: The plaintext bytes to encrypt.
    - `key`: The encryption key (will be normalized to 32 bytes).
    - `iv`: Optional IV; auto-generated if null.
  - Returns: A Result containing encrypted bytes with embedded IV (IV || Ciphertext) or an error.
- **DecryptString** - Decrypts Base64-encoded ciphertext using AES-256-CBC.
  - Parameters:
    - `ciphertext`: The Base64-encoded ciphertext in "IV:Ciphertext" format.
    - `key`: The decryption key (will be normalized to 32 bytes).
  - Returns: A Result containing the decrypted plaintext or an error.
- **DecryptBytes** - Decrypts byte data using AES-256-CBC.
  - Parameters:
    - `ciphertext`: The encrypted bytes with embedded IV (IV || Ciphertext).
    - `key`: The decryption key (will be normalized to 32 bytes).
  - Returns: A Result containing decrypted bytes or an error.
- **GenerateKey** - Generates a random cryptographic key of the specified size.
  - Parameters:
    - `keySize`: The key size in bytes (default 32 for AES-256). Must be 16, 24, or 32.
  - Returns: A Result containing Base64-encoded random key or an error.
- **GenerateIv** - Generates a random cryptographic IV (Initialization Vector).
  - Returns: A Result containing Base64-encoded random IV or an error.
- **GenerateRandomBytes** - Generates cryptographically secure random bytes.
- **NormalizeKey** - Normalizes a key to exactly 32 bytes (256 bits).
            If the key is shorter, it's padded with zeros. If longer, it's trimmed.

### CryptOptions

- **Namespace:** `SmartWorkz.Shared.CryptOptions`
- **Summary:** Configuration options for AES cryptographic operations.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if valid, false otherwise.

### HashHelper

- **Namespace:** `SmartWorkz.Shared.HashHelper`
- **Summary:** Provides utilities for cryptographic hash operations (SHA256 and MD5).

#### Methods & Properties

- **Sha256** - Computes the SHA256 hash of a string and returns it as a hexadecimal string.
- **Sha256Bytes** - Computes the SHA256 hash of a byte array and returns the hash as a byte array.
- **Md5** - Computes the MD5 hash of a string and returns it as a hexadecimal string.
            Note: MD5 is cryptographically broken; use SHA256 for security-critical applications.
- **VerifyHash** - Verifies that a text matches its SHA256 hash.

### HmacAlgorithm

- **Namespace:** `SmartWorkz.Shared.HmacAlgorithm`
- **Summary:** Specifies the HMAC algorithm to use for message signing and verification.

### HmacHelper

- **Namespace:** `SmartWorkz.Shared.HmacHelper`
- **Summary:** Provides HMAC-SHA256/SHA512 message signing and verification for API requests and webhook verification.
            Implements constant-time comparison to prevent timing attacks.

#### Methods & Properties

- **Sign** - Signs a message using HMAC with the specified algorithm and returns a Base64-encoded hex digest.
  - Parameters:
    - `message`: The message to sign.
    - `secret`: The secret key for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing the Base64-encoded signature or an error.
- **SignBytes** - Signs a message using HMAC with the specified algorithm and returns the raw byte digest.
  - Parameters:
    - `message`: The message bytes to sign.
    - `secret`: The secret key for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing the byte signature or an error.
- **Verify** - Verifies a message signature using HMAC with constant-time comparison to prevent timing attacks.
  - Parameters:
    - `message`: The original message that was signed.
    - `signature`: The Base64-encoded signature to verify.
    - `secret`: The secret key used for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing true if the signature is valid, false if not, or an error.
- **VerifyBytes** - Verifies a message signature using HMAC with raw byte inputs and constant-time comparison.
  - Parameters:
    - `message`: The original message bytes that were signed.
    - `signature`: The byte signature to verify.
    - `secret`: The secret key used for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing true if the signature is valid, false if not, or an error.
- **SignBytes** - Internal method to compute HMAC signature from raw bytes.
- **CreateHmac** - Creates the appropriate HMAC instance based on the algorithm.

### InputSanitizer

- **Namespace:** `SmartWorkz.Shared.InputSanitizer`
- **Summary:** Input sanitization to prevent XSS, SQL injection, and path traversal attacks.

#### Methods & Properties

- **SanitizeHtml** - Sanitize HTML by removing dangerous tags and attributes.
- **EscapeHtml** - Escape HTML special characters to prevent XSS.
- **SanitizeSql** - Sanitize string to prevent SQL injection (basic, not a replacement for parameterized queries).
- **SanitizeFilePath** - Sanitize file path to prevent directory traversal attacks.
- **SanitizeUrl** - Sanitize and validate URL.
- **EscapeJson** - Escape string for safe JSON inclusion.
- **IsValidEmail** - Validate email format (basic check, server-side SMTP validation recommended).
- **RemoveControlCharacters** - Remove null bytes and control characters.

### JwtSettings

- **Namespace:** `SmartWorkz.Shared.JwtSettings`
- **Summary:** Settings for JWT token generation and validation.

#### Methods & Properties

- **Validate** - Validate settings: Secret >= 32 chars, other fields non-empty.

### JwtClaims

- **Namespace:** `SmartWorkz.Shared.JwtClaims`
- **Summary:** JWT claims that can be included in a token.

#### Methods & Properties

- **GetClaimValue** - Get claim value by type (supports standard claims + custom).

### JwtTokenValidationResult

- **Namespace:** `SmartWorkz.Shared.JwtTokenValidationResult`
- **Summary:** Result of JWT token validation.

### JwtHelper

- **Namespace:** `SmartWorkz.Shared.JwtHelper`
- **Summary:** Provides JWT token generation, validation, and refresh functionality.

#### Methods & Properties

- **GenerateTokenInternal** - Internal token generation logic shared by GenerateToken and GenerateRefreshToken.
  - Parameters:
    - `claims`: The claims to include in the token.
    - `settings`: The JWT settings for signing and configuration.
    - `isRefreshToken`: If true, uses RefreshTokenExpiryDays; otherwise uses ExpiryMinutes.
  - Returns: A Result containing the signed token or an error.
- **GenerateToken** - Generates a JWT access token with the specified claims and settings.
  - Parameters:
    - `claims`: The claims to include in the token.
    - `settings`: The JWT settings for signing and configuration.
  - Returns: A Result containing the signed token or an error.
- **ValidateToken** - Validates a JWT token and extracts claims if valid.
  - Parameters:
    - `token`: The token to validate.
    - `settings`: The JWT settings for validation.
  - Returns: A Result containing the validation result.
- **RefreshToken** - Refreshes an access token using a refresh token.
  - Parameters:
    - `refreshToken`: The refresh token.
    - `settings`: The JWT settings.
  - Returns: A Result containing the new access token or an error.
- **GenerateRefreshToken** - Generates a refresh token with extended expiry.
  - Parameters:
    - `claims`: The claims to include in the refresh token.
    - `settings`: The JWT settings.
  - Returns: A Result containing the refresh token or an error.
- **ToBase64Url** - Encodes bytes to Base64Url format (no padding, + → -, / → _).
- **FromBase64Url** - Decodes Base64Url format to bytes.

### PasswordHelper

- **Namespace:** `SmartWorkz.Shared.PasswordHelper`
- **Summary:** Provides secure password generation and validation using cryptographically secure random number generation.

#### Methods & Properties

- **GeneratePassword** - Generates a cryptographically secure random password.
  - Parameters:
    - `length`: Length of the password (8-128, default 12).
    - `includeSpecialChars`: Whether to include special characters.
  - Returns: A Result containing the generated password or an error.
- **ValidateStrength** - Validates the strength of a password against a policy.
  - Parameters:
    - `password`: The password to validate.
    - `policy`: The policy to validate against (uses default if null).
  - Returns: A Result containing the validation result.
- **GetRandomChar** - Gets a random character from the specified character set using cryptographic randomness.
- **Shuffle** - Performs Fisher-Yates shuffle on the character array.
- **CheckPasswordLength** - Checks if password meets minimum length requirement.
- **CheckUppercase** - Checks if password contains at least one uppercase letter.
- **CheckLowercase** - Checks if password contains at least one lowercase letter.
- **CheckNumbers** - Checks if password contains at least one digit.
- **CheckSpecialChars** - Checks if password contains at least one special character.

### PasswordPolicy

- **Namespace:** `SmartWorkz.Shared.PasswordPolicy`
- **Summary:** Policy for password validation requirements.

#### Methods & Properties

- **Validate** - Validates the policy invariants.

### PasswordValidationResult

- **Namespace:** `SmartWorkz.Shared.PasswordValidationResult`
- **Summary:** Result of password validation against a policy.

### ITemplateEngine

- **Namespace:** `SmartWorkz.Shared.ITemplateEngine`
- **Summary:** Defines operations for rendering templates with placeholder substitution.

#### Methods & Properties

- **Render** - Renders a template string by replacing placeholders with values from a dictionary.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `values`: Dictionary of key-value pairs to substitute into the template.
  - Returns: The rendered content with placeholders replaced. Unmatched placeholders remain unchanged.
- **Render** - Renders a template string by extracting properties from an object model.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `model`: The model object whose public properties are used for placeholder substitution.
  - Returns: The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.
- **RenderFileAsync** - Asynchronously renders a template file by replacing placeholders with values from a dictionary.
  - Parameters:
    - `filePath`: The path to the template file.
    - `values`: Dictionary of key-value pairs to substitute into the template.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **RenderFileAsync** - Asynchronously renders a template file by extracting properties from an object model.
  - Parameters:
    - `filePath`: The path to the template file.
    - `model`: The model object whose public properties are used for placeholder substitution.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **LoadDirectoryAsync** - Asynchronously loads all template files from a directory into a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.
- **RenderDirectoryAsync** - Asynchronously loads and renders all template files from a directory using values from a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `values`: Dictionary of key-value pairs to substitute into each template.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.

### TemplateEngine

- **Namespace:** `SmartWorkz.Shared.TemplateEngine`
- **Summary:** Provides template rendering services with support for placeholder substitution.

#### Methods & Properties

- **PlaceholderRegex** - 
- **Render** - Renders a template string by replacing placeholders with values from a dictionary.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `values`: Dictionary of key-value pairs to substitute into the template.
  - Returns: The rendered content with placeholders replaced. Unmatched placeholders and null/empty content remain unchanged.
- **Render** - Renders a template string by extracting properties from an object model.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `model`: The model object whose public properties are used for placeholder substitution.
  - Returns: The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.
- **RenderFileAsync** - Asynchronously renders a template file by replacing placeholders with values from a dictionary.
  - Parameters:
    - `filePath`: The path to the template file.
    - `values`: Dictionary of key-value pairs to substitute into the template.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **RenderFileAsync** - Asynchronously renders a template file by extracting properties from an object model.
  - Parameters:
    - `filePath`: The path to the template file.
    - `model`: The model object whose public properties are used for placeholder substitution.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **LoadDirectoryAsync** - Asynchronously loads all template files from a directory into a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.
- **RenderDirectoryAsync** - Asynchronously loads and renders all template files from a directory using values from a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `values`: Dictionary of key-value pairs to substitute into each template.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.
- **ReflectModel** - Reflects over a model object and builds a case-insensitive dictionary of public properties
            mapped to their string values. Uses cached property metadata for performance.
- **ValidateFilePath** - Validates a file path to prevent directory traversal attacks.
  - Parameters:
    - `filePath`: The file path to validate.
  - Returns: A result indicating if the path is valid and safe.

### CompressHelper

- **Namespace:** `SmartWorkz.Shared.CompressHelper`
- **Summary:** Provides utilities for GZip compression and decompression.

#### Methods & Properties

- **CompressString** - Compresses a string using GZip compression.
- **DecompressString** - Decompresses a GZip-compressed byte array back to a string.
- **CompressBytes** - Compresses a byte array using GZip compression.
- **DecompressBytes** - Decompresses a GZip-compressed byte array.

### DateHelper

- **Namespace:** `SmartWorkz.Shared.DateHelper`
- **Summary:** Provides utilities for date and time operations.

#### Methods & Properties

- **GetAge** - Calculates the age in years from a birth date to today.
- **GetRelativeTime** - Returns a human-readable relative time string (e.g., "2 days ago", "in 3 hours").
- **StartOfDay** - Returns the start of the day (00:00:00) for the given date.
- **EndOfDay** - Returns the end of the day (23:59:59.999) for the given date.
- **IsWeekend** - Determines if the given date falls on a weekend (Saturday or Sunday).
- **GetDayOfWeekName** - Returns the name of the day of week (e.g., "Monday", "Tuesday").
- **DaysBetween** - Calculates the number of days between two dates (inclusive of the from date, exclusive of the to date).

### EnumHelper

- **Namespace:** `SmartWorkz.Shared.EnumHelper`
- **Summary:** Provides utilities for enum operations including reflection and description retrieval.

#### Methods & Properties

- **GetDescription** - Gets the description of an enum value from its [Description] attribute.
            Falls back to the enum name if no description is found.
- **GetValue``1** - Attempts to get an enum value by its name.
- **GetAllValues``1** - Returns all values of the specified enum type as a list.
- **GetName** - Gets the name of an enum value.

### MathHelper

- **Namespace:** `SmartWorkz.Shared.MathHelper`
- **Summary:** Provides utilities for common math operations.

#### Methods & Properties

- **Percentage** - Calculates the percentage of a value.
            Example: Percentage(100, 20) returns 20 (20% of 100).
- **PercentageChange** - Calculates the percentage change from oldValue to newValue.
            Positive result indicates increase, negative indicates decrease.
- **RoundTo** - Rounds a decimal value to the specified number of decimal places.
- **Clamp``1** - Clamps a value within a specified range [min, max].
- **Average** - Calculates the average of the provided decimal values.

### SlugHelper

- **Namespace:** `SmartWorkz.Shared.SlugHelper`
- **Summary:** Helper for generating URL-friendly slugs from text input.

#### Methods & Properties

- **GenerateSlug** - Generates a URL-friendly slug from the given text with optional configuration.
  - Parameters:
    - `text`: The input text to convert to a slug.
    - `options`: Configuration options. If null, default options are used.
  - Returns: A Result containing the generated slug or an error.
- **ToSlug** - Generates a URL-friendly slug from the given text using default options.
            Convenience method equivalent to GenerateSlug(text, null).
  - Parameters:
    - `text`: The input text to convert to a slug.
  - Returns: A Result containing the generated slug or an error.
- **RemoveAccents** - Removes accented characters from text by decomposing them and filtering out combining marks.
            For example: "café" → "cafe", "naïve" → "naive", "Señor" → "Senor".
  - Parameters:
    - `input`: The input text potentially containing accented characters.
  - Returns: The text with accented characters converted to their base forms.
- **ReplaceSpecialCharacters** - Replaces special characters and spaces with the specified separator.
            Keeps only alphanumeric characters and the separator.
  - Parameters:
    - `input`: The input text.
    - `separator`: The separator to use for special characters and spaces.
  - Returns: The text with special characters replaced by the separator.

### SlugOptions

- **Namespace:** `SmartWorkz.Shared.SlugOptions`
- **Summary:** Options for configuring slug generation behavior in .

### TextHelper

- **Namespace:** `SmartWorkz.Shared.TextHelper`
- **Summary:** Sealed class providing advanced text processing and formatting utilities.
            All methods return Result<string> for consistent error handling.

#### Methods & Properties

- **Truncate** - Truncates text to a maximum length and appends a suffix (default "...").
  - Parameters:
    - `text`: The input text to truncate.
    - `maxLength`: The maximum length including the suffix.
    - `suffix`: The suffix to append when truncating. Defaults to "...".
  - Returns: A Result containing the truncated text or an error.
- **Capitalize** - Capitalizes the first letter of the text while preserving the rest.
  - Parameters:
    - `text`: The input text to capitalize.
  - Returns: A Result containing the capitalized text or an error.
- **Decapitalize** - Decapitalizes the first letter of the text while preserving the rest.
  - Parameters:
    - `text`: The input text to decapitalize.
  - Returns: A Result containing the decapitalized text or an error.
- **StripHtml** - Removes HTML tags from the input string using regex.
  - Parameters:
    - `html`: The HTML string to process.
  - Returns: A Result containing the plain text with HTML tags removed or an error.
- **Pluralize** - Pluralizes a word based on count using a simple heuristic.
            If count == 1, returns singular form. Otherwise appends 's'.
  - Parameters:
    - `singular`: The singular form of the word.
    - `count`: The count to determine plural form.
  - Returns: A Result containing the appropriately pluralized word or an error.
- **TitleCase** - Converts text to title case by capitalizing the first letter of each word.
  - Parameters:
    - `text`: The input text to convert.
  - Returns: A Result containing the title-cased text or an error.
- **Reverse** - Reverses the input string.
  - Parameters:
    - `text`: The input text to reverse.
  - Returns: A Result containing the reversed text or an error.
- **RemoveWhitespace** - Removes all whitespace characters from the input string.
  - Parameters:
    - `text`: The input text to process.
  - Returns: A Result containing the text with all whitespace removed or an error.
- **WordWrap** - Wraps text at a specified line length while preserving word boundaries.
  - Parameters:
    - `text`: The input text to wrap.
    - `lineLength`: The maximum length of each line.
    - `newline`: The newline character(s) to use. Defaults to "\n".
  - Returns: A Result containing the word-wrapped text or an error.
- **Repeat** - Repeats the input string the specified number of times.
  - Parameters:
    - `text`: The input text to repeat.
    - `count`: The number of times to repeat the text.
  - Returns: A Result containing the repeated text or an error.

### CompositeValidator`1

- **Namespace:** `SmartWorkz.Shared.CompositeValidator`1`
- **Summary:** Combines multiple validators into a single validator.
            Useful for composing validators from different sources.

### IValidationRule`2

- **Namespace:** `SmartWorkz.Shared.IValidationRule`2`
- **Summary:** Single validation rule for a property.

#### Methods & Properties

- **ValidateAsync** - Validate property and return results.

### ValidationRule`2

- **Namespace:** `SmartWorkz.Shared.ValidationRule`2`
- **Summary:** Base implementation for custom validation rules.

### ValidationRules

- **Namespace:** `SmartWorkz.Shared.ValidationRules`
- **Summary:** Pre-built validation rules for common scenarios.

### ValidatorBuilder`1

- **Namespace:** `SmartWorkz.Shared.ValidatorBuilder`1`
- **Summary:** Fluent validator builder for defining validation rules.
            Provides an alternative to ValidatorBase for more concise validator definitions.

#### Methods & Properties

- **RuleFor``1** - Add a rule for a property using fluent API.
- **ValidateAsync** - Validate instance against all rules.

### IWebhookRegistry

- **Namespace:** `SmartWorkz.Shared.IWebhookRegistry`
- **Summary:** Abstraction for managing webhook subscriptions and registrations.
            Supports CRUD operations and subscription queries.

#### Methods & Properties

- **RegisterAsync** - Register a new webhook subscription.
  - Parameters:
    - `url`: The webhook endpoint URL.
    - `events`: Array of event names to subscribe to.
    - `secret`: Optional HMAC-SHA256 secret for signature verification.
    - `cancellationToken`: Cancellation token.
  - Returns: The ID of the newly registered subscription.
- **UnregisterAsync** - Unregister and remove a webhook subscription.
  - Parameters:
    - `subscriptionId`: The subscription ID to unregister.
    - `cancellationToken`: Cancellation token.
- **GetSubscriptionsForEventAsync** - Get all active subscriptions for a specific event.
  - Parameters:
    - `eventName`: The event name to filter by.
    - `cancellationToken`: Cancellation token.
  - Returns: Collection of subscriptions interested in this event.
- **GetActiveSubscriptionsAsync** - Get all currently active subscriptions.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: Collection of all active subscriptions.
- **UpdateSubscriptionStatusAsync** - Update the status and failure tracking of a subscription.
  - Parameters:
    - `subscriptionId`: The subscription ID to update.
    - `isActive`: Whether the subscription should remain active.
    - `failureCount`: Number of consecutive failures (null to leave unchanged).
    - `failureReason`: Reason for failure (null to clear).
    - `cancellationToken`: Cancellation token.

### SqlWebhookRegistry

- **Namespace:** `SmartWorkz.Shared.SqlWebhookRegistry`
- **Summary:** SQL Server implementation of IWebhookRegistry.
            Persists webhook subscriptions to the database with support for querying and status updates.

### WebhookDeliveryService

- **Namespace:** `SmartWorkz.Shared.WebhookDeliveryService`
- **Summary:** Service for publishing domain events to registered webhook endpoints.
            Implements exponential backoff retry logic, HMAC signature verification, and failure tracking.

#### Methods & Properties

- **PublishEventAsync** - Publish an event to all subscribed webhook endpoints.
  - Parameters:
    - `eventName`: The name of the event being published.
    - `payload`: The event payload to send.
    - `cancellationToken`: Cancellation token.
- **DeliverAsync** - Deliver an event to a single webhook endpoint with exponential backoff retry logic.
- **GenerateSignature** - Generate HMAC-SHA256 signature for webhook payload verification.

### AuditEntry

- **Namespace:** `SmartWorkz.Shared.AuditEntry`
- **Summary:** Immutable audit log entry for tracking entity changes and domain events.
            Records who did what, when, where, and why for compliance and debugging.

### AuditEventSubscriber

- **Namespace:** `SmartWorkz.Shared.AuditEventSubscriber`
- **Summary:** Subscribes to domain events and records them in the audit trail.
            Enables automatic audit capture without requiring explicit audit calls in business logic.

#### Methods & Properties

- **OnEventPublishedAsync** - Record a domain event in the audit trail.
  - Parameters:
    - `evt`: The domain event to record.
    - `userId`: User ID who triggered the event (optional for system events).
    - `ipAddress`: IP address of the request originator (optional).
    - `cancellationToken`: Cancellation token.

### AuditStartupExtensions

- **Namespace:** `SmartWorkz.Shared.AuditStartupExtensions`
- **Summary:** Dependency injection and schema setup for audit trail functionality.

#### Methods & Properties

- **AddAuditTrail** - Register IAuditTrail with SQL Server implementation.
- **CreateAuditTrailSchema** - Create the AuditTrail table and indexes if they don't exist.
            Call this during application startup or migration.

### IAuditTrail

- **Namespace:** `SmartWorkz.Shared.IAuditTrail`
- **Summary:** Service for recording and querying immutable audit entries.
            Abstracts the persistence mechanism for audit trails.

#### Methods & Properties

- **RecordAsync** - Record an audit entry (immutable append-only).
  - Parameters:
    - `entry`: The audit entry to record.
    - `cancellationToken`: Cancellation token.
- **GetEntriesAsync** - Get all audit entries for a specific entity instance.
  - Parameters:
    - `entityType`: Type of entity (e.g., "Order").
    - `entityId`: Entity instance ID.
    - `cancellationToken`: Cancellation token.
- **GetEntriesByActionAsync** - Get audit entries by action type (Created, Updated, Deleted, etc.).
  - Parameters:
    - `action`: The action to filter by.
    - `since`: Optional: only entries after this date.
    - `cancellationToken`: Cancellation token.
- **GetEntriesByUserAsync** - Get audit entries for a specific user.
  - Parameters:
    - `userId`: User ID who performed actions.
    - `since`: Optional: only entries after this date.
    - `cancellationToken`: Cancellation token.
- **SearchAsync** - Search audit trail with multiple filter criteria.
            All criteria are AND'd together (null criteria are ignored).
  - Parameters:
    - `entityType`: Optional entity type filter.
    - `action`: Optional action filter.
    - `userId`: Optional user ID filter.
    - `since`: Optional timestamp filter (inclusive).
    - `cancellationToken`: Cancellation token.

### SqlAuditTrail

- **Namespace:** `SmartWorkz.Shared.SqlAuditTrail`
- **Summary:** SQL Server implementation of IAuditTrail for immutable audit log persistence.
            Appends audit entries to a single table with indexes for efficient querying.

#### Methods & Properties

- **RecordAsync** - 
- **GetEntriesAsync** - 
- **GetEntriesByActionAsync** - 
- **GetEntriesByUserAsync** - 
- **SearchAsync** - 

### ValueConverter`1

- **Namespace:** `SmartWorkz.Shared.ValueConverter`1`
- **Summary:** Abstract base class for type conversion between domain objects and DTOs.
            Enables loose coupling between layers by centralizing conversion logic.

#### Methods & Properties

- **Convert``1** - Convert a single source object to target type.
- **Convert** - Convert a single source object using dynamic target type resolution.
- **ConvertList``1** - Convert a collection of source objects to target type.
- **ConvertList** - Convert a collection using dynamic target type resolution.
- **ConvertFromList``2** - Convert from a collection of different source types.

### CacheEntry`1

- **Namespace:** `SmartWorkz.Shared.CacheEntry`1`
- **Summary:** Represents a cached entry with data, expiration time, and metadata.

#### Methods & Properties

- **#ctor** - Creates a new CacheEntry instance.
- **#ctor** - Creates a new CacheEntry instance with data and expiration.
- **RenewExpiry** - Renews the expiry time based on the cache strategy and TTL.

### CacheEntryWrapper

- **Namespace:** `SmartWorkz.Shared.CacheEntryWrapper`
- **Summary:** Non-generic wrapper for CacheEntry to store in the cache dictionary.

### CacheOptions

- **Namespace:** `SmartWorkz.Shared.CacheOptions`
- **Summary:** Configuration options for cache operations.

#### Methods & Properties

- **#ctor** - Creates a new CacheOptions instance with default values.
- **#ctor** - Creates a new CacheOptions instance with specified TTL.
- **#ctor** - Creates a new CacheOptions instance with specified TTL and cache strategy.
- **#ctor** - Creates a new CacheOptions instance with all parameters.

### CacheStrategy

- **Namespace:** `SmartWorkz.Shared.CacheStrategy`
- **Summary:** Enumeration of cache expiration strategies.

### ICacheService

- **Namespace:** `SmartWorkz.Shared.ICacheService`
- **Summary:** Service for caching with tenant isolation and L1/L2 hybrid support.
            Implementations may use memory cache (L1) and distributed cache (L2).
            All cache operations are tenant-scoped with automatic key prefixing.

#### Methods & Properties

- **GetAsync``1** - Gets a cached value by key with tenant isolation.
  - Parameters:
    - `key`: Cache key (will be tenant-scoped automatically).
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Result containing cached value, or failure if not found or expired.
- **SetAsync``1** - Sets a value in cache with optional TTL for the specified tenant.
  - Parameters:
    - `key`: Cache key (will be tenant-scoped automatically).
    - `value`: Value to cache.
    - `ttlMinutes`: Time to live in minutes. If null, value never expires.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveAsync** - Removes a cached value by key for the specified tenant.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveByPrefixAsync** - Removes all cached values matching a key prefix for the specified tenant.
            Example: RemoveByPrefixAsync("user:") removes all "user:*" entries for that tenant.
  - Parameters:
    - `prefix`: Key prefix to match (may include wildcard suffix like "user:*").
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **ExistsAsync** - Checks if a key exists in cache for the specified tenant.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: True if key exists and is not expired; false otherwise.
- **ClearAsync** - Clears all cache entries for the specified tenant.
            Does not affect entries for other tenants.
  - Parameters:
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.

### ICacheStore

- **Namespace:** `SmartWorkz.Shared.ICacheStore`
- **Summary:** Abstraction for a cache store with support for various operations including TTL and expiration strategies.

#### Methods & Properties

- **GetAsync``1** - Retrieves a value from the cache.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result containing the cached value or null if not found or expired.
- **SetAsync``1** - Sets a value in the cache with optional TTL.
  - Parameters:
    - `key`: The cache key.
    - `value`: The value to cache.
    - `ttlMinutes`: Optional time-to-live in minutes. If null, uses default or no expiration.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **SetAsync``1** - Sets a value in the cache with cache options.
  - Parameters:
    - `key`: The cache key.
    - `value`: The value to cache.
    - `options`: Cache options including TTL, strategy, and sliding expiration.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **RemoveAsync** - Removes a value from the cache.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **RemoveByPrefixAsync** - Removes all values from the cache that match the specified key prefix.
  - Parameters:
    - `keyPrefix`: The prefix to match.
    - `ct`: Cancellation token.
  - Returns: A Result containing the number of entries removed.
- **ExistsAsync** - Checks if a key exists in the cache and is not expired.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result indicating whether the key exists and is valid.
- **ClearAsync** - Clears all entries from the cache.
  - Parameters:
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.

### MemoryCacheService

- **Namespace:** `SmartWorkz.Shared.MemoryCacheService`
- **Summary:** In-memory L1 cache service implementation with thread-safe operations and tenant isolation.
            Suitable for single-process deployments with TTL and expiration support.

#### Methods & Properties

- **BuildKey** - Builds a tenant-scoped cache key.
  - Parameters:
    - `key`: Original cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
  - Returns: Tenant-scoped key in format "{tenantId}:{key}".
- **GetAsync``1** - Gets a cached value by key with tenant isolation. Returns failure if not found or expired.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Result containing cached value, or failure if not found or expired.
- **SetAsync``1** - Sets a cached value with optional TTL expiration and tenant isolation.
  - Parameters:
    - `key`: Cache key.
    - `value`: Value to cache.
    - `ttlMinutes`: Time to live in minutes. If null, no expiration.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveAsync** - Removes a single cache entry with tenant isolation.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveByPrefixAsync** - Removes all cache entries matching a prefix pattern with tenant isolation.
            Example: RemoveByPrefixAsync("user:*", "tenant1") removes "tenant1:user:*" entries.
  - Parameters:
    - `prefix`: Key prefix to match.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **ExistsAsync** - Checks if a key exists in the cache with tenant isolation (ignores expiration check).
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: True if key exists and is not expired; false otherwise.
- **ClearAsync** - Clears all cache entries for the specified tenant (or "default" if not specified).
            Does not clear entries from other tenants.
  - Parameters:
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.

### MemoryCacheStore

- **Namespace:** `SmartWorkz.Shared.MemoryCacheStore`
- **Summary:** In-memory implementation of ICacheStore with TTL support and thread-safe operations.

#### Methods & Properties

- **#ctor** - Creates a new instance of MemoryCacheStore with default options.
- **#ctor** - Creates a new instance of MemoryCacheStore with specified default options.
- **GetAsync``1** - Retrieves a value from the cache.
- **SetAsync``1** - Sets a value in the cache with optional TTL.
- **SetAsync``1** - Sets a value in the cache with cache options.
- **RemoveAsync** - Removes a value from the cache.
- **RemoveByPrefixAsync** - Removes all values from the cache that match the specified key prefix.
- **ExistsAsync** - Checks if a key exists in the cache and is not expired.
- **ClearAsync** - Clears all entries from the cache.
- **CleanupExpiredEntries** - Performs cleanup of expired entries. This is useful for periodic maintenance.

### ISmsService

- **Namespace:** `SmartWorkz.Shared.ISmsService`
- **Summary:** Defines a contract for SMS communication services.
            Provides methods for sending SMS messages to single or multiple recipients.

#### Methods & Properties

- **SendAsync** - Sends an SMS message to a single recipient.
  - Parameters:
    - `phoneNumber`: The recipient phone number (E.164 format recommended)
    - `message`: The SMS message content
    - `cancellationToken`: Cancellation token
  - Returns: Result containing the SMS ID if successful
- **SendBatchAsync** - Sends an SMS message to multiple recipients (batch).
  - Parameters:
    - `phoneNumbers`: Collection of recipient phone numbers
    - `message`: The SMS message content sent to all recipients
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of SMS IDs if successful

### IWebSocketClient

- **Namespace:** `SmartWorkz.Shared.IWebSocketClient`
- **Summary:** Abstraction for WebSocket client operations.

#### Methods & Properties

- **SendAsync** - Sends a message asynchronously through the WebSocket connection.
- **ReceiveAsync** - Receives a message asynchronously from the WebSocket connection.
            Returns null if the connection is closed.
- **CloseAsync** - Closes the WebSocket connection gracefully.

### WebSocketClient

- **Namespace:** `SmartWorkz.Shared.WebSocketClient`
- **Summary:** Sealed implementation of IWebSocketClient using System.Net.WebSockets.

#### Methods & Properties

- **ConnectAsync** - Connects to a WebSocket server at the specified URI.
- **SendAsync** - Sends a message asynchronously through the WebSocket connection.
- **ReceiveAsync** - Receives a message asynchronously from the WebSocket connection.
            Returns null if the connection is closed.
- **CloseAsync** - Closes the WebSocket connection gracefully.

### ConfigurationHelper

- **Namespace:** `SmartWorkz.Shared.ConfigurationHelper`
- **Summary:** Provides typed access to configuration values with automatic type conversion and validation.
            
             This sealed class implements IConfigurationHelper to provide a strongly-typed interface
             for accessing configuration values. It supports automatic type conversion for common types
             including strings, numeric types, booleans, DateTimes, and enums.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ConfigurationHelper class.
  - Parameters:
    - `configuration`: The configuration source to read from.
- **GetRequired``1** - Gets a required configuration value and converts it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: The configuration value converted to type T.
- **GetOptional``1** - Gets an optional configuration value and converts it to the specified type,
            returning a default value if the key is not found or conversion fails.
  - Parameters:
    - `key`: The configuration key to retrieve.
    - `defaultValue`: The value to return if the key is not found or conversion fails.
  - Returns: The configuration value converted to type T, or the default value if retrieval or conversion fails.
- **TryGet``1** - Attempts to get a configuration value and convert it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: A Result<T> containing the converted value on success, or a failure result
            if the key is not found or conversion fails.
- **Exists** - Checks whether a configuration key exists and is not empty.
  - Parameters:
    - `key`: The configuration key to check.
  - Returns: True if the key exists and is not null or whitespace; otherwise false.
- **ConvertValue``1** - Converts a string value to the specified type using invariant culture for numeric types.
  - Parameters:
    - `value`: The string value to convert.
  - Returns: The converted value of type T.

### ConfigurationValidationException

- **Namespace:** `SmartWorkz.Shared.ConfigurationValidationException`
- **Summary:** Exception thrown when configuration validation fails, indicating that a required
            configuration key is missing, empty, or cannot be converted to the requested type.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ConfigurationValidationException class with a specified message.
  - Parameters:
    - `message`: The error message that explains the reason for the exception.
- **#ctor** - Initializes a new instance of the ConfigurationValidationException class with a specified message
            and a reference to the inner exception that is the cause of this exception.
  - Parameters:
    - `message`: The error message that explains the reason for the exception.
    - `innerException`: The exception that is the cause of the current exception.

### IConfigurationHelper

- **Namespace:** `SmartWorkz.Shared.IConfigurationHelper`
- **Summary:** Provides typed access to configuration values with automatic type conversion and validation.

#### Methods & Properties

- **GetRequired``1** - Gets a required configuration value and converts it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: The configuration value converted to type T.
- **GetOptional``1** - Gets an optional configuration value and converts it to the specified type,
            returning a default value if the key is not found or conversion fails.
  - Parameters:
    - `key`: The configuration key to retrieve.
    - `defaultValue`: The value to return if the key is not found or conversion fails.
  - Returns: The configuration value converted to type T, or the default value if retrieval or conversion fails.
- **TryGet``1** - Attempts to get a configuration value and convert it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: A Result<T> containing the converted value on success, or a failure result
            if the key is not found or conversion fails.
- **Exists** - Checks whether a configuration key exists and is not empty.
  - Parameters:
    - `key`: The configuration key to check.
  - Returns: True if the key exists and is not null or whitespace; otherwise false.

### SharedConstants

- **Namespace:** `SmartWorkz.Shared.SharedConstants`
- **Summary:** Shared configuration constants used throughout SmartWorkz.Shared.
            Enables centralized management of default values and limits.

### ICommand

- **Namespace:** `SmartWorkz.Shared.ICommand`
- **Summary:** Marker interface for command objects representing intent to change state.

### ICommandHandler`1

- **Namespace:** `SmartWorkz.Shared.ICommandHandler`1`
- **Summary:** Handler for processing a specific command type.

#### Methods & Properties

- **HandleAsync** - Handles the specified command asynchronously.
  - Parameters:
    - `command`: The command to handle.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task that represents the asynchronous handle operation.

### IQuery`1

- **Namespace:** `SmartWorkz.Shared.IQuery`1`
- **Summary:** Marker interface for query objects that return a result without modifying state.

### IQueryHandler`2

- **Namespace:** `SmartWorkz.Shared.IQueryHandler`2`
- **Summary:** Handler for processing a specific query type and returning results.

#### Methods & Properties

- **HandleAsync** - Handles the specified query asynchronously and returns the result.
  - Parameters:
    - `query`: The query to handle.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task that represents the asynchronous handle operation and contains the query result.

### MediatorCommandDispatcher

- **Namespace:** `SmartWorkz.Shared.MediatorCommandDispatcher`
- **Summary:** Routes commands to their appropriate handlers via dependency injection.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `serviceProvider`: The service provider for resolving handlers.
- **DispatchAsync``1** - Dispatches the specified command to its handler asynchronously.
  - Parameters:
    - `command`: The command to dispatch.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task representing the asynchronous dispatch operation.

### AdoHelper

- **Namespace:** `SmartWorkz.Shared.AdoHelper`
- **Summary:** ADO.NET helper for executing queries and managing connections.
            Works with any IDbProvider implementation.

#### Methods & Properties

- **ExecuteScalarAsync``1** - Execute scalar query (returns single value).
- **ExecuteNonQueryAsync** - Execute non-query command (INSERT, UPDATE, DELETE).
- **ExecuteQueryAsync``1** - Execute query and map results to objects.
- **ExecuteStoredProcedureAsync** - Execute stored procedure.
- **ExecuteQueryMultipleAsync``2** - Execute query returning multiple result sets (2 sets).
- **ExecuteQueryMultipleAsync``3** - Execute query returning multiple result sets (3 sets).
- **ExecuteQueryMultipleAsync``4** - Execute query returning multiple result sets (4 sets).
- **ExecuteTransactionAsync** - Execute transaction with multiple commands.

### CsvHelper

- **Namespace:** `SmartWorkz.Shared.CsvHelper`
- **Summary:** Provides static methods for reading and writing CSV data with support for column mapping,
            quoted fields, embedded delimiters, and newlines.
            RFC 4180 compliant CSV parsing and writing.
            Sealed class to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **CsvWriter``1** - Serializes a collection of objects to CSV format.
  - Parameters:
    - `items`: The collection of objects to serialize.
    - `options`: CSV options. If null, defaults are used.
  - Returns: A Result containing the CSV string if successful; otherwise a failure.
- **CsvReader``1** - Asynchronously deserializes CSV content to a collection of objects.
  - Parameters:
    - `content`: The CSV content string.
    - `mapping`: Column mapping configuration. If null, property names are used as headers.
    - `options`: CSV options. If null, defaults are used.
  - Returns: A Result containing the deserialized list if successful; otherwise a failure.
- **ParseCsvLines** - Parses CSV content into a list of records (each record is a list of field values).
            Handles quoted fields with embedded delimiters and newlines.
- **WriteRecord** - Writes a single CSV record (list of field values) to the string builder.
            Handles quoting of fields with special characters.
- **ConvertValue** - Converts a string value to the specified type.
- **IsNullableType** - Determines if a type is nullable (Nullable<T> or reference type).

### CsvMapping`1

- **Namespace:** `SmartWorkz.Shared.CsvMapping`1`
- **Summary:** Defines column mapping for CSV operations using a fluent API.
            Supports mapping object properties to CSV columns with custom headers.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **Column``1** - Adds a column mapping for the specified property.
  - Parameters:
    - `propertyExpression`: Expression selecting the property to map.
    - `csvHeader`: The CSV column header name.
  - Returns: This instance for method chaining.
- **ExtractPropertyInfo``1** - Extracts property information from a lambda expression.
  - Parameters:
    - `expression`: The lambda expression.
  - Returns: The PropertyInfo if the expression resolves to a property; otherwise null.
- **CreateAuto** - Creates a mapping automatically from all public properties of type T.
            Property names are used as CSV headers.
  - Returns: A new CsvMapping instance with all properties mapped.

### CsvOptions

- **Namespace:** `SmartWorkz.Shared.CsvOptions`
- **Summary:** Configuration options for CSV read/write operations.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **#ctor** - Creates a default instance of CsvOptions.
- **#ctor** - Creates an instance of CsvOptions with specified delimiter and quote character.
  - Parameters:
    - `delimiter`: The field delimiter character.
    - `quoteChar`: The quote character for quoted fields.

### DbProviderFactory

- **Namespace:** `SmartWorkz.Shared.DbProviderFactory`
- **Summary:** Factory for creating database provider instances.
            Resolves provider name from connection string or explicit specification.

#### Methods & Properties

- **Register** - Register custom provider implementation.
- **GetProvider** - Get provider by name.
- **GetProvider** - Get provider by enum value.
- **GetProviderFromConnectionString** - Get provider from connection string (detects provider automatically).

### IDbProvider

- **Namespace:** `SmartWorkz.Shared.IDbProvider`
- **Summary:** Abstraction for database provider-specific operations.
            Supports multiple providers: SQL Server, MySQL, PostgreSQL, SQLite, Oracle.

#### Methods & Properties

- **CreateConnection** - Create connection with connection string.
- **GetParameterPrefix** - Get parameter prefix for this provider (@, :, $).
- **GetLastInsertIdSql** - Get SQL for last inserted ID based on provider.
- **GetPaginationSql** - Get SQL for pagination based on provider.
- **FormatIdentifier** - Format table/column name for provider (e.g., [brackets] for SQL Server).
- **TestConnectionAsync** - Test connection validity.

### DatabaseProvider

- **Namespace:** `SmartWorkz.Shared.DatabaseProvider`
- **Summary:** Enum of supported database providers.

### QueryMultipleHelper

- **Namespace:** `SmartWorkz.Shared.QueryMultipleHelper`
- **Summary:** Helper for executing multiple queries in a single database roundtrip.
            Eliminates N+1 query problems by batching queries together.

#### Methods & Properties

- **QueryMultipleAsync``2** - Execute multiple queries and return results as tuple.
             Single roundtrip, single SQL execution, improved performance.
- **QueryMultipleAsync``3** - Execute 3 queries in single roundtrip.
- **QueryMultipleAsync``4** - Execute 4 queries in single roundtrip.
- **QueryMultipleAsync``5** - Execute 5 queries in single roundtrip.

### QueryResult`1

- **Namespace:** `SmartWorkz.Shared.QueryResult`1`
- **Summary:** Result wrapper for query operations.

### XmlHelper

- **Namespace:** `SmartWorkz.Shared.XmlHelper`
- **Summary:** Provides static methods for XML serialization, deserialization, and XPath queries.
            Uses System.Xml.Linq for manipulation and reflection for property mapping.
            Sealed class to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **Serialize``1** - Serializes an object to an XML string using reflection.
  - Parameters:
    - `obj`: The object to serialize.
    - `options`: XML options. If null, defaults are used.
  - Returns: A Result containing the XML string if successful; otherwise a failure.
- **Deserialize``1** - Deserializes an XML string to an object of type T using reflection.
  - Parameters:
    - `xml`: The XML string to deserialize.
    - `options`: XML options. If null, defaults are used.
  - Returns: A Result containing the deserialized object if successful; otherwise a failure.
- **Query** - Executes an XPath query on an XML string and returns matching element values.
  - Parameters:
    - `xml`: The XML string to query.
    - `xpathExpression`: The XPath expression to execute.
  - Returns: A Result containing a list of matched values if successful; otherwise a failure.
- **SerializeObject** - Recursively serializes an object's properties into an XML element.
- **DeserializeObject** - Recursively deserializes an XML element into an object's properties.
- **IsBasicType** - Determines if a type is a basic/primitive type supported by XML.
- **IsGenericList** - Determines if a type is a generic List<T>.
- **IsComplexType** - Determines if a type is a complex (non-primitive) type.
- **ConvertToXmlValue** - Converts a value to its XML-safe string representation.
- **ConvertFromXmlValue** - Converts an XML string value to the specified type.

### XmlOptions

- **Namespace:** `SmartWorkz.Shared.XmlOptions`
- **Summary:** Configuration options for XML serialization, deserialization, and query operations.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **#ctor** - Creates a default instance of XmlOptions.
- **#ctor** - Creates an instance of XmlOptions with a specified root element name.
  - Parameters:
    - `rootElement`: The name of the root element.
- **#ctor** - Creates an instance of XmlOptions with specified configuration.
  - Parameters:
    - `rootElement`: The name of the root element.
    - `includeXmlDeclaration`: Whether to include the XML declaration.
    - `indent`: Whether to indent the output.

### ApplicationHealth

- **Namespace:** `SmartWorkz.Shared.ApplicationHealth`
- **Summary:** Represents the overall health status of the application.

### CorrelationContext

- **Namespace:** `SmartWorkz.Shared.CorrelationContext`
- **Summary:** A sealed implementation of  for distributed request tracing.

#### Methods & Properties

- **#ctor** - Initializes a new instance with a generated correlation ID.
- **#ctor** - Initializes a new instance with a specified correlation ID.
  - Parameters:
    - `correlationId`: The correlation ID to use
- **#ctor** - Initializes a child context from a parent context.

### CpuUsage

- **Namespace:** `SmartWorkz.Shared.CpuUsage`
- **Summary:** Represents CPU usage information.

### DiagnosticsHelper

- **Namespace:** `SmartWorkz.Shared.DiagnosticsHelper`
- **Summary:** Sealed helper class for system diagnostics and application health monitoring.
            Provides methods to gather system information, CPU/memory/disk usage, and determine application health.

#### Methods & Properties

- **Initialize** - Initializes the application start time (called once at application startup).
- **GetSystemInfo** - Gets comprehensive system information including CPU, memory, disk, and processor count.
  - Returns: A Result containing SystemInfo or error details.
- **GetMemoryUsage** - Gets memory usage statistics for the current process and system.
  - Returns: A Result containing MemoryUsage or error details.
- **GetCpuUsage** - Gets CPU utilization percentage.
  - Returns: A Result containing CpuUsage or error details.
- **GetDiskSpace** - Gets disk space information for a specific drive.
  - Parameters:
    - `drive`: The drive letter (e.g., "C:", "D:"). Defaults to "C:".
  - Returns: A Result containing DiskSpace or error details.
- **GetUptime** - Gets the application uptime since the last Initialize() call or application start.
  - Returns: A Result containing the uptime as a TimeSpan or error details.
- **GetApplicationHealth** - Gets the overall health status of the application based on system metrics.
  - Returns: A Result containing ApplicationHealth or error details.
- **IsHealthy** - Determines if the application is considered healthy based on the provided health status.
  - Parameters:
    - `health`: The ApplicationHealth object to evaluate.
  - Returns: True if the status is Healthy, false otherwise.
- **InitializeCpuCounter** - Initializes the CPU performance counter (called once).
- **GetMemoryUsageInternal** - Internal method to get memory usage statistics.
- **GetCpuUsageInternal** - Internal method to get CPU usage percentage.
- **GetDiskSpaceInternal** - Internal method to get disk space information.

### DiskSpace

- **Namespace:** `SmartWorkz.Shared.DiskSpace`
- **Summary:** Represents disk space information for a drive.

### HealthCheck

- **Namespace:** `SmartWorkz.Shared.HealthCheck`
- **Summary:** Represents a single health check result.

### HealthStatus

- **Namespace:** `SmartWorkz.Shared.HealthStatus`
- **Summary:** Represents the health status of the application.

### ICorrelationContext

- **Namespace:** `SmartWorkz.Shared.ICorrelationContext`
- **Summary:** Defines a correlation context for distributed request tracing across systems.

#### Methods & Properties

- **SetProperty** - Adds or updates a property in the correlation context.
- **TryGetProperty** - Attempts to retrieve a property from the correlation context.
- **CreateChildContext** - Creates a child correlation context for nested operations (for async/distributed flows).

### MemoryUsage

- **Namespace:** `SmartWorkz.Shared.MemoryUsage`
- **Summary:** Represents memory usage information.

### MetricsHelper

- **Namespace:** `SmartWorkz.Shared.MetricsHelper`
- **Summary:** Provides utilities for collecting and tracking performance metrics.

#### Methods & Properties

- **StartTimer** - Starts a timer and returns an IDisposable that logs elapsed time on disposal.
- **TrackExecution``1** - Tracks the execution time and result of a function.
- **MeasureMemory** - Captures memory usage before and after a block of code execution.

### SystemInfo

- **Namespace:** `SmartWorkz.Shared.SystemInfo`
- **Summary:** Represents system information including CPU, memory, and disk details.

### EventStoreSnapshot

- **Namespace:** `SmartWorkz.Shared.EventStoreSnapshot`
- **Summary:** Represents a snapshot of an aggregate's state at a specific version.
            Snapshots optimize event sourcing by reducing the number of events needed for reconstruction.

### IEventStore

- **Namespace:** `SmartWorkz.Shared.IEventStore`
- **Summary:** Abstraction for an immutable event store that persists domain events.
            Enables event sourcing patterns for temporal queries, audit trails, and event replay.

#### Methods & Properties

- **AppendEventsAsync** - Appends events to the event stream for an aggregate.
            Events are immutable and persist as an append-only log.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `events`: The domain events to append
    - `cancellationToken`: Cancellation token
  - Returns: Task representing the asynchronous operation
- **GetEventsAsync** - Retrieves all events for an aggregate in chronological order.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: Collection of domain events for the aggregate, empty if none exist
- **GetEventsSinceAsync** - Retrieves events for an aggregate after a specific version.
            Useful for incremental event replay and event streaming.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `version`: The version after which to retrieve events
    - `cancellationToken`: Cancellation token
  - Returns: Collection of events after the specified version, empty if none exist
- **GetSnapshotAsync** - Retrieves the latest snapshot for an aggregate if one exists.
            Snapshots optimize aggregate reconstruction by storing intermediate state.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: Snapshot data if exists; null if no snapshot is available
- **SaveSnapshotAsync** - Saves a snapshot of aggregate state at a specific version.
            Snapshots reduce the number of events needed to replay an aggregate.
  - Parameters:
    - `snapshot`: The snapshot to save
    - `cancellationToken`: Cancellation token
  - Returns: Task representing the asynchronous operation
- **GetAggregateAsync``1** - Reconstructs an aggregate from its event history.
            Optionally uses snapshots for performance optimization.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: The reconstructed aggregate instance, or null if no events exist

### SqlEventStore

- **Namespace:** `SmartWorkz.Shared.SqlEventStore`
- **Summary:** SQL Server implementation of the event store using Dapper for data access.
            Provides immutable append-only event log with snapshot support for optimization.
            Implements optimistic concurrency control using version numbers.

#### Methods & Properties

- **AppendEventsAsync** - Appends events to the event stream for an aggregate.
- **GetEventsAsync** - Retrieves all events for an aggregate in chronological order.
- **GetEventsSinceAsync** - Retrieves events for an aggregate after a specific version.
- **GetSnapshotAsync** - Retrieves the latest snapshot for an aggregate if one exists.
- **SaveSnapshotAsync** - Saves a snapshot of aggregate state at a specific version.
- **GetAggregateAsync``1** - Reconstructs an aggregate from its event history.
            Optionally uses snapshots for performance optimization.
- **GetCurrentVersionAsync** - Gets the current version number for an aggregate.
- **DeserializeEvent** - Deserializes a stored event record back to IDomainEvent.

### IDomainEvent

- **Namespace:** `SmartWorkz.Shared.IDomainEvent`
- **Summary:** Base interface for domain events in event-driven architecture.
            Provides core event metadata for tracking and publishing.

### IEventPublisher

- **Namespace:** `SmartWorkz.Shared.IEventPublisher`
- **Summary:** Publishes domain events for event-driven architecture.

#### Methods & Properties

- **PublishAsync``1** - Publishes a single domain event.
  - Parameters:
    - `event`: Event to publish.
    - `cancellationToken`: Cancellation token.
- **PublishAsync``1** - Publishes multiple domain events.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token.

### IEventSubscriber

- **Namespace:** `SmartWorkz.Shared.IEventSubscriber`
- **Summary:** Registers event handlers for domain events.

#### Methods & Properties

- **Subscribe``1** - Subscribes a handler to an event type.
            Multiple handlers can subscribe to the same event.
  - Parameters:
    - `handler`: Async handler function. Receives event and cancellation token.

### InMemoryEventPublisher

- **Namespace:** `SmartWorkz.Shared.InMemoryEventPublisher`
- **Summary:** In-memory event publisher that executes all registered handlers sequentially.
            Provides synchronous event delivery with exception handling and result reporting.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the InMemoryEventPublisher with a subscriber.
  - Parameters:
    - `subscriber`: The event subscriber containing registered handlers.
- **PublishAsync``1** - Publishes a single domain event to all registered handlers.
            Handlers are invoked sequentially in registration order.
            If any handler throws an exception, it is caught and a failure Result is returned.
            Other handlers will attempt to execute even if a previous handler fails.
  - Parameters:
    - `event`: The event instance to publish.
    - `cancellationToken`: Cancellation token to cancel handler execution.
  - Returns: A task representing the asynchronous operation.
- **PublishAsync``1** - Publishes multiple domain events to all registered handlers.
            Events are published sequentially in the order provided.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token to cancel handler execution.
  - Returns: A task representing the asynchronous batch operation.

### InMemoryEventSubscriber

- **Namespace:** `SmartWorkz.Shared.InMemoryEventSubscriber`
- **Summary:** In-memory event subscriber that maintains a registry of event handlers.
            Supports multiple handlers per event type using thread-safe concurrent collections.

#### Methods & Properties

- **Subscribe``1** - Subscribes a handler to an event type.
            Multiple handlers can be registered for the same event type and will execute sequentially.
  - Parameters:
    - `handler`: Async handler function that receives event and cancellation token.
- **GetHandlers** - Gets all registered handlers for a given event type.
            Returns an empty list if no handlers are registered for the type.
  - Parameters:
    - `eventType`: The event type to retrieve handlers for.
  - Returns: List of registered handlers (delegates).

### MassTransitEventPublisher

- **Namespace:** `SmartWorkz.Shared.MassTransitEventPublisher`
- **Summary:** Distributed event publisher using MassTransit message bus.
            Supports both single and batch event publishing with async/await patterns.
            Suitable for production environments with message broker backend (RabbitMQ, Azure Service Bus, etc).

#### Methods & Properties

- **#ctor** - Initializes a new instance of MassTransitEventPublisher.
  - Parameters:
    - `publishEndpoint`: MassTransit publish endpoint for message distribution.
    - `logger`: Logger for event publication tracking.
- **PublishAsync``1** - Publishes a single domain event to the message bus asynchronously.
  - Parameters:
    - `event`: Event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous publish operation.
- **PublishAsync``1** - Publishes multiple domain events to the message bus asynchronously.
            Events are published sequentially in the order provided.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous batch publish operation.

### PublisherType

- **Namespace:** `SmartWorkz.Shared.PublisherType`
- **Summary:** Specifies the publisher type for event publishing.

### ServiceCollectionExtensions

- **Namespace:** `SmartWorkz.Shared.ServiceCollectionExtensions`
- **Summary:** Extension methods for IServiceCollection to register Core.Shared services.

#### Methods & Properties

- **AddCoreSharedServices** - Adds Core.Shared services including TemplateEngine for template rendering.
- **AddEventPublishing** - Adds event publishing services to the dependency injection container.
            Supports switching between in-memory and MassTransit publishers based on application needs.
  - Parameters:
    - `services`: The service collection.
    - `publisherType`: The publisher type to use (defaults to InMemory).
  - Returns: The service collection for method chaining.

### DefaultFeatureFlagService

- **Namespace:** `SmartWorkz.Shared.DefaultFeatureFlagService`
- **Summary:** Global (non-tenant) feature flag service with in-memory storage.
            Thread-safe implementation suitable for single-process deployments.
            Use for organization-wide feature toggles; use ITenantFeatureFlags for tenant-scoped flags.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature flag is enabled.
            Returns false for unknown flags (does not throw).
  - Parameters:
    - `flagName`: The name of the feature flag to check.
    - `cancellationToken`: Cancellation token.
  - Returns: True if the flag exists and is enabled; false otherwise.
- **GetEnabledFeaturesAsync** - Gets all enabled feature flags.
            Returns empty list if no flags are enabled.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: A read-only list of enabled feature flag names.
- **EnableFlag** - Enables a feature flag.
            Creates the flag if it doesn't exist.
  - Parameters:
    - `flagName`: The name of the feature flag to enable.
- **DisableFlag** - Disables a feature flag.
            Creates the flag as disabled if it doesn't exist.
  - Parameters:
    - `flagName`: The name of the feature flag to disable.

### IFeatureFlagService

- **Namespace:** `SmartWorkz.Shared.IFeatureFlagService`
- **Summary:** Global feature flag service for cross-tenant feature control.
            Use for organization-wide feature toggles (not tenant-specific).
            For tenant-scoped flags, use ITenantFeatureFlags.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a global feature is enabled.
  - Parameters:
    - `flagName`: Feature flag name (e.g., "NEW_DASHBOARD", "BETA_REPORTING").
    - `cancellationToken`: Cancellation token.
  - Returns: True if feature is enabled globally.
- **GetEnabledFeaturesAsync** - Gets all enabled global features.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: List of enabled feature flag names.

### IFileStorageService

- **Namespace:** `SmartWorkz.Shared.IFileStorageService`
- **Summary:** Interface for file storage operations supporting both local and cloud providers.

#### Methods & Properties

- **UploadAsync** - Uploads a file to storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `content`: The file content stream.
    - `metadata`: The file metadata.
    - `cancellationToken`: The cancellation token.
  - Returns: The full path or URI of the uploaded file.
- **DownloadAsync** - Downloads a file from storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: A stream containing the file content. Caller must dispose using 'using' statement.
- **DeleteAsync** - Deletes a file from storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
- **ExistsAsync** - Checks if a file exists in storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: True if the file exists, false otherwise.
- **GetMetadataAsync** - Gets metadata for a file in storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: FileMetadata if file exists, null otherwise.
- **ListAsync** - Lists files in a directory or container prefix.
  - Parameters:
    - `folderPath`: The relative folder path or blob prefix.
    - `cancellationToken`: The cancellation token.
  - Returns: A read-only collection of FileMetadata for files in the directory/prefix.
- **GenerateTemporaryUrlAsync** - Generates a temporary download URL for a file.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `expiration`: The expiration duration from now.
    - `cancellationToken`: The cancellation token.
  - Returns: A URL that can be used to download the file. For local storage, returns the full file path.

### GridColumn

- **Namespace:** `SmartWorkz.Shared.GridColumn`
- **Summary:** Defines a single column in a grid, including display options, sorting, filtering, and rendering hints.

### GridExportOptions

- **Namespace:** `SmartWorkz.Shared.GridExportOptions`
- **Summary:** Configuration for grid data export (CSV, Excel).

### GridRequest

- **Namespace:** `SmartWorkz.Shared.GridRequest`
- **Summary:** Request parameters for grid data fetching, extending PagedQuery with filtering support.

#### Methods & Properties

- **#ctor** - Request parameters for grid data fetching, extending PagedQuery with filtering support.

### GridResponse`1

- **Namespace:** `SmartWorkz.Shared.GridResponse`1`
- **Summary:** Response from a grid data request, including paged data, column metadata, and filter options.

### IGridDataProvider

- **Namespace:** `SmartWorkz.Shared.IGridDataProvider`
- **Summary:** Abstraction for grid data fetching. Implementations handle API calls or in-memory queries.
            Enables platform independence: Web uses HTTP, MAUI uses direct API client, Desktop uses local DB.

#### Methods & Properties

- **GetDataAsync``1** - Fetch paged grid data based on request (sorting, filtering, pagination).
  - Parameters:
    - `request`: Grid request with sorting, paging, and filter criteria.
    - `cancellationToken`: Cancellation token for async operations.
  - Returns: Result containing GridResponse or error details.

### Guard

- **Namespace:** `SmartWorkz.Shared.Guard`
- **Summary:** Static guard clauses for argument validation at method entry points.
             Throw immediately on invalid input — fail fast, fail loudly.
            
             Usage:
               Guard.NotNull(userId, nameof(userId));
               Guard.NotEmpty(name, nameof(name));
               Guard.InRange(pageSize, 1, 100, nameof(pageSize));
            
             These replace the ValidationExtensions.EnsureNotNull() extension method
             and the scattered ArgumentNullException throws throughout the codebase.

#### Methods & Properties

- **NotNull``1** - Throws ArgumentNullException if value is null.
- **NotNull``1** - Throws ArgumentNullException if value is null (struct/nullable).
- **NotEmpty** - Throws ArgumentException if string is null, empty, or whitespace.
- **NotEmpty``1** - Throws ArgumentException if collection is null or has no elements.
- **NotDefault``1** - Throws ArgumentException if value equals the default for its type (0, null, Guid.Empty).
- **InRange``1** - Throws ArgumentOutOfRangeException if value is outside [min, max].
- **Requires** - Throws ArgumentException if condition is false.

### EncryptionHelper

- **Namespace:** `SmartWorkz.Shared.EncryptionHelper`
- **Summary:** Cryptographic utilities for hashing and encryption.
            Uses PBKDF2 for password hashing and AES-256 for data encryption.

#### Methods & Properties

- **HashPassword** - Hash password using PBKDF2 with SHA256.
- **VerifyPassword** - Verify password against hash.
- **Encrypt** - Encrypt text using AES-256-GCM with provided key.
- **Decrypt** - Decrypt text using AES-256-GCM with provided key.
- **GenerateRandomString** - Generate cryptographically secure random string.
- **GenerateEncryptionKey** - Generate random encryption key (Base64 encoded).
- **ComputeSha256** - Compute SHA256 hash of text for integrity checking.

### JsonHelper

- **Namespace:** `SmartWorkz.Shared.JsonHelper`
- **Summary:** JSON serialization utilities using System.Text.Json.
            Provides consistent serialization options across the application.

#### Methods & Properties

- **Serialize``1** - Serialize object to JSON string.
- **Serialize** - Serialize object to JSON string with dynamic type.
- **Deserialize``1** - Deserialize JSON string to object.
- **Deserialize** - Deserialize JSON string to object with dynamic type.
- **DeserializeAsync``1** - Deserialize JSON asynchronously from stream.
- **SerializeAsync``1** - Serialize asynchronously to stream.
- **IsValidJson** - Check if string is valid JSON.
- **GetValueByPath** - Parse JSON and extract value at specified path (dot notation).

### IHttpClient

- **Namespace:** `SmartWorkz.Shared.IHttpClient`
- **Summary:** Abstraction for HTTP client operations with support for async/await and cancellation.
            Implementations should handle retries, timeouts, and error responses gracefully.

#### Methods & Properties

- **GetAsync``1** - Sends a GET request and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **PostAsync``1** - Sends a POST request with JSON body and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **PutAsync``1** - Sends a PUT request with JSON body and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **DeleteAsync``1** - Sends a DELETE request and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **GetAsync** - Sends a GET request and returns a string response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.
- **PostAsync** - Sends a POST request with JSON body and returns a string response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.

### RetryStrategy

- **Namespace:** `SmartWorkz.Shared.RetryStrategy`
- **Summary:** Specifies the backoff strategy to use when retrying failed HTTP requests.

### RetryPolicy

- **Namespace:** `SmartWorkz.Shared.RetryPolicy`
- **Summary:** Configures automatic retry behavior for failed HTTP requests.

### AuditRecord

- **Namespace:** `SmartWorkz.Shared.AuditRecord`
- **Summary:** Represents an immutable audit record with all relevant audit information.

#### Methods & Properties

- **#ctor** - Represents an immutable audit record with all relevant audit information.
  - Parameters:
    - `Id`: The unique identifier of the audit record
    - `EntityType`: The type of entity being audited (e.g., "User", "BlogPost")
    - `EntityId`: The identifier of the audited entity
    - `Action`: The action performed (Create, Update, Delete, etc.)
    - `UserId`: The identifier of the user who performed the action
    - `PerformedAt`: The timestamp when the action was performed
    - `Metadata`: Optional metadata dictionary containing additional context

### EnrichedLogger

- **Namespace:** `SmartWorkz.Shared.EnrichedLogger`
- **Summary:** Enriched logger wrapper around ILogger that provides structured logging methods
            for domain events, commands, sagas, file operations, and background jobs.
            Uses structured properties instead of string interpolation for better queryability.

#### Methods & Properties

- **#ctor** - Creates a new instance of EnrichedLogger.
  - Parameters:
    - `logger`: The underlying ILogger instance
- **LogCommandExecuted** - Logs command execution with duration and other metrics.
  - Parameters:
    - `commandType`: The type of command being executed
    - `duration`: How long the command took to execute
- **LogCommandExecutionError** - Logs a command execution error with exception details.
  - Parameters:
    - `commandType`: The type of command that failed
    - `exception`: The exception that occurred
- **LogCommandValidationError** - Logs a command with validation errors.
  - Parameters:
    - `commandType`: The type of command
    - `errors`: Dictionary of validation errors
- **LogEventPublished** - Logs an event publication with metadata.
  - Parameters:
    - `eventType`: The type of event being published
    - `eventId`: The unique identifier of the event
- **LogEventPublishedWithContext** - Logs an event with additional context properties.
  - Parameters:
    - `eventType`: The type of event
    - `eventId`: The event identifier
    - `context`: Additional context data
- **LogEventSubscribed** - Logs an event subscription.
  - Parameters:
    - `eventType`: The type of event being subscribed to
    - `subscriberType`: The subscriber type
- **LogSagaStarted** - Logs the start of a saga with its initial state.
  - Parameters:
    - `sagaId`: The unique saga identifier
    - `state`: The initial saga state
- **LogSagaStateTransition** - Logs a saga state transition.
  - Parameters:
    - `sagaId`: The saga identifier
    - `fromState`: The previous state
    - `toState`: The new state
- **LogSagaCompleted** - Logs the completion of a saga.
  - Parameters:
    - `sagaId`: The saga identifier
    - `duration`: How long the saga took to complete
- **LogSagaFailed** - Logs a saga failure.
  - Parameters:
    - `sagaId`: The saga identifier
    - `exception`: The exception that caused the failure
- **LogFileOperation** - Logs file operations such as upload, download, delete.
  - Parameters:
    - `operation`: The type of operation (Upload, Download, Delete, etc.)
    - `filePath`: The file path or URI
- **LogFileOperationWithSize** - Logs a file operation with size information.
  - Parameters:
    - `operation`: The type of operation
    - `filePath`: The file path
    - `sizeBytes`: The file size in bytes
- **LogFileOperationError** - Logs a file operation error.
  - Parameters:
    - `operation`: The operation that failed
    - `filePath`: The file path
    - `exception`: The exception that occurred
- **LogJobQueued** - Logs when a background job is queued.
  - Parameters:
    - `jobId`: The unique job identifier
    - `jobType`: The type of job being queued
- **LogJobStarted** - Logs when a background job starts processing.
  - Parameters:
    - `jobId`: The job identifier
    - `jobType`: The job type
- **LogJobCompleted** - Logs successful job completion.
  - Parameters:
    - `jobId`: The job identifier
    - `duration`: How long the job took to complete
- **LogJobFailed** - Logs a job failure.
  - Parameters:
    - `jobId`: The job identifier
    - `exception`: The exception that caused the failure
- **LogJobRetry** - Logs job retry attempt.
  - Parameters:
    - `jobId`: The job identifier
    - `attemptNumber`: The current attempt number
    - `maxRetries`: The maximum number of retries
- **LogWithContext** - Logs a message with structured context properties.
  - Parameters:
    - `operationName`: The name of the operation
    - `context`: Dictionary of contextual properties
- **LogPerformanceMetrics** - Logs performance metrics for an operation.
  - Parameters:
    - `operationName`: The operation name
    - `duration`: The operation duration
    - `resultStatus`: The result status (Success, Failure, etc.)
- **LogCorrelation** - Logs a correlation ID for request tracing.
  - Parameters:
    - `correlationId`: The correlation identifier
    - `userId`: Optional user identifier
    - `requestPath`: Optional request path
- **LogUnhandledException** - Logs unhandled exceptions as critical errors.
  - Parameters:
    - `exception`: The exception that occurred
    - `operationName`: The operation that failed

### IAuditLogger

- **Namespace:** `SmartWorkz.Shared.IAuditLogger`
- **Summary:** Interface for structured audit logging with metadata support.

#### Methods & Properties

- **LogAuditAsync** - Logs an audit event with structured metadata.
  - Parameters:
    - `entityType`: The entity type being audited (e.g., "User", "BlogPost")
    - `entityId`: The unique identifier of the entity
    - `action`: The action performed (Create, Update, Delete, etc.)
    - `metadata`: Optional metadata dictionary for additional context
    - `cancellationToken`: Cancellation token
  - Returns: Result indicating success or failure
- **GetAuditHistoryAsync** - Retrieves audit logs for a specific entity.
  - Parameters:
    - `entityType`: The entity type
    - `entityId`: The entity identifier
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of audit records
- **GetUserActivityAsync** - Retrieves audit logs for a specific user across all entities.
  - Parameters:
    - `userId`: The user identifier
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of audit records

### ILogger

- **Namespace:** `SmartWorkz.Shared.ILogger`
- **Summary:** Abstraction for application logging.
            Decouples from specific logging frameworks (Serilog, NLog, etc.).

### LogLevel

- **Namespace:** `SmartWorkz.Shared.LogLevel`
- **Summary:** Log level severity.

### ILoggerFactory

- **Namespace:** `SmartWorkz.Shared.ILoggerFactory`
- **Summary:** Factory for creating logger instances by category/source.

### IMapper

- **Namespace:** `SmartWorkz.Shared.IMapper`
- **Summary:** Mapping service abstraction for transforming objects between types.
            Supports registration of mapping profiles and bidirectional conversions.

#### Methods & Properties

- **Map``2** - Map source object to target type.
- **Map** - Map source object to target type using dynamic type.
- **MapAsync``2** - Map asynchronously with potential async operations in profile.
- **MapCollection``2** - Map collection of sources to targets.
- **MapCollectionAsync``2** - Map collection asynchronously.
- **RegisterProfile``2** - Register a mapping profile.

### IMapperProfile

- **Namespace:** `SmartWorkz.Shared.IMapperProfile`
- **Summary:** Profile for defining mapping rules between types.
            Implemented by concrete profiles that configure source-to-target transformations.

### IMapperProfile`2

- **Namespace:** `SmartWorkz.Shared.IMapperProfile`2`
- **Summary:** Typed mapper profile for strong typing.

#### Methods & Properties

- **Map** - Transform source to target synchronously.
- **MapAsync** - Transform source to target asynchronously.

### SimpleMapper

- **Namespace:** `SmartWorkz.Shared.SimpleMapper`
- **Summary:** A simple in-memory mapper that supports registering and executing mapping profiles.

### IMetricsCollector

- **Namespace:** `SmartWorkz.Shared.IMetricsCollector`
- **Summary:** Abstraction for collecting application metrics and performance data.
            Enables tracking of operation duration, throughput, error rates, and custom metrics.
            Implementations integrate with OpenTelemetry for export to Prometheus/Grafana.

#### Methods & Properties

- **RecordOperationDuration** - Record operation duration in milliseconds.
  - Parameters:
    - `operationName`: Name of the operation being measured.
    - `durationMs`: Duration in milliseconds.
    - `status`: Optional status (e.g., "success", "error").
    - `tags`: Optional metadata tags for grouping and filtering.
- **RecordOperationCount** - Record operation count (increments counter).
  - Parameters:
    - `operationName`: Name of the operation.
    - `count`: Number to increment by (default 1).
    - `status`: Optional status label.
    - `tags`: Optional metadata tags.
- **RecordGaugeValue** - Record a gauge value (e.g., queue depth, memory usage).
  - Parameters:
    - `metricName`: Name of the gauge metric.
    - `value`: The gauge value to record.
    - `tags`: Optional metadata tags.
- **RecordError** - Record error/exception occurrence.
  - Parameters:
    - `operationName`: Name of the operation that failed.
    - `ex`: The exception that occurred.
    - `tags`: Optional metadata tags.
- **IncrementCounter** - Increment a custom counter.
  - Parameters:
    - `counterName`: Name of the counter.
    - `increment`: Amount to increment (default 1).
    - `tags`: Optional metadata tags.

### MetricsMiddleware

- **Namespace:** `SmartWorkz.Shared.MetricsMiddleware`
- **Summary:** ASP.NET Core middleware for automatic HTTP request/response metrics collection.
             Records operation duration, status, and errors for all HTTP requests.
            
             Usage:
                 app.UseMiddleware<MetricsMiddleware>();

### MetricsStartupExtensions

- **Namespace:** `SmartWorkz.Shared.MetricsStartupExtensions`
- **Summary:** Extension methods for registering application metrics in dependency injection.

#### Methods & Properties

- **AddApplicationMetrics** - Registers IMetricsCollector with OpenTelemetry implementation.
  - Parameters:
    - `services`: The service collection to register with.
  - Returns: The service collection for method chaining.

### OpenTelemetryMetricsCollector

- **Namespace:** `SmartWorkz.Shared.OpenTelemetryMetricsCollector`
- **Summary:** OpenTelemetry-based implementation of IMetricsCollector.
            Collects metrics using System.Diagnostics.Metrics for export to Prometheus/Grafana.

### DefaultTenantFeatureFlags

- **Namespace:** `SmartWorkz.Shared.DefaultTenantFeatureFlags`
- **Summary:** In-memory feature flag provider for tenant-scoped feature control.
            
             Uses ConcurrentDictionary to store tenant-specific flags:
             - Key: tenant ID
             - Value: HashSet of enabled feature flag names
            
             Thread-safe for concurrent operations. Suitable for in-process caching
             or dev/test scenarios. For distributed systems, integrate with a
             centralized feature flag service (Unleash, LaunchDarkly, etc.).

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature is enabled for the specified tenant.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").
    - `cancellationToken`: Cancellation token.
  - Returns: Task containing true if the feature is enabled for this tenant,
            false if the tenant doesn't exist or the flag is not enabled.
- **GetEnabledFeaturesAsync** - Gets all enabled features for a tenant.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Task containing a read-only list of enabled feature flag names.
            Returns an empty list if the tenant doesn't exist or has no enabled flags.
- **EnableFlag** - Enables a feature flag for a tenant.
            Idempotent — calling multiple times with the same flag is safe.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name.
- **DisableFlag** - Disables a feature flag for a tenant.
            Idempotent — calling multiple times with the same flag is safe.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name.

### ITenantContext

- **Namespace:** `SmartWorkz.Shared.ITenantContext`
- **Summary:** Scoped service providing current tenant ID for multi-tenant applications.
            Resolved from request context or claims principal.

#### Methods & Properties

- **GetTenantId** - Gets the current tenant identifier.
  - Returns: Tenant ID, or null if operating in single-tenant context.
- **SetTenantId** - Sets the current tenant identifier (rarely used; typically set from request context).
  - Parameters:
    - `tenantId`: Tenant ID to set.

### ITenantFeatureFlags

- **Namespace:** `SmartWorkz.Shared.ITenantFeatureFlags`
- **Summary:** Feature flag provider scoped to a specific tenant.
            Allows per-tenant feature control.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature is enabled for the specified tenant.
  - Parameters:
    - `tenantId`: Tenant ID.
    - `flagName`: Feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").
    - `cancellationToken`: Cancellation token.
  - Returns: True if feature is enabled for this tenant.
- **GetEnabledFeaturesAsync** - Gets all enabled features for a tenant.
  - Parameters:
    - `tenantId`: Tenant ID.
    - `cancellationToken`: Cancellation token.
  - Returns: List of enabled feature flag names.

### TenantContext

- **Namespace:** `SmartWorkz.Shared.TenantContext`
- **Summary:** Scoped tenant context using AsyncLocal for proper isolation across async boundaries.
            
             AsyncLocal ensures:
             - Thread-safe storage per async execution context
             - Isolation between concurrent requests (each gets its own context)
             - Proper inheritance to child tasks (when awaited)
            
             Survives async/await boundaries unlike ThreadLocal, making it suitable for async methods.

#### Methods & Properties

- **GetTenantId** - Gets the current tenant identifier.
  - Returns: The current tenant ID, or "default" if not set.
- **SetTenantId** - Sets the current tenant identifier.
  - Parameters:
    - `tenantId`: The tenant ID to set. Cannot be null or empty.

### FirebaseCloudMessagingService

- **Namespace:** `SmartWorkz.Shared.FirebaseCloudMessagingService`
- **Summary:** Firebase Cloud Messaging service implementation for sending push notifications.
            Supports single/batch user notifications, topic-based broadcasting, and multi-platform delivery (Android, iOS, Web).

#### Methods & Properties

- **#ctor** - Initializes a new instance of the FirebaseCloudMessagingService.
  - Parameters:
    - `logger`: Logger for diagnostic and error information.
- **SendAsync** - Sends a simple push notification to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends simple push notifications to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase device tokens).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends a rich push notification with metadata to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends a rich push notification to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase device tokens).
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SendToTopicAsync** - Sends a rich push notification to all users subscribed to a topic (broadcast).
  - Parameters:
    - `topic`: Topic name (e.g., "news", "promotions").
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SubscribeToTopicAsync** - Subscribes a user to a topic for broadcast notifications.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `topic`: Topic name to subscribe to.
    - `cancellationToken`: Cancellation token.
- **UnsubscribeFromTopicAsync** - Unsubscribes a user from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `topic`: Topic name to unsubscribe from.
    - `cancellationToken`: Cancellation token.

### IPushNotificationService

- **Namespace:** `SmartWorkz.Shared.IPushNotificationService`
- **Summary:** Service for sending push notifications using Firebase Cloud Messaging.

#### Methods & Properties

- **SendAsync** - Sends a simple push notification to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a simple push notification to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase tokens).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a rich push notification with metadata to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a rich push notification with metadata to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase tokens).
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendToTopicAsync** - Sends a push notification to all users subscribed to a topic.
  - Parameters:
    - `topic`: The topic name.
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SubscribeToTopicAsync** - Subscribes a user to receive notifications from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `topic`: The topic name.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous subscription operation.
- **UnsubscribeFromTopicAsync** - Unsubscribes a user from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `topic`: The topic name.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous unsubscription operation.

### PushNotificationPayload

- **Namespace:** `SmartWorkz.Shared.PushNotificationPayload`
- **Summary:** Represents the payload data for a push notification.

### PushNotificationAction

- **Namespace:** `SmartWorkz.Shared.PushNotificationAction`
- **Summary:** Represents an action that can be performed from a push notification.

### PagedList`1

- **Namespace:** `SmartWorkz.Shared.PagedList`1`
- **Summary:** A page of items with metadata.
             Replaces PaginationResponse<T> in StarterKitMVC.Shared.DTOs.
            
             Migration path: PaginationResponse<T> has the same fields under different names.
             PagedList<T>.Create() is a drop-in replacement for PaginationResponse<T>.Create().

#### Methods & Properties

- **Empty** - Create an empty result set (e.g., when no rows match).
- **Map``1** - Project items to a different type without changing pagination metadata.

### PagedQuery

- **Namespace:** `SmartWorkz.Shared.PagedQuery`
- **Summary:** Standard request parameters for any paginated query.
             Replaces the existing PaginationRequest record in StarterKitMVC.Shared.DTOs.
            
             Migration: PaginationRequest is structurally identical — alias it or replace it.

#### Methods & Properties

- **#ctor** - Standard request parameters for any paginated query.
             Replaces the existing PaginationRequest record in StarterKitMVC.Shared.DTOs.
            
             Migration: PaginationRequest is structurally identical — alias it or replace it.
- **Normalize** - Clamp page and pageSize to safe bounds.

### IEntity`1

- **Namespace:** `SmartWorkz.Shared.IEntity`1`
- **Summary:** Marks a class as a domain entity with a typed primary key.

### CircuitBreaker

- **Namespace:** `SmartWorkz.Shared.CircuitBreaker`
- **Summary:** A thread-safe implementation of the circuit breaker pattern for handling failing dependencies gracefully.
            
             The circuit breaker operates in three states:
             - Closed: Normal operation. Requests pass through. Failures are tracked.
             - Open: Failing. All requests are rejected immediately to prevent cascading failures.
             - HalfOpen: Testing recovery. Limited requests are allowed to test if the dependency has recovered.
            
             State transitions:
             - Closed → Open: When ConsecutiveFailures >= FailureThreshold
             - Open → HalfOpen: Automatically when (DateTime.UtcNow - LastFailureTime) >= TimeoutMilliseconds
             - HalfOpen → Closed: When SuccessCount >= SuccessThreshold
             - HalfOpen → Open: When RecordFailure() is called in HalfOpen state
             - Closed → Closed: When RecordSuccess() is called (resets failure counter)

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `options`: The circuit breaker configuration options.
- **RecordSuccess** - Records a successful operation and updates state transitions accordingly.
            In Closed state: resets the failure counter.
            In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
- **RecordFailure** - Records a failed operation and updates state transitions accordingly.
            In Closed state: increments failure counter and transitions to Open if threshold is met.
            In HalfOpen state: immediately transitions back to Open.
- **Reset** - Resets the circuit breaker to the Closed state, clearing all failure and success counters.

### CircuitBreakerOptions

- **Namespace:** `SmartWorkz.Shared.CircuitBreakerOptions`
- **Summary:** Configuration options for the circuit breaker.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if options are valid; otherwise false.

### CircuitBreakerState

- **Namespace:** `SmartWorkz.Shared.CircuitBreakerState`
- **Summary:** Defines the state of a circuit breaker in the state machine pattern.

### ICircuitBreaker

- **Namespace:** `SmartWorkz.Shared.ICircuitBreaker`
- **Summary:** Defines the contract for a circuit breaker that implements the state machine pattern
            to handle failing dependencies gracefully.

#### Methods & Properties

- **RecordSuccess** - Records a successful operation and updates state transitions accordingly.
            In Closed state: resets the failure counter.
            In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
- **RecordFailure** - Records a failed operation and updates state transitions accordingly.
            In Closed state: increments failure counter and transitions to Open if threshold is met.
            In HalfOpen state: immediately transitions back to Open.
- **Reset** - Resets the circuit breaker to the Closed state, clearing all failure and success counters.

### IRateLimiter

- **Namespace:** `SmartWorkz.Shared.IRateLimiter`
- **Summary:** Defines the contract for a thread-safe rate limiter.

#### Methods & Properties

- **TryAcquireAsync** - Attempts to acquire tokens for a request identified by the given identifier.
  - Parameters:
    - `identifier`: The identifier for rate limiting (e.g., IP address, user ID).
    - `cost`: The number of tokens to acquire (default: 1).
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A result containing true if tokens were acquired successfully; otherwise false.
            On failure, the Error will include retry-after information.
- **GetAvailableTokensAsync** - Gets the number of available tokens for a given identifier.
  - Parameters:
    - `identifier`: The identifier to check.
  - Returns: The number of available tokens.
- **ResetAsync** - Resets the rate limit state for a given identifier.
  - Parameters:
    - `identifier`: The identifier to reset.
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **ClearAsync** - Clears all rate limit state.
  - Parameters:
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.

### RateLimiter

- **Namespace:** `SmartWorkz.Shared.RateLimiter`
- **Summary:** Thread-safe token bucket rate limiter implementation.
            
             This class maintains a per-identifier token bucket that refills at a constant rate.
             Tokens are consumed when requests are made; if insufficient tokens exist, the request is denied.
            
             Thread-safe operations use ConcurrentDictionary and locks on individual buckets to ensure
             consistent state without global locking bottlenecks.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the RateLimiter class.
  - Parameters:
    - `options`: Configuration options for the rate limiter.
- **TryAcquireAsync** - Attempts to acquire tokens for a request identified by the given identifier.
  - Parameters:
    - `identifier`: The identifier for rate limiting (e.g., IP address, user ID).
    - `cost`: The number of tokens to acquire (default: 1).
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A result containing true if tokens were acquired successfully; otherwise false.
            On failure, the Error will include retry-after information.
- **GetAvailableTokensAsync** - Gets the number of available tokens for a given identifier.
  - Parameters:
    - `identifier`: The identifier to check.
  - Returns: The number of available tokens.
- **ResetAsync** - Resets the rate limit state for a given identifier.
  - Parameters:
    - `identifier`: The identifier to reset.
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **ClearAsync** - Clears all rate limit state.
  - Parameters:
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **TokenBucket.TryAcquire** - Tries to acquire the specified number of tokens.
- **TokenBucket.GetAvailableTokens** - Gets the current number of available tokens.
- **TokenBucket.GetRetryAfterMilliseconds** - Gets the number of milliseconds to wait before retrying.
- **TokenBucket.RefillTokens** - Refills the token bucket based on elapsed time.

### RateLimiterOptions

- **Namespace:** `SmartWorkz.Shared.RateLimiterOptions`
- **Summary:** Configuration options for the rate limiter.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if options are valid; otherwise false.

### RateLimiterStrategy

- **Namespace:** `SmartWorkz.Shared.RateLimiterStrategy`
- **Summary:** Specifies the strategy used by the rate limiter to control request flow.

### ApiError

- **Namespace:** `SmartWorkz.Shared.ApiError`
- **Summary:** Structured error representation for API responses.
            Provides code, message, and optional field-level error details.

#### Methods & Properties

- **FromError** - Create from core Error type.
- **FromValidationErrors** - Create from validation errors.
- **FromException** - Create from exception.

### ApiResponse

- **Namespace:** `SmartWorkz.Shared.ApiResponse`
- **Summary:** Generic API response envelope that wraps result data with metadata.
            Non-generic convenience version for non-data responses.

#### Methods & Properties

- **Ok** - Success response without data.
- **Fail** - Failure response with error details.
- **FromResult** - Create from core Result pattern.

### ApiResponse`1

- **Namespace:** `SmartWorkz.Shared.ApiResponse`1`
- **Summary:** Typed API response envelope with data payload.
            Includes optional pagination metadata for list responses.

#### Methods & Properties

- **Ok** - Success response with data.
- **OkPaginated** - Success response with paginated data.
- **Fail** - Failure response with error.

### ProblemDetailsResponse

- **Namespace:** `SmartWorkz.Shared.ProblemDetailsResponse`
- **Summary:** Implements RFC 7807 Problem Details for HTTP APIs standard response format.
            Provides a standardized way to represent error details in API responses.

#### Methods & Properties

- **ValidationError** - Factory method for 400 Bad Request error with validation details.
- **Unauthorized** - Factory method for 401 Unauthorized error.
- **Forbidden** - Factory method for 403 Forbidden error.
- **NotFound** - Factory method for 404 Not Found error.
- **Conflict** - Factory method for 409 Conflict error.
- **InternalServerError** - Factory method for 500 Internal Server Error.
- **Custom** - Factory method for custom problem details.

### Error

- **Namespace:** `SmartWorkz.Shared.Error`
- **Summary:** Represents a structured error with a machine-readable code and human-readable message.
            
             This is the canonical Error type. It replaces:
             - SmartWorkz.StarterKitMVC.Shared.Primitives.Error (record struct)
             - The ad-hoc string errors in Models.Result
            
             Code examples: "USER_NOT_FOUND", "VALIDATION.EMAIL_REQUIRED", "AUTH.INVALID_CREDENTIALS"
             MessageKey maps to localization resource keys for UI display.

### Result

- **Namespace:** `SmartWorkz.Shared.Result`
- **Summary:** Represents the outcome of an operation that does not return a value.
            
             This unifies:
             - SmartWorkz.StarterKitMVC.Shared.Models.Result (Succeeded + MessageKey + Errors[])
             - SmartWorkz.StarterKitMVC.Shared.Primitives.Result (IsSuccess + Error struct)
            
             Design choice — class over struct:
             1. Result<T> inherits from Result to reuse Succeeded/Errors without duplication.
                Structs cannot use inheritance this way.
             2. Services return Result from interface methods — class semantics (null check) are
                simpler than boxing/unboxing structs across interface boundaries.
             3. Errors[] supports field-level validation messages that ModelState.AddErrors() consumes.
                A single Error struct cannot carry multiple field errors.
            
             The Primitives.Result struct in StarterKitMVC.Shared remains valid for pure functions
             where you want zero-allocation returns. This class is for service layer contracts.

#### Methods & Properties

- **Fail** - Failure with a localization message key and optional field-level error strings.
- **Fail** - Failure from a structured Error (bridges the Primitives.Error pattern).
- **Ok``1** - Factory for a typed result. Use in services that return data.

### Result`1

- **Namespace:** `SmartWorkz.Shared.Result`1`
- **Summary:** Result with a typed payload. Data is only valid when Succeeded = true.
            
             Usage:
               Result<UserDto> result = await _userService.GetByIdAsync(id);
               if (!result.Succeeded) return RedirectToPage("Error");
               var user = result.Data!;

### ResultExtensions

- **Namespace:** `SmartWorkz.Shared.ResultExtensions`
- **Summary:** Functional helpers for chaining Result operations.
            Keeps service code flat — avoids nested if (!result.Succeeded) blocks.

#### Methods & Properties

- **Map``2** - Transform the Data value if the result succeeded.
- **BindAsync``2** - Chain a second operation that also returns Result.
- **OnSuccess``1** - Execute a side-effect action on success, then return the original result.
- **OnFailure``1** - Execute a side-effect action on failure, then return the original result.

### ISagaDefinition`1

- **Namespace:** `SmartWorkz.Shared.ISagaDefinition`1`
- **Summary:** Defines the blueprint for a saga orchestration.
            A saga is a pattern for managing distributed transactions and long-running processes
            by coordinating multiple steps with built-in compensation mechanisms.

#### Methods & Properties

- **DefineStep``1** - Defines a step in the saga that will be executed when a specific event type is received.
            Steps are executed sequentially in the order they were defined.
  - Parameters:
    - `handler`: The async handler function that processes the event and updates the saga state.
            Returns a StepResult indicating success or failure.
- **OnFailure** - Defines the failure handler that will be called if any step fails.
            Used for compensation logic and saga-level error handling.
  - Parameters:
    - `compensationHandler`: The async handler that receives the current saga state and the exception that occurred.
            Responsible for compensation/rollback logic.
- **BuildAsync** - Builds and returns the saga definition for execution.
            Can be used for async initialization or validation.
  - Returns: A task that completes with the configured saga definition.
- **GetSteps** - Gets the list of saga steps in execution order.
  - Returns: A read-only list of saga step handlers.
- **GetFailureHandler** - Gets the failure compensation handler if defined.
  - Returns: The failure handler function, or null if not defined.

### SagaOrchestrator

- **Namespace:** `SmartWorkz.Shared.SagaOrchestrator`
- **Summary:** Orchestrates the execution of sagas, managing step sequencing, error handling,
            and compensation/rollback logic for complex distributed processes.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the SagaOrchestrator class.
  - Parameters:
    - `logger`: Logger for saga execution tracking and debugging.
- **ExecuteSagaAsync``1** - Executes a saga definition with the provided initial state and triggering event.
            Manages step execution, error handling, and compensation logic.
  - Parameters:
    - `sagaDefinition`: The saga definition blueprint to execute.
    - `initialState`: The initial saga state.
    - `event`: The domain event triggering the saga.
    - `cancellationToken`: Optional cancellation token.
  - Returns: A task representing the saga execution.
- **ExecuteSagaStepsAsync``1** - Executes saga steps by using reflection to access internal step definitions.
- **CompensateExecutedStepsAsync``1** - Executes compensation handlers for all executed steps in reverse order.
            Uses stored compensation handlers to avoid re-executing steps.
- **ExecuteFailureHandlerAsync``1** - Executes the saga-level failure handler if one is defined.

### SagaStatus

- **Namespace:** `SmartWorkz.Shared.SagaStatus`
- **Summary:** Represents the status of a saga execution.

### SagaState

- **Namespace:** `SmartWorkz.Shared.SagaState`
- **Summary:** Base class for saga state objects.
            Provides common tracking properties for saga execution flow.

### StepResult

- **Namespace:** `SmartWorkz.Shared.StepResult`
- **Summary:** Represents the result of executing a single saga step.
            Provides success/failure status and optional compensation logic for rollback.

#### Methods & Properties

- **Success** - Creates a successful step result.
  - Returns: A StepResult indicating success.
- **Failure** - Creates a failed step result with an optional compensation handler.
  - Parameters:
    - `failureReason`: The reason for the step failure.
    - `compensationHandler`: Optional handler to compensate/rollback this step if a later step fails.
  - Returns: A StepResult indicating failure.
- **FromException** - Creates a failed step result for an exception with optional compensation.
  - Parameters:
    - `exception`: The exception that caused the failure.
    - `compensationHandler`: Optional compensation handler.
  - Returns: A StepResult indicating failure.

### CryptHelper

- **Namespace:** `SmartWorkz.Shared.CryptHelper`
- **Summary:** Provides AES-256-CBC encryption and decryption utilities with secure key and IV generation.
            
             All operations support both string and byte array inputs/outputs.
             Keys are normalized to 32 bytes (256 bits) via padding/trimming as needed.
             IVs are auto-generated if not provided and embedded in the ciphertext (IV:Ciphertext format).

#### Methods & Properties

- **EncryptString** - Encrypts plaintext using AES-256-CBC with a Base64-encoded output.
  - Parameters:
    - `plaintext`: The plaintext to encrypt.
    - `key`: The encryption key (will be normalized to 32 bytes).
    - `iv`: Optional IV; auto-generated if null.
  - Returns: A Result containing Base64-encoded ciphertext in "IV:Ciphertext" format or an error.
- **EncryptBytes** - Encrypts byte data using AES-256-CBC.
  - Parameters:
    - `plaintext`: The plaintext bytes to encrypt.
    - `key`: The encryption key (will be normalized to 32 bytes).
    - `iv`: Optional IV; auto-generated if null.
  - Returns: A Result containing encrypted bytes with embedded IV (IV || Ciphertext) or an error.
- **DecryptString** - Decrypts Base64-encoded ciphertext using AES-256-CBC.
  - Parameters:
    - `ciphertext`: The Base64-encoded ciphertext in "IV:Ciphertext" format.
    - `key`: The decryption key (will be normalized to 32 bytes).
  - Returns: A Result containing the decrypted plaintext or an error.
- **DecryptBytes** - Decrypts byte data using AES-256-CBC.
  - Parameters:
    - `ciphertext`: The encrypted bytes with embedded IV (IV || Ciphertext).
    - `key`: The decryption key (will be normalized to 32 bytes).
  - Returns: A Result containing decrypted bytes or an error.
- **GenerateKey** - Generates a random cryptographic key of the specified size.
  - Parameters:
    - `keySize`: The key size in bytes (default 32 for AES-256). Must be 16, 24, or 32.
  - Returns: A Result containing Base64-encoded random key or an error.
- **GenerateIv** - Generates a random cryptographic IV (Initialization Vector).
  - Returns: A Result containing Base64-encoded random IV or an error.
- **GenerateRandomBytes** - Generates cryptographically secure random bytes.
- **NormalizeKey** - Normalizes a key to exactly 32 bytes (256 bits).
            If the key is shorter, it's padded with zeros. If longer, it's trimmed.

### CryptOptions

- **Namespace:** `SmartWorkz.Shared.CryptOptions`
- **Summary:** Configuration options for AES cryptographic operations.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if valid, false otherwise.

### HashHelper

- **Namespace:** `SmartWorkz.Shared.HashHelper`
- **Summary:** Provides utilities for cryptographic hash operations (SHA256 and MD5).

#### Methods & Properties

- **Sha256** - Computes the SHA256 hash of a string and returns it as a hexadecimal string.
- **Sha256Bytes** - Computes the SHA256 hash of a byte array and returns the hash as a byte array.
- **Md5** - Computes the MD5 hash of a string and returns it as a hexadecimal string.
            Note: MD5 is cryptographically broken; use SHA256 for security-critical applications.
- **VerifyHash** - Verifies that a text matches its SHA256 hash.

### HmacAlgorithm

- **Namespace:** `SmartWorkz.Shared.HmacAlgorithm`
- **Summary:** Specifies the HMAC algorithm to use for message signing and verification.

### HmacHelper

- **Namespace:** `SmartWorkz.Shared.HmacHelper`
- **Summary:** Provides HMAC-SHA256/SHA512 message signing and verification for API requests and webhook verification.
            Implements constant-time comparison to prevent timing attacks.

#### Methods & Properties

- **Sign** - Signs a message using HMAC with the specified algorithm and returns a Base64-encoded hex digest.
  - Parameters:
    - `message`: The message to sign.
    - `secret`: The secret key for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing the Base64-encoded signature or an error.
- **SignBytes** - Signs a message using HMAC with the specified algorithm and returns the raw byte digest.
  - Parameters:
    - `message`: The message bytes to sign.
    - `secret`: The secret key for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing the byte signature or an error.
- **Verify** - Verifies a message signature using HMAC with constant-time comparison to prevent timing attacks.
  - Parameters:
    - `message`: The original message that was signed.
    - `signature`: The Base64-encoded signature to verify.
    - `secret`: The secret key used for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing true if the signature is valid, false if not, or an error.
- **VerifyBytes** - Verifies a message signature using HMAC with raw byte inputs and constant-time comparison.
  - Parameters:
    - `message`: The original message bytes that were signed.
    - `signature`: The byte signature to verify.
    - `secret`: The secret key used for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing true if the signature is valid, false if not, or an error.
- **SignBytes** - Internal method to compute HMAC signature from raw bytes.
- **CreateHmac** - Creates the appropriate HMAC instance based on the algorithm.

### InputSanitizer

- **Namespace:** `SmartWorkz.Shared.InputSanitizer`
- **Summary:** Input sanitization to prevent XSS, SQL injection, and path traversal attacks.

#### Methods & Properties

- **SanitizeHtml** - Sanitize HTML by removing dangerous tags and attributes.
- **EscapeHtml** - Escape HTML special characters to prevent XSS.
- **SanitizeSql** - Sanitize string to prevent SQL injection (basic, not a replacement for parameterized queries).
- **SanitizeFilePath** - Sanitize file path to prevent directory traversal attacks.
- **SanitizeUrl** - Sanitize and validate URL.
- **EscapeJson** - Escape string for safe JSON inclusion.
- **IsValidEmail** - Validate email format (basic check, server-side SMTP validation recommended).
- **RemoveControlCharacters** - Remove null bytes and control characters.

### JwtSettings

- **Namespace:** `SmartWorkz.Shared.JwtSettings`
- **Summary:** Settings for JWT token generation and validation.

#### Methods & Properties

- **Validate** - Validate settings: Secret >= 32 chars, other fields non-empty.

### JwtClaims

- **Namespace:** `SmartWorkz.Shared.JwtClaims`
- **Summary:** JWT claims that can be included in a token.

#### Methods & Properties

- **GetClaimValue** - Get claim value by type (supports standard claims + custom).

### JwtTokenValidationResult

- **Namespace:** `SmartWorkz.Shared.JwtTokenValidationResult`
- **Summary:** Result of JWT token validation.

### JwtHelper

- **Namespace:** `SmartWorkz.Shared.JwtHelper`
- **Summary:** Provides JWT token generation, validation, and refresh functionality.

#### Methods & Properties

- **GenerateTokenInternal** - Internal token generation logic shared by GenerateToken and GenerateRefreshToken.
  - Parameters:
    - `claims`: The claims to include in the token.
    - `settings`: The JWT settings for signing and configuration.
    - `isRefreshToken`: If true, uses RefreshTokenExpiryDays; otherwise uses ExpiryMinutes.
  - Returns: A Result containing the signed token or an error.
- **GenerateToken** - Generates a JWT access token with the specified claims and settings.
  - Parameters:
    - `claims`: The claims to include in the token.
    - `settings`: The JWT settings for signing and configuration.
  - Returns: A Result containing the signed token or an error.
- **ValidateToken** - Validates a JWT token and extracts claims if valid.
  - Parameters:
    - `token`: The token to validate.
    - `settings`: The JWT settings for validation.
  - Returns: A Result containing the validation result.
- **RefreshToken** - Refreshes an access token using a refresh token.
  - Parameters:
    - `refreshToken`: The refresh token.
    - `settings`: The JWT settings.
  - Returns: A Result containing the new access token or an error.
- **GenerateRefreshToken** - Generates a refresh token with extended expiry.
  - Parameters:
    - `claims`: The claims to include in the refresh token.
    - `settings`: The JWT settings.
  - Returns: A Result containing the refresh token or an error.
- **ToBase64Url** - Encodes bytes to Base64Url format (no padding, + → -, / → _).
- **FromBase64Url** - Decodes Base64Url format to bytes.

### PasswordHelper

- **Namespace:** `SmartWorkz.Shared.PasswordHelper`
- **Summary:** Provides secure password generation and validation using cryptographically secure random number generation.

#### Methods & Properties

- **GeneratePassword** - Generates a cryptographically secure random password.
  - Parameters:
    - `length`: Length of the password (8-128, default 12).
    - `includeSpecialChars`: Whether to include special characters.
  - Returns: A Result containing the generated password or an error.
- **ValidateStrength** - Validates the strength of a password against a policy.
  - Parameters:
    - `password`: The password to validate.
    - `policy`: The policy to validate against (uses default if null).
  - Returns: A Result containing the validation result.
- **GetRandomChar** - Gets a random character from the specified character set using cryptographic randomness.
- **Shuffle** - Performs Fisher-Yates shuffle on the character array.
- **CheckPasswordLength** - Checks if password meets minimum length requirement.
- **CheckUppercase** - Checks if password contains at least one uppercase letter.
- **CheckLowercase** - Checks if password contains at least one lowercase letter.
- **CheckNumbers** - Checks if password contains at least one digit.
- **CheckSpecialChars** - Checks if password contains at least one special character.

### PasswordPolicy

- **Namespace:** `SmartWorkz.Shared.PasswordPolicy`
- **Summary:** Policy for password validation requirements.

#### Methods & Properties

- **Validate** - Validates the policy invariants.

### PasswordValidationResult

- **Namespace:** `SmartWorkz.Shared.PasswordValidationResult`
- **Summary:** Result of password validation against a policy.

### ITemplateEngine

- **Namespace:** `SmartWorkz.Shared.ITemplateEngine`
- **Summary:** Defines operations for rendering templates with placeholder substitution.

#### Methods & Properties

- **Render** - Renders a template string by replacing placeholders with values from a dictionary.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `values`: Dictionary of key-value pairs to substitute into the template.
  - Returns: The rendered content with placeholders replaced. Unmatched placeholders remain unchanged.
- **Render** - Renders a template string by extracting properties from an object model.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `model`: The model object whose public properties are used for placeholder substitution.
  - Returns: The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.
- **RenderFileAsync** - Asynchronously renders a template file by replacing placeholders with values from a dictionary.
  - Parameters:
    - `filePath`: The path to the template file.
    - `values`: Dictionary of key-value pairs to substitute into the template.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **RenderFileAsync** - Asynchronously renders a template file by extracting properties from an object model.
  - Parameters:
    - `filePath`: The path to the template file.
    - `model`: The model object whose public properties are used for placeholder substitution.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **LoadDirectoryAsync** - Asynchronously loads all template files from a directory into a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.
- **RenderDirectoryAsync** - Asynchronously loads and renders all template files from a directory using values from a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `values`: Dictionary of key-value pairs to substitute into each template.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.

### TemplateEngine

- **Namespace:** `SmartWorkz.Shared.TemplateEngine`
- **Summary:** Provides template rendering services with support for placeholder substitution.

#### Methods & Properties

- **PlaceholderRegex** - 
- **Render** - Renders a template string by replacing placeholders with values from a dictionary.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `values`: Dictionary of key-value pairs to substitute into the template.
  - Returns: The rendered content with placeholders replaced. Unmatched placeholders and null/empty content remain unchanged.
- **Render** - Renders a template string by extracting properties from an object model.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `model`: The model object whose public properties are used for placeholder substitution.
  - Returns: The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.
- **RenderFileAsync** - Asynchronously renders a template file by replacing placeholders with values from a dictionary.
  - Parameters:
    - `filePath`: The path to the template file.
    - `values`: Dictionary of key-value pairs to substitute into the template.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **RenderFileAsync** - Asynchronously renders a template file by extracting properties from an object model.
  - Parameters:
    - `filePath`: The path to the template file.
    - `model`: The model object whose public properties are used for placeholder substitution.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **LoadDirectoryAsync** - Asynchronously loads all template files from a directory into a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.
- **RenderDirectoryAsync** - Asynchronously loads and renders all template files from a directory using values from a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `values`: Dictionary of key-value pairs to substitute into each template.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.
- **ReflectModel** - Reflects over a model object and builds a case-insensitive dictionary of public properties
            mapped to their string values. Uses cached property metadata for performance.
- **ValidateFilePath** - Validates a file path to prevent directory traversal attacks.
  - Parameters:
    - `filePath`: The file path to validate.
  - Returns: A result indicating if the path is valid and safe.

### CompressHelper

- **Namespace:** `SmartWorkz.Shared.CompressHelper`
- **Summary:** Provides utilities for GZip compression and decompression.

#### Methods & Properties

- **CompressString** - Compresses a string using GZip compression.
- **DecompressString** - Decompresses a GZip-compressed byte array back to a string.
- **CompressBytes** - Compresses a byte array using GZip compression.
- **DecompressBytes** - Decompresses a GZip-compressed byte array.

### DateHelper

- **Namespace:** `SmartWorkz.Shared.DateHelper`
- **Summary:** Provides utilities for date and time operations.

#### Methods & Properties

- **GetAge** - Calculates the age in years from a birth date to today.
- **GetRelativeTime** - Returns a human-readable relative time string (e.g., "2 days ago", "in 3 hours").
- **StartOfDay** - Returns the start of the day (00:00:00) for the given date.
- **EndOfDay** - Returns the end of the day (23:59:59.999) for the given date.
- **IsWeekend** - Determines if the given date falls on a weekend (Saturday or Sunday).
- **GetDayOfWeekName** - Returns the name of the day of week (e.g., "Monday", "Tuesday").
- **DaysBetween** - Calculates the number of days between two dates (inclusive of the from date, exclusive of the to date).

### EnumHelper

- **Namespace:** `SmartWorkz.Shared.EnumHelper`
- **Summary:** Provides utilities for enum operations including reflection and description retrieval.

#### Methods & Properties

- **GetDescription** - Gets the description of an enum value from its [Description] attribute.
            Falls back to the enum name if no description is found.
- **GetValue``1** - Attempts to get an enum value by its name.
- **GetAllValues``1** - Returns all values of the specified enum type as a list.
- **GetName** - Gets the name of an enum value.

### MathHelper

- **Namespace:** `SmartWorkz.Shared.MathHelper`
- **Summary:** Provides utilities for common math operations.

#### Methods & Properties

- **Percentage** - Calculates the percentage of a value.
            Example: Percentage(100, 20) returns 20 (20% of 100).
- **PercentageChange** - Calculates the percentage change from oldValue to newValue.
            Positive result indicates increase, negative indicates decrease.
- **RoundTo** - Rounds a decimal value to the specified number of decimal places.
- **Clamp``1** - Clamps a value within a specified range [min, max].
- **Average** - Calculates the average of the provided decimal values.

### SlugHelper

- **Namespace:** `SmartWorkz.Shared.SlugHelper`
- **Summary:** Helper for generating URL-friendly slugs from text input.

#### Methods & Properties

- **GenerateSlug** - Generates a URL-friendly slug from the given text with optional configuration.
  - Parameters:
    - `text`: The input text to convert to a slug.
    - `options`: Configuration options. If null, default options are used.
  - Returns: A Result containing the generated slug or an error.
- **ToSlug** - Generates a URL-friendly slug from the given text using default options.
            Convenience method equivalent to GenerateSlug(text, null).
  - Parameters:
    - `text`: The input text to convert to a slug.
  - Returns: A Result containing the generated slug or an error.
- **RemoveAccents** - Removes accented characters from text by decomposing them and filtering out combining marks.
            For example: "café" → "cafe", "naïve" → "naive", "Señor" → "Senor".
  - Parameters:
    - `input`: The input text potentially containing accented characters.
  - Returns: The text with accented characters converted to their base forms.
- **ReplaceSpecialCharacters** - Replaces special characters and spaces with the specified separator.
            Keeps only alphanumeric characters and the separator.
  - Parameters:
    - `input`: The input text.
    - `separator`: The separator to use for special characters and spaces.
  - Returns: The text with special characters replaced by the separator.

### SlugOptions

- **Namespace:** `SmartWorkz.Shared.SlugOptions`
- **Summary:** Options for configuring slug generation behavior in .

### TextHelper

- **Namespace:** `SmartWorkz.Shared.TextHelper`
- **Summary:** Sealed class providing advanced text processing and formatting utilities.
            All methods return Result<string> for consistent error handling.

#### Methods & Properties

- **Truncate** - Truncates text to a maximum length and appends a suffix (default "...").
  - Parameters:
    - `text`: The input text to truncate.
    - `maxLength`: The maximum length including the suffix.
    - `suffix`: The suffix to append when truncating. Defaults to "...".
  - Returns: A Result containing the truncated text or an error.
- **Capitalize** - Capitalizes the first letter of the text while preserving the rest.
  - Parameters:
    - `text`: The input text to capitalize.
  - Returns: A Result containing the capitalized text or an error.
- **Decapitalize** - Decapitalizes the first letter of the text while preserving the rest.
  - Parameters:
    - `text`: The input text to decapitalize.
  - Returns: A Result containing the decapitalized text or an error.
- **StripHtml** - Removes HTML tags from the input string using regex.
  - Parameters:
    - `html`: The HTML string to process.
  - Returns: A Result containing the plain text with HTML tags removed or an error.
- **Pluralize** - Pluralizes a word based on count using a simple heuristic.
            If count == 1, returns singular form. Otherwise appends 's'.
  - Parameters:
    - `singular`: The singular form of the word.
    - `count`: The count to determine plural form.
  - Returns: A Result containing the appropriately pluralized word or an error.
- **TitleCase** - Converts text to title case by capitalizing the first letter of each word.
  - Parameters:
    - `text`: The input text to convert.
  - Returns: A Result containing the title-cased text or an error.
- **Reverse** - Reverses the input string.
  - Parameters:
    - `text`: The input text to reverse.
  - Returns: A Result containing the reversed text or an error.
- **RemoveWhitespace** - Removes all whitespace characters from the input string.
  - Parameters:
    - `text`: The input text to process.
  - Returns: A Result containing the text with all whitespace removed or an error.
- **WordWrap** - Wraps text at a specified line length while preserving word boundaries.
  - Parameters:
    - `text`: The input text to wrap.
    - `lineLength`: The maximum length of each line.
    - `newline`: The newline character(s) to use. Defaults to "\n".
  - Returns: A Result containing the word-wrapped text or an error.
- **Repeat** - Repeats the input string the specified number of times.
  - Parameters:
    - `text`: The input text to repeat.
    - `count`: The number of times to repeat the text.
  - Returns: A Result containing the repeated text or an error.

### CompositeValidator`1

- **Namespace:** `SmartWorkz.Shared.CompositeValidator`1`
- **Summary:** Combines multiple validators into a single validator.
            Useful for composing validators from different sources.

### IValidationRule`2

- **Namespace:** `SmartWorkz.Shared.IValidationRule`2`
- **Summary:** Single validation rule for a property.

#### Methods & Properties

- **ValidateAsync** - Validate property and return results.

### ValidationRule`2

- **Namespace:** `SmartWorkz.Shared.ValidationRule`2`
- **Summary:** Base implementation for custom validation rules.

### ValidationRules

- **Namespace:** `SmartWorkz.Shared.ValidationRules`
- **Summary:** Pre-built validation rules for common scenarios.

### ValidatorBuilder`1

- **Namespace:** `SmartWorkz.Shared.ValidatorBuilder`1`
- **Summary:** Fluent validator builder for defining validation rules.
            Provides an alternative to ValidatorBase for more concise validator definitions.

#### Methods & Properties

- **RuleFor``1** - Add a rule for a property using fluent API.
- **ValidateAsync** - Validate instance against all rules.

### IWebhookRegistry

- **Namespace:** `SmartWorkz.Shared.IWebhookRegistry`
- **Summary:** Abstraction for managing webhook subscriptions and registrations.
            Supports CRUD operations and subscription queries.

#### Methods & Properties

- **RegisterAsync** - Register a new webhook subscription.
  - Parameters:
    - `url`: The webhook endpoint URL.
    - `events`: Array of event names to subscribe to.
    - `secret`: Optional HMAC-SHA256 secret for signature verification.
    - `cancellationToken`: Cancellation token.
  - Returns: The ID of the newly registered subscription.
- **UnregisterAsync** - Unregister and remove a webhook subscription.
  - Parameters:
    - `subscriptionId`: The subscription ID to unregister.
    - `cancellationToken`: Cancellation token.
- **GetSubscriptionsForEventAsync** - Get all active subscriptions for a specific event.
  - Parameters:
    - `eventName`: The event name to filter by.
    - `cancellationToken`: Cancellation token.
  - Returns: Collection of subscriptions interested in this event.
- **GetActiveSubscriptionsAsync** - Get all currently active subscriptions.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: Collection of all active subscriptions.
- **UpdateSubscriptionStatusAsync** - Update the status and failure tracking of a subscription.
  - Parameters:
    - `subscriptionId`: The subscription ID to update.
    - `isActive`: Whether the subscription should remain active.
    - `failureCount`: Number of consecutive failures (null to leave unchanged).
    - `failureReason`: Reason for failure (null to clear).
    - `cancellationToken`: Cancellation token.

### SqlWebhookRegistry

- **Namespace:** `SmartWorkz.Shared.SqlWebhookRegistry`
- **Summary:** SQL Server implementation of IWebhookRegistry.
            Persists webhook subscriptions to the database with support for querying and status updates.

### WebhookDeliveryService

- **Namespace:** `SmartWorkz.Shared.WebhookDeliveryService`
- **Summary:** Service for publishing domain events to registered webhook endpoints.
            Implements exponential backoff retry logic, HMAC signature verification, and failure tracking.

#### Methods & Properties

- **PublishEventAsync** - Publish an event to all subscribed webhook endpoints.
  - Parameters:
    - `eventName`: The name of the event being published.
    - `payload`: The event payload to send.
    - `cancellationToken`: Cancellation token.
- **DeliverAsync** - Deliver an event to a single webhook endpoint with exponential backoff retry logic.
- **GenerateSignature** - Generate HMAC-SHA256 signature for webhook payload verification.

### AuditEntry

- **Namespace:** `SmartWorkz.Shared.AuditEntry`
- **Summary:** Immutable audit log entry for tracking entity changes and domain events.
            Records who did what, when, where, and why for compliance and debugging.

### AuditEventSubscriber

- **Namespace:** `SmartWorkz.Shared.AuditEventSubscriber`
- **Summary:** Subscribes to domain events and records them in the audit trail.
            Enables automatic audit capture without requiring explicit audit calls in business logic.

#### Methods & Properties

- **OnEventPublishedAsync** - Record a domain event in the audit trail.
  - Parameters:
    - `evt`: The domain event to record.
    - `userId`: User ID who triggered the event (optional for system events).
    - `ipAddress`: IP address of the request originator (optional).
    - `cancellationToken`: Cancellation token.

### AuditStartupExtensions

- **Namespace:** `SmartWorkz.Shared.AuditStartupExtensions`
- **Summary:** Dependency injection and schema setup for audit trail functionality.

#### Methods & Properties

- **AddAuditTrail** - Register IAuditTrail with SQL Server implementation.
- **CreateAuditTrailSchema** - Create the AuditTrail table and indexes if they don't exist.
            Call this during application startup or migration.

### IAuditTrail

- **Namespace:** `SmartWorkz.Shared.IAuditTrail`
- **Summary:** Service for recording and querying immutable audit entries.
            Abstracts the persistence mechanism for audit trails.

#### Methods & Properties

- **RecordAsync** - Record an audit entry (immutable append-only).
  - Parameters:
    - `entry`: The audit entry to record.
    - `cancellationToken`: Cancellation token.
- **GetEntriesAsync** - Get all audit entries for a specific entity instance.
  - Parameters:
    - `entityType`: Type of entity (e.g., "Order").
    - `entityId`: Entity instance ID.
    - `cancellationToken`: Cancellation token.
- **GetEntriesByActionAsync** - Get audit entries by action type (Created, Updated, Deleted, etc.).
  - Parameters:
    - `action`: The action to filter by.
    - `since`: Optional: only entries after this date.
    - `cancellationToken`: Cancellation token.
- **GetEntriesByUserAsync** - Get audit entries for a specific user.
  - Parameters:
    - `userId`: User ID who performed actions.
    - `since`: Optional: only entries after this date.
    - `cancellationToken`: Cancellation token.
- **SearchAsync** - Search audit trail with multiple filter criteria.
            All criteria are AND'd together (null criteria are ignored).
  - Parameters:
    - `entityType`: Optional entity type filter.
    - `action`: Optional action filter.
    - `userId`: Optional user ID filter.
    - `since`: Optional timestamp filter (inclusive).
    - `cancellationToken`: Cancellation token.

### SqlAuditTrail

- **Namespace:** `SmartWorkz.Shared.SqlAuditTrail`
- **Summary:** SQL Server implementation of IAuditTrail for immutable audit log persistence.
            Appends audit entries to a single table with indexes for efficient querying.

#### Methods & Properties

- **RecordAsync** - 
- **GetEntriesAsync** - 
- **GetEntriesByActionAsync** - 
- **GetEntriesByUserAsync** - 
- **SearchAsync** - 

### ValueConverter`1

- **Namespace:** `SmartWorkz.Shared.ValueConverter`1`
- **Summary:** Abstract base class for type conversion between domain objects and DTOs.
            Enables loose coupling between layers by centralizing conversion logic.

#### Methods & Properties

- **Convert``1** - Convert a single source object to target type.
- **Convert** - Convert a single source object using dynamic target type resolution.
- **ConvertList``1** - Convert a collection of source objects to target type.
- **ConvertList** - Convert a collection using dynamic target type resolution.
- **ConvertFromList``2** - Convert from a collection of different source types.

### CacheEntry`1

- **Namespace:** `SmartWorkz.Shared.CacheEntry`1`
- **Summary:** Represents a cached entry with data, expiration time, and metadata.

#### Methods & Properties

- **#ctor** - Creates a new CacheEntry instance.
- **#ctor** - Creates a new CacheEntry instance with data and expiration.
- **RenewExpiry** - Renews the expiry time based on the cache strategy and TTL.

### CacheEntryWrapper

- **Namespace:** `SmartWorkz.Shared.CacheEntryWrapper`
- **Summary:** Non-generic wrapper for CacheEntry to store in the cache dictionary.

### CacheOptions

- **Namespace:** `SmartWorkz.Shared.CacheOptions`
- **Summary:** Configuration options for cache operations.

#### Methods & Properties

- **#ctor** - Creates a new CacheOptions instance with default values.
- **#ctor** - Creates a new CacheOptions instance with specified TTL.
- **#ctor** - Creates a new CacheOptions instance with specified TTL and cache strategy.
- **#ctor** - Creates a new CacheOptions instance with all parameters.

### CacheStrategy

- **Namespace:** `SmartWorkz.Shared.CacheStrategy`
- **Summary:** Enumeration of cache expiration strategies.

### ICacheService

- **Namespace:** `SmartWorkz.Shared.ICacheService`
- **Summary:** Service for caching with tenant isolation and L1/L2 hybrid support.
            Implementations may use memory cache (L1) and distributed cache (L2).
            All cache operations are tenant-scoped with automatic key prefixing.

#### Methods & Properties

- **GetAsync``1** - Gets a cached value by key with tenant isolation.
  - Parameters:
    - `key`: Cache key (will be tenant-scoped automatically).
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Result containing cached value, or failure if not found or expired.
- **SetAsync``1** - Sets a value in cache with optional TTL for the specified tenant.
  - Parameters:
    - `key`: Cache key (will be tenant-scoped automatically).
    - `value`: Value to cache.
    - `ttlMinutes`: Time to live in minutes. If null, value never expires.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveAsync** - Removes a cached value by key for the specified tenant.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveByPrefixAsync** - Removes all cached values matching a key prefix for the specified tenant.
            Example: RemoveByPrefixAsync("user:") removes all "user:*" entries for that tenant.
  - Parameters:
    - `prefix`: Key prefix to match (may include wildcard suffix like "user:*").
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **ExistsAsync** - Checks if a key exists in cache for the specified tenant.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: True if key exists and is not expired; false otherwise.
- **ClearAsync** - Clears all cache entries for the specified tenant.
            Does not affect entries for other tenants.
  - Parameters:
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.

### ICacheStore

- **Namespace:** `SmartWorkz.Shared.ICacheStore`
- **Summary:** Abstraction for a cache store with support for various operations including TTL and expiration strategies.

#### Methods & Properties

- **GetAsync``1** - Retrieves a value from the cache.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result containing the cached value or null if not found or expired.
- **SetAsync``1** - Sets a value in the cache with optional TTL.
  - Parameters:
    - `key`: The cache key.
    - `value`: The value to cache.
    - `ttlMinutes`: Optional time-to-live in minutes. If null, uses default or no expiration.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **SetAsync``1** - Sets a value in the cache with cache options.
  - Parameters:
    - `key`: The cache key.
    - `value`: The value to cache.
    - `options`: Cache options including TTL, strategy, and sliding expiration.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **RemoveAsync** - Removes a value from the cache.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **RemoveByPrefixAsync** - Removes all values from the cache that match the specified key prefix.
  - Parameters:
    - `keyPrefix`: The prefix to match.
    - `ct`: Cancellation token.
  - Returns: A Result containing the number of entries removed.
- **ExistsAsync** - Checks if a key exists in the cache and is not expired.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result indicating whether the key exists and is valid.
- **ClearAsync** - Clears all entries from the cache.
  - Parameters:
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.

### MemoryCacheService

- **Namespace:** `SmartWorkz.Shared.MemoryCacheService`
- **Summary:** In-memory L1 cache service implementation with thread-safe operations and tenant isolation.
            Suitable for single-process deployments with TTL and expiration support.

#### Methods & Properties

- **BuildKey** - Builds a tenant-scoped cache key.
  - Parameters:
    - `key`: Original cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
  - Returns: Tenant-scoped key in format "{tenantId}:{key}".
- **GetAsync``1** - Gets a cached value by key with tenant isolation. Returns failure if not found or expired.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Result containing cached value, or failure if not found or expired.
- **SetAsync``1** - Sets a cached value with optional TTL expiration and tenant isolation.
  - Parameters:
    - `key`: Cache key.
    - `value`: Value to cache.
    - `ttlMinutes`: Time to live in minutes. If null, no expiration.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveAsync** - Removes a single cache entry with tenant isolation.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveByPrefixAsync** - Removes all cache entries matching a prefix pattern with tenant isolation.
            Example: RemoveByPrefixAsync("user:*", "tenant1") removes "tenant1:user:*" entries.
  - Parameters:
    - `prefix`: Key prefix to match.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **ExistsAsync** - Checks if a key exists in the cache with tenant isolation (ignores expiration check).
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: True if key exists and is not expired; false otherwise.
- **ClearAsync** - Clears all cache entries for the specified tenant (or "default" if not specified).
            Does not clear entries from other tenants.
  - Parameters:
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.

### MemoryCacheStore

- **Namespace:** `SmartWorkz.Shared.MemoryCacheStore`
- **Summary:** In-memory implementation of ICacheStore with TTL support and thread-safe operations.

#### Methods & Properties

- **#ctor** - Creates a new instance of MemoryCacheStore with default options.
- **#ctor** - Creates a new instance of MemoryCacheStore with specified default options.
- **GetAsync``1** - Retrieves a value from the cache.
- **SetAsync``1** - Sets a value in the cache with optional TTL.
- **SetAsync``1** - Sets a value in the cache with cache options.
- **RemoveAsync** - Removes a value from the cache.
- **RemoveByPrefixAsync** - Removes all values from the cache that match the specified key prefix.
- **ExistsAsync** - Checks if a key exists in the cache and is not expired.
- **ClearAsync** - Clears all entries from the cache.
- **CleanupExpiredEntries** - Performs cleanup of expired entries. This is useful for periodic maintenance.

### ISmsService

- **Namespace:** `SmartWorkz.Shared.ISmsService`
- **Summary:** Defines a contract for SMS communication services.
            Provides methods for sending SMS messages to single or multiple recipients.

#### Methods & Properties

- **SendAsync** - Sends an SMS message to a single recipient.
  - Parameters:
    - `phoneNumber`: The recipient phone number (E.164 format recommended)
    - `message`: The SMS message content
    - `cancellationToken`: Cancellation token
  - Returns: Result containing the SMS ID if successful
- **SendBatchAsync** - Sends an SMS message to multiple recipients (batch).
  - Parameters:
    - `phoneNumbers`: Collection of recipient phone numbers
    - `message`: The SMS message content sent to all recipients
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of SMS IDs if successful

### IWebSocketClient

- **Namespace:** `SmartWorkz.Shared.IWebSocketClient`
- **Summary:** Abstraction for WebSocket client operations.

#### Methods & Properties

- **SendAsync** - Sends a message asynchronously through the WebSocket connection.
- **ReceiveAsync** - Receives a message asynchronously from the WebSocket connection.
            Returns null if the connection is closed.
- **CloseAsync** - Closes the WebSocket connection gracefully.

### WebSocketClient

- **Namespace:** `SmartWorkz.Shared.WebSocketClient`
- **Summary:** Sealed implementation of IWebSocketClient using System.Net.WebSockets.

#### Methods & Properties

- **ConnectAsync** - Connects to a WebSocket server at the specified URI.
- **SendAsync** - Sends a message asynchronously through the WebSocket connection.
- **ReceiveAsync** - Receives a message asynchronously from the WebSocket connection.
            Returns null if the connection is closed.
- **CloseAsync** - Closes the WebSocket connection gracefully.

### ConfigurationHelper

- **Namespace:** `SmartWorkz.Shared.ConfigurationHelper`
- **Summary:** Provides typed access to configuration values with automatic type conversion and validation.
            
             This sealed class implements IConfigurationHelper to provide a strongly-typed interface
             for accessing configuration values. It supports automatic type conversion for common types
             including strings, numeric types, booleans, DateTimes, and enums.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ConfigurationHelper class.
  - Parameters:
    - `configuration`: The configuration source to read from.
- **GetRequired``1** - Gets a required configuration value and converts it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: The configuration value converted to type T.
- **GetOptional``1** - Gets an optional configuration value and converts it to the specified type,
            returning a default value if the key is not found or conversion fails.
  - Parameters:
    - `key`: The configuration key to retrieve.
    - `defaultValue`: The value to return if the key is not found or conversion fails.
  - Returns: The configuration value converted to type T, or the default value if retrieval or conversion fails.
- **TryGet``1** - Attempts to get a configuration value and convert it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: A Result<T> containing the converted value on success, or a failure result
            if the key is not found or conversion fails.
- **Exists** - Checks whether a configuration key exists and is not empty.
  - Parameters:
    - `key`: The configuration key to check.
  - Returns: True if the key exists and is not null or whitespace; otherwise false.
- **ConvertValue``1** - Converts a string value to the specified type using invariant culture for numeric types.
  - Parameters:
    - `value`: The string value to convert.
  - Returns: The converted value of type T.

### ConfigurationValidationException

- **Namespace:** `SmartWorkz.Shared.ConfigurationValidationException`
- **Summary:** Exception thrown when configuration validation fails, indicating that a required
            configuration key is missing, empty, or cannot be converted to the requested type.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ConfigurationValidationException class with a specified message.
  - Parameters:
    - `message`: The error message that explains the reason for the exception.
- **#ctor** - Initializes a new instance of the ConfigurationValidationException class with a specified message
            and a reference to the inner exception that is the cause of this exception.
  - Parameters:
    - `message`: The error message that explains the reason for the exception.
    - `innerException`: The exception that is the cause of the current exception.

### IConfigurationHelper

- **Namespace:** `SmartWorkz.Shared.IConfigurationHelper`
- **Summary:** Provides typed access to configuration values with automatic type conversion and validation.

#### Methods & Properties

- **GetRequired``1** - Gets a required configuration value and converts it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: The configuration value converted to type T.
- **GetOptional``1** - Gets an optional configuration value and converts it to the specified type,
            returning a default value if the key is not found or conversion fails.
  - Parameters:
    - `key`: The configuration key to retrieve.
    - `defaultValue`: The value to return if the key is not found or conversion fails.
  - Returns: The configuration value converted to type T, or the default value if retrieval or conversion fails.
- **TryGet``1** - Attempts to get a configuration value and convert it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: A Result<T> containing the converted value on success, or a failure result
            if the key is not found or conversion fails.
- **Exists** - Checks whether a configuration key exists and is not empty.
  - Parameters:
    - `key`: The configuration key to check.
  - Returns: True if the key exists and is not null or whitespace; otherwise false.

### SharedConstants

- **Namespace:** `SmartWorkz.Shared.SharedConstants`
- **Summary:** Shared configuration constants used throughout SmartWorkz.Shared.
            Enables centralized management of default values and limits.

### ICommand

- **Namespace:** `SmartWorkz.Shared.ICommand`
- **Summary:** Marker interface for command objects representing intent to change state.

### ICommandHandler`1

- **Namespace:** `SmartWorkz.Shared.ICommandHandler`1`
- **Summary:** Handler for processing a specific command type.

#### Methods & Properties

- **HandleAsync** - Handles the specified command asynchronously.
  - Parameters:
    - `command`: The command to handle.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task that represents the asynchronous handle operation.

### IQuery`1

- **Namespace:** `SmartWorkz.Shared.IQuery`1`
- **Summary:** Marker interface for query objects that return a result without modifying state.

### IQueryHandler`2

- **Namespace:** `SmartWorkz.Shared.IQueryHandler`2`
- **Summary:** Handler for processing a specific query type and returning results.

#### Methods & Properties

- **HandleAsync** - Handles the specified query asynchronously and returns the result.
  - Parameters:
    - `query`: The query to handle.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task that represents the asynchronous handle operation and contains the query result.

### MediatorCommandDispatcher

- **Namespace:** `SmartWorkz.Shared.MediatorCommandDispatcher`
- **Summary:** Routes commands to their appropriate handlers via dependency injection.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `serviceProvider`: The service provider for resolving handlers.
- **DispatchAsync``1** - Dispatches the specified command to its handler asynchronously.
  - Parameters:
    - `command`: The command to dispatch.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task representing the asynchronous dispatch operation.

### AdoHelper

- **Namespace:** `SmartWorkz.Shared.AdoHelper`
- **Summary:** ADO.NET helper for executing queries and managing connections.
            Works with any IDbProvider implementation.

#### Methods & Properties

- **ExecuteScalarAsync``1** - Execute scalar query (returns single value).
- **ExecuteNonQueryAsync** - Execute non-query command (INSERT, UPDATE, DELETE).
- **ExecuteQueryAsync``1** - Execute query and map results to objects.
- **ExecuteStoredProcedureAsync** - Execute stored procedure.
- **ExecuteQueryMultipleAsync``2** - Execute query returning multiple result sets (2 sets).
- **ExecuteQueryMultipleAsync``3** - Execute query returning multiple result sets (3 sets).
- **ExecuteQueryMultipleAsync``4** - Execute query returning multiple result sets (4 sets).
- **ExecuteTransactionAsync** - Execute transaction with multiple commands.

### CsvHelper

- **Namespace:** `SmartWorkz.Shared.CsvHelper`
- **Summary:** Provides static methods for reading and writing CSV data with support for column mapping,
            quoted fields, embedded delimiters, and newlines.
            RFC 4180 compliant CSV parsing and writing.
            Sealed class to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **CsvWriter``1** - Serializes a collection of objects to CSV format.
  - Parameters:
    - `items`: The collection of objects to serialize.
    - `options`: CSV options. If null, defaults are used.
  - Returns: A Result containing the CSV string if successful; otherwise a failure.
- **CsvReader``1** - Asynchronously deserializes CSV content to a collection of objects.
  - Parameters:
    - `content`: The CSV content string.
    - `mapping`: Column mapping configuration. If null, property names are used as headers.
    - `options`: CSV options. If null, defaults are used.
  - Returns: A Result containing the deserialized list if successful; otherwise a failure.
- **ParseCsvLines** - Parses CSV content into a list of records (each record is a list of field values).
            Handles quoted fields with embedded delimiters and newlines.
- **WriteRecord** - Writes a single CSV record (list of field values) to the string builder.
            Handles quoting of fields with special characters.
- **ConvertValue** - Converts a string value to the specified type.
- **IsNullableType** - Determines if a type is nullable (Nullable<T> or reference type).

### CsvMapping`1

- **Namespace:** `SmartWorkz.Shared.CsvMapping`1`
- **Summary:** Defines column mapping for CSV operations using a fluent API.
            Supports mapping object properties to CSV columns with custom headers.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **Column``1** - Adds a column mapping for the specified property.
  - Parameters:
    - `propertyExpression`: Expression selecting the property to map.
    - `csvHeader`: The CSV column header name.
  - Returns: This instance for method chaining.
- **ExtractPropertyInfo``1** - Extracts property information from a lambda expression.
  - Parameters:
    - `expression`: The lambda expression.
  - Returns: The PropertyInfo if the expression resolves to a property; otherwise null.
- **CreateAuto** - Creates a mapping automatically from all public properties of type T.
            Property names are used as CSV headers.
  - Returns: A new CsvMapping instance with all properties mapped.

### CsvOptions

- **Namespace:** `SmartWorkz.Shared.CsvOptions`
- **Summary:** Configuration options for CSV read/write operations.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **#ctor** - Creates a default instance of CsvOptions.
- **#ctor** - Creates an instance of CsvOptions with specified delimiter and quote character.
  - Parameters:
    - `delimiter`: The field delimiter character.
    - `quoteChar`: The quote character for quoted fields.

### DbProviderFactory

- **Namespace:** `SmartWorkz.Shared.DbProviderFactory`
- **Summary:** Factory for creating database provider instances.
            Resolves provider name from connection string or explicit specification.

#### Methods & Properties

- **Register** - Register custom provider implementation.
- **GetProvider** - Get provider by name.
- **GetProvider** - Get provider by enum value.
- **GetProviderFromConnectionString** - Get provider from connection string (detects provider automatically).

### IDbProvider

- **Namespace:** `SmartWorkz.Shared.IDbProvider`
- **Summary:** Abstraction for database provider-specific operations.
            Supports multiple providers: SQL Server, MySQL, PostgreSQL, SQLite, Oracle.

#### Methods & Properties

- **CreateConnection** - Create connection with connection string.
- **GetParameterPrefix** - Get parameter prefix for this provider (@, :, $).
- **GetLastInsertIdSql** - Get SQL for last inserted ID based on provider.
- **GetPaginationSql** - Get SQL for pagination based on provider.
- **FormatIdentifier** - Format table/column name for provider (e.g., [brackets] for SQL Server).
- **TestConnectionAsync** - Test connection validity.

### DatabaseProvider

- **Namespace:** `SmartWorkz.Shared.DatabaseProvider`
- **Summary:** Enum of supported database providers.

### QueryMultipleHelper

- **Namespace:** `SmartWorkz.Shared.QueryMultipleHelper`
- **Summary:** Helper for executing multiple queries in a single database roundtrip.
            Eliminates N+1 query problems by batching queries together.

#### Methods & Properties

- **QueryMultipleAsync``2** - Execute multiple queries and return results as tuple.
             Single roundtrip, single SQL execution, improved performance.
- **QueryMultipleAsync``3** - Execute 3 queries in single roundtrip.
- **QueryMultipleAsync``4** - Execute 4 queries in single roundtrip.
- **QueryMultipleAsync``5** - Execute 5 queries in single roundtrip.

### QueryResult`1

- **Namespace:** `SmartWorkz.Shared.QueryResult`1`
- **Summary:** Result wrapper for query operations.

### XmlHelper

- **Namespace:** `SmartWorkz.Shared.XmlHelper`
- **Summary:** Provides static methods for XML serialization, deserialization, and XPath queries.
            Uses System.Xml.Linq for manipulation and reflection for property mapping.
            Sealed class to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **Serialize``1** - Serializes an object to an XML string using reflection.
  - Parameters:
    - `obj`: The object to serialize.
    - `options`: XML options. If null, defaults are used.
  - Returns: A Result containing the XML string if successful; otherwise a failure.
- **Deserialize``1** - Deserializes an XML string to an object of type T using reflection.
  - Parameters:
    - `xml`: The XML string to deserialize.
    - `options`: XML options. If null, defaults are used.
  - Returns: A Result containing the deserialized object if successful; otherwise a failure.
- **Query** - Executes an XPath query on an XML string and returns matching element values.
  - Parameters:
    - `xml`: The XML string to query.
    - `xpathExpression`: The XPath expression to execute.
  - Returns: A Result containing a list of matched values if successful; otherwise a failure.
- **SerializeObject** - Recursively serializes an object's properties into an XML element.
- **DeserializeObject** - Recursively deserializes an XML element into an object's properties.
- **IsBasicType** - Determines if a type is a basic/primitive type supported by XML.
- **IsGenericList** - Determines if a type is a generic List<T>.
- **IsComplexType** - Determines if a type is a complex (non-primitive) type.
- **ConvertToXmlValue** - Converts a value to its XML-safe string representation.
- **ConvertFromXmlValue** - Converts an XML string value to the specified type.

### XmlOptions

- **Namespace:** `SmartWorkz.Shared.XmlOptions`
- **Summary:** Configuration options for XML serialization, deserialization, and query operations.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **#ctor** - Creates a default instance of XmlOptions.
- **#ctor** - Creates an instance of XmlOptions with a specified root element name.
  - Parameters:
    - `rootElement`: The name of the root element.
- **#ctor** - Creates an instance of XmlOptions with specified configuration.
  - Parameters:
    - `rootElement`: The name of the root element.
    - `includeXmlDeclaration`: Whether to include the XML declaration.
    - `indent`: Whether to indent the output.

### ApplicationHealth

- **Namespace:** `SmartWorkz.Shared.ApplicationHealth`
- **Summary:** Represents the overall health status of the application.

### CorrelationContext

- **Namespace:** `SmartWorkz.Shared.CorrelationContext`
- **Summary:** A sealed implementation of  for distributed request tracing.

#### Methods & Properties

- **#ctor** - Initializes a new instance with a generated correlation ID.
- **#ctor** - Initializes a new instance with a specified correlation ID.
  - Parameters:
    - `correlationId`: The correlation ID to use
- **#ctor** - Initializes a child context from a parent context.

### CpuUsage

- **Namespace:** `SmartWorkz.Shared.CpuUsage`
- **Summary:** Represents CPU usage information.

### DiagnosticsHelper

- **Namespace:** `SmartWorkz.Shared.DiagnosticsHelper`
- **Summary:** Sealed helper class for system diagnostics and application health monitoring.
            Provides methods to gather system information, CPU/memory/disk usage, and determine application health.

#### Methods & Properties

- **Initialize** - Initializes the application start time (called once at application startup).
- **GetSystemInfo** - Gets comprehensive system information including CPU, memory, disk, and processor count.
  - Returns: A Result containing SystemInfo or error details.
- **GetMemoryUsage** - Gets memory usage statistics for the current process and system.
  - Returns: A Result containing MemoryUsage or error details.
- **GetCpuUsage** - Gets CPU utilization percentage.
  - Returns: A Result containing CpuUsage or error details.
- **GetDiskSpace** - Gets disk space information for a specific drive.
  - Parameters:
    - `drive`: The drive letter (e.g., "C:", "D:"). Defaults to "C:".
  - Returns: A Result containing DiskSpace or error details.
- **GetUptime** - Gets the application uptime since the last Initialize() call or application start.
  - Returns: A Result containing the uptime as a TimeSpan or error details.
- **GetApplicationHealth** - Gets the overall health status of the application based on system metrics.
  - Returns: A Result containing ApplicationHealth or error details.
- **IsHealthy** - Determines if the application is considered healthy based on the provided health status.
  - Parameters:
    - `health`: The ApplicationHealth object to evaluate.
  - Returns: True if the status is Healthy, false otherwise.
- **InitializeCpuCounter** - Initializes the CPU performance counter (called once).
- **GetMemoryUsageInternal** - Internal method to get memory usage statistics.
- **GetCpuUsageInternal** - Internal method to get CPU usage percentage.
- **GetDiskSpaceInternal** - Internal method to get disk space information.

### DiskSpace

- **Namespace:** `SmartWorkz.Shared.DiskSpace`
- **Summary:** Represents disk space information for a drive.

### HealthCheck

- **Namespace:** `SmartWorkz.Shared.HealthCheck`
- **Summary:** Represents a single health check result.

### HealthStatus

- **Namespace:** `SmartWorkz.Shared.HealthStatus`
- **Summary:** Represents the health status of the application.

### ICorrelationContext

- **Namespace:** `SmartWorkz.Shared.ICorrelationContext`
- **Summary:** Defines a correlation context for distributed request tracing across systems.

#### Methods & Properties

- **SetProperty** - Adds or updates a property in the correlation context.
- **TryGetProperty** - Attempts to retrieve a property from the correlation context.
- **CreateChildContext** - Creates a child correlation context for nested operations (for async/distributed flows).

### MemoryUsage

- **Namespace:** `SmartWorkz.Shared.MemoryUsage`
- **Summary:** Represents memory usage information.

### MetricsHelper

- **Namespace:** `SmartWorkz.Shared.MetricsHelper`
- **Summary:** Provides utilities for collecting and tracking performance metrics.

#### Methods & Properties

- **StartTimer** - Starts a timer and returns an IDisposable that logs elapsed time on disposal.
- **TrackExecution``1** - Tracks the execution time and result of a function.
- **MeasureMemory** - Captures memory usage before and after a block of code execution.

### SystemInfo

- **Namespace:** `SmartWorkz.Shared.SystemInfo`
- **Summary:** Represents system information including CPU, memory, and disk details.

### EventStoreSnapshot

- **Namespace:** `SmartWorkz.Shared.EventStoreSnapshot`
- **Summary:** Represents a snapshot of an aggregate's state at a specific version.
            Snapshots optimize event sourcing by reducing the number of events needed for reconstruction.

### IEventStore

- **Namespace:** `SmartWorkz.Shared.IEventStore`
- **Summary:** Abstraction for an immutable event store that persists domain events.
            Enables event sourcing patterns for temporal queries, audit trails, and event replay.

#### Methods & Properties

- **AppendEventsAsync** - Appends events to the event stream for an aggregate.
            Events are immutable and persist as an append-only log.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `events`: The domain events to append
    - `cancellationToken`: Cancellation token
  - Returns: Task representing the asynchronous operation
- **GetEventsAsync** - Retrieves all events for an aggregate in chronological order.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: Collection of domain events for the aggregate, empty if none exist
- **GetEventsSinceAsync** - Retrieves events for an aggregate after a specific version.
            Useful for incremental event replay and event streaming.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `version`: The version after which to retrieve events
    - `cancellationToken`: Cancellation token
  - Returns: Collection of events after the specified version, empty if none exist
- **GetSnapshotAsync** - Retrieves the latest snapshot for an aggregate if one exists.
            Snapshots optimize aggregate reconstruction by storing intermediate state.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: Snapshot data if exists; null if no snapshot is available
- **SaveSnapshotAsync** - Saves a snapshot of aggregate state at a specific version.
            Snapshots reduce the number of events needed to replay an aggregate.
  - Parameters:
    - `snapshot`: The snapshot to save
    - `cancellationToken`: Cancellation token
  - Returns: Task representing the asynchronous operation
- **GetAggregateAsync``1** - Reconstructs an aggregate from its event history.
            Optionally uses snapshots for performance optimization.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: The reconstructed aggregate instance, or null if no events exist

### SqlEventStore

- **Namespace:** `SmartWorkz.Shared.SqlEventStore`
- **Summary:** SQL Server implementation of the event store using Dapper for data access.
            Provides immutable append-only event log with snapshot support for optimization.
            Implements optimistic concurrency control using version numbers.

#### Methods & Properties

- **AppendEventsAsync** - Appends events to the event stream for an aggregate.
- **GetEventsAsync** - Retrieves all events for an aggregate in chronological order.
- **GetEventsSinceAsync** - Retrieves events for an aggregate after a specific version.
- **GetSnapshotAsync** - Retrieves the latest snapshot for an aggregate if one exists.
- **SaveSnapshotAsync** - Saves a snapshot of aggregate state at a specific version.
- **GetAggregateAsync``1** - Reconstructs an aggregate from its event history.
            Optionally uses snapshots for performance optimization.
- **GetCurrentVersionAsync** - Gets the current version number for an aggregate.
- **DeserializeEvent** - Deserializes a stored event record back to IDomainEvent.

### IDomainEvent

- **Namespace:** `SmartWorkz.Shared.IDomainEvent`
- **Summary:** Base interface for domain events in event-driven architecture.
            Provides core event metadata for tracking and publishing.

### IEventPublisher

- **Namespace:** `SmartWorkz.Shared.IEventPublisher`
- **Summary:** Publishes domain events for event-driven architecture.

#### Methods & Properties

- **PublishAsync``1** - Publishes a single domain event.
  - Parameters:
    - `event`: Event to publish.
    - `cancellationToken`: Cancellation token.
- **PublishAsync``1** - Publishes multiple domain events.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token.

### IEventSubscriber

- **Namespace:** `SmartWorkz.Shared.IEventSubscriber`
- **Summary:** Registers event handlers for domain events.

#### Methods & Properties

- **Subscribe``1** - Subscribes a handler to an event type.
            Multiple handlers can subscribe to the same event.
  - Parameters:
    - `handler`: Async handler function. Receives event and cancellation token.

### InMemoryEventPublisher

- **Namespace:** `SmartWorkz.Shared.InMemoryEventPublisher`
- **Summary:** In-memory event publisher that executes all registered handlers sequentially.
            Provides synchronous event delivery with exception handling and result reporting.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the InMemoryEventPublisher with a subscriber.
  - Parameters:
    - `subscriber`: The event subscriber containing registered handlers.
- **PublishAsync``1** - Publishes a single domain event to all registered handlers.
            Handlers are invoked sequentially in registration order.
            If any handler throws an exception, it is caught and a failure Result is returned.
            Other handlers will attempt to execute even if a previous handler fails.
  - Parameters:
    - `event`: The event instance to publish.
    - `cancellationToken`: Cancellation token to cancel handler execution.
  - Returns: A task representing the asynchronous operation.
- **PublishAsync``1** - Publishes multiple domain events to all registered handlers.
            Events are published sequentially in the order provided.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token to cancel handler execution.
  - Returns: A task representing the asynchronous batch operation.

### InMemoryEventSubscriber

- **Namespace:** `SmartWorkz.Shared.InMemoryEventSubscriber`
- **Summary:** In-memory event subscriber that maintains a registry of event handlers.
            Supports multiple handlers per event type using thread-safe concurrent collections.

#### Methods & Properties

- **Subscribe``1** - Subscribes a handler to an event type.
            Multiple handlers can be registered for the same event type and will execute sequentially.
  - Parameters:
    - `handler`: Async handler function that receives event and cancellation token.
- **GetHandlers** - Gets all registered handlers for a given event type.
            Returns an empty list if no handlers are registered for the type.
  - Parameters:
    - `eventType`: The event type to retrieve handlers for.
  - Returns: List of registered handlers (delegates).

### MassTransitEventPublisher

- **Namespace:** `SmartWorkz.Shared.MassTransitEventPublisher`
- **Summary:** Distributed event publisher using MassTransit message bus.
            Supports both single and batch event publishing with async/await patterns.
            Suitable for production environments with message broker backend (RabbitMQ, Azure Service Bus, etc).

#### Methods & Properties

- **#ctor** - Initializes a new instance of MassTransitEventPublisher.
  - Parameters:
    - `publishEndpoint`: MassTransit publish endpoint for message distribution.
    - `logger`: Logger for event publication tracking.
- **PublishAsync``1** - Publishes a single domain event to the message bus asynchronously.
  - Parameters:
    - `event`: Event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous publish operation.
- **PublishAsync``1** - Publishes multiple domain events to the message bus asynchronously.
            Events are published sequentially in the order provided.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous batch publish operation.

### PublisherType

- **Namespace:** `SmartWorkz.Shared.PublisherType`
- **Summary:** Specifies the publisher type for event publishing.

### ServiceCollectionExtensions

- **Namespace:** `SmartWorkz.Shared.ServiceCollectionExtensions`
- **Summary:** Extension methods for IServiceCollection to register Core.Shared services.

#### Methods & Properties

- **AddCoreSharedServices** - Adds Core.Shared services including TemplateEngine for template rendering.
- **AddEventPublishing** - Adds event publishing services to the dependency injection container.
            Supports switching between in-memory and MassTransit publishers based on application needs.
  - Parameters:
    - `services`: The service collection.
    - `publisherType`: The publisher type to use (defaults to InMemory).
  - Returns: The service collection for method chaining.

### DefaultFeatureFlagService

- **Namespace:** `SmartWorkz.Shared.DefaultFeatureFlagService`
- **Summary:** Global (non-tenant) feature flag service with in-memory storage.
            Thread-safe implementation suitable for single-process deployments.
            Use for organization-wide feature toggles; use ITenantFeatureFlags for tenant-scoped flags.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature flag is enabled.
            Returns false for unknown flags (does not throw).
  - Parameters:
    - `flagName`: The name of the feature flag to check.
    - `cancellationToken`: Cancellation token.
  - Returns: True if the flag exists and is enabled; false otherwise.
- **GetEnabledFeaturesAsync** - Gets all enabled feature flags.
            Returns empty list if no flags are enabled.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: A read-only list of enabled feature flag names.
- **EnableFlag** - Enables a feature flag.
            Creates the flag if it doesn't exist.
  - Parameters:
    - `flagName`: The name of the feature flag to enable.
- **DisableFlag** - Disables a feature flag.
            Creates the flag as disabled if it doesn't exist.
  - Parameters:
    - `flagName`: The name of the feature flag to disable.

### IFeatureFlagService

- **Namespace:** `SmartWorkz.Shared.IFeatureFlagService`
- **Summary:** Global feature flag service for cross-tenant feature control.
            Use for organization-wide feature toggles (not tenant-specific).
            For tenant-scoped flags, use ITenantFeatureFlags.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a global feature is enabled.
  - Parameters:
    - `flagName`: Feature flag name (e.g., "NEW_DASHBOARD", "BETA_REPORTING").
    - `cancellationToken`: Cancellation token.
  - Returns: True if feature is enabled globally.
- **GetEnabledFeaturesAsync** - Gets all enabled global features.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: List of enabled feature flag names.

### IFileStorageService

- **Namespace:** `SmartWorkz.Shared.IFileStorageService`
- **Summary:** Interface for file storage operations supporting both local and cloud providers.

#### Methods & Properties

- **UploadAsync** - Uploads a file to storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `content`: The file content stream.
    - `metadata`: The file metadata.
    - `cancellationToken`: The cancellation token.
  - Returns: The full path or URI of the uploaded file.
- **DownloadAsync** - Downloads a file from storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: A stream containing the file content. Caller must dispose using 'using' statement.
- **DeleteAsync** - Deletes a file from storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
- **ExistsAsync** - Checks if a file exists in storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: True if the file exists, false otherwise.
- **GetMetadataAsync** - Gets metadata for a file in storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: FileMetadata if file exists, null otherwise.
- **ListAsync** - Lists files in a directory or container prefix.
  - Parameters:
    - `folderPath`: The relative folder path or blob prefix.
    - `cancellationToken`: The cancellation token.
  - Returns: A read-only collection of FileMetadata for files in the directory/prefix.
- **GenerateTemporaryUrlAsync** - Generates a temporary download URL for a file.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `expiration`: The expiration duration from now.
    - `cancellationToken`: The cancellation token.
  - Returns: A URL that can be used to download the file. For local storage, returns the full file path.

### GridColumn

- **Namespace:** `SmartWorkz.Shared.GridColumn`
- **Summary:** Defines a single column in a grid, including display options, sorting, filtering, and rendering hints.

### GridExportOptions

- **Namespace:** `SmartWorkz.Shared.GridExportOptions`
- **Summary:** Configuration for grid data export (CSV, Excel).

### GridRequest

- **Namespace:** `SmartWorkz.Shared.GridRequest`
- **Summary:** Request parameters for grid data fetching, extending PagedQuery with filtering support.

#### Methods & Properties

- **#ctor** - Request parameters for grid data fetching, extending PagedQuery with filtering support.

### GridResponse`1

- **Namespace:** `SmartWorkz.Shared.GridResponse`1`
- **Summary:** Response from a grid data request, including paged data, column metadata, and filter options.

### IGridDataProvider

- **Namespace:** `SmartWorkz.Shared.IGridDataProvider`
- **Summary:** Abstraction for grid data fetching. Implementations handle API calls or in-memory queries.
            Enables platform independence: Web uses HTTP, MAUI uses direct API client, Desktop uses local DB.

#### Methods & Properties

- **GetDataAsync``1** - Fetch paged grid data based on request (sorting, filtering, pagination).
  - Parameters:
    - `request`: Grid request with sorting, paging, and filter criteria.
    - `cancellationToken`: Cancellation token for async operations.
  - Returns: Result containing GridResponse or error details.

### Guard

- **Namespace:** `SmartWorkz.Shared.Guard`
- **Summary:** Static guard clauses for argument validation at method entry points.
             Throw immediately on invalid input — fail fast, fail loudly.
            
             Usage:
               Guard.NotNull(userId, nameof(userId));
               Guard.NotEmpty(name, nameof(name));
               Guard.InRange(pageSize, 1, 100, nameof(pageSize));
            
             These replace the ValidationExtensions.EnsureNotNull() extension method
             and the scattered ArgumentNullException throws throughout the codebase.

#### Methods & Properties

- **NotNull``1** - Throws ArgumentNullException if value is null.
- **NotNull``1** - Throws ArgumentNullException if value is null (struct/nullable).
- **NotEmpty** - Throws ArgumentException if string is null, empty, or whitespace.
- **NotEmpty``1** - Throws ArgumentException if collection is null or has no elements.
- **NotDefault``1** - Throws ArgumentException if value equals the default for its type (0, null, Guid.Empty).
- **InRange``1** - Throws ArgumentOutOfRangeException if value is outside [min, max].
- **Requires** - Throws ArgumentException if condition is false.

### EncryptionHelper

- **Namespace:** `SmartWorkz.Shared.EncryptionHelper`
- **Summary:** Cryptographic utilities for hashing and encryption.
            Uses PBKDF2 for password hashing and AES-256 for data encryption.

#### Methods & Properties

- **HashPassword** - Hash password using PBKDF2 with SHA256.
- **VerifyPassword** - Verify password against hash.
- **Encrypt** - Encrypt text using AES-256-GCM with provided key.
- **Decrypt** - Decrypt text using AES-256-GCM with provided key.
- **GenerateRandomString** - Generate cryptographically secure random string.
- **GenerateEncryptionKey** - Generate random encryption key (Base64 encoded).
- **ComputeSha256** - Compute SHA256 hash of text for integrity checking.

### JsonHelper

- **Namespace:** `SmartWorkz.Shared.JsonHelper`
- **Summary:** JSON serialization utilities using System.Text.Json.
            Provides consistent serialization options across the application.

#### Methods & Properties

- **Serialize``1** - Serialize object to JSON string.
- **Serialize** - Serialize object to JSON string with dynamic type.
- **Deserialize``1** - Deserialize JSON string to object.
- **Deserialize** - Deserialize JSON string to object with dynamic type.
- **DeserializeAsync``1** - Deserialize JSON asynchronously from stream.
- **SerializeAsync``1** - Serialize asynchronously to stream.
- **IsValidJson** - Check if string is valid JSON.
- **GetValueByPath** - Parse JSON and extract value at specified path (dot notation).

### IHttpClient

- **Namespace:** `SmartWorkz.Shared.IHttpClient`
- **Summary:** Abstraction for HTTP client operations with support for async/await and cancellation.
            Implementations should handle retries, timeouts, and error responses gracefully.

#### Methods & Properties

- **GetAsync``1** - Sends a GET request and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **PostAsync``1** - Sends a POST request with JSON body and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **PutAsync``1** - Sends a PUT request with JSON body and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **DeleteAsync``1** - Sends a DELETE request and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **GetAsync** - Sends a GET request and returns a string response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.
- **PostAsync** - Sends a POST request with JSON body and returns a string response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.

### RetryStrategy

- **Namespace:** `SmartWorkz.Shared.RetryStrategy`
- **Summary:** Specifies the backoff strategy to use when retrying failed HTTP requests.

### RetryPolicy

- **Namespace:** `SmartWorkz.Shared.RetryPolicy`
- **Summary:** Configures automatic retry behavior for failed HTTP requests.

### AuditRecord

- **Namespace:** `SmartWorkz.Shared.AuditRecord`
- **Summary:** Represents an immutable audit record with all relevant audit information.

#### Methods & Properties

- **#ctor** - Represents an immutable audit record with all relevant audit information.
  - Parameters:
    - `Id`: The unique identifier of the audit record
    - `EntityType`: The type of entity being audited (e.g., "User", "BlogPost")
    - `EntityId`: The identifier of the audited entity
    - `Action`: The action performed (Create, Update, Delete, etc.)
    - `UserId`: The identifier of the user who performed the action
    - `PerformedAt`: The timestamp when the action was performed
    - `Metadata`: Optional metadata dictionary containing additional context

### EnrichedLogger

- **Namespace:** `SmartWorkz.Shared.EnrichedLogger`
- **Summary:** Enriched logger wrapper around ILogger that provides structured logging methods
            for domain events, commands, sagas, file operations, and background jobs.
            Uses structured properties instead of string interpolation for better queryability.

#### Methods & Properties

- **#ctor** - Creates a new instance of EnrichedLogger.
  - Parameters:
    - `logger`: The underlying ILogger instance
- **LogCommandExecuted** - Logs command execution with duration and other metrics.
  - Parameters:
    - `commandType`: The type of command being executed
    - `duration`: How long the command took to execute
- **LogCommandExecutionError** - Logs a command execution error with exception details.
  - Parameters:
    - `commandType`: The type of command that failed
    - `exception`: The exception that occurred
- **LogCommandValidationError** - Logs a command with validation errors.
  - Parameters:
    - `commandType`: The type of command
    - `errors`: Dictionary of validation errors
- **LogEventPublished** - Logs an event publication with metadata.
  - Parameters:
    - `eventType`: The type of event being published
    - `eventId`: The unique identifier of the event
- **LogEventPublishedWithContext** - Logs an event with additional context properties.
  - Parameters:
    - `eventType`: The type of event
    - `eventId`: The event identifier
    - `context`: Additional context data
- **LogEventSubscribed** - Logs an event subscription.
  - Parameters:
    - `eventType`: The type of event being subscribed to
    - `subscriberType`: The subscriber type
- **LogSagaStarted** - Logs the start of a saga with its initial state.
  - Parameters:
    - `sagaId`: The unique saga identifier
    - `state`: The initial saga state
- **LogSagaStateTransition** - Logs a saga state transition.
  - Parameters:
    - `sagaId`: The saga identifier
    - `fromState`: The previous state
    - `toState`: The new state
- **LogSagaCompleted** - Logs the completion of a saga.
  - Parameters:
    - `sagaId`: The saga identifier
    - `duration`: How long the saga took to complete
- **LogSagaFailed** - Logs a saga failure.
  - Parameters:
    - `sagaId`: The saga identifier
    - `exception`: The exception that caused the failure
- **LogFileOperation** - Logs file operations such as upload, download, delete.
  - Parameters:
    - `operation`: The type of operation (Upload, Download, Delete, etc.)
    - `filePath`: The file path or URI
- **LogFileOperationWithSize** - Logs a file operation with size information.
  - Parameters:
    - `operation`: The type of operation
    - `filePath`: The file path
    - `sizeBytes`: The file size in bytes
- **LogFileOperationError** - Logs a file operation error.
  - Parameters:
    - `operation`: The operation that failed
    - `filePath`: The file path
    - `exception`: The exception that occurred
- **LogJobQueued** - Logs when a background job is queued.
  - Parameters:
    - `jobId`: The unique job identifier
    - `jobType`: The type of job being queued
- **LogJobStarted** - Logs when a background job starts processing.
  - Parameters:
    - `jobId`: The job identifier
    - `jobType`: The job type
- **LogJobCompleted** - Logs successful job completion.
  - Parameters:
    - `jobId`: The job identifier
    - `duration`: How long the job took to complete
- **LogJobFailed** - Logs a job failure.
  - Parameters:
    - `jobId`: The job identifier
    - `exception`: The exception that caused the failure
- **LogJobRetry** - Logs job retry attempt.
  - Parameters:
    - `jobId`: The job identifier
    - `attemptNumber`: The current attempt number
    - `maxRetries`: The maximum number of retries
- **LogWithContext** - Logs a message with structured context properties.
  - Parameters:
    - `operationName`: The name of the operation
    - `context`: Dictionary of contextual properties
- **LogPerformanceMetrics** - Logs performance metrics for an operation.
  - Parameters:
    - `operationName`: The operation name
    - `duration`: The operation duration
    - `resultStatus`: The result status (Success, Failure, etc.)
- **LogCorrelation** - Logs a correlation ID for request tracing.
  - Parameters:
    - `correlationId`: The correlation identifier
    - `userId`: Optional user identifier
    - `requestPath`: Optional request path
- **LogUnhandledException** - Logs unhandled exceptions as critical errors.
  - Parameters:
    - `exception`: The exception that occurred
    - `operationName`: The operation that failed

### IAuditLogger

- **Namespace:** `SmartWorkz.Shared.IAuditLogger`
- **Summary:** Interface for structured audit logging with metadata support.

#### Methods & Properties

- **LogAuditAsync** - Logs an audit event with structured metadata.
  - Parameters:
    - `entityType`: The entity type being audited (e.g., "User", "BlogPost")
    - `entityId`: The unique identifier of the entity
    - `action`: The action performed (Create, Update, Delete, etc.)
    - `metadata`: Optional metadata dictionary for additional context
    - `cancellationToken`: Cancellation token
  - Returns: Result indicating success or failure
- **GetAuditHistoryAsync** - Retrieves audit logs for a specific entity.
  - Parameters:
    - `entityType`: The entity type
    - `entityId`: The entity identifier
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of audit records
- **GetUserActivityAsync** - Retrieves audit logs for a specific user across all entities.
  - Parameters:
    - `userId`: The user identifier
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of audit records

### ILogger

- **Namespace:** `SmartWorkz.Shared.ILogger`
- **Summary:** Abstraction for application logging.
            Decouples from specific logging frameworks (Serilog, NLog, etc.).

### LogLevel

- **Namespace:** `SmartWorkz.Shared.LogLevel`
- **Summary:** Log level severity.

### ILoggerFactory

- **Namespace:** `SmartWorkz.Shared.ILoggerFactory`
- **Summary:** Factory for creating logger instances by category/source.

### IMapper

- **Namespace:** `SmartWorkz.Shared.IMapper`
- **Summary:** Mapping service abstraction for transforming objects between types.
            Supports registration of mapping profiles and bidirectional conversions.

#### Methods & Properties

- **Map``2** - Map source object to target type.
- **Map** - Map source object to target type using dynamic type.
- **MapAsync``2** - Map asynchronously with potential async operations in profile.
- **MapCollection``2** - Map collection of sources to targets.
- **MapCollectionAsync``2** - Map collection asynchronously.
- **RegisterProfile``2** - Register a mapping profile.

### IMapperProfile

- **Namespace:** `SmartWorkz.Shared.IMapperProfile`
- **Summary:** Profile for defining mapping rules between types.
            Implemented by concrete profiles that configure source-to-target transformations.

### IMapperProfile`2

- **Namespace:** `SmartWorkz.Shared.IMapperProfile`2`
- **Summary:** Typed mapper profile for strong typing.

#### Methods & Properties

- **Map** - Transform source to target synchronously.
- **MapAsync** - Transform source to target asynchronously.

### SimpleMapper

- **Namespace:** `SmartWorkz.Shared.SimpleMapper`
- **Summary:** A simple in-memory mapper that supports registering and executing mapping profiles.

### IMetricsCollector

- **Namespace:** `SmartWorkz.Shared.IMetricsCollector`
- **Summary:** Abstraction for collecting application metrics and performance data.
            Enables tracking of operation duration, throughput, error rates, and custom metrics.
            Implementations integrate with OpenTelemetry for export to Prometheus/Grafana.

#### Methods & Properties

- **RecordOperationDuration** - Record operation duration in milliseconds.
  - Parameters:
    - `operationName`: Name of the operation being measured.
    - `durationMs`: Duration in milliseconds.
    - `status`: Optional status (e.g., "success", "error").
    - `tags`: Optional metadata tags for grouping and filtering.
- **RecordOperationCount** - Record operation count (increments counter).
  - Parameters:
    - `operationName`: Name of the operation.
    - `count`: Number to increment by (default 1).
    - `status`: Optional status label.
    - `tags`: Optional metadata tags.
- **RecordGaugeValue** - Record a gauge value (e.g., queue depth, memory usage).
  - Parameters:
    - `metricName`: Name of the gauge metric.
    - `value`: The gauge value to record.
    - `tags`: Optional metadata tags.
- **RecordError** - Record error/exception occurrence.
  - Parameters:
    - `operationName`: Name of the operation that failed.
    - `ex`: The exception that occurred.
    - `tags`: Optional metadata tags.
- **IncrementCounter** - Increment a custom counter.
  - Parameters:
    - `counterName`: Name of the counter.
    - `increment`: Amount to increment (default 1).
    - `tags`: Optional metadata tags.

### MetricsMiddleware

- **Namespace:** `SmartWorkz.Shared.MetricsMiddleware`
- **Summary:** ASP.NET Core middleware for automatic HTTP request/response metrics collection.
             Records operation duration, status, and errors for all HTTP requests.
            
             Usage:
                 app.UseMiddleware<MetricsMiddleware>();

### MetricsStartupExtensions

- **Namespace:** `SmartWorkz.Shared.MetricsStartupExtensions`
- **Summary:** Extension methods for registering application metrics in dependency injection.

#### Methods & Properties

- **AddApplicationMetrics** - Registers IMetricsCollector with OpenTelemetry implementation.
  - Parameters:
    - `services`: The service collection to register with.
  - Returns: The service collection for method chaining.

### OpenTelemetryMetricsCollector

- **Namespace:** `SmartWorkz.Shared.OpenTelemetryMetricsCollector`
- **Summary:** OpenTelemetry-based implementation of IMetricsCollector.
            Collects metrics using System.Diagnostics.Metrics for export to Prometheus/Grafana.

### DefaultTenantFeatureFlags

- **Namespace:** `SmartWorkz.Shared.DefaultTenantFeatureFlags`
- **Summary:** In-memory feature flag provider for tenant-scoped feature control.
            
             Uses ConcurrentDictionary to store tenant-specific flags:
             - Key: tenant ID
             - Value: HashSet of enabled feature flag names
            
             Thread-safe for concurrent operations. Suitable for in-process caching
             or dev/test scenarios. For distributed systems, integrate with a
             centralized feature flag service (Unleash, LaunchDarkly, etc.).

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature is enabled for the specified tenant.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").
    - `cancellationToken`: Cancellation token.
  - Returns: Task containing true if the feature is enabled for this tenant,
            false if the tenant doesn't exist or the flag is not enabled.
- **GetEnabledFeaturesAsync** - Gets all enabled features for a tenant.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Task containing a read-only list of enabled feature flag names.
            Returns an empty list if the tenant doesn't exist or has no enabled flags.
- **EnableFlag** - Enables a feature flag for a tenant.
            Idempotent — calling multiple times with the same flag is safe.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name.
- **DisableFlag** - Disables a feature flag for a tenant.
            Idempotent — calling multiple times with the same flag is safe.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name.

### ITenantContext

- **Namespace:** `SmartWorkz.Shared.ITenantContext`
- **Summary:** Scoped service providing current tenant ID for multi-tenant applications.
            Resolved from request context or claims principal.

#### Methods & Properties

- **GetTenantId** - Gets the current tenant identifier.
  - Returns: Tenant ID, or null if operating in single-tenant context.
- **SetTenantId** - Sets the current tenant identifier (rarely used; typically set from request context).
  - Parameters:
    - `tenantId`: Tenant ID to set.

### ITenantFeatureFlags

- **Namespace:** `SmartWorkz.Shared.ITenantFeatureFlags`
- **Summary:** Feature flag provider scoped to a specific tenant.
            Allows per-tenant feature control.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature is enabled for the specified tenant.
  - Parameters:
    - `tenantId`: Tenant ID.
    - `flagName`: Feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").
    - `cancellationToken`: Cancellation token.
  - Returns: True if feature is enabled for this tenant.
- **GetEnabledFeaturesAsync** - Gets all enabled features for a tenant.
  - Parameters:
    - `tenantId`: Tenant ID.
    - `cancellationToken`: Cancellation token.
  - Returns: List of enabled feature flag names.

### TenantContext

- **Namespace:** `SmartWorkz.Shared.TenantContext`
- **Summary:** Scoped tenant context using AsyncLocal for proper isolation across async boundaries.
            
             AsyncLocal ensures:
             - Thread-safe storage per async execution context
             - Isolation between concurrent requests (each gets its own context)
             - Proper inheritance to child tasks (when awaited)
            
             Survives async/await boundaries unlike ThreadLocal, making it suitable for async methods.

#### Methods & Properties

- **GetTenantId** - Gets the current tenant identifier.
  - Returns: The current tenant ID, or "default" if not set.
- **SetTenantId** - Sets the current tenant identifier.
  - Parameters:
    - `tenantId`: The tenant ID to set. Cannot be null or empty.

### FirebaseCloudMessagingService

- **Namespace:** `SmartWorkz.Shared.FirebaseCloudMessagingService`
- **Summary:** Firebase Cloud Messaging service implementation for sending push notifications.
            Supports single/batch user notifications, topic-based broadcasting, and multi-platform delivery (Android, iOS, Web).

#### Methods & Properties

- **#ctor** - Initializes a new instance of the FirebaseCloudMessagingService.
  - Parameters:
    - `logger`: Logger for diagnostic and error information.
- **SendAsync** - Sends a simple push notification to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends simple push notifications to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase device tokens).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends a rich push notification with metadata to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends a rich push notification to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase device tokens).
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SendToTopicAsync** - Sends a rich push notification to all users subscribed to a topic (broadcast).
  - Parameters:
    - `topic`: Topic name (e.g., "news", "promotions").
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SubscribeToTopicAsync** - Subscribes a user to a topic for broadcast notifications.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `topic`: Topic name to subscribe to.
    - `cancellationToken`: Cancellation token.
- **UnsubscribeFromTopicAsync** - Unsubscribes a user from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `topic`: Topic name to unsubscribe from.
    - `cancellationToken`: Cancellation token.

### IPushNotificationService

- **Namespace:** `SmartWorkz.Shared.IPushNotificationService`
- **Summary:** Service for sending push notifications using Firebase Cloud Messaging.

#### Methods & Properties

- **SendAsync** - Sends a simple push notification to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a simple push notification to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase tokens).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a rich push notification with metadata to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a rich push notification with metadata to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase tokens).
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendToTopicAsync** - Sends a push notification to all users subscribed to a topic.
  - Parameters:
    - `topic`: The topic name.
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SubscribeToTopicAsync** - Subscribes a user to receive notifications from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `topic`: The topic name.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous subscription operation.
- **UnsubscribeFromTopicAsync** - Unsubscribes a user from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `topic`: The topic name.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous unsubscription operation.

### PushNotificationPayload

- **Namespace:** `SmartWorkz.Shared.PushNotificationPayload`
- **Summary:** Represents the payload data for a push notification.

### PushNotificationAction

- **Namespace:** `SmartWorkz.Shared.PushNotificationAction`
- **Summary:** Represents an action that can be performed from a push notification.

### PagedList`1

- **Namespace:** `SmartWorkz.Shared.PagedList`1`
- **Summary:** A page of items with metadata.
             Replaces PaginationResponse<T> in StarterKitMVC.Shared.DTOs.
            
             Migration path: PaginationResponse<T> has the same fields under different names.
             PagedList<T>.Create() is a drop-in replacement for PaginationResponse<T>.Create().

#### Methods & Properties

- **Empty** - Create an empty result set (e.g., when no rows match).
- **Map``1** - Project items to a different type without changing pagination metadata.

### PagedQuery

- **Namespace:** `SmartWorkz.Shared.PagedQuery`
- **Summary:** Standard request parameters for any paginated query.
             Replaces the existing PaginationRequest record in StarterKitMVC.Shared.DTOs.
            
             Migration: PaginationRequest is structurally identical — alias it or replace it.

#### Methods & Properties

- **#ctor** - Standard request parameters for any paginated query.
             Replaces the existing PaginationRequest record in StarterKitMVC.Shared.DTOs.
            
             Migration: PaginationRequest is structurally identical — alias it or replace it.
- **Normalize** - Clamp page and pageSize to safe bounds.

### IEntity`1

- **Namespace:** `SmartWorkz.Shared.IEntity`1`
- **Summary:** Marks a class as a domain entity with a typed primary key.

### CircuitBreaker

- **Namespace:** `SmartWorkz.Shared.CircuitBreaker`
- **Summary:** A thread-safe implementation of the circuit breaker pattern for handling failing dependencies gracefully.
            
             The circuit breaker operates in three states:
             - Closed: Normal operation. Requests pass through. Failures are tracked.
             - Open: Failing. All requests are rejected immediately to prevent cascading failures.
             - HalfOpen: Testing recovery. Limited requests are allowed to test if the dependency has recovered.
            
             State transitions:
             - Closed → Open: When ConsecutiveFailures >= FailureThreshold
             - Open → HalfOpen: Automatically when (DateTime.UtcNow - LastFailureTime) >= TimeoutMilliseconds
             - HalfOpen → Closed: When SuccessCount >= SuccessThreshold
             - HalfOpen → Open: When RecordFailure() is called in HalfOpen state
             - Closed → Closed: When RecordSuccess() is called (resets failure counter)

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `options`: The circuit breaker configuration options.
- **RecordSuccess** - Records a successful operation and updates state transitions accordingly.
            In Closed state: resets the failure counter.
            In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
- **RecordFailure** - Records a failed operation and updates state transitions accordingly.
            In Closed state: increments failure counter and transitions to Open if threshold is met.
            In HalfOpen state: immediately transitions back to Open.
- **Reset** - Resets the circuit breaker to the Closed state, clearing all failure and success counters.

### CircuitBreakerOptions

- **Namespace:** `SmartWorkz.Shared.CircuitBreakerOptions`
- **Summary:** Configuration options for the circuit breaker.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if options are valid; otherwise false.

### CircuitBreakerState

- **Namespace:** `SmartWorkz.Shared.CircuitBreakerState`
- **Summary:** Defines the state of a circuit breaker in the state machine pattern.

### ICircuitBreaker

- **Namespace:** `SmartWorkz.Shared.ICircuitBreaker`
- **Summary:** Defines the contract for a circuit breaker that implements the state machine pattern
            to handle failing dependencies gracefully.

#### Methods & Properties

- **RecordSuccess** - Records a successful operation and updates state transitions accordingly.
            In Closed state: resets the failure counter.
            In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
- **RecordFailure** - Records a failed operation and updates state transitions accordingly.
            In Closed state: increments failure counter and transitions to Open if threshold is met.
            In HalfOpen state: immediately transitions back to Open.
- **Reset** - Resets the circuit breaker to the Closed state, clearing all failure and success counters.

### IRateLimiter

- **Namespace:** `SmartWorkz.Shared.IRateLimiter`
- **Summary:** Defines the contract for a thread-safe rate limiter.

#### Methods & Properties

- **TryAcquireAsync** - Attempts to acquire tokens for a request identified by the given identifier.
  - Parameters:
    - `identifier`: The identifier for rate limiting (e.g., IP address, user ID).
    - `cost`: The number of tokens to acquire (default: 1).
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A result containing true if tokens were acquired successfully; otherwise false.
            On failure, the Error will include retry-after information.
- **GetAvailableTokensAsync** - Gets the number of available tokens for a given identifier.
  - Parameters:
    - `identifier`: The identifier to check.
  - Returns: The number of available tokens.
- **ResetAsync** - Resets the rate limit state for a given identifier.
  - Parameters:
    - `identifier`: The identifier to reset.
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **ClearAsync** - Clears all rate limit state.
  - Parameters:
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.

### RateLimiter

- **Namespace:** `SmartWorkz.Shared.RateLimiter`
- **Summary:** Thread-safe token bucket rate limiter implementation.
            
             This class maintains a per-identifier token bucket that refills at a constant rate.
             Tokens are consumed when requests are made; if insufficient tokens exist, the request is denied.
            
             Thread-safe operations use ConcurrentDictionary and locks on individual buckets to ensure
             consistent state without global locking bottlenecks.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the RateLimiter class.
  - Parameters:
    - `options`: Configuration options for the rate limiter.
- **TryAcquireAsync** - Attempts to acquire tokens for a request identified by the given identifier.
  - Parameters:
    - `identifier`: The identifier for rate limiting (e.g., IP address, user ID).
    - `cost`: The number of tokens to acquire (default: 1).
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A result containing true if tokens were acquired successfully; otherwise false.
            On failure, the Error will include retry-after information.
- **GetAvailableTokensAsync** - Gets the number of available tokens for a given identifier.
  - Parameters:
    - `identifier`: The identifier to check.
  - Returns: The number of available tokens.
- **ResetAsync** - Resets the rate limit state for a given identifier.
  - Parameters:
    - `identifier`: The identifier to reset.
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **ClearAsync** - Clears all rate limit state.
  - Parameters:
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **TokenBucket.TryAcquire** - Tries to acquire the specified number of tokens.
- **TokenBucket.GetAvailableTokens** - Gets the current number of available tokens.
- **TokenBucket.GetRetryAfterMilliseconds** - Gets the number of milliseconds to wait before retrying.
- **TokenBucket.RefillTokens** - Refills the token bucket based on elapsed time.

### RateLimiterOptions

- **Namespace:** `SmartWorkz.Shared.RateLimiterOptions`
- **Summary:** Configuration options for the rate limiter.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if options are valid; otherwise false.

### RateLimiterStrategy

- **Namespace:** `SmartWorkz.Shared.RateLimiterStrategy`
- **Summary:** Specifies the strategy used by the rate limiter to control request flow.

### ApiError

- **Namespace:** `SmartWorkz.Shared.ApiError`
- **Summary:** Structured error representation for API responses.
            Provides code, message, and optional field-level error details.

#### Methods & Properties

- **FromError** - Create from core Error type.
- **FromValidationErrors** - Create from validation errors.
- **FromException** - Create from exception.

### ApiResponse

- **Namespace:** `SmartWorkz.Shared.ApiResponse`
- **Summary:** Generic API response envelope that wraps result data with metadata.
            Non-generic convenience version for non-data responses.

#### Methods & Properties

- **Ok** - Success response without data.
- **Fail** - Failure response with error details.
- **FromResult** - Create from core Result pattern.

### ApiResponse`1

- **Namespace:** `SmartWorkz.Shared.ApiResponse`1`
- **Summary:** Typed API response envelope with data payload.
            Includes optional pagination metadata for list responses.

#### Methods & Properties

- **Ok** - Success response with data.
- **OkPaginated** - Success response with paginated data.
- **Fail** - Failure response with error.

### ProblemDetailsResponse

- **Namespace:** `SmartWorkz.Shared.ProblemDetailsResponse`
- **Summary:** Implements RFC 7807 Problem Details for HTTP APIs standard response format.
            Provides a standardized way to represent error details in API responses.

#### Methods & Properties

- **ValidationError** - Factory method for 400 Bad Request error with validation details.
- **Unauthorized** - Factory method for 401 Unauthorized error.
- **Forbidden** - Factory method for 403 Forbidden error.
- **NotFound** - Factory method for 404 Not Found error.
- **Conflict** - Factory method for 409 Conflict error.
- **InternalServerError** - Factory method for 500 Internal Server Error.
- **Custom** - Factory method for custom problem details.

### Error

- **Namespace:** `SmartWorkz.Shared.Error`
- **Summary:** Represents a structured error with a machine-readable code and human-readable message.
            
             This is the canonical Error type. It replaces:
             - SmartWorkz.StarterKitMVC.Shared.Primitives.Error (record struct)
             - The ad-hoc string errors in Models.Result
            
             Code examples: "USER_NOT_FOUND", "VALIDATION.EMAIL_REQUIRED", "AUTH.INVALID_CREDENTIALS"
             MessageKey maps to localization resource keys for UI display.

### Result

- **Namespace:** `SmartWorkz.Shared.Result`
- **Summary:** Represents the outcome of an operation that does not return a value.
            
             This unifies:
             - SmartWorkz.StarterKitMVC.Shared.Models.Result (Succeeded + MessageKey + Errors[])
             - SmartWorkz.StarterKitMVC.Shared.Primitives.Result (IsSuccess + Error struct)
            
             Design choice — class over struct:
             1. Result<T> inherits from Result to reuse Succeeded/Errors without duplication.
                Structs cannot use inheritance this way.
             2. Services return Result from interface methods — class semantics (null check) are
                simpler than boxing/unboxing structs across interface boundaries.
             3. Errors[] supports field-level validation messages that ModelState.AddErrors() consumes.
                A single Error struct cannot carry multiple field errors.
            
             The Primitives.Result struct in StarterKitMVC.Shared remains valid for pure functions
             where you want zero-allocation returns. This class is for service layer contracts.

#### Methods & Properties

- **Fail** - Failure with a localization message key and optional field-level error strings.
- **Fail** - Failure from a structured Error (bridges the Primitives.Error pattern).
- **Ok``1** - Factory for a typed result. Use in services that return data.

### Result`1

- **Namespace:** `SmartWorkz.Shared.Result`1`
- **Summary:** Result with a typed payload. Data is only valid when Succeeded = true.
            
             Usage:
               Result<UserDto> result = await _userService.GetByIdAsync(id);
               if (!result.Succeeded) return RedirectToPage("Error");
               var user = result.Data!;

### ResultExtensions

- **Namespace:** `SmartWorkz.Shared.ResultExtensions`
- **Summary:** Functional helpers for chaining Result operations.
            Keeps service code flat — avoids nested if (!result.Succeeded) blocks.

#### Methods & Properties

- **Map``2** - Transform the Data value if the result succeeded.
- **BindAsync``2** - Chain a second operation that also returns Result.
- **OnSuccess``1** - Execute a side-effect action on success, then return the original result.
- **OnFailure``1** - Execute a side-effect action on failure, then return the original result.

### ISagaDefinition`1

- **Namespace:** `SmartWorkz.Shared.ISagaDefinition`1`
- **Summary:** Defines the blueprint for a saga orchestration.
            A saga is a pattern for managing distributed transactions and long-running processes
            by coordinating multiple steps with built-in compensation mechanisms.

#### Methods & Properties

- **DefineStep``1** - Defines a step in the saga that will be executed when a specific event type is received.
            Steps are executed sequentially in the order they were defined.
  - Parameters:
    - `handler`: The async handler function that processes the event and updates the saga state.
            Returns a StepResult indicating success or failure.
- **OnFailure** - Defines the failure handler that will be called if any step fails.
            Used for compensation logic and saga-level error handling.
  - Parameters:
    - `compensationHandler`: The async handler that receives the current saga state and the exception that occurred.
            Responsible for compensation/rollback logic.
- **BuildAsync** - Builds and returns the saga definition for execution.
            Can be used for async initialization or validation.
  - Returns: A task that completes with the configured saga definition.
- **GetSteps** - Gets the list of saga steps in execution order.
  - Returns: A read-only list of saga step handlers.
- **GetFailureHandler** - Gets the failure compensation handler if defined.
  - Returns: The failure handler function, or null if not defined.

### SagaOrchestrator

- **Namespace:** `SmartWorkz.Shared.SagaOrchestrator`
- **Summary:** Orchestrates the execution of sagas, managing step sequencing, error handling,
            and compensation/rollback logic for complex distributed processes.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the SagaOrchestrator class.
  - Parameters:
    - `logger`: Logger for saga execution tracking and debugging.
- **ExecuteSagaAsync``1** - Executes a saga definition with the provided initial state and triggering event.
            Manages step execution, error handling, and compensation logic.
  - Parameters:
    - `sagaDefinition`: The saga definition blueprint to execute.
    - `initialState`: The initial saga state.
    - `event`: The domain event triggering the saga.
    - `cancellationToken`: Optional cancellation token.
  - Returns: A task representing the saga execution.
- **ExecuteSagaStepsAsync``1** - Executes saga steps by using reflection to access internal step definitions.
- **CompensateExecutedStepsAsync``1** - Executes compensation handlers for all executed steps in reverse order.
            Uses stored compensation handlers to avoid re-executing steps.
- **ExecuteFailureHandlerAsync``1** - Executes the saga-level failure handler if one is defined.

### SagaStatus

- **Namespace:** `SmartWorkz.Shared.SagaStatus`
- **Summary:** Represents the status of a saga execution.

### SagaState

- **Namespace:** `SmartWorkz.Shared.SagaState`
- **Summary:** Base class for saga state objects.
            Provides common tracking properties for saga execution flow.

### StepResult

- **Namespace:** `SmartWorkz.Shared.StepResult`
- **Summary:** Represents the result of executing a single saga step.
            Provides success/failure status and optional compensation logic for rollback.

#### Methods & Properties

- **Success** - Creates a successful step result.
  - Returns: A StepResult indicating success.
- **Failure** - Creates a failed step result with an optional compensation handler.
  - Parameters:
    - `failureReason`: The reason for the step failure.
    - `compensationHandler`: Optional handler to compensate/rollback this step if a later step fails.
  - Returns: A StepResult indicating failure.
- **FromException** - Creates a failed step result for an exception with optional compensation.
  - Parameters:
    - `exception`: The exception that caused the failure.
    - `compensationHandler`: Optional compensation handler.
  - Returns: A StepResult indicating failure.

### CryptHelper

- **Namespace:** `SmartWorkz.Shared.CryptHelper`
- **Summary:** Provides AES-256-CBC encryption and decryption utilities with secure key and IV generation.
            
             All operations support both string and byte array inputs/outputs.
             Keys are normalized to 32 bytes (256 bits) via padding/trimming as needed.
             IVs are auto-generated if not provided and embedded in the ciphertext (IV:Ciphertext format).

#### Methods & Properties

- **EncryptString** - Encrypts plaintext using AES-256-CBC with a Base64-encoded output.
  - Parameters:
    - `plaintext`: The plaintext to encrypt.
    - `key`: The encryption key (will be normalized to 32 bytes).
    - `iv`: Optional IV; auto-generated if null.
  - Returns: A Result containing Base64-encoded ciphertext in "IV:Ciphertext" format or an error.
- **EncryptBytes** - Encrypts byte data using AES-256-CBC.
  - Parameters:
    - `plaintext`: The plaintext bytes to encrypt.
    - `key`: The encryption key (will be normalized to 32 bytes).
    - `iv`: Optional IV; auto-generated if null.
  - Returns: A Result containing encrypted bytes with embedded IV (IV || Ciphertext) or an error.
- **DecryptString** - Decrypts Base64-encoded ciphertext using AES-256-CBC.
  - Parameters:
    - `ciphertext`: The Base64-encoded ciphertext in "IV:Ciphertext" format.
    - `key`: The decryption key (will be normalized to 32 bytes).
  - Returns: A Result containing the decrypted plaintext or an error.
- **DecryptBytes** - Decrypts byte data using AES-256-CBC.
  - Parameters:
    - `ciphertext`: The encrypted bytes with embedded IV (IV || Ciphertext).
    - `key`: The decryption key (will be normalized to 32 bytes).
  - Returns: A Result containing decrypted bytes or an error.
- **GenerateKey** - Generates a random cryptographic key of the specified size.
  - Parameters:
    - `keySize`: The key size in bytes (default 32 for AES-256). Must be 16, 24, or 32.
  - Returns: A Result containing Base64-encoded random key or an error.
- **GenerateIv** - Generates a random cryptographic IV (Initialization Vector).
  - Returns: A Result containing Base64-encoded random IV or an error.
- **GenerateRandomBytes** - Generates cryptographically secure random bytes.
- **NormalizeKey** - Normalizes a key to exactly 32 bytes (256 bits).
            If the key is shorter, it's padded with zeros. If longer, it's trimmed.

### CryptOptions

- **Namespace:** `SmartWorkz.Shared.CryptOptions`
- **Summary:** Configuration options for AES cryptographic operations.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if valid, false otherwise.

### HashHelper

- **Namespace:** `SmartWorkz.Shared.HashHelper`
- **Summary:** Provides utilities for cryptographic hash operations (SHA256 and MD5).

#### Methods & Properties

- **Sha256** - Computes the SHA256 hash of a string and returns it as a hexadecimal string.
- **Sha256Bytes** - Computes the SHA256 hash of a byte array and returns the hash as a byte array.
- **Md5** - Computes the MD5 hash of a string and returns it as a hexadecimal string.
            Note: MD5 is cryptographically broken; use SHA256 for security-critical applications.
- **VerifyHash** - Verifies that a text matches its SHA256 hash.

### HmacAlgorithm

- **Namespace:** `SmartWorkz.Shared.HmacAlgorithm`
- **Summary:** Specifies the HMAC algorithm to use for message signing and verification.

### HmacHelper

- **Namespace:** `SmartWorkz.Shared.HmacHelper`
- **Summary:** Provides HMAC-SHA256/SHA512 message signing and verification for API requests and webhook verification.
            Implements constant-time comparison to prevent timing attacks.

#### Methods & Properties

- **Sign** - Signs a message using HMAC with the specified algorithm and returns a Base64-encoded hex digest.
  - Parameters:
    - `message`: The message to sign.
    - `secret`: The secret key for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing the Base64-encoded signature or an error.
- **SignBytes** - Signs a message using HMAC with the specified algorithm and returns the raw byte digest.
  - Parameters:
    - `message`: The message bytes to sign.
    - `secret`: The secret key for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing the byte signature or an error.
- **Verify** - Verifies a message signature using HMAC with constant-time comparison to prevent timing attacks.
  - Parameters:
    - `message`: The original message that was signed.
    - `signature`: The Base64-encoded signature to verify.
    - `secret`: The secret key used for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing true if the signature is valid, false if not, or an error.
- **VerifyBytes** - Verifies a message signature using HMAC with raw byte inputs and constant-time comparison.
  - Parameters:
    - `message`: The original message bytes that were signed.
    - `signature`: The byte signature to verify.
    - `secret`: The secret key used for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing true if the signature is valid, false if not, or an error.
- **SignBytes** - Internal method to compute HMAC signature from raw bytes.
- **CreateHmac** - Creates the appropriate HMAC instance based on the algorithm.

### InputSanitizer

- **Namespace:** `SmartWorkz.Shared.InputSanitizer`
- **Summary:** Input sanitization to prevent XSS, SQL injection, and path traversal attacks.

#### Methods & Properties

- **SanitizeHtml** - Sanitize HTML by removing dangerous tags and attributes.
- **EscapeHtml** - Escape HTML special characters to prevent XSS.
- **SanitizeSql** - Sanitize string to prevent SQL injection (basic, not a replacement for parameterized queries).
- **SanitizeFilePath** - Sanitize file path to prevent directory traversal attacks.
- **SanitizeUrl** - Sanitize and validate URL.
- **EscapeJson** - Escape string for safe JSON inclusion.
- **IsValidEmail** - Validate email format (basic check, server-side SMTP validation recommended).
- **RemoveControlCharacters** - Remove null bytes and control characters.

### JwtSettings

- **Namespace:** `SmartWorkz.Shared.JwtSettings`
- **Summary:** Settings for JWT token generation and validation.

#### Methods & Properties

- **Validate** - Validate settings: Secret >= 32 chars, other fields non-empty.

### JwtClaims

- **Namespace:** `SmartWorkz.Shared.JwtClaims`
- **Summary:** JWT claims that can be included in a token.

#### Methods & Properties

- **GetClaimValue** - Get claim value by type (supports standard claims + custom).

### JwtTokenValidationResult

- **Namespace:** `SmartWorkz.Shared.JwtTokenValidationResult`
- **Summary:** Result of JWT token validation.

### JwtHelper

- **Namespace:** `SmartWorkz.Shared.JwtHelper`
- **Summary:** Provides JWT token generation, validation, and refresh functionality.

#### Methods & Properties

- **GenerateTokenInternal** - Internal token generation logic shared by GenerateToken and GenerateRefreshToken.
  - Parameters:
    - `claims`: The claims to include in the token.
    - `settings`: The JWT settings for signing and configuration.
    - `isRefreshToken`: If true, uses RefreshTokenExpiryDays; otherwise uses ExpiryMinutes.
  - Returns: A Result containing the signed token or an error.
- **GenerateToken** - Generates a JWT access token with the specified claims and settings.
  - Parameters:
    - `claims`: The claims to include in the token.
    - `settings`: The JWT settings for signing and configuration.
  - Returns: A Result containing the signed token or an error.
- **ValidateToken** - Validates a JWT token and extracts claims if valid.
  - Parameters:
    - `token`: The token to validate.
    - `settings`: The JWT settings for validation.
  - Returns: A Result containing the validation result.
- **RefreshToken** - Refreshes an access token using a refresh token.
  - Parameters:
    - `refreshToken`: The refresh token.
    - `settings`: The JWT settings.
  - Returns: A Result containing the new access token or an error.
- **GenerateRefreshToken** - Generates a refresh token with extended expiry.
  - Parameters:
    - `claims`: The claims to include in the refresh token.
    - `settings`: The JWT settings.
  - Returns: A Result containing the refresh token or an error.
- **ToBase64Url** - Encodes bytes to Base64Url format (no padding, + → -, / → _).
- **FromBase64Url** - Decodes Base64Url format to bytes.

### PasswordHelper

- **Namespace:** `SmartWorkz.Shared.PasswordHelper`
- **Summary:** Provides secure password generation and validation using cryptographically secure random number generation.

#### Methods & Properties

- **GeneratePassword** - Generates a cryptographically secure random password.
  - Parameters:
    - `length`: Length of the password (8-128, default 12).
    - `includeSpecialChars`: Whether to include special characters.
  - Returns: A Result containing the generated password or an error.
- **ValidateStrength** - Validates the strength of a password against a policy.
  - Parameters:
    - `password`: The password to validate.
    - `policy`: The policy to validate against (uses default if null).
  - Returns: A Result containing the validation result.
- **GetRandomChar** - Gets a random character from the specified character set using cryptographic randomness.
- **Shuffle** - Performs Fisher-Yates shuffle on the character array.
- **CheckPasswordLength** - Checks if password meets minimum length requirement.
- **CheckUppercase** - Checks if password contains at least one uppercase letter.
- **CheckLowercase** - Checks if password contains at least one lowercase letter.
- **CheckNumbers** - Checks if password contains at least one digit.
- **CheckSpecialChars** - Checks if password contains at least one special character.

### PasswordPolicy

- **Namespace:** `SmartWorkz.Shared.PasswordPolicy`
- **Summary:** Policy for password validation requirements.

#### Methods & Properties

- **Validate** - Validates the policy invariants.

### PasswordValidationResult

- **Namespace:** `SmartWorkz.Shared.PasswordValidationResult`
- **Summary:** Result of password validation against a policy.

### ITemplateEngine

- **Namespace:** `SmartWorkz.Shared.ITemplateEngine`
- **Summary:** Defines operations for rendering templates with placeholder substitution.

#### Methods & Properties

- **Render** - Renders a template string by replacing placeholders with values from a dictionary.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `values`: Dictionary of key-value pairs to substitute into the template.
  - Returns: The rendered content with placeholders replaced. Unmatched placeholders remain unchanged.
- **Render** - Renders a template string by extracting properties from an object model.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `model`: The model object whose public properties are used for placeholder substitution.
  - Returns: The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.
- **RenderFileAsync** - Asynchronously renders a template file by replacing placeholders with values from a dictionary.
  - Parameters:
    - `filePath`: The path to the template file.
    - `values`: Dictionary of key-value pairs to substitute into the template.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **RenderFileAsync** - Asynchronously renders a template file by extracting properties from an object model.
  - Parameters:
    - `filePath`: The path to the template file.
    - `model`: The model object whose public properties are used for placeholder substitution.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **LoadDirectoryAsync** - Asynchronously loads all template files from a directory into a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.
- **RenderDirectoryAsync** - Asynchronously loads and renders all template files from a directory using values from a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `values`: Dictionary of key-value pairs to substitute into each template.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.

### TemplateEngine

- **Namespace:** `SmartWorkz.Shared.TemplateEngine`
- **Summary:** Provides template rendering services with support for placeholder substitution.

#### Methods & Properties

- **PlaceholderRegex** - 
- **Render** - Renders a template string by replacing placeholders with values from a dictionary.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `values`: Dictionary of key-value pairs to substitute into the template.
  - Returns: The rendered content with placeholders replaced. Unmatched placeholders and null/empty content remain unchanged.
- **Render** - Renders a template string by extracting properties from an object model.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `model`: The model object whose public properties are used for placeholder substitution.
  - Returns: The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.
- **RenderFileAsync** - Asynchronously renders a template file by replacing placeholders with values from a dictionary.
  - Parameters:
    - `filePath`: The path to the template file.
    - `values`: Dictionary of key-value pairs to substitute into the template.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **RenderFileAsync** - Asynchronously renders a template file by extracting properties from an object model.
  - Parameters:
    - `filePath`: The path to the template file.
    - `model`: The model object whose public properties are used for placeholder substitution.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **LoadDirectoryAsync** - Asynchronously loads all template files from a directory into a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.
- **RenderDirectoryAsync** - Asynchronously loads and renders all template files from a directory using values from a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `values`: Dictionary of key-value pairs to substitute into each template.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.
- **ReflectModel** - Reflects over a model object and builds a case-insensitive dictionary of public properties
            mapped to their string values. Uses cached property metadata for performance.
- **ValidateFilePath** - Validates a file path to prevent directory traversal attacks.
  - Parameters:
    - `filePath`: The file path to validate.
  - Returns: A result indicating if the path is valid and safe.

### CompressHelper

- **Namespace:** `SmartWorkz.Shared.CompressHelper`
- **Summary:** Provides utilities for GZip compression and decompression.

#### Methods & Properties

- **CompressString** - Compresses a string using GZip compression.
- **DecompressString** - Decompresses a GZip-compressed byte array back to a string.
- **CompressBytes** - Compresses a byte array using GZip compression.
- **DecompressBytes** - Decompresses a GZip-compressed byte array.

### DateHelper

- **Namespace:** `SmartWorkz.Shared.DateHelper`
- **Summary:** Provides utilities for date and time operations.

#### Methods & Properties

- **GetAge** - Calculates the age in years from a birth date to today.
- **GetRelativeTime** - Returns a human-readable relative time string (e.g., "2 days ago", "in 3 hours").
- **StartOfDay** - Returns the start of the day (00:00:00) for the given date.
- **EndOfDay** - Returns the end of the day (23:59:59.999) for the given date.
- **IsWeekend** - Determines if the given date falls on a weekend (Saturday or Sunday).
- **GetDayOfWeekName** - Returns the name of the day of week (e.g., "Monday", "Tuesday").
- **DaysBetween** - Calculates the number of days between two dates (inclusive of the from date, exclusive of the to date).

### EnumHelper

- **Namespace:** `SmartWorkz.Shared.EnumHelper`
- **Summary:** Provides utilities for enum operations including reflection and description retrieval.

#### Methods & Properties

- **GetDescription** - Gets the description of an enum value from its [Description] attribute.
            Falls back to the enum name if no description is found.
- **GetValue``1** - Attempts to get an enum value by its name.
- **GetAllValues``1** - Returns all values of the specified enum type as a list.
- **GetName** - Gets the name of an enum value.

### MathHelper

- **Namespace:** `SmartWorkz.Shared.MathHelper`
- **Summary:** Provides utilities for common math operations.

#### Methods & Properties

- **Percentage** - Calculates the percentage of a value.
            Example: Percentage(100, 20) returns 20 (20% of 100).
- **PercentageChange** - Calculates the percentage change from oldValue to newValue.
            Positive result indicates increase, negative indicates decrease.
- **RoundTo** - Rounds a decimal value to the specified number of decimal places.
- **Clamp``1** - Clamps a value within a specified range [min, max].
- **Average** - Calculates the average of the provided decimal values.

### SlugHelper

- **Namespace:** `SmartWorkz.Shared.SlugHelper`
- **Summary:** Helper for generating URL-friendly slugs from text input.

#### Methods & Properties

- **GenerateSlug** - Generates a URL-friendly slug from the given text with optional configuration.
  - Parameters:
    - `text`: The input text to convert to a slug.
    - `options`: Configuration options. If null, default options are used.
  - Returns: A Result containing the generated slug or an error.
- **ToSlug** - Generates a URL-friendly slug from the given text using default options.
            Convenience method equivalent to GenerateSlug(text, null).
  - Parameters:
    - `text`: The input text to convert to a slug.
  - Returns: A Result containing the generated slug or an error.
- **RemoveAccents** - Removes accented characters from text by decomposing them and filtering out combining marks.
            For example: "café" → "cafe", "naïve" → "naive", "Señor" → "Senor".
  - Parameters:
    - `input`: The input text potentially containing accented characters.
  - Returns: The text with accented characters converted to their base forms.
- **ReplaceSpecialCharacters** - Replaces special characters and spaces with the specified separator.
            Keeps only alphanumeric characters and the separator.
  - Parameters:
    - `input`: The input text.
    - `separator`: The separator to use for special characters and spaces.
  - Returns: The text with special characters replaced by the separator.

### SlugOptions

- **Namespace:** `SmartWorkz.Shared.SlugOptions`
- **Summary:** Options for configuring slug generation behavior in .

### TextHelper

- **Namespace:** `SmartWorkz.Shared.TextHelper`
- **Summary:** Sealed class providing advanced text processing and formatting utilities.
            All methods return Result<string> for consistent error handling.

#### Methods & Properties

- **Truncate** - Truncates text to a maximum length and appends a suffix (default "...").
  - Parameters:
    - `text`: The input text to truncate.
    - `maxLength`: The maximum length including the suffix.
    - `suffix`: The suffix to append when truncating. Defaults to "...".
  - Returns: A Result containing the truncated text or an error.
- **Capitalize** - Capitalizes the first letter of the text while preserving the rest.
  - Parameters:
    - `text`: The input text to capitalize.
  - Returns: A Result containing the capitalized text or an error.
- **Decapitalize** - Decapitalizes the first letter of the text while preserving the rest.
  - Parameters:
    - `text`: The input text to decapitalize.
  - Returns: A Result containing the decapitalized text or an error.
- **StripHtml** - Removes HTML tags from the input string using regex.
  - Parameters:
    - `html`: The HTML string to process.
  - Returns: A Result containing the plain text with HTML tags removed or an error.
- **Pluralize** - Pluralizes a word based on count using a simple heuristic.
            If count == 1, returns singular form. Otherwise appends 's'.
  - Parameters:
    - `singular`: The singular form of the word.
    - `count`: The count to determine plural form.
  - Returns: A Result containing the appropriately pluralized word or an error.
- **TitleCase** - Converts text to title case by capitalizing the first letter of each word.
  - Parameters:
    - `text`: The input text to convert.
  - Returns: A Result containing the title-cased text or an error.
- **Reverse** - Reverses the input string.
  - Parameters:
    - `text`: The input text to reverse.
  - Returns: A Result containing the reversed text or an error.
- **RemoveWhitespace** - Removes all whitespace characters from the input string.
  - Parameters:
    - `text`: The input text to process.
  - Returns: A Result containing the text with all whitespace removed or an error.
- **WordWrap** - Wraps text at a specified line length while preserving word boundaries.
  - Parameters:
    - `text`: The input text to wrap.
    - `lineLength`: The maximum length of each line.
    - `newline`: The newline character(s) to use. Defaults to "\n".
  - Returns: A Result containing the word-wrapped text or an error.
- **Repeat** - Repeats the input string the specified number of times.
  - Parameters:
    - `text`: The input text to repeat.
    - `count`: The number of times to repeat the text.
  - Returns: A Result containing the repeated text or an error.

### CompositeValidator`1

- **Namespace:** `SmartWorkz.Shared.CompositeValidator`1`
- **Summary:** Combines multiple validators into a single validator.
            Useful for composing validators from different sources.

### IValidationRule`2

- **Namespace:** `SmartWorkz.Shared.IValidationRule`2`
- **Summary:** Single validation rule for a property.

#### Methods & Properties

- **ValidateAsync** - Validate property and return results.

### ValidationRule`2

- **Namespace:** `SmartWorkz.Shared.ValidationRule`2`
- **Summary:** Base implementation for custom validation rules.

### ValidationRules

- **Namespace:** `SmartWorkz.Shared.ValidationRules`
- **Summary:** Pre-built validation rules for common scenarios.

### ValidatorBuilder`1

- **Namespace:** `SmartWorkz.Shared.ValidatorBuilder`1`
- **Summary:** Fluent validator builder for defining validation rules.
            Provides an alternative to ValidatorBase for more concise validator definitions.

#### Methods & Properties

- **RuleFor``1** - Add a rule for a property using fluent API.
- **ValidateAsync** - Validate instance against all rules.

### IWebhookRegistry

- **Namespace:** `SmartWorkz.Shared.IWebhookRegistry`
- **Summary:** Abstraction for managing webhook subscriptions and registrations.
            Supports CRUD operations and subscription queries.

#### Methods & Properties

- **RegisterAsync** - Register a new webhook subscription.
  - Parameters:
    - `url`: The webhook endpoint URL.
    - `events`: Array of event names to subscribe to.
    - `secret`: Optional HMAC-SHA256 secret for signature verification.
    - `cancellationToken`: Cancellation token.
  - Returns: The ID of the newly registered subscription.
- **UnregisterAsync** - Unregister and remove a webhook subscription.
  - Parameters:
    - `subscriptionId`: The subscription ID to unregister.
    - `cancellationToken`: Cancellation token.
- **GetSubscriptionsForEventAsync** - Get all active subscriptions for a specific event.
  - Parameters:
    - `eventName`: The event name to filter by.
    - `cancellationToken`: Cancellation token.
  - Returns: Collection of subscriptions interested in this event.
- **GetActiveSubscriptionsAsync** - Get all currently active subscriptions.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: Collection of all active subscriptions.
- **UpdateSubscriptionStatusAsync** - Update the status and failure tracking of a subscription.
  - Parameters:
    - `subscriptionId`: The subscription ID to update.
    - `isActive`: Whether the subscription should remain active.
    - `failureCount`: Number of consecutive failures (null to leave unchanged).
    - `failureReason`: Reason for failure (null to clear).
    - `cancellationToken`: Cancellation token.

### SqlWebhookRegistry

- **Namespace:** `SmartWorkz.Shared.SqlWebhookRegistry`
- **Summary:** SQL Server implementation of IWebhookRegistry.
            Persists webhook subscriptions to the database with support for querying and status updates.

### WebhookDeliveryService

- **Namespace:** `SmartWorkz.Shared.WebhookDeliveryService`
- **Summary:** Service for publishing domain events to registered webhook endpoints.
            Implements exponential backoff retry logic, HMAC signature verification, and failure tracking.

#### Methods & Properties

- **PublishEventAsync** - Publish an event to all subscribed webhook endpoints.
  - Parameters:
    - `eventName`: The name of the event being published.
    - `payload`: The event payload to send.
    - `cancellationToken`: Cancellation token.
- **DeliverAsync** - Deliver an event to a single webhook endpoint with exponential backoff retry logic.
- **GenerateSignature** - Generate HMAC-SHA256 signature for webhook payload verification.

### AuditEntry

- **Namespace:** `SmartWorkz.Shared.AuditEntry`
- **Summary:** Immutable audit log entry for tracking entity changes and domain events.
            Records who did what, when, where, and why for compliance and debugging.

### AuditEventSubscriber

- **Namespace:** `SmartWorkz.Shared.AuditEventSubscriber`
- **Summary:** Subscribes to domain events and records them in the audit trail.
            Enables automatic audit capture without requiring explicit audit calls in business logic.

#### Methods & Properties

- **OnEventPublishedAsync** - Record a domain event in the audit trail.
  - Parameters:
    - `evt`: The domain event to record.
    - `userId`: User ID who triggered the event (optional for system events).
    - `ipAddress`: IP address of the request originator (optional).
    - `cancellationToken`: Cancellation token.

### AuditStartupExtensions

- **Namespace:** `SmartWorkz.Shared.AuditStartupExtensions`
- **Summary:** Dependency injection and schema setup for audit trail functionality.

#### Methods & Properties

- **AddAuditTrail** - Register IAuditTrail with SQL Server implementation.
- **CreateAuditTrailSchema** - Create the AuditTrail table and indexes if they don't exist.
            Call this during application startup or migration.

### IAuditTrail

- **Namespace:** `SmartWorkz.Shared.IAuditTrail`
- **Summary:** Service for recording and querying immutable audit entries.
            Abstracts the persistence mechanism for audit trails.

#### Methods & Properties

- **RecordAsync** - Record an audit entry (immutable append-only).
  - Parameters:
    - `entry`: The audit entry to record.
    - `cancellationToken`: Cancellation token.
- **GetEntriesAsync** - Get all audit entries for a specific entity instance.
  - Parameters:
    - `entityType`: Type of entity (e.g., "Order").
    - `entityId`: Entity instance ID.
    - `cancellationToken`: Cancellation token.
- **GetEntriesByActionAsync** - Get audit entries by action type (Created, Updated, Deleted, etc.).
  - Parameters:
    - `action`: The action to filter by.
    - `since`: Optional: only entries after this date.
    - `cancellationToken`: Cancellation token.
- **GetEntriesByUserAsync** - Get audit entries for a specific user.
  - Parameters:
    - `userId`: User ID who performed actions.
    - `since`: Optional: only entries after this date.
    - `cancellationToken`: Cancellation token.
- **SearchAsync** - Search audit trail with multiple filter criteria.
            All criteria are AND'd together (null criteria are ignored).
  - Parameters:
    - `entityType`: Optional entity type filter.
    - `action`: Optional action filter.
    - `userId`: Optional user ID filter.
    - `since`: Optional timestamp filter (inclusive).
    - `cancellationToken`: Cancellation token.

### SqlAuditTrail

- **Namespace:** `SmartWorkz.Shared.SqlAuditTrail`
- **Summary:** SQL Server implementation of IAuditTrail for immutable audit log persistence.
            Appends audit entries to a single table with indexes for efficient querying.

#### Methods & Properties

- **RecordAsync** - 
- **GetEntriesAsync** - 
- **GetEntriesByActionAsync** - 
- **GetEntriesByUserAsync** - 
- **SearchAsync** - 

### ValueConverter`1

- **Namespace:** `SmartWorkz.Shared.ValueConverter`1`
- **Summary:** Abstract base class for type conversion between domain objects and DTOs.
            Enables loose coupling between layers by centralizing conversion logic.

#### Methods & Properties

- **Convert``1** - Convert a single source object to target type.
- **Convert** - Convert a single source object using dynamic target type resolution.
- **ConvertList``1** - Convert a collection of source objects to target type.
- **ConvertList** - Convert a collection using dynamic target type resolution.
- **ConvertFromList``2** - Convert from a collection of different source types.

### CacheEntry`1

- **Namespace:** `SmartWorkz.Shared.CacheEntry`1`
- **Summary:** Represents a cached entry with data, expiration time, and metadata.

#### Methods & Properties

- **#ctor** - Creates a new CacheEntry instance.
- **#ctor** - Creates a new CacheEntry instance with data and expiration.
- **RenewExpiry** - Renews the expiry time based on the cache strategy and TTL.

### CacheEntryWrapper

- **Namespace:** `SmartWorkz.Shared.CacheEntryWrapper`
- **Summary:** Non-generic wrapper for CacheEntry to store in the cache dictionary.

### CacheOptions

- **Namespace:** `SmartWorkz.Shared.CacheOptions`
- **Summary:** Configuration options for cache operations.

#### Methods & Properties

- **#ctor** - Creates a new CacheOptions instance with default values.
- **#ctor** - Creates a new CacheOptions instance with specified TTL.
- **#ctor** - Creates a new CacheOptions instance with specified TTL and cache strategy.
- **#ctor** - Creates a new CacheOptions instance with all parameters.

### CacheStrategy

- **Namespace:** `SmartWorkz.Shared.CacheStrategy`
- **Summary:** Enumeration of cache expiration strategies.

### ICacheService

- **Namespace:** `SmartWorkz.Shared.ICacheService`
- **Summary:** Service for caching with tenant isolation and L1/L2 hybrid support.
            Implementations may use memory cache (L1) and distributed cache (L2).
            All cache operations are tenant-scoped with automatic key prefixing.

#### Methods & Properties

- **GetAsync``1** - Gets a cached value by key with tenant isolation.
  - Parameters:
    - `key`: Cache key (will be tenant-scoped automatically).
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Result containing cached value, or failure if not found or expired.
- **SetAsync``1** - Sets a value in cache with optional TTL for the specified tenant.
  - Parameters:
    - `key`: Cache key (will be tenant-scoped automatically).
    - `value`: Value to cache.
    - `ttlMinutes`: Time to live in minutes. If null, value never expires.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveAsync** - Removes a cached value by key for the specified tenant.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveByPrefixAsync** - Removes all cached values matching a key prefix for the specified tenant.
            Example: RemoveByPrefixAsync("user:") removes all "user:*" entries for that tenant.
  - Parameters:
    - `prefix`: Key prefix to match (may include wildcard suffix like "user:*").
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **ExistsAsync** - Checks if a key exists in cache for the specified tenant.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: True if key exists and is not expired; false otherwise.
- **ClearAsync** - Clears all cache entries for the specified tenant.
            Does not affect entries for other tenants.
  - Parameters:
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.

### ICacheStore

- **Namespace:** `SmartWorkz.Shared.ICacheStore`
- **Summary:** Abstraction for a cache store with support for various operations including TTL and expiration strategies.

#### Methods & Properties

- **GetAsync``1** - Retrieves a value from the cache.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result containing the cached value or null if not found or expired.
- **SetAsync``1** - Sets a value in the cache with optional TTL.
  - Parameters:
    - `key`: The cache key.
    - `value`: The value to cache.
    - `ttlMinutes`: Optional time-to-live in minutes. If null, uses default or no expiration.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **SetAsync``1** - Sets a value in the cache with cache options.
  - Parameters:
    - `key`: The cache key.
    - `value`: The value to cache.
    - `options`: Cache options including TTL, strategy, and sliding expiration.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **RemoveAsync** - Removes a value from the cache.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **RemoveByPrefixAsync** - Removes all values from the cache that match the specified key prefix.
  - Parameters:
    - `keyPrefix`: The prefix to match.
    - `ct`: Cancellation token.
  - Returns: A Result containing the number of entries removed.
- **ExistsAsync** - Checks if a key exists in the cache and is not expired.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result indicating whether the key exists and is valid.
- **ClearAsync** - Clears all entries from the cache.
  - Parameters:
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.

### MemoryCacheService

- **Namespace:** `SmartWorkz.Shared.MemoryCacheService`
- **Summary:** In-memory L1 cache service implementation with thread-safe operations and tenant isolation.
            Suitable for single-process deployments with TTL and expiration support.

#### Methods & Properties

- **BuildKey** - Builds a tenant-scoped cache key.
  - Parameters:
    - `key`: Original cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
  - Returns: Tenant-scoped key in format "{tenantId}:{key}".
- **GetAsync``1** - Gets a cached value by key with tenant isolation. Returns failure if not found or expired.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Result containing cached value, or failure if not found or expired.
- **SetAsync``1** - Sets a cached value with optional TTL expiration and tenant isolation.
  - Parameters:
    - `key`: Cache key.
    - `value`: Value to cache.
    - `ttlMinutes`: Time to live in minutes. If null, no expiration.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveAsync** - Removes a single cache entry with tenant isolation.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveByPrefixAsync** - Removes all cache entries matching a prefix pattern with tenant isolation.
            Example: RemoveByPrefixAsync("user:*", "tenant1") removes "tenant1:user:*" entries.
  - Parameters:
    - `prefix`: Key prefix to match.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **ExistsAsync** - Checks if a key exists in the cache with tenant isolation (ignores expiration check).
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: True if key exists and is not expired; false otherwise.
- **ClearAsync** - Clears all cache entries for the specified tenant (or "default" if not specified).
            Does not clear entries from other tenants.
  - Parameters:
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.

### MemoryCacheStore

- **Namespace:** `SmartWorkz.Shared.MemoryCacheStore`
- **Summary:** In-memory implementation of ICacheStore with TTL support and thread-safe operations.

#### Methods & Properties

- **#ctor** - Creates a new instance of MemoryCacheStore with default options.
- **#ctor** - Creates a new instance of MemoryCacheStore with specified default options.
- **GetAsync``1** - Retrieves a value from the cache.
- **SetAsync``1** - Sets a value in the cache with optional TTL.
- **SetAsync``1** - Sets a value in the cache with cache options.
- **RemoveAsync** - Removes a value from the cache.
- **RemoveByPrefixAsync** - Removes all values from the cache that match the specified key prefix.
- **ExistsAsync** - Checks if a key exists in the cache and is not expired.
- **ClearAsync** - Clears all entries from the cache.
- **CleanupExpiredEntries** - Performs cleanup of expired entries. This is useful for periodic maintenance.

### ISmsService

- **Namespace:** `SmartWorkz.Shared.ISmsService`
- **Summary:** Defines a contract for SMS communication services.
            Provides methods for sending SMS messages to single or multiple recipients.

#### Methods & Properties

- **SendAsync** - Sends an SMS message to a single recipient.
  - Parameters:
    - `phoneNumber`: The recipient phone number (E.164 format recommended)
    - `message`: The SMS message content
    - `cancellationToken`: Cancellation token
  - Returns: Result containing the SMS ID if successful
- **SendBatchAsync** - Sends an SMS message to multiple recipients (batch).
  - Parameters:
    - `phoneNumbers`: Collection of recipient phone numbers
    - `message`: The SMS message content sent to all recipients
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of SMS IDs if successful

### IWebSocketClient

- **Namespace:** `SmartWorkz.Shared.IWebSocketClient`
- **Summary:** Abstraction for WebSocket client operations.

#### Methods & Properties

- **SendAsync** - Sends a message asynchronously through the WebSocket connection.
- **ReceiveAsync** - Receives a message asynchronously from the WebSocket connection.
            Returns null if the connection is closed.
- **CloseAsync** - Closes the WebSocket connection gracefully.

### WebSocketClient

- **Namespace:** `SmartWorkz.Shared.WebSocketClient`
- **Summary:** Sealed implementation of IWebSocketClient using System.Net.WebSockets.

#### Methods & Properties

- **ConnectAsync** - Connects to a WebSocket server at the specified URI.
- **SendAsync** - Sends a message asynchronously through the WebSocket connection.
- **ReceiveAsync** - Receives a message asynchronously from the WebSocket connection.
            Returns null if the connection is closed.
- **CloseAsync** - Closes the WebSocket connection gracefully.

### ConfigurationHelper

- **Namespace:** `SmartWorkz.Shared.ConfigurationHelper`
- **Summary:** Provides typed access to configuration values with automatic type conversion and validation.
            
             This sealed class implements IConfigurationHelper to provide a strongly-typed interface
             for accessing configuration values. It supports automatic type conversion for common types
             including strings, numeric types, booleans, DateTimes, and enums.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ConfigurationHelper class.
  - Parameters:
    - `configuration`: The configuration source to read from.
- **GetRequired``1** - Gets a required configuration value and converts it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: The configuration value converted to type T.
- **GetOptional``1** - Gets an optional configuration value and converts it to the specified type,
            returning a default value if the key is not found or conversion fails.
  - Parameters:
    - `key`: The configuration key to retrieve.
    - `defaultValue`: The value to return if the key is not found or conversion fails.
  - Returns: The configuration value converted to type T, or the default value if retrieval or conversion fails.
- **TryGet``1** - Attempts to get a configuration value and convert it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: A Result<T> containing the converted value on success, or a failure result
            if the key is not found or conversion fails.
- **Exists** - Checks whether a configuration key exists and is not empty.
  - Parameters:
    - `key`: The configuration key to check.
  - Returns: True if the key exists and is not null or whitespace; otherwise false.
- **ConvertValue``1** - Converts a string value to the specified type using invariant culture for numeric types.
  - Parameters:
    - `value`: The string value to convert.
  - Returns: The converted value of type T.

### ConfigurationValidationException

- **Namespace:** `SmartWorkz.Shared.ConfigurationValidationException`
- **Summary:** Exception thrown when configuration validation fails, indicating that a required
            configuration key is missing, empty, or cannot be converted to the requested type.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ConfigurationValidationException class with a specified message.
  - Parameters:
    - `message`: The error message that explains the reason for the exception.
- **#ctor** - Initializes a new instance of the ConfigurationValidationException class with a specified message
            and a reference to the inner exception that is the cause of this exception.
  - Parameters:
    - `message`: The error message that explains the reason for the exception.
    - `innerException`: The exception that is the cause of the current exception.

### IConfigurationHelper

- **Namespace:** `SmartWorkz.Shared.IConfigurationHelper`
- **Summary:** Provides typed access to configuration values with automatic type conversion and validation.

#### Methods & Properties

- **GetRequired``1** - Gets a required configuration value and converts it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: The configuration value converted to type T.
- **GetOptional``1** - Gets an optional configuration value and converts it to the specified type,
            returning a default value if the key is not found or conversion fails.
  - Parameters:
    - `key`: The configuration key to retrieve.
    - `defaultValue`: The value to return if the key is not found or conversion fails.
  - Returns: The configuration value converted to type T, or the default value if retrieval or conversion fails.
- **TryGet``1** - Attempts to get a configuration value and convert it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: A Result<T> containing the converted value on success, or a failure result
            if the key is not found or conversion fails.
- **Exists** - Checks whether a configuration key exists and is not empty.
  - Parameters:
    - `key`: The configuration key to check.
  - Returns: True if the key exists and is not null or whitespace; otherwise false.

### SharedConstants

- **Namespace:** `SmartWorkz.Shared.SharedConstants`
- **Summary:** Shared configuration constants used throughout SmartWorkz.Shared.
            Enables centralized management of default values and limits.

### ICommand

- **Namespace:** `SmartWorkz.Shared.ICommand`
- **Summary:** Marker interface for command objects representing intent to change state.

### ICommandHandler`1

- **Namespace:** `SmartWorkz.Shared.ICommandHandler`1`
- **Summary:** Handler for processing a specific command type.

#### Methods & Properties

- **HandleAsync** - Handles the specified command asynchronously.
  - Parameters:
    - `command`: The command to handle.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task that represents the asynchronous handle operation.

### IQuery`1

- **Namespace:** `SmartWorkz.Shared.IQuery`1`
- **Summary:** Marker interface for query objects that return a result without modifying state.

### IQueryHandler`2

- **Namespace:** `SmartWorkz.Shared.IQueryHandler`2`
- **Summary:** Handler for processing a specific query type and returning results.

#### Methods & Properties

- **HandleAsync** - Handles the specified query asynchronously and returns the result.
  - Parameters:
    - `query`: The query to handle.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task that represents the asynchronous handle operation and contains the query result.

### MediatorCommandDispatcher

- **Namespace:** `SmartWorkz.Shared.MediatorCommandDispatcher`
- **Summary:** Routes commands to their appropriate handlers via dependency injection.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `serviceProvider`: The service provider for resolving handlers.
- **DispatchAsync``1** - Dispatches the specified command to its handler asynchronously.
  - Parameters:
    - `command`: The command to dispatch.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task representing the asynchronous dispatch operation.

### AdoHelper

- **Namespace:** `SmartWorkz.Shared.AdoHelper`
- **Summary:** ADO.NET helper for executing queries and managing connections.
            Works with any IDbProvider implementation.

#### Methods & Properties

- **ExecuteScalarAsync``1** - Execute scalar query (returns single value).
- **ExecuteNonQueryAsync** - Execute non-query command (INSERT, UPDATE, DELETE).
- **ExecuteQueryAsync``1** - Execute query and map results to objects.
- **ExecuteStoredProcedureAsync** - Execute stored procedure.
- **ExecuteQueryMultipleAsync``2** - Execute query returning multiple result sets (2 sets).
- **ExecuteQueryMultipleAsync``3** - Execute query returning multiple result sets (3 sets).
- **ExecuteQueryMultipleAsync``4** - Execute query returning multiple result sets (4 sets).
- **ExecuteTransactionAsync** - Execute transaction with multiple commands.

### CsvHelper

- **Namespace:** `SmartWorkz.Shared.CsvHelper`
- **Summary:** Provides static methods for reading and writing CSV data with support for column mapping,
            quoted fields, embedded delimiters, and newlines.
            RFC 4180 compliant CSV parsing and writing.
            Sealed class to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **CsvWriter``1** - Serializes a collection of objects to CSV format.
  - Parameters:
    - `items`: The collection of objects to serialize.
    - `options`: CSV options. If null, defaults are used.
  - Returns: A Result containing the CSV string if successful; otherwise a failure.
- **CsvReader``1** - Asynchronously deserializes CSV content to a collection of objects.
  - Parameters:
    - `content`: The CSV content string.
    - `mapping`: Column mapping configuration. If null, property names are used as headers.
    - `options`: CSV options. If null, defaults are used.
  - Returns: A Result containing the deserialized list if successful; otherwise a failure.
- **ParseCsvLines** - Parses CSV content into a list of records (each record is a list of field values).
            Handles quoted fields with embedded delimiters and newlines.
- **WriteRecord** - Writes a single CSV record (list of field values) to the string builder.
            Handles quoting of fields with special characters.
- **ConvertValue** - Converts a string value to the specified type.
- **IsNullableType** - Determines if a type is nullable (Nullable<T> or reference type).

### CsvMapping`1

- **Namespace:** `SmartWorkz.Shared.CsvMapping`1`
- **Summary:** Defines column mapping for CSV operations using a fluent API.
            Supports mapping object properties to CSV columns with custom headers.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **Column``1** - Adds a column mapping for the specified property.
  - Parameters:
    - `propertyExpression`: Expression selecting the property to map.
    - `csvHeader`: The CSV column header name.
  - Returns: This instance for method chaining.
- **ExtractPropertyInfo``1** - Extracts property information from a lambda expression.
  - Parameters:
    - `expression`: The lambda expression.
  - Returns: The PropertyInfo if the expression resolves to a property; otherwise null.
- **CreateAuto** - Creates a mapping automatically from all public properties of type T.
            Property names are used as CSV headers.
  - Returns: A new CsvMapping instance with all properties mapped.

### CsvOptions

- **Namespace:** `SmartWorkz.Shared.CsvOptions`
- **Summary:** Configuration options for CSV read/write operations.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **#ctor** - Creates a default instance of CsvOptions.
- **#ctor** - Creates an instance of CsvOptions with specified delimiter and quote character.
  - Parameters:
    - `delimiter`: The field delimiter character.
    - `quoteChar`: The quote character for quoted fields.

### DbProviderFactory

- **Namespace:** `SmartWorkz.Shared.DbProviderFactory`
- **Summary:** Factory for creating database provider instances.
            Resolves provider name from connection string or explicit specification.

#### Methods & Properties

- **Register** - Register custom provider implementation.
- **GetProvider** - Get provider by name.
- **GetProvider** - Get provider by enum value.
- **GetProviderFromConnectionString** - Get provider from connection string (detects provider automatically).

### IDbProvider

- **Namespace:** `SmartWorkz.Shared.IDbProvider`
- **Summary:** Abstraction for database provider-specific operations.
            Supports multiple providers: SQL Server, MySQL, PostgreSQL, SQLite, Oracle.

#### Methods & Properties

- **CreateConnection** - Create connection with connection string.
- **GetParameterPrefix** - Get parameter prefix for this provider (@, :, $).
- **GetLastInsertIdSql** - Get SQL for last inserted ID based on provider.
- **GetPaginationSql** - Get SQL for pagination based on provider.
- **FormatIdentifier** - Format table/column name for provider (e.g., [brackets] for SQL Server).
- **TestConnectionAsync** - Test connection validity.

### DatabaseProvider

- **Namespace:** `SmartWorkz.Shared.DatabaseProvider`
- **Summary:** Enum of supported database providers.

### QueryMultipleHelper

- **Namespace:** `SmartWorkz.Shared.QueryMultipleHelper`
- **Summary:** Helper for executing multiple queries in a single database roundtrip.
            Eliminates N+1 query problems by batching queries together.

#### Methods & Properties

- **QueryMultipleAsync``2** - Execute multiple queries and return results as tuple.
             Single roundtrip, single SQL execution, improved performance.
- **QueryMultipleAsync``3** - Execute 3 queries in single roundtrip.
- **QueryMultipleAsync``4** - Execute 4 queries in single roundtrip.
- **QueryMultipleAsync``5** - Execute 5 queries in single roundtrip.

### QueryResult`1

- **Namespace:** `SmartWorkz.Shared.QueryResult`1`
- **Summary:** Result wrapper for query operations.

### XmlHelper

- **Namespace:** `SmartWorkz.Shared.XmlHelper`
- **Summary:** Provides static methods for XML serialization, deserialization, and XPath queries.
            Uses System.Xml.Linq for manipulation and reflection for property mapping.
            Sealed class to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **Serialize``1** - Serializes an object to an XML string using reflection.
  - Parameters:
    - `obj`: The object to serialize.
    - `options`: XML options. If null, defaults are used.
  - Returns: A Result containing the XML string if successful; otherwise a failure.
- **Deserialize``1** - Deserializes an XML string to an object of type T using reflection.
  - Parameters:
    - `xml`: The XML string to deserialize.
    - `options`: XML options. If null, defaults are used.
  - Returns: A Result containing the deserialized object if successful; otherwise a failure.
- **Query** - Executes an XPath query on an XML string and returns matching element values.
  - Parameters:
    - `xml`: The XML string to query.
    - `xpathExpression`: The XPath expression to execute.
  - Returns: A Result containing a list of matched values if successful; otherwise a failure.
- **SerializeObject** - Recursively serializes an object's properties into an XML element.
- **DeserializeObject** - Recursively deserializes an XML element into an object's properties.
- **IsBasicType** - Determines if a type is a basic/primitive type supported by XML.
- **IsGenericList** - Determines if a type is a generic List<T>.
- **IsComplexType** - Determines if a type is a complex (non-primitive) type.
- **ConvertToXmlValue** - Converts a value to its XML-safe string representation.
- **ConvertFromXmlValue** - Converts an XML string value to the specified type.

### XmlOptions

- **Namespace:** `SmartWorkz.Shared.XmlOptions`
- **Summary:** Configuration options for XML serialization, deserialization, and query operations.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **#ctor** - Creates a default instance of XmlOptions.
- **#ctor** - Creates an instance of XmlOptions with a specified root element name.
  - Parameters:
    - `rootElement`: The name of the root element.
- **#ctor** - Creates an instance of XmlOptions with specified configuration.
  - Parameters:
    - `rootElement`: The name of the root element.
    - `includeXmlDeclaration`: Whether to include the XML declaration.
    - `indent`: Whether to indent the output.

### ApplicationHealth

- **Namespace:** `SmartWorkz.Shared.ApplicationHealth`
- **Summary:** Represents the overall health status of the application.

### CorrelationContext

- **Namespace:** `SmartWorkz.Shared.CorrelationContext`
- **Summary:** A sealed implementation of  for distributed request tracing.

#### Methods & Properties

- **#ctor** - Initializes a new instance with a generated correlation ID.
- **#ctor** - Initializes a new instance with a specified correlation ID.
  - Parameters:
    - `correlationId`: The correlation ID to use
- **#ctor** - Initializes a child context from a parent context.

### CpuUsage

- **Namespace:** `SmartWorkz.Shared.CpuUsage`
- **Summary:** Represents CPU usage information.

### DiagnosticsHelper

- **Namespace:** `SmartWorkz.Shared.DiagnosticsHelper`
- **Summary:** Sealed helper class for system diagnostics and application health monitoring.
            Provides methods to gather system information, CPU/memory/disk usage, and determine application health.

#### Methods & Properties

- **Initialize** - Initializes the application start time (called once at application startup).
- **GetSystemInfo** - Gets comprehensive system information including CPU, memory, disk, and processor count.
  - Returns: A Result containing SystemInfo or error details.
- **GetMemoryUsage** - Gets memory usage statistics for the current process and system.
  - Returns: A Result containing MemoryUsage or error details.
- **GetCpuUsage** - Gets CPU utilization percentage.
  - Returns: A Result containing CpuUsage or error details.
- **GetDiskSpace** - Gets disk space information for a specific drive.
  - Parameters:
    - `drive`: The drive letter (e.g., "C:", "D:"). Defaults to "C:".
  - Returns: A Result containing DiskSpace or error details.
- **GetUptime** - Gets the application uptime since the last Initialize() call or application start.
  - Returns: A Result containing the uptime as a TimeSpan or error details.
- **GetApplicationHealth** - Gets the overall health status of the application based on system metrics.
  - Returns: A Result containing ApplicationHealth or error details.
- **IsHealthy** - Determines if the application is considered healthy based on the provided health status.
  - Parameters:
    - `health`: The ApplicationHealth object to evaluate.
  - Returns: True if the status is Healthy, false otherwise.
- **InitializeCpuCounter** - Initializes the CPU performance counter (called once).
- **GetMemoryUsageInternal** - Internal method to get memory usage statistics.
- **GetCpuUsageInternal** - Internal method to get CPU usage percentage.
- **GetDiskSpaceInternal** - Internal method to get disk space information.

### DiskSpace

- **Namespace:** `SmartWorkz.Shared.DiskSpace`
- **Summary:** Represents disk space information for a drive.

### HealthCheck

- **Namespace:** `SmartWorkz.Shared.HealthCheck`
- **Summary:** Represents a single health check result.

### HealthStatus

- **Namespace:** `SmartWorkz.Shared.HealthStatus`
- **Summary:** Represents the health status of the application.

### ICorrelationContext

- **Namespace:** `SmartWorkz.Shared.ICorrelationContext`
- **Summary:** Defines a correlation context for distributed request tracing across systems.

#### Methods & Properties

- **SetProperty** - Adds or updates a property in the correlation context.
- **TryGetProperty** - Attempts to retrieve a property from the correlation context.
- **CreateChildContext** - Creates a child correlation context for nested operations (for async/distributed flows).

### MemoryUsage

- **Namespace:** `SmartWorkz.Shared.MemoryUsage`
- **Summary:** Represents memory usage information.

### MetricsHelper

- **Namespace:** `SmartWorkz.Shared.MetricsHelper`
- **Summary:** Provides utilities for collecting and tracking performance metrics.

#### Methods & Properties

- **StartTimer** - Starts a timer and returns an IDisposable that logs elapsed time on disposal.
- **TrackExecution``1** - Tracks the execution time and result of a function.
- **MeasureMemory** - Captures memory usage before and after a block of code execution.

### SystemInfo

- **Namespace:** `SmartWorkz.Shared.SystemInfo`
- **Summary:** Represents system information including CPU, memory, and disk details.

### EventStoreSnapshot

- **Namespace:** `SmartWorkz.Shared.EventStoreSnapshot`
- **Summary:** Represents a snapshot of an aggregate's state at a specific version.
            Snapshots optimize event sourcing by reducing the number of events needed for reconstruction.

### IEventStore

- **Namespace:** `SmartWorkz.Shared.IEventStore`
- **Summary:** Abstraction for an immutable event store that persists domain events.
            Enables event sourcing patterns for temporal queries, audit trails, and event replay.

#### Methods & Properties

- **AppendEventsAsync** - Appends events to the event stream for an aggregate.
            Events are immutable and persist as an append-only log.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `events`: The domain events to append
    - `cancellationToken`: Cancellation token
  - Returns: Task representing the asynchronous operation
- **GetEventsAsync** - Retrieves all events for an aggregate in chronological order.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: Collection of domain events for the aggregate, empty if none exist
- **GetEventsSinceAsync** - Retrieves events for an aggregate after a specific version.
            Useful for incremental event replay and event streaming.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `version`: The version after which to retrieve events
    - `cancellationToken`: Cancellation token
  - Returns: Collection of events after the specified version, empty if none exist
- **GetSnapshotAsync** - Retrieves the latest snapshot for an aggregate if one exists.
            Snapshots optimize aggregate reconstruction by storing intermediate state.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: Snapshot data if exists; null if no snapshot is available
- **SaveSnapshotAsync** - Saves a snapshot of aggregate state at a specific version.
            Snapshots reduce the number of events needed to replay an aggregate.
  - Parameters:
    - `snapshot`: The snapshot to save
    - `cancellationToken`: Cancellation token
  - Returns: Task representing the asynchronous operation
- **GetAggregateAsync``1** - Reconstructs an aggregate from its event history.
            Optionally uses snapshots for performance optimization.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: The reconstructed aggregate instance, or null if no events exist

### SqlEventStore

- **Namespace:** `SmartWorkz.Shared.SqlEventStore`
- **Summary:** SQL Server implementation of the event store using Dapper for data access.
            Provides immutable append-only event log with snapshot support for optimization.
            Implements optimistic concurrency control using version numbers.

#### Methods & Properties

- **AppendEventsAsync** - Appends events to the event stream for an aggregate.
- **GetEventsAsync** - Retrieves all events for an aggregate in chronological order.
- **GetEventsSinceAsync** - Retrieves events for an aggregate after a specific version.
- **GetSnapshotAsync** - Retrieves the latest snapshot for an aggregate if one exists.
- **SaveSnapshotAsync** - Saves a snapshot of aggregate state at a specific version.
- **GetAggregateAsync``1** - Reconstructs an aggregate from its event history.
            Optionally uses snapshots for performance optimization.
- **GetCurrentVersionAsync** - Gets the current version number for an aggregate.
- **DeserializeEvent** - Deserializes a stored event record back to IDomainEvent.

### IDomainEvent

- **Namespace:** `SmartWorkz.Shared.IDomainEvent`
- **Summary:** Base interface for domain events in event-driven architecture.
            Provides core event metadata for tracking and publishing.

### IEventPublisher

- **Namespace:** `SmartWorkz.Shared.IEventPublisher`
- **Summary:** Publishes domain events for event-driven architecture.

#### Methods & Properties

- **PublishAsync``1** - Publishes a single domain event.
  - Parameters:
    - `event`: Event to publish.
    - `cancellationToken`: Cancellation token.
- **PublishAsync``1** - Publishes multiple domain events.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token.

### IEventSubscriber

- **Namespace:** `SmartWorkz.Shared.IEventSubscriber`
- **Summary:** Registers event handlers for domain events.

#### Methods & Properties

- **Subscribe``1** - Subscribes a handler to an event type.
            Multiple handlers can subscribe to the same event.
  - Parameters:
    - `handler`: Async handler function. Receives event and cancellation token.

### InMemoryEventPublisher

- **Namespace:** `SmartWorkz.Shared.InMemoryEventPublisher`
- **Summary:** In-memory event publisher that executes all registered handlers sequentially.
            Provides synchronous event delivery with exception handling and result reporting.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the InMemoryEventPublisher with a subscriber.
  - Parameters:
    - `subscriber`: The event subscriber containing registered handlers.
- **PublishAsync``1** - Publishes a single domain event to all registered handlers.
            Handlers are invoked sequentially in registration order.
            If any handler throws an exception, it is caught and a failure Result is returned.
            Other handlers will attempt to execute even if a previous handler fails.
  - Parameters:
    - `event`: The event instance to publish.
    - `cancellationToken`: Cancellation token to cancel handler execution.
  - Returns: A task representing the asynchronous operation.
- **PublishAsync``1** - Publishes multiple domain events to all registered handlers.
            Events are published sequentially in the order provided.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token to cancel handler execution.
  - Returns: A task representing the asynchronous batch operation.

### InMemoryEventSubscriber

- **Namespace:** `SmartWorkz.Shared.InMemoryEventSubscriber`
- **Summary:** In-memory event subscriber that maintains a registry of event handlers.
            Supports multiple handlers per event type using thread-safe concurrent collections.

#### Methods & Properties

- **Subscribe``1** - Subscribes a handler to an event type.
            Multiple handlers can be registered for the same event type and will execute sequentially.
  - Parameters:
    - `handler`: Async handler function that receives event and cancellation token.
- **GetHandlers** - Gets all registered handlers for a given event type.
            Returns an empty list if no handlers are registered for the type.
  - Parameters:
    - `eventType`: The event type to retrieve handlers for.
  - Returns: List of registered handlers (delegates).

### MassTransitEventPublisher

- **Namespace:** `SmartWorkz.Shared.MassTransitEventPublisher`
- **Summary:** Distributed event publisher using MassTransit message bus.
            Supports both single and batch event publishing with async/await patterns.
            Suitable for production environments with message broker backend (RabbitMQ, Azure Service Bus, etc).

#### Methods & Properties

- **#ctor** - Initializes a new instance of MassTransitEventPublisher.
  - Parameters:
    - `publishEndpoint`: MassTransit publish endpoint for message distribution.
    - `logger`: Logger for event publication tracking.
- **PublishAsync``1** - Publishes a single domain event to the message bus asynchronously.
  - Parameters:
    - `event`: Event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous publish operation.
- **PublishAsync``1** - Publishes multiple domain events to the message bus asynchronously.
            Events are published sequentially in the order provided.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous batch publish operation.

### PublisherType

- **Namespace:** `SmartWorkz.Shared.PublisherType`
- **Summary:** Specifies the publisher type for event publishing.

### ServiceCollectionExtensions

- **Namespace:** `SmartWorkz.Shared.ServiceCollectionExtensions`
- **Summary:** Extension methods for IServiceCollection to register Core.Shared services.

#### Methods & Properties

- **AddCoreSharedServices** - Adds Core.Shared services including TemplateEngine for template rendering.
- **AddEventPublishing** - Adds event publishing services to the dependency injection container.
            Supports switching between in-memory and MassTransit publishers based on application needs.
  - Parameters:
    - `services`: The service collection.
    - `publisherType`: The publisher type to use (defaults to InMemory).
  - Returns: The service collection for method chaining.

### DefaultFeatureFlagService

- **Namespace:** `SmartWorkz.Shared.DefaultFeatureFlagService`
- **Summary:** Global (non-tenant) feature flag service with in-memory storage.
            Thread-safe implementation suitable for single-process deployments.
            Use for organization-wide feature toggles; use ITenantFeatureFlags for tenant-scoped flags.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature flag is enabled.
            Returns false for unknown flags (does not throw).
  - Parameters:
    - `flagName`: The name of the feature flag to check.
    - `cancellationToken`: Cancellation token.
  - Returns: True if the flag exists and is enabled; false otherwise.
- **GetEnabledFeaturesAsync** - Gets all enabled feature flags.
            Returns empty list if no flags are enabled.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: A read-only list of enabled feature flag names.
- **EnableFlag** - Enables a feature flag.
            Creates the flag if it doesn't exist.
  - Parameters:
    - `flagName`: The name of the feature flag to enable.
- **DisableFlag** - Disables a feature flag.
            Creates the flag as disabled if it doesn't exist.
  - Parameters:
    - `flagName`: The name of the feature flag to disable.

### IFeatureFlagService

- **Namespace:** `SmartWorkz.Shared.IFeatureFlagService`
- **Summary:** Global feature flag service for cross-tenant feature control.
            Use for organization-wide feature toggles (not tenant-specific).
            For tenant-scoped flags, use ITenantFeatureFlags.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a global feature is enabled.
  - Parameters:
    - `flagName`: Feature flag name (e.g., "NEW_DASHBOARD", "BETA_REPORTING").
    - `cancellationToken`: Cancellation token.
  - Returns: True if feature is enabled globally.
- **GetEnabledFeaturesAsync** - Gets all enabled global features.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: List of enabled feature flag names.

### IFileStorageService

- **Namespace:** `SmartWorkz.Shared.IFileStorageService`
- **Summary:** Interface for file storage operations supporting both local and cloud providers.

#### Methods & Properties

- **UploadAsync** - Uploads a file to storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `content`: The file content stream.
    - `metadata`: The file metadata.
    - `cancellationToken`: The cancellation token.
  - Returns: The full path or URI of the uploaded file.
- **DownloadAsync** - Downloads a file from storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: A stream containing the file content. Caller must dispose using 'using' statement.
- **DeleteAsync** - Deletes a file from storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
- **ExistsAsync** - Checks if a file exists in storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: True if the file exists, false otherwise.
- **GetMetadataAsync** - Gets metadata for a file in storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: FileMetadata if file exists, null otherwise.
- **ListAsync** - Lists files in a directory or container prefix.
  - Parameters:
    - `folderPath`: The relative folder path or blob prefix.
    - `cancellationToken`: The cancellation token.
  - Returns: A read-only collection of FileMetadata for files in the directory/prefix.
- **GenerateTemporaryUrlAsync** - Generates a temporary download URL for a file.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `expiration`: The expiration duration from now.
    - `cancellationToken`: The cancellation token.
  - Returns: A URL that can be used to download the file. For local storage, returns the full file path.

### GridColumn

- **Namespace:** `SmartWorkz.Shared.GridColumn`
- **Summary:** Defines a single column in a grid, including display options, sorting, filtering, and rendering hints.

### GridExportOptions

- **Namespace:** `SmartWorkz.Shared.GridExportOptions`
- **Summary:** Configuration for grid data export (CSV, Excel).

### GridRequest

- **Namespace:** `SmartWorkz.Shared.GridRequest`
- **Summary:** Request parameters for grid data fetching, extending PagedQuery with filtering support.

#### Methods & Properties

- **#ctor** - Request parameters for grid data fetching, extending PagedQuery with filtering support.

### GridResponse`1

- **Namespace:** `SmartWorkz.Shared.GridResponse`1`
- **Summary:** Response from a grid data request, including paged data, column metadata, and filter options.

### IGridDataProvider

- **Namespace:** `SmartWorkz.Shared.IGridDataProvider`
- **Summary:** Abstraction for grid data fetching. Implementations handle API calls or in-memory queries.
            Enables platform independence: Web uses HTTP, MAUI uses direct API client, Desktop uses local DB.

#### Methods & Properties

- **GetDataAsync``1** - Fetch paged grid data based on request (sorting, filtering, pagination).
  - Parameters:
    - `request`: Grid request with sorting, paging, and filter criteria.
    - `cancellationToken`: Cancellation token for async operations.
  - Returns: Result containing GridResponse or error details.

### Guard

- **Namespace:** `SmartWorkz.Shared.Guard`
- **Summary:** Static guard clauses for argument validation at method entry points.
             Throw immediately on invalid input — fail fast, fail loudly.
            
             Usage:
               Guard.NotNull(userId, nameof(userId));
               Guard.NotEmpty(name, nameof(name));
               Guard.InRange(pageSize, 1, 100, nameof(pageSize));
            
             These replace the ValidationExtensions.EnsureNotNull() extension method
             and the scattered ArgumentNullException throws throughout the codebase.

#### Methods & Properties

- **NotNull``1** - Throws ArgumentNullException if value is null.
- **NotNull``1** - Throws ArgumentNullException if value is null (struct/nullable).
- **NotEmpty** - Throws ArgumentException if string is null, empty, or whitespace.
- **NotEmpty``1** - Throws ArgumentException if collection is null or has no elements.
- **NotDefault``1** - Throws ArgumentException if value equals the default for its type (0, null, Guid.Empty).
- **InRange``1** - Throws ArgumentOutOfRangeException if value is outside [min, max].
- **Requires** - Throws ArgumentException if condition is false.

### EncryptionHelper

- **Namespace:** `SmartWorkz.Shared.EncryptionHelper`
- **Summary:** Cryptographic utilities for hashing and encryption.
            Uses PBKDF2 for password hashing and AES-256 for data encryption.

#### Methods & Properties

- **HashPassword** - Hash password using PBKDF2 with SHA256.
- **VerifyPassword** - Verify password against hash.
- **Encrypt** - Encrypt text using AES-256-GCM with provided key.
- **Decrypt** - Decrypt text using AES-256-GCM with provided key.
- **GenerateRandomString** - Generate cryptographically secure random string.
- **GenerateEncryptionKey** - Generate random encryption key (Base64 encoded).
- **ComputeSha256** - Compute SHA256 hash of text for integrity checking.

### JsonHelper

- **Namespace:** `SmartWorkz.Shared.JsonHelper`
- **Summary:** JSON serialization utilities using System.Text.Json.
            Provides consistent serialization options across the application.

#### Methods & Properties

- **Serialize``1** - Serialize object to JSON string.
- **Serialize** - Serialize object to JSON string with dynamic type.
- **Deserialize``1** - Deserialize JSON string to object.
- **Deserialize** - Deserialize JSON string to object with dynamic type.
- **DeserializeAsync``1** - Deserialize JSON asynchronously from stream.
- **SerializeAsync``1** - Serialize asynchronously to stream.
- **IsValidJson** - Check if string is valid JSON.
- **GetValueByPath** - Parse JSON and extract value at specified path (dot notation).

### IHttpClient

- **Namespace:** `SmartWorkz.Shared.IHttpClient`
- **Summary:** Abstraction for HTTP client operations with support for async/await and cancellation.
            Implementations should handle retries, timeouts, and error responses gracefully.

#### Methods & Properties

- **GetAsync``1** - Sends a GET request and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **PostAsync``1** - Sends a POST request with JSON body and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **PutAsync``1** - Sends a PUT request with JSON body and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **DeleteAsync``1** - Sends a DELETE request and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **GetAsync** - Sends a GET request and returns a string response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.
- **PostAsync** - Sends a POST request with JSON body and returns a string response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.

### RetryStrategy

- **Namespace:** `SmartWorkz.Shared.RetryStrategy`
- **Summary:** Specifies the backoff strategy to use when retrying failed HTTP requests.

### RetryPolicy

- **Namespace:** `SmartWorkz.Shared.RetryPolicy`
- **Summary:** Configures automatic retry behavior for failed HTTP requests.

### AuditRecord

- **Namespace:** `SmartWorkz.Shared.AuditRecord`
- **Summary:** Represents an immutable audit record with all relevant audit information.

#### Methods & Properties

- **#ctor** - Represents an immutable audit record with all relevant audit information.
  - Parameters:
    - `Id`: The unique identifier of the audit record
    - `EntityType`: The type of entity being audited (e.g., "User", "BlogPost")
    - `EntityId`: The identifier of the audited entity
    - `Action`: The action performed (Create, Update, Delete, etc.)
    - `UserId`: The identifier of the user who performed the action
    - `PerformedAt`: The timestamp when the action was performed
    - `Metadata`: Optional metadata dictionary containing additional context

### EnrichedLogger

- **Namespace:** `SmartWorkz.Shared.EnrichedLogger`
- **Summary:** Enriched logger wrapper around ILogger that provides structured logging methods
            for domain events, commands, sagas, file operations, and background jobs.
            Uses structured properties instead of string interpolation for better queryability.

#### Methods & Properties

- **#ctor** - Creates a new instance of EnrichedLogger.
  - Parameters:
    - `logger`: The underlying ILogger instance
- **LogCommandExecuted** - Logs command execution with duration and other metrics.
  - Parameters:
    - `commandType`: The type of command being executed
    - `duration`: How long the command took to execute
- **LogCommandExecutionError** - Logs a command execution error with exception details.
  - Parameters:
    - `commandType`: The type of command that failed
    - `exception`: The exception that occurred
- **LogCommandValidationError** - Logs a command with validation errors.
  - Parameters:
    - `commandType`: The type of command
    - `errors`: Dictionary of validation errors
- **LogEventPublished** - Logs an event publication with metadata.
  - Parameters:
    - `eventType`: The type of event being published
    - `eventId`: The unique identifier of the event
- **LogEventPublishedWithContext** - Logs an event with additional context properties.
  - Parameters:
    - `eventType`: The type of event
    - `eventId`: The event identifier
    - `context`: Additional context data
- **LogEventSubscribed** - Logs an event subscription.
  - Parameters:
    - `eventType`: The type of event being subscribed to
    - `subscriberType`: The subscriber type
- **LogSagaStarted** - Logs the start of a saga with its initial state.
  - Parameters:
    - `sagaId`: The unique saga identifier
    - `state`: The initial saga state
- **LogSagaStateTransition** - Logs a saga state transition.
  - Parameters:
    - `sagaId`: The saga identifier
    - `fromState`: The previous state
    - `toState`: The new state
- **LogSagaCompleted** - Logs the completion of a saga.
  - Parameters:
    - `sagaId`: The saga identifier
    - `duration`: How long the saga took to complete
- **LogSagaFailed** - Logs a saga failure.
  - Parameters:
    - `sagaId`: The saga identifier
    - `exception`: The exception that caused the failure
- **LogFileOperation** - Logs file operations such as upload, download, delete.
  - Parameters:
    - `operation`: The type of operation (Upload, Download, Delete, etc.)
    - `filePath`: The file path or URI
- **LogFileOperationWithSize** - Logs a file operation with size information.
  - Parameters:
    - `operation`: The type of operation
    - `filePath`: The file path
    - `sizeBytes`: The file size in bytes
- **LogFileOperationError** - Logs a file operation error.
  - Parameters:
    - `operation`: The operation that failed
    - `filePath`: The file path
    - `exception`: The exception that occurred
- **LogJobQueued** - Logs when a background job is queued.
  - Parameters:
    - `jobId`: The unique job identifier
    - `jobType`: The type of job being queued
- **LogJobStarted** - Logs when a background job starts processing.
  - Parameters:
    - `jobId`: The job identifier
    - `jobType`: The job type
- **LogJobCompleted** - Logs successful job completion.
  - Parameters:
    - `jobId`: The job identifier
    - `duration`: How long the job took to complete
- **LogJobFailed** - Logs a job failure.
  - Parameters:
    - `jobId`: The job identifier
    - `exception`: The exception that caused the failure
- **LogJobRetry** - Logs job retry attempt.
  - Parameters:
    - `jobId`: The job identifier
    - `attemptNumber`: The current attempt number
    - `maxRetries`: The maximum number of retries
- **LogWithContext** - Logs a message with structured context properties.
  - Parameters:
    - `operationName`: The name of the operation
    - `context`: Dictionary of contextual properties
- **LogPerformanceMetrics** - Logs performance metrics for an operation.
  - Parameters:
    - `operationName`: The operation name
    - `duration`: The operation duration
    - `resultStatus`: The result status (Success, Failure, etc.)
- **LogCorrelation** - Logs a correlation ID for request tracing.
  - Parameters:
    - `correlationId`: The correlation identifier
    - `userId`: Optional user identifier
    - `requestPath`: Optional request path
- **LogUnhandledException** - Logs unhandled exceptions as critical errors.
  - Parameters:
    - `exception`: The exception that occurred
    - `operationName`: The operation that failed

### IAuditLogger

- **Namespace:** `SmartWorkz.Shared.IAuditLogger`
- **Summary:** Interface for structured audit logging with metadata support.

#### Methods & Properties

- **LogAuditAsync** - Logs an audit event with structured metadata.
  - Parameters:
    - `entityType`: The entity type being audited (e.g., "User", "BlogPost")
    - `entityId`: The unique identifier of the entity
    - `action`: The action performed (Create, Update, Delete, etc.)
    - `metadata`: Optional metadata dictionary for additional context
    - `cancellationToken`: Cancellation token
  - Returns: Result indicating success or failure
- **GetAuditHistoryAsync** - Retrieves audit logs for a specific entity.
  - Parameters:
    - `entityType`: The entity type
    - `entityId`: The entity identifier
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of audit records
- **GetUserActivityAsync** - Retrieves audit logs for a specific user across all entities.
  - Parameters:
    - `userId`: The user identifier
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of audit records

### ILogger

- **Namespace:** `SmartWorkz.Shared.ILogger`
- **Summary:** Abstraction for application logging.
            Decouples from specific logging frameworks (Serilog, NLog, etc.).

### LogLevel

- **Namespace:** `SmartWorkz.Shared.LogLevel`
- **Summary:** Log level severity.

### ILoggerFactory

- **Namespace:** `SmartWorkz.Shared.ILoggerFactory`
- **Summary:** Factory for creating logger instances by category/source.

### IMapper

- **Namespace:** `SmartWorkz.Shared.IMapper`
- **Summary:** Mapping service abstraction for transforming objects between types.
            Supports registration of mapping profiles and bidirectional conversions.

#### Methods & Properties

- **Map``2** - Map source object to target type.
- **Map** - Map source object to target type using dynamic type.
- **MapAsync``2** - Map asynchronously with potential async operations in profile.
- **MapCollection``2** - Map collection of sources to targets.
- **MapCollectionAsync``2** - Map collection asynchronously.
- **RegisterProfile``2** - Register a mapping profile.

### IMapperProfile

- **Namespace:** `SmartWorkz.Shared.IMapperProfile`
- **Summary:** Profile for defining mapping rules between types.
            Implemented by concrete profiles that configure source-to-target transformations.

### IMapperProfile`2

- **Namespace:** `SmartWorkz.Shared.IMapperProfile`2`
- **Summary:** Typed mapper profile for strong typing.

#### Methods & Properties

- **Map** - Transform source to target synchronously.
- **MapAsync** - Transform source to target asynchronously.

### SimpleMapper

- **Namespace:** `SmartWorkz.Shared.SimpleMapper`
- **Summary:** A simple in-memory mapper that supports registering and executing mapping profiles.

### IMetricsCollector

- **Namespace:** `SmartWorkz.Shared.IMetricsCollector`
- **Summary:** Abstraction for collecting application metrics and performance data.
            Enables tracking of operation duration, throughput, error rates, and custom metrics.
            Implementations integrate with OpenTelemetry for export to Prometheus/Grafana.

#### Methods & Properties

- **RecordOperationDuration** - Record operation duration in milliseconds.
  - Parameters:
    - `operationName`: Name of the operation being measured.
    - `durationMs`: Duration in milliseconds.
    - `status`: Optional status (e.g., "success", "error").
    - `tags`: Optional metadata tags for grouping and filtering.
- **RecordOperationCount** - Record operation count (increments counter).
  - Parameters:
    - `operationName`: Name of the operation.
    - `count`: Number to increment by (default 1).
    - `status`: Optional status label.
    - `tags`: Optional metadata tags.
- **RecordGaugeValue** - Record a gauge value (e.g., queue depth, memory usage).
  - Parameters:
    - `metricName`: Name of the gauge metric.
    - `value`: The gauge value to record.
    - `tags`: Optional metadata tags.
- **RecordError** - Record error/exception occurrence.
  - Parameters:
    - `operationName`: Name of the operation that failed.
    - `ex`: The exception that occurred.
    - `tags`: Optional metadata tags.
- **IncrementCounter** - Increment a custom counter.
  - Parameters:
    - `counterName`: Name of the counter.
    - `increment`: Amount to increment (default 1).
    - `tags`: Optional metadata tags.

### MetricsMiddleware

- **Namespace:** `SmartWorkz.Shared.MetricsMiddleware`
- **Summary:** ASP.NET Core middleware for automatic HTTP request/response metrics collection.
             Records operation duration, status, and errors for all HTTP requests.
            
             Usage:
                 app.UseMiddleware<MetricsMiddleware>();

### MetricsStartupExtensions

- **Namespace:** `SmartWorkz.Shared.MetricsStartupExtensions`
- **Summary:** Extension methods for registering application metrics in dependency injection.

#### Methods & Properties

- **AddApplicationMetrics** - Registers IMetricsCollector with OpenTelemetry implementation.
  - Parameters:
    - `services`: The service collection to register with.
  - Returns: The service collection for method chaining.

### OpenTelemetryMetricsCollector

- **Namespace:** `SmartWorkz.Shared.OpenTelemetryMetricsCollector`
- **Summary:** OpenTelemetry-based implementation of IMetricsCollector.
            Collects metrics using System.Diagnostics.Metrics for export to Prometheus/Grafana.

### DefaultTenantFeatureFlags

- **Namespace:** `SmartWorkz.Shared.DefaultTenantFeatureFlags`
- **Summary:** In-memory feature flag provider for tenant-scoped feature control.
            
             Uses ConcurrentDictionary to store tenant-specific flags:
             - Key: tenant ID
             - Value: HashSet of enabled feature flag names
            
             Thread-safe for concurrent operations. Suitable for in-process caching
             or dev/test scenarios. For distributed systems, integrate with a
             centralized feature flag service (Unleash, LaunchDarkly, etc.).

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature is enabled for the specified tenant.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").
    - `cancellationToken`: Cancellation token.
  - Returns: Task containing true if the feature is enabled for this tenant,
            false if the tenant doesn't exist or the flag is not enabled.
- **GetEnabledFeaturesAsync** - Gets all enabled features for a tenant.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Task containing a read-only list of enabled feature flag names.
            Returns an empty list if the tenant doesn't exist or has no enabled flags.
- **EnableFlag** - Enables a feature flag for a tenant.
            Idempotent — calling multiple times with the same flag is safe.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name.
- **DisableFlag** - Disables a feature flag for a tenant.
            Idempotent — calling multiple times with the same flag is safe.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name.

### ITenantContext

- **Namespace:** `SmartWorkz.Shared.ITenantContext`
- **Summary:** Scoped service providing current tenant ID for multi-tenant applications.
            Resolved from request context or claims principal.

#### Methods & Properties

- **GetTenantId** - Gets the current tenant identifier.
  - Returns: Tenant ID, or null if operating in single-tenant context.
- **SetTenantId** - Sets the current tenant identifier (rarely used; typically set from request context).
  - Parameters:
    - `tenantId`: Tenant ID to set.

### ITenantFeatureFlags

- **Namespace:** `SmartWorkz.Shared.ITenantFeatureFlags`
- **Summary:** Feature flag provider scoped to a specific tenant.
            Allows per-tenant feature control.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature is enabled for the specified tenant.
  - Parameters:
    - `tenantId`: Tenant ID.
    - `flagName`: Feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").
    - `cancellationToken`: Cancellation token.
  - Returns: True if feature is enabled for this tenant.
- **GetEnabledFeaturesAsync** - Gets all enabled features for a tenant.
  - Parameters:
    - `tenantId`: Tenant ID.
    - `cancellationToken`: Cancellation token.
  - Returns: List of enabled feature flag names.

### TenantContext

- **Namespace:** `SmartWorkz.Shared.TenantContext`
- **Summary:** Scoped tenant context using AsyncLocal for proper isolation across async boundaries.
            
             AsyncLocal ensures:
             - Thread-safe storage per async execution context
             - Isolation between concurrent requests (each gets its own context)
             - Proper inheritance to child tasks (when awaited)
            
             Survives async/await boundaries unlike ThreadLocal, making it suitable for async methods.

#### Methods & Properties

- **GetTenantId** - Gets the current tenant identifier.
  - Returns: The current tenant ID, or "default" if not set.
- **SetTenantId** - Sets the current tenant identifier.
  - Parameters:
    - `tenantId`: The tenant ID to set. Cannot be null or empty.

### FirebaseCloudMessagingService

- **Namespace:** `SmartWorkz.Shared.FirebaseCloudMessagingService`
- **Summary:** Firebase Cloud Messaging service implementation for sending push notifications.
            Supports single/batch user notifications, topic-based broadcasting, and multi-platform delivery (Android, iOS, Web).

#### Methods & Properties

- **#ctor** - Initializes a new instance of the FirebaseCloudMessagingService.
  - Parameters:
    - `logger`: Logger for diagnostic and error information.
- **SendAsync** - Sends a simple push notification to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends simple push notifications to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase device tokens).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends a rich push notification with metadata to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends a rich push notification to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase device tokens).
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SendToTopicAsync** - Sends a rich push notification to all users subscribed to a topic (broadcast).
  - Parameters:
    - `topic`: Topic name (e.g., "news", "promotions").
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SubscribeToTopicAsync** - Subscribes a user to a topic for broadcast notifications.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `topic`: Topic name to subscribe to.
    - `cancellationToken`: Cancellation token.
- **UnsubscribeFromTopicAsync** - Unsubscribes a user from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `topic`: Topic name to unsubscribe from.
    - `cancellationToken`: Cancellation token.

### IPushNotificationService

- **Namespace:** `SmartWorkz.Shared.IPushNotificationService`
- **Summary:** Service for sending push notifications using Firebase Cloud Messaging.

#### Methods & Properties

- **SendAsync** - Sends a simple push notification to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a simple push notification to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase tokens).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a rich push notification with metadata to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a rich push notification with metadata to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase tokens).
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendToTopicAsync** - Sends a push notification to all users subscribed to a topic.
  - Parameters:
    - `topic`: The topic name.
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SubscribeToTopicAsync** - Subscribes a user to receive notifications from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `topic`: The topic name.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous subscription operation.
- **UnsubscribeFromTopicAsync** - Unsubscribes a user from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `topic`: The topic name.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous unsubscription operation.

### PushNotificationPayload

- **Namespace:** `SmartWorkz.Shared.PushNotificationPayload`
- **Summary:** Represents the payload data for a push notification.

### PushNotificationAction

- **Namespace:** `SmartWorkz.Shared.PushNotificationAction`
- **Summary:** Represents an action that can be performed from a push notification.

### PagedList`1

- **Namespace:** `SmartWorkz.Shared.PagedList`1`
- **Summary:** A page of items with metadata.
             Replaces PaginationResponse<T> in StarterKitMVC.Shared.DTOs.
            
             Migration path: PaginationResponse<T> has the same fields under different names.
             PagedList<T>.Create() is a drop-in replacement for PaginationResponse<T>.Create().

#### Methods & Properties

- **Empty** - Create an empty result set (e.g., when no rows match).
- **Map``1** - Project items to a different type without changing pagination metadata.

### PagedQuery

- **Namespace:** `SmartWorkz.Shared.PagedQuery`
- **Summary:** Standard request parameters for any paginated query.
             Replaces the existing PaginationRequest record in StarterKitMVC.Shared.DTOs.
            
             Migration: PaginationRequest is structurally identical — alias it or replace it.

#### Methods & Properties

- **#ctor** - Standard request parameters for any paginated query.
             Replaces the existing PaginationRequest record in StarterKitMVC.Shared.DTOs.
            
             Migration: PaginationRequest is structurally identical — alias it or replace it.
- **Normalize** - Clamp page and pageSize to safe bounds.

### IEntity`1

- **Namespace:** `SmartWorkz.Shared.IEntity`1`
- **Summary:** Marks a class as a domain entity with a typed primary key.

### CircuitBreaker

- **Namespace:** `SmartWorkz.Shared.CircuitBreaker`
- **Summary:** A thread-safe implementation of the circuit breaker pattern for handling failing dependencies gracefully.
            
             The circuit breaker operates in three states:
             - Closed: Normal operation. Requests pass through. Failures are tracked.
             - Open: Failing. All requests are rejected immediately to prevent cascading failures.
             - HalfOpen: Testing recovery. Limited requests are allowed to test if the dependency has recovered.
            
             State transitions:
             - Closed → Open: When ConsecutiveFailures >= FailureThreshold
             - Open → HalfOpen: Automatically when (DateTime.UtcNow - LastFailureTime) >= TimeoutMilliseconds
             - HalfOpen → Closed: When SuccessCount >= SuccessThreshold
             - HalfOpen → Open: When RecordFailure() is called in HalfOpen state
             - Closed → Closed: When RecordSuccess() is called (resets failure counter)

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `options`: The circuit breaker configuration options.
- **RecordSuccess** - Records a successful operation and updates state transitions accordingly.
            In Closed state: resets the failure counter.
            In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
- **RecordFailure** - Records a failed operation and updates state transitions accordingly.
            In Closed state: increments failure counter and transitions to Open if threshold is met.
            In HalfOpen state: immediately transitions back to Open.
- **Reset** - Resets the circuit breaker to the Closed state, clearing all failure and success counters.

### CircuitBreakerOptions

- **Namespace:** `SmartWorkz.Shared.CircuitBreakerOptions`
- **Summary:** Configuration options for the circuit breaker.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if options are valid; otherwise false.

### CircuitBreakerState

- **Namespace:** `SmartWorkz.Shared.CircuitBreakerState`
- **Summary:** Defines the state of a circuit breaker in the state machine pattern.

### ICircuitBreaker

- **Namespace:** `SmartWorkz.Shared.ICircuitBreaker`
- **Summary:** Defines the contract for a circuit breaker that implements the state machine pattern
            to handle failing dependencies gracefully.

#### Methods & Properties

- **RecordSuccess** - Records a successful operation and updates state transitions accordingly.
            In Closed state: resets the failure counter.
            In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
- **RecordFailure** - Records a failed operation and updates state transitions accordingly.
            In Closed state: increments failure counter and transitions to Open if threshold is met.
            In HalfOpen state: immediately transitions back to Open.
- **Reset** - Resets the circuit breaker to the Closed state, clearing all failure and success counters.

### IRateLimiter

- **Namespace:** `SmartWorkz.Shared.IRateLimiter`
- **Summary:** Defines the contract for a thread-safe rate limiter.

#### Methods & Properties

- **TryAcquireAsync** - Attempts to acquire tokens for a request identified by the given identifier.
  - Parameters:
    - `identifier`: The identifier for rate limiting (e.g., IP address, user ID).
    - `cost`: The number of tokens to acquire (default: 1).
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A result containing true if tokens were acquired successfully; otherwise false.
            On failure, the Error will include retry-after information.
- **GetAvailableTokensAsync** - Gets the number of available tokens for a given identifier.
  - Parameters:
    - `identifier`: The identifier to check.
  - Returns: The number of available tokens.
- **ResetAsync** - Resets the rate limit state for a given identifier.
  - Parameters:
    - `identifier`: The identifier to reset.
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **ClearAsync** - Clears all rate limit state.
  - Parameters:
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.

### RateLimiter

- **Namespace:** `SmartWorkz.Shared.RateLimiter`
- **Summary:** Thread-safe token bucket rate limiter implementation.
            
             This class maintains a per-identifier token bucket that refills at a constant rate.
             Tokens are consumed when requests are made; if insufficient tokens exist, the request is denied.
            
             Thread-safe operations use ConcurrentDictionary and locks on individual buckets to ensure
             consistent state without global locking bottlenecks.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the RateLimiter class.
  - Parameters:
    - `options`: Configuration options for the rate limiter.
- **TryAcquireAsync** - Attempts to acquire tokens for a request identified by the given identifier.
  - Parameters:
    - `identifier`: The identifier for rate limiting (e.g., IP address, user ID).
    - `cost`: The number of tokens to acquire (default: 1).
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A result containing true if tokens were acquired successfully; otherwise false.
            On failure, the Error will include retry-after information.
- **GetAvailableTokensAsync** - Gets the number of available tokens for a given identifier.
  - Parameters:
    - `identifier`: The identifier to check.
  - Returns: The number of available tokens.
- **ResetAsync** - Resets the rate limit state for a given identifier.
  - Parameters:
    - `identifier`: The identifier to reset.
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **ClearAsync** - Clears all rate limit state.
  - Parameters:
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **TokenBucket.TryAcquire** - Tries to acquire the specified number of tokens.
- **TokenBucket.GetAvailableTokens** - Gets the current number of available tokens.
- **TokenBucket.GetRetryAfterMilliseconds** - Gets the number of milliseconds to wait before retrying.
- **TokenBucket.RefillTokens** - Refills the token bucket based on elapsed time.

### RateLimiterOptions

- **Namespace:** `SmartWorkz.Shared.RateLimiterOptions`
- **Summary:** Configuration options for the rate limiter.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if options are valid; otherwise false.

### RateLimiterStrategy

- **Namespace:** `SmartWorkz.Shared.RateLimiterStrategy`
- **Summary:** Specifies the strategy used by the rate limiter to control request flow.

### ApiError

- **Namespace:** `SmartWorkz.Shared.ApiError`
- **Summary:** Structured error representation for API responses.
            Provides code, message, and optional field-level error details.

#### Methods & Properties

- **FromError** - Create from core Error type.
- **FromValidationErrors** - Create from validation errors.
- **FromException** - Create from exception.

### ApiResponse

- **Namespace:** `SmartWorkz.Shared.ApiResponse`
- **Summary:** Generic API response envelope that wraps result data with metadata.
            Non-generic convenience version for non-data responses.

#### Methods & Properties

- **Ok** - Success response without data.
- **Fail** - Failure response with error details.
- **FromResult** - Create from core Result pattern.

### ApiResponse`1

- **Namespace:** `SmartWorkz.Shared.ApiResponse`1`
- **Summary:** Typed API response envelope with data payload.
            Includes optional pagination metadata for list responses.

#### Methods & Properties

- **Ok** - Success response with data.
- **OkPaginated** - Success response with paginated data.
- **Fail** - Failure response with error.

### ProblemDetailsResponse

- **Namespace:** `SmartWorkz.Shared.ProblemDetailsResponse`
- **Summary:** Implements RFC 7807 Problem Details for HTTP APIs standard response format.
            Provides a standardized way to represent error details in API responses.

#### Methods & Properties

- **ValidationError** - Factory method for 400 Bad Request error with validation details.
- **Unauthorized** - Factory method for 401 Unauthorized error.
- **Forbidden** - Factory method for 403 Forbidden error.
- **NotFound** - Factory method for 404 Not Found error.
- **Conflict** - Factory method for 409 Conflict error.
- **InternalServerError** - Factory method for 500 Internal Server Error.
- **Custom** - Factory method for custom problem details.

### Error

- **Namespace:** `SmartWorkz.Shared.Error`
- **Summary:** Represents a structured error with a machine-readable code and human-readable message.
            
             This is the canonical Error type. It replaces:
             - SmartWorkz.StarterKitMVC.Shared.Primitives.Error (record struct)
             - The ad-hoc string errors in Models.Result
            
             Code examples: "USER_NOT_FOUND", "VALIDATION.EMAIL_REQUIRED", "AUTH.INVALID_CREDENTIALS"
             MessageKey maps to localization resource keys for UI display.

### Result

- **Namespace:** `SmartWorkz.Shared.Result`
- **Summary:** Represents the outcome of an operation that does not return a value.
            
             This unifies:
             - SmartWorkz.StarterKitMVC.Shared.Models.Result (Succeeded + MessageKey + Errors[])
             - SmartWorkz.StarterKitMVC.Shared.Primitives.Result (IsSuccess + Error struct)
            
             Design choice — class over struct:
             1. Result<T> inherits from Result to reuse Succeeded/Errors without duplication.
                Structs cannot use inheritance this way.
             2. Services return Result from interface methods — class semantics (null check) are
                simpler than boxing/unboxing structs across interface boundaries.
             3. Errors[] supports field-level validation messages that ModelState.AddErrors() consumes.
                A single Error struct cannot carry multiple field errors.
            
             The Primitives.Result struct in StarterKitMVC.Shared remains valid for pure functions
             where you want zero-allocation returns. This class is for service layer contracts.

#### Methods & Properties

- **Fail** - Failure with a localization message key and optional field-level error strings.
- **Fail** - Failure from a structured Error (bridges the Primitives.Error pattern).
- **Ok``1** - Factory for a typed result. Use in services that return data.

### Result`1

- **Namespace:** `SmartWorkz.Shared.Result`1`
- **Summary:** Result with a typed payload. Data is only valid when Succeeded = true.
            
             Usage:
               Result<UserDto> result = await _userService.GetByIdAsync(id);
               if (!result.Succeeded) return RedirectToPage("Error");
               var user = result.Data!;

### ResultExtensions

- **Namespace:** `SmartWorkz.Shared.ResultExtensions`
- **Summary:** Functional helpers for chaining Result operations.
            Keeps service code flat — avoids nested if (!result.Succeeded) blocks.

#### Methods & Properties

- **Map``2** - Transform the Data value if the result succeeded.
- **BindAsync``2** - Chain a second operation that also returns Result.
- **OnSuccess``1** - Execute a side-effect action on success, then return the original result.
- **OnFailure``1** - Execute a side-effect action on failure, then return the original result.

### ISagaDefinition`1

- **Namespace:** `SmartWorkz.Shared.ISagaDefinition`1`
- **Summary:** Defines the blueprint for a saga orchestration.
            A saga is a pattern for managing distributed transactions and long-running processes
            by coordinating multiple steps with built-in compensation mechanisms.

#### Methods & Properties

- **DefineStep``1** - Defines a step in the saga that will be executed when a specific event type is received.
            Steps are executed sequentially in the order they were defined.
  - Parameters:
    - `handler`: The async handler function that processes the event and updates the saga state.
            Returns a StepResult indicating success or failure.
- **OnFailure** - Defines the failure handler that will be called if any step fails.
            Used for compensation logic and saga-level error handling.
  - Parameters:
    - `compensationHandler`: The async handler that receives the current saga state and the exception that occurred.
            Responsible for compensation/rollback logic.
- **BuildAsync** - Builds and returns the saga definition for execution.
            Can be used for async initialization or validation.
  - Returns: A task that completes with the configured saga definition.
- **GetSteps** - Gets the list of saga steps in execution order.
  - Returns: A read-only list of saga step handlers.
- **GetFailureHandler** - Gets the failure compensation handler if defined.
  - Returns: The failure handler function, or null if not defined.

### SagaOrchestrator

- **Namespace:** `SmartWorkz.Shared.SagaOrchestrator`
- **Summary:** Orchestrates the execution of sagas, managing step sequencing, error handling,
            and compensation/rollback logic for complex distributed processes.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the SagaOrchestrator class.
  - Parameters:
    - `logger`: Logger for saga execution tracking and debugging.
- **ExecuteSagaAsync``1** - Executes a saga definition with the provided initial state and triggering event.
            Manages step execution, error handling, and compensation logic.
  - Parameters:
    - `sagaDefinition`: The saga definition blueprint to execute.
    - `initialState`: The initial saga state.
    - `event`: The domain event triggering the saga.
    - `cancellationToken`: Optional cancellation token.
  - Returns: A task representing the saga execution.
- **ExecuteSagaStepsAsync``1** - Executes saga steps by using reflection to access internal step definitions.
- **CompensateExecutedStepsAsync``1** - Executes compensation handlers for all executed steps in reverse order.
            Uses stored compensation handlers to avoid re-executing steps.
- **ExecuteFailureHandlerAsync``1** - Executes the saga-level failure handler if one is defined.

### SagaStatus

- **Namespace:** `SmartWorkz.Shared.SagaStatus`
- **Summary:** Represents the status of a saga execution.

### SagaState

- **Namespace:** `SmartWorkz.Shared.SagaState`
- **Summary:** Base class for saga state objects.
            Provides common tracking properties for saga execution flow.

### StepResult

- **Namespace:** `SmartWorkz.Shared.StepResult`
- **Summary:** Represents the result of executing a single saga step.
            Provides success/failure status and optional compensation logic for rollback.

#### Methods & Properties

- **Success** - Creates a successful step result.
  - Returns: A StepResult indicating success.
- **Failure** - Creates a failed step result with an optional compensation handler.
  - Parameters:
    - `failureReason`: The reason for the step failure.
    - `compensationHandler`: Optional handler to compensate/rollback this step if a later step fails.
  - Returns: A StepResult indicating failure.
- **FromException** - Creates a failed step result for an exception with optional compensation.
  - Parameters:
    - `exception`: The exception that caused the failure.
    - `compensationHandler`: Optional compensation handler.
  - Returns: A StepResult indicating failure.

### CryptHelper

- **Namespace:** `SmartWorkz.Shared.CryptHelper`
- **Summary:** Provides AES-256-CBC encryption and decryption utilities with secure key and IV generation.
            
             All operations support both string and byte array inputs/outputs.
             Keys are normalized to 32 bytes (256 bits) via padding/trimming as needed.
             IVs are auto-generated if not provided and embedded in the ciphertext (IV:Ciphertext format).

#### Methods & Properties

- **EncryptString** - Encrypts plaintext using AES-256-CBC with a Base64-encoded output.
  - Parameters:
    - `plaintext`: The plaintext to encrypt.
    - `key`: The encryption key (will be normalized to 32 bytes).
    - `iv`: Optional IV; auto-generated if null.
  - Returns: A Result containing Base64-encoded ciphertext in "IV:Ciphertext" format or an error.
- **EncryptBytes** - Encrypts byte data using AES-256-CBC.
  - Parameters:
    - `plaintext`: The plaintext bytes to encrypt.
    - `key`: The encryption key (will be normalized to 32 bytes).
    - `iv`: Optional IV; auto-generated if null.
  - Returns: A Result containing encrypted bytes with embedded IV (IV || Ciphertext) or an error.
- **DecryptString** - Decrypts Base64-encoded ciphertext using AES-256-CBC.
  - Parameters:
    - `ciphertext`: The Base64-encoded ciphertext in "IV:Ciphertext" format.
    - `key`: The decryption key (will be normalized to 32 bytes).
  - Returns: A Result containing the decrypted plaintext or an error.
- **DecryptBytes** - Decrypts byte data using AES-256-CBC.
  - Parameters:
    - `ciphertext`: The encrypted bytes with embedded IV (IV || Ciphertext).
    - `key`: The decryption key (will be normalized to 32 bytes).
  - Returns: A Result containing decrypted bytes or an error.
- **GenerateKey** - Generates a random cryptographic key of the specified size.
  - Parameters:
    - `keySize`: The key size in bytes (default 32 for AES-256). Must be 16, 24, or 32.
  - Returns: A Result containing Base64-encoded random key or an error.
- **GenerateIv** - Generates a random cryptographic IV (Initialization Vector).
  - Returns: A Result containing Base64-encoded random IV or an error.
- **GenerateRandomBytes** - Generates cryptographically secure random bytes.
- **NormalizeKey** - Normalizes a key to exactly 32 bytes (256 bits).
            If the key is shorter, it's padded with zeros. If longer, it's trimmed.

### CryptOptions

- **Namespace:** `SmartWorkz.Shared.CryptOptions`
- **Summary:** Configuration options for AES cryptographic operations.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if valid, false otherwise.

### HashHelper

- **Namespace:** `SmartWorkz.Shared.HashHelper`
- **Summary:** Provides utilities for cryptographic hash operations (SHA256 and MD5).

#### Methods & Properties

- **Sha256** - Computes the SHA256 hash of a string and returns it as a hexadecimal string.
- **Sha256Bytes** - Computes the SHA256 hash of a byte array and returns the hash as a byte array.
- **Md5** - Computes the MD5 hash of a string and returns it as a hexadecimal string.
            Note: MD5 is cryptographically broken; use SHA256 for security-critical applications.
- **VerifyHash** - Verifies that a text matches its SHA256 hash.

### HmacAlgorithm

- **Namespace:** `SmartWorkz.Shared.HmacAlgorithm`
- **Summary:** Specifies the HMAC algorithm to use for message signing and verification.

### HmacHelper

- **Namespace:** `SmartWorkz.Shared.HmacHelper`
- **Summary:** Provides HMAC-SHA256/SHA512 message signing and verification for API requests and webhook verification.
            Implements constant-time comparison to prevent timing attacks.

#### Methods & Properties

- **Sign** - Signs a message using HMAC with the specified algorithm and returns a Base64-encoded hex digest.
  - Parameters:
    - `message`: The message to sign.
    - `secret`: The secret key for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing the Base64-encoded signature or an error.
- **SignBytes** - Signs a message using HMAC with the specified algorithm and returns the raw byte digest.
  - Parameters:
    - `message`: The message bytes to sign.
    - `secret`: The secret key for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing the byte signature or an error.
- **Verify** - Verifies a message signature using HMAC with constant-time comparison to prevent timing attacks.
  - Parameters:
    - `message`: The original message that was signed.
    - `signature`: The Base64-encoded signature to verify.
    - `secret`: The secret key used for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing true if the signature is valid, false if not, or an error.
- **VerifyBytes** - Verifies a message signature using HMAC with raw byte inputs and constant-time comparison.
  - Parameters:
    - `message`: The original message bytes that were signed.
    - `signature`: The byte signature to verify.
    - `secret`: The secret key used for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing true if the signature is valid, false if not, or an error.
- **SignBytes** - Internal method to compute HMAC signature from raw bytes.
- **CreateHmac** - Creates the appropriate HMAC instance based on the algorithm.

### InputSanitizer

- **Namespace:** `SmartWorkz.Shared.InputSanitizer`
- **Summary:** Input sanitization to prevent XSS, SQL injection, and path traversal attacks.

#### Methods & Properties

- **SanitizeHtml** - Sanitize HTML by removing dangerous tags and attributes.
- **EscapeHtml** - Escape HTML special characters to prevent XSS.
- **SanitizeSql** - Sanitize string to prevent SQL injection (basic, not a replacement for parameterized queries).
- **SanitizeFilePath** - Sanitize file path to prevent directory traversal attacks.
- **SanitizeUrl** - Sanitize and validate URL.
- **EscapeJson** - Escape string for safe JSON inclusion.
- **IsValidEmail** - Validate email format (basic check, server-side SMTP validation recommended).
- **RemoveControlCharacters** - Remove null bytes and control characters.

### JwtSettings

- **Namespace:** `SmartWorkz.Shared.JwtSettings`
- **Summary:** Settings for JWT token generation and validation.

#### Methods & Properties

- **Validate** - Validate settings: Secret >= 32 chars, other fields non-empty.

### JwtClaims

- **Namespace:** `SmartWorkz.Shared.JwtClaims`
- **Summary:** JWT claims that can be included in a token.

#### Methods & Properties

- **GetClaimValue** - Get claim value by type (supports standard claims + custom).

### JwtTokenValidationResult

- **Namespace:** `SmartWorkz.Shared.JwtTokenValidationResult`
- **Summary:** Result of JWT token validation.

### JwtHelper

- **Namespace:** `SmartWorkz.Shared.JwtHelper`
- **Summary:** Provides JWT token generation, validation, and refresh functionality.

#### Methods & Properties

- **GenerateTokenInternal** - Internal token generation logic shared by GenerateToken and GenerateRefreshToken.
  - Parameters:
    - `claims`: The claims to include in the token.
    - `settings`: The JWT settings for signing and configuration.
    - `isRefreshToken`: If true, uses RefreshTokenExpiryDays; otherwise uses ExpiryMinutes.
  - Returns: A Result containing the signed token or an error.
- **GenerateToken** - Generates a JWT access token with the specified claims and settings.
  - Parameters:
    - `claims`: The claims to include in the token.
    - `settings`: The JWT settings for signing and configuration.
  - Returns: A Result containing the signed token or an error.
- **ValidateToken** - Validates a JWT token and extracts claims if valid.
  - Parameters:
    - `token`: The token to validate.
    - `settings`: The JWT settings for validation.
  - Returns: A Result containing the validation result.
- **RefreshToken** - Refreshes an access token using a refresh token.
  - Parameters:
    - `refreshToken`: The refresh token.
    - `settings`: The JWT settings.
  - Returns: A Result containing the new access token or an error.
- **GenerateRefreshToken** - Generates a refresh token with extended expiry.
  - Parameters:
    - `claims`: The claims to include in the refresh token.
    - `settings`: The JWT settings.
  - Returns: A Result containing the refresh token or an error.
- **ToBase64Url** - Encodes bytes to Base64Url format (no padding, + → -, / → _).
- **FromBase64Url** - Decodes Base64Url format to bytes.

### PasswordHelper

- **Namespace:** `SmartWorkz.Shared.PasswordHelper`
- **Summary:** Provides secure password generation and validation using cryptographically secure random number generation.

#### Methods & Properties

- **GeneratePassword** - Generates a cryptographically secure random password.
  - Parameters:
    - `length`: Length of the password (8-128, default 12).
    - `includeSpecialChars`: Whether to include special characters.
  - Returns: A Result containing the generated password or an error.
- **ValidateStrength** - Validates the strength of a password against a policy.
  - Parameters:
    - `password`: The password to validate.
    - `policy`: The policy to validate against (uses default if null).
  - Returns: A Result containing the validation result.
- **GetRandomChar** - Gets a random character from the specified character set using cryptographic randomness.
- **Shuffle** - Performs Fisher-Yates shuffle on the character array.
- **CheckPasswordLength** - Checks if password meets minimum length requirement.
- **CheckUppercase** - Checks if password contains at least one uppercase letter.
- **CheckLowercase** - Checks if password contains at least one lowercase letter.
- **CheckNumbers** - Checks if password contains at least one digit.
- **CheckSpecialChars** - Checks if password contains at least one special character.

### PasswordPolicy

- **Namespace:** `SmartWorkz.Shared.PasswordPolicy`
- **Summary:** Policy for password validation requirements.

#### Methods & Properties

- **Validate** - Validates the policy invariants.

### PasswordValidationResult

- **Namespace:** `SmartWorkz.Shared.PasswordValidationResult`
- **Summary:** Result of password validation against a policy.

### ITemplateEngine

- **Namespace:** `SmartWorkz.Shared.ITemplateEngine`
- **Summary:** Defines operations for rendering templates with placeholder substitution.

#### Methods & Properties

- **Render** - Renders a template string by replacing placeholders with values from a dictionary.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `values`: Dictionary of key-value pairs to substitute into the template.
  - Returns: The rendered content with placeholders replaced. Unmatched placeholders remain unchanged.
- **Render** - Renders a template string by extracting properties from an object model.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `model`: The model object whose public properties are used for placeholder substitution.
  - Returns: The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.
- **RenderFileAsync** - Asynchronously renders a template file by replacing placeholders with values from a dictionary.
  - Parameters:
    - `filePath`: The path to the template file.
    - `values`: Dictionary of key-value pairs to substitute into the template.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **RenderFileAsync** - Asynchronously renders a template file by extracting properties from an object model.
  - Parameters:
    - `filePath`: The path to the template file.
    - `model`: The model object whose public properties are used for placeholder substitution.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **LoadDirectoryAsync** - Asynchronously loads all template files from a directory into a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.
- **RenderDirectoryAsync** - Asynchronously loads and renders all template files from a directory using values from a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `values`: Dictionary of key-value pairs to substitute into each template.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.

### TemplateEngine

- **Namespace:** `SmartWorkz.Shared.TemplateEngine`
- **Summary:** Provides template rendering services with support for placeholder substitution.

#### Methods & Properties

- **PlaceholderRegex** - 
- **Render** - Renders a template string by replacing placeholders with values from a dictionary.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `values`: Dictionary of key-value pairs to substitute into the template.
  - Returns: The rendered content with placeholders replaced. Unmatched placeholders and null/empty content remain unchanged.
- **Render** - Renders a template string by extracting properties from an object model.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `model`: The model object whose public properties are used for placeholder substitution.
  - Returns: The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.
- **RenderFileAsync** - Asynchronously renders a template file by replacing placeholders with values from a dictionary.
  - Parameters:
    - `filePath`: The path to the template file.
    - `values`: Dictionary of key-value pairs to substitute into the template.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **RenderFileAsync** - Asynchronously renders a template file by extracting properties from an object model.
  - Parameters:
    - `filePath`: The path to the template file.
    - `model`: The model object whose public properties are used for placeholder substitution.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **LoadDirectoryAsync** - Asynchronously loads all template files from a directory into a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.
- **RenderDirectoryAsync** - Asynchronously loads and renders all template files from a directory using values from a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `values`: Dictionary of key-value pairs to substitute into each template.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.
- **ReflectModel** - Reflects over a model object and builds a case-insensitive dictionary of public properties
            mapped to their string values. Uses cached property metadata for performance.
- **ValidateFilePath** - Validates a file path to prevent directory traversal attacks.
  - Parameters:
    - `filePath`: The file path to validate.
  - Returns: A result indicating if the path is valid and safe.

### CompressHelper

- **Namespace:** `SmartWorkz.Shared.CompressHelper`
- **Summary:** Provides utilities for GZip compression and decompression.

#### Methods & Properties

- **CompressString** - Compresses a string using GZip compression.
- **DecompressString** - Decompresses a GZip-compressed byte array back to a string.
- **CompressBytes** - Compresses a byte array using GZip compression.
- **DecompressBytes** - Decompresses a GZip-compressed byte array.

### DateHelper

- **Namespace:** `SmartWorkz.Shared.DateHelper`
- **Summary:** Provides utilities for date and time operations.

#### Methods & Properties

- **GetAge** - Calculates the age in years from a birth date to today.
- **GetRelativeTime** - Returns a human-readable relative time string (e.g., "2 days ago", "in 3 hours").
- **StartOfDay** - Returns the start of the day (00:00:00) for the given date.
- **EndOfDay** - Returns the end of the day (23:59:59.999) for the given date.
- **IsWeekend** - Determines if the given date falls on a weekend (Saturday or Sunday).
- **GetDayOfWeekName** - Returns the name of the day of week (e.g., "Monday", "Tuesday").
- **DaysBetween** - Calculates the number of days between two dates (inclusive of the from date, exclusive of the to date).

### EnumHelper

- **Namespace:** `SmartWorkz.Shared.EnumHelper`
- **Summary:** Provides utilities for enum operations including reflection and description retrieval.

#### Methods & Properties

- **GetDescription** - Gets the description of an enum value from its [Description] attribute.
            Falls back to the enum name if no description is found.
- **GetValue``1** - Attempts to get an enum value by its name.
- **GetAllValues``1** - Returns all values of the specified enum type as a list.
- **GetName** - Gets the name of an enum value.

### MathHelper

- **Namespace:** `SmartWorkz.Shared.MathHelper`
- **Summary:** Provides utilities for common math operations.

#### Methods & Properties

- **Percentage** - Calculates the percentage of a value.
            Example: Percentage(100, 20) returns 20 (20% of 100).
- **PercentageChange** - Calculates the percentage change from oldValue to newValue.
            Positive result indicates increase, negative indicates decrease.
- **RoundTo** - Rounds a decimal value to the specified number of decimal places.
- **Clamp``1** - Clamps a value within a specified range [min, max].
- **Average** - Calculates the average of the provided decimal values.

### SlugHelper

- **Namespace:** `SmartWorkz.Shared.SlugHelper`
- **Summary:** Helper for generating URL-friendly slugs from text input.

#### Methods & Properties

- **GenerateSlug** - Generates a URL-friendly slug from the given text with optional configuration.
  - Parameters:
    - `text`: The input text to convert to a slug.
    - `options`: Configuration options. If null, default options are used.
  - Returns: A Result containing the generated slug or an error.
- **ToSlug** - Generates a URL-friendly slug from the given text using default options.
            Convenience method equivalent to GenerateSlug(text, null).
  - Parameters:
    - `text`: The input text to convert to a slug.
  - Returns: A Result containing the generated slug or an error.
- **RemoveAccents** - Removes accented characters from text by decomposing them and filtering out combining marks.
            For example: "café" → "cafe", "naïve" → "naive", "Señor" → "Senor".
  - Parameters:
    - `input`: The input text potentially containing accented characters.
  - Returns: The text with accented characters converted to their base forms.
- **ReplaceSpecialCharacters** - Replaces special characters and spaces with the specified separator.
            Keeps only alphanumeric characters and the separator.
  - Parameters:
    - `input`: The input text.
    - `separator`: The separator to use for special characters and spaces.
  - Returns: The text with special characters replaced by the separator.

### SlugOptions

- **Namespace:** `SmartWorkz.Shared.SlugOptions`
- **Summary:** Options for configuring slug generation behavior in .

### TextHelper

- **Namespace:** `SmartWorkz.Shared.TextHelper`
- **Summary:** Sealed class providing advanced text processing and formatting utilities.
            All methods return Result<string> for consistent error handling.

#### Methods & Properties

- **Truncate** - Truncates text to a maximum length and appends a suffix (default "...").
  - Parameters:
    - `text`: The input text to truncate.
    - `maxLength`: The maximum length including the suffix.
    - `suffix`: The suffix to append when truncating. Defaults to "...".
  - Returns: A Result containing the truncated text or an error.
- **Capitalize** - Capitalizes the first letter of the text while preserving the rest.
  - Parameters:
    - `text`: The input text to capitalize.
  - Returns: A Result containing the capitalized text or an error.
- **Decapitalize** - Decapitalizes the first letter of the text while preserving the rest.
  - Parameters:
    - `text`: The input text to decapitalize.
  - Returns: A Result containing the decapitalized text or an error.
- **StripHtml** - Removes HTML tags from the input string using regex.
  - Parameters:
    - `html`: The HTML string to process.
  - Returns: A Result containing the plain text with HTML tags removed or an error.
- **Pluralize** - Pluralizes a word based on count using a simple heuristic.
            If count == 1, returns singular form. Otherwise appends 's'.
  - Parameters:
    - `singular`: The singular form of the word.
    - `count`: The count to determine plural form.
  - Returns: A Result containing the appropriately pluralized word or an error.
- **TitleCase** - Converts text to title case by capitalizing the first letter of each word.
  - Parameters:
    - `text`: The input text to convert.
  - Returns: A Result containing the title-cased text or an error.
- **Reverse** - Reverses the input string.
  - Parameters:
    - `text`: The input text to reverse.
  - Returns: A Result containing the reversed text or an error.
- **RemoveWhitespace** - Removes all whitespace characters from the input string.
  - Parameters:
    - `text`: The input text to process.
  - Returns: A Result containing the text with all whitespace removed or an error.
- **WordWrap** - Wraps text at a specified line length while preserving word boundaries.
  - Parameters:
    - `text`: The input text to wrap.
    - `lineLength`: The maximum length of each line.
    - `newline`: The newline character(s) to use. Defaults to "\n".
  - Returns: A Result containing the word-wrapped text or an error.
- **Repeat** - Repeats the input string the specified number of times.
  - Parameters:
    - `text`: The input text to repeat.
    - `count`: The number of times to repeat the text.
  - Returns: A Result containing the repeated text or an error.

### CompositeValidator`1

- **Namespace:** `SmartWorkz.Shared.CompositeValidator`1`
- **Summary:** Combines multiple validators into a single validator.
            Useful for composing validators from different sources.

### IValidationRule`2

- **Namespace:** `SmartWorkz.Shared.IValidationRule`2`
- **Summary:** Single validation rule for a property.

#### Methods & Properties

- **ValidateAsync** - Validate property and return results.

### ValidationRule`2

- **Namespace:** `SmartWorkz.Shared.ValidationRule`2`
- **Summary:** Base implementation for custom validation rules.

### ValidationRules

- **Namespace:** `SmartWorkz.Shared.ValidationRules`
- **Summary:** Pre-built validation rules for common scenarios.

### ValidatorBuilder`1

- **Namespace:** `SmartWorkz.Shared.ValidatorBuilder`1`
- **Summary:** Fluent validator builder for defining validation rules.
            Provides an alternative to ValidatorBase for more concise validator definitions.

#### Methods & Properties

- **RuleFor``1** - Add a rule for a property using fluent API.
- **ValidateAsync** - Validate instance against all rules.

### IWebhookRegistry

- **Namespace:** `SmartWorkz.Shared.IWebhookRegistry`
- **Summary:** Abstraction for managing webhook subscriptions and registrations.
            Supports CRUD operations and subscription queries.

#### Methods & Properties

- **RegisterAsync** - Register a new webhook subscription.
  - Parameters:
    - `url`: The webhook endpoint URL.
    - `events`: Array of event names to subscribe to.
    - `secret`: Optional HMAC-SHA256 secret for signature verification.
    - `cancellationToken`: Cancellation token.
  - Returns: The ID of the newly registered subscription.
- **UnregisterAsync** - Unregister and remove a webhook subscription.
  - Parameters:
    - `subscriptionId`: The subscription ID to unregister.
    - `cancellationToken`: Cancellation token.
- **GetSubscriptionsForEventAsync** - Get all active subscriptions for a specific event.
  - Parameters:
    - `eventName`: The event name to filter by.
    - `cancellationToken`: Cancellation token.
  - Returns: Collection of subscriptions interested in this event.
- **GetActiveSubscriptionsAsync** - Get all currently active subscriptions.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: Collection of all active subscriptions.
- **UpdateSubscriptionStatusAsync** - Update the status and failure tracking of a subscription.
  - Parameters:
    - `subscriptionId`: The subscription ID to update.
    - `isActive`: Whether the subscription should remain active.
    - `failureCount`: Number of consecutive failures (null to leave unchanged).
    - `failureReason`: Reason for failure (null to clear).
    - `cancellationToken`: Cancellation token.

### SqlWebhookRegistry

- **Namespace:** `SmartWorkz.Shared.SqlWebhookRegistry`
- **Summary:** SQL Server implementation of IWebhookRegistry.
            Persists webhook subscriptions to the database with support for querying and status updates.

### WebhookDeliveryService

- **Namespace:** `SmartWorkz.Shared.WebhookDeliveryService`
- **Summary:** Service for publishing domain events to registered webhook endpoints.
            Implements exponential backoff retry logic, HMAC signature verification, and failure tracking.

#### Methods & Properties

- **PublishEventAsync** - Publish an event to all subscribed webhook endpoints.
  - Parameters:
    - `eventName`: The name of the event being published.
    - `payload`: The event payload to send.
    - `cancellationToken`: Cancellation token.
- **DeliverAsync** - Deliver an event to a single webhook endpoint with exponential backoff retry logic.
- **GenerateSignature** - Generate HMAC-SHA256 signature for webhook payload verification.

### AuditEntry

- **Namespace:** `SmartWorkz.Shared.AuditEntry`
- **Summary:** Immutable audit log entry for tracking entity changes and domain events.
            Records who did what, when, where, and why for compliance and debugging.

### AuditEventSubscriber

- **Namespace:** `SmartWorkz.Shared.AuditEventSubscriber`
- **Summary:** Subscribes to domain events and records them in the audit trail.
            Enables automatic audit capture without requiring explicit audit calls in business logic.

#### Methods & Properties

- **OnEventPublishedAsync** - Record a domain event in the audit trail.
  - Parameters:
    - `evt`: The domain event to record.
    - `userId`: User ID who triggered the event (optional for system events).
    - `ipAddress`: IP address of the request originator (optional).
    - `cancellationToken`: Cancellation token.

### AuditStartupExtensions

- **Namespace:** `SmartWorkz.Shared.AuditStartupExtensions`
- **Summary:** Dependency injection and schema setup for audit trail functionality.

#### Methods & Properties

- **AddAuditTrail** - Register IAuditTrail with SQL Server implementation.
- **CreateAuditTrailSchema** - Create the AuditTrail table and indexes if they don't exist.
            Call this during application startup or migration.

### IAuditTrail

- **Namespace:** `SmartWorkz.Shared.IAuditTrail`
- **Summary:** Service for recording and querying immutable audit entries.
            Abstracts the persistence mechanism for audit trails.

#### Methods & Properties

- **RecordAsync** - Record an audit entry (immutable append-only).
  - Parameters:
    - `entry`: The audit entry to record.
    - `cancellationToken`: Cancellation token.
- **GetEntriesAsync** - Get all audit entries for a specific entity instance.
  - Parameters:
    - `entityType`: Type of entity (e.g., "Order").
    - `entityId`: Entity instance ID.
    - `cancellationToken`: Cancellation token.
- **GetEntriesByActionAsync** - Get audit entries by action type (Created, Updated, Deleted, etc.).
  - Parameters:
    - `action`: The action to filter by.
    - `since`: Optional: only entries after this date.
    - `cancellationToken`: Cancellation token.
- **GetEntriesByUserAsync** - Get audit entries for a specific user.
  - Parameters:
    - `userId`: User ID who performed actions.
    - `since`: Optional: only entries after this date.
    - `cancellationToken`: Cancellation token.
- **SearchAsync** - Search audit trail with multiple filter criteria.
            All criteria are AND'd together (null criteria are ignored).
  - Parameters:
    - `entityType`: Optional entity type filter.
    - `action`: Optional action filter.
    - `userId`: Optional user ID filter.
    - `since`: Optional timestamp filter (inclusive).
    - `cancellationToken`: Cancellation token.

### SqlAuditTrail

- **Namespace:** `SmartWorkz.Shared.SqlAuditTrail`
- **Summary:** SQL Server implementation of IAuditTrail for immutable audit log persistence.
            Appends audit entries to a single table with indexes for efficient querying.

#### Methods & Properties

- **RecordAsync** - 
- **GetEntriesAsync** - 
- **GetEntriesByActionAsync** - 
- **GetEntriesByUserAsync** - 
- **SearchAsync** - 

### ValueConverter`1

- **Namespace:** `SmartWorkz.Shared.ValueConverter`1`
- **Summary:** Abstract base class for type conversion between domain objects and DTOs.
            Enables loose coupling between layers by centralizing conversion logic.

#### Methods & Properties

- **Convert``1** - Convert a single source object to target type.
- **Convert** - Convert a single source object using dynamic target type resolution.
- **ConvertList``1** - Convert a collection of source objects to target type.
- **ConvertList** - Convert a collection using dynamic target type resolution.
- **ConvertFromList``2** - Convert from a collection of different source types.

### CacheEntry`1

- **Namespace:** `SmartWorkz.Shared.CacheEntry`1`
- **Summary:** Represents a cached entry with data, expiration time, and metadata.

#### Methods & Properties

- **#ctor** - Creates a new CacheEntry instance.
- **#ctor** - Creates a new CacheEntry instance with data and expiration.
- **RenewExpiry** - Renews the expiry time based on the cache strategy and TTL.

### CacheEntryWrapper

- **Namespace:** `SmartWorkz.Shared.CacheEntryWrapper`
- **Summary:** Non-generic wrapper for CacheEntry to store in the cache dictionary.

### CacheOptions

- **Namespace:** `SmartWorkz.Shared.CacheOptions`
- **Summary:** Configuration options for cache operations.

#### Methods & Properties

- **#ctor** - Creates a new CacheOptions instance with default values.
- **#ctor** - Creates a new CacheOptions instance with specified TTL.
- **#ctor** - Creates a new CacheOptions instance with specified TTL and cache strategy.
- **#ctor** - Creates a new CacheOptions instance with all parameters.

### CacheStrategy

- **Namespace:** `SmartWorkz.Shared.CacheStrategy`
- **Summary:** Enumeration of cache expiration strategies.

### ICacheService

- **Namespace:** `SmartWorkz.Shared.ICacheService`
- **Summary:** Service for caching with tenant isolation and L1/L2 hybrid support.
            Implementations may use memory cache (L1) and distributed cache (L2).
            All cache operations are tenant-scoped with automatic key prefixing.

#### Methods & Properties

- **GetAsync``1** - Gets a cached value by key with tenant isolation.
  - Parameters:
    - `key`: Cache key (will be tenant-scoped automatically).
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Result containing cached value, or failure if not found or expired.
- **SetAsync``1** - Sets a value in cache with optional TTL for the specified tenant.
  - Parameters:
    - `key`: Cache key (will be tenant-scoped automatically).
    - `value`: Value to cache.
    - `ttlMinutes`: Time to live in minutes. If null, value never expires.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveAsync** - Removes a cached value by key for the specified tenant.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveByPrefixAsync** - Removes all cached values matching a key prefix for the specified tenant.
            Example: RemoveByPrefixAsync("user:") removes all "user:*" entries for that tenant.
  - Parameters:
    - `prefix`: Key prefix to match (may include wildcard suffix like "user:*").
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **ExistsAsync** - Checks if a key exists in cache for the specified tenant.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: True if key exists and is not expired; false otherwise.
- **ClearAsync** - Clears all cache entries for the specified tenant.
            Does not affect entries for other tenants.
  - Parameters:
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.

### ICacheStore

- **Namespace:** `SmartWorkz.Shared.ICacheStore`
- **Summary:** Abstraction for a cache store with support for various operations including TTL and expiration strategies.

#### Methods & Properties

- **GetAsync``1** - Retrieves a value from the cache.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result containing the cached value or null if not found or expired.
- **SetAsync``1** - Sets a value in the cache with optional TTL.
  - Parameters:
    - `key`: The cache key.
    - `value`: The value to cache.
    - `ttlMinutes`: Optional time-to-live in minutes. If null, uses default or no expiration.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **SetAsync``1** - Sets a value in the cache with cache options.
  - Parameters:
    - `key`: The cache key.
    - `value`: The value to cache.
    - `options`: Cache options including TTL, strategy, and sliding expiration.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **RemoveAsync** - Removes a value from the cache.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **RemoveByPrefixAsync** - Removes all values from the cache that match the specified key prefix.
  - Parameters:
    - `keyPrefix`: The prefix to match.
    - `ct`: Cancellation token.
  - Returns: A Result containing the number of entries removed.
- **ExistsAsync** - Checks if a key exists in the cache and is not expired.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result indicating whether the key exists and is valid.
- **ClearAsync** - Clears all entries from the cache.
  - Parameters:
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.

### MemoryCacheService

- **Namespace:** `SmartWorkz.Shared.MemoryCacheService`
- **Summary:** In-memory L1 cache service implementation with thread-safe operations and tenant isolation.
            Suitable for single-process deployments with TTL and expiration support.

#### Methods & Properties

- **BuildKey** - Builds a tenant-scoped cache key.
  - Parameters:
    - `key`: Original cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
  - Returns: Tenant-scoped key in format "{tenantId}:{key}".
- **GetAsync``1** - Gets a cached value by key with tenant isolation. Returns failure if not found or expired.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Result containing cached value, or failure if not found or expired.
- **SetAsync``1** - Sets a cached value with optional TTL expiration and tenant isolation.
  - Parameters:
    - `key`: Cache key.
    - `value`: Value to cache.
    - `ttlMinutes`: Time to live in minutes. If null, no expiration.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveAsync** - Removes a single cache entry with tenant isolation.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveByPrefixAsync** - Removes all cache entries matching a prefix pattern with tenant isolation.
            Example: RemoveByPrefixAsync("user:*", "tenant1") removes "tenant1:user:*" entries.
  - Parameters:
    - `prefix`: Key prefix to match.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **ExistsAsync** - Checks if a key exists in the cache with tenant isolation (ignores expiration check).
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: True if key exists and is not expired; false otherwise.
- **ClearAsync** - Clears all cache entries for the specified tenant (or "default" if not specified).
            Does not clear entries from other tenants.
  - Parameters:
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.

### MemoryCacheStore

- **Namespace:** `SmartWorkz.Shared.MemoryCacheStore`
- **Summary:** In-memory implementation of ICacheStore with TTL support and thread-safe operations.

#### Methods & Properties

- **#ctor** - Creates a new instance of MemoryCacheStore with default options.
- **#ctor** - Creates a new instance of MemoryCacheStore with specified default options.
- **GetAsync``1** - Retrieves a value from the cache.
- **SetAsync``1** - Sets a value in the cache with optional TTL.
- **SetAsync``1** - Sets a value in the cache with cache options.
- **RemoveAsync** - Removes a value from the cache.
- **RemoveByPrefixAsync** - Removes all values from the cache that match the specified key prefix.
- **ExistsAsync** - Checks if a key exists in the cache and is not expired.
- **ClearAsync** - Clears all entries from the cache.
- **CleanupExpiredEntries** - Performs cleanup of expired entries. This is useful for periodic maintenance.

### ISmsService

- **Namespace:** `SmartWorkz.Shared.ISmsService`
- **Summary:** Defines a contract for SMS communication services.
            Provides methods for sending SMS messages to single or multiple recipients.

#### Methods & Properties

- **SendAsync** - Sends an SMS message to a single recipient.
  - Parameters:
    - `phoneNumber`: The recipient phone number (E.164 format recommended)
    - `message`: The SMS message content
    - `cancellationToken`: Cancellation token
  - Returns: Result containing the SMS ID if successful
- **SendBatchAsync** - Sends an SMS message to multiple recipients (batch).
  - Parameters:
    - `phoneNumbers`: Collection of recipient phone numbers
    - `message`: The SMS message content sent to all recipients
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of SMS IDs if successful

### IWebSocketClient

- **Namespace:** `SmartWorkz.Shared.IWebSocketClient`
- **Summary:** Abstraction for WebSocket client operations.

#### Methods & Properties

- **SendAsync** - Sends a message asynchronously through the WebSocket connection.
- **ReceiveAsync** - Receives a message asynchronously from the WebSocket connection.
            Returns null if the connection is closed.
- **CloseAsync** - Closes the WebSocket connection gracefully.

### WebSocketClient

- **Namespace:** `SmartWorkz.Shared.WebSocketClient`
- **Summary:** Sealed implementation of IWebSocketClient using System.Net.WebSockets.

#### Methods & Properties

- **ConnectAsync** - Connects to a WebSocket server at the specified URI.
- **SendAsync** - Sends a message asynchronously through the WebSocket connection.
- **ReceiveAsync** - Receives a message asynchronously from the WebSocket connection.
            Returns null if the connection is closed.
- **CloseAsync** - Closes the WebSocket connection gracefully.

### ConfigurationHelper

- **Namespace:** `SmartWorkz.Shared.ConfigurationHelper`
- **Summary:** Provides typed access to configuration values with automatic type conversion and validation.
            
             This sealed class implements IConfigurationHelper to provide a strongly-typed interface
             for accessing configuration values. It supports automatic type conversion for common types
             including strings, numeric types, booleans, DateTimes, and enums.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ConfigurationHelper class.
  - Parameters:
    - `configuration`: The configuration source to read from.
- **GetRequired``1** - Gets a required configuration value and converts it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: The configuration value converted to type T.
- **GetOptional``1** - Gets an optional configuration value and converts it to the specified type,
            returning a default value if the key is not found or conversion fails.
  - Parameters:
    - `key`: The configuration key to retrieve.
    - `defaultValue`: The value to return if the key is not found or conversion fails.
  - Returns: The configuration value converted to type T, or the default value if retrieval or conversion fails.
- **TryGet``1** - Attempts to get a configuration value and convert it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: A Result<T> containing the converted value on success, or a failure result
            if the key is not found or conversion fails.
- **Exists** - Checks whether a configuration key exists and is not empty.
  - Parameters:
    - `key`: The configuration key to check.
  - Returns: True if the key exists and is not null or whitespace; otherwise false.
- **ConvertValue``1** - Converts a string value to the specified type using invariant culture for numeric types.
  - Parameters:
    - `value`: The string value to convert.
  - Returns: The converted value of type T.

### ConfigurationValidationException

- **Namespace:** `SmartWorkz.Shared.ConfigurationValidationException`
- **Summary:** Exception thrown when configuration validation fails, indicating that a required
            configuration key is missing, empty, or cannot be converted to the requested type.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ConfigurationValidationException class with a specified message.
  - Parameters:
    - `message`: The error message that explains the reason for the exception.
- **#ctor** - Initializes a new instance of the ConfigurationValidationException class with a specified message
            and a reference to the inner exception that is the cause of this exception.
  - Parameters:
    - `message`: The error message that explains the reason for the exception.
    - `innerException`: The exception that is the cause of the current exception.

### IConfigurationHelper

- **Namespace:** `SmartWorkz.Shared.IConfigurationHelper`
- **Summary:** Provides typed access to configuration values with automatic type conversion and validation.

#### Methods & Properties

- **GetRequired``1** - Gets a required configuration value and converts it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: The configuration value converted to type T.
- **GetOptional``1** - Gets an optional configuration value and converts it to the specified type,
            returning a default value if the key is not found or conversion fails.
  - Parameters:
    - `key`: The configuration key to retrieve.
    - `defaultValue`: The value to return if the key is not found or conversion fails.
  - Returns: The configuration value converted to type T, or the default value if retrieval or conversion fails.
- **TryGet``1** - Attempts to get a configuration value and convert it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: A Result<T> containing the converted value on success, or a failure result
            if the key is not found or conversion fails.
- **Exists** - Checks whether a configuration key exists and is not empty.
  - Parameters:
    - `key`: The configuration key to check.
  - Returns: True if the key exists and is not null or whitespace; otherwise false.

### SharedConstants

- **Namespace:** `SmartWorkz.Shared.SharedConstants`
- **Summary:** Shared configuration constants used throughout SmartWorkz.Shared.
            Enables centralized management of default values and limits.

### ICommand

- **Namespace:** `SmartWorkz.Shared.ICommand`
- **Summary:** Marker interface for command objects representing intent to change state.

### ICommandHandler`1

- **Namespace:** `SmartWorkz.Shared.ICommandHandler`1`
- **Summary:** Handler for processing a specific command type.

#### Methods & Properties

- **HandleAsync** - Handles the specified command asynchronously.
  - Parameters:
    - `command`: The command to handle.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task that represents the asynchronous handle operation.

### IQuery`1

- **Namespace:** `SmartWorkz.Shared.IQuery`1`
- **Summary:** Marker interface for query objects that return a result without modifying state.

### IQueryHandler`2

- **Namespace:** `SmartWorkz.Shared.IQueryHandler`2`
- **Summary:** Handler for processing a specific query type and returning results.

#### Methods & Properties

- **HandleAsync** - Handles the specified query asynchronously and returns the result.
  - Parameters:
    - `query`: The query to handle.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task that represents the asynchronous handle operation and contains the query result.

### MediatorCommandDispatcher

- **Namespace:** `SmartWorkz.Shared.MediatorCommandDispatcher`
- **Summary:** Routes commands to their appropriate handlers via dependency injection.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `serviceProvider`: The service provider for resolving handlers.
- **DispatchAsync``1** - Dispatches the specified command to its handler asynchronously.
  - Parameters:
    - `command`: The command to dispatch.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task representing the asynchronous dispatch operation.

### AdoHelper

- **Namespace:** `SmartWorkz.Shared.AdoHelper`
- **Summary:** ADO.NET helper for executing queries and managing connections.
            Works with any IDbProvider implementation.

#### Methods & Properties

- **ExecuteScalarAsync``1** - Execute scalar query (returns single value).
- **ExecuteNonQueryAsync** - Execute non-query command (INSERT, UPDATE, DELETE).
- **ExecuteQueryAsync``1** - Execute query and map results to objects.
- **ExecuteStoredProcedureAsync** - Execute stored procedure.
- **ExecuteQueryMultipleAsync``2** - Execute query returning multiple result sets (2 sets).
- **ExecuteQueryMultipleAsync``3** - Execute query returning multiple result sets (3 sets).
- **ExecuteQueryMultipleAsync``4** - Execute query returning multiple result sets (4 sets).
- **ExecuteTransactionAsync** - Execute transaction with multiple commands.

### CsvHelper

- **Namespace:** `SmartWorkz.Shared.CsvHelper`
- **Summary:** Provides static methods for reading and writing CSV data with support for column mapping,
            quoted fields, embedded delimiters, and newlines.
            RFC 4180 compliant CSV parsing and writing.
            Sealed class to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **CsvWriter``1** - Serializes a collection of objects to CSV format.
  - Parameters:
    - `items`: The collection of objects to serialize.
    - `options`: CSV options. If null, defaults are used.
  - Returns: A Result containing the CSV string if successful; otherwise a failure.
- **CsvReader``1** - Asynchronously deserializes CSV content to a collection of objects.
  - Parameters:
    - `content`: The CSV content string.
    - `mapping`: Column mapping configuration. If null, property names are used as headers.
    - `options`: CSV options. If null, defaults are used.
  - Returns: A Result containing the deserialized list if successful; otherwise a failure.
- **ParseCsvLines** - Parses CSV content into a list of records (each record is a list of field values).
            Handles quoted fields with embedded delimiters and newlines.
- **WriteRecord** - Writes a single CSV record (list of field values) to the string builder.
            Handles quoting of fields with special characters.
- **ConvertValue** - Converts a string value to the specified type.
- **IsNullableType** - Determines if a type is nullable (Nullable<T> or reference type).

### CsvMapping`1

- **Namespace:** `SmartWorkz.Shared.CsvMapping`1`
- **Summary:** Defines column mapping for CSV operations using a fluent API.
            Supports mapping object properties to CSV columns with custom headers.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **Column``1** - Adds a column mapping for the specified property.
  - Parameters:
    - `propertyExpression`: Expression selecting the property to map.
    - `csvHeader`: The CSV column header name.
  - Returns: This instance for method chaining.
- **ExtractPropertyInfo``1** - Extracts property information from a lambda expression.
  - Parameters:
    - `expression`: The lambda expression.
  - Returns: The PropertyInfo if the expression resolves to a property; otherwise null.
- **CreateAuto** - Creates a mapping automatically from all public properties of type T.
            Property names are used as CSV headers.
  - Returns: A new CsvMapping instance with all properties mapped.

### CsvOptions

- **Namespace:** `SmartWorkz.Shared.CsvOptions`
- **Summary:** Configuration options for CSV read/write operations.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **#ctor** - Creates a default instance of CsvOptions.
- **#ctor** - Creates an instance of CsvOptions with specified delimiter and quote character.
  - Parameters:
    - `delimiter`: The field delimiter character.
    - `quoteChar`: The quote character for quoted fields.

### DbProviderFactory

- **Namespace:** `SmartWorkz.Shared.DbProviderFactory`
- **Summary:** Factory for creating database provider instances.
            Resolves provider name from connection string or explicit specification.

#### Methods & Properties

- **Register** - Register custom provider implementation.
- **GetProvider** - Get provider by name.
- **GetProvider** - Get provider by enum value.
- **GetProviderFromConnectionString** - Get provider from connection string (detects provider automatically).

### IDbProvider

- **Namespace:** `SmartWorkz.Shared.IDbProvider`
- **Summary:** Abstraction for database provider-specific operations.
            Supports multiple providers: SQL Server, MySQL, PostgreSQL, SQLite, Oracle.

#### Methods & Properties

- **CreateConnection** - Create connection with connection string.
- **GetParameterPrefix** - Get parameter prefix for this provider (@, :, $).
- **GetLastInsertIdSql** - Get SQL for last inserted ID based on provider.
- **GetPaginationSql** - Get SQL for pagination based on provider.
- **FormatIdentifier** - Format table/column name for provider (e.g., [brackets] for SQL Server).
- **TestConnectionAsync** - Test connection validity.

### DatabaseProvider

- **Namespace:** `SmartWorkz.Shared.DatabaseProvider`
- **Summary:** Enum of supported database providers.

### QueryMultipleHelper

- **Namespace:** `SmartWorkz.Shared.QueryMultipleHelper`
- **Summary:** Helper for executing multiple queries in a single database roundtrip.
            Eliminates N+1 query problems by batching queries together.

#### Methods & Properties

- **QueryMultipleAsync``2** - Execute multiple queries and return results as tuple.
             Single roundtrip, single SQL execution, improved performance.
- **QueryMultipleAsync``3** - Execute 3 queries in single roundtrip.
- **QueryMultipleAsync``4** - Execute 4 queries in single roundtrip.
- **QueryMultipleAsync``5** - Execute 5 queries in single roundtrip.

### QueryResult`1

- **Namespace:** `SmartWorkz.Shared.QueryResult`1`
- **Summary:** Result wrapper for query operations.

### XmlHelper

- **Namespace:** `SmartWorkz.Shared.XmlHelper`
- **Summary:** Provides static methods for XML serialization, deserialization, and XPath queries.
            Uses System.Xml.Linq for manipulation and reflection for property mapping.
            Sealed class to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **Serialize``1** - Serializes an object to an XML string using reflection.
  - Parameters:
    - `obj`: The object to serialize.
    - `options`: XML options. If null, defaults are used.
  - Returns: A Result containing the XML string if successful; otherwise a failure.
- **Deserialize``1** - Deserializes an XML string to an object of type T using reflection.
  - Parameters:
    - `xml`: The XML string to deserialize.
    - `options`: XML options. If null, defaults are used.
  - Returns: A Result containing the deserialized object if successful; otherwise a failure.
- **Query** - Executes an XPath query on an XML string and returns matching element values.
  - Parameters:
    - `xml`: The XML string to query.
    - `xpathExpression`: The XPath expression to execute.
  - Returns: A Result containing a list of matched values if successful; otherwise a failure.
- **SerializeObject** - Recursively serializes an object's properties into an XML element.
- **DeserializeObject** - Recursively deserializes an XML element into an object's properties.
- **IsBasicType** - Determines if a type is a basic/primitive type supported by XML.
- **IsGenericList** - Determines if a type is a generic List<T>.
- **IsComplexType** - Determines if a type is a complex (non-primitive) type.
- **ConvertToXmlValue** - Converts a value to its XML-safe string representation.
- **ConvertFromXmlValue** - Converts an XML string value to the specified type.

### XmlOptions

- **Namespace:** `SmartWorkz.Shared.XmlOptions`
- **Summary:** Configuration options for XML serialization, deserialization, and query operations.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **#ctor** - Creates a default instance of XmlOptions.
- **#ctor** - Creates an instance of XmlOptions with a specified root element name.
  - Parameters:
    - `rootElement`: The name of the root element.
- **#ctor** - Creates an instance of XmlOptions with specified configuration.
  - Parameters:
    - `rootElement`: The name of the root element.
    - `includeXmlDeclaration`: Whether to include the XML declaration.
    - `indent`: Whether to indent the output.

### ApplicationHealth

- **Namespace:** `SmartWorkz.Shared.ApplicationHealth`
- **Summary:** Represents the overall health status of the application.

### CorrelationContext

- **Namespace:** `SmartWorkz.Shared.CorrelationContext`
- **Summary:** A sealed implementation of  for distributed request tracing.

#### Methods & Properties

- **#ctor** - Initializes a new instance with a generated correlation ID.
- **#ctor** - Initializes a new instance with a specified correlation ID.
  - Parameters:
    - `correlationId`: The correlation ID to use
- **#ctor** - Initializes a child context from a parent context.

### CpuUsage

- **Namespace:** `SmartWorkz.Shared.CpuUsage`
- **Summary:** Represents CPU usage information.

### DiagnosticsHelper

- **Namespace:** `SmartWorkz.Shared.DiagnosticsHelper`
- **Summary:** Sealed helper class for system diagnostics and application health monitoring.
            Provides methods to gather system information, CPU/memory/disk usage, and determine application health.

#### Methods & Properties

- **Initialize** - Initializes the application start time (called once at application startup).
- **GetSystemInfo** - Gets comprehensive system information including CPU, memory, disk, and processor count.
  - Returns: A Result containing SystemInfo or error details.
- **GetMemoryUsage** - Gets memory usage statistics for the current process and system.
  - Returns: A Result containing MemoryUsage or error details.
- **GetCpuUsage** - Gets CPU utilization percentage.
  - Returns: A Result containing CpuUsage or error details.
- **GetDiskSpace** - Gets disk space information for a specific drive.
  - Parameters:
    - `drive`: The drive letter (e.g., "C:", "D:"). Defaults to "C:".
  - Returns: A Result containing DiskSpace or error details.
- **GetUptime** - Gets the application uptime since the last Initialize() call or application start.
  - Returns: A Result containing the uptime as a TimeSpan or error details.
- **GetApplicationHealth** - Gets the overall health status of the application based on system metrics.
  - Returns: A Result containing ApplicationHealth or error details.
- **IsHealthy** - Determines if the application is considered healthy based on the provided health status.
  - Parameters:
    - `health`: The ApplicationHealth object to evaluate.
  - Returns: True if the status is Healthy, false otherwise.
- **InitializeCpuCounter** - Initializes the CPU performance counter (called once).
- **GetMemoryUsageInternal** - Internal method to get memory usage statistics.
- **GetCpuUsageInternal** - Internal method to get CPU usage percentage.
- **GetDiskSpaceInternal** - Internal method to get disk space information.

### DiskSpace

- **Namespace:** `SmartWorkz.Shared.DiskSpace`
- **Summary:** Represents disk space information for a drive.

### HealthCheck

- **Namespace:** `SmartWorkz.Shared.HealthCheck`
- **Summary:** Represents a single health check result.

### HealthStatus

- **Namespace:** `SmartWorkz.Shared.HealthStatus`
- **Summary:** Represents the health status of the application.

### ICorrelationContext

- **Namespace:** `SmartWorkz.Shared.ICorrelationContext`
- **Summary:** Defines a correlation context for distributed request tracing across systems.

#### Methods & Properties

- **SetProperty** - Adds or updates a property in the correlation context.
- **TryGetProperty** - Attempts to retrieve a property from the correlation context.
- **CreateChildContext** - Creates a child correlation context for nested operations (for async/distributed flows).

### MemoryUsage

- **Namespace:** `SmartWorkz.Shared.MemoryUsage`
- **Summary:** Represents memory usage information.

### MetricsHelper

- **Namespace:** `SmartWorkz.Shared.MetricsHelper`
- **Summary:** Provides utilities for collecting and tracking performance metrics.

#### Methods & Properties

- **StartTimer** - Starts a timer and returns an IDisposable that logs elapsed time on disposal.
- **TrackExecution``1** - Tracks the execution time and result of a function.
- **MeasureMemory** - Captures memory usage before and after a block of code execution.

### SystemInfo

- **Namespace:** `SmartWorkz.Shared.SystemInfo`
- **Summary:** Represents system information including CPU, memory, and disk details.

### EventStoreSnapshot

- **Namespace:** `SmartWorkz.Shared.EventStoreSnapshot`
- **Summary:** Represents a snapshot of an aggregate's state at a specific version.
            Snapshots optimize event sourcing by reducing the number of events needed for reconstruction.

### IEventStore

- **Namespace:** `SmartWorkz.Shared.IEventStore`
- **Summary:** Abstraction for an immutable event store that persists domain events.
            Enables event sourcing patterns for temporal queries, audit trails, and event replay.

#### Methods & Properties

- **AppendEventsAsync** - Appends events to the event stream for an aggregate.
            Events are immutable and persist as an append-only log.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `events`: The domain events to append
    - `cancellationToken`: Cancellation token
  - Returns: Task representing the asynchronous operation
- **GetEventsAsync** - Retrieves all events for an aggregate in chronological order.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: Collection of domain events for the aggregate, empty if none exist
- **GetEventsSinceAsync** - Retrieves events for an aggregate after a specific version.
            Useful for incremental event replay and event streaming.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `version`: The version after which to retrieve events
    - `cancellationToken`: Cancellation token
  - Returns: Collection of events after the specified version, empty if none exist
- **GetSnapshotAsync** - Retrieves the latest snapshot for an aggregate if one exists.
            Snapshots optimize aggregate reconstruction by storing intermediate state.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: Snapshot data if exists; null if no snapshot is available
- **SaveSnapshotAsync** - Saves a snapshot of aggregate state at a specific version.
            Snapshots reduce the number of events needed to replay an aggregate.
  - Parameters:
    - `snapshot`: The snapshot to save
    - `cancellationToken`: Cancellation token
  - Returns: Task representing the asynchronous operation
- **GetAggregateAsync``1** - Reconstructs an aggregate from its event history.
            Optionally uses snapshots for performance optimization.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: The reconstructed aggregate instance, or null if no events exist

### SqlEventStore

- **Namespace:** `SmartWorkz.Shared.SqlEventStore`
- **Summary:** SQL Server implementation of the event store using Dapper for data access.
            Provides immutable append-only event log with snapshot support for optimization.
            Implements optimistic concurrency control using version numbers.

#### Methods & Properties

- **AppendEventsAsync** - Appends events to the event stream for an aggregate.
- **GetEventsAsync** - Retrieves all events for an aggregate in chronological order.
- **GetEventsSinceAsync** - Retrieves events for an aggregate after a specific version.
- **GetSnapshotAsync** - Retrieves the latest snapshot for an aggregate if one exists.
- **SaveSnapshotAsync** - Saves a snapshot of aggregate state at a specific version.
- **GetAggregateAsync``1** - Reconstructs an aggregate from its event history.
            Optionally uses snapshots for performance optimization.
- **GetCurrentVersionAsync** - Gets the current version number for an aggregate.
- **DeserializeEvent** - Deserializes a stored event record back to IDomainEvent.

### IDomainEvent

- **Namespace:** `SmartWorkz.Shared.IDomainEvent`
- **Summary:** Base interface for domain events in event-driven architecture.
            Provides core event metadata for tracking and publishing.

### IEventPublisher

- **Namespace:** `SmartWorkz.Shared.IEventPublisher`
- **Summary:** Publishes domain events for event-driven architecture.

#### Methods & Properties

- **PublishAsync``1** - Publishes a single domain event.
  - Parameters:
    - `event`: Event to publish.
    - `cancellationToken`: Cancellation token.
- **PublishAsync``1** - Publishes multiple domain events.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token.

### IEventSubscriber

- **Namespace:** `SmartWorkz.Shared.IEventSubscriber`
- **Summary:** Registers event handlers for domain events.

#### Methods & Properties

- **Subscribe``1** - Subscribes a handler to an event type.
            Multiple handlers can subscribe to the same event.
  - Parameters:
    - `handler`: Async handler function. Receives event and cancellation token.

### InMemoryEventPublisher

- **Namespace:** `SmartWorkz.Shared.InMemoryEventPublisher`
- **Summary:** In-memory event publisher that executes all registered handlers sequentially.
            Provides synchronous event delivery with exception handling and result reporting.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the InMemoryEventPublisher with a subscriber.
  - Parameters:
    - `subscriber`: The event subscriber containing registered handlers.
- **PublishAsync``1** - Publishes a single domain event to all registered handlers.
            Handlers are invoked sequentially in registration order.
            If any handler throws an exception, it is caught and a failure Result is returned.
            Other handlers will attempt to execute even if a previous handler fails.
  - Parameters:
    - `event`: The event instance to publish.
    - `cancellationToken`: Cancellation token to cancel handler execution.
  - Returns: A task representing the asynchronous operation.
- **PublishAsync``1** - Publishes multiple domain events to all registered handlers.
            Events are published sequentially in the order provided.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token to cancel handler execution.
  - Returns: A task representing the asynchronous batch operation.

### InMemoryEventSubscriber

- **Namespace:** `SmartWorkz.Shared.InMemoryEventSubscriber`
- **Summary:** In-memory event subscriber that maintains a registry of event handlers.
            Supports multiple handlers per event type using thread-safe concurrent collections.

#### Methods & Properties

- **Subscribe``1** - Subscribes a handler to an event type.
            Multiple handlers can be registered for the same event type and will execute sequentially.
  - Parameters:
    - `handler`: Async handler function that receives event and cancellation token.
- **GetHandlers** - Gets all registered handlers for a given event type.
            Returns an empty list if no handlers are registered for the type.
  - Parameters:
    - `eventType`: The event type to retrieve handlers for.
  - Returns: List of registered handlers (delegates).

### MassTransitEventPublisher

- **Namespace:** `SmartWorkz.Shared.MassTransitEventPublisher`
- **Summary:** Distributed event publisher using MassTransit message bus.
            Supports both single and batch event publishing with async/await patterns.
            Suitable for production environments with message broker backend (RabbitMQ, Azure Service Bus, etc).

#### Methods & Properties

- **#ctor** - Initializes a new instance of MassTransitEventPublisher.
  - Parameters:
    - `publishEndpoint`: MassTransit publish endpoint for message distribution.
    - `logger`: Logger for event publication tracking.
- **PublishAsync``1** - Publishes a single domain event to the message bus asynchronously.
  - Parameters:
    - `event`: Event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous publish operation.
- **PublishAsync``1** - Publishes multiple domain events to the message bus asynchronously.
            Events are published sequentially in the order provided.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous batch publish operation.

### PublisherType

- **Namespace:** `SmartWorkz.Shared.PublisherType`
- **Summary:** Specifies the publisher type for event publishing.

### ServiceCollectionExtensions

- **Namespace:** `SmartWorkz.Shared.ServiceCollectionExtensions`
- **Summary:** Extension methods for IServiceCollection to register Core.Shared services.

#### Methods & Properties

- **AddCoreSharedServices** - Adds Core.Shared services including TemplateEngine for template rendering.
- **AddEventPublishing** - Adds event publishing services to the dependency injection container.
            Supports switching between in-memory and MassTransit publishers based on application needs.
  - Parameters:
    - `services`: The service collection.
    - `publisherType`: The publisher type to use (defaults to InMemory).
  - Returns: The service collection for method chaining.

### DefaultFeatureFlagService

- **Namespace:** `SmartWorkz.Shared.DefaultFeatureFlagService`
- **Summary:** Global (non-tenant) feature flag service with in-memory storage.
            Thread-safe implementation suitable for single-process deployments.
            Use for organization-wide feature toggles; use ITenantFeatureFlags for tenant-scoped flags.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature flag is enabled.
            Returns false for unknown flags (does not throw).
  - Parameters:
    - `flagName`: The name of the feature flag to check.
    - `cancellationToken`: Cancellation token.
  - Returns: True if the flag exists and is enabled; false otherwise.
- **GetEnabledFeaturesAsync** - Gets all enabled feature flags.
            Returns empty list if no flags are enabled.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: A read-only list of enabled feature flag names.
- **EnableFlag** - Enables a feature flag.
            Creates the flag if it doesn't exist.
  - Parameters:
    - `flagName`: The name of the feature flag to enable.
- **DisableFlag** - Disables a feature flag.
            Creates the flag as disabled if it doesn't exist.
  - Parameters:
    - `flagName`: The name of the feature flag to disable.

### IFeatureFlagService

- **Namespace:** `SmartWorkz.Shared.IFeatureFlagService`
- **Summary:** Global feature flag service for cross-tenant feature control.
            Use for organization-wide feature toggles (not tenant-specific).
            For tenant-scoped flags, use ITenantFeatureFlags.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a global feature is enabled.
  - Parameters:
    - `flagName`: Feature flag name (e.g., "NEW_DASHBOARD", "BETA_REPORTING").
    - `cancellationToken`: Cancellation token.
  - Returns: True if feature is enabled globally.
- **GetEnabledFeaturesAsync** - Gets all enabled global features.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: List of enabled feature flag names.

### IFileStorageService

- **Namespace:** `SmartWorkz.Shared.IFileStorageService`
- **Summary:** Interface for file storage operations supporting both local and cloud providers.

#### Methods & Properties

- **UploadAsync** - Uploads a file to storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `content`: The file content stream.
    - `metadata`: The file metadata.
    - `cancellationToken`: The cancellation token.
  - Returns: The full path or URI of the uploaded file.
- **DownloadAsync** - Downloads a file from storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: A stream containing the file content. Caller must dispose using 'using' statement.
- **DeleteAsync** - Deletes a file from storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
- **ExistsAsync** - Checks if a file exists in storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: True if the file exists, false otherwise.
- **GetMetadataAsync** - Gets metadata for a file in storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: FileMetadata if file exists, null otherwise.
- **ListAsync** - Lists files in a directory or container prefix.
  - Parameters:
    - `folderPath`: The relative folder path or blob prefix.
    - `cancellationToken`: The cancellation token.
  - Returns: A read-only collection of FileMetadata for files in the directory/prefix.
- **GenerateTemporaryUrlAsync** - Generates a temporary download URL for a file.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `expiration`: The expiration duration from now.
    - `cancellationToken`: The cancellation token.
  - Returns: A URL that can be used to download the file. For local storage, returns the full file path.

### GridColumn

- **Namespace:** `SmartWorkz.Shared.GridColumn`
- **Summary:** Defines a single column in a grid, including display options, sorting, filtering, and rendering hints.

### GridExportOptions

- **Namespace:** `SmartWorkz.Shared.GridExportOptions`
- **Summary:** Configuration for grid data export (CSV, Excel).

### GridRequest

- **Namespace:** `SmartWorkz.Shared.GridRequest`
- **Summary:** Request parameters for grid data fetching, extending PagedQuery with filtering support.

#### Methods & Properties

- **#ctor** - Request parameters for grid data fetching, extending PagedQuery with filtering support.

### GridResponse`1

- **Namespace:** `SmartWorkz.Shared.GridResponse`1`
- **Summary:** Response from a grid data request, including paged data, column metadata, and filter options.

### IGridDataProvider

- **Namespace:** `SmartWorkz.Shared.IGridDataProvider`
- **Summary:** Abstraction for grid data fetching. Implementations handle API calls or in-memory queries.
            Enables platform independence: Web uses HTTP, MAUI uses direct API client, Desktop uses local DB.

#### Methods & Properties

- **GetDataAsync``1** - Fetch paged grid data based on request (sorting, filtering, pagination).
  - Parameters:
    - `request`: Grid request with sorting, paging, and filter criteria.
    - `cancellationToken`: Cancellation token for async operations.
  - Returns: Result containing GridResponse or error details.

### Guard

- **Namespace:** `SmartWorkz.Shared.Guard`
- **Summary:** Static guard clauses for argument validation at method entry points.
             Throw immediately on invalid input — fail fast, fail loudly.
            
             Usage:
               Guard.NotNull(userId, nameof(userId));
               Guard.NotEmpty(name, nameof(name));
               Guard.InRange(pageSize, 1, 100, nameof(pageSize));
            
             These replace the ValidationExtensions.EnsureNotNull() extension method
             and the scattered ArgumentNullException throws throughout the codebase.

#### Methods & Properties

- **NotNull``1** - Throws ArgumentNullException if value is null.
- **NotNull``1** - Throws ArgumentNullException if value is null (struct/nullable).
- **NotEmpty** - Throws ArgumentException if string is null, empty, or whitespace.
- **NotEmpty``1** - Throws ArgumentException if collection is null or has no elements.
- **NotDefault``1** - Throws ArgumentException if value equals the default for its type (0, null, Guid.Empty).
- **InRange``1** - Throws ArgumentOutOfRangeException if value is outside [min, max].
- **Requires** - Throws ArgumentException if condition is false.

### EncryptionHelper

- **Namespace:** `SmartWorkz.Shared.EncryptionHelper`
- **Summary:** Cryptographic utilities for hashing and encryption.
            Uses PBKDF2 for password hashing and AES-256 for data encryption.

#### Methods & Properties

- **HashPassword** - Hash password using PBKDF2 with SHA256.
- **VerifyPassword** - Verify password against hash.
- **Encrypt** - Encrypt text using AES-256-GCM with provided key.
- **Decrypt** - Decrypt text using AES-256-GCM with provided key.
- **GenerateRandomString** - Generate cryptographically secure random string.
- **GenerateEncryptionKey** - Generate random encryption key (Base64 encoded).
- **ComputeSha256** - Compute SHA256 hash of text for integrity checking.

### JsonHelper

- **Namespace:** `SmartWorkz.Shared.JsonHelper`
- **Summary:** JSON serialization utilities using System.Text.Json.
            Provides consistent serialization options across the application.

#### Methods & Properties

- **Serialize``1** - Serialize object to JSON string.
- **Serialize** - Serialize object to JSON string with dynamic type.
- **Deserialize``1** - Deserialize JSON string to object.
- **Deserialize** - Deserialize JSON string to object with dynamic type.
- **DeserializeAsync``1** - Deserialize JSON asynchronously from stream.
- **SerializeAsync``1** - Serialize asynchronously to stream.
- **IsValidJson** - Check if string is valid JSON.
- **GetValueByPath** - Parse JSON and extract value at specified path (dot notation).

### IHttpClient

- **Namespace:** `SmartWorkz.Shared.IHttpClient`
- **Summary:** Abstraction for HTTP client operations with support for async/await and cancellation.
            Implementations should handle retries, timeouts, and error responses gracefully.

#### Methods & Properties

- **GetAsync``1** - Sends a GET request and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **PostAsync``1** - Sends a POST request with JSON body and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **PutAsync``1** - Sends a PUT request with JSON body and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **DeleteAsync``1** - Sends a DELETE request and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **GetAsync** - Sends a GET request and returns a string response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.
- **PostAsync** - Sends a POST request with JSON body and returns a string response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.

### RetryStrategy

- **Namespace:** `SmartWorkz.Shared.RetryStrategy`
- **Summary:** Specifies the backoff strategy to use when retrying failed HTTP requests.

### RetryPolicy

- **Namespace:** `SmartWorkz.Shared.RetryPolicy`
- **Summary:** Configures automatic retry behavior for failed HTTP requests.

### AuditRecord

- **Namespace:** `SmartWorkz.Shared.AuditRecord`
- **Summary:** Represents an immutable audit record with all relevant audit information.

#### Methods & Properties

- **#ctor** - Represents an immutable audit record with all relevant audit information.
  - Parameters:
    - `Id`: The unique identifier of the audit record
    - `EntityType`: The type of entity being audited (e.g., "User", "BlogPost")
    - `EntityId`: The identifier of the audited entity
    - `Action`: The action performed (Create, Update, Delete, etc.)
    - `UserId`: The identifier of the user who performed the action
    - `PerformedAt`: The timestamp when the action was performed
    - `Metadata`: Optional metadata dictionary containing additional context

### EnrichedLogger

- **Namespace:** `SmartWorkz.Shared.EnrichedLogger`
- **Summary:** Enriched logger wrapper around ILogger that provides structured logging methods
            for domain events, commands, sagas, file operations, and background jobs.
            Uses structured properties instead of string interpolation for better queryability.

#### Methods & Properties

- **#ctor** - Creates a new instance of EnrichedLogger.
  - Parameters:
    - `logger`: The underlying ILogger instance
- **LogCommandExecuted** - Logs command execution with duration and other metrics.
  - Parameters:
    - `commandType`: The type of command being executed
    - `duration`: How long the command took to execute
- **LogCommandExecutionError** - Logs a command execution error with exception details.
  - Parameters:
    - `commandType`: The type of command that failed
    - `exception`: The exception that occurred
- **LogCommandValidationError** - Logs a command with validation errors.
  - Parameters:
    - `commandType`: The type of command
    - `errors`: Dictionary of validation errors
- **LogEventPublished** - Logs an event publication with metadata.
  - Parameters:
    - `eventType`: The type of event being published
    - `eventId`: The unique identifier of the event
- **LogEventPublishedWithContext** - Logs an event with additional context properties.
  - Parameters:
    - `eventType`: The type of event
    - `eventId`: The event identifier
    - `context`: Additional context data
- **LogEventSubscribed** - Logs an event subscription.
  - Parameters:
    - `eventType`: The type of event being subscribed to
    - `subscriberType`: The subscriber type
- **LogSagaStarted** - Logs the start of a saga with its initial state.
  - Parameters:
    - `sagaId`: The unique saga identifier
    - `state`: The initial saga state
- **LogSagaStateTransition** - Logs a saga state transition.
  - Parameters:
    - `sagaId`: The saga identifier
    - `fromState`: The previous state
    - `toState`: The new state
- **LogSagaCompleted** - Logs the completion of a saga.
  - Parameters:
    - `sagaId`: The saga identifier
    - `duration`: How long the saga took to complete
- **LogSagaFailed** - Logs a saga failure.
  - Parameters:
    - `sagaId`: The saga identifier
    - `exception`: The exception that caused the failure
- **LogFileOperation** - Logs file operations such as upload, download, delete.
  - Parameters:
    - `operation`: The type of operation (Upload, Download, Delete, etc.)
    - `filePath`: The file path or URI
- **LogFileOperationWithSize** - Logs a file operation with size information.
  - Parameters:
    - `operation`: The type of operation
    - `filePath`: The file path
    - `sizeBytes`: The file size in bytes
- **LogFileOperationError** - Logs a file operation error.
  - Parameters:
    - `operation`: The operation that failed
    - `filePath`: The file path
    - `exception`: The exception that occurred
- **LogJobQueued** - Logs when a background job is queued.
  - Parameters:
    - `jobId`: The unique job identifier
    - `jobType`: The type of job being queued
- **LogJobStarted** - Logs when a background job starts processing.
  - Parameters:
    - `jobId`: The job identifier
    - `jobType`: The job type
- **LogJobCompleted** - Logs successful job completion.
  - Parameters:
    - `jobId`: The job identifier
    - `duration`: How long the job took to complete
- **LogJobFailed** - Logs a job failure.
  - Parameters:
    - `jobId`: The job identifier
    - `exception`: The exception that caused the failure
- **LogJobRetry** - Logs job retry attempt.
  - Parameters:
    - `jobId`: The job identifier
    - `attemptNumber`: The current attempt number
    - `maxRetries`: The maximum number of retries
- **LogWithContext** - Logs a message with structured context properties.
  - Parameters:
    - `operationName`: The name of the operation
    - `context`: Dictionary of contextual properties
- **LogPerformanceMetrics** - Logs performance metrics for an operation.
  - Parameters:
    - `operationName`: The operation name
    - `duration`: The operation duration
    - `resultStatus`: The result status (Success, Failure, etc.)
- **LogCorrelation** - Logs a correlation ID for request tracing.
  - Parameters:
    - `correlationId`: The correlation identifier
    - `userId`: Optional user identifier
    - `requestPath`: Optional request path
- **LogUnhandledException** - Logs unhandled exceptions as critical errors.
  - Parameters:
    - `exception`: The exception that occurred
    - `operationName`: The operation that failed

### IAuditLogger

- **Namespace:** `SmartWorkz.Shared.IAuditLogger`
- **Summary:** Interface for structured audit logging with metadata support.

#### Methods & Properties

- **LogAuditAsync** - Logs an audit event with structured metadata.
  - Parameters:
    - `entityType`: The entity type being audited (e.g., "User", "BlogPost")
    - `entityId`: The unique identifier of the entity
    - `action`: The action performed (Create, Update, Delete, etc.)
    - `metadata`: Optional metadata dictionary for additional context
    - `cancellationToken`: Cancellation token
  - Returns: Result indicating success or failure
- **GetAuditHistoryAsync** - Retrieves audit logs for a specific entity.
  - Parameters:
    - `entityType`: The entity type
    - `entityId`: The entity identifier
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of audit records
- **GetUserActivityAsync** - Retrieves audit logs for a specific user across all entities.
  - Parameters:
    - `userId`: The user identifier
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of audit records

### ILogger

- **Namespace:** `SmartWorkz.Shared.ILogger`
- **Summary:** Abstraction for application logging.
            Decouples from specific logging frameworks (Serilog, NLog, etc.).

### LogLevel

- **Namespace:** `SmartWorkz.Shared.LogLevel`
- **Summary:** Log level severity.

### ILoggerFactory

- **Namespace:** `SmartWorkz.Shared.ILoggerFactory`
- **Summary:** Factory for creating logger instances by category/source.

### IMapper

- **Namespace:** `SmartWorkz.Shared.IMapper`
- **Summary:** Mapping service abstraction for transforming objects between types.
            Supports registration of mapping profiles and bidirectional conversions.

#### Methods & Properties

- **Map``2** - Map source object to target type.
- **Map** - Map source object to target type using dynamic type.
- **MapAsync``2** - Map asynchronously with potential async operations in profile.
- **MapCollection``2** - Map collection of sources to targets.
- **MapCollectionAsync``2** - Map collection asynchronously.
- **RegisterProfile``2** - Register a mapping profile.

### IMapperProfile

- **Namespace:** `SmartWorkz.Shared.IMapperProfile`
- **Summary:** Profile for defining mapping rules between types.
            Implemented by concrete profiles that configure source-to-target transformations.

### IMapperProfile`2

- **Namespace:** `SmartWorkz.Shared.IMapperProfile`2`
- **Summary:** Typed mapper profile for strong typing.

#### Methods & Properties

- **Map** - Transform source to target synchronously.
- **MapAsync** - Transform source to target asynchronously.

### SimpleMapper

- **Namespace:** `SmartWorkz.Shared.SimpleMapper`
- **Summary:** A simple in-memory mapper that supports registering and executing mapping profiles.

### IMetricsCollector

- **Namespace:** `SmartWorkz.Shared.IMetricsCollector`
- **Summary:** Abstraction for collecting application metrics and performance data.
            Enables tracking of operation duration, throughput, error rates, and custom metrics.
            Implementations integrate with OpenTelemetry for export to Prometheus/Grafana.

#### Methods & Properties

- **RecordOperationDuration** - Record operation duration in milliseconds.
  - Parameters:
    - `operationName`: Name of the operation being measured.
    - `durationMs`: Duration in milliseconds.
    - `status`: Optional status (e.g., "success", "error").
    - `tags`: Optional metadata tags for grouping and filtering.
- **RecordOperationCount** - Record operation count (increments counter).
  - Parameters:
    - `operationName`: Name of the operation.
    - `count`: Number to increment by (default 1).
    - `status`: Optional status label.
    - `tags`: Optional metadata tags.
- **RecordGaugeValue** - Record a gauge value (e.g., queue depth, memory usage).
  - Parameters:
    - `metricName`: Name of the gauge metric.
    - `value`: The gauge value to record.
    - `tags`: Optional metadata tags.
- **RecordError** - Record error/exception occurrence.
  - Parameters:
    - `operationName`: Name of the operation that failed.
    - `ex`: The exception that occurred.
    - `tags`: Optional metadata tags.
- **IncrementCounter** - Increment a custom counter.
  - Parameters:
    - `counterName`: Name of the counter.
    - `increment`: Amount to increment (default 1).
    - `tags`: Optional metadata tags.

### MetricsMiddleware

- **Namespace:** `SmartWorkz.Shared.MetricsMiddleware`
- **Summary:** ASP.NET Core middleware for automatic HTTP request/response metrics collection.
             Records operation duration, status, and errors for all HTTP requests.
            
             Usage:
                 app.UseMiddleware<MetricsMiddleware>();

### MetricsStartupExtensions

- **Namespace:** `SmartWorkz.Shared.MetricsStartupExtensions`
- **Summary:** Extension methods for registering application metrics in dependency injection.

#### Methods & Properties

- **AddApplicationMetrics** - Registers IMetricsCollector with OpenTelemetry implementation.
  - Parameters:
    - `services`: The service collection to register with.
  - Returns: The service collection for method chaining.

### OpenTelemetryMetricsCollector

- **Namespace:** `SmartWorkz.Shared.OpenTelemetryMetricsCollector`
- **Summary:** OpenTelemetry-based implementation of IMetricsCollector.
            Collects metrics using System.Diagnostics.Metrics for export to Prometheus/Grafana.

### DefaultTenantFeatureFlags

- **Namespace:** `SmartWorkz.Shared.DefaultTenantFeatureFlags`
- **Summary:** In-memory feature flag provider for tenant-scoped feature control.
            
             Uses ConcurrentDictionary to store tenant-specific flags:
             - Key: tenant ID
             - Value: HashSet of enabled feature flag names
            
             Thread-safe for concurrent operations. Suitable for in-process caching
             or dev/test scenarios. For distributed systems, integrate with a
             centralized feature flag service (Unleash, LaunchDarkly, etc.).

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature is enabled for the specified tenant.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").
    - `cancellationToken`: Cancellation token.
  - Returns: Task containing true if the feature is enabled for this tenant,
            false if the tenant doesn't exist or the flag is not enabled.
- **GetEnabledFeaturesAsync** - Gets all enabled features for a tenant.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Task containing a read-only list of enabled feature flag names.
            Returns an empty list if the tenant doesn't exist or has no enabled flags.
- **EnableFlag** - Enables a feature flag for a tenant.
            Idempotent — calling multiple times with the same flag is safe.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name.
- **DisableFlag** - Disables a feature flag for a tenant.
            Idempotent — calling multiple times with the same flag is safe.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name.

### ITenantContext

- **Namespace:** `SmartWorkz.Shared.ITenantContext`
- **Summary:** Scoped service providing current tenant ID for multi-tenant applications.
            Resolved from request context or claims principal.

#### Methods & Properties

- **GetTenantId** - Gets the current tenant identifier.
  - Returns: Tenant ID, or null if operating in single-tenant context.
- **SetTenantId** - Sets the current tenant identifier (rarely used; typically set from request context).
  - Parameters:
    - `tenantId`: Tenant ID to set.

### ITenantFeatureFlags

- **Namespace:** `SmartWorkz.Shared.ITenantFeatureFlags`
- **Summary:** Feature flag provider scoped to a specific tenant.
            Allows per-tenant feature control.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature is enabled for the specified tenant.
  - Parameters:
    - `tenantId`: Tenant ID.
    - `flagName`: Feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").
    - `cancellationToken`: Cancellation token.
  - Returns: True if feature is enabled for this tenant.
- **GetEnabledFeaturesAsync** - Gets all enabled features for a tenant.
  - Parameters:
    - `tenantId`: Tenant ID.
    - `cancellationToken`: Cancellation token.
  - Returns: List of enabled feature flag names.

### TenantContext

- **Namespace:** `SmartWorkz.Shared.TenantContext`
- **Summary:** Scoped tenant context using AsyncLocal for proper isolation across async boundaries.
            
             AsyncLocal ensures:
             - Thread-safe storage per async execution context
             - Isolation between concurrent requests (each gets its own context)
             - Proper inheritance to child tasks (when awaited)
            
             Survives async/await boundaries unlike ThreadLocal, making it suitable for async methods.

#### Methods & Properties

- **GetTenantId** - Gets the current tenant identifier.
  - Returns: The current tenant ID, or "default" if not set.
- **SetTenantId** - Sets the current tenant identifier.
  - Parameters:
    - `tenantId`: The tenant ID to set. Cannot be null or empty.

### FirebaseCloudMessagingService

- **Namespace:** `SmartWorkz.Shared.FirebaseCloudMessagingService`
- **Summary:** Firebase Cloud Messaging service implementation for sending push notifications.
            Supports single/batch user notifications, topic-based broadcasting, and multi-platform delivery (Android, iOS, Web).

#### Methods & Properties

- **#ctor** - Initializes a new instance of the FirebaseCloudMessagingService.
  - Parameters:
    - `logger`: Logger for diagnostic and error information.
- **SendAsync** - Sends a simple push notification to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends simple push notifications to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase device tokens).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends a rich push notification with metadata to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends a rich push notification to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase device tokens).
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SendToTopicAsync** - Sends a rich push notification to all users subscribed to a topic (broadcast).
  - Parameters:
    - `topic`: Topic name (e.g., "news", "promotions").
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SubscribeToTopicAsync** - Subscribes a user to a topic for broadcast notifications.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `topic`: Topic name to subscribe to.
    - `cancellationToken`: Cancellation token.
- **UnsubscribeFromTopicAsync** - Unsubscribes a user from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `topic`: Topic name to unsubscribe from.
    - `cancellationToken`: Cancellation token.

### IPushNotificationService

- **Namespace:** `SmartWorkz.Shared.IPushNotificationService`
- **Summary:** Service for sending push notifications using Firebase Cloud Messaging.

#### Methods & Properties

- **SendAsync** - Sends a simple push notification to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a simple push notification to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase tokens).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a rich push notification with metadata to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a rich push notification with metadata to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase tokens).
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendToTopicAsync** - Sends a push notification to all users subscribed to a topic.
  - Parameters:
    - `topic`: The topic name.
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SubscribeToTopicAsync** - Subscribes a user to receive notifications from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `topic`: The topic name.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous subscription operation.
- **UnsubscribeFromTopicAsync** - Unsubscribes a user from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `topic`: The topic name.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous unsubscription operation.

### PushNotificationPayload

- **Namespace:** `SmartWorkz.Shared.PushNotificationPayload`
- **Summary:** Represents the payload data for a push notification.

### PushNotificationAction

- **Namespace:** `SmartWorkz.Shared.PushNotificationAction`
- **Summary:** Represents an action that can be performed from a push notification.

### PagedList`1

- **Namespace:** `SmartWorkz.Shared.PagedList`1`
- **Summary:** A page of items with metadata.
             Replaces PaginationResponse<T> in StarterKitMVC.Shared.DTOs.
            
             Migration path: PaginationResponse<T> has the same fields under different names.
             PagedList<T>.Create() is a drop-in replacement for PaginationResponse<T>.Create().

#### Methods & Properties

- **Empty** - Create an empty result set (e.g., when no rows match).
- **Map``1** - Project items to a different type without changing pagination metadata.

### PagedQuery

- **Namespace:** `SmartWorkz.Shared.PagedQuery`
- **Summary:** Standard request parameters for any paginated query.
             Replaces the existing PaginationRequest record in StarterKitMVC.Shared.DTOs.
            
             Migration: PaginationRequest is structurally identical — alias it or replace it.

#### Methods & Properties

- **#ctor** - Standard request parameters for any paginated query.
             Replaces the existing PaginationRequest record in StarterKitMVC.Shared.DTOs.
            
             Migration: PaginationRequest is structurally identical — alias it or replace it.
- **Normalize** - Clamp page and pageSize to safe bounds.

### IEntity`1

- **Namespace:** `SmartWorkz.Shared.IEntity`1`
- **Summary:** Marks a class as a domain entity with a typed primary key.

### CircuitBreaker

- **Namespace:** `SmartWorkz.Shared.CircuitBreaker`
- **Summary:** A thread-safe implementation of the circuit breaker pattern for handling failing dependencies gracefully.
            
             The circuit breaker operates in three states:
             - Closed: Normal operation. Requests pass through. Failures are tracked.
             - Open: Failing. All requests are rejected immediately to prevent cascading failures.
             - HalfOpen: Testing recovery. Limited requests are allowed to test if the dependency has recovered.
            
             State transitions:
             - Closed → Open: When ConsecutiveFailures >= FailureThreshold
             - Open → HalfOpen: Automatically when (DateTime.UtcNow - LastFailureTime) >= TimeoutMilliseconds
             - HalfOpen → Closed: When SuccessCount >= SuccessThreshold
             - HalfOpen → Open: When RecordFailure() is called in HalfOpen state
             - Closed → Closed: When RecordSuccess() is called (resets failure counter)

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `options`: The circuit breaker configuration options.
- **RecordSuccess** - Records a successful operation and updates state transitions accordingly.
            In Closed state: resets the failure counter.
            In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
- **RecordFailure** - Records a failed operation and updates state transitions accordingly.
            In Closed state: increments failure counter and transitions to Open if threshold is met.
            In HalfOpen state: immediately transitions back to Open.
- **Reset** - Resets the circuit breaker to the Closed state, clearing all failure and success counters.

### CircuitBreakerOptions

- **Namespace:** `SmartWorkz.Shared.CircuitBreakerOptions`
- **Summary:** Configuration options for the circuit breaker.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if options are valid; otherwise false.

### CircuitBreakerState

- **Namespace:** `SmartWorkz.Shared.CircuitBreakerState`
- **Summary:** Defines the state of a circuit breaker in the state machine pattern.

### ICircuitBreaker

- **Namespace:** `SmartWorkz.Shared.ICircuitBreaker`
- **Summary:** Defines the contract for a circuit breaker that implements the state machine pattern
            to handle failing dependencies gracefully.

#### Methods & Properties

- **RecordSuccess** - Records a successful operation and updates state transitions accordingly.
            In Closed state: resets the failure counter.
            In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
- **RecordFailure** - Records a failed operation and updates state transitions accordingly.
            In Closed state: increments failure counter and transitions to Open if threshold is met.
            In HalfOpen state: immediately transitions back to Open.
- **Reset** - Resets the circuit breaker to the Closed state, clearing all failure and success counters.

### IRateLimiter

- **Namespace:** `SmartWorkz.Shared.IRateLimiter`
- **Summary:** Defines the contract for a thread-safe rate limiter.

#### Methods & Properties

- **TryAcquireAsync** - Attempts to acquire tokens for a request identified by the given identifier.
  - Parameters:
    - `identifier`: The identifier for rate limiting (e.g., IP address, user ID).
    - `cost`: The number of tokens to acquire (default: 1).
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A result containing true if tokens were acquired successfully; otherwise false.
            On failure, the Error will include retry-after information.
- **GetAvailableTokensAsync** - Gets the number of available tokens for a given identifier.
  - Parameters:
    - `identifier`: The identifier to check.
  - Returns: The number of available tokens.
- **ResetAsync** - Resets the rate limit state for a given identifier.
  - Parameters:
    - `identifier`: The identifier to reset.
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **ClearAsync** - Clears all rate limit state.
  - Parameters:
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.

### RateLimiter

- **Namespace:** `SmartWorkz.Shared.RateLimiter`
- **Summary:** Thread-safe token bucket rate limiter implementation.
            
             This class maintains a per-identifier token bucket that refills at a constant rate.
             Tokens are consumed when requests are made; if insufficient tokens exist, the request is denied.
            
             Thread-safe operations use ConcurrentDictionary and locks on individual buckets to ensure
             consistent state without global locking bottlenecks.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the RateLimiter class.
  - Parameters:
    - `options`: Configuration options for the rate limiter.
- **TryAcquireAsync** - Attempts to acquire tokens for a request identified by the given identifier.
  - Parameters:
    - `identifier`: The identifier for rate limiting (e.g., IP address, user ID).
    - `cost`: The number of tokens to acquire (default: 1).
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A result containing true if tokens were acquired successfully; otherwise false.
            On failure, the Error will include retry-after information.
- **GetAvailableTokensAsync** - Gets the number of available tokens for a given identifier.
  - Parameters:
    - `identifier`: The identifier to check.
  - Returns: The number of available tokens.
- **ResetAsync** - Resets the rate limit state for a given identifier.
  - Parameters:
    - `identifier`: The identifier to reset.
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **ClearAsync** - Clears all rate limit state.
  - Parameters:
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **TokenBucket.TryAcquire** - Tries to acquire the specified number of tokens.
- **TokenBucket.GetAvailableTokens** - Gets the current number of available tokens.
- **TokenBucket.GetRetryAfterMilliseconds** - Gets the number of milliseconds to wait before retrying.
- **TokenBucket.RefillTokens** - Refills the token bucket based on elapsed time.

### RateLimiterOptions

- **Namespace:** `SmartWorkz.Shared.RateLimiterOptions`
- **Summary:** Configuration options for the rate limiter.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if options are valid; otherwise false.

### RateLimiterStrategy

- **Namespace:** `SmartWorkz.Shared.RateLimiterStrategy`
- **Summary:** Specifies the strategy used by the rate limiter to control request flow.

### ApiError

- **Namespace:** `SmartWorkz.Shared.ApiError`
- **Summary:** Structured error representation for API responses.
            Provides code, message, and optional field-level error details.

#### Methods & Properties

- **FromError** - Create from core Error type.
- **FromValidationErrors** - Create from validation errors.
- **FromException** - Create from exception.

### ApiResponse

- **Namespace:** `SmartWorkz.Shared.ApiResponse`
- **Summary:** Generic API response envelope that wraps result data with metadata.
            Non-generic convenience version for non-data responses.

#### Methods & Properties

- **Ok** - Success response without data.
- **Fail** - Failure response with error details.
- **FromResult** - Create from core Result pattern.

### ApiResponse`1

- **Namespace:** `SmartWorkz.Shared.ApiResponse`1`
- **Summary:** Typed API response envelope with data payload.
            Includes optional pagination metadata for list responses.

#### Methods & Properties

- **Ok** - Success response with data.
- **OkPaginated** - Success response with paginated data.
- **Fail** - Failure response with error.

### ProblemDetailsResponse

- **Namespace:** `SmartWorkz.Shared.ProblemDetailsResponse`
- **Summary:** Implements RFC 7807 Problem Details for HTTP APIs standard response format.
            Provides a standardized way to represent error details in API responses.

#### Methods & Properties

- **ValidationError** - Factory method for 400 Bad Request error with validation details.
- **Unauthorized** - Factory method for 401 Unauthorized error.
- **Forbidden** - Factory method for 403 Forbidden error.
- **NotFound** - Factory method for 404 Not Found error.
- **Conflict** - Factory method for 409 Conflict error.
- **InternalServerError** - Factory method for 500 Internal Server Error.
- **Custom** - Factory method for custom problem details.

### Error

- **Namespace:** `SmartWorkz.Shared.Error`
- **Summary:** Represents a structured error with a machine-readable code and human-readable message.
            
             This is the canonical Error type. It replaces:
             - SmartWorkz.StarterKitMVC.Shared.Primitives.Error (record struct)
             - The ad-hoc string errors in Models.Result
            
             Code examples: "USER_NOT_FOUND", "VALIDATION.EMAIL_REQUIRED", "AUTH.INVALID_CREDENTIALS"
             MessageKey maps to localization resource keys for UI display.

### Result

- **Namespace:** `SmartWorkz.Shared.Result`
- **Summary:** Represents the outcome of an operation that does not return a value.
            
             This unifies:
             - SmartWorkz.StarterKitMVC.Shared.Models.Result (Succeeded + MessageKey + Errors[])
             - SmartWorkz.StarterKitMVC.Shared.Primitives.Result (IsSuccess + Error struct)
            
             Design choice — class over struct:
             1. Result<T> inherits from Result to reuse Succeeded/Errors without duplication.
                Structs cannot use inheritance this way.
             2. Services return Result from interface methods — class semantics (null check) are
                simpler than boxing/unboxing structs across interface boundaries.
             3. Errors[] supports field-level validation messages that ModelState.AddErrors() consumes.
                A single Error struct cannot carry multiple field errors.
            
             The Primitives.Result struct in StarterKitMVC.Shared remains valid for pure functions
             where you want zero-allocation returns. This class is for service layer contracts.

#### Methods & Properties

- **Fail** - Failure with a localization message key and optional field-level error strings.
- **Fail** - Failure from a structured Error (bridges the Primitives.Error pattern).
- **Ok``1** - Factory for a typed result. Use in services that return data.

### Result`1

- **Namespace:** `SmartWorkz.Shared.Result`1`
- **Summary:** Result with a typed payload. Data is only valid when Succeeded = true.
            
             Usage:
               Result<UserDto> result = await _userService.GetByIdAsync(id);
               if (!result.Succeeded) return RedirectToPage("Error");
               var user = result.Data!;

### ResultExtensions

- **Namespace:** `SmartWorkz.Shared.ResultExtensions`
- **Summary:** Functional helpers for chaining Result operations.
            Keeps service code flat — avoids nested if (!result.Succeeded) blocks.

#### Methods & Properties

- **Map``2** - Transform the Data value if the result succeeded.
- **BindAsync``2** - Chain a second operation that also returns Result.
- **OnSuccess``1** - Execute a side-effect action on success, then return the original result.
- **OnFailure``1** - Execute a side-effect action on failure, then return the original result.

### ISagaDefinition`1

- **Namespace:** `SmartWorkz.Shared.ISagaDefinition`1`
- **Summary:** Defines the blueprint for a saga orchestration.
            A saga is a pattern for managing distributed transactions and long-running processes
            by coordinating multiple steps with built-in compensation mechanisms.

#### Methods & Properties

- **DefineStep``1** - Defines a step in the saga that will be executed when a specific event type is received.
            Steps are executed sequentially in the order they were defined.
  - Parameters:
    - `handler`: The async handler function that processes the event and updates the saga state.
            Returns a StepResult indicating success or failure.
- **OnFailure** - Defines the failure handler that will be called if any step fails.
            Used for compensation logic and saga-level error handling.
  - Parameters:
    - `compensationHandler`: The async handler that receives the current saga state and the exception that occurred.
            Responsible for compensation/rollback logic.
- **BuildAsync** - Builds and returns the saga definition for execution.
            Can be used for async initialization or validation.
  - Returns: A task that completes with the configured saga definition.
- **GetSteps** - Gets the list of saga steps in execution order.
  - Returns: A read-only list of saga step handlers.
- **GetFailureHandler** - Gets the failure compensation handler if defined.
  - Returns: The failure handler function, or null if not defined.

### SagaOrchestrator

- **Namespace:** `SmartWorkz.Shared.SagaOrchestrator`
- **Summary:** Orchestrates the execution of sagas, managing step sequencing, error handling,
            and compensation/rollback logic for complex distributed processes.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the SagaOrchestrator class.
  - Parameters:
    - `logger`: Logger for saga execution tracking and debugging.
- **ExecuteSagaAsync``1** - Executes a saga definition with the provided initial state and triggering event.
            Manages step execution, error handling, and compensation logic.
  - Parameters:
    - `sagaDefinition`: The saga definition blueprint to execute.
    - `initialState`: The initial saga state.
    - `event`: The domain event triggering the saga.
    - `cancellationToken`: Optional cancellation token.
  - Returns: A task representing the saga execution.
- **ExecuteSagaStepsAsync``1** - Executes saga steps by using reflection to access internal step definitions.
- **CompensateExecutedStepsAsync``1** - Executes compensation handlers for all executed steps in reverse order.
            Uses stored compensation handlers to avoid re-executing steps.
- **ExecuteFailureHandlerAsync``1** - Executes the saga-level failure handler if one is defined.

### SagaStatus

- **Namespace:** `SmartWorkz.Shared.SagaStatus`
- **Summary:** Represents the status of a saga execution.

### SagaState

- **Namespace:** `SmartWorkz.Shared.SagaState`
- **Summary:** Base class for saga state objects.
            Provides common tracking properties for saga execution flow.

### StepResult

- **Namespace:** `SmartWorkz.Shared.StepResult`
- **Summary:** Represents the result of executing a single saga step.
            Provides success/failure status and optional compensation logic for rollback.

#### Methods & Properties

- **Success** - Creates a successful step result.
  - Returns: A StepResult indicating success.
- **Failure** - Creates a failed step result with an optional compensation handler.
  - Parameters:
    - `failureReason`: The reason for the step failure.
    - `compensationHandler`: Optional handler to compensate/rollback this step if a later step fails.
  - Returns: A StepResult indicating failure.
- **FromException** - Creates a failed step result for an exception with optional compensation.
  - Parameters:
    - `exception`: The exception that caused the failure.
    - `compensationHandler`: Optional compensation handler.
  - Returns: A StepResult indicating failure.

### CryptHelper

- **Namespace:** `SmartWorkz.Shared.CryptHelper`
- **Summary:** Provides AES-256-CBC encryption and decryption utilities with secure key and IV generation.
            
             All operations support both string and byte array inputs/outputs.
             Keys are normalized to 32 bytes (256 bits) via padding/trimming as needed.
             IVs are auto-generated if not provided and embedded in the ciphertext (IV:Ciphertext format).

#### Methods & Properties

- **EncryptString** - Encrypts plaintext using AES-256-CBC with a Base64-encoded output.
  - Parameters:
    - `plaintext`: The plaintext to encrypt.
    - `key`: The encryption key (will be normalized to 32 bytes).
    - `iv`: Optional IV; auto-generated if null.
  - Returns: A Result containing Base64-encoded ciphertext in "IV:Ciphertext" format or an error.
- **EncryptBytes** - Encrypts byte data using AES-256-CBC.
  - Parameters:
    - `plaintext`: The plaintext bytes to encrypt.
    - `key`: The encryption key (will be normalized to 32 bytes).
    - `iv`: Optional IV; auto-generated if null.
  - Returns: A Result containing encrypted bytes with embedded IV (IV || Ciphertext) or an error.
- **DecryptString** - Decrypts Base64-encoded ciphertext using AES-256-CBC.
  - Parameters:
    - `ciphertext`: The Base64-encoded ciphertext in "IV:Ciphertext" format.
    - `key`: The decryption key (will be normalized to 32 bytes).
  - Returns: A Result containing the decrypted plaintext or an error.
- **DecryptBytes** - Decrypts byte data using AES-256-CBC.
  - Parameters:
    - `ciphertext`: The encrypted bytes with embedded IV (IV || Ciphertext).
    - `key`: The decryption key (will be normalized to 32 bytes).
  - Returns: A Result containing decrypted bytes or an error.
- **GenerateKey** - Generates a random cryptographic key of the specified size.
  - Parameters:
    - `keySize`: The key size in bytes (default 32 for AES-256). Must be 16, 24, or 32.
  - Returns: A Result containing Base64-encoded random key or an error.
- **GenerateIv** - Generates a random cryptographic IV (Initialization Vector).
  - Returns: A Result containing Base64-encoded random IV or an error.
- **GenerateRandomBytes** - Generates cryptographically secure random bytes.
- **NormalizeKey** - Normalizes a key to exactly 32 bytes (256 bits).
            If the key is shorter, it's padded with zeros. If longer, it's trimmed.

### CryptOptions

- **Namespace:** `SmartWorkz.Shared.CryptOptions`
- **Summary:** Configuration options for AES cryptographic operations.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if valid, false otherwise.

### HashHelper

- **Namespace:** `SmartWorkz.Shared.HashHelper`
- **Summary:** Provides utilities for cryptographic hash operations (SHA256 and MD5).

#### Methods & Properties

- **Sha256** - Computes the SHA256 hash of a string and returns it as a hexadecimal string.
- **Sha256Bytes** - Computes the SHA256 hash of a byte array and returns the hash as a byte array.
- **Md5** - Computes the MD5 hash of a string and returns it as a hexadecimal string.
            Note: MD5 is cryptographically broken; use SHA256 for security-critical applications.
- **VerifyHash** - Verifies that a text matches its SHA256 hash.

### HmacAlgorithm

- **Namespace:** `SmartWorkz.Shared.HmacAlgorithm`
- **Summary:** Specifies the HMAC algorithm to use for message signing and verification.

### HmacHelper

- **Namespace:** `SmartWorkz.Shared.HmacHelper`
- **Summary:** Provides HMAC-SHA256/SHA512 message signing and verification for API requests and webhook verification.
            Implements constant-time comparison to prevent timing attacks.

#### Methods & Properties

- **Sign** - Signs a message using HMAC with the specified algorithm and returns a Base64-encoded hex digest.
  - Parameters:
    - `message`: The message to sign.
    - `secret`: The secret key for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing the Base64-encoded signature or an error.
- **SignBytes** - Signs a message using HMAC with the specified algorithm and returns the raw byte digest.
  - Parameters:
    - `message`: The message bytes to sign.
    - `secret`: The secret key for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing the byte signature or an error.
- **Verify** - Verifies a message signature using HMAC with constant-time comparison to prevent timing attacks.
  - Parameters:
    - `message`: The original message that was signed.
    - `signature`: The Base64-encoded signature to verify.
    - `secret`: The secret key used for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing true if the signature is valid, false if not, or an error.
- **VerifyBytes** - Verifies a message signature using HMAC with raw byte inputs and constant-time comparison.
  - Parameters:
    - `message`: The original message bytes that were signed.
    - `signature`: The byte signature to verify.
    - `secret`: The secret key used for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing true if the signature is valid, false if not, or an error.
- **SignBytes** - Internal method to compute HMAC signature from raw bytes.
- **CreateHmac** - Creates the appropriate HMAC instance based on the algorithm.

### InputSanitizer

- **Namespace:** `SmartWorkz.Shared.InputSanitizer`
- **Summary:** Input sanitization to prevent XSS, SQL injection, and path traversal attacks.

#### Methods & Properties

- **SanitizeHtml** - Sanitize HTML by removing dangerous tags and attributes.
- **EscapeHtml** - Escape HTML special characters to prevent XSS.
- **SanitizeSql** - Sanitize string to prevent SQL injection (basic, not a replacement for parameterized queries).
- **SanitizeFilePath** - Sanitize file path to prevent directory traversal attacks.
- **SanitizeUrl** - Sanitize and validate URL.
- **EscapeJson** - Escape string for safe JSON inclusion.
- **IsValidEmail** - Validate email format (basic check, server-side SMTP validation recommended).
- **RemoveControlCharacters** - Remove null bytes and control characters.

### JwtSettings

- **Namespace:** `SmartWorkz.Shared.JwtSettings`
- **Summary:** Settings for JWT token generation and validation.

#### Methods & Properties

- **Validate** - Validate settings: Secret >= 32 chars, other fields non-empty.

### JwtClaims

- **Namespace:** `SmartWorkz.Shared.JwtClaims`
- **Summary:** JWT claims that can be included in a token.

#### Methods & Properties

- **GetClaimValue** - Get claim value by type (supports standard claims + custom).

### JwtTokenValidationResult

- **Namespace:** `SmartWorkz.Shared.JwtTokenValidationResult`
- **Summary:** Result of JWT token validation.

### JwtHelper

- **Namespace:** `SmartWorkz.Shared.JwtHelper`
- **Summary:** Provides JWT token generation, validation, and refresh functionality.

#### Methods & Properties

- **GenerateTokenInternal** - Internal token generation logic shared by GenerateToken and GenerateRefreshToken.
  - Parameters:
    - `claims`: The claims to include in the token.
    - `settings`: The JWT settings for signing and configuration.
    - `isRefreshToken`: If true, uses RefreshTokenExpiryDays; otherwise uses ExpiryMinutes.
  - Returns: A Result containing the signed token or an error.
- **GenerateToken** - Generates a JWT access token with the specified claims and settings.
  - Parameters:
    - `claims`: The claims to include in the token.
    - `settings`: The JWT settings for signing and configuration.
  - Returns: A Result containing the signed token or an error.
- **ValidateToken** - Validates a JWT token and extracts claims if valid.
  - Parameters:
    - `token`: The token to validate.
    - `settings`: The JWT settings for validation.
  - Returns: A Result containing the validation result.
- **RefreshToken** - Refreshes an access token using a refresh token.
  - Parameters:
    - `refreshToken`: The refresh token.
    - `settings`: The JWT settings.
  - Returns: A Result containing the new access token or an error.
- **GenerateRefreshToken** - Generates a refresh token with extended expiry.
  - Parameters:
    - `claims`: The claims to include in the refresh token.
    - `settings`: The JWT settings.
  - Returns: A Result containing the refresh token or an error.
- **ToBase64Url** - Encodes bytes to Base64Url format (no padding, + → -, / → _).
- **FromBase64Url** - Decodes Base64Url format to bytes.

### PasswordHelper

- **Namespace:** `SmartWorkz.Shared.PasswordHelper`
- **Summary:** Provides secure password generation and validation using cryptographically secure random number generation.

#### Methods & Properties

- **GeneratePassword** - Generates a cryptographically secure random password.
  - Parameters:
    - `length`: Length of the password (8-128, default 12).
    - `includeSpecialChars`: Whether to include special characters.
  - Returns: A Result containing the generated password or an error.
- **ValidateStrength** - Validates the strength of a password against a policy.
  - Parameters:
    - `password`: The password to validate.
    - `policy`: The policy to validate against (uses default if null).
  - Returns: A Result containing the validation result.
- **GetRandomChar** - Gets a random character from the specified character set using cryptographic randomness.
- **Shuffle** - Performs Fisher-Yates shuffle on the character array.
- **CheckPasswordLength** - Checks if password meets minimum length requirement.
- **CheckUppercase** - Checks if password contains at least one uppercase letter.
- **CheckLowercase** - Checks if password contains at least one lowercase letter.
- **CheckNumbers** - Checks if password contains at least one digit.
- **CheckSpecialChars** - Checks if password contains at least one special character.

### PasswordPolicy

- **Namespace:** `SmartWorkz.Shared.PasswordPolicy`
- **Summary:** Policy for password validation requirements.

#### Methods & Properties

- **Validate** - Validates the policy invariants.

### PasswordValidationResult

- **Namespace:** `SmartWorkz.Shared.PasswordValidationResult`
- **Summary:** Result of password validation against a policy.

### ITemplateEngine

- **Namespace:** `SmartWorkz.Shared.ITemplateEngine`
- **Summary:** Defines operations for rendering templates with placeholder substitution.

#### Methods & Properties

- **Render** - Renders a template string by replacing placeholders with values from a dictionary.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `values`: Dictionary of key-value pairs to substitute into the template.
  - Returns: The rendered content with placeholders replaced. Unmatched placeholders remain unchanged.
- **Render** - Renders a template string by extracting properties from an object model.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `model`: The model object whose public properties are used for placeholder substitution.
  - Returns: The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.
- **RenderFileAsync** - Asynchronously renders a template file by replacing placeholders with values from a dictionary.
  - Parameters:
    - `filePath`: The path to the template file.
    - `values`: Dictionary of key-value pairs to substitute into the template.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **RenderFileAsync** - Asynchronously renders a template file by extracting properties from an object model.
  - Parameters:
    - `filePath`: The path to the template file.
    - `model`: The model object whose public properties are used for placeholder substitution.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **LoadDirectoryAsync** - Asynchronously loads all template files from a directory into a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.
- **RenderDirectoryAsync** - Asynchronously loads and renders all template files from a directory using values from a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `values`: Dictionary of key-value pairs to substitute into each template.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.

### TemplateEngine

- **Namespace:** `SmartWorkz.Shared.TemplateEngine`
- **Summary:** Provides template rendering services with support for placeholder substitution.

#### Methods & Properties

- **PlaceholderRegex** - 
- **Render** - Renders a template string by replacing placeholders with values from a dictionary.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `values`: Dictionary of key-value pairs to substitute into the template.
  - Returns: The rendered content with placeholders replaced. Unmatched placeholders and null/empty content remain unchanged.
- **Render** - Renders a template string by extracting properties from an object model.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `model`: The model object whose public properties are used for placeholder substitution.
  - Returns: The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.
- **RenderFileAsync** - Asynchronously renders a template file by replacing placeholders with values from a dictionary.
  - Parameters:
    - `filePath`: The path to the template file.
    - `values`: Dictionary of key-value pairs to substitute into the template.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **RenderFileAsync** - Asynchronously renders a template file by extracting properties from an object model.
  - Parameters:
    - `filePath`: The path to the template file.
    - `model`: The model object whose public properties are used for placeholder substitution.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **LoadDirectoryAsync** - Asynchronously loads all template files from a directory into a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.
- **RenderDirectoryAsync** - Asynchronously loads and renders all template files from a directory using values from a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `values`: Dictionary of key-value pairs to substitute into each template.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.
- **ReflectModel** - Reflects over a model object and builds a case-insensitive dictionary of public properties
            mapped to their string values. Uses cached property metadata for performance.
- **ValidateFilePath** - Validates a file path to prevent directory traversal attacks.
  - Parameters:
    - `filePath`: The file path to validate.
  - Returns: A result indicating if the path is valid and safe.

### CompressHelper

- **Namespace:** `SmartWorkz.Shared.CompressHelper`
- **Summary:** Provides utilities for GZip compression and decompression.

#### Methods & Properties

- **CompressString** - Compresses a string using GZip compression.
- **DecompressString** - Decompresses a GZip-compressed byte array back to a string.
- **CompressBytes** - Compresses a byte array using GZip compression.
- **DecompressBytes** - Decompresses a GZip-compressed byte array.

### DateHelper

- **Namespace:** `SmartWorkz.Shared.DateHelper`
- **Summary:** Provides utilities for date and time operations.

#### Methods & Properties

- **GetAge** - Calculates the age in years from a birth date to today.
- **GetRelativeTime** - Returns a human-readable relative time string (e.g., "2 days ago", "in 3 hours").
- **StartOfDay** - Returns the start of the day (00:00:00) for the given date.
- **EndOfDay** - Returns the end of the day (23:59:59.999) for the given date.
- **IsWeekend** - Determines if the given date falls on a weekend (Saturday or Sunday).
- **GetDayOfWeekName** - Returns the name of the day of week (e.g., "Monday", "Tuesday").
- **DaysBetween** - Calculates the number of days between two dates (inclusive of the from date, exclusive of the to date).

### EnumHelper

- **Namespace:** `SmartWorkz.Shared.EnumHelper`
- **Summary:** Provides utilities for enum operations including reflection and description retrieval.

#### Methods & Properties

- **GetDescription** - Gets the description of an enum value from its [Description] attribute.
            Falls back to the enum name if no description is found.
- **GetValue``1** - Attempts to get an enum value by its name.
- **GetAllValues``1** - Returns all values of the specified enum type as a list.
- **GetName** - Gets the name of an enum value.

### MathHelper

- **Namespace:** `SmartWorkz.Shared.MathHelper`
- **Summary:** Provides utilities for common math operations.

#### Methods & Properties

- **Percentage** - Calculates the percentage of a value.
            Example: Percentage(100, 20) returns 20 (20% of 100).
- **PercentageChange** - Calculates the percentage change from oldValue to newValue.
            Positive result indicates increase, negative indicates decrease.
- **RoundTo** - Rounds a decimal value to the specified number of decimal places.
- **Clamp``1** - Clamps a value within a specified range [min, max].
- **Average** - Calculates the average of the provided decimal values.

### SlugHelper

- **Namespace:** `SmartWorkz.Shared.SlugHelper`
- **Summary:** Helper for generating URL-friendly slugs from text input.

#### Methods & Properties

- **GenerateSlug** - Generates a URL-friendly slug from the given text with optional configuration.
  - Parameters:
    - `text`: The input text to convert to a slug.
    - `options`: Configuration options. If null, default options are used.
  - Returns: A Result containing the generated slug or an error.
- **ToSlug** - Generates a URL-friendly slug from the given text using default options.
            Convenience method equivalent to GenerateSlug(text, null).
  - Parameters:
    - `text`: The input text to convert to a slug.
  - Returns: A Result containing the generated slug or an error.
- **RemoveAccents** - Removes accented characters from text by decomposing them and filtering out combining marks.
            For example: "café" → "cafe", "naïve" → "naive", "Señor" → "Senor".
  - Parameters:
    - `input`: The input text potentially containing accented characters.
  - Returns: The text with accented characters converted to their base forms.
- **ReplaceSpecialCharacters** - Replaces special characters and spaces with the specified separator.
            Keeps only alphanumeric characters and the separator.
  - Parameters:
    - `input`: The input text.
    - `separator`: The separator to use for special characters and spaces.
  - Returns: The text with special characters replaced by the separator.

### SlugOptions

- **Namespace:** `SmartWorkz.Shared.SlugOptions`
- **Summary:** Options for configuring slug generation behavior in .

### TextHelper

- **Namespace:** `SmartWorkz.Shared.TextHelper`
- **Summary:** Sealed class providing advanced text processing and formatting utilities.
            All methods return Result<string> for consistent error handling.

#### Methods & Properties

- **Truncate** - Truncates text to a maximum length and appends a suffix (default "...").
  - Parameters:
    - `text`: The input text to truncate.
    - `maxLength`: The maximum length including the suffix.
    - `suffix`: The suffix to append when truncating. Defaults to "...".
  - Returns: A Result containing the truncated text or an error.
- **Capitalize** - Capitalizes the first letter of the text while preserving the rest.
  - Parameters:
    - `text`: The input text to capitalize.
  - Returns: A Result containing the capitalized text or an error.
- **Decapitalize** - Decapitalizes the first letter of the text while preserving the rest.
  - Parameters:
    - `text`: The input text to decapitalize.
  - Returns: A Result containing the decapitalized text or an error.
- **StripHtml** - Removes HTML tags from the input string using regex.
  - Parameters:
    - `html`: The HTML string to process.
  - Returns: A Result containing the plain text with HTML tags removed or an error.
- **Pluralize** - Pluralizes a word based on count using a simple heuristic.
            If count == 1, returns singular form. Otherwise appends 's'.
  - Parameters:
    - `singular`: The singular form of the word.
    - `count`: The count to determine plural form.
  - Returns: A Result containing the appropriately pluralized word or an error.
- **TitleCase** - Converts text to title case by capitalizing the first letter of each word.
  - Parameters:
    - `text`: The input text to convert.
  - Returns: A Result containing the title-cased text or an error.
- **Reverse** - Reverses the input string.
  - Parameters:
    - `text`: The input text to reverse.
  - Returns: A Result containing the reversed text or an error.
- **RemoveWhitespace** - Removes all whitespace characters from the input string.
  - Parameters:
    - `text`: The input text to process.
  - Returns: A Result containing the text with all whitespace removed or an error.
- **WordWrap** - Wraps text at a specified line length while preserving word boundaries.
  - Parameters:
    - `text`: The input text to wrap.
    - `lineLength`: The maximum length of each line.
    - `newline`: The newline character(s) to use. Defaults to "\n".
  - Returns: A Result containing the word-wrapped text or an error.
- **Repeat** - Repeats the input string the specified number of times.
  - Parameters:
    - `text`: The input text to repeat.
    - `count`: The number of times to repeat the text.
  - Returns: A Result containing the repeated text or an error.

### CompositeValidator`1

- **Namespace:** `SmartWorkz.Shared.CompositeValidator`1`
- **Summary:** Combines multiple validators into a single validator.
            Useful for composing validators from different sources.

### IValidationRule`2

- **Namespace:** `SmartWorkz.Shared.IValidationRule`2`
- **Summary:** Single validation rule for a property.

#### Methods & Properties

- **ValidateAsync** - Validate property and return results.

### ValidationRule`2

- **Namespace:** `SmartWorkz.Shared.ValidationRule`2`
- **Summary:** Base implementation for custom validation rules.

### ValidationRules

- **Namespace:** `SmartWorkz.Shared.ValidationRules`
- **Summary:** Pre-built validation rules for common scenarios.

### ValidatorBuilder`1

- **Namespace:** `SmartWorkz.Shared.ValidatorBuilder`1`
- **Summary:** Fluent validator builder for defining validation rules.
            Provides an alternative to ValidatorBase for more concise validator definitions.

#### Methods & Properties

- **RuleFor``1** - Add a rule for a property using fluent API.
- **ValidateAsync** - Validate instance against all rules.

### IWebhookRegistry

- **Namespace:** `SmartWorkz.Shared.IWebhookRegistry`
- **Summary:** Abstraction for managing webhook subscriptions and registrations.
            Supports CRUD operations and subscription queries.

#### Methods & Properties

- **RegisterAsync** - Register a new webhook subscription.
  - Parameters:
    - `url`: The webhook endpoint URL.
    - `events`: Array of event names to subscribe to.
    - `secret`: Optional HMAC-SHA256 secret for signature verification.
    - `cancellationToken`: Cancellation token.
  - Returns: The ID of the newly registered subscription.
- **UnregisterAsync** - Unregister and remove a webhook subscription.
  - Parameters:
    - `subscriptionId`: The subscription ID to unregister.
    - `cancellationToken`: Cancellation token.
- **GetSubscriptionsForEventAsync** - Get all active subscriptions for a specific event.
  - Parameters:
    - `eventName`: The event name to filter by.
    - `cancellationToken`: Cancellation token.
  - Returns: Collection of subscriptions interested in this event.
- **GetActiveSubscriptionsAsync** - Get all currently active subscriptions.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: Collection of all active subscriptions.
- **UpdateSubscriptionStatusAsync** - Update the status and failure tracking of a subscription.
  - Parameters:
    - `subscriptionId`: The subscription ID to update.
    - `isActive`: Whether the subscription should remain active.
    - `failureCount`: Number of consecutive failures (null to leave unchanged).
    - `failureReason`: Reason for failure (null to clear).
    - `cancellationToken`: Cancellation token.

### SqlWebhookRegistry

- **Namespace:** `SmartWorkz.Shared.SqlWebhookRegistry`
- **Summary:** SQL Server implementation of IWebhookRegistry.
            Persists webhook subscriptions to the database with support for querying and status updates.

### WebhookDeliveryService

- **Namespace:** `SmartWorkz.Shared.WebhookDeliveryService`
- **Summary:** Service for publishing domain events to registered webhook endpoints.
            Implements exponential backoff retry logic, HMAC signature verification, and failure tracking.

#### Methods & Properties

- **PublishEventAsync** - Publish an event to all subscribed webhook endpoints.
  - Parameters:
    - `eventName`: The name of the event being published.
    - `payload`: The event payload to send.
    - `cancellationToken`: Cancellation token.
- **DeliverAsync** - Deliver an event to a single webhook endpoint with exponential backoff retry logic.
- **GenerateSignature** - Generate HMAC-SHA256 signature for webhook payload verification.

### AuditEntry

- **Namespace:** `SmartWorkz.Shared.AuditEntry`
- **Summary:** Immutable audit log entry for tracking entity changes and domain events.
            Records who did what, when, where, and why for compliance and debugging.

### AuditEventSubscriber

- **Namespace:** `SmartWorkz.Shared.AuditEventSubscriber`
- **Summary:** Subscribes to domain events and records them in the audit trail.
            Enables automatic audit capture without requiring explicit audit calls in business logic.

#### Methods & Properties

- **OnEventPublishedAsync** - Record a domain event in the audit trail.
  - Parameters:
    - `evt`: The domain event to record.
    - `userId`: User ID who triggered the event (optional for system events).
    - `ipAddress`: IP address of the request originator (optional).
    - `cancellationToken`: Cancellation token.

### AuditStartupExtensions

- **Namespace:** `SmartWorkz.Shared.AuditStartupExtensions`
- **Summary:** Dependency injection and schema setup for audit trail functionality.

#### Methods & Properties

- **AddAuditTrail** - Register IAuditTrail with SQL Server implementation.
- **CreateAuditTrailSchema** - Create the AuditTrail table and indexes if they don't exist.
            Call this during application startup or migration.

### IAuditTrail

- **Namespace:** `SmartWorkz.Shared.IAuditTrail`
- **Summary:** Service for recording and querying immutable audit entries.
            Abstracts the persistence mechanism for audit trails.

#### Methods & Properties

- **RecordAsync** - Record an audit entry (immutable append-only).
  - Parameters:
    - `entry`: The audit entry to record.
    - `cancellationToken`: Cancellation token.
- **GetEntriesAsync** - Get all audit entries for a specific entity instance.
  - Parameters:
    - `entityType`: Type of entity (e.g., "Order").
    - `entityId`: Entity instance ID.
    - `cancellationToken`: Cancellation token.
- **GetEntriesByActionAsync** - Get audit entries by action type (Created, Updated, Deleted, etc.).
  - Parameters:
    - `action`: The action to filter by.
    - `since`: Optional: only entries after this date.
    - `cancellationToken`: Cancellation token.
- **GetEntriesByUserAsync** - Get audit entries for a specific user.
  - Parameters:
    - `userId`: User ID who performed actions.
    - `since`: Optional: only entries after this date.
    - `cancellationToken`: Cancellation token.
- **SearchAsync** - Search audit trail with multiple filter criteria.
            All criteria are AND'd together (null criteria are ignored).
  - Parameters:
    - `entityType`: Optional entity type filter.
    - `action`: Optional action filter.
    - `userId`: Optional user ID filter.
    - `since`: Optional timestamp filter (inclusive).
    - `cancellationToken`: Cancellation token.

### SqlAuditTrail

- **Namespace:** `SmartWorkz.Shared.SqlAuditTrail`
- **Summary:** SQL Server implementation of IAuditTrail for immutable audit log persistence.
            Appends audit entries to a single table with indexes for efficient querying.

#### Methods & Properties

- **RecordAsync** - 
- **GetEntriesAsync** - 
- **GetEntriesByActionAsync** - 
- **GetEntriesByUserAsync** - 
- **SearchAsync** - 

### ValueConverter`1

- **Namespace:** `SmartWorkz.Shared.ValueConverter`1`
- **Summary:** Abstract base class for type conversion between domain objects and DTOs.
            Enables loose coupling between layers by centralizing conversion logic.

#### Methods & Properties

- **Convert``1** - Convert a single source object to target type.
- **Convert** - Convert a single source object using dynamic target type resolution.
- **ConvertList``1** - Convert a collection of source objects to target type.
- **ConvertList** - Convert a collection using dynamic target type resolution.
- **ConvertFromList``2** - Convert from a collection of different source types.

### CacheEntry`1

- **Namespace:** `SmartWorkz.Shared.CacheEntry`1`
- **Summary:** Represents a cached entry with data, expiration time, and metadata.

#### Methods & Properties

- **#ctor** - Creates a new CacheEntry instance.
- **#ctor** - Creates a new CacheEntry instance with data and expiration.
- **RenewExpiry** - Renews the expiry time based on the cache strategy and TTL.

### CacheEntryWrapper

- **Namespace:** `SmartWorkz.Shared.CacheEntryWrapper`
- **Summary:** Non-generic wrapper for CacheEntry to store in the cache dictionary.

### CacheOptions

- **Namespace:** `SmartWorkz.Shared.CacheOptions`
- **Summary:** Configuration options for cache operations.

#### Methods & Properties

- **#ctor** - Creates a new CacheOptions instance with default values.
- **#ctor** - Creates a new CacheOptions instance with specified TTL.
- **#ctor** - Creates a new CacheOptions instance with specified TTL and cache strategy.
- **#ctor** - Creates a new CacheOptions instance with all parameters.

### CacheStrategy

- **Namespace:** `SmartWorkz.Shared.CacheStrategy`
- **Summary:** Enumeration of cache expiration strategies.

### ICacheService

- **Namespace:** `SmartWorkz.Shared.ICacheService`
- **Summary:** Service for caching with tenant isolation and L1/L2 hybrid support.
            Implementations may use memory cache (L1) and distributed cache (L2).
            All cache operations are tenant-scoped with automatic key prefixing.

#### Methods & Properties

- **GetAsync``1** - Gets a cached value by key with tenant isolation.
  - Parameters:
    - `key`: Cache key (will be tenant-scoped automatically).
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Result containing cached value, or failure if not found or expired.
- **SetAsync``1** - Sets a value in cache with optional TTL for the specified tenant.
  - Parameters:
    - `key`: Cache key (will be tenant-scoped automatically).
    - `value`: Value to cache.
    - `ttlMinutes`: Time to live in minutes. If null, value never expires.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveAsync** - Removes a cached value by key for the specified tenant.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveByPrefixAsync** - Removes all cached values matching a key prefix for the specified tenant.
            Example: RemoveByPrefixAsync("user:") removes all "user:*" entries for that tenant.
  - Parameters:
    - `prefix`: Key prefix to match (may include wildcard suffix like "user:*").
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **ExistsAsync** - Checks if a key exists in cache for the specified tenant.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: True if key exists and is not expired; false otherwise.
- **ClearAsync** - Clears all cache entries for the specified tenant.
            Does not affect entries for other tenants.
  - Parameters:
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.

### ICacheStore

- **Namespace:** `SmartWorkz.Shared.ICacheStore`
- **Summary:** Abstraction for a cache store with support for various operations including TTL and expiration strategies.

#### Methods & Properties

- **GetAsync``1** - Retrieves a value from the cache.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result containing the cached value or null if not found or expired.
- **SetAsync``1** - Sets a value in the cache with optional TTL.
  - Parameters:
    - `key`: The cache key.
    - `value`: The value to cache.
    - `ttlMinutes`: Optional time-to-live in minutes. If null, uses default or no expiration.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **SetAsync``1** - Sets a value in the cache with cache options.
  - Parameters:
    - `key`: The cache key.
    - `value`: The value to cache.
    - `options`: Cache options including TTL, strategy, and sliding expiration.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **RemoveAsync** - Removes a value from the cache.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.
- **RemoveByPrefixAsync** - Removes all values from the cache that match the specified key prefix.
  - Parameters:
    - `keyPrefix`: The prefix to match.
    - `ct`: Cancellation token.
  - Returns: A Result containing the number of entries removed.
- **ExistsAsync** - Checks if a key exists in the cache and is not expired.
  - Parameters:
    - `key`: The cache key.
    - `ct`: Cancellation token.
  - Returns: A Result indicating whether the key exists and is valid.
- **ClearAsync** - Clears all entries from the cache.
  - Parameters:
    - `ct`: Cancellation token.
  - Returns: A Result indicating success or failure.

### MemoryCacheService

- **Namespace:** `SmartWorkz.Shared.MemoryCacheService`
- **Summary:** In-memory L1 cache service implementation with thread-safe operations and tenant isolation.
            Suitable for single-process deployments with TTL and expiration support.

#### Methods & Properties

- **BuildKey** - Builds a tenant-scoped cache key.
  - Parameters:
    - `key`: Original cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
  - Returns: Tenant-scoped key in format "{tenantId}:{key}".
- **GetAsync``1** - Gets a cached value by key with tenant isolation. Returns failure if not found or expired.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Result containing cached value, or failure if not found or expired.
- **SetAsync``1** - Sets a cached value with optional TTL expiration and tenant isolation.
  - Parameters:
    - `key`: Cache key.
    - `value`: Value to cache.
    - `ttlMinutes`: Time to live in minutes. If null, no expiration.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveAsync** - Removes a single cache entry with tenant isolation.
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **RemoveByPrefixAsync** - Removes all cache entries matching a prefix pattern with tenant isolation.
            Example: RemoveByPrefixAsync("user:*", "tenant1") removes "tenant1:user:*" entries.
  - Parameters:
    - `prefix`: Key prefix to match.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.
- **ExistsAsync** - Checks if a key exists in the cache with tenant isolation (ignores expiration check).
  - Parameters:
    - `key`: Cache key.
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: True if key exists and is not expired; false otherwise.
- **ClearAsync** - Clears all cache entries for the specified tenant (or "default" if not specified).
            Does not clear entries from other tenants.
  - Parameters:
    - `tenantId`: Tenant identifier. Defaults to "default" if null.
    - `cancellationToken`: Cancellation token.
  - Returns: Success result.

### MemoryCacheStore

- **Namespace:** `SmartWorkz.Shared.MemoryCacheStore`
- **Summary:** In-memory implementation of ICacheStore with TTL support and thread-safe operations.

#### Methods & Properties

- **#ctor** - Creates a new instance of MemoryCacheStore with default options.
- **#ctor** - Creates a new instance of MemoryCacheStore with specified default options.
- **GetAsync``1** - Retrieves a value from the cache.
- **SetAsync``1** - Sets a value in the cache with optional TTL.
- **SetAsync``1** - Sets a value in the cache with cache options.
- **RemoveAsync** - Removes a value from the cache.
- **RemoveByPrefixAsync** - Removes all values from the cache that match the specified key prefix.
- **ExistsAsync** - Checks if a key exists in the cache and is not expired.
- **ClearAsync** - Clears all entries from the cache.
- **CleanupExpiredEntries** - Performs cleanup of expired entries. This is useful for periodic maintenance.

### ISmsService

- **Namespace:** `SmartWorkz.Shared.ISmsService`
- **Summary:** Defines a contract for SMS communication services.
            Provides methods for sending SMS messages to single or multiple recipients.

#### Methods & Properties

- **SendAsync** - Sends an SMS message to a single recipient.
  - Parameters:
    - `phoneNumber`: The recipient phone number (E.164 format recommended)
    - `message`: The SMS message content
    - `cancellationToken`: Cancellation token
  - Returns: Result containing the SMS ID if successful
- **SendBatchAsync** - Sends an SMS message to multiple recipients (batch).
  - Parameters:
    - `phoneNumbers`: Collection of recipient phone numbers
    - `message`: The SMS message content sent to all recipients
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of SMS IDs if successful

### IWebSocketClient

- **Namespace:** `SmartWorkz.Shared.IWebSocketClient`
- **Summary:** Abstraction for WebSocket client operations.

#### Methods & Properties

- **SendAsync** - Sends a message asynchronously through the WebSocket connection.
- **ReceiveAsync** - Receives a message asynchronously from the WebSocket connection.
            Returns null if the connection is closed.
- **CloseAsync** - Closes the WebSocket connection gracefully.

### WebSocketClient

- **Namespace:** `SmartWorkz.Shared.WebSocketClient`
- **Summary:** Sealed implementation of IWebSocketClient using System.Net.WebSockets.

#### Methods & Properties

- **ConnectAsync** - Connects to a WebSocket server at the specified URI.
- **SendAsync** - Sends a message asynchronously through the WebSocket connection.
- **ReceiveAsync** - Receives a message asynchronously from the WebSocket connection.
            Returns null if the connection is closed.
- **CloseAsync** - Closes the WebSocket connection gracefully.

### ConfigurationHelper

- **Namespace:** `SmartWorkz.Shared.ConfigurationHelper`
- **Summary:** Provides typed access to configuration values with automatic type conversion and validation.
            
             This sealed class implements IConfigurationHelper to provide a strongly-typed interface
             for accessing configuration values. It supports automatic type conversion for common types
             including strings, numeric types, booleans, DateTimes, and enums.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ConfigurationHelper class.
  - Parameters:
    - `configuration`: The configuration source to read from.
- **GetRequired``1** - Gets a required configuration value and converts it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: The configuration value converted to type T.
- **GetOptional``1** - Gets an optional configuration value and converts it to the specified type,
            returning a default value if the key is not found or conversion fails.
  - Parameters:
    - `key`: The configuration key to retrieve.
    - `defaultValue`: The value to return if the key is not found or conversion fails.
  - Returns: The configuration value converted to type T, or the default value if retrieval or conversion fails.
- **TryGet``1** - Attempts to get a configuration value and convert it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: A Result<T> containing the converted value on success, or a failure result
            if the key is not found or conversion fails.
- **Exists** - Checks whether a configuration key exists and is not empty.
  - Parameters:
    - `key`: The configuration key to check.
  - Returns: True if the key exists and is not null or whitespace; otherwise false.
- **ConvertValue``1** - Converts a string value to the specified type using invariant culture for numeric types.
  - Parameters:
    - `value`: The string value to convert.
  - Returns: The converted value of type T.

### ConfigurationValidationException

- **Namespace:** `SmartWorkz.Shared.ConfigurationValidationException`
- **Summary:** Exception thrown when configuration validation fails, indicating that a required
            configuration key is missing, empty, or cannot be converted to the requested type.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ConfigurationValidationException class with a specified message.
  - Parameters:
    - `message`: The error message that explains the reason for the exception.
- **#ctor** - Initializes a new instance of the ConfigurationValidationException class with a specified message
            and a reference to the inner exception that is the cause of this exception.
  - Parameters:
    - `message`: The error message that explains the reason for the exception.
    - `innerException`: The exception that is the cause of the current exception.

### IConfigurationHelper

- **Namespace:** `SmartWorkz.Shared.IConfigurationHelper`
- **Summary:** Provides typed access to configuration values with automatic type conversion and validation.

#### Methods & Properties

- **GetRequired``1** - Gets a required configuration value and converts it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: The configuration value converted to type T.
- **GetOptional``1** - Gets an optional configuration value and converts it to the specified type,
            returning a default value if the key is not found or conversion fails.
  - Parameters:
    - `key`: The configuration key to retrieve.
    - `defaultValue`: The value to return if the key is not found or conversion fails.
  - Returns: The configuration value converted to type T, or the default value if retrieval or conversion fails.
- **TryGet``1** - Attempts to get a configuration value and convert it to the specified type.
  - Parameters:
    - `key`: The configuration key to retrieve.
  - Returns: A Result<T> containing the converted value on success, or a failure result
            if the key is not found or conversion fails.
- **Exists** - Checks whether a configuration key exists and is not empty.
  - Parameters:
    - `key`: The configuration key to check.
  - Returns: True if the key exists and is not null or whitespace; otherwise false.

### SharedConstants

- **Namespace:** `SmartWorkz.Shared.SharedConstants`
- **Summary:** Shared configuration constants used throughout SmartWorkz.Shared.
            Enables centralized management of default values and limits.

### ICommand

- **Namespace:** `SmartWorkz.Shared.ICommand`
- **Summary:** Marker interface for command objects representing intent to change state.

### ICommandHandler`1

- **Namespace:** `SmartWorkz.Shared.ICommandHandler`1`
- **Summary:** Handler for processing a specific command type.

#### Methods & Properties

- **HandleAsync** - Handles the specified command asynchronously.
  - Parameters:
    - `command`: The command to handle.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task that represents the asynchronous handle operation.

### IQuery`1

- **Namespace:** `SmartWorkz.Shared.IQuery`1`
- **Summary:** Marker interface for query objects that return a result without modifying state.

### IQueryHandler`2

- **Namespace:** `SmartWorkz.Shared.IQueryHandler`2`
- **Summary:** Handler for processing a specific query type and returning results.

#### Methods & Properties

- **HandleAsync** - Handles the specified query asynchronously and returns the result.
  - Parameters:
    - `query`: The query to handle.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task that represents the asynchronous handle operation and contains the query result.

### MediatorCommandDispatcher

- **Namespace:** `SmartWorkz.Shared.MediatorCommandDispatcher`
- **Summary:** Routes commands to their appropriate handlers via dependency injection.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `serviceProvider`: The service provider for resolving handlers.
- **DispatchAsync``1** - Dispatches the specified command to its handler asynchronously.
  - Parameters:
    - `command`: The command to dispatch.
    - `cancellationToken`: Cancellation token to support graceful shutdown.
  - Returns: A task representing the asynchronous dispatch operation.

### AdoHelper

- **Namespace:** `SmartWorkz.Shared.AdoHelper`
- **Summary:** ADO.NET helper for executing queries and managing connections.
            Works with any IDbProvider implementation.

#### Methods & Properties

- **ExecuteScalarAsync``1** - Execute scalar query (returns single value).
- **ExecuteNonQueryAsync** - Execute non-query command (INSERT, UPDATE, DELETE).
- **ExecuteQueryAsync``1** - Execute query and map results to objects.
- **ExecuteStoredProcedureAsync** - Execute stored procedure.
- **ExecuteQueryMultipleAsync``2** - Execute query returning multiple result sets (2 sets).
- **ExecuteQueryMultipleAsync``3** - Execute query returning multiple result sets (3 sets).
- **ExecuteQueryMultipleAsync``4** - Execute query returning multiple result sets (4 sets).
- **ExecuteTransactionAsync** - Execute transaction with multiple commands.

### CsvHelper

- **Namespace:** `SmartWorkz.Shared.CsvHelper`
- **Summary:** Provides static methods for reading and writing CSV data with support for column mapping,
            quoted fields, embedded delimiters, and newlines.
            RFC 4180 compliant CSV parsing and writing.
            Sealed class to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **CsvWriter``1** - Serializes a collection of objects to CSV format.
  - Parameters:
    - `items`: The collection of objects to serialize.
    - `options`: CSV options. If null, defaults are used.
  - Returns: A Result containing the CSV string if successful; otherwise a failure.
- **CsvReader``1** - Asynchronously deserializes CSV content to a collection of objects.
  - Parameters:
    - `content`: The CSV content string.
    - `mapping`: Column mapping configuration. If null, property names are used as headers.
    - `options`: CSV options. If null, defaults are used.
  - Returns: A Result containing the deserialized list if successful; otherwise a failure.
- **ParseCsvLines** - Parses CSV content into a list of records (each record is a list of field values).
            Handles quoted fields with embedded delimiters and newlines.
- **WriteRecord** - Writes a single CSV record (list of field values) to the string builder.
            Handles quoting of fields with special characters.
- **ConvertValue** - Converts a string value to the specified type.
- **IsNullableType** - Determines if a type is nullable (Nullable<T> or reference type).

### CsvMapping`1

- **Namespace:** `SmartWorkz.Shared.CsvMapping`1`
- **Summary:** Defines column mapping for CSV operations using a fluent API.
            Supports mapping object properties to CSV columns with custom headers.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **Column``1** - Adds a column mapping for the specified property.
  - Parameters:
    - `propertyExpression`: Expression selecting the property to map.
    - `csvHeader`: The CSV column header name.
  - Returns: This instance for method chaining.
- **ExtractPropertyInfo``1** - Extracts property information from a lambda expression.
  - Parameters:
    - `expression`: The lambda expression.
  - Returns: The PropertyInfo if the expression resolves to a property; otherwise null.
- **CreateAuto** - Creates a mapping automatically from all public properties of type T.
            Property names are used as CSV headers.
  - Returns: A new CsvMapping instance with all properties mapped.

### CsvOptions

- **Namespace:** `SmartWorkz.Shared.CsvOptions`
- **Summary:** Configuration options for CSV read/write operations.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **#ctor** - Creates a default instance of CsvOptions.
- **#ctor** - Creates an instance of CsvOptions with specified delimiter and quote character.
  - Parameters:
    - `delimiter`: The field delimiter character.
    - `quoteChar`: The quote character for quoted fields.

### DbProviderFactory

- **Namespace:** `SmartWorkz.Shared.DbProviderFactory`
- **Summary:** Factory for creating database provider instances.
            Resolves provider name from connection string or explicit specification.

#### Methods & Properties

- **Register** - Register custom provider implementation.
- **GetProvider** - Get provider by name.
- **GetProvider** - Get provider by enum value.
- **GetProviderFromConnectionString** - Get provider from connection string (detects provider automatically).

### IDbProvider

- **Namespace:** `SmartWorkz.Shared.IDbProvider`
- **Summary:** Abstraction for database provider-specific operations.
            Supports multiple providers: SQL Server, MySQL, PostgreSQL, SQLite, Oracle.

#### Methods & Properties

- **CreateConnection** - Create connection with connection string.
- **GetParameterPrefix** - Get parameter prefix for this provider (@, :, $).
- **GetLastInsertIdSql** - Get SQL for last inserted ID based on provider.
- **GetPaginationSql** - Get SQL for pagination based on provider.
- **FormatIdentifier** - Format table/column name for provider (e.g., [brackets] for SQL Server).
- **TestConnectionAsync** - Test connection validity.

### DatabaseProvider

- **Namespace:** `SmartWorkz.Shared.DatabaseProvider`
- **Summary:** Enum of supported database providers.

### QueryMultipleHelper

- **Namespace:** `SmartWorkz.Shared.QueryMultipleHelper`
- **Summary:** Helper for executing multiple queries in a single database roundtrip.
            Eliminates N+1 query problems by batching queries together.

#### Methods & Properties

- **QueryMultipleAsync``2** - Execute multiple queries and return results as tuple.
             Single roundtrip, single SQL execution, improved performance.
- **QueryMultipleAsync``3** - Execute 3 queries in single roundtrip.
- **QueryMultipleAsync``4** - Execute 4 queries in single roundtrip.
- **QueryMultipleAsync``5** - Execute 5 queries in single roundtrip.

### QueryResult`1

- **Namespace:** `SmartWorkz.Shared.QueryResult`1`
- **Summary:** Result wrapper for query operations.

### XmlHelper

- **Namespace:** `SmartWorkz.Shared.XmlHelper`
- **Summary:** Provides static methods for XML serialization, deserialization, and XPath queries.
            Uses System.Xml.Linq for manipulation and reflection for property mapping.
            Sealed class to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **Serialize``1** - Serializes an object to an XML string using reflection.
  - Parameters:
    - `obj`: The object to serialize.
    - `options`: XML options. If null, defaults are used.
  - Returns: A Result containing the XML string if successful; otherwise a failure.
- **Deserialize``1** - Deserializes an XML string to an object of type T using reflection.
  - Parameters:
    - `xml`: The XML string to deserialize.
    - `options`: XML options. If null, defaults are used.
  - Returns: A Result containing the deserialized object if successful; otherwise a failure.
- **Query** - Executes an XPath query on an XML string and returns matching element values.
  - Parameters:
    - `xml`: The XML string to query.
    - `xpathExpression`: The XPath expression to execute.
  - Returns: A Result containing a list of matched values if successful; otherwise a failure.
- **SerializeObject** - Recursively serializes an object's properties into an XML element.
- **DeserializeObject** - Recursively deserializes an XML element into an object's properties.
- **IsBasicType** - Determines if a type is a basic/primitive type supported by XML.
- **IsGenericList** - Determines if a type is a generic List<T>.
- **IsComplexType** - Determines if a type is a complex (non-primitive) type.
- **ConvertToXmlValue** - Converts a value to its XML-safe string representation.
- **ConvertFromXmlValue** - Converts an XML string value to the specified type.

### XmlOptions

- **Namespace:** `SmartWorkz.Shared.XmlOptions`
- **Summary:** Configuration options for XML serialization, deserialization, and query operations.
            Sealed to prevent inheritance and ensure consistent behavior.

#### Methods & Properties

- **#ctor** - Creates a default instance of XmlOptions.
- **#ctor** - Creates an instance of XmlOptions with a specified root element name.
  - Parameters:
    - `rootElement`: The name of the root element.
- **#ctor** - Creates an instance of XmlOptions with specified configuration.
  - Parameters:
    - `rootElement`: The name of the root element.
    - `includeXmlDeclaration`: Whether to include the XML declaration.
    - `indent`: Whether to indent the output.

### ApplicationHealth

- **Namespace:** `SmartWorkz.Shared.ApplicationHealth`
- **Summary:** Represents the overall health status of the application.

### CorrelationContext

- **Namespace:** `SmartWorkz.Shared.CorrelationContext`
- **Summary:** A sealed implementation of  for distributed request tracing.

#### Methods & Properties

- **#ctor** - Initializes a new instance with a generated correlation ID.
- **#ctor** - Initializes a new instance with a specified correlation ID.
  - Parameters:
    - `correlationId`: The correlation ID to use
- **#ctor** - Initializes a child context from a parent context.

### CpuUsage

- **Namespace:** `SmartWorkz.Shared.CpuUsage`
- **Summary:** Represents CPU usage information.

### DiagnosticsHelper

- **Namespace:** `SmartWorkz.Shared.DiagnosticsHelper`
- **Summary:** Sealed helper class for system diagnostics and application health monitoring.
            Provides methods to gather system information, CPU/memory/disk usage, and determine application health.

#### Methods & Properties

- **Initialize** - Initializes the application start time (called once at application startup).
- **GetSystemInfo** - Gets comprehensive system information including CPU, memory, disk, and processor count.
  - Returns: A Result containing SystemInfo or error details.
- **GetMemoryUsage** - Gets memory usage statistics for the current process and system.
  - Returns: A Result containing MemoryUsage or error details.
- **GetCpuUsage** - Gets CPU utilization percentage.
  - Returns: A Result containing CpuUsage or error details.
- **GetDiskSpace** - Gets disk space information for a specific drive.
  - Parameters:
    - `drive`: The drive letter (e.g., "C:", "D:"). Defaults to "C:".
  - Returns: A Result containing DiskSpace or error details.
- **GetUptime** - Gets the application uptime since the last Initialize() call or application start.
  - Returns: A Result containing the uptime as a TimeSpan or error details.
- **GetApplicationHealth** - Gets the overall health status of the application based on system metrics.
  - Returns: A Result containing ApplicationHealth or error details.
- **IsHealthy** - Determines if the application is considered healthy based on the provided health status.
  - Parameters:
    - `health`: The ApplicationHealth object to evaluate.
  - Returns: True if the status is Healthy, false otherwise.
- **InitializeCpuCounter** - Initializes the CPU performance counter (called once).
- **GetMemoryUsageInternal** - Internal method to get memory usage statistics.
- **GetCpuUsageInternal** - Internal method to get CPU usage percentage.
- **GetDiskSpaceInternal** - Internal method to get disk space information.

### DiskSpace

- **Namespace:** `SmartWorkz.Shared.DiskSpace`
- **Summary:** Represents disk space information for a drive.

### HealthCheck

- **Namespace:** `SmartWorkz.Shared.HealthCheck`
- **Summary:** Represents a single health check result.

### HealthStatus

- **Namespace:** `SmartWorkz.Shared.HealthStatus`
- **Summary:** Represents the health status of the application.

### ICorrelationContext

- **Namespace:** `SmartWorkz.Shared.ICorrelationContext`
- **Summary:** Defines a correlation context for distributed request tracing across systems.

#### Methods & Properties

- **SetProperty** - Adds or updates a property in the correlation context.
- **TryGetProperty** - Attempts to retrieve a property from the correlation context.
- **CreateChildContext** - Creates a child correlation context for nested operations (for async/distributed flows).

### MemoryUsage

- **Namespace:** `SmartWorkz.Shared.MemoryUsage`
- **Summary:** Represents memory usage information.

### MetricsHelper

- **Namespace:** `SmartWorkz.Shared.MetricsHelper`
- **Summary:** Provides utilities for collecting and tracking performance metrics.

#### Methods & Properties

- **StartTimer** - Starts a timer and returns an IDisposable that logs elapsed time on disposal.
- **TrackExecution``1** - Tracks the execution time and result of a function.
- **MeasureMemory** - Captures memory usage before and after a block of code execution.

### SystemInfo

- **Namespace:** `SmartWorkz.Shared.SystemInfo`
- **Summary:** Represents system information including CPU, memory, and disk details.

### EventStoreSnapshot

- **Namespace:** `SmartWorkz.Shared.EventStoreSnapshot`
- **Summary:** Represents a snapshot of an aggregate's state at a specific version.
            Snapshots optimize event sourcing by reducing the number of events needed for reconstruction.

### IEventStore

- **Namespace:** `SmartWorkz.Shared.IEventStore`
- **Summary:** Abstraction for an immutable event store that persists domain events.
            Enables event sourcing patterns for temporal queries, audit trails, and event replay.

#### Methods & Properties

- **AppendEventsAsync** - Appends events to the event stream for an aggregate.
            Events are immutable and persist as an append-only log.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `events`: The domain events to append
    - `cancellationToken`: Cancellation token
  - Returns: Task representing the asynchronous operation
- **GetEventsAsync** - Retrieves all events for an aggregate in chronological order.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: Collection of domain events for the aggregate, empty if none exist
- **GetEventsSinceAsync** - Retrieves events for an aggregate after a specific version.
            Useful for incremental event replay and event streaming.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `version`: The version after which to retrieve events
    - `cancellationToken`: Cancellation token
  - Returns: Collection of events after the specified version, empty if none exist
- **GetSnapshotAsync** - Retrieves the latest snapshot for an aggregate if one exists.
            Snapshots optimize aggregate reconstruction by storing intermediate state.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: Snapshot data if exists; null if no snapshot is available
- **SaveSnapshotAsync** - Saves a snapshot of aggregate state at a specific version.
            Snapshots reduce the number of events needed to replay an aggregate.
  - Parameters:
    - `snapshot`: The snapshot to save
    - `cancellationToken`: Cancellation token
  - Returns: Task representing the asynchronous operation
- **GetAggregateAsync``1** - Reconstructs an aggregate from its event history.
            Optionally uses snapshots for performance optimization.
  - Parameters:
    - `aggregateId`: The unique identifier of the aggregate root
    - `cancellationToken`: Cancellation token
  - Returns: The reconstructed aggregate instance, or null if no events exist

### SqlEventStore

- **Namespace:** `SmartWorkz.Shared.SqlEventStore`
- **Summary:** SQL Server implementation of the event store using Dapper for data access.
            Provides immutable append-only event log with snapshot support for optimization.
            Implements optimistic concurrency control using version numbers.

#### Methods & Properties

- **AppendEventsAsync** - Appends events to the event stream for an aggregate.
- **GetEventsAsync** - Retrieves all events for an aggregate in chronological order.
- **GetEventsSinceAsync** - Retrieves events for an aggregate after a specific version.
- **GetSnapshotAsync** - Retrieves the latest snapshot for an aggregate if one exists.
- **SaveSnapshotAsync** - Saves a snapshot of aggregate state at a specific version.
- **GetAggregateAsync``1** - Reconstructs an aggregate from its event history.
            Optionally uses snapshots for performance optimization.
- **GetCurrentVersionAsync** - Gets the current version number for an aggregate.
- **DeserializeEvent** - Deserializes a stored event record back to IDomainEvent.

### IDomainEvent

- **Namespace:** `SmartWorkz.Shared.IDomainEvent`
- **Summary:** Base interface for domain events in event-driven architecture.
            Provides core event metadata for tracking and publishing.

### IEventPublisher

- **Namespace:** `SmartWorkz.Shared.IEventPublisher`
- **Summary:** Publishes domain events for event-driven architecture.

#### Methods & Properties

- **PublishAsync``1** - Publishes a single domain event.
  - Parameters:
    - `event`: Event to publish.
    - `cancellationToken`: Cancellation token.
- **PublishAsync``1** - Publishes multiple domain events.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token.

### IEventSubscriber

- **Namespace:** `SmartWorkz.Shared.IEventSubscriber`
- **Summary:** Registers event handlers for domain events.

#### Methods & Properties

- **Subscribe``1** - Subscribes a handler to an event type.
            Multiple handlers can subscribe to the same event.
  - Parameters:
    - `handler`: Async handler function. Receives event and cancellation token.

### InMemoryEventPublisher

- **Namespace:** `SmartWorkz.Shared.InMemoryEventPublisher`
- **Summary:** In-memory event publisher that executes all registered handlers sequentially.
            Provides synchronous event delivery with exception handling and result reporting.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the InMemoryEventPublisher with a subscriber.
  - Parameters:
    - `subscriber`: The event subscriber containing registered handlers.
- **PublishAsync``1** - Publishes a single domain event to all registered handlers.
            Handlers are invoked sequentially in registration order.
            If any handler throws an exception, it is caught and a failure Result is returned.
            Other handlers will attempt to execute even if a previous handler fails.
  - Parameters:
    - `event`: The event instance to publish.
    - `cancellationToken`: Cancellation token to cancel handler execution.
  - Returns: A task representing the asynchronous operation.
- **PublishAsync``1** - Publishes multiple domain events to all registered handlers.
            Events are published sequentially in the order provided.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token to cancel handler execution.
  - Returns: A task representing the asynchronous batch operation.

### InMemoryEventSubscriber

- **Namespace:** `SmartWorkz.Shared.InMemoryEventSubscriber`
- **Summary:** In-memory event subscriber that maintains a registry of event handlers.
            Supports multiple handlers per event type using thread-safe concurrent collections.

#### Methods & Properties

- **Subscribe``1** - Subscribes a handler to an event type.
            Multiple handlers can be registered for the same event type and will execute sequentially.
  - Parameters:
    - `handler`: Async handler function that receives event and cancellation token.
- **GetHandlers** - Gets all registered handlers for a given event type.
            Returns an empty list if no handlers are registered for the type.
  - Parameters:
    - `eventType`: The event type to retrieve handlers for.
  - Returns: List of registered handlers (delegates).

### MassTransitEventPublisher

- **Namespace:** `SmartWorkz.Shared.MassTransitEventPublisher`
- **Summary:** Distributed event publisher using MassTransit message bus.
            Supports both single and batch event publishing with async/await patterns.
            Suitable for production environments with message broker backend (RabbitMQ, Azure Service Bus, etc).

#### Methods & Properties

- **#ctor** - Initializes a new instance of MassTransitEventPublisher.
  - Parameters:
    - `publishEndpoint`: MassTransit publish endpoint for message distribution.
    - `logger`: Logger for event publication tracking.
- **PublishAsync``1** - Publishes a single domain event to the message bus asynchronously.
  - Parameters:
    - `event`: Event to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous publish operation.
- **PublishAsync``1** - Publishes multiple domain events to the message bus asynchronously.
            Events are published sequentially in the order provided.
  - Parameters:
    - `events`: Collection of events to publish.
    - `cancellationToken`: Cancellation token.
  - Returns: Task representing the asynchronous batch publish operation.

### PublisherType

- **Namespace:** `SmartWorkz.Shared.PublisherType`
- **Summary:** Specifies the publisher type for event publishing.

### ServiceCollectionExtensions

- **Namespace:** `SmartWorkz.Shared.ServiceCollectionExtensions`
- **Summary:** Extension methods for IServiceCollection to register Core.Shared services.

#### Methods & Properties

- **AddCoreSharedServices** - Adds Core.Shared services including TemplateEngine for template rendering.
- **AddEventPublishing** - Adds event publishing services to the dependency injection container.
            Supports switching between in-memory and MassTransit publishers based on application needs.
  - Parameters:
    - `services`: The service collection.
    - `publisherType`: The publisher type to use (defaults to InMemory).
  - Returns: The service collection for method chaining.

### DefaultFeatureFlagService

- **Namespace:** `SmartWorkz.Shared.DefaultFeatureFlagService`
- **Summary:** Global (non-tenant) feature flag service with in-memory storage.
            Thread-safe implementation suitable for single-process deployments.
            Use for organization-wide feature toggles; use ITenantFeatureFlags for tenant-scoped flags.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature flag is enabled.
            Returns false for unknown flags (does not throw).
  - Parameters:
    - `flagName`: The name of the feature flag to check.
    - `cancellationToken`: Cancellation token.
  - Returns: True if the flag exists and is enabled; false otherwise.
- **GetEnabledFeaturesAsync** - Gets all enabled feature flags.
            Returns empty list if no flags are enabled.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: A read-only list of enabled feature flag names.
- **EnableFlag** - Enables a feature flag.
            Creates the flag if it doesn't exist.
  - Parameters:
    - `flagName`: The name of the feature flag to enable.
- **DisableFlag** - Disables a feature flag.
            Creates the flag as disabled if it doesn't exist.
  - Parameters:
    - `flagName`: The name of the feature flag to disable.

### IFeatureFlagService

- **Namespace:** `SmartWorkz.Shared.IFeatureFlagService`
- **Summary:** Global feature flag service for cross-tenant feature control.
            Use for organization-wide feature toggles (not tenant-specific).
            For tenant-scoped flags, use ITenantFeatureFlags.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a global feature is enabled.
  - Parameters:
    - `flagName`: Feature flag name (e.g., "NEW_DASHBOARD", "BETA_REPORTING").
    - `cancellationToken`: Cancellation token.
  - Returns: True if feature is enabled globally.
- **GetEnabledFeaturesAsync** - Gets all enabled global features.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: List of enabled feature flag names.

### IFileStorageService

- **Namespace:** `SmartWorkz.Shared.IFileStorageService`
- **Summary:** Interface for file storage operations supporting both local and cloud providers.

#### Methods & Properties

- **UploadAsync** - Uploads a file to storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `content`: The file content stream.
    - `metadata`: The file metadata.
    - `cancellationToken`: The cancellation token.
  - Returns: The full path or URI of the uploaded file.
- **DownloadAsync** - Downloads a file from storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: A stream containing the file content. Caller must dispose using 'using' statement.
- **DeleteAsync** - Deletes a file from storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
- **ExistsAsync** - Checks if a file exists in storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: True if the file exists, false otherwise.
- **GetMetadataAsync** - Gets metadata for a file in storage.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `cancellationToken`: The cancellation token.
  - Returns: FileMetadata if file exists, null otherwise.
- **ListAsync** - Lists files in a directory or container prefix.
  - Parameters:
    - `folderPath`: The relative folder path or blob prefix.
    - `cancellationToken`: The cancellation token.
  - Returns: A read-only collection of FileMetadata for files in the directory/prefix.
- **GenerateTemporaryUrlAsync** - Generates a temporary download URL for a file.
  - Parameters:
    - `path`: The relative path or blob name for the file.
    - `expiration`: The expiration duration from now.
    - `cancellationToken`: The cancellation token.
  - Returns: A URL that can be used to download the file. For local storage, returns the full file path.

### GridColumn

- **Namespace:** `SmartWorkz.Shared.GridColumn`
- **Summary:** Defines a single column in a grid, including display options, sorting, filtering, and rendering hints.

### GridExportOptions

- **Namespace:** `SmartWorkz.Shared.GridExportOptions`
- **Summary:** Configuration for grid data export (CSV, Excel).

### GridRequest

- **Namespace:** `SmartWorkz.Shared.GridRequest`
- **Summary:** Request parameters for grid data fetching, extending PagedQuery with filtering support.

#### Methods & Properties

- **#ctor** - Request parameters for grid data fetching, extending PagedQuery with filtering support.

### GridResponse`1

- **Namespace:** `SmartWorkz.Shared.GridResponse`1`
- **Summary:** Response from a grid data request, including paged data, column metadata, and filter options.

### IGridDataProvider

- **Namespace:** `SmartWorkz.Shared.IGridDataProvider`
- **Summary:** Abstraction for grid data fetching. Implementations handle API calls or in-memory queries.
            Enables platform independence: Web uses HTTP, MAUI uses direct API client, Desktop uses local DB.

#### Methods & Properties

- **GetDataAsync``1** - Fetch paged grid data based on request (sorting, filtering, pagination).
  - Parameters:
    - `request`: Grid request with sorting, paging, and filter criteria.
    - `cancellationToken`: Cancellation token for async operations.
  - Returns: Result containing GridResponse or error details.

### Guard

- **Namespace:** `SmartWorkz.Shared.Guard`
- **Summary:** Static guard clauses for argument validation at method entry points.
             Throw immediately on invalid input — fail fast, fail loudly.
            
             Usage:
               Guard.NotNull(userId, nameof(userId));
               Guard.NotEmpty(name, nameof(name));
               Guard.InRange(pageSize, 1, 100, nameof(pageSize));
            
             These replace the ValidationExtensions.EnsureNotNull() extension method
             and the scattered ArgumentNullException throws throughout the codebase.

#### Methods & Properties

- **NotNull``1** - Throws ArgumentNullException if value is null.
- **NotNull``1** - Throws ArgumentNullException if value is null (struct/nullable).
- **NotEmpty** - Throws ArgumentException if string is null, empty, or whitespace.
- **NotEmpty``1** - Throws ArgumentException if collection is null or has no elements.
- **NotDefault``1** - Throws ArgumentException if value equals the default for its type (0, null, Guid.Empty).
- **InRange``1** - Throws ArgumentOutOfRangeException if value is outside [min, max].
- **Requires** - Throws ArgumentException if condition is false.

### EncryptionHelper

- **Namespace:** `SmartWorkz.Shared.EncryptionHelper`
- **Summary:** Cryptographic utilities for hashing and encryption.
            Uses PBKDF2 for password hashing and AES-256 for data encryption.

#### Methods & Properties

- **HashPassword** - Hash password using PBKDF2 with SHA256.
- **VerifyPassword** - Verify password against hash.
- **Encrypt** - Encrypt text using AES-256-GCM with provided key.
- **Decrypt** - Decrypt text using AES-256-GCM with provided key.
- **GenerateRandomString** - Generate cryptographically secure random string.
- **GenerateEncryptionKey** - Generate random encryption key (Base64 encoded).
- **ComputeSha256** - Compute SHA256 hash of text for integrity checking.

### JsonHelper

- **Namespace:** `SmartWorkz.Shared.JsonHelper`
- **Summary:** JSON serialization utilities using System.Text.Json.
            Provides consistent serialization options across the application.

#### Methods & Properties

- **Serialize``1** - Serialize object to JSON string.
- **Serialize** - Serialize object to JSON string with dynamic type.
- **Deserialize``1** - Deserialize JSON string to object.
- **Deserialize** - Deserialize JSON string to object with dynamic type.
- **DeserializeAsync``1** - Deserialize JSON asynchronously from stream.
- **SerializeAsync``1** - Serialize asynchronously to stream.
- **IsValidJson** - Check if string is valid JSON.
- **GetValueByPath** - Parse JSON and extract value at specified path (dot notation).

### IHttpClient

- **Namespace:** `SmartWorkz.Shared.IHttpClient`
- **Summary:** Abstraction for HTTP client operations with support for async/await and cancellation.
            Implementations should handle retries, timeouts, and error responses gracefully.

#### Methods & Properties

- **GetAsync``1** - Sends a GET request and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **PostAsync``1** - Sends a POST request with JSON body and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **PutAsync``1** - Sends a PUT request with JSON body and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **DeleteAsync``1** - Sends a DELETE request and returns a typed response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with typed data.
- **GetAsync** - Sends a GET request and returns a string response.
  - Parameters:
    - `url`: The request URL.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.
- **PostAsync** - Sends a POST request with JSON body and returns a string response.
  - Parameters:
    - `url`: The request URL.
    - `body`: The request body to serialize as JSON.
    - `cancellationToken`: Cancellation token for the request.
  - Returns: Result containing the HTTP response with string data.

### RetryStrategy

- **Namespace:** `SmartWorkz.Shared.RetryStrategy`
- **Summary:** Specifies the backoff strategy to use when retrying failed HTTP requests.

### RetryPolicy

- **Namespace:** `SmartWorkz.Shared.RetryPolicy`
- **Summary:** Configures automatic retry behavior for failed HTTP requests.

### AuditRecord

- **Namespace:** `SmartWorkz.Shared.AuditRecord`
- **Summary:** Represents an immutable audit record with all relevant audit information.

#### Methods & Properties

- **#ctor** - Represents an immutable audit record with all relevant audit information.
  - Parameters:
    - `Id`: The unique identifier of the audit record
    - `EntityType`: The type of entity being audited (e.g., "User", "BlogPost")
    - `EntityId`: The identifier of the audited entity
    - `Action`: The action performed (Create, Update, Delete, etc.)
    - `UserId`: The identifier of the user who performed the action
    - `PerformedAt`: The timestamp when the action was performed
    - `Metadata`: Optional metadata dictionary containing additional context

### EnrichedLogger

- **Namespace:** `SmartWorkz.Shared.EnrichedLogger`
- **Summary:** Enriched logger wrapper around ILogger that provides structured logging methods
            for domain events, commands, sagas, file operations, and background jobs.
            Uses structured properties instead of string interpolation for better queryability.

#### Methods & Properties

- **#ctor** - Creates a new instance of EnrichedLogger.
  - Parameters:
    - `logger`: The underlying ILogger instance
- **LogCommandExecuted** - Logs command execution with duration and other metrics.
  - Parameters:
    - `commandType`: The type of command being executed
    - `duration`: How long the command took to execute
- **LogCommandExecutionError** - Logs a command execution error with exception details.
  - Parameters:
    - `commandType`: The type of command that failed
    - `exception`: The exception that occurred
- **LogCommandValidationError** - Logs a command with validation errors.
  - Parameters:
    - `commandType`: The type of command
    - `errors`: Dictionary of validation errors
- **LogEventPublished** - Logs an event publication with metadata.
  - Parameters:
    - `eventType`: The type of event being published
    - `eventId`: The unique identifier of the event
- **LogEventPublishedWithContext** - Logs an event with additional context properties.
  - Parameters:
    - `eventType`: The type of event
    - `eventId`: The event identifier
    - `context`: Additional context data
- **LogEventSubscribed** - Logs an event subscription.
  - Parameters:
    - `eventType`: The type of event being subscribed to
    - `subscriberType`: The subscriber type
- **LogSagaStarted** - Logs the start of a saga with its initial state.
  - Parameters:
    - `sagaId`: The unique saga identifier
    - `state`: The initial saga state
- **LogSagaStateTransition** - Logs a saga state transition.
  - Parameters:
    - `sagaId`: The saga identifier
    - `fromState`: The previous state
    - `toState`: The new state
- **LogSagaCompleted** - Logs the completion of a saga.
  - Parameters:
    - `sagaId`: The saga identifier
    - `duration`: How long the saga took to complete
- **LogSagaFailed** - Logs a saga failure.
  - Parameters:
    - `sagaId`: The saga identifier
    - `exception`: The exception that caused the failure
- **LogFileOperation** - Logs file operations such as upload, download, delete.
  - Parameters:
    - `operation`: The type of operation (Upload, Download, Delete, etc.)
    - `filePath`: The file path or URI
- **LogFileOperationWithSize** - Logs a file operation with size information.
  - Parameters:
    - `operation`: The type of operation
    - `filePath`: The file path
    - `sizeBytes`: The file size in bytes
- **LogFileOperationError** - Logs a file operation error.
  - Parameters:
    - `operation`: The operation that failed
    - `filePath`: The file path
    - `exception`: The exception that occurred
- **LogJobQueued** - Logs when a background job is queued.
  - Parameters:
    - `jobId`: The unique job identifier
    - `jobType`: The type of job being queued
- **LogJobStarted** - Logs when a background job starts processing.
  - Parameters:
    - `jobId`: The job identifier
    - `jobType`: The job type
- **LogJobCompleted** - Logs successful job completion.
  - Parameters:
    - `jobId`: The job identifier
    - `duration`: How long the job took to complete
- **LogJobFailed** - Logs a job failure.
  - Parameters:
    - `jobId`: The job identifier
    - `exception`: The exception that caused the failure
- **LogJobRetry** - Logs job retry attempt.
  - Parameters:
    - `jobId`: The job identifier
    - `attemptNumber`: The current attempt number
    - `maxRetries`: The maximum number of retries
- **LogWithContext** - Logs a message with structured context properties.
  - Parameters:
    - `operationName`: The name of the operation
    - `context`: Dictionary of contextual properties
- **LogPerformanceMetrics** - Logs performance metrics for an operation.
  - Parameters:
    - `operationName`: The operation name
    - `duration`: The operation duration
    - `resultStatus`: The result status (Success, Failure, etc.)
- **LogCorrelation** - Logs a correlation ID for request tracing.
  - Parameters:
    - `correlationId`: The correlation identifier
    - `userId`: Optional user identifier
    - `requestPath`: Optional request path
- **LogUnhandledException** - Logs unhandled exceptions as critical errors.
  - Parameters:
    - `exception`: The exception that occurred
    - `operationName`: The operation that failed

### IAuditLogger

- **Namespace:** `SmartWorkz.Shared.IAuditLogger`
- **Summary:** Interface for structured audit logging with metadata support.

#### Methods & Properties

- **LogAuditAsync** - Logs an audit event with structured metadata.
  - Parameters:
    - `entityType`: The entity type being audited (e.g., "User", "BlogPost")
    - `entityId`: The unique identifier of the entity
    - `action`: The action performed (Create, Update, Delete, etc.)
    - `metadata`: Optional metadata dictionary for additional context
    - `cancellationToken`: Cancellation token
  - Returns: Result indicating success or failure
- **GetAuditHistoryAsync** - Retrieves audit logs for a specific entity.
  - Parameters:
    - `entityType`: The entity type
    - `entityId`: The entity identifier
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of audit records
- **GetUserActivityAsync** - Retrieves audit logs for a specific user across all entities.
  - Parameters:
    - `userId`: The user identifier
    - `cancellationToken`: Cancellation token
  - Returns: Result containing list of audit records

### ILogger

- **Namespace:** `SmartWorkz.Shared.ILogger`
- **Summary:** Abstraction for application logging.
            Decouples from specific logging frameworks (Serilog, NLog, etc.).

### LogLevel

- **Namespace:** `SmartWorkz.Shared.LogLevel`
- **Summary:** Log level severity.

### ILoggerFactory

- **Namespace:** `SmartWorkz.Shared.ILoggerFactory`
- **Summary:** Factory for creating logger instances by category/source.

### IMapper

- **Namespace:** `SmartWorkz.Shared.IMapper`
- **Summary:** Mapping service abstraction for transforming objects between types.
            Supports registration of mapping profiles and bidirectional conversions.

#### Methods & Properties

- **Map``2** - Map source object to target type.
- **Map** - Map source object to target type using dynamic type.
- **MapAsync``2** - Map asynchronously with potential async operations in profile.
- **MapCollection``2** - Map collection of sources to targets.
- **MapCollectionAsync``2** - Map collection asynchronously.
- **RegisterProfile``2** - Register a mapping profile.

### IMapperProfile

- **Namespace:** `SmartWorkz.Shared.IMapperProfile`
- **Summary:** Profile for defining mapping rules between types.
            Implemented by concrete profiles that configure source-to-target transformations.

### IMapperProfile`2

- **Namespace:** `SmartWorkz.Shared.IMapperProfile`2`
- **Summary:** Typed mapper profile for strong typing.

#### Methods & Properties

- **Map** - Transform source to target synchronously.
- **MapAsync** - Transform source to target asynchronously.

### SimpleMapper

- **Namespace:** `SmartWorkz.Shared.SimpleMapper`
- **Summary:** A simple in-memory mapper that supports registering and executing mapping profiles.

### IMetricsCollector

- **Namespace:** `SmartWorkz.Shared.IMetricsCollector`
- **Summary:** Abstraction for collecting application metrics and performance data.
            Enables tracking of operation duration, throughput, error rates, and custom metrics.
            Implementations integrate with OpenTelemetry for export to Prometheus/Grafana.

#### Methods & Properties

- **RecordOperationDuration** - Record operation duration in milliseconds.
  - Parameters:
    - `operationName`: Name of the operation being measured.
    - `durationMs`: Duration in milliseconds.
    - `status`: Optional status (e.g., "success", "error").
    - `tags`: Optional metadata tags for grouping and filtering.
- **RecordOperationCount** - Record operation count (increments counter).
  - Parameters:
    - `operationName`: Name of the operation.
    - `count`: Number to increment by (default 1).
    - `status`: Optional status label.
    - `tags`: Optional metadata tags.
- **RecordGaugeValue** - Record a gauge value (e.g., queue depth, memory usage).
  - Parameters:
    - `metricName`: Name of the gauge metric.
    - `value`: The gauge value to record.
    - `tags`: Optional metadata tags.
- **RecordError** - Record error/exception occurrence.
  - Parameters:
    - `operationName`: Name of the operation that failed.
    - `ex`: The exception that occurred.
    - `tags`: Optional metadata tags.
- **IncrementCounter** - Increment a custom counter.
  - Parameters:
    - `counterName`: Name of the counter.
    - `increment`: Amount to increment (default 1).
    - `tags`: Optional metadata tags.

### MetricsMiddleware

- **Namespace:** `SmartWorkz.Shared.MetricsMiddleware`
- **Summary:** ASP.NET Core middleware for automatic HTTP request/response metrics collection.
             Records operation duration, status, and errors for all HTTP requests.
            
             Usage:
                 app.UseMiddleware<MetricsMiddleware>();

### MetricsStartupExtensions

- **Namespace:** `SmartWorkz.Shared.MetricsStartupExtensions`
- **Summary:** Extension methods for registering application metrics in dependency injection.

#### Methods & Properties

- **AddApplicationMetrics** - Registers IMetricsCollector with OpenTelemetry implementation.
  - Parameters:
    - `services`: The service collection to register with.
  - Returns: The service collection for method chaining.

### OpenTelemetryMetricsCollector

- **Namespace:** `SmartWorkz.Shared.OpenTelemetryMetricsCollector`
- **Summary:** OpenTelemetry-based implementation of IMetricsCollector.
            Collects metrics using System.Diagnostics.Metrics for export to Prometheus/Grafana.

### DefaultTenantFeatureFlags

- **Namespace:** `SmartWorkz.Shared.DefaultTenantFeatureFlags`
- **Summary:** In-memory feature flag provider for tenant-scoped feature control.
            
             Uses ConcurrentDictionary to store tenant-specific flags:
             - Key: tenant ID
             - Value: HashSet of enabled feature flag names
            
             Thread-safe for concurrent operations. Suitable for in-process caching
             or dev/test scenarios. For distributed systems, integrate with a
             centralized feature flag service (Unleash, LaunchDarkly, etc.).

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature is enabled for the specified tenant.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").
    - `cancellationToken`: Cancellation token.
  - Returns: Task containing true if the feature is enabled for this tenant,
            false if the tenant doesn't exist or the flag is not enabled.
- **GetEnabledFeaturesAsync** - Gets all enabled features for a tenant.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Task containing a read-only list of enabled feature flag names.
            Returns an empty list if the tenant doesn't exist or has no enabled flags.
- **EnableFlag** - Enables a feature flag for a tenant.
            Idempotent — calling multiple times with the same flag is safe.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name.
- **DisableFlag** - Disables a feature flag for a tenant.
            Idempotent — calling multiple times with the same flag is safe.
  - Parameters:
    - `tenantId`: The tenant ID.
    - `flagName`: The feature flag name.

### ITenantContext

- **Namespace:** `SmartWorkz.Shared.ITenantContext`
- **Summary:** Scoped service providing current tenant ID for multi-tenant applications.
            Resolved from request context or claims principal.

#### Methods & Properties

- **GetTenantId** - Gets the current tenant identifier.
  - Returns: Tenant ID, or null if operating in single-tenant context.
- **SetTenantId** - Sets the current tenant identifier (rarely used; typically set from request context).
  - Parameters:
    - `tenantId`: Tenant ID to set.

### ITenantFeatureFlags

- **Namespace:** `SmartWorkz.Shared.ITenantFeatureFlags`
- **Summary:** Feature flag provider scoped to a specific tenant.
            Allows per-tenant feature control.

#### Methods & Properties

- **IsEnabledAsync** - Checks if a feature is enabled for the specified tenant.
  - Parameters:
    - `tenantId`: Tenant ID.
    - `flagName`: Feature flag name (e.g., "PAYMENTS", "ADVANCED_ANALYTICS").
    - `cancellationToken`: Cancellation token.
  - Returns: True if feature is enabled for this tenant.
- **GetEnabledFeaturesAsync** - Gets all enabled features for a tenant.
  - Parameters:
    - `tenantId`: Tenant ID.
    - `cancellationToken`: Cancellation token.
  - Returns: List of enabled feature flag names.

### TenantContext

- **Namespace:** `SmartWorkz.Shared.TenantContext`
- **Summary:** Scoped tenant context using AsyncLocal for proper isolation across async boundaries.
            
             AsyncLocal ensures:
             - Thread-safe storage per async execution context
             - Isolation between concurrent requests (each gets its own context)
             - Proper inheritance to child tasks (when awaited)
            
             Survives async/await boundaries unlike ThreadLocal, making it suitable for async methods.

#### Methods & Properties

- **GetTenantId** - Gets the current tenant identifier.
  - Returns: The current tenant ID, or "default" if not set.
- **SetTenantId** - Sets the current tenant identifier.
  - Parameters:
    - `tenantId`: The tenant ID to set. Cannot be null or empty.

### FirebaseCloudMessagingService

- **Namespace:** `SmartWorkz.Shared.FirebaseCloudMessagingService`
- **Summary:** Firebase Cloud Messaging service implementation for sending push notifications.
            Supports single/batch user notifications, topic-based broadcasting, and multi-platform delivery (Android, iOS, Web).

#### Methods & Properties

- **#ctor** - Initializes a new instance of the FirebaseCloudMessagingService.
  - Parameters:
    - `logger`: Logger for diagnostic and error information.
- **SendAsync** - Sends a simple push notification to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends simple push notifications to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase device tokens).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends a rich push notification with metadata to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SendAsync** - Sends a rich push notification to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase device tokens).
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SendToTopicAsync** - Sends a rich push notification to all users subscribed to a topic (broadcast).
  - Parameters:
    - `topic`: Topic name (e.g., "news", "promotions").
    - `payload`: Notification payload with title, body, images, data, and actions.
    - `cancellationToken`: Cancellation token.
- **SubscribeToTopicAsync** - Subscribes a user to a topic for broadcast notifications.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `topic`: Topic name to subscribe to.
    - `cancellationToken`: Cancellation token.
- **UnsubscribeFromTopicAsync** - Unsubscribes a user from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase device token).
    - `topic`: Topic name to unsubscribe from.
    - `cancellationToken`: Cancellation token.

### IPushNotificationService

- **Namespace:** `SmartWorkz.Shared.IPushNotificationService`
- **Summary:** Service for sending push notifications using Firebase Cloud Messaging.

#### Methods & Properties

- **SendAsync** - Sends a simple push notification to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a simple push notification to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase tokens).
    - `title`: Notification title.
    - `message`: Notification body text.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a rich push notification with metadata to a single user.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendAsync** - Sends a rich push notification with metadata to multiple users.
  - Parameters:
    - `userIds`: Collection of user identifiers (Firebase tokens).
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SendToTopicAsync** - Sends a push notification to all users subscribed to a topic.
  - Parameters:
    - `topic`: The topic name.
    - `payload`: Notification payload with title, body, images, and custom data.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous send operation.
- **SubscribeToTopicAsync** - Subscribes a user to receive notifications from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `topic`: The topic name.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous subscription operation.
- **UnsubscribeFromTopicAsync** - Unsubscribes a user from a topic.
  - Parameters:
    - `userId`: User identifier (Firebase token).
    - `topic`: The topic name.
    - `cancellationToken`: Cancellation token.
  - Returns: A task that represents the asynchronous unsubscription operation.

### PushNotificationPayload

- **Namespace:** `SmartWorkz.Shared.PushNotificationPayload`
- **Summary:** Represents the payload data for a push notification.

### PushNotificationAction

- **Namespace:** `SmartWorkz.Shared.PushNotificationAction`
- **Summary:** Represents an action that can be performed from a push notification.

### PagedList`1

- **Namespace:** `SmartWorkz.Shared.PagedList`1`
- **Summary:** A page of items with metadata.
             Replaces PaginationResponse<T> in StarterKitMVC.Shared.DTOs.
            
             Migration path: PaginationResponse<T> has the same fields under different names.
             PagedList<T>.Create() is a drop-in replacement for PaginationResponse<T>.Create().

#### Methods & Properties

- **Empty** - Create an empty result set (e.g., when no rows match).
- **Map``1** - Project items to a different type without changing pagination metadata.

### PagedQuery

- **Namespace:** `SmartWorkz.Shared.PagedQuery`
- **Summary:** Standard request parameters for any paginated query.
             Replaces the existing PaginationRequest record in StarterKitMVC.Shared.DTOs.
            
             Migration: PaginationRequest is structurally identical — alias it or replace it.

#### Methods & Properties

- **#ctor** - Standard request parameters for any paginated query.
             Replaces the existing PaginationRequest record in StarterKitMVC.Shared.DTOs.
            
             Migration: PaginationRequest is structurally identical — alias it or replace it.
- **Normalize** - Clamp page and pageSize to safe bounds.

### IEntity`1

- **Namespace:** `SmartWorkz.Shared.IEntity`1`
- **Summary:** Marks a class as a domain entity with a typed primary key.

### CircuitBreaker

- **Namespace:** `SmartWorkz.Shared.CircuitBreaker`
- **Summary:** A thread-safe implementation of the circuit breaker pattern for handling failing dependencies gracefully.
            
             The circuit breaker operates in three states:
             - Closed: Normal operation. Requests pass through. Failures are tracked.
             - Open: Failing. All requests are rejected immediately to prevent cascading failures.
             - HalfOpen: Testing recovery. Limited requests are allowed to test if the dependency has recovered.
            
             State transitions:
             - Closed → Open: When ConsecutiveFailures >= FailureThreshold
             - Open → HalfOpen: Automatically when (DateTime.UtcNow - LastFailureTime) >= TimeoutMilliseconds
             - HalfOpen → Closed: When SuccessCount >= SuccessThreshold
             - HalfOpen → Open: When RecordFailure() is called in HalfOpen state
             - Closed → Closed: When RecordSuccess() is called (resets failure counter)

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `options`: The circuit breaker configuration options.
- **RecordSuccess** - Records a successful operation and updates state transitions accordingly.
            In Closed state: resets the failure counter.
            In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
- **RecordFailure** - Records a failed operation and updates state transitions accordingly.
            In Closed state: increments failure counter and transitions to Open if threshold is met.
            In HalfOpen state: immediately transitions back to Open.
- **Reset** - Resets the circuit breaker to the Closed state, clearing all failure and success counters.

### CircuitBreakerOptions

- **Namespace:** `SmartWorkz.Shared.CircuitBreakerOptions`
- **Summary:** Configuration options for the circuit breaker.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if options are valid; otherwise false.

### CircuitBreakerState

- **Namespace:** `SmartWorkz.Shared.CircuitBreakerState`
- **Summary:** Defines the state of a circuit breaker in the state machine pattern.

### ICircuitBreaker

- **Namespace:** `SmartWorkz.Shared.ICircuitBreaker`
- **Summary:** Defines the contract for a circuit breaker that implements the state machine pattern
            to handle failing dependencies gracefully.

#### Methods & Properties

- **RecordSuccess** - Records a successful operation and updates state transitions accordingly.
            In Closed state: resets the failure counter.
            In HalfOpen state: increments success counter and transitions to Closed if threshold is met.
- **RecordFailure** - Records a failed operation and updates state transitions accordingly.
            In Closed state: increments failure counter and transitions to Open if threshold is met.
            In HalfOpen state: immediately transitions back to Open.
- **Reset** - Resets the circuit breaker to the Closed state, clearing all failure and success counters.

### IRateLimiter

- **Namespace:** `SmartWorkz.Shared.IRateLimiter`
- **Summary:** Defines the contract for a thread-safe rate limiter.

#### Methods & Properties

- **TryAcquireAsync** - Attempts to acquire tokens for a request identified by the given identifier.
  - Parameters:
    - `identifier`: The identifier for rate limiting (e.g., IP address, user ID).
    - `cost`: The number of tokens to acquire (default: 1).
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A result containing true if tokens were acquired successfully; otherwise false.
            On failure, the Error will include retry-after information.
- **GetAvailableTokensAsync** - Gets the number of available tokens for a given identifier.
  - Parameters:
    - `identifier`: The identifier to check.
  - Returns: The number of available tokens.
- **ResetAsync** - Resets the rate limit state for a given identifier.
  - Parameters:
    - `identifier`: The identifier to reset.
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **ClearAsync** - Clears all rate limit state.
  - Parameters:
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.

### RateLimiter

- **Namespace:** `SmartWorkz.Shared.RateLimiter`
- **Summary:** Thread-safe token bucket rate limiter implementation.
            
             This class maintains a per-identifier token bucket that refills at a constant rate.
             Tokens are consumed when requests are made; if insufficient tokens exist, the request is denied.
            
             Thread-safe operations use ConcurrentDictionary and locks on individual buckets to ensure
             consistent state without global locking bottlenecks.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the RateLimiter class.
  - Parameters:
    - `options`: Configuration options for the rate limiter.
- **TryAcquireAsync** - Attempts to acquire tokens for a request identified by the given identifier.
  - Parameters:
    - `identifier`: The identifier for rate limiting (e.g., IP address, user ID).
    - `cost`: The number of tokens to acquire (default: 1).
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A result containing true if tokens were acquired successfully; otherwise false.
            On failure, the Error will include retry-after information.
- **GetAvailableTokensAsync** - Gets the number of available tokens for a given identifier.
  - Parameters:
    - `identifier`: The identifier to check.
  - Returns: The number of available tokens.
- **ResetAsync** - Resets the rate limit state for a given identifier.
  - Parameters:
    - `identifier`: The identifier to reset.
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **ClearAsync** - Clears all rate limit state.
  - Parameters:
    - `cancellationToken`: Cancellation token for the operation.
  - Returns: A task representing the asynchronous operation.
- **TokenBucket.TryAcquire** - Tries to acquire the specified number of tokens.
- **TokenBucket.GetAvailableTokens** - Gets the current number of available tokens.
- **TokenBucket.GetRetryAfterMilliseconds** - Gets the number of milliseconds to wait before retrying.
- **TokenBucket.RefillTokens** - Refills the token bucket based on elapsed time.

### RateLimiterOptions

- **Namespace:** `SmartWorkz.Shared.RateLimiterOptions`
- **Summary:** Configuration options for the rate limiter.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if options are valid; otherwise false.

### RateLimiterStrategy

- **Namespace:** `SmartWorkz.Shared.RateLimiterStrategy`
- **Summary:** Specifies the strategy used by the rate limiter to control request flow.

### ApiError

- **Namespace:** `SmartWorkz.Shared.ApiError`
- **Summary:** Structured error representation for API responses.
            Provides code, message, and optional field-level error details.

#### Methods & Properties

- **FromError** - Create from core Error type.
- **FromValidationErrors** - Create from validation errors.
- **FromException** - Create from exception.

### ApiResponse

- **Namespace:** `SmartWorkz.Shared.ApiResponse`
- **Summary:** Generic API response envelope that wraps result data with metadata.
            Non-generic convenience version for non-data responses.

#### Methods & Properties

- **Ok** - Success response without data.
- **Fail** - Failure response with error details.
- **FromResult** - Create from core Result pattern.

### ApiResponse`1

- **Namespace:** `SmartWorkz.Shared.ApiResponse`1`
- **Summary:** Typed API response envelope with data payload.
            Includes optional pagination metadata for list responses.

#### Methods & Properties

- **Ok** - Success response with data.
- **OkPaginated** - Success response with paginated data.
- **Fail** - Failure response with error.

### ProblemDetailsResponse

- **Namespace:** `SmartWorkz.Shared.ProblemDetailsResponse`
- **Summary:** Implements RFC 7807 Problem Details for HTTP APIs standard response format.
            Provides a standardized way to represent error details in API responses.

#### Methods & Properties

- **ValidationError** - Factory method for 400 Bad Request error with validation details.
- **Unauthorized** - Factory method for 401 Unauthorized error.
- **Forbidden** - Factory method for 403 Forbidden error.
- **NotFound** - Factory method for 404 Not Found error.
- **Conflict** - Factory method for 409 Conflict error.
- **InternalServerError** - Factory method for 500 Internal Server Error.
- **Custom** - Factory method for custom problem details.

### Error

- **Namespace:** `SmartWorkz.Shared.Error`
- **Summary:** Represents a structured error with a machine-readable code and human-readable message.
            
             This is the canonical Error type. It replaces:
             - SmartWorkz.StarterKitMVC.Shared.Primitives.Error (record struct)
             - The ad-hoc string errors in Models.Result
            
             Code examples: "USER_NOT_FOUND", "VALIDATION.EMAIL_REQUIRED", "AUTH.INVALID_CREDENTIALS"
             MessageKey maps to localization resource keys for UI display.

### Result

- **Namespace:** `SmartWorkz.Shared.Result`
- **Summary:** Represents the outcome of an operation that does not return a value.
            
             This unifies:
             - SmartWorkz.StarterKitMVC.Shared.Models.Result (Succeeded + MessageKey + Errors[])
             - SmartWorkz.StarterKitMVC.Shared.Primitives.Result (IsSuccess + Error struct)
            
             Design choice — class over struct:
             1. Result<T> inherits from Result to reuse Succeeded/Errors without duplication.
                Structs cannot use inheritance this way.
             2. Services return Result from interface methods — class semantics (null check) are
                simpler than boxing/unboxing structs across interface boundaries.
             3. Errors[] supports field-level validation messages that ModelState.AddErrors() consumes.
                A single Error struct cannot carry multiple field errors.
            
             The Primitives.Result struct in StarterKitMVC.Shared remains valid for pure functions
             where you want zero-allocation returns. This class is for service layer contracts.

#### Methods & Properties

- **Fail** - Failure with a localization message key and optional field-level error strings.
- **Fail** - Failure from a structured Error (bridges the Primitives.Error pattern).
- **Ok``1** - Factory for a typed result. Use in services that return data.

### Result`1

- **Namespace:** `SmartWorkz.Shared.Result`1`
- **Summary:** Result with a typed payload. Data is only valid when Succeeded = true.
            
             Usage:
               Result<UserDto> result = await _userService.GetByIdAsync(id);
               if (!result.Succeeded) return RedirectToPage("Error");
               var user = result.Data!;

### ResultExtensions

- **Namespace:** `SmartWorkz.Shared.ResultExtensions`
- **Summary:** Functional helpers for chaining Result operations.
            Keeps service code flat — avoids nested if (!result.Succeeded) blocks.

#### Methods & Properties

- **Map``2** - Transform the Data value if the result succeeded.
- **BindAsync``2** - Chain a second operation that also returns Result.
- **OnSuccess``1** - Execute a side-effect action on success, then return the original result.
- **OnFailure``1** - Execute a side-effect action on failure, then return the original result.

### ISagaDefinition`1

- **Namespace:** `SmartWorkz.Shared.ISagaDefinition`1`
- **Summary:** Defines the blueprint for a saga orchestration.
            A saga is a pattern for managing distributed transactions and long-running processes
            by coordinating multiple steps with built-in compensation mechanisms.

#### Methods & Properties

- **DefineStep``1** - Defines a step in the saga that will be executed when a specific event type is received.
            Steps are executed sequentially in the order they were defined.
  - Parameters:
    - `handler`: The async handler function that processes the event and updates the saga state.
            Returns a StepResult indicating success or failure.
- **OnFailure** - Defines the failure handler that will be called if any step fails.
            Used for compensation logic and saga-level error handling.
  - Parameters:
    - `compensationHandler`: The async handler that receives the current saga state and the exception that occurred.
            Responsible for compensation/rollback logic.
- **BuildAsync** - Builds and returns the saga definition for execution.
            Can be used for async initialization or validation.
  - Returns: A task that completes with the configured saga definition.
- **GetSteps** - Gets the list of saga steps in execution order.
  - Returns: A read-only list of saga step handlers.
- **GetFailureHandler** - Gets the failure compensation handler if defined.
  - Returns: The failure handler function, or null if not defined.

### SagaOrchestrator

- **Namespace:** `SmartWorkz.Shared.SagaOrchestrator`
- **Summary:** Orchestrates the execution of sagas, managing step sequencing, error handling,
            and compensation/rollback logic for complex distributed processes.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the SagaOrchestrator class.
  - Parameters:
    - `logger`: Logger for saga execution tracking and debugging.
- **ExecuteSagaAsync``1** - Executes a saga definition with the provided initial state and triggering event.
            Manages step execution, error handling, and compensation logic.
  - Parameters:
    - `sagaDefinition`: The saga definition blueprint to execute.
    - `initialState`: The initial saga state.
    - `event`: The domain event triggering the saga.
    - `cancellationToken`: Optional cancellation token.
  - Returns: A task representing the saga execution.
- **ExecuteSagaStepsAsync``1** - Executes saga steps by using reflection to access internal step definitions.
- **CompensateExecutedStepsAsync``1** - Executes compensation handlers for all executed steps in reverse order.
            Uses stored compensation handlers to avoid re-executing steps.
- **ExecuteFailureHandlerAsync``1** - Executes the saga-level failure handler if one is defined.

### SagaStatus

- **Namespace:** `SmartWorkz.Shared.SagaStatus`
- **Summary:** Represents the status of a saga execution.

### SagaState

- **Namespace:** `SmartWorkz.Shared.SagaState`
- **Summary:** Base class for saga state objects.
            Provides common tracking properties for saga execution flow.

### StepResult

- **Namespace:** `SmartWorkz.Shared.StepResult`
- **Summary:** Represents the result of executing a single saga step.
            Provides success/failure status and optional compensation logic for rollback.

#### Methods & Properties

- **Success** - Creates a successful step result.
  - Returns: A StepResult indicating success.
- **Failure** - Creates a failed step result with an optional compensation handler.
  - Parameters:
    - `failureReason`: The reason for the step failure.
    - `compensationHandler`: Optional handler to compensate/rollback this step if a later step fails.
  - Returns: A StepResult indicating failure.
- **FromException** - Creates a failed step result for an exception with optional compensation.
  - Parameters:
    - `exception`: The exception that caused the failure.
    - `compensationHandler`: Optional compensation handler.
  - Returns: A StepResult indicating failure.

### CryptHelper

- **Namespace:** `SmartWorkz.Shared.CryptHelper`
- **Summary:** Provides AES-256-CBC encryption and decryption utilities with secure key and IV generation.
            
             All operations support both string and byte array inputs/outputs.
             Keys are normalized to 32 bytes (256 bits) via padding/trimming as needed.
             IVs are auto-generated if not provided and embedded in the ciphertext (IV:Ciphertext format).

#### Methods & Properties

- **EncryptString** - Encrypts plaintext using AES-256-CBC with a Base64-encoded output.
  - Parameters:
    - `plaintext`: The plaintext to encrypt.
    - `key`: The encryption key (will be normalized to 32 bytes).
    - `iv`: Optional IV; auto-generated if null.
  - Returns: A Result containing Base64-encoded ciphertext in "IV:Ciphertext" format or an error.
- **EncryptBytes** - Encrypts byte data using AES-256-CBC.
  - Parameters:
    - `plaintext`: The plaintext bytes to encrypt.
    - `key`: The encryption key (will be normalized to 32 bytes).
    - `iv`: Optional IV; auto-generated if null.
  - Returns: A Result containing encrypted bytes with embedded IV (IV || Ciphertext) or an error.
- **DecryptString** - Decrypts Base64-encoded ciphertext using AES-256-CBC.
  - Parameters:
    - `ciphertext`: The Base64-encoded ciphertext in "IV:Ciphertext" format.
    - `key`: The decryption key (will be normalized to 32 bytes).
  - Returns: A Result containing the decrypted plaintext or an error.
- **DecryptBytes** - Decrypts byte data using AES-256-CBC.
  - Parameters:
    - `ciphertext`: The encrypted bytes with embedded IV (IV || Ciphertext).
    - `key`: The decryption key (will be normalized to 32 bytes).
  - Returns: A Result containing decrypted bytes or an error.
- **GenerateKey** - Generates a random cryptographic key of the specified size.
  - Parameters:
    - `keySize`: The key size in bytes (default 32 for AES-256). Must be 16, 24, or 32.
  - Returns: A Result containing Base64-encoded random key or an error.
- **GenerateIv** - Generates a random cryptographic IV (Initialization Vector).
  - Returns: A Result containing Base64-encoded random IV or an error.
- **GenerateRandomBytes** - Generates cryptographically secure random bytes.
- **NormalizeKey** - Normalizes a key to exactly 32 bytes (256 bits).
            If the key is shorter, it's padded with zeros. If longer, it's trimmed.

### CryptOptions

- **Namespace:** `SmartWorkz.Shared.CryptOptions`
- **Summary:** Configuration options for AES cryptographic operations.

#### Methods & Properties

- **IsValid** - Validates the options for correctness.
  - Returns: True if valid, false otherwise.

### HashHelper

- **Namespace:** `SmartWorkz.Shared.HashHelper`
- **Summary:** Provides utilities for cryptographic hash operations (SHA256 and MD5).

#### Methods & Properties

- **Sha256** - Computes the SHA256 hash of a string and returns it as a hexadecimal string.
- **Sha256Bytes** - Computes the SHA256 hash of a byte array and returns the hash as a byte array.
- **Md5** - Computes the MD5 hash of a string and returns it as a hexadecimal string.
            Note: MD5 is cryptographically broken; use SHA256 for security-critical applications.
- **VerifyHash** - Verifies that a text matches its SHA256 hash.

### HmacAlgorithm

- **Namespace:** `SmartWorkz.Shared.HmacAlgorithm`
- **Summary:** Specifies the HMAC algorithm to use for message signing and verification.

### HmacHelper

- **Namespace:** `SmartWorkz.Shared.HmacHelper`
- **Summary:** Provides HMAC-SHA256/SHA512 message signing and verification for API requests and webhook verification.
            Implements constant-time comparison to prevent timing attacks.

#### Methods & Properties

- **Sign** - Signs a message using HMAC with the specified algorithm and returns a Base64-encoded hex digest.
  - Parameters:
    - `message`: The message to sign.
    - `secret`: The secret key for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing the Base64-encoded signature or an error.
- **SignBytes** - Signs a message using HMAC with the specified algorithm and returns the raw byte digest.
  - Parameters:
    - `message`: The message bytes to sign.
    - `secret`: The secret key for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing the byte signature or an error.
- **Verify** - Verifies a message signature using HMAC with constant-time comparison to prevent timing attacks.
  - Parameters:
    - `message`: The original message that was signed.
    - `signature`: The Base64-encoded signature to verify.
    - `secret`: The secret key used for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing true if the signature is valid, false if not, or an error.
- **VerifyBytes** - Verifies a message signature using HMAC with raw byte inputs and constant-time comparison.
  - Parameters:
    - `message`: The original message bytes that were signed.
    - `signature`: The byte signature to verify.
    - `secret`: The secret key used for signing.
    - `algorithm`: The HMAC algorithm to use (default: SHA256).
  - Returns: A Result containing true if the signature is valid, false if not, or an error.
- **SignBytes** - Internal method to compute HMAC signature from raw bytes.
- **CreateHmac** - Creates the appropriate HMAC instance based on the algorithm.

### InputSanitizer

- **Namespace:** `SmartWorkz.Shared.InputSanitizer`
- **Summary:** Input sanitization to prevent XSS, SQL injection, and path traversal attacks.

#### Methods & Properties

- **SanitizeHtml** - Sanitize HTML by removing dangerous tags and attributes.
- **EscapeHtml** - Escape HTML special characters to prevent XSS.
- **SanitizeSql** - Sanitize string to prevent SQL injection (basic, not a replacement for parameterized queries).
- **SanitizeFilePath** - Sanitize file path to prevent directory traversal attacks.
- **SanitizeUrl** - Sanitize and validate URL.
- **EscapeJson** - Escape string for safe JSON inclusion.
- **IsValidEmail** - Validate email format (basic check, server-side SMTP validation recommended).
- **RemoveControlCharacters** - Remove null bytes and control characters.

### JwtSettings

- **Namespace:** `SmartWorkz.Shared.JwtSettings`
- **Summary:** Settings for JWT token generation and validation.

#### Methods & Properties

- **Validate** - Validate settings: Secret >= 32 chars, other fields non-empty.

### JwtClaims

- **Namespace:** `SmartWorkz.Shared.JwtClaims`
- **Summary:** JWT claims that can be included in a token.

#### Methods & Properties

- **GetClaimValue** - Get claim value by type (supports standard claims + custom).

### JwtTokenValidationResult

- **Namespace:** `SmartWorkz.Shared.JwtTokenValidationResult`
- **Summary:** Result of JWT token validation.

### JwtHelper

- **Namespace:** `SmartWorkz.Shared.JwtHelper`
- **Summary:** Provides JWT token generation, validation, and refresh functionality.

#### Methods & Properties

- **GenerateTokenInternal** - Internal token generation logic shared by GenerateToken and GenerateRefreshToken.
  - Parameters:
    - `claims`: The claims to include in the token.
    - `settings`: The JWT settings for signing and configuration.
    - `isRefreshToken`: If true, uses RefreshTokenExpiryDays; otherwise uses ExpiryMinutes.
  - Returns: A Result containing the signed token or an error.
- **GenerateToken** - Generates a JWT access token with the specified claims and settings.
  - Parameters:
    - `claims`: The claims to include in the token.
    - `settings`: The JWT settings for signing and configuration.
  - Returns: A Result containing the signed token or an error.
- **ValidateToken** - Validates a JWT token and extracts claims if valid.
  - Parameters:
    - `token`: The token to validate.
    - `settings`: The JWT settings for validation.
  - Returns: A Result containing the validation result.
- **RefreshToken** - Refreshes an access token using a refresh token.
  - Parameters:
    - `refreshToken`: The refresh token.
    - `settings`: The JWT settings.
  - Returns: A Result containing the new access token or an error.
- **GenerateRefreshToken** - Generates a refresh token with extended expiry.
  - Parameters:
    - `claims`: The claims to include in the refresh token.
    - `settings`: The JWT settings.
  - Returns: A Result containing the refresh token or an error.
- **ToBase64Url** - Encodes bytes to Base64Url format (no padding, + → -, / → _).
- **FromBase64Url** - Decodes Base64Url format to bytes.

### PasswordHelper

- **Namespace:** `SmartWorkz.Shared.PasswordHelper`
- **Summary:** Provides secure password generation and validation using cryptographically secure random number generation.

#### Methods & Properties

- **GeneratePassword** - Generates a cryptographically secure random password.
  - Parameters:
    - `length`: Length of the password (8-128, default 12).
    - `includeSpecialChars`: Whether to include special characters.
  - Returns: A Result containing the generated password or an error.
- **ValidateStrength** - Validates the strength of a password against a policy.
  - Parameters:
    - `password`: The password to validate.
    - `policy`: The policy to validate against (uses default if null).
  - Returns: A Result containing the validation result.
- **GetRandomChar** - Gets a random character from the specified character set using cryptographic randomness.
- **Shuffle** - Performs Fisher-Yates shuffle on the character array.
- **CheckPasswordLength** - Checks if password meets minimum length requirement.
- **CheckUppercase** - Checks if password contains at least one uppercase letter.
- **CheckLowercase** - Checks if password contains at least one lowercase letter.
- **CheckNumbers** - Checks if password contains at least one digit.
- **CheckSpecialChars** - Checks if password contains at least one special character.

### PasswordPolicy

- **Namespace:** `SmartWorkz.Shared.PasswordPolicy`
- **Summary:** Policy for password validation requirements.

#### Methods & Properties

- **Validate** - Validates the policy invariants.

### PasswordValidationResult

- **Namespace:** `SmartWorkz.Shared.PasswordValidationResult`
- **Summary:** Result of password validation against a policy.

### ITemplateEngine

- **Namespace:** `SmartWorkz.Shared.ITemplateEngine`
- **Summary:** Defines operations for rendering templates with placeholder substitution.

#### Methods & Properties

- **Render** - Renders a template string by replacing placeholders with values from a dictionary.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `values`: Dictionary of key-value pairs to substitute into the template.
  - Returns: The rendered content with placeholders replaced. Unmatched placeholders remain unchanged.
- **Render** - Renders a template string by extracting properties from an object model.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `model`: The model object whose public properties are used for placeholder substitution.
  - Returns: The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.
- **RenderFileAsync** - Asynchronously renders a template file by replacing placeholders with values from a dictionary.
  - Parameters:
    - `filePath`: The path to the template file.
    - `values`: Dictionary of key-value pairs to substitute into the template.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **RenderFileAsync** - Asynchronously renders a template file by extracting properties from an object model.
  - Parameters:
    - `filePath`: The path to the template file.
    - `model`: The model object whose public properties are used for placeholder substitution.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **LoadDirectoryAsync** - Asynchronously loads all template files from a directory into a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.
- **RenderDirectoryAsync** - Asynchronously loads and renders all template files from a directory using values from a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `values`: Dictionary of key-value pairs to substitute into each template.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.

### TemplateEngine

- **Namespace:** `SmartWorkz.Shared.TemplateEngine`
- **Summary:** Provides template rendering services with support for placeholder substitution.

#### Methods & Properties

- **PlaceholderRegex** - 
- **Render** - Renders a template string by replacing placeholders with values from a dictionary.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `values`: Dictionary of key-value pairs to substitute into the template.
  - Returns: The rendered content with placeholders replaced. Unmatched placeholders and null/empty content remain unchanged.
- **Render** - Renders a template string by extracting properties from an object model.
  - Parameters:
    - `content`: The template content containing placeholders in the format {Key} or {{Key}} (case-insensitive).
    - `model`: The model object whose public properties are used for placeholder substitution.
  - Returns: The rendered content with placeholders replaced using model properties. Unmatched placeholders remain unchanged.
- **RenderFileAsync** - Asynchronously renders a template file by replacing placeholders with values from a dictionary.
  - Parameters:
    - `filePath`: The path to the template file.
    - `values`: Dictionary of key-value pairs to substitute into the template.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **RenderFileAsync** - Asynchronously renders a template file by extracting properties from an object model.
  - Parameters:
    - `filePath`: The path to the template file.
    - `model`: The model object whose public properties are used for placeholder substitution.
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing the rendered file content, or an error if the file operation fails.
- **LoadDirectoryAsync** - Asynchronously loads all template files from a directory into a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their content, or an error if the directory operation fails.
- **RenderDirectoryAsync** - Asynchronously loads and renders all template files from a directory using values from a dictionary.
  - Parameters:
    - `directoryPath`: The directory path to scan for template files.
    - `values`: Dictionary of key-value pairs to substitute into each template.
    - `searchPattern`: The search pattern for files (default "*.html"). Supports wildcards like "*.txt".
    - `ct`: Cancellation token to cancel the operation.
  - Returns: A result containing a dictionary mapping file names (without extension) to their rendered content, or an error if the directory operation fails.
- **ReflectModel** - Reflects over a model object and builds a case-insensitive dictionary of public properties
            mapped to their string values. Uses cached property metadata for performance.
- **ValidateFilePath** - Validates a file path to prevent directory traversal attacks.
  - Parameters:
    - `filePath`: The file path to validate.
  - Returns: A result indicating if the path is valid and safe.

### CompressHelper

- **Namespace:** `SmartWorkz.Shared.CompressHelper`
- **Summary:** Provides utilities for GZip compression and decompression.

#### Methods & Properties

- **CompressString** - Compresses a string using GZip compression.
- **DecompressString** - Decompresses a GZip-compressed byte array back to a string.
- **CompressBytes** - Compresses a byte array using GZip compression.
- **DecompressBytes** - Decompresses a GZip-compressed byte array.

### DateHelper

- **Namespace:** `SmartWorkz.Shared.DateHelper`
- **Summary:** Provides utilities for date and time operations.

#### Methods & Properties

- **GetAge** - Calculates the age in years from a birth date to today.
- **GetRelativeTime** - Returns a human-readable relative time string (e.g., "2 days ago", "in 3 hours").
- **StartOfDay** - Returns the start of the day (00:00:00) for the given date.
- **EndOfDay** - Returns the end of the day (23:59:59.999) for the given date.
- **IsWeekend** - Determines if the given date falls on a weekend (Saturday or Sunday).
- **GetDayOfWeekName** - Returns the name of the day of week (e.g., "Monday", "Tuesday").
- **DaysBetween** - Calculates the number of days between two dates (inclusive of the from date, exclusive of the to date).

### EnumHelper

- **Namespace:** `SmartWorkz.Shared.EnumHelper`
- **Summary:** Provides utilities for enum operations including reflection and description retrieval.

#### Methods & Properties

- **GetDescription** - Gets the description of an enum value from its [Description] attribute.
            Falls back to the enum name if no description is found.
- **GetValue``1** - Attempts to get an enum value by its name.
- **GetAllValues``1** - Returns all values of the specified enum type as a list.
- **GetName** - Gets the name of an enum value.

### MathHelper

- **Namespace:** `SmartWorkz.Shared.MathHelper`
- **Summary:** Provides utilities for common math operations.

#### Methods & Properties

- **Percentage** - Calculates the percentage of a value.
            Example: Percentage(100, 20) returns 20 (20% of 100).
- **PercentageChange** - Calculates the percentage change from oldValue to newValue.
            Positive result indicates increase, negative indicates decrease.
- **RoundTo** - Rounds a decimal value to the specified number of decimal places.
- **Clamp``1** - Clamps a value within a specified range [min, max].
- **Average** - Calculates the average of the provided decimal values.

### SlugHelper

- **Namespace:** `SmartWorkz.Shared.SlugHelper`
- **Summary:** Helper for generating URL-friendly slugs from text input.

#### Methods & Properties

- **GenerateSlug** - Generates a URL-friendly slug from the given text with optional configuration.
  - Parameters:
    - `text`: The input text to convert to a slug.
    - `options`: Configuration options. If null, default options are used.
  - Returns: A Result containing the generated slug or an error.
- **ToSlug** - Generates a URL-friendly slug from the given text using default options.
            Convenience method equivalent to GenerateSlug(text, null).
  - Parameters:
    - `text`: The input text to convert to a slug.
  - Returns: A Result containing the generated slug or an error.
- **RemoveAccents** - Removes accented characters from text by decomposing them and filtering out combining marks.
            For example: "café" → "cafe", "naïve" → "naive", "Señor" → "Senor".
  - Parameters:
    - `input`: The input text potentially containing accented characters.
  - Returns: The text with accented characters converted to their base forms.
- **ReplaceSpecialCharacters** - Replaces special characters and spaces with the specified separator.
            Keeps only alphanumeric characters and the separator.
  - Parameters:
    - `input`: The input text.
    - `separator`: The separator to use for special characters and spaces.
  - Returns: The text with special characters replaced by the separator.

### SlugOptions

- **Namespace:** `SmartWorkz.Shared.SlugOptions`
- **Summary:** Options for configuring slug generation behavior in .

### TextHelper

- **Namespace:** `SmartWorkz.Shared.TextHelper`
- **Summary:** Sealed class providing advanced text processing and formatting utilities.
            All methods return Result<string> for consistent error handling.

#### Methods & Properties

- **Truncate** - Truncates text to a maximum length and appends a suffix (default "...").
  - Parameters:
    - `text`: The input text to truncate.
    - `maxLength`: The maximum length including the suffix.
    - `suffix`: The suffix to append when truncating. Defaults to "...".
  - Returns: A Result containing the truncated text or an error.
- **Capitalize** - Capitalizes the first letter of the text while preserving the rest.
  - Parameters:
    - `text`: The input text to capitalize.
  - Returns: A Result containing the capitalized text or an error.
- **Decapitalize** - Decapitalizes the first letter of the text while preserving the rest.
  - Parameters:
    - `text`: The input text to decapitalize.
  - Returns: A Result containing the decapitalized text or an error.
- **StripHtml** - Removes HTML tags from the input string using regex.
  - Parameters:
    - `html`: The HTML string to process.
  - Returns: A Result containing the plain text with HTML tags removed or an error.
- **Pluralize** - Pluralizes a word based on count using a simple heuristic.
            If count == 1, returns singular form. Otherwise appends 's'.
  - Parameters:
    - `singular`: The singular form of the word.
    - `count`: The count to determine plural form.
  - Returns: A Result containing the appropriately pluralized word or an error.
- **TitleCase** - Converts text to title case by capitalizing the first letter of each word.
  - Parameters:
    - `text`: The input text to convert.
  - Returns: A Result containing the title-cased text or an error.
- **Reverse** - Reverses the input string.
  - Parameters:
    - `text`: The input text to reverse.
  - Returns: A Result containing the reversed text or an error.
- **RemoveWhitespace** - Removes all whitespace characters from the input string.
  - Parameters:
    - `text`: The input text to process.
  - Returns: A Result containing the text with all whitespace removed or an error.
- **WordWrap** - Wraps text at a specified line length while preserving word boundaries.
  - Parameters:
    - `text`: The input text to wrap.
    - `lineLength`: The maximum length of each line.
    - `newline`: The newline character(s) to use. Defaults to "\n".
  - Returns: A Result containing the word-wrapped text or an error.
- **Repeat** - Repeats the input string the specified number of times.
  - Parameters:
    - `text`: The input text to repeat.
    - `count`: The number of times to repeat the text.
  - Returns: A Result containing the repeated text or an error.

### CompositeValidator`1

- **Namespace:** `SmartWorkz.Shared.CompositeValidator`1`
- **Summary:** Combines multiple validators into a single validator.
            Useful for composing validators from different sources.

### IValidationRule`2

- **Namespace:** `SmartWorkz.Shared.IValidationRule`2`
- **Summary:** Single validation rule for a property.

#### Methods & Properties

- **ValidateAsync** - Validate property and return results.

### ValidationRule`2

- **Namespace:** `SmartWorkz.Shared.ValidationRule`2`
- **Summary:** Base implementation for custom validation rules.

### ValidationRules

- **Namespace:** `SmartWorkz.Shared.ValidationRules`
- **Summary:** Pre-built validation rules for common scenarios.

### ValidatorBuilder`1

- **Namespace:** `SmartWorkz.Shared.ValidatorBuilder`1`
- **Summary:** Fluent validator builder for defining validation rules.
            Provides an alternative to ValidatorBase for more concise validator definitions.

#### Methods & Properties

- **RuleFor``1** - Add a rule for a property using fluent API.
- **ValidateAsync** - Validate instance against all rules.

### IWebhookRegistry

- **Namespace:** `SmartWorkz.Shared.IWebhookRegistry`
- **Summary:** Abstraction for managing webhook subscriptions and registrations.
            Supports CRUD operations and subscription queries.

#### Methods & Properties

- **RegisterAsync** - Register a new webhook subscription.
  - Parameters:
    - `url`: The webhook endpoint URL.
    - `events`: Array of event names to subscribe to.
    - `secret`: Optional HMAC-SHA256 secret for signature verification.
    - `cancellationToken`: Cancellation token.
  - Returns: The ID of the newly registered subscription.
- **UnregisterAsync** - Unregister and remove a webhook subscription.
  - Parameters:
    - `subscriptionId`: The subscription ID to unregister.
    - `cancellationToken`: Cancellation token.
- **GetSubscriptionsForEventAsync** - Get all active subscriptions for a specific event.
  - Parameters:
    - `eventName`: The event name to filter by.
    - `cancellationToken`: Cancellation token.
  - Returns: Collection of subscriptions interested in this event.
- **GetActiveSubscriptionsAsync** - Get all currently active subscriptions.
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: Collection of all active subscriptions.
- **UpdateSubscriptionStatusAsync** - Update the status and failure tracking of a subscription.
  - Parameters:
    - `subscriptionId`: The subscription ID to update.
    - `isActive`: Whether the subscription should remain active.
    - `failureCount`: Number of consecutive failures (null to leave unchanged).
    - `failureReason`: Reason for failure (null to clear).
    - `cancellationToken`: Cancellation token.

### SqlWebhookRegistry

- **Namespace:** `SmartWorkz.Shared.SqlWebhookRegistry`
- **Summary:** SQL Server implementation of IWebhookRegistry.
            Persists webhook subscriptions to the database with support for querying and status updates.

### WebhookDeliveryService

- **Namespace:** `SmartWorkz.Shared.WebhookDeliveryService`
- **Summary:** Service for publishing domain events to registered webhook endpoints.
            Implements exponential backoff retry logic, HMAC signature verification, and failure tracking.

#### Methods & Properties

- **PublishEventAsync** - Publish an event to all subscribed webhook endpoints.
  - Parameters:
    - `eventName`: The name of the event being published.
    - `payload`: The event payload to send.
    - `cancellationToken`: Cancellation token.
- **DeliverAsync** - Deliver an event to a single webhook endpoint with exponential backoff retry logic.
- **GenerateSignature** - Generate HMAC-SHA256 signature for webhook payload verification.

