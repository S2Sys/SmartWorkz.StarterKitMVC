/**
 * Authentication handler for SmartWorkz SDK
 * Manages API key and Bearer token authentication
 */

import { SmartWorkzConfig } from '../config';

export class Authenticator {
  /**
   * Generate HTTP headers with authentication
   * @param config SDK configuration
   * @returns Object with authorization headers
   */
  static getHeaders(config: SmartWorkzConfig): Record<string, string> {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json'
    };

    if (config.apiKey) {
      headers['X-API-Key'] = config.apiKey;
    } else if (config.bearerToken) {
      headers['Authorization'] = `Bearer ${config.bearerToken}`;
    }

    return headers;
  }
}
