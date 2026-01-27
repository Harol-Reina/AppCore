# NativeAOT Best Practices

Esta guía establece las mejores prácticas y prohibiciones estrictas para asegurar la compatibilidad con NativeAOT en AppCore y sus consumidores.

## 1. Principios Fundamentales

NativeAOT (Ahead-of-Time compilation) compila el código IL a código máquina nativo antes de la ejecución. Esto implica:
1.  **Sin JIT**: No hay compilación Just-In-Time.
2.  **Sin Generación Dinámica de Código**: No se puede generar IL en runtime (ej. `Reflection.Emit`).
3.  **Trimming**: El código no usado se elimina agresivamente.

## 2. Reglas de Oro (PROHIBICIONES)

### ❌ Reflexión Dinámica
No usar `Type.GetProperties()`, `Activator.CreateInstance()` o similares sin anotaciones apropiadas. El Trimmer no puede rastrear estas referencias.

### ❌ Serialización JSON Clásica
No usar `JsonSerializer.Serialize(obj)` sin un `JsonTypeInfo`.
**Correcto**: Usar Source Generators (`AppCoreJsonContext`).

### ❌ Expresiones Dinámicas (LINQ)
No construir `Expression<Func<T, bool>>` dinámicamente si dependen de tipos no conocidos en tiempo de compilación.

### ❌ Atributos DataAnnotations en DAOs
Dapper.AOT no soporta `[Key]`, `[Table]`, etc. Usar mapeo manual o SQL explícito.

## 3. Patrones Recomendados

### ✅ Source Generators
Usar Source Generators para todo lo que requiera inspección de tipos:
*   **JSON**: `[JsonSerializable(typeof(MyDto))]`
*   **Logging**: `[LoggerMessage]` (si aplica)
*   **Regex**: `[GeneratedRegex]`

### ✅ Inyección de Dependencias
Preferir registro explícito sobre assembly scanning.
**Evitar**: `services.Scan(...)`
**Preferir**: `services.AddScoped<IService, Service>()`

### ✅ Annotations
Si DEBES usar reflexión (solo librerías base), anota el código correctamente:
*   `[RequiresUnreferencedCode]`
*   `[RequiresDynamicCode]`
*   `[DynamicallyAccessedMembers]`

## 4. Troubleshooting AOT

### Warning IL2026 (RequiresUnreferencedCode)
Indica que el método marcado puede romperse si el Trimmer elimina código necesario.
**Solución**: Verificar si existe una alternativa con Source Generators o suprimir si se ha verificado manualmente en `rd.xml`.

### Warning IL3050 (RequiresDynamicCode)
Indica que el método requiere generar código nativo, lo cual es imposible en AOT.
**Solución**: Esta es una limitación dura. Debes refactorizar el código para no depender de JIT.
