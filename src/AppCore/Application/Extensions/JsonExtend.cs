using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppCore.Application.Exceptions;
using AppCore.Application.Serialization;

namespace AppCore.Application.Extensions;

/// <summary>
/// AOT-compatible JSON serialization extensions using source-generated serialization context.
/// Provides efficient JSON operations without runtime reflection.
/// </summary>

public static class JsonExtend {

    /// <summary>
    /// Global JsonSerializerOptions used by JsonExtend.
    /// Configure this at application startup to add custom TypeInfoResolvers (JsonSerializerContexts).
    /// </summary>
    public static JsonSerializerOptions Options { get; set; } = new(new JsonSerializerOptions {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    });

    // Static constructor to ensure default context is registered if needed
    static JsonExtend() {
        // Ensure the internal AppCore context is added by default if not already present in the chain
        // Note: When creating new options above, since it's "new", it doesn't have the context.
        // We must add AppCoreJsonContext to it.
        Options.TypeInfoResolverChain.Add(AppCoreJsonContext.Default);
    }

    /// <summary>
    /// Converts an object to a JsonDocument using AOT-compatible serialization.
    /// Uses compile-time type information for correct serialization in NativeAOT scenarios.
    /// </summary>
    /// <typeparam name="T">The type of object to convert</typeparam>
    /// <param name="value">The object to convert</param>
    /// <returns>A JsonDocument representation of the object</returns>
    /// <exception cref="SerializerException">Thrown when serialization fails</exception>
    public static JsonDocument? ToJsonDocument<T>(T? value) {
        if (value == null) return null;
        try {
            return value switch {
                JsonDocument doc => doc,
                string str => ParseFromStringOrWrap(str),
                _ => JsonDocument.Parse(JsonSerializer.Serialize(value, (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)Options.GetTypeInfo(typeof(T))))
            };
        } catch (Exception ex) {
            throw new SerializerException(ex);
        }
    }

    private static JsonDocument ParseFromStringOrWrap(string input) {
        input = input.Replace("\n", "").Trim();
        if (string.IsNullOrWhiteSpace(input))
            return JsonDocument.Parse("{}");

        if (input.StartsWith("{") || input.StartsWith("[") || input.StartsWith("\"")) {
            try {
                return JsonDocument.Parse(input);
            } catch {/* Continúa abajo para envolverlo */}
        }
        // Esto es útil si el input es un valor simple o una cadena que no es un JSON válido.
        // Por ejemplo, si input es "Hello World", lo convertirá a {"value": "Hello World"}
        var wrapper = new Dictionary<string, object> { { "value", input } };
        string wrapped = JsonSerializer.Serialize(wrapper, Options.GetTypeInfo(typeof(Dictionary<string, object>)));
        return JsonDocument.Parse(wrapped);
    }

    /// <summary>
    /// Converts a JsonDocument to a strongly-typed object using AOT-compatible deserialization.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to</typeparam>
    /// <param name="jsonDocument">The JsonDocument to convert</param>
    /// <returns>The deserialized object</returns>
    /// <exception cref="ArgumentException">Thrown when the JSON is invalid</exception>
    public static T FromJsonDocument<T>(this JsonDocument jsonDocument) {
        try {
            string jsonString = jsonDocument.RootElement.GetRawText();
            return (T)JsonSerializer.Deserialize(jsonString, (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)Options.GetTypeInfo(typeof(T)))!;
        } catch (JsonException) {
            throw new ArgumentException("Invalid JSON string.");
        }
    }


    /// <summary>
    /// Serializes an object to JSON using AOT-compatible source generation.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize</typeparam>
    /// <param name="value">The object to serialize</param>
    /// <param name="memberName">The calling member name (automatically captured)</param>
    /// <param name="sourceFilePath">The source file path (automatically captured)</param>
    /// <param name="sourceLineNumber">The source line number (automatically captured)</param>
    /// <returns>A JSON string representation of the object</returns>
    /// <exception cref="SerializerException">Thrown when serialization fails</exception>
    public static string Serialize<T>(T value,
                                      [CallerMemberName] string memberName = "",
                                      [CallerFilePath] string sourceFilePath = "",
                                      [CallerLineNumber] int sourceLineNumber = 0) {
        try {
            // Use the AOT-compatible JsonSerializerContext for serialization
            return JsonSerializer.Serialize(value, (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)Options.GetTypeInfo(typeof(T)));
        } catch (Exception ex) {
            throw new SerializerException(ex, memberName, sourceFilePath, sourceLineNumber);
        }
    }

    /// <summary>
    /// Deserializes a JSON string to an object using AOT-compatible source generation.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to</typeparam>
    /// <param name="value">The JSON string to deserialize</param>
    /// <param name="memberName">The calling member name (automatically captured)</param>
    /// <param name="sourceFilePath">The source file path (automatically captured)</param>
    /// <param name="sourceLineNumber">The source line number (automatically captured)</param>
    /// <returns>The deserialized object</returns>
    /// <exception cref="SerializerException">Thrown when deserialization fails</exception>
    public static T? Deserialize<T>(string value,
                                    [CallerMemberName] string memberName = "",
                                    [CallerFilePath] string sourceFilePath = "",
                                    [CallerLineNumber] int sourceLineNumber = 0) {
        if (string.IsNullOrWhiteSpace(value)) return default;
        try {
            // Use the AOT-compatible JsonSerializerContext for deserialization
            return JsonSerializer.Deserialize(value, (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)Options.GetTypeInfo(typeof(T)));
        } catch (Exception ex) {
            throw new SerializerException(ex, memberName, sourceFilePath, sourceLineNumber);
        }
    }

}

