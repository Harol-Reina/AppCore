# Guía de Migración: EF Core a Dapper.AOT

Esta guía detalla el proceso para migrar repositorios y acceso a datos desde Entity Framework Core a **Dapper con soporte NativeAOT**.

## 1. Visión General

| Característica | Entity Framework Core | Dapper.AOT |
|----------------|-----------------------|------------|
| **Metodología** | ORM completo, Tracking, LINQ | Micro-ORM, SQL Explícito, POCOs puros |
| **Compatibilidad AOT** | Limitada (requiere reflexión) | **Total** (Sin reflexión dinámica) |
| **Performance** | Alta sobrecarga (tracking, query gen) | **Nativa** (SQL directo) |

## 2. Pasos de Migración

### 2.1 Reemplazar Contexto de Datos

**Antes (EF Core):**
```csharp
public class AppDbContext : DbContext {
    public DbSet<User> Users { get; set; }
}
```

**Después (Dapper):**
Eliminar `DbContext`. Usar `IDbConnectionFactory`.
```csharp
public class UserRepository(IDbConnectionFactory connectionFactory) {
    // ...
}
```

### 2.2 Migrar Repositorios

**Antes (EF Core + Generic Repository):**
```csharp
var users = await _repository.GetAllAsync(includes: x => x.Role);
```

**Después (Dapper.AOT):**
Escribir SQL explícito.
```csharp
[DapperAot]
public async Task<List<UserEntity>> GetAllAsync() {
    using var db = await _connectionFactory.CreateConnectionAsync();
    var sql = "SELECT * FROM Users"; // SQL puro
    return (await db.QueryAsync<UserDao>(sql)).Select(ToEntity).ToList();
}
```

### 2.3 Manejo de Relaciones (Includes)

**Antes:** `Include(x => x.Role)`
**Después:**
1.  **Multi-Mapping**: Usar `QueryAsync<T1, T2, TR>` si se necesita la relación en la misma query.
2.  **Separate Queries**: Cargar IDs y luego consultar la tabla relacionada (recomendado para colecciones grandes).

### 2.4 Entidades y DAOs

*   **Entidades de Dominio**: No cambian (Clean Architecture).
*   **DAOs**:
    *   Deben ser **POCOs puros**.
    *   Implementar `IBaseDao<T>`.
    *   **PROHIBIDO**: `[Key]`, `[Table]`, `[Required]` (DataAnnotations).
    *   **PROHIBIDO**: Herencia de clases (usar interfaces).

## 3. Checklist de Validación

*   [ ] NuGet packages de EF Core eliminados.
*   [ ] No quedan referencias a `Microsoft.EntityFrameworkCore`.
*   [ ] Todos los métodos de repositorio tienen atributo `[DapperAot]`.
*   [ ] Los DAOs implementan interfaces, no heredan clases.
*   [ ] Tests unitarios refactorizados para no usar `InMemoryDatabase`.
