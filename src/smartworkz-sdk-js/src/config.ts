/**
 * Configuration interface for SmartWorkz SDK
 */

export interface SmartWorkzConfig {
  /** Base URL for API calls (default: https://api.smartworkz.com) */
  baseUrl: string;
  /** API key for authentication */
  apiKey?: string;
  /** Bearer token (JWT) for authentication */
  bearerToken?: string;
  /** Request timeout in milliseconds (default: 30000) */
  timeout: number;
}
