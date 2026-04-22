namespace SmartWorkz.Core;

/// <summary>
/// Enumeration representing various state transitions and conditions for domain entities across multiple operational categories.
/// </summary>
/// <remarks>
/// EntityState Categorization Framework:
/// The EntityState enum provides comprehensive state management across 15 major categories, enabling fine-grained
/// entity lifecycle tracking throughout their operational lifetime. This enum extends the basic EntityStatus lifecycle
/// with additional states for specialized operational contexts (verification, approval, documents, security, payments, orders,
/// inventory, communication, subscription, user account, returns/refunds, task/jobs, reviews/ratings, and shipping).
///
/// Category Organization:
///
/// 1. LIFECYCLE (0-3)
/// Active (0) — Entity is fully operational and visible in standard queries
/// Inactive (1) — Entity is temporarily disabled but preserved for potential reactivation
/// Archived (2) — Entity is retained for historical reference with no reactivation expected
/// Deleted (3) — Entity is soft-deleted but retained for data integrity and audit trails
///
/// 2. VERIFICATION (4-7)
/// EmailVerified (4) — Email address has been verified and authenticated
/// EmailPending (5) — Email verification is awaiting confirmation
/// PhoneVerified (6) — Phone number has been verified and authenticated
/// PhonePending (7) — Phone verification is awaiting confirmation
///
/// 3. APPROVAL (8-10)
/// PendingApproval (8) — Entity is awaiting approval from authorized personnel
/// Approved (9) — Entity has been approved and is authorized for use
/// Rejected (10) — Entity has been rejected and requires remediation or resubmission
///
/// 4. DOCUMENT (11-14)
/// Draft (11) — Document is in draft state and not yet submitted
/// Submitted (12) — Document has been submitted for processing
/// InReview (13) — Document is currently under review
/// Completed (14) — Document processing is complete
///
/// 5. SECURITY (15-18)
/// PasswordReset (15) — Password reset process has been initiated
/// PasswordResetting (16) — Password is currently being reset
/// Locked (17) — Account is locked due to security concerns
/// Suspended (18) — Account is suspended, preventing normal operations
///
/// 6. PAYMENT (19-21)
/// PaymentPending (19) — Payment is awaiting processing
/// PaymentReceived (20) — Payment has been successfully received
/// PaymentFailed (21) — Payment processing has failed
///
/// 7. ORDER (22-27)
/// OrderPlaced (22) — Order has been placed
/// OrderConfirmed (23) — Order has been confirmed
/// Shipped (24) — Order has been shipped
/// Delivered (25) — Order has been delivered
/// Returned (26) — Order has been returned
/// Cancelled (27) — Order has been cancelled
///
/// 8. INVENTORY/STOCK (28-31)
/// OutOfStock (28) — Item is out of stock and unavailable for purchase
/// LowStock (29) — Item stock level is below minimum threshold
/// Restocking (30) — Item is currently being restocked
/// BackOrder (31) — Item is on back order pending availability
///
/// 9. COMMUNICATION (32-35)
/// Read (32) — Message has been read by recipient
/// Unread (33) — Message is unread by recipient
/// Forwarded (34) — Message has been forwarded to other recipients
/// Replied (35) — Reply has been sent to the message
///
/// 10. SUBSCRIPTION (36-40)
/// Trial (36) — Subscription is in trial period
/// Active (37) — Subscription is active and current
/// Paused (38) — Subscription is paused but can be resumed
/// Cancelled (39) — Subscription has been cancelled
/// Expired (40) — Subscription has expired
///
/// 11. USER ACCOUNT (41-46)
/// EmailVerificationPending (41) — Email verification is pending
/// PhoneVerificationPending (42) — Phone verification is pending
/// KYCPending (43) — Know Your Customer (KYC) verification is pending
/// KYCApproved (44) — KYC verification has been approved
/// KYCRejected (45) — KYC verification has been rejected
/// AccountSuspended (46) — Account has been suspended
///
/// 12. RETURN/REFUND (47-51)
/// ReturnRequested (47) — Return has been requested by customer
/// ReturnApproved (48) — Return has been approved
/// ReturnRejected (49) — Return has been rejected
/// ReturnInProgress (50) — Return is currently in progress
/// ReturnCompleted (51) — Return has been completed
///
/// 13. TASK/JOB (52-56)
/// Assigned (52) — Task has been assigned to a resource
/// InProgress (53) — Task is currently being worked on
/// OnHold (54) — Task is on hold pending action
/// Completed (55) — Task has been completed
/// Failed (56) — Task has failed
///
/// 14. REVIEW/RATING (57-60)
/// PendingReview (57) — Review is awaiting approval
/// UnderReview (58) — Review is currently under review
/// ReviewApproved (59) — Review has been approved
/// ReviewRejected (60) — Review has been rejected
///
/// 15. SHIPPING STATUS (61-63)
/// InTransit (61) — Order is in transit to destination
/// OutForDelivery (62) — Order is out for delivery
/// Delivered (63) — Order has been delivered
///
/// Integration Guidelines:
/// - Use lifecycle states (0-3) for basic entity presence/absence
/// - Use specialized states (4-63) for context-specific operations
/// - Multiple states may apply to an entity depending on business logic
/// - States should be evaluated together for comprehensive entity status
/// - Default: Depends on entity type (typically Active + context-specific state)
///
/// Design Patterns:
/// - States are sequential and categorized for logical grouping
/// - Display attributes provide user-friendly names for UI rendering
/// - Can be extended with additional categories following the same pattern
/// - Integrate with specifications, repositories, and service layers for querying
/// </remarks>
public enum EntityState
{
    #region Lifecycle Category (0-3)

    /// <summary>
    /// Active state — Entity is fully operational and visible in standard queries.
    /// </summary>
    /// <remarks>
    /// When to use: Default state for all newly created entities. Use when entity should be fully available for business operations.
    ///
    /// Example Scenarios:
    /// - A newly registered user account
    /// - A freshly created product in inventory
    /// - An active project or order
    ///
    /// Query filtering: Standard queries should filter WHERE State = Active unless explicitly including historical states.
    ///
    /// Integration: Use this as the default state for entities entering the system for the first time.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Active")]
    Active = 0,

