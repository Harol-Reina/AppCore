# Análisis de Dependencias - AppCore

## Fecha: Enero 24, 2026
## Fase 1: Preparación y Análisis

## 1. Dependencias Externas de AppCore

### Dependencias Principales (.csproj)
- **Serilog.AspNetCore** 10.0.0 - Logging
- **Serilog.Sinks.Console** 6.1.1 - Console logging output
- **Serilog.Settings.Configuration** 10.0.0 - Serilog configuration
- **Swashbuckle.AspNetCore** 10.1.0 - API documentation
- **Microsoft.Extensions.Logging.Abstractions** 10.0.2 - Core logging abstractions
- **Microsoft.EntityFrameworkCore.Abstractions** 10.0.2 - ORM abstractions
- **AutoMapper.Extensions.Microsoft.DependencyInjection** 12.0.1 - Object mapping
- **FluentValidation.DependencyInjectionExtensions** 12.1.1 - Validation framework
- **MediatR** 14.0.0 - CQRS/mediator pattern
- **Microsoft.AspNetCore.Authentication.JwtBearer** 10.0.2 - JWT authentication
- **Microsoft.EntityFrameworkCore** 10.0.2 - ORM framework
- **Npgsql.EntityFrameworkCore.PostgreSQL** 10.0.0 - PostgreSQL provider

### Categorización de Dependencias

#### 🟢 Core/Essential (mantendrán como están)
- Microsoft.Extensions.Logging.Abstractions
- Microsoft.EntityFrameworkCore.Abstractions
- Microsoft.EntityFrameworkCore
- MediatR

#### 🟡 Framework/Infrastructure (revisar necesidad)
- Microsoft.AspNetCore.Authentication.JwtBearer (¿necesario en biblioteca base?)
- Swashbuckle.AspNetCore (¿necesario en biblioteca base?)
- Npgsql.EntityFrameworkCore.PostgreSQL (específico de PostgreSQL)

#### 🟠 Utilities (evaluar si son realmente necesarias)
- AutoMapper.Extensions.Microsoft.DependencyInjection
- FluentValidation.DependencyInjectionExtensions
- Serilog packages (¿abstraer logging?)

## 2. APIs Públicas Identificadas

### Application Layer - Interfaces Públicas
```
├── ICurrentUserService - Gestión de usuario actual
├── IDateTimeService - Abstracción de tiempo
└── IEndpointGroupBase - Base para endpoints
```

### Domain Layer - Interfaces Públicas
```
├── IGenericRepository<E,I> - Repositorio genérico
└── IHttpRequestRepository - Cliente HTTP
```

### Application Layer - DTOs y Wrappers
```
├── DTOs/
│   ├── EmailRequest
│   ├── LoginRequest
│   ├── LoginResponse
│   ├── MetaInfo
│   ├── PaginationDto<T>
│   └── PaginationResponse
└── Wrappers/
    └── Response<T> (probable ResponseWrapper)
```

### Application Layer - Exceptions
```
├── ApiDBException
├── ApiHttpException
├── AuthenticationException
├── BadRequestException
├── CustomException
├── ForbiddenAccessException
├── HttpBaseException
├── MappingException
├── NotFoundException
├── OperationException
├── SerializerException
└── ValidationException
```

## 3. Estructura Interna vs Pública

### 🔴 APIs que deben ser INTERNAS
- Behaviours/ (MediatR behaviors específicos)
- Infrastructure/ específicos (DAOs, Converters)
- Middleware/ específicos
- Utils/ internos

### 🟢 APIs que deben ser PÚBLICAS
- Interfaces principales (IGenericRepository, ICurrentUserService, etc.)
- DTOs de entrada/salida
- Exceptions públicas
- Extensions de configuración
- Wrappers de respuesta

## 4. Proyectos de Testing Actuales

### AppCore.UnitTests
- **Framework**: xUnit 2.9.3
- **Mocking**: Moq 4.20.72, AutoFixture
- **Assertions**: FluentAssertions 8.8.0
- **Coverage**: coverlet.collector 6.0.4
- **Status**: ✅ Configurado adecuadamente

### AppCore.SpecFlow
- **Framework**: SpecFlow 3.9.74 con xUnit
- **Status**: ⚠️ Básico configurado, necesita features específicas
- **Feature actual**: GenericRepository.feature (basic)

## 5. Posibles Breaking Changes Identificados

### Cambios en Visibilidad
- Marcar clases DAO como `internal`
- Marcar behaviors específicos como `internal`
- Marcar converters internos como `internal`

### Cambios en Interfaces
- IGenericRepository puede necesitar simplificación
- Abstracción de dependencias específicas de PostgreSQL

### Cambios en Dependencias
- Mover Swashbuckle a package opcional
- Abstraer Serilog detrás de ILogger
- Evaluar si JWT debe ser parte del core

## 6. Recomendaciones para NuGet Package

### Dependencias Mínimas Sugeridas
```xml
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Abstractions" Version="10.0.2" />
<PackageReference Include="MediatR" Version="14.0.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="12.1.1" />
```

### Dependencias Opcionales (via feature packages)
```xml
<!-- AppCore.EntityFramework -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.2" />

<!-- AppCore.Serilog -->
<PackageReference Include="Serilog.AspNetCore" Version="10.0.0" />

<!-- AppCore.Swagger -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.0" />
```

## 7. Plan de Matriz de Compatibilidad

### Framework Targets
- .NET 10.0 (actual)
- .NET 9.0 (considerar para compatibilidad)
- .NET 8.0 LTS (considerar para compatibilidad)

### Niveles de Compatibilidad
- **Source Compatible**: API signatures no cambian
- **Binary Compatible**: Assemblies existentes siguen funcionando
- **Behavior Compatible**: Funcionalidad equivalente

---
*Documento generado automáticamente durante Fase 1 de restructuración*