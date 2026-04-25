# Development Guide

This guide provides instructions for developing, building, testing, and publishing the SmartWorkz SDK.

## Prerequisites

- Node.js 14 or higher (https://nodejs.org/)
- npm, yarn, or pnpm package manager
- Git for version control

## Getting Started

### 1. Install Dependencies

```bash
npm install
```

Or with yarn/pnpm:

```bash
yarn install
# or
pnpm install
```

### 2. Build the SDK

Compile TypeScript to JavaScript:

```bash
npm run build
```

This generates:
- `dist/index.js` - Compiled JavaScript
- `dist/index.d.ts` - TypeScript declarations
- `dist/**/*.js` - Compiled modules
- `dist/**/*.d.ts` - Type definitions for all modules

Using the PowerShell script (Windows):

```powershell
.\build.ps1
```

Using the bash script:

```bash
chmod +x build.sh
./build.sh
```

## Project Structure

```
smartworkz-sdk-js/
├── src/                          # TypeScript source files
│   ├── index.ts                  # Main entry point
│   ├── client.ts                 # SmartWorkzClient class
│   ├── config.ts                 # Configuration types
│   ├── auth/
│   │   └── authenticator.ts      # Authentication handler
│   ├── endpoints/                # API endpoint classes
│   │   ├── users.ts
│   │   ├── transactions.ts
│   │   ├── products.ts
│   │   ├── reports.ts
│   │   └── webhooks.ts
│   └── types/                    # Type definitions
│       ├── user.ts
│       ├── transaction.ts
│       ├── product.ts
│       └── report.ts
├── dist/                         # Compiled output (generated)
├── tests/                        # Test files
│   ├── client.test.ts
│   └── authenticator.test.ts
├── examples/                     # Example code
│   ├── basic.js
│   ├── typescript.ts
│   └── async-await.js
├── package.json                  # npm configuration
├── tsconfig.json                 # TypeScript configuration
├── jest.config.js                # Jest test configuration
├── README.md                     # User documentation
└── DEVELOPMENT.md                # This file
```

## Development Tasks

### Running Tests

```bash
npm test
```

Run tests in watch mode:

```bash
npm test -- --watch
```

Run tests with coverage:

```bash
npm test -- --coverage
```

### Linting (optional)

If eslint is configured:

```bash
npm run lint
```

### Building the Package

Create an npm package (.tgz file):

```bash
npm pack
```

This creates a file like `smartworkz-sdk-1.0.0.tgz` that can be:
- Distributed locally
- Uploaded to a private npm registry
- Published to the public npm registry

### Type Checking

Verify TypeScript compilation:

```bash
npm run build
```

To check types without emitting:

```bash
npx tsc --noEmit
```

## Making Changes

### Adding a New Endpoint

1. **Create types** (`src/types/new-resource.ts`):

```typescript
export interface NewResource {
  id: string;
  name: string;
  createdAt: Date;
}

export interface CreateNewResourceRequest {
  name: string;
}

export interface NewResourceListOptions {
  pageSize?: number;
  after?: string;
}
```

2. **Create endpoint class** (`src/endpoints/new-resource.ts`):

```typescript
import { AxiosInstance } from 'axios';
import { NewResource, CreateNewResourceRequest, NewResourceListOptions } from '../types/new-resource';

export class NewResourceEndpoint {
  constructor(private httpClient: AxiosInstance) {}

  async list(options?: NewResourceListOptions): Promise<NewResource[]> {
    const params = {
      pageSize: options?.pageSize || 50,
      ...(options?.after && { after: options.after })
    };
    const response = await this.httpClient.get<NewResource[]>('/api/new-resources', { params });
    return response.data;
  }

  async get(id: string): Promise<NewResource> {
    const response = await this.httpClient.get<NewResource>(`/api/new-resources/${id}`);
    return response.data;
  }

  async create(request: CreateNewResourceRequest): Promise<NewResource> {
    const response = await this.httpClient.post<NewResource>('/api/new-resources', request);
    return response.data;
  }
}
```

3. **Update client** (`src/client.ts`):

```typescript
import { NewResourceEndpoint } from './endpoints/new-resource';

export class SmartWorkzClient {
  // ... existing code ...
  public newResource: NewResourceEndpoint;

  constructor(config: Partial<SmartWorkzConfig> = {}) {
    // ... existing initialization ...
    this.newResource = new NewResourceEndpoint(this.httpClient);
  }
}
```

4. **Update exports** (`src/index.ts`):

```typescript
export { NewResourceEndpoint } from './endpoints/new-resource';
export type { NewResource, CreateNewResourceRequest, NewResourceListOptions } from './types/new-resource';
```

5. **Add tests** (`tests/new-resource.test.ts`):

```typescript
import { SmartWorkzClient } from '../src/client';

describe('NewResourceEndpoint', () => {
  it('should have list method', () => {
    const client = new SmartWorkzClient({ apiKey: 'test-key' });
    expect(typeof client.newResource.list).toBe('function');
  });
});
```

### Updating Types

1. Modify the corresponding file in `src/types/`
2. Update endpoint implementations if methods change
3. Update `src/index.ts` exports
4. Add/update tests
5. Run `npm run build` to verify compilation

## Code Style

- Use TypeScript strict mode
- Add JSDoc comments to public methods
- Use descriptive variable names
- Follow the existing code patterns

Example:

```typescript
/**
 * Get resource by ID
 * @param resourceId Resource ID
 * @returns Resource object or undefined if not found
 */
async get(resourceId: string): Promise<Resource> {
  const response = await this.httpClient.get<Resource>(`/api/resources/${resourceId}`);
  return response.data;
}
```

## Testing Guidelines

1. **Unit Tests** - Test individual classes and functions
2. **Integration Tests** - Test client initialization and endpoint methods
3. **Mock HTTP Calls** - Use axios mock adapter or jest mocks

Example test:

```typescript
import { UsersEndpoint } from '../src/endpoints/users';
import axios from 'axios';
import MockAdapter from 'axios-mock-adapter';

describe('UsersEndpoint', () => {
  it('should list users', async () => {
    const mock = new MockAdapter(axios);
    const mockUsers = [{ id: '1', email: 'test@example.com', ... }];
    mock.onGet('/api/users').reply(200, mockUsers);

    const endpoint = new UsersEndpoint(axios);
    const users = await endpoint.list();

    expect(users).toEqual(mockUsers);
  });
});
```

## Version Management

Update version in `package.json`:

```json
{
  "version": "1.0.1"
}
```

Follow semantic versioning:
- **1.0.0** - Major.Minor.Patch
- Major: Breaking changes
- Minor: New features (backward compatible)
- Patch: Bug fixes

## Publishing to npm

### Setup

1. Create account on https://www.npmjs.com/
2. Login locally:

```bash
npm login
```

### Publish

```bash
npm publish
```

### Scoped Package

The package is published as `@smartworkz/sdk`:

```bash
npm publish --access public
```

Users install with:

```bash
npm install @smartworkz/sdk
```

## Troubleshooting

### TypeScript Errors

Clear and rebuild:

```bash
rm -rf dist
npm run build
```

### Test Failures

Debug specific test:

```bash
npm test -- tests/client.test.ts --verbose
```

### npm Issues

Clear npm cache:

```bash
npm cache clean --force
```

## Documentation

- **README.md** - User documentation and API reference
- **DEVELOPMENT.md** - This development guide
- **JSDoc comments** - Inline code documentation
- **Examples/** - Working code examples

## Resources

- [TypeScript Handbook](https://www.typescriptlang.org/docs/)
- [axios Documentation](https://axios-http.com/docs/intro)
- [npm Documentation](https://docs.npmjs.com/)
- [Jest Testing Framework](https://jestjs.io/)

## Support

For questions or issues:
- Check existing tests
- Review similar endpoint implementations
- Consult the README.md
- Open an issue on GitHub