    /// <summary>
    /// Inactive state — Entity is temporarily disabled but preserved for potential reactivation.
    /// </summary>
    /// <remarks>
    /// When to use: When an entity needs to be hidden from normal operations but may be needed again in the future.
    /// This is a reversible state with lower overhead than archival.
    ///
    /// Example Scenarios:
    /// - A user account temporarily suspended due to security concerns, but can be reactivated
    /// - A sales employee on leave who should not appear in assignments
    /// - A vendor temporarily unable to fulfill orders but expected to resume
    ///
    /// Characteristics:
    /// - Data is fully preserved and can be reactivated with minimal impact
    /// - Associated records (orders, relationships) typically remain linked but become invisible
    /// - Shorter duration expectation than Archived
    ///
    /// Integration: Service layers should exclude Inactive entities from primary queries but include them in administrative interfaces.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Inactive")]
    Inactive = 1,

    /// <summary>
    /// Archived state — Entity is retained for historical reference and audit purposes with no reactivation expected.
    /// </summary>
    /// <remarks>
    /// When to use: When an entity's operational lifecycle is complete and its data should be preserved for historical,
    /// compliance, or statistical purposes without expectation of reactivation.
    ///
    /// Example Scenarios:
    /// - A completed project maintained for portfolio/audit purposes
    /// - A fulfilled order kept for customer history and financial records
    /// - A retired business unit preserved for historical analysis
    ///
    /// Characteristics:
    /// - Typically applies to entities with time-bound relevance
    /// - Data is immutable and preserved indefinitely
    /// - May be included in read-only queries for reporting/analytics
    /// - Generally not filtered from comprehensive audits
    ///
    /// Integration: Include in historical and reporting queries; exclude from operational systems unless explicitly requested.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Archived")]
    Archived = 2,

    /// <summary>
    /// Deleted state — Soft-deleted entity marked for removal but physically retained for data integrity.
    /// </summary>
    /// <remarks>
    /// When to use: When an entity must be removed from normal operations due to user request, compliance requirements,
    /// or data retention policies, while maintaining referential integrity and audit trails.
    ///
    /// Example Scenarios:
    /// - A user account deleted per GDPR request (with anonymization of related records)
    /// - A transaction marked for deletion but retained in ledger for reconciliation
    /// - An organization removed from active use but kept for tax/compliance records
    ///
    /// Characteristics:
    /// - Data is preserved indefinitely per compliance and audit requirements
    /// - Excluded from all standard application queries
    /// - May be included only in administrative/forensic queries
    /// - Physical deletion is rare and requires explicit administrative action
    /// - Often paired with data masking/anonymization for privacy compliance
    ///
    /// Integration: Service layer should always exclude Deleted entities except in compliance/audit contexts.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Deleted")]
    Deleted = 3,

    #endregion

    #region Verification Category (4-7)

    /// <summary>
    /// EmailVerified state — Email address has been verified and authenticated by the owner.
    /// </summary>
    /// <remarks>
    /// When to use: When an email address has been successfully verified through confirmation link or OTP.
    ///
    /// Example Scenarios:
    /// - A user confirms email via confirmation link sent to their inbox
    /// - A registered email address passes verification protocol
    /// - An email contact is confirmed and ready for communications
    ///
    /// Characteristics:
    /// - Email address ownership has been proven
    /// - User can receive critical communications at this address
    /// - May grant access to sensitive features
    /// - Should be tracked for compliance and audit purposes
    ///
    /// Integration: Check this state before sending password reset or authentication links.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Email Verified")]
    EmailVerified = 4,

    /// <summary>
    /// EmailPending state — Email verification is awaiting confirmation from the owner.
    /// </summary>
    /// <remarks>
    /// When to use: When an email address has been provided but not yet verified.
    ///
    /// Example Scenarios:
    /// - A newly registered email waiting for confirmation
    /// - An email address provided during account creation
    /// - A contact email pending verification
    ///
    /// Characteristics:
    /// - Verification token/link has been sent but not yet confirmed
    /// - Temporary state that should be resolved quickly
    /// - Email communications should not be relied upon until verified
    /// - Confirmation link may expire after a set period
    ///
    /// Integration: Services should limit functionality until email is verified.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Email Pending")]
    EmailPending = 5,

    /// <summary>
    /// PhoneVerified state — Phone number has been verified and authenticated by the owner.
    /// </summary>
    /// <remarks>
    /// When to use: When a phone number has been successfully verified through SMS OTP or phone call.
    ///
    /// Example Scenarios:
    /// - A user confirms phone via SMS OTP
    /// - A registered phone number passes verification protocol
    /// - A phone contact is confirmed for 2FA/MFA
    ///
    /// Characteristics:
    /// - Phone number ownership has been proven
    /// - User can receive SMS/calls for authentication and critical messages
    /// - Required for multi-factor authentication setup
    /// - Enables SMS-based notifications
    ///
    /// Integration: Required for 2FA/MFA features and SMS notifications.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Phone Verified")]
    PhoneVerified = 6,

    /// <summary>
    /// PhonePending state — Phone verification is awaiting confirmation from the owner.
    /// </summary>
    /// <remarks>
    /// When to use: When a phone number has been provided but not yet verified.
    ///
    /// Example Scenarios:
    /// - A phone number submitted during profile setup
    /// - Phone verification OTP sent and awaiting confirmation
    /// - A contact phone pending verification
    ///
    /// Characteristics:
    /// - Verification OTP/token has been sent but not yet confirmed
    /// - Temporary state that should be resolved quickly
    /// - Phone communications should not be relied upon until verified
    /// - Verification may be required before account activation
    ///
    /// Integration: Services should enforce phone verification for security-critical operations.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Phone Pending")]
    PhonePending = 7,

    #endregion

    #region Approval Category (8-10)

    /// <summary>
    /// PendingApproval state — Entity is awaiting approval from authorized personnel.
    /// </summary>
    /// <remarks>
    /// When to use: When an entity requires authorization from a designated approver before proceeding.
    ///
    /// Example Scenarios:
    /// - A document submitted for manager approval
    /// - A purchase request awaiting budget approval
    /// - A user account awaiting admin activation
    /// - A vendor application under review
    ///
    /// Characteristics:
    /// - Entity is complete and ready for evaluation
    /// - Awaiting action from authorized reviewer
    /// - May have service restrictions until approved
    /// - Timeline for approval may be tracked
    ///
    /// Integration: Workflows should identify pending items for approvers and track approval deadlines.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Pending Approval")]
    PendingApproval = 8,

