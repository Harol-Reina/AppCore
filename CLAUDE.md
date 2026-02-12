# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AppCore is a .NET 10 Clean Architecture foundation library (NuGet package: `OrionSoft.AppCore`) providing common patterns, abstractions, and utilities for .NET applications. It implements CQRS (via MediatR), DDD patterns, and is fully NativeAOT compatible.

## Build & Test Commands

```bash
# Restore, build, and test (full cycle)
dotnet restore
dotnet build
dotnet test

# Run only unit tests
dotnet test tests/AppCore.UnitTests

# Run only SpecFlow/BDD tests
dotnet test tests/AppCore.SpecFlow

# Run a single test by name
dotnet test tests/AppCore.UnitTests --filter "FullyQualifiedName~ClassName.MethodName"

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage" --settings "./build/coverage/coverage.runsettings"

# Build the CleanArchitectureSample
dotnet build samples/CleanArchitectureSample/App.sln

# NativeAOT dry-run validation
dotnet publish src/AppCore/AppCore.csproj -c Release -r linux-x64 /p:PublishAot=true --no-restore
```

## Architecture

The library follows **Clean Architecture** with three layers, all under `src/AppCore/`:

- **Domain/** — Base entities (`BaseEntity<T>`, `AuditableEntity`), repository interfaces (`IGenericRepository<E,I>`), domain enums
- **Application/** — DTOs, MediatR pipeline behaviors (validation, exception handling), service interfaces (`ICurrentUserService`, `IDateTimeService`, `IDbConnectionFactory`), response wrappers (`Response<T>`, `PaginationResponse`), custom exception hierarchy, JWT settings, JSON serialization context for AOT
- **Infrastructure/** — Service implementations (`CurrentUserService`, `DateTimeService`, `HttpService`, `MappingServiceBase`), DAO interfaces (`IBaseDao`, `IAuditableBaseDao`), auditable extensions

Service registration entry point: `DependencyInjection.AddCoreApplication()` — uses explicit registration (no assembly scanning) for AOT compatibility.

## Key Conventions

- **C# 14** with nullable reference types enabled, implicit usings
- **TreatWarningsAsErrors** is enabled — all warnings must be resolved
- **NativeAOT compatibility is mandatory**: no reflection-based registration, use `AppCoreJsonContext` (System.Text.Json source generators) for serialization, `JsonSerializerIsReflectionEnabledByDefault=false`
- **Conventional Commits** for commit messages (`feat:`, `fix:`, `docs:`, `test:`, `refactor:`)
- **Code style**: PascalCase for public members, `_camelCase` for private fields, K&R braces (opening brace on same line)
- **Testing**: xUnit + FluentAssertions + Moq + AutoFixture; AAA pattern; minimum 80% code coverage
- **Commit policy**: AI must never auto-commit. Propose commits with summary and wait for explicit user approval.

## Key Dependencies

- MediatR 14.0 (CQRS pipeline)
- FluentValidation 12.1 (request validation)
- Dapper 2.1 (data access)
- Serilog 10.0 (structured logging)
- JWT Bearer Authentication (Microsoft.AspNetCore.Authentication.JwtBearer)

## Versioning

Automatic semantic versioning via MinVer from git tags (prefix `v`, e.g., `v1.0.0`). Configuration in `minver.yaml`.

## Solution Structure

- `AppCore.sln` — main solution (library + tests)
- `samples/CleanArchitectureSample/App.sln` — reference implementation showing practical usage
- `samples/AotTestApp/` — NativeAOT validation suite (run via `build/scripts/run-aot-tests.sh`)
- `build/` — MSBuild props, targets, coverage settings, and scripts
