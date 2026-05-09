# GraphQL Integration Guide

**Version:** 1.0  
**Framework:** Hot Chocolate 15.1.15  
**Language:** C# / .NET 9.0  
**Last Updated:** April 27, 2026

---

## Table of Contents

1. [Schema Overview](#schema-overview)
2. [Query Examples](#query-examples)
3. [DataLoader Strategy](#dataloader-strategy)
4. [Authentication & Authorization](#authentication--authorization)
5. [Error Handling](#error-handling)
6. [Rate Limiting](#rate-limiting)
7. [Complete Code Examples](#complete-code-examples)
8. [Architecture Diagrams](#architecture-diagrams)

---

## Schema Overview

The SmartWorkz GraphQL schema defines five core types representing the primary domain entities:

### Type Definitions

#### User Type
```graphql
type User {
  """Unique identifier for the user"""
  id: String!
  """User's email address"""
  email: String!
  """User's full name"""
  name: String!
  """Account creation timestamp"""
  createdAt: DateTime!
  """Last account update timestamp"""
  updatedAt: DateTime!
}
```

**Fields:**
- `id` (String!): Globally unique user identifier
- `email` (String!): User email, unique per system
- `name` (String!): User's display name
- `createdAt` (DateTime!): Account creation date
- `updatedAt` (DateTime!): Last modification date

#### Product Type
```graphql
type Product {
  """Unique product identifier"""
  id: String!
  """Product name"""
  name: String!
  """Unit price in USD"""
  price: Decimal!
  """Detailed product description"""
  description: String!
  """Stock Keeping Unit code"""
  sku: String!
  """Product creation timestamp"""
  createdAt: DateTime!
  """Last product update timestamp"""
  updatedAt: DateTime!
  """Availability flag"""
  isActive: Boolean!
}
```

**Fields:**
- `id` (String!): Unique product SKU reference
- `name` (String!): Human-readable product name
- `price` (Decimal!): Current list price (2-decimal precision)
- `description` (String!): Markdown-formatted product details
- `sku` (String!): Canonical SKU identifier
- `createdAt` (DateTime!): Product launch date
- `updatedAt` (DateTime!): Last inventory sync date
- `isActive` (Boolean!): Product availability status

#### Transaction Type
```graphql
type Transaction {
  """Unique transaction identifier"""
  id: String!
  """Transaction amount in USD"""
  amount: Decimal!
  """Transaction state (Completed|Pending|Failed|Refunded)"""
  status: String!
  """Associated user ID"""
  userId: String!
  """Transaction description"""
  description: String!
  """Transaction creation timestamp"""
  createdAt: DateTime!
  """Last transaction update timestamp"""
  updatedAt: DateTime!
}
```

**Fields:**
- `id` (String!): Idempotent transaction ID
- `amount` (Decimal!): Monetary value in USD
- `status` (String!): Current transaction state
- `userId` (String!): Account owner (FK to User)
- `description` (String!): Human-readable transaction memo
- `createdAt` (DateTime!): Initial transaction timestamp
- `updatedAt` (DateTime!): Status change timestamp

#### Report Type
```graphql
type Report {
  """Unique report identifier"""
  id: String!
  """Report title"""
  title: String!
  """Report content (Markdown)"""
  content: String!
  """Report generation timestamp"""
  generatedAt: DateTime!
  """User who generated this report"""
  generatedBy: String!
  """Report category (Sales|Analytics|UserActivity)"""
  type: String!
  """Report record creation timestamp"""
  createdAt: DateTime!
  """Report record update timestamp"""
  updatedAt: DateTime!
}
```

**Fields:**
- `id` (String!): Report unique reference
- `title` (String!): Report heading
- `content` (String!): Rich text report body
- `generatedAt` (DateTime!): Report execution time
- `generatedBy` (String!): Report creator (FK to User)
- `type` (String!): Report classification
- `createdAt` (DateTime!): Record creation date
- `updatedAt` (DateTime!): Record modification date

#### Relay Connection Types
```graphql
type PageInfo {
  """Has additional pages after current"""
  hasNextPage: Boolean!
  """Has pages before current"""
  hasPreviousPage: Boolean!
  """Cursor to next page"""
  endCursor: String
  """Cursor to previous page"""
  startCursor: String
}

type TransactionConnection {
  """List of transaction nodes"""
  edges: [TransactionEdge!]!
  """Pagination metadata"""
  pageInfo: PageInfo!
  """Total items matching filter"""
  totalCount: Int!
}

type TransactionEdge {
  """Item cursor for pagination"""
  cursor: String!
  """Transaction item"""
  node: Transaction!
}
```

### Query Operations Available

```graphql
type Query {
  """Get single user by ID (requires USER_READ permission)"""
  user(id: String!): User

  """Get single product by ID"""
  product(id: String!): Product

  """Get transactions with cursor-based pagination"""
  transactions(
    first: Int
    after: String
  ): TransactionConnection!

  """Get single transaction by ID"""
  transaction(id: String!): Transaction

  """Get report by ID"""
  report(id: String!): Report
}
```

---

## Query Examples

### Simple Entity Queries

#### Example 1: Get User with Selected Fields
```graphql
query GetUser {
  user(id: "user-123") {
    id
    email
    name
    createdAt
  }
}
```

**Response:**
```json
{
  "data": {
    "user": {
      "id": "user-123",
      "email": "john.doe@smartworkz.com",
      "name": "John Doe",
      "createdAt": "2025-06-15T08:30:00Z"
    }
  }
}
```

#### Example 2: Get Product Details
```graphql
query GetProduct {
  product(id: "prod-456") {
    id
    name
    price
    description
    sku
    isActive
  }
}
```

**Response:**
```json
{
  "data": {
    "product": {
      "id": "prod-456",
      "name": "Professional Analytics Suite",
      "price": 299.99,
      "description": "Enterprise-grade analytics platform for real-time insights",
      "sku": "ANALYTICS-PRO-001",
      "isActive": true
    }
  }
}
```

#### Example 3: Get Transaction
```graphql
query GetTransaction {
  transaction(id: "txn-789") {
    id
    amount
    status
    userId
    description
    createdAt
    updatedAt
  }
}
```

**Response:**
```json
{
  "data": {
    "transaction": {
      "id": "txn-789",
      "amount": 1500.00,
      "status": "Completed",
      "userId": "user-123",
      "description": "Annual Enterprise License Renewal",
      "createdAt": "2026-01-15T14:22:00Z",
      "updatedAt": "2026-01-15T14:25:30Z"
    }
  }
}
```

### Pagination with Cursor-Based Approach

#### Example 4: First Page of Transactions
```graphql
query ListTransactionsPage1 {
  transactions(first: 10) {
    edges {
      cursor
      node {
        id
        amount
        status
        createdAt
      }
    }
    pageInfo {
      hasNextPage
      endCursor
    }
    totalCount
  }
}
```

**Response:**
```json
{
  "data": {
    "transactions": {
      "edges": [
        {
          "cursor": "MjAyNi0wNC0yN1QwMzAwMDBafHR4bi0wMDEw",
          "node": {
            "id": "txn-0010",
            "amount": 250.00,
            "status": "Completed",
            "createdAt": "2026-04-27T03:00:00Z"
          }
        },
        {
          "cursor": "MjAyNi0wNC0yN1QwNjMwMDBafHR4bi0wMDEx",
          "node": {
            "id": "txn-0011",
            "amount": 500.00,
            "status": "Completed",
            "createdAt": "2026-04-27T06:30:00Z"
          }
        }
      ],
      "pageInfo": {
        "hasNextPage": true,
        "endCursor": "MjAyNi0wNC0yN1QwNjMwMDBafHR4bi0wMDEx"
      },
      "totalCount": 5432
    }
  }
}
```

#### Example 5: Subsequent Page Using Cursor
```graphql
query ListTransactionsPage2 {
  transactions(first: 10, after: "MjAyNi0wNC0yN1QwNjMwMDBafHR4bi0wMDEx") {
    edges {
      cursor
      node {
        id
        amount
        status
      }
    }
    pageInfo {
      hasNextPage
      endCursor
    }
  }
}
```

#### Example 6: Pagination with Custom Page Size
```graphql
query ListTransactionsLargePageSize {
  transactions(first: 50) {
    edges {
      node {
        id
        amount
      }
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
      startCursor
      endCursor
    }
    totalCount
  }
}
```

### Batch Queries with DataLoader

#### Example 7: Batch Load Multiple Products
```graphql
query BatchLoadProducts {
  product1: product(id: "prod-001") {
    id
    name
    price
  }
  product2: product(id: "prod-002") {
    id
    name
    price
  }
  product3: product(id: "prod-003") {
    id
    name
    price
  }
}
```

**Response:**
```json
{
  "data": {
    "product1": {
      "id": "prod-001",
      "name": "Starter Plan",
      "price": 29.99
    },
    "product2": {
      "id": "prod-002",
      "name": "Professional Plan",
      "price": 99.99
    },
    "product3": {
      "id": "prod-003",
      "name": "Enterprise Plan",
      "price": 299.99
    }
  }
}
```

#### Example 8: Nested Batch Query with Multiple Entities
```graphql
query BatchLoadUsersAndTransactions {
  user1: user(id: "user-001") {
    id
    name
    email
  }
  transactions1: transactions(first: 5) {
    edges {
      node {
        id
        amount
        userId
      }
    }
  }
  report1: report(id: "report-001") {
    id
    title
    generatedBy
  }
}
```

#### Example 9: DataLoader Query - Load User Transactions
```graphql
query UserWithTransactions {
  user(id: "user-123") {
    id
    name
    email
  }
  transactions(first: 20) {
    edges {
      node {
        id
        amount
        status
        userId
      }
    }
    pageInfo {
      hasNextPage
    }
  }
}
```

---

## DataLoader Strategy

### Problem: N+1 Queries

Without DataLoader, a naive implementation resolves each entity individually:

```
Query: Get 10 transactions
  for each transaction {
    SELECT User WHERE id = userId
  }
Result: 1 initial query + 10 follow-up queries = 11 database round trips
```

### Solution: DataLoader Batching

DataLoader collects all data-fetching requests within a GraphQL operation, deduplicates them, and batches them into a single database call per operation:

```
Query: Get 10 transactions
  Collect all unique userIds: [user-001, user-002, ..., user-010]
  SELECT User WHERE id IN (user-001, ..., user-010)
Result: 1 initial query + 1 batch query = 2 database round trips
```

### ProductDataLoader Implementation

**Purpose:** Efficiently load multiple products by their IDs in a single database query.

```csharp
using GreenDonut;
using SmartWorkz.Core.Web.GraphQL;

namespace SmartWorkz.Core.Web.DataLoaders;

/// <summary>
/// DataLoader for efficiently batching product lookups to prevent N+1 queries.
/// </summary>
[DataLoader]
public async Task<IReadOnlyDictionary<string, ProductType>> 
    GetProductsByIdAsync(
        IReadOnlyList<string> productIds, 
        CancellationToken cancellationToken)
{
    // Example: Replace with your actual data access layer
    var products = await _productRepository
        .GetByIdsAsync(productIds, cancellationToken);
    
    return products
        .ToDictionary(p => p.Id)
        .AsReadOnly();
}
```

**Usage in Resolvers:**
```csharp
[GraphQLType("TransactionType")]
public class TransactionResolver
{
    [GraphQLField]
    public async Task<ProductType?> GetProduct(
        [Parent] TransactionType transaction,
        IDataLoader<string, ProductType> productLoader,
        CancellationToken cancellationToken)
    {
        return await productLoader.LoadAsync(
            transaction.ProductId, 
            cancellationToken);
    }
}
```

### UserDataLoader Implementation

**Purpose:** Resolve user associations for transactions and reports.

```csharp
[DataLoader]
public async Task<IReadOnlyDictionary<string, UserType>> 
    GetUsersByIdAsync(
        IReadOnlyList<string> userIds, 
        CancellationToken cancellationToken)
{
    var users = await _userRepository
        .GetByIdsAsync(userIds, cancellationToken);
    
    return users
        .ToDictionary(u => u.Id)
        .AsReadOnly();
}
```

**Usage for Transaction User Lookup:**
```csharp
[GraphQLField]
public async Task<UserType?> GetUser(
    [Parent] TransactionType transaction,
    IDataLoader<string, UserType> userLoader,
    CancellationToken cancellationToken)
{
    return await userLoader.LoadAsync(
        transaction.UserId, 
        cancellationToken);
}
```

### Performance Benefits

| Scenario | Without DataLoader | With DataLoader | Improvement |
|----------|-------------------|-----------------|-------------|
| 100 transactions | 101 queries | 2 queries | 50x faster |
| 1000 products | 1001 queries | 2 queries | 500x faster |
| Mixed nested entities | 10,000+ queries | 5-10 queries | 1000x+ faster |

**Key Benefits:**
- Single database connection per batch
- Automatic deduplication of identical requests
- Reduced memory allocation
- Lower CPU usage on database server
- Improved response times (typically 50-500x)

---

## Authentication & Authorization

### JWT Token Format

All authenticated requests require a Bearer token in the `Authorization` header.

**Token Structure:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyLTEyMyIsImlhdCI6MTYxNzAwMDAwMH0.signature
```

**JWT Claims:**
```json
{
  "sub": "user-123",
  "email": "user@smartworkz.com",
  "roles": ["User", "Admin"],
  "iat": 1617000000,
  "exp": 1617086400
}
```

### Field-Level Permissions

Different fields require different permission levels:

#### Public Fields (No Auth Required)
- `Product.name`
- `Product.price`
- `Product.description`
- `Product.sku` (if product is active)

#### Authenticated Fields (USER_READ Required)
- `User.email` (requires USER_READ or self)
- `User.createdAt`
- `Transaction.*` (all fields)

#### Admin Fields (ADMIN_READ Required)
- `Report.content` (full content)
- `Report.generatedBy`
- `User.updatedAt`

### Authorization Implementation

```csharp
[ObjectType]
[Authorize] // Requires authentication on all fields
public class TransactionType
{
    [Authorize(Roles = new[] { "User", "Admin" })]
    public string Id { get; set; }
    
    [Authorize(Policy = "UserRead")]
    public string UserId { get; set; }
    
    [Authorize(Roles = new[] { "Admin" })]
    public DateTime UpdatedAt { get; set; }
}
```

### Example: Authenticated Request

```graphql
query GetUserTransactions {
  user(id: "user-123") {
    id
    email  # Requires authentication
  }
}
```

**With Authorization Header:**
```bash
curl -X POST http://localhost:5000/graphql \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGc..." \
  -d '{"query": "query { user(id: \"user-123\") { id email } }"}'
```

---

## Error Handling

### Error Response Format

All GraphQL errors follow the GraphQL specification with extensions for SmartWorkz-specific metadata.

**Standard Error Response:**
```json
{
  "errors": [
    {
      "message": "User not found",
      "extensions": {
        "code": "USER_NOT_FOUND",
        "statusCode": 404,
        "correlationId": "req-2026-04-27-a1b2c3d4",
        "timestamp": "2026-04-27T10:30:45Z"
      }
    }
  ],
  "data": null
}
```

### Error Codes and Meanings

| Code | Status | Description | Action |
|------|--------|-------------|--------|
| `USER_NOT_FOUND` | 404 | Requested user doesn't exist | Verify user ID |
| `PRODUCT_NOT_FOUND` | 404 | Product not in catalog | Check product SKU |
| `TRANSACTION_NOT_FOUND` | 404 | Transaction ID invalid | Use correct ID |
| `UNAUTHORIZED` | 401 | Missing authentication token | Provide JWT token |
| `FORBIDDEN` | 403 | Insufficient permissions | Request additional roles |
| `VALIDATION_ERROR` | 400 | Invalid input parameters | Fix query parameters |
| `RATE_LIMITED` | 429 | Exceeded rate limits | Implement backoff |
| `INTERNAL_ERROR` | 500 | Server-side error | Retry with exponential backoff |

### Correlation IDs for Tracing

Every error includes a unique correlation ID for support troubleshooting:

```json
{
  "errors": [
    {
      "message": "Database connection timeout",
      "extensions": {
        "code": "INTERNAL_ERROR",
        "correlationId": "req-2026-04-27-xyz789",
        "traceId": "0HN1GK2VS5JDM:00000001"
      }
    }
  ]
}
```

**To debug:**
1. Note the correlation ID
2. Check server logs: `grep "req-2026-04-27-xyz789" /var/log/smartworkz/graphql.log`
3. Include correlation ID in support tickets

### Example: Handling Field-Level Errors

```json
{
  "errors": [
    {
      "message": "Cannot access field email: Insufficient permissions",
      "locations": [{"line": 3, "column": 5}],
      "path": ["user", "email"],
      "extensions": {
        "code": "FORBIDDEN",
        "requiredRole": "USER_READ",
        "correlationId": "req-2026-04-27-forbidden-001"
      }
    }
  ],
  "data": {
    "user": {
      "id": "user-123",
      "email": null
    }
  }
}
```

---

## Rate Limiting

### Rate Limit Policy

- **Default Limit:** 100 requests per minute per IP address
- **Authenticated Users:** 500 requests per minute
- **Admin Users:** 2000 requests per minute
- **Window:** Rolling 60-second window

### Rate Limit Headers

Every response includes rate limit metadata:

```http
HTTP/1.1 200 OK
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 87
X-RateLimit-Reset: 1619462400
```

**Header Meanings:**
- `X-RateLimit-Limit`: Maximum requests per window (100)
- `X-RateLimit-Remaining`: Requests remaining (87)
- `X-RateLimit-Reset`: Unix timestamp when limit resets

### Rate Limited Response (429)

```json
{
  "errors": [
    {
      "message": "Rate limit exceeded",
      "extensions": {
        "code": "RATE_LIMITED",
        "statusCode": 429,
        "retryAfter": 45,
        "resetAt": "2026-04-27T10:31:45Z"
      }
    }
  ],
  "data": null
}
```

### Backoff Strategy

When rate limited, implement exponential backoff:

```javascript
const MAX_RETRIES = 5;
let retryCount = 0;

async function queryWithRetry(query) {
  try {
    const response = await fetch('/graphql', {
      method: 'POST',
      body: JSON.stringify({ query })
    });
    
    if (response.status === 429) {
      const retryAfter = response.headers.get('Retry-After');
      const backoffMs = Math.pow(2, retryCount) * 1000; // Exponential backoff
      
      if (retryCount < MAX_RETRIES) {
        retryCount++;
        await sleep(Math.min(backoffMs, 60000)); // Cap at 60s
        return queryWithRetry(query);
      }
    }
    
    return response.json();
  } catch (error) {
    console.error('Query failed:', error);
  }
}
```

---

## Complete Code Examples

### 1. Simple Product Query
```graphql
query GetProductInfo {
  product(id: "prod-001") {
    id
    name
    price
    sku
  }
}
```

### 2. User Query with Full Details
```graphql
query GetUserDetails {
  user(id: "user-001") {
    id
    email
    name
    createdAt
    updatedAt
  }
}
```

### 3. Transaction List with Pagination
```graphql
query GetTransactionHistory {
  transactions(first: 20) {
    edges {
      cursor
      node {
        id
        amount
        status
        description
        createdAt
      }
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
      endCursor
      startCursor
    }
    totalCount
  }
}
```

### 4. Report Query
```graphql
query GetReportDetails {
  report(id: "report-001") {
    id
    title
    type
    generatedAt
    generatedBy
    createdAt
  }
}
```

### 5. Multi-Entity Query
```graphql
query GetDashboardData {
  user(id: "user-001") {
    name
    email
  }
  transactions(first: 10) {
    totalCount
    edges {
      node {
        id
        amount
        status
      }
    }
  }
}
```

### 6. Pagination with Cursor
```graphql
query GetNextPage($cursor: String) {
  transactions(first: 10, after: $cursor) {
    edges {
      cursor
      node {
        id
        amount
      }
    }
    pageInfo {
      hasNextPage
      endCursor
    }
  }
}
```

### 7. Batch Product Query
```graphql
query GetProductCatalog {
  p1: product(id: "prod-001") { name price }
  p2: product(id: "prod-002") { name price }
  p3: product(id: "prod-003") { name price }
  p4: product(id: "prod-004") { name price }
  p5: product(id: "prod-005") { name price }
}
```

### 8. DataLoader Query - Transactions
```graphql
query GetTransactionsOptimized {
  transactions(first: 50) {
    edges {
      node {
        id
        amount
        userId
      }
    }
    totalCount
  }
}
```

### 9. Error Handling Query
```graphql
query TestErrorHandling {
  user(id: "invalid-id") {
    id
    email
  }
}
```

**Expected Error Response:**
```json
{
  "errors": [{
    "message": "User not found",
    "extensions": {
      "code": "USER_NOT_FOUND",
      "correlationId": "req-001"
    }
  }]
}
```

### 10. Authenticated Query
```graphql
query GetSecureUserData {
  user(id: "user-123") {
    id
    email
    createdAt
  }
}
```

**Request with Token:**
```bash
curl -X POST http://localhost:5000/graphql \
  -H "Authorization: Bearer TOKEN_HERE" \
  -d '{"query":"query { user(id: \"user-123\") { id email } }"}'
```

### 11. Rate Limit Response Query
```graphql
query MultipleQueries {
  q1: user(id: "user-001") { id }
  q2: product(id: "prod-001") { id }
  q3: transaction(id: "txn-001") { id }
  q4: report(id: "report-001") { id }
  q5: transactions(first: 10) { totalCount }
}
```

**Response with Rate Limit Headers:**
```json
{
  "data": { ... },
  "extensions": {
    "rateLimit": {
      "limit": 100,
      "remaining": 95,
      "resetAt": "2026-04-27T10:30:00Z"
    }
  }
}
```

### 12. Complex Nested Query
```graphql
query GetComprehensiveData {
  user(id: "user-001") {
    id
    name
    email
  }
  userTransactions: transactions(first: 5) {
    edges {
      node {
        id
        amount
        status
      }
    }
  }
  product(id: "prod-001") {
    name
    price
  }
}
```

### 13. Pagination Backward Navigation
```graphql
query GetPreviousPage {
  transactions(first: 10, after: "cursor-from-pageinfo") {
    edges {
      cursor
      node {
        id
        amount
      }
    }
    pageInfo {
      hasPreviousPage
      startCursor
    }
  }
}
```

### 14. Field Selection Minimization
```graphql
query MinimalQuery {
  user(id: "user-001") {
    id
  }
  product(id: "prod-001") {
    id
  }
}
```

### 15. Full Entity Query with All Available Fields
```graphql
query FullProductDetails {
  product(id: "prod-001") {
    id
    name
    price
    description
    sku
    createdAt
    updatedAt
    isActive
  }
}
```

---

## Architecture Diagrams

### GraphQL Schema Relationships

```
┌─────────────────────────────────────────────────────────┐
│                    GraphQL Query Root                    │
└─────────────────────────────────────────────────────────┘
              │                  │                 │
              ▼                  ▼                 ▼
         ┌────────┐         ┌─────────┐      ┌──────────┐
         │  User  │         │ Product │      │Transaction
         └────────┘         └─────────┘      └──────────┘
         id (PK)            id (PK)          id (PK)
         email              name             amount
         name               price            status
         createdAt          description      userId (FK→User)
         updatedAt          sku              description
                            createdAt        createdAt
                            updatedAt        updatedAt
                            isActive
              │
              │ ┌───────────────────────┐
              │ │ 1:Many Relationship   │
              │ │ User → Transaction    │
              └─┴───────────────────────┘

         ┌──────────┐
         │  Report  │
         └──────────┘
         id (PK)
         title
         content
         generatedAt
         generatedBy (FK→User)
         type
         createdAt
         updatedAt
```

### Authentication Flow Diagram

```
Client Request
     │
     ▼
┌──────────────────────────┐
│ Extract JWT from Header  │
│ "Authorization: Bearer"  │
└──────────────────────────┘
     │
     ▼
┌──────────────────────────┐
│  Validate Token Signature│
│  Check Expiration        │
└──────────────────────────┘
     │
     ├─────[Valid]─────────────┐
     │                         │
     ▼                         ▼
┌─────────────┐        ┌──────────────┐
│ Extract JWT │        │ Return 401   │
│   Claims    │        │ Unauthorized │
└─────────────┘        └──────────────┘
     │
     ▼
┌──────────────────────┐
│ Check Field-Level    │
│ Permissions/Roles    │
└──────────────────────┘
     │
     ├─────[Allowed]────────┐
     │                      │
     ▼                      ▼
┌─────────────┐      ┌────────────┐
│ Execute     │      │ Return 403 │
│ Resolver    │      │ Forbidden  │
└─────────────┘      └────────────┘
```

### DataLoader Batching Flow

```
Request Processing Timeline
────────────────────────────

Time 0ms: Start Request Processing
     │
     ├─ Resolver 1: GetUser(id: "user-001")
     │  └─ Add to DataLoader batch
     │
     ├─ Resolver 2: GetUser(id: "user-002")
     │  └─ Add to DataLoader batch
     │
     ├─ Resolver 3: GetProduct(id: "prod-001")
     │  └─ Add to ProductLoader batch
     │
     └─ Continue resolving...

Time 20ms: All Resolvers Queued
     │
     ├─ UserDataLoader: ["user-001", "user-002"]
     │  └─ Execute 1 query: SELECT * FROM Users WHERE id IN (...)
     │
     └─ ProductDataLoader: ["prod-001"]
        └─ Execute 1 query: SELECT * FROM Products WHERE id IN (...)

Time 35ms: Return Batched Results
     │
     └─ All data available to complete response

Performance Comparison:
─────────────────────
Without Batching: 1 + N resolver queries = ~50ms
With Batching:    1 + M batch queries = ~35ms (where M << N)
Improvement:      ~43% faster
```

### Error Handling Flow

```
GraphQL Query Execution
     │
     ▼
┌──────────────────┐
│ Parse & Validate │
└──────────────────┘
     │
     ├─[Error]──────────────────┐
     │                          │
     ▼                          ▼
Execute Resolvers        Return Validation Error
     │                   {errors: [...]}
     ├─[Resolver Error]─────┐
     │                      ▼
     ▼              Catch & Format Error
Return Data        Add correlationId
     │              Add statusCode
     │              Add error code
     │
     └──────────────────┬─────────────────┘
                        ▼
                 Return Error Response
                 {errors: [{
                   message: "...",
                   extensions: {
                     code: "...",
                     correlationId: "..."
                   }
                 }]}
```

### Rate Limiting Flow

```
Incoming Request
     │
     ▼
┌──────────────────────────┐
│ Extract Client IP/User ID│
└──────────────────────────┘
     │
     ▼
┌──────────────────────────┐
│ Check Rate Limit Counter │
│ (Redis/In-Memory Cache)  │
└──────────────────────────┘
     │
     ├─[Under Limit]─────────────┐
     │                           │
     ▼                           ▼
Increment Counter         Return 429
Execute Query            Rate-Limited
     │
     ▼
Add Rate Limit Headers
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 87
X-RateLimit-Reset: 1619462400
     │
     ▼
Return Response
```

---

## Best Practices

### Query Optimization
1. **Request only needed fields** - Reduces response payload
2. **Use pagination** - Never request unlimited data
3. **Batch related queries** - Reduces round trips
4. **Implement caching** - Cache frequently accessed data on client

### Error Handling
1. **Always check for errors** - Don't assume success
2. **Use correlation IDs** - Help with debugging
3. **Implement backoff** - Retry transient failures exponentially
4. **Log errors** - Track patterns for improvements

### Rate Limiting
1. **Monitor X-RateLimit headers** - Implement client-side limiting
2. **Implement jitter** - Prevent thundering herd on retry
3. **Use authenticated tokens** - Higher limits for authenticated users
4. **Cache responses** - Reduce query volume

### Security
1. **Always authenticate** - Use JWT tokens
2. **Validate permissions** - Check before accessing sensitive fields
3. **Sanitize input** - Prevent injection attacks
4. **Use HTTPS only** - Protect token transmission
5. **Set short expiration** - Rotate tokens regularly

---

## Support & Documentation

- **Schema Documentation:** `/graphql` endpoint in development
- **API Status:** [api-status.smartworkz.com](https://api-status.smartworkz.com)
- **Support:** support@smartworkz.com
- **Issue Tracking:** Use correlation IDs in bug reports

---

**Document Version:** 1.0  
**Last Updated:** April 27, 2026  
**Maintained By:** SmartWorkz Platform Team
