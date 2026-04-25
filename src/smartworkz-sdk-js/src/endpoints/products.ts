/**
 * Products API endpoint client
 *
 * Provides access to:
 * - List products: GET /api/products
 * - Get product: GET /api/products/{id}
 * - Create product: POST /api/products
 * - Update product: PUT /api/products/{id}
 * - Delete product: DELETE /api/products/{id}
 */

import { AxiosInstance } from 'axios';
import { Product, CreateProductRequest, UpdateProductRequest, ProductListOptions } from '../types/product';

export class ProductsEndpoint {
  constructor(private httpClient: AxiosInstance) {}

  /**
   * List all products with pagination and filtering
   * @param options List options
   * @returns Array of products
   */
  async list(options?: ProductListOptions): Promise<Product[]> {
    const params = {
      pageSize: options?.pageSize || 50,
      ...(options?.after && { after: options.after }),
      ...(options?.isActive !== undefined && { isActive: options.isActive })
    };

    const response = await this.httpClient.get<Product[]>('/api/products', { params });
    return response.data;
  }

  /**
   * Get product by ID
   * @param productId Product ID
   * @returns Product object
   */
  async get(productId: string): Promise<Product> {
    const response = await this.httpClient.get<Product>(`/api/products/${productId}`);
    return response.data;
  }

  /**
   * Create new product
   * @param request Product creation request
   * @returns Created product
   */
  async create(request: CreateProductRequest): Promise<Product> {
    const response = await this.httpClient.post<Product>('/api/products', request);
    return response.data;
  }

  /**
   * Update product
   * @param productId Product ID
   * @param request Update request
   * @returns Updated product
   */
  async update(productId: string, request: UpdateProductRequest): Promise<Product> {
    const response = await this.httpClient.put<Product>(`/api/products/${productId}`, request);
    return response.data;
  }

  /**
   * Delete product
   * @param productId Product ID
   */
  async delete(productId: string): Promise<void> {
    await this.httpClient.delete(`/api/products/${productId}`);
  }
}
