# Identificación de APIs Públicas vs Internas - AppCore

## Fecha: Enero 24, 2026
## Fase 1: Preparación y Análisis - Tarea 2

## 1. APIs Públicas Identificadas

### 🟢 Domain Layer - Interfaces Públicas

#### IGenericRepository<E, I>
```csharp
namespace AppCore.Domain.Interfaces;
public interface IGenericRepository<E, I> where E : BaseEntity<I>
{
    // CRUD Operations
    Task<List<E>?> GetAllAsync(params Expression<Func<E, object>>[]? includes);
    Task<PaginationDto<E>?> GetPagedAsync(int pageNumber, int pageSize, params Expression<Func<E, object>>[]? includes);
    Task<E?> GetByIdAsync(I id, params Expression<Func<E, object>>[]? includes);
    Task<E?> AddAsync(E entity);
    Task UpdateAsync(E entity);
    Task DeleteAsync(E entity);
    
    // Query Operations  
    Task<List<E>?> FindAsync(Expression<Func<E, bool>> expression, params Expression<Func<E, object>>[]? includes);
    Task<E?> FindFirstAsync(Expression<Func<E, bool>> expression, params Expression<Func<E, object>>[]? includes);
    Task<int> CountAsync(Expression<Func<E, bool>>? expression = null);
    Task<bool> AnyAsync(Expression<Func<E, bool>>? expression = null);
}
```
**Status**: ✅ PÚBLICA - Interface principal para repositorios

#### IHttpRequestRepository  
```csharp
namespace AppCore.Domain.Interfaces;
// Pendiente de revisión
```
**Status**: ⚠️ REVISAR - Evaluar si debe ser pública

### 🟢 Application Layer - Interfaces Públicas

#### ICurrentUserService
```csharp
namespace AppCore.Application.Interfaces;
public interface ICurrentUserService 
{
    string GetUserId([CallerMemberName] string memberName = "");
    string GetUserName([CallerMemberName] string memberName = "");  
    string GetToken([CallerMemberName] string memberName = "");
    JwtSecurityToken GetJwtToken([CallerMemberName] string memberName = "");
    string GetXtraceId([CallerMemberName] string memberName = "");
}
```
**Status**: ✅ PÚBLICA - Interface esencial para autenticación

#### IDateTimeService
```csharp
namespace AppCore.Application.Interfaces;
// Pendiente de revisión
```
**Status**: ✅ PÚBLICA - Abstracción de tiempo

#### IEndpointGroupBase
```csharp
namespace AppCore.Application.Interfaces;
// Pendiente de revisión
```
**Status**: ⚠️ REVISAR - Evaluar necesidad pública

### 🟢 Application Layer - DTOs Públicos

#### Response<T> (Wrapper Principal)
```csharp
namespace AppCore.Application.Wrappers;
public class Response<T>
{
    public string? Message { get; init; }
    public T? Data { get; set; }
    
    public static Response<T> Success(string message, T? data = default);
    public static Response<T> Success(T data);
    public static Response<T> Failure(string message, T? data = default);
}
```
**Status**: ✅ PÚBLICA - Wrapper principal de respuestas

#### PaginationDto<T>
```csharp
namespace AppCore.Application.DTOs;
public class PaginationDto<T>
{
    // Properties de paginación
}
```
**Status**: ✅ PÚBLICA - DTO esencial para paginación

#### DTOs de Autenticación
```csharp
namespace AppCore.Application.DTOs;
public class LoginRequest { }      // ✅ PÚBLICA
public class LoginResponse { }     // ✅ PÚBLICA  
public class EmailRequest { }      // ✅ PÚBLICA
```

### 🟢 Application Layer - Exceptions Públicas

#### Exceptions Base
```csharp
namespace AppCore.Application.Exceptions;
public abstract class CustomException : Exception         // ✅ PÚBLICA
public class BadRequestException : CustomException        // ✅ PÚBLICA
public class NotFoundException : CustomException          // ✅ PÚBLICA
public class ValidationException : CustomException       // ✅ PÚBLICA
public class AuthenticationException : CustomException   // ✅ PÚBLICA
public class ForbiddenAccessException : CustomException  // ✅ PÚBLICA
```

#### Exceptions Internas/Específicas
```csharp
namespace AppCore.Application.Exceptions;
public class ApiDBException : CustomException            // 🔴 INTERNA
public class ApiHttpException : CustomException          // 🔴 INTERNA
public class HttpBaseException : CustomException         // 🔴 INTERNA
public class MappingException : CustomException          // 🔴 INTERNA
public class OperationException : CustomException        // ⚠️ REVISAR
public class SerializerException : CustomException       // 🔴 INTERNA
```