    /// <summary>
    /// Approved state — Entity has been approved and is authorized for use.
    /// </summary>
    /// <remarks>
    /// When to use: When an entity has received required authorization and is cleared for operation.
    ///
    /// Example Scenarios:
    /// - A document approved and ready for distribution
    /// - A purchase request approved for fulfillment
    /// - A user account activated by administrator
    /// - A vendor approved for transactions
    ///
    /// Characteristics:
    /// - All requirements have been met and verified
    /// - Entity is authorized for intended operations
    /// - Audit trail shows approval chain
    /// - Typically the state enabling full functionality
    ///
    /// Integration: Business logic should enable full functionality when Approved.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Approved")]
    Approved = 9,

    /// <summary>
    /// Rejected state — Entity has been rejected and requires remediation or resubmission.
    /// </summary>
    /// <remarks>
    /// When to use: When an entity fails approval and must be corrected before resubmission.
    ///
    /// Example Scenarios:
    /// - A document rejected due to missing information
    /// - A purchase request denied due to budget constraints
    /// - A user account application rejected for non-compliance
    /// - A vendor rejected due to failed verification
    ///
    /// Characteristics:
    /// - Rejection reason/feedback should be documented
    /// - Entity may be resubmitted after corrections
    /// - Original submission remains in history
    /// - User notification about rejection is critical
    ///
    /// Integration: Workflow should notify user of rejection with detailed feedback and resubmission guidance.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Rejected")]
    Rejected = 10,

    #endregion

    #region Document Category (11-14)

    /// <summary>
    /// Draft state — Document is in draft state and not yet submitted.
    /// </summary>
    /// <remarks>
    /// When to use: When a document is being created or edited by the owner and not ready for official processing.
    ///
    /// Example Scenarios:
    /// - A report being written and edited
    /// - A form being completed by user
    /// - An application being prepared for submission
    /// - A contract under negotiation
    ///
    /// Characteristics:
    /// - Only visible to the creator/owner
    /// - Can be freely edited without workflow impact
    /// - Not officially recorded until submitted
    /// - Typically has automatic save functionality
    ///
    /// Integration: Allow unrestricted editing; prevent sharing until submitted.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Draft")]
    Draft = 11,

    /// <summary>
    /// Submitted state — Document has been submitted for processing.
    /// </summary>
    /// <remarks>
    /// When to use: When a document has been formally submitted and entered the official workflow.
    ///
    /// Example Scenarios:
    /// - A report submitted to management
    /// - A form submitted for processing
    /// - An application formally filed
    /// - A contract signed and submitted
    ///
    /// Characteristics:
    /// - Document enters official processing workflow
    /// - Timestamp recorded for audit trail
    /// - Document becomes immutable (may need approval to edit)
    /// - Visible to relevant workflow participants
    ///
    /// Integration: Lock document editing; route to appropriate reviewers.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Submitted")]
    Submitted = 12,

    /// <summary>
    /// InReview state — Document is currently under review.
    /// </summary>
    /// <remarks>
    /// When to use: When a document is actively being reviewed or processed.
    ///
    /// Example Scenarios:
    /// - A report being reviewed by manager
    /// - A form being verified by processor
    /// - An application under evaluation
    /// - A contract being reviewed by legal
    ///
    /// Characteristics:
    /// - Document is assigned to reviewer/processor
    /// - May be marked with review progress
    /// - Comments/feedback may be accumulated
    /// - Timeline for review completion tracked
    ///
    /// Integration: Provide review interface; track review completion time.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "In Review")]
    InReview = 13,

    /// <summary>
    /// Completed state — Document processing is complete.
    /// </summary>
    /// <remarks>
    /// When to use: When a document has completed all required processing and is finalized.
    ///
    /// Example Scenarios:
    /// - A report approved and finalized
    /// - A form processed and data entered
    /// - An application approved or denied
    /// - A contract fully executed
    ///
    /// Characteristics:
    /// - All processing steps completed
    /// - Final status determined (approved/rejected)
    /// - Document is archived or filed appropriately
    /// - Action items resolved
    ///
    /// Integration: Archive document; notify relevant parties of completion.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Completed")]
    Completed = 14,

    #endregion

    #region Security Category (15-18)

    /// <summary>
    /// PasswordReset state — Password reset process has been initiated.
    /// </summary>
    /// <remarks>
    /// When to use: When a user has requested a password reset and the process is initiated.
    ///
    /// Example Scenarios:
    /// - User clicks "Forgot Password" link
    /// - Reset email has been sent
    /// - User awaiting reset link in email
    ///
    /// Characteristics:
    /// - Reset token/link has been generated
    /// - User receives reset instructions
    /// - Account typically remains accessible until reset
    /// - Reset link has expiration time
    ///
    /// Integration: Send reset link; track reset link expiry; prevent password-dependent operations.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Password Reset")]
    PasswordReset = 15,

    /// <summary>
    /// PasswordResetting state — Password is currently being reset.
    /// </summary>
    /// <remarks>
    /// When to use: When a user is actively in the process of resetting their password.
    ///
    /// Example Scenarios:
    /// - User has clicked reset link and opened reset form
    /// - User is entering new password
    /// - Password validation in progress
    ///
    /// Characteristics:
    /// - User has verified identity (via reset link)
    /// - New password validation in progress
    /// - Temporary state during reset operation
    /// - Failed validations should keep user in this state
    ///
    /// Integration: Provide password reset UI; validate new password requirements; show reset progress.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Password Resetting")]
    PasswordResetting = 16,

    /// <summary>
    /// Locked state — Account is locked due to security concerns.
    /// </summary>
    /// <remarks>
    /// When to use: When an account is temporarily locked due to security issues or excessive failed attempts.
    ///
    /// Example Scenarios:
    /// - Too many failed login attempts
    /// - Suspicious login activity detected
    /// - Administrator locks account due to investigation
    /// - Account compromise suspected
    ///
    /// Characteristics:
    /// - Login attempts prevented
    /// - Account may be unlocked automatically after timeout
    /// - Administrator can unlock manually
    /// - User should be notified of lock reason
    /// - Lock duration should be tracked
    ///
    /// Integration: Prevent all login attempts; enforce unlock through admin or wait period.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Locked")]
    Locked = 17,

