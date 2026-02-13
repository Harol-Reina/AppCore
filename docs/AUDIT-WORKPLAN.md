# Plan de Trabajo - Auditoría AppCore: Hallazgos Pendientes

> **Objetivo**: Completar el 100% de los hallazgos de `docs/AUDIT.md`
> **Estado actual**: 25/33 hallazgos accionables resueltos (76%)
> **Meta**: 33/33 (100%)
> **Fecha**: Febrero 2026

---

## Resumen de Hallazgos Pendientes

| # | ID | Severidad | Descripción | Archivos impactados |
|---|-----|-----------|-------------|---------------------|
| 1 | A-02 | Alto | Namespace `AppCore` no coincide con AssemblyName `OrionSoft.AppCore` | ~120 archivos |
| 2 | M-02 | Medio | `DynamicProxyGenAssembly2` InternalsVisibleTo en producción | 1 archivo |
| 3 | M-04 | Medio | `MessageLog.Message` tipo `object` (no AOT-safe) | ~15 archivos |
| 4 | M-05 | Medio | `CustomErrorResponse(object Error)` (no AOT-safe) | 2 archivos |
| 5 | B-02 | Bajo | `CreatedAt` nullability inconsistente entre DAO e Entity | 2 archivos |
| 6 | B-03 | Bajo | Acoplamiento directo con `Serilog.Context.LogContext` | 1 archivo + csproj |
| 7 | B-05 | Bajo | `nuget.config` fuente GitHub sin credenciales/documentación | 1 archivo |
| 8 | B-08 | Bajo | `CleanArchitectureSample` sin documentación | Archivo nuevo |

---

## Fase 1 — Cambios Simples y de Bajo Riesgo

> Cambios aislados que no afectan API pública ni requieren migración.

### 1.1 — B-02: Unificar nullability de `CreatedAt`

**Problema**: `IAuditableBaseDao.CreatedAt` es `DateTime` (non-nullable) pero `AuditableEntity.CreatedAt` es `DateTime?` (nullable). Esto genera inconsistencia al mapear entre capas.

**Solución**: Cambiar `IAuditableBaseDao.CreatedAt` a `DateTime?` para alinear con la entidad de dominio.

**Archivos a modificar**:
| Archivo | Cambio |
|---------|--------|
| `src/AppCore/Infrastructure/Data/DAOs/Common/IAuditableBaseDao.cs:5` | `DateTime` → `DateTime?` |

**Tests a actualizar**: Verificar `AuditableExtensionsTests.cs` y `BaseDaoTests.cs`.

**Riesgo**: Bajo — solo afecta implementaciones de la interfaz DAO.

---

### 1.2 — B-05: Documentar o limpiar `nuget.config`

**Problema**: La fuente GitHub Packages (`https://nuget.pkg.github.com/Harol-Reina/index.json`) requiere autenticación pero no hay configuración de credenciales ni documentación de setup.

**Solución**: Agregar sección de credenciales con variables de entorno.

**Archivo a modificar**:
```xml
<!-- nuget.config -->
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="github" value="https://nuget.pkg.github.com/Harol-Reina/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="%GITHUB_USERNAME%" />
      <add key="ClearTextPassword" value="%GITHUB_TOKEN%" />
    </github>
  </packageSourceCredentials>
</configuration>
```

**Riesgo**: Ninguno — solo agrega configuración opcional.

---

### 1.3 — M-02: Documentar `DynamicProxyGenAssembly2`

**Problema**: `InternalsVisibleTo("DynamicProxyGenAssembly2")` en `AppCore.csproj` es necesario para Moq (usado en UnitTests y SpecFlow) pero no debería estar en producción idealmente.

**Solución**: Este atributo es requerido mientras se use Moq. Documentar con comentario explicativo y considerar migración futura.

**Archivo a modificar**:
| Archivo | Cambio |
|---------|--------|
| `src/AppCore/AppCore.csproj:29-32` | Mejorar comentario XML y agregar nota de migración futura |

