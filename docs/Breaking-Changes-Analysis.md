# Breaking Changes Potenciales - AppCore Restructuración

## Fecha: Enero 24, 2026
## Fase 1: Preparación y Análisis - Tarea 3

## 1. Cambios de Visibilidad (BREAKING)

### 🔴 Infrastructure Layer - APIs que se volverán internas

#### Clases que cambiarán de `public` a `internal`
```csharp
// ANTES (accesible desde consumidores)
public class GenericRepository<E, I, D> where D : BaseDao<I>

// DESPUÉS (solo interno a AppCore)  
internal class GenericRepository<E, I, D> where D : BaseDao<I>
```

**Impacto**: ⚠️ ALTO - Consumidores que hereden directamente de `GenericRepository<,,>` fallarán en compilación.

**Mitigación**: Usar interface `IGenericRepository<E,I>` en lugar de la implementación concreta.

#### DAOs y clases base internas
```csharp
// ANTES
public class BaseDao<I>
public class AuditableBaseDao

// DESPUÉS  
internal class BaseDao<I>
internal class AuditableBaseDao
```

**Impacto**: ⚠️ MEDIO - Consumidores que referencien directamente DAOs.

### 🔴 Behaviors de MediatR
```csharp
// ANTES
public class ValidationBehaviour<TRequest, TResponse>
public class UnhandledExceptionBehaviour<TRequest, TResponse>

// DESPUÉS
internal class ValidationBehaviour<TRequest, TResponse>
internal class UnhandledExceptionBehaviour<TRequest, TResponse>
```

**Impacto**: ⚠️ BAJO - Normalmente registrados via DI, no accedidos directamente.

### 🔴 Exceptions Específicas
```csharp
// ANTES
public class ApiDBException
public class ApiHttpException
public class HttpBaseException
public class MappingException
public class SerializerException

// DESPUÉS
internal class ApiDBException
internal class ApiHttpException  
internal class HttpBaseException
internal class MappingException
internal class SerializerException
```

**Impacto**: ⚠️ MEDIO - Código que capture específicamente estas excepciones.

**Mitigación**: Usar `CustomException` base o exceptions públicas más generales.

## 2. Cambios en Dependencias (BREAKING)

### 🔴 Dependencias específicas removidas del package público

#### PostgreSQL Provider
```csharp
// ANTES: Incluido automáticamente
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />

// DESPUÉS: Opcional, consumidor debe agregarlo
// Solo abstracciones en el paquete base
```

**Impacto**: ⚠️ ALTO - Aplicaciones que dependan de PostgreSQL automáticamente.

**Mitigación**: 
- Agregar `Npgsql.EntityFrameworkCore.PostgreSQL` explícitamente
- Crear package opcional `OrionSoft.AppCore.PostgreSQL`

#### Swagger/OpenAPI
```csharp
// ANTES: Incluido automáticamente
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.0" />

// DESPUÉS: Opcional
// Solo si el consumidor necesita documentación API
```

**Impacto**: ⚠️ BAJO - La mayoría de APIs web ya incluyen Swagger explícitamente.

#### JWT Authentication
```csharp
// ANTES: Incluido automáticamente  
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.2" />

// DESPUÉS: Evaluar si mover a package opcional
```

**Impacto**: ⚠️ MEDIO - `ICurrentUserService` depende de JWT.

## 3. Cambios en Estructura de APIs (POTENTIALLY BREAKING)

### 🟡 Simplificación de IGenericRepository

#### Cambio en signatura del constructor
```csharp
// ANTES: Expone detalles de implementación
public class ConcreteRepository : GenericRepository<Entity, int, EntityDao>

// DESPUÉS: API más limpia
public class ConcreteRepository : IGenericRepository<Entity, int>
```

**Impacto**: ⚠️ ALTO para herencia directa, ✅ NINGUNO para uso via interface.

### 🟡 Consolidación de Response Wrappers

#### Posible unificación
```csharp
// ANTES: Múltiples wrappers
public class Response<T>
public class HttpResponse<T>  
public class PageResult<T>

// DESPUÉS: Wrapper único optimizado
public class Response<T>
// HttpResponse<T> -> obsoleto
// PageResult<T> -> integrado en Response<T>
```

**Impacto**: ⚠️ MEDIO - Código que use `HttpResponse<T>` específicamente.

## 4. Cambios en Configuración (BREAKING)

### 🔴 Service Registration Changes

