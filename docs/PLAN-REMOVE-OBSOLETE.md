# Plan: Eliminar clases y metodos marcados como `[Obsolete]`

> **Fecha**: Febrero 2026
> **Alcance**: 2 miembros obsoletos en la libreria + consumidores en el sample + tests

---

## Inventario de miembros obsoletos

| # | Miembro | Ubicacion | Mensaje |
|---|---------|-----------|---------|
| 1 | `class Configuration` (static) | `src/AppCore/Application/Utils/Configuration.cs` | "Use IConfiguration via dependency injection instead of this static accessor." |
| 2 | `AddSwaggerExtension()` (extension) | `src/AppCore/Application/Extensions/ServiceExtensions.cs:36-38` | "Use AddOpenApiExtension instead." |

---

## Consumidores identificados

### 1. `Configuration` (clase estatica)

| Consumidor | Archivo | Uso |
|------------|---------|-----|
| Sample — Program.cs | `samples/CleanArchitectureSample/src/ApiRest/Program.cs:17` | `Configuration.Initialize(builder.Configuration)` |
| Sample — AppConstants.cs | `samples/CleanArchitectureSample/src/Application/Common/AppConstants.cs:21` | `Configuration.GetConfig(key)` |
| Tests — ConfigurationTests.cs | `tests/AppCore.UnitTests/Application/Utils/ConfigurationTests.cs` | 9 tests que cubren `Initialize`, `GetConfig`, `RequiredConfig`, `IntConfig`, `StringArray` |

**Dependientes indirectos de `AppConstants`** (via propiedades estaticas `DefaultConnection`, `SchemaDB`, `PokemonHost`):
- `NpgsqlConnectionFactory.cs` — lee `AppConstants.DefaultConnection`
- `PokemonService.cs` — lee `AppConstants.PokemonHost`
- `DbInitializer.cs` — lee `AppConstants.SchemaDB`

### 2. `AddSwaggerExtension()`

| Consumidor | Archivo | Uso |
|------------|---------|-----|
| Sample — Program.cs | `samples/CleanArchitectureSample/src/ApiRest/Program.cs:55` | `builder.Services.AddSwaggerExtension()` (ya NO compila — falta argumento `IConfiguration`) |

**Nota**: El sample tambien llama `AddCorsExtension()` sin `IConfiguration` (linea 67), lo cual tampoco compila. Esto indica que el sample nunca fue actualizado cuando estas extensiones cambiaron su firma.

---

## Hallazgo adicional: Sample roto

El `CleanArchitectureSample` actualmente **no compila** debido a 2 errores + 2 warnings:

```
error CS7036: 'ServiceExtensions.AddSwaggerExtension(IServiceCollection, IConfiguration)' — falta argumento 'configuration'
error CS7036: 'ServiceExtensions.AddCorsExtension(IServiceCollection, IConfiguration)' — falta argumento 'configuration'
warning CS0618: 'Configuration' is obsolete (2 ocurrencias)
```

---

## Fases de ejecucion

### Fase 1 — Eliminar `Configuration` static class de la libreria

**1.1 Eliminar el archivo fuente**

| Accion | Archivo |
|--------|---------|
| Eliminar | `src/AppCore/Application/Utils/Configuration.cs` |

**1.2 Eliminar tests asociados**

| Accion | Archivo |
|--------|---------|
| Eliminar | `tests/AppCore.UnitTests/Application/Utils/ConfigurationTests.cs` |

**Riesgo**: Ninguno — la clase es `static`, sin interfaces ni herencia. No afecta DI del framework.

---

### Fase 2 — Eliminar `AddSwaggerExtension` de la libreria

**2.1 Eliminar el metodo obsoleto**

| Archivo | Cambio |
|---------|--------|
| `src/AppCore/Application/Extensions/ServiceExtensions.cs:36-38` | Eliminar las 3 lineas del metodo `AddSwaggerExtension` y su atributo `[Obsolete]` |

**Riesgo**: Ninguno — el metodo solo delega a `AddOpenApiExtension`. Ningun codigo interno lo usa.

---

### Fase 3 — Actualizar CleanArchitectureSample

Esta es la fase mas compleja porque hay que reemplazar el patron `Configuration` statico por inyeccion de dependencias.

**3.1 Refactorizar `AppConstants` → inyeccion via `IConfiguration`**

Estrategia: Eliminar `AppConstants` y reemplazar con `IConfiguration` inyectado directamente donde se necesita, o crear una clase de opciones tipada.

**Opcion A — Opciones tipadas (Recomendada)**

Crear `AppSettings` como record/class con `IConfiguration.GetSection().Get<T>()`:

```csharp
// Nueva clase en Application/Common/AppSettings.cs
namespace App.Application.Common;

public sealed class AppSettings {
    public required string DefaultConnection { get; init; }
    public required string SchemaDB { get; init; }
    public string PokemonHost { get; init; } = "https://pokeapi.co/api/v2/";
}
```

Registrar en DI:
```csharp
// En Program.cs o DependencyInjection.cs
services.AddSingleton(configuration.GetSection("App").Get<AppSettings>()
    ?? throw new InvalidOperationException("AppSettings section not found"));
```

Inyectar `AppSettings` donde se necesite (en vez de acceder a constantes estaticas).

**Opcion B — Inyectar `IConfiguration` directamente**

Mas simple pero menos tipado. Inyectar `IConfiguration` en los constructores y leer las claves directamente.

**Archivos a modificar**:

