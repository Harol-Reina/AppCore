# Adopción de C# 14

AppCore utiliza características modernas de C# 14 (parte de .NET 10) para escribir código más conciso, expresivo y eficiente.

## 1. Collection Expressions

Usar la sintaxis de corchetes `[]` para inicializar colecciones.

**❌ Antes:**
```csharp
var list = new List<string> { "a", "b" };
var array = new string[] { "a", "b" };
```

**✅ C# 14:**
```csharp
List<string> list = ["a", "b"];
string[] array = ["a", "b"];
```

### Spread Operator
```csharp
string[] combined = [..array1, ..array2, "new item"];
```

## 2. Primary Constructors

Usar constructores primarios para DTOs, Records y Servicios simples.

**❌ Antes:**
```csharp
public class UserDto {
    public string Name { get; }
    public UserDto(string name) { Name = name; }
}
```

**✅ C# 14:**
```csharp
public class UserDto(string name) {
    public string Name { get; init; } = name;
}
```

## 3. Params Collections

En interfaces y métodos que aceptan arrays `params`, preferir `IEnumerable<T>` o `ReadOnlySpan<T>` para mayor flexibilidad y performance.

**✅ Ejemplo:**
```csharp
public void Log(params IEnumerable<string> messages) {
    foreach (var msg in messages) Console.WriteLine(msg);
}

// Llamada:
Log("Error 1", "Error 2"); // Funciona igual
Log(myList); // Ahora funciona sin .ToArray()
```

## 4. Pattern Matching Mejorado

Usar pattern matching exhaustivo en `switch expressions`.

**✅ Ejemplo:**
```csharp
var result = status switch {
    > 200 and < 300 => "Success",
    400 or 404 => "Client Error",
    500 => "Server Error",
    _ => "Unknown"
};
```

## 5. Reglas de Estilo

1.  **Var**: Usar `var` solo cuando el tipo es obvio.
2.  **Target-typed new**: `User u = new("Name");` permitido si el tipo es explícito a la izquierda.
3.  **File-scoped namespaces**: `namespace MyApp.Services;` (sin llaves).
