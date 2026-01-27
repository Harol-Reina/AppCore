# API Reference

Referencia oficial de las APIs públicas de AppCore.

## 1. Interfaces de Dominio

### `IGenericRepository<E, I>`
Contrato base para repositorios.
**Uso**: Inyectar para operaciones de lectura/escritura sobre entidades.
**Métodos Clave**: `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`.
**Nota AOT**: No soporta `Include(string)`. Usar sobrecargas con SQL explícito/Dapper si se requiere en implementación.

## 2. Interfaces de Aplicación

### `ICurrentUserService`
Acceso al usuario actual autenticado.
*   `GetUserId()`: Retorna el ID del usuario (sub).
*   `GetUserName()`: Retorna el nombre de usuario (preferred_username).

### `IDateTimeService`
Abstracción del tiempo del sistema.
*   `NowUtc`: Retorna `DateTime.UtcNow`.

## 3. DTOs y Wrappers

### `Response<T>`
Wrapper estándar para todas las respuestas de API.
```csharp
public class Response<T> {
    public bool Succeeded { get; }
    public string Message { get; }
    public T Data { get; }
    public List<string> Errors { get; }
}
```

### `PaginationDto<T>`
Estructura para datos paginados.
```csharp
public class PaginationDto<T> {
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalRecords { get; }
    public List<T> Data { get; }
}
```

## 4. Excepciones

*   `CustomException`: Base para todas las excepciones controladas.
*   `ValidationException`: Error de validación de negocio (400).
*   `NotFoundException`: Recurso no encontrado (404).
*   `ForbiddenAccessException`: Usuario sin permisos (403).

## 5. DAO Interfaces (AOT)

### `IBaseDao<T>`
Interfaz obligatoria para todos los Data Access Objects.
```csharp
public interface IBaseDao<T> : IAuditableBaseDao {
    public T Id { get; set; }
}
```

### `IAuditableBaseDao`
Contrato para objetos con auditoría.
```csharp
public interface IAuditableBaseDao {
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
}
```
