namespace SmartWorkz.Core;

/// <summary>
/// Enumeration representing the status of an operation result in domain or service layer responses.
/// </summary>
/// <remarks>
/// Result Status Framework:
/// ResultStatus defines the outcome of service operations, enabling consistent error handling and client-side
/// decision making. It maps directly to HTTP status codes and guides appropriate error messages and recovery actions.
///
/// Status Categories:
/// Success States:
/// - Success (0) — Operation completed successfully. Data is valid and ready for use.
///
/// Error States:
/// - Failure (1) — Generic operational failure. Used for unexpected errors not covered by specific categories.
/// - ValidationError (2) — Input validation failure. Indicates client-submitted data violates business rules or format constraints.
/// - NotFound (3) — Requested resource does not exist. Maps to HTTP 404.
/// - Unauthorized (4) — User is not authenticated or authentication failed. Maps to HTTP 401.
/// - Forbidden (5) — User lacks permissions for the requested operation. Maps to HTTP 403.
///
/// Integration:
/// - Used in: Service return types (Result{T}), API responses, error handling middleware
/// - HTTP Mapping: Success→200, ValidationError→400, NotFound→404, Unauthorized→401, Forbidden→403, Failure→500
/// - Client Handling: Clients inspect ResultStatus to determine response action (retry, redirect, display error)
/// - Error Messages: Each status guides localized error message selection
/// - Logging: Status determines log level (Success→Info, Failure→Error, Unauthorized→Warn)
///
/// Example Scenario:
/// A GetCustomer(id) service operation returns:
/// - Success if customer found
/// - NotFound if customer ID doesn't exist
/// - Unauthorized if user lacks permission to view that customer
/// - ValidationError if the provided ID format is invalid
/// </remarks>
public enum ResultStatus
{
    /// <summary>
    /// Success state — Operation completed successfully with valid result data.
    /// </summary>
    /// <remarks>
    /// When to use: Returned when the requested operation completed successfully and produced a valid result.
    /// No error condition or exceptional circumstance occurred.
    ///
    /// Example Scenarios:
    /// - GetCustomer(customerId) successfully returned customer data
    /// - SaveOrder() successfully persisted the order to database
    /// - ValidateEmail(email) confirmed email is valid and not in use
    ///
    /// Client Action: Process the returned data normally. Update UI with results. Log as informational.
    /// HTTP Status: 200 OK
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Success")]
    Success = 0,

    /// <summary>
    /// Failure state — Operation encountered an unexpected error outside categorized failure types.
    /// </summary>
    /// <remarks>
    /// When to use: For operational errors that don't fit specific categories (ValidationError, NotFound, etc.).
    /// Typically indicates a server-side or system-level exception.
    ///
    /// Example Scenarios:
    /// - Database connection timeout
    /// - Third-party service integration failure
    /// - Unhandled exception in business logic
    /// - File system or resource access error
    ///
    /// Client Action: Log error for diagnostics. Display generic "Something went wrong" message.
    /// Optionally retry after exponential backoff for transient failures.
    /// HTTP Status: 500 Internal Server Error
    ///
    /// Note: Avoid returning Failure for client-correctable issues (use ValidationError instead).
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Failure")]
    Failure = 1,

    /// <summary>
    /// Validation Error state — Input data violates business rules or format constraints.
    /// </summary>
    /// <remarks>
    /// When to use: When client-supplied data fails validation checks before or during processing.
    /// Indicates the client should correct input and retry.
    ///
    /// Example Scenarios:
    /// - Email address format is invalid
    /// - Password too short or doesn't meet complexity requirements
    /// - Required field is empty
    /// - Age constraint (e.g., must be 18+) is violated
    /// - Quantity exceeds available inventory
    ///
    /// Client Action: Display field-level error messages to user. Enable correction and resubmission.
    /// Do not retry automatically without user correction.
    /// HTTP Status: 400 Bad Request
    ///
    /// Best Practice: Include detailed validation error details in response (field names, failure reasons).
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Validation Error")]
    ValidationError = 2,

    /// <summary>
    /// Not Found state — Requested resource does not exist or is no longer available.
    /// </summary>
    /// <remarks>
    /// When to use: When a resource identified by the request (ID, key, etc.) does not exist in the system.
    ///
    /// Example Scenarios:
    /// - GetCustomer(999) where customer ID 999 doesn't exist
    /// - GetOrder(deletedOrderId) where the order was deleted
    /// - GetProduct(discontinuedSku) where the SKU is no longer in inventory
    ///
    /// Client Action: Inform user that the resource no longer exists. Offer related alternatives.
    /// Do not retry (resource won't magically appear).
    /// HTTP Status: 404 Not Found
    ///
    /// Note: Distinguish from Unauthorized (401) and Forbidden (403) when access is restricted by permissions.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Not Found")]
    NotFound = 3,

    /// <summary>
    /// Unauthorized state — User authentication failed or is not authenticated.
    /// </summary>
    /// <remarks>
    /// When to use: When the current user has not authenticated or authentication credentials are invalid/expired.
    ///
    /// Example Scenarios:
    /// - User attempts action without logging in
    /// - API token is missing or invalid
    /// - Authentication cookie expired
    /// - Wrong username/password provided
    ///
    /// Client Action: Redirect to login page. Clear any cached credentials. Prompt for re-authentication.
    /// HTTP Status: 401 Unauthorized
    ///
    /// Note: Distinguish from Forbidden (403). Unauthorized = "Who are you?", Forbidden = "I know who you are, but you can't do this."
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Unauthorized")]
    Unauthorized = 4,

    /// <summary>
    /// Forbidden state — User is authenticated but lacks permission for the requested operation.
    /// </summary>
    /// <remarks>
    /// When to use: When the current authenticated user lacks the required role, permission, or authorization
    /// to perform the requested action.
    ///
    /// Example Scenarios:
    /// - Non-admin user attempts to delete another user
    /// - Standard employee attempts to access payroll reports (requires Manager role)
    /// - User attempts to modify another user's profile (ownership check fails)
    /// - API endpoint requires "Admin" scope but token only has "User" scope
    ///
    /// Client Action: Display permission-denied message. Offer alternative authorized actions.
    /// Do not retry (permissions won't change without explicit grant).
    /// HTTP Status: 403 Forbidden
    ///
    /// Best Practice: Log authorization failures for security audit and anomaly detection.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Forbidden")]
    Forbidden = 5
}
