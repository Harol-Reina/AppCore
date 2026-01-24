# Matriz de Compatibilidad - AppCore v2.0

## Fecha: Enero 24, 2026
## Fase 1: Preparación y Análisis - Tarea 4

## 1. Matriz de Compatibilidad por Componente

### 🎯 Leyenda de Compatibilidad
- ✅ **Compatible**: No requiere cambios
- ⚠️ **Deprecated**: Funciona con warnings, cambios requeridos en futuro
- ❌ **Breaking**: Requiere cambios inmediatos
- 🆕 **New**: Nueva funcionalidad disponible

---

## 2. APIs Públicas - Matriz de Compatibilidad

| Componente | v1.x | v2.x | Compatibilidad | Acción Requerida |
|------------|------|------|----------------|------------------|
| **Domain Interfaces** | | | | |
| `IGenericRepository<E,I>` | ✅ | ✅ | ✅ Compatible | Ninguna |
| `IHttpRequestRepository` | ✅ | ✅ | ✅ Compatible | Ninguna |
| **Application Interfaces** | | | | |
| `ICurrentUserService` | ✅ | ✅ | ✅ Compatible | Ninguna |
| `IDateTimeService` | ✅ | ✅ | ✅ Compatible | Ninguna |
| `IEndpointGroupBase` | ✅ | ⚠️ | ⚠️ Evaluación | TBD |
| **DTOs & Wrappers** | | | | |
| `Response<T>` | ✅ | ✅ | ✅ Compatible | Ninguna |
| `HttpResponse<T>` | ✅ | ⚠️ | ⚠️ Deprecated | Migrar a `Response<T>` |
| `PaginationDto<T>` | ✅ | ✅ | ✅ Compatible | Ninguna |
| `PaginationResponse` | ✅ | ✅ | ✅ Compatible | Ninguna |
| `PageResult<T>` | ✅ | ⚠️ | ⚠️ Deprecated | Migrar a `Response<T>` |
| **Core DTOs** | | | | |
| `LoginRequest` | ✅ | ✅ | ✅ Compatible | Ninguna |
| `LoginResponse` | ✅ | ✅ | ✅ Compatible | Ninguna |
| `EmailRequest` | ✅ | ✅ | ✅ Compatible | Ninguna |
| `MetaInfo` | ✅ | ✅ | ✅ Compatible | Ninguna |

---

## 3. APIs Internas - Breaking Changes

| Componente | v1.x | v2.x | Impacto | Alternativa |
|------------|------|------|---------|-------------|
| **Infrastructure Layer** | | | | |
| `GenericRepository<E,I,D>` | public | internal | ❌ Breaking | `IGenericRepository<E,I>` |
| `BaseDao<I>` | public | internal | ❌ Breaking | `BaseEntity<I>` |
| `AuditableBaseDao` | public | internal | ❌ Breaking | `AuditableEntity` |
| **Behaviors** | | | | |
| `ValidationBehaviour` | public | internal | ❌ Breaking | Auto-registered via DI |
| `UnhandledExceptionBehaviour` | public | internal | ❌ Breaking | Auto-registered via DI |
| **Specific Exceptions** | | | | |
| `ApiDBException` | public | internal | ❌ Breaking | `CustomException` |
| `ApiHttpException` | public | internal | ❌ Breaking | `CustomException` |
| `HttpBaseException` | public | internal | ❌ Breaking | `CustomException` |
| `MappingException` | public | internal | ❌ Breaking | `CustomException` |
| `SerializerException` | public | internal | ❌ Breaking | `CustomException` |

---

## 4. Dependencias Externas - Matriz de Impacto

