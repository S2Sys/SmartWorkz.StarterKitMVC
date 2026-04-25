/**
 * Reports API endpoint client
 *
 * Provides access to:
 * - List reports: GET /api/reports
 * - Get report: GET /api/reports/{id}
 * - Create report: POST /api/reports
 * - Cancel report: DELETE /api/reports/{id}
 */

import { AxiosInstance } from 'axios';
import { Report, CreateReportRequest, ReportListOptions } from '../types/report';

export class ReportsEndpoint {
  constructor(private httpClient: AxiosInstance) {}

  /**
   * List all reports with pagination and filtering
   * @param options List options
   * @returns Array of reports
   */
  async list(options?: ReportListOptions): Promise<Report[]> {
    const params = {
      pageSize: options?.pageSize || 50,
      ...(options?.after && { after: options.after }),
      ...(options?.type && { type: options.type }),
      ...(options?.status && { status: options.status })
    };

    const response = await this.httpClient.get<Report[]>('/api/reports', { params });
    return response.data;
  }

  /**
   * Get report by ID
   * @param reportId Report ID
   * @returns Report object
   */
  async get(reportId: string): Promise<Report> {
    const response = await this.httpClient.get<Report>(`/api/reports/${reportId}`);
    return response.data;
  }

  /**
   * Create new report (asynchronous)
   * @param request Report creation request
   * @returns Report object (will be in 'pending' status)
   */
  async create(request: CreateReportRequest): Promise<Report> {
    const response = await this.httpClient.post<Report>('/api/reports', request);
    return response.data;
  }

  /**
   * Cancel/delete report
   * @param reportId Report ID
   */
  async cancel(reportId: string): Promise<void> {
    await this.httpClient.delete(`/api/reports/${reportId}`);
  }
}
