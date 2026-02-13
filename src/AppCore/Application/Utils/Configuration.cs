using System.Diagnostics.CodeAnalysis;
using OrionSoft.AppCore.Application.Exceptions;
using Microsoft.Extensions.Configuration;

namespace OrionSoft.AppCore.Application.Utils;

[Obsolete("Use IConfiguration via dependency injection instead of this static accessor. Will be removed in a future version.")]
public static class Configuration {
    private static IConfiguration _configuration { get; set; } = null!;

    // Inicializa la configuración
    public static void Initialize(IConfiguration configuration) {
        _configuration = configuration;
    }

    // Obtiene una variable de configuración opcional
    public static string? GetConfig(string key) {
        var value = _configuration[key];
        return string.IsNullOrEmpty(value) ? null : value;
    }

    // Obtiene una variable de configuración requerida
    public static string RequiredConfig(string key) {
        var value = GetConfig(key) ??
            throw new NotFoundException($"Configuration key '{key}' is not set or is empty.");
        return value;
    }

    // Obtiene una variable de configuración como entero
    public static int IntConfig(string key) {
        var value = GetConfig(key) ??
            throw new NotFoundException($"Configuration key '{key}' is not set or is empty.");
        if (!int.TryParse(value, out int result))
            throw new OperationException($"Configuration key '{key}' is not a valid integer.");
        return result;
    }

    public static string[] StringArray(string key) {
        var section = _configuration.GetSection(key);
        if (!section.Exists())
            throw new NotFoundException($"Configuration key '{key}' is not set or is empty.");

        return section.GetChildren()
                      .Select(x => x.Value)
                      .Where(x => x != null)
                      .Cast<string>()
                      .ToArray();
    }
}
