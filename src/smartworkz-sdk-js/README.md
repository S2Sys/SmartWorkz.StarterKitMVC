# SmartWorkz SDK for Node.js

Official Node.js/TypeScript SDK for SmartWorkz APIs with full type safety and comprehensive documentation.

![npm](https://img.shields.io/npm/v/@smartworkz/sdk)
![License](https://img.shields.io/npm/l/@smartworkz/sdk)
![Node Version](https://img.shields.io/node/v/@smartworkz/sdk)

## Features

- **Full TypeScript Support** - Complete type definitions for all API endpoints
- **Promise-based API** - Modern async/await support
- **Multiple Auth Methods** - API key and Bearer token authentication
- **Type Safety** - Catch errors at compile time with TypeScript
- **Well Documented** - Comprehensive JSDoc comments and examples
- **Production Ready** - Used in production environments
- **Lightweight** - Minimal dependencies (only axios)

## Installation

```bash
npm install @smartworkz/sdk
# or
yarn add @smartworkz/sdk
# or
pnpm add @smartworkz/sdk
```

## Requirements

- Node.js 14 or higher
- npm/yarn/pnpm package manager

## Quick Start

### Basic Usage with API Key

```typescript
import { SmartWorkzClient } from '@smartworkz/sdk';

const client = new SmartWorkzClient({
  apiKey: 'your-api-key'
});

// List users
const users = await client.users.list({ pageSize: 50 });

// Get specific user
const user = await client.users.get('user-id');

// Create user
const newUser = await client.users.create({
  email: 'user@example.com',
  firstName: 'John',
  lastName: 'Doe'
});

// Update user
const updated = await client.users.update('user-id', {
  firstName: 'Jane'
});

// Delete user
await client.users.delete('user-id');
```

### Using Bearer Token (JWT)

```typescript
const client = new SmartWorkzClient({
  bearerToken: 'your-jwt-token'
});
```

### Custom Configuration

```typescript
const client = new SmartWorkzClient({
  baseUrl: 'https://api.smartworkz.com',
  apiKey: 'your-api-key',
  timeout: 30000 // 30 seconds
});
```

## API Endpoints

### Users

Manage user accounts and profiles.

```typescript
// List users
const users = await client.users.list({ pageSize: 50 });

// Get user
const user = await client.users.get('user-id');

// Create user
const newUser = await client.users.create({
  email: 'user@example.com',
  firstName: 'John',
  lastName: 'Doe'
});

// Update user
const updated = await client.users.update('user-id', {
  firstName: 'Jane'
});

// Delete user
await client.users.delete('user-id');
```

### Transactions

Handle financial transactions and payments.

```typescript
// List transactions
const transactions = await client.transactions.list({
  pageSize: 50,
  userId: 'user-id',
  status: 'completed'
});

// Get transaction
const transaction = await client.transactions.get('transaction-id');

// Create transaction
const newTransaction = await client.transactions.create({
  userId: 'user-id',
  amount: 99.99,
  currency: 'USD',
  description: 'Purchase'
});

// Update transaction
const updated = await client.transactions.update('transaction-id', {
  status: 'completed'
});

// Delete transaction
await client.transactions.delete('transaction-id');
```

### Products

Manage product catalog and inventory.

```typescript
// List products
const products = await client.products.list({
  pageSize: 50,
  isActive: true
});

// Get product
const product = await client.products.get('product-id');

// Create product
const newProduct = await client.products.create({
  name: 'Product Name',
  description: 'Product description',
  price: 29.99,
  currency: 'USD',
  stock: 100,
  sku: 'SKU-123'
});

// Update product
const updated = await client.products.update('product-id', {
  price: 34.99,
  stock: 95
});

// Delete product
await client.products.delete('product-id');
```

### Reports

Generate and manage business reports.

```typescript
// List reports
const reports = await client.reports.list({
  pageSize: 10,
  type: 'sales',
  status: 'completed'
});

// Get report
const report = await client.reports.get('report-id');

// Create report
const newReport = await client.reports.create({
  title: 'Monthly Sales Report',
  description: 'Sales data for the month',
  type: 'sales',
  filters: { month: 'April', year: 2026 }
});

// Cancel report
await client.reports.cancel('report-id');
```

### Webhooks

Configure webhooks for event notifications.

```typescript
// List webhooks
const webhooks = await client.webhooks.list({
  pageSize: 10,
  isActive: true
});

// Get webhook
const webhook = await client.webhooks.get('webhook-id');

// Create webhook
const newWebhook = await client.webhooks.create({
  url: 'https://your-domain.com/webhooks',
  events: ['user.created', 'transaction.completed']
});

// Update webhook
const updated = await client.webhooks.update('webhook-id', {
  events: ['user.created', 'user.updated', 'transaction.completed']
});

// Delete webhook
await client.webhooks.delete('webhook-id');

// Test webhook
await client.webhooks.test('webhook-id');
```

## Error Handling

```typescript
try {
  const user = await client.users.get('non-existent-id');
} catch (error) {
  if (error.response?.status === 404) {
    console.error('User not found');
  } else if (error.response?.status === 401) {
    console.error('Unauthorized - check API key');
  } else {
    console.error('Error:', error.message);
  }
}
```

## Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `baseUrl` | string | `https://api.smartworkz.com` | API base URL |
| `apiKey` | string | undefined | API key for authentication |
| `bearerToken` | string | undefined | JWT bearer token for authentication |
| `timeout` | number | `30000` | Request timeout in milliseconds |

Either `apiKey` or `bearerToken` must be provided.

## Examples

See the `examples/` directory for complete examples:

- `basic.js` - Basic usage with common operations
- `typescript.ts` - Type-safe TypeScript usage
- `async-await.js` - Async/await patterns and concurrent operations

Run examples:

```bash
# After installing dependencies
npm install

# Build the SDK
npm run build

# Run basic example
node examples/basic.js

# Run async/await example
node examples/async-await.js

# Run TypeScript example (requires ts-node)
npx ts-node examples/typescript.ts
```

## Building from Source

```bash
# Install dependencies
npm install

# Compile TypeScript
npm run build

# Run tests
npm test

# Create package for npm
npm pack
```

## Type Definitions

All types are included in the package under `dist/index.d.ts`:

```typescript
import type {
  User,
  Transaction,
  Product,
  Report,
  Webhook
} from '@smartworkz/sdk';
```

## API Documentation

For complete API documentation, visit:
https://api.smartworkz.com/docs

## Contributing

Contributions are welcome! Please read our contributing guidelines.

## Support

For issues, questions, or feedback:

- GitHub Issues: https://github.com/S2Sys/SmartWorkz/issues
- Email: support@smartworkz.com
- Documentation: https://docs.smartworkz.com

## License

MIT License - see LICENSE file for details

## Changelog

### 1.0.0 (2026-04-24)

- Initial release
- Full TypeScript support
- Support for Users, Transactions, Products, Reports, and Webhooks endpoints
- API key and Bearer token authentication
- Comprehensive documentation and examples

---

Made with ❤️ by S2 Systems