    /// <summary>
    /// Suspended state — Account is suspended, preventing normal operations.
    /// </summary>
    /// <remarks>
    /// When to use: When an account is temporarily or permanently suspended due to policy violations or administrative action.
    ///
    /// Example Scenarios:
    /// - Account suspended due to terms of service violation
    /// - Administrative suspension pending investigation
    /// - Account suspended due to non-compliance
    /// - User suspended pending review
    ///
    /// Characteristics:
    /// - Intentional administrative action
    /// - All operations prevented (login, API calls, etc.)
    /// - Longer-term than locked state
    /// - May require administrator to reinstate
    /// - Reason for suspension should be documented
    ///
    /// Integration: Block all operations; require administrative review for reinstatement.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Suspended")]
    Suspended = 18,

    #endregion

    #region Payment Category (19-21)

    /// <summary>
    /// PaymentPending state — Payment is awaiting processing.
    /// </summary>
    /// <remarks>
    /// When to use: When a payment has been initiated but not yet processed or confirmed.
    ///
    /// Example Scenarios:
    /// - Order placed and awaiting payment processing
    /// - Payment submitted but not cleared by processor
    /// - Invoice sent and awaiting customer payment
    /// - Subscription renewal awaiting payment confirmation
    ///
    /// Characteristics:
    /// - Payment amount captured but not confirmed
    /// - Temporary state during processing
    /// - Transaction may still be cancelled
    /// - Timeline for processing should be tracked
    /// - Order fulfillment typically blocked until confirmed
    ///
    /// Integration: Prevent order fulfillment; monitor payment gateway for confirmation.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Payment Pending")]
    PaymentPending = 19,

    /// <summary>
    /// PaymentReceived state — Payment has been successfully received.
    /// </summary>
    /// <remarks>
    /// When to use: When a payment has been successfully processed and confirmed.
    ///
    /// Example Scenarios:
    /// - Payment cleared by bank/payment processor
    /// - Funds received and available
    /// - Order approved for fulfillment
    /// - Invoice marked as paid
    ///
    /// Characteristics:
    /// - Payment fully confirmed
    /// - Funds available for settlement
    /// - Receipt may be generated
    /// - Order fulfillment can proceed
    /// - Financial records should be updated
    ///
    /// Integration: Update financials; approve order fulfillment; generate receipt.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Payment Received")]
    PaymentReceived = 20,

    /// <summary>
    /// PaymentFailed state — Payment processing has failed.
    /// </summary>
    /// <remarks>
    /// When to use: When a payment attempt has failed for any reason.
    ///
    /// Example Scenarios:
    /// - Card declined by processor
    /// - Insufficient funds in account
    /// - Payment gateway error or timeout
    /// - Account restrictions prevent payment
    ///
    /// Characteristics:
    /// - Payment processing did not complete
    /// - Transaction rolled back
    /// - Funds not charged
    /// - Failure reason should be documented
    /// - May enable retry attempts
    /// - Order fulfillment blocked
    ///
    /// Integration: Notify user of failure with reason; offer retry or alternative payment methods.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Payment Failed")]
    PaymentFailed = 21,

    #endregion

    #region Order Category (22-27)

    /// <summary>
    /// OrderPlaced state — Order has been placed.
    /// </summary>
    /// <remarks>
    /// When to use: When a customer has submitted an order for fulfillment.
    ///
    /// Example Scenarios:
    /// - Customer completes checkout and places order
    /// - Order created in system
    /// - Order confirmation sent to customer
    /// - Order awaiting payment verification
    ///
    /// Characteristics:
    /// - Order information captured in system
    /// - Order number generated
    /// - Confirmation sent to customer
    /// - Payment verification pending
    /// - Inventory may be temporarily reserved
    ///
    /// Integration: Generate order number; send confirmation; initialize fulfillment workflow.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Order Placed")]
    OrderPlaced = 22,

    /// <summary>
    /// OrderConfirmed state — Order has been confirmed.
    /// </summary>
    /// <remarks>
    /// When to use: When an order has been verified and confirmed for fulfillment.
    ///
    /// Example Scenarios:
    /// - Payment received and confirmed
    /// - Order details verified
    /// - Inventory allocation confirmed
    /// - Order ready for fulfillment
    ///
    /// Characteristics:
    /// - All verifications passed
    /// - Payment confirmed
    /// - Inventory confirmed available
    /// - Order prepared for warehouse processing
    /// - Fulfillment timeline established
    ///
    /// Integration: Route to warehouse; block inventory; establish fulfillment SLA.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Order Confirmed")]
    OrderConfirmed = 23,

    /// <summary>
    /// Shipped state — Order has been shipped.
    /// </summary>
    /// <remarks>
    /// When to use: When an order has been picked, packed, and dispatched.
    ///
    /// Example Scenarios:
    /// - Order picked from warehouse
    /// - Items packed and label applied
    /// - Handed to shipping carrier
    /// - Tracking number provided to customer
    ///
    /// Characteristics:
    /// - Order no longer in warehouse
    /// - In transit to customer
    /// - Tracking information available
    /// - Delivery timeline estimated
    /// - Return to warehouse blocked
    ///
    /// Integration: Update inventory; send tracking notification; monitor carrier status.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Shipped")]
    Shipped = 24,

    /// <summary>
    /// Delivered state — Order has been delivered.
    /// </summary>
    /// <remarks>
    /// When to use: When an order has reached the customer or designated delivery address.
    ///
    /// Example Scenarios:
    /// - Package delivered to customer address
    /// - Signature obtained (if required)
    /// - Delivery confirmation received
    /// - Customer can begin return window
    ///
    /// Characteristics:
    /// - Order in customer's possession
    /// - Delivery confirmed
    /// - Return period typically starts
    /// - Order fulfillment essentially complete
    /// - Customer satisfaction tracking begins
    ///
    /// Integration: Confirm delivery; start return window; request feedback/review.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Delivered")]
    Delivered = 25,

