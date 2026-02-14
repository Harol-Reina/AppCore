using System.Text.Json;

namespace OrionSoft.AppCore.Application.Wrappers;

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
