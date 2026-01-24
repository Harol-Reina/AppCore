using System.Text.Json.Serialization;

namespace AppCore.Application.Wrappers;
public class Response<T>(string? message = null, T? data = default) {
    /// <summary>Reply message operation</summary>
    /// <example>Finish Ok</example>
    public string? Message { get; init; } = message;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Data { get; set; } = data;

    public static Response<T> Success(string message, T? data = default) {
        return new Response<T>(message, data);
    }

    public static Response<T> Success(T data) {
        return new Response<T>(data: data);
    }

    public static Response<T> Failure(string message, T? data = default) {
        return new Response<T>(message, data);
    }
}