**Acción futura recomendada**: Migrar de Moq a NSubstitute (AOT-friendly) en un sprint dedicado, lo cual permitiría eliminar este `InternalsVisibleTo`.

**Riesgo**: Ninguno — solo documentación.

---

## Fase 2 — Cambios de Diseño (API interna)

> Cambios que modifican tipos internos para mejorar compatibilidad AOT.

### 2.1 — M-04 + M-05: Eliminar `object` en MessageLog y CustomErrorResponse

**Problema**:
- `MessageLog.Message` es tipo `object` (línea 25 de `MessageLog.cs`)
- `CustomErrorResponse.Error` es tipo `object` (línea 27 de `HttpResponse.cs`)
- `DictionaryError.Exception` es tipo `object?` (línea 87 de `CustomException.cs`)

En NativeAOT, `System.Text.Json` no puede serializar `object` correctamente — produce `{}` o pérdida de datos porque no tiene metadata de tipo en runtime.

**Solución propuesta**: Usar tipos concretos que `System.Text.Json` pueda resolver en compile-time.

#### 2.1.1 — `MessageLog.Message`: `object` → `JsonElement`

`MessageLog.Message` recibe dos tipos de valores:
- `DictionaryError` (desde `CustomException`)
- `string` (desde algunas excepciones directas)

**Estrategia**: Pre-serializar a `JsonElement` en el punto de creación.

```csharp
// MessageLog.cs
public required JsonElement Message { get; init; }
```

**Archivos a modificar**:
| Archivo | Cambio |
|---------|--------|
| `src/AppCore/Application/Wrappers/MessageLog.cs:25` | `object` → `JsonElement` |
| `src/AppCore/Application/Exceptions/CustomException.cs:34` | Pre-serializar `DictionaryError` a `JsonElement` |
| `src/AppCore/Application/Exceptions/ApiHttpException.cs` | Adaptar asignación de Message |
| `src/AppCore/Application/Exceptions/ApiDBException.cs` | Adaptar asignación de Message |
| `src/AppCore/Application/Exceptions/SerializerException.cs` | Adaptar asignación de Message |
| `src/AppCore/Application/Exceptions/OperationException.cs` | Adaptar asignación de Message |
| `src/AppCore/Application/Exceptions/AuthenticationException.cs` | Adaptar asignación de Message |
| `src/AppCore/Application/Middleware/ExceptionHandlingMiddleware.cs:48,54-56` | Adaptar lectura de Message |
| `src/AppCore/Application/Behaviours/UnhandledExceptionBehaviour.cs` | Adaptar si usa Message |

**Tests a actualizar**:
- `MessageLogTests.cs`
- `CustomExceptionTests.cs`
- `ApiHttpExceptionTests.cs`
- `ExceptionHandlingMiddlewareTests.cs`
- `ExceptionsTests.cs`
- Todos los tests que construyen `MessageLog` directamente

**Helper necesario**: Método de extensión en `JsonExtend` para convertir valores a `JsonElement`:
```csharp
public static JsonElement ToJsonElement<T>(T value, JsonTypeInfo<T> typeInfo)
    => JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(value, typeInfo)).RootElement.Clone();
```

#### 2.1.2 — `CustomErrorResponse.Error`: `object` → `JsonElement`

Mismo patrón que `MessageLog.Message`.

```csharp
// HttpResponse.cs
public sealed record CustomErrorResponse(JsonElement Error);
```

**Archivos a modificar**:
| Archivo | Cambio |
|---------|--------|
| `src/AppCore/Application/Wrappers/HttpResponse.cs:27` | `object` → `JsonElement` |
| `src/AppCore/Application/Middleware/ExceptionHandlingMiddleware.cs:54-56` | Pre-serializar antes de construir |

#### 2.1.3 — `DictionaryError.Exception`: `object?` → `string?`

