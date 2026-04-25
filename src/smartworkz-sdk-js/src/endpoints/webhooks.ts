/**
 * Webhooks API endpoint client
 *
 * Provides access to:
 * - List webhooks: GET /api/webhooks
 * - Get webhook: GET /api/webhooks/{id}
 * - Create webhook: POST /api/webhooks
 * - Update webhook: PUT /api/webhooks/{id}
 * - Delete webhook: DELETE /api/webhooks/{id}
 * - Test webhook: POST /api/webhooks/{id}/test
 */

import { AxiosInstance } from 'axios';

export interface Webhook {
  id: string;
  url: string;
  events: string[];
  isActive: boolean;
  secret?: string;
  createdAt: Date;
  updatedAt?: Date;
}

export interface CreateWebhookRequest {
  url: string;
  events: string[];
}

export interface UpdateWebhookRequest {
  url?: string;
  events?: string[];
  isActive?: boolean;
}

export interface WebhookListOptions {
  pageSize?: number;
  after?: string;
  isActive?: boolean;
}

export class WebhooksEndpoint {
  constructor(private httpClient: AxiosInstance) {}

  /**
   * List all webhooks with pagination
   * @param options List options
   * @returns Array of webhooks
   */
  async list(options?: WebhookListOptions): Promise<Webhook[]> {
    const params = {
      pageSize: options?.pageSize || 50,
      ...(options?.after && { after: options.after }),
      ...(options?.isActive !== undefined && { isActive: options.isActive })
    };

    const response = await this.httpClient.get<Webhook[]>('/api/webhooks', { params });
    return response.data;
  }

  /**
   * Get webhook by ID
   * @param webhookId Webhook ID
   * @returns Webhook object
   */
  async get(webhookId: string): Promise<Webhook> {
    const response = await this.httpClient.get<Webhook>(`/api/webhooks/${webhookId}`);
    return response.data;
  }

  /**
   * Create new webhook
   * @param request Webhook creation request
   * @returns Created webhook
   */
  async create(request: CreateWebhookRequest): Promise<Webhook> {
    const response = await this.httpClient.post<Webhook>('/api/webhooks', request);
    return response.data;
  }

  /**
   * Update webhook
   * @param webhookId Webhook ID
   * @param request Update request
   * @returns Updated webhook
   */
  async update(webhookId: string, request: UpdateWebhookRequest): Promise<Webhook> {
    const response = await this.httpClient.put<Webhook>(`/api/webhooks/${webhookId}`, request);
    return response.data;
  }

  /**
   * Delete webhook
   * @param webhookId Webhook ID
   */
  async delete(webhookId: string): Promise<void> {
    await this.httpClient.delete(`/api/webhooks/${webhookId}`);
  }

  /**
   * Test webhook by sending a test event
   * @param webhookId Webhook ID
   */
  async test(webhookId: string): Promise<void> {
    await this.httpClient.post(`/api/webhooks/${webhookId}/test`);
  }
}
