# AppCore API Surface Definition

## Public API (Expuesta a consumidores)

### Application Layer - Interfaces
- `ICurrentUserService` - Gestión de usuario actual
- `IDateTimeService` - Abstracción de tiempo
- `IEndpointGroupBase` - Base para endpoints

### Application Layer - DTOs y Wrappers  
- `Response<T>` - Wrapper de respuesta estándar
- `PaginationDto<T>` - Paginación genérica
- `HttpResponse<T>` - Respuesta HTTP tipada
- DTOs base: `LoginRequest`, `LoginResponse`, `EmailRequest`

### Application Layer - Exceptions
- `ValidationException` - Validaciones
- `NotFoundException` - Recurso no encontrado
- `BadRequestException` - Solicitud inválida
- `CustomException` - Excepciones personalizadas

### Domain Layer - Base Classes
- `BaseEntity<T>` - Entidad base
- `AuditableEntity` - Entidad auditable
- `IGenericRepository<E,I>` - Repositorio genérico

### Infrastructure Layer - Services
- `HttpService` - Cliente HTTP base
- `GenericRepository<E,I,D>` - Implementación repositorio
- `DateTimeService` - Implementación tiempo
- `CurrentUserService` - Implementación usuario

### Extensions y Utilities
- `JsonExtend` - Utilidades JSON
- `ServiceExtensions` - Extensiones DI
- `Configuration` - Utilidades configuración

## Internal API (Solo para AppCore)

### Infrastructure Internals
- Clases DAO concretas (`BaseDao`, `AuditableBaseDao`)
- Interceptors específicos (`SaveChangesInterceptor`)
- Middlewares internos
- Converters específicos

### Application Internals  
- Behaviors de MediatR
- Validadores internos
- Mappers específicos
- Utilidades internas

## Flujo de Migración

1. **Marcar APIs internas**: Usar `internal` keyword
2. **Documentar API pública**: XML docs completa
3. **Crear ejemplos de uso**: Para cada API pública
4. **Versioning contract**: Seguir SemVer estrictamente

## Interfaces que necesitan refactoring

```csharp
// Antes (expone detalles internos)
public class GenericRepository<E, I, D> 
    where D : BaseDao<I>

// Después (oculta implementación)
public abstract class GenericRepository<E, I> : IGenericRepository<E, I>
    where E : BaseEntity<I>
{
    internal abstract D ToDao(E entity);
    internal abstract E ToEntity(D dao);
}
```

## Breaking Changes Policy

- **Major**: Cambios en interfaces públicas, eliminación de APIs
- **Minor**: Nuevas APIs, deprecation warnings
- **Patch**: Bug fixes, improvements internos