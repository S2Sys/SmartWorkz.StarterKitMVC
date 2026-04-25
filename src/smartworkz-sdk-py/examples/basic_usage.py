"""Basic usage examples for SmartWorkz SDK."""

from smartworkz import SmartWorkzClient, SmartWorkzConfig

# Example 1: Initialize client with API key
print("Example 1: Initialize with API key")
client = SmartWorkzClient(api_key="your-api-key-here")

# Example 2: Initialize client with bearer token
print("\nExample 2: Initialize with bearer token")
client = SmartWorkzClient(bearer_token="your-jwt-token-here")

# Example 3: Use custom configuration
print("\nExample 3: Custom configuration")
config = SmartWorkzConfig(
    base_url="https://api.smartworkz.com",
    api_key="your-api-key-here",
    timeout=30,
)
client = SmartWorkzClient(config=config)

# Example 4: Context manager for automatic cleanup
print("\nExample 4: Context manager")
with SmartWorkzClient(api_key="your-api-key-here") as client:
    # Client will be automatically closed after the block
    pass

# Example 5: List users with pagination
print("\nExample 5: List users")
try:
    users = client.users.list(page_size=50)
    print(f"Found {len(users)} users")
    for user in users:
        print(f"  - {user.full_name} ({user.email})")
except Exception as e:
    print(f"Error listing users: {e}")

# Example 6: Get specific user
print("\nExample 6: Get user by ID")
try:
    user = client.users.get("user-123")
    print(f"User: {user.full_name}")
    print(f"Email: {user.email}")
    print(f"Active: {user.is_active}")
except Exception as e:
    print(f"Error getting user: {e}")

# Example 7: Create new user
print("\nExample 7: Create new user")
try:
    new_user = client.users.create(
        email="john.doe@example.com",
        first_name="John",
        last_name="Doe",
    )
    print(f"Created user: {new_user.id}")
except Exception as e:
    print(f"Error creating user: {e}")

# Example 8: Update user
print("\nExample 8: Update user")
try:
    updated_user = client.users.update(
        "user-123",
        first_name="Jane",
        last_name="Smith",
    )
    print(f"Updated user: {updated_user.full_name}")
except Exception as e:
    print(f"Error updating user: {e}")

# Example 9: List transactions
print("\nExample 9: List transactions")
try:
    transactions = client.transactions.list(page_size=50)
    print(f"Found {len(transactions)} transactions")
    for txn in transactions:
        print(f"  - {txn.id}: {txn.amount} {txn.currency} ({txn.status.value})")
except Exception as e:
    print(f"Error listing transactions: {e}")

# Example 10: Create transaction
print("\nExample 10: Create transaction")
try:
    transaction = client.transactions.create(
        user_id="user-123",
        amount=99.99,
        description="Purchase order #12345",
        currency="USD",
    )
    print(f"Created transaction: {transaction.id}")
except Exception as e:
    print(f"Error creating transaction: {e}")

# Example 11: Get transaction status
print("\nExample 11: Get transaction status")
try:
    status = client.transactions.get_status("txn-456")
    print(f"Transaction status: {status.value}")
except Exception as e:
    print(f"Error getting transaction status: {e}")

# Example 12: List products
print("\nExample 12: List products")
try:
    products = client.products.list(page_size=50)
    print(f"Found {len(products)} products")
    for product in products:
        print(f"  - {product.name}: {product.display_price}")
except Exception as e:
    print(f"Error listing products: {e}")

# Example 13: Create product
print("\nExample 13: Create product")
try:
    product = client.products.create(
        name="Premium Widget",
        description="High-quality widget for all purposes",
        price=49.99,
        sku="PREMIUM-WIDGET-001",
        currency="USD",
    )
    print(f"Created product: {product.id}")
except Exception as e:
    print(f"Error creating product: {e}")

# Example 14: Generate report
print("\nExample 14: Generate report")
try:
    report = client.reports.generate(
        title="Monthly Sales Report",
        report_type="sales",
    )
    print(f"Generated report: {report.id}")
except Exception as e:
    print(f"Error generating report: {e}")

# Example 15: Register webhook
print("\nExample 15: Register webhook")
try:
    webhook = client.webhooks.register(
        url="https://your-domain.com/webhooks/smartworkz",
        events=["transaction.created", "user.updated"],
        active=True,
    )
    print(f"Registered webhook: {webhook.id}")
except Exception as e:
    print(f"Error registering webhook: {e}")

# Example 16: Test webhook
print("\nExample 16: Test webhook")
try:
    result = client.webhooks.test("webhook-789")
    print(f"Webhook test result: {result}")
except Exception as e:
    print(f"Error testing webhook: {e}")

# Always close the client when done
client.close()

print("\nAll examples completed!")
