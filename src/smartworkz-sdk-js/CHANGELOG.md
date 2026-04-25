# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-04-24

### Added

- Initial release of SmartWorkz SDK for Node.js
- Full TypeScript support with strict type checking
- SmartWorkzClient main class for API interaction
- Users endpoint with full CRUD operations
- Transactions endpoint with filtering options
- Products endpoint with active/inactive filtering
- Reports endpoint with async report generation
- Webhooks endpoint with event management
- Authentication support for API key and Bearer token
- Comprehensive type definitions for all resources
- JSDoc documentation for all public APIs
- Example code in JavaScript, TypeScript, and async/await patterns
- Jest test framework with basic unit tests
- Build scripts for npm package creation
- Development guide for contributing
- MIT License

### Features

#### Users Endpoint (`client.users`)
- `list()` - List users with pagination
- `get(id)` - Get user by ID
- `create(request)` - Create new user
- `update(id, request)` - Update user
- `delete(id)` - Delete user

#### Transactions Endpoint (`client.transactions`)
- `list()` - List transactions with filtering
- `get(id)` - Get transaction by ID
- `create(request)` - Create transaction
- `update(id, request)` - Update transaction status
- `delete(id)` - Delete transaction

#### Products Endpoint (`client.products`)
- `list()` - List products with filtering
- `get(id)` - Get product by ID
- `create(request)` - Create product
- `update(id, request)` - Update product
- `delete(id)` - Delete product

#### Reports Endpoint (`client.reports`)
- `list()` - List reports with filtering
- `get(id)` - Get report by ID
- `create(request)` - Create async report
- `cancel(id)` - Cancel report generation

#### Webhooks Endpoint (`client.webhooks`)
- `list()` - List webhooks
- `get(id)` - Get webhook by ID
- `create(request)` - Create webhook
- `update(id, request)` - Update webhook
- `delete(id)` - Delete webhook
- `test(id)` - Send test event to webhook

### Documentation

- README.md with quick start and API reference
- DEVELOPMENT.md with development guidelines
- JSDoc comments for all classes and methods
- 3 example files covering different use cases

---

## Unreleased

### Planned

- Request/response interceptors
- Retry logic with exponential backoff
- Rate limiting support
- Advanced error handling
- Performance metrics
- Request caching
- Batch operations
- Stream support for large data
- WebSocket support for real-time updates

---

## Version History

| Version | Date | Status |
|---------|------|--------|
| 1.0.0 | 2026-04-24 | Released |

---

## Guidelines for Version Updates

### Patch Version (X.Y.Z)
- Bug fixes
- Documentation updates
- Minor improvements

### Minor Version (X.Y.Z)
- New features (backward compatible)
- New endpoints
- Enhanced existing endpoints

### Major Version (X.Y.Z)
- Breaking changes
- Major refactoring
- Removed features

---

For more information, see the [README.md](./README.md) and [DEVELOPMENT.md](./DEVELOPMENT.md).