| Dependencia | v1.x Status | v2.x Status | Impacto | Migración |
|-------------|-------------|-------------|---------|-----------|
| **Core Dependencies** | | | | |
| `Microsoft.Extensions.Logging.Abstractions` | ✅ Incluido | ✅ Incluido | ✅ Compatible | Ninguna |
| `Microsoft.EntityFrameworkCore.Abstractions` | ✅ Incluido | ✅ Incluido | ✅ Compatible | Ninguna |
| `MediatR` | ✅ Incluido | ✅ Incluido | ✅ Compatible | Ninguna |
| `FluentValidation.DependencyInjectionExtensions` | ✅ Incluido | ✅ Incluido | ✅ Compatible | Ninguna |
| **Optional Dependencies** | | | | |
| `Microsoft.EntityFrameworkCore` | ✅ Incluido | ✅ Incluido | ✅ Compatible | Ninguna |
| `AutoMapper.Extensions.Microsoft.DependencyInjection` | ✅ Incluido | ✅ Incluido | ✅ Compatible | Ninguna |
| **Potentially Optional** | | | | |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | ✅ Incluido | ⚠️ Opcional | ⚠️ Requiere decisión | Agregar explícitamente si necesario |
| `Swashbuckle.AspNetCore` | ✅ Incluido | ⚠️ Opcional | ⚠️ Evaluación | Mover a package opcional |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | ✅ Incluido | ⚠️ Evaluación | ⚠️ TBD | TBD |
| **Logging** | | | | |
| `Serilog.AspNetCore` | ✅ Incluido | ⚠️ Opcional | ⚠️ Evaluación | Abstraer detrás de ILogger |
| `Serilog.Sinks.Console` | ✅ Incluido | ⚠️ Opcional | ⚠️ Evaluación | Mover a package opcional |
| `Serilog.Settings.Configuration` | ✅ Incluido | ⚠️ Opcional | ⚠️ Evaluación | Mover a package opcional |

---

## 5. Compatibilidad por Patrón de Uso

### ✅ Uso Compatible (Sin cambios requeridos)

#### Patrón 1: Uso de Interfaces
```csharp
// ANTES y DESPUÉS - Idéntico
public class UserService 
{
    private readonly IGenericRepository<User, int> _repository;
    private readonly ICurrentUserService _currentUser;
    
    public UserService(
        IGenericRepository<User, int> repository, 
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }
    
    public async Task<Response<User>> GetUserAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        return Response<User>.Success(user);
    }
}
```

#### Patrón 2: Exception Handling Base
```csharp
// ANTES y DESPUÉS - Compatible
try 
{
    await _repository.AddAsync(entity);
}
catch (ValidationException ex)  // ✅ Sigue siendo público
{
    return Response<T>.Failure(ex.Message);
}
catch (CustomException ex)      // ✅ Base exception pública
{
    return Response<T>.Failure(ex.Message);
}
```

### ❌ Uso Incompatible (Cambios requeridos)

#### Patrón 3: Herencia Directa de Implementación
```csharp
// ANTES - Funcionaba
public class UserRepository : GenericRepository<User, int, UserDao>
{
    public UserRepository(AppDbContext context) : base(context) { }
}

// DESPUÉS - Breaking change
// GenericRepository<,,> ahora es internal
public class UserRepository : IGenericRepository<User, int>
{
    // Implementación propia o usar servicio registrado
}
```

#### Patrón 4: Exception Handling Específico
```csharp
// ANTES - Funcionaba
try 
{
    await _repository.AddAsync(entity);
}
catch (ApiDBException ex)  // ❌ Ahora es internal
{
    _logger.LogError(ex, "Database error");
}

// DESPUÉS - Requerido
catch (CustomException ex) // ✅ Usar base exception
{
    _logger.LogError(ex, "Operation error");
}
```

### ⚠️ Uso Deprecated (Cambios recomendados)

#### Patrón 5: Wrappers Duplicados
```csharp
// ANTES - Funcionaba
public HttpResponse<User> GetUser(int id)
{
    return new HttpResponse<User> { Data = user };
}

// DESPUÉS - Deprecated warning
[Obsolete("Use Response<T> instead")]
public HttpResponse<User> GetUser(int id)  // ⚠️ Deprecated
{
    return new Response<User> { Data = user }; // ✅ Use this
}
```

---

## 6. Matriz de Compatibilidad por Target Framework

