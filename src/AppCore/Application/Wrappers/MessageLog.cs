using System.Text.Json;
using AppCore.Application.Extensions;

namespace AppCore.Application.Wrappers;

/// <summary>
/// Represents a structured log message for AOT-compatible logging.
/// Uses object instead of dynamic for Native AOT compatibility.
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
    /// Gets or sets the message content. 
    /// Changed from dynamic to object for AOT compatibility.
    /// </summary>
    public required object Message { get; init; }

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