    /// <summary>
    /// Returned state — Order has been returned.
    /// </summary>
    /// <remarks>
    /// When to use: When a customer has returned delivered items.
    ///
    /// Example Scenarios:
    /// - Customer initiates return request
    /// - Items returned to warehouse
    /// - Return inspected and verified
    /// - Refund processed
    ///
    /// Characteristics:
    /// - Return authorized and processed
    /// - Items back in warehouse
    /// - Inspection/testing in progress
    /// - Refund initiated or completed
    /// - Inventory restored
    ///
    /// Integration: Track return status; inspect items; process refund; update inventory.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Returned")]
    Returned = 26,

    /// <summary>
    /// Cancelled state — Order has been cancelled.
    /// </summary>
    /// <remarks>
    /// When to use: When an order is cancelled by customer or system before completion.
    ///
    /// Example Scenarios:
    /// - Customer cancels before fulfillment
    /// - System cancels due to inventory unavailability
    /// - Payment failed and no retry
    /// - Order cancelled due to inactivity
    ///
    /// Characteristics:
    /// - Order will not be fulfilled
    /// - Cancellation reason documented
    /// - Refund may be processed
    /// - Inventory released
    /// - Fulfillment workflow stopped
    ///
    /// Integration: Release inventory; stop fulfillment; process refund if applicable; notify customer.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Cancelled")]
    Cancelled = 27,

    #endregion

    #region Inventory/Stock Category (28-31)

    /// <summary>
    /// OutOfStock state — Item is out of stock and unavailable for purchase.
    /// </summary>
    /// <remarks>
    /// When to use: When an item's inventory is exhausted and not currently available.
    ///
    /// Example Scenarios:
    /// - Popular item sold out
    /// - Inventory depleted faster than expected
    /// - Item temporarily unavailable
    /// - Waiting for replenishment
    ///
    /// Characteristics:
    /// - No units available for sale
    /// - Customer orders may be blocked or added to waitlist
    /// - Backfill process may be initiated
    /// - Restocking notification may be offered
    ///
    /// Integration: Block purchase; offer notifications; manage waitlist.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Out Of Stock")]
    OutOfStock = 28,

    /// <summary>
    /// LowStock state — Item stock level is below minimum threshold.
    /// </summary>
    /// <remarks>
    /// When to use: When item inventory falls below predetermined safety threshold.
    ///
    /// Example Scenarios:
    /// - Stock approaching critical level
    /// - Automatic reorder threshold reached
    /// - Limited quantities remaining
    /// - High-velocity item depleting quickly
    ///
    /// Characteristics:
    /// - Available for purchase with notification
    /// - Automatic reorder may be triggered
    /// - Inventory monitoring intensified
    /// - Supplier alerts may be sent
    ///
    /// Integration: Send reorder notifications; display limited availability warnings.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Low Stock")]
    LowStock = 29,

    /// <summary>
    /// Restocking state — Item is currently being restocked.
    /// </summary>
    /// <remarks>
    /// When to use: When inventory replenishment is actively in progress.
    ///
    /// Example Scenarios:
    /// - Purchase order received and being processed
    /// - Items in receiving area pending inspection
    /// - Quality checks in progress
    /// - Items being moved to storage
    ///
    /// Characteristics:
    /// - Inventory count increasing
    /// - Items may not yet be available for sale
    /// - Temporary operational status
    /// - Expected completion timeline
    ///
    /// Integration: Track restocking progress; update availability when complete.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Restocking")]
    Restocking = 30,

    /// <summary>
    /// BackOrder state — Item is on back order pending availability.
    /// </summary>
    /// <remarks>
    /// When to use: When customer orders are placed for unavailable items to be fulfilled later.
    ///
    /// Example Scenarios:
    /// - Customer orders item not currently in stock
    /// - Expected to arrive from supplier
    /// - Orders queued for shipment when available
    /// - Advance orders for upcoming releases
    ///
    /// Characteristics:
    /// - Orders accepted despite stock shortage
    /// - Fulfillment pending supplier delivery
    /// - Customer notified of expected availability
    /// - Priority queue management
    ///
    /// Integration: Accept orders; manage backlog; notify customer of shipment readiness.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Back Order")]
    BackOrder = 31,

    #endregion

    #region Communication Category (32-35)

    /// <summary>
    /// Read state — Message has been read by recipient.
    /// </summary>
    /// <remarks>
    /// When to use: When a message has been opened and viewed by the recipient.
    ///
    /// Example Scenarios:
    /// - Email opened by recipient
    /// - In-app notification viewed
    /// - SMS message read
    /// - Document reviewed
    ///
    /// Characteristics:
    /// - Read timestamp recorded
    /// - Read status updated in UI
    /// - May trigger follow-up workflows
    /// - Analytics tracked
    ///
    /// Integration: Mark as read; update timestamp; trigger read receipts.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Read")]
    Read = 32,

    /// <summary>
    /// Unread state — Message is unread by recipient.
    /// </summary>
    /// <remarks>
    /// When to use: When a message has been delivered but not yet viewed.
    ///
    /// Example Scenarios:
    /// - Email received but not opened
    /// - Notification received and pending
    /// - SMS delivered but not read
    /// - Document available but not accessed
    ///
    /// Characteristics:
    /// - Delivery confirmed
    /// - Read status not recorded
    /// - Message remains in unread collection
    /// - Notification badge may appear
    ///
    /// Integration: Filter unread messages; show notification indicators.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Unread")]
    Unread = 33,

    /// <summary>
    /// Forwarded state — Message has been forwarded to other recipients.
    /// </summary>
    /// <remarks>
    /// When to use: When a message is resent to additional recipients by the receiver.
    ///
    /// Example Scenarios:
    /// - Email forwarded to colleagues
    /// - Message shared with team
    /// - Document distributed to stakeholders
    /// - Information cascade to relevant parties
    ///
    /// Characteristics:
    /// - Original message preserved
    /// - New recipient list added
    /// - Forward chain tracked
    /// - Timestamp of forward recorded
    ///
    /// Integration: Track forwarding; update recipient list; log audit trail.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Forwarded")]
    Forwarded = 34,

