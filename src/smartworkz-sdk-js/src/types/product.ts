/**
 * Product type definitions and interfaces
 */

export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  currency: string;
  stock: number;
  sku: string;
  createdAt: Date;
  updatedAt?: Date;
  isActive: boolean;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  price: number;
  currency: string;
  stock: number;
  sku: string;
}

export interface UpdateProductRequest {
  name?: string;
  description?: string;
  price?: number;
  stock?: number;
  isActive?: boolean;
}

export interface ProductListOptions {
  pageSize?: number;
  after?: string;
  isActive?: boolean;
}
