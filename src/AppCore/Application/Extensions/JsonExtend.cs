using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppCore.Application.Exceptions;

namespace AppCore.Application.Extensions;

public static class JsonExtend {

    /// <summary>
    /// Converts an object to a JsonDocument.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="SerializerException"></exception>
    public static JsonDocument? ToJsonDocument(object? value) {
        if (value == null) return null;
        try {
            return value switch {
                JsonDocument doc => doc,
                string str => ParseFromStringOrWrap(str),
                _ => JsonDocument.Parse(Serialize(value))
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
        string wrapped = JsonSerializer.Serialize(new { value = input });
        return JsonDocument.Parse(wrapped);
    }

    public static T FromJsonDocument<T>(this JsonDocument jsonDocument) {
        try {
            string jsonString = jsonDocument.RootElement.GetRawText();
            return JsonSerializer.Deserialize<T>(jsonString, s_readOptions)!;
        } catch (JsonException) {
            throw new ArgumentException("Invalid JSON string.");
        }
    }

    private static readonly JsonSerializerOptions s_writeOptions = new() {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions s_readOptions = new() {
        PropertyNameCaseInsensitive = true,
        Converters = {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    public static string Serialize<T>(T value,
                                      [CallerMemberName] string memberName = "",
                                      [CallerFilePath] string sourceFilePath = "",
                                      [CallerLineNumber] int sourceLineNumber = 0) {
        try {
            return JsonSerializer.Serialize(value, s_writeOptions);
        } catch (Exception ex) {
            throw new SerializerException(ex, memberName, sourceFilePath, sourceLineNumber);
        }

    }

    public static T? Deserialize<T>(string value,
                                    [CallerMemberName] string memberName = "",
                                    [CallerFilePath] string sourceFilePath = "",
                                    [CallerLineNumber] int sourceLineNumber = 0) {
        if (string.IsNullOrWhiteSpace(value)) return default;
        try {
            T? data = JsonSerializer.Deserialize<T>(value, s_readOptions);
            return data;
        } catch (Exception ex) {
            throw new SerializerException(ex, memberName, sourceFilePath, sourceLineNumber);
        }
    }

}
