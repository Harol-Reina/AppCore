# Troubleshooting Guide

Guía consolidada de resolución de problemas para AppCore y NativeAOT.

## 1. Errores de NativeAOT

### "JsonTypeInfo metadata for type 'X' was not provided"
**Causa**: Estás intentando serializar un tipo que no está registrado en el `JsonSerializerContext`. En AOT, `System.Text.Json` no puede usar reflexión para descubrir propiedades.
**Solución**:
1.  Crear un contexto parcial:
    ```csharp
    [JsonSerializable(typeof(MyType))]
    public partial class MyJsonContext : JsonSerializerContext {}
    ```
2.  Pasar el contexto a las opciones:
    ```csharp
    JsonSerializer.Serialize(obj, MyJsonContext.Default.MyType);
    ```

### "Requires unreferenced code..."
**Causa**: Estás llamando a un método que usa reflexión insegura para AOT.
**Solución**:
*   Si es código propio: Refactorizar para evitar reflexión o usar Source Generators.
*   Si es librería de terceros: Verificar si tiene versión compatible con AOT o buscar alternativa.

## 2. Errores de GitHub Packages

### "401 Unauthorized" al restaurar paquetes
**Causa**: El PAT (Personal Access Token) ha expirado, no tiene permisos `read:packages`, o no está configurado en `nuget.config`.
**Solución**:
1.  Verificar que el PAT tenga scope `read:packages`.
2.  Regenerar el PAT si es necesario.
3.  Verificar credenciales en `~/.nuget/NuGet/NuGet.Config` o variables de entorno.

## 3. Errores de Base de Datos (Dapper + Npgsql)

### "Npgsql.PostgresException: 42703: column 'id' does not exist"
**Causa**: PostgreSQL es case-sensitive con identificadores si fueron creados con comillas. Dapper mapea propiedades a columnas por nombre exacto.
**Solución**: Verificar que el nombre de la propiedad C# coincida exactamente con la columna SQL, o usar alias en la query:
```sql
SELECT id as Id, name as Name FROM users
```

### "InvalidCastException" en Dapper
**Causa**: El tipo de dato en C# no coincide con el de PG (ej. `DateTime` vs `Timestamp`).
**Solución**: Asegurar que las propiedades del DAO usen tipos compatibles (ej. `DateTime` para `timestamp`, `string` para `text`).
