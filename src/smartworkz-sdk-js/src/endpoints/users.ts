/**
 * Users API endpoint client
 *
 * Provides access to:
 * - List users: GET /api/users
 * - Get user: GET /api/users/{id}
 * - Create user: POST /api/users
 * - Update user: PUT /api/users/{id}
 * - Delete user: DELETE /api/users/{id}
 *
 * @example
 * ```typescript
 * const client = new SmartWorkzClient({ apiKey: 'key' });
 *
 * // List users
 * const users = await client.users.list({ pageSize: 50 });
 *
 * // Get user
 * const user = await client.users.get('user-123');
 *
 * // Create user
 * const newUser = await client.users.create({
 *   email: 'test@example.com',
 *   firstName: 'John',
 *   lastName: 'Doe'
 * });
 *
 * // Update user
 * const updated = await client.users.update('user-123', {
 *   firstName: 'Jane'
 * });
 *
 * // Delete user
 * await client.users.delete('user-123');
 * ```
 */

import { AxiosInstance } from 'axios';
import { User, CreateUserRequest, UpdateUserRequest, ListOptions } from '../types/user';

export class UsersEndpoint {
  constructor(private httpClient: AxiosInstance) {}

  /**
   * List all users with pagination
   * @param options Pagination options
   * @returns Array of users
   */
  async list(options?: ListOptions): Promise<User[]> {
    const params = {
      pageSize: options?.pageSize || 50,
      ...(options?.after && { after: options.after })
    };

    const response = await this.httpClient.get<User[]>('/api/users', { params });
    return response.data;
  }

  /**
   * Get user by ID
   * @param userId User ID
   * @returns User object
   */
  async get(userId: string): Promise<User> {
    const response = await this.httpClient.get<User>(`/api/users/${userId}`);
    return response.data;
  }

  /**
   * Create new user
   * @param request User creation request
   * @returns Created user
   */
  async create(request: CreateUserRequest): Promise<User> {
    const response = await this.httpClient.post<User>('/api/users', request);
    return response.data;
  }

  /**
   * Update user
   * @param userId User ID
   * @param request Update request
   * @returns Updated user
   */
  async update(userId: string, request: UpdateUserRequest): Promise<User> {
    const response = await this.httpClient.put<User>(`/api/users/${userId}`, request);
    return response.data;
  }

  /**
   * Delete user
   * @param userId User ID
   */
  async delete(userId: string): Promise<void> {
    await this.httpClient.delete(`/api/users/${userId}`);
  }
}