## 2. APIs que deben ser INTERNAS

### 🔴 Infrastructure Layer - Todas Internas
```csharp
namespace AppCore.Infrastructure;
// Todo el namespace debe ser internal
internal class GenericRepository<E, I, D>    // Implementación específica
internal class BaseDao<I>                    // DAO base
internal class AuditableBaseDao              // DAO auditable
internal class SaveChangesInterceptor        // Interceptor específico
```

### 🔴 Application Layer - Behaviors y Middleware
```csharp
namespace AppCore.Application.Behaviours;
internal class ValidationBehaviour<TRequest, TResponse>      // MediatR behavior
internal class UnhandledExceptionBehaviour<TRequest, TResponse>  // MediatR behavior

namespace AppCore.Application.Middleware;
// Todos los middlewares específicos deben ser internos
```

### 🔴 Application Layer - Utilities Internos
```csharp
namespace AppCore.Application.Utils;
// Utilities específicos que no son parte de la API pública
```

## 3. APIs con Decisión Pendiente

### ⚠️ Extensions y Configuration
```csharp
namespace AppCore.Application.Extensions;
public static class ServiceExtensions        // ⚠️ EVALUAR - ¿Pública para DI?
public static class AppExtensions           // ⚠️ EVALUAR - ¿Pública para configuración?
public static class JsonExtend              // ⚠️ EVALUAR - ¿Utility pública?
```

### ⚠️ Wrappers Adicionales
```csharp
namespace AppCore.Application.Wrappers;
public class HttpResponse<T>                 // ⚠️ EVALUAR - ¿Duplica Response<T>?
public class PageResult<T>                   // ⚠️ EVALUAR - ¿Relacionado con PaginationDto?
public class MessageLog                      // 🔴 INTERNA - Logging específico
```

## 4. Domain Layer - Entities Base

### 🟢 Base Entities (Públicas)
```csharp
namespace AppCore.Domain.Common;
public abstract class BaseEntity<I>         // ✅ PÚBLICA - Base para todas las entidades
{
    public virtual I Id { get; set; }
}

public abstract class AuditableEntity : BaseEntity<Guid>  // ✅ PÚBLICA - Auditoría
{
    public DateTime Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}
```

## 5. Enumeraciones y Constants

### 🟢 Enums Públicos (si existen)
```csharp
namespace AppCore.Domain.Enums;
// Todos los enums de dominio deberían ser públicos
```

## 6. Plan de Refactoring de Visibilidad

### Acciones Inmediatas

1. **Marcar como `internal`**:
   ```csharp
   // Infrastructure layer completa
   namespace AppCore.Infrastructure { /* todo internal */ }
   
   // Behaviors específicos
   internal class ValidationBehaviour<TRequest, TResponse>
   internal class UnhandledExceptionBehaviour<TRequest, TResponse>
   
   // Exceptions específicas
   internal class ApiDBException
   internal class ApiHttpException
   internal class HttpBaseException
   internal class MappingException
   internal class SerializerException
   ```

2. **Mantener como `public`**:
   ```csharp
   // Domain interfaces
   public interface IGenericRepository<E, I>
   
   // Application interfaces  
   public interface ICurrentUserService
   public interface IDateTimeService
   
   // Base entities
   public abstract class BaseEntity<I>
   public abstract class AuditableEntity
   
   // Main wrappers & DTOs
   public class Response<T>
   public class PaginationDto<T>
   
   // Core exceptions
   public class CustomException
   public class BadRequestException
   public class NotFoundException
   public class ValidationException
   public class AuthenticationException
   ```

3. **Evaluar caso por caso**:
   - ServiceExtensions (¿necesario para DI?)
   - JsonExtend (¿utility pública?)
   - HttpResponse<T> vs Response<T>
   - IEndpointGroupBase (¿necesario público?)

## 7. Documentación XML Requerida

Todas las APIs públicas necesitan documentación XML completa:

```csharp
/// <summary>
/// Interfaz de repositorio genérico para realizar operaciones CRUD en entidades.
/// </summary>
/// <typeparam name="E">El tipo de la entidad de dominio.</typeparam>
/// <typeparam name="I">El tipo del identificador de la entidad.</typeparam>
/// <example>
/// <code>
/// var repository = serviceProvider.GetService&lt;IGenericRepository&lt;User, int&gt;&gt;();
/// var user = await repository.GetByIdAsync(1);
/// </code>
/// </example>
public interface IGenericRepository<E, I> where E : BaseEntity<I>
```

---
*Documento generado durante Fase 1 - Tarea 2: Identificación de APIs públicas*