# SmartWorkz.StarterKitMVC – Coding Standards & Quality Rules

## C# Naming Conventions
- **PascalCase** for public members, types, namespaces.
- **camelCase** for private fields (prefix with `_`), parameters, local variables.
- **I** prefix for interfaces (e.g., `ISettingsService`).

## Architecture Rules
- **Domain** has no dependencies on Infrastructure or Web.
- **Application** depends only on Domain and Shared.
- **Infrastructure** implements Application contracts.
- **Web** is the composition root; wires DI and middleware.

## Clean Code Guidelines
- Keep methods short (<30 lines).
- One class per file.
- Favor composition over inheritance.
- Use dependency injection everywhere.

## Async Patterns
- Suffix async methods with `Async`.
- Always pass `CancellationToken` where applicable.
- Avoid `.Result` or `.Wait()` on tasks.

## Performance Rules
- Use `IReadOnlyCollection<T>` for return types when mutation is not needed.
- Prefer `ValueTask` for hot paths that often complete synchronously.
- Cache expensive computations.

## Security Rules (OWASP)
- Validate all user input.
- Use parameterized queries (no string concatenation for SQL).
- Store secrets in environment variables or secret managers.
- Use HTTPS everywhere.

## Do's & Don'ts
- **Do** write unit tests for all business logic.
- **Do** use structured logging with correlation IDs.
- **Don't** catch generic `Exception` without re-throwing or logging.
- **Don't** hardcode configuration values.
