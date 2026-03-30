# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Localization infrastructure with resource keys and constants
- Authorization system with claims, permissions, and roles management
- Admin dashboard with comprehensive UI components
- Email templates and notification system
- Multi-language support with resource files
- Permission middleware for access control
- Calendar functionality for admin users
- Settings management system
- Wiki system for documentation
- List of Values (LOV) management for categories and items
- Claims management for identity and authorization
- Resource management for localization
- Email template management system
- Notification template management

### Enhanced
- Admin UI with responsive design and modern components
- Security infrastructure with role-based access control
- Database schema for authorization and localization
- Middleware pipeline for permission checking
- View localization with integrated resource service

### Fixed
- Authentication flow improvements
- Permission validation edge cases
- Resource loading optimization

## [1.0.0] - 2025-01-03

### Added
- Initial project setup with Clean Architecture
- ASP.NET Core MVC framework integration
- Bootstrap 5.3.3 UI framework
- Basic authentication with ASP.NET Identity
- Multi-tenancy support
- Caching infrastructure (Memory, Redis)
- Background job processing (InMemory, Hangfire, Quartz)
- Event bus system (InMemory, RabbitMQ, Azure, Kafka)
- Email notifications (SMTP, SendGrid)
- File storage (Local, Azure Blob, S3)
- Swagger/OpenAPI documentation
- Health checks and monitoring
- Rate limiting protection
- Docker and Kubernetes support
- Comprehensive testing setup (Unit, Integration)
- Project rename scripts and tooling

### Features
- **Architecture**: Clean Architecture with Domain, Application, Infrastructure, Shared, and Web layers
- **Authentication**: JWT, OAuth (Google, Microsoft, GitHub, Facebook), Two-Factor Auth
- **Multi-Tenancy**: Subdomain, Header, Query parameter strategies
- **UI/UX**: Dark/Light mode, responsive design, admin dashboard
- **Developer Experience**: XML documentation, health checks, feature toggles

### Infrastructure
- **Database**: Entity Framework Core with SQL Server support
- **Caching**: Memory cache with Redis fallback
- **Logging**: Serilog with multiple sinks (Seq, Application Insights, ElasticSearch)
- **Monitoring**: Health checks, performance counters
- **Deployment**: Docker containers, Kubernetes manifests

---

## Version History

| Version | Date | Notes |
|---------|------|-------|
| 1.0.0 | 2025-01-03 | Initial release with core MVC framework and infrastructure |
| 1.1.0 | TBD | Authorization and localization enhancements |

---

## Breaking Changes

### v1.1.0 (Unreleased)
- None planned yet

### v1.0.0
- Initial release - no breaking changes

---

## Deprecations

### v1.1.0 (Unreleased)
- None planned yet

---

## Security Updates

### v1.1.0 (Unreleased)
- Enhanced permission validation
- Improved claim-based authorization
- Security middleware updates

### v1.0.0
- Initial security implementation with ASP.NET Identity
- JWT token validation
- OAuth provider integration

---

## Performance Improvements

### v1.1.0 (Unreleased)
- Optimized resource loading
- Improved caching strategies
- Database query optimization

### v1.0.0
- Implemented caching layer
- Optimized static asset delivery
- Database connection pooling

---

## Documentation Updates

### v1.1.0 (Unreleased)
- Added authorization guide
- Localization documentation
- Admin panel user guide

### v1.0.0
- Initial README with setup instructions
- API documentation via Swagger
- Architecture overview