| Archivo | Cambio |
|---------|--------|
| `samples/.../Application/Common/AppConstants.cs` | Eliminar o reemplazar por `AppSettings` |
| `samples/.../Application/DependencyInjection.cs` | Registrar `AppSettings` (si se usa Opcion A) |
| `samples/.../Infrastructure/Data/NpgsqlConnectionFactory.cs` | Inyectar `AppSettings` en constructor (reemplaza `AppConstants.DefaultConnection`) |
| `samples/.../Infrastructure/Services/PokemonService.cs` | Inyectar `AppSettings` en constructor (reemplaza `AppConstants.PokemonHost`) |
| `samples/.../Infrastructure/Data/DbInitializer.cs` | Revisar si usa `AppConstants.SchemaDB`; inyectar si es necesario |
| `samples/.../ApiRest/appsettings.json` | Agrupar claves bajo seccion `"App"` (si se usa Opcion A) |

**3.2 Reemplazar `AddSwaggerExtension()` por `AddOpenApiExtension()`**

| Archivo | Cambio |
|---------|--------|
| `samples/.../ApiRest/Program.cs:55` | `AddSwaggerExtension()` → `AddOpenApiExtension(builder.Configuration)` |

**3.3 Corregir `AddCorsExtension()` — pasar `IConfiguration`**

| Archivo | Cambio |
|---------|--------|
| `samples/.../ApiRest/Program.cs:67` | `AddCorsExtension()` → `AddCorsExtension(builder.Configuration)` |

**3.4 Actualizar `Program.cs` — eliminar `Configuration.Initialize()` y `AppConstants.Init()`**

| Archivo | Lineas a eliminar |
|---------|-------------------|
| `samples/.../ApiRest/Program.cs:4` | `using OrionSoft.AppCore.Application.Utils;` |
| `samples/.../ApiRest/Program.cs:17` | `Configuration.Initialize(builder.Configuration);` |
| `samples/.../ApiRest/Program.cs:85` | `AppConstants.Init();` |

**3.5 Migrar Swagger UI a OpenAPI**

El sample usa `UseSwagger()` + `UseSwaggerUI()` (Swashbuckle) pero la libreria migro a `Microsoft.AspNetCore.OpenApi`. Actualizar:

```csharp
// Antes (Swashbuckle)
app.UseSwagger();
app.UseSwaggerUI();

// Despues (OpenAPI nativo)
app.MapOpenApi();
```

Revisar si el sample tiene paquete `Swashbuckle.AspNetCore` en su csproj — si lo tiene, eliminarlo. Si no lo tiene y compila sin el, probablemente hereda de algun paquete transitivo.

---

### Fase 4 — Build y tests

```bash
# Verificar libreria principal
dotnet build --configuration Release
dotnet test --configuration Release

# Verificar sample compila
dotnet build samples/CleanArchitectureSample/App.sln --configuration Release
```

**Resultado esperado**:
- 0 errores, 0 warnings en libreria
- 401 tests pasan (menos los tests eliminados de `ConfigurationTests` — nuevo total sera ~392)
- Sample compila sin errores ni warnings

---

### Fase 5 — Limpieza de `bin/` recursivos (bonus)

Durante la investigacion se detecto que el sample tiene directorios `bin/Debug/` anidados recursivamente (~40+ niveles de profundidad). Esto puede causar problemas de rendimiento y espacio.

```bash
# Limpiar artifacts
git clean -xfd samples/CleanArchitectureSample/src/ApiRest/bin/
# O agregar al .gitignore si no esta
```

---

## Resumen de archivos afectados

| Accion | Archivo |
|--------|--------|
| **Eliminar** | `src/AppCore/Application/Utils/Configuration.cs` |
| **Eliminar** | `tests/AppCore.UnitTests/Application/Utils/ConfigurationTests.cs` |
| **Editar** | `src/AppCore/Application/Extensions/ServiceExtensions.cs` (eliminar metodo obsoleto) |
| **Crear** | `samples/.../Application/Common/AppSettings.cs` (opciones tipadas) |
| **Eliminar** | `samples/.../Application/Common/AppConstants.cs` |
| **Editar** | `samples/.../ApiRest/Program.cs` (eliminar Configuration, corregir extensiones) |
| **Editar** | `samples/.../Application/DependencyInjection.cs` (registrar AppSettings) |
| **Editar** | `samples/.../Infrastructure/Data/NpgsqlConnectionFactory.cs` (inyectar AppSettings) |
| **Editar** | `samples/.../Infrastructure/Services/PokemonService.cs` (inyectar AppSettings) |
| **Editar** | `samples/.../Infrastructure/Data/DbInitializer.cs` (inyectar si usa AppConstants) |
| **Editar** | `samples/.../ApiRest/appsettings.json` (reorganizar claves bajo seccion App) |
| **Editar** | `samples/.../ApiRest/App.ApiRest.csproj` (eliminar Swashbuckle si existe) |

## Criterios de aceptacion

- [ ] `dotnet build --configuration Release` — 0 errores, 0 warnings
- [ ] `dotnet test --configuration Release` — todos los tests pasan
- [ ] `dotnet build samples/CleanArchitectureSample/App.sln` — 0 errores, 0 warnings
- [ ] `grep -rn '\[Obsolete\]' --include='*.cs' src/` — 0 resultados
- [ ] `grep -rn 'Configuration\.' --include='*.cs' samples/` no muestra referencias a la clase estatica eliminada
