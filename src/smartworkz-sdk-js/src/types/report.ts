/**
 * Report type definitions and interfaces
 */

export interface Report {
  id: string;
  title: string;
  description: string;
  type: 'sales' | 'users' | 'transactions' | 'inventory' | 'custom';
  status: 'pending' | 'processing' | 'completed' | 'failed';
  createdAt: Date;
  completedAt?: Date;
  url?: string;
}

export interface CreateReportRequest {
  title: string;
  description: string;
  type: 'sales' | 'users' | 'transactions' | 'inventory' | 'custom';
  filters?: Record<string, unknown>;
}

export interface ReportListOptions {
  pageSize?: number;
  after?: string;
  type?: 'sales' | 'users' | 'transactions' | 'inventory' | 'custom';
  status?: 'pending' | 'processing' | 'completed' | 'failed';
}