| Target Framework | v1.x Support | v2.x Support | Compatibility | Notes |
|------------------|--------------|--------------|---------------|--------|
| .NET 8.0 LTS | ❌ | 🆕 | 🆕 New Support | Multi-targeting |
| .NET 9.0 | ❌ | 🆕 | 🆕 New Support | Multi-targeting |
| .NET 10.0 | ✅ | ✅ | ✅ Compatible | Primary target |

---

## 7. DI Registration Compatibility Matrix

| Registration Pattern | v1.x | v2.x | Compatibility | Migration |
|---------------------|------|------|---------------|-----------|
| **Single Method** | | | | |
| `services.AddAppCore()` | ✅ | ⚠️ | ⚠️ Deprecated | Mantener por compatibilidad |
| **Granular Methods** | | | | |
| `services.AddAppCoreCore()` | ❌ | 🆕 | 🆕 New | Registro de APIs públicas |
| `services.AddAppCorePostgreSQL()` | ❌ | 🆕 | 🆕 New | PostgreSQL specific |
| `services.AddAppCoreSerilog()` | ❌ | 🆕 | 🆕 New | Logging specific |
| **Manual Registration** | | | | |
| Manual service registration | ✅ | ✅ | ✅ Compatible | Sigue funcionando |

---

## 8. Testing Compatibility Matrix

| Test Pattern | v1.x | v2.x | Compatibility | Notes |
|--------------|------|------|---------------|--------|
| **Unit Testing** | | | | |
| Interface mocking | ✅ | ✅ | ✅ Compatible | No changes needed |
| Concrete class testing | ✅ | ❌ | ❌ Breaking | Use interface abstractions |
| **Integration Testing** | | | | |
| Service container testing | ✅ | ⚠️ | ⚠️ Partial | Update DI registration |
| End-to-end testing | ✅ | ✅ | ✅ Compatible | No changes if using interfaces |

---

## 9. Migration Path Matrix

### Low Risk Migration (Recommended first)
| Component | Risk Level | Effort | Timeline |
|-----------|------------|--------|----------|
| ✅ Interface-only consumers | 🟢 Low | 1-2 hours | Week 1 |
| ✅ DI registration updates | 🟢 Low | 2-4 hours | Week 1 |
| ✅ Exception handling base classes | 🟢 Low | 1-2 hours | Week 1 |

### Medium Risk Migration
| Component | Risk Level | Effort | Timeline |
|-----------|------------|--------|----------|
| ⚠️ Wrapper consolidation | 🟡 Medium | 4-8 hours | Week 2-3 |
| ⚠️ Dependency package additions | 🟡 Medium | 2-4 hours | Week 2 |
| ⚠️ Configuration updates | 🟡 Medium | 4-6 hours | Week 2-3 |

### High Risk Migration (Requires careful planning)
| Component | Risk Level | Effort | Timeline |
|-----------|------------|--------|----------|
| ❌ Concrete class inheritance | 🔴 High | 8-16 hours | Week 3-4 |
| ❌ Specific exception handling | 🔴 High | 4-8 hours | Week 3-4 |
| ❌ Internal API usage | 🔴 High | 8-24 hours | Week 3-5 |

---

## 10. Validation Checklist

### Pre-Migration Validation
- [ ] ✅ Inventory all AppCore usage patterns in consumer applications
- [ ] ✅ Identify concrete class inheritance instances
- [ ] ✅ Catalog specific exception handling code
- [ ] ✅ Document custom DI registration patterns
- [ ] ✅ Create compatibility test suite

### Post-Migration Validation
- [ ] ✅ All unit tests pass
- [ ] ✅ Integration tests pass
- [ ] ✅ No compilation errors
- [ ] ✅ No runtime exceptions related to missing types
- [ ] ✅ Performance benchmarks within acceptable range
- [ ] ✅ Memory usage patterns stable

### Rollback Plan
- [ ] ✅ Maintain v1.x package availability
- [ ] ✅ Document rollback procedure
- [ ] ✅ Test rollback scenarios
- [ ] ✅ Prepare hotfix process for critical issues

---
*Matriz generada durante Fase 1 - Tarea 4: Matriz de Compatibilidad*