namespace AppCore.Application.DTOs;

public sealed class MetaInfo {
    /// <summary>Entorno de ejecución Development|Production.</summary>
    /// <example>Development</example>
    public string? Environment { get; set; }

    /// <summary>Version del compilado</summary>
    /// <example>1.0</example>
    public string? Version { get; set; }

    /// <summary>Fecha de la compilación</summary>
    /// <example>2023-01-01</example>
    public DateTime DateCompile { get; set; }

    /// <summary>Nombre de la base de datos usada</summary>
    /// <example>DataBaseName</example>
    public string? TableName { get; set; }

    public List<string>? Endpoints { get; set; } = [];
}