#### Extensions de DI
```csharp
// ANTES: Registro automático de servicios internos
services.AddAppCore(); // registra todo automáticamente

// DESPUÉS: Registro más granular
services.AddAppCoreCore();        // Solo APIs públicas
services.AddAppCorePostgreSQL();  // Extensión específica PostgreSQL
services.AddAppCoreSerilog();     // Extensión específica logging
```

**Impacto**: ⚠️ ALTO - Cambios en startup/configuration.

**Mitigación**: Mantener `AddAppCore()` como método de compatibilidad.

## 5. Cambios en Serialización/JSON (POTENTIALLY BREAKING)

### 🟡 JsonConverters específicos
```csharp
// Si existen JsonConverters específicos en JsonExtend
// Evaluar si deben ser públicos o internos
```

**Impacto**: ⚠️ TBD - Depende de implementación actual.

## 6. Target Framework Changes (POTENTIALLY BREAKING)

### 🟡 Multi-targeting support
```csharp
// ANTES: Solo .NET 10.0
<TargetFramework>net10.0</TargetFramework>

// DESPUÉS: Posible multi-targeting
<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
```

**Impacto**: ✅ POSITIVO - Mayor compatibilidad, pero podría afectar APIs específicas de versión.

## 7. Plan de Mitigación de Breaking Changes

### Estrategia de Versionado Semántico

#### Version 2.0.0 (Major Breaking Release)
- ✅ Cambios de visibilidad (internal)
- ✅ Simplificación de APIs
- ✅ Restructuración de dependencias

#### Compatibility Shims (Temporal)
```csharp
// Obsolete wrappers para transición
[Obsolete("Use Response<T> instead. Will be removed in v3.0.0")]
public class HttpResponse<T> : Response<T> { }

[Obsolete("Use IGenericRepository<E,I> interface. Will be removed in v3.0.0")]
public abstract class GenericRepositoryBase<E, I> : IGenericRepository<E, I> { }
```

### Migration Guide
```csharp
// STEP 1: Cambiar implementaciones concretas por interfaces
// ANTES
public class MyRepository : GenericRepository<User, int, UserDao>

// DESPUÉS  
public class MyRepository : IGenericRepository<User, int>

// STEP 2: Agregar dependencias específicas explícitamente
// ANTES: Automático
// DESPUÉS: En .csproj
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />

// STEP 3: Actualizar DI registration
// ANTES
services.AddAppCore();

// DESPUÉS  
services.AddAppCoreCore();
services.AddAppCorePostgreSQL();
```

## 8. Testing de Compatibilidad

### Test Matrix
| Scenario | v1.x Consumer | v2.x AppCore | Expected Result |
|----------|---------------|--------------|-----------------|
| Interface usage | ✅ Uses IGenericRepository | ✅ | ✅ Compatible |
| Concrete inheritance | ❌ Inherits GenericRepository | ✅ | ❌ Compile Error |
| Exception catching | ❌ Catches ApiDBException | ✅ | ❌ Won't catch |
| DI Registration | ✅ Uses AddAppCore() | ✅ | ⚠️ Deprecated warning |

### Automated Breaking Change Detection
```bash
# Using Microsoft.CodeAnalysis.PublicApiAnalyzers
dotnet add package Microsoft.CodeAnalysis.PublicApiAnalyzers

# Generate baseline
dotnet pack --configuration Release --verbosity normal

# Compare APIs
# Implement script para comparar assemblies v1 vs v2
```

## 9. Cronograma de Breaking Changes

### Pre-Release (v2.0.0-preview)
- **Semana 5**: Implementar cambios de visibilidad
- **Semana 6**: Testing compatibility matrix
- **Semana 7**: Documentation y migration guide

### Release Candidato (v2.0.0-rc)
- **Semana 8**: Validación final con consumidores
- **Semana 9**: Ajustes basados en feedback

### Release Final (v2.0.0)
- **Semana 10**: Release con migration guide completa

### Support Timeline
- **v1.x**: Mantener por 6 meses (solo critical fixes)
- **v2.x**: Soporte completo a partir de release

---

## 10. Checklist de Validación Breaking Changes

- [ ] ✅ Todas las APIs internas marcadas como `internal`
- [ ] ✅ Migration guide documentado
- [ ] ✅ Compatibility matrix tested
- [ ] ✅ Obsolete attributes en APIs deprecated
- [ ] ✅ Unit tests para nuevas APIs públicas
- [ ] ✅ Integration tests con consumidores principales
- [ ] ✅ Performance benchmarks comparativos
- [ ] ✅ Documentation actualizada

---
*Documento generado durante Fase 1 - Tarea 3: Documentación de Breaking Changes*