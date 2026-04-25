/**
 * Transaction type definitions and interfaces
 */

export interface Transaction {
  id: string;
  userId: string;
  amount: number;
  currency: string;
  status: 'pending' | 'completed' | 'failed' | 'cancelled';
  description: string;
  createdAt: Date;
  updatedAt?: Date;
}

export interface CreateTransactionRequest {
  userId: string;
  amount: number;
  currency: string;
  description: string;
}

export interface UpdateTransactionRequest {
  status?: 'pending' | 'completed' | 'failed' | 'cancelled';
  description?: string;
}

export interface TransactionListOptions {
  pageSize?: number;
  after?: string;
  userId?: string;
  status?: 'pending' | 'completed' | 'failed' | 'cancelled';
}
