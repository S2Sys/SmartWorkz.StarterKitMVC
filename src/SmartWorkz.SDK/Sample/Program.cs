using Microsoft.Extensions.DependencyInjection;
using SmartWorkz;
using SmartWorkz.Extensions;
using SmartWorkz.Models;

// ─── Configuration ────────────────────────────────────────────────────────
// Note: Configuration from appsettings.json requires additional package
// For this sample, we use direct configuration instead

// ─── DI Container Setup ───────────────────────────────────────────────────
var services = new ServiceCollection();

// Register SmartWorkz client with direct configuration
services.AddSmartWorkzClient(options =>
{
    options.BaseUrl = "https://localhost:7001";  // Local development API
    options.BearerToken = "your-jwt-token-here";  // Replace with actual token
    options.Timeout = TimeSpan.FromSeconds(30);
});

var serviceProvider = services.BuildServiceProvider();

// ─── Examples ─────────────────────────────────────────────────────────────
Console.WriteLine("SmartWorkz SDK Sample Console Application");
Console.WriteLine("==========================================\n");

try
{
    var client = serviceProvider.GetRequiredService<ISmartWorkzClient>();

    // ─── Authentication Example ────────────────────────────────────────────
    Console.WriteLine("Authentication Examples");
    Console.WriteLine("-----------------------");

    // Example 1: Login
    Console.WriteLine("\n[Example 1] User Login");
    var loginRequest = new LoginRequest(
        Email: "user@example.com",
        Password: "password123");
    Console.WriteLine($"Attempting login for: {loginRequest.Email}");
    // var authResponse = await client.Authentication.LoginAsync(loginRequest);
    // if (authResponse.Success)
    //     Console.WriteLine($"✓ Login successful. Token: {authResponse.Token}");

    // Example 2: Register
    Console.WriteLine("\n[Example 2] User Registration");
    var registerRequest = new RegisterRequest(
        Email: "newuser@example.com",
        FirstName: "John",
        LastName: "Doe",
        Password: "SecurePass123!");
    Console.WriteLine($"Registering new user: {registerRequest.Email}");
    // var registerResponse = await client.Authentication.RegisterAsync(registerRequest);
    // if (registerResponse.Success)
    //     Console.WriteLine("✓ Registration successful");

    // ─── Users API Example ─────────────────────────────────────────────────
    Console.WriteLine("\n\nUsers API Examples");
    Console.WriteLine("------------------");

    // Example 3: List Users
    Console.WriteLine("\n[Example 3] List Users");
    var listRequest = new ListUsersRequest(
        PageNumber: 1,
        PageSize: 25,
        SortBy: "Email");
    Console.WriteLine("Fetching users (Page 1, Size 25)...");
    // var usersResponse = await client.Users.ListAsync(listRequest);
    // Console.WriteLine($"✓ Found {usersResponse.TotalCount} total users");
    // if (usersResponse.Items != null && usersResponse.Items.Count > 0)
    // {
    //     Console.WriteLine("Displaying first 5 users:");
    //     foreach (var user in usersResponse.Items.Take(5))
    //     {
    //         Console.WriteLine($"  - {user.FirstName} {user.LastName} <{user.Email}>");
    //     }
    // }

    // Example 4: Get Single User
    Console.WriteLine("\n[Example 4] Get User by ID");
    const string userId = "user-123";
    Console.WriteLine($"Fetching user: {userId}");
    // var user = await client.Users.GetAsync(userId);
    // Console.WriteLine($"✓ User found: {user.FirstName} {user.LastName}");
    // Console.WriteLine($"  Email: {user.Email}");
    // Console.WriteLine($"  Created: {user.CreatedAt:G}");

    // Example 5: Create User
    Console.WriteLine("\n[Example 5] Create New User");
    var createUserRequest = new CreateUserRequest(
        Email: "alice@example.com",
        FirstName: "Alice",
        LastName: "Smith",
        PhoneNumber: "+1-555-0100");
    Console.WriteLine($"Creating user: {createUserRequest.Email}");
    // var newUser = await client.Users.CreateAsync(createUserRequest);
    // Console.WriteLine("✓ User created successfully");
    // Console.WriteLine($"  ID: {newUser.Id}");
    // Console.WriteLine($"  Email: {newUser.Email}");

    // Example 6: Update User
    Console.WriteLine("\n[Example 6] Update User");
    var updateRequest = new UpdateUserRequest(
        FirstName: "Jane",
        LastName: "Smith Updated");
    Console.WriteLine($"Updating user: {userId}");
    // var updatedUser = await client.Users.UpdateAsync(userId, updateRequest);
    // Console.WriteLine("✓ User updated successfully");
    // Console.WriteLine($"  Name: {updatedUser.FirstName} {updatedUser.LastName}");

    // ─── Transactions API Example ──────────────────────────────────────────
    Console.WriteLine("\n\nTransactions API Examples");
    Console.WriteLine("-------------------------");

    // Example 7: List Transactions
    Console.WriteLine("\n[Example 7] List Transactions");
    var listTransactions = new ListTransactionsRequest(
        PageNumber: 1,
        PageSize: 10,
        Status: "completed");
    Console.WriteLine("Fetching completed transactions...");
    // var txnResponse = await client.Transactions.ListAsync(listTransactions);
    // Console.WriteLine($"✓ Found {txnResponse.TotalCount} transactions");

    // Example 8: Create Transaction
    Console.WriteLine("\n[Example 8] Create Transaction");
    var createTxn = new CreateTransactionRequest(
        UserId: userId,
        Amount: 99.99m,
        Currency: "USD",
        Type: "payment",
        Description: "Monthly subscription renewal");
    Console.WriteLine($"Creating transaction for user {userId}...");
    // var newTxn = await client.Transactions.CreateAsync(createTxn);
    // Console.WriteLine("✓ Transaction created");
    // Console.WriteLine($"  ID: {newTxn.Id}");
    // Console.WriteLine($"  Amount: {newTxn.Amount} {newTxn.Currency}");
    // Console.WriteLine($"  Status: {newTxn.Status}");

    // ─── Products API Example ──────────────────────────────────────────────
    Console.WriteLine("\n\nProducts API Examples");
    Console.WriteLine("---------------------");

    // Example 9: List Products
    Console.WriteLine("\n[Example 9] List Products");
    var listProducts = new ListProductsRequest(
        PageNumber: 1,
        PageSize: 20,
        SearchTerm: "laptop",
        MinPrice: 500m,
        MaxPrice: 2000m);
    Console.WriteLine($"Searching products: '{listProducts.SearchTerm}' (${listProducts.MinPrice}-${listProducts.MaxPrice})...");
    // var productsResponse = await client.Products.ListAsync(listProducts);
    // Console.WriteLine($"✓ Found {productsResponse.TotalCount} matching products");

    // Example 10: Create Product
    Console.WriteLine("\n[Example 10] Create Product");
    var createProduct = new CreateProductRequest(
        Name: "USB-C Cable Pro",
        Description: "High-speed USB 3.1 Gen 2 cable with fast charging",
        Price: 39.99m,
        StockQuantity: 150);
    Console.WriteLine($"Creating product: {createProduct.Name}");
    // var newProduct = await client.Products.CreateAsync(createProduct);
    // Console.WriteLine("✓ Product created");
    // Console.WriteLine($"  ID: {newProduct.Id}");
    // Console.WriteLine($"  Price: ${newProduct.Price}");
    // Console.WriteLine($"  Stock: {newProduct.StockQuantity} units");

    // ─── Reports API Example ───────────────────────────────────────────────
    Console.WriteLine("\n\nReports API Examples");
    Console.WriteLine("--------------------");

    // Example 11: Generate Report
    Console.WriteLine("\n[Example 11] Generate Report");
    var generateReport = new GenerateReportRequest(
        Title: "April 2026 Sales Report",
        Type: "sales",
        Format: "pdf",
        Filters: new Dictionary<string, object>
        {
            { "month", 4 },
            { "year", 2026 }
        });
    Console.WriteLine($"Generating report: {generateReport.Title}");
    // var report = await client.Reports.GenerateAsync(generateReport);
    // Console.WriteLine("✓ Report generated");
    // Console.WriteLine($"  ID: {report.Id}");
    // Console.WriteLine($"  Records: {report.RecordCount}");
    // Console.WriteLine($"  Download: {report.DataUrl}");

    // ─── Webhooks API Example ──────────────────────────────────────────────
    Console.WriteLine("\n\nWebhooks API Examples");
    Console.WriteLine("---------------------");

    // Example 12: Create Webhook
    Console.WriteLine("\n[Example 12] Register Webhook");
    var createWebhook = new CreateWebhookRequest(
        Url: "https://myapp.com/webhooks/smartworkz",
        Events: new List<string> { "user.created", "transaction.completed", "product.updated" },
        IsActive: true,
        RetryAttempts: 3,
        TimeoutSeconds: 10);
    Console.WriteLine($"Registering webhook for URL: {createWebhook.Url}");
    Console.WriteLine($"  Events: {string.Join(", ", createWebhook.Events)}");
    // var webhook = await client.Webhooks.CreateAsync(createWebhook);
    // Console.WriteLine("✓ Webhook registered");
    // Console.WriteLine($"  ID: {webhook.Id}");

    // Example 13: Test Webhook
    Console.WriteLine("\n[Example 13] Test Webhook");
    // var testRequest = new TestWebhookRequest(
    //     WebhookId: webhook.Id,
    //     EventType: "user.created");
    // var testResult = await client.Webhooks.TestAsync(testRequest);
    // Console.WriteLine("✓ Test event sent to webhook");
    // if (testResult.Success)
    //     Console.WriteLine($"  Message: {testResult.Message}");

    Console.WriteLine("\n\n============================================");
    Console.WriteLine("Examples completed successfully!");
    Console.WriteLine("(Note: API calls are commented out - uncomment to test with actual API)");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    Environment.Exit(1);
}

if (serviceProvider is IAsyncDisposable asyncDisposable)
    await asyncDisposable.DisposeAsync();
else
    serviceProvider?.Dispose();
