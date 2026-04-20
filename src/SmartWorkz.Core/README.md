# SmartWorkz.Core

Core domain models, entities, and business logic library providing the foundation for SmartWorkz applications.

## Getting Started

### Prerequisites
- .NET 9.0 or higher
- Visual Studio 2022+ or VS Code

### Installation

Add project reference in your application:

```xml
<ProjectReference Include="path/to/SmartWorkz.Core/SmartWorkz.Core.csproj" />
```

### Basic Usage

```csharp
using SmartWorkz.Core.Entities;
using SmartWorkz.Core.Services;
using SmartWorkz.Core.Results;

// Access domain models and services
var result = Result<T>.Ok(data);
```

## Project Structure

- **Abstractions/** — Interfaces and contracts for services and repositories
- **Entities/** — Domain entities and value objects
- **Enums/** — Domain enumerations
- **DTOs/** — Data transfer objects
- **Services/** — Business logic and service implementations
- **Extensions/** — Helper extension methods
- **Validators/** — Data validation logic
- **Results/** — Result<T> wrapper for operation outcomes
- **Constants/** — Application-wide constants
- **Models/** — View and request models

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.Extensions.* | 9.0.0 | Dependency injection, logging, configuration |

## Configuration

No special configuration required. This is a class library consumed by other applications.

### Usage Pattern

```csharp
public class MyService
{
    public Result<UserDto> GetUser(int id)
    {
        if (id <= 0)
            return Result<UserDto>.Fail("InvalidId", "User ID must be positive");
        
        return Result<UserDto>.Ok(new UserDto { Id = id });
    }
}
```

## Features

- **Domain-Driven Design** — Rich domain models and entities
- **Result Pattern** — Functional error handling with `Result<T>`
- **Service Abstractions** — Clean dependency injection interfaces
- **Validation Support** — Built-in validation framework integration
- **Extension Methods** — Utility extensions for common operations

## Testing

Include this project in test projects:

```xml
<ProjectReference Include="path/to/SmartWorkz.Core/SmartWorkz.Core.csproj" />
```

## Contributing

Follow project conventions for entity naming, service abstractions, and result handling.
