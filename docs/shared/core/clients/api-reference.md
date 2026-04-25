# Clients API Reference

## Classes & Interfaces

### IAuthenticationClient

- **Namespace:** `SmartWorkz.Clients.IAuthenticationClient`
- **Summary:** Client for the Authentication API endpoint.
            
             Endpoints:
             • POST /api/authentication/login - User login
             • POST /api/authentication/register - User registration
             • POST /api/authentication/refresh - Refresh JWT token
             • POST /api/authentication/logout - User logout
             • POST /api/authentication/change-password - Change user password
             Usage Examples:
             
             // Login and get JWT token
             var response = await client.Authentication.LoginAsync(
                 new LoginRequest("user@example.com", "password123"));
             var token = response.Token;
            
             // Register new user
             var registerResponse = await client.Authentication.RegisterAsync(
                 new RegisterRequest("newuser@example.com", "John", "Doe", "password123"));
            
             // Refresh token
             var refreshResponse = await client.Authentication.RefreshTokenAsync(
                 new RefreshTokenRequest(currentRefreshToken));

#### Methods & Properties

- **LoginAsync** - Authenticates a user with email and password, returning a JWT token.
  - Parameters:
    - `request`: Login credentials.
    - `cancellationToken`: Cancellation token.
  - Returns: Authentication response with JWT token.
- **RegisterAsync** - Registers a new user account.
  - Parameters:
    - `request`: Registration details.
    - `cancellationToken`: Cancellation token.
  - Returns: Authentication response.
- **RefreshTokenAsync** - Refreshes an expired JWT token using a refresh token.
  - Parameters:
    - `request`: Refresh token request.
    - `cancellationToken`: Cancellation token.
  - Returns: New authentication response with updated token.
- **ChangePasswordAsync** - Changes the current user's password.
  - Parameters:
    - `request`: Password change request.
    - `cancellationToken`: Cancellation token.
  - Returns: Response indicating success or failure.
- **LogoutAsync** - Logs out the current user (invalidates token on server).
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: Response indicating logout status.

### IProductsClient

- **Namespace:** `SmartWorkz.Clients.IProductsClient`
- **Summary:** Client for the Products API endpoint.
            
             Endpoints:
             • GET /api/products - List products (paginated)
             • GET /api/products/{id} - Get product by ID
             • POST /api/products - Create new product
             • PUT /api/products/{id} - Update product
             • DELETE /api/products/{id} - Delete product
             Usage Examples:
             
             // List products with price filtering
             var listRequest = new ListProductsRequest(
                 PageNumber: 1,
                 PageSize: 20,
                 SearchTerm: "laptop",
                 MinPrice: 500m,
                 MaxPrice: 2000m);
             var products = await client.Products.ListAsync(listRequest);
             Console.WriteLine($"Found {products.TotalCount} products");
            
             // Get specific product
             var product = await client.Products.GetAsync("prod-789");
             Console.WriteLine($"{product.Name} - ${product.Price}");
             Console.WriteLine($"Stock: {product.StockQuantity} units");
            
             // Create product
             var createRequest = new CreateProductRequest(
                 Name: "USB-C Cable",
                 Description: "High-speed USB-C charging cable",
                 Price: 29.99m,
                 StockQuantity: 100);
             var created = await client.Products.CreateAsync(createRequest);

#### Methods & Properties

- **ListAsync** - Lists all products with pagination and optional filtering.
  - Parameters:
    - `request`: List query options.
    - `cancellationToken`: Cancellation token.
  - Returns: Paginated product list.
- **GetAsync** - Gets a product by ID.
  - Parameters:
    - `productId`: The product ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Product information.
- **CreateAsync** - Creates a new product.
  - Parameters:
    - `request`: Product creation details.
    - `cancellationToken`: Cancellation token.
  - Returns: Created product information.
- **UpdateAsync** - Updates an existing product.
  - Parameters:
    - `productId`: The product ID to update.
    - `request`: Product update details.
    - `cancellationToken`: Cancellation token.
  - Returns: Updated product information.
