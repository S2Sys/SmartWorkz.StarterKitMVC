/**
 * Transactions API endpoint client
 *
 * Provides access to:
 * - List transactions: GET /api/transactions
 * - Get transaction: GET /api/transactions/{id}
 * - Create transaction: POST /api/transactions
 * - Update transaction: PUT /api/transactions/{id}
 * - Delete transaction: DELETE /api/transactions/{id}
 */

import { AxiosInstance } from 'axios';
import { Transaction, CreateTransactionRequest, UpdateTransactionRequest, TransactionListOptions } from '../types/transaction';

export class TransactionsEndpoint {
  constructor(private httpClient: AxiosInstance) {}

  /**
   * List all transactions with pagination and filtering
   * @param options List options
   * @returns Array of transactions
   */
  async list(options?: TransactionListOptions): Promise<Transaction[]> {
    const params = {
      pageSize: options?.pageSize || 50,
      ...(options?.after && { after: options.after }),
      ...(options?.userId && { userId: options.userId }),
      ...(options?.status && { status: options.status })
    };

    const response = await this.httpClient.get<Transaction[]>('/api/transactions', { params });
    return response.data;
  }

  /**
   * Get transaction by ID
   * @param transactionId Transaction ID
   * @returns Transaction object
   */
  async get(transactionId: string): Promise<Transaction> {
    const response = await this.httpClient.get<Transaction>(`/api/transactions/${transactionId}`);
    return response.data;
  }

  /**
   * Create new transaction
   * @param request Transaction creation request
   * @returns Created transaction
   */
  async create(request: CreateTransactionRequest): Promise<Transaction> {
    const response = await this.httpClient.post<Transaction>('/api/transactions', request);
    return response.data;
  }

  /**
   * Update transaction
   * @param transactionId Transaction ID
   * @param request Update request
   * @returns Updated transaction
   */
  async update(transactionId: string, request: UpdateTransactionRequest): Promise<Transaction> {
    const response = await this.httpClient.put<Transaction>(`/api/transactions/${transactionId}`, request);
    return response.data;
  }

  /**
   * Delete transaction
   * @param transactionId Transaction ID
   */
  async delete(transactionId: string): Promise<void> {
    await this.httpClient.delete(`/api/transactions/${transactionId}`);
  }
}
