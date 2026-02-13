using System.Text.Json;
using OrionSoft.AppCore.Application.Extensions;

namespace OrionSoft.AppCore.Application.Wrappers;

/// <summary>
/// Represents a structured log message for AOT-compatible logging.
/// Uses JsonElement for Native AOT compatibility (no runtime type metadata needed).
/// </summary>
public sealed record MessageLog {
    /// <summary>
    /// Gets or sets the type of the log message.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the source of the log message.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Gets or sets the message content as a pre-serialized JSON value.
    /// </summary>
    public required JsonElement Message { get; init; }

    /// <summary>
    /// Gets or sets the method that generated this log.
    /// </summary>
    public required string Method { get; init; }

    /// <summary>
    /// Gets or sets the path where this log was generated.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets or sets the stack trace information.
    /// </summary>
    public string? StackTrace { get; init; }

    /// <summary>
    /// Converts the MessageLog to a JSON string representation.
    /// </summary>
    /// <returns>A JSON string representation of the message log</returns>
    public override string ToString()
        => JsonExtend.Serialize(this);
}