- **DeleteAsync** - Deletes a product.
  - Parameters:
    - `productId`: The product ID to delete.
    - `cancellationToken`: Cancellation token.
  - Returns: Deletion response.

### ProductsClient

- **Namespace:** `SmartWorkz.Clients.ProductsClient`
- **Summary:** Default implementation of .

### IReportsClient

- **Namespace:** `SmartWorkz.Clients.IReportsClient`
- **Summary:** Client for the Reports API endpoint.
            
             Endpoints:
             • GET /api/reports - List generated reports (paginated)
             • GET /api/reports/{id} - Get report by ID
             • POST /api/reports - Generate new report
             • DELETE /api/reports/{id} - Delete report
             Usage Examples:
             
             // List existing reports
             var listRequest = new ListReportsRequest(PageNumber: 1, PageSize: 10, Type: "sales");
             var reports = await client.Reports.ListAsync(listRequest);
             Console.WriteLine($"Found {reports.TotalCount} reports");
            
             // Generate new report
             var generateRequest = new GenerateReportRequest(
                 Title: "Monthly Sales Report",
                 Type: "sales",
                 Format: "pdf",
                 Filters: new Dictionary<string, object>
                 {
                     { "month", 4 },
                     { "year", 2026 }
                 });
             var generated = await client.Reports.GenerateAsync(generateRequest);
             Console.WriteLine($"Report ID: {generated.Id}");
             Console.WriteLine($"Download: {generated.DataUrl}");
            
             // Get report details
             var report = await client.Reports.GetAsync(generated.Id);
             Console.WriteLine($"Records: {report.RecordCount}");

#### Methods & Properties

- **ListAsync** - Lists all generated reports with pagination and optional filtering.
  - Parameters:
    - `request`: List query options.
    - `cancellationToken`: Cancellation token.
  - Returns: Paginated report list.
- **GetAsync** - Gets a report by ID.
  - Parameters:
    - `reportId`: The report ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Report information.
- **GenerateAsync** - Generates a new report asynchronously.
  - Parameters:
    - `request`: Report generation details.
    - `cancellationToken`: Cancellation token.
  - Returns: Generated report information.
- **DeleteAsync** - Deletes a report.
  - Parameters:
    - `reportId`: The report ID to delete.
    - `cancellationToken`: Cancellation token.
  - Returns: Deletion response.

### ReportsClient

- **Namespace:** `SmartWorkz.Clients.ReportsClient`
- **Summary:** Default implementation of .

### ITransactionsClient

- **Namespace:** `SmartWorkz.Clients.ITransactionsClient`
- **Summary:** Client for the Transactions API endpoint.
            
             Endpoints:
             • GET /api/transactions - List transactions (paginated)
             • GET /api/transactions/{id} - Get transaction by ID
             • POST /api/transactions - Create new transaction
             • GET /api/transactions/{id}/status - Get transaction status
             Usage Examples:
             
             // List transactions with date filtering
             var listRequest = new ListTransactionsRequest(
                 PageNumber: 1,
                 PageSize: 25,
                 UserId: "user-123",
                 Status: "completed");
             var transactions = await client.Transactions.ListAsync(listRequest);
            
             // Get specific transaction
             var transaction = await client.Transactions.GetAsync("txn-456");
             Console.WriteLine($"Amount: {transaction.Amount} {transaction.Currency}");
             Console.WriteLine($"Status: {transaction.Status}");
            
             // Create transaction
             var createRequest = new CreateTransactionRequest(
                 UserId: "user-123",
                 Amount: 99.99m,
                 Currency: "USD",
                 Type: "payment",
                 Description: "Monthly subscription");
             var created = await client.Transactions.CreateAsync(createRequest);

#### Methods & Properties

- **ListAsync** - Lists all transactions with pagination and optional filtering.
  - Parameters:
    - `request`: List query options.
    - `cancellationToken`: Cancellation token.
  - Returns: Paginated transaction list.
- **GetAsync** - Gets a transaction by ID.
  - Parameters:
    - `transactionId`: The transaction ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Transaction information.
