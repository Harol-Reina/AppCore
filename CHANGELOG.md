# Changelog

All notable changes to the AppCore project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial project setup for standalone AppCore package
- Comprehensive documentation structure
- BDD test framework setup with SpecFlow
- GitHub workflows configuration

## [2.0.0] - TBD

### Added
- Clean Architecture foundation with Domain, Application, and Infrastructure layers
- Generic Repository pattern with `IGenericRepository<E, I>`
- Response wrapper pattern with `Response<T>`
- Comprehensive exception hierarchy based on `CustomException`
- User context management with `ICurrentUserService`
- Time abstraction with `IDateTimeService`
- Pagination support with `PaginationDto<T>`
- FluentValidation integration with MediatR behaviors
- Entity Framework Core abstractions and implementations
- Serilog integration for structured logging
- JWT Bearer authentication support
- Comprehensive unit testing with xUnit, FluentAssertions, and Moq
- BDD specifications with SpecFlow
- AutoMapper integration for object mapping

### Security
- JWT token validation and user context extraction
- Input validation with FluentValidation
- Exception handling middleware for security

### Documentation
- Complete API surface documentation
- Implementation plan and migration guides
- Compatibility matrix for version upgrades
- Contributing guidelines and code of conduct

## [1.0.0] - Legacy

### Note
This changelog starts from version 2.0.0 as part of the AppCore standalone package initiative. 
Previous versions were part of larger application solutions and not distributed as independent packages.

---

## Legend

- `Added` for new features
- `Changed` for changes in existing functionality
- `Deprecated` for soon-to-be removed features
- `Removed` for now removed features
- `Fixed` for any bug fixes
- `Security` for security-related changes