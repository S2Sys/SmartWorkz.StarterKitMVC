/**
 * SmartWorkz API Client for Node.js
 *
 * @example Basic usage with API key:
 * ```typescript
 * import { SmartWorkzClient } from '@smartworkz/sdk';
 *
 * const client = new SmartWorkzClient({
 *   apiKey: 'your-api-key'
 * });
 *
 * const users = await client.users.list();
 * ```
 *
 * @example With bearer token:
 * ```typescript
 * const client = new SmartWorkzClient({
 *   bearerToken: 'your-jwt-token'
 * });
 *
 * const user = await client.users.get('user-123');
 * ```
 *
 * @example Custom configuration:
 * ```typescript
 * const client = new SmartWorkzClient({
 *   baseUrl: 'https://api.smartworkz.com',
 *   timeout: 30000,
 *   apiKey: 'your-api-key'
 * });
 *
 * const products = await client.products.list({ pageSize: 100 });
 * ```
 */

import axios, { AxiosInstance } from 'axios';
import { SmartWorkzConfig } from './config';
import { Authenticator } from './auth/authenticator';
import { UsersEndpoint } from './endpoints/users';
import { TransactionsEndpoint } from './endpoints/transactions';
import { ProductsEndpoint } from './endpoints/products';
import { ReportsEndpoint } from './endpoints/reports';
import { WebhooksEndpoint } from './endpoints/webhooks';

export class SmartWorkzClient {
  private httpClient: AxiosInstance;
  private config: SmartWorkzConfig;

  /** Users API endpoint */
  public users: UsersEndpoint;
  /** Transactions API endpoint */
  public transactions: TransactionsEndpoint;
  /** Products API endpoint */
  public products: ProductsEndpoint;
  /** Reports API endpoint */
  public reports: ReportsEndpoint;
  /** Webhooks API endpoint */
  public webhooks: WebhooksEndpoint;

  /**
   * Initialize SmartWorkz API client
   * @param config Configuration options
   * @throws Error if neither apiKey nor bearerToken is provided
   */
  constructor(config: Partial<SmartWorkzConfig> = {}) {
    this.config = {
      baseUrl: 'https://api.smartworkz.com',
      timeout: 30000,
      ...config
    };

    // Validate authentication
    if (!this.config.apiKey && !this.config.bearerToken) {
      throw new Error('Either apiKey or bearerToken must be provided');
    }

    // Create HTTP client with authentication
    this.httpClient = axios.create({
      baseURL: this.config.baseUrl,
      timeout: this.config.timeout,
      headers: Authenticator.getHeaders(this.config)
    });

    // Initialize endpoints
    this.users = new UsersEndpoint(this.httpClient);
    this.transactions = new TransactionsEndpoint(this.httpClient);
    this.products = new ProductsEndpoint(this.httpClient);
    this.reports = new ReportsEndpoint(this.httpClient);
    this.webhooks = new WebhooksEndpoint(this.httpClient);
  }
}