- **CreateAsync** - Creates a new transaction.
  - Parameters:
    - `request`: Transaction creation details.
    - `cancellationToken`: Cancellation token.
  - Returns: Created transaction information.
- **GetStatusAsync** - Gets the current status of a transaction.
  - Parameters:
    - `transactionId`: The transaction ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Transaction status information.

### TransactionsClient

- **Namespace:** `SmartWorkz.Clients.TransactionsClient`
- **Summary:** Default implementation of .

### IUsersClient

- **Namespace:** `SmartWorkz.Clients.IUsersClient`
- **Summary:** Client for the Users API endpoint.
            
             Endpoints:
             • GET /api/users - List all users (paginated)
             • GET /api/users/{id} - Get user by ID
             • POST /api/users - Create new user
             • PUT /api/users/{id} - Update user
             • DELETE /api/users/{id} - Delete user
             Usage Examples:
             
             // List users with pagination
             var request = new ListUsersRequest(PageNumber: 1, PageSize: 25);
             var result = await client.Users.ListAsync(request);
             Console.WriteLine($"Total users: {result.TotalCount}");
             foreach (var user in result.Items ?? new())
                 Console.WriteLine($"- {user.FirstName} {user.LastName} ({user.Email})");
            
             // Get specific user
             var user = await client.Users.GetAsync("user-123");
             Console.WriteLine($"{user.FirstName} {user.LastName}");
            
             // Create new user
             var newUserRequest = new CreateUserRequest(
                 Email: "newuser@example.com",
                 FirstName: "John",
                 LastName: "Doe");
             var created = await client.Users.CreateAsync(newUserRequest);
             Console.WriteLine($"Created user: {created.Id}");
            
             // Update user
             var updateRequest = new UpdateUserRequest(FirstName: "Jane");
             var updated = await client.Users.UpdateAsync("user-123", updateRequest);
            
             // Delete user
             await client.Users.DeleteAsync("user-123");

#### Methods & Properties

- **ListAsync** - Lists all users with pagination and optional filtering.
  - Parameters:
    - `request`: List query options.
    - `cancellationToken`: Cancellation token.
  - Returns: Paginated user list.
- **GetAsync** - Gets a user by ID.
  - Parameters:
    - `userId`: The user ID.
    - `cancellationToken`: Cancellation token.
  - Returns: User information.
- **CreateAsync** - Creates a new user.
  - Parameters:
    - `request`: User creation details.
    - `cancellationToken`: Cancellation token.
  - Returns: Created user information.
- **UpdateAsync** - Updates an existing user.
  - Parameters:
    - `userId`: The user ID to update.
    - `request`: User update details.
    - `cancellationToken`: Cancellation token.
  - Returns: Updated user information.
- **DeleteAsync** - Deletes a user.
  - Parameters:
    - `userId`: The user ID to delete.
    - `cancellationToken`: Cancellation token.
  - Returns: Deletion result.

### UsersClient

- **Namespace:** `SmartWorkz.Clients.UsersClient`
- **Summary:** Default implementation of .

### IWebhooksClient

- **Namespace:** `SmartWorkz.Clients.IWebhooksClient`
- **Summary:** Client for the Webhooks API endpoint.
            
             Endpoints:
             • GET /api/webhooks - List registered webhooks (paginated)
             • GET /api/webhooks/{id} - Get webhook by ID
             • POST /api/webhooks - Register new webhook
             • PUT /api/webhooks/{id} - Update webhook
             • DELETE /api/webhooks/{id} - Delete webhook
             • POST /api/webhooks/{id}/test - Test webhook delivery
             Usage Examples:
             
             // List registered webhooks
             var webhooks = await client.Webhooks.ListAsync();
             foreach (var hook in webhooks.Items ?? new())
                 Console.WriteLine($"- {hook.Url} (Events: {string.Join(", ", hook.Events)})");
            
             // Register new webhook
             var registerRequest = new CreateWebhookRequest(
                 Url: "https://myapp.com/webhooks/smartworkz",
                 Events: new List<string> { "user.created", "transaction.completed" },
                 IsActive: true,
                 RetryAttempts: 3,
                 TimeoutSeconds: 10);
             var created = await client.Webhooks.CreateAsync(registerRequest);
             Console.WriteLine($"Webhook registered: {created.Id}");
            
             // Test webhook delivery
             var testRequest = new TestWebhookRequest(
                 WebhookId: created.Id,
                 EventType: "user.created");
             await client.Webhooks.TestAsync(testRequest);
             Console.WriteLine("Test event sent");
            
             // Update webhook
             var updateRequest = new UpdateWebhookRequest(
                 Events: new List<string> { "user.created", "user.deleted" });
             await client.Webhooks.UpdateAsync(created.Id, updateRequest);