    /// <summary>
    /// Replied state — Reply has been sent to the message.
    /// </summary>
    /// <remarks>
    /// When to use: When a response has been sent back to the original message sender.
    ///
    /// Example Scenarios:
    /// - Email reply sent
    /// - Conversation continued
    /// - Question answered
    /// - Feedback provided
    ///
    /// Characteristics:
    /// - Response timestamp recorded
    /// - Original message linked to reply
    /// - Conversation thread updated
    /// - Status marked as replied
    ///
    /// Integration: Link reply to original; update conversation thread; mark status.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Replied")]
    Replied = 35,

    #endregion

    #region Subscription Category (36-40)

    /// <summary>
    /// Trial state — Subscription is in trial period.
    /// </summary>
    /// <remarks>
    /// When to use: When a subscription is in its initial trial or evaluation period.
    ///
    /// Example Scenarios:
    /// - New subscriber in free trial
    /// - Evaluation period before paid conversion
    /// - Limited-time access for testing
    /// - Freemium tier with upgrade option
    ///
    /// Characteristics:
    /// - Limited duration (typically 7-30 days)
    /// - Full or restricted feature access
    /// - No payment required
    /// - Conversion to paid pending
    /// - Expiration warning notifications
    ///
    /// Integration: Apply feature restrictions; send conversion prompts; manage expiration.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Trial")]
    Trial = 36,

    /// <summary>
    /// Active state — Subscription is active and current.
    /// </summary>
    /// <remarks>
    /// When to use: When a subscription is in good standing and services are active.
    ///
    /// Example Scenarios:
    /// - Paid subscriber with current payment
    /// - Service fully accessible
    /// - Billing cycle current
    /// - All features enabled
    ///
    /// Characteristics:
    /// - Payment up to date
    /// - Service fully functional
    /// - Renewal date scheduled
    /// - No access restrictions
    ///
    /// Integration: Grant full access; enable all features; track renewal.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Subscription Active")]
    SubscriptionActive = 37,

    /// <summary>
    /// Paused state — Subscription is paused but can be resumed.
    /// </summary>
    /// <remarks>
    /// When to use: When a subscriber temporarily suspends their subscription.
    ///
    /// Example Scenarios:
    /// - Subscriber pauses due to non-use
    /// - Temporary deferment of service
    /// - Seasonal subscription pause
    /// - Customer retention feature
    ///
    /// Characteristics:
    /// - Payment suspended
    /// - Data retained
    /// - Services unavailable
    /// - Can be resumed on-demand
    /// - Often maintains user settings/data
    ///
    /// Integration: Suspend charges; retain data; provide resume options.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Paused")]
    Paused = 38,

    /// <summary>
    /// Cancelled state — Subscription has been cancelled.
    /// </summary>
    /// <remarks>
    /// When to use: When a subscriber has terminated their subscription.
    ///
    /// Example Scenarios:
    /// - Customer cancels service
    /// - Voluntary unsubscription
    /// - End of contract
    /// - Non-renewal of subscription
    ///
    /// Characteristics:
    /// - Services revoked
    /// - Access terminated
    /// - Data may be retained for period
    /// - No further charges
    /// - Offboarding workflow initiated
    ///
    /// Integration: Revoke access; stop billing; offer win-back campaigns.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Subscription Cancelled")]
    SubscriptionCancelled = 39,

    /// <summary>
    /// Expired state — Subscription has expired.
    /// </summary>
    /// <remarks>
    /// When to use: When a subscription has reached its natural end date.
    ///
    /// Example Scenarios:
    /// - Trial period ended without conversion
    /// - Annual subscription year expired
    /// - Membership term concluded
    /// - License validity period ended
    ///
    /// Characteristics:
    /// - Services no longer available
    /// - Renewal required to restore
    /// - Automatic expiration without action
    /// - May trigger renewal reminders
    ///
    /// Integration: Block access; send renewal prompts; manage grace periods.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Expired")]
    Expired = 40,

    #endregion

    #region User Account Category (41-46)

    /// <summary>
    /// EmailVerificationPending state — Email verification is pending.
    /// </summary>
    /// <remarks>
    /// When to use: When a user account is awaiting email confirmation.
    ///
    /// Example Scenarios:
    /// - New user account created pending email confirmation
    /// - Email change awaiting re-verification
    /// - Account reactivation pending email
    /// - Critical operations blocked until verified
    ///
    /// Characteristics:
    /// - Verification link sent
    /// - Limited account functionality
    /// - Temporary pending state
    /// - Expiration timer on link
    ///
    /// Integration: Send verification emails; restrict features; auto-expire links.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Email Verification Pending")]
    EmailVerificationPending = 41,

    /// <summary>
    /// PhoneVerificationPending state — Phone verification is pending.
    /// </summary>
    /// <remarks>
    /// When to use: When a user account is awaiting phone confirmation.
    ///
    /// Example Scenarios:
    /// - Two-factor authentication setup
    /// - Phone number addition to account
    /// - SMS verification for sensitive operations
    /// - Account recovery via phone
    ///
    /// Characteristics:
    /// - OTP/SMS sent to phone
    /// - Confirmation code pending
    /// - Limited verification attempts
    /// - Temporary state with expiration
    ///
    /// Integration: Send SMS codes; validate codes; manage retry limits.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Phone Verification Pending")]
    PhoneVerificationPending = 42,

    /// <summary>
    /// KYCPending state — Know Your Customer (KYC) verification is pending.
    /// </summary>
    /// <remarks>
    /// When to use: When a user account requires identity/business verification.
    ///
    /// Example Scenarios:
    /// - New user identity verification
    /// - Regulatory compliance check
    /// - Enhanced due diligence required
    /// - Document submission awaiting review
    ///
    /// Characteristics:
    /// - Documents submitted/required
    /// - Under review by compliance team
    /// - Account may have limited access
    /// - Potential rejection or additional info needed
    ///
    /// Integration: Manage document submission; track review status; enforce limits.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "KYC Pending")]
    KYCPending = 43,

    /// <summary>
    /// KYCApproved state — KYC verification has been approved.
    /// </summary>
    /// <remarks>
    /// When to use: When KYC verification has been completed successfully.
    ///
    /// Example Scenarios:
    /// - Identity verified and approved
    /// - Compliance requirements met
    /// - Full account access restored
    /// - Transaction limits lifted
    ///
    /// Characteristics:
    /// - Full account functionality enabled
    /// - No transaction restrictions
    /// - Compliance status current
    /// - Verification valid for period
    ///
    /// Integration: Enable full features; remove transaction limits; update compliance status.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "KYC Approved")]
    KYCApproved = 44,

