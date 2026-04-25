# SmartWorkz Python SDK

Official Python SDK for SmartWorkz APIs - production-ready, fully typed, and easy to use.

[![PyPI version](https://badge.fury.io/py/smartworkz-sdk.svg)](https://badge.fury.io/py/smartworkz-sdk)
[![Python 3.9+](https://img.shields.io/badge/python-3.9+-blue.svg)](https://www.python.org/downloads/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- **Fully Typed**: Complete type hints for IDE autocomplete and static analysis
- **Sync & Async**: Both synchronous and asynchronous client implementations
- **Comprehensive**: Access to Users, Transactions, Products, Reports, and Webhooks APIs
- **Error Handling**: Rich exception hierarchy with meaningful error messages
- **Pagination**: Built-in support for paginated responses
- **Authentication**: API key and JWT bearer token authentication
- **Context Managers**: Clean resource management with context managers
- **Validation**: Input validation with helpful error messages

## Installation

Install from PyPI:

```bash
pip install smartworkz-sdk
```

Or from source:

```bash
git clone https://github.com/S2Sys/SmartWorkz.git
cd SmartWorkz/src/smartworkz-sdk-py
pip install -e .
```

## Quick Start

### Basic Usage

```python
from smartworkz import SmartWorkzClient

# Initialize client with API key
client = SmartWorkzClient(api_key="your-api-key")

# List users
users = client.users.list(page_size=50)
for user in users:
    print(f"{user.full_name} ({user.email})")

# Get specific user
user = client.users.get("user-123")
print(f"Name: {user.full_name}, Active: {user.is_active}")

# Create new user
new_user = client.users.create(
    email="john@example.com",
    first_name="John",
    last_name="Doe"
)

# Always close the client when done
client.close()
```

### Context Manager

Use context managers for automatic cleanup:

```python
from smartworkz import SmartWorkzClient

with SmartWorkzClient(api_key="your-api-key") as client:
    users = client.users.list()
    for user in users:
        print(user.full_name)
# Client is automatically closed
```

### Async Usage

```python
import asyncio
from smartworkz import AsyncSmartWorkzClient

async def main():
    async with AsyncSmartWorkzClient(api_key="your-api-key") as client:
        users = await client.users.list()
        print(f"Found {len(users)} users")

asyncio.run(main())
```

## Authentication

### API Key Authentication

```python
from smartworkz import SmartWorkzClient

client = SmartWorkzClient(api_key="sk_live_abc123...")
```

### Bearer Token (JWT)

```python
from smartworkz import SmartWorkzClient

client = SmartWorkzClient(bearer_token="eyJhbGciOiJIUzI1NiIs...")
```

### Custom Configuration

```python
from smartworkz import SmartWorkzClient, SmartWorkzConfig

config = SmartWorkzConfig(
    base_url="https://api.smartworkz.com",
    api_key="your-api-key",
    timeout=30
)
client = SmartWorkzClient(config=config)
```

## API Reference

### Users

```python
# List users
users = client.users.list(page_size=100, after="cursor")

# Get user by ID
user = client.users.get("user-123")

# Create user
new_user = client.users.create(
    email="user@example.com",
    first_name="John",
    last_name="Doe"
)

# Update user
updated_user = client.users.update(
    "user-123",
    first_name="Jane",
    is_active=True
)

# Delete user
client.users.delete("user-123")
```

### Transactions

```python
# List transactions
transactions = client.transactions.list(
    page_size=50,
    user_id="user-123",
    status="PENDING"
)

# Get transaction
transaction = client.transactions.get("txn-456")

# Create transaction
new_txn = client.transactions.create(
    user_id="user-123",
    amount=99.99,
    currency="USD",
    description="Purchase order #12345"
)

# Check status
status = client.transactions.get_status("txn-456")

# Cancel transaction
cancelled = client.transactions.cancel("txn-456")
```

### Products

```python
# List products
products = client.products.list(page_size=100)

# Get product
product = client.products.get("prod-789")

# Create product
new_product = client.products.create(
    name="Premium Widget",
    description="High-quality widget",
    price=49.99,
    sku="PREMIUM-001",
    currency="USD"
)

# Update product
updated = client.products.update(
    "prod-789",
    name="Updated Name",
    price=59.99
)

# Delete product
client.products.delete("prod-789")
```

### Reports

```python
# List reports
reports = client.reports.list(page_size=50)

# Get report
report = client.reports.get("report-101")

# Generate report
new_report = client.reports.generate(
    title="Monthly Sales Report",
    report_type="sales"
)

# Check generation status
status = client.reports.get_status("report-101")

# Download report
file_content = client.reports.download("report-101")
with open("report.pdf", "wb") as f:
    f.write(file_content)
```

### Webhooks

```python
# List webhooks
webhooks = client.webhooks.list()

# Get webhook
webhook = client.webhooks.get("webhook-202")

# Register webhook
new_webhook = client.webhooks.register(
    url="https://your-domain.com/webhooks/smartworkz",
    events=["transaction.created", "user.updated"],
    active=True
)

# Update webhook
updated = client.webhooks.update(
    "webhook-202",
    active=False,
    events=["transaction.completed"]
)

# Delete webhook
client.webhooks.delete("webhook-202")

# Test webhook
result = client.webhooks.test("webhook-202")
```

## Error Handling

```python
from smartworkz import (
    SmartWorkzClient,
    SmartWorkzError,
    SmartWorkzNotFoundError,
    SmartWorkzValidationError,
    SmartWorkzAuthenticationError
)

client = SmartWorkzClient(api_key="your-api-key")

try:
    user = client.users.get("user-123")
except SmartWorkzNotFoundError:
    print("User not found")
except SmartWorkzValidationError as e:
    print(f"Validation error: {e}")
except SmartWorkzAuthenticationError:
    print("Authentication failed")
except SmartWorkzError as e:
    print(f"API error: {e}")
```

### Exception Hierarchy

- `SmartWorkzError` - Base exception
  - `SmartWorkzAuthenticationError` - Authentication failed (401)
  - `SmartWorkzAuthorizationError` - Authorization failed (403)
  - `SmartWorkzNotFoundError` - Resource not found (404)
  - `SmartWorkzValidationError` - Validation error (400)
  - `SmartWorkzConflictError` - Resource conflict (409)
  - `SmartWorkzServerError` - Server error (5xx)
  - `SmartWorkzRateLimitError` - Rate limit exceeded (429)
  - `SmartWorkzConnectionError` - Connection error
  - `SmartWorkzTimeoutError` - Request timeout

## Models

### User

```python
user = client.users.get("user-123")
print(user.id)           # User ID
print(user.email)        # Email address
print(user.first_name)   # First name
print(user.last_name)    # Last name
print(user.full_name)    # Full name (property)
print(user.is_active)    # Active status
print(user.created_at)   # Creation timestamp
print(user.updated_at)   # Last update timestamp
print(user.metadata)     # Additional metadata
```

### Transaction

```python
txn = client.transactions.get("txn-456")
print(txn.id)             # Transaction ID
print(txn.user_id)        # User ID
print(txn.amount)         # Amount
print(txn.currency)       # Currency code
print(txn.status)         # TransactionStatus enum
print(txn.is_complete)    # Completion check (property)
print(txn.is_failed)      # Failed check (property)
print(txn.description)    # Description
print(txn.created_at)     # Creation timestamp
```

### Product

```python
product = client.products.get("prod-789")
print(product.id)             # Product ID
print(product.name)           # Product name
print(product.description)    # Description
print(product.price)          # Price
print(product.currency)       # Currency code
print(product.sku)            # Stock keeping unit
print(product.display_price)  # Formatted price (property)
print(product.is_active)      # Active status
```

### Report

```python
report = client.reports.get("report-101")
print(report.id)            # Report ID
print(report.title)         # Report title
print(report.report_type)   # Report type
print(report.is_ready)      # Ready for download
print(report.status)        # Status string (property)
print(report.download_url)  # Download URL
print(report.created_at)    # Creation timestamp
```

## Examples

See the `examples/` directory for complete examples:

- `basic_usage.py` - Basic SDK usage patterns
- `user_management.py` - User CRUD operations
- `transaction_processing.py` - Transaction workflows and bulk operations

## Configuration

### Environment Variables

You can set your API key as an environment variable:

```bash
export SMARTWORKZ_API_KEY="your-api-key"
```

Then load it in your code:

```python
import os
from smartworkz import SmartWorkzClient

api_key = os.getenv("SMARTWORKZ_API_KEY")
client = SmartWorkzClient(api_key=api_key)
```

### Custom Base URL

For development or self-hosted instances:

```python
from smartworkz import SmartWorkzClient

client = SmartWorkzClient(
    api_key="your-api-key",
    base_url="https://api.dev.smartworkz.local"
)
```

## Development

### Setup Development Environment

```bash
git clone https://github.com/S2Sys/SmartWorkz.git
cd SmartWorkz/src/smartworkz-sdk-py

# Create virtual environment
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate

# Install in development mode
pip install -e ".[dev]"
```

### Running Tests

```bash
pytest
pytest -v                  # Verbose output
pytest -k test_users       # Run specific tests
pytest --cov=smartworkz    # With coverage
```

### Code Quality

```bash
# Format code
black smartworkz/

# Sort imports
isort smartworkz/

# Type checking
mypy smartworkz/

# Linting
flake8 smartworkz/
```

## Building and Publishing

### Build Distribution

```bash
python setup.py sdist bdist_wheel
```

### Verify Package

```bash
twine check dist/*
```

### Upload to PyPI

```bash
# Test PyPI
twine upload --repository testpypi dist/*

# Production PyPI
twine upload dist/*
```

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

MIT License - see LICENSE file for details

## Support

For issues, questions, or suggestions:

- GitHub Issues: https://github.com/S2Sys/SmartWorkz/issues
- Email: sdk@smartworkz.com
- Documentation: https://smartworkz.com/docs

## Changelog

### 1.0.0 (2024-04-24)

- Initial release
- Support for Users, Transactions, Products, Reports, and Webhooks APIs
- Sync and async client implementations
- Full type hints
- Comprehensive error handling
- Pagination support

## Security

If you discover a security vulnerability, please send an email to sdk@smartworkz.com instead of using the issue tracker.