#### Methods & Properties

- **ListAsync** - Lists all registered webhooks with pagination.
  - Parameters:
    - `request`: List query options.
    - `cancellationToken`: Cancellation token.
  - Returns: Paginated webhook list.
- **GetAsync** - Gets a webhook by ID.
  - Parameters:
    - `webhookId`: The webhook ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Webhook information.
- **CreateAsync** - Registers a new webhook.
  - Parameters:
    - `request`: Webhook registration details.
    - `cancellationToken`: Cancellation token.
  - Returns: Registered webhook information.
- **UpdateAsync** - Updates an existing webhook.
  - Parameters:
    - `webhookId`: The webhook ID to update.
    - `request`: Webhook update details.
    - `cancellationToken`: Cancellation token.
  - Returns: Updated webhook information.
- **DeleteAsync** - Deletes a webhook.
  - Parameters:
    - `webhookId`: The webhook ID to delete.
    - `cancellationToken`: Cancellation token.
  - Returns: Deletion response.
- **TestAsync** - Sends a test event to a webhook to verify connectivity.
  - Parameters:
    - `request`: Test webhook request.
    - `cancellationToken`: Cancellation token.
  - Returns: Test result response.

### IAuthenticationClient

- **Namespace:** `SmartWorkz.Clients.IAuthenticationClient`
- **Summary:** Client for the Authentication API endpoint.
            
             Endpoints:
             • POST /api/authentication/login - User login
             • POST /api/authentication/register - User registration
             • POST /api/authentication/refresh - Refresh JWT token
             • POST /api/authentication/logout - User logout
             • POST /api/authentication/change-password - Change user password
             Usage Examples:
             
             // Login and get JWT token
             var response = await client.Authentication.LoginAsync(
                 new LoginRequest("user@example.com", "password123"));
             var token = response.Token;
            
             // Register new user
             var registerResponse = await client.Authentication.RegisterAsync(
                 new RegisterRequest("newuser@example.com", "John", "Doe", "password123"));
            
             // Refresh token
             var refreshResponse = await client.Authentication.RefreshTokenAsync(
                 new RefreshTokenRequest(currentRefreshToken));

#### Methods & Properties

- **LoginAsync** - Authenticates a user with email and password, returning a JWT token.
  - Parameters:
    - `request`: Login credentials.
    - `cancellationToken`: Cancellation token.
  - Returns: Authentication response with JWT token.
- **RegisterAsync** - Registers a new user account.
  - Parameters:
    - `request`: Registration details.
    - `cancellationToken`: Cancellation token.
  - Returns: Authentication response.
- **RefreshTokenAsync** - Refreshes an expired JWT token using a refresh token.
  - Parameters:
    - `request`: Refresh token request.
    - `cancellationToken`: Cancellation token.
  - Returns: New authentication response with updated token.
- **ChangePasswordAsync** - Changes the current user's password.
  - Parameters:
    - `request`: Password change request.
    - `cancellationToken`: Cancellation token.
  - Returns: Response indicating success or failure.
- **LogoutAsync** - Logs out the current user (invalidates token on server).
  - Parameters:
    - `cancellationToken`: Cancellation token.
  - Returns: Response indicating logout status.

### IProductsClient