    /// <summary>
    /// KYCRejected state — KYC verification has been rejected.
    /// </summary>
    /// <remarks>
    /// When to use: When KYC verification has failed or documents rejected.
    ///
    /// Example Scenarios:
    /// - Documents failed verification
    /// - Identity could not be confirmed
    /// - Compliance requirements not met
    /// - Resubmission required with corrections
    ///
    /// Characteristics:
    /// - Account access restricted
    /// - Transaction functionality blocked
    /// - Rejection reason provided
    /// - Resubmission instructions given
    ///
    /// Integration: Restrict access; notify user of rejection; provide resubmission guidance.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "KYC Rejected")]
    KYCRejected = 45,

    /// <summary>
    /// AccountSuspended state — Account has been suspended.
    /// </summary>
    /// <remarks>
    /// When to use: When an account is suspended due to violations or compliance.
    ///
    /// Example Scenarios:
    /// - Suspended for policy violation
    /// - Compliance investigation in progress
    /// - Fraudulent activity detected
    /// - Administrative hold pending review
    ///
    /// Characteristics:
    /// - All account functionality blocked
    /// - Access completely restricted
    /// - Communications may be limited
    /// - Review period established
    /// - Potential for reinstatement
    ///
    /// Integration: Block all operations; notify user; manage appeal process.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Account Suspended")]
    AccountSuspended = 46,

    #endregion

    #region Return/Refund Category (47-51)

    /// <summary>
    /// ReturnRequested state — Return has been requested by customer.
    /// </summary>
    /// <remarks>
    /// When to use: When a customer initiates a return request for an order.
    ///
    /// Example Scenarios:
    /// - Customer requests return within return window
    /// - Return authorization initiated
    /// - RMA (Return Merchandise Authorization) requested
    /// - Defective item return started
    ///
    /// Characteristics:
    /// - Return reason provided
    /// - Return window validated
    /// - Return label may be generated
    /// - Initial assessment pending
    ///
    /// Integration: Validate return eligibility; generate RMA; send return instructions.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Return Requested")]
    ReturnRequested = 47,

    /// <summary>
    /// ReturnApproved state — Return has been approved.
    /// </summary>
    /// <remarks>
    /// When to use: When a return request has been approved.
    ///
    /// Example Scenarios:
    /// - Return eligibility verified
    /// - RMA number issued
    /// - Return shipping authorized
    /// - Refund amount calculated
    ///
    /// Characteristics:
    /// - Return instructions sent
    /// - Return label provided
    /// - Refund amount determined
    /// - Expected return date established
    ///
    /// Integration: Issue RMA number; send return label; establish refund schedule.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Return Approved")]
    ReturnApproved = 48,

    /// <summary>
    /// ReturnRejected state — Return has been rejected.
    /// </summary>
    /// <remarks>
    /// When to use: When a return request does not meet eligibility criteria.
    ///
    /// Example Scenarios:
    /// - Return period expired
    /// - Item outside return policy
    /// - Damage beyond coverage
    /// - Non-returnable product
    ///
    /// Characteristics:
    /// - Rejection reason provided
    /// - No refund issued
    /// - Appeal process available
    /// - Customer notification sent
    ///
    /// Integration: Notify customer; provide appeal option; document reason.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Return Rejected")]
    ReturnRejected = 49,

    /// <summary>
    /// ReturnInProgress state — Return is currently in progress.
    /// </summary>
    /// <remarks>
    /// When to use: When a return is being processed after receiving returned items.
    ///
    /// Example Scenarios:
    /// - Items received at return facility
    /// - Quality inspection in progress
    /// - Condition assessment underway
    /// - Refund calculation in progress
    ///
    /// Characteristics:
    /// - Items verified and inspected
    /// - Condition assessed
    /// - Refund amount confirmed
    /// - Restocking determination made
    ///
    /// Integration: Track inspection progress; determine refund; manage restocking.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Return In Progress")]
    ReturnInProgress = 50,

    /// <summary>
    /// ReturnCompleted state — Return has been completed.
    /// </summary>
    /// <remarks>
    /// When to use: When a return has been fully processed and closed.
    ///
    /// Example Scenarios:
    /// - Items inspected and processed
    /// - Refund issued
    /// - Inventory restored
    /// - Return closed
    ///
    /// Characteristics:
    /// - Refund completed
    /// - Items restocked or disposed
    /// - Return closed
    /// - Final status confirmed
    ///
    /// Integration: Complete refund; restore inventory; archive return.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Return Completed")]
    ReturnCompleted = 51,

    #endregion

    #region Task/Job Category (52-56)

    /// <summary>
    /// Assigned state — Task has been assigned to a resource.
    /// </summary>
    /// <remarks>
    /// When to use: When a task has been allocated to a person or team.
    ///
    /// Example Scenarios:
    /// - Work item assigned to team member
    /// - Project task delegated
    /// - Support ticket assigned to agent
    /// - Maintenance job scheduled
    ///
    /// Characteristics:
    /// - Resource identified
    /// - Due date established
    /// - Task visible to assignee
    /// - Notification sent
    /// - Awaiting start
    ///
    /// Integration: Send assignment notifications; track ownership; monitor SLA.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Assigned")]
    Assigned = 52,

    /// <summary>
    /// InProgress state — Task is currently being worked on.
    /// </summary>
    /// <remarks>
    /// When to use: When work has begun on an assigned task.
    ///
    /// Example Scenarios:
    /// - Developer coding feature
    /// - Support agent helping customer
    /// - Maintenance technician working on issue
    /// - Team actively executing deliverable
    ///
    /// Characteristics:
    /// - Work commenced
    /// - Progress updates available
    /// - Completion estimate tracked
    /// - Dependencies managed
    ///
    /// Integration: Track progress; manage blockers; estimate completion.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "In Progress")]
    InProgress = 53,

    /// <summary>
    /// OnHold state — Task is on hold pending action.
    /// </summary>
    /// <remarks>
    /// When to use: When a task is paused awaiting external action.
    ///
    /// Example Scenarios:
    /// - Awaiting customer response
    /// - Blocked by dependency
    /// - Waiting for approval
    /// - Third-party coordination pending
    ///
    /// Characteristics:
    /// - Work suspended temporarily
    /// - Hold reason documented
    /// - Expected resumption date
    /// - Not contributing to timeline
    ///
    /// Integration: Track hold duration; manage dependencies; send reminders.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "On Hold")]
    OnHold = 54,

