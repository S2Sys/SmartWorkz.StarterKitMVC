/**
 * Authenticator Tests
 */

import { Authenticator } from '../src/auth/authenticator';
import { SmartWorkzConfig } from '../src/config';

describe('Authenticator', () => {
  it('should create headers with API key', () => {
    const config: SmartWorkzConfig = {
      baseUrl: 'https://api.smartworkz.com',
      timeout: 30000,
      apiKey: 'test-api-key'
    };

    const headers = Authenticator.getHeaders(config);

    expect(headers['Content-Type']).toBe('application/json');
    expect(headers['X-API-Key']).toBe('test-api-key');
    expect(headers['Authorization']).toBeUndefined();
  });

  it('should create headers with bearer token', () => {
    const config: SmartWorkzConfig = {
      baseUrl: 'https://api.smartworkz.com',
      timeout: 30000,
      bearerToken: 'test-jwt-token'
    };

    const headers = Authenticator.getHeaders(config);

    expect(headers['Content-Type']).toBe('application/json');
    expect(headers['Authorization']).toBe('Bearer test-jwt-token');
    expect(headers['X-API-Key']).toBeUndefined();
  });

  it('should prefer apiKey over bearerToken', () => {
    const config: SmartWorkzConfig = {
      baseUrl: 'https://api.smartworkz.com',
      timeout: 30000,
      apiKey: 'test-api-key',
      bearerToken: 'test-jwt-token'
    };

    const headers = Authenticator.getHeaders(config);

    expect(headers['X-API-Key']).toBe('test-api-key');
    expect(headers['Authorization']).toBeUndefined();
  });

  it('should always include Content-Type header', () => {
    const config: SmartWorkzConfig = {
      baseUrl: 'https://api.smartworkz.com',
      timeout: 30000,
      apiKey: 'test-api-key'
    };

    const headers = Authenticator.getHeaders(config);

    expect(headers['Content-Type']).toBe('application/json');
  });
});