- **Namespace:** `SmartWorkz.Clients.IProductsClient`
- **Summary:** Client for the Products API endpoint.
            
             Endpoints:
             • GET /api/products - List products (paginated)
             • GET /api/products/{id} - Get product by ID
             • POST /api/products - Create new product
             • PUT /api/products/{id} - Update product
             • DELETE /api/products/{id} - Delete product
             Usage Examples:
             
             // List products with price filtering
             var listRequest = new ListProductsRequest(
                 PageNumber: 1,
                 PageSize: 20,
                 SearchTerm: "laptop",
                 MinPrice: 500m,
                 MaxPrice: 2000m);
             var products = await client.Products.ListAsync(listRequest);
             Console.WriteLine($"Found {products.TotalCount} products");
            
             // Get specific product
             var product = await client.Products.GetAsync("prod-789");
             Console.WriteLine($"{product.Name} - ${product.Price}");
             Console.WriteLine($"Stock: {product.StockQuantity} units");
            
             // Create product
             var createRequest = new CreateProductRequest(
                 Name: "USB-C Cable",
                 Description: "High-speed USB-C charging cable",
                 Price: 29.99m,
                 StockQuantity: 100);
             var created = await client.Products.CreateAsync(createRequest);

#### Methods & Properties

- **ListAsync** - Lists all products with pagination and optional filtering.
  - Parameters:
    - `request`: List query options.
    - `cancellationToken`: Cancellation token.
  - Returns: Paginated product list.
- **GetAsync** - Gets a product by ID.
  - Parameters:
    - `productId`: The product ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Product information.
- **CreateAsync** - Creates a new product.
  - Parameters:
    - `request`: Product creation details.
    - `cancellationToken`: Cancellation token.
  - Returns: Created product information.
- **UpdateAsync** - Updates an existing product.
  - Parameters:
    - `productId`: The product ID to update.
    - `request`: Product update details.
    - `cancellationToken`: Cancellation token.
  - Returns: Updated product information.
- **DeleteAsync** - Deletes a product.
  - Parameters:
    - `productId`: The product ID to delete.
    - `cancellationToken`: Cancellation token.
  - Returns: Deletion response.

### ProductsClient

- **Namespace:** `SmartWorkz.Clients.ProductsClient`
- **Summary:** Default implementation of .

### IReportsClient

- **Namespace:** `SmartWorkz.Clients.IReportsClient`
- **Summary:** Client for the Reports API endpoint.
            
             Endpoints:
             • GET /api/reports - List generated reports (paginated)
             • GET /api/reports/{id} - Get report by ID
             • POST /api/reports - Generate new report
             • DELETE /api/reports/{id} - Delete report
             Usage Examples:
             
             // List existing reports
             var listRequest = new ListReportsRequest(PageNumber: 1, PageSize: 10, Type: "sales");
             var reports = await client.Reports.ListAsync(listRequest);
             Console.WriteLine($"Found {reports.TotalCount} reports");
            
             // Generate new report
             var generateRequest = new GenerateReportRequest(
                 Title: "Monthly Sales Report",
                 Type: "sales",
                 Format: "pdf",
                 Filters: new Dictionary<string, object>
                 {
                     { "month", 4 },
                     { "year", 2026 }
                 });
             var generated = await client.Reports.GenerateAsync(generateRequest);
             Console.WriteLine($"Report ID: {generated.Id}");
             Console.WriteLine($"Download: {generated.DataUrl}");
            
             // Get report details
             var report = await client.Reports.GetAsync(generated.Id);
             Console.WriteLine($"Records: {report.RecordCount}");

#### Methods & Properties

- **ListAsync** - Lists all generated reports with pagination and optional filtering.
  - Parameters:
    - `request`: List query options.
    - `cancellationToken`: Cancellation token.
  - Returns: Paginated report list.
- **GetAsync** - Gets a report by ID.
  - Parameters:
    - `reportId`: The report ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Report information.
- **GenerateAsync** - Generates a new report asynchronously.
  - Parameters:
    - `request`: Report generation details.
    - `cancellationToken`: Cancellation token.
  - Returns: Generated report information.
