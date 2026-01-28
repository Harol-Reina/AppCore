# AppCore - Clean Architecture Foundation Library

[![Build Status](https://github.com/Harol-Reina/AppCore/workflows/CI/badge.svg)](https://github.com/Harol-Reina/AppCore/actions)
[![NuGet Version](https://img.shields.io/nuget/v/AppCore.svg)](https://www.nuget.org/packages/AppCore)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AppCore.svg)](https://www.nuget.org/packages/AppCore)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

AppCore is a comprehensive Clean Architecture foundation library that provides common patterns, utilities, and abstractions for .NET applications. It implements the principles of Clean Architecture, CQRS, and Domain-Driven Design (DDD) to help developers build maintainable and scalable applications.

## 🚀 Quick Start

### Installation

```bash
dotnet add package AppCore
```

### Basic Usage

```csharp
// Configure services
services.AddAppCore();

// Use generic repository
public class UserService
{
    private readonly IGenericRepository<User, int> _repository;
    
    public UserService(IGenericRepository<User, int> repository)
    {
        _repository = repository;
    }
    
    public async Task<Response<User>> GetUserAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        return Response<User>.Success(user);
    }
}
```

## 📦 Features

### Core Abstractions
- **Generic Repository Pattern**: `IGenericRepository<E, I>` with full CRUD operations
- **Response Wrappers**: Standardized `Response<T>` for consistent API responses
- **Base Entities**: `BaseEntity<I>` and `AuditableEntity` for domain models
- **Exception Handling**: Comprehensive exception hierarchy with `CustomException` base

### Application Layer
- **User Context**: `ICurrentUserService` for user information management
- **Time Abstraction**: `IDateTimeService` for testable time operations
- **Validation**: FluentValidation integration with MediatR behaviors
- **Pagination**: Built-in `PaginationDto<T>` support

### Infrastructure Support
- **Entity Framework**: Full EF Core integration and abstractions
- **Logging**: Structured logging with Serilog integration
- **Authentication**: JWT Bearer token support
- **Dependency Injection**: Comprehensive service registration extensions

## 🏗️ Architecture

AppCore follows Clean Architecture principles:

```
┌─── Presentation Layer ───────────────────┐
│  Controllers, Endpoints, Middleware      │
├─── Application Layer ───────────────────┤
│  ├── Services & Use Cases               │
│  ├── DTOs & Commands/Queries            │
│  ├── Behaviors & Validation             │
│  └── Interfaces & Abstractions          │
├─── Domain Layer ────────────────────────┤
│  ├── Entities & Value Objects           │
│  ├── Domain Services                    │
│  ├── Repository Interfaces              │
│  └── Domain Events                      │
└─── Infrastructure Layer ────────────────┘
   ├── Data Access & Repositories
   ├── External Services
   ├── File System & I/O
   └── Third-party Integrations
```

## 📖 Documentation

- [API Documentation](docs/API-Reference.md)
- [Implementation Plan](docs/technical/Implementation-Plan.md)
- [Migration Guide (EF->Dapper)](docs/guides/Migration-EF-to-Dapper.md)
- [Compatibility Matrix](docs/technical/Compatibility-Matrix.md)

## 🔧 Advanced Configuration

### Granular Service Registration

```csharp
// Core services only
services.AddAppCoreCore();

// With specific providers
services.AddAppCorePostgreSQL();
services.AddAppCoreSerilog();

// Or all-in-one (deprecated but supported)
services.AddAppCore();
```

### Custom Repository Implementation

```csharp
public class UserRepository : IGenericRepository<User, int>
{
    // Your custom implementation
}

// Register in DI
services.AddScoped<IGenericRepository<User, int>, UserRepository>();
```

## 🧪 Testing

AppCore is designed with testing in mind:

```csharp
[Fact]
public async Task Should_Get_User_By_Id()
{
    // Arrange
    var repository = new Mock<IGenericRepository<User, int>>();
    var service = new UserService(repository.Object);
    
    // Act
    var result = await service.GetUserAsync(1);
    
    // Assert
    result.Should().NotBeNull();
}
```

### BDD Support

AppCore includes SpecFlow specifications for behavior-driven development:

```gherkin
Feature: Generic Repository
  As a developer
  I want to use a generic repository
  So that I can perform CRUD operations consistently

Scenario: Get entity by ID
  Given I have a repository for User entities
  When I request a user with ID 1
  Then I should receive the user data
```

## 📋 Requirements

- **.NET 10.0** or later
- **Entity Framework Core 10.0** or later
- **MediatR 14.0** or later

### Optional Dependencies

- **PostgreSQL**: Install `Npgsql.EntityFrameworkCore.PostgreSQL`
- **Serilog**: Install Serilog packages for structured logging
- **Swagger**: Install `Swashbuckle.AspNetCore` for API documentation

## 🔄 Migration from v1.x

If you're upgrading from AppCore v1.x, see our [Migration Guide](docs/Migration-Guide.md) for detailed instructions.

### Key Changes in v2.0
- Internal APIs are no longer public
- Simplified dependency structure
- Better package separation
- Enhanced testing support

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

```bash
# Clone the repository
git clone https://github.com/Harol-Reina/AppCore.git

# Build the solution
dotnet build

# Run tests
dotnet test

# Run BDD specifications
dotnet test tests/AppCore.SpecFlow
```

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙋‍♂️ Support

- **Issues**: [GitHub Issues](https://github.com/Harol-Reina/AppCore/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Harol-Reina/AppCore/discussions)
- **Email**: harol.reina@example.com

## 📈 Roadmap

### v2.1 (Q2 2026)
- [ ] .NET 11 support
- [ ] Additional database providers
- [ ] Caching abstractions
- [ ] Event sourcing support

### v2.2 (Q3 2026)
- [ ] GraphQL integration
- [ ] OpenTelemetry support
- [ ] Cloud-native patterns
- [ ] Performance optimizations

---

**Made with ❤️ by [Harol A. Reina](https://github.com/Harol-Reina)**
