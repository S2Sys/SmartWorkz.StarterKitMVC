/**
 * SmartWorkz Client Tests
 */

import { SmartWorkzClient } from '../src/client';

describe('SmartWorkzClient', () => {
  it('should throw error if neither apiKey nor bearerToken is provided', () => {
    expect(() => {
      new SmartWorkzClient({});
    }).toThrow('Either apiKey or bearerToken must be provided');
  });

  it('should initialize with apiKey', () => {
    const client = new SmartWorkzClient({
      apiKey: 'test-key'
    });

    expect(client).toBeDefined();
    expect(client.users).toBeDefined();
    expect(client.transactions).toBeDefined();
    expect(client.products).toBeDefined();
    expect(client.reports).toBeDefined();
    expect(client.webhooks).toBeDefined();
  });

  it('should initialize with bearerToken', () => {
    const client = new SmartWorkzClient({
      bearerToken: 'test-token'
    });

    expect(client).toBeDefined();
    expect(client.users).toBeDefined();
  });

  it('should use custom baseUrl', () => {
    const client = new SmartWorkzClient({
      apiKey: 'test-key',
      baseUrl: 'https://custom.example.com'
    });

    expect(client).toBeDefined();
  });

  it('should use custom timeout', () => {
    const client = new SmartWorkzClient({
      apiKey: 'test-key',
      timeout: 60000
    });

    expect(client).toBeDefined();
  });

  it('should have all endpoint classes', () => {
    const client = new SmartWorkzClient({
      apiKey: 'test-key'
    });

    expect(typeof client.users.list).toBe('function');
    expect(typeof client.users.get).toBe('function');
    expect(typeof client.users.create).toBe('function');
    expect(typeof client.users.update).toBe('function');
    expect(typeof client.users.delete).toBe('function');

    expect(typeof client.transactions.list).toBe('function');
    expect(typeof client.transactions.get).toBe('function');
    expect(typeof client.transactions.create).toBe('function');

    expect(typeof client.products.list).toBe('function');
    expect(typeof client.products.get).toBe('function');

    expect(typeof client.reports.list).toBe('function');
    expect(typeof client.reports.get).toBe('function');

    expect(typeof client.webhooks.list).toBe('function');
    expect(typeof client.webhooks.get).toBe('function');
  });
});
