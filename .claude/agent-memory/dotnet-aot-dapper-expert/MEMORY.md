# AppCore-Standalone - Agent Memory

## Project Overview
- Library: OrionSoft.AppCore (NuGet package name)
- Namespace root: `AppCore` (mismatch with assembly name `OrionSoft.AppCore`)
- Target: net10.0 with NativeAOT compatibility declared
- Main project: `src/AppCore/AppCore.csproj`
- Tests: `tests/AppCore.UnitTests/` (xUnit, Moq, FluentAssertions) and `tests/AppCore.SpecFlow/`
- CI/CD: GitHub Actions with JIT/AOT matrix, 80% coverage gate

## Key AOT Issues (from 2026-02-12 audit)
- Dapper 2.1.66 (classic, Emit-based) referenced but unused -- should be removed
- Swashbuckle.AspNetCore referenced -- not AOT-compatible
- `JsonExtend.ToJsonDocument(object?)` uses `value.GetType()` at runtime -- AOT unsafe
- `HttpService` methods use `object? body` losing static type info for serialization
- `rd.xml` uses legacy `<linker>` format, not ILC `<Directives>` format
- No `sealed` keyword used anywhere in the library
- No `ConfigureAwait(false)` in any async method
- `MessageLog.Message` and `CustomErrorResponse.Error` are typed as `object` -- AOT serialization issue

## Architecture Notes
- Clean Architecture: Domain/Application/Infrastructure layers
- Domain layer violates dependency rule: `HttpAuditEntity` depends on `AppCore.Application.Extensions`
- `Configuration` class uses static mutable state (anti-pattern)
- `Response<T>` has no `Succeeded` property -- cannot distinguish success from failure
- `PaginationDto<E>` and `PaginationResponse<T>` are duplicate concepts

## Naming Issues in Public API
- `PakageLeadType` (should be `PackageLeadType`)
- `EndPoinds` in MetaInfo (should be `Endpoints`)
- `HttpClientCustomHandler` is actually an ASP.NET middleware, not an HttpClient handler
- Spanish property names in public types: `Tipo`, `Metodo`, `mensaje`

## File/Namespace Details
- 36 source files (excluding generated)
- File `HttpRequestEntity.cs` contains class `HttpAuditEntity`
- `JsonTestModel` is in production code (should be in tests)
- Coding style: K&R brackets (Egyptian), 4-space indent

## Audit Report
- Full audit saved to: `docs/AUDIT.md` (2026-02-12)
