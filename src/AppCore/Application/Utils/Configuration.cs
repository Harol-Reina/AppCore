using System.Diagnostics.CodeAnalysis;
using AppCore.Application.Exceptions;
using Microsoft.Extensions.Configuration;

namespace AppCore.Application.Utils;

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

    [RequiresUnreferencedCode("Configuration binding may require types that cannot be statically analyzed.")]
    [RequiresDynamicCode("Configuration binding may require runtime code generation.")]
    public static string[] StringArray(string key) {
        var value = _configuration.GetSection(key).Get<string[]>() ??
            throw new NotFoundException($"Configuration key '{key}' is not set or is empty.");
        return value;
    }
}