- **DeleteAsync** - Deletes a report.
  - Parameters:
    - `reportId`: The report ID to delete.
    - `cancellationToken`: Cancellation token.
  - Returns: Deletion response.

### ReportsClient

- **Namespace:** `SmartWorkz.Clients.ReportsClient`
- **Summary:** Default implementation of .

### ITransactionsClient

- **Namespace:** `SmartWorkz.Clients.ITransactionsClient`
- **Summary:** Client for the Transactions API endpoint.
            
             Endpoints:
             • GET /api/transactions - List transactions (paginated)
             • GET /api/transactions/{id} - Get transaction by ID
             • POST /api/transactions - Create new transaction
             • GET /api/transactions/{id}/status - Get transaction status
             Usage Examples:
             
             // List transactions with date filtering
             var listRequest = new ListTransactionsRequest(
                 PageNumber: 1,
                 PageSize: 25,
                 UserId: "user-123",
                 Status: "completed");
             var transactions = await client.Transactions.ListAsync(listRequest);
            
             // Get specific transaction
             var transaction = await client.Transactions.GetAsync("txn-456");
             Console.WriteLine($"Amount: {transaction.Amount} {transaction.Currency}");
             Console.WriteLine($"Status: {transaction.Status}");
            
             // Create transaction
             var createRequest = new CreateTransactionRequest(
                 UserId: "user-123",
                 Amount: 99.99m,
                 Currency: "USD",
                 Type: "payment",
                 Description: "Monthly subscription");
             var created = await client.Transactions.CreateAsync(createRequest);

#### Methods & Properties

- **ListAsync** - Lists all transactions with pagination and optional filtering.
  - Parameters:
    - `request`: List query options.
    - `cancellationToken`: Cancellation token.
  - Returns: Paginated transaction list.
- **GetAsync** - Gets a transaction by ID.
  - Parameters:
    - `transactionId`: The transaction ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Transaction information.
- **CreateAsync** - Creates a new transaction.
  - Parameters:
    - `request`: Transaction creation details.
    - `cancellationToken`: Cancellation token.
  - Returns: Created transaction information.
- **GetStatusAsync** - Gets the current status of a transaction.
  - Parameters:
    - `transactionId`: The transaction ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Transaction status information.

### TransactionsClient

- **Namespace:** `SmartWorkz.Clients.TransactionsClient`
- **Summary:** Default implementation of .

### IUsersClient

- **Namespace:** `SmartWorkz.Clients.IUsersClient`
- **Summary:** Client for the Users API endpoint.
            
             Endpoints:
             • GET /api/users - List all users (paginated)
             • GET /api/users/{id} - Get user by ID
             • POST /api/users - Create new user
             • PUT /api/users/{id} - Update user
             • DELETE /api/users/{id} - Delete user
             Usage Examples:
             
             // List users with pagination
             var request = new ListUsersRequest(PageNumber: 1, PageSize: 25);
             var result = await client.Users.ListAsync(request);
             Console.WriteLine($"Total users: {result.TotalCount}");
             foreach (var user in result.Items ?? new())
                 Console.WriteLine($"- {user.FirstName} {user.LastName} ({user.Email})");
            
             // Get specific user
             var user = await client.Users.GetAsync("user-123");
             Console.WriteLine($"{user.FirstName} {user.LastName}");
            
             // Create new user
             var newUserRequest = new CreateUserRequest(
                 Email: "newuser@example.com",
                 FirstName: "John",
                 LastName: "Doe");
             var created = await client.Users.CreateAsync(newUserRequest);
             Console.WriteLine($"Created user: {created.Id}");
            
             // Update user
             var updateRequest = new UpdateUserRequest(FirstName: "Jane");
             var updated = await client.Users.UpdateAsync("user-123", updateRequest);
            
             // Delete user
             await client.Users.DeleteAsync("user-123");

#### Methods & Properties

