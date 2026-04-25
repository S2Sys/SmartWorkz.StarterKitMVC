# SmartWorkz SDK Installation & Setup Guide

## Quick Setup (5 minutes)

### Prerequisites

- **Node.js**: 14 or higher ([download](https://nodejs.org/))
- **npm/yarn/pnpm**: Package manager (included with Node.js)
- **Git**: For version control (optional)

Verify installation:

```bash
node --version
npm --version
```

### Installation Steps

#### 1. Navigate to SDK Directory

```bash
cd src/smartworkz-sdk-js
```

#### 2. Install Dependencies

```bash
npm install
```

This installs:
- `axios` - HTTP client library
- `typescript` - TypeScript compiler
- `jest` - Testing framework
- Development dependencies

#### 3. Build the SDK

```bash
npm run build
```

This creates the `dist/` directory with:
- `dist/index.js` - Main entry point
- `dist/index.d.ts` - Type definitions
- `dist/**/*.js` - Compiled modules
- `dist/**/*.d.ts` - Type definitions for all modules

#### 4. Verify Build

Check that the build was successful:

```bash
ls -la dist/
# Should show: index.js, index.d.ts, auth/, endpoints/, types/
```

### Next Steps

#### Run Examples

```bash
# Basic JavaScript example
node examples/basic.js

# Async/await example
node examples/async-await.js

# TypeScript example (requires ts-node)
npx ts-node examples/typescript.ts
```

#### Run Tests

```bash
npm test
```

#### Create npm Package

```bash
npm pack
# Creates: smartworkz-sdk-1.0.0.tgz
```

## Platform-Specific Instructions

### Windows (PowerShell)

```powershell
# Navigate to directory
cd src/smartworkz-sdk-js

# Install dependencies
npm install

# Build using PowerShell script (optional)
.\build.ps1

# Or standard npm command
npm run build

# Verify
dir dist
```

### macOS/Linux (Bash)

```bash
# Navigate to directory
cd src/smartworkz-sdk-js

# Install dependencies
npm install

# Build using bash script (optional)
chmod +x build.sh
./build.sh

# Or standard npm command
npm run build

# Verify
ls -la dist/
```

## Using the SDK

### As a Local Package

For development, you can use the compiled SDK directly:

```bash
# From parent directory
npm install file:./src/smartworkz-sdk-js
```

### As an npm Package

#### Local Installation

```bash
# Create package
npm pack

# In another project
npm install path/to/smartworkz-sdk-1.0.0.tgz
```

#### Publishing to npm

```bash
# Login to npm account
npm login

# Publish to public registry
npm publish --access public
```

Users can then install with:

```bash
npm install @smartworkz/sdk
```

## Directory Structure

```
smartworkz-sdk-js/
├── src/                           # TypeScript source
│   ├── index.ts                   # Main exports
│   ├── client.ts                  # SmartWorkzClient
│   ├── config.ts                  # Configuration types
│   ├── auth/                      # Authentication
│   │   └── authenticator.ts
│   ├── endpoints/                 # API endpoints
│   │   ├── users.ts
│   │   ├── transactions.ts
│   │   ├── products.ts
│   │   ├── reports.ts
│   │   └── webhooks.ts
│   └── types/                     # Type definitions
│       ├── user.ts
│       ├── transaction.ts
│       ├── product.ts
│       └── report.ts
├── dist/                          # Compiled output (generated)
├── tests/                         # Test files
├── examples/                      # Example code
├── package.json                   # npm configuration
├── tsconfig.json                  # TypeScript config
├── jest.config.js                 # Jest config
├── README.md                      # User documentation
├── DEVELOPMENT.md                 # Development guide
└── INSTALLATION_GUIDE.md          # This file
```

## Troubleshooting

### Node.js Not Found

```
Command not found: node
```

**Solution**: Install Node.js from https://nodejs.org/

### npm Modules Error

```
npm ERR! code ERESOLVE
npm ERR! ERESOLVE unable to resolve dependency tree
```

**Solution**: Clear npm cache and reinstall

```bash
npm cache clean --force
rm -rf node_modules package-lock.json
npm install
```

### TypeScript Compilation Error

```
src/client.ts:10:5 - error TS2339: Property 'users' does not exist
```

**Solution**: Rebuild TypeScript

```bash
rm -rf dist
npm run build
```

### Port Already in Use (Examples)

If running examples that use a port:

```bash
# Kill process on port
lsof -ti:3000 | xargs kill -9  # macOS/Linux
```

## Environment Variables

Set environment variables for examples:

```bash
# API Key authentication
export SMARTWORKZ_API_KEY=your-api-key

# Bearer token authentication
export SMARTWORKZ_TOKEN=your-jwt-token

# Custom API URL
export SMARTWORKZ_API_URL=https://api.smartworkz.com
```

### PowerShell

```powershell
$env:SMARTWORKZ_API_KEY = "your-api-key"
$env:SMARTWORKZ_TOKEN = "your-jwt-token"
$env:SMARTWORKZ_API_URL = "https://api.smartworkz.com"
```

## Support & Resources

| Resource | Link |
|----------|------|
| Documentation | [README.md](./README.md) |
| Development | [DEVELOPMENT.md](./DEVELOPMENT.md) |
| Changelog | [CHANGELOG.md](./CHANGELOG.md) |
| Examples | [examples/](./examples/) |
| TypeScript Docs | https://www.typescriptlang.org/docs/ |
| axios Docs | https://axios-http.com/docs/intro |
| npm Docs | https://docs.npmjs.com/ |

## Quick Reference

```bash
# Install dependencies
npm install

# Build TypeScript
npm run build

# Run tests
npm test

# Create package
npm pack

# Publish to npm
npm publish

# Watch mode (auto-rebuild)
npx tsc --watch

# Type checking only
npx tsc --noEmit
```

## Next Steps

1. Read [README.md](./README.md) for API documentation
2. Check [DEVELOPMENT.md](./DEVELOPMENT.md) for contributing guidelines
3. Review [examples/](./examples/) for usage patterns
4. Run tests: `npm test`
5. Build package: `npm pack`

---

For questions or issues, please refer to the [GitHub Issues](https://github.com/S2Sys/SmartWorkz/issues) or [DEVELOPMENT.md](./DEVELOPMENT.md#support).