`DictionaryError.Exception` almacena detalles de excepción. Como es metadata de diagnóstico, `string?` es suficiente.

```csharp
// CustomException.cs - DictionaryError
public string? Exception { get; init; }
```

**Archivos a modificar**:
| Archivo | Cambio |
|---------|--------|
| `src/AppCore/Application/Exceptions/CustomException.cs:87,102` | `object?` → `string?` |
| Tests que construyen `DictionaryError` con `Exception` | Adaptar tipo |

**Riesgo**: Medio — cambio de API interna. Requiere actualizar todos los consumidores de estas propiedades. No hay impacto en API pública de HTTP (los responses ya se serializan a JSON string).

---

### 2.2 — B-03: Desacoplar Serilog del middleware

**Problema**: `ExceptionHandlingMiddleware.cs:71` usa directamente `Serilog.Context.LogContext.PushProperty()`, acoplando la capa Application a una implementación concreta de logging.

**Código actual**:
```csharp
using (Serilog.Context.LogContext.PushProperty("XTraceID", traceId)) {
```

**Solución**: Reemplazar con `ILogger.BeginScope()` que es provider-agnostic.

```csharp
// Inyectar ILogger via constructor
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger) {

    // Reemplazar Serilog directo
    using (logger.BeginScope(new Dictionary<string, object> { ["XTraceID"] = traceId })) {
```

**Archivos a modificar**:
| Archivo | Cambio |
|---------|--------|
| `src/AppCore/Application/Middleware/ExceptionHandlingMiddleware.cs:9,67-77` | Inyectar `ILogger`, reemplazar `Serilog.Context.LogContext` |

**Consideración sobre paquetes Serilog**: Los `PackageReference` de Serilog en `AppCore.csproj` (líneas 81-83) pueden permanecer si son usados en `DependencyInjection.cs` o configuración del host. Revisar si hay otros usos directos.

**Tests a actualizar**:
- `ExceptionHandlingMiddlewareTests.cs` — inyectar mock de `ILogger<ExceptionHandlingMiddleware>`

**Riesgo**: Bajo — `ILogger.BeginScope` es soportado por Serilog cuando se configura con `Serilog.Extensions.Logging`.

---

## Fase 3 — Cambio Mayor: Namespace Refactoring

> Cambio masivo que afecta todos los archivos del proyecto.

### 3.1 — A-02: Alinear namespace con AssemblyName

**Problema**:
- `AssemblyName` = `OrionSoft.AppCore`
- Todos los namespaces usan `AppCore.*`
- Esto viola la convención .NET de alinear namespace con AssemblyName

**Alcance del impacto**:
| Ubicación | Archivos con `namespace AppCore` | Archivos con `using AppCore` |
|-----------|----------------------------------|------------------------------|
| `src/AppCore/` | 50 | (internos) |
| `tests/` | 60 | ~80 |
| `samples/` | ~10 | ~30 |
| **Total** | **~120** | **~216 ocurrencias en 121 archivos** |

**Opciones**:

#### Opción A — Renombrar namespaces a `OrionSoft.AppCore.*` (Recomendada)

Alinea todo con el AssemblyName oficial. Es el approach correcto pero es un **breaking change** para consumidores existentes.

**Pasos**:
1. Agregar `<RootNamespace>OrionSoft.AppCore</RootNamespace>` al `.csproj`
2. Buscar y reemplazar `namespace AppCore` → `namespace OrionSoft.AppCore` en los 50 archivos fuente
3. Buscar y reemplazar `using AppCore` → `using OrionSoft.AppCore` en los ~121 archivos
4. Actualizar `AppCoreJsonContext` y todas las referencias de tipo completo
5. Actualizar tests (60 archivos de namespace + usings)
6. Actualizar samples (10+ archivos)
7. Actualizar `DependencyInjection.cs` del sample
8. Actualizar archivos `.feature` de SpecFlow
9. Ejecutar build completo + tests
10. Actualizar documentación

