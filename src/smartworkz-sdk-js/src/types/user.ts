/**
 * User type definitions and interfaces
 */

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  createdAt: Date;
  updatedAt?: Date;
  isActive: boolean;
}

export interface CreateUserRequest {
  email: string;
  firstName: string;
  lastName: string;
}

export interface UpdateUserRequest {
  firstName?: string;
  lastName?: string;
}

export interface ListOptions {
  pageSize?: number;
  after?: string;
}
