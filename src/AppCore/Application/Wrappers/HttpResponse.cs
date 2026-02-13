using System.Net;
using System.Text.Json;
using AppCore.Application.Extensions;

namespace AppCore.Application.Wrappers;

public sealed record HttpResponse<T>(HttpStatusCode StatusCode, long Time, JsonDocument? Response = null) {
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public override string ToString()
        => JsonExtend.Serialize(this);
}

/// <summary>
/// Simple error response for API exceptions. Used for AOT-compatible serialization.
/// </summary>
public sealed record ErrorResponse(string Message);

/// <summary>
/// Mapping error response with errors list. Used for AOT-compatible serialization.
/// </summary>
public sealed record MappingErrorResponse(string Title, IDictionary<string, string> Errors);

/// <summary>
/// Custom error response wrapper. Uses JsonElement for AOT-compatible serialization.
/// </summary>
public sealed record CustomErrorResponse(JsonElement Error);