**Mitigación de breaking change**: Crear un archivo de `global using` o type-forwarding temporal:
```csharp
// Backwards compatibility (temporal)
[assembly: TypeForwardedTo(typeof(OrionSoft.AppCore.Application.Wrappers.Response<>))]
```

#### Opción B — Cambiar AssemblyName a `AppCore`

Menos trabajo pero pierde el namespace corporativo en NuGet.

**Cambio**: Solo modificar `AppCore.csproj`:
```xml
<AssemblyName>AppCore</AssemblyName>
```

#### Recomendación

**Opción A** es la correcta a largo plazo. Dado que es la librería base y el paquete NuGet se llama `OrionSoft.AppCore`, los consumidores esperan `using OrionSoft.AppCore.*`. Este cambio debe hacerse en un **branch dedicado** con su propio PR.

**Riesgo**: Alto — breaking change para todos los consumidores. Requiere bump de versión major.

---

## Fase 4 — Documentación

> Creación de documentación faltante.

### 4.1 — B-08: Documentar `CleanArchitectureSample`

**Problema**: El sample `CleanArchitectureSample` no tiene `README.md` ni documentación que explique su propósito, arquitectura o cómo ejecutarlo.

**Estructura actual del sample**:
```
samples/CleanArchitectureSample/
├── App.sln
├── src/
│   ├── ApiRest/          # Web API (Program.cs, Endpoints, Config)
│   ├── Application/      # CQRS Commands/Queries, DTOs, Interfaces
│   └── Infrastructure/   # EF/Dapper repos, Services, Mappings
└── .vscode/
```

**Archivo a crear**: `samples/CleanArchitectureSample/README.md`

**Contenido mínimo**:
- Descripción del sample y qué demuestra
- Prerequisitos (PostgreSQL, .NET 10)
- Instrucciones de configuración y ejecución
- Diagrama de arquitectura (capas y dependencias)
- Features de AppCore demostradas (Response wrappers, Exceptions, HttpService, etc.)
- Estructura del proyecto

**Riesgo**: Ninguno — solo documentación.

---

## Orden de Ejecución Recomendado

```
Fase 1 (Bajo riesgo)          Fase 2 (Diseño)             Fase 3 (Mayor)      Fase 4
┌─────────────────────┐    ┌─────────────────────────┐    ┌──────────────┐    ┌──────────┐
│ 1.1 B-02 CreatedAt  │    │ 2.1 M-04/M-05 object→   │    │ 3.1 A-02     │    │ 4.1 B-08 │
│ 1.2 B-05 nuget.conf │───→│     JsonElement/string   │───→│ Namespace    │───→│ README   │
│ 1.3 M-02 Documentar │    │ 2.2 B-03 Serilog decouple│    │ Refactoring  │    │ Sample   │
└─────────────────────┘    └─────────────────────────┘    └──────────────┘    └──────────┘
       ~1 hora                    ~3-4 horas                  ~4-6 horas        ~1 hora
```

---

## Criterios de Aceptación

Para cada hallazgo, el criterio de éxito es:

- [ ] **Build**: 0 errores, 0 warnings
- [ ] **Tests**: Todos los tests existentes pasan (actualmente 380)
- [ ] **AOT**: `dotnet publish` con `PublishAot=true` sin nuevos warnings
- [ ] **AotTestApp**: Todos los tests de validación AOT pasan
- [ ] **Commit**: Cambios organizados en commits lógicos con mensajes descriptivos

---

## Resumen de Progreso Esperado

| Fase | Hallazgos | Acumulado | % Total |
|------|-----------|-----------|---------|
| Ya completado | 25/33 | 25/33 | 76% |
| Fase 1 (B-02, B-05, M-02) | +3 | 28/33 | 85% |
| Fase 2 (M-04, M-05, B-03) | +3 | 31/33 | 94% |
| Fase 3 (A-02) | +1 | 32/33 | 97% |
| Fase 4 (B-08) | +1 | 33/33 | **100%** |
