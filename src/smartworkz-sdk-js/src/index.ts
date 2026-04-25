/**
 * SmartWorkz SDK - Official Node.js/TypeScript SDK for SmartWorkz APIs
 *
 * @packageDocumentation
 *
 * @example
 * ```typescript
 * import { SmartWorkzClient } from '@smartworkz/sdk';
 *
 * const client = new SmartWorkzClient({
 *   apiKey: 'your-api-key'
 * });
 *
 * // List users
 * const users = await client.users.list();
 *
 * // Create transaction
 * const transaction = await client.transactions.create({
 *   userId: 'user-123',
 *   amount: 99.99,
 *   currency: 'USD',
 *   description: 'Purchase'
 * });
 * ```
 */

export { SmartWorkzClient } from './client';
export type { SmartWorkzConfig } from './config';
export { Authenticator } from './auth/authenticator';

// Export endpoint classes
export { UsersEndpoint } from './endpoints/users';
export { TransactionsEndpoint } from './endpoints/transactions';
export { ProductsEndpoint } from './endpoints/products';
export { ReportsEndpoint } from './endpoints/reports';
export { WebhooksEndpoint } from './endpoints/webhooks';

// Export type definitions
export type { User, CreateUserRequest, UpdateUserRequest, ListOptions } from './types/user';
export type { Transaction, CreateTransactionRequest, UpdateTransactionRequest, TransactionListOptions } from './types/transaction';
export type { Product, CreateProductRequest, UpdateProductRequest, ProductListOptions } from './types/product';
export type { Report, CreateReportRequest, ReportListOptions } from './types/report';
export type { Webhook, CreateWebhookRequest, UpdateWebhookRequest, WebhookListOptions } from './endpoints/webhooks';
