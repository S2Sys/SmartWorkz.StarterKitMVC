# Schema API Reference

## Classes & Interfaces

### ProductType

- **Namespace:** `SmartWorkz.Core.Web.GraphQL.Schema.ProductType`
- **Summary:** GraphQL type for Product entity.
            Exposes product information for catalog queries.

### Query

- **Namespace:** `SmartWorkz.Core.Web.GraphQL.Schema.Query`
- **Summary:** Root Query type for GraphQL API.
            Provides access to Users, Transactions, Products, and Reports with cursor-based pagination.

#### Methods & Properties

- **GetUsers** - Query a list of users with cursor-based pagination.
- **GetUser** - Query a specific user by ID.
- **GetTransactions** - Query a list of transactions with cursor-based pagination.
- **GetTransaction** - Query a specific transaction by ID.
- **GetProducts** - Query a list of products with cursor-based pagination.
- **GetProduct** - Query a specific product by ID.
- **GetReports** - Query a list of reports with cursor-based pagination.
- **GetReport** - Query a specific report by ID.

### ReportType

- **Namespace:** `SmartWorkz.Core.Web.GraphQL.Schema.ReportType`
- **Summary:** GraphQL type for Report entity.
            Exposes report information for analytics and reporting.

### TransactionType

- **Namespace:** `SmartWorkz.Core.Web.GraphQL.Schema.TransactionType`
- **Summary:** GraphQL type for Transaction entity.
            Exposes transaction information for financial reporting.

### UserType

- **Namespace:** `SmartWorkz.Core.Web.GraphQL.Schema.UserType`
- **Summary:** GraphQL type for User entity.
            Exposes user information with appropriate field visibility.