- **ListAsync** - Lists all users with pagination and optional filtering.
  - Parameters:
    - `request`: List query options.
    - `cancellationToken`: Cancellation token.
  - Returns: Paginated user list.
- **GetAsync** - Gets a user by ID.
  - Parameters:
    - `userId`: The user ID.
    - `cancellationToken`: Cancellation token.
  - Returns: User information.
- **CreateAsync** - Creates a new user.
  - Parameters:
    - `request`: User creation details.
    - `cancellationToken`: Cancellation token.
  - Returns: Created user information.
- **UpdateAsync** - Updates an existing user.
  - Parameters:
    - `userId`: The user ID to update.
    - `request`: User update details.
    - `cancellationToken`: Cancellation token.
  - Returns: Updated user information.
- **DeleteAsync** - Deletes a user.
  - Parameters:
    - `userId`: The user ID to delete.
    - `cancellationToken`: Cancellation token.
  - Returns: Deletion result.

### UsersClient

- **Namespace:** `SmartWorkz.Clients.UsersClient`
- **Summary:** Default implementation of .

### IWebhooksClient

- **Namespace:** `SmartWorkz.Clients.IWebhooksClient`
- **Summary:** Client for the Webhooks API endpoint.
            
             Endpoints:
             • GET /api/webhooks - List registered webhooks (paginated)
             • GET /api/webhooks/{id} - Get webhook by ID
             • POST /api/webhooks - Register new webhook
             • PUT /api/webhooks/{id} - Update webhook
             • DELETE /api/webhooks/{id} - Delete webhook
             • POST /api/webhooks/{id}/test - Test webhook delivery
             Usage Examples:
             
             // List registered webhooks
             var webhooks = await client.Webhooks.ListAsync();
             foreach (var hook in webhooks.Items ?? new())
                 Console.WriteLine($"- {hook.Url} (Events: {string.Join(", ", hook.Events)})");
            
             // Register new webhook
             var registerRequest = new CreateWebhookRequest(
                 Url: "https://myapp.com/webhooks/smartworkz",
                 Events: new List<string> { "user.created", "transaction.completed" },
                 IsActive: true,
                 RetryAttempts: 3,
                 TimeoutSeconds: 10);
             var created = await client.Webhooks.CreateAsync(registerRequest);
             Console.WriteLine($"Webhook registered: {created.Id}");
            
             // Test webhook delivery
             var testRequest = new TestWebhookRequest(
                 WebhookId: created.Id,
                 EventType: "user.created");
             await client.Webhooks.TestAsync(testRequest);
             Console.WriteLine("Test event sent");
            
             // Update webhook
             var updateRequest = new UpdateWebhookRequest(
                 Events: new List<string> { "user.created", "user.deleted" });
             await client.Webhooks.UpdateAsync(created.Id, updateRequest);

#### Methods & Properties

- **ListAsync** - Lists all registered webhooks with pagination.
  - Parameters:
    - `request`: List query options.
    - `cancellationToken`: Cancellation token.
  - Returns: Paginated webhook list.
- **GetAsync** - Gets a webhook by ID.
  - Parameters:
    - `webhookId`: The webhook ID.
    - `cancellationToken`: Cancellation token.
  - Returns: Webhook information.
- **CreateAsync** - Registers a new webhook.
  - Parameters:
    - `request`: Webhook registration details.
    - `cancellationToken`: Cancellation token.
  - Returns: Registered webhook information.
- **UpdateAsync** - Updates an existing webhook.
  - Parameters:
    - `webhookId`: The webhook ID to update.
    - `request`: Webhook update details.
    - `cancellationToken`: Cancellation token.
  - Returns: Updated webhook information.
- **DeleteAsync** - Deletes a webhook.
  - Parameters:
    - `webhookId`: The webhook ID to delete.
    - `cancellationToken`: Cancellation token.
  - Returns: Deletion response.
- **TestAsync** - Sends a test event to a webhook to verify connectivity.
  - Parameters:
    - `request`: Test webhook request.
    - `cancellationToken`: Cancellation token.
  - Returns: Test result response.