    /// <summary>
    /// Completed state — Task has been completed.
    /// </summary>
    /// <remarks>
    /// When to use: When a task has been successfully finished.
    ///
    /// Example Scenarios:
    /// - Feature development finished
    /// - Support ticket resolved
    /// - Maintenance completed
    /// - Deliverable submitted
    ///
    /// Characteristics:
    /// - All work finished
    /// - Quality verified
    /// - Deliverables submitted
    /// - Metrics recorded
    /// - Ready for closure
    ///
    /// Integration: Update metrics; archive task; validate completion.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Task Completed")]
    TaskCompleted = 55,

    /// <summary>
    /// Failed state — Task has failed.
    /// </summary>
    /// <remarks>
    /// When to use: When a task cannot be completed as specified.
    ///
    /// Example Scenarios:
    /// - Technical issue prevents completion
    /// - Resource unavailable
    /// - Requirements cannot be met
    /// - Quality standards not achievable
    ///
    /// Characteristics:
    /// - Task incomplete
    /// - Failure reason documented
    /// - Root cause analysis needed
    /// - Potential rework required
    /// - Escalation may be needed
    ///
    /// Integration: Document failure; escalate; plan rework; notify stakeholders.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Failed")]
    Failed = 56,

    #endregion

    #region Review/Rating Category (57-60)

    /// <summary>
    /// PendingReview state — Review is awaiting approval.
    /// </summary>
    /// <remarks>
    /// When to use: When a review or rating is submitted but awaiting moderation.
    ///
    /// Example Scenarios:
    /// - Customer review submitted pending approval
    /// - Product rating awaiting moderation
    /// - User feedback under review
    /// - Content compliance check pending
    ///
    /// Characteristics:
    /// - Submitted and queued
    /// - Not yet published
    /// - Awaiting moderator review
    /// - Potential rejection/editing
    ///
    /// Integration: Queue for review; provide moderation interface; publish on approval.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Pending Review")]
    PendingReview = 57,

    /// <summary>
    /// UnderReview state — Review is currently under review.
    /// </summary>
    /// <remarks>
    /// When to use: When a review is actively being evaluated by moderators.
    ///
    /// Example Scenarios:
    /// - Moderator actively reviewing content
    /// - Compliance check in progress
    /// - Authenticity verification underway
    /// - Quality assessment in process
    ///
    /// Characteristics:
    /// - Active moderation in progress
    /// - May be flagged for concerns
    /// - Temporary processing state
    /// - Expected decision pending
    ///
    /// Integration: Track review progress; provide moderation tools; escalate if needed.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Under Review")]
    UnderReview = 58,

    /// <summary>
    /// ReviewApproved state — Review has been approved.
    /// </summary>
    /// <remarks>
    /// When to use: When a review has passed moderation and is approved.
    ///
    /// Example Scenarios:
    /// - Review approved and published
    /// - Rating goes live
    /// - Feedback publicly visible
    /// - Contributing to aggregate scores
    ///
    /// Characteristics:
    /// - Published and visible
    /// - Contributing to metrics
    /// - Counted in ratings
    /// - Archived for history
    ///
    /// Integration: Publish review; update ratings; trigger notifications.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Review Approved")]
    ReviewApproved = 59,

    /// <summary>
    /// ReviewRejected state — Review has been rejected.
    /// </summary>
    /// <remarks>
    /// When to use: When a review fails moderation and is rejected.
    ///
    /// Example Scenarios:
    /// - Review flagged for inappropriate content
    /// - Does not meet community guidelines
    /// - Rejected as spam or fake
    /// - Fails authenticity check
    ///
    /// Characteristics:
    /// - Not published
    /// - Rejection reason may be provided
    /// - User may resubmit with corrections
    /// - Not contributing to ratings
    ///
    /// Integration: Notify user; provide rejection reason; offer revision option.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Review Rejected")]
    ReviewRejected = 60,

    #endregion

    #region Shipping Status Category (61-63)

    /// <summary>
    /// InTransit state — Order is in transit to destination.
    /// </summary>
    /// <remarks>
    /// When to use: When an order has been picked up by carrier and is in transit.
    ///
    /// Example Scenarios:
    /// - Package on truck for delivery
    /// - In-flight cargo
    /// - Being transported by logistics network
    /// - En route to destination
    ///
    /// Characteristics:
    /// - Actively moving
    /// - Tracking updates available
    /// - Estimated delivery date known
    /// - Cannot be intercepted easily
    /// - Nearing delivery
    ///
    /// Integration: Track via carrier; provide updates; manage delivery expectations.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "In Transit")]
    InTransit = 61,

    /// <summary>
    /// OutForDelivery state — Order is out for delivery.
    /// </summary>
    /// <remarks>
    /// When to use: When an order is with delivery driver making final deliveries.
    ///
    /// Example Scenarios:
    /// - Order on delivery truck
    /// - Final leg of delivery
    /// - Expected delivery today
    /// - Driver en route to address
    ///
    /// Characteristics:
    /// - Imminent delivery
    /// - Tracking updates frequent
    /// - Customer may receive notification
    /// - Delivery within hours
    /// - May request delivery window
    ///
    /// Integration: Send delivery notifications; provide driver tracking; enable delivery options.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Out For Delivery")]
    OutForDelivery = 62,

    /// <summary>
    /// Delivered state — Order has been delivered.
    /// </summary>
    /// <remarks>
    /// When to use: When an order has been successfully delivered to recipient.
    ///
    /// Example Scenarios:
    /// - Package placed at address
    /// - Signature obtained
    /// - Recipient confirmed
    /// - Delivery completed
    ///
    /// Characteristics:
    /// - Delivery confirmed
    /// - Signature/photo proof available
    /// - Return period may commence
    /// - Fulfillment complete
    /// - Customer satisfaction phase begins
    ///
    /// Integration: Confirm delivery; start return window; request feedback; archive order.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Display(Name = "Delivered (Shipping)")]
    DeliveredShipping = 63

    #endregion
}
